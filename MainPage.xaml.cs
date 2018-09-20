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


using System.Threading.Tasks;
using System.Text;
using System.ComponentModel;
using NLog;
// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace ble
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public List<string> Log { get; private set; } = new List<string>();
        private NewLib dev;
        private ILogger log = LogManager.GetLogger("BLE");
        public MainPage()
        {
            this.InitializeComponent();
            dev = new NewLib();
            dev.HaveData += Read_data;
            dev.Start();
        }

        private void Read_data(object sender, string e)
        {
            // data arrives here, if you attach debugger you can capture it.
            Log.Add(e);
            log.Debug("Got data {data}", e);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Log)));
        }
    }
}
