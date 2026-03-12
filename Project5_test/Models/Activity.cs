using System;
using System.Collections.Generic;

namespace Project5_test.Models;

public partial class Activity
{
    public int ActivityId { get; set; }

    public string? UserName { get; set; }

    public string? Action { get; set; }

    public string? Icon { get; set; }

    public string? Color { get; set; }

    public DateTime? CreatedAt { get; set; }
}
