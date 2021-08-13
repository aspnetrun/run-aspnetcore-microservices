using AutoMapper;
//using Ordering.Application.Features.Orders.Commands.CheckoutOrder;
//using Ordering.Application.Features.Orders.Commands.UpdateOrder;
//using Ordering.Application.Features.Orders.Queries.GetOrdersList;
using Ordering.Domain.Entities;
using Ordering.Application.Model;


namespace Ordering.Application.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Order, OrdersVm>().ReverseMap();
            //CreateMap<Order, CheckoutOrderCommand>().ReverseMap();
        }
    }
}
