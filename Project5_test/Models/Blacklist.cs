using System;
using System.Collections.Generic;

namespace Project5_test.Models;

public partial class Blacklist
{
    public int BlacklistId { get; set; }

    public int? UserId { get; set; }

    public string? Reason { get; set; }

    public DateTime? DateAdded { get; set; }

    public virtual User? User { get; set; }
}
