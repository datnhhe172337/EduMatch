using System;
using System.Collections.Generic;

namespace EduMatch.DataAccessLayer.Entities;

public partial class UserProfile
{
    public string UserEmail { get; set; } = null!;

    public string? AvatarUrl { get; set; }

    public int? CityId { get; set; }

    public int? SubDistrictId { get; set; }

    public string? AddressLine { get; set; }

    public decimal? Latitude { get; set; }

    public decimal? Longitude { get; set; }

    public virtual Province? City { get; set; }

    public virtual SubDistrict? SubDistrict { get; set; }

    public virtual User UserEmailNavigation { get; set; } = null!;
}
