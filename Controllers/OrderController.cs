using Microsoft.AspNetCore.Mvc;
using RestaurantFoodOrderingCustomer.Data;
using RestaurantFoodOrderingCustomer.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace RestaurantFoodOrderingCustomer.Controllers
{
    public class OrderController : Controller
    {
        private readonly AppDbContext db;
        private const string CartSessionKey = "ShoppingCart";

        public OrderController(AppDbContext context)
        {
            db = context;
        }

        // GET: Order/Checkout
        public IActionResult Checkout()
        {
            var customerId = HttpContext.Session.GetInt32("CustomerId");
            if (customerId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var cart = GetCart();
            if (cart.Count == 0)
            {
                return RedirectToAction("Index", "Home");
            }

            var customer = db.Customers.Find(customerId.Value);
            ViewBag.Customer = customer;
            ViewBag.Cart = cart;
            ViewBag.TotalAmount = cart.Sum(c => c.TotalPrice);

            return View();
        }

        // POST: Order/PlaceOrder
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult PlaceOrder(string address, string phone, string instructions, string paymentMethod,
                                       string? couponCode, int? couponId, decimal? discountAmount, decimal? finalAmount)
        {
            var customerId = HttpContext.Session.GetInt32("CustomerId");
            if (customerId == null)
            {
                return Json(new { success = false, message = "Please login to place order" });
            }

            var cart = GetCart();
            if (cart.Count == 0)
            {
                return Json(new { success = false, message = "Cart is empty" });
            }

            // Set payment status based on payment method
            string paymentStatus = paymentMethod == "Cash on Delivery" ? "Pending" : "Paid";
            string? transactionId = paymentMethod != "Cash on Delivery"
                ? "TXN" + DateTime.Now.Ticks.ToString().Substring(0, 12)
                : null;

            decimal totalAmount = cart.Sum(c => c.TotalPrice) + 40; // Include delivery fee

            var order = new Order
            {
                CustomerId = customerId.Value,
                TotalAmount = cart.Sum(c => c.TotalPrice),
                DiscountAmount = discountAmount,
                FinalAmount = finalAmount ?? totalAmount,
                CouponCode = couponCode,
                CouponId = couponId,
                Status = "Pending",
                DeliveryAddress = address,
                ContactPhone = phone,
                SpecialInstructions = instructions,
                PaymentMethod = paymentMethod ?? "Cash on Delivery",
                PaymentStatus = paymentStatus,
                TransactionId = transactionId,
                OrderDate = DateTime.Now
            };

            db.Orders.Add(order);
            db.SaveChanges();

            foreach (var item in cart)
            {
                var orderItem = new OrderItem
                {
                    OrderId = order.OrderId,
                    FoodId = item.FoodId,
                    Quantity = item.Quantity,
                    Price = item.Price,
                    TotalPrice = item.TotalPrice
                };
                db.OrderItems.Add(orderItem);
            }

            db.SaveChanges();

            HttpContext.Session.Remove(CartSessionKey);

            // Alternative: Direct redirect instead of JSON response
            // return RedirectToAction("OrderSuccess", new { orderId = order.OrderId });

            return Json(new { success = true, orderId = order.OrderId });
        }

        // GET: Order/OrderSuccess
        [Route("Order/OrderSuccess/{orderId}")]
        public IActionResult OrderSuccess(int orderId)
        {
            var customerId = HttpContext.Session.GetInt32("CustomerId");
            if (customerId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var order = db.Orders
                .Include(o => o.OrderItems!)
                .ThenInclude(oi => oi.FoodItem)
                .FirstOrDefault(o => o.OrderId == orderId && o.CustomerId == customerId.Value);


            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // GET: Order/MyOrders (Active Orders Only)
        public IActionResult MyOrders(string searchTerm)
        {
            var customerId = HttpContext.Session.GetInt32("CustomerId");
            if (customerId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var orders = db.Orders
                .Include(o => o.OrderItems!)
                .ThenInclude(oi => oi.FoodItem)
                .Where(o => o.CustomerId == customerId.Value && o.Status != "Delivered")
                .AsQueryable();

            // Search filter
            if (!string.IsNullOrEmpty(searchTerm))
            {
                orders = orders.Where(o => 
                    o.OrderId.ToString().Contains(searchTerm) ||
                    o.OrderItems!.Any(oi => oi.FoodItem!.FoodName.Contains(searchTerm)));
                ViewBag.SearchTerm = searchTerm;
            }

            var orderList = orders.OrderByDescending(o => o.OrderDate).ToList();

            return View(orderList);
        }

        // GET: Order/OrderHistory (Delivered Orders Only)
        public IActionResult OrderHistory(string searchTerm)
        {
            var customerId = HttpContext.Session.GetInt32("CustomerId");
            if (customerId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var orders = db.Orders
                .Include(o => o.OrderItems!)
                .ThenInclude(oi => oi.FoodItem)
                .Where(o => o.CustomerId == customerId.Value && o.Status == "Delivered")
                .AsQueryable();

            // Search filter
            if (!string.IsNullOrEmpty(searchTerm))
            {
                orders = orders.Where(o => 
                    o.OrderId.ToString().Contains(searchTerm) ||
                    o.OrderItems!.Any(oi => oi.FoodItem!.FoodName.Contains(searchTerm)));
                ViewBag.SearchTerm = searchTerm;
            }

            var orderList = orders.OrderByDescending(o => o.OrderDate).ToList();

            return View(orderList);
        }

        // GET: Order/OrderDetails
        public IActionResult OrderDetails(int id)
        {
            var customerId = HttpContext.Session.GetInt32("CustomerId");
            if (customerId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var order = db.Orders
                .Include(o => o.OrderItems!)
                .ThenInclude(oi => oi.FoodItem)
                .FirstOrDefault(o => o.OrderId == id && o.CustomerId == customerId.Value);


            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // Helper method
        private List<CartItem> GetCart()
        {
            var cartJson = HttpContext.Session.GetString(CartSessionKey);
            if (string.IsNullOrEmpty(cartJson))
            {
                return new List<CartItem>();
            }
            return JsonSerializer.Deserialize<List<CartItem>>(cartJson) ?? new List<CartItem>();
        }
    }
}
