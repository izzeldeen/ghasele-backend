using System;

namespace Ghasele.Domain.Entities
{
    /// <summary>
    /// A kind of garment and what the customer pays to have it done.
    /// </summary>
    /// <remarks>
    /// Customer prices only. What we pay a laundry is a share of that price, agreed per
    /// laundry - see <see cref="Cleaner.SharePercentage"/>.
    /// <para>
    /// There is one service: washing and ironing together. Ironing alone and washing alone
    /// were dropped because every order in practice wanted both, and three prices per item
    /// meant three chances for an operator to leave one wrong.
    /// </para>
    /// </remarks>
    public class ItemType
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string TypeNameAr { get; set; } = string.Empty;
        public string TypeNameEn { get; set; } = string.Empty;

        /// <summary>
        /// The least washing and ironing one of these can cost, in JOD.
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// The most it can cost, or null when the price is open-ended.
        /// </summary>
        /// <remarks>
        /// A catalogue price is a quote, not a bill - what an evening dress costs is not
        /// knowable until a driver has the garment in hand. The pair decides how the price
        /// reads to the customer:
        /// <list type="table">
        /// <item><term>null</term><description>"starts from 3 JOD" - open-ended</description></item>
        /// <item><term>equal to Price</term><description>"1.25 JOD" - a fixed price</description></item>
        /// <item><term>above Price</term><description>"from 3 to 10 JOD" - a quoted range</description></item>
        /// </list>
        /// Nullable rather than defaulting to the base: a price that has never been given a
        /// ceiling and one deliberately fixed at the base are different promises, and a
        /// non-nullable column could not tell them apart.
        /// </remarks>
        public decimal? MaxPrice { get; set; }

        /// <summary>
        /// Where this sits in the catalogue the customer reads, lowest first.
        /// </summary>
        /// <remarks>
        /// The pricing page in the app renders the list in the order the API hands it over,
        /// so this is the only thing deciding what a customer sees at the top. Ties break on
        /// the English name, which keeps the order stable for rows that have never been
        /// arranged - everything starts at 0, so an untouched catalogue reads alphabetically
        /// rather than in whatever order the rows happen to come back from the database.
        /// </remarks>
        public int SortOrder { get; set; }

        public bool IsDeleted { get; set; } = false;
    }
}
