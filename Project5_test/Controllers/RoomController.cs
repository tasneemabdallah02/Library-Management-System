using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Project5_test.Models;
using System.Diagnostics;


namespace Project5_test.Controllers
{
    public class RoomsController : Controller
    {
        private readonly MyDbContext _context;

        public RoomsController(MyDbContext context)
        {
            _context = context;
        }

        // 1. عرض جميع الغرف المتاحة للطلاب
        public IActionResult Index()
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
                return RedirectToAction("Login", "Account");

            var rooms = _context.Rooms.Include(r => r.Status).ToList();
            return View(rooms);
        }
        public IActionResult Rooms()
        {
            var rooms = _context.Rooms.ToList(); // جلب كل الغرف
            return View(rooms);
        }
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public IActionResult RoomRequest(int roomId)
        //{
        //    // جلب رقم المستخدم من الجلسة (Session)
        //    var userId = HttpContext.Session.GetInt32("UserId");

        //    if (userId == null)
        //    {
        //        // إذا لم يكن مسجلاً للدخول، يتم توجيهه لصفحة الدخول
        //        return RedirectToAction("Login", "Account");
        //    }

        //    // إنشاء كائن الحجز الجديد
        //    RoomReservation reservation = new RoomReservation
        //    {
        //        RoomId = roomId,
        //        UserId = userId.Value,
        //        // بما أن الحقل في الـ Model هو DateTime?، نقوم بإسناد القيمة مباشرة 
        //        // لضمان حفظ التاريخ والوقت كما اختارهما المستخدم
        //        ReservationDate = reservationDate,
        //        StatusId = 1 // حالة الطلب: قيد الانتظار (Pending)
        //    };

        //    // إضافة السجل لقاعدة البيانات
        //    _context.RoomReservations.Add(reservation);
        //    _context.SaveChanges();

        //    TempData["SuccessMessage"] = "تم إرسال طلب حجز الغرفة بنجاح! يرجى انتظار موافقة الإدارة.";

        //    // التوجه لصفحة قائمة الكتب أو صفحة تأكيد الحجز
        //    return RedirectToAction("BooksList", "Books");
        //}
        [HttpGet]
        public async Task<IActionResult> Reserve(int roomId)
        {
            ViewBag.RoomId = roomId;

            var reservations = await _context.RoomReservations
                .Where(r => r.RoomId == roomId)
                .ToListAsync();

            return View(reservations);
        }

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Reserve(int roomId, DateOnly reservationDate)
        //{
        //    // 1. جلب رقم المستخدم من السيشن (نفس الطريقة اللي شغالة معاك في باقي الكنترولر)
        //    var userId = HttpContext.Session.GetInt32("UserId");

        //    // 2. إذا السيشن فاضي، رجعه للوجن
        //    if (userId == null)
        //    {
        //        return RedirectToAction("Login", "Account");
        //    }

        //    // 3. التحقق من أن التاريخ ليس في الماضي
        //    if (reservationDate < DateOnly.FromDateTime(DateTime.Today))
        //    {
        //        TempData["SuccessMessage"] = "You cannot reserve a past date!";
        //        return RedirectToAction("Reserve", new { roomId = roomId });
        //    }

        //    // 4. إنشاء الحجز
        //    var reservation = new RoomReservation
        //    {
        //        RoomId = roomId,
        //        UserId = userId.Value, // استخدمنا .Value عشان نحول من int? إلى int
        //        ReservationDate = reservationDate,
        //        StatusId = 1
        //    };

        //    _context.RoomReservations.Add(reservation);
        //    await _context.SaveChangesAsync();

        //    TempData["SuccessMessage"] = "Your request has been sent to the admin successfully!";

        //    // 5. إرجاع المستخدم لنفس الصفحة مع بقاء رقم الغرفة
        //    return RedirectToAction("Reserve", new { roomId = roomId });
        //}



        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Reserve(int roomId, DateOnly reservationDate)
        //{
        //    var userId = HttpContext.Session.GetInt32("UserId");

