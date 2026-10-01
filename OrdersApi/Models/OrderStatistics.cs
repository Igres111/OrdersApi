namespace OrdersApi.Models
{
    public class OrderStatistics
    {
        public int CompletedOrdersCount { get; set; }
        public decimal TotalSales { get; set; }
        public decimal AverageOrderValue { get; set; }
        public List<string> MostPopularProducts { get; set; } = new();
        public int MostPopularQuantity { get; set; }
    }
}
