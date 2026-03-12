using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Project5_test.Models;
using static System.Collections.Specialized.BitVector32;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Kernel.Font;
using iText.IO.Font.Constants;
using PdfTable = iText.Layout.Element.Table;
public class AdminController : Controller
{
    private readonly MyDbContext _context;

    public AdminController(MyDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    public IActionResult Login(string email, string password)
    {
        var admin = _context.Users
            .FirstOrDefault(u => u.Email == email && u.Password == password && u.Role == "Admin");

        if (admin != null)
        {
            HttpContext.Session.SetString("Role", "Admin");
            HttpContext.Session.SetInt32("UserId", admin.UserId);

            return RedirectToAction("Dashboard");
        }
        else
        {
            ViewBag.Error = "Invalid email or password!";
            return View();
        }
    }
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Login");
    }

    public IActionResult Dashboard()
    {
        if (HttpContext.Session.GetString("Role") != "Admin")
            return RedirectToAction("Login", "Admin");

        ViewBag.UsersCount = _context.Users.Count();
        ViewBag.BooksCount = _context.Books.Count();
        ViewBag.PendingBorrows = _context.Borrowings.Count(b => b.StatusId == 1);
        ViewBag.PendingReservation = _context.RoomReservations
                                     .Count(r => r.StatusId == 1); // أو حسب حالتك
        ViewBag.feedback = _context.Feedbacks.Count();

        return View();
    }
    public IActionResult UsersList()
    {
        var users = _context.Users.ToList();
        return View(users);
    }

    public IActionResult UserDetails(int id)
    {
        var user = _context.Users
                .Include(u => u.Borrowings)            // جلب قائمة الاستعارات
                    .ThenInclude(b => b.Book)         // من داخل كل استعارة، اجلب بيانات الكتاب المرتبط
                .Include(u => u.Borrowings)
                    .ThenInclude(b => b.Status)       // من داخل كل استعارة، اجلب بيانات الحالة (مثل: مستعار، تم الإرجاع)
                .FirstOrDefault(u => u.UserId == id);

        // التحقق من وجود المستخدم قبل إرساله للعرض
        if (user == null)
        {
            return NotFound();
        }

        return View(user);
    }

    [HttpPost]
    public IActionResult DeleteUser(int id)
    {
        var user = _context.Users.Find(id);
        if (user != null)
        {
            user.AccountStatus = "Block";
            //_context.Users.Remove(user);
            _context.SaveChanges();
        }
        return RedirectToAction("UsersList");
    }



    [HttpPost]
    [ValidateAntiForgeryToken]

