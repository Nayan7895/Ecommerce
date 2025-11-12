using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Ecommerce.Data;
using Ecommerce.Models;

namespace Ecommerce.Areas.Dashboard.Controllers
{
    [Area("Dashboard")]
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Dashboard/Products
        public async Task<IActionResult> Index()
        {
            return View(await _context.Products.Include(x => x.Category).ToListAsync());
        }

        // GET: Dashboard/Products/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products.Include(x => x.Category)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // GET: Dashboard/Products/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.categories = await _context.Categories.ToListAsync();

            return View();
        }

        // POST: Dashboard/Products/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product, IFormFile Image)
        {
            if (ModelState.IsValid)
            {
                ViewBag.categories = await _context.Categories.ToListAsync();

                if (Image == null)
                {
                    ModelState.AddModelError(product.Image, "Image is required.");
                    return View(product);
                }

                var imageName = Guid.NewGuid() + Path.GetExtension(Image.FileName);
                if (!Directory.Exists(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/img/Products")))
                {
                    Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/img/Products"));
                }

                var savePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/img/Products", imageName);
                await using (var stream = new FileStream(savePath, FileMode.Create))
                {
                    await Image.CopyToAsync(stream);
                }

                product.Image = $"/img/Products/{imageName}";

                _context.Add(product);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(product);
        }

        // GET: Dashboard/Products/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            ViewBag.categories = await _context.Categories.ToListAsync();

            return View(product);
        }

        // POST: Dashboard/Products/Edit/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Product product, IFormFile Image)
        {
            if (id != product.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var oldProduct = await _context.Products.FirstOrDefaultAsync(x => x.Id == id);

                    if (Image != null)
                    {
                        var imageName = Guid.NewGuid() + Path.GetExtension(Image.FileName);
                        if (!Directory.Exists(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/img/Products")))
                        {
                            Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/img/Products"));
                        }

                        var savePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/img/Products", imageName);
                        await using (var stream = new FileStream(savePath, FileMode.Create))
                        {
                            await Image.CopyToAsync(stream);
                        }

                        oldProduct.Image = $"/img/Products/{imageName}";

                    }

                    oldProduct.Price = product.Price;
                    oldProduct.Description = product.Description;
                    oldProduct.Name = product.Name;
                    oldProduct.CategoryId = product.CategoryId;

                    _context.Update(oldProduct);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(product.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(product);
        }

        // GET: Dashboard/Products/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products.Include(x => x.Category)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // POST: Dashboard/Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                _context.Products.Remove(product);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.Id == id);
        }
    }
}
