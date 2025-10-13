namespace EduMatch.BusinessLogicLayer.Requests
{
	public class TimeSlotUpdateRequest
	{
		public int Id { get; set; }
		public TimeOnly StartTime { get; set; }
		public TimeOnly EndTime { get; set; }
	}
}
