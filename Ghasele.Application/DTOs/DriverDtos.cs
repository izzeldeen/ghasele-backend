using System;

namespace Ghasele.Application.DTOs
{
    public class DriverDto
    {
        public Guid Id { get; set; }
        public string NameAr { get; set; } = string.Empty;
        public string NameEn { get; set; } = string.Empty;
        /// <summary>NameAr or NameEn, chosen for the request's language - for callers that just want a name to display.</summary>
        public string Name { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string? Note { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid? UserId { get; set; }
    }

    public class CreateDriverDto
    {
        public string NameAr { get; set; } = string.Empty;
        public string NameEn { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string? Note { get; set; }
        public string Password { get; set; } = string.Empty;
    }

    public class UpdateDriverDto
    {
        public string? NameAr { get; set; }
        public string? NameEn { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Note { get; set; }
    }
}
