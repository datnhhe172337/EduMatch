using AutoMapper;
using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Requests.TutorProfile;
using EduMatch.BusinessLogicLayer.Requests.TutorCertificate;
using EduMatch.BusinessLogicLayer.Requests.TutorEducation;
using EduMatch.BusinessLogicLayer.Requests.TutorSubject;
using EduMatch.BusinessLogicLayer.Requests.TutorAvailability;
using EduMatch.BusinessLogicLayer.Requests.User;
using EduMatch.BusinessLogicLayer.Requests.CertificateType;
using EduMatch.BusinessLogicLayer.Requests.Level;
using EduMatch.BusinessLogicLayer.Requests.Subject;
using EduMatch.BusinessLogicLayer.Requests.TimeSlot;
using EduMatch.DataAccessLayer.Entities;
using EduMatch.DataAccessLayer.Enum;
using System;

namespace EduMatch.BusinessLogicLayer.Mappings
{
	public class MappingProfile : Profile
	{
		public MappingProfile()
		{
			// CertificateType mappings
			CreateMap<CertificateType, CertificateTypeDto>()
				.ForMember(dest => dest.Verified, opt => opt.MapFrom(src => (VerifyStatus)src.Verified))
				.ForMember(d => d.Subjects, o => o.MapFrom(s =>
					s.CertificateTypeSubjects.Select(x => new SubjectDto
					{
						Id = x.Subject.Id,
						SubjectName = x.Subject.SubjectName,
						CertificateTypes = new List<CertificateTypeDto>()
					})
				));	


			CreateMap<CertificateTypeCreateRequest, CertificateType>()
				.ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.Now));

			CreateMap<CertificateTypeUpdateRequest, CertificateType>();

			// CertificateTypeSubject mappings
			CreateMap<CertificateTypeSubject, CertificateTypeSubjectDto>().ReverseMap();

			// EducationInstitution mappings
			CreateMap<EducationInstitution, EducationInstitutionDto>()
				.ForMember(dest => dest.InstitutionType, opt => opt.MapFrom(src => (InstitutionType)src.InstitutionType))
				.ForMember(dest => dest.Verified, opt => opt.MapFrom(src => (VerifyStatus)src.Verified));



			// Level mappings
			CreateMap<Level, LevelDto>().ReverseMap();
			CreateMap<LevelCreateRequest, Level>();
			CreateMap<LevelUpdateRequest, Level>();

			// Subject mappings
			CreateMap<Subject, SubjectDto>()
				.ForMember(dest => dest.CertificateTypes, opt => opt.MapFrom(src => src.CertificateTypeSubjects.Select(cts => cts.CertificateType)));

			CreateMap<SubjectCreateRequest, Subject>();
			CreateMap<SubjectUpdateRequest, Subject>();

			// TimeSlot mappings
			CreateMap<TimeSlot, TimeSlotDto>().ReverseMap();
			CreateMap<TimeSlotCreateRequest, TimeSlot>();
			CreateMap<TimeSlotUpdateRequest, TimeSlot>();

