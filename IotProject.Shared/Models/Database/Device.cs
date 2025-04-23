namespace IotProject.Shared.Models.Database
{
    public class Device : Base
    {
        public string OwnerId { get; set; }
        public User Owner { get; set; }
        public string? RoomId { get; set; }
        public Room? Room { get; set; }
        public string DeviceType { get; set; }
        public ICollection<DeviceData> Data { get; set; } = new List<DeviceData>();
        public string ApiKey { get; set; }
        public string Config { get; set; }
        
    }
}
