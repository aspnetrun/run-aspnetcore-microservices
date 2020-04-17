using AspnetRunBasics.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AspnetRunBasics.Repositories
{
    public interface IOrderRepository
    {
        Task<Order> CheckOut(Order order);
        Task<IEnumerable<Order>> GetOrdersByUserName(string userName);
    }
}
