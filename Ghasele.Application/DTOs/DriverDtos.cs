using System;

namespace Ghasele.Application.DTOs
{
    public class DriverDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string? Note { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateDriverDto
    {
        public string Name { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string? Note { get; set; }
    }

    public class UpdateDriverDto
    {
        public string? Name { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Note { get; set; }
    }
}
