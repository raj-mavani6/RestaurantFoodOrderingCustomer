using Microsoft.AspNetCore.Mvc;
using RestaurantFoodOrderingCustomer.Data;
using RestaurantFoodOrderingCustomer.Models;
using System.Text.Json;

namespace RestaurantFoodOrderingCustomer.Controllers
{
    public class CartController : Controller
    {
        private readonly AppDbContext db;
        private const string CartSessionKey = "ShoppingCart"; // Cart session key constant

        public CartController(AppDbContext context)
        {
            db = context;
        }

        // GET: Cart/Index
        public IActionResult Index()
        {
            var cart = GetCart();
            return View(cart);
        }

        // POST: Cart/AddToCart
        [HttpPost]
        public IActionResult AddToCart(int foodId, int quantity = 1)
        {
            // Check if customer is logged in
            if (HttpContext.Session.GetInt32("CustomerId") == null)
            {
                return Json(new { success = false, message = "Please login to add items to cart", requireLogin = true });
            }

            var food = db.FoodItems.Find(foodId);  // Find food item by ID
            if (food == null || !food.IsAvailable) // Check food availability
            {
                return Json(new { success = false, message = "Food item not available" });
            }

            var cart = GetCart();
            var existingItem = cart.FirstOrDefault(c => c.FoodId == foodId); // Find existing cart item

            if (existingItem != null)
            {
                existingItem.Quantity += quantity; // Update quantity if item exists
            }
            else
            {
                cart.Add(new CartItem
                {
                    FoodId = food.FoodId,
                    FoodName = food.FoodName,
                    Price = food.Price,
                    Quantity = quantity,
                    ImageUrl = food.ImageUrl
                });
            }

            SaveCart(cart);
            return Json(new { success = true, message = "Item added to cart", cartCount = cart.Sum(c => c.Quantity) });
        }

        // POST: Cart/UpdateQuantity
        [HttpPost]
        public IActionResult UpdateQuantity(int foodId, int quantity)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(c => c.FoodId == foodId);

            if (item != null)
            {
                if (quantity <= 0) // Check if quantity is zero or negative
                {
                    cart.Remove(item);
                }
                else
                {
                    item.Quantity = quantity;
                }
                SaveCart(cart);
            }

            return Json(new { success = true, cartCount = cart.Sum(c => c.Quantity) });
        }

        // POST: Cart/RemoveFromCart
        [HttpPost]
        public IActionResult RemoveFromCart(int foodId)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(c => c.FoodId == foodId);

            if (item != null)
            {
                cart.Remove(item);
                SaveCart(cart);
            }

            return Json(new { success = true, cartCount = cart.Sum(c => c.Quantity) });
        }

        // GET: Cart/GetCartCount
        public IActionResult GetCartCount()
        {
            var cart = GetCart();
            return Json(new { count = cart.Sum(c => c.Quantity) });
        }

        // Helper methods
        private List<CartItem> GetCart()
        {
            var cartJson = HttpContext.Session.GetString(CartSessionKey);
            if (string.IsNullOrEmpty(cartJson))
            {
                return new List<CartItem>();
            }
            return JsonSerializer.Deserialize<List<CartItem>>(cartJson) ?? new List<CartItem>();
        }

        private void SaveCart(List<CartItem> cart)
        {
            var cartJson = JsonSerializer.Serialize(cart); // Serialize cart to JSON
            HttpContext.Session.SetString(CartSessionKey, cartJson); // Store cart JSON in session
        }
    }
}
