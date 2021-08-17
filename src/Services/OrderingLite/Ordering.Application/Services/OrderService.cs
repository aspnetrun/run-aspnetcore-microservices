using Ordering.Domain;
using Ordering.Application.Contracts.Services;
using Ordering.Application.Contracts.Persistence;
using Ordering.Application.Models;
using Ordering.Domain.Entities;
using Ordering.Application.Exceptions;
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

         public OrderService(IOrderRepository orderRepository, ILogger<OrderService> logger, IMapper mapper)
        {
            _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }       

        public async Task<int> CreateOrder(CheckoutOrderCommand order)
        {  
            var orderEntity = _mapper.Map<Order>(order);
            var newOrder = await _orderRepository.AddAsync(orderEntity);
            
            _logger.LogInformation($"Order {newOrder.Id} is successfully created.");
            
            //await SendMail(newOrder);

            return newOrder.Id;
        }

        public async Task<IEnumerable<OrdersVm>> GetOrdersByUserName(string userName)
        {
            var orderList = await _orderRepository.GetOrdersByUserName(userName);
            return _mapper.Map<List<OrdersVm>>(orderList);
        }

        public async Task<int> UpdateOrder(UpdateOrderCommand order)
        {
            var orderToUpdate = await _orderRepository.GetByIdAsync(order.Id);
            if (orderToUpdate == null)
            {
                throw new NotFoundException(nameof(Order), order.Id);
            }
            
            _mapper.Map(order, orderToUpdate, typeof(UpdateOrderCommand), typeof(Order));

            await _orderRepository.UpdateAsync(orderToUpdate);

            _logger.LogInformation($"Order {orderToUpdate.Id} is successfully updated.");

            return orderToUpdate.Id;          
        }

        public async Task<int> DeleteOrder(int id)
        {
             var orderToDelete = await _orderRepository.GetByIdAsync(id);
            if (orderToDelete == null)
            {
                throw new NotFoundException(nameof(OrdersVm), id);
            }            

            await _orderRepository.DeleteAsync(orderToDelete);

            _logger.LogInformation($"Order {orderToDelete.Id} is successfully deleted.");

            return orderToDelete.Id;
        }

    }

}