        //    if (userId == null)
        //        return RedirectToAction("Login", "Account");
        //    if (reservationDate < DateOnly.FromDateTime(DateTime.Today))
        //    {
        //        TempData["SuccessMessage"] = "You cannot reserve a past date!";
        //        return RedirectToAction("Reserve", new { id = roomId });
        //    }

        //    var reservation = new RoomReservation
        //    {

        //        RoomId = roomId,
        //        UserId = userId.Value,
        //        ReservationDate = reservationDate,

        //        // الأدمن سيقوم بتغيير هذا الوقت عندما يوافق على الطلب
        //        StartTime = TimeOnly.MinValue,
        //        EndTime = TimeOnly.MinValue,

        //        StatusId = 1 // 1 = Pending (طلب معلق)
        //    };

        //    _context.RoomReservations.Add(reservation);
        //    await _context.SaveChangesAsync();

        //    // نجهز الرسالة لتظهر كـ Alert
        //    TempData["SuccessMessage"] = "Your request has been sent to the admin successfully!";

        //    // التعديل الثاني: التوجيه لصفحة الغرف (Index) في كنترولر (Room) بدلاً من الكتب
        //    return RedirectToAction("Index", "Room");
        //}
        public JsonResult GetApprovedReservations()
        {
            var reservations = _context.RoomReservations
                .Include(r => r.Room)
                .Where(r => r.StatusId == 2) // Approved
                .ToList() // جلب البيانات أولاً إلى الذاكرة
                .Select(r => new
                {
                    reservationDate = r.ReservationDate?.ToString("yyyy-MM-dd"),
                    startTime = r.StartTime.HasValue
                        ? r.StartTime.Value.ToString("HH:mm")
                        : "",  // لو فارغ
                    roomName = r.Room.RoomName,
                    roomId = r.RoomId
                })
                .ToList();

            return Json(reservations);
        }

        // 3. عرض حجوزات الطالب الحالية والسابقة (View current and past activities)
        public IActionResult MyReservations()
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdStr)) return RedirectToAction("Login", "Account");

            int userId = int.Parse(userIdStr);

            var myRes = _context.RoomReservations
                .Include(r => r.Room)
                .Include(r => r.Status)
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.ReservationDate)
                .ToList();

            return View(myRes);
        }

        [HttpPost]
        public IActionResult Cancel(int id)
        {
            var Borrow = _context.Borrowings.Find(id);

            if (Borrow != null && Borrow.StatusId == 1)
            {
                _context.Borrowings.Remove(Borrow);
                Borrow.StatusId = 5; 
                _context.SaveChanges();
                TempData["CancelSuccess"] = "Reservation cancelled successfully.";
            }
            else
            {
                TempData["CancelError"] = "Cannot cancel an approved or rejected reservation.";
            }

            return RedirectToAction("BooksList","Books");
        }
        public IActionResult Calendar(int roomId)
        {
            var reservations = _context.RoomReservations
                .Where(r => r.RoomId == roomId)
                .Include(r => r.User)
                .ToList();

            return View(reservations);
        }

        [HttpPost]
        public IActionResult Book([FromBody] RoomReservation reservation)
        {
            // منع التداخل الزمني
            var conflict = _context.RoomReservations.Any(r =>
                r.RoomId == reservation.RoomId &&
                r.ReservationDate == reservation.ReservationDate &&
                (
                    (reservation.StartTime >= r.StartTime && reservation.StartTime < r.EndTime) ||
                    (reservation.EndTime > r.StartTime && reservation.EndTime <= r.EndTime)
                )
            );

            if (conflict)
                return Json(new { success = false, message = "Time slot already booked!" });

            reservation.StatusId = 1; // Approved

            _context.RoomReservations.Add(reservation);
            _context.SaveChanges();

            return Json(new { success = true });
        }

    }
}