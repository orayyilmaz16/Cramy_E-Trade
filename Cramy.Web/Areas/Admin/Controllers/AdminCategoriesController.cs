// Cramy.Web/Areas/Admin/Controllers/AdminCategoriesController.cs
using Cramy.Domain.Entities;
using Cramy.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Cramy.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class AdminCategoriesController : Controller
    {
        private readonly CramyDbContext _db;

        public AdminCategoriesController(CramyDbContext db)
        {
            _db = db;
        }

        // GET: /Admin/AdminCategories
        public async Task<IActionResult> Index(string? q, int page = 1, int pageSize = 20)
        {
            page = page < 1 ? 1 : page;
            pageSize = pageSize is < 5 or > 200 ? 20 : pageSize;

            var query = _db.Categories.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(q))
            {
                q = q.Trim();
                query = query.Where(c => c.Name.Contains(q));
            }

            var total = await query.CountAsync();

            var items = await query
                .OrderBy(c => c.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Basit VM kullanmak istemezsen direkt ViewBag de olur.
            // Burada minimal anonymous model yerine ViewBag kullanıyoruz.
            ViewBag.Q = q;
            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalCount = total;

            return View(items);
        }

        // GET: /Admin/AdminCategories/Create
        [HttpGet]
        public IActionResult Create()
        {
            return View(new Category());
        }

        // POST: /Admin/AdminCategories/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Category model)
        {
            model.Name = (model.Name ?? "").Trim();

            if (string.IsNullOrWhiteSpace(model.Name))
                ModelState.AddModelError(nameof(model.Name), "Kategori adı zorunludur.");

            var exists = await _db.Categories.AnyAsync(c => c.Name == model.Name);
            if (exists)
                ModelState.AddModelError(nameof(model.Name), "Bu kategori zaten mevcut.");

            if (!ModelState.IsValid)
                return View(model);

            model.Id = model.Id == Guid.Empty ? Guid.NewGuid() : model.Id;
            _db.Categories.Add(model);
            await _db.SaveChangesAsync();

            TempData["ok"] = "Kategori eklendi.";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Admin/AdminCategories/Edit/{id}
        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            if (id == Guid.Empty) return NotFound();

            var entity = await _db.Categories.FirstOrDefaultAsync(c => c.Id == id);
            if (entity is null) return NotFound();

            return View(entity);
        }

        // POST: /Admin/AdminCategories/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Category model)
        {
            if (model.Id == Guid.Empty) return NotFound();

            model.Name = (model.Name ?? "").Trim();

            if (string.IsNullOrWhiteSpace(model.Name))
                ModelState.AddModelError(nameof(model.Name), "Kategori adı zorunludur.");

            // Unique kontrol (kendisi hariç)
            var exists = await _db.Categories.AnyAsync(c => c.Name == model.Name && c.Id != model.Id);
            if (exists)
                ModelState.AddModelError(nameof(model.Name), "Bu kategori zaten mevcut.");

            if (!ModelState.IsValid)
                return View(model);

            var entity = await _db.Categories.FirstOrDefaultAsync(c => c.Id == model.Id);
            if (entity is null) return NotFound();

            entity.Name = model.Name;
            await _db.SaveChangesAsync();

            TempData["ok"] = "Kategori güncellendi.";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Admin/AdminCategories/Delete/{id} (confirm page)
        [HttpGet]
        public async Task<IActionResult> Delete(Guid id)
        {
            if (id == Guid.Empty) return NotFound();

            // Kategoriye bağlı ürün var mı? (silmeden önce uyarı için)
            var entity = await _db.Categories
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id);

            if (entity is null) return NotFound();

            ViewBag.ProductCount = await _db.Products.CountAsync(p => p.CategoryId == id);

            return View(entity);
        }

        // POST: /Admin/AdminCategories/Delete/{id}
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            if (id == Guid.Empty) return NotFound();

            var entity = await _db.Categories.FirstOrDefaultAsync(c => c.Id == id);
            if (entity is null) return NotFound();

            // Eğer ürün bağlıysa silmeyi engelle (istersen cascade yaparsın)
            var hasProducts = await _db.Products.AnyAsync(p => p.CategoryId == id);
            if (hasProducts)
            {
                TempData["err"] = "Bu kategoriye bağlı ürünler var. Önce ürünleri başka kategoriye taşıyın veya silin.";
                return RedirectToAction(nameof(Delete), new { id });
            }

            _db.Categories.Remove(entity);
            await _db.SaveChangesAsync();

            TempData["ok"] = "Kategori silindi.";
            return RedirectToAction(nameof(Index));
        }
    }
}
