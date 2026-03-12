using System;
using System.Collections.Generic;

namespace Project5_test.Models;

public partial class Room
{
    public int RoomId { get; set; }

    public string RoomName { get; set; } = null!;

    public int Capacity { get; set; }

    public int? StatusId { get; set; }

    public string? Roomstatus { get; set; }

    public DateOnly? RoomsDate { get; set; }

    public DateOnly? RoomsTime { get; set; }

    public int? UserId { get; set; }

    public virtual ICollection<RoomReservation> RoomReservations { get; set; } = new List<RoomReservation>();

    public virtual Status? Status { get; set; }

    public virtual User? User { get; set; }
}
