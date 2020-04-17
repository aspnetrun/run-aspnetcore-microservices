using AspnetRunBasics.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AspnetRunBasics.Repositories
{
    public interface IProductRepository
    {
        Task<IEnumerable<Product>> GetProducts();
        Task<Product> GetProductById(int id);
        Task<IEnumerable<Product>> GetProductByName(string name);
        Task<IEnumerable<Product>> GetProductByCategory(int categoryId);
        Task<IEnumerable<Category>> GetCategories();
    }
}
