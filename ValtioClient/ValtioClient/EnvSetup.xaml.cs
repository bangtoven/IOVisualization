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
        String device = null;
        int _traceLength;
        int _timeWindow;
        int _blockUnit;
        List<String> items = null;

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
            Boolean exOccured = false;
            // Check if device was selected
            if (device == null)
            {
                MessageBox.Show("Please select target device.");
                exOccured = true;
            }

            // Store trace length
            try
            {
                _traceLength = Convert.ToInt32(traceLength.Text);
            }
            catch (FormatException)
            {
                MessageBox.Show("Please input trace length in integer form.");
                exOccured = true;
            }
            catch (OverflowException)
            {
                MessageBox.Show("Integer overflow: please input shorter trace length");
                exOccured = true;
            }

            // Store time window
            try
            {
                _timeWindow = Convert.ToInt32(timeWindow.Text);
            }
            catch (FormatException)
            {
                MessageBox.Show("Please input time window in integer form.");
                exOccured = true;
            }
            catch (OverflowException)
            {
                MessageBox.Show("Integer overflow: please input shorter time window");
                exOccured = true;
            }

            // Store block unit
            try
            {
                _blockUnit = Convert.ToInt32(blockUnit.Text);
            }
            catch (FormatException)
            {
                MessageBox.Show("Please input block unit in integer form.");
                exOccured = true;
            }
            catch (OverflowException)
            {
                MessageBox.Show("Integer overflow: please input shorter block unit");
                exOccured = true;
            }

            if (!exOccured)
            {
                // Store above data as global preference
                GlobalPref.setDeviceID(device);
                GlobalPref.setTraceLength(_traceLength);
                GlobalPref.setTimeWindow(_timeWindow);
                GlobalPref.setBlockUnit(_blockUnit);

                MessageBox.Show("Device: " + device + "\nTrace length: " + _traceLength + "\nTime window: " + _timeWindow + "\nBlock unit: " + _blockUnit); // DEBUG
                Window TracingIcon = new TracingIcon();
                TracingIcon.Show();
                this.Close();
            }
        }

        private void deviceList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            device = deviceList.SelectedItem.ToString();
            MessageBox.Show(device);
        }
    }
}
