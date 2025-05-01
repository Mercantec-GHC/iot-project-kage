using IotProject.RazorShared.Models.Devices;

namespace IotProject.RazorShared.Models.Sensors
{
    public class IotSensor
    {
        public IotDevice Device { get; set; }
        public string Name { get; set; }
    }
}
