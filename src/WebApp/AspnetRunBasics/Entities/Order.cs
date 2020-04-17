namespace AspnetRunBasics.Entities
{
    public class Order
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public decimal TotalPrice { get; set; }

        // BillingAddress
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string EmailAddress { get; set; }
        public string AddressLine { get; set; }
        public string Country { get; set; }
        public string State { get; set; }
        public string ZipCode { get; set; }

        // Payment
        public string CardName { get; set; }
        public string CardNumber { get; set; }
        public string Expiration { get; set; }
        public string CVV { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
    }

    public enum PaymentMethod
    {
        CreditCard = 1,
        DebitCard = 2,
        Paypal = 3
    }
}
