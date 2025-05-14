using Android;

namespace IotProject.Mobile
{
    public static class BluetoothPermisions
    {
        public static async Task<bool> CheckAndRequestPermissions()
        {
            // Tjek BLUETOOTH_SCAN tilladelse
            var scanStatus = await Permissions.CheckStatusAsync<BluetoothScanPermission>();
            if (scanStatus != PermissionStatus.Granted)
            {
                scanStatus = await Permissions.RequestAsync<BluetoothScanPermission>();
                if (scanStatus != PermissionStatus.Granted)
                {
                    Console.WriteLine("Bluetooth Scan permission blev ikke givet.");
                    return false;
                }
            }

            // Tjek BLUETOOTH_CONNECT tilladelse
            var connectStatus = await Permissions.CheckStatusAsync<BluetoothConnectPermission>();
            if (connectStatus != PermissionStatus.Granted)
            {
                connectStatus = await Permissions.RequestAsync<BluetoothConnectPermission>();
                if (connectStatus != PermissionStatus.Granted)
                {
                    Console.WriteLine("Bluetooth Connect permission blev ikke givet.");
                    return false;
                }
            }

            return true;
        }
    }

    /// <summary>
    /// Custom permission til at anmode om BLUETOOTH_SCAN (Android 12+).
    /// </summary>
    public class BluetoothScanPermission : Permissions.BasePlatformPermission
    {
        public override (string androidPermission, bool isRuntime)[] RequiredPermissions =>
            new[]
            {
                (Manifest.Permission.BluetoothScan, true)
            };
    }

    /// <summary>
    /// Custom permission til at anmode om BLUETOOTH_CONNECT (Android 12+).
    /// </summary>
    public class BluetoothConnectPermission : Permissions.BasePlatformPermission
    {
        public override (string androidPermission, bool isRuntime)[] RequiredPermissions =>
            new[]
            {
                (Manifest.Permission.BluetoothConnect, true)
            };
    }

}
