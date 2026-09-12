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
    public class DeliveryWindowService : IDeliveryWindowService
    {
        private readonly IDeliveryWindowRepository _repository;
        private readonly IOrderRepository _orderRepository;

        public DeliveryWindowService(IDeliveryWindowRepository repository, IOrderRepository orderRepository)
        {
            _repository = repository;
            _orderRepository = orderRepository;
        }

        public async Task<List<DeliveryWindowDto>> GetAllAsync()
        {
            var windows = await _repository.GetAllAsync();
            return windows.Select(MapToDto).ToList();
        }

        public async Task<DeliveryWindowDto> CreateAsync(CreateDeliveryWindowDto dto)
        {
            var start = ParseTime(dto.Start);
            var end = ParseTime(dto.End);
            Validate(start, end, dto.Capacity);

            var window = new DeliveryWindow
            {
                StartTime = start,
                EndTime = end,
                Capacity = dto.Capacity,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await _repository.AddAsync(window);
            return MapToDto(window);
        }

        public async Task<DeliveryWindowDto> UpdateAsync(Guid id, UpdateDeliveryWindowDto dto)
        {
            var window = await _repository.GetByIdAsync(id)
                         ?? throw AppException.NotFound(ErrorCodes.DeliveryWindowNotFound);

            var start = dto.Start != null ? ParseTime(dto.Start) : window.StartTime;
            var end = dto.End != null ? ParseTime(dto.End) : window.EndTime;
            var capacity = dto.Capacity ?? window.Capacity;
            Validate(start, end, capacity);

            window.StartTime = start;
            window.EndTime = end;
            window.Capacity = capacity;
            if (dto.IsActive.HasValue) window.IsActive = dto.IsActive.Value;

            await _repository.UpdateAsync(window);
            return MapToDto(window);
        }

        public async Task DeleteAsync(Guid id)
        {
            var window = await _repository.GetByIdAsync(id)
                         ?? throw AppException.NotFound(ErrorCodes.DeliveryWindowNotFound);
            await _repository.DeleteAsync(window.Id);
        }

        public async Task<List<DeliverySlotDto>> GetUpcomingSlotsAsync(int days)
        {
            if (days < 1) days = 1;
            if (days > 30) days = 30;

            var windows = await _repository.GetActiveAsync();

            // Amman's day, not UTC's. Between 21:00 and midnight local the two disagree, and
            // using UTC there would offer this morning's windows as if they were still ahead.
            var today = JordanTime.Today;
            var booked = await _orderRepository.GetBookedCountsAsync(today);

            var slots = new List<DeliverySlotDto>();
            for (var d = 0; d < days; d++)
            {
                var date = today.AddDays(d);
                foreach (var w in windows)
                {
                    // Today's earlier windows have already been driven; offering one would
                    // promise a collection that cannot happen.
                    if (!JordanTime.IsUpcoming(date, w.StartTime)) continue;

                    booked.TryGetValue((w.Id, date), out var count);

                    slots.Add(new DeliverySlotDto
                    {
                        WindowId = w.Id,
                        Date = date,
                        Start = w.StartTime.ToString("HH:mm"),
                        End = w.EndTime.ToString("HH:mm"),
                        Capacity = w.Capacity,
                        Booked = count,
                        Remaining = Math.Max(0, w.Capacity - count)
                    });
                }
            }
            return slots;
        }

        private static void Validate(TimeOnly start, TimeOnly end, int capacity)
        {
            if (end <= start)
                throw new AppException(ErrorCodes.DeliveryWindowInvalidRange);
            if (capacity < 1)
                throw new AppException(ErrorCodes.DeliveryWindowInvalidCapacity);
        }

        private static TimeOnly ParseTime(string value)
        {
            if (TimeOnly.TryParseExact(value, "HH:mm", CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out var parsed) ||
                TimeOnly.TryParse(value, CultureInfo.InvariantCulture, out parsed))
            {
                return parsed;
            }
            throw new AppException(ErrorCodes.DeliveryWindowInvalidRange);
        }

        private static DeliveryWindowDto MapToDto(DeliveryWindow w) => new()
        {
            Id = w.Id,
            Start = w.StartTime.ToString("HH:mm"),
            End = w.EndTime.ToString("HH:mm"),
            Capacity = w.Capacity,
            IsActive = w.IsActive,
            CreatedAt = w.CreatedAt
        };
    }
}
