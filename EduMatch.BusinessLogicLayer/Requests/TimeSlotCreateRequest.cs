namespace EduMatch.BusinessLogicLayer.Requests
{
	public class TimeSlotCreateRequest
	{
		public TimeOnly StartTime { get; set; }
		public TimeOnly EndTime { get; set; }
	}
}
