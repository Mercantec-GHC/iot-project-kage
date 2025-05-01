using System.Text.Json;
using IotProject.RazorShared.Models.Sensors;

namespace IotProject.RazorShared.Models.Devices
{
    public class DemoDevice : IotDevice
    {
        public string DemoData = @"
        {
            ""temperature"": {
                ""celcius"": 20,
                ""fahrenheit"": 68
            },
            ""humidity"": {
                ""percentage"": 81
            }
        }";

        public DemoDevice(string name) : base(name) { }

        public override List<IotSensor> GetSensors()
        {
            var sensorList = new List<IotSensor>();

            using JsonDocument doc = JsonDocument.Parse(DemoData);
            JsonElement root = doc.RootElement;

            if (root.TryGetProperty("temperature", out JsonElement tempElement))
            {
                var temperatureSensor = JsonSerializer.Deserialize<TemperatureSensor>(tempElement);
                temperatureSensor!.Name = "Temperature";
                sensorList.Add(temperatureSensor!);
                Console.WriteLine($"Temp: {temperatureSensor!.Celcius} °C / {temperatureSensor.Fahrenheit} °F");
            }

            //if (root.TryGetProperty("humidity", out JsonElement humidityElement))
            //{
            //    var humiditySensor = JsonSerializer.Deserialize<HumiditySensor>(humidityElement);
            //    Console.WriteLine($"Humidity: {humiditySensor.Percentage}%");
            //}

            return sensorList;
        }
    }
}
