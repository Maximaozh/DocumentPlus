using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocumentPlus.Shared.Dto.Docs
{
    public class DocFilter
    {
        public string? Search { get; set; }
        public string? Order { get; set; }
        public int SortDirection { get; set; }
    }
}
