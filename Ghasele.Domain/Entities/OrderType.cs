namespace Ghasele.Domain.Entities
{
    /// <summary>
    /// How urgently the customer wants the order collected. Normal is 0 so every
    /// pre-existing order falls into it without a data-fill migration.
    /// </summary>
    public enum OrderType
    {
        Normal = 0,
        Express = 1
    }
}
