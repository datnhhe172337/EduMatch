using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.Settings
{
    public class QdrantSettings
    {
        public string Endpoint { get; set; } = "http://localhost:6333/";
        public int VectorSize { get; set; } = 768;

        public string Host { get; set; } = "";
        public string Port { get; set; } = "";

        public string ApiKey { get; set; } = "";
    }
}
