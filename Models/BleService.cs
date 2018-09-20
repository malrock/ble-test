using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
namespace ble.Models
{
    public class BleService
    {
        private GattDeviceService _serv;
        public GattDeviceService Service
        {
            get
            {
                return _serv;
            }
            set
            {
                _serv = value;
                Task.Run(() => MapCharacteristics());
            }
        }
        public IList<BleCharacteristic> Characteristics { get; set; }
        private async Task MapCharacteristics()
        {
            Characteristics.Clear();
            GattCharacteristicsResult res = await Service.GetCharacteristicsAsync();
            if (res.Status == GattCommunicationStatus.Success)
            {
                var characteristics = res.Characteristics;
                foreach (var characteristic in characteristics)
                {
                    Characteristics.Add(
                        new BleCharacteristic
                        {
                            Characteristic = characteristic
                        });
                }
            }
        }
    }
}
