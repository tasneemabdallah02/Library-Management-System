using iText.Layout.Properties.Grid;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Project5_test.Models;

namespace Project5_test.Controllers
{
    public class FeedbackController : Controller

    {
        private readonly MyDbContext _context;
        public FeedbackController(MyDbContext context) { _context = context; }

        [HttpPost]
        public IActionResult Submit(string message)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var feedback = new Feedback
            {
                UserId = userId,
                Message = message,
                Date = DateTime.Now
            };
            _context.Feedbacks.Add(feedback);
            _context.SaveChanges();
            TempData["FeedbackSuccess"] = "Your feedback has been successfully submitted!";

            return RedirectToAction("Contact", "Home");
        }
    }
      
}
