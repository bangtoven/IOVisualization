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
    public partial class EnvSetup
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
            items.Add("Element 4");
            items.Add("Element 5");
            items.Add("Element 6");
            items.Add("Element 7");
            items.Add("Element 8");
            items.Add("Element 9");
            items.Add("Element 10");
            deviceList.ItemsSource = items;
        }

        private void traceBtn_Click(object sender, RoutedEventArgs e)
        {
            Boolean exOccured = false;
            // Check if device was selected
            if (device == null)
            {
                GlobalFunc.ShowMessageBox("Error", "Please select target device.");
                exOccured = true;
            }

            // Store trace length
            try
            {
                _traceLength = (Convert.ToInt32(traceLengthHour.Text) * 60 + Convert.ToInt32(traceLengthMin.Text)) * 60;
            }
            catch (FormatException)
            {
                GlobalFunc.ShowMessageBox("Error", "Please input trace length in integer form.");
                exOccured = true;
            }
            catch (OverflowException)
            {
                GlobalFunc.ShowMessageBox("Error", "Integer overflow: please input shorter trace length.");
                exOccured = true;
            }

            // Store time window
            try
            {
                _timeWindow = Convert.ToInt32(timeWindow.Text);
            }
            catch (FormatException)
            {
                GlobalFunc.ShowMessageBox("Error", "Please input time window in integer form.");
                exOccured = true;
            }
            catch (OverflowException)
            {
                GlobalFunc.ShowMessageBox("Error", "Integer overflow: please input shorter time window.");
                exOccured = true;
            }

            // Store block unit
            try
            {
                _blockUnit = Convert.ToInt32(blockUnit.Text);
            }
            catch (FormatException)
            {
                GlobalFunc.ShowMessageBox("Error", "Please input block unit in integer form.");
                exOccured = true;
            }
            catch (OverflowException)
            {
                GlobalFunc.ShowMessageBox("Error", "Integer overflow: please input smaller block unit.");
                exOccured = true;
            }

            if (!exOccured)
            {
                // Store above data as global preference
                GlobalPref.setDeviceID(device);
                GlobalPref.setTraceLength(_traceLength);
                GlobalPref.setTimeWindow(_timeWindow);
                GlobalPref.setBlockUnit(_blockUnit);

                // Confirm and start tracing
                GlobalFunc.ShowConfirmMessageBox("Confirm", "Are you sure you want to proceed?", ShowTray);
            }
        }

        private void deviceList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (deviceList.SelectedItem == null)
            {
                device = null;
            }
            else
            {
                device = deviceList.SelectedItem.ToString();
            }
        }

        private void ShowTray()
        {
            // Show tray icon
            Window TracingIcon = new TracingIcon();
            TracingIcon.Show();
            this.Close();
        }
    }
}
