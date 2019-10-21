using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EthernetPLC
{
    public partial class Form1 : Form
    {
        SocketCommunication SockCom;
        enum Signal
        {
            PressUp = 4000,
            CheckStart,
            BarcodeStart,
            BarcodeNG = 4010,
            BarcodeOK,
            CheckEND = 4020,
            CheckOK,
            CheckNG,
            CheckRetest
        }
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                if(SockCom == null)
                {
                    SockCom = new SocketCommunication(IpAddress.Text, Convert.ToInt32(Port.Text));
                    //foreach (var item in Enum.GetValues(typeof(Signal)))
                    //    SelectSignal.Items.Add(item);
                    SignalSelect.Items.AddRange(Enum.GetNames(typeof(Signal)));
                }
            }
            catch
            {
                MessageBox.Show("올바른 정보를 입력해 주세요");
                SockCom = null;
            }
        }

        private void SendBtn_Click(object sender, EventArgs e)
        {
            if (!SockCom.GetSoketState())
                return;

            Signal Command = (Signal)Enum.Parse(typeof(Signal), SignalSelect.SelectedText);
            string recvData = SockCom.SendAndReceive("MX" + (int)Command + "=01");
            textBox1.Text += "Send: " + "MX" + (int)Command + "=01" + "\r\n";
            textBox1.Text += "Receive: " + recvData + "\r\n\r\n";

            recvData = SockCom.SendAndReceive("MX" + (int)Command + "?");
            textBox1.Text += "Send: " + "MX" + (int)Command + "?" + "\r\n";
            textBox1.Text += "Receive: " + recvData + "\r\n\r\n";
        }
    }
}
