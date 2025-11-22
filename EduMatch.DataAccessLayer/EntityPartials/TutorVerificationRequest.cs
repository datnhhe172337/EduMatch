using EduMatch.DataAccessLayer.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduMatch.DataAccessLayer.Entities;

public partial class TutorVerificationRequest
{
	[NotMapped]
	public TutorVerificationRequestStatus StatusEnum
	{
		get => (TutorVerificationRequestStatus)Status;
		set => Status = (int)value;
	}
}

