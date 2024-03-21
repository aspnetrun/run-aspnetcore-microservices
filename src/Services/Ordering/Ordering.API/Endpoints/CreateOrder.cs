using Ordering.Application.Orders.Commands.CreateOrder;

namespace Ordering.API.Endpoints;

//- Accepts a CreateOrderRequest object.
//- Maps the request to a CreateOrderCommand.
//- Uses MediatR to send the command to the corresponding handler.
//- Returns a response with the created order's ID.

public record CreateOrderRequest(OrderDto Order);
public record CreateOrderResponse(Guid Id);

public class CreateOrder : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/orders", async (CreateOrderRequest request, ISender sender) =>
        {
            var command = request.Adapt<CreateOrderCommand>();

            var result = await sender.Send(command);

            var response = result.Adapt<CreateOrderResponse>();

            return Results.Created($"/orders/{response.Id}", response);
        })
        .WithName("CreateOrder")
        .Produces<CreateOrderResponse>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .WithSummary("Create Order")
        .WithDescription("Create Order");
    }
}