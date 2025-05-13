namespace IotProject.Shared.Utilities
{
    public record DeviceType(string Name);

    public static class DeviceTypes
    {
        private static readonly Dictionary<string, DeviceType> DeviceTypeMappings = new Dictionary<string, DeviceType>(StringComparer.OrdinalIgnoreCase)
        {
            { "DemoDevice", new DeviceType("Arduino Demo Device") }
        };

        public static DeviceType? GetDeviceType(string type)
        {
            return DeviceTypeMappings.TryGetValue(type, out var deviceType) ? deviceType : null;
        }
    }
}
