using Cramy.Application.Contracts;
using Cramy.Application.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cramy.Web.Controllers
{
    [Authorize(Roles = "Admin,Seller")]
    public class ProductsController : Controller
    {
        private readonly IProductService _service;
        public ProductsController(IProductService service) { _service = service; }

        [AllowAnonymous]
        public async Task<IActionResult> Index(int page = 1) =>
            View(await _service.GetPagedAsync(page, 12));

        [AllowAnonymous]
        public async Task<IActionResult> Detay(Guid id)
        {
            var product = await _service.GetByIdAsync(id);
            if (product is null) return NotFound();
            return View(product);
        }

        public IActionResult Olustur() => View();

        [HttpPost]
        public async Task<IActionResult> Olustur(CreateProductDto dto)
        {
            if (!ModelState.IsValid) return View(dto);
            var id = await _service.CreateAsync(dto);
            return RedirectToAction(nameof(Detay), new { id });
        }
    }

}
