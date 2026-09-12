using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Ghasele.Application.DTOs;
using Ghasele.Application.Exceptions;
using Ghasele.Application.Interfaces;
using Ghasele.Application.Localization;
using Ghasele.Application.Scheduling;
using Ghasele.Domain.Entities;
using Ghasele.Domain.Interfaces;

namespace Ghasele.Application.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IDeliveryWindowRepository _deliveryWindowRepository;
        private readonly IItemTypeRepository _itemTypeRepository;
        private readonly ICleanerItemPriceRepository _cleanerItemPriceRepository;
        private readonly IMarketingCodeRepository _marketingCodeRepository;
        private readonly ITripRepository _tripRepository;
        private readonly IAppSettingsRepository _appSettingsRepository;
        private readonly ICurrentLanguageProvider _language;
        private readonly INotificationService _notificationService;
        private readonly IUserNotificationService _userNotificationService;

        public OrderService(IOrderRepository orderRepository, IDeliveryWindowRepository deliveryWindowRepository, IItemTypeRepository itemTypeRepository, ICleanerItemPriceRepository cleanerItemPriceRepository, IMarketingCodeRepository marketingCodeRepository, ITripRepository tripRepository, IAppSettingsRepository appSettingsRepository, ICurrentLanguageProvider language, INotificationService notificationService, IUserNotificationService userNotificationService)
        {
            _orderRepository = orderRepository;
            _deliveryWindowRepository = deliveryWindowRepository;
            _itemTypeRepository = itemTypeRepository;
            _cleanerItemPriceRepository = cleanerItemPriceRepository;
            _marketingCodeRepository = marketingCodeRepository;
            _tripRepository = tripRepository;
            _appSettingsRepository = appSettingsRepository;
            _language = language;
            _notificationService = notificationService;
            _userNotificationService = userNotificationService;
        }

        public async Task<OrderDto> CreateOrderAsync(CreateOrderDto dto)
        {
            // Customers may now place a new order while an earlier one is still being
            // processed. The old one-open-order-at-a-time rule was removed by product;
            // HasPendingOrderAsync is still exposed on the repository for reporting.

            // No UserId means a guest checkout. The contact number is then the only route to the
            // customer, so it is required here rather than trusted to the client's own validation.
            var isGuest = dto.UserId == null;
            var contactPhoneNumber = dto.ContactPhoneNumber?.Trim();
            var deviceToken = dto.DeviceToken?.Trim();

            if (isGuest && string.IsNullOrWhiteSpace(contactPhoneNumber))
            {
                throw new AppException(ErrorCodes.GuestContactNumberRequired);
            }

            // Without a device token the order would be unreachable afterwards: the guest has no
            // account to look it up under, so the Orders tab would silently never show it.
            if (isGuest && string.IsNullOrWhiteSpace(deviceToken))
            {
                throw new AppException(ErrorCodes.GuestDeviceTokenRequired);
            }

            // The booked trip schedule slot. Validated here rather than trusted from the
            // client: the slot list the app rendered may be minutes old, and in that time the
            // window can have been deactivated, started, or filled by someone else.
            var window = await ResolveScheduledWindowAsync(dto);

            // Stamp the fee at order time from the admin-managed settings, so a price change
            // between ordering and collection cannot charge the customer more than they were
            // quoted. The client's DeliveryAmount is ignored - it is not authoritative.
            var settings = await _appSettingsRepository.GetAsync();
            var deliveryAmount = settings.NormalDeliveryPrice;

            var order = new Order
            {
                Lat = dto.Lat,
                Long = dto.Long,
                UserId = dto.UserId,
                IsGuest = isGuest,
                // Only stored for guests; a signed-in order carries the number on the user row,
                // and duplicating it here would let the two drift apart.
                ContactPhoneNumber = isGuest ? contactPhoneNumber : null,
                // Likewise guest-only: it is how the app finds these orders again, and a
                // signed-in customer is expected to see their orders on any device.
                DeviceToken = isGuest ? deviceToken : null,
                // Guest-only as well: a signed-in order pushes to the token on the user row,
                // which is kept current by the app on every sign-in and token refresh.
                FcmToken = isGuest ? dto.FcmToken?.Trim() : null,
                TotalAmount = dto.TotalAmount,
                NetAmount = dto.NetAmount,
                DeliveryAmount = deliveryAmount,
                CleanerAmount = dto.CleanerAmount,
                ReferenceNumber = $"GH-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N").Substring(0, 4).ToUpper()}",
                Status = OrderStatus.PendingCollection,
                // Always Normal now. The enum survives for orders placed when Express existed.
                Type = OrderType.Normal,
                DeliveryWindowId = window.Id,
                ScheduledDate = dto.ScheduledDate!.Value,
                // Copied, not derived - see Order.ScheduledStartTime for why.
                ScheduledStartTime = window.StartTime,
                ScheduledEndTime = window.EndTime,
                CreatedAt = DateTime.UtcNow
            };

            if (!string.IsNullOrEmpty(dto.MarketingCode))
            {
                var marketingCode = await _marketingCodeRepository.GetByCodeAsync(dto.MarketingCode);
                if (marketingCode != null)
                {
                    order.MarketingCodeId = marketingCode.Id;
                }
            }

            await _orderRepository.AddAsync(order);

            return MapToDto(order, _language.Language);
        }

        /// <summary>
        /// Resolves and validates the trip schedule slot the customer booked, returning the
        /// window to stamp on the order.
        /// </summary>
        /// <remarks>
        /// Every rejection here is a slot the app offered moments ago but that is no longer
        /// bookable, so each gets its own message - "that time is now full" and "we no longer
        /// run that window" need different things from the customer.
        /// </remarks>
        private async Task<DeliveryWindow> ResolveScheduledWindowAsync(CreateOrderDto dto)
        {
            if (dto.DeliveryWindowId == null || dto.ScheduledDate == null)
            {
                throw new AppException(ErrorCodes.OrderScheduleRequired);
            }

            var window = await _deliveryWindowRepository.GetByIdAsync(dto.DeliveryWindowId.Value)
                         ?? throw new AppException(ErrorCodes.DeliveryWindowNotFound);

            if (!window.IsActive)
            {
                throw new AppException(ErrorCodes.DeliveryWindowInactive);
            }

            // Rejects both past dates and a date whose window has already started today, on the
            // operator's clock. This is also what stops a client inventing its own date: only a
            // date the slots endpoint would have offered survives.
            if (!JordanTime.IsUpcoming(dto.ScheduledDate.Value, window.StartTime))
            {
                throw new AppException(ErrorCodes.OrderScheduleNotBookable);
            }

            var booked = await _orderRepository.CountBookedAsync(window.Id, dto.ScheduledDate.Value);
            if (booked >= window.Capacity)
            {
                throw new AppException(ErrorCodes.DeliveryWindowFull);
            }

            return window;
        }

        public async Task<List<OrderDto>> GetUserOrdersAsync(Guid userId, int page = 1, int pageSize = 20)
        {
            var orders = await _orderRepository.GetByUserIdAsync(userId, page, pageSize);
            var itemTypes = await _itemTypeRepository.GetAllAsync();
            return orders.Select(o => MapToDto(o, _language.Language, itemTypes)).ToList();
        }

        public async Task<List<OrderDto>> GetGuestOrdersAsync(string deviceToken, int page = 1, int pageSize = 20)
        {
            var orders = await _orderRepository.GetByDeviceTokenAsync(deviceToken, page, pageSize);
            var itemTypes = await _itemTypeRepository.GetAllAsync();
            return orders.Select(o => MapToDto(o, _language.Language, itemTypes)).ToList();
        }

        public async Task<int> UpdateGuestFcmTokenAsync(string deviceToken, string fcmToken)
        {
            return await _orderRepository.UpdateGuestFcmTokenAsync(deviceToken.Trim(), fcmToken.Trim());
        }

        /// <summary>
        /// Claims the orders a guest placed on this device for the account they just signed in
        /// with, so ordering before registering does not cost them their history.
        /// </summary>
        /// <remarks>
        /// Deliberately quiet when there is nothing to do: most sign-ins are on a device that
        /// never placed a guest order, and a caller should not have to distinguish "no token
        /// sent" from "nothing to claim" to know the sign-in went fine.
        /// </remarks>
        public async Task<int> ClaimGuestOrdersAsync(Guid userId, string? deviceToken)
        {
            if (string.IsNullOrWhiteSpace(deviceToken) || userId == Guid.Empty)
            {
                return 0;
            }

            return await _orderRepository.ClaimGuestOrdersAsync(deviceToken.Trim(), userId);
        }

        public async Task<List<OrderDto>> GetAllOrdersAsync(string? status = null, string? searchTerm = null)
        {
            OrderStatus? orderStatus = null;
            if (!string.IsNullOrEmpty(status) && Enum.TryParse<OrderStatus>(status, true, out var parsedStatus))
            {
                orderStatus = parsedStatus;
            }

            var orders = await _orderRepository.GetAllAsync(orderStatus, searchTerm);
            var itemTypes = await _itemTypeRepository.GetAllAsync();
            return orders.Select(o => MapToDto(o, _language.Language, itemTypes)).ToList();
        }

        public async Task<OrderDto?> GetOrderByIdAsync(Guid id)
        {
            var order = await _orderRepository.GetByIdAsync(id);
            if (order == null) return null;
            var itemTypes = await _itemTypeRepository.GetAllAsync();
            return MapToDto(order, _language.Language, itemTypes);
        }

        public async Task<OrderDto> AddItemsToOrderAsync(Guid orderId, AddOrderItemsDto dto)
        {
            // Load order with minimal includes to avoid tracking conflicts
            var order = await _orderRepository.GetByIdForUpdateAsync(orderId);
            if (order == null)
            {
                throw AppException.NotFound(ErrorCodes.OrderNotFound);
            }

            // Need to calculate price based on ItemTypes
            var allItemTypes = await _itemTypeRepository.GetAllAsync();

            // What we pay is negotiated per laundry, so pricing needs to know which one is
            // handling this order. The trip carries that, and a trip is always assigned before a
            // driver can itemise the pickup. An order priced with no cleaner assigned, or with a
            // laundry whose rate card is empty, records no cleaner cost at all - see below.
            var cleanerPrices = await GetCleanerRateCardAsync(order.TripId);

            decimal itemsTotal = 0;
            decimal costTotal = 0;

            var orderItems = new List<OrderItem>();
            foreach (var itemDto in dto.Items)
            {
                var serviceType = ServiceType.Iron;
                if (!string.IsNullOrEmpty(itemDto.ServiceType) && Enum.TryParse<ServiceType>(itemDto.ServiceType, true, out var parsedType))
                {
                    serviceType = parsedType;
                }

                var orderItem = new OrderItem
                {
                    Id = Guid.NewGuid(),
                    ItemType = itemDto.ItemType,
                    ServiceType = serviceType,
                    Quantity = itemDto.Quantity,
                    OrderId = order.Id
                };
                orderItems.Add(orderItem);

                // Find price. The driver types this in free text on whatever language their
                // device is in, so it can arrive in either name.
                var itemType = allItemTypes.FirstOrDefault(t =>
                    t.TypeNameEn.Equals(itemDto.ItemType, StringComparison.OrdinalIgnoreCase) ||
                    t.TypeNameAr.Equals(itemDto.ItemType, StringComparison.OrdinalIgnoreCase));
                if (itemType != null)
                {
                    decimal price = serviceType switch
                    {
                        ServiceType.Cleaning => itemType.CleaningPrice,
                        ServiceType.Both => itemType.BothPrice,
                        _ => itemType.IronPrice
                    };

                    // What we pay comes only from the rate agreed with the laundry handling this
                    // order. There is no per-item fallback: item types carry customer prices
                    // alone, so an item with no agreed rate costs us nothing on paper until one
                    // is entered on that laundry's rate card. The customer price above is never
                    // touched by any of this - what we pay and what we charge move independently.
                    decimal cost = 0;
                    if (cleanerPrices != null && cleanerPrices.TryGetValue(itemType.Id, out var agreed))
                    {
                        cost = agreed.PriceFor(serviceType);
                    }

                    // Frozen onto the line so that re-reading this order later shows what it was
                    // priced at, not what the pricing tables happen to say today.
                    orderItem.ItemTypeId = itemType.Id;
                    orderItem.UnitPrice = price;
                    orderItem.UnitCleanerPrice = cost;

                    itemsTotal += price * itemDto.Quantity;
                    costTotal += cost * itemDto.Quantity;
                }
            }

            // Save items first
            await _orderRepository.AddItemsAsync(orderItems);

            // Fetch order again to ensure context tracks correctly or just modify attached order?
            // Since we fetched `order` via `GetByIdForUpdateAsync` it might be tracked.
            // But we modified `order.Items` by adding previously? No, I stopped adding to `order.Items` directly above.
            // So `order` is unmodified and its Items collection might not know about new items yet, which is fine for calculation.

            // DeliveryAmount is deliberately left alone: it was stamped at order time from
            // the settings that applied to this order's type (see CreateOrderAsync). Re-reading
            // it here would silently re-price the order if an admin changed the fee meanwhile.
            order.CleanerAmount = costTotal;

            // Apply marketing discount if applicable
            decimal discountAmount = 0;
            decimal marketerShare = 0;
            if (order.MarketingCodeId.HasValue)
            {
                var marketingCode = await _marketingCodeRepository.GetByIdAsync(order.MarketingCodeId.Value);
                if (marketingCode != null)
                {
                    discountAmount = itemsTotal * (marketingCode.DiscountPercentage / 100);
                    marketerShare = itemsTotal * (marketingCode.SharePercentage / 100);
                    order.MarketingDiscount = discountAmount;
                    order.MarketerShare = marketerShare;
                    order.MarketingDiscountPercentage = marketingCode.DiscountPercentage;
                    order.MarketerSharePercentage = marketingCode.SharePercentage;
                }
            }

            order.TotalAmount = (itemsTotal - discountAmount) + order.DeliveryAmount; // Plus 1 delivery minus discount
            order.NetAmount = order.TotalAmount - order.CleanerAmount - order.DeliveryAmount - marketerShare;

            // Update status
            order.Status = OrderStatus.Collected;

            // Update order separately
             await _orderRepository.UpdateAsync(order); // Use async update

            // Auto-transition trip status if all orders are Collected
            if (order.TripId.HasValue)
            {
                var trip = await _tripRepository.GetByIdAsync(order.TripId.Value);
                if (trip != null && trip.Orders.All(o => o.Status == OrderStatus.Collected))
                {
                    trip.Status = TripStatus.Collected;
                    await _tripRepository.UpdateAsync(trip);
                }
            }

            // Reload with all includes for the DTO
            var updatedOrder = await _orderRepository.GetByIdAsync(orderId);

            // The driver has just itemised the pickup, so this is the first moment the customer
            // can be told what the order actually costs. GetByIdForUpdateAsync deliberately skips
            // the User include, which is why this runs on the reloaded order instead.
            await NotifyOrderPricedAsync(updatedOrder!);

            return MapToDto(updatedOrder!, _language.Language, allItemTypes);
        }

        /// <summary>
        /// The agreed rates of the laundry assigned to <paramref name="tripId"/>, keyed by item
        /// type, or null when there is no trip, no cleaner on it, or nothing agreed with them.
        /// </summary>
        /// <remarks>
        /// Null and empty mean the same thing to the caller - nothing is owed to a laundry we
        /// have agreed no rates with - so there is no point distinguishing them.
        /// </remarks>
        private async Task<Dictionary<Guid, CleanerItemPrice>?> GetCleanerRateCardAsync(Guid? tripId)
        {
            if (tripId is not Guid id) return null;

            var trip = await _tripRepository.GetByIdAsync(id);
            if (trip?.CleanerId is not Guid cleanerId) return null;

            var prices = await _cleanerItemPriceRepository.GetByCleanerAsync(cleanerId);
            return prices.Count == 0 ? null : prices.ToDictionary(p => p.ItemTypeId);
        }

        // Tells the customer the price once the driver has itemised their pickup. A push failure
        // must never roll back the priced order, hence the in-app record is written first and the
        // FCM send is best-effort.
        private async Task NotifyOrderPricedAsync(Order order)
        {
            // Two decimals with invariant digits: an Arabic device locale would otherwise render
            // Eastern-Arabic numerals here, which is not how the apps show prices elsewhere.
            var amount = order.TotalAmount.ToString("0.00", CultureInfo.InvariantCulture);
            string title = "تحديث طلب";
            string body = $"تم تجهيز طلبك {order.ReferenceNumber} بقيمة {amount} دينار.";

            // The in-app list is keyed by user, so only a signed-in order gets a stored record.
            // A guest has no account to read one from - the push is their whole notification.
            if (order.UserId is Guid ownerId)
            {
                await _userNotificationService.CreateNotificationAsync(ownerId, title, body);
            }

            // Guest orders push to the token captured at checkout; see Order.ResolvePushToken.
            var pushToken = order.ResolvePushToken();
            if (!string.IsNullOrEmpty(pushToken))
            {
                await _notificationService.SendNotificationAsync(pushToken, title, body);
            }
        }

        public async Task<OrderDto> UpdateOrderAsync(Guid id, UpdateOrderDto dto)
        {
            var order = await _orderRepository.GetByIdAsync(id);
            if (order == null) throw AppException.NotFound(ErrorCodes.OrderNotFound);

            if (dto.Lat.HasValue) order.Lat = dto.Lat.Value;
            if (dto.Long.HasValue) order.Long = dto.Long.Value;
            if (dto.CleanerAmount.HasValue) order.CleanerAmount = dto.CleanerAmount.Value;
            if (!string.IsNullOrEmpty(dto.Status) && Enum.TryParse<OrderStatus>(dto.Status, true, out var status))
            {
                order.Status = status;
            }

            await _orderRepository.UpdateAsync(order);
            var itemTypes = await _itemTypeRepository.GetAllAsync();
            return MapToDto(order, _language.Language, itemTypes);
        }

        public async Task<OrderDto> DeleteOrderItemAsync(Guid orderId, Guid itemId)
        {
            var item = await _orderRepository.GetItemByIdAsync(itemId);
            if (item == null || item.OrderId != orderId)
            {
                throw AppException.NotFound(ErrorCodes.OrderItemNotFound);
            }

            await _orderRepository.DeleteItemAsync(item);

            var order = await _orderRepository.GetByIdForUpdateAsync(orderId);
            if (order == null)
            {
                throw AppException.NotFound(ErrorCodes.OrderNotFound);
            }
            var itemTypes = await _itemTypeRepository.GetAllAsync();
            return MapToDto(order, _language.Language, itemTypes);
        }

        public async Task DeleteOrderAsync(Guid id)
        {
            await _orderRepository.DeleteAsync(id);
        }

        public static OrderDto MapToDto(Order order, string language, IEnumerable<ItemType>? itemTypes = null)
        {
            if (order == null) return new OrderDto();

            return new OrderDto
            {
                Id = order.Id,
                Lat = order.Lat,
                Long = order.Long,
                UserId = order.UserId,
                IsGuest = order.IsGuest,
                UserFullName = order.User?.FullName ?? (order.IsGuest ? "Guest" : "Unknown Customer"),
                UserEmail = order.User?.Email ?? string.Empty,
                // A guest has no user row, so the number captured at checkout is the only way to
                // reach them. Drivers read UserPhoneNumber regardless of which kind of order it is.
                UserPhoneNumber = !string.IsNullOrEmpty(order.User?.PhoneNumber)
                    ? order.User.PhoneNumber
                    : (!string.IsNullOrEmpty(order.ContactPhoneNumber) ? order.ContactPhoneNumber : "No Phone"),
                TotalAmount = order.TotalAmount,
                NetAmount = order.NetAmount,
                DeliveryAmount = order.DeliveryAmount,
                CleanerAmount = order.CleanerAmount,
                ReferenceNumber = order.ReferenceNumber,
                Status = order.Status.ToString(),
                Type = order.Type.ToString(),
                DeliveryWindowId = order.DeliveryWindowId,
                ScheduledDate = order.ScheduledDate,
                // The times agreed with the customer, not the window's current values.
                ScheduledStart = order.ScheduledStartTime?.ToString("HH:mm"),
                ScheduledEnd = order.ScheduledEndTime?.ToString("HH:mm"),
                CreatedAt = order.CreatedAt,
                TripId = order.TripId,
                TripReferenceNumber = order.Trip?.ReferenceNumber,
                CleanerId = order.Trip?.CleanerId,
                CleanerName = order.Trip?.Cleaner != null
                    ? BilingualText.Pick(order.Trip.Cleaner.NameAr, order.Trip.Cleaner.NameEn, language)
                    : null,
                // Carried on the order so delivery rounds can route from the
                // cleaner without a second call to fetch its coordinates.
                CleanerLat = order.Trip?.Cleaner?.Latitude,
                CleanerLng = order.Trip?.Cleaner?.Longitude,
                DriverName = order.Trip?.Driver != null
                    ? BilingualText.Pick(order.Trip.Driver.NameAr, order.Trip.Driver.NameEn, language)
                    : null,
                DriverPhoneNumber = order.Trip?.Driver?.PhoneNumber,
                MarketingCode = order.MarketingCode?.Code,
                MarketingDiscount = order.MarketingDiscount,
                MarketerShare = order.MarketerShare,
                MarketingDiscountPercentage = order.MarketingDiscountPercentage,
                MarketerSharePercentage = order.MarketerSharePercentage,
                Items = order.Items?.Select(i => new OrderItemDto
                {
                    Id = i.Id,
                    ItemType = ResolveItemTypeName(i.ItemType, itemTypes, language),
                    ServiceType = i.ServiceType.ToString(),
                    Quantity = i.Quantity,
                    // Read straight off the line, never recomputed from the current pricing
                    // tables: these are what this order was actually priced at.
                    UnitPrice = i.UnitPrice,
                    UnitCleanerPrice = i.UnitCleanerPrice,
                    LineTotal = i.UnitPrice * i.Quantity,
                    LineCleanerAmount = i.UnitCleanerPrice * i.Quantity
                }).ToList() ?? new List<OrderItemDto>()
            };
        }

        /// <summary>
        /// OrderItem stores whatever name the client's device language sent at collection
        /// time (see AddItemsToOrderAsync), not an ItemTypeId. To display it correctly
        /// regardless of the *viewer's* language, match it back to its ItemType by either
        /// name and re-pick the name for the current language. Falls back to the stored
        /// text if no match is found (e.g. the item type was later renamed or deleted).
        /// </summary>
        private static string ResolveItemTypeName(string stored, IEnumerable<ItemType>? itemTypes, string language)
        {
            var match = itemTypes?.FirstOrDefault(t =>
                t.TypeNameEn.Equals(stored, StringComparison.OrdinalIgnoreCase) ||
                t.TypeNameAr.Equals(stored, StringComparison.OrdinalIgnoreCase));
            return match != null ? BilingualText.Pick(match.TypeNameAr, match.TypeNameEn, language) : stored;
        }
    }
}
