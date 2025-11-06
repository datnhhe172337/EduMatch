using System;

namespace EduMatch.BusinessLogicLayer.DTOs
{
    public class SystemFeeDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public decimal? Percentage { get; set; }
        public decimal? FixedAmount { get; set; }
        public DateTime EffectiveFrom { get; set; }
        public DateTime? EffectiveTo { get; set; }
        public bool? IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
