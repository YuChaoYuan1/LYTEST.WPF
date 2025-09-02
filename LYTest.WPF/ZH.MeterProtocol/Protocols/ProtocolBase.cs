using LYTest.Core;
using LYTest.MeterProtocol.Protocols.DLT698.Struct;
using LYTest.MeterProtocol.SocketModule.Packet;
using System;
using System.Collections.Generic;

namespace LYTest.MeterProtocol.Protocols
{
    /// <summary>
    /// 电能表基类
    /// </summary>
    public class ProtocolBase : IMeterProtocol
    {
        public string FrameMean { get; set; }
        /// <summary>
        /// 多功能协议配置对象
        /// </summary>
        protected DgnProtocolInfo protocolInfo = null;

        /// <summary>
        /// 数据端口名称
        /// </summary>
        private string _PortName = string.Empty;

        /// <summary>
        /// 电表地址
        /// </summary>
        protected string MeterAddress { get; set; }


        public int IdentityStatus { get; set; }


        /// <summary>
        /// 通讯测试方式
        /// </summary>
        protected int CommTestType { get { return GetProtocolConfigValue("001", 1); } }
        /// <summary>
        /// 写日期方式
        /// </summary>
        protected int WriteDateTimeType { get { return GetProtocolConfigValue("002", 1); } }
        /// <summary>
        /// 读时间方式
        /// </summary>
        protected int ReadDateTimeType { get { return GetProtocolConfigValue("003", 1); } }
        /// <summary>
        /// 清除需量方式
        /// </summary>
        protected int ClearDemandType { get { return GetProtocolConfigValue("004", 1); } }
        /// <summary>
        /// 读需量方式
        /// </summary>
        protected int ReadDemandType { get { return GetProtocolConfigValue("005", 1); } }
        /// <summary>
        /// 读电量方式
        /// </summary>
        protected int ReadEnergyType { get { return GetProtocolConfigValue("006", 1); } }
        /// <summary>
        /// 读时段方式
        /// </summary>
        protected int ReadPeriodTimeType { get { return GetProtocolConfigValue("007", 1); } }
        /// <summary>
        /// 清电量方式
        /// </summary>
        protected int ClearEnergyType { get { return GetProtocolConfigValue("008", 1); } }

