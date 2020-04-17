using System.ComponentModel.DataAnnotations;

namespace AspnetRunBasics.Entities
{
    public class Product
    {
        public int Id { get; set; }

        [Required, StringLength(80)]
        public string Name { get; set; }

        public string Summary { get; set; }
        public string Description { get; set; }
        public string ImageFile { get; set; }
        public decimal Price { get; set; }
        public int CategoryId { get; set; }
        public Category Category { get; set; }
    }
}
