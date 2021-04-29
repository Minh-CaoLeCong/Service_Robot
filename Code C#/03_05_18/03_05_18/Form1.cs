using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO.Ports; // Them dong nay de dung lenh Serial.Port
using System.IO;
using System.Xml;


namespace _03_05_18
{
    public partial class Form1 : Form
    {
        string InputData = String.Empty;
        public Form1()
        {
            InitializeComponent();
            this.TB1.ReadOnly = true;
        }
        string data;
        string data1;
        private void btnDisConnect_Click(object sender, EventArgs e)
        {
            serialPort1.Close();
            this.lblStatus.Text = "Disconnected";
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            comboBox1.DataSource = SerialPort.GetPortNames();
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            if (!serialPort1.IsOpen) // Nếu đối tượng serialPort1 chưa được mở , sau khi nhấn button 1 thỳ…
            {

                serialPort1.PortName = comboBox1.Text;//cổng serialPort1 = ComboBox mà bạn đang chọn
                serialPort1.Open();// Mở cổng serialPort1
                this.lblStatus.Text = "Connected";

            }
        }

        private void btnDenSang_Click(object sender, EventArgs e)
        {
            //serialPort1.Write(turnon);
            //this.lblStatus.Text = "Đèn sáng";
        }

        private void lblStatus_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            data = txtData.Text;
            serialPort1.Write(data);
            txtData.Text = "";

        }

        private void btnDenTat_Click(object sender, EventArgs e)
        {
            // serialPort1.Write(turnoff);
            //this.lblStatus.Text = "Đèn tắt";
        }

        private void maskedTextBox1_MaskInputRejected(object sender, MaskInputRejectedEventArgs e)
        {

        }

        private void TB1_MaskInputRejected(object sender, MaskInputRejectedEventArgs e)
        {

           string s = serialPort1.ReadExisting();

            if (s != "")
            {
                SetText(s);
            }
        }

        private void oncom(object sender, SerialDataReceivedEventArgs e)
        {
            
            string s = serialPort1.ReadExisting();

            if (s != "")
            {
                SetText("");
                SetText(s);
            }
        }
        delegate void SetTextCallback(string text);
        private void SetText(string text)

        {

            if (this.TB1.InvokeRequired)

            {

                SetTextCallback d = new SetTextCallback(SetText); // khởi tạo 1 delegate mới gọi đến SetText

                this.Invoke(d, new object[] { text });

            }

            else this.TB1.Text = text;

        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            data1 = "f"+X.Text+"x"+ Y.Text+"y";
            serialPort1.Write(data1);
            X.Text = "";
            Y.Text = "";
        }

        private void label6_Click(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {
            serialPort1.Write("w");
        }

        private void button6_Click(object sender, EventArgs e)
        {
            serialPort1.Write("q");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            serialPort1.Write("q");
        }

        private void button5_Click(object sender, EventArgs e)
        {
            serialPort1.Write("a");
        }

        private void button4_Click(object sender, EventArgs e)
        {
            serialPort1.Write("s");
        }

        private void txtData_TextChanged(object sender, EventArgs e)
        {

        }
    }
}