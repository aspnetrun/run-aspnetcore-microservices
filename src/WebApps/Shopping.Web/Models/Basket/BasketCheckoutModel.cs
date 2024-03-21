namespace Shopping.Web.Models.Basket;

public class BasketCheckoutModel
{
    public string UserName { get; set; } = default!;
    public Guid CustomerId { get; set; } = default!;
    public decimal TotalPrice { get; set; } = default!;

    // Shipping and BillingAddress
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public string EmailAddress { get; set; } = default!;
    public string AddressLine { get; set; } = default!;
    public string Country { get; set; } = default!;
    public string State { get; set; } = default!;
    public string ZipCode { get; set; } = default!;

    // Payment
    public string CardName { get; set; } = default!;
    public string CardNumber { get; set; } = default!;
    public string Expiration { get; set; } = default!;
    public string CVV { get; set; } = default!;
    public int PaymentMethod { get; set; } = default!;
}

// wrapper classes
public record CheckoutBasketRequest(BasketCheckoutModel BasketCheckoutDto);
public record CheckoutBasketResponse(bool IsSuccess);