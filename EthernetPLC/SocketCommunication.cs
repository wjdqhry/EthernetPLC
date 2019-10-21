using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EthernetPLC
{
    class SocketCommunication : IDisposable
    {
        private Socket Sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        private Thread ReceiveThread;
        private IPEndPoint Ipep;
        private ManualResetEvent mManualResetEvent;
        private byte mAddress = 0;

        bool running = false;
        
        public SocketCommunication(string address, int port)
        {
            try
            {
                Sock.Connect(address, port);

                //running = true;
                //mManualResetEvent.Reset();
                //ReceiveThread = new Thread(Receiving);
                //ReceiveThread.Start();
            }
            catch
            {
                MessageBox.Show("연결 실패");
                Sock.Close();
            }
        }

        private void Receiving()
        {
            while (running)
            {
                
            }
        }

        private string ReceiveData(string command)
        {
            StringBuilder tRecvBuilder = new StringBuilder();
            string tRcvdStr = "";
            do
            {
                byte[] tRecvBuffer = new byte[1024];
                int tReadLength = Sock.Receive(tRecvBuffer, tRecvBuffer.Length, SocketFlags.None);

                byte[] trueBuffer = new byte[tReadLength];
                Array.Copy(tRecvBuffer, trueBuffer, trueBuffer.Length);

                if (tReadLength == 0)
                {
                    return null;
                }

                tRcvdStr += (Encoding.ASCII.GetString(trueBuffer).TrimEnd((char)0x00));
                tRecvBuilder.Append(Encoding.ASCII.GetString(trueBuffer).TrimEnd((char)0x00));
            }
            while (!ParseRecvBuffer(command, ref tRecvBuilder));
            //// 받았으면 다시 보내기 모드가 된다.
            //NEXT_PLC_STEP = PLC_STEP.SEND_PING;
            return tRcvdStr;
        }

        public bool GetSoketState() => Sock.Connected;

        private void SendData(string command)
        {
            byte[] tSendBuffer = CreateSendPacket(mAddress, command);
            Sock.Send(tSendBuffer);
            Console.WriteLine("PLC_SEND (DATA) : " + Encoding.ASCII.GetString(tSendBuffer));
        }
        public string SendAndReceive(string command)
        {
            SendData(command);
            string msg = ReceiveData(command);
            return msg;
        }
        private byte[] CreateSendPacket(byte pTargetAddress, string pInput) =>
            new CScriptCommand(pTargetAddress).SetCommand(pInput);

        bool ParseRecvBuffer(String pCmd, ref StringBuilder pRecvList)
        {
            if (pRecvList.Length == 0)
                return false;

            if (pRecvList[0] != (byte)PacketEnum.PLCCode.ACK &&
                pRecvList[0] != (byte)PacketEnum.PLCCode.NAK)
            {
                pRecvList.Clear();
                return false;
            }

            int tPacketEndIndex = pRecvList.ToString().IndexOf((char)PacketEnum.PLCCode.ETX);
            if (tPacketEndIndex == -1)   // ETX 신호를 못 받은 경우 : 데이터가 부족한 경우
                return false;


            string tPacketStr = pRecvList.ToString(0, tPacketEndIndex + 1).TrimEnd((char)0x00);
            CPLCPacket cPLCPacket = new CPLCPacket(Encoding.ASCII.GetBytes(tPacketStr));

            // ACK 데이터 처리
            if (pRecvList[0] == (byte)PacketEnum.PLCCode.ACK)
            {
                byte[] tACKData = cPLCPacket.GetACKData();
                if (tACKData != null)
                {
                    string tACKStr = Encoding.ASCII.GetString(tACKData);
                    /* JDLib.JDLog.ErrorLog(tACKStr); */  //PLC 로그는 뺴자...
                }
            }

            // NAK 에러 메세지 처리
            else if (pRecvList[0] == (byte)PacketEnum.PLCCode.NAK)
            {
                ushort tErrorCode = cPLCPacket.GetNAKErrorCode();

                Console.WriteLine("NAK ERROR : " + tErrorCode.ToString());
            }
            //JDLib.JDLog.InfoLog("PLC_RECV (PONG) : " + tPacketStr);
            else Console.WriteLine("PLC_RECV (DATA) : " + tPacketStr);
            pRecvList.Remove(0, tPacketEndIndex + 1);

            return true;
        }
        #region IDisposable Support
        private bool disposedValue = false; // 중복 호출을 검색하려면

        

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: 관리되는 상태(관리되는 개체)를 삭제합니다.
                }

                // TODO: 관리되지 않는 리소스(관리되지 않는 개체)를 해제하고 아래의 종료자를 재정의합니다.
                // TODO: 큰 필드를 null로 설정합니다.

                disposedValue = true;
            }
        }

        // TODO: 위의 Dispose(bool disposing)에 관리되지 않는 리소스를 해제하는 코드가 포함되어 있는 경우에만 종료자를 재정의합니다.
        // ~SocketCommunication() {
        //   // 이 코드를 변경하지 마세요. 위의 Dispose(bool disposing)에 정리 코드를 입력하세요.
        //   Dispose(false);
        // }

        // 삭제 가능한 패턴을 올바르게 구현하기 위해 추가된 코드입니다.
        public void Dispose()
        {
            // 이 코드를 변경하지 마세요. 위의 Dispose(bool disposing)에 정리 코드를 입력하세요.
            Dispose(true);
            // TODO: 위의 종료자가 재정의된 경우 다음 코드 줄의 주석 처리를 제거합니다.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
