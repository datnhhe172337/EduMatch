using EduMatch.BusinessLogicLayer.Interfaces;
using EduMatch.BusinessLogicLayer.Settings;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration; // <-- ADD THIS
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace EduMatch.BusinessLogicLayer.Services
{
    public class VnpayService : IVnpayService
    {
        private readonly VnpaySettings _settings;
        // private readonly IConfiguration _configuration; 

        // --- UPDATE THE CONSTRUCTOR ---
        public VnpayService(IOptions<VnpaySettings> settings)
        {
            _settings = settings.Value;
            // _configuration = configuration; 
        }

        // Filename: VnpayService.cs

        public string CreatePaymentUrl(string orderId, decimal amount, string orderInfo, HttpContext context)
        {
            var vnpayData = new SortedList<string, string>(new VnpayComparer());

            vnpayData.Add("vnp_Version", _settings.Version);
            vnpayData.Add("vnp_Command", "pay");
            vnpayData.Add("vnp_TmnCode", _settings.TmnCode);
            vnpayData.Add("vnp_Amount", ((long)amount * 100).ToString());
            vnpayData.Add("vnp_CreateDate", DateTime.UtcNow.ToString("yyyyMMddHHmmss"));
            vnpayData.Add("vnp_CurrCode", "VND");
            vnpayData.Add("vnp_IpAddr", GetIpAddress(context));
            vnpayData.Add("vnp_Locale", "vn");
            vnpayData.Add("vnp_OrderInfo", orderInfo);
            vnpayData.Add("vnp_OrderType", "other");
            vnpayData.Add("vnp_ReturnUrl", _settings.ReturnUrl);

            //vnpayData.Add("vnp_IpnUrl", _settings.IpnUrl);
            // ---------------------------------

            vnpayData.Add("vnp_TxnRef", orderId);

            string queryData = string.Join("&", vnpayData.Select(kv => $"{kv.Key}={WebUtility.UrlEncode(kv.Value)}"));
            string signature = HmacSHA512(_settings.HashSecret, queryData);

            return $"{_settings.BaseUrl}?{queryData}&vnp_SecureHash={signature}";
        }

        public VnpayResponse ValidatePaymentResponse(IQueryCollection vnpayData)
        {
            var data = new SortedList<string, string>(new VnpayComparer());
            foreach (var key in vnpayData.Keys)
            {
                // Note: VNPay's new docs might use vnp_SecureHashType, adjust as needed
                if (key.StartsWith("vnp_") && key != "vnp_SecureHash")
                {
                    data.Add(key, vnpayData[key].ToString());
                }
            }

            string receivedSignature = vnpayData["vnp_SecureHash"];
            string queryData = string.Join("&", data.Select(kv => $"{kv.Key}={WebUtility.UrlEncode(kv.Value)}"));
            string calculatedSignature = HmacSHA512(_settings.HashSecret, queryData);

            bool isSuccess = calculatedSignature.Equals(receivedSignature, StringComparison.OrdinalIgnoreCase);

            return new VnpayResponse
            {
                IsSuccess = isSuccess,
                ResponseCode = vnpayData["vnp_ResponseCode"],
                TransactionId = vnpayData["vnp_TransactionNo"],
                OrderId = vnpayData["vnp_TxnRef"],
                Amount = decimal.Parse(vnpayData["vnp_Amount"]) / 100,
                Message = vnpayData["vnp_OrderInfo"]
            };
        }

        private string HmacSHA512(string key, string inputData)
        {
            var hash = new StringBuilder();
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            byte[] inputBytes = Encoding.UTF8.GetBytes(inputData);
            using (var hmac = new HMACSHA512(keyBytes))
            {
                byte[] hashValue = hmac.ComputeHash(inputBytes);
                foreach (var b in hashValue)
                {
                    hash.Append(b.ToString("x2"));
                }
            }
            return hash.ToString();
        }

        private string GetIpAddress(HttpContext context)
        {
            // Check for X-Forwarded-For header in case of reverse proxy
            if (context.Request.Headers.TryGetValue("X-Forwarded-For", out var forwardedFor))
            {
                var ip = forwardedFor.ToString().Split(',').FirstOrDefault();
                if (!string.IsNullOrEmpty(ip))
                {
                    return ip;
                }
            }

            var ipAddress = context.Connection.RemoteIpAddress?.ToString();
            if (ipAddress == "::1") ipAddress = "127.0.0.1";
            return ipAddress ?? "127.0.0.1";
        }
    }

    // Helper class for sorting parameters
    public class VnpayComparer : IComparer<string>
    {
        public int Compare(string? x, string? y)
        {
            if (x == y) return 0;
            if (x == null) return -1;
            if (y == null) return 1;
            return string.CompareOrdinal(x, y);
        }
    }
}