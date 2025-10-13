namespace EduMatch.BusinessLogicLayer.Requests
{
	public class SubjectUpdateRequest
	{
		public int Id { get; set; }
		public string SubjectName { get; set; } = null!;
	}
}
