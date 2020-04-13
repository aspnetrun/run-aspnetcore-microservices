using AutoMapper;
using Ordering.Application.Models;
using Ordering.Core.Entities;

namespace Ordering.Application.Mapper
{
    public class OrderMappingProfile : Profile
    {
        public OrderMappingProfile()
        {
            CreateMap<Order, OrderModel>().ReverseMap();
        }
    }
}
