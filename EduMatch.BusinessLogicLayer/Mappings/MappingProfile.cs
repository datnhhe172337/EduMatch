using AutoMapper;
using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Requests;
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
				.ForMember(dest => dest.Subjects, opt => opt.Ignore());


			CreateMap<CertificateTypeCreateRequest, CertificateType>()
				.ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.Now));

			CreateMap<CertificateTypeUpdateRequest, CertificateType>();

			// CertificateTypeSubject mappings
			CreateMap<CertificateTypeSubject, CertificateTypeSubjectDto>().ReverseMap();

			// EducationInstitution mappings
			CreateMap<EducationInstitution, EducationInstitutionDto>().ReverseMap();
			 
		

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
			CreateMap<TutorAvailability, TutorAvailabilityDto>().ReverseMap();
			CreateMap<TutorAvailabilityCreateRequest, TutorAvailability>();
			CreateMap<TutorAvailabilityUpdateRequest, TutorAvailability>();

			// TutorCertificate mappings
			CreateMap<TutorCertificate, TutorCertificateDto>().ReverseMap();


			// TutorEducation mappings
			CreateMap<TutorEducation, TutorEducationDto>().ReverseMap();


			// TutorProfile mappings
			// ========== PROFILE ==========
			CreateMap<TutorProfile, TutorProfileDto>()
				.ForMember(d => d.TutorAvailabilities,
					opt => opt.MapFrom(s => s.TutorAvailabilities != null
						? s.TutorAvailabilities.Select(a => new TutorAvailabilityDto
						{
							Id = a.Id,
							Slot = a.Slot != null
								? new TimeSlotDto { Id = a.Slot.Id, StartTime = a.Slot.StartTime,EndTime = a.Slot.EndTime }
								: null
						})
						: new List<TutorAvailabilityDto>()))

				.ForMember(d => d.TutorCertificates,
					opt => opt.MapFrom(s => s.TutorCertificates != null
						? s.TutorCertificates.Select(c => new TutorCertificateDto
						{
							Id = c.Id,
							CertificateUrl = c.CertificateUrl,
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
							Subject = ts.Subject != null
								? new SubjectDto { Id = ts.Subject.Id, SubjectName = ts.Subject.SubjectName }
								: null,
							Level = ts.Level != null
								? new LevelDto { Id = ts.Level.Id, Name = ts.Level.Name }
								: null
						})
						: new List<TutorSubjectDto>()));








			// TutorSubject mappings
			CreateMap<TutorSubject, TutorSubjectDto>().ReverseMap();
			CreateMap<TutorSubjectCreateRequest, TutorSubject>();
			CreateMap<TutorSubjectUpdateRequest, TutorSubject>();

			




			// User mappings
			CreateMap<User, UserDto>().ReverseMap();

			// Role mappings
			CreateMap<Role, RoleDto>().ReverseMap();

			// RefreshToken mappings
			CreateMap<RefreshToken, RefreshTokenDto>().ReverseMap();

			// UserProfile mappings
			CreateMap<UserProfile, UserProfileDto>().ReverseMap();

			// Province mappings
			CreateMap<Province, ProvinceDto>().ReverseMap();

			// SubDistrict mappings
			CreateMap<SubDistrict, SubDistrictDto>().ReverseMap();
		}
	}
}
