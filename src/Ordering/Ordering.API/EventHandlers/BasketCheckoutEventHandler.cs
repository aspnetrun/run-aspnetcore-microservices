using EventBusRabbitMQ.Events;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ordering.API.EventHandlers
{
    public class BasketCheckoutEventHandler
    {
        private readonly IOrderService _orderService;
        private readonly ILogger<BasketCheckoutEventHandler> _logger;

        public BasketCheckoutEventHandler(IOrderService orderService, ILogger<BasketCheckoutEventHandler> logger)
        {
            _orderService = orderService ?? throw new ArgumentNullException(nameof(orderService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task Handle(BasketCheckoutEvent @event)
        {
            //var order = _mapping order object
            //await _orderService.CheckOut(order);
        }        
    }

}
