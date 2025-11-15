using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.Settings
{
    public class QdrantScoredPoint
    {
        public string Id { get; set; }
        public double Score { get; set; }
        public Dictionary<string, object> Payload { get; set; }
    }
}
