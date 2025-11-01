using EduMatch.DataAccessLayer.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduMatch.DataAccessLayer.Entities;


public partial class TutorProfile
{
	[NotMapped]
	public TeachingMode? TeachingModeEnum
	{
		get =>(TeachingMode) TeachingModes;
		set => TeachingModes =(int) value;
	}


	[NotMapped]
	public TutorStatus? StatusEnum
	{
		get =>(TutorStatus) Status;
		set => Status = (int)value;
	}
}
