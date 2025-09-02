using LYTest.Core.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LYTest.ViewModel.Device
{
    public class DeviceData : ViewModelBase
    {

        public CommMode conn_Type;
        /// <summary>
        /// 通讯方式
        /// </summary>
        public CommMode Conn_Type
        {
            get { return conn_Type; }
            set { SetPropertyValue(value, ref conn_Type, "Conn_Type"); }
        }
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



        private string model;
        /// <summary>
        /// 型号
        /// </summary>
        public string Model
        {
            get { return model; }
            set { SetPropertyValue(value, ref model, "Model"); }
        }


        private string name;
        /// <summary>
        /// 名称
        /// </summary>
        public string Name
        {
            get { return name; }
            set { SetPropertyValue(value, ref name, "Name"); }
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


        private object obj;
        /// <summary>
        /// 设备实例
        ///MethodInfo mInfo = type.GetMethod(方法名称); //获取当前方法
        /// mInfo.Invoke(type, value);  //接收调用返回值，判断调用是否成功  new object[1] {5}
        /// </summary>
        public object Obj
        {
            get { return obj; }
            set { SetPropertyValue(value, ref obj, "Obj"); }
        }

       
       private Type type;
        /// <summary>
        /// 设备 类型
        ///MethodInfo mInfo = type.GetMethod(方法名称); //获取当前方法
        /// mInfo.Invoke(type, value);  //接收调用返回值，判断调用是否成功  new object[1] {5}
        /// </summary>
        public Type Type
        {
            get { return type; }
            set { SetPropertyValue(value, ref type, "Type"); }
        }

    }

    public class DeviceData2 : ViewModelBase
    {

        private List<DeviceListData> deviceList;
        /// <summary>
        /// 设备列表
        /// </summary>
        public List<DeviceListData> DeviceList
        {
            get { return deviceList; }
            set { SetPropertyValue(value, ref deviceList, "DeviceList"); }
        }

    }
    public class DeviceListData : ViewModelBase
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


        private string model;
        /// <summary>
        /// 型号
        /// </summary>
        public string Model
        {
            get { return model; }
            set { SetPropertyValue(value, ref model, "Model"); }
        }


        private string name;
        /// <summary>
        /// 名称
        /// </summary>
        public string Name
        {
            get { return name; }
            set { SetPropertyValue(value, ref name, "Name"); }
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

        private object obj;
        /// <summary>
        /// 设备实例
        ///MethodInfo mInfo = type.GetMethod(方法名称); //获取当前方法
        /// mInfo.Invoke(type, value);  //接收调用返回值，判断调用是否成功  new object[1] {5}
        /// </summary>
        public object Obj
        {
            get { return obj; }
            set { SetPropertyValue(value, ref obj, "Obj"); }
        }


        private Type type;
        /// <summary>
        /// 设备 类型
        ///MethodInfo mInfo = type.GetMethod(方法名称); //获取当前方法
        /// mInfo.Invoke(type, value);  //接收调用返回值，判断调用是否成功  new object[1] {5}
        /// </summary>
        public Type Type
        {
            get { return type; }
            set { SetPropertyValue(value, ref type, "Type"); }
        }
    }
}
