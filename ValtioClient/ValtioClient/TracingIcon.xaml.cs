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
        private String timeElapsed;
        private int donePercent;

        public TracingIcon()
        {
            InitializeComponent();

            tb = ValtioNotifyIcon;

            tb.ToolTipText = "Tracing...";
            sw = new Stopwatch();
            sw.Start();
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
            sw.Stop();
            TimeSpan ts = sw.Elapsed;
            timeElapsed = String.Format("{0:00}:{1:00}:{2:00}.{3:00}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10);
            double frac = ts.TotalSeconds / GlobalPref.getTraceLength();
            donePercent = (int)(frac * 100);
            tb.ToolTipText = "Tracing...\n" + "Elapsed: " + timeElapsed + "\nProgress: " + donePercent + "%";
            sw.Start();
        }
    }
}
