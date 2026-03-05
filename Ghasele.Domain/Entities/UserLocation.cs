using System;

namespace Ghasele.Domain.Entities
{
    public class UserLocation
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public double Lat { get; set; }
        public double Long { get; set; }
        
        public User? User { get; set; }
    }
}
