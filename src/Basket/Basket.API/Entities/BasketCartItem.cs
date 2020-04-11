namespace Basket.API.Entities
{
    public class BasketCartItem
    {        
        public int Quantity { get; set; }
        public string Color { get; set; }
        public decimal Price { get; set; }
        public int ProductId { get; set; }
    }
}
