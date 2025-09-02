using LYTest.Utility.Log;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;


namespace LYTest.Mis.Houda
{


    /// <summary>
    /// 登陆服务器的委托
    /// </summary>
    /// <param name="logontime"></param>
    /// <returns></returns>
    public delegate bool LogonDelegate(int logontime);

    /// <summary>
    /// 通信数据处理委托
    /// </summary>
    /// <param name="data"></param>
    public delegate void MessageDelegate(string data);
    /// <summary>
    /// 厚达接口通讯
    /// </summary>
    public class HoudaInteraction//:Form
    {
        /*
        1.建立连接到服务端程序
        2.连接断开后自动进行重新连接
        3.建立发送数据定时处理
        4.建立接受数据处理线程
         * 
         * 
         * */
        /// <summary>
        /// 连接服务器的客户端
        /// </summary>
        public SocketPortParameter NetworkClientPatam = new SocketPortParameter();

        //登陆定时器
        private readonly System.Timers.Timer LogonTimer = new System.Timers.Timer();

        private System.Timers.Timer sendNetworkMessageTimer = new System.Timers.Timer();

        private readonly System.Timers.Timer responsionSendNetworkMessageTimer = new System.Timers.Timer();

        //网络委托读取数据
        private readonly MessageDelegate ReceivNetworkMessageDelegate = null;

        //重复发送等待时间
        private int responsionSendNetworkTime = 0;

        /// <summary>
        /// 向服务器发送信息的事件
        /// </summary>
        public event EventHandler<XmlMsg> RevcMessageEvent;
        /// <summary>
        /// 更新状态栏中网络状态
        /// </summary>
        public event EventHandler<EventArgs> UpdataStatusLabel;

        //登录线程
        private Thread LogonThread = null;
        //登陆锁
        private bool LogonLock = true;
        //监视窗口
        //private StakeoutShowForm networkStakeoutForm = null;

        private readonly string ServerIpAddress = string.Empty;

        private readonly int SeverPort = 1002;

        private readonly string SystemCode = string.Empty;
        ///// <summary>
        ///// 台体状态
        ///// </summary>
        //private string DeviceStart = string.Empty;


        /// <summary>
        /// 发送序号
        /// </summary>
        private static int Seq = 1;

        private NetworkMessage CurrentNetMes = null;

        //Form MainForm = null;


        /// <summary>
        /// 构造 
        /// </summary>
        /// <param name="mainForm">主窗口</param>
        /// <param name="serverIpAddress">服务端ip 地址</param>
        /// <param name="port">端口号</param>
        public HoudaInteraction(string serverIpAddress, int port, string strTaitiCode)
        {
            //MainForm = mainForm;
            ServerIpAddress = serverIpAddress;
            SeverPort = port;

            SystemCode = strTaitiCode;

            //接收 数据委托
            ReceivNetworkMessageDelegate = new MessageDelegate(ReceiveNetworkMessage);

            //登陆模块
            if (NetworkClientPatam.LogonStateDelegate == null)
                NetworkClientPatam.LogonStateDelegate = new MessageDelegateBool(LogonStatus);

            InitNetworkServerPortManage();
        }
        // public HoudaInteraction(Form mainForm, string serverIpAddress, int port, string strTaitiCode)


        private const int BufferSize = 10000;
        public byte[] buffer = new byte[BufferSize];
        public CommEventArgs _commEventMsg;        //帧传输参数




        /// <summary>
        /// 当异步请求连接成功是回调函数
        /// </summary>
        /// <param name="ar"></param>
        private void Connected(IAsyncResult ar)
        {
            _commEventMsg = new CommEventArgs();
            Socket socket = ar.AsyncState as Socket;
            if (socket.Connected)
            {
                _commEventMsg.RemotSocket = socket;
                _commEventMsg.State = "OK";
                NetworkClientPatam.TcpClient.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, ReceiveFrameCallBack, NetworkClientPatam.TcpClient);

                UpdataStatusLabel("connectSuccess", null);
                #region 登陆指令

                XmlMsg xm = new XmlMsg
                {
                    strSysCode = SystemCode
                };
                xm.headMsg.FromStart = SystemCode;
                xm.headMsg.ToRecive = "Main";
                xm.headMsg.Seq = "1";
                xm.headMsg.CmdType = "1";
                xm.headMsg.CurrentTime = string.Empty;
                xm.headMsg.Command = "9998";


                NetworkMessage NetMes = new NetworkMessage
                {
                    MessageAttribute = NetworkMessageAttribute.登陆9998,
                    xmlDoc = xm.ComposeXml(true)
                };


                SendData(NetMes.xmlDoc);

                #endregion
            }

        }



