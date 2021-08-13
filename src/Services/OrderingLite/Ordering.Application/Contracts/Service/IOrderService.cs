using Ordering.Domain.Entities;

namespace Ordering.Application.Contracts.Services
{
    public interface IOrderService
    {
        Task<IEnumerable<Order>> GetOrdersByUserName(string userName);
        Task<int> CreateOrder(Order order);

        Task<ActionResult<IEnumerable<OrdersVm>>> GetOrdersByUserName(string userName);

        Task<ActionResult> UpdateOrder(Order order);

        Task<ActionResult> DeleteOrder(int id);
    }
}