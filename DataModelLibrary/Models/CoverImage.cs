
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace DataModelLibrary.Models
{
    public class CoverImage
    {
        public int BookId { get; set; }
        
        [Url]
        public string? CoverImageUrl { get; set; }

        [JsonIgnore]
        public virtual Book Book { get; set; } = null!;
    }
}
