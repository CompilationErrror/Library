using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace DataModelLibrary.Models
{
    [Index(nameof(Username), IsUnique = true)]

    public partial class User
    {
        public User()
        {
            Id = Guid.NewGuid();
        }

        [Key]
        public Guid Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        [Required]
        [StringLength(50)]
        public string Surname { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        public string PasswordHash { get; set; } = string.Empty;

        public int TotalBooksReturned { get; set; } = 0;

        public bool IsAdmin { get; set; }

        [JsonIgnore]
        public virtual ICollection<OrderedBook> OrderedBooks { get; } = new List<OrderedBook>();
    }

    public class UserUpdateModel
    {
        public Guid UserId { get; set; }
        public string? Name { get; set; }
        public string? Surname { get; set; }
        public string? Email { get; set; }
        public string? CurrentPassword { get; set; }
        public string? NewPassword { get; set; }
    }

    public class UserStats
    {
        public int CurrentlyBorrowed { get; set; }
        public int BooksReturned { get; set; }
        public int OverdueBooks { get; set; }
    }
}