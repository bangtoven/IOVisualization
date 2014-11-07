using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ValtioClient
{
    /// <summary>
    /// Interaction logic for EnvSetup.xaml
    /// </summary>
    public partial class EnvSetup : Window
    {
        String device;
        String _traceLength;
        String _timeWindow;
        String _blockUnit;
        List<String> items;

        public EnvSetup()
        {
            InitializeComponent();

            items = new List<String>();
            items.Add("Element 1");
            items.Add("Element 2");
            items.Add("Element 3");
            deviceList.ItemsSource = items;
        }

        private void traceBtn_Click(object sender, RoutedEventArgs e)
        {
            _traceLength = traceLength.Text;
            _timeWindow = timeWindow.Text;
            _blockUnit = blockUnit.Text;
            MessageBox.Show("Device: " + device + "\nTrace length: " + _traceLength + "\nTime window: " + _timeWindow + "\nBlock unit: " + _blockUnit);
            Window TracingIcon = new TracingIcon();
            this.WindowStyle = WindowStyle.SingleBorderWindow;
            this.WindowState = WindowState.Minimized;
            TracingIcon.Show();
        }

        private void deviceList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            device = deviceList.SelectedItem.ToString();
            MessageBox.Show(device);
        }
    }
}
