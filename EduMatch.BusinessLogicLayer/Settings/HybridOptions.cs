using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.Settings
{
    public class HybridOptions
    {
        public double VectorWeight { get; set; } = 0.5;
        public double KeywordWeight { get; set; } = 0.5;
        public int RerankTopN { get; set; } = 30;
        public int ReturnTopK { get; set; } = 10;
    }
}
