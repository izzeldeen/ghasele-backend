namespace Ghasele.Domain.Entities
{
    public enum OrderStatus
    {
        PendingCollection,
        Assigned,
        Collected,
        Cleaning,
        Ready,
        OutForDelivery,
        Delivered,
        Cancelled
    }
}
