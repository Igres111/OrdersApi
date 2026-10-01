using OrdersApi.Models;

namespace OrdersApi.Services.Interfaces
{
    public interface IOrderService
    {
        IReadOnlyList<Order> GetAllOrders();
        IReadOnlyList<Order> GetOrdersByCustomer(string customer);
        OrderStatistics GetStatistics();
    }
}
