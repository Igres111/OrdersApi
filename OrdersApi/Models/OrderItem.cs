namespace OrdersApi.Models
{
    public class OrderItem
    {
        public string Product { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }
}
