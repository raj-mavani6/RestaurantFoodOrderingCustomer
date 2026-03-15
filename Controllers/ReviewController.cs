using Microsoft.AspNetCore.Mvc;
using RestaurantFoodOrderingCustomer.Data;
using RestaurantFoodOrderingCustomer.Models;
using Microsoft.EntityFrameworkCore;

namespace RestaurantFoodOrderingCustomer.Controllers
{
    public class ReviewController : Controller
    {
        private readonly AppDbContext db;

        public ReviewController(AppDbContext context)
        {
            db = context;
        }

        // GET: Review/Create
        public IActionResult Create(int foodId, int? orderId)
        {
            var customerId = HttpContext.Session.GetInt32("CustomerId");
            if (customerId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var foodItem = db.FoodItems
                .Include(f => f.Category)
                .FirstOrDefault(f => f.FoodId == foodId);

            if (foodItem == null)
            {
                return NotFound();
            }

            // Check if customer already reviewed this food item
            var existingReview = db.Reviews
                .FirstOrDefault(r => r.CustomerId == customerId.Value && r.FoodId == foodId);

            if (existingReview != null)
            {
                ViewBag.Message = "You have already reviewed this item. You can update your review.";
                ViewBag.ExistingReview = existingReview;
            }

            ViewBag.FoodItem = foodItem;
            ViewBag.OrderId = orderId;

            return View();
        }

        // POST: Review/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Review review)
        {
            var customerId = HttpContext.Session.GetInt32("CustomerId");
            if (customerId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (ModelState.IsValid)
            {
                // Check if customer already reviewed this food item
                var existingReview = db.Reviews
                    .FirstOrDefault(r => r.CustomerId == customerId.Value && r.FoodId == review.FoodId);

                if (existingReview != null)
                {
                    // Update existing review
                    existingReview.Rating = review.Rating;
                    existingReview.Comment = review.Comment;
                    existingReview.ReviewDate = DateTime.Now;
                    db.SaveChanges();

                    TempData["Success"] = "Your review has been updated successfully!";
                }
                else
                {
                    // Create new review
                    review.CustomerId = customerId.Value;
                    review.ReviewDate = DateTime.Now;
                    review.IsApproved = true;

                    db.Reviews.Add(review);
                    db.SaveChanges();

                    TempData["Success"] = "Thank you for your review!";
                }

                return RedirectToAction("FoodDetails", "Home", new { id = review.FoodId });
            }

            var foodItem = db.FoodItems
                .Include(f => f.Category)
                .FirstOrDefault(f => f.FoodId == review.FoodId);

            ViewBag.FoodItem = foodItem;
            return View(review);
        }

        // GET: Review/MyReviews
        public IActionResult MyReviews()
        {
            var customerId = HttpContext.Session.GetInt32("CustomerId");
            if (customerId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var reviews = db.Reviews
                .Include(r => r.FoodItem)
                .ThenInclude(f => f!.Category)
                .Where(r => r.CustomerId == customerId.Value)
                .OrderByDescending(r => r.ReviewDate)
                .ToList();

            return View(reviews);
        }

        // GET: Review/Edit
        public IActionResult Edit(int id)
        {
            var customerId = HttpContext.Session.GetInt32("CustomerId");
            if (customerId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var review = db.Reviews
                .Include(r => r.FoodItem)
                .ThenInclude(f => f!.Category)
                .FirstOrDefault(r => r.ReviewId == id && r.CustomerId == customerId.Value);

            if (review == null)
            {
                return NotFound();
            }

            return View(review);
        }

        // POST: Review/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Review review)
        {
            var customerId = HttpContext.Session.GetInt32("CustomerId");
            if (customerId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (ModelState.IsValid)
            {
                var existingReview = db.Reviews
                    .FirstOrDefault(r => r.ReviewId == review.ReviewId && r.CustomerId == customerId.Value);

                if (existingReview != null)
                {
                    existingReview.Rating = review.Rating;
                    existingReview.Comment = review.Comment;
                    existingReview.ReviewDate = DateTime.Now;

                    db.SaveChanges();

                    TempData["Success"] = "Review updated successfully!";
                    return RedirectToAction("MyReviews");
                }
            }

            return View(review);
        }

        // POST: Review/Delete
        [HttpPost]
        public IActionResult Delete(int id)
        {
            var customerId = HttpContext.Session.GetInt32("CustomerId");
            if (customerId == null)
            {
                return Json(new { success = false, message = "Please login" });
            }

            var review = db.Reviews
                .FirstOrDefault(r => r.ReviewId == id && r.CustomerId == customerId.Value);

            if (review != null)
            {
                db.Reviews.Remove(review);
                db.SaveChanges();
                return Json(new { success = true, message = "Review deleted successfully" });
            }

            return Json(new { success = false, message = "Review not found" });
        }

        // GET: Review/FoodReviews (for displaying all reviews of a food item)
        public IActionResult FoodReviews(int foodId)
        {
            var foodItem = db.FoodItems
                .Include(f => f.Category)
                .FirstOrDefault(f => f.FoodId == foodId);

            if (foodItem == null)
            {
                return NotFound();
            }

            var reviews = db.Reviews
                .Include(r => r.Customer)
                .Where(r => r.FoodId == foodId && r.IsApproved)
                .OrderByDescending(r => r.ReviewDate)
                .ToList();

            ViewBag.FoodItem = foodItem;
            ViewBag.AverageRating = reviews.Any() ? reviews.Average(r => r.Rating) : 0;
            ViewBag.TotalReviews = reviews.Count;

            return View(reviews);
        }
    }
}
