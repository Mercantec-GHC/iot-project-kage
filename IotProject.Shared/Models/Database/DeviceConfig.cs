namespace IotProject.Shared.Models.Database
{
	public class DeviceConfig
	{
		public int Id { get; set; }
		public string DeviceId { get; set; }
		public Device Device { get; set; }
		public Dictionary<string, object> Config { get; set; }
		public long Timestamp { get; set; }
	}
}