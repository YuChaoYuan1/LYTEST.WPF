using System;
using System.Collections.Generic;
using System.IO.Ports;

namespace LYTest.MeterProtocol.SocketModule.Sock
{
    internal class COM32 : IConnection
    {
        //写个方法，叫做开启制定端口的监听
        //
        /// <summary>
        /// 是否开启数据监听
        /// </summary>
        private bool IsOpneDataReceived = false;
        static readonly object oadLock = new object();
        private readonly List<object> OADList = new List<object>();
        /// <summary>
        /// 开启或关闭串口监听
        /// </summary>
        /// <param name="OpenAndClose">ture,开启，false,关闭</param>
        public void SetDataReceived(bool OpenAndClose)
        {
            try
            {
                this.Open();//没开启端口就开启以下
                spCom.DataReceived += SpCom_DataReceived;
                IsOpneDataReceived = OpenAndClose;
                OADList.Clear();
            }
            catch (Exception ex)
            {
                Console.WriteLine("==============【开启端口监听失败】" + ex.ToString());
            }
        }
        /// <summary>
        /// 获取读取的列表数据
        /// </summary>
        /// <returns></returns>
        public List<object> GetOADList()
        {
            lock (oadLock)
            {
                return OADList;
            }
        }

        /// <summary>
        /// 监听的数据
        /// </summary>
        private void SpCom_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            int n = spCom.BytesToRead;
            if (n <= 0) return;
            byte[] vData = new byte[n];
            spCom.Read(vData, 0, n);
            Console.WriteLine(BitConverter.ToString(vData));
            //68-4B-00-C3-00-00-00-00-00-09-06-10-00-01-03-39-68-37-00-83-05-43-73-74-00-00-00-00-BB-27-10-00-12-88-01-01-01-33-20-02-00-01-01-01-51-30-16-02-00-00-00-02-0C-2B-9A-2A-12-66-83-7B-55-00-00-00-90-04-E4-1E-BD-DD-D0-B9-16-87-16
            int iStart = -2;
            int iend = -2;
            for (int i = 0; i < vData.Length; i++)
            {
                if (vData[i] == 0x68)
                {
                    if (iStart == -1)
                    {
                        iStart = i;
                        break;
                    }
                    if (iStart == -2) iStart++;//找第二个68
                }
            }
            for (int i = vData.Length - 1; i >= 0; i--)
            {
                if (vData[i] == 0x16)
                {
                    if (iend == -1)
                    {
                        iend = i;
                        break;
                    }
                    if (iend == -2) iend++;//
                }
            }

            if (iStart < 0 || iend < 0) return;

            byte[] data = new byte[iend - iStart + 1];
            Array.Copy(vData, iStart, data, 0, iend - iStart + 1);
            Console.WriteLine(BitConverter.ToString(data));
            var datalink = new Protocols.DLT698.DataLinkLayer();
            List<object> data3 = new List<object>();
            List<object> report = new List<object>();
            int errCode = 0;
            var r = datalink.ParseFrame(data, ref errCode, ref data3, ref report);
            if (r)
            {
                lock (oadLock)
                {
                    OADList.AddRange(data3);
                }
            }
        }




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
        /// 端口号
        /// </summary>
        private string PortNum;

        private readonly object ThreadObject = new object();


        private readonly SerialPort spCom;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="Settings">通信参数1200,e,8,1</param>
        /// <param name="ComNum">端口号(1,2,3,4)</param>
        public COM32(string Settings, int ComNum)
        {
            UpdatePortInfo(Settings);
            PortNum = "COM" + ComNum;
            spCom = new SerialPort();
            MaxWaitSeconds = 1000;
            WaitSecondsPerByte = 100;

        }
        #region IConnection 成员

