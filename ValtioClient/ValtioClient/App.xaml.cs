using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace ValtioClient
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        private void ElysiumApplication_Startup(object sender, StartupEventArgs e)
        {
            // Check argument for debug
            for (int i = 0; i != e.Args.Length; ++i) {
                if (e.Args[i] == "-debug")
                {
                    GlobalPref.debug = true;
                }
            }
        }
    }
}
