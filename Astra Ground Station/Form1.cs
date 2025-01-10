using GMap.NET;
using GMap.NET.MapProviders;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.IO.Ports;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;
namespace arayüz_okuma
{
    public partial class Form1 : Form
    {
        public struct payload
        {
            public float bme_alt, bme_pres, bme_temp;
            public float gps_alt, gps_lat, gps_lon;
            public float gyro_x, gyro_y, gyro_z, acc_x, acc_y, acc_z;
            public float pl_angx, pl_angy, pl_angz;
        }
        public struct Rocket
        {
            public float bme_alt, bme_pres, bme_temp;
            public float gps_alt, gps_lat, gps_lon;
            public float gyro_x, gyro_y, gyro_z, acc_x, acc_y, acc_z;
            public float rc_angx, rc_angy, rc_angz, rc_angle;
            public int packetnbr;
            public float magx, magy, magz;
            public int rc_stat;
            public float speed;
        }
        string start_time = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
        StreamWriter log_file_astra;
        String[] ports = SerialPort.GetPortNames();
        public byte[] package = new byte[78];
        public int checksum;
        Rocket rocket = new Rocket();
        payload Payload = new payload();
        bool hyi;
        int minm = 0;
        int maksm = 0;
        private Process _unityProcess;
        public float adress_lat = 38.3686F;
        public float adress_lon = 34.0297F;
        string similas;
        public Form1()
        {
            InitializeComponent();
            serialPort1.BaudRate = 9600;
            serialPort1.RtsEnable = true;
            serialPort1.DtrEnable = true;
            Control.CheckForIllegalCrossThreadCalls = false;
            this.FormClosing += Form1_FormClosing;

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            string log_path = Path.Combine(Application.StartupPath, start_time + "_astra_log.txt");
            if (!File.Exists(log_path))
            {
                File.Create(log_path).Close();
            }
            log_file_astra = new StreamWriter(log_path, true);
            gMapControl1.MapProvider = GMapProviders.GoogleMap;
            gMapControl1.Position = new PointLatLng(38.7228, 33.5683);
            gMapControl2.MapProvider = GMapProviders.GoogleMap;
            gMapControl2.Position = new PointLatLng(40.9917, 28.83);
            StartUnityApplication();
            rocket.packetnbr = 0;
        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_unityProcess != null && !_unityProcess.HasExited)
            {
                _unityProcess.CloseMainWindow(); // Unity uygulamasını düzgün bir şekilde kapatmayı dener
                _unityProcess.WaitForExit(1000); // Kapanması için bekler (isteğe bağlı timeout)
                if (!_unityProcess.HasExited)
                {
                    _unityProcess.Kill(); // Hala kapanmamışsa zorla kapatır
                }
            }
        }

