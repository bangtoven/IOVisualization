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

        public EnvSetup()
        {
            InitializeComponent();
        }

        private void traceBtn_Click(object sender, RoutedEventArgs e)
        {
            if (GlobalPref.debug)
            {
                GlobalPref.setDeviceID("/dev/sda");
                GlobalPref.setTraceLength(60);
                GlobalPref.setTimeWindow(2);
                GlobalPref.setBlockUnit(1000);

                ShowTray();
            }
            else
            {
                int errortype = 0;

                // Store target device
                if (errortype == 0)
                {
                    device = targetDevice.Text;

                    if (device.Length == 0)
                    {
                        errortype = 1;
                    }
                }

                // Store trace length
                if (errortype == 0)
                {
                    try
                    {
                        _traceLength = (Convert.ToInt32(traceLengthHour.Text) * 60 + Convert.ToInt32(traceLengthMin.Text)) * 60;
                    }
                    catch (FormatException)
                    {
                        errortype = 2;
                    }
                    catch (OverflowException)
                    {
                        errortype = 3;
                    }
                }

                // Store time window
                if (errortype == 0)
                {
                    try
                    {
                        _timeWindow = Convert.ToInt32(timeWindow.Text);
                    }
                    catch (FormatException)
                    {
                        errortype = 4;
                    }
                    catch (OverflowException)
                    {
                        errortype = 5;
                    }
                }

                // Store block unit
                if (errortype == 0)
                {
                    try
                    {
                        _blockUnit = Convert.ToInt32(blockUnit.Text);
                    }
                    catch (FormatException)
                    {
                        errortype = 6;
                    }
                    catch (OverflowException)
                    {
                        errortype = 7;
                    }
                }

                // Check for errors
                if (errortype == 1)
                {
                    GlobalFunc.ShowMessageBox("Error", "Please input target device.");
                }
                else if (errortype == 2)
                {
                    GlobalFunc.ShowMessageBox("Error", "Please input trace length in integer form.");
                }
                else if (errortype == 3)
                {
                    GlobalFunc.ShowMessageBox("Error", "Integer overflow: please input shorter trace length.");
                }
                else if (errortype == 4)
                {
                    GlobalFunc.ShowMessageBox("Error", "Please input time window in integer form.");
                }
                else if (errortype == 5)
                {
                    GlobalFunc.ShowMessageBox("Error", "Integer overflow: please input shorter time window.");
                }
                else if (errortype == 6)
                {
                    GlobalFunc.ShowMessageBox("Error", "Please input block unit in integer form.");
                }
                else if (errortype == 7)
                {
                    GlobalFunc.ShowMessageBox("Error", "Integer overflow: please input smaller block unit.");
                }
                else
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
