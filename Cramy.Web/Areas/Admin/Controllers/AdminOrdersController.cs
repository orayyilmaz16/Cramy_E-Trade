using Cramy.Domain.Entities;
using Cramy.Persistence;
using Cramy.Web.Areas.Admin.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Cramy.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class AdminOrdersController : Controller
    {
        private readonly CramyDbContext _db;

        public AdminOrdersController(CramyDbContext db)
        {
            _db = db;
        }

        // GET: /Admin/AdminOrders
        // Listeleme + filtre + sayfalama
        public async Task<IActionResult> Index(string? q, OrderStatus? status, int page = 1, int pageSize = 20)
        {
            page = page < 1 ? 1 : page;
            pageSize = pageSize is < 5 or > 200 ? 20 : pageSize;

            var query = _db.Orders
                .AsNoTracking()
                .AsQueryable();

            // Arama: UserId üzerinden (istersen email/username join ile genişletirsin)
            if (!string.IsNullOrWhiteSpace(q))
            {
                q = q.Trim();
                query = query.Where(o => o.UserId.Contains(q));
            }

            if (status.HasValue)
            {
                query = query.Where(o => o.Status == status.Value);
            }

            var total = await query.CountAsync();

            var items = await query
                .OrderByDescending(o => o.CreatedAtUtc)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(o => new OrderListRowViewModel
                {
                    Id = o.Id,
                    UserId = o.UserId,
                    Status = o.Status,
                    CreatedAtUtc = o.CreatedAtUtc,
                    ItemCount = o.Items.Count,
                    Total = o.Items.Sum(i => i.Quantity * i.UnitPrice)
                })
                .ToListAsync();

            var vm = new AdminOrderListViewModel
            {
                Items = items,
                Q = q,
                Status = status,
                Page = page,
                PageSize = pageSize,
                TotalCount = total
            };

            return View(vm);
        }

        // GET: /Admin/AdminOrders/Details/{id}
        // Denetleme: sipariş + kalemler
        public async Task<IActionResult> Details(Guid id)
        {
            if (id == Guid.Empty) return NotFound();

            var order = await _db.Orders
                .AsNoTracking()
                .Include(o => o.Items)
                    .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order is null) return NotFound();

            var vm = new AdminOrderDetailsViewModel
            {
                Id = order.Id,
                UserId = order.UserId,
                Status = order.Status,
                CreatedAtUtc = order.CreatedAtUtc,
                Items = order.Items.Select(i => new AdminOrderItemViewModel
                {
                    Id = i.Id,
                    ProductId = i.ProductId,
                    ProductName = i.Product != null ? i.Product.Name : "(Ürün bulunamadı)",
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    LineTotal = i.Quantity * i.UnitPrice
                }).ToList()
            };

            vm.Total = vm.Items.Sum(x => x.LineTotal);

            return View(vm);
        }

        // POST: /Admin/AdminOrders/UpdateStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(Guid id, OrderStatus status)
        {
            if (id == Guid.Empty) return NotFound();

            var order = await _db.Orders.FirstOrDefaultAsync(o => o.Id == id);
            if (order is null) return NotFound();

            order.Status = status;
            await _db.SaveChangesAsync();

            TempData["ok"] = "Sipariş durumu güncellendi.";
            return RedirectToAction(nameof(Details), new { id });
        }

        // GET: /Admin/AdminOrders/Delete/{id}
        public async Task<IActionResult> Delete(Guid id)
        {
            if (id == Guid.Empty) return NotFound();

            var order = await _db.Orders
                .AsNoTracking()
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order is null) return NotFound();

            var vm = new AdminOrderDeleteViewModel
            {
                Id = order.Id,
                UserId = order.UserId,
                Status = order.Status,
                CreatedAtUtc = order.CreatedAtUtc,
                ItemCount = order.Items.Count,
                Total = order.Items.Sum(i => i.Quantity * i.UnitPrice)
            };

            return View(vm);
        }

        // POST: /Admin/AdminOrders/Delete/{id}
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            if (id == Guid.Empty) return NotFound();

            var order = await _db.Orders
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order is null) return NotFound();

            // Cascade yoksa önce kalemleri sil
            if (order.Items is not null && order.Items.Count > 0)
                _db.Set<OrderItem>().RemoveRange(order.Items);

            _db.Orders.Remove(order);
            await _db.SaveChangesAsync();

            TempData["ok"] = "Sipariş silindi.";
            return RedirectToAction(nameof(Index));
        }


       
    }
}
