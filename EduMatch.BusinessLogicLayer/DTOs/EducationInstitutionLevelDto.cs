namespace EduMatch.BusinessLogicLayer.DTOs
{
	public class EducationInstitutionLevelDto
	{
		public int Id { get; set; }
		public int InstitutionId { get; set; }
		public int EducationLevelId { get; set; }
		public EducationInstitutionDto? Institution { get; set; }
		public EducationLevelDto? EducationLevel { get; set; }
	}
}
