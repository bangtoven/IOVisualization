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
    public class Request
    {
        public UInt64 st_addr; // Starting address
        public UInt64 ed_addr; // Ending address
        public bool rw; // 0: Read, 1: Write
        public UInt64 lat; // Latency

        public Request(UInt64 st_addr, UInt64 ed_addr, bool rw, UInt64 lat) {
            this.st_addr = st_addr;
            this.ed_addr = ed_addr;
            this.rw = rw;
            this.lat = lat;
        }
    }

    public class ProcessInfo
    {
        public int pid;
        public List<LinkedList<Request>> time_windows;
        public int cur_window; // Increases from 0

        public ProcessInfo()
        {
            this.pid = -1;
            this.time_windows = new List<LinkedList<Request>>();
            LinkedList<Request> temp = new LinkedList<Request>();
            this.time_windows.Add(temp);
            this.cur_window = 0;
        }

        public ProcessInfo(int pid)
        {
            this.pid = pid;
            this.time_windows = new List<LinkedList<Request>>();
            LinkedList<Request> temp = new LinkedList<Request>();
            this.time_windows.Add(temp);
            this.cur_window = 0;
        }

        public void addRequest(Request req, int time_window)
        {
            // Given time window can only be either equal to or 1 greater than current time window
            if (time_window == this.cur_window)
            {
                // Add request to the end of the current time window
                this.time_windows[cur_window].AddLast(req);
            }
            else if (time_window == this.cur_window + 1)
            {
                // Make new LinkedList with the given request and add it to the time window list
                LinkedList<Request> temp = new LinkedList<Request>();
                temp.AddLast(req);
                this.time_windows.Add(temp);
                this.cur_window = time_window; // Increment current time window
            }
            else
            {
                // Invalid time window
                throw new IndexOutOfRangeException();
            }
        }
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void processList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
