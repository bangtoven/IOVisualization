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
        private static int time_len = GlobalPref.getTraceLength();
        private static int tu = GlobalPref.getTimeWindow();

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

        private PlotModel plotModel;

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
        }

        /* Button click methods */
        private void AddrFreq_Click(object sender, RoutedEventArgs e)
        {
            Preprocessing();
            oxyLoading.IsContentLoaded = false;
            whichGraph = 1;
            CheckRadio();
            this.PlotModel = this.AddrFreq();
            oxyLoading.IsContentLoaded = true;
        }

        private void TimeAddr_Click(object sender, RoutedEventArgs e)
        {
            Preprocessing();
            oxyLoading.IsContentLoaded = false;
            whichGraph = 2;
            CheckRadio();
            this.PlotModel = this.TimeAddr();
            oxyLoading.IsContentLoaded = true;
        }

        private void TimeFreq_Click(object sender, RoutedEventArgs e)
        {
            Preprocessing();
            oxyLoading.IsContentLoaded = false;
            whichGraph = 3;
            CheckRadio();
            this.PlotModel = this.TimeFreq();
            oxyLoading.IsContentLoaded = true;
        }

        private void LatFreq_Click(object sender, RoutedEventArgs e)
        {
            Preprocessing();
            oxyLoading.IsContentLoaded = false;
            whichGraph = 4;
            CheckRadio();
            this.PlotModel = this.LatFreq();
            oxyLoading.IsContentLoaded = true;
        }

        private void TimeLatAvg_Click(object sender, RoutedEventArgs e)
        {
            Preprocessing();
            oxyLoading.IsContentLoaded = false;
            whichGraph = 5;
            CheckRadio();
            this.PlotModel = this.TimeLatAvg();
            oxyLoading.IsContentLoaded = true;
        }

        private void Throughput_Click(object sender, RoutedEventArgs e)
        {
            Preprocessing();
            oxyLoading.IsContentLoaded = false;
            whichGraph = 6;
            CheckRadio();
            this.PlotModel = this.Throughput();
            oxyLoading.IsContentLoaded = true;
        }

        /* Preprocessing - use this to make something happen before each graph is called */
        private void Preprocessing()
        {
            allRB.IsChecked = true;
        }

        /* Checks radio buttons */
        private void CheckRadio()
        {
            if (whichGraph == 4)
            {
                // Disable radio buttons
                readRB.IsEnabled = false;
                writeRB.IsEnabled = false;
                allRB.IsEnabled = false;
            }
            else
            {
                // Enable radio buttons
                readRB.IsEnabled = true;
                writeRB.IsEnabled = true;
                allRB.IsEnabled = true;
            }
        }

        /* Address vs Frequency */
        private PlotModel AddrFreq()
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

            for (int i = 0; i < GlobalPref.totalInfo.time_units.Count; i++)
            {
                TimeUnit temp = GlobalPref.totalInfo.time_units[i];
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
            lineSeries1.Color = OxyColors.Blue;
            lineSeries2.Color = OxyColors.Red;

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
        private PlotModel TimeAddr()
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
            linearAxis1.Unit = "s";
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

            // Count frequency at block address per time unit & store points in graph
            UInt64[] r_count = new UInt64[time_cnt];
            UInt64[] w_count = new UInt64[time_cnt];
            Array.Clear(r_count, 0, r_count.Length);
            Array.Clear(w_count, 0, w_count.Length);

            int tuCount = GlobalPref.totalInfo.time_units.Count;
            for (int i = 0; i < tuCount; i++)
            {
                TimeUnit temp = GlobalPref.totalInfo.time_units[i];
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
        private PlotModel TimeFreq()
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
            linearAxis2.Unit = "s";
            linearAxis2.Position = AxisPosition.Bottom;
            plotModel1.Axes.Add(linearAxis2);
            

            // Calculate frequency according to time window
            UInt64[] r_count = new UInt64[time_cnt];
            UInt64[] w_count = new UInt64[time_cnt];
            Array.Clear(r_count, 0, r_count.Length);
            Array.Clear(w_count, 0, w_count.Length);

            int tuCount = GlobalPref.totalInfo.time_units.Count;
            for (int i = 0; i < tuCount; i++)
            {
                TimeUnit temp = GlobalPref.totalInfo.time_units[i];
                int time_index = temp.tu / GlobalPref.getTimeWindow();
                int reqCount = temp.time_unit.Count;

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

            // Add points
            var lineSeries1 = new LineSeries();
            var lineSeries2 = new LineSeries();
            var lineSeries3 = new LineSeries();
            lineSeries1.Color = OxyColors.Blue;
            lineSeries2.Color = OxyColors.Red;
            lineSeries1.Smooth = true;
            lineSeries2.Smooth = true;
            lineSeries3.Smooth = true;
            lineSeries1.MarkerType = MarkerType.Circle;
            lineSeries2.MarkerType = MarkerType.Circle;
            lineSeries3.MarkerType = MarkerType.Circle;
            lineSeries1.MarkerFill = OxyColors.Blue;
            lineSeries2.MarkerFill = OxyColors.Red;

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
        private PlotModel LatFreq()
        {
            // Intialize count
            int[] r_count = new int[10];
            int[] w_count = new int[10];

            // Count frequency
            for (int i = 0; i < GlobalPref.totalInfo.time_units.Count; i++)
            {
                TimeUnit temp = GlobalPref.totalInfo.time_units[i];
                for (int j = 0; j < temp.time_unit.Count; j++)
                {
                    Request t = temp.time_unit[j];
                    UInt64 templat = t.lat;
                    if (t.rw)
                    {
                        if (templat < 10)
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
                        if (templat < 10)
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
            columnSeries2.FillColor = OxyColors.Lavender;

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
        private PlotModel TimeLatAvg()
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
            linearAxis2.Unit = "s";
            linearAxis2.Position = AxisPosition.Bottom;
            plotModel1.Axes.Add(linearAxis2);

            // Calculate Average Latency according to time window
            UInt64[] r_count = new UInt64[time_cnt];
            UInt64[] w_count = new UInt64[time_cnt];
            Array.Clear(r_count, 0, r_count.Length);
            Array.Clear(w_count, 0, w_count.Length);

            int tuCount = GlobalPref.totalInfo.time_units.Count;
            for (int i = 0; i < tuCount; i++)
            {
                TimeUnit temp = GlobalPref.totalInfo.time_units[i];
                int time_index = temp.tu / GlobalPref.getTimeWindow();
                int reqCount = temp.time_unit.Count;
                UInt64 r_latSum = 0;
                UInt64 w_latSum = 0;
                for (int j = 0; j < reqCount; j++)
                {
                    Request t = temp.time_unit[j];
                    if (t.rw)
                    {
                        w_latSum += t.lat;
                    }
                    else
                    {
                        r_latSum += t.lat;
                    }
                }
                //average 구해서 각 tu 에 넣는다. 이때, count[0] 에는 아무것도 안들어있을 수도 있음. count[4]부터 들어갔을 수도 있다.
                r_count[time_index] = r_latSum / (UInt64)reqCount; 
                w_count[time_index] = w_latSum / (UInt64)reqCount;
            }

            // Add points
            var lineSeries1 = new LineSeries();
            var lineSeries2 = new LineSeries();
            var lineSeries3 = new LineSeries();
            lineSeries1.Color = OxyColors.Blue;
            lineSeries2.Color = OxyColors.Red;
            lineSeries1.Smooth = true;
            lineSeries2.Smooth = true;
            lineSeries3.Smooth = true;
            lineSeries1.MarkerType = MarkerType.Circle;
            lineSeries2.MarkerType = MarkerType.Circle;
            lineSeries3.MarkerType = MarkerType.Circle;
            lineSeries1.MarkerFill = OxyColors.Blue;
            lineSeries2.MarkerFill = OxyColors.Red;

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
        private PlotModel Throughput()
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
            linearAxis2.Unit = "s";
            linearAxis2.Position = AxisPosition.Bottom;
            plotModel1.Axes.Add(linearAxis2);

            // Calculate byte size according to time window
            UInt64[] r_count = new UInt64[time_cnt];
            UInt64[] w_count = new UInt64[time_cnt];
            Array.Clear(r_count, 0, r_count.Length);
            Array.Clear(w_count, 0, w_count.Length);

            int tuCount = GlobalPref.totalInfo.time_units.Count;
            for (int i = 0; i < tuCount; i++)
            {
                TimeUnit temp = GlobalPref.totalInfo.time_units[i];
                int time_index = temp.tu / GlobalPref.getTimeWindow();
                int reqCount = temp.time_unit.Count;
                UInt64 r_dataSum = 0;
                UInt64 w_dataSum = 0;
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
            lineSeries1.Color = OxyColors.Blue;
            lineSeries2.Color = OxyColors.Red;
            lineSeries1.Smooth = true;
            lineSeries2.Smooth = true;
            lineSeries3.Smooth = true;
            lineSeries1.MarkerType = MarkerType.Circle;
            lineSeries2.MarkerType = MarkerType.Circle;
            lineSeries3.MarkerType = MarkerType.Circle;
            lineSeries1.MarkerFill = OxyColors.Blue;
            lineSeries2.MarkerFill = OxyColors.Red;

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
            PlotModel.InvalidatePlot(true);
        }

        private void SeriesWrite()
        {
            this.PlotModel.Series[0].IsVisible = false;
            this.PlotModel.Series[1].IsVisible = true;
            this.PlotModel.Series[2].IsVisible = false;
            PlotModel.InvalidatePlot(true);
        }

        private void SeriesAll()
        {
            this.PlotModel.Series[0].IsVisible = false;
            this.PlotModel.Series[1].IsVisible = false;
            this.PlotModel.Series[2].IsVisible = true;
            PlotModel.InvalidatePlot(true);
        }

        /* Functions used for RadioButton control */
        private void readRB_Checked(object sender, RoutedEventArgs e)
        {
            oxyLoading.IsContentLoaded = false;
            if (whichGraph != 4)
            {
                SeriesRead();
            }
            oxyLoading.IsContentLoaded = true;
        }

        private void writeRB_Checked(object sender, RoutedEventArgs e)
        {
            oxyLoading.IsContentLoaded = false;
            if (whichGraph != 4)
            {
                SeriesWrite();
            }
            oxyLoading.IsContentLoaded = true;
        }

        private void allRB_Checked(object sender, RoutedEventArgs e)
        {
            if (firstLoad)
            {
                firstLoad = false;
            }
            else
            {
                oxyLoading.IsContentLoaded = false;
                if (whichGraph != 4)
                {
                    SeriesAll();
                }
                oxyLoading.IsContentLoaded = true;
            }
        }
    }
}
