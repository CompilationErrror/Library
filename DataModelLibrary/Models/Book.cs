using System.Text.Json.Serialization;

namespace DataModelLibrary.Models
{
    public partial class Book
    {
        public int Id { get; set; }

        public string? Title { get; set; }

        public string Author { get; set; } = string.Empty;

        public bool IsAvailable { get; set; } = true;

        [JsonIgnore]
        public virtual OrderedBook? OrderedBook { get; set; }

        [JsonIgnore]
        public virtual CoverImage? Cover { get; set; }
    }
}