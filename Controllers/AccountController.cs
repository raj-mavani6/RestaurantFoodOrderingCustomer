using Microsoft.AspNetCore.Mvc;
using RestaurantFoodOrderingCustomer.Data;
using RestaurantFoodOrderingCustomer.Models;
using Microsoft.EntityFrameworkCore;

namespace RestaurantFoodOrderingCustomer.Controllers
{
    public class AccountController : Controller // Main class declaration
    {
        private readonly AppDbContext db; // Database connection

        public AccountController(AppDbContext context)
        {
            db = context;
        }

        // GET: Account/Login
        public IActionResult Login() // Method declaration
        {
            return View();
        }

        // POST: Account/Login
        [HttpPost]
        public IActionResult Login(string email, string password)
        {
            // Email and password validation
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ViewBag.Error = "Please enter email and password";
                return View();
            }

            // Customer authentication
            var customer = db.Customers.FirstOrDefault(c => c.Email == email && c.Password == password && c.IsActive);

            if (customer != null)
            {
                HttpContext.Session.SetInt32("CustomerId", customer.CustomerId);
                HttpContext.Session.SetString("CustomerName", customer.FullName);
                return RedirectToAction("Index", "Home");
            }

            ViewBag.Error = "Invalid email or password";
            return View();
        }

        // GET: Account/Register
        public IActionResult Register()
        {
            return View();
        }

        // POST: Account/Register
        [HttpPost]
        public IActionResult Register(Customer customer)
        {
            if (ModelState.IsValid)
            {
                // Check for existing customer with the same email
                var existingCustomer = db.Customers.FirstOrDefault(c => c.Email == customer.Email);
                if (existingCustomer != null)
                {
                    ViewBag.Error = "Email already registered";
                    return View(customer);
                }

                customer.CreatedDate = DateTime.Now; // Set creation date
                customer.IsActive = true; // Set active status
                db.Customers.Add(customer);
                db.SaveChanges();

                ViewBag.Success = "Registration successful! Please login.";

                return RedirectToAction("Login"); // Redirect to login page
            }

            return View(customer);
        }

        // GET: Account/Profile
        public IActionResult Profile()
        {
            var customerId = HttpContext.Session.GetInt32("CustomerId"); // Get customer ID from session
            if (customerId == null)
            {
                return RedirectToAction("Login");
            }

            var customer = db.Customers.Find(customerId.Value);
            if (customer == null)
            {
                return RedirectToAction("Login");
            }

            return View(customer);
        }

        // POST: Account/UpdateProfile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateProfile(Customer customer)
        {
            var customerId = HttpContext.Session.GetInt32("CustomerId");
            if (customerId == null)
            {
                return RedirectToAction("Login");
            }

            if (ModelState.IsValid)
            {
                var existingCustomer = db.Customers.Find(customerId.Value);
                if (existingCustomer != null)
                {
                    // Update only allowed fields (not password or email for security)
                    existingCustomer.FullName = customer.FullName;
                    existingCustomer.Phone = customer.Phone;
                    existingCustomer.Address = customer.Address;

                    db.SaveChanges();
                    HttpContext.Session.SetString("CustomerName", customer.FullName);

                    ViewBag.Success = "Profile updated successfully!";
                    return View("Profile", existingCustomer);
                }
            }
            return View("Profile", customer);
        }

        // GET: Account/ChangePassword
        public IActionResult ChangePassword()
        {
            var customerId = HttpContext.Session.GetInt32("CustomerId");
            if (customerId == null)
            {
                return RedirectToAction("Login");
            }

            return View();
        }

        // POST: Account/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ChangePassword(string currentPassword, string newPassword, string confirmPassword)
        {
            // GetInt32 - Integer valur read in session
            // SetInt32 - Integer valur Store in session
            var customerId = HttpContext.Session.GetInt32("CustomerId");
            if (customerId == null)
            {
                return RedirectToAction("Login");
            }

            if (string.IsNullOrEmpty(currentPassword) || string.IsNullOrEmpty(newPassword) || string.IsNullOrEmpty(confirmPassword))
            {
                ViewBag.Error = "All fields are required";
                return View();
            }

            if (newPassword != confirmPassword) // Check if new password matches confirm password
            {
                ViewBag.Error = "New password and confirm password do not match";
                return View();
            }

            if (newPassword.Length < 6)
            {
                ViewBag.Error = "Password must be at least 6 characters long";
                return View();
            }

            var customer = db.Customers.Find(customerId.Value);
            if (customer == null || customer.Password != currentPassword) // Verify current password
            {
                ViewBag.Error = "Current password is incorrect";
                return View();
            }

            customer.Password = newPassword; // Update password
            db.SaveChanges();

            ViewBag.Success = "Password changed successfully!";
            return View();
        }

        // GET: Account/Logout
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}
