using System.Collections.Generic;

namespace Basket.API.Entities
{
    public class BasketCart
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public List<BasketCartItem> Items { get; set; } = new List<BasketCartItem>();

        public BasketCart(string userName)
        {
            UserName = userName;
        }

        public decimal TotalPrice
        {
            get
            {
                decimal totalprice = 0;
                foreach (var item in Items)
                {
                    totalprice += item.Price * item.Quantity;
                }

                return totalprice;
            }
        }
    }
}
