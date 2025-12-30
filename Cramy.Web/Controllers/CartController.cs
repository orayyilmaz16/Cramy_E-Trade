using Cramy.Application.Contracts;
using Cramy.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Cramy.Web.Controllers
{
    [Authorize(Roles = "Customer,Admin")]
    public class CartController : Controller
    {
        private readonly ICardService _card;
        private readonly CramyDbContext _context;
        public CartController(ICardService card, CramyDbContext context) { _card = card; _context = context; }

        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var card = await _card.GetCardAsync(userId);
            return View(card);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Ekle(Guid productId, int quantity = 1, string? returnUrl = null)
        {
            if (productId == Guid.Empty) return NotFound();

            // ✅ quantity güvenliği
            quantity = quantity < 1 ? 1 : quantity;
            if (quantity > 999) quantity = 999;

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId)) return Challenge();

            await _card.AddItemAsync(userId, productId, quantity);

            // ✅ Ürün sayfasına geri dönmek için (Products/AddToCart -> Cart/Ekle -> ReturnUrl)
            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                return LocalRedirect(returnUrl);

            // Varsayılan davranış: sepete git
            return RedirectToAction(nameof(Index));
        }

        // diğer actionlar (Index, Kaldir, SatinAl...) aynı kalır
    

        [HttpPost]
        public async Task<IActionResult> Kaldir(Guid productId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            await _card.RemoveItemAsync(userId, productId);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> SatinAl()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await _card.CheckoutAsync(userId);
            return RedirectToAction("BuyConfirm", "Cart", new { id = result.OrderId });
        }
        [HttpGet]
        public async Task<IActionResult> BuyConfirm(Guid id)
        {
            if (id == Guid.Empty) return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId)) return Challenge();

            var order = await _context.Orders
                .AsNoTracking()
                .Include(o => o.Items)
                    .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId);

            if (order is null) return NotFound();

            return View(order);
        }
    }

}