    public IActionResult DeleteBook(int id)
    {
        var book = _context.Books.Find(id);
        if (book == null)
        {
            TempData["BookErrorMessage"] = "Book not found!";
            return RedirectToAction("BooksList");
        }

        bool hasBorrowings = _context.Borrowings.Any(b => b.StatusId == 2);
        if (hasBorrowings)
        {
            TempData["BookErrorMessage"] = "Cannot delete this book because it has borrowings.";
            return RedirectToAction("BooksList");
        }

        book.BooksStatus = "inactive";
        _context.SaveChanges();

        TempData["BookSuccessMessage"] = "Book deleted successfully!";
        return RedirectToAction("BooksList");
    }
    public IActionResult RoomsList()
    {
        var reservations = _context.RoomReservations.Where(r => r.StatusId !=6)
            .Include(r => r.Room)
            .Include(r => r.User)
            .Where(r => r.StatusId == 2) 
            .ToList();

        return View(reservations);
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteRoom(int id)
    {
        var room = _context.Rooms.Find(id);
        var res = _context.RoomReservations.Find(id);

        if (room == null)
        {
            TempData["RoomError"] = "Room not found!";
            return RedirectToAction("RoomsList");
        }

        bool hasReservations = _context.RoomReservations
            .Any(r => r.RoomId == id && r.StatusId == 2);

        if (hasReservations)
        {
            TempData["RoomError"] = "Cannot delete this room because it has active reservations.";
            return RedirectToAction("RoomsList");
        }

        room.Roomstatus = "Inactive";
        res.StatusId = 6;
        _context.SaveChanges();

        TempData["RoomSuccess"] = "Room deleted successfully!";
        return RedirectToAction("RoomsList");
    }
    public IActionResult BooksList()
    {
        var books = _context.Books.Where(r => r.BooksStatus != "inactive").ToList();
        return View(books);
    }
    [HttpGet]
    public IActionResult EditRoom(int id)
    {
        var room = _context.Rooms.Find(id);
        return View(room);
    }

    [HttpPost]
    public IActionResult EditRoom(Room room)
    {
        if (ModelState.IsValid)
        {
            // 1. نتأكد إن الغرفة موجودة فعلاً في قاعدة البيانات
            var roomInDb = _context.Rooms.Find(room.RoomId);

            if (roomInDb == null)
            {
                return NotFound(); // إذا الـ ID غلط أو مش موجود
            }

            // 2. تحديث البيانات (انقل القيم اللي بدك تعدلها بس)
            roomInDb.RoomName = room.RoomName;
            roomInDb.Capacity = room.Capacity;
            roomInDb.RoomsTime = room.RoomsTime;

            // roomInDb.AnyOtherProperty = room.AnyOtherProperty;

            // 3. حفظ التغييرات
            _context.SaveChanges();

            return RedirectToAction("RoomsList");
        }

        // إذا فيه مشكلة في المدخلات (Validation Error) بنرجع لنفس الصفحة
        return View(room);
        //if (ModelState.IsValid)
        //{
        //    _context.Rooms.Update(room);
        //    _context.SaveChanges();
        //    return RedirectToAction("RoomsList");
        //}
        //return View(room);
    }
    [HttpGet]
    public IActionResult EditBook(int id)
    {
        var book = _context.Books.Find(id);
        return View(book);
    }

    [HttpPost]
    public IActionResult EditBook(Book book)
    {
        if (ModelState.IsValid)
        {
            _context.Books.Update(book);
            _context.SaveChanges();
            return RedirectToAction("BooksList");
        }
        return View(book);
    }
    [HttpGet]
    public IActionResult AddBook() => View();

    [HttpPost]
    public IActionResult AddBook(Book book, IFormFile imageFile)
    {
        if (ModelState.IsValid)
        {
            if (imageFile != null)
            {
                string folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");
                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                string filePath = Path.Combine(folder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    imageFile.CopyTo(stream);
                }

                book.ImagePath = "/images/" + fileName;
            }

            _context.Books.Add(book);
            _context.SaveChanges();
            return RedirectToAction("Dashboard");
        }

        return View(book);
    }
    [HttpGet]
    public IActionResult AddRoom() => View();

    [HttpPost]
    public IActionResult AddRoom(Room room)
    {
        if (ModelState.IsValid)
        {

            _context.Rooms.Add(room);
            _context.SaveChanges();
            return RedirectToAction("Dashboard");
        }

        return View(room);
    }

    public IActionResult ManageBorrows()
    {
        var borrows = _context.Borrowings.Where(b=>b.StatusId !=5)
            .Include(b => b.Book)
            .Include(b => b.User)
            .Include(b => b.Status)
            .ToList();
        return View(borrows);
    }

    public IActionResult ManageReservations()
    {
        var requests = _context.RoomReservations
            .Include(r => r.User)
            .Include(r => r.Room)
            .Where(r => r.StatusId == 1) // Pending
            .ToList();
        return View(requests);
    }

    //[HttpPost]
    //[ValidateAntiForgeryToken]
    //public IActionResult RoomRequest(int roomId)
    //{
    //    var userId = HttpContext.Session.GetInt32("UserId");
    //    if (userId == null)
    //        return RedirectToAction("Login", "Account");

    //    var room = _context.Rooms.Find(roomId);
    //    if (room == null || room.StatusId != 1)
    //    {
    //        TempData["RoomErrorMessage"] = "This room is not available.";
    //        return RedirectToAction("RoomsList");
    //    }

    //    var reservation = new RoomReservation
    //    {
    //        RoomId = null,
    //        UserId = userId.Value,
    //        StatusId = 1, 
    //        ReservationDate = DateTime.Now
    //    };

    //    room.StatusId = 2; 

    //    _context.RoomReservations.Add(reservation);
    //    _context.SaveChanges();

    //    TempData["RoomSuccessMessage"] = "Your room request has been sent to the admin successfully!";
    //    return RedirectToAction("RoomsList");
    //}
    [HttpPost]
    public IActionResult ConfirmRoom(int reservationId, DateTime? selectedDateTime)
    {
                   var record = _context.RoomReservations
                .Include(r => r.Room)
                .FirstOrDefault(r => r.ReservationId == reservationId);

            if (record == null)
                return NotFound();

            record.StatusId = 2; // Approved

            if (selectedDateTime.HasValue)
            {
                record.ReservationDate = selectedDateTime.Value;
            }

            if (record.Room != null)
            {
                record.Room.StatusId = 2; 
            }

            _context.SaveChanges();

            return RedirectToAction("ManageReservations", "Admin");
        
    }
    //[HttpPost]
    //public IActionResult RejectRoom(int reservationId)
    //{
    //    var record = _context.RoomReservations
    //        .Include(r => r.Room)
    //        .FirstOrDefault(r => r.ReservationId == reservationId);

    //    if (record == null)
    //        return NotFound();

    //    record.StatusId = 4; 

    //    if (record.Room != null)
    //        record.Room.StatusId = 1; 

    //    _context.SaveChanges();

    //    return RedirectToAction("ManageReservations", "Admin");
    //}
    [HttpPost]
    public IActionResult ConfirmBorrow(int borrowId)
    {
        var record = _context.Borrowings
            .Include(r => r.Book)
            .Include(r => r.Status)
            .FirstOrDefault(r => r.BorrowId == borrowId);

        if (record == null)
            return NotFound();

        record.StatusId = 2;
        record.BorrowDate = DateTime.Now;

        if (record.Book != null)
        {
            record.Book.StatusId = 2;
        }

        _context.SaveChanges();

        TempData["Success"] = "Borrow request approved successfully.";
        return RedirectToAction("ManageBorrows");
    }
    [HttpPost]
    public IActionResult ReturnBorrow(int borrowId)
    {
        var record = _context.Borrowings
            .Include(r => r.Book)
            .FirstOrDefault(r => r.BorrowId == borrowId);

        if (record == null)
            return NotFound();

        record.StatusId = 4; 
        record.ReturnDate = DateTime.Now;

        if (record.Book != null)
            record.Book.StatusId = 1; 

        _context.SaveChanges();

        return RedirectToAction("ManageBorrows");
    }
    [HttpPost]
    public async Task<IActionResult> ApproveBorrow(int borrowId)
    {
        var record = await _context.Borrowings
            .Include(r => r.Book)
            .Include(r => r.Status)
            .FirstOrDefaultAsync(r => r.BorrowId == borrowId);

        if (record == null) return NotFound();

        record.StatusId = 2;
        record.BorrowDate = DateTime.Now;
        record.ReturnDate = null;

        if (record.Book != null)
        {
            record.Book.StatusId = 2;
        }

        await _context.SaveChangesAsync();

        TempData["Success"] = "Borrow request approved successfully.";
        return RedirectToAction("BorrowRequests");
    }
    [HttpPost]
    public IActionResult RejectBorrow(int borrowId)
    {
        var record = _context.Borrowings
            .Include(r => r.Book)
            .FirstOrDefault(r => r.BorrowId == borrowId);

        if (record == null)
            return NotFound();

        record.StatusId = 3; 

        if (record.Book != null)
            record.Book.StatusId = 1;

        _context.SaveChanges();

        return RedirectToAction("ManageBorrows");
    }
    //[HttpGet] 
    //public async Task<IActionResult> Reserve(int id)
    //{
    //    var reservations = await _context.RoomReservations
    //                           .Where(r => r.RoomId == id)
    //                           .ToListAsync();

    //    if (reservations == null)
    //    {
    //        reservations = new List<RoomReservation>();
    //    }

    //    ViewBag.RoomId = id;

    //    return View(reservations);
    //}
    public IActionResult ManageFeedback()
    {
        var feedbacks = _context.Feedbacks
                             .Include(f => f.User)
                             .OrderByDescending(f => f.Date)
                             .ToList();
        return View(feedbacks);

    }
    [HttpPost]
    public IActionResult Cancel(int roomId)
    {
        var room = _context.Rooms.Find(roomId);

        if (room == null)
            return RedirectToAction("RoomsList");

        room.StatusId = 1; // 1 = Available
        _context.SaveChanges();


        return RedirectToAction("RoomsList", "Books");
    }
    //user list pdf
    [HttpGet]
    public IActionResult ExportUsersPdf()
    {
        var users = _context.Users.ToList();

        using (MemoryStream stream = new MemoryStream())
        {
            PdfWriter writer = new PdfWriter(stream);
            PdfDocument pdf = new PdfDocument(writer);
            Document document = new Document(pdf);

            PdfFont font = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
            PdfFont boldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);

            // Title
            document.Add(new Paragraph("Users Report")
                .SetFont(boldFont)
                .SetFontSize(18));

            document.Add(new Paragraph($"Generated on: {DateTime.Now:yyyy-MM-dd}")
                .SetFont(font)
                .SetFontSize(12));

            document.Add(new Paragraph("\n"));

            // Table
            PdfTable table = new PdfTable(5).UseAllAvailableWidth(); table.AddHeaderCell("ID");
            table.AddHeaderCell("First Name");
            table.AddHeaderCell("Last Name");
            table.AddHeaderCell("Email");
            table.AddHeaderCell("Phone");

            foreach (var user in users)
            {
                table.AddCell(user.UserId.ToString());
                table.AddCell(user.FirstName ?? "");
                table.AddCell(user.LastName ?? "");
                table.AddCell(user.Email ?? "");
                table.AddCell(user.Phone ?? "");
            }

            document.Add(table);
            document.Close();

            return File(stream.ToArray(),
                "application/pdf",
                "Users_Report.pdf");
        }
    }
    //Book list pdf
    [HttpGet]
    public IActionResult ExportBooksPdf()
    {
        var books = _context.Books.ToList();

        using (MemoryStream stream = new MemoryStream())
        {
            PdfWriter writer = new PdfWriter(stream);
            PdfDocument pdf = new PdfDocument(writer);
            Document document = new Document(pdf);

            PdfFont font = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
            PdfFont boldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);

            // Title
            document.Add(new Paragraph("Books Report")
                .SetFont(boldFont)
                .SetFontSize(18));

            document.Add(new Paragraph($"Generated on: {DateTime.Now:yyyy-MM-dd}")
                .SetFont(font)
                .SetFontSize(12));

            document.Add(new Paragraph("\n"));

            // Table
            PdfTable table = new PdfTable(5).UseAllAvailableWidth();

            table.AddHeaderCell("ID");
            table.AddHeaderCell("Book Name");
            table.AddHeaderCell("Author");
            table.AddHeaderCell("Category");
            table.AddHeaderCell("Status");

            foreach (var book in books)
            {
                table.AddCell(book.BookId.ToString());
                table.AddCell(book.BookName ?? "");
                table.AddCell(book.Author ?? "");
                table.AddCell(book.Category?.ToString() ?? "");
                table.AddCell(book.StatusId?.ToString() ?? "");
            }

            document.Add(table);
            document.Close();

            return File(stream.ToArray(),
                "application/pdf",
                "Books_Report.pdf");
        }
    }
    //Room list pdf 
    [HttpGet]
    public IActionResult ExportRoomsPdf()
    {
        var reservations = _context.RoomReservations
            .Include(r => r.Room)
            .Include(r => r.User)
            .Where(r => r.StatusId == 2)
            .ToList();

        using (MemoryStream stream = new MemoryStream())
        {
            PdfWriter writer = new PdfWriter(stream);
            PdfDocument pdf = new PdfDocument(writer);
            Document document = new Document(pdf);

            PdfFont font = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
            PdfFont boldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);

            document.Add(new Paragraph("Rooms Report")
                .SetFont(boldFont)
                .SetFontSize(18));

            document.Add(new Paragraph($"Generated on: {DateTime.Now:yyyy-MM-dd}")
                .SetFont(font)
                .SetFontSize(12));

            document.Add(new Paragraph("\n"));

            PdfTable table = new PdfTable(5).UseAllAvailableWidth();

            table.AddHeaderCell("Reservation ID");
            table.AddHeaderCell("Room Name");
            table.AddHeaderCell("User");
            table.AddHeaderCell("Reservation Date");
            table.AddHeaderCell("Status");

            foreach (var r in reservations)
            {
                table.AddCell(r.ReservationId.ToString());
                table.AddCell(r.Room != null ? r.Room.RoomName : "No Room");
                table.AddCell(r.User != null ? r.User.FirstName + " " + r.User.LastName : "No User");
                table.AddCell(r.ReservationDate?.ToString("yyyy-MM-dd") ?? "N/A");

                string statusText = r.StatusId switch
                {
                    1 => "Pending",
                    2 => "Aprroved",
                    3 => "Rejected",
                    _ => "Unknown"
                };
                table.AddCell(statusText);
            }

            document.Add(table);
            document.Close();

            return File(stream.ToArray(),
                "application/pdf",
                "Rooms_Report.pdf");
        }
    }
}