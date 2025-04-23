namespace IotProject.Shared.Models.Database
{
    public class DeviceData : Base
    {
        public string DeviceId { get; set; }
        public Device Device { get; set; }
    }
}
