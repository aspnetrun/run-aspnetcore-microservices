using AutoMapper;
using EventBus.Messages.Events;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Ordering.Application.Models;
using Ordering.Application.Contracts.Services;

namespace Ordering.API.EventBusConsumer
{
    public class BasketCheckoutConsumer : IConsumer<BasketCheckoutEvent>
    {
        //private readonly IMediator _mediator;
        private readonly IOrderService _orderService;

        private readonly IMapper _mapper;
        private readonly ILogger<BasketCheckoutConsumer> _logger;

        public BasketCheckoutConsumer(IOrderService orderService, IMapper mapper, ILogger<BasketCheckoutConsumer> logger)
        {
            //_mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _orderService = orderService ?? throw new ArgumentNullException(nameof(orderService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task Consume(ConsumeContext<BasketCheckoutEvent> context)
        {
            var command = _mapper.Map<CheckoutOrderCommand>(context.Message);            
            //var result = await _mediator.Send(command);
            var result = await _orderService.CreateOrder(command);

            _logger.LogInformation("BasketCheckoutEvent consumed successfully. Created Order Id : {newOrderId}", result);
        }
    }
}
