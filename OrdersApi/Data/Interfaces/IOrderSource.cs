using OrdersApi.Models;

namespace OrdersApi.Data.Interfaces
{
    public interface IOrderSource
    {
        IReadOnlyList<Order> GetOrders();
    }
}