        /// <summary>
        /// 获取多功能协议配置参数
        /// </summary>
        /// <param name="key">要取值的KEY</param>
        /// <param name="defaultValue">默认值</param>
        /// <returns>对应配置项的值，如果没有对应的配置项目则返回默认值</returns>
        protected int GetProtocolConfigValue(string key, int defaultValue)
        {
            //如果无协议或是参数中不包括主键则返回默认值
            if (protocolInfo == null || protocolInfo.DgnPras == null || !protocolInfo.DgnPras.ContainsKey(key))
            {
                return defaultValue;
            }
            string[] arr = protocolInfo.DgnPras[key].Split('|');
            if (arr.Length == 2)
            {
                return int.Parse(arr[0]);
            }
            else
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// 数据收发
        /// </summary>
        /// <param name="sendPacket">发送数据包</param>
        /// <param name="recvPacket">接收数据包</param>
        /// <returns>发送结果</returns>
        protected bool SendData(SendPacket sendPacket, RecvPacket recvPacket)
        {
            sendPacket.IsNeedReturn = true;
            if (recvPacket == null) sendPacket.IsNeedReturn = false;
            //发送前更新一下波特率
            return MeterProtocolManager.Instance.SendData(_PortName, sendPacket, recvPacket, protocolInfo.Setting);
        }
        /// <summary>
        /// 数据收发
        /// </summary>
        /// <param name="sendPacket">发送数据包</param>
        /// <param name="recvPacket">接收数据包</param>
        /// <returns>发送结果</returns>
        protected bool SendData(string _PortName, SendPacket sendPacket, RecvPacket recvPacket)
        {
            sendPacket.IsNeedReturn = true;
            if (recvPacket == null) sendPacket.IsNeedReturn = false;
            //发送前更新一下波特率
            return MeterProtocolManager.Instance.SendData(_PortName, sendPacket, recvPacket, "9600,e,8,1");
        }
        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="sendData"></param>
        /// <param name="recvData"></param>
        /// <returns></returns>
        protected bool SendData(byte[] sendData, ref byte[] recvData)
        {
            Packet.MeterProtocolRecvPacket recvPacket = new Packet.MeterProtocolRecvPacket();
            Packet.MeterProtocolSendPacket sendPacket = new Packet.MeterProtocolSendPacket()
            {
                SendData = sendData
            };
            if (recvData == null) sendPacket.IsNeedReturn = false;
            bool ret = SendData(sendPacket, recvPacket);
            recvData = recvPacket.RecvData;
            return ret;
        }

        /// <summary>
        /// 获取电表地址数组
        /// </summary>
        /// <returns></returns>
        protected byte[] GetAddressByte()
        {
            string tmp = "0x";
            if (MeterAddress.Length > 12)
                tmp += MeterAddress.Substring(0, 12);
            else if (MeterAddress.Length == 0)
                tmp += "0";
            else
                tmp += MeterAddress;

            return BitConverter.GetBytes(Convert.ToInt64(tmp, 16));
        }

        #region IMeterProtocol 成员
        /// <summary>
        /// 设置协议
        /// </summary>
        /// <param name="protocol">电能表协议</param>
        /// <returns>设置是否成功</returns>
        public bool SetProtocol(DgnProtocolInfo protocol)
        {
            //合并一下密码以及密码等级
            protocolInfo = protocol;
            return true;
        }

        /// <summary>
        /// 设置数据端口名称
        /// </summary>
        /// <param name="portName">数据端口名称</param>
        public void SetPortName(string portName)
        {
            _PortName = portName;
        }

        /// <summary>
        /// 设置电表地址
        /// </summary>
        /// <param name="address">电表地址</param>
        public void SetMeterAddress(string address)
        {
            MeterAddress = address;
        }

        /// <summary>
        /// 通讯测试
        /// </summary>
        /// <returns>通讯测试结果</returns>
        public virtual bool ComTest()
        {
            return true;
        }

        public virtual Dictionary<string, string> ReadHplcID()
        {
            return new Dictionary<string, string>();
        }

        /// <summary>
        /// 读取电量
        /// </summary>
        /// <param name="energyType"></param>
        /// <param name="tariffType"></param>
        /// <returns></returns>
        public virtual float ReadEnergy(byte energyType, byte tariffType)
        {
            throw new NotImplementedException();
        }

        public virtual float ReadEnergyExtension(byte energyType, byte tariffType, int DecimalDigits)
        {
            throw new NotImplementedException();
        }

        public virtual FloatE ReadEnergyR(byte energyType, byte tariffType)
        {
            throw new NotImplementedException();
        }

        public virtual FloatE ReadEnergyExtensionR(byte energyType, byte tariffType, int DecimalDigits)
        {
            throw new NotImplementedException();
        }

        public virtual float ReadDemand(byte energyType, byte tariffType)
        {
            throw new NotImplementedException();
        }

        public virtual float ReadData(string dataFlag, int len, int dot)
        {
            return 0f;
            throw new NotImplementedException();
        }

        public virtual string ReadData(string dataFlag, int len)
        {
            return "";
        }

        public virtual string ReadData(string dataFlag, int len, string item)
        {
            throw new NotImplementedException();
        }


        public virtual byte[] ReadData(byte[] sendData)
        {
            throw new NotImplementedException();
        }


        public virtual string[] ReadDataBlock(string dataFlag, int len)
        {
            throw new NotImplementedException();
        }

        public virtual bool WriteData(string dataFlag, byte[] value)
        {
            throw new NotImplementedException();
        }

        public virtual bool WriteData(string dataFlag, int len, string value)
        {
            throw new NotImplementedException();
        }

        public virtual bool WriteData(string dataFlag, int len, int dot, float value)
        {
            throw new NotImplementedException();
        }

        public virtual bool WriteData(string dataFlag, int len, string[] value)
        {
            throw new NotImplementedException();
        }

        public virtual bool ChangePassword(int clas, string oldPws, string newPsw)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public virtual bool FreezeCmd(string dateHour)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public virtual string ReadAddress()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public virtual bool ClearEventLog(string dataFlag)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        /// <summary>
        /// 清空需量
        /// </summary>
        /// <param name="strEndata">密文</param>
        /// <returns></returns>
        public virtual bool ClearEventLog(string dataFlag, string endata)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public virtual bool WriteAddress(string address)
        {
            throw new Exception("The method or operation is not implemented.");
        }
        public virtual bool WriteAddress(string address, out string rAddr)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public virtual bool BroadcastTime(DateTime broadCaseTime)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public virtual string[] ReadPeriodTime()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public virtual bool ChangeSetting(string setting)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public virtual float[] ReadDemand(byte energyType)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public virtual float[] ReadDataBlock(string dataFlag, int len, int dot)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public virtual bool WriteData(string dataFlag, int len, int dot, float[] value)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public virtual bool WritePeriodTime(string[] pTime)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public virtual bool SetPulseCom(byte pulse)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public virtual bool ClearDemand()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public virtual bool ClearDemand(string endata)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public virtual bool ClearEnergy()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public virtual bool ClearEnergy(string endata)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        /// <summary>
        /// 钱包初始化
        /// </summary>
        /// <param name="str_Endata">密文</param>
        /// <returns>是否成功</returns>
        public virtual bool InitPurse(string endata)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public virtual float[] ReadEnergy(byte energyType)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public virtual float[] ReadEnergyExtension(byte energyType, int DecimalDigits)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public virtual DateTime ReadDateTime()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public virtual bool WriteDateTime(string dateTime)
        {
            throw new Exception("The method or operation is not implemented.");
        }
        public virtual bool WriteDateTime()
        {
            throw new Exception("The method or operation is not implemented.");
        }
        #endregion

        /// <summary>
        /// 密钥下装指令
        /// </summary>
        /// <param name="cmd">命令字</param>
        /// <param name="data">数据域</param>
        /// <param name="sequela">是否有后续帧</param>
        /// <param name="revData">返回帧数据域</param>
        /// <returns></returns>
        public virtual bool UpdateRemoteEncryptionCommand(byte cmd, byte[] data, ref bool sequela, ref byte[] revData)
        {
            //bln_Sequela = false;
            //byt_RevDataF = null;
            return false;
            //return false;
            //throw new Exception("The method or operation is not implemented.");
        }
        public override string ToString()
        {
            if (protocolInfo != null)
                return protocolInfo.ToString();
            return GetHashCode().ToString();
        }

        #region IMeterProtocol 成员

        public virtual float[] ReadEnergys(byte energyType, int freezeTimes)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IMeterProtocol 成员


        public virtual bool ReadSpecialEnergy(int dLType, int times, ref float[] curDL)
        {
            throw new NotImplementedException();
        }

        public virtual bool ReadPatternWord(int type, int patternType, ref string patternWord)
        {
            throw new NotImplementedException();
        }


        public virtual bool ReadFreezeTime(int freezeType, ref string freezeTime)
        {
            throw new NotImplementedException();
        }

        public virtual bool WritePatternWord(int type, string data)
        {
            throw new NotImplementedException();
        }
        #endregion

        public virtual bool WriteRatesPrice(string dataFlag, byte[] value)
        {
            return false;
            throw new NotImplementedException();
        }

        /// <summary>
        /// 读取数据    698协议
        /// </summary>
        /// <param name="oad">对象操作符</param>
        /// <param name="mode">参数安全模式</param>
        /// <param name="LstObj">返回内容列表</param>
        public virtual bool ReadData(StPackParas DataInfo, ref List<object> LstObj, ref List<object> reportdata, ref int errCode)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        /// <summary>
        /// 读取记录数据    698协议
        /// </summary>
        /// <param name="oad">对象操作符</param>
        /// <param name="mode">参数安全模式</param>
        /// <param name="LstObj">返回内容列表</param>
        public virtual bool ReadRecordData(StPackParas DataInfo, ref List<object> LstCsd, ref Dictionary<string, List<object>> DicObj, int recordNo, List<string> rcsd, ref int intErrCode)
        {
            throw new Exception("The method or operation is not implemented.");
        }


        /// <summary>
        /// 操作组帧
        /// </summary>        
        /// <param name="DataInfo">参数信息</param>
        /// <returns>帧长度</returns>
        public virtual bool Operation(StPackParas DataInfo, ref List<object> LstObj, ref List<object> reportdata, ref int errCode)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        /// <summary>
        /// 分帧操作，针对密钥下装，必须分帧
        /// </summary>
        /// <param name="frameDatas">帧数据</param>
        /// <param name="DataInfo">参数</param>
        /// <returns>帧长</returns>
        public virtual bool OperationSubFrame(StPackParas DataInfo, ref List<object> LstObj, ref List<object> reportdata, ref int errCode)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        /// <summary>
        /// 读取安全模式参数
        /// </summary>
        /// <returns></returns>
        public virtual bool SecurityParameter(byte state)
        {
            return true;
        }

        public virtual bool AppConnection(StConnectMechanismInfo ConnectInfo, ref List<object> LstObj, ref List<object> reportdata, ref int intErrCode)
        {
            return true;
        }

        #region 物联网表--多芯表
        /// <summary>
        ///  物联网表--复位
        /// </summary>
        /// <returns></returns>
        public virtual bool IOTMete_Reset()
        {
            return true;
        }







        /// <summary>
        ///物联网表-- 连接电表
        /// </summary>
        /// <param name="Address">表地址</param>
        /// <returns></returns>
        public virtual bool IOTMete_Connect(string Address, string ping)
        {
            return true;
        }
        /// <summary>
        ///  物联网表--预处理--需要等待35秒以上
        /// </summary>
        /// <returns></returns>
        public virtual bool IOTMete_Pretreatment()
        {
            return true;
        }

        /// <summary>
        ///  物联网表--预处理-状态查询
        /// </summary>
        /// <returns></returns>
        public virtual bool IOTMete_PretreatmentSelect()
        {
            return true;
        }
        /// <summary>
        ///  物联网表--切换到光脉冲
        /// </summary>
        /// <param name="pulseType">脉冲类型</param>
        /// <returns></returns>
        public virtual bool IOTMete_SetLightPulse(byte pulseType)
        {
            return true;
        }
        /// <summary>
        ///   切换电表到检定模式--参数：脉冲类型，通道数量，发射频率，频段
        /// </summary>
        /// <param name="pulseType">脉冲类型</param>
        /// <param name="powerGear">功率挡位</param>
        /// <param name="powerGear">频段00全频段。01带内频段，02带外频段</param>
        /// <param name="powerGear">通道数量</param>
        /// <returns></returns>
        public virtual byte IOTMete_SetMeterTestModel(byte pulseType, byte powerGear, byte frequencyBand, byte passagewayCount)
        {
            return 0x00;
        }

        /// <summary>
        ///  切换转换器到检定模式---脉冲类型，发射功率，通信模式
        /// </summary>
        /// <param name="pulseType">脉冲类型</param>
        /// <param name="powerGear">功率挡位</param>
        /// <param name="model">模式--00普通检定模式--10脉冲跟随模式</param>
        /// <returns></returns>
        public virtual bool IOTMete_SetConverterModel(byte pulseType, byte powerGear, byte model)
        {
            return true;
        }




        #endregion
    }
}
