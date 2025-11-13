using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.Responses
{
    public class VectorSearchResult
    {
        public string Id { get; set; } = string.Empty;
        public double Score { get; set; }
        public string? Metadata { get; set; }
    }
}
