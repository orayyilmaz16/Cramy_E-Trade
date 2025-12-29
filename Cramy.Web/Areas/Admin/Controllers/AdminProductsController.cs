// Cramy.Web/Areas/Admin/Controllers/AdminProductController.cs
using Cramy.Domain.Entities;
using Cramy.Persistence;
using Cramy.Web.Areas.Admin.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Cramy.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class AdminProductsController : Controller
    {
        private readonly CramyDbContext _db;

        public AdminProductsController(CramyDbContext db)
        {
            _db = db;
        }

        // GET: /Admin/AdminProduct
        public async Task<IActionResult> Index(string? q, bool? active, Guid? categoryId, int page = 1, int pageSize = 20)
        {
            page = page < 1 ? 1 : page;
            pageSize = pageSize is < 5 or > 200 ? 20 : pageSize;

            var query = _db.Products
                .AsNoTracking()
                .Include(p => p.Category)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(q))
            {
                q = q.Trim();
                query = query.Where(p =>
                    p.Name.Contains(q) ||
                    p.SKU.Contains(q));
            }

            if (active.HasValue)
                query = query.Where(p => p.IsActive == active.Value);

            if (categoryId.HasValue && categoryId.Value != Guid.Empty)
                query = query.Where(p => p.CategoryId == categoryId.Value);

            var total = await query.CountAsync();

            var items = await query
                .OrderByDescending(p => p.CreatedAtUtc)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var categories = await _db.Categories
                .AsNoTracking()
                .OrderBy(c => c.Name)
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name
                })
                .ToListAsync();

            var vm = new ProductListViewModel
            {
                Items = items,
                Q = q,
                Active = active,
                CategoryId = categoryId,
                Categories = categories,
                Page = page,
                PageSize = pageSize,
                TotalCount = total
            };

            return View(vm);
        }

        // GET: /Admin/AdminProduct/Details/{id}
        public async Task<IActionResult> Details(Guid id)
        {
            if (id == Guid.Empty) return NotFound();

            var product = await _db.Products
                .AsNoTracking()
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product is null) return NotFound();

            return View(product);
        }

        // GET: /Admin/AdminProduct/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.Categories = await GetCategorySelectListAsync();
            return View(new ProductCreateViewModel());
        }

        // POST: /Admin/AdminProduct/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductCreateViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Categories = await GetCategorySelectListAsync(vm.CategoryId);
                return View(vm);
            }

            // SKU unique kontrol (istersen kaldır)
            var skuExists = await _db.Products.AnyAsync(p => p.SKU == vm.SKU);
            if (skuExists)
            {
                ModelState.AddModelError(nameof(vm.SKU), "Bu SKU zaten kullanılıyor.");
                ViewBag.Categories = await GetCategorySelectListAsync(vm.CategoryId);
                return View(vm);
            }

            // Category var mı?
            var categoryExists = await _db.Categories.AnyAsync(c => c.Id == vm.CategoryId);
            if (!categoryExists)
            {
                ModelState.AddModelError(nameof(vm.CategoryId), "Kategori bulunamadı.");
                ViewBag.Categories = await GetCategorySelectListAsync(vm.CategoryId);
                return View(vm);
            }

            var entity = new Product
            {
                Id = Guid.NewGuid(),
                SKU = vm.SKU.Trim(),
                Name = vm.Name.Trim(),
                Description = vm.Description.Trim(),
                Price = vm.Price,
                ImageUrl = string.IsNullOrWhiteSpace(vm.ImageUrl) ? null : vm.ImageUrl.Trim(),
                StockQuantity = vm.StockQuantity,
                CategoryId = vm.CategoryId,
                IsActive = vm.IsActive,
                CreatedAtUtc = DateTime.UtcNow
            };

            _db.Products.Add(entity);
            await _db.SaveChangesAsync();

            TempData["ok"] = "Ürün eklendi.";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Admin/AdminProduct/Edit/{id}
        public async Task<IActionResult> Edit(Guid id)
        {
            if (id == Guid.Empty) return NotFound();

            var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == id);
            if (product is null) return NotFound();

            var vm = new ProductEditViewModel
            {
                Id = product.Id,
                SKU = product.SKU,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                ImageUrl = product.ImageUrl,
                StockQuantity = product.StockQuantity,
                CategoryId = product.CategoryId,
                IsActive = product.IsActive
            };

            ViewBag.Categories = await GetCategorySelectListAsync(product.CategoryId);
            return View(vm);
        }

        // POST: /Admin/AdminProduct/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ProductEditViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Categories = await GetCategorySelectListAsync(vm.CategoryId);
                return View(vm);
            }

            var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == vm.Id);
            if (product is null) return NotFound();

            // SKU unique kontrol (kendisi hariç)
            var skuExists = await _db.Products.AnyAsync(p => p.SKU == vm.SKU && p.Id != vm.Id);
            if (skuExists)
            {
                ModelState.AddModelError(nameof(vm.SKU), "Bu SKU zaten kullanılıyor.");
                ViewBag.Categories = await GetCategorySelectListAsync(vm.CategoryId);
                return View(vm);
            }

            // Category var mı?
            var categoryExists = await _db.Categories.AnyAsync(c => c.Id == vm.CategoryId);
            if (!categoryExists)
            {
                ModelState.AddModelError(nameof(vm.CategoryId), "Kategori bulunamadı.");
                ViewBag.Categories = await GetCategorySelectListAsync(vm.CategoryId);
                return View(vm);
            }

            product.SKU = vm.SKU.Trim();
            product.Name = vm.Name.Trim();
            product.Description = vm.Description.Trim();
            product.Price = vm.Price;
            product.ImageUrl = string.IsNullOrWhiteSpace(vm.ImageUrl) ? null : vm.ImageUrl.Trim();
            product.StockQuantity = vm.StockQuantity;
            product.CategoryId = vm.CategoryId;
            product.IsActive = vm.IsActive;

            await _db.SaveChangesAsync();

            TempData["ok"] = "Ürün güncellendi.";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Admin/AdminProduct/Delete/{id} (confirm page)
        public async Task<IActionResult> Delete(Guid id)
        {
            if (id == Guid.Empty) return NotFound();

            var product = await _db.Products
                .AsNoTracking()
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product is null) return NotFound();

            return View(product);
        }

        // POST: /Admin/AdminProduct/Delete/{id}
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == id);
            if (product is null) return NotFound();

            _db.Products.Remove(product);
            await _db.SaveChangesAsync();

            TempData["ok"] = "Ürün silindi.";
            return RedirectToAction(nameof(Index));
        }

        private async Task<List<SelectListItem>> GetCategorySelectListAsync(Guid? selectedId = null)
        {
            var items = await _db.Categories
                .AsNoTracking()
                .OrderBy(c => c.Name)
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name,
                    Selected = selectedId.HasValue && c.Id == selectedId.Value
                })
                .ToListAsync();

            return items;
        }
    }
}
