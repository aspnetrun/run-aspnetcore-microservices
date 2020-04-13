using Ordering.Application.Interfaces;
using Ordering.Application.Mapper;
using Ordering.Application.Models;
using Ordering.Core.Entities;
using Ordering.Core.Repositories;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ordering.Application.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;

        public OrderService(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
        }

        public async Task<OrderModel> CheckOut(OrderModel orderModel)
        {
            var mappedEntity = ObjectMapper.Mapper.Map<Order>(orderModel);
            if (mappedEntity == null)
                throw new ApplicationException($"Entity could not be mapped.");

            var newEntity = await _orderRepository.AddAsync(mappedEntity);

            var newMappedEntity = ObjectMapper.Mapper.Map<OrderModel>(newEntity);
            return newMappedEntity;
        }

        public async Task<IEnumerable<OrderModel>> GetOrdersByUserName(string userName)
        {
            var orderList = await _orderRepository.GetOrdersByUserName(userName);
            var mapped = ObjectMapper.Mapper.Map<IEnumerable<OrderModel>>(orderList);
            return mapped;            
        }
    }
}
