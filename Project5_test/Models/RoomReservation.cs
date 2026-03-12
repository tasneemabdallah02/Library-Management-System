using System;
using System.Collections.Generic;

namespace Project5_test.Models;

public partial class RoomReservation
{
    public int ReservationId { get; set; }

    public int? UserId { get; set; }

    public int? RoomId { get; set; }

    public DateTime? ReservationDate { get; set; }

    public TimeOnly? StartTime { get; set; }

    public TimeOnly? EndTime { get; set; }

    public int? StatusId { get; set; }

    public string? RoomReservation1 { get; set; }

    public virtual Room? Room { get; set; }

    public virtual Status? Status { get; set; }

    public virtual User? User { get; set; }
}
