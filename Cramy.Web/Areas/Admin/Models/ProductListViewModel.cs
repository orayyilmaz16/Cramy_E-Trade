using Cramy.Domain.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Cramy.Web.Areas.Admin.Models
{
    public class ProductListViewModel
    {
         
        public List<Product> Items { get; set; } = new();

        public string? Q { get; set; }
        public bool? Active { get; set; }
        public Guid? CategoryId { get; set; }

        public List<SelectListItem> Categories { get; set; } = new();

        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public int TotalCount { get; set; }
    }
}

