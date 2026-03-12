using System;
using System.Collections.Generic;

namespace Project5_test.Models;

public partial class Borrowing
{
    public int BorrowId { get; set; }

    public int? UserId { get; set; }

    public int? BookId { get; set; }

    public DateTime? BorrowDate { get; set; }

    public DateTime? ReturnDate { get; set; }

    public int? StatusId { get; set; }

    public virtual Book? Book { get; set; }

    public virtual Status? Status { get; set; }

    public virtual User? User { get; set; }
}
