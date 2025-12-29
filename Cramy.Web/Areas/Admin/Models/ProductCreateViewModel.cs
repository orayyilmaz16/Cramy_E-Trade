using System.ComponentModel.DataAnnotations;

namespace Cramy.Web.Areas.Admin.Models
{
    public class ProductCreateViewModel
    {
         
        [Required, StringLength(50)]
        public string SKU { get; set; } = default!;

        [Required, StringLength(150)]
        public string Name { get; set; } = default!;

        [Required, StringLength(2000)]
        public string Description { get; set; } = default!;

        [Range(0, 1_000_000)]
        public decimal Price { get; set; }

        [Url, StringLength(500)]
        public string? ImageUrl { get; set; }

        [Range(0, 1_000_000)]
        public int StockQuantity { get; set; }

        [Required]
        public Guid CategoryId { get; set; }

        public bool IsActive { get; set; } = true;
    }
}

