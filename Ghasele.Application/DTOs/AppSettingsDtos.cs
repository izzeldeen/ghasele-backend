using System;

namespace Ghasele.Application.DTOs
{
    public class AppSettingsDto
    {
        public decimal NormalDeliveryPrice { get; set; }
        public decimal ExpressDeliveryPrice { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class UpdateAppSettingsDto
    {
        public decimal NormalDeliveryPrice { get; set; }
        public decimal ExpressDeliveryPrice { get; set; }
    }

    /// <summary>
    /// Public slice of the settings handed to the customer app so it can label the
    /// Normal/Express choice with real prices. Deliberately separate from
    /// <see cref="AppSettingsDto"/> so admin-only settings added later are not leaked.
    /// </summary>
    public class DeliveryPricingDto
    {
        public decimal NormalDeliveryPrice { get; set; }
        public decimal ExpressDeliveryPrice { get; set; }
    }
}
