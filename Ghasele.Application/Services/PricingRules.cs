using System;

namespace Ghasele.Application.Services
{
    /// <summary>
    /// The two money rules that decide what an order line is worth: what the customer pays,
    /// and what the laundry is paid out of it.
    /// </summary>
    /// <remarks>
    /// Pulled out of <see cref="OrderService"/> and kept pure so both can be exercised
    /// directly. Pricing a line otherwise sits behind an order, a trip, a laundry and four
    /// repositories, and the arithmetic that actually moves money is the last thing that
    /// should only be reachable through all of that.
    /// </remarks>
    public static class PricingRules
    {
        /// <summary>
        /// What one unit costs the customer: the figure settled at the door when there is
        /// one, and the item's catalogue price when there is not.
        /// </summary>
        /// <param name="enteredPrice">
        /// What the driver typed with the garment in hand, or null if they left it empty.
        /// A negative figure is treated as absent - it would credit the customer and pay the
        /// laundry a negative amount.
        /// </param>
        /// <param name="cataloguePrice">The item type's base price.</param>
        public static decimal ResolveUnitPrice(decimal? enteredPrice, decimal cataloguePrice)
        {
            return enteredPrice is decimal entered && entered >= 0 ? entered : cataloguePrice;
        }

        /// <summary>
        /// What the laundry is paid for one unit: their agreed share of what the customer
        /// pays, rounded to the fils.
        /// </summary>
        /// <remarks>
        /// Rounded away from zero, so a half fils goes to the laundry rather than to us.
        /// It is a rounding rule that costs almost nothing and cannot be read as shaving the
        /// people doing the work.
        /// <para>
        /// A share outside 0-100 is clamped rather than trusted. The API refuses to store
        /// one, but a row written before that check existed must not be able to pay out more
        /// than the customer paid, or a negative amount.
        /// </para>
        /// </remarks>
        public static decimal CleanerCost(decimal unitPrice, decimal sharePercentage)
        {
            var share = Math.Clamp(sharePercentage, 0m, 100m);
            return decimal.Round(unitPrice * share / 100m, 2, MidpointRounding.AwayFromZero);
        }
    }
}
