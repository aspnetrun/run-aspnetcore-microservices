using Ordering.Application.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ordering.Application.Interfaces
{
    interface IOrderService
    {
        Task<OrderModel> CheckOut(OrderModel order);
        Task<IEnumerable<OrderModel>> GetOrdersByUserName(string userName);
    }
}
