using System.Collections.Generic;

namespace EduMatch.BusinessLogicLayer.DTOs
{
	public class ProvinceDto
	{
		public int Id { get; set; }
		public string Name { get; set; } = null!;
		public List<SubDistrictDto>? SubDistricts { get; set; }
		public List<UserProfileDto>? UserProfiles { get; set; }
	}
}
