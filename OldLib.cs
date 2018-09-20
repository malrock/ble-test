using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Storage.Streams;

namespace ble
{
    public class OldLib
    {
        // here are id's of device and services, may need to be updated based on OS values.
        public const string BLEDeviceID = "BluetoothLE#BluetoothLE5c:f3:70:8d:05:85-c5:12:07:48:ff:8f";
        public Guid UARTServiceId = Guid.Parse("6E400001-B5A3-F393-E0A9-E50E24DCCA9E");
        public Guid TXCharacteristicsId = Guid.Parse("6E400002-B5A3-F393-E0A9-E50E24DCCA9E");
        public Guid RXCharacteristicsId = Guid.Parse("6E400003-B5A3-F393-E0A9-E50E24DCCA9E");

        BluetoothLEDevice BluetoothLeDevice { get; set; }
        GattCharacteristic TX;
        GattCharacteristic RX;
        GattDeviceService UART;
        public StringBuilder _log = new StringBuilder();
        // Call it first.
        public void Init()
        {
            Task.Run(() => InitDevice(BLEDeviceID)).Wait();
            Task.Run(() => GetServices()).Wait();
        }
        public async Task InitDevice(string id)
        {
            BluetoothLeDevice = await BluetoothLEDevice.FromIdAsync(id);
            _log.AppendLine($"Connected to {BluetoothLeDevice?.Name}");
        }
        public async Task GetServices()
        {
            GattDeviceServicesResult result = await BluetoothLeDevice.GetGattServicesAsync();

            if (result.Status == GattCommunicationStatus.Success)
            {
                var services = result.Services;
                foreach (var service in services)
                {
                    _log.Append($"Found service {service.Uuid} comparing to {UARTServiceId}");
                    if (service.Uuid != UARTServiceId)
                    {
                        _log.AppendLine(" no match, ignoring.");
                        continue;
                    }
                    UART = service;
                    _log.AppendLine(" is a match, polling characteristics.");
                    GattCharacteristicsResult res = await service.GetCharacteristicsAsync();

                    if (res.Status == GattCommunicationStatus.Success)
                    {
                        var characteristics = res.Characteristics;
                        foreach (var characteristic in characteristics)
                        {
                            var c = characteristic.Uuid;
                            _log.Append($"Found characteristics {c}");

                            if (!c.Equals(TXCharacteristicsId) && !c.Equals(RXCharacteristicsId))
                            {
                                _log.AppendLine(" no match, ignoring.");
                                continue;
                            }
                            if (c.Equals(TXCharacteristicsId))
                            {
                                TX = characteristic;
                            }
                            else
                            {
                                RX = characteristic;
                            }
                            _log.AppendLine(" is a match, polling properties.");
                            GattCharacteristicProperties properties = characteristic.CharacteristicProperties;
                            if (properties.HasFlag(GattCharacteristicProperties.Read))
                            {
                                _log.AppendLine("Can read");
                                await Read(characteristic);
                            }
                            if (properties.HasFlag(GattCharacteristicProperties.Write))
                            {
                                _log.AppendLine("Can write");
                            }
                            if (properties.HasFlag(GattCharacteristicProperties.Notify))
                            {
                                _log.AppendLine("Can notify");
                                GattCommunicationStatus status = await characteristic.WriteClientCharacteristicConfigurationDescriptorAsync(
                                GattClientCharacteristicConfigurationDescriptorValue.Notify);
                                if (status == GattCommunicationStatus.Success)
                                {
                                    // Server has been informed of clients interest.
                                    (c.Equals(TXCharacteristicsId) ? TX : RX).ValueChanged += Characteristic_ValueChanged;
                                    _log.AppendLine("Subscribed to notification");
                                }
                                else
                                {
                                    _log.AppendLine("Subscription failed");
                                }
                            }
                            _log.AppendLine("Finished reading properties for characteristic.");
                        }
                    }
                    else
                    {
                        _log.AppendLine($"Characterisitcs read failed. {res.Status}");
                    }
                    //break;
                }
            }
        }
        public async Task<byte[]> Read(GattCharacteristic selectedCharacteristic)
        {
            GattReadResult result = await selectedCharacteristic.ReadValueAsync();
            if (result.Status == GattCommunicationStatus.Success)
            {
                var reader = DataReader.FromBuffer(result.Value);
                byte[] input = new byte[reader.UnconsumedBufferLength];
                reader.ReadBytes(input);
                _log.AppendLine($"Got data {BitConverter.ToString(input)}");
                return input;
            }
            return null;
        }
        public async Task<bool> Write(GattCharacteristic selectedCharacteristic, byte[] data)
        {
            var writer = new DataWriter();
            // WriteByte used for simplicity. Other commmon functions - WriteInt16 and WriteSingle
            writer.WriteBytes(data);
            var result = await selectedCharacteristic.WriteValueAsync(writer.DetachBuffer());
            return result.HasFlag(GattCommunicationStatus.Success);
        }
        void Characteristic_ValueChanged(GattCharacteristic sender,
                                    GattValueChangedEventArgs args)
        {
            // An Indicate or Notify reported that the value has changed.
            var reader = DataReader.FromBuffer(args.CharacteristicValue);
            byte[] input = new byte[reader.UnconsumedBufferLength];
            reader.ReadBytes(input);
            _log.AppendLine($"Got data {BitConverter.ToString(input)}");
            // Parse the data however required.
        }
    }
}
