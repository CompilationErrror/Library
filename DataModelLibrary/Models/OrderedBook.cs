using System.Text.Json.Serialization;

namespace DataModelLibrary.Models
{
    public partial class OrderedBook
    {
        public int BookId { get; set; }
        public Guid? UserId { get; set; }
        public DateTime OrderDate { get; set; } = DateTime.Now;
        public DateTime? ReturnDate { get; set; } = DateTime.Now.AddDays(14);
        
        public virtual Book Book { get; set; } = null!;

        [JsonIgnore]
        public virtual User? User { get; set; }
    }
}