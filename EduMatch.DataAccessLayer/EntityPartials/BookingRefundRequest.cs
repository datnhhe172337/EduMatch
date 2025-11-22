using EduMatch.DataAccessLayer.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduMatch.DataAccessLayer.Entities;

public partial class BookingRefundRequest
{
	[NotMapped]
	public BookingRefundRequestStatus StatusEnum
	{
		get => (BookingRefundRequestStatus)Status;
		set => Status = (int)value;
	}
}