        private void serialPort1_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                // Veriyi oku
                string data = serialPort1.ReadLine();  // ReadLine ile veriyi oku
                if (string.IsNullOrEmpty(data))
                {
                    label27.ForeColor = Color.Red;
                    label27.Text= "Gelen veri boş";
                    return;
                }
                log_file_astra.Write(data);
                // Veriyi ',' karakterine göre ayır
                string[] splitdata = data.Split(',');
                // Veriyi ilgili kontrol elemanlarına yerleştir
                this.Invoke((MethodInvoker)delegate
                {
                    // splitdata dizisinin uzunluğunu kontrol et
                    if (splitdata.Length >= 14)  // Minimum uzunluk 19, veri formatınıza bağlı olarak 20'yi deneyin
                    {
                        try
                        {
                            // Verileri ekranda göster
                            label3.Text = data;
                            // Verileri parse et ve ekranda göster
                            if (splitdata[0] == "$AR")
                            {
                                if (float.TryParse(splitdata[1], NumberStyles.Float, CultureInfo.InvariantCulture, out float lat)) rocket.gps_lat = lat;
                                if (float.TryParse(splitdata[2], NumberStyles.Float, CultureInfo.InvariantCulture, out float lon)) rocket.gps_lon = lon;
                                if (float.TryParse(splitdata[3], NumberStyles.Float, CultureInfo.InvariantCulture, out float gpsAlt)) rocket.gps_alt = gpsAlt;
                                if (float.TryParse(splitdata[4], NumberStyles.Float, CultureInfo.InvariantCulture, out float bmePres)) rocket.bme_pres = bmePres;
                                if (float.TryParse(splitdata[5], NumberStyles.Float, CultureInfo.InvariantCulture, out float bmealt)) rocket.bme_alt = bmealt;
                                if (float.TryParse(splitdata[6], NumberStyles.Float, CultureInfo.InvariantCulture, out float gyroX)) rocket.gyro_x = gyroX;
                                if (float.TryParse(splitdata[7], NumberStyles.Float, CultureInfo.InvariantCulture, out float gyroY)) rocket.gyro_y = gyroY;
                                if (float.TryParse(splitdata[8], NumberStyles.Float, CultureInfo.InvariantCulture, out float gyroZ)) rocket.gyro_z = gyroZ;
                                if (float.TryParse(splitdata[9], NumberStyles.Float, CultureInfo.InvariantCulture, out float accX)) rocket.acc_x = accX;
                                if (float.TryParse(splitdata[10], NumberStyles.Float, CultureInfo.InvariantCulture, out float accY)) rocket.acc_y = accY;
                                if (float.TryParse(splitdata[11], NumberStyles.Float, CultureInfo.InvariantCulture, out float accZ)) rocket.acc_z = accZ;
                                if (float.TryParse(splitdata[12], NumberStyles.Float, CultureInfo.InvariantCulture, out float angles)) rocket.rc_angle = angles;
                                if (float.TryParse(splitdata[13], NumberStyles.Float, CultureInfo.InvariantCulture, out float durum)) rocket.rc_stat = (int)durum;


                                accx_label.Text = Convert.ToString(rocket.acc_x);
                                accy_label.Text = Convert.ToString(rocket.acc_y);
                                accz_label.Text = Convert.ToString(rocket.acc_z);
                                gyrox_label.Text = Convert.ToString(rocket.gyro_x);
                                gyroy_label.Text = Convert.ToString(rocket.gyro_y);
                                gyroz_label.Text = Convert.ToString(rocket.gyro_z);
                                lat_label.Text = Convert.ToString(rocket.gps_lat);
                                lon_label.Text = Convert.ToString(rocket.gps_lon);
                                gps_alt_label.Text = Convert.ToString(rocket.gps_alt);
                                bme_pres_label.Text = Convert.ToString(rocket.bme_pres);
                                drm.Text = Convert.ToString(rocket.rc_stat);
                                bme_alt_label.Text = Convert.ToString(rocket.bme_alt);
                                angle.Text = Convert.ToString(rocket.rc_angle);
                                // Paket numarasını güncelle ve etikette göster

                                label2.Text = rocket.packetnbr.ToString();
                                similas = splitdata[10] + "," + splitdata[11] + "," + splitdata[12];
                                // X ekseni ayarları
                                chart1.ChartAreas[0].AxisX.Minimum = 0;
                                chart1.ChartAreas[0].AxisX.Maximum = maksm;
                                chart1.ChartAreas[0].AxisY.Minimum = 0;
                                chart1.ChartAreas[0].AxisY.Maximum = 120000;
                                // Mevcut maksimum Y ekseni değeri (float olarak)
                                float currentYAxisMax = (float)chart1.ChartAreas[0].AxisY.Maximum;
                                // Mevcut veri noktasının Y değeri (rocket.bme_pres)
                                float currentValue = rocket.bme_pres;
                                // Eğer mevcut veri noktası, Y ekseninin maksimum değerinden büyükse
                                if (currentValue > currentYAxisMax)
                                {
                                    // Y ekseninin maksimum değerini güncelle
                                    chart1.ChartAreas[0].AxisY.Maximum = currentValue;
                                }
                                // Yeni veri noktası ekle
                                this.chart1.Series[0].Points.AddXY(maksm, currentValue);
                                // Grafik serisinin sınır genişliği
                                //chart1.Series["Series1"].BorderWidth = 3;
                                //  chart1.Series["Series1"].BorderColor = Color.FromArgb(255, 0, 0); // Kırmızı renk
                                chart2.ChartAreas[0].AxisX.Minimum = 0;
                                chart2.ChartAreas[0].AxisX.Maximum = maksm;
                                chart2.ChartAreas[0].AxisY.Minimum = 0;
                                chart2.ChartAreas[0].AxisY.Maximum = 180;
                                chart2.ChartAreas[0].AxisX.ScaleView.Zoom(minm, maksm);
                                this.chart2.Series[0].Points.AddXY(maksm, rocket.rc_angle);
                                chart3.ChartAreas[0].AxisX.Minimum = 0;
                                chart3.ChartAreas[0].AxisX.Maximum = maksm;
                                chart3.ChartAreas[0].AxisY.Minimum = 0;
                                if (rocket.bme_alt > 900)
                                {
                                    chart3.ChartAreas[0].AxisY.Maximum = rocket.bme_alt;
                                }
                                else
                                {
                                    chart3.ChartAreas[0].AxisY.Maximum = 900;
                                }
                                chart3.ChartAreas[0].AxisX.ScaleView.Zoom(minm, maksm);
                                this.chart3.Series[0].Points.AddXY(maksm, rocket.bme_alt);
                                // chart4.ChartAreas[0].AxisX.Minimum = 0;
                                // chart4.ChartAreas[0].AxisX.Maximum = maksm;
                                // chart4.ChartAreas[0].AxisY.Minimum = 0;
                                // chart4.ChartAreas[0].AxisY.Maximum = 1200;
                                // chart4.ChartAreas[0].AxisX.ScaleView.Zoom(minm, maksm);
                                // this.chart4.Series[0].Points.AddXY(maksm/2 , payload.bme_alt);
                                maksm++;
                                // Veriyi Unity'e gönder
                                gMapControl1.Position = new PointLatLng(rocket.gps_lat, rocket.gps_lon);

                                SendDataToUnity(similas);
                                string basePath = AppDomain.CurrentDomain.BaseDirectory;
                                string txtPath = Path.Combine(basePath, "Astra_Rocket_Log.txt");

                                using (StreamWriter writer = new StreamWriter(txtPath, true))
                                {
                                    writer.WriteLine(data);
                                }
                                gMapControl2.Position = new PointLatLng(Payload.gps_lat, Payload.gps_lon);
                            }
                            label27.ForeColor = Color.Green;
                                label27.Text = "başarılı";
                            
                            // Rocket objesini güncelle
                        }
                        catch (Exception ex)
                        {
                            // Verilerin işlenmesi sırasında oluşan hataları yakala
                            label27.ForeColor = Color.Red;
                            label27.Text="Veri işlenirken hata oluştu";
                        }
                    }
                    else
                    {
                        // Hata mesajı göster veya işleme yap
                        label27.ForeColor = Color.Red;
                        label27.Text="Gelen veri formatı hatalı veya eksik.";
                    }
                });
            }
            catch (Exception ex)
            {
                // Genel hata mesajı
                label27.ForeColor = Color.Red;
                label27.Text = $"Hata: {ex.Message}";
                
            }
        }
        private void button1_Click(object sender, EventArgs e)
        {
            start_time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            if (!serialPort1.IsOpen)
            {
                try
                {
                    serialPort1.PortName = comboBox1.Text;
                    serialPort2.PortName = comboBox3.Text;
                    serialPort3.PortName = comboBox2.Text;
                    try
                    {
                        serialPort1.Open();
                        serialPort2.Open();
                        serialPort3.Open();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Seri port açılamadı: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    // Veri alım olayına dinleyici ekle
                    serialPort1.DataReceived += new SerialDataReceivedEventHandler(serialPort1_DataReceived);
                    serialPort3.DataReceived += new SerialDataReceivedEventHandler(serialPort3_DataReceived);
                }
                catch
                {
                }
            }
        }
        private void SendDataToUnity(string simila)
        {
            UdpClient udpClient = new UdpClient();
            try
            {
                // Unity'deki UDP sunucusunun IP adresi ve port numarası
                string unityIP = "127.0.0.1"; // böyle iyi
                int unityPort = 6063;
                // Gönderilecek veri
                byte[] veri = Encoding.ASCII.GetBytes(simila);
                // Veriyi gönderin
                udpClient.Send(veri, veri.Length, unityIP, unityPort);
            }
            catch (Exception e)
            {
                MessageBox.Show("Send error: " + e.Message);
            }
            finally
            {
                udpClient.Close();
            }
        }
        public byte[] float_to_byte(float input_data)
        {
            return BitConverter.GetBytes(input_data);
        }
        private void button2_Click(object sender, EventArgs e)
        {
            serialPort1.Close();
        }
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
        }
        private void button3_Click(object sender, EventArgs e)
        {
            
            hyi = true;
        }
        private void button4_Click(object sender, EventArgs e)
        {
            serialPort2.Close();
            hyi = false;
        }
        private void timer1_Tick_1(object sender, EventArgs e)
        {
            if (hyi == true)
            {
                checksum = 0;
                package[0] = 0xFF; // Sabit
                package[1] = 0xFF; // Sabit
                package[2] = 0x54; // Sabit
                package[3] = 0x52; // Sabit
                package[4] = 114; 
                package[5] = (byte)(rocket.packetnbr & 0xFF);
                package[6] = float_to_byte(rocket.bme_alt)[0];
                package[7] = float_to_byte(rocket.bme_alt)[1];
                package[8] = float_to_byte(rocket.bme_alt)[2];
                package[9] = float_to_byte(rocket.bme_alt)[3];
                package[10] = float_to_byte(rocket.gps_alt)[0];
                package[11] = float_to_byte(rocket.gps_alt)[1];
                package[12] = float_to_byte(rocket.gps_alt)[2];
                package[13] = float_to_byte(rocket.gps_alt)[3];
                package[14] = float_to_byte(rocket.gps_lat)[0];
                package[15] = float_to_byte(rocket.gps_lat)[1];
                package[16] = float_to_byte(rocket.gps_lat)[2];
                package[17] = float_to_byte(rocket.gps_lat)[3];
                package[18] = float_to_byte(rocket.gps_lon)[0];
                package[19] = float_to_byte(rocket.gps_lon)[1];
                package[20] = float_to_byte(rocket.gps_lon)[2];
                package[21] = float_to_byte(rocket.gps_lon)[3];
                package[22] = float_to_byte(Payload.gps_alt)[0];
                package[23] = float_to_byte(Payload.gps_alt)[1];
                package[24] = float_to_byte(Payload.gps_alt)[2];
                package[25] = float_to_byte(Payload.gps_alt)[3];
                package[26] = float_to_byte(Payload.gps_lat)[0];
                package[27] = float_to_byte(Payload.gps_lat)[1];
                package[28] = float_to_byte(Payload.gps_lat)[2];
                package[29] = float_to_byte(Payload.gps_lat)[3];
                package[30] = float_to_byte(Payload.gps_lon)[0];
                package[31] = float_to_byte(Payload.gps_lon)[1];
                package[32] = float_to_byte(Payload.gps_lon)[2];
                package[33] = float_to_byte(Payload.gps_lon)[3];
                //Kademe yok bizde
                package[34] = 0;
                package[35] = 0;
                package[36] = 0;
                package[37] = 0;
                package[38] = 0;
                package[39] = 0;
                package[40] = 0;
                package[41] = 0;
                package[42] = 0;
                package[43] = 0;
                package[44] = 0;
                package[45] = 0;
                package[46] = float_to_byte(rocket.gyro_x)[0];
                package[47] = float_to_byte(rocket.gyro_x)[1];
                package[48] = float_to_byte(rocket.gyro_x)[2];
                package[49] = float_to_byte(rocket.gyro_x)[3];
                package[50] = float_to_byte(rocket.gyro_y)[0];
                package[51] = float_to_byte(rocket.gyro_y)[1];
                package[52] = float_to_byte(rocket.gyro_y)[2];
                package[53] = float_to_byte(rocket.gyro_y)[3];
                package[54] = float_to_byte(rocket.gyro_z)[0];
                package[55] = float_to_byte(rocket.gyro_z)[1];
                package[56] = float_to_byte(rocket.gyro_z)[2];
                package[57] = float_to_byte(rocket.gyro_z)[3];
                package[58] = float_to_byte(rocket.acc_x)[0];
                package[59] = float_to_byte(rocket.acc_x)[1];
                package[60] = float_to_byte(rocket.acc_x)[2];
                package[61] = float_to_byte(rocket.acc_x)[3];
                package[62] = float_to_byte(rocket.acc_y)[0];
                package[63] = float_to_byte(rocket.acc_y)[1];
                package[64] = float_to_byte(rocket.acc_y)[2];
                package[65] = float_to_byte(rocket.acc_y)[3];
                package[66] = float_to_byte(rocket.acc_z)[0];
                package[67] = float_to_byte(rocket.acc_z)[1];
                package[68] = float_to_byte(rocket.acc_z)[2];
                package[69] = float_to_byte(rocket.acc_z)[3];
                package[70] = float_to_byte(rocket.rc_angle)[0];
                package[71] = float_to_byte(rocket.rc_angle)[1];
                package[72] = float_to_byte(rocket.rc_angle)[2];
                package[73] = float_to_byte(rocket.rc_angle)[3];
                
                package[74] = (byte)(rocket.rc_stat & 0xFF);
                checksum = 0;
                for (int i = 4; i < 75; i++)
                    checksum += package[i];
                package[75] = (byte)(checksum % 256);
                package[76] = 0x0D;
                package[77] = 0x0A;              
                serialPort2.Write(package, 0, 78);
                rocket.packetnbr++;
                if (rocket.packetnbr > 255)
                {
                    rocket.packetnbr = 0;
                }
            }
        }
        private void StartUnityApplication()
        {
            _unityProcess = new Process();
            string basePath = AppDomain.CurrentDomain.BaseDirectory;
            string exePath = Path.Combine(basePath, "similasyon\\ozgunn.exe");
            _unityProcess.StartInfo.FileName = exePath;
            _unityProcess.StartInfo.Arguments = "-parentHWND " + panel4.Handle.ToInt32();
            _unityProcess.StartInfo.UseShellExecute = true;
            _unityProcess.Start();
            _unityProcess.WaitForInputIdle();
            SetParent(_unityProcess.MainWindowHandle, panel1.Handle);
            SendMessage(_unityProcess.MainWindowHandle, WM_SYSCOMMAND, SC_MAXIMIZE, 0);
        }
        private const int WM_SYSCOMMAND = 0x0112;
        private const int SC_MAXIMIZE = 0xF030;
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern int SendMessage(IntPtr hWnd, uint Msg, int wParam, int lParam);
        private void timer2_Tick(object sender, EventArgs e)
        {
        }
        private void gMapControl1_Load(object sender, EventArgs e)
        {
        }
        private void PORT_button_Click(object sender, EventArgs e)
        {
            foreach (var port in ports)
            {
                if (!comboBox1.Items.Contains(port))
                {
                    comboBox1.Items.Add(port);
                }
            }
            foreach (var port in ports)
            {
                if (!comboBox2.Items.Contains(port))
                {
                    comboBox2.Items.Add(port);
                }
            }
            foreach (var port in ports)
            {
                if (!comboBox3.Items.Contains(port))
                {
                    comboBox3.Items.Add(port);
                }
            }

        }
        private void serialPort3_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            string data2 = serialPort3.ReadLine();
            label28.Text = data2;

            log_file_astra.Write(data2);
            string[] splitdata2 = data2.Split(',');
            if (splitdata2[0] == "$AP")
            {
                if(splitdata2.Length >= 6)
                {
                    if (float.TryParse(splitdata2[1], NumberStyles.Float, CultureInfo.InvariantCulture, out float lat2)) Payload.gps_lat = lat2;
                    if (float.TryParse(splitdata2[2], NumberStyles.Float, CultureInfo.InvariantCulture, out float lon2)) Payload.gps_lon = lon2;
                    if (float.TryParse(splitdata2[3], NumberStyles.Float, CultureInfo.InvariantCulture, out float gpsAlt2)) Payload.gps_alt = gpsAlt2;
                    if (float.TryParse(splitdata2[4], NumberStyles.Float, CultureInfo.InvariantCulture, out float bmePres2)) Payload.bme_pres = bmePres2;
                    if (float.TryParse(splitdata2[5], NumberStyles.Float, CultureInfo.InvariantCulture, out float bmealt2)) Payload.bme_alt = bmealt2;
                }
                else
                {

                    label28.Text = "veri hatalı";
                }


                fyaccx_label.Text = Convert.ToString(Payload.acc_x);
                fyaccy_label.Text = Convert.ToString(Payload.acc_y);
                fyaccz_label.Text = Convert.ToString(Payload.acc_z);
                fygyrox_label.Text = Convert.ToString(Payload.gyro_x);
                fygyroy_label.Text = Convert.ToString(Payload.gyro_y);
                fygyroz_label.Text = Convert.ToString(Payload.gyro_z);
                fyrx_angx_label.Text = Convert.ToString(Payload.pl_angx);
                fyrx_angy_label.Text = Convert.ToString(Payload.pl_angy);
                fyrx_angz_label.Text = Convert.ToString(Payload.pl_angz);
                fylat_label.Text = Convert.ToString(Payload.gps_lat);
                fylon_label.Text = Convert.ToString(Payload.gps_lon);
                fygps_alt_label.Text = Convert.ToString(Payload.gps_alt);
                fybme_alt_label.Text = Convert.ToString(Payload.bme_alt);
                fybme_pres_label.Text = Convert.ToString(Payload.bme_pres);
                fybme_tmp_label.Text = Convert.ToString(Payload.bme_temp);




                string basePath = AppDomain.CurrentDomain.BaseDirectory;
                string txtPath = Path.Combine(basePath, "Astra_Payload_Log.txt");
                string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                string logEntry = $"{timestamp} - {data2}";
                using (StreamWriter writer = new StreamWriter(txtPath, true))
                {
                    writer.WriteLine(data2);
                }
                gMapControl2.Position = new PointLatLng(Payload.gps_lat, Payload.gps_lon);
            }
        }
        private void label14_Click(object sender, EventArgs e)
        {
        }
        private void label27_Click(object sender, EventArgs e)
        {
        }

        private void button5_Click(object sender, EventArgs e)
        {
            serialPort1.Write("T");
            serialPort2.Write("T");
        }

        private void button6_Click(object sender, EventArgs e)
        {
            serialPort1.Write("R");
            serialPort2.Write("R");
        }
    }
}
