using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.DTOs
{
    public class QdrantPoint
    {
        public ulong Id { get; set; }
        public float Score { get; set; }
        public Dictionary<string, object> Payload { get; set; } = new();
    }
}
