using System;
using System.Collections.Generic;
using System.Text;

namespace Cramy.Domain.Entities
{
    public class Category
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = default!;
        public string Slug { get; set; } = default!;
        public bool IsActive { get; set; } = true;
        public ICollection<Product> Products { get; set; } = new List<Product>();
    }

}
