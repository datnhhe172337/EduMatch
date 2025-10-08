using System;
using System.Collections.Generic;

namespace EduMatch.DataAccessLayer.Entities;

public partial class SubDistrict
{
    public int Id { get; set; }

    public int ProvinceId { get; set; }

    public string Name { get; set; } = null!;

    public virtual Province Province { get; set; } = null!;
}
