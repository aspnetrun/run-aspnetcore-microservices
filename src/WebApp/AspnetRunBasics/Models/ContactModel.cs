using System.ComponentModel.DataAnnotations;

namespace AspnetRunBasics.Models
{
    public class ContactModel
    {
        public string Id { get; set; }
        [Required]
        [StringLength(100)]
        public string Name { get; set; }
        [EmailAddress]
        [StringLength(20)]
        public string Email { get; set; }
        [Phone]
        public string Phone { get; set; }
        [Required]
        [StringLength(20)]
        public string Message { get; set; }
    }
}
