using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Enumeration;
using Windows.Storage.Streams;

namespace ble
{
    public class NewLib
    {
        // Query for extra properties you want returned
        readonly string[] requestedProperties = { };
        readonly bool paired = true;
        public readonly Guid UARTServiceId = Guid.Parse("6E400001-B5A3-F393-E0A9-E50E24DCCA9E");
        public readonly Guid TXCharacteristicsId = Guid.Parse("6E400002-B5A3-F393-E0A9-E50E24DCCA9E");
        public readonly Guid RXCharacteristicsId = Guid.Parse("6E400003-B5A3-F393-E0A9-E50E24DCCA9E");
        DeviceWatcher deviceWatcher;
        readonly string BleDevice = "BLEtest";
        public string BleId { get; set; }
        public BluetoothLEDevice BluetoothLeDevice { get; private set; }
        public GattDeviceService Service { get; private set; }
        public GattCharacteristic Characteristic { get; private set; }
        public EventHandler<string> HaveData;
        public void  Start()
        {
            deviceWatcher =
            DeviceInformation.CreateWatcher(
                    BluetoothLEDevice.GetDeviceSelectorFromPairingState(paired),
                    requestedProperties,
                    DeviceInformationKind.AssociationEndpoint);

            // Register event handlers before starting the watcher.
            // Added, Updated and Removed are required to get all nearby devices
            deviceWatcher.Added += DeviceWatcher_Added;
            deviceWatcher.Updated += DeviceWatcher_Updated;
            deviceWatcher.Removed += DeviceWatcher_Removed;

            // EnumerationCompleted and Stopped are optional to implement.
            deviceWatcher.EnumerationCompleted += DeviceWatcher_EnumerationCompleted;
            deviceWatcher.Stopped += DeviceWatcher_Stopped;

            // Start the watcher.
            deviceWatcher.Start();
        }

        private void DeviceWatcher_Stopped(DeviceWatcher sender, object args)
        {

        }

        private void DeviceWatcher_EnumerationCompleted(DeviceWatcher sender, object args)
        {

        }

        private void DeviceWatcher_Updated(DeviceWatcher sender, DeviceInformationUpdate args)
        {

        }

        private void DeviceWatcher_Removed(DeviceWatcher sender, DeviceInformationUpdate args)
        {

        }

        private void DeviceWatcher_Added(DeviceWatcher sender, DeviceInformation args)
        {
            HaveData?.Invoke(this,$"Found device {args.Name} with id {args.Id}");
            if (args.Name==BleDevice)
            {
                HaveData?.Invoke(this, "Match, linking.");
                BleId = args.Id;
                Task.Run(() => ConnectDevice(args.Id));
            }
            
        }
        async void ConnectDevice(string Id)
        {
            // Note: BluetoothLEDevice.FromIdAsync must be called from a UI thread because it may prompt for consent.
            HaveData?.Invoke(this, "Connecting...");
            BluetoothLeDevice = await BluetoothLEDevice.FromIdAsync(Id);
            HaveData?.Invoke(this, "Reading service");
            var res = await BluetoothLeDevice.GetGattServicesForUuidAsync(UARTServiceId);
            if(res.Status== GattCommunicationStatus.Success)
            {
                Service = res.Services.FirstOrDefault();
            } else
            {
                HaveData?.Invoke(this, "service denied.");
                return;
            }
            HaveData?.Invoke(this, "Connecting to characteristic");
            GattCharacteristicsResult cres =await Service.GetCharacteristicsForUuidAsync(RXCharacteristicsId);
            if (cres.Status == GattCommunicationStatus.Success)
            {
                Characteristic = cres.Characteristics.FirstOrDefault();
            } else
            {
                HaveData?.Invoke(this, "Reading characteristic failed");
            }
            Characteristic.ValueChanged += Characteristic_ValueChanged;
            HaveData?.Invoke(this, "Subscribing for notification");
            GattCommunicationStatus status = await Characteristic.WriteClientCharacteristicConfigurationDescriptorAsync(
                        GattClientCharacteristicConfigurationDescriptorValue.Notify);
            if (status == GattCommunicationStatus.Success)
            {
                // Server has been informed of clients interest.
                HaveData?.Invoke(this, "Subscribed.");
            } else
            {
                HaveData?.Invoke(this, "Subsrcription failed");
            }
        }
        void Characteristic_ValueChanged(GattCharacteristic sender,
                                    GattValueChangedEventArgs args)
        {
            // An Indicate or Notify reported that the value has changed.
            var reader = DataReader.FromBuffer(args.CharacteristicValue);
            // Parse the data however required.
            string res = reader.ReadString(reader.UnconsumedBufferLength);
            var plainTextBytes = Encoding.UTF8.GetBytes(res);
            var outString = Convert.ToBase64String(plainTextBytes);
            HaveData?.Invoke(this, outString);
        }
    }
}
