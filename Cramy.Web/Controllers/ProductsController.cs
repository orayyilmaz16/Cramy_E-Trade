using Cramy.Application.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cramy.Web.Controllers
{
    [AllowAnonymous] // ürün liste + detay herkese açık
    public class ProductsController : Controller
    {
        private readonly IProductService _service;

        public ProductsController(IProductService service)
        {
            _service = service;
        }

        // GET: /Products
        [HttpGet]
        public async Task<IActionResult> Index(int page = 1)
            => View(await _service.GetPagedAsync(page, 12));

        // GET: /Products/Detail/{id}
        [HttpGet]
        public async Task<IActionResult> Detail(Guid id)
        {
            if (id == Guid.Empty) return NotFound();

            var product = await _service.GetByIdAsync(id);
            if (product is null) return NotFound();

            return View(product);
        }

        // POST: /Products/AddToCart
        // Sepete çoklu miktar ekleme (quantity)
        [HttpPost]
        [Authorize(Roles = "Customer,Admin")]
        [ValidateAntiForgeryToken]
        public IActionResult AddToCart(Guid productId, int quantity = 1, string? returnUrl = null)
        {
            if (productId == Guid.Empty) return NotFound();

            quantity = quantity < 1 ? 1 : quantity;
            if (quantity > 999) quantity = 999;

            // returnUrl boşsa bulunduğun sayfaya dönmek için fallback
            returnUrl ??= Request.Headers.Referer.ToString();

            return RedirectToAction("Ekle", "Cart", new { productId, quantity, returnUrl });
        }
    }
}
