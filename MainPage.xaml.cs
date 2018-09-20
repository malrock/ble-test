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
using System.Collections.ObjectModel;
using Windows.UI.Core;
// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace ble
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        /// <summary>
        /// Data binding object
        /// </summary>
        public ObservableCollection<string> Log { get; private set; } = new ObservableCollection<string>();
        /// <summary>
        /// Private instance of lib
        /// </summary>
        private NewLib dev;
        /// <summary>
        /// Instance of NLog logger.
        /// </summary>
        private ILogger log = LogManager.GetLogger("BLE");
        /// <summary>
        /// Main constructor
        /// </summary>
        public MainPage()
        {
            this.InitializeComponent();
            // set instance
            dev = new NewLib();
            // bind event handler
            dev.HaveData += Read_data;
            // run detection and notification
            dev.Start();
        }
        /// <summary>
        /// Event handler to read BLE UART data
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Read_data(object sender, string e)
        {
            // data arrives here, if you attach debugger you can capture it.
            Task.Run(() => Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
              {
                  // add value to datasource for visualization
                  Log.Add(e);
              }));
            // Log data
            log.Debug("Got data {data}", e);
        }
    }
}
