using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;

namespace ble.Models
{
    public class BleDevice
    {
        public BleDevice(string Id)
        {
            Task.Run(() => SetDevice(Id)).Wait();
        }
        private async Task SetDevice(string Id)
        {
            BluetoothLeDevice = await BluetoothLEDevice.FromIdAsync(Id);
        }
        private BluetoothLEDevice _dev;
        public BluetoothLEDevice BluetoothLeDevice
        {
            get
            {
                return _dev;
            }
            set
            {
                _dev = value;
                Task.Run(() => MapServices()).Wait();
            }
        }
        private async Task MapServices()
        {
            Services.Clear();
            GattDeviceServicesResult result = await BluetoothLeDevice.GetGattServicesAsync();
            if (result.Status == GattCommunicationStatus.Success)
            {
                var services = result.Services;
                foreach (var service in services)
                {
                    Services.Add(
                        new BleService
                        {
                            Service = service
                        });
                }
            }
        }
        public IList<BleService> Services { get; set; } = new List<BleService>();
    }
}
