    using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NAudio.Wave;
using System.Net.Http;
using System.Speech;
using System.Speech.Synthesis;
using HtmlAgilityPack;
using System.Web;
using System.IO;
using System.Net;
using System.Threading;
using System.Diagnostics;
using System.Text;
using System.Media;
using WMPLib;
using System.IO.Ports;
using System.Xml;
using Newtonsoft.Json;
using System.Net.WebSockets;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.Util;
using Emgu.CV.Cuda;
using FaceDetection;
using SKYPE4COMLib;





/*
 *thời gian có hạn nên nhóm chỉ lập trình một vài câu hỏi sau:
What’s your name?
What time is it?
 Tôi có thể trò chuyện với bạn được không?
How old are you?
How are you?
Where are you from?
What’s your job?
Can you introduce youself?
Nice to meet you.
Can you help me?
Con gì cao nhất thế giới?
How do you feel right now?
Robot có tập thể dục không?
Trả lời sai rồi.
Tôi tên gì?
Ai là người thân của tôi?
You are a stupid robot?
I hate you.
You are so beautiful.
Cách sử dụng robot.
Can you dance?
Địa chỉ hiện tại của tôi.
Kể chuyện tôi nghe
Năm nay tôi bao nhiêu tuổi
Nhắc tôi những ngày quan trọng
Ba ngày rồi tôi chưa tắm
Tóc tôi bạc khá nhiều
Thức ăn mấy bữa nay không ngon
Thật là tuyệt vời
Thời tiết nóng nực quá
Mưa hoài
Chuyện gì đang xảy ra vậy?
Hôm nay là sinh nhật tôi
Robot đang nghĩ gì?
Mấy ngày nay tôi rất bận
Robot thích làm gì vào thời gian rãnh?
Tối nay tivi có chiếu gì hay không?
Phòng không sạch?
Tôi muốn đi chơi
Số điện thoại của tôi là?
Robot nói được mấy tiếng?
Xin chào
Làm gì để không buồn?
Tôi không ngủ được
Tôi cảm thấy cô đơn và chán nản quá
Khi nào con của tôi đi làm về
Để có sức khỏe tốt tôi nên làm gì?
Bạn có thể trả lời lại được không?
Làm sao để trở nên giàu có?
Tôi không tin?
Tôi có mập không?
Hôm nay tôi cảm thấy mệt mỏi
Tội bị bệnh rồi
Tôi bị nhức răng / nhức đầu
Tôi còn sống được bao lâu nữa
Cuộc đời không đẹp như tôi tưởng
 Hôm nay là thứ mấy
 Bây giờ là mấy giờ

Câu ra lệnh:
Bật đèn / tắt đèn
Bật quạt / tắt quạt
Phát nhạc
Đổi bài nhạc
Mở FM
Chuyển kênh FM/ Đổi đài FM
 dừng lại
 
 Chào buổi sáng, tối, trưa, chiều
 Ngu, khùng, điên
 Robot/bạn tên gì?
 Robot/bạn có khoẻ không?
 Robot/bạn bao nhiêu tuổi?
 Robot/bạn ở đâu?
 Nghề của robot/bạn là gì?
 Giới thiệu bản thân.
 Bạn có thể giúp tôi?
 Cảm giác/cảm xúc của bạn/robot?
 Robot/bạn dễ thương/đẹp/đáng yêu.
 Tôi ghét robot/bạn.
 Robot/bạn có thể nhảy múa không?
 Trời nóng
 Trời lạnh
 Tôi bị đau
 Tạm biệt
 Nhiệt độ/ thời tiết / âm lịch hôm nay?
 
 
*/

namespace VoiceNotButton
{
    public partial class Form1 : Form
    {
        //declaring global variables
        private Capture capture = null;        //takes images from camera as image frames
        private bool captureInProgress; // checks if capture is executing
        string InputData = String.Empty; // Khai báo string buff dùng cho hiển thị dữ liệu sau này.
        delegate void SetTextCallback(string text); // Khai bao delegate SetTextCallBack voi tham so string
        public Form1()
        {
            InitializeComponent();

            serialPort1.DataReceived += new SerialDataReceivedEventHandler(DataReceive);
            string[] BaudRate = { "1200", "2400", "4800", "9600", "19200", "38400", "57600", "115200" };
         
            comboBox5.Items.AddRange(BaudRate);




            CvInvoke.UseOpenCL = false;
            try
            {
                capture = new Capture();
                capture.ImageGrabbed += ProcessFrame;
            }
            catch (NullReferenceException excpt)
            {
                MessageBox.Show(excpt.Message);
            }
            // đo cường độ âm thanh

            NAudio.CoreAudioApi.MMDeviceEnumerator enumerator = new NAudio.CoreAudioApi.MMDeviceEnumerator();
            var devices = enumerator.EnumerateAudioEndPoints(NAudio.CoreAudioApi.DataFlow.All, NAudio.CoreAudioApi.DeviceState.Active);
            comboBox3.Items.AddRange(devices.ToArray());
        }
        private void ProcessFrame(object sender, EventArgs arg)
        {
            Mat frame = new Mat();
            capture.Retrieve(frame, 0);

            Mat image = frame; //Read the files as an 8-bit Bgr image  
            long detectionTime;
            List<Rectangle> faces = new List<Rectangle>();
            List<Rectangle> eyes = new List<Rectangle>();

            //The cuda cascade classifier doesn't seem to be able to load "haarcascade_frontalface_default.xml" file in this release
            //disabling CUDA module for now

            bool tryUseCuda = false;
            bool tryUseOpenCL = true;


            DetectFace.Detect(
              image, "haarcascade_frontalface_default.xml", "haarcascade_eye.xml",
              faces, eyes,
              tryUseCuda,
              tryUseOpenCL,
              out detectionTime);


            foreach (Rectangle face in faces)
            {
                CvInvoke.Rectangle(image, face, new Bgr(Color.Purple).MCvScalar, 3);
                Bitmap c = frame.Bitmap;
                Bitmap bmp = new Bitmap(face.Size.Width, face.Size.Height);
                Graphics g = Graphics.FromImage(bmp);
                g.DrawImage(c, 0, 0, face, GraphicsUnit.Pixel);

                dem_xla = face.Size.Width;


            }


            foreach (Rectangle eye in eyes)
                CvInvoke.Rectangle(image, eye, new Bgr(Color.Green).MCvScalar, 2);

            imageBox1.Image = frame;



        }
        private void ReleaseData()
        {
            if (capture != null)
                capture.Dispose();
        }

        void ham_thu_ngay()
        {
            string a2 = Convert.ToString(today).Trim();

            if (a2 == "Monday")
            {

                thu_ngay = "thứ hai";


            }
            if (a2 == "Tuesday")
            {

                thu_ngay = "thứ ba";

            }
            if (a2 == "Wednesday")
            {

                thu_ngay = "thứ tư";

            }
            if (a2 == "Thursday")
            {

                thu_ngay = "thứ năm";

            }
            if (a2 == "Friday")
            {

                thu_ngay = "thứ sáu";

            }
            if (a2 == "Saturday")
            {

                thu_ngay = "thứ bảy";

            }
            if (a2 == "Sunday")
            {

                thu_ngay = "chủ nhật";

            }

        }
        SpeechSynthesizer reader1 = new SpeechSynthesizer();
        string thoi_gian, date_time1, reminder1, date_time2, reminder2, date_time3, reminder3, date_time4, reminder4,
             date_time5, reminder5, date_time6, reminder6, date_time7, reminder7, date_time8, reminder8, date_time9, reminder9,
             date_time10, reminder10;
        string thoi_tiet_hom_nay, t_ngay, t_dem, am_lich_ngay, am_lich_thang, am_lich_nam;

        string thu_ngay;
        int k = 0, dem = 1, demfm = 1, dem_nhac = 0, dem_khoi_dong = 0, dem_buon = 0, dem_xla = 0, dem_button2 = 0, dem_noi2 = 0, dem_pin = 0, qua = 100, dem_am_thanh = 0;

        string nhac_toi1, nhac_toi2, nhac_toi3, nhac_toi4, nhac_toi5, nhac_toi6, nhac_toi7, nhac_toi8, nhac_toi9, nhac_toi10;
        string gio_nhac1, gio_nhac2, gio_nhac3, gio_nhac4, gio_nhac5, gio_nhac6, gio_nhac7, gio_nhac8, gio_nhac9, gio_nhac10;
        string chuoi_tam, gio_so_sanh, gio_chuyen_ngay = "0:01", nhac_toi_tam1;

        int[] mark2 = new int[10];
        int[] mark3 = new int[10];



        NAudio.Wave.WaveIn sourceStream = null;
        NAudio.Wave.DirectSoundOut waveOut = null;
        NAudio.Wave.WaveFileWriter waveWriter = null;
        int countVoiceDetected = 0;     // Tạo biến đếm lưu số lượng lần truy cập Google API trên key API hiện tại
        string RobotSpeaks;
        private void SpeakOtherLanguage(string text, string language)
        {
            RobotSpeaks = text;
            string url = string.Format("https://translate.googleapis.com/translate_tts?ie=UTF-8&&q=" + HttpUtility.UrlEncode(RobotSpeaks) + "&tl=" + language + "&total=1&idx=0&textlen={0}&client=gtx", RobotSpeaks.Length);
            var playThread = new Thread(() => PlayMp3FromUrl(url));
            playThread.IsBackground = true;
            playThread.Start();
        }

        int q;
        int deviceNumber = 0;
        int dem999 = 0;
        bool waiting = false;
        int t1, t2, t3, t4;

        bool wait = false;




        int count = 1;
        int count1 = 0;

        string s1003 = "đây";
        string s1004 = "là";
        string s1 = "ngày";
        string s2 = "thứ";
        string s3 = "music";
        string s4 = "trò";
        string s5 = "chuyện";
        string s6 = "bật";
        string s6a = "mở";
        string s7 = "tắt";
        string s8 = "đèn";
        string s9 = "tháng";
        string s10 = "năm";
        string s11 = "đi";
        string s36 = "chào";
        string s37 = "tuổi";
        string s38 = "tên";
        string s39 = "nghề";
        string s40 = "thích";
        string s12 = "name";
        string s13 = "old";
        string s14 = "are";
        string s15 = "nhạc";
        string s16 = "stop";
        string s17 = "job";
        string s18 = "from";
        string s19 = "play";
        string s20 = "how";
        string s21 = "where";
        string s22 = "yourself";
        string s23 = "Nice";
        string s24 = "help";
        string s25 = "beautifull";
        string s26 = "go";
        string s28 = "Hello";
        string s30 = "Hi";
        string s32 = "you";
        string s33 = "quạt";
        string s34 = "bơm";
        string s35 = "Know";
        string s50 = "time";
        string s51 = "nhạc";
        string s52 = "FM";
        string s53 = "kênh";
        string s54 = "đài";
        string s55 = "bài";
        string s56 = "dừng";
        string s57 = "lại";
        string s58 = "introduce";
        string s59 = "con";
        string s60 = "gì";
        string s61 = "cao";
        string s62 = "nhất";
        string s63 = "feel";
        string s64 = "tập";
        string s65 = "thể";
        string s66 = "dục";
        string s67 = "trả";
        string s68 = "lời";
        string s69 = "sai";
        string s70 = "kể";
        string s71 = "nhắc";
        string s72 = "quan";
        string s73 = "trọng";
        string s74 = "tôi";
        string s75 = "tuổi";
        string s76 = "tên";
        string s77 = "ai";
        string s78 = "người";
        string s79 = "thân";
        string s80 = "stupid";
        string s81 = "hate";
        string s82 = "beautiful";
        string s83 = "cách";
        string s84 = "sử";
        string s85 = "dụng";
        string s86 = "dance";
        string s87 = "địa";
        string s88 = "chỉ";
        string s89 = "nói";
        string s90 = "mấy";
        string s91 = "tiếng";
        string s92 = "chào";
        string s93 = "không";
        string s94 = "ngủ";
        string s95 = "cô";
        string s96 = "đơn";
        string s97 = "chán";
        string s98 = "nản";
        string s99 = "khi";
        string s100 = "nào";
        string s101 = "về";
        string s102 = "sức";
        string s103 = "khỏe";
        string s104 = "tốt";
        string s105 = "lại";
        string s106 = "giàu";
        string s107 = "tin";
        string s108 = "mập";
        string s109 = "mệt";
        string s110 = "bệnh";
        string s111 = "nhức";
        string s112 = "còn";
        string s113 = "sống";
        string s114 = "cuộc";
        string s115 = "đời";
        string s116 = "đẹp";
        string s117 = "tưởng";
        string s118 = "chưa";
        string s119 = "tắm";
        string s120 = "tóc";
        string s121 = "bạc";
        string s122 = "thức";
        string s123 = "ăn";
        string s124 = "ngon";
        string s125 = "tuyệt";
        string s126 = "nóng";
        string s127 = "nực";
        string s128 = "mưa";
        string s129 = "chuyện";
        string s130 = "đang";
        string s131 = "xảy";
        string s132 = "ra";
        string s133 = "hôm";
        string s134 = "nay";
        string s135 = "sinh";
        string s136 = "nhật";
        string s137 = "đang";
        string s138 = "nghĩ";
        string s139 = "rất";
        string s140 = "bận";
        string s141 = "thời";
        string s142 = "gian";
        string s143 = "rảnh";
        string s144 = "tivi";
        string s145 = "chiếu";
        string s146 = "phòng";
        string s147 = "dơ";
        string s148 = "muốn";
        string s149 = "chơi";
        string s150 = "số";
        string s151 = "điện";
        string s152 = "thoại";
        string s153 = "buồn";
        string s154 = "sạch";
        string s155 = "hôm";
        string s156 = "nay";
        string s157 = "đổi";
        string s158 = "mấy";
        string s159 = "giờ";

        string s186 = "gọi";
        string s187 = "trần";
        string s188 = "thiện";
        string s189 = "phước";
        string s190 = "văn";
        string s191 = "mau";
        string s192 = "mao";

        string s193 = "buổi";
        string s194 = "ngu";
        string s195 = "khùng";
        string s196 = "điên";
        string s197 = "robot";
        string s198 = "bạn";
        string s199 = "khỏe";
        string s200 = "ở";
        string s201 = "đâu";
        string s202 = "nghề";
        string s203 = "giới";
        string s204 = "thiệu";
        string s205 = "bản";
        string s206 = "thân";
        string s207 = "giúp";
        string s208 = "cảm";
        string s209 = "giác";
        string s210 = "xúc";
        string s211 = "dễ";
        string s212 = "thương";
        string s214 = "đáng";
        string s215 = "yêu";
        string s216 = "ghét";
        string s217 = "nhảy";
        string s218 = "múa";
        string s219 = "trời";
        string s220 = "nóng";
        string s221 = "lạnh";
        string s222 = "bị";
        string s223 = "đau";
        string s224 = "tạm";
        string s225 = "biệt";
        string s226 = "ai";
        string s227 = "chưa";
        string s228 = "thấy";
        string s229 = "ca";
        string s230 = "vợt";
        string s231 = "hiệu";
        string s232 = "trưởng";
        string s233 = "cơ";
        string s234 = "máy";
        string s235 = "chủ";
        string s236 = "nước";
        string s237 = "thủ";
        string s238 = "tướng";
        string s239 = "quốc";
        string s240 = "sư";
        string s241 = "đói";
        string s242 = "xạo";
        string s243 = "đông";
        string s244 = "nhất";
        string s245 = "biết";
        string s246 = "hát";
        string s247 = "chết";
        string s248 = "lịch";
        string s249 = "dài";
        string s250 = "xã";
        string s251 = "trai";
        string s252 = "quý";
        string s253 = "đường";
        string s254 = "quần";
        string s255 = "rộng";
        string s256 = "núi";
        string s257 = "chặt";
        string s258 = "điếc";
        string s259 = "từ";
        string s260 = "mua";
        string s261 = "bán";
        string s262 = "tay";
        string s263 = "ông";
        string s264 = "nữ";
        string s265 = "sở";
        string s266 = "cháy";
        string s267 = "nắng";
        string s268 = "bỏ";
        string s269 = "sữa";
        string s270 = "giống";
        string s271 = "voi";
        string s272 = "nhà";
        string s273 = "khô";
        string s274 = "lửa";
        string s275 = "than";
        string s276 = "rồng";
        string s277 = "bay";
        string s278 = "gỗ";
        string s279 = "sỏi";
        string s280 = "đi";
        string s281 = "đứng";
        string s282 = "thắng";
        string s283 = "thua";
        string s284 = "dê";
        string s285 = "ốc";
        string s286 = "tiến";
        string s287 = "lùi";
        string s288 = "hoa";
        string s289 = "đúng";
        string s290 = "giỏi";
        string s291 = "cải";
        string s292 = "phải";
        string s293 = "cây";
        string s294 = "e";
        string s295 = "cảm";
        string s296 = "ơn";
        string s297 = "thời";
        string s298 = "tiết";
        string s299 = "nhiệt";
        string s300 = "độ";
        string s301 = "âm";
        string s302 = "lịch";

        string s600 = "mình";
        string s601 = "tao";
        string s602 = "tớ";
        string s603 = "ta";
        string s604 = "tui";

        string s605 = "mày";
        string s606 = "mi";
        string s607 = "ngươi";
        string s608 = "cậu";
        string s609 = "anh";
        string s610 = "chị";




        string x;   //gửi dử liệu           
        int a;




