using Android;

namespace IotProject.Mobile
{
    public static class BluetoothPermisions
    {
        /// <summary>
        /// Checks and requests the necessary Bluetooth permissions for Android 12+.
        /// </summary>
        /// <returns>
        /// Returns <c>true</c> if all required Bluetooth permissions are granted; otherwise, <c>false</c>.
        /// </returns>
        public static async Task<bool> CheckAndRequestPermissions()
        {
            // Check BLUETOOTH_SCAN permission (Android 12+)
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

            // Check BLUETOOTH_CONNECT permission (Android 12+)
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
    /// Custom permission to request BLUETOOTH_SCAN (Android 12+).
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
    /// Custom permission to request BLUETOOTH_CONNECT (Android 12+).
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
