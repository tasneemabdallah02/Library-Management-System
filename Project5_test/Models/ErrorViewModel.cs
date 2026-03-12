namespace Project5_test.Models
{
    public class ErrorViewModel
    {
        public string? RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
    public class DashboardViewModel
    {
        // ??????? ???????? ??????
        public int UsersCount { get; set; }
        public int BooksCount { get; set; }
        public int PendingBorrows { get; set; }
        public int PendingReservation { get; set; }
        public int Feedback { get; set; }
        //public List<Notification> Notifications { get; set; }
        //public List<Activity> Activities { get; set; }
        public string CurrentTheme { get; set; }


        public string ActiveSection { get; set; } = "dashboard"; // ???? ????????

        public List<int> RevenueData { get; set; }
    }
    public class AddEventViewModel
    {
        //public List<Room> Rooms { get; set; } = new List<Room>();
        public DateOnly Date { get; set; }
        public TimeOnly StartTime { get; set; }
        public int SelectedRoomId { get; set; }
    }
    public class PasswordResetViewModel
    {
        public string OldPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;

        public string? ErrorMessage { get; set; }
        public string? SuccessMessage { get; set; }
    }

    }
