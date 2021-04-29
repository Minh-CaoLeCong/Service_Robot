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
using System.Windows.Threading;
using System.Net.Http;
using HtmlAgilityPack;
using System.IO.Ports;

namespace LoginWPF_Part1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string account = "admin";
        string pass = "admin";
        public MainWindow()
        {
            InitializeComponent();
        }
        
        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            if (TxtUser.Text == account && TxtPasswords.Password == pass)
            {
                MessageBox.Show("Welcome to Service Robot", "Windows", MessageBoxButton.OK);
                Window1 win1 = new Window1();
                this.Close();
                win1.ShowDialog();
            }
            else MessageBox.Show("Sorry! Try again!", "Windows", MessageBoxButton.OK);
        }
        
        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
