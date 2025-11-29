using EduMatch.DataAccessLayer.Entities;
using EduMatch.DataAccessLayer.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduMatch.Tests.Common
{
	/// <summary>
	/// Factory class để tạo các fake data objects cho testing
	/// Cung cấp các helper methods để tạo TutorProfile, EducationInstitution, TutorEducation với các tham số tùy chỉnh
	/// </summary>
	public static class FakeDataFactory
	{
		
		public static TutorProfile CreateFakeTutorProfile(string email = "abc@gmail.com")
		{
			return new TutorProfile
			{
				Id = 1,
				UserEmail = email,
				TeachingModes = (int)TeachingMode.Online,
				Status = (int)TutorStatus.Approved,

				UserEmailNavigation = new User
				{
					UserName = "Tutor A",
					Phone = "0123456789",
					UserProfile = new UserProfile
					{
						City = new Province { Id = 1, Name = "Hà Nội" },
						SubDistrict = new SubDistrict { Id = 1, Name = "Cầu Giấy" },

						Dob = new DateTime(1998, 1, 1),
						AvatarUrl = "avatar.jpg",
						AddressLine = "123 Nguyen Trai",
						Latitude = 21.0285m,
						Longitude = 105.8542m,

					}
				},

				TutorSubjects = new List<TutorSubject>
				{
					new TutorSubject
					{
						Id = 1,
						HourlyRate = 200000,
						Subject = new Subject { Id = 10, SubjectName = "Math" },
						Level = new Level { Id = 5, Name = "High School" }
					}
				},

				TutorCertificates = new List<TutorCertificate>
				{
					new TutorCertificate
					{
						Id = 1,
						Verified = (int)VerifyStatus.Verified,
						CertificateUrl = "cert.jpg",
						CertificateType = new CertificateType { Id = 1, Name = "TESOL" }
					}
				},

				TutorEducations = new List<TutorEducation>
				{
					new TutorEducation
					{
						Id = 1,
						Verified = (int)VerifyStatus.Verified,
						Institution = new EducationInstitution { Id = 1, Name = "Hanoi University" }
					}
				}
			};
		}

		/// <summary>
		/// Tạo TutorProfile giả với ID cụ thể (dùng cho test)
		/// </summary>
		public static TutorProfile CreateFakeTutorProfile(int id = 1)
		{
			return new TutorProfile
			{
				Id = id,
				UserEmail = $"tutor{id}@example.com"
			};
		}

	/// <summary>
	/// Tạo EducationInstitution giả với ID cụ thể (dùng cho test)
	/// </summary>
	public static EducationInstitution CreateFakeEducationInstitution(int id = 1)
	{
		return new EducationInstitution
		{
			Id = id,
			Code = $"INST{id}",
			Name = $"Institution {id}",
			Verified = (int)VerifyStatus.Verified,
			InstitutionType = (int)InstitutionType.University
		};
	}

		/// <summary>
		/// Tạo TutorEducation giả với các tham số tùy chỉnh (dùng cho test)
		/// </summary>
		public static TutorEducation CreateFakeTutorEducation(
			int id = 1,
			int tutorId = 1,
			int institutionId = 1,
			VerifyStatus verified = VerifyStatus.Verified,
			string? certificateUrl = null,
			string? rejectReason = null)
		{
			return new TutorEducation
			{
				Id = id,
				TutorId = tutorId,
				InstitutionId = institutionId,
				IssueDate = new DateTime(2020, 1, 1),
				CertificateUrl = certificateUrl ?? "https://example.com/certificate.jpg",
				CertificatePublicId = null,
				CreatedAt = DateTime.UtcNow,
				Verified = (int)verified,
				RejectReason = rejectReason,
				Institution = new EducationInstitution
				{
					Id = institutionId,
					Code = $"INST{institutionId}",
					Name = $"Institution {institutionId}",
					Verified = (int)VerifyStatus.Verified,
					InstitutionType = (int)InstitutionType.University
				},
				Tutor = new TutorProfile
				{
					Id = tutorId,
					UserEmail = $"tutor{tutorId}@example.com"
				}
			};
		}

		/// <summary>
		/// Tạo Subject giả với ID cụ thể (dùng cho test)
		/// </summary>
		public static Subject CreateFakeSubject(int id = 1)
		{
			return new Subject
			{
				Id = id,
				SubjectName = $"Subject {id}"
			};
		}

		/// <summary>
		/// Tạo Level giả với ID cụ thể (dùng cho test)
		/// </summary>
		public static Level CreateFakeLevel(int id = 1)
		{
			return new Level
			{
				Id = id,
				Name = $"Level {id}"
			};
		}

		/// <summary>
		/// Tạo TutorSubject giả với các tham số tùy chỉnh (dùng cho test)
		/// </summary>
		public static TutorSubject CreateFakeTutorSubject(
			int id = 1,
			int tutorId = 1,
			int subjectId = 1,
			int? levelId = 1,
			decimal? hourlyRate = 200000)
		{
			return new TutorSubject
			{
				Id = id,
				TutorId = tutorId,
				SubjectId = subjectId,
				LevelId = levelId,
				HourlyRate = hourlyRate,
				Tutor = new TutorProfile
				{
					Id = tutorId,
					UserEmail = $"tutor{tutorId}@example.com"
				},
				Subject = new Subject
				{
					Id = subjectId,
					SubjectName = $"Subject {subjectId}"
				},
				Level = levelId.HasValue ? new Level
				{
					Id = levelId.Value,
					Name = $"Level {levelId.Value}"
				} : null
			};
		}

		/// <summary>
		/// Tạo TimeSlot giả với các tham số tùy chỉnh (dùng cho test)
		/// </summary>
		public static TimeSlot CreateFakeTimeSlot(
			int id = 1,
			TimeOnly? startTime = null,
			TimeOnly? endTime = null)
		{
			return new TimeSlot
			{
				Id = id,
				StartTime = startTime ?? new TimeOnly(8, 0),
				EndTime = endTime ?? new TimeOnly(10, 0)
			};
		}

		/// <summary>
		/// Tạo TutorAvailability giả với các tham số tùy chỉnh (dùng cho test)
		/// </summary>
		public static TutorAvailability CreateFakeTutorAvailability(
			int id = 1,
			int tutorId = 1,
			int slotId = 1,
			DateTime? startDate = null,
			int status = 0)
		{
			var date = startDate ?? DateTime.UtcNow.Date;
			return new TutorAvailability
			{
				Id = id,
				TutorId = tutorId,
				SlotId = slotId,
				StartDate = date.AddHours(8),
				EndDate = date.AddHours(10),
				Status = status,
				CreatedAt = DateTime.UtcNow,
				Tutor = new TutorProfile
				{
					Id = tutorId,
					UserEmail = $"tutor{tutorId}@example.com"
				},
				Slot = new TimeSlot
				{
					Id = slotId,
					StartTime = new TimeOnly(8, 0),
					EndTime = new TimeOnly(10, 0)
				}
			};
		}

		/// <summary>
		/// Tạo CertificateType giả với các tham số tùy chỉnh (dùng cho test)
		/// </summary>
		public static CertificateType CreateFakeCertificateType(
			int id = 1,
			string? code = null,
			string? name = null)
		{
			return new CertificateType
			{
				Id = id,
				Code = code ?? $"CERT{id}",
				Name = name ?? $"Certificate Type {id}"
			};
		}

		/// <summary>
		/// Tạo TutorCertificate giả với các tham số tùy chỉnh (dùng cho test)
		/// </summary>
		public static TutorCertificate CreateFakeTutorCertificate(
			int id = 1,
			int tutorId = 1,
			int certificateTypeId = 1,
			DateTime? issueDate = null,
			DateTime? expiryDate = null,
			string? certificateUrl = null,
			VerifyStatus verified = VerifyStatus.Pending,
			string? rejectReason = null)
		{
			return new TutorCertificate
			{
				Id = id,
				TutorId = tutorId,
				CertificateTypeId = certificateTypeId,
				IssueDate = issueDate ?? new DateTime(2020, 1, 1),
				ExpiryDate = expiryDate ?? new DateTime(2025, 12, 31),
				CertificateUrl = certificateUrl ?? "https://example.com/certificate.jpg",
				CertificatePublicId = null,
				CreatedAt = DateTime.UtcNow,
				Verified = (int)verified,
				RejectReason = rejectReason,
				Tutor = CreateFakeTutorProfile(tutorId),
				CertificateType = CreateFakeCertificateType(certificateTypeId)
			};
		}

		/// <summary>
		/// Tạo SystemFee giả với các tham số tùy chỉnh (dùng cho test)
		/// </summary>
		public static SystemFee CreateFakeSystemFee(
			int id = 1,
			string? name = null,
			decimal? percentage = 10,
			decimal? fixedAmount = 5000,
			bool? isActive = true)
		{
			return new SystemFee
			{
				Id = id,
				Name = name ?? $"System Fee {id}",
				Percentage = percentage,
				FixedAmount = fixedAmount,
				EffectiveFrom = DateTime.UtcNow.AddDays(-30),
				EffectiveTo = null,
				IsActive = isActive,
				CreatedAt = DateTime.UtcNow,
				UpdatedAt = null
			};
		}

		/// <summary>
		/// Tạo User giả với email cụ thể (dùng cho test)
		/// </summary>
		public static User CreateFakeUser(string email = "learner@example.com")
		{
			return new User
			{
				Email = email,
				UserName = "Test User",
				LoginProvider = "Email"
			};
		}

		/// <summary>
		/// Tạo Booking giả với các tham số tùy chỉnh (dùng cho test)
		/// </summary>
		public static Booking CreateFakeBooking(
			int id = 1,
			string? learnerEmail = null,
			int tutorSubjectId = 1,
			int totalSessions = 1,
			decimal unitPrice = 200000,
			int systemFeeId = 1,
			decimal systemFeeAmount = 25000,
			int paymentStatus = 0,
			int status = 0)
		{
			return new Booking
			{
				Id = id,
				LearnerEmail = learnerEmail ?? "learner@example.com",
				TutorSubjectId = tutorSubjectId,
				BookingDate = DateTime.UtcNow,
				TotalSessions = totalSessions,
				UnitPrice = unitPrice,
				TotalAmount = unitPrice * totalSessions,
				PaymentStatus = paymentStatus,
				RefundedAmount = 0,
				Status = status,
				SystemFeeId = systemFeeId,
				SystemFeeAmount = systemFeeAmount,
				CreatedAt = DateTime.UtcNow,
				UpdatedAt = null,
				LearnerEmailNavigation = CreateFakeUser(learnerEmail ?? "learner@example.com"),
				TutorSubject = CreateFakeTutorSubject(tutorSubjectId),
				SystemFee = CreateFakeSystemFee(systemFeeId)
			};
		}

		/// <summary>
		/// Tạo GoogleToken giả với các tham số tùy chỉnh (dùng cho test)
		/// </summary>
		public static GoogleToken CreateFakeGoogleToken(
			string accountEmail = "system@edumatch.com",
			string? accessToken = null,
			string? refreshToken = null)
		{
			return new GoogleToken
			{
				Id = 1,
				AccountEmail = accountEmail,
				AccessToken = accessToken ?? "fake_access_token",
				RefreshToken = refreshToken ?? "fake_refresh_token",
				TokenType = "Bearer",
				Scope = "https://www.googleapis.com/auth/calendar",
				ExpiresAt = DateTime.UtcNow.AddHours(1),
				CreatedAt = DateTime.UtcNow,
				UpdatedAt = null
			};
		}

		/// <summary>
		/// Tạo Schedule giả với các tham số tùy chỉnh (dùng cho test)
		/// </summary>
		public static Schedule CreateFakeSchedule(
			int id = 1,
			int availabilityId = 1,
			int bookingId = 1,
			int status = 0,
			bool includeBooking = true,
			bool includeAvailability = true)
		{
			var schedule = new Schedule
			{
				Id = id,
				AvailabilitiId = availabilityId,
				BookingId = bookingId,
				Status = status,
				AttendanceNote = null,
				IsRefunded = false,
				RefundedAt = null,
				CreatedAt = DateTime.UtcNow,
				UpdatedAt = null
			};

			if (includeAvailability)
			{
				var availability = CreateFakeTutorAvailability(availabilityId);
				schedule.Availabiliti = availability;
			}

			if (includeBooking)
			{
				var booking = CreateFakeBooking(bookingId);
				schedule.Booking = booking;
			}

			return schedule;
		}

		/// <summary>
		/// Tạo MeetingSession giả với các tham số tùy chỉnh (dùng cho test)
		/// </summary>
		public static MeetingSession CreateFakeMeetingSession(
			int id = 1,
			int scheduleId = 1,
			string? organizerEmail = null,
			string? meetLink = null,
			string? meetCode = null,
			string? eventId = null,
			DateTime? startTime = null,
			DateTime? endTime = null,
			int meetingType = 0)
		{
			var now = DateTime.UtcNow;
			return new MeetingSession
			{
				Id = id,
				ScheduleId = scheduleId,
				OrganizerEmail = organizerEmail ?? "system@edumatch.com",
				MeetLink = meetLink ?? "https://meet.google.com/abc-defg-hij",
				MeetCode = meetCode ?? "abc-defg-hij",
				EventId = eventId ?? "event123",
				StartTime = startTime ?? now.AddDays(1),
				EndTime = endTime ?? now.AddDays(1).AddHours(2),
				MeetingType = meetingType,
				CreatedAt = now,
				UpdatedAt = null,
				OrganizerEmailNavigation = CreateFakeGoogleToken(organizerEmail ?? "system@edumatch.com"),
				Schedule = CreateFakeSchedule(scheduleId)
			};
		}

		/// <summary>
		/// Tạo RefundPolicy giả với các tham số tùy chỉnh (dùng cho test)
		/// </summary>
		public static RefundPolicy CreateFakeRefundPolicy(
			int id = 1,
			string? name = null,
			decimal refundPercentage = 50,
			bool isActive = true)
		{
			return new RefundPolicy
			{
				Id = id,
				Name = name ?? $"Refund Policy {id}",
				Description = $"Description for policy {id}",
				RefundPercentage = refundPercentage,
				IsActive = isActive,
				CreatedAt = DateTime.UtcNow,
				CreatedBy = "admin@example.com",
				UpdatedAt = null,
				UpdatedBy = null
			};
		}

		/// <summary>
		/// Tạo BookingRefundRequest giả với các tham số tùy chỉnh (dùng cho test)
		/// </summary>
		public static BookingRefundRequest CreateFakeBookingRefundRequest(
			int id = 1,
			int bookingId = 1,
			string? learnerEmail = null,
			int refundPolicyId = 1,
			string? reason = null,
			int status = 0,
			decimal? approvedAmount = null)
		{
			return new BookingRefundRequest
			{
				Id = id,
				BookingId = bookingId,
				LearnerEmail = learnerEmail ?? "learner@example.com",
				RefundPolicyId = refundPolicyId,
				Reason = reason ?? "Test reason",
				Status = status,
				ApprovedAmount = approvedAmount,
				AdminNote = null,
				CreatedAt = DateTime.UtcNow,
				ProcessedAt = null,
				ProcessedBy = null,
				Booking = CreateFakeBooking(bookingId),
				LearnerEmailNavigation = CreateFakeUser(learnerEmail ?? "learner@example.com"),
				RefundPolicy = CreateFakeRefundPolicy(refundPolicyId)
			};
		}

		/// <summary>
		/// Tạo RefundRequestEvidence giả với các tham số tùy chỉnh (dùng cho test)
		/// </summary>
		public static RefundRequestEvidence CreateFakeRefundRequestEvidence(
			int id = 1,
			int bookingRefundRequestId = 1,
			string? fileUrl = null)
		{
			return new RefundRequestEvidence
			{
				Id = id,
				BookingRefundRequestId = bookingRefundRequestId,
				FileUrl = fileUrl ?? "https://example.com/evidence.jpg",
				CreatedAt = DateTime.UtcNow,
				BookingRefundRequest = CreateFakeBookingRefundRequest(bookingRefundRequestId)
			};
		}
	}
}
