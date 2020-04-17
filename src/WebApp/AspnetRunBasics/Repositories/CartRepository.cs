using AspnetRunBasics.Data;
using AspnetRunBasics.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace AspnetRunBasics.Repositories
{
    public class CartRepository : ICartRepository
    {
        protected readonly AspnetRunContext _dbContext;

        public CartRepository(AspnetRunContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public async Task<Cart> GetCartByUserName(string userName)
        {
            var cart =  _dbContext.Carts
                        .Include(c => c.Items)
                            .ThenInclude(i => i.Product)
                        .FirstOrDefault(c => c.UserName == userName);

            if (cart != null)
                return cart;

            // if it is first attempt create new
            var newCart = new Cart
            {
                UserName = userName
            };

            _dbContext.Carts.Add(newCart);
            await _dbContext.SaveChangesAsync();
            return newCart;
        }

        public async Task AddItem(string userName, int productId, int quantity = 1, string color = "Black")
        {
            var cart = await GetCartByUserName(userName);
            
            cart.Items.Add(
                    new CartItem
                    {
                        ProductId = productId,
                        Color = color,
                        Price = _dbContext.Products.FirstOrDefault(p => p.Id == productId).Price,
                        Quantity = quantity
                    }
                );

            _dbContext.Entry(cart).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync();
        }

        public async Task RemoveItem(int cartId, int cartItemId)
        {
            var cart = _dbContext.Carts
                       .Include(c => c.Items)
                       .FirstOrDefault(c => c.Id == cartId);

            if (cart != null)
            {
                var removedItem = cart.Items.FirstOrDefault(x => x.Id == cartItemId);
                cart.Items.Remove(removedItem);

                _dbContext.Entry(cart).State = EntityState.Modified;
                await _dbContext.SaveChangesAsync();
            }                

        }

        public async Task ClearCart(string userName)
        {
            var cart = await GetCartByUserName(userName);

            cart.Items.Clear();

            _dbContext.Entry(cart).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync();
        }
    }
}