        /// <summary>
        /// 异步发送帧，等待等待回调函数
        /// </summary>
        /// <param name="frameBytes"></param>
        public void SendData(string frameString)
        {
            _commEventMsg = new CommEventArgs();
            ///服务器端可能关闭报错
            if (NetworkClientPatam.TcpClient == null || !NetworkClientPatam.TcpClient.Connected)
            {
                ConnectServer();
                //_commEventMsg.State = "CLOSED";
                _commEventMsg.State = "END";


            }
            else
            {
                NetworkClientPatam.TcpClient.Send(Encoding.ASCII.GetBytes(frameString));
            }

        }


        //<summary>
        //接收到数据回调函数
        //</summary>
        //<param name="ar"></param>
        private void ReceiveFrameCallBack(IAsyncResult ar)
        {
            Socket scoketStemp = ar.AsyncState as Socket;
            ///接收异步读取，并返回接收自己长度
            int lenTtemp = scoketStemp.EndReceive(ar);
            ///表示接收到了最后了，所以接收完成，接收到

            if (lenTtemp > 0)
            {
                ///如果接收到的字符里面有结束字符则表示结束，接收完成，出发回调事件
                ///接收次次发回来的字符串--------------------要总拼接
                ///            ///
                string sRecieved = "";
                try
                {
                    sRecieved = Encoding.ASCII.GetString(buffer, 0, lenTtemp);
                }
                catch (Exception ex)
                {
                    ErrorLog(ex + "--------" + DateTime.Now.ToString());  //打印信息
                }


                NetworkClientPatam.DataValue = sRecieved;//streamreader.ReadLine();
                NetworkClientPatam.BufferLength = sRecieved.Length;

                if (NetworkClientPatam.BufferLength > 0)
                {
                    //if (MainForm != null)
                    //    MainForm.Invoke(ReceivNetworkMessageDelegate, new object[] { NetworkClientPatam.DataValue });
                    ReceivNetworkMessageDelegate(NetworkClientPatam.DataValue);
                }

                try
                {
                    NetworkClientPatam.TcpClient.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, ReceiveFrameCallBack, NetworkClientPatam.TcpClient);
                }
                catch (Exception ex)
                {

                    ErrorLog(ex + "--------" + DateTime.Now.ToString());  //打印信息
                }
            }
            else
            {
                ///禁用接收和发送
                scoketStemp.Shutdown(SocketShutdown.Both);
                scoketStemp.Close();

                //MainForm.Invoke(NetworkClientPatam.LogonStateDelegate, new object[] { false });
                NetworkClientPatam.LogonStateDelegate(false);
            }

        }


        /// <summary>
        /// 关闭串口通信工具
        /// </summary>
        public void Close()
        {
            if (NetworkClientPatam.TcpClient != null && NetworkClientPatam.TcpClient.Connected)
            {
                NetworkClientPatam.TcpClient.Shutdown(SocketShutdown.Both);
                Thread.Sleep(50);
                NetworkClientPatam.TcpClient.Close();
            }
            else
            {
                NetworkClientPatam.TcpClient.Close();
            }
        }

        /// <summary>
        /// 初始化程序窗口
        /// </summary>
        public void InitNetworkServerPortManage()
        {

            Thread.Sleep(1000);

            //网路发消息定时器
            //sendNetworkMessageTimer.Interval = 500;
            //sendNetworkMessageTimer.Tick += new EventHandler(SendNetworkMessageTimer_Tick);
            //sendNetworkMessageTimer.Start();
            sendNetworkMessageTimer.Interval = 500;
            sendNetworkMessageTimer.Elapsed += new System.Timers.ElapsedEventHandler(SendNetworkMessageTimer_Tick);
            sendNetworkMessageTimer.Start();

            //网络命令重复发送定时器
            responsionSendNetworkMessageTimer.Interval = 1000;
            responsionSendNetworkMessageTimer.Elapsed += new System.Timers.ElapsedEventHandler(ResponsionSendNetworkMessageTimer_Tick);
            responsionSendNetworkMessageTimer.Start();

            //连接服务器
            LogonTimer.Interval = 100;
            LogonTimer.Elapsed += new System.Timers.ElapsedEventHandler(LogonTimer_Tick);
            LogonTimer.Start();

            //更新状态栏
            //EventArgs ea = new EventArgs();
            //UpdataStatusLabel("connect", ea);
            //网路心跳定时器
        }

        //void SendNetWorkTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        //{
        //    throw new NotImplementedException();
        //}

        // 登陆定时器实现
        void LogonTimer_Tick(object sender, EventArgs e)
        {

            LogonTimer.Stop();
            //登陆锁。现在正在登录
            if (!LogonLock)
                return;

            if (!NetworkClientPatam.TcpClient.Connected)
            {
                //关闭心跳定时器
                //heartbeatTimer.Stop();
                //更新工具栏
                UpdataStatusLabel("connect", e);
                if (LogonThread != null)
                {
                    LogonThread.Abort();
                    LogonThread = null;
                }
                LogonThread = new Thread(new ThreadStart(ConnectServer))
                {
                    IsBackground = true
                };
                LogonThread.Start();
            }
            else
            {
                sendNetworkMessageTimer.Start();
                responsionSendNetworkMessageTimer.Start();
                UpdataStatusLabel("connectSuccess", null);
            }

        }
        //登陆
        private void LogonStatus(bool logon)
        {
            if (logon)
            {//连接成功
                EventArgs ea = new EventArgs();
                UpdataStatusLabel("connectSuccess", ea);
            }
            else
            {//登录失败

                //停止登陆服务器的数据发送
                if (sendNetworkMessageTimer != null)
                    sendNetworkMessageTimer.Stop();
                if (responsionSendNetworkMessageTimer != null)
                    responsionSendNetworkMessageTimer.Stop();
                EventArgs ea = new EventArgs();
                UpdataStatusLabel("disconNexion", ea);
                LogonTimer.Interval = 3000;
                LogonTimer.Start();
            }

        }

        // 连接服务器
        public void ConnectServer()
        {
            LogonLock = false;

            //关闭连接
            if (NetworkClientPatam.TcpClient != null)
            {
                if (NetworkClientPatam.TcpClient.Connected)
                    NetworkClientPatam.TcpClient.Close();
            }
            IPAddress ServerIp = IPAddress.Parse(ServerIpAddress);

            NetworkClientPatam.TcpClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint Ep = new IPEndPoint(ServerIp, SeverPort);
            NetworkClientPatam.TcpClient.SendBufferSize = 50 * 1024;
            NetworkClientPatam.TcpClient.ReceiveBufferSize = 10 * 1024;
            try
            {
                //建立连接到 服务器  端
                NetworkClientPatam.TcpClient.Connect(Ep);

                NetworkClientPatam.TcpClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
                {
                    Blocking = false
                };

                UpdataStatusLabel("connect", null);

                IAsyncResult result = NetworkClientPatam.TcpClient.BeginConnect(Ep, new AsyncCallback(Connected), NetworkClientPatam.TcpClient);   //对远程对象的异步请求
            }
            catch (Exception ex)
            {
                ErrorLog(string.Format("{0}{1}", "NetworkServerProtManage - ConnectServer", ex.Message));
                NetworkClientPatam.LogonStateDelegate?.Invoke(false);
                LogonLock = true;
                return;
            }
            Thread.Sleep(1500);

            //如果连接没有建立成功 接着进行连接
            if (!NetworkClientPatam.TcpClient.Connected)
            {
                NetworkClientPatam.LogonStateDelegate?.Invoke(false);
                LogonLock = true;
                return;
            }
            else
            {

                //开启发送数据定时器
                sendNetworkMessageTimer = new System.Timers.Timer
                {
                    Interval = 500
                };
                sendNetworkMessageTimer.Elapsed += new System.Timers.ElapsedEventHandler(SendNetworkMessageTimer_Tick);
                sendNetworkMessageTimer.Start();
                //网络命令重复发送定时器
                responsionSendNetworkMessageTimer.Start();

                //发送登陆命令

                XmlMsg xmlmsg = new XmlMsg
                {
                    strSysCode = SystemCode
                };
                xmlmsg.headMsg.FromStart = SystemCode;
                xmlmsg.headMsg.ToRecive = "Main";
                xmlmsg.headMsg.Seq = "1";
                xmlmsg.headMsg.CmdType = "1";
                xmlmsg.headMsg.CurrentTime = string.Empty;
                xmlmsg.headMsg.Command = "9998";

                NetworkMessage NetMes = new NetworkMessage
                {
                    MessageAttribute = NetworkMessageAttribute.登陆9998,
                    xmlDoc = xmlmsg.ComposeXml(true)
                };


                SendData(NetMes.xmlDoc);

                NetworkClientPatam.LogonStateDelegate?.Invoke(false);
            }
            LogonLock = true;

            return;

        }

        //// 读取网络缓冲区的数据
        //private void NetworkReceive()
        //{
        //    bool bOk = true;
        //    while (bOk)
        //    {
        //        // 创建并实例化用于存放数据的字节数组
        //        byte[] getData = new byte[1024];

        //        NetworkClientPatam.TcpClient.Receive(getData, SocketFlags.None);
        //        // 将字节数组转换为文本形式
        //        string getMsg = Encoding.Default.GetString(getData).Replace("\0", "");

        //        NetworkClientPatam.DataValue = getMsg;//streamreader.ReadLine();
        //        NetworkClientPatam.BufferLength = NetworkClientPatam.DataValue.Length;

        //        if (NetworkClientPatam.BufferLength > 0)
        //        {
        //            //if (MainForm != null)
        //            //    MainForm.Invoke(ReceivNetworkMessageDelegate, new object[] { NetworkClientPatam.DataValue });

        //            ReceivNetworkMessageDelegate(NetworkClientPatam.DataValue);


        //        }

        //    }
        //}

        // 定时发送数据到服务器
        void SendNetworkMessageTimer_Tick(object sender, EventArgs e)
        {
            if (NetworkClientPatam == null)
                return;
            if (NetworkClientPatam.NetworkMessageLinkedListResponsion.Count > 0)
                return;
            if (NetworkClientPatam.NetworkMessageLinkedList.Count > 0)
            {
                CurrentNetMes = NetworkClientPatam.NetworkMessageLinkedList.First.Value;


                if (NetworkClientPatam.TcpClient.Connected)
                {

                    NetworkClientPatam.TcpClient.SendBufferSize = 50 * 1024;
                    SendData(CurrentNetMes.xmlDoc);
                    //心跳计数器,如果没有指令就发心跳过去
                    //heartbeatWaitTime = 0;
                    //命令监视窗口
                    //if (networkStakeoutForm != null)
                    //{
                    //    if (CurrentNetMes.MessageAttribute != NetworkMessageAttribute.heartbeat)
                    //    {
                    //        networkStakeoutForm.SendMessages(CurrentNetMes.xmlDoc);
                    //    }
                    //}
                    //保存校验服务器命令，便于调试  
                    if (CurrentNetMes.MessageAttribute != NetworkMessageAttribute.heartbeat)
                    {
                        //ZH.Core.Function.LogHelper.WriteLog("AutoLineLog\\SendLog：" + CurrentNetMes.xmlDoc.ToString());
                        LogManager.AddMessage("AutoLineLog\\SendLog：" + CurrentNetMes.xmlDoc.ToString(), EnumLogSource.服务器日志, EnumLevel.Information);
                    }

                }
                if (CurrentNetMes.WaitForValidate)
                {
                    NetworkClientPatam.NetworkMessageLinkedListResponsion.AddLast(CurrentNetMes);
                }
                if (NetworkClientPatam.NetworkMessageLinkedList.Count > 0)
                    NetworkClientPatam.NetworkMessageLinkedList.RemoveFirst();
            }
        }

        //网络命令重复发送定时器
        void ResponsionSendNetworkMessageTimer_Tick(object sender, EventArgs e)
        {
            if (NetworkClientPatam == null)
                return;
            if (NetworkClientPatam.NetworkMessageLinkedListResponsion.Count > 0)
            {
                NetworkMessage NetMes = NetworkClientPatam.NetworkMessageLinkedListResponsion.First.Value;
                if (NetMes.WaitForValidate)
                {
                    // 重新发送延时
                    responsionSendNetworkTime++;
                    if (responsionSendNetworkTime > 50)
                    {
                        NetworkClientPatam.TcpClient.SendBufferSize = 50 * 1024;
                        SendData(NetMes.xmlDoc);

                        //心跳计数器,如果没有指令就发心跳过去
                        //heartbeatWaitTime = 0;

                        ////命令监视窗口
                        //if (networkStakeoutForm != null)
                        //{
                        //    networkStakeoutForm.SendMessages(NetMes.xmlDoc);
                        //}


                        responsionSendNetworkTime = 0;
                        NetworkClientPatam.RepairNumber++;
                        if (NetworkClientPatam.RepairNumber > 3)
                        {
                            NetworkClientPatam.NetworkMessageLinkedListResponsion.RemoveFirst();
                            NetworkClientPatam.RepairNumber = 0;
                        }
                    }

                }
                else
                {
                    //收到命令清除缓冲
                    NetworkClientPatam.NetworkMessageLinkedListResponsion.RemoveFirst();
                    NetworkClientPatam.RepairNumber = 0;
                    responsionSendNetworkTime = 0;
                }
            }
        }

        /// <summary>
        /// 处理发送到服务器的消息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void SendNetworkMessage(XmlMsg e)
        {
            NetworkMessage netMes = new NetworkMessage();
            //e.headMsg.Seq = Seq.ToString();
            e.headMsg.FromStart = SystemCode;
            if (!e.headMsg.Command.Equals("9999"))
            {
                e.headMsg.Seq = Seq++.ToString();
                if (Seq == 999)
                {
                    Seq = 1;
                }
            }
            netMes.xmlDoc = e.ComposeXml(true);
            //协议类型
            netMes.MessageAttribute = e.MessageAttribute;
            netMes.WaitForValidate = false;

            NetworkClientPatam.NetworkMessageLinkedList.AddLast(netMes);
        }


        // 处理收到的服务器的命令
        private void ReceiveNetworkMessage(string dataValue)
        {
            NetworkMessage netMes = new NetworkMessage
            {
                xmlDoc = dataValue
            };
            XmlMsg xmlData = new XmlMsg();
            //解析 相应的数据
            xmlData.AnalysisXml(dataValue);
            //获取数据类型
            netMes.MessageAttribute = xmlData.MessageAttribute;

            //返回接收到的数据
            RevcMessageEvent?.Invoke(this, xmlData);

            //保存校验服务器回复命令，便于调试 
            if (netMes.MessageAttribute != NetworkMessageAttribute.heartbeat)
            {
                LogManager.AddMessage("AutoLineLog\\RecvLog：" + netMes.xmlDoc.ToString(), EnumLogSource.服务器日志, EnumLevel.Information);
            }
            //命令成功返回 自动清除重发缓存区
            ClearNetworkMessageLinkedListRepair(CurrentNetMes);


        }

        //命令成功返回 自动清除重发缓存区
        private void ClearNetworkMessageLinkedListRepair(NetworkMessage NetworkMes)
        {
            if (NetworkMes == null)
                return;
            foreach (NetworkMessage NetMes in NetworkClientPatam.NetworkMessageLinkedListResponsion)
            {
                if (NetworkMes.MessageAttribute == NetMes.MessageAttribute)
                {
                    NetMes.WaitForValidate = false;
                    break;
                }
            }

        }

        /// <summary>
        /// 关闭服务器连接
        /// </summary>
        public void CloseNetworkServerProtManage()
        {
            sendNetworkMessageTimer.Stop();
            responsionSendNetworkMessageTimer.Stop();

            if (NetworkClientPatam != null)
            {
                NetworkClientPatam.TcpClient.Close();
            }
        }

        /// <summary>
        /// 系统错误日志
        /// </summary>
        /// <param name="ErrorValue"></param>
        /// <returns></returns>
        static public bool ErrorLog(string ErrorValue)
        {
            //string PathName = string.Format("{0}Config\\Cmd\\ErrorLog{1}{2}{3}.txt", AppDomain.CurrentDomain.BaseDirectory, DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
            //LYTest.Core.Function.LogHelper.WriteLog("AutoLineLog\\ErrorLog：" + ErrorValue);
            LogManager.AddMessage(ErrorValue, EnumLogSource.服务器日志, EnumLevel.Error);
            return true;
        }

    }


    /// <summary>
    /// 在理想情况下，，一个帧的所有数据都能一次接受完成
    /// </summary>
    public class CommEventArgs : EventArgs
    {
        public string FrameDataOfString
        {
            get;
            set;
        }

        public byte[] FrameDataOfByte
        {
            get;
            set;
        }

        public string State
        {
            get;
            set;
        }

        public Socket RemotSocket
        {
            get;
            set;
        }

        public string ZDAddr
        {
            get;
            set;
        }
    }
}

