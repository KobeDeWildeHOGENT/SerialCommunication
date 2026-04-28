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

        public Form1()
        {
            InitializeComponent();
            serialPortArduino = new SerialPort();
            serialPortArduino.ReadTimeout = 1000;
            serialPortArduino.WriteTimeout = 1000;
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
                    serialPortArduino.Close();
                    radioButtonVerbonden.Checked = false;
                    buttonConnect.Text = "Connect";
                    labelStatus.Text = "Disconnected";
                }
                else
                {
                    if (comboBoxPoort.SelectedItem == null)
                    {
                        labelStatus.Text = "No port selected";
                        return;
                    }

                    serialPortArduino.PortName = comboBoxPoort.SelectedItem.ToString();

                    if (comboBoxBaudrate.SelectedItem != null && int.TryParse(comboBoxBaudrate.SelectedItem.ToString(), out int baud))
                        serialPortArduino.BaudRate = baud;
                    else
                        serialPortArduino.BaudRate = 115200;

                    serialPortArduino.DataBits = (int)numericUpDownDatabits.Value;

                    if (radioButtonParityEven.Checked) serialPortArduino.Parity = Parity.Even;
                    else if (radioButtonParityOdd.Checked) serialPortArduino.Parity = Parity.Odd;
                    else if (radioButtonParityMark.Checked) serialPortArduino.Parity = Parity.Mark;
                    else if (radioButtonParitySpace.Checked) serialPortArduino.Parity = Parity.Space;
                    else serialPortArduino.Parity = Parity.None;

                    if (radioButtonStopbitsNone.Checked) serialPortArduino.StopBits = StopBits.None;
                    else if (radioButtonStopbitsOne.Checked) serialPortArduino.StopBits = StopBits.One;
                    else if (radioButtonStopbitsOnePointFive.Checked) serialPortArduino.StopBits = StopBits.OnePointFive;
                    else if (radioButtonStopbitsTwo.Checked) serialPortArduino.StopBits = StopBits.Two;
                    else serialPortArduino.StopBits = StopBits.One;

                    if (radioButtonHandshakeNone.Checked) serialPortArduino.Handshake = Handshake.None;
                    else if (radioButtonHandshakeRTS.Checked) serialPortArduino.Handshake = Handshake.RequestToSend;
                    else if (radioButtonHandshakeRTSXonXoff.Checked) serialPortArduino.Handshake = Handshake.RequestToSendXOnXOff;
                    else if (radioButtonHandshakeXonXoff.Checked) serialPortArduino.Handshake = Handshake.XOnXOff;
                    else serialPortArduino.Handshake = Handshake.None;

                    serialPortArduino.RtsEnable = checkBoxRtsEnable.Checked;
                    serialPortArduino.DtrEnable = checkBoxDtrEnable.Checked;

                    serialPortArduino.Open();
                    radioButtonVerbonden.Checked = true;
                    buttonConnect.Text = "Disconnect";
                    labelStatus.Text = $"Connected: {serialPortArduino.PortName}";
                }
            }
            catch (Exception ex)
            {
                labelStatus.Text = $"Error: {ex.Message}";
            }
        }
    }
}
