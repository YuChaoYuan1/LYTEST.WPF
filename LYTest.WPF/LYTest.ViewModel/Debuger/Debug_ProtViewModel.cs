using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Ports;
using System.Text;

namespace LYTest.ViewModel.Debug
{
    public class Debug_ProtViewModel : ViewModelBase
    {
        public Debug_ProtViewModel()
        {
            RefProt();
            BaudRateList.Clear();
            BaudRateList.Add("1200");
            BaudRateList.Add("2400");
            BaudRateList.Add("9600");
            BaudRateList.Add("38400");
            BaudRateList.Add("115200");

            DataBitList.Add("5");
            DataBitList.Add("6");
            DataBitList.Add("7");
            DataBitList.Add("8");

            StopBitList.Add("1");
            StopBitList.Add("2");

            CheckTypeList.Add("NODE");
            CheckTypeList.Add("ODD");
            CheckTypeList.Add("EVEN");


            Port.DataReceived += new SerialDataReceivedEventHandler(Port_DataReceived); //接收返回值回调函数
        }

        readonly SerialPort Port = new SerialPort();

        private ObservableCollection<string> protList = new ObservableCollection<string>();
        /// <summary>
        ///端口列表
        public ObservableCollection<string> ProtList
        {
            get { return protList; }
            set { SetPropertyValue(value, ref protList, "ProtList"); }
        }
        private string protName;
        /// <summary>
        /// 端口名称
        /// </summary>
        public string ProtName
        {
            get { return protName; }
            set
            {
                SetPropertyValue(value, ref protName, "ProtName");
                //LoadTableFields();
            }
        }



        #region 串口通讯基本属性

        private string baudRate = "9600";
        /// <summary>
        /// 波特率
        /// </summary>
        public string BaudRate
        {
            get { return baudRate; }
            set
            {
                SetPropertyValue(value, ref baudRate, "BaudRate");
                //LoadTableFields();
            }
        }
        private ObservableCollection<string> baudRateList = new ObservableCollection<string>();
        /// <summary>
        ///波特率
        public ObservableCollection<string> BaudRateList
        {
            get { return baudRateList; }
            set { SetPropertyValue(value, ref baudRateList, "BaudRateList"); }
        }

        private string dataBit = "8";
        /// <summary>
        /// 数据位
        /// </summary>
        public string DataBit
        {
            get { return dataBit; }
            set
            {
                SetPropertyValue(value, ref dataBit, "DataBit");
                //LoadTableFields();
            }
        }
        private ObservableCollection<string> dataBitList = new ObservableCollection<string>();
        /// <summary>
        ///数据位
        public ObservableCollection<string> DataBitList
        {
            get { return dataBitList; }
            set { SetPropertyValue(value, ref dataBitList, "DataBitList"); }
        }

        private string stopBit = "1";
        /// <summary>
        /// 停止位
        /// </summary>
        public string StopBit
        {
            get { return stopBit; }
            set
            {
                SetPropertyValue(value, ref stopBit, "StopBit");
                //LoadTableFields();
            }
        }
        private ObservableCollection<string> stopBitList = new ObservableCollection<string>();
        /// <summary>
        ///停止位
        public ObservableCollection<string> StopBitList
        {
            get { return stopBitList; }
            set { SetPropertyValue(value, ref stopBitList, "StopBitList"); }
        }



        private string checkType = "EVEN";
        /// <summary>
        /// 校验方式
        /// </summary>
        public string CheckType
        {
            get { return checkType; }
            set
            {
                SetPropertyValue(value, ref checkType, "CheckType");
                //LoadTableFields();
            }
        }
        private ObservableCollection<string> checkTypeList = new ObservableCollection<string>();
        /// <summary>
        ///校验方式
        public ObservableCollection<string> CheckTypeList
        {
            get { return checkTypeList; }
            set { SetPropertyValue(value, ref checkTypeList, "CheckTypeList"); }
        }
        #endregion

        private string protStartName = "打开串口";
        /// <summary>
        /// 端口开启按钮显示文字
        /// </summary>
        public string ProtStartName
        {
            get { return protStartName; }
            set
            {
                SetPropertyValue(value, ref protStartName, "ProtStartName");
                //LoadTableFields();
            }
        }


        private bool isHexSendData = true;
        /// <summary>
        /// 是否十六进制发送数据
        /// </summary>
        public bool IsHexSendData
        {
            get { return isHexSendData; }
            set
            {
                SetPropertyValue(value, ref isHexSendData, "IsHexSendData");
            }
        }

