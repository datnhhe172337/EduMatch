using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.Responses
{
    public class PayosResponseDataDto
    {
        [JsonPropertyName("checkoutUrl")]
        public string CheckoutUrl { get; set; }

        // Add other fields from the "data" object if you need them
        [JsonPropertyName("paymentLinkId")]
        public string PaymentLinkId { get; set; }
    }
}
