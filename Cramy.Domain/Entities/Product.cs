
namespace Cramy.Domain.Entities
{
    public class Product
    {
        public Guid Id { get; set; }
        public string SKU { get; set; } = default!;
        public string Name { get; set; } = default!;
        public string Description { get; set; } = default!;
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public Guid CategoryId { get; set; }
        public Category? Category { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    }

}
