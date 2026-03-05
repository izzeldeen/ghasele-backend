using System;

namespace Ghasele.Application.DTOs
{
    public class CleanerDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Note { get; set; }
        public string? CleaningLocation { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateCleanerDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Note { get; set; }
        public string? CleaningLocation { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }

    public class UpdateCleanerDto
    {
        public string? Name { get; set; }
        public string? Note { get; set; }
        public string? CleaningLocation { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
    }
}
