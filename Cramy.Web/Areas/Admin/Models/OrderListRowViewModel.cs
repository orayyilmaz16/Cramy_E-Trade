using Cramy.Domain.Entities;

namespace Cramy.Web.Areas.Admin.Models
{
    public sealed class OrderListRowViewModel
    {
        public Guid Id { get; set; }
        public string UserId { get; set; } = default!;
        public OrderStatus Status { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public int ItemCount { get; set; }
        public decimal Total { get; set; }
    }
}
