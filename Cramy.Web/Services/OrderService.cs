using System;
using System.Collections.Generic;
using System.Linq;
using Cramy.Persistence;           // CramyDbContext burada ise ekle
using Cramy.Web.Models;
using Cramy.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Cramy.Web.Services.Interfaces;

public class OrderService : IOrderService
{
    private readonly CramyDbContext _context;

    public OrderService(CramyDbContext context)
    {
        _context = context;
    }

    public List<OrderViewModel> GetOrdersForUser(string userId)
    {
        return _context.Orders
            .AsNoTracking()
            .Include(o => o.Items)
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.CreatedAtUtc)
            .Select(o => new OrderViewModel
            {
                Id = o.Id,

                // Range operator yerine Substring: her C# sürümünde çalışır
                OrderNumber = "ORD-" + o.Id.ToString("N").Substring(0, 8).ToUpper(),

                CreatedAtUtc = o.CreatedAtUtc,
                Status = o.Status.ToString(),

                // Null güvenliği (Items boş/null olursa patlamasın)
                TotalPrice = o.Items.Sum(i => i.UnitPrice * i.Quantity)
            })
            .ToList();
    }
}