        //private ObservableCollection<string>  returnData= new ObservableCollection<string>();
        ///// <summary>
        /////数据列表
        //public ObservableCollection<string> ReturnData
        //{
        //    get { return returnData; }
        //    set { SetPropertyValue(value, ref returnData, "ReturnData"); }
        //}

        private string sendData = "";
        /// <summary>
        /// 发送的数据
        /// </summary>
        public string SendData
        {
            get { return sendData; }
            set
            {
                SetPropertyValue(value, ref sendData, "SendData");
            }
        }

        private string receiveData = "";
        /// <summary>
        /// 接收的数据
        /// </summary>
        public string ReceiveData
        {
            get { return receiveData; }
            set
            {
                SetPropertyValue(value, ref receiveData, "ReceiveData");
            }
        }


        //刷新端口

        public void RefProt()
        {
            ProtList.Clear();
            try
            {
                string[] names = SerialPort.GetPortNames();
                if (names.Length > 0)
                {

                    int[] nams = new int[names.Length];
                    for (int i = 0; i < names.Length; i++)
                    {
                        nams[i] = int.Parse(names[i].Replace("COM", ""));
                    }
                    Array.Sort(nams);
                    for (int i = 0; i < nams.Length; i++)
                    {
                        ProtList.Add("COM" + nams[i]);
                    }
                    ProtName = ProtList[0];
                }

            }
            catch (Exception)
            {

                throw;
            }


        }

        //打开端口
        public void OpenPort()
        {
            try
            {
                string temName = ProtStartName;
                if (ProtStartName == "打开串口")
                {
                    Port.PortName = ProtName;
                    Port.BaudRate = int.Parse(BaudRate);
                    Port.StopBits = (StopBits)int.Parse(StopBit);
                    Port.DataBits = int.Parse(DataBit);
                    Port.Parity = CheckType.ToLower() == "NONE" ? Parity.None : CheckType.ToLower() == "EVEN" ? Parity.Even : Parity.Mark;
                    Port.DtrEnable = true;
                    Port.Open();
                    //OpenPortEnabled = false;
                    ProtStartName = "关闭串口";
                }
                else
                {
                    Port.Close();
                    ProtStartName = "打开串口";
                }
                Utility.Log.LogManager.AddMessage($"{temName}【{ProtName}】成功");
            }
            catch (Exception ex)
            {
                Utility.Log.LogManager.AddMessage($"{ProtStartName}【{ProtName}】失败" + ex.ToString());
                //OpenPortEnabled = true;
            }
        }


        //串口接收数据回调函数 
        private void Port_DataReceived(Object sender, SerialDataReceivedEventArgs e)
        {
            //String str = Port.ReadExisting();
            //ReturnData.Add(str);

            try
            {
                int count = Port.BytesToRead;
                byte[] readBuffer = new byte[count];
                Port.Read(readBuffer, 0, count);
                string strReceive = "";
                if (IsHexSendData)
                    strReceive = GetStringFromBytes(readBuffer);  //转十六进制
                else
                    strReceive = Encoding.Default.GetString(readBuffer);  //字母、数字、汉字转换为字符串          

                ReceiveData += "\r\n" + DateTime.Now + "\r\n" + "接收数据：" + strReceive;
            }
            catch (Exception err)
            {
                ReceiveData += DateTime.Now + "\r\n接收数据出错:" + err.ToString();
            }


        }
        ////发送数据
        //public void But_SendData()
        //{
        //    byte[] Data = new byte[1];
        //    SendData = SendData.Trim().Replace('-', ' ').Replace(" ", "");
        //    if (SendData.Length % 2 == 0)
        //    {
        //        string s = "";
        //        for (int i = 0; i < SendData.Length; i += 2)
        //            s += SendData.Substring(i, 2) + " ";
        //        //buffer[i / 2] = (byte)Convert.ToByte(s.Substring(i, 2), 16);
        //        SendData = s.TrimEnd(' ');//整理格式
        //    }

        //    //判断串口是否打开
        //    if (Port.IsOpen)
        //    {
        //        //判断发送域是否有数据
        //        if (SendData != "")
        //        {
        //            //以字符形式发送数据
        //            try
        //            {
        //                //向串口中写入数据 
        //                byte[] data = null;
        //                if (IsHexSendData)
        //                    data = HexStringToByteArray(SendData);
        //                else
        //                    data = Encoding.Default.GetBytes(SendData);
        //                ReceiveData += "\r\n" + DateTime.Now + "\r\n" + "发送数据：" + GetStringFromBytes(data);
        //                Port.Write(data, 0, data.Length);
        //            }
        //            catch (Exception err)
        //            {
        //                Utility.Log.LogManager.AddMessage("串口写入数据错误" + err.ToString());
        //                //关闭串口
        //                Port.Close();
        //            }
        //        }
        //    }
        //}

