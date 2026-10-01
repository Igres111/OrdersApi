using Microsoft.AspNetCore.Mvc;
using OrdersApi.Models;
using OrdersApi.Services.Interfaces;

namespace OrdersApi.Controllers
{
    [ApiController]
    [Route("api/orders")]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrdersController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        /// <summary>
        /// Returns all orders, including cancelled ones.
        /// </summary>
        [HttpGet]
        public ActionResult<IReadOnlyList<Order>> GetAll()
        {
            return Ok(_orderService.GetAllOrders());
        }

        /// <summary>
        /// Returns the orders of one customer. The name is matched ignoring case and surrounding spaces.
        /// </summary>
        /// <param name="name">Customer name, e.g. "Nino".</param>
        /// <returns>The customer's orders, or an empty list if there are none.</returns>
        [HttpGet("customer/{name}")]
        public ActionResult<IReadOnlyList<Order>> GetByCustomer([FromRoute] string name)
        {
            return Ok(_orderService.GetOrdersByCustomer(name));
        }

        /// <summary>
        /// Returns statistics for completed orders only: order count, total sales, average order value,
        /// and the most popular product(s) by quantity sold. Cancelled orders are excluded.
        /// </summary>
        [HttpGet("statistics")]
        public ActionResult<OrderStatistics> GetStatistics()
        {
            return Ok(_orderService.GetStatistics());
        }
    }
}
