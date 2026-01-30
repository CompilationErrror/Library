using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataModelLibrary.QueryParameters
{
    public class BookQueryParameters
    {
        public int Offset { get; set; } = 0;
        public int Limit { get; set; } = 10;

        //Sort Parameters
        public string? SortBy { get; set; } = "Id"; 
        public bool SortDescending { get; set; } = false;

        //Filter parametrers
        public string? Author { get; set; }
        public int? YearFrom { get; set; }
        public int? YearTo { get; set; }
        public double? PriceFrom { get; set; }
        public double? PriceTo { get; set; }
        public bool AvailableOnly { get; set; } = false;
        public List<int>? GenreIds { get; set; }
    }
}
