using System;
using System.Collections.Generic;

namespace Project5_test.Models;

public partial class User
{
    public int UserId { get; set; }

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string? Phone { get; set; }

    public string? Role { get; set; }

    public string? ProfilePicture { get; set; }

    public string? AccountStatus { get; set; }

    public virtual ICollection<Blacklist> Blacklists { get; set; } = new List<Blacklist>();

    public virtual ICollection<Borrowing> Borrowings { get; set; } = new List<Borrowing>();

    public virtual ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();

    public virtual ICollection<RoomReservation> RoomReservations { get; set; } = new List<RoomReservation>();

    public virtual ICollection<Room> Rooms { get; set; } = new List<Room>();
}
