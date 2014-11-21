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
    public partial class TracingIcon : INotifyPropertyChanged
    {
        private TaskbarIcon tb;
        private Stopwatch sw;
        private TimeSpan ts;
        BackgroundWorker bw = new BackgroundWorker();
        private int tl = GlobalPref.getTraceLength();
        private int tl_hours = 0;
        private int tl_minutes = 0;
        private int tl_seconds = 0;

        // Used for data binding
        private string _info;
        public string Info
        {
            get { return _info; }
            set
            {
                _info = value;
                RaisePropertyChanged("Info");
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged(string p)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(p));
            }
        }

        public TracingIcon()
        {
            // Save trace length
            tl_hours = tl / 3600;
            tl_minutes = (tl - tl_hours * 3600) / 60;
            tl_seconds = tl - tl_hours * 3600 - tl_minutes * 60;

            // Set DataContext for data binding
            Info = "";
            DataContext = this;

            InitializeComponent();

            // DEBUG - set trace length to 30 seconds
            GlobalPref.setTraceLength(30);

            // Set up tray icon
            tb = ValtioNotifyIcon;

            // Initialize stopwatch
            sw = new Stopwatch();

            // Use background worker thread to track elapsed time
            bw.DoWork += new DoWorkEventHandler(TrackTime);
            bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(StopTrace);
            bw.RunWorkerAsync();
        }

        // Keeps track of time
        private void TrackTime(object sender, DoWorkEventArgs e)
        {
            sw.Start();

            while(true)
            {
                ts = sw.Elapsed;
                GlobalPref.timeElapsed = String.Format("{0:00}:{1:00}:{2:00}.{3:00}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10);
                double frac = ts.TotalSeconds / tl;
                GlobalPref.donePercent = (int)(frac * 100);
                Info = "Tracing...\nTrace Length: " + String.Format("{0:00}:{1:00}:{2:00}:{3:00}", tl_hours, tl_minutes, tl_seconds, 0) + "\nElapsed: " + GlobalPref.timeElapsed + "\nProgress: " + GlobalPref.donePercent + "%";
                
                Thread.Sleep(1);

                if (ts.TotalSeconds >= GlobalPref.getTraceLength())
                    break;
            }

            sw.Stop();
        }

        // Called when background worker is complete
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
    }
}
