using LYTest.Core.Function;
using LYTest.Core.Model.DnbModel;
using LYTest.Core.Model.DnbModel.DnbInfo;
using System;
using System.Collections.Generic;

namespace LYTest.Core.Model.Meter
{
    [Serializable()]
    /// <summary>
    /// 检定表的数据
    /// </summary>
    public class TestMeterInfo
    {

        #region MyRegion

        /// <summary>
        /// 到货批次号
        /// </summary>
        public string BatchNo { get; set; }
        /// <summary>
        /// 46工单号
        /// </summary>
        /// 
        public string WorkNo { get; set; }

        /// <summary>
        /// 表id，用户数据库查找
        /// </summary>
        public string Meter_ID { get; set; }

        /// <summary>
        /// 表号，用于加密参数 
        /// </summary>
        public string MD_MeterNo { get; set; }

        /// <summary>
        /// 表位号
        /// </summary>
        public int MD_Epitope { get; set; }

        /// <summary>
        /// 费控类型:0-远程,1-本地,2-不带费空
        /// </summary>
        public int FKType { get; set; }
        /// <summary>
        /// 电压
        /// </summary>
        public float MD_UB { get; set; }
        /// <summary>
        /// 电流
        /// </summary>
        public string MD_UA { get; set; }

        /// <summary>
        /// 频率
        /// </summary>
        public int MD_Frequency { get; set; }

        /// <summary>
        /// 首检还是周检
        /// </summary>
        public string MD_TestModel { get; set; }

        /// <summary>
        /// 全检抽检
        /// </summary>
        public string MD_TestType { get; set; }

        /// <summary>
        /// 测量方式--单相-三相三线-三相四线
        /// </summary>
        public string MD_WiringMode { get; set; }
        /// <summary>
        /// 互感器(直接式-互感式)
        /// </summary>
        public string MD_ConnectionFlag { get; set; }

        /// <summary>
        /// 检定规程
        /// </summary>
        public string MD_JJGC { get; set; }


        /// <summary>
        /// 止逆器--有止逆,无止逆
        /// </summary>
        public string MD_CheckDevice { get; set; }
        /// <summary>
        /// 条形码
        /// </summary>
        public string MD_BarCode { get; set; }
        /// <summary>
        /// 资产编号
        /// </summary>
        public string MD_AssetNo { get; set; }

        /// <summary>
        /// 表类型--电子式--智能式
        /// </summary>
        public string MD_MeterType { get; set; }

        /// <summary>
        /// 常数
        /// </summary>
        public string MD_Constant { get; set; }
        /// <summary>
        /// 等级
        /// </summary>
        public string MD_Grane { get; set; }

        /// <summary>
        /// 表型号
        /// </summary>
        public string MD_MeterModel { get; set; }

        /// <summary>
        /// 通讯协议
        /// </summary>
        public string MD_ProtocolName { get; set; }

        /// <summary>
        /// 载波协议
        /// </summary>
        public string MD_CarrName { get; set; }

        /// <summary>
        /// 通讯地址
        /// </summary>
        public string MD_PostalAddress { get; set; }

        /// <summary>
        /// 制造厂家
        /// </summary>
        public string MD_Factory { get; set; }

        /// <summary>
        /// 送检单位
        /// </summary>
        public string MD_Customer { get; set; }
        /// <summary>
        /// 任务编号
        /// </summary>
        public string MD_TaskNo { get; set; }

        /// <summary>
        /// 出厂编号
        /// </summary>
        public string MD_MadeNo { get; set; }
        /// <summary>
        /// 类别：10 智能表，13 物联电能表
        /// </summary>
        public string MD_Sort { get; set; }
        /// <summary>
        /// 证书编号
        /// </summary>
        public string MD_CertificateNo { get; set; }
        /// <summary>
        /// 下发下载的表标记1，自动下载的要上传
        /// </summary>
        public string MD_UPDOWN { get; set; }
        /// <summary>
        /// 是否要检
        /// </summary>
        public bool YaoJianYn { get; set; }
        /// <summary>
        /// 备用1--存放是否需要上传数据--仅自动化线使用，下载数据时赋值
        /// </summary>
        public string Other1 { get; set; }
        /// <summary>
        ///  备用2--存放上传标识--未上传和已上传
        /// </summary>
        public string Other2 { get; set; }
        /// <summary>
        /// 备用3--脉冲类型--蓝牙脉冲光电脉冲
        /// </summary>
        public string Other3 { get; set; }
        /// <summary>
        /// 备用4--预处理状态--未处理(进行蓝牙预处理-如果成功改成已处理，失败改成未授权)-已处理-未授权
        /// </summary>
        public string Other4 { get; set; }
        /// <summary>
        /// 备用5[检定依据]
        /// </summary>
        public string Other5 { get; set; }
        /// <summary>
        /// 铅封1
        /// </summary>
        public string Seal1 { get; set; }

