using Ordering.Domain.Entities;
using Ordering.Application.Services;
using Ordering.Application.Models;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Ordering.Application.Contracts.Services
{
    public interface IOrderService
    {
        Task<IEnumerable<OrdersVm>> GetOrdersByUserName(string userName);
        
        Task<int> CreateOrder(CheckoutOrderCommand order);

        Task<int> UpdateOrder(UpdateOrderCommand order);

        Task<int> DeleteOrder(int id);
    }
}