using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantFoodOrderingCustomer.Data;
using RestaurantFoodOrderingCustomer.Models;

namespace RestaurantFoodOrderingCustomer.Controllers
{
    public class ComboController : Controller
    {
        private readonly AppDbContext db;

        public ComboController(AppDbContext context)
        {
            db = context;
        }

        // GET: Combo/Index
        public IActionResult Index(string searchTerm, bool? isVeg, decimal? maxPrice, string sortBy)
        {
            // Get available combos
            var combosQuery = db.Combos
                .Where(c => c.IsAvailable)
                .AsQueryable();

            // Apply search filter
            if (!string.IsNullOrEmpty(searchTerm))
            {
                combosQuery = combosQuery.Where(c => 
                    c.ComboName.Contains(searchTerm) || 
                    c.Description!.Contains(searchTerm));
                ViewBag.SearchTerm = searchTerm;
            }

            // Apply veg filter
            if (isVeg.HasValue)
            {
                combosQuery = combosQuery.Where(c => c.IsVeg == isVeg.Value);
                ViewBag.IsVeg = isVeg.Value;
            }

            // Apply price filter
            if (maxPrice.HasValue)
            {
                combosQuery = combosQuery.Where(c => c.ComboPrice <= maxPrice.Value);
                ViewBag.MaxPrice = maxPrice.Value;
            }

            // Apply sorting
            combosQuery = sortBy switch
            {
                "price_low" => combosQuery.OrderBy(c => c.ComboPrice),
                "price_high" => combosQuery.OrderByDescending(c => c.ComboPrice),
                "discount" => combosQuery.OrderByDescending(c => c.Discount),
                "name" => combosQuery.OrderBy(c => c.ComboName),
                _ => combosQuery.OrderBy(c => c.ComboName)
            };
            ViewBag.SortBy = sortBy;

            var combos = combosQuery.ToList();
            return View(combos);
        }

        // GET: Combo/Details/5
        public IActionResult Details(int id)
        {
            var combo = db.Combos
                .Include(c => c.ComboItems!)
                    .ThenInclude(ci => ci.FoodItem)
                        .ThenInclude(f => f!.Category)
                .FirstOrDefault(c => c.ComboId == id);

            if (combo == null)
            {
                TempData["Error"] = "Combo not found";
                return RedirectToAction("Index");
            }

            return View(combo);
        }

        // POST: Combo/AddComboToCart
        [HttpPost]
        public IActionResult AddComboToCart(int comboId, int quantity = 1)
        {
            // Check if customer is logged in
            if (HttpContext.Session.GetInt32("CustomerId") == null)
            {
                return Json(new { success = false, message = "Please login to add combo to cart", requireLogin = true });
            }

            var combo = db.Combos
                .Include(c => c.ComboItems!)
                    .ThenInclude(ci => ci.FoodItem)
                .FirstOrDefault(c => c.ComboId == comboId);

            if (combo == null || !combo.IsAvailable)
            {
                return Json(new { success = false, message = "Combo not available" });
            }

            // Add combo items to cart
            var cart = GetCart();
            
            if (combo.ComboItems != null)
            {
                foreach (var comboItem in combo.ComboItems)
                {
                    if (comboItem.FoodItem != null)
                    {
                        var existingItem = cart.FirstOrDefault(c => c.FoodId == comboItem.FoodId);
                        
                        if (existingItem != null)
                        {
                            existingItem.Quantity += comboItem.Quantity * quantity;
                        }
                        else
                        {
                            cart.Add(new CartItem
                            {
                                FoodId = comboItem.FoodId,
                                FoodName = comboItem.FoodItem.FoodName,
                                Price = combo.ComboPrice / combo.ComboItems.Count, // Distribute combo price
                                Quantity = comboItem.Quantity * quantity,
                                ImageUrl = comboItem.FoodItem.ImageUrl
                            });
                        }
                    }
                }
            }

            SaveCart(cart);
            return Json(new { success = true, message = $"{combo.ComboName} added to cart", cartCount = cart.Sum(c => c.Quantity) });
        }

        // Helper methods
        private List<CartItem> GetCart()
        {
            var cartJson = HttpContext.Session.GetString("ShoppingCart");
            if (string.IsNullOrEmpty(cartJson))
            {
                return new List<CartItem>();
            }
            return System.Text.Json.JsonSerializer.Deserialize<List<CartItem>>(cartJson) ?? new List<CartItem>();
        }

        private void SaveCart(List<CartItem> cart)
        {
            var cartJson = System.Text.Json.JsonSerializer.Serialize(cart);
            HttpContext.Session.SetString("ShoppingCart", cartJson);
        }
    }
}
