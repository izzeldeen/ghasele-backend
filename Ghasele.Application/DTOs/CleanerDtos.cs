using System;

namespace Ghasele.Application.DTOs
{
    public class CleanerDto
    {
        public Guid Id { get; set; }
        public string NameAr { get; set; } = string.Empty;
        public string NameEn { get; set; } = string.Empty;
        /// <summary>NameAr or NameEn, chosen for the request's language - for callers that just want a name to display.</summary>
        public string Name { get; set; } = string.Empty;
        public string? Note { get; set; }
        public string? CleaningLocation { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateCleanerDto
    {
        public string NameAr { get; set; } = string.Empty;
        public string NameEn { get; set; } = string.Empty;
        public string? Note { get; set; }
        public string? CleaningLocation { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }

    public class UpdateCleanerDto
    {
        public string? NameAr { get; set; }
        public string? NameEn { get; set; }
        public string? Note { get; set; }
        public string? CleaningLocation { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
    }
}
