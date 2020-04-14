//using AutoMapper;
//using EventBusRabbitMQ.Events;
//using Microsoft.Extensions.Logging;
//using Ordering.Application.Interfaces;
//using Ordering.Application.Models;
//using System;
//using System.Threading.Tasks;

//namespace Ordering.API.EventHandlers
//{
//    public class BasketCheckoutEventHandler
//    {
//        private readonly IOrderService _orderService;
//        private readonly IMapper _mapper;
//        private readonly ILogger<BasketCheckoutEventHandler> _logger;

//        public BasketCheckoutEventHandler(IOrderService orderService, IMapper mapper, ILogger<BasketCheckoutEventHandler> logger)
//        {
//            _orderService = orderService ?? throw new ArgumentNullException(nameof(orderService));
//            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
//            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
//        }

//        public async Task Handle(BasketCheckoutEvent @event)
//        {
//            var order = _mapper.Map<OrderModel>(@event);

//            try
//            {
//                await _orderService.CheckOut(order);
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "ERROR Order service checkout operation.");
//                throw;
//            }
//        }
//    }

//}
