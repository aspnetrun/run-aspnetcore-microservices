using BuildingBlocks.Exceptions;

namespace Ordering.Application.Exceptions;
public class OrderNotFoundException : NotFoundException
{
    public OrderNotFoundException(Guid id) : base("Order", id)
    {
    }
}
