using IotProject.RazorShared.Models.Devices;

namespace IotProject.RazorShared.Services
{
    public interface IDeviceService
    {
        public List<IotDevice> GetDevices();
    }
}
