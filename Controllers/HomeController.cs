using Microsoft.AspNetCore.Mvc;
using RestaurantFoodOrderingCustomer.Data;
using RestaurantFoodOrderingCustomer.Models;
using Microsoft.EntityFrameworkCore;

namespace RestaurantFoodOrderingCustomer.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext db;

        public HomeController(AppDbContext context) // Constructor method
        {
            db = context;
        }

        // GET: Home/Index
        public IActionResult Index()
        {
            // Categories fetch query
            var categories = db.Categories.Where(c => c.IsActive).ToList();
            // Food items fetch query
            var foodItems = db.FoodItems.Include(f => f.Category).Where(f => f.IsAvailable).ToList();

            ViewBag.Categories = categories;
            return View(foodItems);
        }

        // GET: Home/Menu
        public IActionResult Menu(int? categoryId, string searchName, decimal? minPrice, decimal? maxPrice,
                                   bool? isVeg, decimal? minRating, string sortBy, List<int>? categories)
        {
            var categoriesList = db.Categories.Where(c => c.IsActive).ToList();
            ViewBag.Categories = categoriesList;

            var foodItems = db.FoodItems
                .Include(f => f.Category)
                .Include(f => f.Reviews)
                .Where(f => f.IsAvailable)
                .AsQueryable();

            // Category filter (single or multiple)
            if (categoryId.HasValue && categoryId.Value > 0)
            {
                foodItems = foodItems.Where(f => f.CategoryId == categoryId.Value);
                ViewBag.SelectedCategory = categoryId.Value;
            }
            else if (categories != null && categories.Any())
            {
                foodItems = foodItems.Where(f => categories.Contains(f.CategoryId));
                ViewBag.SelectedCategories = categories;
            }

            // Name search filter
            if (!string.IsNullOrEmpty(searchName))
            {
                foodItems = foodItems.Where(f => f.FoodName.Contains(searchName) ||
                                                 f.Description.Contains(searchName));
                ViewBag.SearchName = searchName;
            }

            // Price range filter
            if (minPrice.HasValue)
            {
                foodItems = foodItems.Where(f => f.Price >= minPrice.Value);
                ViewBag.MinPrice = minPrice.Value;
            }
            if (maxPrice.HasValue)
            {
                foodItems = foodItems.Where(f => f.Price <= maxPrice.Value);
                ViewBag.MaxPrice = maxPrice.Value;
            }

            // Veg/Non-veg filter
            if (isVeg.HasValue)
            {
                foodItems = foodItems.Where(f => f.IsVeg == isVeg.Value);
                ViewBag.IsVeg = isVeg.Value;
            }

            // Get results with reviews for rating filter and sorting
            var results = foodItems.ToList().Select(f => new
            {
                FoodItem = f,
                AverageRating = f.Reviews != null && f.Reviews.Any()
                    ? f.Reviews.Where(r => r.IsApproved).Average(r => (decimal)r.Rating)
                    : 0,
                ReviewCount = f.Reviews != null ? f.Reviews.Count(r => r.IsApproved) : 0
            }).ToList();

            // Rating filter
            if (minRating.HasValue)
            {
                results = results.Where(r => r.AverageRating >= minRating.Value).ToList();
                ViewBag.MinRating = minRating.Value;
            }

            // Sorting
            results = sortBy switch
            {
                "price_low" => results.OrderBy(r => r.FoodItem.Price).ToList(),
                "price_high" => results.OrderByDescending(r => r.FoodItem.Price).ToList(),
                "rating_high" => results.OrderByDescending(r => r.AverageRating).ToList(),
                "rating_low" => results.OrderBy(r => r.AverageRating).ToList(),
                "popular" => results.OrderByDescending(r => r.ReviewCount).ToList(),
                "name" => results.OrderBy(r => r.FoodItem.FoodName).ToList(),
                _ => results.OrderBy(r => r.FoodItem.FoodName).ToList()
            };
            ViewBag.SortBy = sortBy ?? "name";

            // Get price range for slider
            var allPrices = db.FoodItems.Where(f => f.IsAvailable).Select(f => f.Price).ToList();
            ViewBag.MinPriceRange = allPrices.Any() ? allPrices.Min() : 0;
            ViewBag.MaxPriceRange = allPrices.Any() ? allPrices.Max() : 1000;

            return View(results.Select(r => r.FoodItem).ToList());
        }

        // GET: Home/FoodDetails
        public IActionResult FoodDetails(int id)
        {
            // Specific food item ID thi fetch kare che
            var foodItem = db.FoodItems.Include(f => f.Category).FirstOrDefault(f => f.FoodId == id);
            if (foodItem == null)
            {
                return NotFound();
            }

            // Load reviews for this food item
            var reviews = db.Reviews
                .Include(r => r.Customer)
                .Where(r => r.FoodId == id && r.IsApproved)
                .OrderByDescending(r => r.ReviewDate)
                .ToList();

            ViewBag.Reviews = reviews;
            ViewBag.AverageRating = reviews.Any() ? reviews.Average(r => r.Rating) : 0;
            ViewBag.TotalReviews = reviews.Count;

            // Check if current customer has already reviewed
            var customerId = HttpContext.Session.GetInt32("CustomerId");
            if (customerId != null)
            {
                var customerReview = reviews.FirstOrDefault(r => r.CustomerId == customerId.Value);
                ViewBag.HasReviewed = customerReview != null;
            }

            return View(foodItem);
        }

        // GET: Home/About
        public IActionResult About()
        {
            return View();
        }

        // GET: Home/Contact
        public IActionResult Contact()
        {
            return View();
        }

        // POST: Home/Contact
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Contact(string name, string email, string phone, string subject, string message)
        {
            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(message))
            {
                TempData["Error"] = "Please fill all required fields!";
                return View();
            }

            // Here you can add logic to save the contact message to database
            // or send email notification to admin

            TempData["Success"] = "Thank you for contacting us! We will get back to you soon.";
            return RedirectToAction("Contact");
        }
    }
}
