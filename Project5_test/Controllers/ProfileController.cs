using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.EntityFrameworkCore;
using Project5_test.Models;
using BCrypt.Net;


namespace Project5_test.Controllers
{
    public class ProfileController : Controller
    {
        private readonly MyDbContext _context;

        public ProfileController(MyDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }


            var user = _context.Users
                .Include(u => u.Borrowings).ThenInclude(b => b.Book)
                .Include(u => u.RoomReservations).ThenInclude(r => r.Room)
                .FirstOrDefault(u => u.UserId == userId.Value);

            if (user == null) return NotFound();

            return View(user);
        }
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var user = _context.Users.Find(id);

            if (user == null)
                return RedirectToAction("Index");

            return View(user);
        }

        [HttpPost]
        public IActionResult Edit(User model, string confirmPassword, IFormFile profilePicture)
        {
            var user = _context.Users.Find(model.UserId);

            if (user == null)
                return RedirectToAction("Index");

            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.Phone = model.Phone;

            if (profilePicture != null && profilePicture.Length > 0)
            {
                string folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Images");

                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);

                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(profilePicture.FileName);
                string path = Path.Combine(folder, fileName);

                using (var stream = new FileStream(path, FileMode.Create))
                {
                    profilePicture.CopyTo(stream);
                }

                user.ProfilePicture = "/Images/" + fileName;
            }

            _context.SaveChanges();

            TempData["Success"] = "Profile updated successfully.";
            return RedirectToAction("Index");
        }
        [HttpGet]
        public IActionResult ResetPassword()
        {
            return View(new PasswordResetViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ResetPassword(PasswordResetViewModel model)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var user = _context.Users.Find(userId);
            if (user == null)
                return RedirectToAction("Login", "Account");

            bool oldPasswordValid = false;
            bool hashValid = true;
            string storedHash = user.Password;

            if (storedHash.StartsWith("$2y$"))
                storedHash = "$2a$" + storedHash.Substring(4);

            try
            {
                oldPasswordValid = BCrypt.Net.BCrypt.Verify(model.OldPassword, storedHash);
            }
            catch (BCrypt.Net.SaltParseException)
            {
                hashValid = false;
            }

            if (hashValid && !oldPasswordValid)
            {
                model.ErrorMessage = "Old password is incorrect.";
                return View(model);
            }

            if (model.NewPassword != model.ConfirmPassword)
            {
                model.ErrorMessage = "New passwords do not match.";
                return View(model);
            }

            user.Password = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
            _context.SaveChanges();

            if (!hashValid)
                model.SuccessMessage = "Password updated successfully! (Previous hash was invalid and has been reset)";
            else
                model.SuccessMessage = "Password updated successfully!";

            model.OldPassword = model.NewPassword = model.ConfirmPassword = string.Empty;

            return View(model);
        }
        public IActionResult Cancel()
        {
            return View("Index");
        }
    }

    }
