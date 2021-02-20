using Microsoft.AspNetCore.Http;

namespace AspnetRunBasics.Models
{
    public class AddCatalogModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public string Summary { get; set; }
        public string Description { get; set; }
        public IFormFile ImageURL { get; set; }
        public decimal Price { get; set; }
    }
}
