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

namespace ValtioClient
{
    /// <summary>
    /// Interaction logic for Graph1.xaml
    /// </summary>
    public partial class Graph1
    {
        public Graph1()
        {
            InitializeComponent();
        }
    }

    public class MainViewModel
    {
        private static UInt64 len = GlobalPref.maxBlock - GlobalPref.minBlock;
        private static UInt64 cnt;
        private static UInt64 bu = (UInt64)GlobalPref.getBlockUnit();
        public static int[] count;

        public MainViewModel()
        {
            /* Initialize array */
            if (len % bu == 0)
            {
                cnt = len / bu;
            }
            else
            {
                cnt = (len / bu) + 1;
            }

            count = new int[cnt];
            for (UInt64 i = 0; i < cnt; i++)
            {
                count[i] = 0;
            }

            /***************DEBUG*************/
            Console.WriteLine("MinBlock: " + GlobalPref.minBlock);
            Console.WriteLine("MaxBlock: " + GlobalPref.maxBlock);
            Console.WriteLine("MaxLat: " + GlobalPref.maxLat);

            for (int i = 0; i < GlobalPref.pids.Count; i++)
            {
                Console.WriteLine("pid: " + GlobalPref.pids[i]);
            }

            Console.WriteLine("Array count: " + cnt);
            /***************DEBUG*************/

            this.MyModel = AddrFreq();
        }

        public PlotModel MyModel { get; private set; }

        public static PlotModel AddrFreq()
        {
            var plotModel1 = new PlotModel();

            // Set title
            plotModel1.Title = "Address vs Frequency";

            // Set axes
            var linearAxis1 = new LinearAxis();
            plotModel1.Axes.Add(linearAxis1);
            var linearAxis2 = new LinearAxis();
            linearAxis2.Position = AxisPosition.Bottom;
            plotModel1.Axes.Add(linearAxis2);
            var lineSeries1 = new LineSeries();

            // Set color
            lineSeries1.Color = OxyColor.FromArgb(255, 78, 154, 6);
            lineSeries1.MarkerFill = OxyColor.FromArgb(255, 78, 154, 6);

            // Count frequency
            for (int i = 0; i < GlobalPref.totalInfo.time_units.Count; i++)
            {
                TimeUnit temp = GlobalPref.totalInfo.time_units[i];
                for (int j = 0; j < temp.time_unit.Count; j++)
                {
                    Request t = temp.time_unit[j];
                    UInt64 first = (t.st_addr - t.st_addr % bu) / bu;
                    UInt64 last = (t.ed_addr - t.ed_addr % bu) / bu;
                    UInt64 err_k = 0;
                    try
                    {
                        for (UInt64 k = first; k <= last; k++)
                        {
                            err_k = k;
                            count[k]++;
                        }
                    }
                    catch (IndexOutOfRangeException)
                    {
                        Console.WriteLine("Index out of bounds: " + err_k);
                    }
                }
            }

            // Add points
            for (UInt64 i = 0; i < cnt; i++)
                lineSeries1.Points.Add(new DataPoint(Convert.ToDouble(GlobalPref.minBlock + i * bu), count[i]));
            /***********NOT ENOUGH MEMORY if there are too many array cells....************/

            plotModel1.Series.Add(lineSeries1);
            return plotModel1;
        }
    }
}
