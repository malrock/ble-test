using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Storage.Streams;

namespace ble.Models
{
    public class BleCharacteristic
    {
        public GattCharacteristic Characteristic { get; set; }
        public bool CanRead
        {
            get
            {
                return Characteristic.CharacteristicProperties.HasFlag(GattCharacteristicProperties.Read);
            }
        }
        public bool CanWrite
        {
            get
            {
                return Characteristic.CharacteristicProperties.HasFlag(GattCharacteristicProperties.Write);
            }
        }
        public bool CanNotify
        {
            get
            {
                return Characteristic.CharacteristicProperties.HasFlag(GattCharacteristicProperties.Notify);
            }
        }
        public async Task<byte[]> Read()
        {
            if (!CanRead) return null;
            GattReadResult result = await Characteristic.ReadValueAsync();
            if (result.Status == GattCommunicationStatus.Success)
            {
                var reader = DataReader.FromBuffer(result.Value);
                byte[] input = new byte[reader.UnconsumedBufferLength];
                reader.ReadBytes(input);
                return input;
            }
            return null;
        }
        public async Task<bool> Write(byte[] data)
        {
            var writer = new DataWriter();
            writer.WriteBytes(data);
            var result = await Characteristic.WriteValueAsync(writer.DetachBuffer());
            return result.HasFlag(GattCommunicationStatus.Success);
        }
        //void Characteristic_ValueChanged(GattCharacteristic sender,
        //                            GattValueChangedEventArgs args)
        //{
        //    // An Indicate or Notify reported that the value has changed.
        //    var reader = DataReader.FromBuffer(args.CharacteristicValue);
        //    byte[] input = new byte[reader.UnconsumedBufferLength];
        //    reader.ReadBytes(input);
        //    _log.AppendLine($"Got data {BitConverter.ToString(input)}");
        //    // Parse the data however required.
        //}
    }
}
