using LYTest.DAL.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
namespace LYTest.WPF.WanTe
{
    /// <summary>
    /// 万特心跳、登录TCP客户端
    /// </summary>
    public class TestSystemTCPClient : TcpClient
    {
        ///// <summary>
        ///// 本机网络终结点
        ///// </summary>
        //protected IPEndPoint LocalEndPoint { get; set; }
        /// <summary>
        /// 远端网络终结点
        /// </summary>
        protected IPEndPoint RemoteEndPoint { get; set; }
        /// <summary>
        /// 数据流
        /// </summary>
        protected NetworkStream Stream { get; set; }
        /// <summary>
        /// 心跳间隔
        /// </summary>
        protected int BeatInterval { get; set; } = 60000;
        /// <summary>
        /// 心跳标识符
        /// </summary>
        protected bool IsBeat { get; set; } = false;
        /// <summary>
        /// 收到登录成功帧后事件
        /// </summary>
        public event Action OnLogin;

        protected bool _SuccessLogin = false;
        /// <summary>
        /// 登录成功标识符
        /// </summary>
        protected bool SuccessLogin
        {
            get
            {
                return _SuccessLogin;
            }
            set
            {
                if (value != _SuccessLogin && value)
                {
                    _SuccessLogin = value;
                    OnLogin?.Invoke();
                }
            }
        }
        /// <summary>
        /// 收到考试开始帧后事件
        /// </summary>
        public event Action OnReceiveTestStart;
        /// <summary>
        /// 构造万特考试系统客户端
        /// </summary>
        /// <param name="localEndPoint">本机网络终结点</param>
        /// <param name="remoteEndPoint">远端网络终结点</param>
        public TestSystemTCPClient(IPEndPoint remoteEndPoint = default, AddressFamily family = AddressFamily.InterNetwork) : base(family)
        {
            try
            {
                WriteLog($"[{DateTime.Now:T}]创建TCP客户端");
                //this.LocalEndPoint = localEndPoint == default ? new IPEndPoint(IPAddress.Parse(ConfigHelper.Instance.SetLocal_Ip), int.Parse(ConfigHelper.Instance.SetLocal_Port)) : localEndPoint;
                this.RemoteEndPoint = remoteEndPoint == default ? new IPEndPoint(IPAddress.Parse(ConfigHelper.Instance.SetControl_Ip), int.Parse(ConfigHelper.Instance.SetControl_Port)) : remoteEndPoint;
                if (!Initialize())
                {
                    return;
                }
                // 注册登录事件，启动新线程接收考试开始帧
                OnLogin += () => Task.Run(HandleTestStart);
            }
            catch (Exception ex)
            {
                LYTest.Utility.Log.LogManager.AddMessage("客户端初始化失败，请检测远端地址是否配置正确。\n错误信息：" + ex.Message, Utility.Log.EnumLogSource.服务器日志, Utility.Log.EnumLevel.Error);
                return;
            }
        }
        /// <summary>
        /// 初始化服务器
        /// </summary>
        /// <returns></returns>
        protected virtual bool Initialize()
        {
            try
            {
                // 关闭现有连接并释放资源
                if (this.Connected)
                {
                    this.Close();
                }

                // 创建新的 Socket 并绑定本地端点
                // 假设 LocalEndPoint 的 AddressFamily 是 InterNetwork
                this.Client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                //if (LocalEndPoint != default) this.Client.Bind(this.LocalEndPoint);// 不使用手动输入的本机IP，让套接字自动分配端口以联机

                // 连接远程端点（调用基类的 Connect 方法确保状态正确）
                base.Connect(this.RemoteEndPoint);

                // 获取新的网络流
                if (Connected)
                {
                    Stream = this.GetStream();
                    return true;
                }
                else
                {
                    LYTest.Utility.Log.LogManager.AddMessage("客户端获取数据流失败，未联机到可联机的服务器。", Utility.Log.EnumLogSource.服务器日志, Utility.Log.EnumLevel.Error);
                    return false;
                }
            }
            catch (Exception ex)
            {
                string str = $"客户端联机远端地址({RemoteEndPoint.Address}_{RemoteEndPoint.Port})失败。\n错误信息：{ex}";
                LYTest.Utility.Log.LogManager.AddMessage(str, Utility.Log.EnumLogSource.服务器日志, Utility.Log.EnumLevel.Error);
                return false;
            }
        }
        /// <summary>
        /// 发送登录帧
        /// </summary>
        /// <returns></returns>
        public virtual async Task<bool> SendLogin(bool NeedReply = true)
        {
            return await Command(CommandType.Login, NeedReply);
        }
        /// <summary>
        /// 发送登出帧
        /// </summary>
        /// <returns></returns>
        public virtual async Task<bool> SendLogout(bool NeedReply = true)
        {
            return await Command(CommandType.Logout, NeedReply);
        }
        /// <summary>
        /// 开始心跳
        /// </summary>
        public virtual void StartBeat()
        {
            IsBeat = true;
            Task.Run(Beat);
        }
        /// <summary>
        /// 结束心跳
        /// </summary>
        public virtual void StopBeat()
        {
            IsBeat = false;
        }
        //68H                帧头                        1位
        //L                    长度                        2位            用户数据长度 L 采用 BIN 编码，是控制域、地址域、链路用户数据（应用层）的字节总数。
        //L                    长度                        2位                
        //68H                固定帧                    1位
        //C                    控制域                    1位            C9
        //A1                地址域1：省地市区县码        3位            00 00 01
        //A2                地址域2：终端地址            3位            00 00 00 
        //A3                地址域3：主站地址            1位            01
        //AFN = 02H            应用层功能码                1位            
        //SEQ                帧序列域                    1位            0b0111 0000 = 0x70 -> D7是否有时间标签        D6是否为报文第一帧    D5是否为报文尾帧        D4是否需要对该帧报文进行确认        D3-D0 启动帧序号/响应帧序号（0~15）
        //信息点标识（DA=0）                            2位            DA2 DA1 0x00 0x00
        //数据标识编码                                4位
        //数据标识内容                                N位
        //CS                校验位                    1位
        //16H                帧尾                        1位
        /// <summary>
        /// 根据指令构造数据帧
        /// </summary>
        /// <returns></returns>
        protected virtual byte[] CreateFrame(CommandType commandType = CommandType.HeartBeat)
        {
            try
            {
                List<byte> Frame = new List<byte>();
                Frame.AddRange(new byte[] { 0x68, 0x00, 0x00, 0x00, 0x00, 0x68 });// 固定帧格式，68h L2 L2 68H
                #region 构建自定义信息部分
                List<byte> UserDataArea = new List<byte>();
                UserDataArea.Add(GetControlArea(commandType));// 控制域 1位
                UserDataArea.AddRange(GetAddress());// 地址域 7位
                UserDataArea.Add(GetFunctionNumber(commandType));// 应用层功能码 1位
                UserDataArea.Add(GetSequence());// 帧序列域 1位
                UserDataArea.AddRange(GetDataAssignment(commandType));// 信息点标识 2位
                UserDataArea.AddRange(GetDataIdentity(commandType));// 获取数据标识编码 4位
                UserDataArea.AddRange(GetDataContext(commandType));// 获取数据标识内容
                #endregion
                byte[] Length = GetLittleEndianBytes((ushort)UserDataArea.Count, 2);// 修改长度
                for (int i = 0; i < Length.Length; i++)
                {
                    Frame[i + 1] = Length[i];
                    Frame[i + 3] = Length[i];
                }
                Frame.AddRange(UserDataArea);
                byte CS = CheckSum(UserDataArea);
                Frame.Add(CS);
                Frame.Add(0x16);// 固定帧尾
                return Frame.ToArray();
            }
            catch (Exception ex)
            {
                LYTest.Utility.Log.LogManager.AddMessage("客户端数据帧构造失败，错误信息：" + ex.Message, Utility.Log.EnumLogSource.服务器日志, Utility.Log.EnumLevel.Error);
                return new byte[0];
            }
        }
        /// <summary>
        /// 获取控制域
        /// </summary>
        /// <returns></returns>
        protected virtual byte GetControlArea(CommandType commandType)
        {
            switch (commandType)
            {
                case CommandType.Default | CommandType.HeartBeat | CommandType.Login | CommandType.Logout:
                    return 0xC9;
                case CommandType.CallbackTestStart:
                    return 0x4B;
                default:
                    return 0xC9;
            }
        }
        /// <summary>
        /// 获取地址域
        /// </summary>
        /// <returns></returns>
        protected virtual byte[] GetAddress()
        {
            List<byte> address = new List<byte>();
            #region 计算流水编号
            ConfigHelper.Instance.SetControl_No = Regex.Replace(
                ConfigHelper.Instance.SetControl_No, @"[^0-9]", "", RegexOptions.IgnoreCase)
                .PadLeft(6, '0').Substring(0, 6);
            string AssemblyID = ConfigHelper.Instance.SetControl_No;
            #endregion
            #region 计算台体编号
            ConfigHelper.Instance.SetControl_BenthNo = Regex.Replace(
                ConfigHelper.Instance.SetControl_BenthNo, @"[^0-9]", "", RegexOptions.IgnoreCase)
                .PadLeft(6, '0').Substring(0, 6);
            string BenchID = ConfigHelper.Instance.SetControl_BenthNo;
            #endregion
            byte[] assemblyNoBytes = ByteStringToBytes(AssemblyID);
            address.AddRange(assemblyNoBytes.Reverse());
            byte[] benchNoBytes = ByteStringToBytes(BenchID);
            address.AddRange(benchNoBytes.Reverse());
            // 终端地址
            address.Add(0x01);

            return address.ToArray();
        }
        /// <summary>
        /// 将十六进制字符串转换为字节数组
        /// </summary>
        /// <param name="byteString"></param>
        /// <returns></returns>
        protected virtual byte[] ByteStringToBytes(string byteString)
        {
            int byteCount = byteString.Length % 2 + byteString.Length / 2;
            byte[] bytes = new byte[byteCount];
            for (int i = 0; i < byteCount; i++)
            {
                bytes[i] = Convert.ToByte(byteString.Substring(i * 2, 2), 16);
            }
            return bytes;
        }
        /// <summary>
        /// 将ulong转换为小端序字节数组（自动处理系统字节序）
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        protected virtual byte[] GetLittleEndianBytes(ulong value, int length = 6)
        {
            length = Math.Max(Math.Min(length, 8), 0);
            byte[] result = new byte[length];
            byte[] bytes = BitConverter.GetBytes(value);
            if (!BitConverter.IsLittleEndian) Array.Reverse(bytes);
            Array.Copy(bytes, result, length);
            return result;
        }
        /// <summary>
        /// 获取应用层功能码
        /// </summary>
        /// <returns></returns>
        protected virtual byte GetFunctionNumber(CommandType commandType = CommandType.HeartBeat)
        {
            switch (commandType)
            {
                case CommandType.HeartBeat | CommandType.Login | CommandType.Logout:
                    return 0x02;
                case CommandType.CallbackTestStart:
                    return 0x04;
                default:
                    return 0x02;
            }
        }
        /// <summary>
        /// 获取数据标识
        /// </summary>
        /// <returns></returns>
        protected virtual byte[] GetDataAssignment(CommandType commandType)
        {
            switch (commandType)
            {
                case CommandType.Default | CommandType.HeartBeat | CommandType.Login | CommandType.Logout:
                    return new byte[] { 0x00, 0x00 };
                case CommandType.CallbackTestStart:
                    return new byte[] { 0x01, 0x01 };
                default:
                    return new byte[] { 0x00, 0x00 };
            }
        }
        /// <summary>
        /// 获取序列域
        /// </summary>
        /// <param name="frameIndex">帧索引</param>
        /// <param name="needCheck">需要确认</param>
        /// <param name="isTailFrame">是尾帧</param>
        /// <param name="isHeadFrame">是头帧</param>
        /// <param name="hasTimeTag">有时间标签</param>
        /// <returns></returns>
        protected virtual byte GetSequence(int frameIndex = 0, bool needCheck = false, bool isTailFrame = true, bool isHeadFrame = true, bool hasTimeTag = false)
        {
            byte Sequence = 0;
            Sequence ^= ReverseBits((byte)(frameIndex % 0x10), 4);// 逆序表示
            Sequence ^= needCheck ? (byte)(1 << 4) : (byte)(0);
            Sequence ^= isTailFrame ? (byte)(1 << 5) : (byte)(0);
            Sequence ^= isHeadFrame ? (byte)(1 << 6) : (byte)(0);
            Sequence ^= hasTimeTag ? (byte)(1 << 7) : (byte)(0);
            return Sequence;
        }
        /// <summary>
        /// 获取数据标识编码
        /// </summary>
        /// <returns></returns>
        protected virtual byte[] GetDataIdentity(CommandType commandType = CommandType.HeartBeat)
        {
            List<byte> DataIdentity = new List<byte>();
            switch (commandType)
            {
                case CommandType.HeartBeat:
                    DataIdentity = new List<byte>() { 0xE0, 0x00, 0x10, 0x01 };
                    break;
                case CommandType.Login:
                    DataIdentity = new List<byte>() { 0xE0, 0x00, 0x10, 0x00 };
                    break;
                case CommandType.Logout:
                    DataIdentity = new List<byte>() { 0xE0, 0x00, 0x10, 0x02 };
                    break;
                case CommandType.CallbackTestStart:
                    DataIdentity = CallbackTestStartDataIdentity.ToList(); ;
                    break;
                default:
                    break;
            }
            DataIdentity.Reverse();
            return DataIdentity.ToArray();
        }
        /// <summary>
        /// 获取数据内容
        /// </summary>
        /// <returns></returns>
        protected virtual byte[] GetDataContext(CommandType commandType = CommandType.HeartBeat)
        {
            List<byte> DataContext = new List<byte>();
            switch (commandType)
            {
                case CommandType.HeartBeat:// 心跳包
                    break;
                case CommandType.Login: // 规约版本号
                    DataContext.AddRange(new byte[] { 0x10, 0x01 });
                    break;
                case CommandType.Logout:// 登出
                    break;
                case CommandType.CallbackTestStart:// 响应考试开始
                    DataContext.Add(0x00);
                    break;
                default:
                    break;
            }
            return DataContext.ToArray();
        }
        /// <summary>
        /// 心跳功能
        /// </summary>
        /// <returns></returns>
        protected virtual async Task Beat()
        {
            while (IsBeat)
            {
                await Command(CommandType.HeartBeat);
                LYTest.Utility.Log.LogManager.AddMessage("心跳包发送", Utility.Log.EnumLogSource.服务器日志, Utility.Log.EnumLevel.Information);
                await Task.Delay(BeatInterval);
            }
            await Task.Yield();
        }
        /// <summary>
        /// 根据指令发送数据帧
        /// </summary>
        protected virtual async Task<bool> Command(CommandType commandType, bool NeedReply = false)
        {
            try
            {
                byte[] sendFrame;
                string str = string.Empty;
                switch (commandType)
                {
                    case CommandType.HeartBeat:
                        {
                            str = "【组帧内容：心跳帧】";
                            sendFrame = CreateFrame(CommandType.HeartBeat);
                            break;
                        }
                    case CommandType.Login:
                        {
                            str = "【组帧内容：登录帧】";
                            sendFrame = CreateFrame(CommandType.Login);
                            break;
                        }
                    case CommandType.Logout:
                        {
                            str = "【组帧内容：登出帧】";
                            sendFrame = CreateFrame(CommandType.HeartBeat);
                            break;
                        }
                    case CommandType.CallbackTestStart:
                        {
                            str = "【组帧内容：相应考试开始帧】";
                            sendFrame = CreateFrame(CommandType.CallbackTestStart);
                            break;
                        }
                    default:
                        {
                            sendFrame = new byte[] { };
                            break;
                        }
                }
                await Stream.WriteAsync(sendFrame, 0, sendFrame.Length);
                str = $"[{DateTime.Now:T}]客户端 >>> 上位机：{BitConverter.ToString(sendFrame).Replace("-", " ")}";
                LYTest.Utility.Log.LogManager.AddMessage(str, Utility.Log.EnumLogSource.服务器日志, Utility.Log.EnumLevel.Information);
                WriteLog(str);
                if (NeedReply)
                {
                    LYTest.Utility.Log.LogManager.AddMessage($"客户端等待服务器响应中...", Utility.Log.EnumLogSource.服务器日志, Utility.Log.EnumLevel.Information);
                    DateTime endT = DateTime.Now.AddMilliseconds(10000);// 10秒等待
                    while (DateTime.Now < endT)
                    {
                        // 检测可供读取的数据量是否大于0
                        if (Available > 0)
                        {
                            byte[] frame = new byte[256];
                            int len = await Stream.ReadAsync(frame, 0, frame.Length);
                            byte[] bytes = new byte[len];
                            Array.Copy(frame, 0, bytes, 0, len);
                            str = $"[{DateTime.Now:T}]客户端 <<< 上位机：{BitConverter.ToString(bytes).Replace("-", " ")}";
                            LYTest.Utility.Log.LogManager.AddMessage(str, Utility.Log.EnumLogSource.服务器日志, Utility.Log.EnumLevel.Information);
                            WriteLog(str);
                            if (commandType == CommandType.Login && len > 0) SuccessLogin = true;
                            if (len <= 0) continue;
                            return len > 0;
                        }
                        Task.Delay(100).Wait();
                    }
                    LYTest.Utility.Log.LogManager.AddMessage($"服务器响应失败，可供读取的数据量为0.", Utility.Log.EnumLogSource.服务器日志, Utility.Log.EnumLevel.Information);
                    return true;
                }
                await Task.Yield();
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                await Task.Yield();
                return false;
            }
        }
        /// <summary>
        /// 位逆序
        /// </summary>
        /// <param name="b"></param>
        /// <param name="bitsCount"></param>
        /// <returns></returns>
        public static byte ReverseBits(byte b, int bitsCount = 4)
        {
            byte result = 0;
            for (int i = 0; i < bitsCount; i++)
            {
                result = (byte)((result << 1) | (b & 1)); // 左移结果，取最低位
                b >>= 1; // 原字节右移
            }
            return result;
        }
        /// <summary>
        /// 获取和校验
        /// </summary>
        /// <param name="UserDataArea"></param>
        /// <returns></returns>
        public static byte CheckSum(IEnumerable<byte> UserDataArea)
        {
            byte result = 0x00;
            foreach (var data in UserDataArea)
            {
                result += data;
            }
            return result;
        }
        /// <summary>
        /// 释放资源
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            Stream?.Dispose();
            OnLogin = null;
            OnReceiveTestStart = null;
            base.Dispose(disposing);
        }
        protected static List<byte[]> TestStartDataIdentityCollection { get; } =
            new List<byte[]> {
                new byte[] { 0xA0, 0x20, 0x00, 0x00 },
                new byte[] { 0xA0, 0x20, 0x01, 0x00 },
                new byte[] { 0xA0, 0x20, 0xFF, 0x00 }
            };
        protected byte[] CallbackTestStartDataIdentity;
        /// <summary>
        /// 处理服务器发送开始帧
        /// </summary>
        /// <returns></returns>
        private async Task HandleTestStart()
        {
            try
            {
                while (Connected)
                {
                    if (Available <= 0)
                    {
                        await Task.Delay(100);
                        continue;
                    }
                    byte[] buffer = new byte[0xff];
                    int len = await Stream.ReadAsync(buffer, 0, buffer.Length);
                    if (len <= 0) continue;
                    byte[] frame = new byte[len];
                    Array.Copy(buffer, 0, frame, 0, len);
                    //LYTest.Utility.Log.LogManager.AddMessage($"客户端接收到数据帧：{BitConverter.ToString(frame).Replace("-", " ")}", Utility.Log.EnumLogSource.服务器日志, Utility.Log.EnumLevel.Information);
                    byte[] lenFrame = new byte[2];
                    Array.Copy(frame, 1, lenFrame, 0, 2);
                    lenFrame.Reverse();
                    int userDataLength = BitConverter.ToInt16(lenFrame, 0);// 获取数据域长度
                    if (userDataLength + 8 != len)
                    {
                        // 长度不对
                        continue;
                    }
                    // 获取数据标识编码 // 手动设置
                    //byte[] DataAssignment = DeserializeDataAssignment(frame);
                    // 截取考试开始指令
                    CallbackTestStartDataIdentity = DeserializeDataIdentity(frame);
                    if (!TestStartDataIdentityCollection.Any(bytes => bytes.SequenceEqual(CallbackTestStartDataIdentity)))
                    {
                        // 不是考试开始指令
                        continue;
                    }
                    // TODO:回复收到指令帧
                    await Command(CommandType.CallbackTestStart, false);
                    // 触发事件
                    OnReceiveTestStart?.Invoke();
                    break;
                }
            }
            catch (Exception ex)
            {
                LYTest.Utility.Log.LogManager.AddMessage($"服务器响解析数据帧失败，错误信息：{ex.Message}", Utility.Log.EnumLogSource.服务器日志, Utility.Log.EnumLevel.Error);
            }
            await Task.Yield();
        }
        /// <summary>
        /// 信息点标识
        /// </summary>
        /// <param name="originalFrame"></param>
        /// <returns>byte[2] = [ DA2,DA1 ]</returns>
        protected virtual byte[] DeserializeDataAssignment(byte[] originalFrame)
        {
            // 跳过前16位  = 固定1 + 长度4 + 固定1 + 控制域1 + 地址7 + 应用层功能码1 + 序列域1  
            // 取2位
            return originalFrame.Skip(16).Take(2).ToArray();
        }
        /// <summary>
        /// 逆序列化数据标识编码
        /// </summary>
        /// <param name="originalFrame"></param>
        /// <returns></returns>
        protected virtual byte[] DeserializeDataIdentity(byte[] originalFrame)
        {
            // 跳过前18位  = 固定1 + 长度4 + 固定1 + 控制域1 + 地址7 + 应用层功能码1 + 序列域1 + 信息点标识2 
            // 取4位，逆序
            return originalFrame.Skip(18).Take(4).Reverse().ToArray();
        }
        /// <summary>
        /// 指令类型
        /// </summary>
        public enum CommandType
        {
            /// <summary>
            /// 默认指令
            /// </summary>
            Default = 0x00,
            /// <summary>
            /// 心跳
            /// </summary>
            HeartBeat = 0x01,
            /// <summary>
            /// 登录
            /// </summary>
            Login = 0x02,
            /// <summary>
            /// 登出
            /// </summary>
            Logout = 0x03,
            /// <summary>
            /// 响应接收到考试开始信号
            /// </summary>
            CallbackTestStart = 0x04,
        }

        #region 日志
        /// <summary>
        /// 客户端日志文件夹路径
        /// </summary>
        private static object lockObj = new object();
        /// <summary>
        /// 将文本写入客户端日志
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        private static bool WriteLog(string msg)
        {
            lock (lockObj)
            {
                try
                {
                    string DirectoryPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"Log\\客户端日志"); //文件夹路径
                    if (!System.IO.Directory.Exists(DirectoryPath))  //创建目录
                    {
                        System.IO.Directory.CreateDirectory(DirectoryPath);
                        System.Threading.Thread.Sleep(500);//创建目录稍等一点延迟，以防创建失败
                    }

                    string FileName = DirectoryPath + $"\\{DateTime.Now:yyyy-MM-dd}.txt";
                    System.IO.File.AppendAllText(FileName, msg + "\r\n");
                }
                catch (Exception ex)
                {
                    LYTest.Utility.Log.LogManager.AddMessage($"写入客户端日志失败，错误信息：{ex.Message}", Utility.Log.EnumLogSource.服务器日志, Utility.Log.EnumLevel.Error);
                    return false;
                }
            }
            return true;
        }
        #endregion
    }
}
