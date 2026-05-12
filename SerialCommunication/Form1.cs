using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace SerialCommunication
{
    public partial class Form1 : Form
    {
        private SerialPort serialPortArduino;
        private System.Windows.Forms.Timer timerOefening3;
        private System.Windows.Forms.Timer timerOefening4;
        private System.Windows.Forms.Timer timerOefening5;
        public Form1()
        {
            InitializeComponent();

            // instantiate serial port and set timeouts (milliseconds)
            serialPortArduino = new SerialPort();
            serialPortArduino.ReadTimeout = 1000;
            serialPortArduino.WriteTimeout = 1000;

            // timer for oefening 3 (interval in milliseconds)
            timerOefening3 = new System.Windows.Forms.Timer();
            timerOefening3.Interval = 1000; // 1000 ms
            timerOefening3.Tick += timerOefening3_Tick;
            timerOefening3.Enabled = false;
            // timer for oefening 4 (interval in milliseconds)
            timerOefening4 = new System.Windows.Forms.Timer();
            timerOefening4.Interval = 1000; // 1000 ms
            timerOefening4.Tick += timerOefening4_Tick;
            timerOefening4.Enabled = false;
            // timer for oefening 5 (interval in milliseconds)
            timerOefening5 = new System.Windows.Forms.Timer();
            timerOefening5.Interval = 1000; // 1000 ms
            timerOefening5.Tick += timerOefening5_Tick;
            timerOefening5.Enabled = false;
            // handle tab selection changes to enable/disable the timers
            tabControl.SelectedIndexChanged += tabControl_SelectedIndexChanged;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                string[] portNames = SerialPort.GetPortNames().Distinct().ToArray();
                comboBoxPoort.Items.Clear();
                comboBoxPoort.Items.AddRange(portNames);
                if (comboBoxPoort.Items.Count > 0) comboBoxPoort.SelectedIndex = 0;

                comboBoxBaudrate.SelectedIndex = comboBoxBaudrate.Items.IndexOf("115200");
            }
            catch (Exception)
            { }
        }

        private void cboPoort_DropDown(object sender, EventArgs e)
        {
            try
            {
                string selected = (string)comboBoxPoort.SelectedItem;
                string[] portNames = SerialPort.GetPortNames().Distinct().ToArray();

                comboBoxPoort.Items.Clear();
                comboBoxPoort.Items.AddRange(portNames);

                comboBoxPoort.SelectedIndex = comboBoxPoort.Items.IndexOf(selected);
            }
            catch (Exception)
            {
                if (comboBoxPoort.Items.Count > 0) comboBoxPoort.SelectedIndex = 0;
            }
        }

        private void buttonConnect_Click(object sender, EventArgs e)
        {
            try
            {
                if (serialPortArduino.IsOpen)
                {
                    // we are connected — disconnect
                    serialPortArduino.Close();
                    radioButtonVerbonden.Checked = false;
                    buttonConnect.Text = "Connect";
                    buttonConnect.BackColor = System.Drawing.Color.Blue;
                    labelStatus.Text = "Disconnected";
                }
                else
                {
                    // not connected — set necessary properties and open
                    if (comboBoxPoort.SelectedItem != null)
                        serialPortArduino.PortName = comboBoxPoort.SelectedItem.ToString();

                    int baud = 115200;
                    if (comboBoxBaudrate.SelectedItem != null)
                        int.TryParse(comboBoxBaudrate.SelectedItem.ToString(), out baud);
                    serialPortArduino.BaudRate = baud;

                    // data bits
                    serialPortArduino.DataBits = (int)numericUpDownDatabits.Value;

                    // parity
                    if (radioButtonParityEven.Checked) serialPortArduino.Parity = Parity.Even;
                    else if (radioButtonParityOdd.Checked) serialPortArduino.Parity = Parity.Odd;
                    else if (radioButtonParityNone.Checked) serialPortArduino.Parity = Parity.None;
                    else if (radioButtonParityMark.Checked) serialPortArduino.Parity = Parity.Mark;
                    else if (radioButtonParitySpace.Checked) serialPortArduino.Parity = Parity.Space;

                    // stop bits
                    if (radioButtonStopbitsNone.Checked) serialPortArduino.StopBits = StopBits.None;
                    else if (radioButtonStopbitsOne.Checked) serialPortArduino.StopBits = StopBits.One;
                    else if (radioButtonStopbitsOnePointFive.Checked) serialPortArduino.StopBits = StopBits.OnePointFive;
                    else if (radioButtonStopbitsTwo.Checked) serialPortArduino.StopBits = StopBits.Two;

                    // handshake
                    if (radioButtonHandshakeNone.Checked) serialPortArduino.Handshake = Handshake.None;
                    else if (radioButtonHandshakeRTS.Checked) serialPortArduino.Handshake = Handshake.RequestToSend;
                    else if (radioButtonHandshakeRTSXonXoff.Checked) serialPortArduino.Handshake = Handshake.RequestToSendXOnXOff;
                    else if (radioButtonHandshakeXonXoff.Checked) serialPortArduino.Handshake = Handshake.XOnXOff;

                    // RTS / DTR
                    serialPortArduino.RtsEnable = checkBoxRtsEnable.Checked;
                    serialPortArduino.DtrEnable = checkBoxDtrEnable.Checked;

                    // open and verify device responds to ping
                    serialPortArduino.Open();
                    serialPortArduino.DiscardInBuffer();
                    serialPortArduino.WriteLine("ping");

                    string response = string.Empty;
                    try
                    {
                        response = serialPortArduino.ReadLine().Trim();
                    }
                    catch (TimeoutException)
                    {
                        // no response within timeout
                    }

                    if (string.Equals(response, "pong", StringComparison.OrdinalIgnoreCase))
                    {
                        radioButtonVerbonden.Checked = true;
                        buttonConnect.Text = "Disconnect";
                        buttonConnect.BackColor = System.Drawing.Color.Red;
                        labelStatus.Text = "Connected";
                    }
                    else
                    {
                        serialPortArduino.Close();
                        MessageBox.Show("Geen geldig antwoord ontvangen van apparaat. Antwoord: " + (string.IsNullOrEmpty(response) ? "(geen)" : response));
                        labelStatus.Text = "Disconnected";
                        radioButtonVerbonden.Checked = false;
                        buttonConnect.Text = "Connect";
                        buttonConnect.BackColor = System.Drawing.Color.Blue;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Fout bij (dis)connect: " + ex.Message);
                labelStatus.Text = "Error: " + ex.Message;
            }
        }

        private void checkBoxDigital2_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                if (serialPortArduino == null || !serialPortArduino.IsOpen)
                {
                    MessageBox.Show("Geen open seriële verbinding.");
                    return;
                }

                string command = checkBoxDigital2.Checked ? "set d2 high" : "set d2 low";
                serialPortArduino.WriteLine(command);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Fout bij verzenden: " + ex.Message);
                labelStatus.Text = "Error: " + ex.Message;
            }
        }

        private void checkBoxDigital3_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                if (serialPortArduino == null || !serialPortArduino.IsOpen)
                {
                    MessageBox.Show("Geen open seriële verbinding.");
                    return;
                }

                string command = checkBoxDigital3.Checked ? "set d3 high" : "set d3 low";
                serialPortArduino.WriteLine(command);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Fout bij verzenden: " + ex.Message);
                labelStatus.Text = "Error: " + ex.Message;
            }
        }

        private void checkBoxDigital4_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                if (serialPortArduino == null || !serialPortArduino.IsOpen)
                {
                    MessageBox.Show("Geen open seriële verbinding.");
                    return;
                }

                string command = checkBoxDigital4.Checked ? "set d4 high" : "set d4 low";
                serialPortArduino.WriteLine(command);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Fout bij verzenden: " + ex.Message);
                labelStatus.Text = "Error: " + ex.Message;
            }
        }

        private void trackBarPWM9_Scroll(object sender, EventArgs e)
        {
            try
            {
                if (serialPortArduino == null || !serialPortArduino.IsOpen)
                {
                    MessageBox.Show("Geen open seriële verbinding.");
                    return;
                }

                string command = "set pwm9 " + trackBarPWM9.Value.ToString();
                serialPortArduino.WriteLine(command);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Fout bij verzenden: " + ex.Message);
                labelStatus.Text = "Error: " + ex.Message;
            }
        }

        private void trackBarPWM10_Scroll(object sender, EventArgs e)
        {
            try
            {
                if (serialPortArduino == null || !serialPortArduino.IsOpen)
                {
                    MessageBox.Show("Geen open seriële verbinding.");
                    return;
                }

                string command = "set pwm10 " + trackBarPWM10.Value.ToString();
                serialPortArduino.WriteLine(command);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Fout bij verzenden: " + ex.Message);
                labelStatus.Text = "Error: " + ex.Message;
            }
        }

        private void trackBarPWM11_Scroll(object sender, EventArgs e)
        {
            try
            {
                if (serialPortArduino == null || !serialPortArduino.IsOpen)
                {
                    MessageBox.Show("Geen open seriële verbinding.");
                    return;
                }

                string command = "set pwm11 " + trackBarPWM11.Value.ToString();
                serialPortArduino.WriteLine(command);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Fout bij verzenden: " + ex.Message);
                labelStatus.Text = "Error: " + ex.Message;
            }
        }

        private void tabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                timerOefening3.Enabled = (tabControl.SelectedTab == tabPageOefening3);
                timerOefening4.Enabled = (tabControl.SelectedTab == tabPageOefening4);
                timerOefening5.Enabled = (tabControl.SelectedTab == tabPageOefening5);
            }
            catch (Exception ex)
            {
                labelStatus.Text = "Error: " + ex.Message;
            }
        }

        private void timerOefening3_Tick(object sender, EventArgs e)
        {
            try
            {
                if (serialPortArduino == null || !serialPortArduino.IsOpen) return;

                // remove any previous data from Arduino
                try { serialPortArduino.ReadExisting(); } catch { }

                string response = string.Empty;
                string value = string.Empty;

                // digital 5
                serialPortArduino.WriteLine("get d5");
                try { response = serialPortArduino.ReadLine(); } catch (TimeoutException) { response = string.Empty; }
                if (!string.IsNullOrEmpty(response) && response.Contains(":"))
                    value = response.Split(':')[1].Trim();
                else
                    value = response.Trim();
                radioButtonDigital5.Checked = (value == "1");

                // digital 6
                try { serialPortArduino.ReadExisting(); } catch { }
                serialPortArduino.WriteLine("get d6");
                try { response = serialPortArduino.ReadLine(); } catch (TimeoutException) { response = string.Empty; }
                if (!string.IsNullOrEmpty(response) && response.Contains(":"))
                    value = response.Split(':')[1].Trim();
                else
                    value = response.Trim();
                radioButtonDigital6.Checked = (value == "1");

                // digital 7
                try { serialPortArduino.ReadExisting(); } catch { }
                serialPortArduino.WriteLine("get d7");
                try { response = serialPortArduino.ReadLine(); } catch (TimeoutException) { response = string.Empty; }
                if (!string.IsNullOrEmpty(response) && response.Contains(":"))
                    value = response.Split(':')[1].Trim();
                else
                    value = response.Trim();
                radioButtonDigital7.Checked = (value == "1");
            }
            catch (Exception ex)
            {
                labelStatus.Text = "Error: " + ex.Message;
            }
        }

        private void timerOefening4_Tick(object sender, EventArgs e)
        {
            try
            {
                if (serialPortArduino == null || !serialPortArduino.IsOpen) return;

                // remove any previous data from Arduino
                try { serialPortArduino.ReadExisting(); } catch { }

                string response = string.Empty;
                string value = string.Empty;

                // analog 0
                serialPortArduino.WriteLine("get a0");
                try { response = serialPortArduino.ReadLine(); } catch (TimeoutException) { response = string.Empty; }
                if (!string.IsNullOrEmpty(response) && response.Contains(":"))
                    value = response.Split(':')[1].Trim();
                else
                    value = response.Trim();

                labelAnalog0.Text = value;
            }
            catch (Exception ex)
            {
                labelStatus.Text = "Error: " + ex.Message;
            }
        }

        private void timerOefening5_Tick(object sender, EventArgs e)
        {
            try
            {
                // Controleer seriële verbinding
                if (serialPortArduino == null || !serialPortArduino.IsOpen)
                    return;

                // Buffer leegmaken
                try { serialPortArduino.ReadExisting(); } catch { }

                string response;
                string value;

                // -----------------------------
                // ANALOG 0 – GEWENSTE TEMPERATUUR
                // -----------------------------
                serialPortArduino.WriteLine("get a0");

                try { response = serialPortArduino.ReadLine(); }
                catch (TimeoutException) { response = string.Empty; }

                if (!string.IsNullOrEmpty(response) && response.Contains(":"))
                    value = response.Split(':')[1].Trim();
                else
                    value = response.Trim();

                int rawA0 = 0;
                int.TryParse(value, out rawA0);

                // -----------------------------
                // ANALOG 1 – HUIDIGE TEMPERATUUR
                // -----------------------------
                try { serialPortArduino.ReadExisting(); } catch { }

                serialPortArduino.WriteLine("get a1");

                try { response = serialPortArduino.ReadLine(); }
                catch (TimeoutException) { response = string.Empty; }

                if (!string.IsNullOrEmpty(response) && response.Contains(":"))
                    value = response.Split(':')[1].Trim();
                else
                    value = response.Trim();

                int rawA1 = 0;
                int.TryParse(value, out rawA1);

                // -----------------------------
                // HERSCHALEN
                // -----------------------------
                double gewenste = Math.Round((40.0 / 1023.0) * rawA0 + 5.0, 1);
                double huidig = Math.Round((500.0 / 1023.0) * rawA1, 1);

                // -----------------------------
                // UI UPDATEN
                // -----------------------------
                labelGewensteTemp.Text = $"{gewenste:0.0} °C";
                labelHuidigeTemp.Text = $"{huidig:0.0} °C";

                // -----------------------------
                // LED AANSTUREN (DIGITALE PIN 2)
                // -----------------------------
                string cmd = (huidig < gewenste) ? "set d2 on" : "set d2 off";
                serialPortArduino.WriteLine(cmd);
            }
            catch (Exception ex)
            {
                labelStatus.Text = "Error: " + ex.Message;
            }
        }

        private void label9_Click(object sender, EventArgs e)
        {

        }
    }
}
