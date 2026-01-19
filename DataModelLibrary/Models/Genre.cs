using DataModelLibrary.Models.Base;
using System.Text.Json.Serialization;

namespace DataModelLibrary.Models
{
    public partial class Genre: BaseEntity
    {
        public string Name { get; set; }

        [JsonIgnore]
        public virtual ICollection<Book> Books { get; } = new List<Book>();
    }
}
