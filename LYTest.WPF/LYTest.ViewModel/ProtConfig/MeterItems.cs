using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LYTest.ViewModel.ProtConfig
{
   public class MeterItems : ViewModelBase
    {
        private bool flagChanged;
        /// <summary>
        /// 修改标记
        /// </summary>
        public bool FlagChanged
        {
            get { return flagChanged; }
            set { SetPropertyValue(value, ref flagChanged, "FlagChanged"); }
        }

        protected internal override void OnPropertyChanged(string propertyName)
        {
            base.OnPropertyChanged(propertyName);
            if (propertyName != "FlagChanged")
            {
                FlagChanged = true;
            }
        }


        private ServerItem server;
        /// <summary>
        /// 服务器名
        /// </summary>
        public ServerItem Server
        {
            get { return server; }
            set { SetPropertyValue(value, ref server, "Server"); }
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


        private string comParam ="2400,e,8,1";
        /// <summary>
        /// 串口参数
        /// </summary>
        public string ComParam
        {
            get { return comParam; }
            set { SetPropertyValue(value, ref comParam, "ComParam"); }
        }

        private string portCount = "24";
        /// <summary>
        /// 端口数量
        /// </summary>
        public string PortCount
        {
            get { return portCount; }
            set { SetPropertyValue(value, ref portCount, "PortCount"); }
        }


        private string intervalValue = "1";
        /// <summary>
        /// 端口间隔
        /// </summary>
        public string IntervalValue
        {
            get { return intervalValue; }
            set { SetPropertyValue(value, ref intervalValue, "IntervalValue"); }
        }


        private string startPort = "1";
        /// <summary>
        /// 开始端口
        /// </summary>
        public string StartPort
        {
            get { return startPort; }
            set { SetPropertyValue(value, ref startPort, "StartPort"); }
        }

        public override string ToString()
        {
               // < !--服务器编号   串口参数 开始端口 端口间隔 端口数量   帧最大间隔 字节最大间隔  是否编辑 删除-->
            string str = string.Format("{0}|{1}|{2}|{3}|{4}|{5}|{6}",Server.Address, ComParam, StartPort, IntervalValue, PortCount, MaxTimePerFrame, MaxTimePerByte);
            return str;
        }
    }
}
