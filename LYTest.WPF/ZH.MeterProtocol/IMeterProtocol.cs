using LYTest.Core;
using System;
using System.Collections.Generic;
using LYTest.MeterProtocol.Protocols.DLT698.Struct;

namespace LYTest.MeterProtocol
{
    /// <summary>
    /// 电能表多功能协议接口
    /// </summary>
    public interface IMeterProtocol
    {
        /// <summary>
        /// 身份认证状态:0-公钥，1-私钥
        /// </summary>
        int IdentityStatus { get; set; }



        /// <summary>
        /// 设置通讯端口名称
        /// </summary>
        /// <param name="portName">通讯端口名称</param>
        void SetPortName(string portName);

        /// <summary>
        /// 设置表地址
        /// </summary>
        /// <param name="meterAddress">电表地址</param>
        void SetMeterAddress(string meterAddress);

        /// <summary>
        /// 设置电能表协议
        /// </summary>
        /// <param name="protocol">电能表协议</param>
        /// <returns>设置电能表协议是否成功</returns>
        bool SetProtocol(DgnProtocolInfo protocol);

        /// <summary>
        /// 通信测试
        /// </summary>
        /// <returns>通讯测试是否成功</returns>
        bool ComTest();

        /// <summary>
        /// 广播校时
        /// </summary>
        /// <param name="broadCaseTime">广播校准时间</param>
        /// <returns>广播校时是否成功</returns>
        bool BroadcastTime(DateTime broadCaseTime);

        /// <summary>
        /// 读取电量
        /// </summary>
        /// <param name="energyType">功率类型，0=P+ 1=P- 2=Q+ 3=Q- 4=Q1 5=Q2 6=Q3 6=Q4</param>
        /// <param name="tariffType">费率类型，0-4,顺序根据协议内费率顺序转换,对于Q1 Q2 Q3 Q4本参数无效</param>
        /// <returns>返回电量:-1读失败，0~N.M</returns>
        float ReadEnergy(byte energyType, byte tariffType);
        float ReadEnergyExtension(byte energyType, byte tariffType, int DecimalDigits);
        FloatE ReadEnergyR(byte energyType, byte tariffType);
        FloatE ReadEnergyExtensionR(byte energyType, byte tariffType, int DecimalDigits);


        /// <summary>
        /// 读取电量(所有费率读取)
        /// </summary>
        /// <param name="energyType">功率类型，0=P+ 1=P- 2=Q+ 3=Q- 4=Q1 5=Q2 6=Q3 6=Q4</param>
        /// <returns>返回电量,当energyType小于4时，返回一个长度为5的数组，反之返回一个长度为1的数组</returns>
        float[] ReadEnergys(byte energyType, int freezeTimes);

        /// <summary>
        /// 读取电量(所有费率读取)
        /// </summary>
        /// <param name="energyType">功率类型，0=P+ 1=P- 2=Q+ 3=Q- 4=Q1 5=Q2 6=Q3 6=Q4</param>
        /// <returns>返回电量,当energyType小于4时，返回一个长度为5的数组，反之返回一个长度为1的数组</returns>
        float[] ReadEnergy(byte energyType);
        float[] ReadEnergyExtension(byte energyType, int DecimalDigits);

        /// <summary>
        /// 读取需量(分费率读取)
        /// </summary>
        /// <param name="energyType">功率类型，0=P+ 1=P- 2=Q+ 3=Q- 4=Q1 5=Q2 6=Q3 6=Q4</param>
        /// <param name="tariffType">费率类型，0=总，1=峰，2=平，3=谷，4=尖</param>
        /// <returns>返回需量</returns>
        float ReadDemand(byte energyType, byte tariffType);


        /// <summary>
        /// 读取需量(所有费率读取)
        /// </summary>
        /// <param name="ept_DirectType">功率类型，0=P+ 1=P- 2=Q+ 3=Q- 4=Q1 5=Q2 6=Q3 6=Q4</param>
        /// <returns>返回需量</returns>
        float[] ReadDemand(byte energyType);

        /// <summary>
        /// 读日期时间
        /// </summary>
        /// <returns>读取到的电表时间</returns>
        DateTime ReadDateTime();


        /// <summary>
        /// 读地址
        /// </summary>
        /// <returns>返回地址</returns>
        string ReadAddress();

        /// <summary>
        /// 读取时段
        /// </summary>
        /// <returns>时段表</returns>
        string[] ReadPeriodTime();


