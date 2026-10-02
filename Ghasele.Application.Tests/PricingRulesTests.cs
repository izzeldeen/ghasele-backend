using Ghasele.Application.Services;
using Xunit;

namespace Ghasele.Application.Tests;

/// <summary>
/// The arithmetic that decides what a customer is billed and what a laundry is paid.
/// </summary>
public class PricingRulesTests
{
    public class ResolveUnitPrice
    {
        [Fact]
        public void The_price_the_driver_settled_on_wins()
        {
            Assert.Equal(7.50m, PricingRules.ResolveUnitPrice(7.50m, cataloguePrice: 3.00m));
        }

        [Fact]
        public void An_empty_box_falls_back_to_the_catalogue_price()
        {
            Assert.Equal(3.00m, PricingRules.ResolveUnitPrice(null, cataloguePrice: 3.00m));
        }

        [Fact]
        public void Zero_is_a_real_price_and_is_honoured()
        {
            // A driver writing off an item is a decision, not a blank field.
            Assert.Equal(0m, PricingRules.ResolveUnitPrice(0m, cataloguePrice: 3.00m));
        }

        [Fact]
        public void A_negative_price_is_ignored_rather_than_crediting_the_customer()
        {
            Assert.Equal(3.00m, PricingRules.ResolveUnitPrice(-5m, cataloguePrice: 3.00m));
        }
    }

    public class CleanerCost
    {
        [Theory]
        [InlineData(50, 10.00, 5.00)]   // the standard half share
        [InlineData(60, 10.00, 6.00)]   // a laundry on better terms
        [InlineData(40, 12.50, 5.00)]
        [InlineData(100, 8.00, 8.00)]   // everything goes to the laundry
        [InlineData(0, 8.00, 0.00)]     // nothing does
        public void The_laundry_gets_its_agreed_share(decimal share, decimal price, decimal expected)
        {
            Assert.Equal(expected, PricingRules.CleanerCost(price, share));
        }

        [Fact]
        public void Two_laundries_on_different_terms_are_paid_differently_for_the_same_line()
        {
            const decimal price = 10.00m;

            Assert.Equal(5.00m, PricingRules.CleanerCost(price, 50m));
            Assert.Equal(6.00m, PricingRules.CleanerCost(price, 60m));
        }

        [Fact]
        public void A_fraction_of_a_fils_rounds_to_the_laundry()
        {
            // 1.25 * 33% = 0.4125, which is not payable - it rounds up, not down.
            Assert.Equal(0.41m, PricingRules.CleanerCost(1.25m, 33m));
            // 3.33 * 50% = 1.665, the midpoint case: away from zero, so 1.67.
            Assert.Equal(1.67m, PricingRules.CleanerCost(3.33m, 50m));
        }

        [Fact]
        public void The_result_is_always_payable_to_the_fils()
        {
            var cost = PricingRules.CleanerCost(9.99m, 37m);

            Assert.Equal(decimal.Round(cost, 2), cost);
        }

        [Theory]
        [InlineData(150, 10.00, 10.00)]  // clamped to 100: never more than the customer paid
        [InlineData(-20, 10.00, 0.00)]   // clamped to 0: never a negative payout
        public void A_share_that_is_not_a_percentage_is_clamped(decimal share, decimal price, decimal expected)
        {
            Assert.Equal(expected, PricingRules.CleanerCost(price, share));
        }
    }
}
