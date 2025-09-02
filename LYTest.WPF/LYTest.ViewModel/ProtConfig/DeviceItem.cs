using LYTest.ViewModel.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LYTest.ViewModel.ProtConfig
{
   public class DeviceItem : ViewModelBase
    {
        private ServerItem server;
        /// <summary>
        /// 服务器名
        /// </summary>
        public ServerItem Server
        {
            get { return server; }
            set { SetPropertyValue(value, ref server, "Server"); }
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


        private string model;
        /// <summary>
        /// 型号
        /// </summary>
        public string Model
        {
            get { return model; }
            set { SetPropertyValue(value, ref model, "Model"); }
        }
        private bool isLink;
        /// <summary>
        /// 端口是否存在
        /// </summary>
        public bool IsLink
        {
            get { return isLink; }
            set { SetPropertyValue(value, ref isLink, "IsLink"); }
        }

        public override string ToString()
        {
            // < !--服务器编号  设备型号  串口参数 端口号   帧最大间隔 字节最大间隔  是否编辑 删除-->
            string str = string.Format("{0}|{1}|{2}|{3}|{4}|{5}", Server.Address, Model, ComParam,PortNum, MaxTimePerFrame, MaxTimePerByte);
            return str;
        }


        //private List<string> modelItems = new List<string>();
        ///// <summary>
        ///// 型号列表
        ///// </summary>
        //public List<string> ModelItems
        //{
        //    get { return modelItems; }
        //    set { SetPropertyValue(value, ref modelItems, "ModelItems"); }
        //}

        private AsyncObservableCollection<string> modelItems = new AsyncObservableCollection<string>();
        /// <summary>
        /// 设备列表
        /// </summary>                      
        public AsyncObservableCollection<string> ModelItems
        {
            get { return modelItems; }
            set { SetPropertyValue(value, ref modelItems, "ModelItems"); }
        }


    }
}
