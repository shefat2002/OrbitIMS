namespace OrbitIMS.Data
{
    public enum OrderStatus
    {
        Pending=1,
        Processing,
        Shipped,
        delivered,
        Cancelled
    }
    public class Order : BaseEntity
    {
        public Order()
        {
            OrderDetails = new List<OrderDetails>();
        }
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        public decimal TotalAmount { get; set; }
        public OrderStatus Status { get; set; } = OrderStatus.Pending;
        public int CustomerId { get; set; }
        public Customer? Customer { get; set; }
        public List<OrderDetails> OrderDetails { get; set; } 
    }
}
