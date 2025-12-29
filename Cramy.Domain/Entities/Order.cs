using System;
using System.Collections.Generic;
using System.Text;

namespace Cramy.Domain.Entities
{
    public class Order
    {
        public Guid Id { get; set; }
        public string UserId { get; set; } = default!;
        public OrderStatus Status { get; set; } = OrderStatus.Paid;
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

        public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    }


    public enum OrderStatus { Pending, Paid, Shipped, Completed, Cancelled }



}
