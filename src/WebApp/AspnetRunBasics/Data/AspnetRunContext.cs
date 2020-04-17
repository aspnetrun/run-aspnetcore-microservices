using AspnetRunBasics.Entities;
using Microsoft.EntityFrameworkCore;

namespace AspnetRunBasics.Data
{
    public class AspnetRunContext : DbContext
    {
        public AspnetRunContext(DbContextOptions<AspnetRunContext> options)
            : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<Contact> Contacts { get; set; }
    }
}
