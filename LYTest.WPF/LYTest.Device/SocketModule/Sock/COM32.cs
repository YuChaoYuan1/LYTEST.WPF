using LY.SocketModule.Packet;
using LYTest.Device.SocketModule;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Threading;

namespace LY.SocketModule.Sock
{
    internal class COM32 : IConnection
    {
        /// <summary>
        /// 波特率
        /// </summary>
        private string BaudRate;
        /// <summary>
        /// 数据位
        /// </summary>
        private string DataBits;
        /// <summary>
        /// 停止位
        /// </summary>
        private string StopBits;
        /// <summary>
        /// 校验位
        /// </summary>
        private string CheckBits;
        /// <summary>
        /// 端口号: COM1
        /// </summary>
        private readonly string PortNum;



        private readonly SerialPort spCom;

        private readonly List<byte> recvFrame = new List<byte>();

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="settings">通信参数1200,e,8,1</param>
        /// <param name="ComNum">端口号(1,2,3,4)</param>
        public COM32(string settings, int ComNum)
        {
            UpdatePortInfo(settings);
            PortNum = $"COM{ComNum}";
            spCom = new SerialPort();
            spCom.DataReceived += OnDataReceived;
            Timeout = 1200;

        }

        private void OnDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort sp = (SerialPort)sender;
            if (sp.BytesToRead > 0)      //如果缓冲区待接收数据量大于0
            {
                byte[] buf = new byte[sp.BytesToRead];
                sp.Read(buf, 0, buf.Length);
                recvFrame.AddRange(buf);
            }

        }

        /// <summary>
        /// 打开串口
        /// </summary>
        /// <returns></returns>
        public bool Open()
        {

            try
            {
                if (spCom.IsOpen)
                {
                    return true;
                }

                spCom.BaudRate = int.Parse(BaudRate);
                spCom.StopBits = (StopBits)int.Parse(StopBits);
                spCom.DataBits = int.Parse(DataBits);
                spCom.Parity = CheckBits.ToLower() == "n" ? Parity.None : CheckBits.ToLower() == "e" ? Parity.Even : Parity.Mark;
                spCom.PortName = PortNum;
                spCom.DtrEnable = true;
                spCom.Open();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("功率源打开端口错误:", ex.ToString());
                spCom.Close();
                return false;
            }
        }

        /// <summary>
        /// 关闭端口
        /// </summary>
        /// <returns></returns>
        public bool Close()
        {
            try
            {
                if (spCom.IsOpen) spCom.Close();
            }
            catch { }
            return spCom.IsOpen == false;
        }
        /// <summary>
        /// 更新串口波特率
        /// </summary>
        /// <param name="setting"></param>
        /// <returns></returns>
        public bool UpdateBaudrate(string setting)
        {
            UpdatePortInfo(setting);
            return true;
        }

        /// <summary>
        /// 连接名称
        /// </summary>
        public string PortName
        {
            get { return PortNum; }
            //set { PortNum = value; }
        }

        /// <summary>
        /// 超时时间
        /// </summary>
        public int Timeout { get; set; }



        /// <summary>
        /// 发送数据\接收数据
        /// </summary>
        /// <param name="vData">发送数据</param>
        /// <param name="IsReturn">是否需要等待返回</param>
        /// <param name="WaiteTime"></param>
        /// <returns></returns>
        public bool SendData(SendPacket sPacke, RecvPacket rPacket)
        {
            recvFrame.Clear();

            if (!this.Open())
            {
                this.Close();
                return false;
            }

            spCom.DiscardOutBuffer();
            spCom.DiscardInBuffer();
            byte[] sendFrame = sPacke.GetPacketData();
            LogHelper.Write(true, spCom.PortName, sendFrame);
            spCom.Write(sendFrame, 0, sendFrame.Length);

            if (!sPacke.IsNeedReturn)
            {
                spCom.Close();
                LogHelper.Write(false, spCom.PortName, recvFrame, "无需回复");
                return true;
            }
            Thread.Sleep(100);
            bool ret = false;
            DateTime endT = DateTime.Now.AddSeconds(Timeout);
            while (DateTime.Now.Subtract(endT).Seconds < 0)          //1秒超时器，如果超过表示收不到任何数据，直接退出
            {
                if (rPacket.ParsePacket(recvFrame))
                {
                    ret = true;
                    break;
                }
                Thread.Sleep(100);
            }
            LogHelper.Write(false, spCom.PortName, recvFrame);

            spCom.Close();

            return ret;
        }


        /// <summary>
        /// 更新端口信息
        /// </summary>
        /// <param name="Settings"></param>
        private void UpdatePortInfo(string Settings)
        {
            string[] Tmp = Settings.Split(',');

            if (Tmp.Length != 4)
            {

                BaudRate = "1200";
                CheckBits = "n";
                DataBits = "8";
                StopBits = "1";
            }
            else
            {
                BaudRate = Tmp[0];
                CheckBits = Tmp[1];
                DataBits = Tmp[2];
                StopBits = Tmp[3];
            }
        }
    }
}
