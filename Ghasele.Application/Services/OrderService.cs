using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ghasele.Application.DTOs;
using Ghasele.Application.Interfaces;
using Ghasele.Domain.Entities;
using Ghasele.Domain.Interfaces;

namespace Ghasele.Application.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IItemTypeRepository _itemTypeRepository;
        private readonly IMarketingCodeRepository _marketingCodeRepository;

        public OrderService(IOrderRepository orderRepository, IItemTypeRepository itemTypeRepository, IMarketingCodeRepository marketingCodeRepository)
        {
            _orderRepository = orderRepository;
            _itemTypeRepository = itemTypeRepository;
            _marketingCodeRepository = marketingCodeRepository;
        }

        public async Task<OrderDto> CreateOrderAsync(CreateOrderDto dto)
        {
            if (await _orderRepository.HasPendingOrderAsync(dto.UserId))
            {
                throw new Exception("You already have a pending order. Please wait for it to be processed.");
            }

            var order = new Order
            {
                Lat = dto.Lat,
                Long = dto.Long,
                UserId = dto.UserId,
                TotalAmount = dto.TotalAmount,
                NetAmount = dto.NetAmount,
                DeliveryAmount = dto.DeliveryAmount,
                CleanerAmount = dto.CleanerAmount,
                ReferenceNumber = $"GH-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N").Substring(0, 4).ToUpper()}",
                Status = OrderStatus.Pending,
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

            return MapToDto(order);
        }

        public async Task<List<OrderDto>> GetUserOrdersAsync(Guid userId)
        {
            var orders = await _orderRepository.GetByUserIdAsync(userId);
            return orders.Select(MapToDto).ToList();
        }

        public async Task<List<OrderDto>> GetAllOrdersAsync(string? status = null, string? searchTerm = null)
        {
            OrderStatus? orderStatus = null;
            if (!string.IsNullOrEmpty(status) && Enum.TryParse<OrderStatus>(status, true, out var parsedStatus))
            {
                orderStatus = parsedStatus;
            }

            var orders = await _orderRepository.GetAllAsync(orderStatus, searchTerm);
            return orders.Select(MapToDto).ToList();
        }

        public async Task<OrderDto?> GetOrderByIdAsync(Guid id)
        {
            var order = await _orderRepository.GetByIdAsync(id);
            return order != null ? MapToDto(order) : null;
        }

        public async Task<OrderDto> AddItemsToOrderAsync(Guid orderId, AddOrderItemsDto dto)
        {
            // Load order with minimal includes to avoid tracking conflicts
            var order = await _orderRepository.GetByIdForUpdateAsync(orderId);
            if (order == null)
            {
                throw new Exception("Order not found");
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

                // Find price
                var itemType = allItemTypes.FirstOrDefault(t => t.TypeName.Equals(itemDto.ItemType, StringComparison.OrdinalIgnoreCase));
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

            // Update amounts
            order.DeliveryAmount = 1.0m; // Fixed delivery amount
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

            // Reload with all includes for the DTO
            var updatedOrder = await _orderRepository.GetByIdAsync(orderId);
            return MapToDto(updatedOrder!);
        }

        public async Task<OrderDto> UpdateOrderAsync(Guid id, UpdateOrderDto dto)
        {
            var order = await _orderRepository.GetByIdAsync(id);
            if (order == null) throw new Exception("Order not found");

            if (dto.Lat.HasValue) order.Lat = dto.Lat.Value;
            if (dto.Long.HasValue) order.Long = dto.Long.Value;
            if (dto.CleanerAmount.HasValue) order.CleanerAmount = dto.CleanerAmount.Value;
            if (!string.IsNullOrEmpty(dto.Status) && Enum.TryParse<OrderStatus>(dto.Status, true, out var status))
            {
                order.Status = status;
            }

            await _orderRepository.UpdateAsync(order);
            return MapToDto(order);
        }

        public async Task DeleteOrderAsync(Guid id)
        {
            await _orderRepository.DeleteAsync(id);
        }

        public static OrderDto MapToDto(Order order)
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
                CreatedAt = order.CreatedAt,
                TripId = order.TripId,
                TripReferenceNumber = order.Trip?.ReferenceNumber,
                CleanerName = order.Trip?.Cleaner?.Name,
                DriverName = order.Trip?.Driver?.Name,
                DriverPhoneNumber = order.Trip?.Driver?.PhoneNumber,
                MarketingCode = order.MarketingCode?.Code,
                MarketingDiscount = order.MarketingDiscount,
                MarketerShare = order.MarketerShare,
                MarketingDiscountPercentage = order.MarketingDiscountPercentage,
                MarketerSharePercentage = order.MarketerSharePercentage,
                Items = order.Items?.Select(i => new OrderItemDto
                {
                    Id = i.Id,
                    ItemType = i.ItemType,
                    ServiceType = i.ServiceType.ToString(),
                    Quantity = i.Quantity
                }).ToList() ?? new List<OrderItemDto>()
            };
        }
    }
}
