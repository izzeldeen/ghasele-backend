using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ghasele.Application.DTOs;
using Ghasele.Application.Exceptions;
using Ghasele.Application.Interfaces;
using Ghasele.Application.Localization;
using Ghasele.Domain.Entities;
using Ghasele.Domain.Interfaces;

namespace Ghasele.Application.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IItemTypeRepository _itemTypeRepository;
        private readonly IMarketingCodeRepository _marketingCodeRepository;
        private readonly ITripRepository _tripRepository;
        private readonly IAppSettingsRepository _appSettingsRepository;
        private readonly ICurrentLanguageProvider _language;

        public OrderService(IOrderRepository orderRepository, IItemTypeRepository itemTypeRepository, IMarketingCodeRepository marketingCodeRepository, ITripRepository tripRepository, IAppSettingsRepository appSettingsRepository, ICurrentLanguageProvider language)
        {
            _orderRepository = orderRepository;
            _itemTypeRepository = itemTypeRepository;
            _marketingCodeRepository = marketingCodeRepository;
            _tripRepository = tripRepository;
            _appSettingsRepository = appSettingsRepository;
            _language = language;
        }

        public async Task<OrderDto> CreateOrderAsync(CreateOrderDto dto)
        {
            // Customers may now place a new order while an earlier one is still being
            // processed. The old one-open-order-at-a-time rule was removed by product;
            // HasPendingOrderAsync is still exposed on the repository for reporting.

            var orderType = OrderType.Normal;
            if (!string.IsNullOrEmpty(dto.Type) && Enum.TryParse<OrderType>(dto.Type, true, out var parsedType))
            {
                orderType = parsedType;
            }

            // Stamp the fee at order time from the admin-managed settings, so a price change
            // between ordering and collection cannot charge the customer more than they were
            // quoted. The client's DeliveryAmount is ignored - it is not authoritative.
            var settings = await _appSettingsRepository.GetAsync();
            var deliveryAmount = orderType == OrderType.Express
                ? settings.ExpressDeliveryPrice
                : settings.NormalDeliveryPrice;

            var order = new Order
            {
                Lat = dto.Lat,
                Long = dto.Long,
                UserId = dto.UserId,
                TotalAmount = dto.TotalAmount,
                NetAmount = dto.NetAmount,
                DeliveryAmount = deliveryAmount,
                CleanerAmount = dto.CleanerAmount,
                ReferenceNumber = $"GH-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N").Substring(0, 4).ToUpper()}",
                Status = OrderStatus.PendingCollection,
                Type = orderType,
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

        public async Task<List<OrderDto>> GetUserOrdersAsync(Guid userId, int page = 1, int pageSize = 20)
        {
            var orders = await _orderRepository.GetByUserIdAsync(userId, page, pageSize);
            var itemTypes = await _itemTypeRepository.GetAllAsync();
            return orders.Select(o => MapToDto(o, _language.Language, itemTypes)).ToList();
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
                    decimal price = 0;
                    decimal cost = 0;

                    switch (serviceType)
                    {
                        case ServiceType.Cleaning:
                            price = itemType.CleaningPrice;
                            cost = itemType.CleaningCost;
                            break;
                        case ServiceType.Both:
                            price = itemType.BothPrice;
                            cost = itemType.BothCost;
                            break;
                        default: // Iron
                            price = itemType.IronPrice;
                            cost = itemType.IronCost;
                            break;
                    }

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
            return MapToDto(updatedOrder!, _language.Language, allItemTypes);
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
                UserFullName = order.User?.FullName ?? "Unknown Customer",
                UserEmail = order.User?.Email ?? string.Empty,
                UserPhoneNumber = !string.IsNullOrEmpty(order.User?.PhoneNumber) ? order.User.PhoneNumber : "No Phone",
                TotalAmount = order.TotalAmount,
                NetAmount = order.NetAmount,
                DeliveryAmount = order.DeliveryAmount,
                CleanerAmount = order.CleanerAmount,
                ReferenceNumber = order.ReferenceNumber,
                Status = order.Status.ToString(),
                Type = order.Type.ToString(),
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
                    Quantity = i.Quantity
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
