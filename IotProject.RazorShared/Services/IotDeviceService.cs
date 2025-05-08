using IotProject.RazorShared.Models.Devices;

namespace IotProject.RazorShared.Services
{
    public class IotDeviceService : IDeviceService
    {
        public List<IotDevice> GetDevices()
        {
            var deviceList = new List<IotDevice>();
            deviceList.Add(new DemoDevice("Demo Device"));
            return deviceList;
        }
    }
}
