using Ecommerce.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Controllers
{
    public class ProductController : Controller
    {
        public readonly ApplicationDbContext _context;
        public ProductController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int? categoryId)
        {
            var title = "All Products";

            var data = _context.Products.AsQueryable();

            if (categoryId != null)
            {
                data = data.Where(x => x.CategoryId == categoryId);

                var category = await _context.Categories.FirstOrDefaultAsync(x => x.Id == categoryId);

                if (category != null)
                {
                    title = $"{category.Name}s";
                }
            }

            var categories = await _context.Categories.ToListAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "Name", categoryId);

            ViewBag.Title = title;

            var products = await data.ToListAsync();

            return View(products);
        }

        public async Task<IActionResult> Detail(int productId)
        {
            var product = await _context.Products.Where(x => x.Id == productId).FirstOrDefaultAsync();
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }
    }
}
