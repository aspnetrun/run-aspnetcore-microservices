using MediatR;
using Microsoft.Extensions.Logging;
using Ordering.Application.Contracts.Infrastructure;
using Ordering.Application.Models;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Ordering.Application.Features.Orders.Notifications.CheckoutOrder
{
    public class CheckoutOrderAddedSendEmailHandler : INotificationHandler<CheckoutOrderAddedNotification>
    {
        private readonly IEmailService _emailService;
        private readonly ILogger<CheckoutOrderAddedSendEmailHandler> _logger;
        public CheckoutOrderAddedSendEmailHandler(IEmailService emailService, ILogger<CheckoutOrderAddedSendEmailHandler> logger)
        {
            _emailService = emailService;
            _logger = logger;
        }

        public async Task Handle(CheckoutOrderAddedNotification notification, CancellationToken cancellationToken)
        {
            var email = new Email() { To = "ezozkme@gmail.com", Body = $"Order was created.", Subject = "Order was created" };

            try
            {
                await _emailService.SendEmail(email);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Order {notification.orderId} failed due to an error with the mail service: {ex.Message}");
            }
        }
    }
}
