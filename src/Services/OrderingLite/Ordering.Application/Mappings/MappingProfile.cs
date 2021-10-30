using AutoMapper;
using Ordering.Domain.Entities;
using Ordering.Application.Models;


namespace Ordering.Application.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Order, OrderVm>().ReverseMap();
            CreateMap<Order, CheckoutOrderVm>().ReverseMap();
        }
    }
}
