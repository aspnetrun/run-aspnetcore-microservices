using System.ComponentModel.DataAnnotations;

namespace AspnetRunBasics.Entities
{
    public class Category
    {
        public int Id { get; set; }

        [Required, StringLength(80)]
        public string Name { get; set; }
        public string Description { get; set; }
        public string ImageName { get; set; }
    }
}
