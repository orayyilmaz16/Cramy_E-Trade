using Cramy.Domain.Entities;

namespace Cramy.Web.Areas.Admin.Models
{
    public sealed class AdminOrderListViewModel
    {
        public List<OrderListRowViewModel> Items { get; set; } = new();
        public string? Q { get; set; }
        public OrderStatus? Status { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
    }
}
