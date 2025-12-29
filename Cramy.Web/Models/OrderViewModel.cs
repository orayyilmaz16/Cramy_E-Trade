namespace Cramy.Web.Models;

public class OrderViewModel
{
    public Guid Id { get; set; }

    public string OrderNumber { get; set; } = default!;

    // ✅ EKLENMELİ
    public DateTime CreatedAtUtc { get; set; }

    public string Status { get; set; } = default!;

    public decimal TotalPrice { get; set; }
}