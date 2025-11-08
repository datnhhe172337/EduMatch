using EduMatch.DataAccessLayer.Entities;
using EduMatch.DataAccessLayer.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static EduMatch.DataAccessLayer.Enum.InstitutionType;

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
	}
}
