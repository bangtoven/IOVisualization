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
using OxyPlot;
using OxyPlot.Series;
using OxyPlot.Annotations;
using OxyPlot.Axes;
using System.ComponentModel;

namespace ValtioClient
{
    /// <summary>
    /// Interaction logic for Graph.xaml
    /// </summary>
    public partial class Graph : INotifyPropertyChanged
    {
        private static UInt64 blk_len = GlobalPref.maxBlock - GlobalPref.minBlock + 1;
        private static UInt64 bu = (UInt64)GlobalPref.getBlockUnit();
        private static int time_len = GlobalPref.getTraceLength() * 1000;
        private static int tu = GlobalPref.getTimeWindow();
        private static OxyColor ReadColor = OxyColors.Blue;
        private static OxyColor WriteColor = OxyColors.Red;

        private static UInt64 blk_cnt;
        private static int time_cnt;

        private static bool firstLoad = true;
        private static int whichGraph = 0;

        /* Data binding */
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged(string property)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(property));
            }
        }

        private Boolean loadedBool = true; // Used for OxyLoading
        public Boolean LoadedBool
        {
            get
            {
                return this.loadedBool;
            }
            set
            {
                this.loadedBool = value;
                this.RaisePropertyChanged("LoadedBool");
            }
        }

        private PlotModel plotModel; // Used for OxyPlot graph
        public PlotModel PlotModel
        {
            get
            {
                return this.plotModel;
            }
            set
            {
                this.plotModel = value;
                this.RaisePropertyChanged("PlotModel");
            }
        }

        /* Window constructor */
        public Graph()
        {
            // Count number of blocks
            if (blk_len % bu == 0)
            {
                blk_cnt = blk_len / bu;
            }
            else
            {
                blk_cnt = (blk_len / bu) + 1;
            }

            // Count number of time units (+1 because of last second)
            if (time_len % tu == 0)
            {
                time_cnt = time_len / tu + 1;
            }
            else
            {
                time_cnt = (time_len / tu) + 2;
            }

            // UI
            InitializeComponent();
            this.DataContext = this;
            CheckRadio();

            // Process Infos
            GlobalPref.requestCountList = GlobalPref.requestCount.ToList();
            GlobalPref.requestCountList.Sort(delegate(KeyValuePair<uint, int> pair1, KeyValuePair<uint, int> pair2)
            {
                return pair2.Value.CompareTo(pair1.Value);
            });

            for (int i = 0; i < GlobalPref.requestCountList.Count; i++)
            {
                uint pid = GlobalPref.requestCountList[i].Key;
                int count = GlobalPref.requestCountList[i].Value;
                processList.Items.Add(pid + " (" + count + ")");
                Console.WriteLine("pid: " + pid + " count: " + count); // Debug
            }
        }

        /* Button click methods */
        private void AddrFreq_Click(object sender, RoutedEventArgs e)
        {
            Preprocessing(1);
            this.PlotModel = this.AddrFreq(-1);
            Postprocessing(1);
        }

        private void TimeAddr_Click(object sender, RoutedEventArgs e)
        {
            Preprocessing(2);
            this.PlotModel = this.TimeAddr(-1);
            Postprocessing(2);
        }

        private void TimeFreq_Click(object sender, RoutedEventArgs e)
        {
            Preprocessing(3);
            this.PlotModel = this.TimeFreq(-1);
            Postprocessing(3);
        }

        private void LatFreq_Click(object sender, RoutedEventArgs e)
        {
            Preprocessing(4);
            this.PlotModel = this.LatFreq(-1);
            Postprocessing(4);
        }

        private void TimeLatAvg_Click(object sender, RoutedEventArgs e)
        {
            Preprocessing(5);
            this.PlotModel = this.TimeLatAvg(-1);
            Postprocessing(5);
        }

        private void Throughput_Click(object sender, RoutedEventArgs e)
        {
            Preprocessing(6);
            this.PlotModel = this.Throughput(-1);
            Postprocessing(6);
        }

        /* Preprocessing - use this to make something happen before each graph is called */
        private void Preprocessing(int type)
        {
            LoadedBool = false;
            whichGraph = type;
            allRB.IsChecked = true;
            CheckRadio();
        }

        /* Postprocessing - use this to make something happen after each graph is called */
        private void Postprocessing(int type)
        {
            LoadedBool = true;
        }

        /* Checks radio buttons */
        private void CheckRadio()
        {
            // Disable radio buttons either on startup or in LatFreq graph
            if (whichGraph == 0 || whichGraph == 4)
            {
                readRB.IsEnabled = false;
                writeRB.IsEnabled = false;
                allRB.IsEnabled = false;
            }
            // Enable radio buttons otherwise
            else
            {

                readRB.IsEnabled = true;
                writeRB.IsEnabled = true;
                allRB.IsEnabled = true;
            }
        }

        /* Address vs Frequency */
        private PlotModel AddrFreq(int pid)
        {
            var plotModel1 = new PlotModel();

            // Set title
            plotModel1.Title = "Address vs Frequency";

            // Set axes
            var linearAxis1 = new LinearAxis();
            linearAxis1.Title = "Frequency";
            plotModel1.Axes.Add(linearAxis1);
            var linearAxis2 = new LinearAxis();
            linearAxis2.Title = "Block address";
            linearAxis2.Unit = "sectors";
            linearAxis2.Position = AxisPosition.Bottom;
            plotModel1.Axes.Add(linearAxis2);

            // Count frequency
            UInt64[] r_count = new UInt64[blk_cnt];
            UInt64[] w_count = new UInt64[blk_cnt];
            for (UInt64 i = 0; i < blk_cnt; i++)
            {
                r_count[i] = 0;
                w_count[i] = 0;
            }

            ProcessInfo pinfo;

            if (pid == -1) // All processes
            {
                pinfo = GlobalPref.totalInfo;
            }
            else // Specific process
            {
                pinfo = GlobalPref.processInfos[(UInt32)pid];
            }

            for (int i = 0; i < pinfo.time_units.Count; i++)
            {
                TimeUnit temp = pinfo.time_units[i];
                for (int j = 0; j < temp.time_unit.Count; j++)
                {
                    Request t = temp.time_unit[j];
                    UInt64 first = (t.st_addr - GlobalPref.minBlock) / bu;
                    UInt64 last = (t.ed_addr - GlobalPref.minBlock) / bu;
                    UInt64 err_k = 0;
                    try
                    {
                        for (UInt64 k = first; k <= last; k++)
                        {
                            err_k = k;
                            if (t.rw)
                                w_count[k]++;
                            else
                                r_count[k]++;
                        }
                    }
                    catch (IndexOutOfRangeException)
                    {
                        Console.WriteLine("Index out of bounds: " + err_k);
                    }
                    catch (OverflowException)
                    {
                        Console.WriteLine("Overflow");
                    }
                }
            }

            // Add points
            var lineSeries1 = new LineSeries();
            var lineSeries2 = new LineSeries();
            var lineSeries3 = new LineSeries();
            lineSeries1.Color = ReadColor;
            lineSeries2.Color = WriteColor;

            for (UInt64 i = 0; i < blk_cnt; i++)
            {
                lineSeries1.Points.Add(new DataPoint(Convert.ToDouble(GlobalPref.minBlock + i * bu), r_count[i]));
                lineSeries2.Points.Add(new DataPoint(Convert.ToDouble(GlobalPref.minBlock + i * bu), w_count[i]));
                lineSeries3.Points.Add(new DataPoint(Convert.ToDouble(GlobalPref.minBlock + i * bu), r_count[i] + w_count[i]));
            }
            /***********NOT ENOUGH MEMORY if there are too many array cells....************/

            plotModel1.Series.Add(lineSeries1);
            plotModel1.Series.Add(lineSeries2);
            plotModel1.Series.Add(lineSeries3);

            plotModel1.Series[0].IsVisible = false;
            plotModel1.Series[1].IsVisible = false;

            return plotModel1;
        }

        /* Time vs Address */
        private PlotModel TimeAddr(int pid)
        {
            var plotModel1 = new PlotModel();

            // Set title
            plotModel1.Title = "Time vs Address";

            // Set axes
            var linearColorAxis1 = new LinearColorAxis();
            linearColorAxis1.Key = "ColorAxis";
            linearColorAxis1.Position = AxisPosition.Right;
            plotModel1.Axes.Add(linearColorAxis1);

            var linearAxis1 = new LinearAxis();
            linearAxis1.Title = "Time";
            linearAxis1.Unit = "ms";
            linearAxis1.Position = AxisPosition.Bottom;
            plotModel1.Axes.Add(linearAxis1);

            var linearAxis2 = new LinearAxis();
            linearAxis2.Title = "Block address";
            linearAxis2.Unit = "sectors";
            plotModel1.Axes.Add(linearAxis2);

            // Initialize graph
            var scatterSeries1 = new ScatterSeries();
            scatterSeries1.MarkerSize = 1;
            scatterSeries1.TrackerFormatString = "{0}\n{1}: {2:0.###}\n{3}: {4:0.###}\n{5}: {6:0.###}";
            var scatterSeries2 = new ScatterSeries();
            scatterSeries2.MarkerSize = 1;
            scatterSeries2.TrackerFormatString = "{0}\n{1}: {2:0.###}\n{3}: {4:0.###}\n{5}: {6:0.###}";
            var scatterSeries3 = new ScatterSeries();
            scatterSeries3.MarkerSize = 1;
            scatterSeries3.TrackerFormatString = "{0}\n{1}: {2:0.###}\n{3}: {4:0.###}\n{5}: {6:0.###}";

            ProcessInfo pinfo;

            if (pid == -1) // All processes
            {
                pinfo = GlobalPref.totalInfo;
            }
            else // Specific process
            {
                pinfo = GlobalPref.processInfos[(UInt32)pid];
            }

            // Count frequency at block address per time unit & store points in graph
            UInt64[] r_count = new UInt64[time_cnt];
            UInt64[] w_count = new UInt64[time_cnt];
            Array.Clear(r_count, 0, r_count.Length);
            Array.Clear(w_count, 0, w_count.Length);

            int tuCount = pinfo.time_units.Count;
            for (int i = 0; i < tuCount; i++)
            {
                TimeUnit temp = pinfo.time_units[i];
                int reqCount = temp.time_unit.Count;
                UInt64[] r_addr_count = new UInt64[blk_cnt];
                UInt64[] w_addr_count = new UInt64[blk_cnt];

                for (UInt64 j = 0; j < blk_cnt; j++)
                {
                    r_addr_count[j] = 0;
                    w_addr_count[j] = 0;
                }

                for (int j = 0; j < reqCount; j++)
                {
                    Request t = temp.time_unit[j];
                    UInt64 first = (t.st_addr - GlobalPref.minBlock) / bu;
                    UInt64 last = (t.ed_addr - GlobalPref.minBlock) / bu;
                    UInt64 err_k = 0;
                    try
                    {
                        for (UInt64 k = first; k <= last; k++)
                        {
                            err_k = k;
                            if (t.rw)
                                w_addr_count[k]++;
                            else
                                r_addr_count[k]++;
                        }
                    }
                    catch (IndexOutOfRangeException)
                    {
                        Console.WriteLine("Index out of bounds: " + err_k);
                    }
                    catch (OverflowException)
                    {
                        Console.WriteLine("Overflow");
                    }
                }

                for (UInt64 j = 0; j < blk_cnt; j++)
                {
                    bool all = false;

                    if (r_addr_count[j] != 0)
                    {
                        all = true;
                        scatterSeries1.Points.Add(new ScatterPoint(temp.tu, Convert.ToDouble(GlobalPref.minBlock + j * bu), 1, r_addr_count[j]));
                    }
                    if (w_addr_count[j] != 0)
                    {
                        all = true;
                        scatterSeries2.Points.Add(new ScatterPoint(temp.tu, Convert.ToDouble(GlobalPref.minBlock + j * bu), 1, w_addr_count[j]));
                    }
                    if (all)
                    {
                        scatterSeries3.Points.Add(new ScatterPoint(temp.tu, Convert.ToDouble(GlobalPref.minBlock + j * bu), 1, r_addr_count[j] + w_addr_count[j]));
                    }
                }
            }

            plotModel1.Series.Add(scatterSeries1);
            plotModel1.Series.Add(scatterSeries2);
            plotModel1.Series.Add(scatterSeries3);

            plotModel1.Series[0].IsVisible = false;
            plotModel1.Series[1].IsVisible = false;

            return plotModel1;
        }

        /* Time vs Frequency */
        private PlotModel TimeFreq(int pid)
        {
            var plotModel1 = new PlotModel();

            // Set title
            plotModel1.Title = "Time vs Frequency";

            // Set axes
            var linearAxis1 = new LinearAxis();
            linearAxis1.Title = "Frequency";
            plotModel1.Axes.Add(linearAxis1);

            var linearAxis2 = new LinearAxis();
            linearAxis2.Title = "Time";
            linearAxis2.Unit = "ms";
            linearAxis2.Position = AxisPosition.Bottom;
            plotModel1.Axes.Add(linearAxis2);

            ProcessInfo pinfo;

            if (pid == -1) // All processes
            {
                pinfo = GlobalPref.totalInfo;
            }
            else // Specific process
            {
                pinfo = GlobalPref.processInfos[(UInt32)pid];
            }

            // Calculate frequency according to time window
            UInt64[] r_count = new UInt64[time_cnt];
            UInt64[] w_count = new UInt64[time_cnt];
            Array.Clear(r_count, 0, r_count.Length);
            Array.Clear(w_count, 0, w_count.Length);

            int tuCount = pinfo.time_units.Count;
            for (int i = 0; i < tuCount; i++)
            {
                TimeUnit temp = pinfo.time_units[i];
                int time_index = temp.tu / GlobalPref.getTimeWindow();
                int reqCount = temp.time_unit.Count;

                try
                {
                    for (int j = 0; j < reqCount; j++)
                    {
                        Request t = temp.time_unit[j];
                        if (t.rw)
                        {
                            w_count[time_index]++;
                        }
                        else
                        {
                            r_count[time_index]++;
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }

            // Add points
            var lineSeries1 = new LineSeries();
            var lineSeries2 = new LineSeries();
            var lineSeries3 = new LineSeries();
            lineSeries1.Color = WriteColor;
            lineSeries2.Color = ReadColor;
            //lineSeries1.MarkerType = MarkerType.Circle;
            //lineSeries2.MarkerType = MarkerType.Circle;
            //lineSeries3.MarkerType = MarkerType.Circle;
            //lineSeries1.MarkerFill = WriteColor;
            //lineSeries2.MarkerFill = ReadColor;

            for (int i = 0; i < time_cnt; i++)
            {
                lineSeries1.Points.Add(new DataPoint(Convert.ToDouble(i * GlobalPref.getTimeWindow()), r_count[i]));
                lineSeries2.Points.Add(new DataPoint(Convert.ToDouble(i * GlobalPref.getTimeWindow()), w_count[i]));
                lineSeries3.Points.Add(new DataPoint(Convert.ToDouble(i * GlobalPref.getTimeWindow()), r_count[i] + w_count[i]));
            }

            plotModel1.Series.Add(lineSeries1);
            plotModel1.Series.Add(lineSeries2);
            plotModel1.Series.Add(lineSeries3);

            plotModel1.Series[0].IsVisible = false;
            plotModel1.Series[1].IsVisible = false;

            return plotModel1;
        }

        /* Latency vs Frequency */
        private PlotModel LatFreq(int pid)
        {
            // Intialize count
            int[] r_count = new int[10];
            int[] w_count = new int[10];
            Array.Clear(r_count, 0, r_count.Length);
            Array.Clear(w_count, 0, w_count.Length);

            ProcessInfo pinfo;

            if (pid == -1) // All processes
            {
                pinfo = GlobalPref.totalInfo;
            }
            else // Specific process
            {
                pinfo = GlobalPref.processInfos[(UInt32)pid];
            }

            // Count frequency
            for (int i = 0; i < pinfo.time_units.Count; i++)
            {
                TimeUnit temp = pinfo.time_units[i];
                for (int j = 0; j < temp.time_unit.Count; j++)
                {
                    Request t = temp.time_unit[j];
                    UInt64 templat = t.lat;
                    if (t.rw)
                    {
                        if (templat <= 0)
                        {
                            if (GlobalPref.debug)
                                w_count[0]++;
                        }
                        else if (templat < 10)
                            w_count[0]++;
                        else if (templat < 100)
                            w_count[1]++;
                        else if (templat < 1000)
                            w_count[2]++;
                        else if (templat < 10000)
                            w_count[3]++;
                        else if (templat < 100000)
                            w_count[4]++;
                        else if (templat < 1000000)
                            w_count[5]++;
                        else if (templat < 10000000)
                            w_count[6]++;
                        else if (templat < 100000000)
                            w_count[7]++;
                        else if (templat < 1000000000)
                            w_count[8]++;
                        else //여기서 부터는 초단위
                            w_count[9]++;
                    }
                    else
                    {
                        if (templat <= 0)
                        {
                            if (GlobalPref.debug)
                                w_count[0]++;
                        }
                        else if (templat < 10)
                            r_count[0]++;
                        else if (templat < 100)
                            r_count[1]++;
                        else if (templat < 1000)
                            r_count[2]++;
                        else if (templat < 10000)
                            r_count[3]++;
                        else if (templat < 100000)
                            r_count[4]++;
                        else if (templat < 1000000)
                            r_count[5]++;
                        else if (templat < 10000000)
                            r_count[6]++;
                        else if (templat < 100000000)
                            r_count[7]++;
                        else if (templat < 1000000000)
                            r_count[8]++;
                        else //여기서 부터는 초단위
                            r_count[9]++;
                    }
                }
            }

            // Make graph
            var plotModel1 = new PlotModel();
            plotModel1.Title = "Latency vs Frequency";

            var categoryAxis1 = new CategoryAxis();
            categoryAxis1.Title = "Latency";
            categoryAxis1.MinorStep = 1;
            categoryAxis1.Labels.Add("0-10ns");
            categoryAxis1.Labels.Add("10-100ns");
            categoryAxis1.Labels.Add("0.1-1\u03BCs");
            categoryAxis1.Labels.Add("1-10\u03BCs");
            categoryAxis1.Labels.Add("10-100\u03BCs");
            categoryAxis1.Labels.Add("0.1-1ms");
            categoryAxis1.Labels.Add("1-10ms");
            categoryAxis1.Labels.Add("10-100ms");
            categoryAxis1.Labels.Add("0.1-1s");
            categoryAxis1.Labels.Add("1s~");
            plotModel1.Axes.Add(categoryAxis1);

            var linearAxis1 = new LinearAxis();
            linearAxis1.Title = "Frequency";
            linearAxis1.AbsoluteMinimum = 0;
            linearAxis1.MaximumPadding = 0.06;
            linearAxis1.MinimumPadding = 0;
            plotModel1.Axes.Add(linearAxis1);

            var columnSeries1 = new ColumnSeries();
            columnSeries1.IsStacked = true;
            columnSeries1.LabelFormatString = "{0}";
            columnSeries1.StrokeThickness = 1;
            columnSeries1.Title = "Read";
            columnSeries1.LabelPlacement = LabelPlacement.Middle;
            columnSeries1.FillColor = OxyColors.SkyBlue;

            var columnSeries2 = new ColumnSeries();
            columnSeries2.IsStacked = true;
            columnSeries2.LabelFormatString = "{0}";
            columnSeries2.StrokeThickness = 1;
            columnSeries2.Title = "Write";
            columnSeries2.LabelPlacement = LabelPlacement.Middle;
            columnSeries2.FillColor = OxyColors.LightPink;

            for (int i = 0; i < 10; i++)
            {
                columnSeries1.Items.Add(new ColumnItem(r_count[i], -1));
                columnSeries2.Items.Add(new ColumnItem(w_count[i], -1));
            }

            plotModel1.Series.Add(columnSeries1);
            plotModel1.Series.Add(columnSeries2);

            return plotModel1;
        }

        /* Time vs Average Latency*/
        private PlotModel TimeLatAvg(int pid)
        {
            var plotModel1 = new PlotModel();

            // Set title
            plotModel1.Title = "Time vs Latency Average";

            // Set axes
            var linearAxis1 = new LinearAxis();
            linearAxis1.Title = "Latency Average";
            linearAxis1.Unit = "ns";
            linearAxis1.UseSuperExponentialFormat = true;
            plotModel1.Axes.Add(linearAxis1);
            var linearAxis2 = new LinearAxis();
            linearAxis2.Title = "Time";
            linearAxis2.Unit = "ms";
            linearAxis2.Position = AxisPosition.Bottom;
            plotModel1.Axes.Add(linearAxis2);

            ProcessInfo pinfo;

            if (pid == -1) // All processes
            {
                pinfo = GlobalPref.totalInfo;
            }
            else // Specific process
            {
                pinfo = GlobalPref.processInfos[(UInt32)pid];
            }

            // Calculate Average Latency according to time window
            UInt64[] r_count = new UInt64[time_cnt];
            UInt64[] w_count = new UInt64[time_cnt];
            Array.Clear(r_count, 0, r_count.Length);
            Array.Clear(w_count, 0, w_count.Length);

            int tuCount = pinfo.time_units.Count;
            for (int i = 0; i < tuCount; i++)
            {
                TimeUnit temp = pinfo.time_units[i];
                int time_index = temp.tu / GlobalPref.getTimeWindow();
                int reqCount = temp.time_unit.Count;
                UInt64 r_latSum = 0;
                UInt64 w_latSum = 0;

                try
                {
                    for (int j = 0; j < reqCount; j++)
                    {
                        Request t = temp.time_unit[j];
                        if (t.lat > 0)
                        {
                            if (t.rw)
                            {
                                w_latSum += t.lat;
                            }
                            else
                            {
                                r_latSum += t.lat;
                            }
                            //notZeroCount++;
                        }
                    }

                    //average 구해서 각 tu 에 넣는다. 이때, count[0] 에는 아무것도 안들어있을 수도 있음. count[4]부터 들어갔을 수도 있다.
                    r_count[time_index] = r_latSum / (UInt64)reqCount;
                    w_count[time_index] = w_latSum / (UInt64)reqCount;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }

            // Add points
            var lineSeries1 = new LineSeries();
            var lineSeries2 = new LineSeries();
            var lineSeries3 = new LineSeries();
            lineSeries1.Color = ReadColor;
            lineSeries2.Color = WriteColor;
            //lineSeries1.MarkerType = MarkerType.Circle;
           //lineSeries2.MarkerType = MarkerType.Circle;
           //lineSeries3.MarkerType = MarkerType.Circle;
           //lineSeries1.MarkerFill = ReadColor;
           //lineSeries2.MarkerFill = WriteColor;

            for (int i = 0; i < time_cnt; i++)
            {
                lineSeries1.Points.Add(new DataPoint(Convert.ToDouble(i * GlobalPref.getTimeWindow()), r_count[i]));
                lineSeries2.Points.Add(new DataPoint(Convert.ToDouble(i * GlobalPref.getTimeWindow()), w_count[i]));
                lineSeries3.Points.Add(new DataPoint(Convert.ToDouble(i * GlobalPref.getTimeWindow()), r_count[i] + w_count[i]));
            }

            plotModel1.Series.Add(lineSeries1);
            plotModel1.Series.Add(lineSeries2);
            plotModel1.Series.Add(lineSeries3);

            plotModel1.Series[0].IsVisible = false;
            plotModel1.Series[1].IsVisible = false;

            return plotModel1;
        }

        /* Throughput */
        private PlotModel Throughput(int pid)
        {
            var plotModel1 = new PlotModel();

            // Set title
            plotModel1.Title = "Time vs Sector Length";

            // Set axes
            var linearAxis1 = new LinearAxis();
            linearAxis1.Title = "Sector Length";
            plotModel1.Axes.Add(linearAxis1);
            var linearAxis2 = new LinearAxis();
            linearAxis2.Title = "Time";
            linearAxis2.Unit = "ms";
            linearAxis2.Position = AxisPosition.Bottom;
            plotModel1.Axes.Add(linearAxis2);

            ProcessInfo pinfo;

            if (pid == -1) // All processes
            {
                pinfo = GlobalPref.totalInfo;
            }
            else // Specific process
            {
                pinfo = GlobalPref.processInfos[(UInt32)pid];
            }

            // Calculate byte size according to time window
            UInt64[] r_count = new UInt64[time_cnt];
            UInt64[] w_count = new UInt64[time_cnt];
            Array.Clear(r_count, 0, r_count.Length);
            Array.Clear(w_count, 0, w_count.Length);

            int tuCount = pinfo.time_units.Count;
            for (int i = 0; i < tuCount; i++)
            {
                TimeUnit temp = pinfo.time_units[i];
                int time_index = temp.tu / GlobalPref.getTimeWindow();
                int reqCount = temp.time_unit.Count;
                UInt64 r_dataSum = 0;
                UInt64 w_dataSum = 0;

                if (time_index >= time_cnt)
                {
                    break;
                }

                for (int j = 0; j < reqCount; j++)
                {
                    Request t = temp.time_unit[j];
                    if (t.rw)
                    {
                        w_dataSum += (t.ed_addr - t.st_addr + 1);
                    }
                    else
                    {
                        r_dataSum += (t.ed_addr - t.st_addr + 1);
                    }
                }
                r_count[time_index] = r_dataSum;
                w_count[time_index] = w_dataSum;
            }

            // Add points
            var lineSeries1 = new LineSeries();
            var lineSeries2 = new LineSeries();
            var lineSeries3 = new LineSeries();
            lineSeries1.Color = ReadColor;
            lineSeries2.Color = WriteColor;
            //lineSeries1.MarkerType = MarkerType.Circle;
            //lineSeries2.MarkerType = MarkerType.Circle;
            //lineSeries3.MarkerType = MarkerType.Circle;
            //lineSeries1.MarkerFill = ReadColor;
            //lineSeries2.MarkerFill = WriteColor;

            for (int i = 0; i < time_cnt; i++)
            {
                lineSeries1.Points.Add(new DataPoint(Convert.ToDouble(i * GlobalPref.getTimeWindow()), r_count[i]));
                lineSeries2.Points.Add(new DataPoint(Convert.ToDouble(i * GlobalPref.getTimeWindow()), w_count[i]));
                lineSeries3.Points.Add(new DataPoint(Convert.ToDouble(i * GlobalPref.getTimeWindow()), r_count[i] + w_count[i]));
            }
            plotModel1.Series.Add(lineSeries1);
            plotModel1.Series.Add(lineSeries2);
            plotModel1.Series.Add(lineSeries3);

            plotModel1.Series[0].IsVisible = false;
            plotModel1.Series[1].IsVisible = false;

            return plotModel1;
        }

        /* Functions used for read/write/all selection in Series */
        private void SeriesRead()
        {
            this.PlotModel.Series[0].IsVisible = true;
            this.PlotModel.Series[1].IsVisible = false;
            this.PlotModel.Series[2].IsVisible = false;
            this.PlotModel.InvalidatePlot(true);
        }

        private void SeriesWrite()
        {
            this.PlotModel.Series[0].IsVisible = false;
            this.PlotModel.Series[1].IsVisible = true;
            this.PlotModel.Series[2].IsVisible = false;
            this.PlotModel.InvalidatePlot(true);
        }

        private void SeriesAll()
        {
            this.PlotModel.Series[0].IsVisible = false;
            this.PlotModel.Series[1].IsVisible = false;
            this.PlotModel.Series[2].IsVisible = true;
            this.PlotModel.InvalidatePlot(true);
        }

        /* Functions used for RadioButton control */
        private void readRB_Checked(object sender, RoutedEventArgs e)
        {
            LoadedBool = false;
            if (whichGraph != 4)
            {
                SeriesRead();
            }
            LoadedBool = true;
        }

        private void writeRB_Checked(object sender, RoutedEventArgs e)
        {
            LoadedBool = false;
            if (whichGraph != 4)
            {
                SeriesWrite();
            }
            LoadedBool = true;
        }

        private void allRB_Checked(object sender, RoutedEventArgs e)
        {
            if (firstLoad)
            {
                firstLoad = false;
            }
            else
            {
                LoadedBool = false;
                if (whichGraph != 4)
                {
                    SeriesAll();
                }
                LoadedBool = true;
            }
        }

        private void processList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (processList.SelectedIndex < 0) return;

            UInt32 pid = GlobalPref.requestCountList[processList.SelectedIndex].Key;

            if (whichGraph != 0)
            {
                LoadedBool = false;
                switch (whichGraph)
                {
                    case 1:
                        this.PlotModel = AddrFreq((int)pid);
                        this.PlotModel.InvalidatePlot(true);
                        break;
                    case 2:
                        this.PlotModel = TimeAddr((int)pid);
                        this.PlotModel.InvalidatePlot(true);
                        break;
                    case 3:
                        this.PlotModel = TimeFreq((int)pid);
                        this.PlotModel.InvalidatePlot(true);
                        break;
                    case 4:
                        this.PlotModel = LatFreq((int)pid);
                        this.PlotModel.InvalidatePlot(true);
                        break;
                    case 5:
                        this.PlotModel = TimeLatAvg((int)pid);
                        this.PlotModel.InvalidatePlot(true);
                        break;
                    case 6:
                        this.PlotModel = Throughput((int)pid);
                        this.PlotModel.InvalidatePlot(true);
                        break;
                }
                LoadedBool = true;
            }
        }

        private void allProcBtn_Click(object sender, RoutedEventArgs e)
        {
            if (whichGraph != 0)
            {
                LoadedBool = false;
                switch (whichGraph)
                {
                    case 1:
                        this.PlotModel = AddrFreq(-1);
                        this.PlotModel.InvalidatePlot(true);
                        break;
                    case 2:
                        this.PlotModel = TimeAddr(-1);
                        this.PlotModel.InvalidatePlot(true);
                        break;
                    case 3:
                        this.PlotModel = TimeFreq(-1);
                        this.PlotModel.InvalidatePlot(true);
                        break;
                    case 4:
                        this.PlotModel = LatFreq(-1);
                        this.PlotModel.InvalidatePlot(true);
                        break;
                    case 5:
                        this.PlotModel = TimeLatAvg(-1);
                        this.PlotModel.InvalidatePlot(true);
                        break;
                    case 6:
                        this.PlotModel = Throughput(-1);
                        this.PlotModel.InvalidatePlot(true);
                        break;
                }
                LoadedBool = true;
            }
        }
    }
}
