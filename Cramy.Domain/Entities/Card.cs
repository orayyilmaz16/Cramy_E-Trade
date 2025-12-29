using System;
using System.Collections.Generic;
using System.Text;

namespace Cramy.Domain.Entities
{
    public class Card
    {
        public Guid Id { get; set; }
        public string UserId { get; set; } = default!;
        public ApplicationUser? User { get; set; }
        public ICollection<CartItem> Items { get; set; } = new List<CartItem>();
    }

}
