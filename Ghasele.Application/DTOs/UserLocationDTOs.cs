using System;

namespace Ghasele.Application.DTOs
{
    public class UserLocationDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public double Lat { get; set; }
        public double Long { get; set; }
    }

    public class CreateUserLocationDto
    {
        public Guid UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public double Lat { get; set; }
        public double Long { get; set; }
    }
}
