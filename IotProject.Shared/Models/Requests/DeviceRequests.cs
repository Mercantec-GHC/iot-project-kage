namespace IotProject.Shared.Models.Requests
{
    public class DeviceRegisterRequest
    {
        public string DeviceType { get; set; }
        public string OwnerId { get; set; }
        public string Config { get; set; }
    }
}