        public static byte[] HexStringToByteArray(string s)
        {
            s = s.Replace(" ", "");
            byte[] buffer = new byte[s.Length / 2];
            for (int i = 0; i < s.Length; i += 2)
                buffer[i / 2] = (byte)Convert.ToByte(s.Substring(i, 2), 16);
            return buffer;
        }

        ///// <summary>
        ///// 把十六进制格式的字符串转换成字节数组。
        ///// </summary>
        ///// <param name="pString">要转换的十六进制格式的字符串</param>
        ///// <returns>返回字节数组。</returns>
        //public static byte[] getBytesFromString(string pString)
        //{
        //    string[] str = pString.Split(' ');     //把十六进制格式的字符串按空格转换为字符串数组。
        //    byte[] bytes = new byte[str.Length];     //定义字节数组并初始化，长度为字符串数组的长度。
        //    for (int i = 0; i < str.Length; i++)     //遍历字符串数组，把每个字符串转换成字节类型赋值给每个字节变量。
        //        bytes[i] = Convert.ToByte(Convert.ToInt32(str[i], 16));
        //    return bytes;     //返回字节数组。
        //}
        /// <summary>
        /// 把字节数组转换为十六进制格式的字符串。
        /// </summary>
        /// <param name="pByte">要转换的字节数组。</param>
        /// <returns>返回十六进制格式的字符串。</returns>
        public static string GetStringFromBytes(byte[] pByte)
        {
            string str = "";     //定义字符串类型临时变量。
            //遍历字节数组，把每个字节转换成十六进制字符串，不足两位前面添“0”，以空格分隔累加到字符串变量里。
            for (int i = 0; i < pByte.Length; i++)
                str += (pByte[i].ToString("X").PadLeft(2, '0') + " ");
            str = str.TrimEnd(' ');     //去掉字符串末尾的空格。
            return str;     //返回字符串临时变量。
        }



        #region 串口监听

        private string portDataLogName = "开启端口监听";
        /// <summary>
        /// 波特率
        /// </summary>
        public string PortDataLogName
        {
            get { return portDataLogName; }
            set
            {
                SetPropertyValue(value, ref portDataLogName, "PortDataLogName");
                //LoadTableFields();
            }
        }

        TextWriter txt = null;
        //打开日志监听保存
        public void OpenPortDataLog()
        {
            if (PortDataLogName.IndexOf("开启") != -1)
            {
                if (txt == null)
                {
                    txt = Console.Out;
                }
                ConsoleLogTextWriter logSW = new ConsoleLogTextWriter();
                Console.SetOut(logSW);
                PortDataLogName = "关闭端口监听";
            }
            else
            {
                if (txt == null)
                {
                    return;
                }
                Console.SetOut(txt);
                PortDataLogName = "开启端口监听";

            }

        }
        #endregion

    }
    /// <summary>
    /// 捕获控制台输出并写入日志文件（推荐通过日志接口写日志）
    /// </summary>
    class ConsoleLogTextWriter : TextWriter
    {
        public List<string> list = new List<string>();
        public ConsoleLogTextWriter() : base() { }

        public override Encoding Encoding { get { return Encoding.UTF8; } }

        public override void Write(string value)
        {
            Log.WriteLog(value);
        }
        public override void WriteLine(string value)
        {
            Log.WriteLog(value);
        }
        public override void Close()
        {
            base.Close();
        }
    }
    /// <summary>
    /// 日志类（只作演示使用，可自己定义实现）
    /// </summary>
    class Log
    {
        public static void WriteLog(string msg)
        {
            try
            {
                string LogPath = string.Format(@"Log\DataFrameLog\{0}.txt", DateTime.Now.ToString("yyyy-MM-dd"));
                LogPath = Directory.GetCurrentDirectory() + "\\" + LogPath;
                FileStream fs = Utility.Log.LogManager.Create(LogPath);
                if (fs == null)
                {
                    return;
                }
                fs.Close();
                fs.Dispose();

                System.IO.File.AppendAllText(LogPath, DateTime.Now.ToString("HH:mm:ss") + ":  " + msg + "\r\n\r\n");
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
