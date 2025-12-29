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
    }
}
