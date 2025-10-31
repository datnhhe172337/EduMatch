using System;
using System.ComponentModel.DataAnnotations;

namespace EduMatch.BusinessLogicLayer.Requests.SystemFee
{
    public class SystemFeeUpdateRequest
    {
        [Required(ErrorMessage = "Id là bắt buộc")]
        public int Id { get; set; }

        [Required(ErrorMessage = "Name là bắt buộc")]
        public string? Name { get; set; }

        [Range(0, 100, ErrorMessage = "Percentage phải trong khoảng 0 - 100")]
        public decimal? Percentage { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "FixedAmount không được âm")]
        public decimal? FixedAmount { get; set; }

        public bool? IsActive { get; set; }
    }
}