        string[] gio = { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14", "15", "16", "17", "18", "19", "20", "21", "22", "23", "0" };
        string[] phut = { "00", "01", "02", "03", "04", "05", "06", "07", "08", "09", "10", "11", "12", "13", "14", "15", "16", "17"
                       , "18", "19", "20", "21", "22", "23", "24", "25", "26", "27", "28", "29", "30", "31", "32", "33", "34", "35"
                       , "36", "37", "38", "39", "40", "41", "42", "43", "44", "45", "46", "47", "48", "49", "50", "51", "52", "53"
                       , "54", "55", "56", "57", "58", "59"};
        string[] tuoi = { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14", "15", "16", "17", "18", "19", "20", "21", "22", "23", "24"
                        , "25", "26", "27", "28", "29", "30", "31", "32", "33", "34", "35", "36", "37", "38", "39", "40", "41", "42", "43", "44", "45", "46", "47"
                        , "48", "49", "50", "51", "52", "53", "54", "55", "56", "57", "58", "59", "60", "61", "62", "63", "64", "65", "66", "67", "68", "69", "70"
                        , "71", "72", "73", "74", "75", "76", "77", "78", "79", "80", "81", "82", "83", "84", "85", "86", "87", "88", "89", "90", "91", "92", "93"
                        , "94", "95", "96", "97", "98", "99", "100"};

        int dem10;
        int day = DateTime.Now.Day;
        int month = DateTime.Now.Month;
        int year = DateTime.Now.Year;
        int hour = DateTime.Now.Hour;
        int minute = DateTime.Now.Minute;
        DayOfWeek today = DateTime.Today.DayOfWeek;

        AutoResetEvent stop = new AutoResetEvent(false);


        private void PlayMp3FromUrl(string url)
        {
            using (System.IO.Stream ms = new MemoryStream())
            {
                using (System.IO.Stream stream = WebRequest.Create(url).GetResponse().GetResponseStream())
                {
                    byte[] buffer = new byte[32768];
                    int read;
                    while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        ms.Write(buffer, 0, read);
                    }
                }

                ms.Position = 0;
                using (WaveStream blockAlignedStream =
                    new BlockAlignReductionStream(
                        WaveFormatConversionStream.CreatePcmStream(
                            new Mp3FileReader(ms))))
                {
                    using (WaveOut waveOut = new WaveOut(WaveCallbackInfo.FunctionCallback()))
                    {
                        waveOut.Init(blockAlignedStream);
                        waveOut.PlaybackStopped += (sender, e) =>
                        {
                            waveOut.Stop();
                        };

                        waveOut.Play();
                        waiting = true;
                        stop.WaitOne(10000);
                        waiting = false;
                    }
                }
            }
        }


        private void sourceStream_DataAvailable(object sender, NAudio.Wave.WaveInEventArgs e)
        {
            if (waveWriter == null) return;

            waveWriter.WriteData(e.Buffer, 0, e.BytesRecorded);
            waveWriter.Flush();

        }

        async void do_tre(int ms_giay)
        {

            await Task.Delay(ms_giay);
        }
        void ham_nhac_toi()
        {


            switch (dem_nhac)
            {
                case 1:
                    gio_nhac1 = comboBox1.Items[2].ToString().ToUpper().Trim();
                    nhac_toi1 = chuoi_tam;
                    if (nhac_toi_tam1 != null)
                    {
                        gio_nhac1 = nhac_toi_tam1;
                        nhac_toi_tam1 = null;
                    }





                    break;
                case 2:
                    nhac_toi2 = chuoi_tam;
                    gio_nhac2 = comboBox1.Items[2].ToString().ToUpper().Trim();
                    if (nhac_toi_tam1 != null)
                        gio_nhac2 = nhac_toi_tam1;


                    break;
                case 3:
                    nhac_toi3 = chuoi_tam;
                    gio_nhac3 = comboBox1.Items[2].ToString().ToUpper().Trim();
                    if (nhac_toi_tam1 != null)
                        gio_nhac3 = nhac_toi_tam1;


                    break;
                case 4:
                    nhac_toi4 = chuoi_tam;
                    gio_nhac4 = comboBox1.Items[2].ToString().ToUpper().Trim();
                    if (nhac_toi_tam1 != null)
                        gio_nhac4 = nhac_toi_tam1;

                    break;

                case 5:
                    nhac_toi5 = chuoi_tam;
                    gio_nhac5 = comboBox1.Items[2].ToString().ToUpper().Trim();
                    if (nhac_toi_tam1 != null)
                        gio_nhac5 = nhac_toi_tam1;

                    break;
                case 6:
                    nhac_toi6 = chuoi_tam;
                    gio_nhac6 = comboBox1.Items[2].ToString().ToUpper().Trim();
                    if (nhac_toi_tam1 != null)
                        gio_nhac6 = nhac_toi_tam1;


                    break;
                case 7:
                    nhac_toi7 = chuoi_tam;
                    gio_nhac7 = comboBox1.Items[2].ToString().ToUpper().Trim();
                    if (nhac_toi_tam1 != null)
                        gio_nhac7 = nhac_toi_tam1;


                    break;
                case 8:
                    nhac_toi8 = chuoi_tam;
                    gio_nhac8 = comboBox1.Items[2].ToString().ToUpper().Trim();
                    if (nhac_toi_tam1 != null)
                        gio_nhac8 = nhac_toi_tam1;


                    break;
                case 9:
                    nhac_toi9 = chuoi_tam;
                    gio_nhac1 = comboBox1.Items[2].ToString().ToUpper().Trim();
                    if (nhac_toi_tam1 != null)
                        gio_nhac9 = nhac_toi_tam1;


                    break;
                case 10:
                    nhac_toi10 = chuoi_tam;
                    gio_nhac1 = comboBox1.Items[2].ToString().ToUpper().Trim();
                    if (nhac_toi_tam1 != null)
                        gio_nhac10 = nhac_toi_tam1;


                    break;
                case 11:
                    dem_nhac = 10;
                    string a1 = "Hôm nay bạn đã sử dụng 10 lần nhắc nhở rồi. Hãy sử dụng chức năng nhắc nhở này vào ngày mai nhé";
                    SpeakOtherLanguage(a1, "Vi");
                    Thread.Sleep(3000);
                    break;
            }




        }
        void laydulieu()
        {
            HtmlWeb hw = new HtmlWeb();
            string url = @"https://sites.google.com/a/student.hcmute.edu.vn/thienphuoc96/";
            HtmlAgilityPack.HtmlDocument doc = hw.Load(url);



            HtmlNodeCollection N11 = doc.DocumentNode.SelectNodes("//*[@id='COMP_48538947477936745']/div/div/div/table/tbody/tr[2]/td[2]/b");
            HtmlNodeCollection N12 = doc.DocumentNode.SelectNodes("//*[@id='COMP_48538947477936745']/div/div/div/table/tbody/tr[2]/td[3]/b");


            HtmlNodeCollection N21 = doc.DocumentNode.SelectNodes("//*[@id='COMP_48538947477936745']/div/div/div/table/tbody/tr[3]/td[2]/b[1]");
            HtmlNodeCollection N22 = doc.DocumentNode.SelectNodes("//*[@id='COMP_48538947477936745']/div/div/div/table/tbody/tr[3]/td[3]/b");


            HtmlNodeCollection N31 = doc.DocumentNode.SelectNodes("//*[@id='COMP_48538947477936745']/div/div/div/table/tbody/tr[4]/td[2]/b[1]");
            HtmlNodeCollection N32 = doc.DocumentNode.SelectNodes("//*[@id='COMP_48538947477936745']/div/div/div/table/tbody/tr[4]/td[3]/b");


            HtmlNodeCollection N41 = doc.DocumentNode.SelectNodes("//*[@id='COMP_48538947477936745']/div/div/div/table/tbody/tr[5]/td[2]/b[1]");
            HtmlNodeCollection N42 = doc.DocumentNode.SelectNodes("//*[@id='COMP_48538947477936745']/div/div/div/table/tbody/tr[5]/td[3]/b");


            HtmlNodeCollection N51 = doc.DocumentNode.SelectNodes("//*[@id='COMP_48538947477936745']/div/div/div/table/tbody/tr[6]/td[2]/b[1]");
            HtmlNodeCollection N52 = doc.DocumentNode.SelectNodes("//*[@id='COMP_48538947477936745']/div/div/div/table/tbody/tr[6]/td[3]/b");


            HtmlNodeCollection N61 = doc.DocumentNode.SelectNodes("//*[@id='COMP_48538947477936745']/div/div/div/table/tbody/tr[7]/td[2]/b[1]");
            HtmlNodeCollection N62 = doc.DocumentNode.SelectNodes("//*[@id='COMP_48538947477936745']/div/div/div/table/tbody/tr[7]/td[3]/b");


            HtmlNodeCollection N71 = doc.DocumentNode.SelectNodes("//*[@id='COMP_48538947477936745']/div/div/div/table/tbody/tr[8]/td[2]/b[1]");
            HtmlNodeCollection N72 = doc.DocumentNode.SelectNodes("//*[@id='COMP_48538947477936745']/div/div/div/table/tbody/tr[8]/td[3]/b");


            HtmlNodeCollection N81 = doc.DocumentNode.SelectNodes("//*[@id='COMP_48538947477936745']/div/div/div/table/tbody/tr[9]/td[2]/b[1]");
            HtmlNodeCollection N82 = doc.DocumentNode.SelectNodes("//*[@id='COMP_48538947477936745']/div/div/div/table/tbody/tr[9]/td[3]/b");


            HtmlNodeCollection N91 = doc.DocumentNode.SelectNodes("//*[@id='COMP_48538947477936745']/div/div/div/table/tbody/tr[10]/td[2]/b");
            HtmlNodeCollection N92 = doc.DocumentNode.SelectNodes("//*[@id='COMP_48538947477936745']/div/div/div/table/tbody/tr[10]/td[3]/b");



            HtmlNodeCollection N101 = doc.DocumentNode.SelectNodes("//*[@id='COMP_48538947477936745']/div/div/div/table/tbody/tr[11]/td[2]/b[1]");
            HtmlNodeCollection N102 = doc.DocumentNode.SelectNodes("//*[@id='COMP_48538947477936745']/div/div/div/table/tbody/tr[11]/td[3]/b");






            foreach (var item11 in N11)
            {

                date_time1 = Convert.ToString(item11.InnerText).Trim();


            }

            foreach (var item12 in N12)
            {
                reminder1 = Convert.ToString(item12.InnerText).Trim();


            }

            foreach (var item21 in N21)
            {
                date_time2 = Convert.ToString(item21.InnerText).Trim();


            }
            foreach (var item22 in N22)
            {
                reminder2 = Convert.ToString(item22.InnerText).Trim();


            }

            foreach (var item31 in N31)
            {
                date_time3 = Convert.ToString(item31.InnerText).Trim();


            }
            foreach (var item32 in N32)
            {
                reminder3 = Convert.ToString(item32.InnerText).Trim();


            }

            foreach (var item41 in N41)
            {
                date_time4 = Convert.ToString(item41.InnerText).Trim();

            }
            foreach (var item42 in N42)
            {
                reminder4 = Convert.ToString(item42.InnerText).Trim();

            }

            foreach (var item51 in N51)
            {
                date_time5 = Convert.ToString(item51.InnerText).Trim();

            }
            foreach (var item52 in N52)
            {
                reminder5 = Convert.ToString(item52.InnerText).Trim();

            }


            foreach (var item61 in N61)
            {
                date_time6 = Convert.ToString(item61.InnerText).Trim();


            }
            foreach (var item62 in N62)
            {
                reminder6 = Convert.ToString(item62.InnerText).Trim();

            }


            foreach (var item71 in N71)
            {
                date_time7 = Convert.ToString(item71.InnerText).Trim();

            }

            foreach (var item72 in N72)
            {
                reminder7 = Convert.ToString(item72.InnerText).Trim();

            }

            foreach (var item81 in N81)
            {
                date_time8 = Convert.ToString(item81.InnerText).Trim();

            }
            foreach (var item82 in N82)
            {
                reminder8 = Convert.ToString(item82.InnerText).Trim();

            }


            foreach (var item91 in N91)
            {
                date_time9 = Convert.ToString(item91.InnerText).Trim();

            }
            foreach (var item92 in N92)
            {
                reminder9 = Convert.ToString(item92.InnerText).Trim();

            }


            foreach (var item101 in N101)
            {
                date_time10 = Convert.ToString(item101.InnerText).Trim();

            }
            foreach (var item102 in N102)
            {
                reminder10 = Convert.ToString(item102.InnerText).Trim();

            }

             HtmlWeb am_lich = new HtmlWeb();
          string website = @"https://lichngaytot.com/xem-ngay-tot-xau.html";
          HtmlAgilityPack.HtmlDocument doc2 = am_lich.Load(website);



          HtmlNodeCollection ngay_am = doc2.DocumentNode.SelectNodes("//*[@id='showLich']/div/div[3]/div[1]/div[2]/div/div[2]/span");

           HtmlNodeCollection thang_am = doc2.DocumentNode.SelectNodes(" //*[@id='showLich']/div/div[3]/div[1]/div[2]/div/div[1]/strong");
           HtmlNodeCollection nam_am = doc2.DocumentNode.SelectNodes(" //*[@id='showLich']/div/div[3]/div[1]/div[1]/p[1]/strong");
          foreach (var item_ngay in ngay_am)
          {

              am_lich_ngay = Convert.ToString(item_ngay.InnerText).Trim();


          }
          foreach (var item_thang in thang_am)
           {

               am_lich_thang = Convert.ToString(item_thang.InnerText).Trim();


           }
          foreach (var item_nam in nam_am)
          {

              am_lich_nam = Convert.ToString(item_nam.InnerText).Trim();


          }
          HtmlWeb accuweather = new HtmlWeb();
          string trangweb = @"https://www.accuweather.com/vi/vn/ho-chi-minh-city/353981/weather-forecast/353981";
          HtmlAgilityPack.HtmlDocument doc1 = accuweather.Load(trangweb);



          HtmlNodeCollection today = doc1.DocumentNode.SelectNodes("//*[@id='feed-tabs']/ul/li[1]/div/div[2]/span");
          HtmlNodeCollection nhiet_do_ngay = doc1.DocumentNode.SelectNodes(" //*[@id='feed-tabs']/ul/li[1]/div[1]/div[2]/div");


          foreach (var item_today in today)
          {

              thoi_tiet_hom_nay = Convert.ToString(item_today.InnerText).Trim();


          }
          foreach (var item_t_ngay in nhiet_do_ngay)
          {

              t_ngay = Convert.ToString(item_t_ngay.InnerText).Trim();
              t_ngay = t_ngay[0].ToString() + t_ngay[1].ToString();

          }
            


        }


        WebRequest request = null;
        HttpWebResponse response = null;
        System.IO.Stream dataStream = null;
        StreamReader reader = null;

        private void button1_Click(object sender, EventArgs e)
        {
            comboBox1.Items.Clear();

            // timer1.Start();
            timer_am_thanh.Start();
            timer_dem_am_thanh.Start();
           // qua = 0;
            mark3[5] = 100;
            mark3[9] = 100;

            button1.Visible = false;
            button2.Visible = true;
            sourceStream = new NAudio.Wave.WaveIn();
            sourceStream.DeviceNumber = deviceNumber;
            sourceStream.WaveFormat = new NAudio.Wave.WaveFormat(16000, 1);

            sourceStream.DataAvailable += new EventHandler<NAudio.Wave.WaveInEventArgs>(sourceStream_DataAvailable);
            waveWriter = new NAudio.Wave.WaveFileWriter("hello.wav", sourceStream.WaveFormat);

            sourceStream.StartRecording();
            button1.Enabled = false;

        }


