using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantFoodOrderingCustomer.Data;
using RestaurantFoodOrderingCustomer.Models;

namespace RestaurantFoodOrderingCustomer.Controllers
{
    public class CouponController : Controller
    {
        private readonly AppDbContext db;

        public CouponController(AppDbContext context) // Constructor with dependency injection
        {
            db = context; // Assign injected context to db field
        }

        // GET: Coupon/MyCoupons - Display all coupons assigned to customer
        public IActionResult MyCoupons()
        {
            var customerId = HttpContext.Session.GetInt32("CustomerId"); 
            if (customerId == null)                                      
            {
                return RedirectToAction("Login", "Account");
            }

            var customerCoupons = db.CustomerCoupons                                 // Get customer's coupons from database
                .Include(cc => cc.Coupon)                                            // Include related coupon details
                .Where(cc => cc.CustomerId == customerId.Value && cc.Coupon != null) // Filter by customer ID
                .OrderByDescending(cc => cc.AssignedDate)                            // Sort by assigned date (latest first)
                .ToList();                                                           // Execute query and convert to list

            return View(customerCoupons);
        }

        // GET: Coupon/AvailableCoupons - Get all valid coupons for checkout page
        public IActionResult AvailableCoupons(decimal orderAmount)
        {
            var customerId = HttpContext.Session.GetInt32("CustomerId"); 
            if (customerId == null)
            {
                return Json(new { success = false, message = "Please login" });
            }

            var today = DateTime.Now;

            var availableCoupons = db.CustomerCoupons                    // Query customer coupons
                .Include(cc => cc.Coupon)                                // Include coupon details
                .Where(cc => cc.CustomerId == customerId.Value &&        // Filter by customer
                            !cc.IsUsed &&                                // Not used yet
                            cc.RemainingUsage > 0 &&                     // Has remaining usage
                            cc.Coupon != null &&                         // Coupon exists
                            cc.Coupon.IsActive &&                        // Coupon is active
                            cc.Coupon.StartDate <= today &&              // Started
                            cc.Coupon.ExpiryDate >= today &&             // Not expired
                            cc.Coupon.MinimumOrderAmount <= orderAmount) // Meets minimum order requirement
                .Select(cc => new                                        // Project to anonymous object
                {
                    couponId = cc.CouponId,                    // Coupon ID
                    code = cc.Coupon.CouponCode,               // Coupon code
                    description = cc.Coupon.Description,       // Description
                    discountType = cc.Coupon.DiscountType,     // Percentage or Fixed
                    discountValue = cc.Coupon.DiscountValue,   // Discount value
                    maxDiscount = cc.Coupon.MaxDiscountAmount, // Maximum discount cap
                    minOrder = cc.Coupon.MinimumOrderAmount,   // Minimum order amount
                    expiryDate = cc.Coupon.ExpiryDate,         // Expiry date
                    remainingUsage = cc.RemainingUsage         // Remaining usage count
                })
                .ToList(); // Execute query

            return Json(new { success = true, coupons = availableCoupons });
        }

        // POST: Coupon/ValidateCoupon - Validate coupon code and calculate discount
        [HttpPost]
        public IActionResult ValidateCoupon(string couponCode, decimal orderAmount) 
        {
            var customerId = HttpContext.Session.GetInt32("CustomerId");
            if (customerId == null)
            {
                return Json(new { success = false, message = "Please login" });
            }

            var today = DateTime.Now;

            var customerCoupon = db.CustomerCoupons                        // Find customer's coupon
                .Include(cc => cc.Coupon)                                  // Include coupon details
                .FirstOrDefault(cc => cc.CustomerId == customerId.Value && // Match customer
                                     cc.Coupon != null &&                  // Coupon exists
                                     cc.Coupon.CouponCode == couponCode && // Match coupon code
                                     !cc.IsUsed &&                         // Not used
                                     cc.RemainingUsage > 0 &&              // Has usage left
                                     cc.Coupon.IsActive &&                 // Active
                                     cc.Coupon.StartDate <= today &&       // Valid start date
                                     cc.Coupon.ExpiryDate >= today);       // Not expired

            if (customerCoupon == null || customerCoupon.Coupon == null)
            {
                return Json(new { success = false, message = "Invalid or expired coupon code" });
            }

            var coupon = customerCoupon.Coupon;

            // Order amount less than minimum
            if (orderAmount < coupon.MinimumOrderAmount)
            {
                // Return minimum order error
                return Json(new
                {
                    success = false,
                    message = $"Minimum order amount of ₹{coupon.MinimumOrderAmount} required"
                });
            }

            // Calculate discount based on coupon type
            decimal discountAmount = 0; 
            if (coupon.DiscountType == "Percentage") 
            {
                // Calculate percentage discount
                discountAmount = (orderAmount * coupon.DiscountValue) / 100;

                // Apply maximum discount
                if (coupon.MaxDiscountAmount.HasValue && discountAmount > coupon.MaxDiscountAmount.Value)
                {
                    // Cap at maximum discount
                    discountAmount = coupon.MaxDiscountAmount.Value;
                }
            }
            else if (coupon.DiscountType == "Fixed") // Fixed discount type
            {
                discountAmount = coupon.DiscountValue;
            }

            // Calculate final amount after discount
            decimal finalAmount = orderAmount - discountAmount;

            return Json(new
            {
                success = true,
                couponId = coupon.CouponId, 
                code = coupon.CouponCode,
                description = coupon.Description,
                discountType = coupon.DiscountType,
                discountValue = coupon.DiscountValue,
                discountAmount = Math.Round(discountAmount, 2),
                finalAmount = Math.Round(finalAmount, 2),
                message = $"Coupon applied! You saved ₹{Math.Round(discountAmount, 2)}"
            });
        }

