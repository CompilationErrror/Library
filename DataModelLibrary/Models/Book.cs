using DataModelLibrary.Models.Base;
using System.Text.Json.Serialization;

namespace DataModelLibrary.Models
{
    public partial class Book : BaseEntity
    {
        public string? Title { get; set; }

        public string Author { get; set; } = string.Empty;

        public int PublishedYear { get; set; } = 0;

        public int QuantityInStock { get; set; }

        public double Price { get; set; }

        public int? GenreId { get; set; }

        [JsonIgnore]
        public virtual OrderedBook? OrderedBook { get; set; }

        [JsonIgnore]
        public virtual CoverImage? Cover { get; set; }

        public virtual Genre? Genre { get; set; }
    }
}