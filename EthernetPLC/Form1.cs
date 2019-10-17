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
                SockCom = new SocketCommunication(IpAddress.Text, Convert.ToInt32(Port.Text));
                //foreach (var item in Enum.GetValues(typeof(Signal)))
                //    SelectSignal.Items.Add(item);
                SelectSignal.Items.AddRange(Enum.GetNames(typeof(Form1.Signal)));
            }
            catch
            {
                MessageBox.Show("올바른 정보를 입력해 주세요");
            }
        }
    }
}
