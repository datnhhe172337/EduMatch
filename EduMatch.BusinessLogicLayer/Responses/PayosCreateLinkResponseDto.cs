using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.Responses
{
    public class PayosCreateLinkResponseDto
    {
        [JsonPropertyName("code")]
        public string Code { get; set; }

        [JsonPropertyName("desc")]
        public string Desc { get; set; }

        [JsonPropertyName("data")]
        public PayosResponseDataDto Data { get; set; }

        [JsonPropertyName("signature")]
        public string Signature { get; set; }
    }
}
