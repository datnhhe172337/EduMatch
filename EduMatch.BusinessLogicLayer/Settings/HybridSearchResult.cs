using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.Settings
{
    public class HybridSearchResult
    {
        public int TutorId { get; set; }
        public string Snippet { get; set; } = "";
        public double QdrantScore { get; set; }   // Qdrant integrated hybrid score (normalized 0..1)
        public double CombinedScore { get; set; } // post-rerank score
    }
}