        /// <summary>
        /// 读取数据（数据型，数据项）
        /// </summary>
        /// <param name="dataFlag">标识符,2个字节</param>
        /// <param name="len">数据长度(字节数)</param>
        /// <param name="dot">小数位</param>
        /// <returns>返回数据</returns>
        float ReadData(string dataFlag, int len, int dot);

        /// <summary>
        /// 读取数据（字符型，数据项）
        /// </summary>
        /// <param name="dataFlag">标识符,2个字节</param>
        /// <param name="len">数据长度(字节数)</param>
        /// <returns></returns>
        string ReadData(string dataFlag, int len);

        /// <summary>
        /// 读取数据（字符型，数据项）
        /// </summary>
        /// <param name="dataFlag">标识符,两个字节</param>
        /// <param name="len">数据长度(字节数)</param>
        /// <param name="str_Value">返回数据</param>
        /// <returns></returns>
        string ReadData(string dataFlag, int len, string item);


        /// <summary>
        /// 读取数据（字符型）
        /// </summary>
        /// <param name="sendData">发送侦</param>
        /// <returns></returns>
        byte[] ReadData(byte[] sendData);


        /// <summary>
        /// 读取数据（数据型，数据块）
        /// </summary>
        /// <param name="str_ID">标识符,2个字节</param>
        /// <param name="int_Len">数据长度(字节数)</param>
        /// <param name="int_Dot">小数位</param>
        /// <returns>返回数据</returns>
        float[] ReadDataBlock(string str_ID, int int_Len, int int_Dot);

        /// <summary>
        /// 读取数据（字符型，数据块）
        /// </summary>
        /// <param name="str_ID">标识符,2个字节</param>
        /// <param name="int_Len">数据长度(字节数)</param>
        /// <returns>返回数据</returns>
        string[] ReadDataBlock(string str_ID, int int_Len);




        /// <summary>
        /// 写地址
        /// </summary>
        /// <param name="address">写入地址</param>
        /// <returns>写地址结果</returns>
        bool WriteAddress(string address);
        bool WriteAddress(string address, out string rAddr);

        /// <summary>
        /// 写日期时间
        /// </summary>
        /// <param name="dateTime">日期时间</param>
        /// <returns>是否成功</returns>
        bool WriteDateTime(string dateTime);
        bool WriteDateTime();

        /// <summary>
        /// 写时段
        /// </summary>
        /// <param name="periodTime">时段组</param>
        /// <returns>是否成功</returns>
        bool WritePeriodTime(string[] periodTime);

        /// <summary>
        /// 写费率1
        /// </summary>
        /// <param name="dataFlag">数据标识</param>
        /// <param name="value"></param>
        /// <returns></returns>
        bool WriteRatesPrice(string dataFlag, byte[] value);

        /// <summary>
        /// 写数据
        /// </summary>
        /// <param name="dataFlag">标识符</param>
        /// <param name="value">写入数据</param>
        /// <returns>是否成功</returns>
        bool WriteData(string dataFlag, byte[] value);

        /// <summary>
        /// 写数据(字符型，数据项)
        /// </summary>
        /// <param name="dataFlag">数据标识符,两个字节</param>
        /// <param name="int_Len">数据长度(块中每项字节数)</param>
        /// <param name="data">写入数据</param>
        /// <returns>是否成功</returns>
        bool WriteData(string dataFlag, int int_Len, string data);


        /// <summary>
        /// 写数据(数据型，数据项)
        /// </summary>
        /// <param name="dataFlag">标识符</param>
        /// <param name="len">数据长度(字节数)</param>
        /// <param name="dot">小数位</param>
        /// <param name="value">写入数据</param>
        /// <returns>是否成功</returns>
        bool WriteData(string dataFlag, int len, int dot, float value);


        /// <summary>
        /// 写数据(字符型，数据块)
        /// </summary>
        /// <param name="dataFlag">标识符</param>
        /// <param name="int_Len">数据长度(字节数)</param>
        /// <param name="str_Value">写入数据</param>
        /// <returns>是否成功</returns>
        bool WriteData(string dataFlag, int int_Len, string[] str_Value);


        /// <summary>
        /// 写数据(数据型，数据块)
        /// </summary>
        /// <param name="dataFlag">标识符</param>
        /// <param name="len">数据长度(字节数)</param>
        /// <param name="dot">小数位</param>
        /// <param name="str_Value">写入数据</param>
        /// <returns>是否成功</returns>
        bool WriteData(string dataFlag, int len, int dot, float[] value);


