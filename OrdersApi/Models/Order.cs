using OrdersApi.Enums;

namespace OrdersApi.Models
{
    public class Order
    {
        public int OrderId { get; set; }
        public string Customer { get; set; } = string.Empty;
        public OrderStatus Status { get; set; }
        public List<OrderItem> Items { get; set; } = new();
    }
}
