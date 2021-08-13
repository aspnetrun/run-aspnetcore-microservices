using Ordering.Domain.Entities;
using Ordering.Application.Services;
using Ordering.Application.Model;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Ordering.Application.Contracts.Services
{
    public interface IOrderService
    {
        Task<IEnumerable<OrdersVm>> GetOrdersByUserName(string userName);
        
        Task<int> CreateOrder(OrdersVm order);

        Task<int> UpdateOrder(OrdersVm order);

        Task<int> DeleteOrder(int id);
    }
}