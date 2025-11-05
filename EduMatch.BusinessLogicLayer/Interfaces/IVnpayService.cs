using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace EduMatch.BusinessLogicLayer.Interfaces
{
    public class VnpayResponse
    {
        public bool IsSuccess { get; set; }
        public string TransactionId { get; set; }
        public string OrderId { get; set; }
        public string ResponseCode { get; set; }
        public decimal Amount { get; set; }
        public string Message { get; set; }
    }

    public interface IVnpayService
    {
        string CreatePaymentUrl(string orderId, decimal amount, string orderInfo, HttpContext context);
        VnpayResponse ValidatePaymentResponse(IQueryCollection vnpayData);
    }
}