        /// <summary>
        /// 铅封2
        /// </summary>
        public string Seal2 { get; set; }

        /// <summary>
        /// 铅封3
        /// </summary>
        public string Seal3 { get; set; }

        /// <summary>
        /// 37铅封号4
        /// </summary>
        public string Seal4 { get; set; }
        /// <summary>
        /// 38铅封号5 不要占用铅封
        /// </summary>
        public string Seal5 { get; set; }

        /// <summary>
        ///  表位在托盘内的ID
        /// </summary>
        public int Meter_TrayTotalID { get; set; }
        /// <summary>
        /// 保存系统编号
        /// </summary>
        public string Meter_SysNo { get; set; }

        #endregion


        #region 密钥属性

        #region 物联表加特殊参数
        /// <summary>
        /// 表随机数
        /// </summary>
        public string Rand
        {
            get
            {
                if (IsMeteringCore) return ITO_rand;
                else return rand;
            }
            set
            {
                if (IsMeteringCore) ITO_rand = value;
                else rand = value;
            }
        }

        /// <summary>
        /// 会话密钥,只用在DLT698密钥
        /// </summary>
        public string SessionKey
        {
            get
            {
                if (IsMeteringCore) return ITO_sessionKey;
                else return sessionKey;
            }
            set
            {
                if (IsMeteringCore) ITO_sessionKey = value;
                else sessionKey = value;
            }
        }

        /// <summary>
        /// 会话计数器 698用
        /// </summary>
        public string SessionNo
        {
            get
            {
                if (IsMeteringCore) return ITO_sessionNo;
                else return sessionNo;
            }
            set
            {
                if (IsMeteringCore) ITO_sessionNo = value;
                else sessionNo = value;
            }
        }
        /// <summary>
        /// ESAM序列号
        /// </summary>
        public string EsamId
        {
            get
            {
                if (IsMeteringCore) return ITO_esamId;
                else return esamId;
            }
            set
            {
                if (IsMeteringCore) ITO_esamId = value;
                else esamId = value;
            }
        }
        /// <summary>
        /// ESAM密钥信息
        /// </summary>
        public string EsamKey
        {
            get
            {
                if (IsMeteringCore) return ITO_esamKey;
                else return esamKey;
            }
            set
            {
                if (IsMeteringCore) ITO_esamKey = value;
                else esamKey = value;
            }
        }
        /// <summary>
        /// ESAM密钥状态[私钥,公钥]: 0-公钥,1-私钥,2-未知
        /// </summary>
        public int EsamStatus
        {
            get
            {
                if (IsMeteringCore) return ITO_esamStatus;
                else return esamStatus;
            }
            set
            {
                if (IsMeteringCore) ITO_esamStatus = value;
                else esamStatus = value;
            }
        }

        private string sessionNo = "";
        private string ITO_sessionNo = "";

        private string esamId = "";
        private string ITO_esamId = "";

        private string esamKey;
        private string ITO_esamKey;

        private int esamStatus;
        private int ITO_esamStatus;

        private string sessionKey;
        private string ITO_sessionKey;

        private string rand;
        private string ITO_rand;


        /// <summary>
        /// 是否是计量芯片
        /// </summary>
        public bool IsMeteringCore = false;
        #endregion


        #endregion

        /// <summary>
        /// 表位端口信息
        /// </summary>
        public ComPortInfo ProtInfo { get; set; }

        /// <summary>
        /// 电能表多功能通信配置协议
        /// </summary>
        public DgnProtocolInfo DgnProtocol { get; set; }


        //private object obj;
        /// <summary>
        /// 设备实例
        ///MethodInfo mInfo = type.GetMethod(方法名称); //获取当前方法
        /// mInfo.Invoke(type, value);  //接收调用返回值，判断调用是否成功  new object[1] {5}
        /// </summary>
        public object Obj { get; set; }