        /// <summary>
        /// 清空需量
        /// </summary>
        /// <returns>是否成功</returns>
        bool ClearDemand();
        /// <summary>
        /// 清空需量
        /// </summary>
        /// <param name="endata">密文</param>
        /// <returns>是否成功</returns>
        bool ClearDemand(string endata);
        /// <summary>
        /// 清空电量
        /// </summary>
        /// <returns>是否成功</returns>
        bool ClearEnergy();
        /// <summary>
        /// 清空电量
        /// </summary>
        /// <param name="endata">密文</param>
        /// <returns>是否成功</returns>
        bool ClearEnergy(string endata);

        /// <summary>
        /// 钱包初始化
        /// </summary>
        /// <param name="endata">密文</param>
        /// <returns>是否成功</returns>
        bool InitPurse(string endata);

        /// <summary>
        /// 清空事件记录
        /// </summary>
        /// <param name="dataFlag">事件清零内容</param>
        /// <returns>是否成功</returns>
        bool ClearEventLog(string dataFlag);

        /// <summary>
        /// 清空事件记录
        /// </summary>
        /// <param name="dataFlag">事件清零内容</param>
        /// <param name="endata">密文</param>
        /// <returns></returns>
        bool ClearEventLog(string dataFlag, string endata);

        /// <summary>
        /// 设置脉冲端子
        /// </summary>
        /// <param name="ecp_PulseType">端子输出脉冲类型</param>
        /// <returns>设置是否成功</returns>
        bool SetPulseCom(byte ecp_PulseType);


        /// <summary>
        /// 冻结命令
        /// </summary>
        /// <param name="str_DateHour">冻结时间，MMDDhhmm(月.日.时.分)</param>
        /// <returns></returns>
        bool FreezeCmd(string str_DateHour);


        /// <summary>
        /// 更改波特率
        /// </summary>
        /// <param name="str_Setting">波特率</param>
        /// <returns></returns>
        bool ChangeSetting(string str_Setting);


        /// <summary>
        /// 更改密码
        /// </summary>
        /// <param name="int_Class">密码等级，即修改几级的密码</param>
        /// <param name="str_OldPws">旧密码,如果是更高等级修改本等级密码则需加更高等级，原密码则不包含等级</param>
        /// <param name="str_NewPsw">新密码,不包含等级</param>
        /// <returns>是否成功</returns>
        bool ChangePassword(int int_Class, string str_OldPws, string str_NewPsw);

        /// <summary>
        /// 密钥下装指令
        /// </summary>
        /// <param name="cmd">命令字</param>
        /// <param name="data">数据域</param>
        /// <param name="sequela">是否有后续帧</param>
        /// <param name="revData">返回帧数据域</param>
        /// <returns></returns>
        bool UpdateRemoteEncryptionCommand(byte cmd, byte[] data, ref bool sequela, ref byte[] revData);



        /// <summary>
        /// 读取特殊电量
        /// </summary>
        /// <param name="int_type">读取类型0=块读，1=分项读</param>
        /// <param name="int_DLType">读取类型 1=剩余电量，2=透支电量，3=(上1次)定时冻结正向有功电能,4=(上1次)日冻结正向有功电能
        /// 5=(上1次)整点冻结正向有功总电能,6=(上1次)瞬时冻结正向有功电能</param>
        /// <param name="int_Times">第几次</param>
        /// <param name="str_PatternWord">返回特殊电量</param>
        /// <returns></returns>
        bool ReadSpecialEnergy(int int_DLType, int int_Times, ref float[] flt_CurDL);


        /// <summary>
        /// 读取冻结模式字
        /// </summary>
        /// <param name="int_type">读取类型0=块读，1=分项读</param>
        /// <param name="patternType">模式字类型1=定时冻结，2=瞬时冻结，3=日冻结，4=约定冻结，5=整点冻结</param>
        /// <param name="patternWord">返回冻结模式字</param>
        /// <returns></returns>
        bool ReadPatternWord(int int_type, int patternType, ref string patternWord);

        //模式字类型1=定时冻结，2=瞬时冻结，4=约定冻结，5=整点冻结，3=日冻结
        bool WritePatternWord(int patternType, string data);