        // GET: Coupon/GetBestCoupon - Auto-select best coupon for maximum savings
        public IActionResult GetBestCoupon(decimal orderAmount)
        {
            var customerId = HttpContext.Session.GetInt32("CustomerId"); 
            if (customerId == null)
            {
                return Json(new { success = false, message = "Please login" });
            }

            var today = DateTime.Now;

            var availableCoupons = db.CustomerCoupons // Get all valid coupons
                .Include(cc => cc.Coupon)
                .Where(cc => cc.CustomerId == customerId.Value &&
                            !cc.IsUsed && 
                            cc.RemainingUsage > 0 && 
                            cc.Coupon != null &&
                            cc.Coupon.IsActive && 
                            cc.Coupon.StartDate <= today && 
                            cc.Coupon.ExpiryDate >= today &&
                            cc.Coupon.MinimumOrderAmount <= orderAmount) 
                .ToList();

            if (!availableCoupons.Any())
            {
                return Json(new { success = false, message = "No coupons available" });
            }

            // Calculate discount for each coupon and find the best one (maximum savings)
            var bestCoupon = availableCoupons 
                .Select(cc =>
                {
                    decimal discountAmount = 0; 
                    var coupon = cc.Coupon;

                    if (coupon.DiscountType == "Percentage") 
                    {
                        discountAmount = (orderAmount * coupon.DiscountValue) / 100; // 
                        if (coupon.MaxDiscountAmount.HasValue && discountAmount > coupon.MaxDiscountAmount.Value) 
                        {
                            discountAmount = coupon.MaxDiscountAmount.Value;
                        }
                    }
                    else if (coupon.DiscountType == "Fixed") 
                    {
                        discountAmount = coupon.DiscountValue;
                    }

                    return new 
                    {
                        customerCoupon = cc,
                        coupon = coupon,
                        discountAmount = discountAmount
                    };
                })
                .OrderByDescending(x => x.discountAmount)
                .FirstOrDefault();

            // Best coupon found
            if (bestCoupon != null)
            {
                decimal finalAmount = orderAmount - bestCoupon.discountAmount;

                return Json(new
                {
                    success = true,
                    couponId = bestCoupon.coupon.CouponId,
                    code = bestCoupon.coupon.CouponCode,
                    description = bestCoupon.coupon.Description,
                    discountType = bestCoupon.coupon.DiscountType,
                    discountValue = bestCoupon.coupon.DiscountValue,
                    discountAmount = Math.Round(bestCoupon.discountAmount, 2),
                    finalAmount = Math.Round(finalAmount, 2),
                    message = $"Best coupon applied! You saved ₹{Math.Round(bestCoupon.discountAmount, 2)}"
                });
            }

            return Json(new { success = false, message = "No suitable coupon found" });
        }

        // Mark coupon as used
        public IActionResult MarkCouponAsUsed(int couponId, int orderId)
        {
            var customerId = HttpContext.Session.GetInt32("CustomerId");
            if (customerId == null)
            {
                return Json(new { success = false, message = "Unauthorized" });
            }

            var customerCoupon = db.CustomerCoupons
                .FirstOrDefault(cc => cc.CustomerId == customerId.Value && // Match customer
                                     cc.CouponId == couponId && // Match coupon ID
                                     !cc.IsUsed);

            if (customerCoupon != null)
            {
                customerCoupon.IsUsed = true;
                customerCoupon.UsedDate = DateTime.Now;
                customerCoupon.OrderId = orderId;
                customerCoupon.RemainingUsage = 0;

                db.SaveChanges();

                return Json(new { success = true, message = "Coupon marked as used" }); 
            }

            return Json(new { success = false, message = "Coupon not found" });
        }
    }
}
