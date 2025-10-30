using EduMatch.DataAccessLayer.Entities;
using EduMatch.DataAccessLayer.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduMatch.Tests.Common
{
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
	}
}