        /// <summary>
        /// 读取上一次冻结时间
        /// </summary>
        /// <param name="int_Type">类型0=块读(hhmmNN)，1=块读(NNhhmm)</param>
        /// <param name="int_FreezeType">类型 1=整点冻结时间，2=日冻结时间，3=定时冻结时间,4=整点冻结起始时间</param>
        /// <param name="str_PTime">返回时间</param>
        /// <returns></returns>
        bool ReadFreezeTime(int int_FreezeType, ref string str_FreezeTime);


        /// <summary>
        /// 读取数据    698协议
        /// </summary>
        /// <param name="oad">对象操作符</param>
        /// <param name="mode">参数安全模式</param>
        /// <param name="LstObj">返回内容列表</param>
        bool ReadData(StPackParas DataInfo, ref List<object> LstObj, ref List<object> reportdata, ref int errCode);

        /// <summary>
        /// 读取记录数据    698协议
        /// </summary>
        /// <param name="oad">对象操作符</param>
        /// <param name="mode">参数安全模式</param>
        /// <param name="LstObj">返回内容列表</param>
        bool ReadRecordData(StPackParas DataInfo, ref List<object> LstCsd, ref Dictionary<string, List<object>> LstObj, int recordNo, List<string> rcsd, ref int errCode);

        /// <summary>
        /// 应用连接请求    698协议
        /// </summary>
        /// <param name="frameData">帧</param>
        /// <param name="MeterAdd">表地址</param>
        /// <param name="ConnectInfo">认证机制信息</param>
        /// <returns>帧长度</returns>
        bool AppConnection(StConnectMechanismInfo ConnectInfo, ref List<object> LstObj, ref List<object> reportdata, ref int intErrCode);


        /// <summary>
        /// 操作组帧
        /// </summary>        
        /// <param name="DataInfo">参数信息</param>
        /// <returns>帧长度</returns>
        bool Operation(StPackParas DataInfo, ref List<object> LstObj, ref List<object> reportdata, ref int intErrCode);

        /// <summary>
        /// 分帧操作，针对密钥下装，必须分帧
        /// </summary>
        /// <param name="frameDatas">帧数据</param>
        /// <param name="DataInfo">参数</param>
        /// <returns>帧长</returns>
        bool OperationSubFrame(StPackParas DataInfo, ref List<object> LstObj, ref List<object> reportdata, ref int intErrCode);

        /// <summary>
        /// 读取安全模式参数
        /// </summary>
        /// <returns></returns>
        bool SecurityParameter(byte state);


        Dictionary<string, string> ReadHplcID();


        #region 物联网表--多芯表
        //复位转换器--也就是恢复到蓝牙
        //连接电表--参数电表地址
        //蓝牙预处理
        //查询预处理状态





        /// <summary>
        ///  物联网表--复位
        /// </summary>
        /// <returns></returns>
        bool IOTMete_Reset();

        /// <summary>
        ///物联网表-- 连接电表
        /// </summary>
        /// <param name="Address">表地址</param>
        /// <returns></returns>
        bool IOTMete_Connect(string Address, string ping);
        /// <summary>
        ///  物联网表--预处理--需要等待35秒以上
        /// </summary>
        /// <returns></returns>
        bool IOTMete_Pretreatment();

        /// <summary>
        ///  物联网表--预处理-状态查询
        /// </summary>
        /// <returns></returns>
        bool IOTMete_PretreatmentSelect();
        /// <summary>
        ///  物联网表--切换到光脉冲
        /// </summary>
        /// <param name="pulseType">脉冲类型</param>
        /// <returns></returns>
        bool IOTMete_SetLightPulse(byte pulseType);
        /// <summary>
        ///   切换电表到检定模式--参数：脉冲类型，通道数量，发射频率，频段
        /// </summary>
        /// <param name="pulseType">脉冲类型</param>
        /// <param name="powerGear">功率挡位</param>
        /// <param name="powerGear">频段00全频段。01带内频段，02带外频段</param>
        /// <param name="powerGear">通道数量</param>
        /// <returns></returns>
        byte IOTMete_SetMeterTestModel(byte pulseType, byte powerGear, byte frequencyBand, byte passagewayCount);

        /// <summary>
        ///  切换转换器到检定模式---脉冲类型，发射功率，通信模式
        /// </summary>
        /// <param name="pulseType">脉冲类型</param>
        /// <param name="powerGear">功率挡位</param>
        /// <param name="model">模式--00普通检定模式--10脉冲跟随模式</param>
        /// <returns></returns>
        bool IOTMete_SetConverterModel(byte pulseType, byte powerGear, byte model);




        #endregion
    }
}
