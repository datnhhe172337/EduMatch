using AutoMapper;
using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Interfaces;
using EduMatch.BusinessLogicLayer.Mappings;
using EduMatch.BusinessLogicLayer.Services;
using EduMatch.DataAccessLayer.Entities;
using EduMatch.DataAccessLayer.Interfaces;
using EduMatch.Tests.Common;
using Moq;

namespace EduMatch.Tests;

public class TutorProfileServiceTests
{

	private Mock<ITutorProfileRepository> _repositoryMock;
	private Mock<ICloudMediaService> _cloudMediaMock;
	private Mock<IUserService> _userServiceMock;
	private Mock<IUserProfileService> _userProfileServiceMock;
	private IMapper _mapper;
	private CurrentUserService _currentUserService;
	private TutorProfileService _service;

	[SetUp]
	public void Setup()
	{
		_repositoryMock = new Mock<ITutorProfileRepository>();
		_cloudMediaMock = new Mock<ICloudMediaService>();
		_userServiceMock = new Mock<IUserService>();
		_userProfileServiceMock = new Mock<IUserProfileService>();

		var config = new MapperConfiguration(cfg =>
		{
			cfg.AddProfile<MappingProfile>();
		});
		_mapper = config.CreateMapper();

		_currentUserService = new CurrentUserServiceFake("abc@gmail.com");



		_service = new TutorProfileService(
			_repositoryMock.Object,
			_mapper,
			_cloudMediaMock.Object,
			_currentUserService,
			_userServiceMock.Object,
			_userProfileServiceMock.Object
		);

	}


	[Test]
	public async Task GetByEmailFullAsync_WhenExists_ReturnsMappedDto()
	{
		var email = "abc@gmail.com";
		var fakeTutor = FakeDataFactory.CreateFakeTutorProfile(email);

		_repositoryMock
			.Setup(r => r.GetByEmailFullAsync(email))
			.ReturnsAsync(fakeTutor);

		var result = await _service.GetByEmailFullAsync(email);

		Assert.That(result, Is.Not.Null);
		Assert.That(result.UserEmail, Is.EqualTo(email));
		Assert.That(result.Province.Name, Is.EqualTo("Hà Nội"));
		Assert.That(result.TutorSubjects.First().Subject.SubjectName, Is.EqualTo("Math"));
	}




}
