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
using System.Windows.Threading;
using System.Net.Http;
using HtmlAgilityPack;
using System.IO.Ports;

namespace LoginWPF_Part1
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        SerialPort sp = new SerialPort();
        int i;
        string data1, sgetpos, sgetposx, sgetposy;
        char[] cgetpos, cgetposx, cgetposy;
        bool test = false;
        double dgetposx, dgetposy, dgetpos0x, dgetpos0y, scale = 0.8;
        double dgetposxx, dgetposyy;
        public Window1()
        {
            InitializeComponent();
            startclock();
        }
        private void startclock()
        {
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += tickevent;
            timer.Start();
        }

        private void tickevent(object sender, EventArgs e)
        {
            lblDayTime.Text = DateTime.Now.ToString();

        }
        private void ListViewItem_Selected(object sender, RoutedEventArgs e)
        {
            FrameYoutube.Content = new Page1();
        }

        private void ListViewItem_Selected_1(object sender, RoutedEventArgs e)
        {

        }
        private void ButtonTable_Click(object sender, RoutedEventArgs e)
        {
       
        }
        private void ButtonBack_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainwin = new MainWindow();
            this.Close();
            mainwin.ShowDialog();
        }

        private void ButtonOpen_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ButtonConnect_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string portName = ComPortNo.Text;
                sp.PortName = portName;
                sp.BaudRate = 9600;
                sp.Open();
                StatusPort.Text = "Connected";
            }
            catch (Exception)
            {
                MessageBox.Show("Please give a valid port name or check your connection");
            }
        }

        private void ButtonDisconect_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                sp.Close();
                StatusPort.Text = "Disconnected";
            }
            catch (Exception)
            {
                MessageBox.Show("First connect and then disconnect");
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            data1 = "f100x100y";
            sp.Write(data1);

        }

        private void ButtonSend_Click(object sender, RoutedEventArgs e)
        {
            data1 = SendCoordinate.Text;
            sp.Write(data1);
        }

        private void ButtonPosition_Click(object sender, RoutedEventArgs e)
        {
            dgetposxx = 0; dgetposyy = 0;
            dgetpos0x = 1197;   dgetpos0y = 510;
            dgetposx = 0;   dgetposy = 0;
            sgetpos = "";
            sgetposx = "";
            sgetposy = "";
            test = false;
            Point getpos = PointToScreen(Mouse.GetPosition(this));
            sgetpos = getpos.ToString();
            MessageBox.Show(sgetpos);
            cgetpos = sgetpos.ToCharArray();
            for (i=0; i < cgetpos.Length; i++)
            {
                if (cgetpos[i] == ',') break;
                else sgetposx = sgetposx + cgetpos[i];
            }
            for (i=0; i < cgetpos.Length; i++)
            {
                if (cgetpos[i] == ',') test = true;
                if ((test == true) && ((i + 1) < cgetpos.Length)) sgetposy = sgetposy + cgetpos[i + 1];
            }
            //SendCoordinate.Text =sgetposy;
            dgetposx = Convert.ToDouble(sgetposx);
            dgetposy = Convert.ToDouble(sgetposy);
            dgetposyy = (dgetpos0x - dgetposx) / scale ;
            dgetposxx = (dgetpos0y - dgetposy) / scale ;
            sgetposx = dgetposxx.ToString();
            sgetposy = dgetposyy.ToString();
            cgetposx = sgetposx.ToCharArray();
            cgetposy = sgetposy.ToCharArray();
            sgetposx = ""; sgetposy = "";
            for(i = 0; i < cgetposx.Length; i++)
            {
                if (cgetposx[i] == '.') break;
                else sgetposx = sgetposx + cgetposx[i];
            }
            for (i = 0; i < cgetposy.Length; i++)
            {
                if (cgetposy[i] == '.') break;
                else sgetposy = sgetposy + cgetposy[i];
            }
            SendCoordinate.Text = "f" + sgetposx + "x" + sgetposy + "y";
        }
    }
}
