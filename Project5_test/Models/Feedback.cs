using System;
using System.Collections.Generic;

namespace Project5_test.Models;

public partial class Feedback
{
    public int FeedbackId { get; set; }

    public int? UserId { get; set; }

    public string? Message { get; set; }

    public DateTime? Date { get; set; }

    public DateOnly? DateCreated { get; set; }

    public virtual User? User { get; set; }
}
