namespace IotProject.Shared.Models.Database
{
    public class Room : Base
    {
        public string OwnerId { get; set; }
        public User Owner { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public ICollection<Device> Devices { get; set; } = new List<Device>();
    }
}