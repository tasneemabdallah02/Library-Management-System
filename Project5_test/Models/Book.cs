using System;
using System.Collections.Generic;

namespace Project5_test.Models;

public partial class Book
{
    public int BookId { get; set; }

    public string BookName { get; set; } = null!;

    public string? Author { get; set; }

    public int? CategoryId { get; set; }

    public int? StatusId { get; set; }

    public string? ImagePath { get; set; }

    public string? BooksStatus { get; set; }

    public virtual ICollection<Borrowing> Borrowings { get; set; } = new List<Borrowing>();

    public virtual Category? Category { get; set; }

    public virtual Status? Status { get; set; }
}
