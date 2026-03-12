using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Project5_test.Models;
using System.Diagnostics;


namespace Project5_test.Controllers
{
    public class AccountController : Controller
    {
        private readonly MyDbContext _context;

        public AccountController(MyDbContext context)
        {
            _context = context;
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string email, string password)
        {
            var user = _context.Users
                .FirstOrDefault(x => x.Email == email && x.Password == password);

            if (user == null)
            {
                ViewBag.msg = "Invalid Email or Password";
                return View();
            }

            HttpContext.Session.SetInt32("UserId", user.UserId);
            HttpContext.Session.SetString("UserName", user.FirstName);
            HttpContext.Session.SetString("Role", user.Role ?? "Student");


            if (user.Role == "Admin")
            {
                return RedirectToAction("Dashboard", "Admin");
            }

            if (user.AccountStatus == "Block")
            {
                ViewBag.msg = "Sorry! You are Blocked";
                return View();
            }

            return RedirectToAction("Index", "Home");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult SignUp()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SignUp(User model, string confirmPassword)
        {
            if (model.Password != confirmPassword)
            {
                ModelState.AddModelError("Password", "Passwords do not match.");
            }

            if (ModelState.IsValid)
            {
                if (model.Email != null && !model.Email.EndsWith("ses.yu.edu.jo"))
                {
                    ModelState.AddModelError("Email", "Must be a university email (@ses.yu.edu.jo)");
                    return View(model);
                }

                if (_context.Users.Any(u => u.Email == model.Email))
                {
                    ModelState.AddModelError("Email", "This email is already registered.");
                    return View(model);
                }

                model.Role = "Student";
                _context.Users.Add(model);
                _context.SaveChanges();

                HttpContext.Session.SetString("UserId", model.UserId.ToString());
                HttpContext.Session.SetString("UserName", model.FirstName);
                HttpContext.Session.SetString("Role", model.Role);

                return RedirectToAction("Index", "Home");
            }

            return View(model);
        }
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public IActionResult ForgotPassword(string email, string newPassword)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == email);
            if (user != null)
            {
                user.Password = newPassword;
                _context.SaveChanges();
                ViewBag.msg = "Password reset successfully!";
                return View("Login");
            }
            ViewBag.msg = "Email not found.";
            return View();
        }

        public IActionResult GoogleLogin()
        {
            var redirectUrl = Url.Action("GoogleResponse", "Account");
            var properties = new AuthenticationProperties
            {
                RedirectUri = redirectUrl
            };

            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }
        public async Task<IActionResult> GoogleResponse()
        {
            var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            var claims = result.Principal.Identities
                .FirstOrDefault().Claims
                .Select(claim => new
                {
                    claim.Type,
                    claim.Value
                });

            var email = claims.FirstOrDefault(x => x.Type.Contains("email"))?.Value;
            var name = claims.FirstOrDefault(x => x.Type.Contains("name"))?.Value;

            var user = _context.Users.FirstOrDefault(x => x.Email == email);

            if (user == null)
            {
                user = new User
                {
                    Email = email,
                    FirstName = name,
                    Role = "Student"
                };

                _context.Users.Add(user);
                _context.SaveChanges();
            }

            HttpContext.Session.SetInt32("UserId", user.UserId);
            HttpContext.Session.SetString("UserName", user.FirstName);
            HttpContext.Session.SetString("Role", user.Role ?? "Student");

            return RedirectToAction("Index", "Home");
        }
    }
}