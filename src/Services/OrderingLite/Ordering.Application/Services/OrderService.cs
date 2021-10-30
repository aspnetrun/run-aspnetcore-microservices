using Ordering.Domain;
using Ordering.Application.Contracts.Services;
using Ordering.Application.Contracts.Persistence;
using Ordering.Application.Models;
using Ordering.Domain.Entities;
using Ordering.Application.Exceptions;
using Ordering.Application.Contracts.Infrastructure;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net;
using AutoMapper;

namespace Ordering.Application.Services 
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ILogger<OrderService> _logger;
        private readonly IMapper _mapper;
        private readonly IEmailService _emailService;

        public OrderService(IOrderRepository orderRepository, ILogger<OrderService> logger, IMapper mapper, IEmailService emailService)
        {
            _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
        }       

        public async Task<int> CreateOrder(CheckoutOrderVm order)
        {  
            var orderEntity = _mapper.Map<Order>(order);
            var newOrder = await _orderRepository.AddAsync(orderEntity);
            
            _logger.LogInformation($"Order {newOrder.Id} is successfully created.");
            
            await SendMail(newOrder);

            return newOrder.Id;
        }

        public async Task<IEnumerable<OrderVm>> GetOrdersByUserName(string userName)
        {
            var orderList = await _orderRepository.GetOrdersByUserName(userName);
            return _mapper.Map<List<OrderVm>>(orderList);
        }

        public async Task<int> UpdateOrder(OrderVm order)
        {
            var orderToUpdate = await _orderRepository.GetByIdAsync(order.Id);
            if (orderToUpdate == null)
            {
                throw new NotFoundException(nameof(Order), order.Id);
            }
            
            _mapper.Map(order, orderToUpdate, typeof(OrderVm), typeof(Order));

            await _orderRepository.UpdateAsync(orderToUpdate);

            _logger.LogInformation($"Order {orderToUpdate.Id} is successfully updated.");

            return orderToUpdate.Id;          
        }

        public async Task<int> DeleteOrder(int id)
        {
             var orderToDelete = await _orderRepository.GetByIdAsync(id);
            if (orderToDelete == null)
            {
                throw new NotFoundException(nameof(OrderVm), id);
            }            

            await _orderRepository.DeleteAsync(orderToDelete);

            _logger.LogInformation($"Order {orderToDelete.Id} is successfully deleted.");

            return orderToDelete.Id;
        }

        private async Task SendMail(Order order)
        {            
            var email = new Email() { To = "ezozkme@gmail.com", Body = $"Order was created.", Subject = "Order was created" };

            try
            {
                await _emailService.SendEmail(email);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Order {order.Id} failed due to an error with the mail service: {ex.Message}");
            }
        }

    }

}