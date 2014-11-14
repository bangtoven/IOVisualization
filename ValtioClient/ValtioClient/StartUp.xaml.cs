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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ValtioClient
{
    /// <summary>
    /// Interaction logic for StartUp.xaml
    /// </summary>
    public partial class StartUp
    {
        private String _serverIP;
        private String _serverPort;

        public StartUp()
        {
            InitializeComponent();
        }

        private void connectBtn_Click(object sender, RoutedEventArgs e)
        {
            _serverIP = serverIP.Text;
            _serverPort = serverPort.Text;
            if (_serverIP == null || _serverIP == "" || _serverPort == null || _serverPort == "")
            {
                GlobalFunc.ShowMessageBox("Error", "Please input both the server's IP address and port.");
                return;
            }

            // Store server IP and port as global preference
            GlobalPref.setServerIP(_serverIP);
            GlobalPref.setServerPort(_serverPort);

            // Prepare and show environment setup window
            Window envSetup = new EnvSetup();
            envSetup.Show();
            this.Close();
        }
    }
}
