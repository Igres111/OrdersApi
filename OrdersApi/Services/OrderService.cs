using OrdersApi.Data.Interfaces;
using OrdersApi.Enums;
using OrdersApi.Models;
using OrdersApi.Services.Interfaces;

namespace OrdersApi.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderSource _orderSource;

        public OrderService(IOrderSource orderSource)
        {
            _orderSource = orderSource;
        }

        public IReadOnlyList<Order> GetAllOrders() => _orderSource.GetOrders();

        public IReadOnlyList<Order> GetOrdersByCustomer(string customer)
        {
            if (string.IsNullOrWhiteSpace(customer))
            {
                throw new ArgumentException("Customer name is required.", nameof(customer));
            }

            var name = customer.Trim();

            return _orderSource.GetOrders()
                .Where(o => string.Equals(o.Customer.Trim(), name, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        public OrderStatistics GetStatistics()
        {
            var completedOrders = _orderSource.GetOrders()
                .Where(o => o.Status == OrderStatus.Completed)
                .ToList();

            var orderTotals = completedOrders.Select(CalculateOrderTotal).ToList();

            var totalSales = orderTotals.Sum();

            decimal averageOrderValue = 0;

            if (orderTotals.Count > 0)
            {
                averageOrderValue = Math.Round(orderTotals.Average(), 2, MidpointRounding.AwayFromZero);
            }

            var quantityByProduct = completedOrders
                .SelectMany(o => o.Items)
                .GroupBy(i => i.Product, StringComparer.OrdinalIgnoreCase)
                .Select(g => new { Product = g.Key, Quantity = g.Sum(i => i.Quantity) })
                .ToList();

            int topQuantity = 0;

            if (quantityByProduct.Count > 0)
            {
                topQuantity = quantityByProduct.Max(p => p.Quantity);
            }

            var mostPopularProducts = quantityByProduct
                .Where(p => p.Quantity == topQuantity && topQuantity > 0)
                .Select(p => p.Product)
                .OrderBy(p => p, StringComparer.OrdinalIgnoreCase)
                .ToList();

            return new OrderStatistics
            {
                CompletedOrdersCount = completedOrders.Count,
                TotalSales = totalSales,
                AverageOrderValue = averageOrderValue,
                MostPopularProducts = mostPopularProducts,
                MostPopularQuantity = topQuantity
            };
        }

        private static decimal CalculateOrderTotal(Order order) => order.Items.Sum(i => i.Quantity * i.Price);

    }
}
