using EduMatch.DataAccessLayer.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduMatch.DataAccessLayer.Entities;

public partial class Booking
{
	[NotMapped]
	public PaymentStatus PaymentStatusEnum
	{
		get => (PaymentStatus)PaymentStatus;
		set => PaymentStatus = (int)value;
	}

	[NotMapped]
	public BookingStatus BookingStatusEnum
	{
		get => (BookingStatus)Status;
		set => Status = (int)value;
	}

}