			// TutorAvailability mappings
			CreateMap<TutorAvailability, TutorAvailabilityDto>()
				.ForMember(dest => dest.Slot, opt => opt.MapFrom(src => src.Slot))
				.ForMember(dest => dest.Tutor, opt => opt.MapFrom(src => src.Tutor))
				.ForMember(dest => dest.Status, opt => opt.MapFrom(src => (TutorAvailabilityStatus)src.Status))
				.ReverseMap();
			CreateMap<TutorAvailabilityCreateRequest, TutorAvailability>()
				.ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.Now))
				.ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
				.ForMember(dest => dest.Status, opt => opt.Ignore())
				.ForMember(dest => dest.Slot, opt => opt.Ignore())
				.ForMember(dest => dest.Tutor, opt => opt.Ignore());
			CreateMap<TutorAvailabilityUpdateRequest, TutorAvailability>()
				.ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.Now))
				.ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
				.ForMember(dest => dest.Slot, opt => opt.Ignore())
				.ForMember(dest => dest.Tutor, opt => opt.Ignore());

			// TutorCertificate mappings
			CreateMap<TutorCertificate, TutorCertificateDto>()
				.ForMember(dest => dest.Verified, opt => opt.MapFrom(src => (VerifyStatus)src.Verified))
				.ForMember(dest => dest.CertificateType, opt => opt.MapFrom(src => src.CertificateType));

			// TutorEducation mappings
			CreateMap<TutorEducation, TutorEducationDto>()
				.ForMember(dest => dest.Verified, opt => opt.MapFrom(src => (VerifyStatus)src.Verified))
				.ForMember(dest => dest.Institution, opt => opt.MapFrom(src => src.Institution));


			// TutorProfile mappings
			// ========== PROFILE ==========
			CreateMap<TutorProfile, TutorProfileDto>()
				.ForMember(dest => dest.TeachingModes, opt => opt.MapFrom(src => (TeachingMode)src.TeachingModes))
				.ForMember(dest => dest.Status, opt => opt.MapFrom(src => (TutorStatus)src.Status))
				.ForMember(d => d.TutorAvailabilities,
					opt => opt.MapFrom(s => s.TutorAvailabilities != null
						? s.TutorAvailabilities.Select(a => new TutorAvailabilityDto
						{
							Id = a.Id,
							StartDate = a.StartDate,
							EndDate = a.EndDate,
							Status = (TutorAvailabilityStatus?)a.Status,
							CreatedAt = a.CreatedAt,
							UpdatedAt = a.UpdatedAt,
							Slot = a.Slot != null
								? new TimeSlotDto { Id = a.Slot.Id, StartTime = a.Slot.StartTime, EndTime = a.Slot.EndTime }
								: null
						})
						: new List<TutorAvailabilityDto>()))

				.ForMember(d => d.TutorCertificates,
					opt => opt.MapFrom(s => s.TutorCertificates != null
						? s.TutorCertificates.Select(c => new TutorCertificateDto
						{
							Id = c.Id,
							IssueDate = c.IssueDate,
							ExpiryDate = c.ExpiryDate,
							CreatedAt = c.CreatedAt,
							Verified = (VerifyStatus)c.Verified,
							CertificateUrl = c.CertificateUrl,
							RejectReason = c.RejectReason,
							CertificateType = c.CertificateType != null
								? new CertificateTypeDto { Id = c.CertificateType.Id, Name = c.CertificateType.Name }
								: null
						})
						: new List<TutorCertificateDto>()))
				.ForMember(d => d.TutorEducations,
					opt => opt.MapFrom(s => s.TutorEducations != null
						? s.TutorEducations.Select(e => new TutorEducationDto
						{
							Id = e.Id,
							CertificateUrl = e.CertificateUrl,
							IssueDate = e.IssueDate,
							CreatedAt = e.CreatedAt,
							Verified = (VerifyStatus)e.Verified,
							RejectReason = e.RejectReason,

							Institution = e.Institution != null
								? new EducationInstitutionDto { Id = e.Institution.Id, Name = e.Institution.Name }
								: null
						})
						: new List<TutorEducationDto>()))
				.ForMember(d => d.TutorSubjects,
					opt => opt.MapFrom(s => s.TutorSubjects != null
						? s.TutorSubjects.Select(ts => new TutorSubjectDto
						{
							Id = ts.Id,
							HourlyRate = ts.HourlyRate,
							Subject = ts.Subject != null
								? new SubjectDto { Id = ts.Subject.Id, SubjectName = ts.Subject.SubjectName }
								: null,
							Level = ts.Level != null
								? new LevelDto { Id = ts.Level.Id, Name = ts.Level.Name }
								: null
						})
						: new List<TutorSubjectDto>()))

				// mapping from UserProfile
				.ForMember(d => d.Gender, opt => opt.MapFrom(src =>(Gender) src.UserEmailNavigation.UserProfile.Gender))
				.ForMember(d => d.Province, opt => opt.MapFrom(src => src.UserEmailNavigation.UserProfile.City))
				.ForMember(d => d.SubDistrict, opt => opt.MapFrom(src => src.UserEmailNavigation.UserProfile.SubDistrict))
				.ForMember(d => d.Dob, opt => opt.MapFrom(src => src.UserEmailNavigation.UserProfile.Dob))
				.ForMember(d => d.AvatarUrl, opt => opt.MapFrom(src => src.UserEmailNavigation.UserProfile.AvatarUrl))
				.ForMember(d => d.AddressLine, opt => opt.MapFrom(src => src.UserEmailNavigation.UserProfile.AddressLine))
				.ForMember(d => d.Latitude, opt => opt.MapFrom(src => src.UserEmailNavigation.UserProfile.Latitude))
				.ForMember(d => d.Longitude, opt => opt.MapFrom(src => src.UserEmailNavigation.UserProfile.Longitude))
				// mapping to User 
				.ForMember(d => d.UserName, opt => opt.MapFrom(src => src.UserEmailNavigation.UserName))
				.ForMember(d => d.Phone, opt => opt.MapFrom(src => src.UserEmailNavigation.Phone));










			// TutorSubject mappings
			CreateMap<TutorSubject, TutorSubjectDto>().ReverseMap();
			CreateMap<TutorSubjectCreateRequest, TutorSubject>();
			CreateMap<TutorSubjectUpdateRequest, TutorSubject>();

			




			// User mappings
			CreateMap<User, UserDto>().ReverseMap();
			CreateMap<UserUpdateRequest, User>()
				.ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
		


			// Role mappings
			CreateMap<Role, RoleDto>().ReverseMap();

			// RefreshToken mappings
			CreateMap<RefreshToken, RefreshTokenDto>().ReverseMap();

			// UserProfile mappings
			CreateMap<UserProfile, UserProfileDto>().ReverseMap();

			CreateMap<UserProfileUpdateRequest, UserProfile>()
			.ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));


			// Province mappings
			CreateMap<Province, ProvinceDto>();

			// SubDistrict mappings
			CreateMap<SubDistrict, SubDistrictDto>();
		}
	}
}