        //private Type type;
        /// <summary>
        /// 设备 类型
        ///MethodInfo mInfo = type.GetMethod(方法名称); //获取当前方法
        /// mInfo.Invoke(type, value);  //接收调用返回值，判断调用是否成功  new object[1] {5}
        /// </summary>
        public Type Type { get; set; }




        #region 公用
        /// <summary>
        /// 获取表常数 
        /// </summary>
        /// <returns>[有功，无功]</returns>
        public int[] GetBcs()
        {
            MD_Constant = MD_Constant.Replace("（", "(").Replace("）", ")");

            if (MD_Constant.Trim().Length < 1)
            {
                return new int[] { 1, 1 };
            }

            string[] arTmp = MD_Constant.Trim().Replace(")", "").Split('(');

            if (arTmp.Length == 1)
            {
                if (Number.IsNumeric(arTmp[0]))
                    return new int[] { int.Parse(arTmp[0]), int.Parse(arTmp[0]) };
                else
                    return new int[] { 1, 1 };
            }
            else
            {
                if (Number.IsNumeric(arTmp[0]) && Number.IsNumeric(arTmp[1]))
                    return new int[] { int.Parse(arTmp[0]), int.Parse(arTmp[1]) };
                else
                    return new int[] { 1, 1 };
            }
        }




        /// <summary>
        /// 获取电流
        /// </summary>
        /// <returns>[最小电流,最大电流]</returns>
        public float[] GetIb()
        {
            MD_UA = MD_UA.Replace("（", "(").Replace("）", ")");

            if (MD_UA.Trim().Length < 1)
            {
                return new float[] { 1, 1 };
            }

            string[] arTmp = MD_UA.Trim().Replace(")", "").Split('(');

            //add yjt 20220303 判断IRl46表的电流
            //string[] IR46Itr = new string[2];


            //if (arTmp.Length == 1)
            //{
            //    if (Number.IsNumeric(arTmp[0]))
            //        return new float[] { float.Parse(arTmp[0]), float.Parse(arTmp[0]) };
            //    else
            //        return new float[] { 1, 1 };
            //}
            //else
            //{
            //    if (Number.IsNumeric(arTmp[0]) && Number.IsNumeric(arTmp[1]))
            //        return new float[] { float.Parse(arTmp[0]), float.Parse(arTmp[1]) };
            //    else
            //        return new float[] { 1, 1 };
            //}

            if (arTmp.Length == 1)
            {
                if (MD_UA.IndexOf("-") != -1)
                {
                    string[] IR46Itr = arTmp[0].Split('-');

                    if (Number.IsNumeric(IR46Itr[0]) && Number.IsNumeric(IR46Itr[1]))
                        return new float[] { float.Parse(IR46Itr[0]), float.Parse(IR46Itr[1]) };
                    else
                        return new float[] { 1, 1 };
                }
                else
                {
                    if (Number.IsNumeric(arTmp[0]))
                        return new float[] { float.Parse(arTmp[0]), float.Parse(arTmp[0]) };
                    else
                        return new float[] { 1, 1 };
                }
            }
            else
            {
                if (MD_UA.IndexOf("-") != -1)
                {
                    string[] IR46Itr = arTmp[0].Split('-');

                    if (Number.IsNumeric(IR46Itr[0]) && Number.IsNumeric(IR46Itr[1]) && Number.IsNumeric(arTmp[1]))
                        return new float[] { float.Parse(IR46Itr[1]), float.Parse(arTmp[1]), float.Parse(IR46Itr[0]) };
                    else
                        return new float[] { 1, 1, 1 };
                }
                else
                {

                }
                if (Number.IsNumeric(arTmp[0]) && Number.IsNumeric(arTmp[1]))
                    return new float[] { float.Parse(arTmp[0]), float.Parse(arTmp[1]) };
                else
                    return new float[] { 1, 1 };
            }
        }


        #endregion


        #region 基本信息
        /// <summary>
        /// 检定日期[YYYY-MM-DD HH:NN:SS]
        /// </summary>
        public string VerifyDate { get; set; }

        //add yjt 20220222 新增计检日期
        /// <summary>
        /// 计检日期[YYYY-MM-DD HH:NN:SS] 有效日期
        /// </summary>
        public string ExpiryDate { get; set; }

