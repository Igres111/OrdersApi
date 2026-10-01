using OrdersApi.Data.Interfaces;
using OrdersApi.Enums;
using OrdersApi.Models;
using OrdersApi.Services;

namespace OrdersApi.Tests
{
    public class OrderServiceTests
    {
        private static List<Order> SampleOrders() => new()
        {
            CreateOrder(1, "Nino", OrderStatus.Completed, ("Keyboard", 2, 50m), ("Mouse", 1, 25m)),
            CreateOrder(2, "Giorgi", OrderStatus.Cancelled, ("Keyboard", 1, 50m)),
            CreateOrder(3, "Nino", OrderStatus.Completed, ("Mouse", 2, 25m), ("Monitor", 1, 200m)),
            CreateOrder(4, "Ana", OrderStatus.Completed, ("Keyboard", 1, 50m))
        };

        [Fact]
        public void GetAllOrders_ReturnsAllOrdersIncludingCancelled()
        {
            var service = CreateService(SampleOrders());

            var orders = service.GetAllOrders();

            Assert.Equal(new[] { 1, 2, 3, 4 }, orders.Select(o => o.OrderId));
        }

        [Theory]
        [InlineData("Nino")]
        [InlineData("nino")]
        [InlineData("  NINO  ")]
        public void GetOrdersByCustomer_IgnoresCaseAndSurroundingSpaces(string customer)
        {
            var service = CreateService(SampleOrders());

            var orders = service.GetOrdersByCustomer(customer);

            Assert.Equal(new[] { 1, 3 }, orders.Select(o => o.OrderId));
        }

        [Fact]
        public void GetOrdersByCustomer_UnknownCustomer_ReturnsEmptyList()
        {
            var service = CreateService(SampleOrders());

            var orders = service.GetOrdersByCustomer("Luka");

            Assert.Empty(orders);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public void GetOrdersByCustomer_BlankName_ThrowsArgumentException(string? customer)
        {
            var service = CreateService(SampleOrders());

            Assert.Throws<ArgumentException>(() => service.GetOrdersByCustomer(customer!));
        }

        [Fact]
        public void GetStatistics_ExcludesCancelledOrders()
        {
            var service = CreateService(SampleOrders());

            var statistics = service.GetStatistics();

            Assert.Equal(3, statistics.CompletedOrdersCount);
            Assert.Equal(425m, statistics.TotalSales);
        }

        [Fact]
        public void GetStatistics_CalculatesAverageOrderValueRoundedToTwoDecimals()
        {
            var service = CreateService(SampleOrders());

            var statistics = service.GetStatistics();

            Assert.Equal(141.67m, statistics.AverageOrderValue);
        }

        [Fact]
        public void GetStatistics_AverageMidpoint_RoundsAwayFromZero()
        {
            var service = CreateService(new List<Order>
            {
                CreateOrder(1, "Nino", OrderStatus.Completed, ("Cable", 1, 0.12m)),
                CreateOrder(2, "Ana", OrderStatus.Completed, ("Cable", 1, 0.13m))
            });

            var statistics = service.GetStatistics();

            Assert.Equal(0.13m, statistics.AverageOrderValue);
        }

        [Fact]
        public void GetStatistics_WhenProductsTie_ReturnsAllOfThemAlphabetically()
        {
            var service = CreateService(SampleOrders());

            var statistics = service.GetStatistics();

            Assert.Equal(new[] { "Keyboard", "Mouse" }, statistics.MostPopularProducts);
            Assert.Equal(3, statistics.MostPopularQuantity);
        }

        [Fact]
        public void GetStatistics_CancelledOrdersDoNotAffectMostPopularProduct()
        {
            var service = CreateService(new List<Order>
            {
                CreateOrder(1, "Nino", OrderStatus.Completed, ("Mouse", 2, 25m)),
                CreateOrder(2, "Giorgi", OrderStatus.Cancelled, ("Keyboard", 10, 50m))
            });

            var statistics = service.GetStatistics();

            Assert.Equal(new[] { "Mouse" }, statistics.MostPopularProducts);
            Assert.Equal(2, statistics.MostPopularQuantity);
        }

        [Fact]
        public void GetStatistics_GroupsProductNamesIgnoringCase()
        {
            var service = CreateService(new List<Order>
            {
                CreateOrder(1, "Nino", OrderStatus.Completed, ("Keyboard", 1, 50m), ("Mouse", 2, 25m)),
                CreateOrder(2, "Ana", OrderStatus.Completed, ("keyboard", 2, 50m))
            });

            var statistics = service.GetStatistics();

            Assert.Equal(new[] { "Keyboard" }, statistics.MostPopularProducts);
            Assert.Equal(3, statistics.MostPopularQuantity);
        }

        [Fact]
        public void GetStatistics_WhenNoCompletedOrders_ReturnsZeros()
        {
            var service = CreateService(new List<Order>
            {
                CreateOrder(1, "Giorgi", OrderStatus.Cancelled, ("Keyboard", 1, 50m))
            });

            var statistics = service.GetStatistics();

            Assert.Equal(0, statistics.CompletedOrdersCount);
            Assert.Equal(0m, statistics.TotalSales);
            Assert.Equal(0m, statistics.AverageOrderValue);
            Assert.Empty(statistics.MostPopularProducts);
            Assert.Equal(0, statistics.MostPopularQuantity);
        }

        private static OrderService CreateService(List<Order> orders) => new(new FakeOrderSource(orders));

        private static Order CreateOrder(int id, string customer, OrderStatus status,
            params (string Product, int Quantity, decimal Price)[] items) => new()
        {
            OrderId = id,
            Customer = customer,
            Status = status,
            Items = items
                .Select(i => new OrderItem { Product = i.Product, Quantity = i.Quantity, Price = i.Price })
                .ToList()
        };

        private class FakeOrderSource : IOrderSource
        {
            private readonly List<Order> _orders;

            public FakeOrderSource(List<Order> orders)
            {
                _orders = orders;
            }

            public IReadOnlyList<Order> GetOrders() => _orders;
        }
    }
}
