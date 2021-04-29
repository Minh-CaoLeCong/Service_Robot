using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Web;
using System.IO;
using System.Net;
using System.Threading;
using System.Diagnostics;
using System.Text;
using System.Media;
using System.IO.Ports;
using System.Xml;
using System.Net.Http;
using HtmlAgilityPack;

namespace _16_06_18
{
    public partial class Form1 : Form
    {
        string tt;
        public Form1()
        {
            InitializeComponent();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            HtmlWeb dubao = new HtmlWeb();
            string website = @"https://www.accuweather.com/vi/vn/ho-chi-minh-city/353981/current-weather/353981";
            HtmlAgilityPack.HtmlDocument doc = dubao.Load(website);

            HtmlNodeCollection trangthai = doc.DocumentNode.SelectNodes("//*[@id='feed - tabs']/ul/li[1]/div/div[2]/span");
            foreach (var item_tt1 in trangthai)
            {
                tt = Convert.ToString(item_tt1.InnerText).Trim();
            }
            label1.Text = tt;
        }
        
    }
}