        /// <summary>
        /// 台体编号
        /// </summary>
        public string BenthNo { get; set; }

        /// <summary>
        /// 湿度
        /// </summary>
        public string Humidity { get; set; }
        /// <summary>
        /// 温度
        /// </summary>
        public string Temperature { get; set; }
        /// <summary>
        /// 检验员
        /// </summary>
        public string Checker1 { get; set; }
        /// <summary>
        /// 核验员
        /// </summary>
        public string Checker2 { get; set; }

        //add yjt 20220222 新增主管，主管人员编号，检验员编号，核验员编号，送检单位编号
        /// <summary>
        /// 主管
        /// </summary>
        public string Manager { get; set; }
        /// <summary>
        /// 主管人员编号
        /// </summary>
        public string ManagerNo { get; set; }
        /// <summary>
        /// 检验员编号
        /// </summary>
        public string CheckerNo1 { get; set; }
        /// <summary>
        /// 核验员编号
        /// </summary>
        public string CheckerNo2 { get; set; }
        /// <summary>
        /// 送检单位编号
        /// </summary>
        public string MD_CustomerNo { get; set; }
        #endregion


        #region 一块表检定数据模型


        /// <summary>
        /// 表的检定结论
        /// </summary>
        public string Result { get; set; }

        /// <summary>
        /// 电能表测试原始数据集； Key值为项目Prj_ID值
        /// </summary>
        public Dictionary<string, MeterTestData> MeterTestDatas = new Dictionary<string, MeterTestData>();

        /// <summary>
        /// 电能示值误差数据集； Key值为项目Prj_ID值
        /// </summary>
        public Dictionary<string, MeterRegister> MeterRegs = new Dictionary<string, MeterRegister>();
        /// <summary>
        /// 电能表走字数据误差集；Key值为Prj_ID
        /// </summary>
        public Dictionary<string, MeterZZError> MeterZZErrors = new Dictionary<string, MeterZZError>();
        /// <summary>
        /// 电能表误差集合Key值为项目Prj_ID值，由于特殊检定部分被T出去单独建结构所以不会出现关键字重复的情况 
        /// </summary>
        public Dictionary<string, MeterError> MeterErrors = new Dictionary<string, MeterError>();
        /// <summary>
        /// 潜动启动数据；Key值为项目Prj_ID值
        /// </summary>
        public Dictionary<string, MeterQdQid> MeterQdQids = new Dictionary<string, MeterQdQid>();
        /// <summary>
        /// 电能表多功能数据集； Key值为项目Prj_ID值
        /// </summary>
        public Dictionary<string, MeterDgn> MeterDgns = new Dictionary<string, MeterDgn>();
        /// <summary>
        /// 电能表结论集；Key值为检定项目ID编号格式化字符串。格式为[检定项目ID号]参照数据库结构设计文档中附2
        /// </summary>
        public Dictionary<string, MeterResult> MeterResults = new Dictionary<string, MeterResult>();
        /// <summary>
        /// 费控数据；Key值为项目Prj_ID值
        /// </summary>
        public Dictionary<string, MeterFK> MeterCostControls = new Dictionary<string, MeterFK>();
        /// <summary>
        /// 电能表误差一致性集；Key值为项目Prj_ID值
        /// </summary>
        public Dictionary<string, MeterErrAccord> MeterErrAccords = new Dictionary<string, MeterErrAccord>();
        /// <summary>
        /// 规约一致性数据
        /// </summary>
        public Dictionary<string, MeterDLTData> MeterDLTDatas = new Dictionary<string, MeterDLTData>();

        //add yjt 20220306 新增结论集
        /// <summary>
        /// 默认结论集  外观检查，工频耐压试验，显示功能
        /// </summary>
        public Dictionary<string, MeterDefault> MeterDefaults = new Dictionary<string, MeterDefault>();

        //add yjt 20220310 新增结论集
        /// <summary>
        /// 耐压数据集
        /// </summary>
        public Dictionary<string, MeterInsulation> MeterInsulations = new Dictionary<string, MeterInsulation>();
        /// <summary>
        /// 数据显示功能；Key值为项目Prj_ID值
        /// </summary>
        public Dictionary<string, MeterShow> MeterShows = new Dictionary<string, MeterShow>();

