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
        public string? SortBy { get; set; } = "Id"; 
        public bool SortDescending { get; set; } = false;
    }
}
