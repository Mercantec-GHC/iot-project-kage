using IotProject.RazorShared.Models.Devices;

namespace IotProject.RazorShared.Models.Sensors
{
    public class TemperatureSensor : IotSensor
    {
        public double Celcius {  get; set; }
        public double Fahrenheit { get; set; }
    }
}
