using System;
using System.Collections.Generic;
using System.Text;

namespace Cramy.Domain.Entities
{
    public class InventoryTransaction
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public Product? Product { get; set; }

        public int Delta { get; set; } // +stok/-stok
        public string Reason { get; set; } = default!; // "Sell","Buy","Checkout","Return","AdminAdjust"
        public string? ReferenceId { get; set; }
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    }

}
