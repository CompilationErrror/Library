
namespace DataModelLibrary.FilterModels
{
    public class BookFilter
    {
        public string? Author { get; set; }
        public int? YearFrom { get; set; }
        public int? YearTo { get; set; }
        public int? PriceFrom { get; set; }
        public int? PriceTo { get; set; }
        public bool AvailableOnly { get; set; }
    }
}
