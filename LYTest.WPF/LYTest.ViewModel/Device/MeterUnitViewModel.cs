using LYTest.Core.Enum;

namespace LYTest.ViewModel.Device
{
    /// <summary>
    /// 表状态数据模型
    /// </summary>
    public class MeterUnitViewModel : ViewModelBase
    {

        private bool status;
        /// <summary>
        /// 联机状态
        /// </summary>
        public bool Status
        {
            get { return status; }
            set { SetPropertyValue(value, ref status, "Status"); }
        }
        private bool initialStatus;
        /// <summary>
        /// 初始化状态
        /// </summary>
        public bool InitialStatus
        {
            get { return initialStatus; }
            set { SetPropertyValue(value, ref initialStatus, "InitialStatus"); }
        }


        private CommMode conn_Type;
        /// <summary>
        /// 通讯方式
        /// </summary>
        public CommMode Conn_Type
        {
            get { return conn_Type; }
            set { SetPropertyValue(value, ref conn_Type, "Conn_Type"); }
        }


        private string portNum = "1";
        /// <summary>
        /// 端口号
        /// </summary>
        public string PortNum
        {
            get { return portNum; }
            set { SetPropertyValue(value, ref portNum, "PortNum"); }
        }


        private string comParam = "38400,n,8,1";
        /// <summary>
        /// 串口参数
        /// </summary>
        public string ComParam
        {
            get { return comParam; }
            set { SetPropertyValue(value, ref comParam, "ComParam"); }
        }

        private string maxTimePerByte = "10";
        /// <summary>
        /// 字节最大时间间隔(ms)
        /// </summary>
        public string MaxTimePerByte
        {
            get { return maxTimePerByte; }
            set { SetPropertyValue(value, ref maxTimePerByte, "MaxTimePerByte"); }
        }


        private string maxTimePerFrame = "3000";
        /// <summary>
        /// 帧最大时间间隔(ms)
        /// </summary>
        public string MaxTimePerFrame
        {
            get { return maxTimePerFrame; }
            set { SetPropertyValue(value, ref maxTimePerFrame, "MaxTimePerFrame"); }
        }



        private bool isExist;
        /// <summary>
        /// 端口是否存在
        /// </summary>
        public bool IsExist
        {
            get { return isExist; }
            set { SetPropertyValue(value, ref isExist, "IsExist"); }
        }

        private string address;
        /// <summary>
        /// IP或“COM”
        /// </summary>
        public string Address
        {
            get { return address; }
            set
            {
                SetPropertyValue(value, ref address, "Address");
            }
        }
        private string startPort;
        /// <summary>
        /// 起始端口
        /// </summary>
        public string StartPort
        {
            get { return startPort; }
            set { SetPropertyValue(value, ref startPort, "StartPort"); }
        }
        private string remotePort;
        /// <summary>
        /// 远程端口
        /// </summary>
        public string RemotePort
        {
            get { return remotePort; }
            set { SetPropertyValue(value, ref remotePort, "RemotePort"); }
        }
    }

}
