using IotProject.RazorShared.Models.Sensors;

namespace IotProject.RazorShared.Models.Devices
{
    public abstract class IotDevice
    {
        public string Name { get; protected set; }
        public string Data { get; set; } = string.Empty;

        public IotDevice(string name)
        {
            Name = name;
        }

        public abstract List<IotSensor> GetSensors();
    }
}
