using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Storage.Streams;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using System.Threading.Tasks;
using System.Text;
// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace ble
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        // here are id's of device and services, may need to be updated based on OS values.
        public const string BLEDeviceID = "BluetoothLE#BluetoothLE5c:f3:70:8d:05:85-c5:12:07:48:ff:8f";
        public Guid UARTServiceId = Guid.Parse("6E400001-B5A3-F393-E0A9-E50E24DCCA9E");
        public Guid TXCharacteristicsId = Guid.Parse("6E400002-B5A3-F393-E0A9-E50E24DCCA9E");
        public Guid RXCharacteristicsId =Guid.Parse("6E400003-B5A3-F393-E0A9-E50E24DCCA9E");

        BluetoothLEDevice bluetoothLeDevice { get; set; }
        GattCharacteristic TX;
        GattCharacteristic RX;
        GattDeviceService UART;
        public StringBuilder _log = new StringBuilder();

        public MainPage()
        {
            this.InitializeComponent();
            Task.Run(() => InitDevice(BLEDeviceID)).Wait();
            Task.Run(() => GetServices()).Wait();
        }
        public async Task InitDevice(string id)
        {
            bluetoothLeDevice = await BluetoothLEDevice.FromIdAsync(id);
            _log.AppendLine($"Connected to {bluetoothLeDevice?.Name}");
        }
        public async Task GetServices()
        {
            GattDeviceServicesResult result = await bluetoothLeDevice.GetGattServicesAsync();

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
                            } else
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
                                    (c.Equals(TXCharacteristicsId)?TX:RX).ValueChanged += Characteristic_ValueChanged;
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