        /// <summary>
        /// 打开串口
        /// </summary>
        /// <returns></returns>
        public bool Open()
        {
            lock (ThreadObject)
            {
                try
                {
                    if (spCom.IsOpen)
                    {
                        return true;
                        //spCom.Close();
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
                catch
                {
                    spCom.Close();
                    return false;
                }
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
        /// <param name="szSetting"></param>
        /// <returns></returns>
        public bool UpdateBltSetting(string szSetting)
        {
            UpdatePortInfo(szSetting);
            return true;
        }

        /// <summary>
        /// 连接名称
        /// </summary>
        public string ConnectName
        {
            get
            {
                return PortNum;
            }
            set
            {
                PortNum = value;
            }
        }

        public int MaxWaitSeconds
        {
            get;
            set;
        }

        public int WaitSecondsPerByte
        {
            get;
            set;
        }

        /// <summary>
        /// 发送数据\接收数据
        /// </summary>
        /// <param name="vData">发送数据</param>
        /// <param name="IsReturn">是否需要等待返回</param>
        /// <param name="WaiteTime"></param>
        /// <returns></returns>
        public bool SendData(ref byte[] vData, bool IsReturn )
        {
            lock (ThreadObject)
            {
                if (!this.Open())
                {
                    this.Close();
                    return false;
                }

                spCom.DiscardOutBuffer();
                spCom.DiscardInBuffer();

                //spCom.WriteTimeout = MaxWaitSeconds;
                try
                {
                    spCom.Write(vData, 0, vData.Length);
                }
                catch (TimeoutException)
                {
                    Console.WriteLine($"端口【{PortNum}】发送数据超时");
                    return false;
                }
                if (!IsReturn) return true;     //如果不需要返回

                if (IsOpneDataReceived)//开启了监听，就不需要等待回复数据了
                {
                    int time = 200;
                    while (time-- > 0)       //100毫秒超时器，目的是检查最后一个字符后面是否还存在待接受的数据
                    {
                        System.Threading.Thread.Sleep(1000);
                    }
                    return true;
                }

                //bool IsOut = false;

                System.Threading.Thread.Sleep(200);
                List<byte> TotalBytes = new List<byte>();
                bool byteStart = false;
                DateTime begin = DateTime.Now;
                while (TimeSub(DateTime.Now, begin) < MaxWaitSeconds)
                {
                    System.Threading.Thread.Sleep(1);
                    if (spCom.BytesToRead > 0)
                    {
                        byteStart = true;
                        byte[] bufs = new byte[spCom.BytesToRead];
                        spCom.Read(bufs, 0, bufs.Length);
                        TotalBytes.AddRange(bufs);
                    }
                    if (byteStart)
                    {
                        bool byteEnd = true;
                        DateTime bytebegin = DateTime.Now;
                        while (TimeSub(DateTime.Now, bytebegin) < WaitSecondsPerByte)
                        {
                            System.Threading.Thread.Sleep(1);
                            if (spCom.BytesToRead > 0)
                            {
                                byteEnd = false;
                                break;
                            }
                        }
                        if (byteEnd)
                        {
                            if (TotalBytes.Count < 13) continue;
                            break;
                        }
                    }
                }
                if (spCom.BytesToRead > 0)
                {
                    byte[] bufs = new byte[spCom.BytesToRead];
                    spCom.Read(bufs, 0, bufs.Length);
                    TotalBytes.AddRange(bufs);
                }
                vData = TotalBytes.ToArray();
                if (vData.Length == 0)
                {
                    Console.WriteLine($"端口【{PortNum}】接收超时");
                    return true;
                }

                return true;
            }
        }
        #endregion


        private double TimeSub(DateTime Time1, DateTime Time2)
        {
            TimeSpan tsSub = Time1.Subtract(Time2);
            return tsSub.TotalMilliseconds;
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

                //add yjt 20220621 新增切换波特率读地址的时候方便 
                if (spCom != null)
                {
                    if (spCom.IsOpen)
                    {
                        spCom.BaudRate = int.Parse(BaudRate);
                        spCom.StopBits = (StopBits)int.Parse(StopBits);
                        spCom.DataBits = int.Parse(DataBits);
                        spCom.Parity = CheckBits.ToLower() == "n" ? Parity.None : CheckBits.ToLower() == "e" ? Parity.Even : Parity.Mark;
                    }
                }

            }
        }
    }
}
