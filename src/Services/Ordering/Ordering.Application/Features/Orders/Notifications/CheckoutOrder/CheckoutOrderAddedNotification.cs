using MediatR;

namespace Ordering.Application.Features.Orders.Notifications.CheckoutOrder
{
    public record CheckoutOrderAddedNotification(int orderId) : INotification;

}
