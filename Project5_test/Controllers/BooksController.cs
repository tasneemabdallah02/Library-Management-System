using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Project5_test.Models;
using System.Diagnostics;
using System.Net;


namespace Project5_test.Controllers
{
    public class BooksController : Controller
    {
        private readonly MyDbContext _context;

        public BooksController(MyDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Search(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return RedirectToAction("BooksList");
            }

            // البحث في الكتب بناءً على العنوان أو المؤلف
            var results = await _context.Books
                .Where(b => b.BookName.Contains(query) || b.Author.Contains(query))
                .ToListAsync();

            ViewData["SearchQuery"] = query;

            // يمكنك توجيه النتائج لنفس صفحة قائمة الكتب BooksList
            // أو عمل صفحة مخصصة للنتائج
            return View("BooksList", results);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]

        public async Task<IActionResult> BorrowRequest(int bookId)
        {
            //var userId = HttpContext.Session.GetInt32("UserId");

            //if (userId == null)
            //    return RedirectToAction("Login", "Account");
            //var exists = _context.Borrowings.Any(b => b.BookId == bookId && b.StatusId == 2);
            //if (exists)
            //{
            //    TempData["BorrowErrorMessage"] = "This book is already borrowed.";
            //    return RedirectToAction("BooksList");
            //}
            //Borrowing borrow = new Borrowing
            //{
            //    BookId = bookId,
            //    UserId = userId.Value,
            //    BorrowDate = DateTime.Now,
            //    StatusId = 1
            //};

            //_context.Borrowings.Add(borrow);
            //await _context.SaveChangesAsync();

            //TempData["BorrowSuccessMessage"] = "Your request has been sent to the admin successfully!";

            //return RedirectToAction("BooksList");
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
                return RedirectToAction("Login", "Account");

            // تحقق إذا المستخدم لديه طلب فعال
            var hasActiveRequest = _context.Borrowings.Any(b =>
                b.BookId == bookId &&
                b.UserId == userId &&
                (b.StatusId == 1 || b.StatusId == 2));

            if (hasActiveRequest)
            {
                TempData["BorrowErrorMessage"] = "You already requested this book.";
                return RedirectToAction("BooksList");
            }

            // تحقق إذا الكتاب مستعار حالياً
            var borrowed = _context.Borrowings
                .Any(b => b.BookId == bookId && b.StatusId == 2);

            if (borrowed)
            {
                TempData["BorrowErrorMessage"] = "This book is currently borrowed.";
                return RedirectToAction("BooksList");
            }

            Borrowing borrow = new Borrowing
            {
                BookId = bookId,
                UserId = userId.Value,
                BorrowDate = DateTime.Now,
                StatusId = 1
            };

            _context.Borrowings.Add(borrow);
            await _context.SaveChangesAsync();

            TempData["BorrowSuccessMessage"] = "Your request was sent to admin.";

            return RedirectToAction("BooksList");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int bookId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            var borrow = _context.Borrowings
                .FirstOrDefault(x => x.BookId == bookId && x.UserId == userId);

            if (borrow != null)
            {
                borrow.StatusId = 5; 

                _context.Borrowings.Remove(borrow);
                await _context.SaveChangesAsync();
                TempData["BorrowSuccessMessage"] = "Reservation cancelled";
            }

            return RedirectToAction("BooksList");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ConfirmRoom(int reservationId)
        {
            var record = _context.RoomReservations
                .Include(r => r.Room)
                .FirstOrDefault(r => r.ReservationId == reservationId);

            if (record == null) return NotFound();

            record.StatusId = 2; // Approved
            if (record.Room != null) record.Room.StatusId = 2;

            _context.SaveChanges();
            return RedirectToAction("ManageReservations");
        }
        [HttpPost]
        public async Task<IActionResult> RejectBorrow(int borrowId)
        {
            var record = await _context.Borrowings
                .Include(r => r.Book)
                .FirstOrDefaultAsync(r => r.BorrowId == borrowId);

            if (record == null) return NotFound();

            record.StatusId = 3;

            if (record.Book != null)
            {
                record.Book.StatusId = 1;
            }

            await _context.SaveChangesAsync();

            TempData["RejectSuccess"] = "Borrow request rejected.";
            return RedirectToAction("BorrowRequests");
        }


        public async Task <IActionResult> BooksList()
        {
         
            var books = await _context.Books.Where(b => b.BooksStatus == "Active")
    .Include(b => b.Borrowings)
    .ToListAsync();
            ViewBag.AvailableRooms = _context.Rooms
    .Where(r => r.Roomstatus == "Active" && r.StatusId==1) 
    .ToList();

            return View(books);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ReserveRoom(int roomId, DateTime reservationDate)
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            RoomReservation reservation = new RoomReservation
            {
                RoomId = roomId,
                UserId = userId.Value,
                ReservationDate = reservationDate,
                StatusId = 1 
            };

            _context.RoomReservations.Add(reservation);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "your Requset sent to asdmin.";

            return RedirectToAction("BooksList", "Books");
        }
        //  Test action////////////////////////
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RoomRequest(int roomId, DateTime reservationDate)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var exists = _context.RoomReservations
                .FirstOrDefault(r =>
                    r.RoomId == roomId &&
                    r.ReservationDate == reservationDate &&
                    r.StatusId == 2); 

            if (exists != null)
            {
                TempData["RoomRequestErrorMessage"] = "This time slot is already booked.";
                return RedirectToAction("BooksList");
            }

            var reservation = new RoomReservation
            {
                RoomId = roomId,
                UserId = userId.Value,
                StatusId = 1, // 1 = Pending
                ReservationDate = reservationDate
            };

            _context.RoomReservations.Add(reservation);
            _context.SaveChanges();

            TempData["RoomRequestSuccessMessage"] = "Your request has been sent to admin.";
            return RedirectToAction("BooksList");
        }
        //الموافقه
        [HttpPost]
        public IActionResult ApproveRoom(int reservationId)
        {
            var record = _context.RoomReservations
                .Include(r => r.Room)
                .FirstOrDefault(r => r.ReservationId == reservationId);

            if (record == null)
                return NotFound();

            var conflict = _context.RoomReservations.Any(r =>
                r.RoomId == record.RoomId &&
                r.ReservationDate == record.ReservationDate &&
                r.StatusId == 2);

            if (conflict)
            {
                TempData["ApproveRoomErrorMessage"] = "Time already confirmed for another user.";
                return RedirectToAction("ManageReservations", "Admin");
            }

            record.StatusId = 2; // 2 = Confirmed

            _context.SaveChanges();

            return RedirectToAction("ManageReservations", "Admin");
        }
        //الرفض

        [HttpPost]
        public IActionResult RejectRoom(int reservationId)
        {
            var record = _context.RoomReservations
                .FirstOrDefault(r => r.ReservationId == reservationId);

            if (record == null)
                return NotFound();

            record.StatusId = 3; // 3 = Rejected

            _context.SaveChanges();

            return RedirectToAction("ManageReservations", "Admin");
        }
        [HttpPost]
        public IActionResult CompleteRoom(int reservationId)
        {
            var record = _context.RoomReservations
                .Include(r => r.Room)
                .FirstOrDefault(r => r.ReservationId == reservationId);

            if (record == null)
                return NotFound();

            record.StatusId = 5; // Completed

            if (record.Room != null)
                record.Room.StatusId = 1; // Room available again

            _context.SaveChanges();

            return RedirectToAction("ManageReservations", "Admin");
        }
    }
}
