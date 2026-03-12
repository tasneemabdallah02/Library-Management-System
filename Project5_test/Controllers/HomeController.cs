using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Project5_test.Models;
using System.Diagnostics;

namespace Project5_test.Controllers
{
    public class HomeController : Controller

    {
        private readonly MyDbContext _context;

        public HomeController(MyDbContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            return View();
        }

        //public IActionResult test()
        //{
        //    return View();
        //}

        public IActionResult ReserveRoom()
        {
            return View();
        }
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public IActionResult ReserveRoom(int roomId, DateTime reservationDate)
        //{
        //    // ??? ??? ???????? ?? ?????? (Session)
        //    var userId = HttpContext.Session.GetInt32("UserId");

        //    if (userId == null)
        //    {
        //        // ??? ?? ??? ?????? ??????? ??? ?????? ????? ??????
        //        return RedirectToAction("Login", "Account");
        //    }

        //    // ????? ???? ????? ??????
        //    RoomReservation reservation = new RoomReservation
        //    {
        //        RoomId = roomId,
        //        UserId = userId.Value,
        //        // ????? DateTime ?????? ?? ???????? ??? DateOnly ?????? ????? ????????
        //        ReservationDate = DateOnly.FromDateTime(reservationDate),
        //        StatusId = 1 // ???? ?????: ??? ???????? (Pending)
        //    };

        //    // ????? ????? ?????? ????????
        //    _context.RoomReservations.Add(reservation);
        //    _context.SaveChanges();

        //    TempData["SuccessMessage"] = "?? ????? ??? ??? ?????? ?????! ???? ?????? ?????? ???????.";

        //    // ?????? ????? ????? ????? ?? ???? ????? ?????
        //    return RedirectToAction("BooksList", "Books");
        //}
        public IActionResult Categories()
        {
            return View();
        }
        public IActionResult About()
        {
            var feedbacks = _context.Feedbacks
                         .Include(f => f.User)
                                    .OrderByDescending(f => f.DateCreated)
                                    .ToList();

            return View(feedbacks);
        }
        public IActionResult Contact()
        {
            return View();
        }
        public IActionResult message()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Borrow(int id)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Account");

            // ?????? ?? Blacklist
            bool isBlacklisted = _context.Blacklists.Any(b => b.UserId == userId);
            if (isBlacklisted)
            {
                TempData["Error"] = "You are blacklisted. Please contact admin.";
                return RedirectToAction("Index");
            }

            // ?????? ?? ???? ??????
            var book = _context.Books.FirstOrDefault(b => b.BookId == id);
            if (book == null)
            {
                TempData["Error"] = "Book not found.";
                return RedirectToAction("Index");
            }

            // ?????? ??? ??? ?????? ??? ????
            if (book.StatusId != 1) // 1 = Available
            {
                TempData["Error"] = "Book is not available.";
                return RedirectToAction("Index");
            }

            // ?????? ??? ??? ???????? ??? ?????? ?????? ??? ??? ??? ?????
            bool alreadyRequested = _context.Borrowings.Any(b =>
                b.BookId == id &&
                b.UserId == userId &&
                (b.StatusId == 1 || b.StatusId == 2)); // Pending ?? Approved

            if (alreadyRequested)
            {
                TempData["Error"] = "You already requested this book.";
                return RedirectToAction("Index");
            }

            Borrowing borrowingRequest = new Borrowing
            {
                BookId = id,
                UserId = userId,
                BorrowDate = DateTime.Now,
                StatusId = 2 // Pending
            };

            _context.Borrowings.Add(borrowingRequest);
            _context.SaveChanges();

            TempData["Success"] = "Your request has been sent to admin.";
            return RedirectToAction("Bookcat");
        }

        //       public async Task<IActionResult> BorrowRequests()
        //{
        //    var requests = await _dbContext.BorrowRecords
        //        .Include(r=>r.Book)
        //        .Include(r=>r.User)
        //        .OrderByDescending(r=>r.BorrowDate)
        //        .ToListAsync();
        //    return View(requests);
        //}

        //[HttpPost]
        //public async Task<IActionResult> ApproveBorrow(int borrowId)
        //{
        //    var record=await _dbContext.BorrowRecords
        //        .Include(r=>r.Book)
        //        .FirstOrDefaultAsync(r=>r.BorrowId == borrowId);

        //    if (record == null)
        //        return NotFound();


        //    record.Book.BookStatus= "Borrowed";
        //    record.DueDate = DateOnly.FromDateTime(DateTime.Now.AddDays(14));

        //    await _dbContext.SaveChangesAsync();

        //    return RedirectToAction("BorrowRequests");
        //}
        //[HttpPost]
        //public async Task<IActionResult> RejectBorrow(int borrowId)
        //{
        //    var record = await _dbContext.BorrowRecords
        //         .Include(r => r.Book)
        //        .FirstOrDefaultAsync(r => r.BorrowId == borrowId);

        //    if (record == null)
        //        return NotFound();


        //    record.Book.BookStatus = "Available";
        //    record.ReturnDate = DateOnly.FromDateTime(DateTime.Now);

        //    await _dbContext.SaveChangesAsync();

        //    return RedirectToAction("BorrowRequests");
        //}
        //[HttpPost]
        //public async Task<IActionResult> ReturnBorrow(int borrowId)
        //{
        //    var record = await _dbContext.BorrowRecords
        //        .Include(r => r.Book)
        //        .FirstOrDefaultAsync(r => r.BorrowId == borrowId);

        //    if (record == null)
        //        return NotFound();

        //    record.ReturnDate= DateOnly.FromDateTime(DateTime.Now);
        //    record.Book.BookStatus = "Available";
        //    await _dbContext.SaveChangesAsync();

        //    return RedirectToAction("BorrowRequests");
        //}
    }

}