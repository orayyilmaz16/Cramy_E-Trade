using Cramy.Domain.Entities;
using Cramy.Web.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Cramy.Web.Controllers
{
    public class OrderController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IOrderService _orderService;

        public OrderController(UserManager<ApplicationUser> userManager, IOrderService orderService)
        {
            _userManager = userManager;
            _orderService = orderService;
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User); // string userId
            var orders = _orderService.GetOrdersForUser(userId!);
            return View(orders);
        }

        // GET: /Order/Details/{id}
        public async Task<IActionResult> Details(Guid id)
        {
            if (id == Guid.Empty) return NotFound();

            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrWhiteSpace(userId)) return Challenge();

            // Bu metodu OrderService'e eklemen gerekir:
            // Order + Items + Product dahil dönmeli
            var order = await _orderService.GetOrderDetailsForUserAsync(userId, id);

            if (order is null) return NotFound(); // user'a ait değilse de null döndürmeni öneririm

            return View(order);
        }
    }
}
