using Ordering.Domain;
using Ordering.Infrastructure;

namespace Ordering.Application.Services 
{
    public class Order : IOrderService
    {
        private readonly IOrderRepository _orderRepository;

         public Order(IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }       

        public async Task<int> CreateOrder(Order order)
        {  
            return null;          
        }

        public async Task<ActionResult<IEnumerable<OrdersVm>>> GetOrdersByUserName(string userName)
        {
            return null;
        }

        public async Task<ActionResult> UpdateOrder(Order order)
        {
            return null;          
        }

        public async Task<ActionResult> DeleteOrder(int id)
        {
            return null;
        }

    }

}