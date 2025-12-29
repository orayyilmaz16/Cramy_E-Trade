using System;
using System.Collections.Generic;
using System.Text;

namespace Cramy.Domain.Entities
{
    public class CartItem
    {
        public Guid Id { get; set; }
        public Guid CardId { get; set; }
        public Card? Card { get; set; }

        public Guid ProductId { get; set; }
        public Product? Product { get; set; }

        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; } // snapshot
    }

}
