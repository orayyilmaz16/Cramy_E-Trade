using Cramy.Application.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Cramy.Web.Controllers
{
    [Authorize(Roles = "Customer,Admin")]
    public class CartController : Controller
    {
        private readonly ICardService _card;
        public CartController(ICardService card) { _card = card; }

        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var card = await _card.GetCardAsync(userId);
            return View(card);
        }

        [HttpPost]
        public async Task<IActionResult> Ekle(Guid productId, int quantity = 1)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            await _card.AddItemAsync(userId, productId, quantity);
            return RedirectToAction(nameof(Index));
        }

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
            return RedirectToAction("Detay", "Orders", new { id = result.OrderId });
        }
    }

}
