using Cramy.Domain.Entities;

namespace Cramy.Web.Areas.Admin.Models
{
    public sealed class AdminOrderDetailsViewModel
    {
        public Guid Id { get; set; }
        public string UserId { get; set; } = default!;
        public OrderStatus Status { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public List<AdminOrderItemViewModel> Items { get; set; } = new();
        public decimal Total { get; set; }
    }
}
