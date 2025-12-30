// Cramy.Web/Areas/Admin/Controllers/AdminCategoriesController.cs
using Cramy.Domain.Entities;
using Cramy.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Text.RegularExpressions;

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

        // GET
        [HttpGet]
        public IActionResult Create()
        {
            return View(new Category());
        }

        // POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Category model)
        {
            model.Name = (model.Name ?? string.Empty).Trim();

            // Slug alanı Category'de varsa doldur.
            // Eğer kullanıcı formdan göndermiyorsa otomatik üret.
            model.Slug = (model.Slug ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(model.Slug))
                model.Slug = Slugify(model.Name);

            // Validation
            if (string.IsNullOrWhiteSpace(model.Name))
                ModelState.AddModelError(nameof(model.Name), "Kategori adı zorunludur.");

            if (string.IsNullOrWhiteSpace(model.Slug))
                ModelState.AddModelError(nameof(model.Slug), "Slug alanı zorunludur.");

            // Duplicate checks (case-insensitive)
            var nameLower = model.Name.ToLower();
            var slugLower = model.Slug.ToLower();

            var nameExists = await _db.Categories.AnyAsync(c => c.Name.ToLower() == nameLower);
            if (nameExists)
                ModelState.AddModelError(nameof(model.Name), "Bu kategori zaten mevcut.");

            var slugExists = await _db.Categories.AnyAsync(c => c.Slug.ToLower() == slugLower);
            if (slugExists)
                ModelState.AddModelError(nameof(model.Slug), "Bu slug zaten kullanılıyor.");

            if (!ModelState.IsValid)
                return View(model);

            if (model.Id == Guid.Empty)
                model.Id = Guid.NewGuid();

            try
            {
                _db.Categories.Add(model);
                await _db.SaveChangesAsync();

                TempData["ok"] = "Kategori eklendi.";
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                ModelState.AddModelError(string.Empty, "Kategori kaydedilirken bir hata oluştu. Lütfen tekrar deneyin.");
                return View(model);
            }
        }

        // Türkçe karakter destekli slug üretici
        private static string Slugify(string input)
        {
            input = (input ?? string.Empty).Trim().ToLowerInvariant();

            // Türkçe karakter dönüşümü
            input = input
                .Replace("ç", "c").Replace("ğ", "g").Replace("ı", "i")
                .Replace("ö", "o").Replace("ş", "s").Replace("ü", "u");

            // Diakritik temizliği (genel)
            var normalized = input.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();
            foreach (var ch in normalized)
            {
                var uc = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(ch);
                if (uc != System.Globalization.UnicodeCategory.NonSpacingMark)
                    sb.Append(ch);
            }
            var cleaned = sb.ToString().Normalize(NormalizationForm.FormC);

            // Alfasayısal dışını tire yap
            cleaned = Regex.Replace(cleaned, @"[^a-z0-9]+", "-");
            cleaned = cleaned.Trim('-');
            cleaned = Regex.Replace(cleaned, @"-+", "-");

            return cleaned;
        }

        // GET: /Admin/AdminCategories/Edit/{id}
        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            if (id == Guid.Empty) return NotFound();

            var entity = await _db.Categories.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
            if (entity is null) return NotFound();

            return View(entity);
        }

        // POST: /Admin/AdminCategories/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Category model)
        {
            if (model.Id == Guid.Empty) return NotFound();

            model.Name = (model.Name ?? string.Empty).Trim();
            model.Slug = (model.Slug ?? string.Empty).Trim();

            // Slug boşsa otomatik üret
            if (string.IsNullOrWhiteSpace(model.Slug))
                model.Slug = Slugify(model.Name);

            // Validation
            if (string.IsNullOrWhiteSpace(model.Name))
                ModelState.AddModelError(nameof(model.Name), "Kategori adı zorunludur.");

            if (string.IsNullOrWhiteSpace(model.Slug))
                ModelState.AddModelError(nameof(model.Slug), "Slug alanı zorunludur.");

            // Unique kontroller (kendisi hariç) - case-insensitive
            if (!string.IsNullOrWhiteSpace(model.Name))
            {
                var nameLower = model.Name.ToLower();
                var nameExists = await _db.Categories
                    .AnyAsync(c => c.Id != model.Id && c.Name.ToLower() == nameLower);

                if (nameExists)
                    ModelState.AddModelError(nameof(model.Name), "Bu kategori adı zaten mevcut.");
            }

            if (!string.IsNullOrWhiteSpace(model.Slug))
            {
                var slugLower = model.Slug.ToLower();
                var slugExists = await _db.Categories
                    .AnyAsync(c => c.Id != model.Id && c.Slug.ToLower() == slugLower);

                if (slugExists)
                    ModelState.AddModelError(nameof(model.Slug), "Bu slug zaten kullanılıyor.");
            }

            if (!ModelState.IsValid)
                return View(model);

            var entity = await _db.Categories.FirstOrDefaultAsync(c => c.Id == model.Id);
            if (entity is null) return NotFound();

            entity.Name = model.Name;
            entity.Slug = model.Slug;

            try
            {
                await _db.SaveChangesAsync();
                TempData["ok"] = "Kategori güncellendi.";
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                ModelState.AddModelError(string.Empty, "Kategori güncellenirken bir hata oluştu. Lütfen tekrar deneyin.");
                return View(model);
            }
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
