using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using DataModelLibrary.Models;

namespace LibraryApi.Data;

public partial class LibraryContext : DbContext
{
    public LibraryContext()
    {
    }

    public LibraryContext(DbContextOptions<LibraryContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Book> Books { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<OrderedBook> OrderedBooks { get; set; }

    public virtual DbSet<CoverImage> CoverImages { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Book>(entity =>
        {
            entity.HasIndex(e => e.Id, "IX_Books_Id").IsUnique();

            entity.HasIndex(e => e.Title, "IX_Books_Title").IsUnique();
        });

        modelBuilder.Entity<OrderedBook>(entity =>
        {
            entity.HasKey(e => e.BookId);

            entity.ToTable("Ordered books");

            entity.Property(e => e.BookId).ValueGeneratedNever();

            entity.HasOne(d => d.Book).WithOne(p => p.OrderedBook)
                .HasForeignKey<OrderedBook>(d => d.BookId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.User).WithMany(p => p.OrderedBooks).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<CoverImage>(entity =>
        {
            entity.HasKey(e => e.BookId);

            entity.HasIndex(e => e.BookId, "IX_CoverImages_BookId").IsUnique();

            entity.HasOne(c => c.Book)
                .WithOne(b => b.Cover)
                .HasForeignKey<CoverImage>(c => c.BookId);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