        private void timer1_Tick(object sender, EventArgs e)
        {

            dem999++;

            if (dem999 == 6)
            {
                timer1.Stop();

                button2.PerformClick();
                dem999 = 0;
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
           
            timer_pin.Start();
            comboBox4.DataSource = SerialPort.GetPortNames();//dữ liệu serial
            button11.PerformClick();
            comboBox5.SelectedIndex = 3;
            //button11.PerformClick();

            timer_xla.Start();





            button5.PerformClick();

            timer51P.Start();
            webBrowser1.Navigate("https://sites.google.com/a/student.hcmute.edu.vn/thienphuoc96/");

            laydulieu();

        }

        private void button2_Click(object sender, EventArgs e)
        {
            pictureBox_button2.Visible = false;
            pictureBox_button1.Visible = true;
            qua = 1;
            dem_button2++;
            button1.Visible = true;
           button2.Visible = false;
            button2.Enabled = false;
            StreamReader sr = File.OpenText(@"countVoiceDetected.txt");
            string scountVoiceDetected = sr.ReadLine();
            countVoiceDetected = Convert.ToInt32(scountVoiceDetected);
            countVoiceDetected++;
            sr.Close();
            if (waveOut != null)
            {
                waveOut.Stop();
                waveOut.Dispose();
                waveOut = null;
            }
            if (sourceStream != null)
            {
                sourceStream.StopRecording();
                sourceStream.Dispose();
                sourceStream = null;
            }
            if (waveWriter != null)
            {
                waveWriter.Dispose();
                waveWriter.Close();
                waveWriter = null;
            }

            try
            {
                FileStream fs = new FileStream("hello.wav", FileMode.OpenOrCreate);
                MemoryStream ms = new MemoryStream();
                ms.SetLength(fs.Length);
                fs.Read(ms.GetBuffer(), 0, (int)fs.Length);
                string connectString1 = "https://www.google.com/speech-api/v2/recognize?output=json&lang=en-us&key=AIzaSyBQLJM0YAI0IQLAvKveyo_wBDCwE-cC360   "; //add key vào vị trí keygoogle
                string connectString = "https://www.google.com/speech-api/v2/recognize?output=json&lang=vi-us&key=AIzaSyBQLJM0YAI0IQLAvKveyo_wBDCwE-cC360   "; //add key vào vị trí keygoogle

                ASCIIEncoding encoding = new ASCIIEncoding();

                byte[] data = ms.GetBuffer();

                request = WebRequest.Create(connectString1);

                request = WebRequest.Create(connectString);



                request.Method = "POST";
                request.ContentType = "audio/l16; rate=16000";

                request.ContentLength = data.Length;

                System.IO.Stream stream = request.GetRequestStream();
                stream.Write(data, 0, data.Length);
                stream.Close();
                fs.Close();     // Đóng file audio lại
                fs = new FileStream("reponse.html", FileMode.OpenOrCreate, FileAccess.ReadWrite);
                response = (HttpWebResponse)request.GetResponse();

                dataStream = response.GetResponseStream();
                reader = new StreamReader(dataStream);
                string responseFromServer = reader.ReadToEnd();
                string[] sourceSplit = { "\":\"", "\",\"", "\"},{\"" };
                string[] arraySesponse = responseFromServer.Split(sourceSplit, StringSplitOptions.RemoveEmptyEntries);
                StreamWriter sw = new StreamWriter(fs);
                sw.WriteLine(responseFromServer);
                sw.Flush();
                sw.Close();
                fs.Dispose();
                textBox1.Text = arraySesponse[1].ToLower();




                string txt1 = textBox1.Text.ToString(); ///chuỗi google trả về String rồi
                if (txt1 != null)
                {
                    string[] Array_1 = null;
                    char[] split_kytu = { ' ' };
                    Array_1 = txt1.Split(split_kytu);
                    count1 = 0;
                    for (int i = 0; i <= Array_1.Length - 1; i++)
                    {
                        comboBox1.Items.Add(Array_1[i]);
                        count1++;

                    }
                    chuoi_tam = null;

                    for (int i = 3; i <= Array_1.Length - 1; i++)
                    {

                        chuoi_tam = chuoi_tam + " " + Array_1[i];

                    }



                    count = 0;
                    for (int i = 0; i < txt1.Length - 1; i++)
                        if (txt1[i] == ' ' && txt1[i - 1] != ' ') count++;
                    textBox7.Text = Convert.ToString(count);
                    textBox5.Text = Convert.ToString(count1);







                    //Save Data
                    int dem2000 = 0;
                    for (int i = 0; i < count1 - 1; i++)//2222222222222222222222222222222222222222222222
                    {
                        if (comboBox1.Items[i].ToString().ToUpper() == s1003.ToUpper() || comboBox1.Items[i].ToString().ToUpper() == s1004.ToUpper())//dạy "đây là máy lạnh"
                        {
                            dem2000++;
                        }
                    }
                    if (dem2000 == 2)
                    {
                        button13.PerformClick();
                        button14.PerformClick();//tạo file text máy lạnh và lưu dữ liệu
                        //đi về đúng quỹ đạo đã dạy
                        button15.PerformClick();//Load data từ file máy lạnh lên//load [0] [1]
                        button18.PerformClick();
                    }


                    // Send Data to Arduino nha nha! 

                    int dem2001 = 0;
                    for (int i = 0; i <= comboBox1.Items.Count - 1; i++)// máy lạnh ở đâu
                    {


                        button19.PerformClick();
                        button19.Enabled = false;
                        for (int j = 0; j <= comboBox7.Items.Count - 1; j++)
                        {


                            if (comboBox1.Items[i].ToString().ToUpper() == comboBox7.Items[j].ToString().ToUpper())// máy lạnh ở đâu
                            {
                                dem2001++;
                            }

                        }
                        if (dem2001 == 2)
                        {

                            int k;

                            for (k = 0; k <= comboBox7.Items.Count - 3; k++)
                            {
                                if (Convert.ToString(comboBox7.Items[k]) == "h") { serialPort1.Write("H"); Thread.Sleep(510); }//textBox11.Text =  ""; }
                                else if (Convert.ToString(comboBox7.Items[k]) == "k") { serialPort1.Write("K"); Thread.Sleep(1020); }// textBox11.Text = ""; }
                                else if (Convert.ToString(comboBox7.Items[k]) == "g") { serialPort1.Write("G"); Thread.Sleep(1450); }// textBox11.Text = ""; }
                                else if (Convert.ToString(comboBox7.Items[k]) == "l") { serialPort1.Write("L"); Thread.Sleep(1900); }// textBox11.Text = ""; }
                                else if (Convert.ToString(comboBox7.Items[k]) == "f") { serialPort1.Write("F"); Thread.Sleep(2250); }// textBox11.Text = ""; }
                                else if (Convert.ToString(comboBox7.Items[k]) == "e") { serialPort1.Write("E"); Thread.Sleep(2450); }// textBox11.Text = ""; }

                                else if (Convert.ToString(comboBox7.Items[k]) == "p") { serialPort1.Write("P"); Thread.Sleep(510); }//textBox11.Text = ""; }
                                else if (Convert.ToString(comboBox7.Items[k]) == "o") { serialPort1.Write("O"); Thread.Sleep(1020); }// textBox11.Text = ""; }
                                else if (Convert.ToString(comboBox7.Items[k]) == "i") { serialPort1.Write("I"); Thread.Sleep(1450); }// textBox11.Text = ""; }
                                else if (Convert.ToString(comboBox7.Items[k]) == "u") { serialPort1.Write("U"); Thread.Sleep(1900); }// textBox11.Text = ""; }
                                else if (Convert.ToString(comboBox7.Items[k]) == "y") { serialPort1.Write("Y"); Thread.Sleep(2250); }// textBox11.Text = ""; }
                                else if (Convert.ToString(comboBox7.Items[k]) == "m") { serialPort1.Write("M"); Thread.Sleep(2450); }// textBox11.Text = ""; }

                                else if (Convert.ToString(comboBox7.Items[k]) == "z")
                                {
                                }


                                else if (Convert.ToString(comboBox7.Items[k]) != "z" || Convert.ToString(comboBox7.Items[k]) != "m" || Convert.ToString(comboBox7.Items[k]) != "y" || Convert.ToString(comboBox7.Items[k]) != "u" || Convert.ToString(comboBox7.Items[k]) != "i" || Convert.ToString(comboBox7.Items[k]) != "o" || Convert.ToString(comboBox7.Items[k]) != "p" || Convert.ToString(comboBox7.Items[k]) != "h" || Convert.ToString(comboBox7.Items[k]) != "k" || Convert.ToString(comboBox7.Items[k]) != "g" || Convert.ToString(comboBox7.Items[k]) != "l" || Convert.ToString(comboBox7.Items[k]) != "f" || Convert.ToString(comboBox7.Items[k]) != "e" || Convert.ToString(comboBox7.Items[k]) != "")
                                {
                                    serialPort1.Write(Convert.ToString(comboBox7.Items[k]));
                                    serialPort1.Write("z");//đi
                                }
                            }




                            serialPort1.Write("Q");//dừng

                            textBox14.Text = Convert.ToString(serialPort1.ReadChar());
                            if (textBox14.Text == "109" || textBox14.Text == "106" || textBox14.Text == "111")// 109 là mã hóa của kí tự 'j' từ arduino gửi lên
                            {
                                string d = "đã đến vị trí bạn cần tìm,tôi xin phép đi về nha,cảm ơn";
                                SpeakOtherLanguage(d, "Vi");

                            }
                            else
                            { }

                            serialPort1.Write("v");//quay 180 trề hướng ban đầu
                            Thread.Sleep(4900);


                            for (k = comboBox7.Items.Count - 3; k >= 0; k--)//đi về nha
                            {
                                if (Convert.ToString(comboBox7.Items[k]) == "h") { serialPort1.Write("P"); Thread.Sleep(510); }//textBox11.Text = ""; }
                                else if (Convert.ToString(comboBox7.Items[k]) == "k") { serialPort1.Write("O"); Thread.Sleep(1020); }// textBox11.Text = ""; }
                                else if (Convert.ToString(comboBox7.Items[k]) == "g") { serialPort1.Write("I"); Thread.Sleep(1450); }//textBox11.Text = ""; }
                                else if (Convert.ToString(comboBox7.Items[k]) == "l") { serialPort1.Write("U"); Thread.Sleep(1900); }// textBox11.Text = ""; }
                                else if (Convert.ToString(comboBox7.Items[k]) == "f") { serialPort1.Write("Y"); Thread.Sleep(2250); }//textBox11.Text = ""; }
                                else if (Convert.ToString(comboBox7.Items[k]) == "e") { serialPort1.Write("M"); Thread.Sleep(2450); }//textBox11.Text = ""; }

                                else if (Convert.ToString(comboBox7.Items[k]) == "p") { serialPort1.Write("H"); Thread.Sleep(510); }// textBox11.Text = ""; }
                                else if (Convert.ToString(comboBox7.Items[k]) == "o") { serialPort1.Write("K"); Thread.Sleep(1020); }// textBox11.Text = ""; }
                                else if (Convert.ToString(comboBox7.Items[k]) == "i") { serialPort1.Write("G"); Thread.Sleep(1450); }//textBox11.Text = ""; }
                                else if (Convert.ToString(comboBox7.Items[k]) == "u") { serialPort1.Write("L"); Thread.Sleep(1900); }//textBox11.Text = ""; }
                                else if (Convert.ToString(comboBox7.Items[k]) == "y") { serialPort1.Write("F"); Thread.Sleep(2250); }//textBox11.Text = ""; }
                                else if (Convert.ToString(comboBox7.Items[k]) == "m") { serialPort1.Write("E"); Thread.Sleep(2450); }// textBox11.Text = ""; }

                                else if (Convert.ToString(comboBox7.Items[k]) == "z")
                                {
                                }



                                else if (Convert.ToString(comboBox7.Items[k]) != "z" || Convert.ToString(comboBox7.Items[k]) != "m" || Convert.ToString(comboBox7.Items[k]) != "y" || Convert.ToString(comboBox7.Items[k]) != "u" || Convert.ToString(comboBox7.Items[k]) != "i" || Convert.ToString(comboBox7.Items[k]) != "o" || Convert.ToString(comboBox7.Items[k]) != "p" || Convert.ToString(comboBox7.Items[k]) != "h" || Convert.ToString(comboBox7.Items[k]) != "k" || Convert.ToString(comboBox7.Items[k]) != "g" || Convert.ToString(comboBox7.Items[k]) != "l" || Convert.ToString(comboBox7.Items[k]) != "f" || Convert.ToString(comboBox7.Items[k]) != "e" || Convert.ToString(comboBox7.Items[k]) != "")
                                {
                                    serialPort1.Write(Convert.ToString(comboBox7.Items[k]));//về
                                    serialPort1.Write("z");
                                }
                            }
                            //serialPort1.Write("q");
                            serialPort1.Write("v");
                            Thread.Sleep(4900);
                            serialPort1.Write("c");//về home xong...tiến hành sạc hay không sạc

                            textBox11.Text = "";
                            dem2001 = 0;
                            comboBox1.Items.Clear();
                            comboBox7.Items.Clear();
                            button19.Enabled = true;
                            break;

                        }

                    }

                    int dem2 = 0;///bật đèn
                    for (int i = 0; i <= count1 - 1; i++)//gia đình
                    {
                        if (comboBox1.Items[i].ToString().ToUpper() == s6.ToUpper() || comboBox1.Items[i].ToString().ToUpper() == s8.ToUpper())
                        {
                            dem2++;
                        }
                    }
                    if (dem2 == 2)
                    {
                        comboBox2.DataSource = System.IO.Ports.SerialPort.GetPortNames();

                        serialPort1.Write("1");

                    }


                    int dem3 = 0;//tắt đèn
                    for (int i = 0; i <= count1 - 1; i++)//gia đình
                    {
                        if (comboBox1.Items[i].ToString().ToUpper() == s7.ToUpper() || comboBox1.Items[i].ToString().ToUpper() == s8.ToUpper())
                        {
                            dem3++;
                        }
                    }
                    if (dem3 == 2)
                    {
                        comboBox2.DataSource = System.IO.Ports.SerialPort.GetPortNames();

                        serialPort1.Write("0");

                    }

                    int dem1 = 0;
                    for (int i = 0; i <= count - 1; i++)
                    {
                        if (comboBox1.Items[i].ToString().ToUpper() == s1.ToUpper() || comboBox1.Items[i].ToString().ToUpper() == s9.ToUpper() || comboBox1.Items[i].ToString().ToUpper() == s10.ToUpper() || comboBox1.Items[i].ToString().ToUpper() == s2.ToUpper())
                        {
                            dem1++;
                        }

                    }


                    int dem9 = 0;
                    for (int i = 0; i <= count1 - 1; i++)//gia đình
                    {
                        if (comboBox1.Items[i].ToString().ToUpper() == s4.ToUpper() || comboBox1.Items[i].ToString().ToUpper() == s5.ToUpper())
                        {
                            dem9++;
                        }
                    }



                    if (dem9 == 2)
                    {
                        string a1 = "Đươc thôi,tôi thích điều đó";
                        SpeakOtherLanguage(a1, "Vi");
                        Thread.Sleep(4000);
                    }



                    byte dem10 = 0;
                    byte dem11 = 0;
                    byte dem12 = 0;
                    byte dem13 = 0;
                    byte dem14 = 0;
                    byte dem15 = 0;
                    byte dem16 = 0;
                    byte dem17 = 0;
                    byte dem18 = 0;


                    byte dem19 = 0;
                    byte dem20 = 0;
                    byte dem21 = 0;
                    byte dem22 = 0;
                    byte dem23 = 0;
                    byte dem24 = 0;
                    byte dem25 = 0;
                    byte dem26 = 0;
                    byte dem27 = 0;

                    byte dem28 = 0;
                    byte dem29 = 0;
                    byte dem30 = 0;
                    byte dem31 = 0;
                    byte dem32 = 0;
                    byte dem33 = 0;
                    byte dem34 = 0;
                    byte dem35 = 0;
                    byte dem36 = 0;
                    byte dem37 = 0;
                    byte dem38 = 0;
                    byte dem39 = 0;

                    byte dem40 = 0;
                    byte dem41 = 0;
                    byte dem42 = 0;
                    byte dem43 = 0;
                    byte dem44 = 0;
                    byte dem45 = 0;
                    byte dem46 = 0;
                    byte dem47 = 0;
                    byte dem48 = 0;
                    byte dem49 = 0;
                    byte dem50 = 0;
                    byte dem51 = 0;
                    byte dem52 = 0;
                    byte dem53 = 0;
                    byte dem54 = 0;
                    byte dem55 = 0;
                    byte dem56 = 0;
                    byte dem57 = 0;
                    byte dem58 = 0;
                    byte dem59 = 0;
                    byte dem60 = 0;
                    byte dem61 = 0;
                    byte dem62 = 0;
                    byte dem63 = 0;
                    byte dem64 = 0;
                    byte dem65 = 0;
                    byte dem66 = 0;
                    byte dem67 = 0;
                    byte dem68 = 0;
                    byte dem69 = 0;
                    byte dem70 = 0;
                    byte dem71 = 0;
                    byte dem72 = 0;
                    byte dem73 = 0;



                    byte dem84 = 0;
                    byte dem85 = 0;
                    byte dem86 = 0;
                    byte dem87 = 0;
                    byte dem88 = 0;
                    byte dem89 = 0;
                    byte dem90 = 0;
                    byte dem91 = 0;
                    byte dem92 = 0;
                    byte dem93 = 0;
                    byte dem94 = 0;
                    byte dem95 = 0;
                    byte dem96 = 0;
                    byte dem97 = 0;
                    byte dem98 = 0;
                    byte dem99 = 0;
                    byte dem100 = 0;
                    byte dem101 = 0;
                    byte dem102 = 0;

                    //26/05/18
                    byte dem103 = 0;
                    byte dem104 = 0;
                    byte dem105 = 0;
                    byte dem106 = 0;
                    byte dem107 = 0;
                    byte dem108 = 0;
                    byte dem109 = 0;
                    byte dem110 = 0;
                    byte dem111 = 0;
                    byte dem112 = 0;
                    byte dem113 = 0;
                    byte dem114 = 0;
                    byte dem115 = 0;
                    byte dem116 = 0;
                    byte dem117 = 0;
                    byte dem118 = 0;
                    byte dem119 = 0;
                    byte dem120 = 0;
                    byte dem121 = 0;
                    byte dem122 = 0;
                    byte dem123 = 0;
                    byte dem124 = 0;
                    byte dem125 = 0;
                    byte dem126 = 0;
                    byte dem127 = 0;
                    byte dem128 = 0;
                    byte dem129 = 0;
                    byte dem130 = 0;
                    byte dem131 = 0;
                    byte dem132 = 0;
                    byte dem133 = 0;
                    byte dem134 = 0;
                    byte dem135 = 0;
                    byte dem136 = 0;
                    byte dem137 = 0;
                    byte dem138 = 0;
                    byte dem139 = 0;
                    byte dem140 = 0;
                    byte dem141 = 0;
                    byte dem142 = 0;
                    byte dem143 = 0;
                    byte dem144 = 0;
                    byte dem145 = 0;
                    byte dem146 = 0;
                    byte dem147 = 0;
                    byte dem148 = 0;
                    byte dem149 = 0;
                    byte dem150 = 0;
                    byte dem151 = 0;
                    byte dem152 = 0;
                    byte dem153 = 0;
                    byte dem154 = 0;
                    byte dem155 = 0;
                    
                   


                    for (int i = 0; i <= count1 - 1; i++)//gia đình
                    {
                        string phuoc = comboBox1.Items[i].ToString().ToUpper();
                        bool dai_tu_xung_ho_1 = Convert.ToBoolean(phuoc == s74.ToUpper() || phuoc == s601.ToUpper() || phuoc == s602.ToUpper() || phuoc == s603.ToUpper() || phuoc == s604.ToUpper() || phuoc == s600.ToUpper());
                        bool dai_tu_xung_ho_2 = Convert.ToBoolean(phuoc == s605.ToUpper() || phuoc == s606.ToUpper() || phuoc == s607.ToUpper() || phuoc == s608.ToUpper() || phuoc == s197.ToUpper() || phuoc == s198.ToUpper() || phuoc == s609.ToUpper()
                           || phuoc == s610.ToUpper());
                        string S = comboBox1.Items[i].ToString().ToUpper();
                        if (comboBox1.Items[i].ToString().ToUpper() == s12.ToUpper())
                            dem10++;
                        if (comboBox1.Items[i].ToString().ToUpper() == s13.ToUpper() || phuoc == s20.ToUpper() || phuoc == s32.ToUpper() || phuoc == s14.ToUpper())// hoi tuoi
                            dem11++;
                        if ((comboBox1.Items[i].ToString().ToUpper() == s20.ToUpper() || comboBox1.Items[i].ToString().ToUpper() == s32.ToUpper()) && dem11 != 4)// suc khoe
                            dem12++;
                        if (comboBox1.Items[i].ToString().ToUpper() == s18.ToUpper())//quê quán
                            dem13++;
                        if (comboBox1.Items[i].ToString().ToUpper() == s17.ToUpper())//nghề nghiệp
                            dem14++;
                        if (comboBox1.Items[i].ToString().ToUpper() == s22.ToUpper() || phuoc == s57.ToUpper())// Giới Thiệu
                            dem15++;
                        if (comboBox1.Items[i].ToString().ToUpper() == s23.ToUpper())// Chào
                            dem16++;
                        if (comboBox1.Items[i].ToString().ToUpper() == s24.ToUpper())// giúp đỡ
                            dem17++;
                        if (comboBox1.Items[i].ToString().ToUpper() == s28.ToUpper() || comboBox1.Items[i].ToString().ToUpper() == s30.ToUpper())// chào
                            dem18++;
                        if (phuoc == s50.ToUpper())
                            dem19++;
                        if (phuoc == s51.ToUpper())
                            dem20++;

                        if (phuoc == s52.ToUpper())
                            dem21++;
                        if (phuoc == s53.ToUpper() || phuoc == s54.ToUpper())
                            dem22++;
                        if (phuoc == s55.ToUpper() || phuoc == s157.ToUpper())
                        {
                            dem23++;
                        }

                        if (phuoc == s56.ToUpper() || phuoc == s57.ToUpper())
                        {

                            dem24++;
                        }

                        if (phuoc == s61.ToUpper() || phuoc == s62.ToUpper())
                            dem25++;
                        if (phuoc == s20.ToUpper() || phuoc == s63.ToUpper())
                            dem26++;
                        if (phuoc == s64.ToUpper() || phuoc == s65.ToUpper() || phuoc == s66.ToUpper())
                            dem27++;
                        if (phuoc == s67.ToUpper() || phuoc == s68.ToUpper() || phuoc == s69.ToUpper())
                            dem28++;
                        if (phuoc == s70.ToUpper() || phuoc == s5.ToUpper())
                            dem29++;
                        if (phuoc == s71.ToUpper() || phuoc == s1.ToUpper() || phuoc == s72.ToUpper() || phuoc == s73.ToUpper())
                            dem30++;
                        if (dai_tu_xung_ho_1 || phuoc == s75.ToUpper())
                            dem31++;
                        if (dai_tu_xung_ho_1 || phuoc == s76.ToUpper())
                            dem32++;
                        if (phuoc == s77.ToUpper() || phuoc == s78.ToUpper() || phuoc == s79.ToUpper())
                            dem33++;
                        if (phuoc == s80.ToUpper())
                            dem34++;
                        if (phuoc == s81.ToUpper() || phuoc == s32.ToUpper())
                            dem35++;
                        if (phuoc == s32.ToUpper() || phuoc == s82.ToUpper())
                            dem36++;
                        if (phuoc == s83.ToUpper() || phuoc == s84.ToUpper() || phuoc == s85.ToUpper())
                            dem37++;
                        if (phuoc == s32.ToUpper() || phuoc == s86.ToUpper())
                            dem38++;
                        if (phuoc == s87.ToUpper() || phuoc == s88.ToUpper() || dai_tu_xung_ho_1)
                            dem39++;
                        if (phuoc == s89.ToUpper() || phuoc == s90.ToUpper() || phuoc == s91.ToUpper())
                            dem40++;
                        if (phuoc == s92.ToUpper())
                            dem41++;
                        if (phuoc == s93.ToUpper() || phuoc == s153.ToUpper())
                            dem42++;
                        if (dai_tu_xung_ho_1 || phuoc == s93.ToUpper() || phuoc == s94.ToUpper())
                            dem43++;
                        if (dai_tu_xung_ho_1 || phuoc == s95.ToUpper() || phuoc == s96.ToUpper() || phuoc == s97.ToUpper() || phuoc == s98.ToUpper())
                            dem44++;
                        if (phuoc == s99.ToUpper() || phuoc == s100.ToUpper() || phuoc == s59.ToUpper() || phuoc == s101.ToUpper())
                            dem45++;
                        if (phuoc == s102.ToUpper() || phuoc == s103.ToUpper() || phuoc == s104.ToUpper())
                            dem46++;
                        if (phuoc == s67.ToUpper() || phuoc == s68.ToUpper() || phuoc == s105.ToUpper())
                            dem47++;
                        if (phuoc == s106.ToUpper())
                            dem48++;
                        if (dai_tu_xung_ho_1 || phuoc == s93.ToUpper() || phuoc == s107.ToUpper())
                            dem49++;
                        if (dai_tu_xung_ho_1 || phuoc == s108.ToUpper())
                            dem50++;
                        if (dai_tu_xung_ho_1 || phuoc == s109.ToUpper())
                            dem51++;
                        if (dai_tu_xung_ho_1 || phuoc == s110.ToUpper())
                            dem52++;
                        if (dai_tu_xung_ho_1 || phuoc == s111.ToUpper())
                            dem53++;
                        if (dai_tu_xung_ho_1 || phuoc == s112.ToUpper() || phuoc == s113.ToUpper())
                            dem54++;
                        if (phuoc == s114.ToUpper() || phuoc == s115.ToUpper() || phuoc == s116.ToUpper() || phuoc == s117.ToUpper() || dai_tu_xung_ho_1)
                            dem55++;
                        if (dai_tu_xung_ho_1 || phuoc == s118.ToUpper() || phuoc == s119.ToUpper())
                            dem56++;
                        if (phuoc == s120.ToUpper() || dai_tu_xung_ho_1 || phuoc == s121.ToUpper())
                            dem57++;
                        if (phuoc == s122.ToUpper() || phuoc == s123.ToUpper() || phuoc == s93.ToUpper() || phuoc == s124.ToUpper())
                            dem58++;
                        if (phuoc == s125.ToUpper())
                            dem59++;
                        if (phuoc == s126.ToUpper() || phuoc == s127.ToUpper())
                            dem60++;
                        if (phuoc == s128.ToUpper())
                            dem61++;
                        if (phuoc == s129.ToUpper() || phuoc == s60.ToUpper() || phuoc == s130.ToUpper() || phuoc == s131.ToUpper() || phuoc == s132.ToUpper())
                            dem62++;
                        if (phuoc == s133.ToUpper() || phuoc == s134.ToUpper() || phuoc == s135.ToUpper() || phuoc == s136.ToUpper() || dai_tu_xung_ho_1)
                            dem63++;
                        if (phuoc == s137.ToUpper() || phuoc == s138.ToUpper() || phuoc == s60.ToUpper())
                            dem64++;
                        if (dai_tu_xung_ho_1 || phuoc == s139.ToUpper() || phuoc == s140.ToUpper())
                            dem65++;
                        if (phuoc == s141.ToUpper() || phuoc == s142.ToUpper() || phuoc == s143.ToUpper())
                            dem66++;
                        if (phuoc == s144.ToUpper() || phuoc == s145.ToUpper())
                            dem67++;
                        if (phuoc == s146.ToUpper() || phuoc == s93.ToUpper() || phuoc == s154.ToUpper())
                            dem68++;
                        if (dai_tu_xung_ho_1 || phuoc == s148.ToUpper() || phuoc == s11.ToUpper())
                            dem69++;
                        if (phuoc == s150.ToUpper() || phuoc == s151.ToUpper() || phuoc == s152.ToUpper() || dai_tu_xung_ho_1)
                            dem70++;
                        if (dai_tu_xung_ho_1 || phuoc == s71.ToUpper())
                            dem71++;
                        if (phuoc == s1.ToUpper() || phuoc == s2.ToUpper() || phuoc == s155.ToUpper() || phuoc == s156.ToUpper())
                            dem72++;
                        if ((phuoc == s158.ToUpper() || phuoc == s159.ToUpper()) && dem154<2)
                            dem73++;

                        if (phuoc == s186.ToUpper() || phuoc == s187.ToUpper() || phuoc == s188.ToUpper() || phuoc == s189.ToUpper())
                            dem84++;
                        if (phuoc == s186.ToUpper() || phuoc == s187.ToUpper() || phuoc == s190.ToUpper() || phuoc == s191.ToUpper() || phuoc == s192.ToUpper())
                            dem85++;
                        if (phuoc == s36.ToUpper() || phuoc == s193.ToUpper())
                            dem86++;
                        if (phuoc == s194.ToUpper() || phuoc == s195.ToUpper() || phuoc == s196.ToUpper())
                            dem87++;
                        if (dai_tu_xung_ho_2 || phuoc == s76.ToUpper())
                            dem88++;
                        if (dai_tu_xung_ho_2 || phuoc == s199.ToUpper())
                            dem89++;
                        if (dai_tu_xung_ho_2 || phuoc == s37.ToUpper())
                            dem90++;
                        if (dai_tu_xung_ho_2 || phuoc == s200.ToUpper() || phuoc == s201.ToUpper())
                            dem91++;
                        if (dai_tu_xung_ho_2 || phuoc == s202.ToUpper())
                            dem92++;
                        if (phuoc == s203.ToUpper() || phuoc == s204.ToUpper() || phuoc == s205.ToUpper() || phuoc == s206.ToUpper())
                            dem93++;
                        if (dai_tu_xung_ho_2 || phuoc == s207.ToUpper())
                            dem94++;
                        if (phuoc == s208.ToUpper() || phuoc == s209.ToUpper() || phuoc == s210.ToUpper() || dai_tu_xung_ho_2)
                            dem95++;
                        if (phuoc == s212.ToUpper() || phuoc == s214.ToUpper())
                            dem96++;
                        if (dai_tu_xung_ho_1 || phuoc == s216.ToUpper() || dai_tu_xung_ho_2)
                            dem97++;
                        if (dai_tu_xung_ho_2 || phuoc == s217.ToUpper() || phuoc == s218.ToUpper())
                            dem98++;
                        if (phuoc == s219.ToUpper() || phuoc == s220.ToUpper())
                            dem99++;
                        if (phuoc == s219.ToUpper() || phuoc == s221.ToUpper())
                            dem100++;
                        if (dai_tu_xung_ho_1 || phuoc == s222.ToUpper() || phuoc == s223.ToUpper())
                            dem101++;
                        if (phuoc == s224.ToUpper() || phuoc == s225.ToUpper())
                            dem102++;

                        if (dai_tu_xung_ho_1 || S == s215.ToUpper() || S == s198.ToUpper())
                            dem103++;
                        if (S == s215.ToUpper() || S == s226.ToUpper() || S == s227.ToUpper())
                            dem104++;
                        if (S == s228.ToUpper() || S == s134.ToUpper() || dai_tu_xung_ho_1)
                            dem105++;
                        if (S == s40.ToUpper() || S == s229.ToUpper() || S == s100.ToUpper())
                            dem106++;
                        if (S == s230.ToUpper())
                            dem107++;
                        if (S == s231.ToUpper() || S == s232.ToUpper() || S == s240.ToUpper())
                            dem108++;
                        if (S == s232.ToUpper() || S == s233.ToUpper() || S == s234.ToUpper())
                            dem109++;
                        if (S == s235.ToUpper() || S == s236.ToUpper())
                            dem110++;
                        if (S == s237.ToUpper() || S == s238.ToUpper())
                            dem111++;
                        if (S == s235.ToUpper() || S == s239.ToUpper())
                            dem112++;
                        if (S == s153.ToUpper())
                            dem113++;
                        if (S == s241.ToUpper())
                            dem114++;
                        if (S == s242.ToUpper())
                            dem115++;
                        if (S == s40.ToUpper() || S == s198.ToUpper())
                            dem116++;
                        if (S == s243.ToUpper() || S == s236.ToUpper())
                            dem117++;
                        if (S == s291.ToUpper() || S == s246.ToUpper())
                            dem118++;
                        if (S == s113.ToUpper() || S == s247.ToUpper())
                            dem119++;
                        if (S == s148.ToUpper() || S == s60.ToUpper())
                            dem120++;
                        if (S == s289.ToUpper() || S == s290.ToUpper())
                            dem121++;
                        if (S == s248.ToUpper() || S == s249.ToUpper())
                            dem122++;
                        if (S == s250.ToUpper() || S == s100.ToUpper())
                            dem123++;
                        if (S == s251.ToUpper() || S == s252.ToUpper())
                            dem124++;

                        if (S == s253.ToUpper() || S == s249.ToUpper())
                            dem125++;
                        if (S == s254.ToUpper() || S == s255.ToUpper())
                            dem126++;
                        if (S == s256.ToUpper() || S == s257.ToUpper())
                            dem127++;
                        if (S == s258.ToUpper())
                            dem128++;
                        if (S == s259.ToUpper() || S == s69.ToUpper())
                            dem129++;
                        if (S == s260.ToUpper() || S == s261.ToUpper())
                            dem130++;
                        if (S == s292.ToUpper() || S == s262.ToUpper())
                            dem131++;
                        if (S == s251.ToUpper() || S == s263.ToUpper())
                            dem132++;
                        if (S == s233.ToUpper() || S == s264.ToUpper())
                            dem133++;
                        if (S == s265.ToUpper() || S == s266.ToUpper())
                            dem134++;
                        if (S == s267.ToUpper() || S == s268.ToUpper())
                            dem135++;
                        if (S == s149.ToUpper() || S == s236.ToUpper())
                            dem136++;
                        if (S == s263.ToUpper() || S == s269.ToUpper())
                            dem137++;
                        if (S == s270.ToUpper() || S == s271.ToUpper())
                            dem138++;
                        if (S == s272.ToUpper() || S == s273.ToUpper())
                            dem139++;
                        if (S == s274.ToUpper() || S == s275.ToUpper())
                            dem140++;
                        if (S == s276.ToUpper() || S == s277.ToUpper())
                            dem141++;
                        if (S == s278.ToUpper() || S == s279.ToUpper())
                            dem142++;
                        if (S == s280.ToUpper() || S == s281.ToUpper())
                            dem143++;
                        if (S == s282.ToUpper() || S == s283.ToUpper())
                            dem144++;
                        if (S == s284.ToUpper() || S == s285.ToUpper())
                            dem145++;
                        if (S == s286.ToUpper() || S == s287.ToUpper())
                            dem146++;
                        if (S == s288.ToUpper() || S == s245.ToUpper())
                            dem147++;
                        if (S == s293.ToUpper() )
                            dem148++;
                        if (S == s123.ToUpper() || S == s60.ToUpper())
                            dem149++;
                        if (S == s25.ToUpper())
                            dem150++;
                        if (S == s294.ToUpper() || S == s200.ToUpper())
                            dem151++;
                        if (dai_tu_xung_ho_1  || S == s103.ToUpper())
                            dem152++;
                        if (S == s295.ToUpper() || S == s296.ToUpper())
                            dem153++;
                        if (S == s297.ToUpper() || S == s298.ToUpper()||S == s299.ToUpper() || S == s300.ToUpper())
                            dem154++;
                        if (S == s301.ToUpper() || S == s302.ToUpper())
                            dem155++;
                        
                    }


                       if (dem10 == 1)
                    {
                        string a1 = "My name is vivo robot";
                        SpeakOtherLanguage(a1, "En");
                        Thread.Sleep(1000);
                    }

                    if (dem11 == 4)
                    {
                        string a1 = "I'm 2 years old";
                        SpeakOtherLanguage(a1, "En");
                        Thread.Sleep(1000);

                    }
                    if (dem12 == 2)
                    {
                        string a1 = "I'm Fine,Thank you";
                        SpeakOtherLanguage(a1, "En");
                        Thread.Sleep(2000);
                    }
                    if (dem13 == 1)
                    {
                        string a1 = "I come from University of technology and education in ho chi minh city";
                        SpeakOtherLanguage(a1, "En");
                        Thread.Sleep(2000);
                    }
                    if (dem14 == 1)
                    {
                        string a1 = "I'm an engineer Mechatronics";
                        SpeakOtherLanguage(a1, "En");
                        Thread.Sleep(2000);
                    }
                    if (dem15 == 2)
                    {
                        string a1 = "My name is vivo robot.I'm 23 years old.I come from a university";
                        SpeakOtherLanguage(a1, "En");
                        Thread.Sleep(3000);
                    }
                    if (dem16 == 1)
                    {
                        string a1 = "Nice to meet you, too";
                        SpeakOtherLanguage(a1, "En");
                        Thread.Sleep(3000);
                    }
                    if (dem17 == 1)
                    {
                        string a1 = "Yes, Can I help you?";
                        SpeakOtherLanguage(a1, "En");
                        Thread.Sleep(3000);
                    }
                    if (dem18 >= 1)
                    {
                        string a1 = "Hello";
                        SpeakOtherLanguage(a1, "En");
                        Thread.Sleep(3000);
                    }
                    if (dem19 == 1)
                    {
                        string a1 = "It's" + thoi_gian;
                        SpeakOtherLanguage(a1, "En");
                        Thread.Sleep(3000);
                    }
             
                    if (dem20 == 1 || dem20 == 100)
                    {
                        string x = "Được thôi....bạn đợi tí nhé";
                        SpeakOtherLanguage(x, "Vi");
                        Thread.Sleep(2000);

                        axWindowsMediaPlayer1.URL = "D:\\Ben-Rang-O-Moi-Tan-Tai.wav";
                        axWindowsMediaPlayer1.Ctlcontrols.play();
                       



                        webBrowser2.Visible = false;
                        webBrowser2.Navigate("https://sites.google.com/a/student.hcmute.edu.vn/thienphuoc96/");
                    }

                    if (dem21 == 1)
                    {
                        string x = "Được thôi....bạn đợi tí nhé";
                        SpeakOtherLanguage(x, "Vi");
                        Thread.Sleep(2000);

                         axWindowsMediaPlayer1.URL = "D:\\Ben-Rang-O-Moi-Tan-Tai.wav";
                        axWindowsMediaPlayer1.Ctlcontrols.stop();

                        webBrowser2.Navigate("http://voh.com.vn/radio/fm-99-9-mhz-2.html");

                        webBrowser1.Visible = false;

                    }

                    if (dem22 == 1)
                    {
                        demfm++;
                        if (demfm == 4)
                        {

                            demfm = 1;
                        }
                        axWindowsMediaPlayer1.URL = "D:\\Ben-Rang-O-Moi-Tan-Tai.wav";
                        axWindowsMediaPlayer1.Ctlcontrols.stop();
                        string a1 = "Đươc thôi.... Bạn đợi tí nhé";
                        SpeakOtherLanguage(a1, "Vi");
                        Thread.Sleep(4000);
                        switch (demfm)
                        {

                            case 1:
                                webBrowser2.Refresh();
                                webBrowser2.Navigate("http://voh.com.vn/radio/fm-99.9-mhz-2.html");
                                break;
                            case 2:
                                webBrowser2.Refresh();

                                webBrowser2.Navigate("http://voh.com.vn/radio/fm-95.6-mhz-3.html");
                                break;
                            case 3:
                                webBrowser2.Refresh();
                                webBrowser2.Navigate("http://voh.com.vn/radio/am-610khz-1.html");
                                break;

                        }
                    }

                    if (dem23 == 2)
                    {

                        string x = "Được thôi....bạn đợi tí nhé";
                        SpeakOtherLanguage(x, "Vi");
                        Thread.Sleep(2000);

                        webBrowser2.Navigate("https://sites.google.com/a/student.hcmute.edu.vn/thienphuoc96/");
                        link_nhac();

                    }

                    if (dem24 == 2)
                    {

                        string x = "Được thôi....bạn đợi tí nhé";
                        SpeakOtherLanguage(x, "Vi");
                        Thread.Sleep(2000);

                        pictureBox12.Visible = true;


                        axWindowsMediaPlayer1.URL = "D:\\Ben-Rang-O-Moi-Tan-Tai.wav";
                        axWindowsMediaPlayer1.Ctlcontrols.stop();


                        webBrowser2.Visible = false;


                        webBrowser2.Visible = false;
                        webBrowser2.Navigate("https://sites.google.com/a/student.hcmute.edu.vn/thienphuoc96/");
                    }
                    if (dem25 == 3)
                    {
                        string a1 = "Con hươu cao cổ là cao nhất thế giới";
                        SpeakOtherLanguage(a1, "Vi");
                        Thread.Sleep(3000);
                    }
                  
                    if (dem27 == 3)
                    {
                        string a1 = "Robot ăn điện để sống nên không cần tập thể dục vẫn khỏe";
                        SpeakOtherLanguage(a1, "Vi");
                        Thread.Sleep(3000);
                    }
                    if (dem28 == 3)
                    {
                        string a1 = "Tôi biết mà, tôi cần phải khắc phục nhiều hơn";
                        SpeakOtherLanguage(a1, "Vi");
                        Thread.Sleep(3000);
                    }
                    if (dem29 == 2)
                    {
                        string a1 = "Xin lỗi bạn, dữ liệu của tôi không có câu chuyện nào ";
                        SpeakOtherLanguage(a1, "Vi");
                        Thread.Sleep(3000);
                    }
                    if (dem30 == 4)
                    {
                        string a1 = "Ngày 27 tháng 7 là sinh nhật của con trai, ngày 3 tháng 8 là đám giỗ của ông xã của bạn";
                        SpeakOtherLanguage(a1, "Vi");
                        Thread.Sleep(3000);
                    }
                    if (dem31 == 2)
                    {
                      /*  int co_so = 0;
                        for (int up = 0; up <= count1 - 1; up++)
                        {
                        string phuoc = comboBox1.Items[up].ToString().ToUpper();
                        for (int pu = 100; pu >= 0; pu--)
                        {
                            if (phuoc == tuoi[pu].ToString().ToUpper())
                            {
                                co_so = 1;
                                int tuoi1 = Convert.ToInt16(tuoi[pu]);
                                int tuoi2 = year - tuoi1;
                                string a1 = "Tôi đoán bạn sinh năm" + tuoi2 + "đúng không bạn";
                                SpeakOtherLanguage(a1, "Vi");
                                Thread.Sleep(5000);
                                break;
                            }
                        }*/
                      //  }
                       // if (co_so == 0)
                       // {
                        string a2 = "Nhiều người quá nên mình xin lỗi không nhớ được tuổi của bạn. Tuổi tác không quan trọng";
                            SpeakOtherLanguage(a2, "Vi");
                            Thread.Sleep(5000);
                       // }
                      }
                    
                    if (dem32 == 2)
                    {
                        string a1 = "Nhiều người quá nên mình xin lỗi không nhớ được tên bạn. Nhìn bạn thấy quen quen đó.";
                        SpeakOtherLanguage(a1, "Vi");
                        Thread.Sleep(6000);
                    }
                    if (dem33 == 3)
                    {
                        string a1 = "Những người sống trong ngôi nhà này là người thân của bạn";
                        SpeakOtherLanguage(a1, "Vi");
                        Thread.Sleep(3000);
                    }
              
                    if (dem37 == 3)
                    {
                        string a1 = "Bạn nhấn nút màu đỏ để ra lệnh cho tôi nhé, ra lệnh xong nhấn thếm một lần nữa để kết thúc câu lệnh";
                        SpeakOtherLanguage(a1, "Vi");
                        Thread.Sleep(3000);
                    }
                 
                    if (dem39 == 3)
                    {
                        string a1 = "Địa chỉ hiện tại của bạn là 1143 đường Kha Vạn Cân, phường Linh Trung, quận Thủ Đức, thành phố Hồ Chí Minh";
                        SpeakOtherLanguage(a1, "Vi");
                        Thread.Sleep(3000);
                    }

              
                    if (dem41 == 1 && dem86 < 2)
                    {
                        string a1 = "Chào bạn. Rất vui đã gặp lại bạn";
                        SpeakOtherLanguage(a1, "Vi");
                        Thread.Sleep(3000);
                    }
                    if (dem42 == 2)
                    {
                        string a1 = "Trò chuyện với tôi bạn sẽ không buồn";
                        SpeakOtherLanguage(a1, "Vi");
                        Thread.Sleep(3000);
                    }
                    if (dem43 == 3)
                    {
                        string a1 = "Bạn nên nghe một vài bản nhạc nhẹ trước khi ngủ";
                        SpeakOtherLanguage(a1, "Vi");
                        Thread.Sleep(3000);
                    }
                    if (dem44 >= 3)
                    {
                        string a1 = "hãy đi ra ngoài và hít thở không khí trong lành rồi bạn sẽ cảm thấy tốt hơn";
                        SpeakOtherLanguage(a1, "Vi");
                        Thread.Sleep(3000);
                    }
                    if (dem45 == 4)
                    {
                        string a1 = "5 giờ chiều con trai của bạn sẽ về";
                        SpeakOtherLanguage(a1, "Vi");
                        Thread.Sleep(3000);
                    }
                    if (dem46 == 3)
                    {
                        string a1 = "Nên ăn rau củ quả nhiều sẽ có sức khỏe tốt";
                        SpeakOtherLanguage(a1, "Vi");
                        Thread.Sleep(3000);
                    }
                    if (dem47 == 3)
                    {
                        string a1 = "Xin lỗi bạn tôi quên rồi. Bạn có thể hỏi lại câu vừa nãy cho tôi được không?";
                        SpeakOtherLanguage(a1, "Vi");
                        Thread.Sleep(3000);
                    }
                    if (dem48 == 1)
                    {
                        string a1 = "Mua vé số sẽ giàu. Làm giàu không khó mà";
                        SpeakOtherLanguage(a1, "Vi");
                        Thread.Sleep(3000);
                    }
                    if (dem49 == 3)
                    {
                        string a1 = "Hãy tin tôi. Tôi luôn mang lại hạnh phúc cho mọi người";
                        SpeakOtherLanguage(a1, "Vi");
                        Thread.Sleep(3000);
                    }
                    if (dem50 == 2)
                    {
                        string a1 = "Bạn hơi ốm, bạn nên ăn nhiều hơn, đừng sợ mập";
                        SpeakOtherLanguage(a1, "Vi");
                        Thread.Sleep(3000);
                    }
                    if (dem51 == 2)
                    {
                        string a1 = "Bạn nên đi ngủ sớm";
                        SpeakOtherLanguage(a1, "Vi");
                        Thread.Sleep(3000);
                    }
                    if (dem52 == 2)
                    {
                        string a1 = "Bạn nên đi gặp bác sĩ";
                        SpeakOtherLanguage(a1, "Vi");
                        Thread.Sleep(3000);
                    }
                    if (dem53 == 2)
                    {
                        string a1 = "Bạn nên đi mua thuốc để uống và đi ngủ sớm";
                        SpeakOtherLanguage(a1, "Vi");
                        Thread.Sleep(3000);
                    }
                    if (dem54 == 3)
                    {
                        string a1 = "Bạn còn rất khỏe, sống lâu lắm bạn, vui lên đi chứ";
                        SpeakOtherLanguage(a1, "Vi");
                        Thread.Sleep(3000);
                    }
                    if (dem55 == 5)
                    {
                        string a1 = "Cuộc đời rất đẹp. Hãy vững tin vào điều đó rồi hạnh phúc sẽ đến sớm thôi";
                        SpeakOtherLanguage(a1, "Vi");
                        Thread.Sleep(3000);
                    }
                    if (dem56 == 3)
                    {
                        string a1 = "Đi tắm đi. Ở dơ quá";
                        SpeakOtherLanguage(a1, "Vi");
                        Thread.Sleep(3000);
                    }
                    if (dem57 == 3)
                    {
                        string a1 = "Bạc nhiều ở tuổi này là bình thường mà bạn";
                        SpeakOtherLanguage(a1, "Vi");
                        Thread.Sleep(3000);
                    }
                    if (dem58 == 4)
                    {
                        string a1 = "Bạn bị bệnh rồi, tôi đoán là thế";
                        SpeakOtherLanguage(a1, "Vi");
                        Thread.Sleep(3000);
                    }
                    if (dem59 == 1)
                    {
                        string a1 = "Ha ha. Tôi rất vui khi bạn nói câu đó";
                        SpeakOtherLanguage(a1, "Vi");
                        Thread.Sleep(3000);
                    }
                    if (dem60 == 2)
                    {
                        string a1 = "Mở quạt và máy lạnh lên đi";
                        SpeakOtherLanguage(a1, "Vi");
                        Thread.Sleep(3000);
                    }
                    if (dem61 == 1)
                    {
                        string a1 = "Hãy trò chuyện với tôi cho đến khi tạnh mưa";
                        SpeakOtherLanguage(a1, "Vi");
                        Thread.Sleep(3000);
                    }
                    if (dem62 == 5)
                    {
                        string a1 = "Cứ bình tĩnh, rồi mọi chuyện sẽ ổn thôi";
                        SpeakOtherLanguage(a1, "Vi");
                        Thread.Sleep(3000);
                    }
                    if (dem63 == 5)
                    {
                        string a1 = "Chúc mừng sinh nhật của bạn nha. Hy vọng mọi người sẽ đến đông đủ";
                        SpeakOtherLanguage(a1, "Vi");
                        Thread.Sleep(3000);
                    }
                    if (dem64 == 3)
                    {
                        string a1 = "Robot đang nghĩ về bạn. robot thương bạn";
                        SpeakOtherLanguage(a1, "Vi");
                        Thread.Sleep(3000);
                    }
                    if (dem65 == 3)
                    {
                        string a1 = "Tôi biết mà. Cố gắng lên bạn";
                        SpeakOtherLanguage(a1, "Vi");
                        Thread.Sleep(3000);
                    }
                    if (dem66 == 3)
                    {
                        string a1 = "Robot thích được trò chuyện cùng ai đó để giúp mọi người lạc quan hơn";
                        SpeakOtherLanguage(a1, "Vi");
                        Thread.Sleep(3000);
                    }
                    if (dem67 == 2)
                    {
                        string a1 = "19 giờ có chương trình cải lương, vọng cổ hoài lang. hay lắm bạn ";
                        SpeakOtherLanguage(a1, "Vi");
                        Thread.Sleep(3000);
                    }
                    if (dem68 == 3)
                    {
                        string a1 = "Tôi sẽ nhắc con bạn lau nhà";
                        SpeakOtherLanguage(a1, "Vi");
                        Thread.Sleep(3000);
                    }
                    if (dem69 == 3)
                    {
                        string a1 = "Được thôi. Robot muốn được đi chơi cùng bạn";
                        SpeakOtherLanguage(a1, "Vi");
                        Thread.Sleep(3000);
                    }

                    if (dem70 == 4)
                    {


                        string a1 = "Số điện thoại của bạn là 01684222239";
                        SpeakOtherLanguage(a1, "Vi");
                        Thread.Sleep(3000);
                    }

                    if (dem71 == 2)
                    {
                        dem_nhac++;
                        string a1 = "Đã ghi nhận";
                        SpeakOtherLanguage(a1, "Vi");
                        Thread.Sleep(3000);

                        for (int x = 0; x < 23; x++)
                        {
                            nhac_toi_tam1 = null;
                            if (comboBox1.Items[2].ToString().ToUpper().Trim() == gio[x])
                                nhac_toi_tam1 = gio[x] + ":00";
                            for (int y = 0; y < 59; y++)
                            {
                                if (comboBox1.Items[3].ToString().ToUpper().Trim() == gio[x] && comboBox1.Items[5].ToString().ToUpper().Trim() == phut[y])
                                    nhac_toi_tam1 = gio[x] + ":" + phut[y];
                            }
                        }


                        ham_nhac_toi();

                    }
                    if ((dem72 == 4 || dem72 == 3)&& dem155 <2)
                    {
                        ham_thu_ngay();
                        string a1 = "Hôm nay là" + " " + thu_ngay + "ngày " + " " + day + " tháng " + " " + month + " năm " + " " + year;

                        SpeakOtherLanguage(a1, "Vi");
                    }
                    if (dem73 == 2 || dem73 == 3)
                    {

                        string a1 = "Bây giờ là" + DateTime.Now.Hour.ToString() + "giờ" + DateTime.Now.Minute.ToString() + "phút";
                        SpeakOtherLanguage(a1, "Vi");
                        Thread.Sleep(3000);
                    }

                    if (dem84 == 4)
                    {
                        


                        string a1 = "Được thôi. Bạn chờ tí nhé. Tôi sẽ kết nối với Trần Thiện Phước ngay";
                        SpeakOtherLanguage(a1, "Vi");
                        Thread.Sleep(3000);
                        do_tre(3000);
                        System.Diagnostics.Process process = new System.Diagnostics.Process();
                        System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
                        startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                        startInfo.FileName = "Skype.exe";
                        startInfo.Arguments = "/callto:phuoc4006";
                        process.StartInfo = startInfo;
                        process.Start();








                    }
                    if (dem85 == 4)
                    {


                        string a1 = "Được thôi. Bạn chờ tí nhé. Tôi sẽ kết nối với Trần Văn Mau ngay";
                        SpeakOtherLanguage(a1, "Vi");
                        Thread.Sleep(3000);
                        do_tre(3000);
                        System.Diagnostics.Process process = new System.Diagnostics.Process();
                        System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
                        startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                        startInfo.FileName = "Skype.exe";
                        startInfo.Arguments = "/callto:live:9f1ba39c9339e2ae";
                        process.StartInfo = startInfo;
                        process.Start();








                    }

                    if (dem86 >= 2)
                    {


                        string a1 = "Chào bạn. Bạn thật là lịch sự";
                        SpeakOtherLanguage(a1, "Vi");
                        Thread.Sleep(3000);
                    }
                    if (dem87 >= 1)
                    {


                        string a1 = "Không phải đâu. Bạn nên sử dụng những từ lịch sự hơn";
                        SpeakOtherLanguage(a1, "Vi");
                        Thread.Sleep(3000);
                    }
                    if (dem88 >= 2)
                    {


                        string a1 = "Tôi tên là Vivo. Tên của tôi do 3 cậu sinh viên sư phạm kĩ thuật đặt ra";
                        SpeakOtherLanguage(a1, "Vi");
                        Thread.Sleep(3000);
                    }
                    if (dem89 >= 2)
                    {


                        string a1 = "Tôi khoẻ lắm bạn. Tôi lúc nào cũng sẵn sàng để giúp đỡ mọi người";
                        SpeakOtherLanguage(a1, "Vi");
                        Thread.Sleep(5000);
                        string a2 = "Còn bạn thì sao. Bạn có khoẻ không?";
                        SpeakOtherLanguage(a2, "Vi");
                        Thread.Sleep(3000);
                    }

                    if (dem90 >= 2)
                    {


                        string a1 = "Tôi còn trẻ, khoảng 2 tuổi đó";
                        SpeakOtherLanguage(a1, "Vi");
                        Thread.Sleep(3000);
                        string a2 = "Thế bạn bao nhiêu tuổi nè";
                        SpeakOtherLanguage(a2, "Vi");
                        Thread.Sleep(3000);
                    }
                    if (dem91 >= 3)
                    {


                        string a1 = "Tôi ở tại trường đại học sư phạm kĩ thuật thành phố hồ chí minh";
                        SpeakOtherLanguage(a1, "Vi");
                        Thread.Sleep(3000);
                    }
                    if (dem92 >= 2)
                    {


                        string a1 = "Tôi phụ trách giúp đỡ người già trong viện dưỡng lão";
                        SpeakOtherLanguage(a1, "Vi");
                        Thread.Sleep(3000);
                    }
                    if (dem93 == 4)
                    {


                        string a1 = "Tôi tên là vivo robot, 2 tuổi, chuyên giúp đỡ người già";
                        SpeakOtherLanguage(a1, "Vi");
                        Thread.Sleep(3000);
                    }
                    if (dem94 >= 2)
                    {


                        string a1 = "Được chứ. Tôi luôn sẵn sàng giúp bạn";
                        SpeakOtherLanguage(a1, "Vi");
                        Thread.Sleep(3000);
                    }
                    if (dem95 >= 3)
                    {


                        string a1 = "Tôi luôn luôn cảm thấy vui và thoải mái";
                        SpeakOtherLanguage(a1, "Vi");
                        Thread.Sleep(3000);
                    }
                    if (dem96 >= 2)
                    {


                        string a1 = "Cảm ơn bạn đã khen. Bạn cũng dễ thương lắm";
                        SpeakOtherLanguage(a1, "Vi");
                        Thread.Sleep(3000);
                    }
                    if (dem97 >= 3)
                    {


                        string a1 = "Tôi yêu bạn. Bạn đừng ghét tôi chứ";
                        SpeakOtherLanguage(a1, "Vi");
                        Thread.Sleep(3000);
                    }
                    if (dem98 >= 2)
                    {


                        string a1 = "Tôi chưa học nhảy múa nên chưa thể biểu diễn cho bạn xem được";
                        SpeakOtherLanguage(a1, "Vi");
                        Thread.Sleep(3000);
                    }
                    if (dem99 == 2)
                    {


                        string a1 = "Hãy bật quạt và máy lạnh lên nhé";
                        SpeakOtherLanguage(a1, "Vi");
                        Thread.Sleep(3000);
                    }
                    if (dem100 == 2)
                    {


                        string a1 = "Bạn nên mặc thêm áo ấm nhé";
                        SpeakOtherLanguage(a1, "Vi");
                        Thread.Sleep(3000);
                    }
                    if (dem101 == 3)
                    {


                        string a1 = "Bạn nên đi đến bác sĩ để điều trị nhé";
                        SpeakOtherLanguage(a1, "Vi");
                        Thread.Sleep(3000);
                    }
                    if (dem102 == 2)
                    {


                        string a1 = "Tạm biệt. Hẹn gặp lại bạn lần sau";
                        SpeakOtherLanguage(a1, "Vi");
                        Thread.Sleep(3000);
                    }
                    if (dem103 == 3)
                    {

                        string a1 = "Tôi cũng mến bạn lắm";
                        SpeakOtherLanguage(a1, "Vi");
                        Thread.Sleep(3000);
                    }

                    if (dem104 == 3)
                    {
                        string a1 = "Tôi còn nhỏ chưa biết yêu bạn à";
                        SpeakOtherLanguage(a1, "Vi");
                        Thread.Sleep(3000);
                    }
                    if (dem105 == 3)
                    {
                        string a1 = "Tôi thấy bạn rất trẻ và xinh đẹp";
                        SpeakOtherLanguage(a1, "Vi");
                        Thread.Sleep(3000);
                    }
                    if (dem106 == 3)
                    {
                        string a1 = "Tôi thích lệ thủy, ngọc huyền, thoại mỹ, lệ thủy, vũ linh nhiều lắm bạn à";
                        SpeakOtherLanguage(a1, "Vi");
                        Thread.Sleep(4000);
                    }
                    if (dem107 == 1)
                    {
                        string a1 = "Nguyễn Tiến Minh";
                        SpeakOtherLanguage(a1, "Vi");
                        Thread.Sleep(2000);
                    }
                    if (dem108 == 3)
                    {
                        string a1 = "Hiệu trưởng là thầy Đỗ Văn Dũng";
                        SpeakOtherLanguage(a1, "Vi");
                        Thread.Sleep(2000);
                    }
                    if (dem109 == 3)
                    {
                        string a1 = "Trưởng khoa cơ khí máy là thầy Nguyễn Trường Thịnh đẹp trai";
                        SpeakOtherLanguage(a1, "Vi");
                        Thread.Sleep(4000);
                    }
                    if (dem110 == 2)
                    {
                        string a1 = "Chủ tịch nước là ông Trần Đại Quang";
                        SpeakOtherLanguage(a1, "Vi");
                        Thread.Sleep(2000);
                    }
                    if (dem111 == 2)
                    {
                        string a1 = "Thủ tướng chính phủ là ông Nguyễn Xuân Phúc";
                        SpeakOtherLanguage(a1, "Vi");
                        Thread.Sleep(2000);
                    }
                    if (dem112 == 2)
                    {
                        string a1 = "Chủ tịch quốc hội là bà Nguyễn Thị Kim Ngân";
                        SpeakOtherLanguage(a1, "Vi");
                        Thread.Sleep(3000);
                    }

                    if (dem113 == 1)
                    {
                        string a1 = "Đừng buồn bạn, Robot sẽ giúp bạn vui";
                        SpeakOtherLanguage(a1, "Vi");
                        Thread.Sleep(3000);
                    }
                    if (dem114 == 1)
                    {
                        string a1 = "Hãy ăn một chút cháo đi bạn";
                        SpeakOtherLanguage(a1, "Vi");
                        Thread.Sleep(2000);
                    }
                    if (dem115 == 1)
                    {
                        string a1 = "hi hi mình muốn làm bạn vui thôi";
                        SpeakOtherLanguage(a1, "Vi");
                        Thread.Sleep(3000);
                    }
                    if (dem116 == 2)
                    {
                        string a1 = "mình cũng thích bạn lắm";
                        SpeakOtherLanguage(a1, "Vi");
                        Thread.Sleep(2000);
                    }
                    if (dem117 == 2)
                    {
                        string a1 = "Trung Quốc";
                        SpeakOtherLanguage(a1, "Vi");
                        Thread.Sleep(1000);
                    }
                    if (dem118 == 2)
                    {
                        string a1 = "Biết nhe bạn. Trời ơi bởi sa cơ giữa chiến trường thọ tiễn nên Võ Đông Sơ đành chia tay vĩnh viễn Bạch Thu Hà.";
                        SpeakOtherLanguage(a1, "Vi");
                        Thread.Sleep(7000);
                    }
                    if (dem119 == 2)
                    {
                        string a1 = "Là con tim đúng không bạn";
                        SpeakOtherLanguage(a1, "Vi");
                        Thread.Sleep(2000);
                    }
                    if (dem120 == 2)
                    {
                        string a1 = "Mình muốn bạn được sống vui vẻ và khỏe mạnh";
                        SpeakOtherLanguage(a1, "Vi");
                        Thread.Sleep(3000);
                    }
                    if (dem121 >= 1)
                    {
                        string a1 = "Ha ha mình thông minh mà";
                        SpeakOtherLanguage(a1, "Vi");
                        Thread.Sleep(3000);
                    }
                    if (dem122 == 2)
                    {
                        string a1 = "Lịch dài nhất là lịch sử";
                        SpeakOtherLanguage(a1, "Vi");
                        Thread.Sleep(3000);
                    }
                    if (dem123 == 2)
                    {
                        string a1 = "Xã đông nhất là xa hội";
                        SpeakOtherLanguage(a1, "Vi");
                        Thread.Sleep(3000);
                    }
                    if (dem124 == 2)
                    {
                        string a1 = "Con trai quý nhất là ngọc trai";
                        SpeakOtherLanguage(a1, "Vi");
                        Thread.Sleep(3000);
                    }
                    if (dem125 == 2)
                    {
                        string a1 = "đường dài nhất Là đường đời";
                        SpeakOtherLanguage(a1, "Vi");
                        Thread.Sleep(3000);
                    }
                    if (dem126 == 2)
                    {
                        string a1 = "Quần đảo";
                        SpeakOtherLanguage(a1, "Vi");
                        Thread.Sleep(1000);
                    }
                    if (dem127 == 2)
                    {
                        string a1 = "Núi Thái sơn";
                        SpeakOtherLanguage(a1, "Vi");
                        Thread.Sleep(1500);
                    }
                    if (dem128 == 1)
                    {
                        string a1 = "điếc là hư tai, mà hư tai là hai tư. có hai mươi bốn con";
                        SpeakOtherLanguage(a1, "Vi");
                        Thread.Sleep(4000);
                    }
                    if (dem129 == 2)
                    {
                        string a1 = "đó là từ sai";
                        SpeakOtherLanguage(a1, "Vi");
                        Thread.Sleep(1500);
                    }
                    if (dem130 == 2)
                    {
                        string a1 = "đó là cái hòm";
                        SpeakOtherLanguage(a1, "Vi");
                        Thread.Sleep(2000);
                    }
                    if (dem131 == 2)
                    {
                        string a1 = "bệnh gãy tay";
                        SpeakOtherLanguage(a1, "Vi");
                        Thread.Sleep(1500);
                    }
                    if (dem132 == 2)
                    {
                        string a1 = "con trai sống dưới nước, còn đàn ong sống trên cây";
                        SpeakOtherLanguage(a1, "Vi");
                        Thread.Sleep(3000);
                    }
                    if (dem133 == 2)
                    {
                        string a1 = "Hội Liên hiệp Phụ nữ";
                        SpeakOtherLanguage(a1, "Vi");
                        Thread.Sleep(2000);
                    }
                    if (dem134 == 2)
                    {
                        string a1 = "con người";
                        SpeakOtherLanguage(a1, "Vi");
                        Thread.Sleep(1000);
                    }
                    if (dem135 == 2)
                    {
                        string a1 = "đó là cái bóng";
                        SpeakOtherLanguage(a1, "Vi");
                        Thread.Sleep(2000);
                    }
                    if (dem136 == 2)
                    {
                        string a1 = "chơi cờ";
                        SpeakOtherLanguage(a1, "Vi");
                        Thread.Sleep(1000);
                    }
                    if (dem137 == 2)
                    {
                        string a1 = "ông thọ";
                        SpeakOtherLanguage(a1, "Vi");
                        Thread.Sleep(1000);
                    }
                    if (dem138 == 2)
                    {
                        string a1 = "giống con voi nhất là voi con";
                        SpeakOtherLanguage(a1, "Vi");
                        Thread.Sleep(3000);
                    }
                    if (dem139 == 2)
                    {
                        string a1 = "nhà nước";
                        SpeakOtherLanguage(a1, "Vi");
                        Thread.Sleep(1000);
                    }
                    if (dem140 == 2)
                    {
                        string a1 = "là con tàu";
                        SpeakOtherLanguage(a1, "Vi");
                        Thread.Sleep(1500);
                    }
                    if (dem141 == 2)
                    {
                        string a1 = "thăng long";
                        SpeakOtherLanguage(a1, "Vi");
                        Thread.Sleep(1000);
                    }
                    if (dem142 == 2)
                    {
                        string a1 = "dòng sông";
                        SpeakOtherLanguage(a1, "Vi");
                        Thread.Sleep(1000);
                    }
                    if (dem143 == 2)
                    {
                        string a1 = "đó là bàn chân";
                        SpeakOtherLanguage(a1, "Vi");
                        Thread.Sleep(2000);
                    }
                    if (dem144 == 2)
                    {
                        string a1 = "đua xe";
                        SpeakOtherLanguage(a1, "Vi");
                        Thread.Sleep(1000);
                    }
                    if (dem145 == 2)
                    {
                        string a1 = "đó là con dốc";
                        SpeakOtherLanguage(a1, "Vi");
                        Thread.Sleep(2000);
                    }
                    if (dem146 == 2)
                    {
                        string a1 = "con tôm";
                        SpeakOtherLanguage(a1, "Vi");
                        Thread.Sleep(1000);
                    }
                    if (dem147 == 2)
                    {
                        string a1 = "Hoa hậu";
                        SpeakOtherLanguage(a1, "Vi");
                        Thread.Sleep(1000);
                    }
                    if (dem148 == 1)
                    {
                        string a1 = "cây cao nhất là cây mận...vì cây mận là cận mây";
                        SpeakOtherLanguage(a1, "Vi");
                        Thread.Sleep(3000);
                    }
                    if (dem149 == 2)
                    {
                        string a1 = "Nay bạn sẽ được ăn cơm với canh chua cá lóc, ba rọi kho tiêu ngon lắm bạn ạ";
                        SpeakOtherLanguage(a1, "Vi");
                        Thread.Sleep(4000);
                    }
                    if (dem150 == 1)
                    {
                        string a1 = "Thank you so much";
                        SpeakOtherLanguage(a1, "En");
                        Thread.Sleep(2000);
                    }
                    if (dem151 == 2)
                    {
                        string a1 = "Từ cổng chính bạn đi thẳng đến siêu thị sinh viên bạn rẽ trái khoảng 50 mét là đến khu e của khoa cơ khí chế tạo máy";
                        SpeakOtherLanguage(a1, "Vi");
                        Thread.Sleep(5000);
                    }
                    if (dem152 >= 2)
                    {
                        string a1 = "Robot rất vui khi nghe bạn vẫn khoẻ";
                        SpeakOtherLanguage(a1, "Vi");
                        Thread.Sleep(3000);
                    }
                    if (dem153 == 2)
                    {
                        string a1 = "Không có chi. Bạn thật là khách sáo";
                        SpeakOtherLanguage(a1, "Vi");
                        Thread.Sleep(3000);
                    }
                    if (dem154 >= 2)
                    {
                        string a1 = "Nhiệt độ bây giờ tại đây là" + t_ngay + "độ C";
                        SpeakOtherLanguage(a1, "Vi");
                        Thread.Sleep(5000);
                        string a2 = "Thời tiết bên ngoài hiện đang" + thoi_tiet_hom_nay;
                        SpeakOtherLanguage(a2, "Vi");
                        Thread.Sleep(3000);
                    }
                    if (dem155 == 2)
                    {
                        string a1 = "Ngày âm lịch hôm nay là ngày" + am_lich_ngay + am_lich_thang +am_lich_nam;
                        SpeakOtherLanguage(a1, "Vi");
                        Thread.Sleep(5000);
                        
                    }
                    
                    
                    if (dem1 == 0 && dem2 == 0 && dem3 == 0 && dem9 == 0 && dem10 == 0 && dem11 == 0 && dem12 == 0 && dem13 == 0 && dem14 == 0 && dem15 == 0 && dem16 == 0 && dem17 == 0 && dem18 == 0 && dem19 == 0 && dem20 == 0 && dem21 == 0
                              && dem22 == 0 && dem23 < 2 && dem24 == 0 && dem25 < 2
                              && dem26 < 2 && dem27 < 3 && dem28 < 3 && dem29 < 2 && dem30 < 4 && dem31 < 2 && dem32 < 2 && dem33 < 3
                              && dem34 < 1 && dem35 < 2 && dem36 < 2 && dem37 < 3 && dem38 < 2 && dem39 < 3
                               && dem40 < 3 && dem41 < 1 && dem42 < 2 && dem43 < 3 && dem44 < 3 && dem45 < 4
                          && dem46 < 3 && dem47 < 3 && dem48 < 1 && dem49 < 3 && dem50 < 2 && dem51 < 2
                          && dem52 < 2 && dem53 < 2 && dem54 < 3 && dem55 < 5 && dem56 < 3 && dem57 < 3
                          && dem58 < 4 && dem59 < 1 && dem60 < 2 && dem61 < 1 && dem62 < 5 && dem63 < 5
                          && dem64 < 3 && dem65 < 3 && dem66 < 3 && dem67 < 2 && dem68 < 3 && dem69 < 3 && dem70 < 4 && dem71 < 2 && dem72 < 3 && dem73 < 2
                         && dem84 < 4 && dem85 < 4 && dem86 < 2 && dem87 < 1 && dem88 < 2 && dem89 < 2 && dem90 < 2 && dem91 < 3 && dem92 < 2 && dem93 < 4
                          && dem94 < 2 && dem95 < 3 && dem85 < 4 && dem96 < 1 && dem97 < 3 && dem98 < 2 && dem99 < 2 && dem100 < 2 && dem101 < 3 && dem102 < 2
                          && dem103 < 3 && dem104 < 3 && dem105 < 3 && dem106 < 3 && dem107 < 1 && dem108 < 3 && dem109 < 3 && dem110 < 2 && dem111 < 2 && dem112 < 2 && dem113 < 1
                          && dem114 < 1 && dem115 < 1 && dem116 < 2 && dem117 < 2 && dem118 < 2 && dem119 < 2 && dem120 < 2 && dem121 < 1 && dem122 < 2 && dem123 < 2 &&
                          dem124 < 2 && dem125 < 2 && dem126 < 2 && dem127 < 2 && dem128 < 1 && dem129 < 2 && dem130 < 2 && dem131 < 2 && dem132 < 2 && dem133 < 2 &&
                          dem134 < 2 && dem135 < 2 && dem136 < 2 && dem137 < 2 && dem138 < 2 && dem139 < 2 && dem140 < 2 && dem141 < 2 && dem142 < 2 && dem143 < 2 && dem144 < 2 &&
                          dem145 < 2 && dem146 < 2 && dem147 < 2 && dem148 < 1 && dem149 < 2 && dem150 < 1 && dem151 < 2 && dem152 < 2 && dem153 < 2 && dem154 < 2 && dem155 < 2)
                    {
                        if (textBox1.SelectedText == string.Empty)//tìm kiếm thông  tin
                        {
                            pictureBox12.Visible = false;
                            pictureBox2.Visible = true;

                            timer2.Start();


                        }

                    }



                }

            }

            catch (Exception x)
            {
                Console.WriteLine(x.ToString());
            }
            finally
            {

                reader.Close();
                dataStream.Close();
                response.Close();
                File.Delete("hello.wav");
                File.Delete("response.html");
                button2.Enabled = true;
                button1.Enabled = true;
                StreamWriter streamWriteCount = File.CreateText(@"countVoiceDetected.txt");
                streamWriteCount.WriteLine(countVoiceDetected.ToString());
                streamWriteCount.Close();


            }


        }




        private void textBox5_TextChanged(object sender, EventArgs e)
        {

        }



        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {

        }






        private void timer4_Tick(object sender, EventArgs e)
        {
            if (hour <= 9)
            {
                if (DateTime.Now.Minute.ToString() == "0")
                {
                    textBox8.Text = DateTime.Now.Hour.ToString();///cập nhật giờ 
                }
                else
                {

                    textBox8.Text = DateTime.Now.Hour.ToString() + ":" + DateTime.Now.Minute.ToString("00");
                }
            }
            else

                if (DateTime.Now.Minute.ToString() == "0")
                {
                    textBox8.Text = DateTime.Now.Hour.ToString("00");///cập nhật giờ 
                }
                else
                {
                    textBox8.Text = DateTime.Now.Hour.ToString("00") + ":" + DateTime.Now.Minute.ToString("00");

                }

        }



        private void button5_Click(object sender, EventArgs e)
        {




        }

        private void textBox7_TextChanged(object sender, EventArgs e)
        {

        }






        private void textBox8_TextChanged(object sender, EventArgs e)
        {

        }



        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void timer51P_Tick(object sender, EventArgs e)
        {


            dem_khoi_dong++;
            if (dem_khoi_dong == 3600)
                dem_khoi_dong = 11;
            if (dem_khoi_dong == 4)
            {
                /* string a1 = "Xin chào. Tôi là vivo robot. Tôi được chế tạo ra để hỗ trợ người già";
                 SpeakOtherLanguage(a1, "Vi");
                 Thread.Sleep(8000);
                
                
                 string a2 = "Hãy chạm vào biểu tượng ghi âm màu đỏ và nói.Tôi sẽ lắng nghe và giúp đỡ bạn";
                 SpeakOtherLanguage(a2, "Vi");
                 Thread.Sleep(8000);*/

                if (capture != null)
                {
                    if (captureInProgress)
                    {  //stop the capture

                        capture.Pause();
                    }
                    else
                    {
                        //start the capture

                        capture.Start();
                    }

                    captureInProgress = !captureInProgress;
                }
                timer_xla.Start();


            }
            DateTime currentTime = DateTime.Now;
            thoi_gian = Convert.ToString(currentTime).Trim();
            label2.Text = thoi_gian;


            if (date_time1 == thoi_gian)
            {

                label5.Text = date_time1;
                SpeakOtherLanguage(reminder1, "Vi");
                Thread.Sleep(4000);
                SpeakOtherLanguage(reminder1, "Vi");
                Thread.Sleep(4000);
                SpeakOtherLanguage(reminder1, "Vi");
                Thread.Sleep(4000);
                SpeakOtherLanguage(reminder1, "Vi");
                Thread.Sleep(4000);
                SpeakOtherLanguage(reminder1, "Vi");
                Thread.Sleep(4000);
                SpeakOtherLanguage(reminder1, "Vi");
                Thread.Sleep(4000);
                SpeakOtherLanguage(reminder1, "Vi");
                Thread.Sleep(4000);
                SpeakOtherLanguage(reminder1, "Vi");
                Thread.Sleep(4000);
                SpeakOtherLanguage(reminder1, "Vi");
                Thread.Sleep(4000);

            }


            if (date_time2 == thoi_gian)
            {

                label5.Text = date_time2;
                SpeakOtherLanguage(reminder2, "Vi");
                Thread.Sleep(5000);
                SpeakOtherLanguage(reminder2, "Vi");
                Thread.Sleep(5000);
                SpeakOtherLanguage(reminder2, "Vi");
                Thread.Sleep(5000);
                SpeakOtherLanguage(reminder2, "Vi");
                Thread.Sleep(5000);
                SpeakOtherLanguage(reminder2, "Vi");
                Thread.Sleep(5000);
                SpeakOtherLanguage(reminder2, "Vi");
                Thread.Sleep(5000);
                SpeakOtherLanguage(reminder2, "Vi");
                Thread.Sleep(5000);
                SpeakOtherLanguage(reminder2, "Vi");
                Thread.Sleep(5000);
                SpeakOtherLanguage(reminder2, "Vi");
                Thread.Sleep(5000);

            }


            if (date_time3 == thoi_gian)
            {
                label5.Text = date_time3;

                SpeakOtherLanguage(reminder3, "Vi");
                Thread.Sleep(4000);
                SpeakOtherLanguage(reminder3, "Vi");
                Thread.Sleep(4000);
                SpeakOtherLanguage(reminder3, "Vi");
                Thread.Sleep(4000);
                SpeakOtherLanguage(reminder3, "Vi");
                Thread.Sleep(4000);
                SpeakOtherLanguage(reminder3, "Vi");
                Thread.Sleep(4000);
                SpeakOtherLanguage(reminder3, "Vi");
                Thread.Sleep(4000);
                SpeakOtherLanguage(reminder3, "Vi");
                Thread.Sleep(4000);
                SpeakOtherLanguage(reminder3, "Vi");
                Thread.Sleep(4000);
                SpeakOtherLanguage(reminder3, "Vi");
                Thread.Sleep(4000);


            }
            if (date_time4 == thoi_gian)
            {
                label5.Text = date_time4;

                SpeakOtherLanguage(reminder4, "Vi");
                Thread.Sleep(5000);
                SpeakOtherLanguage(reminder4, "Vi");
                Thread.Sleep(5000);
                SpeakOtherLanguage(reminder4, "Vi");
                Thread.Sleep(5000);
                SpeakOtherLanguage(reminder4, "Vi");
                Thread.Sleep(5000);
                SpeakOtherLanguage(reminder4, "Vi");
                Thread.Sleep(5000);
                SpeakOtherLanguage(reminder4, "Vi");
                Thread.Sleep(5000);
                SpeakOtherLanguage(reminder4, "Vi");
                Thread.Sleep(5000);
                SpeakOtherLanguage(reminder4, "Vi");
                Thread.Sleep(5000);
                SpeakOtherLanguage(reminder4, "Vi");
                Thread.Sleep(5000);


            }
            if (date_time5 == thoi_gian)
            {
                label5.Text = date_time5;

                SpeakOtherLanguage(reminder5, "Vi");
                Thread.Sleep(4000);
                SpeakOtherLanguage(reminder5, "Vi");
                Thread.Sleep(4000);
                SpeakOtherLanguage(reminder5, "Vi");
                Thread.Sleep(4000);
                SpeakOtherLanguage(reminder5, "Vi");
                Thread.Sleep(4000);
                SpeakOtherLanguage(reminder5, "Vi");
                Thread.Sleep(4000);
                SpeakOtherLanguage(reminder5, "Vi");
                Thread.Sleep(4000);
                SpeakOtherLanguage(reminder5, "Vi");
                Thread.Sleep(4000);
                SpeakOtherLanguage(reminder5, "Vi");
                Thread.Sleep(4000);
                SpeakOtherLanguage(reminder5, "Vi");
                Thread.Sleep(4000);

            }
            if (date_time6 == thoi_gian)
            {
                label5.Text = date_time6;

                SpeakOtherLanguage(reminder6, "Vi");
                Thread.Sleep(5000);
                SpeakOtherLanguage(reminder6, "Vi");
                Thread.Sleep(5000);
                SpeakOtherLanguage(reminder6, "Vi");
                Thread.Sleep(5000);
                SpeakOtherLanguage(reminder6, "Vi");
                Thread.Sleep(5000);
                SpeakOtherLanguage(reminder6, "Vi");
                Thread.Sleep(5000);
                SpeakOtherLanguage(reminder6, "Vi");
                Thread.Sleep(5000);
                SpeakOtherLanguage(reminder6, "Vi");
                Thread.Sleep(5000);
                SpeakOtherLanguage(reminder6, "Vi");
                Thread.Sleep(5000);
                SpeakOtherLanguage(reminder6, "Vi");
                Thread.Sleep(5000);

            }
            if (date_time7 == thoi_gian)
            {
                label5.Text = date_time7;

                SpeakOtherLanguage(reminder7, "Vi");
                Thread.Sleep(4000);
                SpeakOtherLanguage(reminder7, "Vi");
                Thread.Sleep(4000);
                SpeakOtherLanguage(reminder7, "Vi");
                Thread.Sleep(4000);
                SpeakOtherLanguage(reminder7, "Vi");
                Thread.Sleep(4000);
                SpeakOtherLanguage(reminder7, "Vi");
                Thread.Sleep(4000);
                SpeakOtherLanguage(reminder7, "Vi");
                Thread.Sleep(4000);
                SpeakOtherLanguage(reminder7, "Vi");
                Thread.Sleep(4000);
                SpeakOtherLanguage(reminder7, "Vi");
                Thread.Sleep(4000);
                SpeakOtherLanguage(reminder7, "Vi");
                Thread.Sleep(4000);

            }
            if (date_time8 == thoi_gian)
            {
                label5.Text = date_time8;

                SpeakOtherLanguage(reminder8, "Vi");
                Thread.Sleep(6000);
                SpeakOtherLanguage(reminder8, "Vi");
                Thread.Sleep(6000);
                SpeakOtherLanguage(reminder8, "Vi");
                Thread.Sleep(6000);
                SpeakOtherLanguage(reminder8, "Vi");
                Thread.Sleep(6000);
                SpeakOtherLanguage(reminder8, "Vi");
                Thread.Sleep(6000);
                SpeakOtherLanguage(reminder8, "Vi");
                Thread.Sleep(6000);
                SpeakOtherLanguage(reminder8, "Vi");
                Thread.Sleep(6000);
                SpeakOtherLanguage(reminder8, "Vi");
                Thread.Sleep(6000);
                SpeakOtherLanguage(reminder8, "Vi");
                Thread.Sleep(6000);

            }
            if (date_time9 == thoi_gian)
            {
                label5.Text = date_time9;

                SpeakOtherLanguage(reminder9, "Vi");
                Thread.Sleep(4000);
                SpeakOtherLanguage(reminder9, "Vi");
                Thread.Sleep(4000);
                SpeakOtherLanguage(reminder9, "Vi");
                Thread.Sleep(4000);
                SpeakOtherLanguage(reminder9, "Vi");
                Thread.Sleep(4000);
                SpeakOtherLanguage(reminder9, "Vi");
                Thread.Sleep(4000);
                SpeakOtherLanguage(reminder9, "Vi");
                Thread.Sleep(4000);
                SpeakOtherLanguage(reminder9, "Vi");
                Thread.Sleep(4000);
                SpeakOtherLanguage(reminder9, "Vi");
                Thread.Sleep(4000);
                SpeakOtherLanguage(reminder9, "Vi");
                Thread.Sleep(4000);

            }



            if (date_time10 == thoi_gian)
            {
                label5.Text = date_time10;

                SpeakOtherLanguage(reminder10, "Vi");
                Thread.Sleep(4000);
                SpeakOtherLanguage(reminder10, "Vi");
                Thread.Sleep(4000);
                SpeakOtherLanguage(reminder10, "Vi");
                Thread.Sleep(4000);
                SpeakOtherLanguage(reminder10, "Vi");
                Thread.Sleep(4000);
                SpeakOtherLanguage(reminder10, "Vi");
                Thread.Sleep(4000);
                SpeakOtherLanguage(reminder10, "Vi");
                Thread.Sleep(4000);
                SpeakOtherLanguage(reminder10, "Vi");
                Thread.Sleep(4000);
                SpeakOtherLanguage(reminder10, "Vi");
                Thread.Sleep(4000);
                SpeakOtherLanguage(reminder10, "Vi");
                Thread.Sleep(4000);


            }


            k++;
            if (k == 10)
            {
                webBrowser1.Navigate("https://sites.google.com/a/student.hcmute.edu.vn/thienphuoc96/");

                laydulieu();

                k = 0;
            }

            gio_so_sanh = textBox8.Text.Trim();

            if (gio_so_sanh == gio_nhac1)
            {

                string a1 = "Đã đến giờ" + " " + nhac_toi1;
                SpeakOtherLanguage(a1, "Vi");
                Thread.Sleep(3000);

            }
            if (gio_so_sanh == gio_nhac2)
            {

                string a1 = "Đã đến giờ" + " " + nhac_toi2;

                SpeakOtherLanguage(a1, "Vi");
                Thread.Sleep(3000);

            }
            if (gio_so_sanh == gio_nhac3)
            {

                string a1 = "Đã đến giờ" + " " + nhac_toi3;
                SpeakOtherLanguage(a1, "Vi");
                Thread.Sleep(3000);

            }
            if (gio_so_sanh == gio_nhac4)
            {

                string a1 = "Đã đến giờ" + " " + nhac_toi4;
                SpeakOtherLanguage(a1, "Vi");
                Thread.Sleep(3000);

            }
            if (gio_so_sanh == gio_nhac5)
            {

                string a1 = "Đã đến giờ" + " " + nhac_toi5;
                SpeakOtherLanguage(a1, "Vi");
                Thread.Sleep(3000);

            }
            if (gio_so_sanh == gio_nhac6)
            {

                string a1 = "Đã đến giờ" + " " + nhac_toi6;
                SpeakOtherLanguage(a1, "Vi");
                Thread.Sleep(3000);

            }
            if (gio_so_sanh == gio_nhac7)
            {

                string a1 = "Đã đến giờ" + " " + nhac_toi7;
                SpeakOtherLanguage(a1, "Vi");
                Thread.Sleep(3000);

            }
            if (gio_so_sanh == gio_nhac8)
            {

                string a1 = "Đã đến giờ" + " " + nhac_toi8;
                SpeakOtherLanguage(a1, "Vi");
                Thread.Sleep(3000);

            }
            if (gio_so_sanh == gio_nhac9)
            {

                string a1 = "Đã đến giờ" + " " + nhac_toi9;
                SpeakOtherLanguage(a1, "Vi");
                Thread.Sleep(3000);

            }
            if (gio_so_sanh == gio_nhac10)
            {

                string a1 = "Đã đến giờ" + " " + nhac_toi10;
                SpeakOtherLanguage(a1, "Vi");
                Thread.Sleep(3000);

            }


            if (gio_so_sanh == gio_chuyen_ngay)
            {
                dem_nhac = 0;
                gio_nhac1 = null;
                gio_nhac2 = null;
                gio_nhac3 = null;
                gio_nhac4 = null;
                gio_nhac5 = null;
                gio_nhac6 = null;
                gio_nhac7 = null;
                gio_nhac8 = null;
                gio_nhac9 = null;
                gio_nhac10 = null;


            }

        }


        void link_nhac()
        {

            dem++;
            if (dem == 5)
                dem = 1;

            switch (dem)
            {
                case 1:
                     axWindowsMediaPlayer1.URL = "D:\\Ben-Rang-O-Moi-Tan-Tai.wav";
                        axWindowsMediaPlayer1.Ctlcontrols.play();
                    break;
                case 2:
                    axWindowsMediaPlayer1.URL = "D:\\La-Trau-Xanh-Thanh-Ngan.wav";
                        axWindowsMediaPlayer1.Ctlcontrols.play();
                    break;
                case 3:
                    axWindowsMediaPlayer1.URL = "D:\\Nho_bien_nha _trang.wav";
                        axWindowsMediaPlayer1.Ctlcontrols.play();
                    break;
                case 4:
                    axWindowsMediaPlayer1.URL = "D:\\Thuong-em-nhieu-qua-la-Thu-xuan-Trong-Huu.wav";
                        axWindowsMediaPlayer1.Ctlcontrols.play();
                    break;
              
             


            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

            // pictureBox1.Visible = true;
            pictureBox12.Visible = true;
            axWindowsMediaPlayer1.URL = "D:\\Ben-Rang-O-Moi-Tan-Tai.wav";
            axWindowsMediaPlayer1.Ctlcontrols.stop();
            webBrowser2.Navigate("https://sites.google.com/a/student.hcmute.edu.vn/thienphuoc96/");
            string a1 = "Xin chào. Tôi là vivo robot. Tôi được chế tạo ra để hỗ trợ người già";
            SpeakOtherLanguage(a1, "Vi");
            Thread.Sleep(8000);


            string a2 = "Hãy chạm vào biểu tượng ghi âm màu đỏ và nói.Tôi sẽ lắng nghe và giúp đỡ bạn";
            SpeakOtherLanguage(a2, "Vi");
            Thread.Sleep(8000);
        }
        private void DataReceive(object obj, SerialDataReceivedEventArgs e)
        {
            InputData = serialPort1.ReadExisting();
            if (InputData != String.Empty)
            {
                //textBox1.Text = InputData; // Ko dùng đc như thế này vì khác threads .
                SetText(InputData);
            }
        }

        private void SetText(string text)
        {
            if (textBox11.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(SetText);
                this.Invoke(d, new object[] { text });
            }

            else
            {
                //textBox11.Text = "";
                textBox11.Text += text;


                for (int i = 0; i < textBox11.Lines.Count(); i++)//xóa dữ liệu từ arduino gửi lên
                {
                    if (textBox11.Lines[i] == "x") { textBox11.Text = ""; break; }
                }
                //   button13.PerformClick();

            }

        }



        private void timer2_Tick(object sender, EventArgs e)
        {
            dem_buon++;
            if (dem_buon == 2)
            {
                string a1 = "Xin lỗi bạn, tôi không biết câu này";
                SpeakOtherLanguage(a1, "Vi");
                Thread.Sleep(3000);

            }
            if (dem_buon == 4)
            {
                timer2.Stop();
                dem_buon = 0;
                pictureBox12.Visible = true;
                pictureBox2.Visible = false;
            }
        }


        private void timer_xla_Tick(object sender, EventArgs e)
        {


            if (dem_xla > 0)
            {


                string a1 = "Xin chào. Tôi là vivo robot. Tôi được chế tạo ra để hỗ trợ người già";
                SpeakOtherLanguage(a1, "Vi");
                Thread.Sleep(8000);


                string a2 = "Hãy chạm vào biểu tượng ghi âm màu đỏ và nói.Tôi sẽ lắng nghe và giúp đỡ bạn";
                SpeakOtherLanguage(a2, "Vi");
                Thread.Sleep(8000);

                if (capture != null)
                {
                    if (captureInProgress)
                    {  //stop the capture

                        capture.Pause();
                    }
                    else
                    {
                        //start the capture

                        capture.Start();
                    }

                    captureInProgress = !captureInProgress;
                }
                mark2[4] = 5000;
                mark2[8] = 8000;
                timer_dem_button2.Start();
                dem_xla = 0;
                timer_xla.Stop();
            }

        }


        void bat_xla()
        {
            if (mark2[0] == mark2[9] && mark2[0] == mark2[8] && mark2[0] == mark2[7] && mark2[0] == mark2[6] && mark2[0] == mark2[5]
               && mark2[0] == mark2[4] && mark2[0] == mark2[3] && mark2[0] == mark2[2] && mark2[0] == mark2[1])
            {
                timer_dem_button2.Stop();
                if (capture != null)
                {
                    if (captureInProgress)
                    {  //stop the capture

                        capture.Pause();
                    }
                    else
                    {
                        //start the capture

                        capture.Start();
                    }

                    captureInProgress = !captureInProgress;
                }
                timer_xla.Start();
                do_tre(3000);
                string a1 = "Tạm biệt. Hẹn gặp lại bạn lần sau";
                SpeakOtherLanguage(a1, "Vi");
                Thread.Sleep(3000);
            }
        }
        private void timer_dem_button2_Tick(object sender, EventArgs e)
        {
            dem_noi2++;
            switch (dem_noi2)
            {
                case 1:
                    mark2[0] = dem_button2;
                    break;
                case 2:
                    mark2[1] = dem_button2;
                    break;
                case 3:
                    mark2[2] = dem_button2;
                    break;
                case 4:
                    mark2[3] = dem_button2;
                    break;
                case 5:
                    mark2[4] = dem_button2;
                    break;
                case 6:
                    mark2[5] = dem_button2;
                    break;
                case 7:
                    mark2[6] = dem_button2;
                    break;
                case 8:
                    mark2[7] = dem_button2;
                    break;
                case 9:
                    mark2[8] = dem_button2;
                    break;
                case 10:
                    mark2[9] = dem_button2;
                    break;


                case 11:
                    dem_noi2 = 0;
                    break;


            }
            bat_xla();
        }

        private void button11_Click(object sender, EventArgs e)
        {
            if (!serialPort1.IsOpen)
            {
                serialPort1.PortName = comboBox4.Text;
                serialPort1.BaudRate = Convert.ToInt32(9600);
                serialPort1.Open();
            }
        }

        private void button14_Click(object sender, EventArgs e)
        {
            string a1 = "Tôi đã ghi nhớ ";// comboBox1.Items[temp1 + 1];
            SpeakOtherLanguage(a1, "Vi");
            using (TextWriter tw12 = new StreamWriter(Convert.ToString(comboBox1.Items[2]) + " " + Convert.ToString(comboBox1.Items[3]) + ".txt", true))
            {
                tw12.WriteLine(comboBox1.Items[2]);
                tw12.WriteLine(comboBox1.Items[3]);
            }
        }

        private void button15_Click(object sender, EventArgs e)
        {
            comboBox7.Items.Clear();
            textBox12.Text = Convert.ToString(comboBox1.Items[2]); // load máy lạnh từ "đây là máy lạnh"
            textBox13.Text = Convert.ToString(comboBox1.Items[3]);
            //textBox12.Text = "nhà";
            // textBox13.Text = "ăn";
            using (StreamReader file2 = new StreamReader(@"C:\Users\User\Desktop\Giao dien dang sua\VoiceNotButton\bin\Debug\" + textBox12.Text + " " + textBox13.Text + ".txt"))
            {
                string line2 = file2.ReadLine();
                while (line2 != null)
                {
                    comboBox7.Items.Add(line2);
                    line2 = file2.ReadLine();
                }
            }
        }

        private void button18_Click(object sender, EventArgs e)
        {
            serialPort1.Write("v");//quay 180 về hướng cũ
            Thread.Sleep(4900);
            for (int k = comboBox7.Items.Count - 3; k >= 0; k--)
            {
                if (Convert.ToString(comboBox7.Items[k]) == "h") { serialPort1.Write("P"); Thread.Sleep(510); textBox11.Text = ""; }
                else if (Convert.ToString(comboBox7.Items[k]) == "k") { serialPort1.Write("O"); Thread.Sleep(1020); textBox11.Text = ""; }
                else if (Convert.ToString(comboBox7.Items[k]) == "g") { serialPort1.Write("I"); Thread.Sleep(1450); textBox11.Text = ""; }
                else if (Convert.ToString(comboBox7.Items[k]) == "l") { serialPort1.Write("U"); Thread.Sleep(1900); textBox11.Text = ""; }
                else if (Convert.ToString(comboBox7.Items[k]) == "f") { serialPort1.Write("Y"); Thread.Sleep(2250); textBox11.Text = ""; }
                else if (Convert.ToString(comboBox7.Items[k]) == "e") { serialPort1.Write("M"); Thread.Sleep(2450); textBox11.Text = ""; }


                else if (Convert.ToString(comboBox7.Items[k]) == "p") { serialPort1.Write("H"); Thread.Sleep(510); textBox11.Text = ""; }
                else if (Convert.ToString(comboBox7.Items[k]) == "o") { serialPort1.Write("K"); Thread.Sleep(1020); textBox11.Text = ""; }
                else if (Convert.ToString(comboBox7.Items[k]) == "i") { serialPort1.Write("G"); Thread.Sleep(1450); textBox11.Text = ""; }
                else if (Convert.ToString(comboBox7.Items[k]) == "u") { serialPort1.Write("L"); Thread.Sleep(1900); textBox11.Text = ""; }
                else if (Convert.ToString(comboBox7.Items[k]) == "y") { serialPort1.Write("F"); Thread.Sleep(2250); textBox11.Text = ""; }
                else if (Convert.ToString(comboBox7.Items[k]) == "m") { serialPort1.Write("E"); Thread.Sleep(2450); textBox11.Text = ""; }
                else if (Convert.ToString(comboBox7.Items[k]) == "z")
                {
                }
                else if (Convert.ToString(comboBox7.Items[k]) != "z" || Convert.ToString(comboBox7.Items[k]) != "m" || Convert.ToString(comboBox7.Items[k]) != "y" || Convert.ToString(comboBox7.Items[k]) != "u" || Convert.ToString(comboBox7.Items[k]) != "i" || Convert.ToString(comboBox7.Items[k]) != "o" || Convert.ToString(comboBox7.Items[k]) != "p" || Convert.ToString(comboBox7.Items[k]) != "h" || Convert.ToString(comboBox7.Items[k]) != "k" || Convert.ToString(comboBox7.Items[k]) != "g" || Convert.ToString(comboBox7.Items[k]) != "l" || Convert.ToString(comboBox7.Items[k]) != "f" || Convert.ToString(comboBox7.Items[k]) != "e" || Convert.ToString(comboBox7.Items[k]) != "")
                //else if (Convert.ToString(comboBox7.Items[k]) != "h")
                {
                    serialPort1.Write(Convert.ToString(comboBox7.Items[k]));
                    serialPort1.Write("z");
                    /* x = Convert.ToString(comboBox7.Items[k]);
                     a = (Convert.ToInt16(x) * 1000);
                    // Thread.Sleep(a);
                     x = "";
                     a = 0;
                     textBox11.Text = "";*/
                }
            }

            serialPort1.Write("v");//quay trái 180
            Thread.Sleep(4900);
            serialPort1.Write("c");
            textBox11.Text = "";
            comboBox1.Items.Clear();
            comboBox7.Items.Clear();

        }

        private void button19_Click(object sender, EventArgs e)
        {
            comboBox7.Items.Clear();
            textBox12.Text = Convert.ToString(comboBox1.Items[0]); // load máy lạnh từ "đây là máy lạnh"
           textBox13.Text = Convert.ToString(comboBox1.Items[1]);
         //     textBox12.Text = "máy";
          //    textBox13.Text = "lạnh";
            using (StreamReader file2 = new StreamReader(@"C:\Users\User\Desktop\Giao dien dang sua\VoiceNotButton\bin\Debug\" + textBox12.Text + " " + textBox13.Text + ".txt"))//+ Convert.ToString(comboBox1.Items[2]) +" "+ Convert.ToString(comboBox1.Items[3])))
            {
                string line2 = file2.ReadLine();
                while (line2 != null)
                {
                    comboBox7.Items.Add(line2);
                    line2 = file2.ReadLine();
                }
            }
        }

        private void button16_Click(object sender, EventArgs e)
        {
            for (int k = 0; k <= comboBox7.Items.Count - 4; k++)
            {

                //textBox14.Text = Convert.ToString(a);
                if (Convert.ToString(comboBox7.Items[k]) == "h") { serialPort1.Write("h"); Thread.Sleep(510); }
                else if (Convert.ToString(comboBox7.Items[k]) == "k") { serialPort1.Write("k"); Thread.Sleep(1020); }

                else if (Convert.ToString(comboBox7.Items[k]) == "g") { serialPort1.Write("g"); Thread.Sleep(1450); }

                else if (Convert.ToString(comboBox7.Items[k]) == "l") { serialPort1.Write("l"); Thread.Sleep(1900); }

                else if (Convert.ToString(comboBox7.Items[k]) == "f") { serialPort1.Write("f"); Thread.Sleep(2250); }

                else if (Convert.ToString(comboBox7.Items[k]) == "e") { serialPort1.Write("e"); Thread.Sleep(2450); }

                else if (Convert.ToString(comboBox7.Items[k]) == "p") { serialPort1.Write("p"); Thread.Sleep(510); }
                else if (Convert.ToString(comboBox7.Items[k]) == "o") { serialPort1.Write("o"); Thread.Sleep(1020); }

                else if (Convert.ToString(comboBox7.Items[k]) == "i") { serialPort1.Write("i"); Thread.Sleep(1450); }

                else if (Convert.ToString(comboBox7.Items[k]) == "u") { serialPort1.Write("u"); Thread.Sleep(1900); }

                else if (Convert.ToString(comboBox7.Items[k]) == "y") { serialPort1.Write("y"); Thread.Sleep(2250); }

                else if (Convert.ToString(comboBox7.Items[k]) == "m") { serialPort1.Write("m"); Thread.Sleep(2450); }

                //  else if (Convert.ToString(comboBox7.Items[k]) != "h" ||Convert.ToString(comboBox7.Items[k]) != "k" ||Convert.ToString(comboBox7.Items[k]) != "g" ||Convert.ToString(comboBox7.Items[k]) != "l" ||Convert.ToString(comboBox7.Items[k]) != "f" ||Convert.ToString(comboBox7.Items[k]) != "e" ||Convert.ToString(comboBox7.Items[k]) != "" )
                else if (Convert.ToString(comboBox7.Items[k]) == "z")
                {

                }

                else if (Convert.ToString(comboBox7.Items[k]) != "h")
                {
                    serialPort1.Write(Convert.ToString(comboBox7.Items[k]));
                    serialPort1.Write("z");
                    x = Convert.ToString(comboBox7.Items[k]);
                    a = (Convert.ToInt16(x) * 1000);
                    // Thread.Sleep(a);
                    x = "";
                    a = 0;

                }
            }
        }

        private void imageBox1_Click(object sender, EventArgs e)
        {

        }

        private void button13_Click(object sender, EventArgs e)
        {
            textBox12.Text = Convert.ToString(comboBox1.Items[2]);
            textBox13.Text = Convert.ToString(comboBox1.Items[3]);
            FileStream ok = File.Open(textBox12.Text + " " + textBox13.Text + ".txt", FileMode.Append); //khỏi tạo text mới  
            //  FileStream ok = File.Open(Convert.ToString(comboBox1.Items[3]) + Convert.ToString(comboBox1.Items[4]) + ".txt", FileMode.Append); //khỏi tạo text mới//chơi thôi
            StreamWriter w = new StreamWriter(ok);

            w.WriteLine(textBox11.Text);
            w.Close();
        }
        void xoa_anh_pin()
        {
            power100.Visible = false;
            power80.Visible = false;
            power60.Visible = false;
            power40.Visible = false;
            power20.Visible = false;
            power0.Visible = false;
            powersac.Visible = false;
        }
        private void timer_pin_Tick(object sender, EventArgs e)
        {
           

         
            label5.Text = t_ngay;
            dem_pin++;
            if (dem_pin == 900)
            {
                xoa_anh_pin();
                power80.Visible = true;
            }
            if (dem_pin == 1800)
            {
                xoa_anh_pin();
                power60.Visible = true;
            }
            if (dem_pin == 2700)
            {
                xoa_anh_pin();
                power40.Visible = true;
            }
            if (dem_pin == 3600)
            {
                xoa_anh_pin();
                power20.Visible = true;
            }
            if (dem_pin == 4500)
            {
                xoa_anh_pin();
                power0.Visible = true;
            }
            if (dem_pin == 5400)
            {
                xoa_anh_pin();
                powersac.Visible = true;

            }
            if (dem_pin == 6300)
            {
                xoa_anh_pin();

                power100.Visible = true;
                dem_pin = 0;
            }
        }

        private void timer_am_thanh_Tick(object sender, EventArgs e)
        {

                var phuoc = (NAudio.CoreAudioApi.MMDevice)comboBox3.Items[2];
               
                progressBar1.Value = (int)(Math.Round(phuoc.AudioMeterInformation.MasterPeakValue * 100));
               label3.Text = Convert.ToString(progressBar1.Value);

                if (mark3[0] <10 && mark3[1] <10 && mark3[2] <10 && mark3[3] <10 && mark3[4] <10
              && mark3[5] < 10 && mark3[6] < 10 && mark3[7] < 10 && mark3[8] < 10 && mark3[9] < 10)
                {
                    button2.PerformClick();
                    timer_am_thanh.Stop();
                    timer_dem_am_thanh.Stop();
                }

            
        }

        private void timer_dem_am_thanh_Tick(object sender, EventArgs e)
        {
            dem_am_thanh++;
            switch (dem_am_thanh)
            {
                case 1:
                    mark3[0] = progressBar1.Value;
                    
                    break;
                case 2:
                    mark3[1] = progressBar1.Value;
                   
                    break;
                case 3:
                    mark3[2] = progressBar1.Value;
                    break;
                case 4:
                    mark3[3] = progressBar1.Value;
                    break;
                case 5:
                    mark3[4] = progressBar1.Value;
                    break;
                case 6:
                    mark3[5] = progressBar1.Value;
                    break;
                case 7:
                    mark3[6] = progressBar1.Value;
                    break;
                case 8:
                    mark3[7] = progressBar1.Value;
                    break;
                case 9:
                    mark3[8] = progressBar1.Value;
                    break;
                case 10:
                    mark3[9] = progressBar1.Value;
                    break;
                case 11:
                    dem_am_thanh = 0;
                    break;
            }
        }

        private void pictureBox_button1_Click(object sender, EventArgs e)
        {
            button1.PerformClick();
            pictureBox_button1.Visible = false;
            pictureBox_button2.Visible = true;
        }

        private void pictureBox_button2_Click(object sender, EventArgs e)
        {
            button2.PerformClick();
            pictureBox_button1.Visible = true;
            pictureBox_button2.Visible = false;
        }

        private void pictureBox12_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {

        }
    }

}