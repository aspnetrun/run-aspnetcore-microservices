using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AspnetRunBasics.Data;
using AspnetRunBasics.Entities;
using Microsoft.EntityFrameworkCore;

namespace AspnetRunBasics.Repositories
{
    public class ProductRepository : IProductRepository
    {
        protected readonly AspnetRunContext _dbContext;

        public ProductRepository(AspnetRunContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public async Task<IEnumerable<Product>> GetProducts()
        {
            return await _dbContext.Products.ToListAsync();
        }

        public async Task<Product> GetProductById(int id)
        {
            return await _dbContext.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<IEnumerable<Product>> GetProductByName(string name)
        {
            return await _dbContext.Products
                    .Include(p => p.Category)
                    .Where(p => string.IsNullOrEmpty(name) || p.Name.ToLower().Contains(name.ToLower()))
                    .OrderBy(p => p.Name)
                    .ToListAsync();
        }

        public async Task<IEnumerable<Product>> GetProductByCategory(int categoryId)
        {
            return await _dbContext.Products
                .Where(x => x.CategoryId == categoryId)
                .ToListAsync();
        }

        public async Task<Product> AddAsync(Product product)
        {
            _dbContext.Products.Add(product);
            await _dbContext.SaveChangesAsync();
            return product;
        }

        public async Task UpdateAsync(Product product)
        {
            _dbContext.Entry(product).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteAsync(Product product)
        {
            _dbContext.Products.Remove(product);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<IEnumerable<Category>> GetCategories()
        {
            return await _dbContext.Categories.ToListAsync();
        }
    }
}
