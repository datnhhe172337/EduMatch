using EduMatch.BusinessLogicLayer.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.Requests
{
    public class QdrantHybridSearchRequest
    {
        public QdrantQuery Query { get; set; }
        public float[] Vector { get; set; }
        public int Limit { get; set; } = 10;
        public bool WithPayload { get; set; } = true;
    }
}
