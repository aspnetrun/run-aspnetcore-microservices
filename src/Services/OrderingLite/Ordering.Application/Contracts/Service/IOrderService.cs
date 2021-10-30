using Ordering.Domain.Entities;
using Ordering.Application.Services;
using Ordering.Application.Models;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Ordering.Application.Contracts.Services
{
    public interface IOrderService
    {
        Task<IEnumerable<OrderVm>> GetOrdersByUserName(string userName);
        
        Task<int> CreateOrder(CheckoutOrderVm order);

        Task<int> UpdateOrder(OrderVm order);

        Task<int> DeleteOrder(int id);
    }
}