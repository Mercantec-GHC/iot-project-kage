namespace IotProject.Shared.Models.Database
{
    public class DeviceData
    {
        public int Id { get; set; }
        public DateTime Timestamp { get; set; }
        public string DeviceId { get; set; }
        public Device Device { get; set; }

        public Dictionary<string, object> Data { get; set; } = new();
    }
}