        //add jx 20221008 新增结结论集合 黑龙江哈尔滨
        /// <summary>
        /// 通信性能检测(HPLC)数据
        /// </summary>
        public Dictionary<string, MeterCommunicationHPLC> MeterCommunicationHPLC = new Dictionary<string, MeterCommunicationHPLC>();
        /// <summary>
        /// 芯片ID认证
        /// </summary>
        public Dictionary<string, MeterHPLCIDAuthen> MeterHPLCIDAuthen = new Dictionary<string, MeterHPLCIDAuthen>();
        /// <summary>
        /// 电表清零数据
        /// </summary>
        public Dictionary<string, MeterClearEnerfy> MeterClearEnerfy = new Dictionary<string, MeterClearEnerfy>();
        /// <summary>
        /// 时钟示值误差数据
        /// </summary>
        public Dictionary<string, MeterClockError> MeterClockError = new Dictionary<string, MeterClockError>();
        /// <summary>
        /// 电能示值组合误差数据
        /// </summary>
        public Dictionary<string, MeterEnergyError> MeterEnergyError = new Dictionary<string, MeterEnergyError>();
        /// <summary>
        /// 接线检查数据
        /// </summary>
        public Dictionary<string, MeterPreWiring> MeterPreWiring = new Dictionary<string, MeterPreWiring>();


        //add yjt 20221121 新增剩余全性能的集合 
        /// <summary>
        /// 电能表特殊检定数据误差集 影响量
        /// </summary>
        public Dictionary<string, MeterSpecialErr> MeterSpecialErrs = new Dictionary<string, MeterSpecialErr>();
        /// <summary>
        /// 智能表功能数据集； Key值为项目Prj_ID值
        /// </summary>
        public Dictionary<string, MeterFunction> MeterFunctions = new Dictionary<string, MeterFunction>();
        /// <summary>
        /// 事件记录数据集； Key值为项目Prj_ID值
        /// </summary>
        public Dictionary<string, MeterSjJLgn> MeterSjJLgns = new Dictionary<string, MeterSjJLgn>();
        /// <summary>
        /// 电能表冻结数据集； Key值为项目Prj_ID值
        /// </summary>
        public Dictionary<string, MeterFreeze> MeterFreezes = new Dictionary<string, MeterFreeze>();
        /// <summary>
        /// 负荷记录数据集；Key值为项目prj_ID值
        /// </summary>
        public Dictionary<string, MeterLoadRecord> MeterLoadRecords = new Dictionary<string, MeterLoadRecord>();
        /// <summary>
        /// 电能表功耗测试数据集；key值为项目Md_PrjID值
        /// </summary>
        public Dictionary<string, MeterPower> MeterPowers = new Dictionary<string, MeterPower>();

        /// <summary>
        /// 电能表初始固有误差
        /// </summary>
        public Dictionary<string, MeterInitialError> MeterInitialError = new Dictionary<string, MeterInitialError>();

        //ADD yjt JX 20230205 合并蒋工西安代码新增结结论集合 陕西西安
        /// <summary>
        /// 远程控制
        /// </summary>
        public Dictionary<string, MeterEncryptionControl> MeterEncryptionControl = new Dictionary<string, MeterEncryptionControl>();

        /// <summary>
        /// 远程保电
        /// </summary>
        public Dictionary<string, MeterFKEnPower> MeterFKEnPower = new Dictionary<string, MeterFKEnPower>();

        /// <summary>
        /// 报警功能
        /// </summary>
        public Dictionary<string, MeterFKWarning> MeterFKWarning = new Dictionary<string, MeterFKWarning>();

        #endregion

        ///// <summary>
        ///// 未完，不要调用
        ///// </summary>
        ///// <returns></returns>
        //private bool GetResult()
        //{
        //    foreach (System.Reflection.FieldInfo field in this.GetType().GetFields())
        //    {
        //        if (field.FieldType.Name.Contains("Dictionary"))
        //        {
        //            object obj = field.GetValue(this);
        //            System.Reflection.PropertyInfo piCount = field.FieldType.GetProperty("Count");
        //            object count = piCount.GetValue(obj);
        //            if ((int)count == 0)
        //            {
        //                continue;
        //            }
        //            System.Reflection.PropertyInfo piValues = field.FieldType.GetProperty("Values");
        //            object values = piValues.GetValue(obj);
        //            //TODO:未完
        //            if (values == null)
        //            {

        //            }

        //        }
        //    }
        //    return true;
        //}
    }
}
