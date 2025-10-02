using System.ComponentModel.DataAnnotations.Schema;

namespace OrbitIMS.Data
{
    public class OrderDetails : BaseEntity
    {
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
        [ForeignKey("Order")]
        public int OrderId { get; set; }
        public Order? Order { get; set; }
        [ForeignKey("Product")]
        public int ProductId { get; set; }
        public Product? Product { get; set; }
    }
}
