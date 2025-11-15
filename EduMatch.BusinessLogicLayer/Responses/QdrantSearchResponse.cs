using EduMatch.BusinessLogicLayer.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.Responses
{
    public class QdrantSearchResponse
    {
        public List<QdrantScoredPoint> Result { get; set; }
    }
}
