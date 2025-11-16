using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.Settings
{
    public class HybridSearchHit
    {
        public int TutorId { get; set; }
        public double Score { get; set; }
        public double VectorScore { get; set; }
        public double KeywordScore { get; set; }
        public string TutorInfo { get; set; } = "";
    }
}
