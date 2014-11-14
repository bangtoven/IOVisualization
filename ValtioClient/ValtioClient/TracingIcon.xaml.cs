using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Diagnostics;
using Hardcodet.Wpf.TaskbarNotification;

namespace ValtioClient
{
    /// <summary>
    /// Interaction logic for TracingIcon.xaml
    /// </summary>
    public partial class TracingIcon
    {
        private TaskbarIcon tb;
        private Stopwatch sw;
        private TimeSpan ts;
        BackgroundWorker bw = new BackgroundWorker();

        public TracingIcon()
        {
            InitializeComponent();

            // Set up tray icon
            tb = ValtioNotifyIcon;
            tb.ToolTipText = "Tracing...\nClick to see progress";

            // Initialize stopwatch
            sw = new Stopwatch();

            // Use background worker thread to track elapsed time
            bw.DoWork += new DoWorkEventHandler(TrackTime);
            bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(StopTrace);
            bw.RunWorkerAsync();
        }

        private void TrackTime(object sender, DoWorkEventArgs e)
        {
            sw.Start();

            while(true)
            {
                ts = sw.Elapsed;
                GlobalPref.timeElapsed = String.Format("{0:00}:{1:00}:{2:00}.{3:00}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10);
                double frac = ts.TotalSeconds / GlobalPref.getTraceLength();
                GlobalPref.donePercent = (int)(frac * 100);
                
                Thread.Sleep(1);

                if (ts.TotalSeconds >= GlobalPref.getTraceLength())
                    break;
            }

            sw.Stop();
        }

        private void StopTrace(object sender, RunWorkerCompletedEventArgs e)
        {
            Window MainWindow = new MainWindow();
            MainWindow.Show();
            tb.Dispose();
            this.Close();
        }

        private void stopTrace_Click(object sender, RoutedEventArgs e)
        {
            Window MainWindow = new MainWindow();
            MainWindow.Show();
            tb.Dispose();
            this.Close();
        }

        private void quitTrace_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void ValtioNotifyIcon_TrayToolTipOpen(object sender, RoutedEventArgs e)
        {
            tb.ToolTipText = "Tracing...\n" + "Elapsed: " + GlobalPref.timeElapsed + "\nProgress: " + GlobalPref.donePercent + "%";
        }

        private void ValtioNotifyIcon_TrayLeftMouseUp(object sender, RoutedEventArgs e)
        {
            // TODO: show notify box
        }
    }
}
