using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO.Ports;
using System.Windows.Threading;
using System.Threading;

namespace WpfApplication1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        SerialPort port;
        DispatcherTimer pollTimer;
        const int LIMIT = 250;
        String comPort= "";

        public MainWindow()
        {
            InitializeComponent();
        }

        public void init()
        {
            try
            {
                if (this.port != null && port.IsOpen == true)
                {
                    this.port.Close();
                }

                this.port = new SerialPort(comPort);
                this.port.Open();
                this.port.Handshake = Handshake.RequestToSend;
                dialog1.Title = "Light meter";
            }
            catch (Exception e)
            {
                statusData.Content = String.Format("{0}", e.Message);
            }

            if (this.port.IsOpen == true)
            {
                statusData.Content = String.Format("Connected to {0}", this.comPort);
            }
        }

        public void updateLabel(Object a, EventArgs e)
        {
            int i = 0;

            if (this.port.IsOpen == true)
            {
                // Discharge
                this.port.DtrEnable = false;
                Thread.Sleep(250);

                // Charge
                this.port.DtrEnable = true;
                for (i = 0; i < MainWindow.LIMIT; i++)
                {
                    if (this.port.CtsHolding == true)
                    {
                        break;
                    }
                    Thread.Sleep(1);
                }
            }

            this.port.DtrEnable = false;
            // Display
            String valStr = i.ToString();
            labelValue.Content = valStr;
        }

        private void Restart_Click(object sender, RoutedEventArgs e)
        {
            init();
        }

        private void buttonStart_Click(object sender, RoutedEventArgs e)
        {
            // Load settings
            int index= listBoxPort.SelectedIndex;
            if (index != -1)
            {
                ListBoxItem lb= (ListBoxItem)listBoxPort.Items[index];
                this.comPort = lb.Content.ToString();
                Properties.Settings.Default.port = comPort;
                Properties.Settings.Default.Save();
            }
            else
            {
                this.comPort = Properties.Settings.Default.port;
            }

            int interval = Properties.Settings.Default.logInterval;

            this.pollTimer = new DispatcherTimer();
            this.pollTimer.Interval = TimeSpan.FromMilliseconds(interval);
            this.pollTimer.Tick += new EventHandler(updateLabel);
            this.pollTimer.Start();

            // Start data collection
            init();

        }
    }
}
