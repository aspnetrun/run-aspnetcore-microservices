using AutoMapper;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using Ordering.Application.Contracts.Services;
using Ordering.Application.Services;
using Ordering.Application.Contracts.Infrastructure;

namespace Ordering.Application
{
    public static class ApplicationServiceRegistration
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddAutoMapper(Assembly.GetExecutingAssembly());

            services.AddScoped<IOrderService, OrderService>();
            
            return services;
        }
    }
}
