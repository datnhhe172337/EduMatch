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
				.ForMember(dest => dest.Subjects, opt => opt.MapFrom(src => src.CertificateTypeSubjects.Select(cts => cts.Subject)));

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
			CreateMap<Subject, SubjectDto>().ReverseMap();
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
			CreateMap<TutorProfile, TutorProfileDto>()
				.ForMember(d => d.TutorAvailabilitiesId,
					opt => opt.MapFrom(s => s.TutorAvailabilities.Select(x => x.Id)))
				.ForMember(d => d.TutorCertificatesId,
					opt => opt.MapFrom(s => s.TutorCertificates.Select(x => x.Id)))
				.ForMember(d => d.TutorEducationsId,
					opt => opt.MapFrom(s => s.TutorEducations.Select(x => x.Id)))
				.ForMember(d => d.TutorSubjectId,
					opt => opt.MapFrom(s => s.TutorSubjects.Select(x => x.Id)));







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
