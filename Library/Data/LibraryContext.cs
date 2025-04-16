using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using DataModelLibrary.Models;
using DataModelLibrary.AuthModels;

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

    public virtual DbSet<CoverImages> CoverImages { get; set; }

    public DbSet<UserToken> UserTokens { get; set; }

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

        modelBuilder.Entity<CoverImages>(entity =>
        {
            entity.HasKey(e => e.BookId);

            entity.HasIndex(e => e.BookId, "IX_CoverImages_BookId").IsUnique();

            entity.HasOne(c => c.Book)
                .WithOne(b => b.Cover)
                .HasForeignKey<CoverImages>(c => c.BookId);
        });

        modelBuilder.Entity<UserToken>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.HasIndex(e => e.Token);

            entity.HasIndex(e => e.UserId);

            entity.HasOne(t => t.User)
                .WithMany(u => u.Tokens)
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Cascade); 

            entity.Property(e => e.Token)
                .IsRequired()
                .HasMaxLength(400); 

            entity.Property(e => e.RefreshToken)
                .IsRequired()
                .HasMaxLength(400);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

