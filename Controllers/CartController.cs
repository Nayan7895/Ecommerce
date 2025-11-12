using Ecommerce.Data;
using Ecommerce.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        public readonly ApplicationDbContext _context;
        public readonly UserManager<IdentityUser> _userManager;

        public CartController(ApplicationDbContext context, UserManager<IdentityUser> userManager = null)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var currentUser = await _userManager.GetUserAsync(HttpContext.User);
            var cart = await _context.Carts.Include(x => x.Product)
                                           .Where(x => x.UserId == currentUser.Id).ToListAsync();

            double totalCost = 0;

            foreach (var cartItem in cart)
            {
                totalCost += cartItem.Product.Price * cartItem.Qty;
            }

            ViewBag.TotalCost = totalCost;

            return View("Index", cart);
        }

        [HttpGet]
        public async Task<IActionResult> UpdateQty(int productId, int qty)
        {
            var product = await _context.Products.Where(x => x.Id == productId).FirstOrDefaultAsync();
            if (product == null)
            {
                return BadRequest();
            }

            var currentUser = await _userManager.GetUserAsync(HttpContext.User);

            var cartItem = await _context.Carts.Where(x => x.UserId == currentUser.Id)
                                               .Where(x => x.ProductId == productId).FirstOrDefaultAsync();
            if (cartItem == null)
            {
                return BadRequest();
            }

            cartItem.Qty = qty;
            _context.Carts.Update(cartItem);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        public async Task<ActionResult> AddToCart(int productId, int qty = 1)
        {
            var currentUser = await _userManager.GetUserAsync(HttpContext.User);

            var cartItem = await _context.Carts.Where(x => x.UserId == currentUser.Id)
                                               .Where(x => x.ProductId == productId).FirstOrDefaultAsync();
            if (cartItem != null)
            {
                cartItem.Qty = cartItem.Qty + 1;
                _context.Update(cartItem);
                await _context.SaveChangesAsync();
            }
            else
            {
                var cart = new Cart { ProductId = productId, Qty = qty, UserId = currentUser.Id };
                _context.Add(cart);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Remove(int id)
        {
            var cartItem = await _context.Carts.FindAsync(id);
            if (cartItem == null)
            {
                return BadRequest();
            }
            _context.Remove(cartItem);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }
    }
}
