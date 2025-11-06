using EduMatch.DataAccessLayer.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduMatch.DataAccessLayer.Entities;


public partial class UserProfile
{
	[NotMapped]
	public Gender? GenderEnum
	{
		get => Gender.HasValue ? (Gender?)Gender.Value : null;
		set => Gender = value.HasValue ? (int)value.Value : (int?)null;
	}
}
