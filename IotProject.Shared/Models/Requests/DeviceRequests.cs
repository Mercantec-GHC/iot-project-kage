namespace IotProject.Shared.Models.Requests
{
    public class DeviceRegisterRequest
    {
        public string DeviceType { get; set; }
        public string OwnerId { get; set; }
        public string Config { get; set; }
    }
    public class DeviceRemoveRequest
    {
        public string Id { get; set; }
    }

    public class DeviceNameRequest
    {
        public string Id { get; set; }
        public string? Name { get; set; }
    }

    public class DeviceRoomRequest
    {
        public string Id { get; set; }
        public string? RoomId { get; set; }
    }
}
