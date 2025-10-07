using System;
using System.Collections.Generic;

namespace EduMatch.DataAccessLayer.Entities;

public partial class Province
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<SubDistrict> SubDistricts { get; set; } = new List<SubDistrict>();
}
