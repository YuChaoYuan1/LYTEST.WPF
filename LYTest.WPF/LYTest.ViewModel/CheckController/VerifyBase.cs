using LYTest.Core;
using LYTest.Core.Enum;
using LYTest.Core.Function;
using LYTest.Core.Model;
using LYTest.Core.Model.Meter;
using LYTest.Core.Struct;
using LYTest.DAL.Config;
using LYTest.MeterProtocol;
using LYTest.MeterProtocol.Enum;
using LYTest.MeterProtocol.Protocols.DLT698.Enum;
using LYTest.Utility.Log;
using LYTest.ViewModel.CheckController.MulitThread;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace LYTest.ViewModel.CheckController
{
    /// <summary>
    /// 检定的虚方法,所有检定类的基类
    /// </summary>
    public class VerifyBase
    {
        #region 属性
        /// <summary>
        /// 必须重新重新进行身份认证
        /// </summary>
        private static MeterLogicalAddressEnum meterLogicalAddressType = MeterLogicalAddressEnum.管理芯;

        /// 是否检测物联表是否连接,每次调用蓝牙连接时需要将此值改为true
        /// </summary>
        public static bool IsCheckITOMeter = false;
        /// <summary>
        ///当前物联表的脉冲类型--如果发生改变，就需要进行切换脉冲了
        /// </summary>
        public static int ItoControlType = -1;
        /// <summary>
        /// 当前使用通讯芯片
        /// </summary>
        public static MeterLogicalAddressEnum MeterLogicalAddressType
        {
            get { return meterLogicalAddressType; }
            set
            {
                ///值发生改变，说明必须重新身份认证--因为加密随机数--序列号等都不一样
                if (meterLogicalAddressType != value)
                {
                    //add 如果不是物联表 不给切换到芯片
                    if (!ConfigHelper.Instance.IsITOMeter) return;

                    meterLogicalAddressType = value;
                    //IsAgainIdentity = true;

                    switch (MeterLogicalAddressType)
                    {
                        case MeterLogicalAddressEnum.管理芯:
                            LYTest.MeterProtocol.App.LogicalAddress = "05";
                            for (int i = 0; i < MeterInfo.Length; i++)
                                MeterInfo[i].IsMeteringCore = false;
                            break;
                        case MeterLogicalAddressEnum.计量芯:
                            LYTest.MeterProtocol.App.LogicalAddress = "15";
                            for (int i = 0; i < MeterInfo.Length; i++)
                                MeterInfo[i].IsMeteringCore = true;
                            break;
                        default:
                            break;
                    }
                }
            }
        }
        private bool isInitIto;
        /// <summary>
        /// 是否需要复位物联表
        /// </summary>
        public bool IsInitIto
        {
            get { return isInitIto; }
            set
            {
                if (IsInitIto != value)//值一样不需要触发
                {
                    if (!ConfigHelper.Instance.IsITOMeter) return;
                    isInitIto = value;
                    if (IsInitIto)//需要复位的情况
                    {
                        OutITOTestModelInitIto();
                    }
                }
            }
        }



        /// <summary>
        /// 是否是双协议的电表
        /// </summary>
        public static bool IsDoubleProtocol = false;

        public string WcLimitName = "规程误差限";

        public MulitEncryptionWorkThreadManager EncryptionThread;

        /// <summary>
        /// 上一次联接加密机的时间
        /// </summary>
        private static DateTime EncryLastLinkTime { get; set; } = new DateTime();


        /// <summary>
        /// 结论字典
        /// </summary>
        protected Dictionary<string, string[]> ResultDictionary { get; set; } = new Dictionary<string, string[]>();

        /// <summary>
        /// 不合格原因
        /// </summary>
        public string[] NoResoult;

        /// <summary>
        /// 检定点的参数值
        /// </summary>
        public string Test_Value { get; set; }
        /// <summary>
        /// 检定点的参数值描述
        /// </summary>
        public string Test_Format { get; set; }
        /// <summary>
        /// 检定点的编号
        /// </summary>
        public string Test_No { get; set; }
        /// <summary>
        /// 检定点的名字
        /// </summary>
        public string Test_Name { get; set; }
        /// <summary>
        /// 是否默认合格
        /// </summary>
        public bool DefaultValue { get; set; }

        /// <summary>
        /// 是否是演示版本-True是
        /// </summary>
        public bool IsDemo { get; set; }

        //add zxg yjt 20220426 新增
        /// <summary>
        /// 是否需要读取表地址和表号
        /// </summary>
        public static bool ReadMeterAddressAndNo { get; set; }

        //add zxg yjt 20220805 新增是否关电流的全局变量
        /// <summary>
        /// 是否关电流
        /// </summary>
        public static bool IsSwitch_I { get; set; }

        /// <summary>
        /// 获取或设置停止检定状态
        /// </summary>
        public bool Stop { get; set; }
        /// <summary>
        /// 是否调表
        /// </summary>
        public static bool IsMeterDebug { get; set; }
        /// <summary>
        /// 是否已经完成本项目检定
        /// 只有m_Stop=true且m_CheckOver=true时，检定停止操作才算真正完成
        /// </summary>
        protected bool CheckOver = false;
        /// <summary>
        /// 表位数量
        /// </summary>
        public int MeterNumber { get; set; }

        //add yjt jx 20230205 修改设置三色灯的功能合并蒋工代码
        public static EmLightColor LightColorType { get; set; }
        /// <summary>
        /// 1/2 做过/共
        /// </summary>
        public static string Progress { get; set; } = "";

        /// <summary>
        /// 用于程序退出或其他情况时关闭误差板
        /// </summary>
        public int WcControlClose { get; set; }
        /// <summary>
        /// 误差板是否在运行
        /// </summary>
        public bool IsRunWc { get; set; }

        /// <summary>
        /// 功率方向
        /// </summary>
        public PowerWay FangXiang = PowerWay.正向有功;

        /// <summary>
        /// 获取当前检定是有功还是无功
        /// </summary>
        public bool IsYouGong
        {
            get
            {
                bool _IsP = false;
                if (FangXiang == PowerWay.正向有功 || FangXiang == PowerWay.反向有功)
                    _IsP = true;
                return _IsP;
            }
        }

        /// <summary>
        /// 表数据
        /// </summary>
        public static TestMeterInfo[] MeterInfo { get; set; }


        public static TestMeterInfo OneMeterInfo
        {
            get
            {
                return GetOneMeterInfo();
            }
        }
        /// <summary>
        /// 检定到当前计时，单位：秒
        /// </summary>
        public long VerifyPassTime
        {
            get
            {
                return (long)(DateTime.Now - StartTime).TotalSeconds;
            }
        }

        /// <summary>
        /// 检定开始时间,用于检定计时
        /// </summary>
        protected DateTime StartTime;


        /// <summary>
        /// 获得要检数组
        /// </summary>
        /// <param name="IsYaoJIan">false的时候获取不要检定的数组</param>
        /// <returns></returns>
        public bool[] GetYaoJian(bool IsYaoJIan = true)
        {
            bool[] yaoJianList = new bool[MeterNumber];
            for (int i = 0; i < MeterNumber; i++)
            {
                if (IsYaoJIan)
                {
                    yaoJianList[i] = MeterInfo[i].YaoJianYn;
                }
                else
                {
                    yaoJianList[i] = !MeterInfo[i].YaoJianYn;
                }
            }
            return yaoJianList;

        }

        #region 静态使用的属性
        public static float U
        {
            get
            {
                try
                {
                    if (OneMeterInfo == null)
                    {
                        return 57.7F;
                    }
                    else
                    {
                        return OneMeterInfo.MD_UB;
                    }
                }
                catch
                {
                    return 57.7F;
                }
            }
        }

        /// <summary>
        /// 测量方式
        /// </summary>
        public static WireMode Clfs
        {
            get
            {
                if (OneMeterInfo == null) return WireMode.三相四线;
                if (string.IsNullOrWhiteSpace(OneMeterInfo.MD_WiringMode))
                {
                    return WireMode.三相四线;
                }
                return (WireMode)Enum.Parse(typeof(WireMode), OneMeterInfo.MD_WiringMode);
            }
        }
        /// <summary>
        /// 是否经互感器 True-互感式，false直接式
        /// </summary>
        public static bool HGQ
        {
            get
            {
                if (OneMeterInfo != null)
                {
                    if (OneMeterInfo.MD_ConnectionFlag == "互感式")
                    {
                        return true;
                    }
                    return false;
                }
                return false;
            }
        }
        /// <summary>
        /// 是否经止逆器
        /// </summary>
        public static bool ZNQ
        {
            get
            {
                if (OneMeterInfo != null)
                {
                    if (OneMeterInfo.MD_ConnectionFlag == "有止逆")
                    {
                        return true;
                    }
                    return false;
                }
                return false;
            }
        }

        /// <summary>
        /// 频率
        /// </summary>
        public static float PL
        {
            get
            {
                if (OneMeterInfo != null)
                {
                    return float.Parse(OneMeterInfo.MD_Frequency.ToString());
                }
                return 50F;
            }
        }
        #endregion


        #endregion

        #region 需要用到的通用方法

        /// <summary>
        /// 做误差的时候初始化设备
        /// </summary>
        /// <param name="PowerFangXiang">功率方向</param>
        /// <param name="PowerYuanJian">功率元件</param>
        /// <param name="PowerYinSu">功率因数</param>
        /// <param name="PowerDianLiu">负载电流</param>
        /// <param name="pulselap">圈数</param>
        /// <returns></returns>
        public bool ErrorInitEquipment(PowerWay PowerFangXiang, Cus_PowerYuanJian PowerYuanJian, string PowerYinSu, string PowerDianLiu, int pulselap)
        {
            if (IsDemo) return true;
            bool isP = PowerFangXiang == PowerWay.正向有功 || PowerFangXiang == PowerWay.反向有功;
            int[] meterconst = MeterHelper.Instance.MeterConst(isP);
            float xIb = Number.GetCurrentByIb(PowerDianLiu, OneMeterInfo.MD_UA, HGQ);//计算电流

            if (Stop) return false;
            MessageAdd("正在升源...", EnumLogType.提示信息);
            if (!PowerOn(OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, xIb, xIb, xIb, PowerYuanJian, PowerFangXiang, PowerYinSu))
            {
                MessageAdd("升源失败,退出检定", EnumLogType.提示信息);
                return false;
            }

            ulong constants = GetStaConst();

            int index = 0;
            if (!isP) index = 1;

            SetPulseType((index + 49).ToString("x"));
            if (Stop) return true;
            MessageAdd("开始初始化基本误差检定参数!", EnumLogType.提示信息);
            //设置误差版被检常数
            MessageAdd("正在设置误差版标准常数...", EnumLogType.提示信息);
            int SetConstants = (int)(constants / 100);
            SetStandardConst(0, SetConstants, -2, 0xff);
            //设置误差版标准常数 TODO2
            MessageAdd("正在设置误差版被检常数...", EnumLogType.提示信息);
            int[] pulselaps = new int[MeterNumber];  //这里是为了以后不同表位不同圈数预留--目前暂时用着吧
            pulselaps.Fill(pulselap);
            if (!SetTestedConst(index, meterconst, 0, pulselaps, 0xff))
            {
                MessageAdd("初始化误差检定参数失败", EnumLogType.提示信息);
                return false;
            }
            return true;
        }


        /// <summary>
        /// 获取ASCII
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public string GetASCII(string data)
        {
            string str = "";
            for (int i = 0; i < data.Length / 2; i++)
            {
                byte asc = Convert.ToByte(data.Substring(i * 2, 2), 16);
                if (asc == 0) break;
                str += (char)asc;
            }
            return str;
        }
        /// <summary>
        /// 计算当前负载下本批表中脉冲常数最小的表跑一个脉冲需要的时间（ms）
        /// </summary>
        /// <param name="bYouGong">有功/无功</param>
        /// <param name="OneKWHTime">一度电需要的时间(分)</param>
        /// <returns>以毫秒为单位</returns>
        protected int OnePulseNeedTime(bool bYouGong, double OneKWHTime)
        {
            float minConst = 999999999;
            int[] arrConst = MeterHelper.Instance.MeterConst(bYouGong);
            for (int i = 0; i < arrConst.Length; i++)
            {

                if (arrConst[i] < minConst)
                    minConst = arrConst[i];

            }
            if (minConst == 999999999) return 1;
            if (minConst == 0) minConst = 1;
            int onePulseTime = (int)Math.Ceiling(OneKWHTime * 60 / minConst * 1000);
            return onePulseTime;
        }
        /// <summary>
        /// 计算seconds秒的脉冲数
        /// </summary>
        /// <param name="seconds">s</param>
        /// <param name="onePulseNeedTime">ms</param>
        /// <returns></returns>
        protected int CalcPulseOfSeconds(float seconds, float onePulseNeedTime)
        {
            if (seconds <= 0) return 1;
            if (onePulseNeedTime <= 0) return 1;
            return (int)Math.Ceiling(seconds * 1000 / onePulseNeedTime);
        }
        /// <summary>
        /// 特定功率下secons秒的累计kWh
        /// </summary>
        /// <param name="seconds">大于0</param>
        /// <param name="current"></param>
        /// <param name="Yj"></param>
        /// <param name="pf"></param>
        /// <returns></returns>
        protected double CalckWhOfSeconds(float seconds, float current, Cus_PowerYuanJian Yj, string pf)
        {
            if (seconds <= 0) seconds = 1;
            return CalculatePower(OneMeterInfo.MD_UB, current, Clfs, Yj, pf, IsYouGong) / 1000 * seconds / 3600;
        }
        /// <summary>
        /// 特定功率下累计kWh所需secons秒
        /// </summary>
        /// <param name="kWh">大于等于0</param>
        /// <param name="current"></param>
        /// <param name="Yj"></param>
        /// <param name="pf"></param>
        /// <returns></returns>
        protected double CalcSecondsOfkWh(float kWh, float current, Cus_PowerYuanJian Yj, string pf)
        {
            float power = CalculatePower(OneMeterInfo.MD_UB, current, Clfs, Yj, pf, IsYouGong);
            if (power == 0) power = 1;
            return Math.Abs(kWh * 3600 * 1000 / power);
        }

        #region 读取误差

        /// <summary>
        /// 演示模式下，误差个数
        /// </summary>
        int errNum = 0;
        /// <summary>
        /// 读取误差
        /// </summary>
        /// <param name="Wc">返回误差值</param>
        /// <param name="WcNum">误差次数</param>
        /// <param name="power">功率方向</param>
        /// <returns></returns>
        public bool ReadWc(ref string[] Wc, ref int[] WcNum, PowerWay power)
        {
            int readType = 0;
            switch (power)
            {
                case PowerWay.正向有功:
                    readType = 0;
                    break;
                case PowerWay.正向无功:
                    readType = 1;
                    break;
                case PowerWay.反向有功:
                    readType = 0;
                    break;
                case PowerWay.反向无功:
                    readType = 1;
                    break;
                default:
                    break;
            } //功率方向

            if (IsDemo)
            {
                Random rand = new Random(DateTime.Now.Millisecond);
                for (int i = 0; i < MeterNumber; i++)
                {
                    Wc[i] = ((rand.Next(10000) - 5000f) / 10000f).ToString("f5");
                    WcNum[i] = errNum;
                }
                errNum++;
            }
            else
            {
                StError[] stErrors = ReadWcbData(GetYaoJian(), readType);  //读取表位误差
                for (int i = 0; i < MeterNumber; i++)
                {
                    if (stErrors[i] != null)
                    {
                        Wc[i] = stErrors[i].szError;
                        WcNum[i] = stErrors[i].Index;
                    }
                }
            }


            return true;



        }

        //add yjt 20220610 新增获取随机误差
        public float[] BasicErr(float MeterLevel, int maxWCnum)
        {
            MeterLevel *= 10000;//扩大10000倍

            float[] tmpWC = new float[maxWCnum];

            for (int e = 0; e < maxWCnum; e++)
            {
                tmpWC[e] = new Random().Next((int)(-MeterLevel), (int)MeterLevel);
                tmpWC[e] = tmpWC[e] / 100000F;

                //延时0.2S
                int delayTime = 1000 / MeterNumber;
                Thread.Sleep(delayTime);
            }

            return tmpWC;
        }

        /// <summary>
        /// 读取误差
        /// </summary>
        /// <param name="Wc">返回误差值</param>
        /// <param name="WcNum">误差次数</param>
        /// <param name="power">功率方向</param>
        /// <returns></returns>
        public bool ReadWc(ref string[] Wc, ref int[] WcNum, int readType)
        {
            try
            {
                StError[] stErrors = ReadWcbData(GetYaoJian(), readType);  //读取表位误差
                for (int i = 0; i < MeterNumber; i++)
                {
                    if (stErrors[i] != null)
                    {
                        Wc[i] = stErrors[i].szError;
                        WcNum[i] = stErrors[i].Index;
                    }
                }
                return true;

            }
            catch (Exception)
            {
                return false;
                throw;
            }

        }


        /// <summary>
        /// 读取误差
        /// </summary>
        /// <param name="Wc">返回误差值</param>
        /// <param name="WcNum">误差次数</param>
        /// <param name="power">功率方向</param>
        /// <returns></returns>
        public StError[] ReadWc(PowerWay power)
        {
            try
            {
                int readType = 0;
                switch (power)
                {
                    case PowerWay.正向有功:
                        readType = 0;
                        break;
                    case PowerWay.正向无功:
                        readType = 1;
                        break;
                    case PowerWay.反向有功:
                        readType = 0;
                        break;
                    case PowerWay.反向无功:
                        readType = 1;
                        break;
                    default:
                        break;
                } //功率方向

                StError[] stErrors = ReadWcbData(GetYaoJian(), readType);  //读取表位误差
                return stErrors;

            }
            catch (Exception)
            {
                return null;
                throw;
            }

        }
        #endregion



        /// <summary>
        /// 计算指定负载下的标准功率.(W)
        /// </summary>
        /// <param name="U">负载电压</param>
        /// <param name="I">负载电流</param>
        /// <param name="Clfs">测量方式</param>
        /// <param name="Yj">元件H，ABC</param>
        /// <param name="Glys">功率因数，0.5L</param>
        /// <param name="isP">true 有功，false 无功</param>
        /// <returns>标准功率</returns>
        protected float CalculatePower(float U, float I, WireMode Clfs, Cus_PowerYuanJian Yj, string Glys, bool isP)
        {
            float flt_GlysP = 1;
            float flt_GlysQ;
            if (isP)
            {
                float.TryParse(Glys.Replace("C", "").Replace("L", "").ToString(), out flt_GlysP);
                flt_GlysQ = (float)Math.Sqrt(1 - Math.Pow(flt_GlysP, 2));
            }
            else
            {
                float.TryParse(Glys.Replace("C", "").Replace("L", "").ToString(), out flt_GlysQ);
                flt_GlysP = (float)Math.Sqrt(1 - Math.Pow(flt_GlysP, 2));
            }
            float p = U * I * flt_GlysP;
            float q = U * I * flt_GlysQ;
            if (Cus_PowerYuanJian.H == Yj)
            {
                if (Clfs == WireMode.三相四线)
                {
                    p *= 3F;
                    q *= 3F;
                }
                else if (Clfs == WireMode.三相三线)
                {
                    p *= 1.732F;
                    q *= 1.732F;

                }
            }
            return isP ? p : q;
        }
        /// <summary>
        /// 获取指定电能表当前功率方向下表等级
        /// </summary>
        /// <returns></returns>
        protected float MeterLevel(TestMeterInfo meter)
        {
            string[] level = Number.GetDj(meter.MD_Grane);
            float mlevel;

            //modify yjt 20220412 修改IR46的误差限
            if (level[IsYouGong ? 0 : 1] == "A")
            {
                mlevel = 2.0F;
            }
            else if (level[IsYouGong ? 0 : 1] == "B")
            {
                mlevel = 1.0F;
            }
            else if (level[IsYouGong ? 0 : 1] == "C")
            {
                mlevel = 0.5F;
            }
            else if (level[IsYouGong ? 0 : 1] == "D")
            {
                mlevel = 0.2F;
            }
            else if (level[IsYouGong ? 0 : 1] == "E")
            {
                mlevel = 0.1F;
            }
            else
            {
                if (!float.TryParse(level[IsYouGong ? 0 : 1], out mlevel))                  //当前表的等级
                {
                    mlevel = 0.02F;
                }
            }

            return mlevel;                   //当前表的等级
        }

        /// <summary>
        /// ms
        /// </summary>
        /// <param name="Time1"></param>
        /// <param name="Time2"></param>
        /// <returns></returns>
        public double TimeSubms(DateTime Time1, DateTime Time2)
        {
            TimeSpan tsSub = Time1.Subtract(Time2);
            return tsSub.TotalMilliseconds;
        }

        /// <summary>
        /// 返回第一个要检定表的数据
        /// </summary>
        /// <returns></returns>
        public static TestMeterInfo GetOneMeterInfo()
        {
            for (int i = 0; i < MeterInfo.Length; i++)
            {
                if (MeterInfo[i].YaoJianYn)
                {
                    return MeterInfo[i];
                }
            }
            return new TestMeterInfo();

        }
        /// <summary>
        /// 首个要检表有效位,由0开始
        /// </summary>
        public static int FirstIndex
        {
            get
            {

                for (int i = 0; i < MeterInfo.Length; i++)
                {
                    if (MeterInfo[i].YaoJianYn)
                        return i;
                }
                return -1;
            }
        }

        //add 20220908 新增获取小数位数
        /// <summary>
        /// 电量的小数位数
        /// </summary>
        public static int DecimalDigits
        {
            get
            {
                int xsws = 2;
                if ((OneMeterInfo.FKType == 2 && OneMeterInfo.DgnProtocol.HaveProgrammingkey)
                    || MeterInfo[FirstIndex].MD_JJGC == "IR46")
                {
                    xsws = 4;
                }
                return xsws;
            }
        }

        /// <summary>
        /// 结论的所有列名称
        /// </summary>
        /// <param name="arrayResultName"></param>
        protected string[] ResultNames
        {
            set
            {
                if (value != null)
                {
                    ResultDictionary.Clear();
                    for (int i = 0; i < value.Length; i++)
                    {
                        if (!ResultDictionary.ContainsKey(value[i]))
                        {
                            ResultDictionary.Add(value[i], new string[MeterNumber]);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 检测是否跳差
        /// </summary>
        /// <param name="lastError">前一误差</param>
        /// <param name="curError">当前误差</param>
        /// <param name="meterLevel">表等级</param>
        /// <param name="m_WCJump">跳差系数</param>
        /// <returns>T:跳差;F:不跳差</returns>
        protected bool CheckJumpError(string lastError, string curError, float meterLevel, float m_WCJump)
        {
            bool result = false;
            if (Number.IsNumeric(lastError) && Number.IsNumeric(curError))
            {
                float _Jump = float.Parse(curError) - float.Parse(lastError);
                if (Math.Abs(_Jump) > meterLevel * m_WCJump)
                {
                    result = true;
                }
            }
            return result;
        }

        #region 结论转换
        public void ConvertTestResult(string resultName, float[] arrayResult, int dotNumber = 2)
        {
            string formatTemp = "0";
            if (dotNumber > 0)
            {
                formatTemp = "0.".PadRight(2 + dotNumber, '0');
            }
            for (int i = 0; i < MeterNumber; i++)
            {
                if (MeterInfo[i].YaoJianYn)
                {
                    if (arrayResult.Length > i)
                    {
                        ResultDictionary[resultName][i] = arrayResult[i].ToString(formatTemp);
                    }
                }
            }
            RefUIData(resultName);
        }
        #endregion

        #endregion

        #region 检定

        /// <summary>
        /// 检定入口
        /// </summary>
        /// <param name="Verify"></param>
        public void DoVerify()
        {
            MeterLogicalAddressType = MeterLogicalTypeHelper.GetMeterLogicalType(Test_No);
            IsInitIto = MeterLogicalTypeHelper.GetTestItemIsInitIot(Test_No); //判断是否需要复位表
            //ConnectIOTMeter();
            try
            {
                if (!CheckPara()) //检定参数是否合法
                {
                    Utility.Log.LogManager.AddMessage($"解析方案参数出错,参数内容:{Test_Value}", Utility.Log.EnumLogSource.检定业务日志, Utility.Log.EnumLevel.Warning);
                    return;
                }
            }
            catch
            {
                Utility.Log.LogManager.AddMessage($"方案参数错误,{Test_Name}参数:{Test_Value}", EnumLogSource.检定业务日志, EnumLevel.Error);
                return;
            }
            try
            {
                if (Test_No.StartsWith("1300"))//表示校准标准表，不需要初始化电表，初始化标准表端口
                {
                    InitStdProtocol();
                }
                StartTime = DateTime.Now;
                MessageAdd(Test_Name + "检定开始。", EnumLogType.提示与流程信息);
                try
                {
                    Verify();
                    //是否开启二次巡检
                    if (VerifyConfig.IsCheckAgin == true)
                    {
                        if (!Stop)
                        {
                            for (int i = 0; i < MeterNumber; i++)
                            {
                                if (!MeterInfo[i].YaoJianYn) continue;
                                if (string.IsNullOrWhiteSpace(ResultDictionary["结论"][i]))
                                {
                                    Utility.Log.LogManager.AddMessage($"检测到结论为空,开始二次巡检", Utility.Log.EnumLogSource.检定业务日志, Utility.Log.EnumLevel.Error);
                                    Verify();
                                    break;
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Utility.Log.LogManager.AddMessage($"检定错误：{ex.Message}", EnumLogSource.检定业务日志, EnumLevel.TipsError);
                }

                //TODO 需要验证切换检测点时降电流。
                if (ConfigHelper.Instance.Std_Switch_I)
                {
                    // 只关闭电流
                    DeviceControl.PowerCurrentOff();
                }

                bool resultChanged = false;
                for (int i = 0; i < MeterNumber; i++)
                {
                    if (!MeterInfo[i].YaoJianYn) continue;
                    if (string.IsNullOrWhiteSpace(ResultDictionary["结论"][i]))
                    {
                        ResultDictionary["结论"][i] = ConstHelper.不合格;
                        resultChanged = true;
                    }
                }
                if (resultChanged) RefUIData("结论");

                if (ResultDictionary["结论"].Count(v => v == ConstHelper.不合格) == MeterNumber)
                {
                    MessageAdd("全部不合格！", EnumLogType.错误信息);
                }
                else if (ResultDictionary["结论"].Count(v => v == ConstHelper.合格) == MeterNumber)
                {

                }
                else if (GetYaoJian().Count(v => v) <= 0)
                {
                    MessageAdd("全部被设置不检！", EnumLogType.错误信息);
                }
                else if (GetYaoJian().Count(v => v) == ResultDictionary["结论"].Count(v => v == Core.ConstHelper.合格))
                {

                }
                else if (GetYaoJian().Count(v => v) == ResultDictionary["结论"].Count(v => v == Core.ConstHelper.不合格))
                {
                    MessageAdd("要检全部不合格！", EnumLogType.错误信息);
                }
                else
                {
                    if (VerifyConfig.UnqualifiedJumpOutOf == true && !Stop)
                    {
                        //if (ConfigHelper.Instance.VerifyModel == "自动模式")
                        {
                            bool IsCheck = false;
                            for (int i = 0; i < MeterNumber; i++)
                            {
                                if (ResultDictionary["结论"][i] == Core.ConstHelper.不合格)
                                {
                                    MeterInfo[i].YaoJianYn = false;//隔离的表位不检查了
                                    IsCheck = true;
                                }

                            }
                            if (IsCheck)
                            {
                                ControlMeterRelay(GetYaoJian(false), 1);    //将不要检定的关闭
                                RefMeterYaoJian();
                            }
                        }

                    }
                }
                MessageAdd(Test_Name + "检定完成。", EnumLogType.提示与流程信息);
            }
            catch (Exception ex)
            {
                Utility.Log.LogManager.AddMessage($"检定出错,错误编号：999－10001　\r\n" + ex, Utility.Log.EnumLogSource.检定业务日志, Utility.Log.EnumLevel.TipsError);

            }
            finally
            {
                //完成后清理工作
                Stop = true;
                CheckOver = true;
            }
            if (IsRunWc) StopWcb(WcControlClose, 0xff);
         }

        /// <summary>
        /// 参数合法性检测[由具体检定器实现]
        /// </summary>
        /// <returns></returns>
        protected virtual bool CheckPara()
        {
            return true;
        }



        public virtual void Verify()    //基类实现
        {
            NoResoult = new string[MeterNumber];  //不合格原因
            NoResoult.Fill("");
        }

        /// <summary>
        /// 错误刷新不合格结论
        /// </summary>
        public void ErrorRefResoult()
        {
            for (int i = 0; i < MeterNumber; i++)
            {
                if (!MeterInfo[i].YaoJianYn) continue;
                ResultDictionary["结论"][i] = "不合格";

            }
            RefUIData("结论");
        }

        #endregion

        #region 电表命令
        /// <summary>
        /// 更新电能表协议信息
        /// </summary>
        public void UpdateMeterProtocol()
        {
            MeterHelper.Instance.Init();
            //把数据库中读取到的串口数据给到类里面
            DgnProtocolInfo[] protocols = MeterHelper.Instance.GetAllProtocols();
            string[] meterAddress = MeterHelper.Instance.GetMeterAddress();
            ComPortInfo[] comPorts = MeterHelper.Instance.GetComPortInfo();
            MeterProtocolAdapter.Instance.Initialize(protocols, meterAddress, comPorts);
        }
        public void UpdateMeterAddress()
        {
            string[] meterAddress = MeterHelper.Instance.GetMeterAddress();
            for (int i = 0; i < MeterNumber; i++)
            {
                if (!MeterInfo[i].YaoJianYn) { continue; }
                if (MeterProtocolAdapter.Instance.MeterProtocols[i] == null)
                {
                    continue;
                }
                MeterProtocolAdapter.Instance.MeterProtocols[i].SetMeterAddress(meterAddress[i]);//设置电表地址
            }
        }

        /// <summary>
        /// 读取表地址和表号。带演示模式
        /// </summary>
        public void ReadMeterAddrAndNo()
        {
            if (IsDemo) return;
            if (!ReadMeterAddressAndNo) return;

            if (OneMeterInfo.DgnProtocol == null)
            {
                //2022.07.06暂时使用
                Utility.Log.LogManager.AddMessage("读取地址时电表协议为空，重新初始化电表协议", Utility.Log.EnumLogSource.检定业务日志, Utility.Log.EnumLevel.Error);
                MeterHelper.Instance.Init();

                EquipmentData.DeviceManager.SetEquipmentThreeColor(EmLightColor.绿, 2);
                if (OneMeterInfo.DgnProtocol == null)
                {
                    Utility.Log.LogManager.AddMessage("初始化结束，OneMeterInfo.DgnProtocol为空", Utility.Log.EnumLogSource.检定业务日志, Utility.Log.EnumLevel.Error);
                    int index = 0;
                    for (int i = 0; i < MeterInfo.Length; i++)
                    {
                        if (MeterInfo[i].YaoJianYn == true)
                        {
                            break;
                        }
                        else
                        {
                            index++;
                        }

                    }
                    DgnProtocolInfo dgn = new DgnProtocolInfo(MeterInfo[index].MD_ProtocolName);
                    if (IsDoubleProtocol)
                    {
                        dgn.Setting = "9600,e,8,1";
                    }
                    dgn.ClassName = "CDLT698";
                    OneMeterInfo.DgnProtocol = dgn;
                }

            }

            if (OneMeterInfo.DgnProtocol.ClassName == "CDLT6452007")
            {
                int intChannelCount = MeterProtocolManager.Instance.GetChannelCount();

                if (MeterNumber / intChannelCount == 1)
                {

                    if (Stop) return;
                    MessageAdd("正在进行【读取表地址】操作...", EnumLogType.提示信息);
                    string[] address = MeterProtocolAdapter.Instance.ReadAddress();

                    for (int i = 0; i < MeterNumber; i++)
                    {
                        TestMeterInfo meter = MeterInfo[i];
                        if (!meter.YaoJianYn) continue;
                        meter.MD_PostalAddress = address[i];

                    }
                    UpdateMeterAddress();
                }


                if (Stop) return;
                MessageAdd("正在进行【读取表号】操作...", EnumLogType.提示信息);
                string[] meterno = MeterProtocolAdapter.Instance.ReadData("表号");

                for (int i = 0; i < MeterNumber; i++)
                {
                    TestMeterInfo meter = MeterInfo[i];
                    if (!meter.YaoJianYn) continue;
                    if (meterno[i].Trim() == "")
                    {
                        MessageAdd($"表位【{i + 1}】没有读取到表号", EnumLogType.错误信息);
                    }
                    meter.MD_MeterNo = meterno[i];
                }
            }
            else if (OneMeterInfo.DgnProtocol.ClassName == "CDLT698")
            {
                MessageAdd("正在进行【读取表地址表号】操作...", EnumLogType.提示信息);
                //List<string> LstOad = new List<string>
                //{
                //    "40010200", //通信地址
                //    "40020200" //表号
                //};
                //Dictionary<int, object[]> DicObj = MeterProtocolAdapter.Instance.ReadData(LstOad, EmSecurityMode.ClearText, EmGetRequestMode.GetRequestNormalList);//某些不支持GetRequestNormalList读不同区域数据
                List<string> LstOad = new List<string>
                {
                    "40010200" //通信地址
                };
                Dictionary<int, object[]> dic1 = MeterProtocolAdapter.Instance.ReadData(LstOad, EmSecurityMode.ClearText, EmGetRequestMode.GetRequestNormal);

                LstOad = new List<string>
                {
                    "40020200" //表号
                };
                Dictionary<int, object[]> dic2 = MeterProtocolAdapter.Instance.ReadData(LstOad, EmSecurityMode.ClearText, EmGetRequestMode.GetRequestNormal);

                Dictionary<int, object[]> DicObj = new Dictionary<int, object[]>();
                for (int i = 0; i < MeterNumber; i++)
                {
                    if (!MeterInfo[i].YaoJianYn) continue;
                    if (dic1.ContainsKey(i) && dic2.ContainsKey(i))
                    {
                        if (dic1[i] != null && dic1[i].Length > 0 && dic2[i] != null && dic2[i].Length > 0)
                            DicObj.Add(i, new object[] { dic1[i][0], dic2[i][0] });
                    }
                }

                for (int i = 0; i < MeterNumber; i++)
                {
                    if (Stop) return;
                    TestMeterInfo meter = MeterInfo[i];
                    if (!meter.YaoJianYn) continue;
                    if (DicObj.ContainsKey(i))
                    {
                        if (DicObj[i].Length > 1)
                        {
                            meter.MD_PostalAddress = DicObj[i][0].ToString();
                            meter.MD_MeterNo = DicObj[i][1].ToString();
                        }
                    }
                }
                UpdateMeterAddress();
            }
            bool Isok = true;
            for (int i = 0; i < MeterNumber; i++)
            {
                TestMeterInfo meter = MeterInfo[i];
                if (!meter.YaoJianYn) continue;
                if (meter.MD_PostalAddress == null || meter.MD_MeterNo == null || meter.MD_PostalAddress == "" || meter.MD_MeterNo == "")
                {
                    Isok = false;
                    break;
                }
            }
            if (Isok)
            {
                ReadMeterAddressAndNo = false;
            }
        }

        private bool IsOneIdenityOK = true;//是否是第一次身份认证--代表身份认证另一个芯片
        /// <summary>
        /// 身份认证。带演示模式
        /// </summary>
        /// <param name="mustLink">true:必须重新认证</param>
        /// <param name="IsOthrtChip">是否认证其他芯片</param>
        public bool Identity(bool mustLink = false, bool IsOthrtChip = true)//TODO:整理
        {
            try
            {
                if (IsDemo) return true;
                if (OneMeterInfo.FKType == 2) return true;//TODO 远程本地还没有添加
                if (VerifyConfig.Dog_Type == "无") return true;
                if (OneMeterInfo.DgnProtocol.ClassName != "CDLT6452007" && OneMeterInfo.DgnProtocol.ClassName != "CDLT698")
                {
                    return true;
                }
                if (OneMeterInfo.DgnProtocol.ClassName == "CDLT6452007" && OneMeterInfo.DgnProtocol.HaveProgrammingkey)
                {
                    return true;
                }
                bool newLink = false;
                if (!DeviceControl.CheckEncrypLink())
                {
                    if (!DeviceControl.LinkPasswordMatchine())  //打开USBKey,登录服务器
                    {
                        MessageAdd("加密机连接失败", EnumLogType.错误信息);
                        return false;
                    }
                    newLink = true;
                }
                if (!mustLink && !newLink && !EquipmentData.Controller.EveryStartTestIdent)
                {
                    if ((DateTime.Now - EncryLastLinkTime).TotalSeconds < 900)
                    {
                        return true;
                    }
                }
                //身份认证超时一般为30分钟，即1800秒
                if (!mustLink && !newLink)
                {
                    bool valid = false;
                    string[] readdata = MeterProtocolAdapter.Instance.ReadData("电表运行状态字3");
                    for (int i = 0; i < MeterNumber; i++)
                    {
                        if (!MeterInfo[i].YaoJianYn) continue;
                        if (MeterInfo[i].SessionKey == null) break;

                        if (!string.IsNullOrWhiteSpace(readdata[i]))
                        {
                            int runningState3 = Convert.ToInt32(readdata[i], 16);
                            if (OneMeterInfo.DgnProtocol.ClassName == "CDLT6452007")
                            {

                            }
                            else if (OneMeterInfo.DgnProtocol.ClassName == "CDLT698")
                            {
                                runningState3 = Number.BitRever(runningState3, 16);
                            }

                            if ((runningState3 & 0x2000) == 0x2000)
                            {
                                valid = true;
                            }
                            else
                            {
                                valid = false;
                                break;
                            }
                        }
                        else
                        {
                            valid = false;
                            break;
                        }
                    }
                    if (valid)
                    {
                        readdata = MeterProtocolAdapter.Instance.ReadData("会话时效剩余时间");
                        int min = 3;
                        for (int i = 0; i < MeterNumber; i++)
                        {
                            if (!MeterInfo[i].YaoJianYn) continue;
                            if (int.TryParse(readdata[i], out int time) && min > time)
                            {
                                min = time;
                            }
                        }
                        if (min < 3)
                        {
                            valid = false;
                        }
                    }

                    if (valid)
                        return true;
                }


                if (Stop) return false;

                MessageAdd("正在进行【身份认证】...", EnumLogType.流程信息);

                if (OneMeterInfo.DgnProtocol.ClassName == "CDLT698")
                {
                    for (int i = 0; i < MeterNumber; i++)
                    {
                        if (MeterInfo[i].YaoJianYn)
                            MeterInfo[i].SessionKey = "";
                    }
                }
                bool[] canExecute = new bool[MeterNumber];
                canExecute.Fill(true);
                if (OneMeterInfo.DgnProtocol.ClassName == "CDLT6452007")
                {
                    SendCostCommand(Cus_EncryptionTrialType.身份认证, canExecute);
                }
                else if (OneMeterInfo.DgnProtocol.ClassName == "CDLT698")
                {
                    MessageAdd("正在进行【读取安全模式参数】操作...", EnumLogType.提示信息);
                    List<string> LstOad = new List<string> { "F1010200" };
                    Dictionary<int, object[]> DicObj = MeterProtocolAdapter.Instance.ReadData(LstOad, EmSecurityMode.ClearText, EmGetRequestMode.GetRequestNormal);
                    bool bSecurity = false;
                    for (int i = 0; i < MeterNumber; i++)
                    {
                        if (!MeterInfo[i].YaoJianYn) continue;
                        if (DicObj.ContainsKey(i))
                        {
                            if (DicObj[i].Length > 1)
                            {
                                if (DicObj[i][0] != null && DicObj[i][0].ToString() == "00")
                                {
                                    bSecurity = true;
                                    break;
                                }
                            }
                        }
                    }

                    if (bSecurity)
                    {
                        if (Stop) return false;
                        MessageAdd("正在进行【启用安全模式参数】操作...", EnumLogType.提示信息);
                        MeterProtocolAdapter.Instance.SecurityParameter(0x01); //启用安全模式
                        //LstOad = new List<string> { "F1010200" };
                        //DicObj = MeterProtocolAdapter.Instance.ReadData(LstOad, EmSecurityMode.ClearText, EmGetRequestMode.GetRequestNormal);
                    }
                    if (Stop) return false;
                    MessageAdd("正在进行【读取ESAM信息】操作...", EnumLogType.提示信息);
                    LstOad = new List<string> { "F1000700", "F1000200", "F1000400" };
                    DicObj = MeterProtocolAdapter.Instance.ReadData(LstOad, EmSecurityMode.ClearText, EmGetRequestMode.GetRequestNormalList);
                    //DicObj = MeterProtocolAdapter.Instance.ReadData(LstOad, EmSecurityMode.CiphertextMac , EmGetRequestMode.GetRequestNormalList);
                    for (int i = 0; i < MeterNumber; i++)
                    {
                        TestMeterInfo meter = MeterInfo[i];
                        if (!meter.YaoJianYn) continue;
                        if (DicObj.ContainsKey(i))
                        {
                            if (DicObj[i].Length > 4)
                            {
                                for (int z = 0; z < DicObj[i].Length; z++)
                                {
                                    if (DicObj[i][z] == null)
                                    {
                                        DicObj[i][z] = "";
                                    }
                                }
                                meter.SessionNo = (int.Parse(DicObj[i][0].ToString()) + 1).ToString("X");
                                meter.EsamId = DicObj[i][3].ToString();

                                meter.EsamKey = DicObj[i][4].ToString();

                                if (meter.EsamKey != "00000000000000000000000000000000")
                                    meter.EsamStatus = 1;
                                else
                                    meter.EsamStatus = 0;
                            }
                        }
                    }
                    if (Stop) return false;
                    MessageAdd("正在进行【建立应用连接】操作...", EnumLogType.提示信息);

                    if (MeterLogicalAddressType == MeterLogicalAddressEnum.计量芯)
                    {
                        MeterProtocolAdapter.Instance.AppConnectionIOMeter();
                    }
                    else
                    {
                        MeterProtocolAdapter.Instance.AppConnection();
                    }
                }

                EncryLastLinkTime = DateTime.Now.AddSeconds(-60);
                EquipmentData.Controller.EveryStartTestIdent = false;//开始检定认证过一次

                if (IsOthrtChip && IsOneIdenityOK && ConfigHelper.Instance.IsITOMeter)
                {
                    IsOneIdenityOK = false;
                    //物联表这里加上切换他芯片-在做一次身份认证-然后在恢复他芯片
                    MeterLogicalAddressEnum tem = MeterLogicalAddressType;
                    //切换到相反的芯片
                    MeterLogicalAddressType = tem == MeterLogicalAddressEnum.计量芯 ? MeterLogicalAddressEnum.管理芯 : MeterLogicalAddressEnum.计量芯;
                    MessageAdd($"开始进行{MeterLogicalAddressType}的身份认证", EnumLogType.流程信息);
                    Identity(true);
                    MeterLogicalAddressType = tem;

                }
                else
                {
                    IsOneIdenityOK = true;
                }

                MessageAdd("【身份认证】完成...", EnumLogType.流程信息);
                return true;
            }
            catch (Exception ex)
            {
                MessageAdd("身份认证出错：" + ex, EnumLogType.错误信息);
                return false;
            }
        }


        /// <summary>
        /// 发送费控相关命令
        /// </summary>
        /// <param name="ControlCommand"></param>
        /// <returns></returns>
        public void SendCostCommand(Cus_EncryptionTrialType ControlCommand, bool[] CanExecute = null)
        {
            if (IsDemo) return;
            if (CanExecute == null)
            {
                CanExecute = new bool[MeterNumber];
                CanExecute.Fill(true);
            }
            int channelCount = MeterProtocolManager.Instance.GetChannelCount(); //通道数
            int oneChannelMeterCount = MeterNumber / channelCount;  //第个通道的表位数据

            for (int i = 1; i <= oneChannelMeterCount; i++)
            {
                if (Stop) return;
                if (EncryptionThread == null)
                    EncryptionThread = new MulitThread.MulitEncryptionWorkThreadManager(MeterNumber);

                //if (oneChannelMeterCount == 2)
                //{
                //if (i == 1)
                //    EquipHelper.Instance.Switch485Channel(2);
                //else
                //    EquipHelper.Instance.Switch485Channel(1);
                //}
                EncryptionThread.ChannelCount = channelCount;
                EncryptionThread.CurrentChannelIndex = i;
                EncryptionThread.IsLocalMeter = true; //远程还是本地费控
                EncryptionThread.OneChannelMeterCount = oneChannelMeterCount;
                EncryptionThread.CanExecute = CanExecute;
                EncryptionThread.Start(ControlCommand);
                if (Stop)
                {
                    EncryptionThread.Stop();
                    return;
                }
                while (!EncryptionThread.IsWorkDone())
                {
                    if (Stop)
                    {
                        EncryptionThread.Stop();
                        return;
                    }
                    Thread.Sleep(100);
                }
            }
        }

        #endregion 

        #region 载波功能

        public void SwitchChannel(Cus_ChannelType communType)
        {
            //LYTest.MeterProtocol.App.g_ChannelType = Cus_ChannelType.通讯485;
            if (IsDemo) return;

            switch (communType)
            {
                case Cus_ChannelType.通讯485:
                    App.g_ChannelType = Cus_ChannelType.通讯485;
                    break;
                case Cus_ChannelType.通讯载波:

                    EquipmentData.DeviceManager.ZBPowerSupplyType(1);
                    App.g_ChannelType = Cus_ChannelType.通讯载波;

                    //if (App.CarrierInfo.CarrierType == "2041")
                    //if (!EquipmentData.IsHplcNet)// TODO没有判断通讯介质，后续补充
                    if(App.CarrierInfo.IsRoute)
                    {

                        //【初始化控制器】
                        MessageAdd("初始化控制器", EnumLogType.提示信息);
                        int yj = 0;
                        Dictionary<string, int> ID = new Dictionary<string, int>();
                        for (int i = 0; i < MeterNumber; i++)
                        {
                            TestMeterInfo meter = MeterInfo[i];
                            if (!meter.YaoJianYn) continue;
                            yj++;
                            if (!ID.ContainsKey(meter.MD_CarrName))
                                ID.Add(meter.MD_CarrName, i);
                        }
                        foreach (int item in ID.Values)
                        {
                            MeterProtocolAdapter.Instance.Init2041(item);
                            Thread.Sleep(1000);
                        }
                        if (Stop) return;
                        MessageAdd("添加从节点...", EnumLogType.提示信息);
                        AddCarrierNodes();
                        if (Stop) return;
                        if (App.CarrierInfo.IsBroad) //宽带载波需要组网时间
                        {
                            //modify yjt jz 20220822 修改组网时间为100秒
                            //int netTime = 5 * 60;
                            int netTime = 160 + 10 * yj;
                            WaitTime("正在模块组网", netTime);
                        }
                        foreach (int item in ID.Values)
                        {
                            MeterProtocolAdapter.Instance.PauseRouter(item);
                            Thread.Sleep(500);
                        }

                        if (App.LY3762 == null)
                        {
                            App.LY3762 = MeterProtocolAdapter.Instance.lY3762;
                        }
                    }
                    //else //断电后上电组网需要30秒左右的时间
                    //{
                    //    WaitTime("正在重新组网", 30);
                    //}
                    EquipmentData.IsHplcNet = true;
                    App.g_ChannelType = Cus_ChannelType.通讯载波;
                    break;
                case Cus_ChannelType.通讯无线:
                    App.g_ChannelType = Cus_ChannelType.通讯无线;
                    break;
                default:
                    break;
            }
        }

        ///// <summary>
        ///// 组网 哈尔滨使用——接线检查组网
        ///// </summary>
        //private void Networking()
        //{
        //    try
        //    {
        //        LYTest.MeterProtocol.App.g_ChannelType = Cus_ChannelType.通讯载波;
        //        if (!EquipmentData.IsHplcNet)// TODO没有判断通讯介质，后续补充
        //        {
        //            //【初始化控制器】
        //            MessageAdd("初始化控制器", EnumLogType.提示信息);
        //            Dictionary<string, int> ID = new Dictionary<string, int>();
        //            for (int i = 0; i < MeterNumber; i++)
        //            {
        //                TestMeterInfo meter = MeterInfo[i];
        //                if (!meter.YaoJianYn) continue;
        //                if (!ID.ContainsKey(meter.MD_CarrName))
        //                    ID.Add(meter.MD_CarrName, i);
        //            }
        //            foreach (int item in ID.Values)
        //            {
        //                //EquipHelper.Instance.Init2041(item);
        //                MeterProtocolAdapter.Instance.Init2041(item);
        //                Thread.Sleep(1000);
        //            }
        //            if (Stop) return;
        //            MessageAdd("添加从节点...", EnumLogType.提示信息);
        //            AddCarrierNodes();
        //            if (Stop) return;
        //            if (App.CarrierInfo.IsBroad) //宽带载波需要组网时间
        //            {
        //                //int netTime = 5 * 60;
        //                //int netTime = 180;
        //                WaitTime("正在模块组网", 5);

        //            }
        //            foreach (int item in ID.Values)
        //            {
        //                MeterProtocolAdapter.Instance.PauseRouter(item);
        //                Thread.Sleep(500);
        //            }

        //            if (LYTest.MeterProtocol.App.lY3762 == null)
        //            {
        //                LYTest.MeterProtocol.App.lY3762 = MeterProtocolAdapter.Instance.lY3762;
        //            }

        //            LYTest.MeterProtocol.App.Protocols = OneMeterInfo.DgnProtocol.ClassName;

        //            EquipmentData.IsHplcNet = true;
        //            LYTest.MeterProtocol.App.g_ChannelType = Cus_ChannelType.通讯485;
        //        }
        //    }
        //    catch (Exception)
        //    {
        //        LYTest.MeterProtocol.App.g_ChannelType = Cus_ChannelType.通讯485;
        //    }
        //}

        /// <summary>
        /// 添加载波从节点S
        /// </summary>
        private void AddCarrierNodes()
        {

            //App.CarrierInfo.IsBroad = App.CarrierInfos[FirstIndex].IsBroad;
            for (int i = 0; i < MeterNumber; i++)
            {
                TestMeterInfo meter = MeterInfo[i];
                if (!meter.YaoJianYn) continue;

                string tmp = "";
                string tmp1 = meter.MD_PostalAddress.PadLeft(12, '0');
                for (int j = 0; j < 6; j++)
                {
                    tmp += tmp1.Substring((5 - j) * 2, 2);
                }

                if (App.CarrierInfos[i].IsBroad) //宽带载波
                {
                    tmp = $"01{tmp}{(meter.DgnProtocol.ClassName == "CDLT698" ? "03" : "02")}";

                }
                else //窄带载波
                {
                    tmp = $"01{tmp}{i + 1:X2}0002"; //长度，表地址，表位，00，645 07协议
                }

                App.Carrier_Cur_BwIndex = i;
                MessageAdd($"设添加载波从节点 {i + 1}表位 地址：{tmp}", EnumLogType.提示信息);
                MeterProtocolAdapter.Instance.AddCarrierNode(1, tmp, i);
            }

        }

        #endregion

        #region 旧物联表
        //#region 
        ///// <summary>
        ///// 连接物联表--用于中途关源导致掉线的情况
        ///// </summary>
        //public void ConnectIOTMeter()
        //{
        //    if (!IsCheckITOMeter) return;
        //    if (IsDemo || !ConfigHelper.Instance.IsITOMeter) return;
        //    //这里判断一下功率源

        //    if ( EquipmentData.StdInfo.Ua<50 && EquipmentData.StdInfo.Ub<50 && EquipmentData.StdInfo.Uc<50)
        //    {
        //        PowerOn();
        //        WaitTime("正在升源",15);
        //    }


        //    //这里还需要获取地址来连接蓝牙表
        //    string[] address = new string[MeterNumber];
        //    for (int i = 0; i < MeterNumber; i++)
        //    {
        //        if (!meterInfo[i].YaoJianYn) continue;
        //        address[i] = meterInfo[i].MD_PostalAddress;
        //    }
        //    bool[] resoult = MeterProtocolAdapter.Instance.IOTMete_Connect(address, ConfigHelper.Instance.Bluetooth_Ping);
        //    bool t = false;
        //    for (int i = 0; i < MeterNumber; i++)
        //    {
        //        if (!meterInfo[i].YaoJianYn) continue;
        //        if (resoult[i] == false)
        //        {
        //            t = true;  //代表有表断开连接了
        //            break;
        //        }
        //    }
        //     //开始重连
        //    if (t)
        //    {
        //        MessageAdd("有表位蓝牙连接中断,开始重连", EnumLogType.流程信息);

        //        bool[] yaojian = new bool[MeterNumber];
        //        for (int i = 0; i < MeterNumber; i++)
        //            yaojian[i] = meterInfo[i].YaoJianYn;//保存要检定标记
        //        for (int i = 0; i < MeterNumber; i++)
        //        {
        //            if (!meterInfo[i].YaoJianYn) continue;
        //            if (resoult[i] == true) meterInfo[i].YaoJianYn = false;   //只需要对有问题的重连就行
        //        }
        //        MeterProtocolAdapter.Instance.IOTMete_Reset();
        //        for (int index = 0; index < 5; index++)
        //        {
        //            resoult = MeterProtocolAdapter.Instance.IOTMete_Connect(address, ConfigHelper.Instance.Bluetooth_Ping);
        //            string err = "";
        //            t = true;
        //            for (int i = 0; i < MeterNumber; i++)
        //            {
        //                if (!meterInfo[i].YaoJianYn) continue;
        //                if (resoult[i] == false)
        //                {
        //                    t = false;
        //                    err += $"【{i + 1}】";
        //                }
        //            }
        //            if (t) break;
        //            MessageAdd($"表位：{err}第{index + 1}次蓝牙连接失败,开始重连", EnumLogType.流程信息);
        //        }

        //        for (int i = 0; i < MeterNumber; i++)
        //            meterInfo[i].YaoJianYn=yaojian[i] ;
        //    }

        //    IsCheckITOMeter = false;
        //}


        ///// <summary>
        ///// 设置蓝牙模块的数据
        ///// </summary>
        //public void SetBluetoothModule(int ControlType)
        //{
        //    if (ConfigHelper.Instance.PulseType == "无") return;
        //    if (ItoControlType == ControlType) return;
        //    ItoControlType = ControlType;
        //    BluetoothModule_PulseModel PulseModel = BluetoothModule_PulseModel.秒脉冲输出;
        //    switch (ControlType)
        //    {
        //        case 0:
        //        case 6:
        //            PulseModel = BluetoothModule_PulseModel.有功脉冲;
        //            break;
        //        case 1:
        //        case 7:
        //        case 8:
        //            PulseModel = BluetoothModule_PulseModel.无功脉冲;
        //            break;
        //        case 4:
        //            PulseModel = BluetoothModule_PulseModel.秒脉冲输出;
        //            break;
        //        default:
        //            break;
        //    }
        //    bool[] resoult=new bool[MeterNumber]; //切换光电脉冲
        //    bool[] resoult3 = new bool[MeterNumber] ; //切换模块的检定模式
        //    bool[] resoult2 = new bool[MeterNumber] ; //切换物联表检定模式
        //    string tips = "";
        //    resoult.Fill(true);
        //    resoult3.Fill(true);
        //    resoult2.Fill(true);


        //    byte Power;
        //    if (OneMeterInfo.Other3 == "光电脉冲") //光脉冲
        //    {
        //        MessageAdd($"切换到光电脉冲", EnumLogType.提示信息);
        //        resoult = MeterProtocolAdapter.Instance.IOTMete_SetLightPulse((byte)PulseModel); //需要切换到光电脉冲模式
        //        for (int i = 0; i < MeterNumber; i++)
        //        {
        //            if (!meterInfo[i].YaoJianYn) continue;
        //            if (!resoult[i]) tips += $"{i + 1}";
        //        }
        //        if (tips != "")
        //        {
        //            MessageAdd($"表位{tips}切换到光电脉冲失败", EnumLogType.错误信息);
        //            tips = "";
        //        }

        //    }
        //    if (ConfigHelper.Instance.IsITOMeter)//物联网表才需要切换检定模式
        //    {
        //        //表发射功率
        //        Power = (byte)((BluetoothModule_TransmitPower)Enum.Parse(typeof(BluetoothModule_TransmitPower), ConfigHelper.Instance.Bluetooth_MeterTransmitPower));
        //        //表频段
        //        byte Frequency = (byte)((BluetoothModule_TableFrequencyBand)Enum.Parse(typeof(BluetoothModule_TableFrequencyBand), ConfigHelper.Instance.Bluetooth_MeterFrequencyBand));
        //        //表通道数量
        //        byte count = (byte)ConfigHelper.Instance.Bluetooth_MeterPassCount;
        //        //切换电表的检定模式
        //        MessageAdd($"正在切换物联表到【{PulseModel}】检定模式", EnumLogType.提示信息);
        //        byte[] TestModel = MeterProtocolAdapter.Instance.IOTMete_SetMeterTestModel((byte)PulseModel, Power, Frequency, count);
        //        List<int> PretreatmentSelectID = new List<int>();//需要预处理查询的表位ID
        //        for (int i = 0; i < MeterNumber; i++)
        //        {
        //            if (!meterInfo[i].YaoJianYn && TestModel[i] == 0x00) continue;
        //            if (TestModel[i] == 0x01) //切换失败的,判断一下是不是没有预处理过
        //            {
        //                if (meterInfo[i].Other4 == null || meterInfo[i].Other4 == "" || meterInfo[i].Other4 == "未处理")
        //                {
        //                    MeterProtocolAdapter.Instance.IOTMete_Pretreatment(i + 1); //进行蓝牙预处理
        //                    PretreatmentSelectID.Add(i);
        //                }
        //            } 
        //            else if (TestModel[i] == 0x03)//未授权
        //            {
        //                meterInfo[PretreatmentSelectID[i]].Other4 = "未授权";
        //                string value = meterInfo[PretreatmentSelectID[i]].Other4;
        //                EquipmentData.MeterGroupInfo.Meters[i].SetProperty("MD_OTHER_4", value);
        //                DAL.DynamicModel Model2 = new DAL.DynamicModel();
        //                Model2.SetProperty("MD_OTHER_4", value);
        //                string where1 = $"METER_ID = '{meterInfo[PretreatmentSelectID[i]].Meter_ID}'";
        //                DAL.DALManager.MeterTempDbDal.Update("T_TMP_METER_INFO", where1, Model2, new List<string> { "MD_OTHER_4" });
        //            }
        //        }
        //        if (PretreatmentSelectID.Count > 0)
        //        {
        //            WaitTime("预处理", 35);  //预处理需要等待35秒
        //            for (int i = 0; i < PretreatmentSelectID.Count; i++)
        //            {
        //                if (MeterProtocolAdapter.Instance.IOTMete_PretreatmentSelect(PretreatmentSelectID[i] + 1)) //预处理成功
        //                {
        //                }
        //                else    //预处理失败
        //                {
        //                }
        //                meterInfo[PretreatmentSelectID[i]].Other4 = "已处理";
        //                string value = meterInfo[PretreatmentSelectID[i]].Other4;
        //                EquipmentData.MeterGroupInfo.Meters[i].SetProperty("MD_OTHER_4", value);
        //                DAL.DynamicModel Model2 = new DAL.DynamicModel();
        //                Model2.SetProperty("MD_OTHER_4", value);
        //                string where1 = $"METER_ID = '{meterInfo[PretreatmentSelectID[i]].Meter_ID}'";
        //                DAL.DALManager.MeterTempDbDal.Update("T_TMP_METER_INFO", where1, Model2, new List<string> { "MD_OTHER_4" });
        //            }
        //            //再次进行检定模式切换
        //            TestModel = MeterProtocolAdapter.Instance.IOTMete_SetMeterTestModel((byte)PulseModel, Power, Frequency, count);
        //            for (int i = 0; i < MeterNumber; i++)
        //            {
        //                if (!meterInfo[i].YaoJianYn) continue;
        //                resoult2[i] = TestModel[i] == 0x00 ? true : false;
        //            }
        //        }

        //        for (int i = 0; i < MeterNumber; i++)
        //        {
        //            if (!meterInfo[i].YaoJianYn) continue;
        //            if (!resoult2[i]) tips += $"{i + 1}";
        //        }
        //        if (tips != "")
        //        {
        //            MessageAdd($"切换物联表表位{tips}切换到切换到【{PulseModel}】检定模式失败", EnumLogType.错误信息);
        //            tips = "";
        //        }
        //        Thread.Sleep(100);
        //    }
        //    //表发射功率
        //    Power = (byte)((BluetoothModule_TransmitPower)Enum.Parse(typeof(BluetoothModule_TransmitPower), ConfigHelper.Instance.Bluetooth_BluetoothTransmitPower));
        //    //通信模式
        //    byte conntype = (byte)((BluetoothModule_CommunicationMode)Enum.Parse(typeof(BluetoothModule_CommunicationMode), ConfigHelper.Instance.Bluetooth_CommunicationMode));
        //    MessageAdd($"正在转换器到切换到【{PulseModel}】检定模式", EnumLogType.提示信息);
        //    resoult3 = MeterProtocolAdapter.Instance.IOTMete_SetConverterModel((byte)PulseModel, Power, conntype);
        //    for (int i = 0; i < MeterNumber; i++)
        //    {
        //        if (!meterInfo[i].YaoJianYn) continue;
        //        if (!resoult3[i]) tips += $"{i + 1}";
        //    }
        //    if (tips != "")
        //    {
        //        MessageAdd($"表位{tips}切换到光电脉冲失败", EnumLogType.错误信息);
        //        tips = "";
        //    }
        //    for (int i = 0; i < MeterNumber; i++)
        //    {
        //        if (!meterInfo[i].YaoJianYn) continue;
        //        if (!resoult[i] && !resoult2[i] && !resoult3[i])   //有处理失败的
        //        {
        //            ItoControlType = -1;
        //            break;
        //        }

        //    }

        //}

        ///// <summary>
        ///// 退出检定模式
        ///// </summary>
        //public void OutITOTestModel()
        //{
        //    if (ConfigHelper.Instance.PulseType == "无") return;
        //    bool[] resoult;
        //    byte[] resoult2;

        //    byte Power;
        //    if (ConfigHelper.Instance.IsITOMeter) //物联表需要退出检定模式
        //    {
        //        //表发射功率
        //        Power = (byte)((BluetoothModule_TransmitPower)Enum.Parse(typeof(BluetoothModule_TransmitPower), ConfigHelper.Instance.Bluetooth_MeterTransmitPower));
        //        //表频段
        //        byte Frequency = (byte)((BluetoothModule_TableFrequencyBand)Enum.Parse(typeof(BluetoothModule_TableFrequencyBand), ConfigHelper.Instance.Bluetooth_MeterFrequencyBand));
        //        //表通道数量
        //        byte count = (byte)ConfigHelper.Instance.Bluetooth_MeterPassCount;
        //        resoult2 = MeterProtocolAdapter.Instance.IOTMete_SetMeterTestModel((byte)BluetoothModule_PulseModel.退出检定模式, Power, Frequency, count);
        //        Thread.Sleep(50);
        //    }
        //    //表发射功率
        //    Power = (byte)((BluetoothModule_TransmitPower)Enum.Parse(typeof(BluetoothModule_TransmitPower), ConfigHelper.Instance.Bluetooth_BluetoothTransmitPower));
        //    //通信模式
        //    byte conntype = (byte)((BluetoothModule_CommunicationMode)Enum.Parse(typeof(BluetoothModule_CommunicationMode), ConfigHelper.Instance.Bluetooth_CommunicationMode));
        //    resoult = MeterProtocolAdapter.Instance.IOTMete_SetConverterModel((byte)BluetoothModule_PulseModel.退出检定模式, Power, conntype);
        //}
        //#endregion
        #endregion

        #region 物联表
        /// <summary>
        /// 连接物联表--用于中途关源导致掉线的情况
        /// </summary>
        public void ConnectIOTMeter()
        {
            if (!IsCheckITOMeter) return;
            if (IsDemo || !ConfigHelper.Instance.IsITOMeter) return;
            //这里判断一下功率源

            if (EquipmentData.StdInfo.Ua < 50 && EquipmentData.StdInfo.Ub < 50 && EquipmentData.StdInfo.Uc < 50)
            {
                PowerOn();
                WaitTime("正在升源", 15);
            }


            //这里还需要获取地址来连接蓝牙表
            string[] address = new string[MeterNumber];
            for (int i = 0; i < MeterNumber; i++)
            {
                if (!MeterInfo[i].YaoJianYn) continue;
                address[i] = MeterInfo[i].MD_PostalAddress;
            }
            bool[] resoult = MeterProtocolAdapter.Instance.IOTMete_Connect(address, ConfigHelper.Instance.Bluetooth_Ping);
            bool reCont = false;
            for (int i = 0; i < MeterNumber; i++)
            {
                if (!MeterInfo[i].YaoJianYn) continue;
                if (resoult[i] == false)
                {
                    reCont = true;  //代表有表断开连接了
                    break;
                }
            }
            //开始重连
            if (reCont)
            {
                MessageAdd("有表位蓝牙连接中断,开始重连", EnumLogType.流程信息);

                bool[] yaojian = new bool[MeterNumber];
                for (int i = 0; i < MeterNumber; i++)
                    yaojian[i] = MeterInfo[i].YaoJianYn;//保存要检定标记
                for (int i = 0; i < MeterNumber; i++)
                {
                    if (!MeterInfo[i].YaoJianYn) continue;
                    if (resoult[i]) MeterInfo[i].YaoJianYn = false;   //只需要对有问题的重连就行
                }
                MeterProtocolAdapter.Instance.IOTMete_Reset();
                for (int index = 0; index < 5; index++)
                {
                    bool[] resoult2 = MeterProtocolAdapter.Instance.IOTMete_Connect(address, ConfigHelper.Instance.Bluetooth_Ping);
                    for (int i = 0; i < MeterNumber; i++)
                    {
                        if (!MeterInfo[i].YaoJianYn) continue;
                        if (resoult2[i])
                        {
                            resoult[i] = true;
                            MeterInfo[i].YaoJianYn = false;   //只需要对有问题的重连
                        }
                    }

                    string err = "";
                    for (int i = 0; i < MeterNumber; i++)
                    {
                        if (!MeterInfo[i].YaoJianYn) continue;
                        if (!resoult[i])
                        {
                            err += $"{i + 1},";
                        }
                    }
                    if (string.IsNullOrWhiteSpace(err)) break;
                    MessageAdd($"表位：{err}第{index + 1}次蓝牙连接失败,开始重连", EnumLogType.流程信息);
                }

                for (int i = 0; i < MeterNumber; i++)
                    MeterInfo[i].YaoJianYn = yaojian[i];
            }

            IsCheckITOMeter = false;
        }


        /// <summary>
        /// 设置蓝牙模块的数据,0/6 P,1/7/8 Q,4 T
        /// </summary>
        public void SetBluetoothModule(int ControlType)
        {
            if (ConfigHelper.Instance.PulseType == "无") return;

            BluetoothModule_PulseModel PulseModel = BluetoothModule_PulseModel.秒脉冲输出;
            switch (ControlType)
            {
                case 0:
                case 6:
                    PulseModel = BluetoothModule_PulseModel.有功脉冲;
                    break;
                case 1:
                case 7:
                case 8:
                    PulseModel = BluetoothModule_PulseModel.无功脉冲;
                    break;
                case 4:
                    PulseModel = BluetoothModule_PulseModel.秒脉冲输出;
                    break;
                default:
                    break;
            }
            if (ItoControlType == (int)PulseModel) return;
            ItoControlType = (int)PulseModel;
            bool[] resoult = new bool[MeterNumber]; //切换光电脉冲
            bool[] resoult3 = new bool[MeterNumber]; //切换模块的检定模式
            //byte[] resoult2 = new byte[MeterNumber]; //切换物联表检定模式
            string tips = "";
            resoult.Fill(true);
            resoult3.Fill(true);


            byte Power;
            if (OneMeterInfo.Other3 == "光电脉冲") //光脉冲--光脉冲只需要切换模块并且给发送报文切换电表即可
            {
                MessageAdd($"切换到光电脉冲", EnumLogType.提示信息);
                for (int re = 0; re < 2; re++)
                {
                    resoult = MeterProtocolAdapter.Instance.IOTMete_SetLightPulse((byte)PulseModel); //需要切换到光电脉冲模式
                    tips = "";
                    for (int i = 0; i < MeterNumber; i++)
                    {
                        if (!MeterInfo[i].YaoJianYn) continue;
                        if (!resoult[i]) tips += $"{i + 1}";
                    }
                    if (string.IsNullOrWhiteSpace(tips)) break;
                }

                if (tips != "")
                {
                    MessageAdd($"表位{tips}切换到光电脉冲失败", EnumLogType.错误信息);
                    MessageAdd($"表位{tips}切换到光电脉冲失败", EnumLogType.提示与流程信息);
                }
                if (PulseModel == BluetoothModule_PulseModel.无功脉冲 || PulseModel == BluetoothModule_PulseModel.秒脉冲输出)//有功不需要切换
                {
                    if (ConfigHelper.Instance.IsITOMeter)
                    {
                        MeterProtocolAdapter.Instance.SetPulseCom((byte)PulseModel);
                    }
                }
                if (PulseModel == BluetoothModule_PulseModel.无功脉冲)
                {
                    if (ConfigHelper.Instance.IsITOMeter)
                    {
                        MeterProtocolAdapter.Instance.SetPulseCom((byte)PulseModel);
                    }
                }
                else if (PulseModel == BluetoothModule_PulseModel.秒脉冲输出)//有功不需要切换
                {
                    if (ConfigHelper.Instance.IsITOMeter)
                    {
                        MeterProtocolAdapter.Instance.SetPulseCom((byte)PulseModel);
                    }
                    else
                    {
                        MeterProtocolAdapter.Instance.IOTMete_SetLightPulse((byte)BluetoothModule_PulseModel.退出检定模式);
                    }
                }

            }
            else
            {
                if (ConfigHelper.Instance.IsITOMeter)//物联网表才需要切换检定模式
                {
                    //表发射功率
                    Power = (byte)(BluetoothModule_TransmitPower)Enum.Parse(typeof(BluetoothModule_TransmitPower), ConfigHelper.Instance.Bluetooth_MeterTransmitPower);
                    //表频段
                    byte Frequency = (byte)(BluetoothModule_TableFrequencyBand)Enum.Parse(typeof(BluetoothModule_TableFrequencyBand), ConfigHelper.Instance.Bluetooth_MeterFrequencyBand);
                    //表通道数量
                    byte count = (byte)ConfigHelper.Instance.Bluetooth_MeterPassCount;
                    //切换电表的检定模式
                    MessageAdd($"正在切换物联表到【{PulseModel}】检定模式", EnumLogType.提示信息);

                    byte[] TestModel = MeterProtocolAdapter.Instance.IOTMete_SetMeterTestModel((byte)PulseModel, Power, Frequency, count);
                    bool[] yaojian = new bool[MeterNumber];
                    for (int i = 0; i < MeterNumber; i++)
                    {
                        if (!MeterInfo[i].YaoJianYn || TestModel[i] == 0x00) continue;
                        //  切换失败的在切换一次
                        yaojian[i] = true;
                    }
                    if (Array.IndexOf(yaojian, true) > -1)
                    {
                        byte[] resoult2 = MeterProtocolAdapter.Instance.IOTMete_SetMeterTestModel((byte)PulseModel, Power, Frequency, count, yaojian);

                        for (int i = 0; i < MeterNumber; i++)
                        {
                            if (!MeterInfo[i].YaoJianYn || !yaojian[i]) continue;
                            TestModel[i] = resoult2[i];
                        }
                    }
                    #region //预处理部分


                    //List<int>PretreatmentSelectID = new List<int>();//需要预处理查询的表位ID
                    //for (int i = 0; i<MeterNumber; i++)
                    //{
                    //    if (!meterInfo[i].YaoJianYn&&TestModel[i] == 0x00) continue;
                    //    if (TestModel[i] == 0x01) //切换失败的,判断一下是不是没有预处理过
                    //    {
                    //        if (meterInfo[i].Other4 == null || meterInfo[i].Other4 == "" || meterInfo[i].Other4 == "未处理")
                    //        {
                    //            MeterProtocolAdapter.Instance.IOTMete_Pretreatment(i + 1); //进行蓝牙预处理
                    //            PretreatmentSelectID.Add(i);
                    //        }
                    //    } 
                    //    else if (TestModel[i] == 0x03)//未授权
                    //    {
                    //        meterInfo[PretreatmentSelectID[i]].Other4 = "未授权";
                    //        string value = meterInfo[PretreatmentSelectID[i]].Other4;
                    //        EquipmentData.MeterGroupInfo.Meters[i].SetProperty("MD_OTHER_4", value);
                    //        DAL.DynamicModel Model2 = new DAL.DynamicModel();
                    //        Model2.SetProperty("MD_OTHER_4", value);
                    //        string where1 = $"METER_ID = '{meterInfo[PretreatmentSelectID[i]].Meter_ID}'";
                    //        DAL.DALManager.MeterTempDbDal.Update("T_TMP_METER_INFO", where1, Model2, new List<string> { "MD_OTHER_4" });
                    //    }
                    //}
                    //bool[] yaojian = new bool[MeterNumber];
                    //if (PretreatmentSelectID.Count> 0)
                    //{
                    //    WaitTime("预处理", 35);  //预处理需要等待35秒
                    //    for (int i = 0; i<PretreatmentSelectID.Count; i++)
                    //    {
                    //        if (MeterProtocolAdapter.Instance.IOTMete_PretreatmentSelect(PretreatmentSelectID[i] + 1)) //预处理成功
                    //        {
                    //        }
                    //        else    //预处理失败
                    //        {
                    //        }
                    //        meterInfo[PretreatmentSelectID[i]].Other4 = "已处理";
                    //        string value = meterInfo[PretreatmentSelectID[i]].Other4;
                    //        EquipmentData.MeterGroupInfo.Meters[i].SetProperty("MD_OTHER_4", value);
                    //        DAL.DynamicModel Model2 = new DAL.DynamicModel();
                    //        Model2.SetProperty("MD_OTHER_4", value);
                    //        string where1 = $"METER_ID = '{meterInfo[PretreatmentSelectID[i]].Meter_ID}'";
                    //        DAL.DALManager.MeterTempDbDal.Update("T_TMP_METER_INFO", where1, Model2, new List<string> { "MD_OTHER_4" });
                    //        yaojian[PretreatmentSelectID[i]] = true;
                    //    }
                    //    //再次进行检定模式切换
                    //    TestModel = MeterProtocolAdapter.Instance.IOTMete_SetMeterTestModel((byte)PulseModel, Power, Frequency, count, yaojian);
                    //    for (int i = 0; i<MeterNumber; i++)
                    //    {
                    //        if (!meterInfo[i].YaoJianYn) continue;
                    //        resoult2[i] = TestModel[i] == 0x00 ? true : false;
                    //    }
                    //}

                    #endregion

                    for (int i = 0; i < MeterNumber; i++)
                    {
                        if (!MeterInfo[i].YaoJianYn) continue;
                        if (TestModel[i] != 0x00) tips += $"【{i + 1}】";
                    }
                    if (tips != "")
                    {
                        MessageAdd($"切换物联表表位{tips}切换到【{PulseModel}】检定模式失败", EnumLogType.错误信息);
                    }
                    Thread.Sleep(1000);
                }

                //表发射功率
                Power = (byte)(BluetoothModule_TransmitPower)Enum.Parse(typeof(BluetoothModule_TransmitPower), ConfigHelper.Instance.Bluetooth_BluetoothTransmitPower);
                //通信模式
                byte conntype = (byte)(BluetoothModule_CommunicationMode)Enum.Parse(typeof(BluetoothModule_CommunicationMode), ConfigHelper.Instance.Bluetooth_CommunicationMode);
                MessageAdd($"正在切换转换器到【{PulseModel}】检定模式", EnumLogType.提示信息);

                tips = "";
                resoult3 = MeterProtocolAdapter.Instance.IOTMete_SetConverterModel((byte)PulseModel, Power, conntype);
                bool[] reTrys = new bool[MeterNumber];
                for (int i = 0; i < MeterNumber; i++)
                {
                    if (!MeterInfo[i].YaoJianYn) continue;
                    if (!resoult3[i]) reTrys[i] = true;
                }
                if (Array.IndexOf(reTrys, true) >= 0)
                {
                    bool[] tmpRst = MeterProtocolAdapter.Instance.IOTMete_SetConverterModel((byte)PulseModel, Power, conntype, reTrys);
                    for (int i = 0; i < MeterNumber; i++)
                    {
                        if (!MeterInfo[i].YaoJianYn) continue;
                        if (reTrys[i]) resoult3[i] = tmpRst[i];
                    }
                }
                for (int i = 0; i < MeterNumber; i++)
                {
                    if (!MeterInfo[i].YaoJianYn) continue;
                    if (!resoult3[i]) tips += $"【{i + 1}】";
                }
                if (tips != "")
                {
                    MessageAdd($"转换器{tips}切换到【{PulseModel}】检定模式失败", EnumLogType.错误信息);
                }

                for (int i = 0; i < MeterNumber; i++)
                {
                    if (!MeterInfo[i].YaoJianYn) continue;
                    if (!resoult[i] && !resoult3[i])   //有处理失败的
                    {
                        ItoControlType = -1;
                        break;
                    }

                }

            }


        }

        /// <summary>
        /// 退出检定模式--并复位物联表
        /// </summary>
        public void OutITOTestModelInitIto()
        {
            if (ItoControlType == -1) return;//第一个检定项目就是通讯的，不需要复位
            bool[] resoult;
            byte[] resoult2;
            byte Power;
            if (ConfigHelper.Instance.IsITOMeter && ConfigHelper.Instance.PulseType == "蓝牙脉冲")
            {
                MessageAdd("通讯模式发送改变,开始复位", EnumLogType.流程信息);
                ItoControlType = -1;
                //表发射功率
                Power = (byte)(BluetoothModule_TransmitPower)Enum.Parse(typeof(BluetoothModule_TransmitPower), ConfigHelper.Instance.Bluetooth_MeterTransmitPower);
                //表频段
                byte Frequency = (byte)(BluetoothModule_TableFrequencyBand)Enum.Parse(typeof(BluetoothModule_TableFrequencyBand), ConfigHelper.Instance.Bluetooth_MeterFrequencyBand);
                //表通道数量
                byte count = (byte)ConfigHelper.Instance.Bluetooth_MeterPassCount;
                MessageAdd("正在退出表检定模式", EnumLogType.提示信息);
                resoult2 = MeterProtocolAdapter.Instance.IOTMete_SetMeterTestModel((byte)BluetoothModule_PulseModel.退出检定模式, Power, Frequency, count);
                bool all = true;
                for (int i = 0; i < resoult2.Length; i++)
                {
                    if (!MeterInfo[i].YaoJianYn) continue;
                    if (resoult2[i] != 0)
                    {
                        all = false;
                        break;
                    }
                }
                if (!all)
                {
                    bool[] reTrys = new bool[MeterNumber];
                    for (int i = 0; i < MeterNumber; i++)
                    {
                        if (!MeterInfo[i].YaoJianYn) continue;
                        if (resoult2[i] != 0) reTrys[i] = true;
                    }
                    MeterProtocolAdapter.Instance.IOTMete_SetMeterTestModel((byte)BluetoothModule_PulseModel.退出检定模式, Power, Frequency, count, reTrys);
                }
                Thread.Sleep(50);
                ////表发射功率
                //Power = (byte)((BluetoothModule_TransmitPower)Enum.Parse(typeof(BluetoothModule_TransmitPower), ConfigHelper.Instance.Bluetooth_BluetoothTransmitPower));
                ////通信模式
                //byte conntype = (byte)((BluetoothModule_CommunicationMode)Enum.Parse(typeof(BluetoothModule_CommunicationMode), ConfigHelper.Instance.Bluetooth_CommunicationMode));
                //resoult = MeterProtocolAdapter.Instance.IOTMete_SetConverterModel((byte)BluetoothModule_PulseModel.退出检定模式, Power, conntype);
                MessageAdd("正在复位模块", EnumLogType.提示信息);
                resoult = MeterProtocolAdapter.Instance.IOTMete_Reset();
                all = true;
                string[] address = new string[MeterNumber];
                for (int i = 0; i < MeterNumber; i++)
                {
                    if (!MeterInfo[i].YaoJianYn) continue;
                    address[i] = MeterInfo[i].MD_PostalAddress;
                    if (!resoult[i]) all = false;
                }
                if (!all)
                {
                    bool[] reTrys = new bool[MeterNumber];
                    for (int i = 0; i < MeterNumber; i++)
                    {
                        if (!MeterInfo[i].YaoJianYn) continue;
                        if (!resoult[i]) reTrys[i] = true;
                    }
                    MeterProtocolAdapter.Instance.IOTMete_Reset(reTrys);
                }

                MessageAdd("正在连接物联表", EnumLogType.提示信息);
                bool[] resoult3 = MeterProtocolAdapter.Instance.IOTMete_Connect(address, ConfigHelper.Instance.Bluetooth_Ping);
                all = true;
                for (int i = 0; i < MeterNumber; i++)
                {
                    if (!MeterInfo[i].YaoJianYn) continue;
                    if (!resoult3[i]) all = false;
                }
                if (!all)
                {
                    bool[] reTrys = new bool[MeterNumber];
                    for (int i = 0; i < MeterNumber; i++)
                    {
                        if (!MeterInfo[i].YaoJianYn) continue;
                        if (!resoult3[i]) reTrys[i] = true;
                    }
                    bool[] resoulttmp = MeterProtocolAdapter.Instance.IOTMete_Connect(address, reTrys, ConfigHelper.Instance.Bluetooth_Ping);
                    //这里开始重新连接物联表
                    for (int i = 0; i < MeterNumber; i++)
                    {
                        if (!MeterInfo[i].YaoJianYn) continue;
                        if (resoulttmp[i]) resoult3[i] = true;
                    }
                }
            }
        }
        #endregion


        #region 跟随上报和主动上报

        #endregion

        public VerifyBase()
        {
            MeterNumber = MeterInfo.Length;
            // 载波修改部分
            App.MeterNumber = MeterNumber;
        }

        #region UI

        //刷新界面上的检定标记
        public void RefMeterYaoJian()
        {
            for (int i = 0; i < MeterNumber; i++)
            {
                string value = MeterInfo[i].YaoJianYn ? "1" : "0";
                EquipmentData.MeterGroupInfo.Meters[i].SetProperty("MD_CHECKED", value);
                DAL.DynamicModel Model2 = new DAL.DynamicModel();
                Model2.SetProperty("MD_CHECKED", value);
                string id = EquipmentData.MeterGroupInfo.Meters[i].GetProperty("METER_ID") as string;
                string where1 = $"METER_ID = '{id}'";
                DAL.DALManager.MeterTempDbDal.Update("T_TMP_METER_INFO", where1, Model2, new List<string> { "MD_CHECKED" });
            }
            EquipmentData.CheckResults.RefreshYaojian();
        }


        /// <summary>
        /// 获得表位的结论
        /// </summary>
        /// <param name="meterNo">表位编号，从0开始</param>
        /// <returns></returns>
        public bool GetMeterResult(int meterNo, string exceptId = "")
        {
            return EquipmentData.CheckResults.GetMeterResult(meterNo, exceptId);
        }

        /// <summary>
        /// 检查检定合格状态
        /// </summary>
        /// <param name="meterNo"></param>
        /// <param name="exceptId"></param>
        /// <returns></returns>
        public bool GetMeterResultExceptMyself(int meterNo, string exceptId = "")
        {
            return EquipmentData.CheckResults.GetMeterResultExceptMyself(meterNo, exceptId);
        }

        /// <summary>
        /// 刷新UI数据--
        /// </summary>
        /// <param name="testNo">检定点编号</param>
        /// <param name="columnName">详细数据列名</param>
        /// <param name="value">值</param>
        public void RefUIData(string testNo, string columnName, string[] value)
        {
            EquipmentData.CheckResults.UpdateCheckResult(testNo, columnName, value);
        }
        public void RefUIData(string columnName, string[] value)
        {
            EquipmentData.CheckResults.UpdateCheckResult(Test_No, columnName, value);
        }
        public void RefUIData(string columnName)
        {
            if (ResultDictionary.ContainsKey(columnName))
            {
                EquipmentData.CheckResults.UpdateCheckResult(Test_No, columnName, ResultDictionary[columnName]);
            }
            else if (ConstHelper.检定界面显示实时误差.Equals(columnName))
            {
                EquipmentData.CheckResults.UpdateCheckResult(Test_No, columnName, new string[0]);
            }
            else
            {
                MessageAdd($"未配置的结论名称:{columnName}", EnumLogType.流程信息);
            }
        }

        //modify yjt 20220822 合并蒋工的代码
        public Dictionary<string, string[]> GetCheckResult()
        {
            return EquipmentData.CheckResults.GetCheckResult();
        }
        /// <summary>
        /// 日志提示
        /// </summary>
        /// <param name="Tips">日志内容</param>
        /// <param name="logType">日志类型</param>
        public void MessageAdd(string Tips, EnumLogType logType = EnumLogType.提示信息)
        {
            EquipmentData.Controller.MessageAdd(Tips, logType);
        }
        /// <summary>
        /// 异常停止检定
        /// </summary>
        public void TryStopTest()
        {
            EquipmentData.Controller.TryStopVerify();
        }


        public void WaitTime(string tips, int seconds)
        {
            while (seconds > 0)
            {
                MessageAdd($"{tips},等待{seconds}秒...", EnumLogType.提示信息);
                Thread.Sleep(1000);
                seconds--;
                if (IsDemo) return;
                if (Stop) return;
            }
        }


        #endregion

        #region 设备控制

        //读标准表电量
        /// <summary>
        /// 读走字数据
        /// </summary>
        /// <param name="TotalEnergy"></param>
        /// <param name="PulseNum"></param>
        /// <returns></returns>
        public bool ReadStdZZData(out float TotalEnergy, out long PulseNum)
        {
            return DeviceControl.ReadStdZZData(out TotalEnergy, out PulseNum);
        }

        /// <summary>
        ///【标准表】 读取累积电量,kWh; 
        /// 9有功总电量，10无功总电量
        /// </summary>
        /// <returns >返回值float数组</returns>
        public float[] ReadEnergy()
        {
            if (IsDemo) return null;
            return DeviceControl.ReadEnergy();
        }

        //add yjt 20230103 新增旧版本标准表的读取标准表电量
        /// <summary>
        /// 读取标准表电量，旧版本的标准表,ws
        /// </summary>
        /// <returns></returns>
        public string[] ReadEnrgyZh311()
        {
            return DeviceControl.ReadEnrgyZh311();
        }

        private static DeviceControlS deviceControl;
        public static DeviceControlS DeviceControl
        {
            get
            {
                if (deviceControl == null)
                {
                    deviceControl = new DeviceControlS();
                }
                return deviceControl;
            }
        }

        #region 设备反射
        /// <summary>
        /// 发送设备命令，反射的方法
        /// </summary>
        /// <param name="type">库类型</param>
        /// <param name="obj">实例</param>
        /// <param name="functionName">方法名称</param>
        /// <param name="value">参数数组</param>
        /// <returns></returns>
        public object SendDeviceControl(Type type, object obj, string functionName, params object[] value)
        {
            return DeviceControl.DeviceControl(type, obj, functionName, value);


        }

        /// <summary>
        /// 发送设备命令，反射的方法
        /// </summary>
        /// <param name="type">库类型</param>
        /// <param name="obj">实例</param>
        /// <param name="functionName">方法名称</param>
        /// <param name="value">参数数组</param>
        /// <returns></returns>
        public object[] SendDeviceControl2(bool[] t, string functionName, params object[] value)
        {
            object[] returnObj = new object[MeterNumber];
            DeviceThreadManager.Instance.MaxThread = MeterNumber;
            DeviceThreadManager.Instance.MaxTaskCountPerThread = 1;
            if (functionName == "SetPulseCalibration")

            {
                //string[] WcString =(string[]) value[0]; //参数0是字符串数组，是所有表的误差
                DeviceThreadManager.Instance.DoWork = delegate (int ID, int pos)
                {
                    if (Stop) return;
                    if (t[pos] && value[pos] != null)
                    {
                        returnObj[pos] = DeviceControl.DeviceControl(MeterInfo[pos].Type, MeterInfo[pos].Obj, functionName, value[pos]);

                    }
                };
                DeviceThreadManager.Instance.Start();
                WaitWorkDone();
            }
            else
            {
                DeviceThreadManager.Instance.DoWork = delegate (int ID, int pos)
                {
                    if (Stop) return;
                    if (t[pos])
                    {
                        returnObj[pos] = DeviceControl.DeviceControl(MeterInfo[pos].Type, MeterInfo[pos].Obj, functionName, value);
                    }
                };
                DeviceThreadManager.Instance.Start();
                WaitWorkDone();

            }
            return returnObj;
        }


        /// <summary>
        /// 等待所有线程完成
        /// </summary>
        public void WaitWorkDone()
        {
            while (true)
            {
                if (Stop) break;
                if (DeviceThreadManager.Instance.IsWorkDone())
                {
                    break;
                }
                Thread.Sleep(50);
            }
        }
        #endregion

        #region 功率源
        /// 关源
        /// </summary>
        public bool PowerOff()
        {
            if (IsDemo) return true;
            EquipmentData.IsHplcNet = false;
            CheckPowerOff(0, 0, 0, Cus_PowerYuanJian.H, FangXiang, "1.0");
            return DeviceControl.PowerOff();

        }
        private static float temIa = 0f;
        private static float temIb = 0f;
        private static float temIc = 0f;
        //private Cus_PowerYuanJian tem_ele;
        //private PowerWay tem_pd;
        //private string tem_pf;


        /// <summary>
        /// 自由升源，同时处理标准表
        /// </summary>
        /// <param name="jxfs">接线方式</param>
        /// <param name="Ua"></param>
        /// <param name="Ub"></param>
        /// <param name="Uc"></param>
        /// <param name="Ia"></param>
        /// <param name="Ib"></param>
        /// <param name="Ic"></param>
        /// <param name="PhiUa"></param>
        /// <param name="PhiUb"></param>
        /// <param name="PhiUc"></param>
        /// <param name="PhiIa"></param>
        /// <param name="PhiIb"></param>
        /// <param name="PhiIc"></param>
        /// <param name="Freq">频率</param>
        /// <param name="Mode">1</param>
        public bool PowerOn(float Ua, float Ub, float Uc, float Ia, float Ib, float Ic, Cus_PowerYuanJian ele, PowerWay glfx, string glys)
        {
            if (IsDemo) return true;

            CheckPowerOff(Ia, Ib, Ic, ele, glfx, glys);//需要关源

            ulong constants = EquipmentData.DeviceManager.GetStdConst(Math.Max(Ia, Math.Max(Ib, Ic)));
            if (!VerifyConfig.FixedConstant) constants = 0;

            if (ConfigHelper.Instance.AutoGearTemp && !VerifyConfig.AutoGear)
            {
                StdGear(0x13, constants, OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, Ia, Ib, Ic);
                WaitTime("正在设置电流挡位", 2);
            }


            if (VerifyConfig.FixedConstant && !VerifyConfig.Test_ZouZi_Control)     //固定常数
            {
                //1:设置标准表挡位、常数
                MessageAdd("正在设置标准表常数...", EnumLogType.提示信息);
                StdGear(0x13, constants, OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, Ia, Ib, Ic);
            }

            if (DeviceControl.PowerOn(Ua, Ub, Uc, Ia, Ib, Ic, ele, glfx, glys))
            {
                WaitTime("升源成功，等待源稳定", ConfigHelper.Instance.Dgn_PowerSourceStableTime);

            }
            bool t = CheckPowerOn(Ua, Ub, Uc, Ia, Ib, Ic, ele, glfx, glys);
            if (!t)
            {
                string data = $"升源参数:电压【A:{Ua}】【B:{Ub}】【C:{Uc}】";
                data += $"\r\n电流【A:{Ia}】【B:{Ib}】【C:{Ic}】";
                data += $"\r\n元件【{ele}】";
                data += $"\r\n方向【{glfx}】";
                data += $"\r\n功率因数【{glys}】";
                MessageAdd(data, EnumLogType.详细信息);
            }
            return t;
        }

        private bool CheckPowerOn(float Ua, float Ub, float Uc, float Ia, float Ib, float Ic, Cus_PowerYuanJian ele, PowerWay glfx, string glys)
        {
            bool IsPower = true;
            for (int i = 0; i < 2; i++)
            {
                if (ele == Cus_PowerYuanJian.H)
                {
                    if (Ia > 0 && EquipmentData.StdInfo.Ia < 0.001)
                        IsPower = false;
                    if (Ib > 0 && EquipmentData.StdInfo.Ib < 0.001 && Clfs == WireMode.三相四线)
                        IsPower = false;
                    if (Ic > 0 && EquipmentData.StdInfo.Ic < 0.001 && (Clfs == WireMode.三相四线 || Clfs == WireMode.三相三线))
                        IsPower = false;
                }
                else if (ele == Cus_PowerYuanJian.A)
                {
                    if (Ia > 0 && EquipmentData.StdInfo.Ia < 0.001) IsPower = false;
                }
                else if (ele == Cus_PowerYuanJian.B)
                {
                    if (Ib > 0 && EquipmentData.StdInfo.Ib < 0.001) IsPower = false;
                }
                else if (ele == Cus_PowerYuanJian.C)
                {
                    if (Ic > 0 && EquipmentData.StdInfo.Ic < 0.001) IsPower = false;
                }

                if (!IsPower)
                {
                    EquipmentData.DeviceManager.ClearOL();
                    IsPower = DeviceControl.PowerOn(Ua, Ub, Uc, Ia, Ib, Ic, ele, glfx, glys);
                    WaitTime("电源电流不正确，重新升源", ConfigHelper.Instance.Dgn_PowerSourceStableTime);
                }

            }
            return IsPower;
        }

        /// <summary>
        /// 特殊检定升源
        /// </summary>
        /// <param name="Ua"></param>
        /// <param name="Ub"></param>
        /// <param name="Uc"></param>
        /// <param name="Ia"></param>
        /// <param name="Ib"></param>
        /// <param name="Ic"></param>
        /// <param name="ele"></param>
        /// <param name="feq"></param>
        /// <param name="glys"></param>
        /// <param name="nxx"></param>
        /// <param name="glfx"></param>
        /// <returns></returns>
        public bool PowerOn(float Ua, float Ub, float Uc, float Ia, float Ib, float Ic, Cus_PowerYuanJian ele, float feq, string glys, Cus_PowerPhase nxx, PowerWay glfx)
        {
            if (IsDemo) return true;
            bool t;

            CheckPowerOff(Ia, Ib, Ic, ele, glfx, glys);//需要关源

            t = DeviceControl.PowerOn(Ua, Ub, Uc, Ia, Ib, Ic, ele, feq, glys, nxx, glfx);
            if (t)
            {
                WaitTime("升源成功，等待源稳定", DAL.Config.ConfigHelper.Instance.Dgn_PowerSourceStableTime);
            }
            bool IsPower = true;

            if (ele == Cus_PowerYuanJian.H)
            {
                if (Ia > 0 && EquipmentData.StdInfo.Ia < 0.001) IsPower = false;
                if (Ib > 0 && EquipmentData.StdInfo.Ib < 0.001 && Clfs == WireMode.三相四线) IsPower = false;
                if (Ic > 0 && EquipmentData.StdInfo.Ic < 0.001 && (Clfs == WireMode.三相四线 || Clfs == WireMode.三相三线)) IsPower = false;
            }
            else if (ele == Cus_PowerYuanJian.A)
            {
                if (Ia > 0 && EquipmentData.StdInfo.Ia < 0.001) IsPower = false;
            }
            else if (ele == Cus_PowerYuanJian.B)
            {
                if (Ib > 0 && EquipmentData.StdInfo.Ib < 0.001) IsPower = false;
            }
            else if (ele == Cus_PowerYuanJian.C)
            {
                if (Ic > 0 && EquipmentData.StdInfo.Ic < 0.001) IsPower = false;
            }

            if (!IsPower)
            {
                WaitTime("电源电流不正确,准备重新升源", 5);
                t = DeviceControl.PowerOn(Ua, Ub, Uc, Ia, Ib, Ic, ele, glfx, glys);
                WaitTime("电源电流不正确，重新升源", DAL.Config.ConfigHelper.Instance.Dgn_PowerSourceStableTime);
            }
            return t;
        }

        //add yjt 20220805 影响量升源
        /// <summary>
        /// 自由升源
        /// </summary>
        /// <param name="jxfs">接线方式</param>
        /// <param name="Ua"></param>
        /// <param name="Ub"></param>
        /// <param name="Uc"></param>
        /// <param name="Ia"></param>
        /// <param name="Ib"></param>
        /// <param name="Ic"></param>
        /// <param name="PhiUa"></param>
        /// <param name="PhiUb"></param>
        /// <param name="PhiUc"></param>
        /// <param name="PhiIa"></param>
        /// <param name="PhiIb"></param>
        /// <param name="PhiIc"></param>
        /// <param name="Freq">频率</param>
        /// <param name="Mode">1</param>
        public bool PowerOn(float Ua, float Ub, float Uc, float Ia, float Ib, float Ic, Cus_PowerYuanJian ele, PowerWay glfx, string glys, float PL = 50,
            Cus_PowerPhase phaseSequenceType = Cus_PowerPhase.正相序)
        {
            if (IsDemo) return true;

            CheckPowerOff(Ia, Ib, Ic, ele, glfx, glys);

            bool t = DeviceControl.PowerOn(Ua, Ub, Uc, Ia, Ib, Ic, ele, glfx, glys, PL, phaseSequenceType);
            if (t)
            {
                WaitTime("升源成功，等待源稳定", DAL.Config.ConfigHelper.Instance.Dgn_PowerSourceStableTime);
            }
            return t;
        }

        /// <summary>
        /// 改变相位角
        /// </summary>
        /// <param name="Ua"></param>
        /// <param name="Ub"></param>
        /// <param name="Uc"></param>
        /// <param name="Ia"></param>
        /// <param name="Ib"></param>
        /// <param name="Ic"></param>
        /// <param name="PhiUa"></param>
        /// <param name="PhiUb"></param>
        /// <param name="PhiUc"></param>
        /// <param name="PhiIa"></param>
        /// <param name="PhiIb"></param>
        /// <param name="PhiIc"></param>
        /// <param name="Hz"></param>
        /// <returns></returns>
        public bool PowerOnFree(double Ua, double Ub, double Uc, double Ia, double Ib, double Ic, double PhiUa, double PhiUb, double PhiUc, double PhiIa, double PhiIb, double PhiIc, float Hz)
        {
            string clfs = Clfs.ToString();
            return DeviceControl.PowerOnFree(clfs, Ua, Ub, Uc, Ia, Ib, Ic, PhiUa, PhiUb, PhiUc, PhiIa, PhiIb, PhiIc, Hz);
        }

        private bool CheckPowerOff(float Ia, float Ib, float Ic, Cus_PowerYuanJian ele, PowerWay pd, string pf)
        {
            bool Ioff = false;
            if (temIa != Ia || temIb != Ib || temIc != Ic)//上一次电流和这一次电流不一样，判断是否需要关闭电流
            {
                //电流挡位有不一样的，就需要关闭电流
                if (CurrentGear(Ia) != CurrentGear(temIa) || CurrentGear(Ib) != CurrentGear(temIb) || CurrentGear(Ic) != CurrentGear(temIc))
                {
                    Ioff = true;
                }
                //保存上一次升源数据，用于判断电流挡位是否改变
            }
            if (temIa == 0 && temIb == 0 && temIc == 0)
            {
                Ioff = false;
            }
            if (EquipmentData.StdInfo.Ia > Ia * 1.2
                || EquipmentData.StdInfo.Ib > Ib * 1.2
                || EquipmentData.StdInfo.Ic > Ic * 1.2)
            {
                Ioff = true;
            }
            temIa = Ia;
            temIb = Ib;
            temIc = Ic;
            //tem_ele = ele;
            //tem_pd = pd;
            //tem_pf = pf;
            if (Ioff)
            {
                DeviceControl.PowerOn(OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, 0, 0, 0, ele, pd, pf);
                if (Math.Max(Ia, Math.Max(Ib, Ic)) > 30)
                    WaitTime("正在切换电流", 5);
                else
                    WaitTime("正在切换电流", 3);
            }
            return Ioff;
        }
        /// <summary>
        ///获得功率源电流挡位
        /// </summary>
        /// <returns></returns>
        private int CurrentGear(float I)
        {
            int Grar = 0;
            if (0 <= I && I < 0.0061)
                Grar = 0;
            else if (0.0061 <= I && I < 0.012)
                Grar = 1;
            else if (0.012 <= I && I < 0.031)
                Grar = 2;
            else if (0.031 <= I && I < 0.051)
                Grar = 3;
            else if (0.051 <= I && I < 0.12)
                Grar = 4;
            else if (0.12 <= I && I < 0.22)
                Grar = 5;
            else if (0.22 <= I && I < 1.21)
                Grar = 6;
            else if (1.21 <= I && I < 6.1)
                Grar = 7;
            else if (6.1 <= I && I < 30.01)
                Grar = 8;
            else if (30.01 <= I && I < 125)
                Grar = 9;
            return Grar;
        }

        public bool PowerOn(float xIb)
        {
            bool ret = PowerOn(OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, xIb, xIb, xIb, Cus_PowerYuanJian.H, FangXiang, "1.0");
            if (ret) Thread.Sleep(3000);
            return ret;
        }

        /// <summary>
        ///  升电压不升电流。带演示模式
        /// </summary>
        /// <returns></returns>
        public bool PowerOn()
        {
            if (IsDemo) return true;
            MessageAdd("开始升源...", EnumLogType.提示信息);
            Cus_PowerYuanJian ele = Cus_PowerYuanJian.H;
            if (OneMeterInfo.MD_WiringMode == "单相")
                ele = Cus_PowerYuanJian.A;

            return DeviceControl.PowerOn(OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, 0, 0, 0, ele, FangXiang, "1.0");
        }

        public bool PowerFreeOn(float ua, float ub, float uc, float ia, float ib, float ic, Cus_PowerYuanJian ele, PowerWay glfx, string glys)
        {
            return DeviceControl.PowerOn(ua, ub, uc, ia, ib, ic, ele, glfx, glys);

        }

        /// <summary>
        /// 0.01-0.6-0.8-0.99
        /// </summary>
        /// <param name="percentx"></param>
        /// <returns></returns>
        protected bool CheckVoltage(float percentx = 0.8f)
        {
            if (percentx >= 1) percentx = 0.99f;
            if (percentx <= 0) percentx = 0.01f;

            if (EquipmentData.StdInfo.Ua < U * percentx)
            {
                if (!PowerOn())
                {
                    MessageAdd("控制源输出失败", EnumLogType.提示信息);
                    return false;
                }
                WaitTime("等待源稳定", VerifyConfig.Dgn_PowerSourceStableTime);
            }
            return true;
        }
        /// <summary>
        /// 电压0.01-0.6-0.8-0.99,电流0.0002A
        /// </summary>
        /// <param name="percentx"></param>
        /// <returns></returns>
        protected bool CheckVoltageAndCurrent(float percentx = 0.8f, float currentmaxpA = 0.0002f)
        {
            if (percentx >= 1) percentx = 0.99f;
            if (percentx <= 0) percentx = 0.01f;

            if (EquipmentData.StdInfo.Ua < U * percentx || EquipmentData.StdInfo.Ia > currentmaxpA || EquipmentData.StdInfo.Ib > currentmaxpA || EquipmentData.StdInfo.Ic > currentmaxpA)
            {
                if (!PowerOn())
                {
                    MessageAdd("控制源输出失败", EnumLogType.提示信息);
                    return false;
                }
                WaitTime("等待源稳定", VerifyConfig.Dgn_PowerSourceStableTime);
            }
            return true;
        }

        ////add yjt 20230131 新增负载点电流快速改变模拟电流改变
        ///// <summary>
        ///// 负载点电流快速改变模拟电流改变
        ///// </summary>
        ///// <param name="xIb"></param>
        ///// <param name="ps"></param>
        ///// <returns></returns>
        //public bool PowerOn(float xIb, string[] ps)
        //{
        //    bool ret = DeviceControl.PowerOn(OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, xIb, xIb, xIb, Cus_PowerYuanJian.H, FangXiang, "1.0");
        //    int time = Convert.ToInt32(float.Parse(ps[0]) * 1000);
        //    //MessageAdd("升源等待" + float.Parse(ps[0]) + "秒");
        //    if (ret) Thread.Sleep(time);

        //    //ret = DeviceControl.PowerOn(OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, 0, 0, 0, Cus_PowerYuanJian.H, FangXiang, "1.0");
        //    PowerOff();
        //    time = Convert.ToInt32(float.Parse(ps[1]) * 1000);
        //    //MessageAdd("关源等待" + float.Parse(ps[1]) + "秒");
        //    Thread.Sleep(time);

        //    return ret;
        //}

        //add yjt 20230131 新增负载电流快速变化实验的时间与启动标志
        /// <summary>
        /// 设置负载电流快速变化实验的时间与启动标志
        /// </summary>
        /// <param name="TonTime">开启时间</param>
        /// <param name="ToffTime">关断时间</param>
        /// <param name="strA">标志A</param>
        /// <param name="strB">标志B</param>
        /// <param name="strC">标志C</param>
        /// <returns></returns>
        public bool SetCurrentChangeByPower(int TonTime, int ToffTime, string strA, string strB, string strC)
        {
            return DeviceControl.SetCurrentChangeByPower(TonTime, ToffTime, strA, strB, strC);
        }

        public bool AC_VoltageSagSndInterruption(int TonTime, int ToffTime, int count, int proportion, string strA, string strB, string strC)
        {
            return DeviceControl.AC_VoltageSagSndInterruption(TonTime, ToffTime, count, proportion, strA, strB, strC);
        }

        //add yjt 20230131 新增负载电流快速变化实验的时间与启动标志
        /// <summary>
        /// 负载电流快速变化实验的时间与启动标志 2 两个都可以
        /// </summary>
        /// <param name="Time1"></param>
        /// <param name="Time2"></param>
        /// <param name="Mode"></param>
        /// <returns></returns>
        public bool SetCurrentChangeByPower2(double Time1, double Time2, int Mode)
        {
            return DeviceControl.SetCurrentChangeByPower2(Time1, Time2, Mode);
        }

        /// <summary>
        /// 【功率源】设置启动或停止电能误差计算
        /// </summary>
        /// <param name="StopOrstart">启动标志：一个字节表示，00H表示停止，01H表示启动</param>
        /// <param name="pushType">电能误差计算模式：一个字节表示，00H表示计算有功电能误差，01H表示计算无功电能误差，02表示启动日计时误差</param>
        /// <param name="meterConst">被检表常数</param>
        /// <param name="qs">检定脉冲数（圈数） 2</param>
        /// <returns></returns>
        public bool SetErrorData(int StopOrstart, int pushType, int meterConst, int qs)
        {
            return DeviceControl.SetErrorData(StopOrstart, pushType, meterConst, qs);
        }

        public bool ReadErrorData(out float[] data)
        {
            return DeviceControl.ReadErrorData(out data);

        }


        #endregion

        #region 误差板
        /// <summary>
        /// 设置误差板标准常数
        /// </summary>
        /// <param name="ControlType">控制类型(6组被检:00-有功(含正向、反向，以下同,01-无功(正向、反向，以下同),04-日计时,05-需量）</param>
        /// <param name="constant">常数</param>
        /// <param name="magnification">放大倍数-2就是缩小100倍</param>
        /// <param name="EpitopeNo">表位编号</param>
        public bool SetStandardConst(int ControlType, int constant, int magnification, byte EpitopeNo = 0x01)
        {
            MessageAdd($"下发误差版常数:【常数：{constant}】【控制类型{ControlType}】【表位号{EpitopeNo}】", EnumLogType.详细信息);
            if (IsDemo) return true;
            bool IsMass = EpitopeNo == 0xff;//是否是广播
            bool[] resoult = new bool[MeterNumber];
            resoult.Fill(true);

            int DeviceCount = EquipmentData.DeviceManager.Devices[Device.DeviceName.误差板].Count;
            DeviceThreadManager.Instance.MaxThread = DeviceCount;
            DeviceThreadManager.Instance.MaxTaskCountPerThread = MeterNumber / DeviceCount;
            if (IsMass) DeviceThreadManager.Instance.MaxTaskCountPerThread = 1; //广播的情况
            DeviceThreadManager.Instance.DoWork = delegate (int ID, int pos)
            {
                if (Stop) return;
                if (IsMass) //广播的话 --没有返回值
                {
                    DeviceControl.SetStandardConst(ControlType, constant, magnification, EpitopeNo, ID);
                }
                else
                {
                    //Trace.WriteLine($"设置误差板标准常数[{ID}---{pos}] --- [{DateTime.Now:hh:mm:ss ffff}]");
                    if (MeterInfo[pos].YaoJianYn)//要检定的表才设置
                    {
                        resoult[pos] = DeviceControl.SetStandardConst(ControlType, constant, magnification, (byte)(pos + 1), ID);
                    }
                }

            };
            DeviceThreadManager.Instance.Start();
            WaitWorkDone();
            //有失败的情况
            if (Array.IndexOf(resoult, false) != -1)
            {
                string err = "";
                for (int i = 0; i < resoult.Length; i++)
                {
                    if (!resoult[i])
                    {
                        err += $"{i + 1},";
                    }
                }
                MessageAdd($"表位:{err}误差板标准常数设置失败", EnumLogType.错误信息);
                return false;
            }
            return true;
        }


        /// <summary>
        /// 设置误差板被检常数
        /// </summary>
        /// <param name="ControlType">控制类型(6组被检:00-有功(含正向、反向，以下同,01-无功(正向、反向，以下同),04-日计时,05-需量）</param>
        /// <param name="constant">常数</param>
        /// <param name="magnification">放大倍数-2就是缩小100倍</param>
        /// <param name="qs">圈数</param>
        public bool SetTestedConst(int ControlType, int[] constant, int magnification, int[] qs, byte EpitopeNo = 0x01)
        {
            MessageAdd($"设置误差版被检常数,【常数{constant[0]}】【控制类型{ControlType}】【圈数{qs}】【表位号{EpitopeNo}】", EnumLogType.详细信息);
            if (IsDemo) return true;
            bool IsMass = EpitopeNo == 0xff;//是否是广播
            bool[] resoult = new bool[MeterNumber];
            resoult.Fill(true);

            int DeviceCount = EquipmentData.DeviceManager.Devices[Device.DeviceName.误差板].Count;
            DeviceThreadManager.Instance.MaxThread = DeviceCount;
            DeviceThreadManager.Instance.MaxTaskCountPerThread = MeterNumber / DeviceCount;
            if (IsMass) DeviceThreadManager.Instance.MaxTaskCountPerThread = 1; //广播的情况
            DeviceThreadManager.Instance.DoWork = delegate (int ID, int pos)
            {
                if (Stop) return;
                if (IsMass) //广播的话 --没有返回值
                {
                    DeviceControl.SetTestedConst(ControlType, constant[0], magnification, qs[0], EpitopeNo, ID);
                }
                else
                {
                    if (MeterInfo[pos].YaoJianYn)//要检定的表才设置
                    {
                        resoult[pos] = DeviceControl.SetTestedConst(ControlType, constant[pos], magnification, qs[pos], (byte)(pos + 1), ID);
                    }
                }

            };
            DeviceThreadManager.Instance.Start();
            WaitWorkDone();
            //有失败的情况
            if (Array.IndexOf(resoult, false) != -1)
            {
                string err = "";
                for (int i = 0; i < resoult.Length; i++)
                {
                    if (!resoult[i])
                    {
                        err += (i + 1).ToString() + ",";
                    }
                }
                MessageAdd($"表位:{err}误差板被检常数设置失败", EnumLogType.错误信息);

                return false;
            }
            return true;
        }

        /// <summary>
        /// 启动误差版
        /// </summary>
        /// <param name="ControlType">控制类型（00：正向有功，01：正向无功，02：反向有功，03：反向无功，04：日计时，05：需量， 06：正向有功脉冲计数， 07：正向无功脉冲计数， 08：反向有功脉冲计数，09 反向无功脉冲计数）</param>
        /// <param name="MeterNo">表位号，FF为广播</param>
        /// <returns></returns>
        public bool StartWcb(int ControlType, byte MeterNo)
        {
            MessageAdd($"启动误差版：【控制类型{ControlType}】【表位号{MeterNo}】", EnumLogType.详细信息);
            if (IsDemo)
            {
                errNum = 1;
                return true;
            }
            bool IsMass = MeterNo == 0xff;//是否是广播
            bool[] resoult = new bool[MeterNumber];
            resoult.Fill(true);

            //SetBluetoothModule(ControlType);
            IsRunWc = true;
            int DeviceCount = EquipmentData.DeviceManager.Devices[Device.DeviceName.误差板].Count;
            DeviceThreadManager.Instance.MaxThread = DeviceCount;
            DeviceThreadManager.Instance.MaxTaskCountPerThread = MeterNumber / DeviceCount;
            if (IsMass)
            {
                DeviceThreadManager.Instance.MaxTaskCountPerThread = 1; //广播的情况

            }
            DeviceThreadManager.Instance.DoWork = delegate (int ID, int pos)
            {
                if (Stop) return;
                if (IsMass) //广播的话 --没有返回值
                {
                    DeviceControl.StartWcb(ControlType, MeterNo, ID);
                }
                else
                {
                    if (MeterInfo[pos].YaoJianYn)//要检定的表才设置
                    {
                        resoult[pos] = DeviceControl.StartWcb(ControlType, (byte)(pos + 1), ID);
                    }
                }

            };
            DeviceThreadManager.Instance.Start();
            WaitWorkDone();
            //有失败的情况
            if (Array.IndexOf(resoult, false) != -1)
            {
                string err = "";
                for (int i = 0; i < resoult.Length; i++)
                {
                    if (!resoult[i])
                    {
                        err += (i + 1).ToString() + ",";
                    }
                }
                MessageAdd($"表位:{err}启动误差版失败", EnumLogType.错误信息);

                return false;
            }
            return true;
        }

        /// <summary>
        /// 停止误差版。带演示模式
        /// </summary>
        /// <param name="ControlType">控制类型（00：正向有功，01：正向无功，02：反向有功，03：反向无功，04：日计时，05：需量， 06：正向有功脉冲计数， 07：正向无功脉冲计数， 08：反向有功脉冲计数，09 反向无功脉冲计数）</param>
        /// <param name="MeterNo">表位号，FF为广播</param>
        /// <returns></returns>
        public bool StopWcb(int ControlType, byte MeterNo)
        {
            if (IsDemo) return true;
            bool IsMass = MeterNo == 0xff;//是否是广播
            bool[] resoult = new bool[MeterNumber];
            resoult.Fill(true);

            int DeviceCount = EquipmentData.DeviceManager.Devices[Device.DeviceName.误差板].Count;
            DeviceThreadManager.Instance.MaxThread = DeviceCount;
            DeviceThreadManager.Instance.MaxTaskCountPerThread = MeterNumber / DeviceCount;
            if (IsMass)
            {
                DeviceThreadManager.Instance.MaxTaskCountPerThread = 1; //广播的情况
                IsRunWc = false;
            }
            DeviceThreadManager.Instance.DoWork = delegate (int ID, int pos)
            {
                //if (Stop) return;
                if (IsMass) //广播的话 --没有返回值
                {
                    DeviceControl.StopWcb(ControlType, MeterNo, ID);
                }
                else
                {
                    if (MeterInfo[pos].YaoJianYn)//要检定的表才设置
                    {
                        resoult[pos] = DeviceControl.StopWcb(ControlType, (byte)(pos + 1), ID);
                    }
                }

            };
            DeviceThreadManager.Instance.Start();
            WaitWorkDone();
            //有失败的情况
            if (Array.IndexOf(resoult, false) != -1)
            {
                string err = "";
                for (int i = 0; i < resoult.Length; i++)
                {
                    if (!resoult[i])
                    {
                        err += (i + 1).ToString() + ",";
                    }
                }
                MessageAdd($"表位:{err}停止误差版失败", EnumLogType.错误信息);
                return false;
            }
            return true;

        }

        /// <summary>
        ///  读取误差板误差 
        /// </summary>
        /// <param name="readType">读取类型(00--正向有功，01--正向无功，02--反向有功，03--反向无功，04--日计时误差</param>
        public StError[] ReadWcbData(bool[] t, int readType)
        {
            StError[] stErrors = new StError[MeterNumber];
            if (IsDemo) return stErrors;
            int DeviceCount = EquipmentData.DeviceManager.DeviceCount;
            DeviceThreadManager.Instance.MaxThread = DeviceCount;
            DeviceThreadManager.Instance.MaxTaskCountPerThread = MeterNumber / DeviceCount;
            DeviceThreadManager.Instance.DoWork = delegate (int ID, int pos)
            {
                if (Stop) return;
                if (t[pos])//要检定的表才设置
                {
                    stErrors[pos] = DeviceControl.ReadWcbData(readType, (byte)(pos + 1), ID);
                }
            };
            DeviceThreadManager.Instance.Start();
            WaitWorkDone();
            //有失败的情况
            return stErrors;

        }
        public StError[] ReadWcbDataLast(bool[] t, int readType)
        {
            StError[] stErrors = new StError[MeterNumber];
            if (IsDemo) return stErrors;
            int DeviceCount = EquipmentData.DeviceManager.DeviceCount;
            DeviceThreadManager.Instance.MaxThread = DeviceCount;
            DeviceThreadManager.Instance.MaxTaskCountPerThread = MeterNumber / DeviceCount;
            DeviceThreadManager.Instance.DoWork = delegate (int ID, int pos)
            {
                if (t[pos])//要检定的表才设置
                {
                    stErrors[pos] = DeviceControl.ReadWcbData(readType, (byte)(pos + 1), ID);
                }
            };
            DeviceThreadManager.Instance.Start();
            while (true)
            {
                if (DeviceThreadManager.Instance.IsWorkDone())
                {
                    break;
                }
                Thread.Sleep(50);
            }
            return stErrors;

        }

        /// <summary>
        ///  控制表位继电器 --1开启-2关闭
        /// </summary>
        /// <param name="t"></param>
        /// <param name="contrnlType">控制类型--1开启（闭合）（不检）-2关闭（断开）（要检）</param>
        public void ControlMeterRelay(bool[] t, int contrnlType)
        {
            if (IsDemo) return;
            //DeviceControl.ControlMeterRelay(contrnlType, EpitopeNo);
            int DeviceCount = EquipmentData.DeviceManager.Devices[Device.DeviceName.误差板].Count;
            DeviceThreadManager.Instance.MaxThread = DeviceCount;
            DeviceThreadManager.Instance.MaxTaskCountPerThread = MeterNumber / DeviceCount;
            DeviceThreadManager.Instance.DoWork = delegate (int ID, int pos)
            {
                if (Stop) return;
                if (t[pos])//要检定的表才设置
                {
                    DeviceControl.ControlMeterRelay(contrnlType, (byte)(pos + 1), ID);
                }
            };
            DeviceThreadManager.Instance.Start();
            WaitWorkDone();
        }

        /// <summary>
        /// 电机控制---
        /// </summary>
        /// <param name="bwNum">表位</param>
        /// <returns></returns>
        public bool ElectricmachineryContrnl(bool[] t, int ControlType)
        {
            if (IsDemo) return true;
            bool[] IsChekc = new bool[MeterNumber];
            IsChekc.Fill(true);
            int DeviceCount = EquipmentData.DeviceManager.Devices[Device.DeviceName.误差板].Count;
            DeviceThreadManager.Instance.MaxThread = DeviceCount;
            DeviceThreadManager.Instance.MaxTaskCountPerThread = MeterNumber / DeviceCount;
            DeviceThreadManager.Instance.DoWork = delegate (int ID, int pos)
            {
                if (Stop) return;
                if (t[pos])//要检定的表才设置
                {
                    IsChekc[pos] = DeviceControl.ElectricmachineryContrnl(ControlType, (byte)(pos + 1), ID);
                    Thread.Sleep(50); //线程太快有时候电机反应不过来
                }
            };
            DeviceThreadManager.Instance.Start();
            WaitWorkDone();
            if (Array.IndexOf<bool>(IsChekc, false) >= 0)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// 读取所有表位状态
        /// </summary>
        /// <param name="bwNum">表位</param>
        /// <returns></returns>
        public MeterState[] Read_Meterstate(bool[] t)
        {
            MeterState[] meters = new MeterState[MeterNumber];
            if (IsDemo) return meters;
            int DeviceCount = EquipmentData.DeviceManager.Devices[Device.DeviceName.误差板].Count;
            DeviceThreadManager.Instance.MaxThread = DeviceCount;
            DeviceThreadManager.Instance.MaxTaskCountPerThread = MeterNumber / DeviceCount;
            DeviceThreadManager.Instance.DoWork = delegate (int ID, int pos)
            {
                if (Stop) return;
                if (t[pos])//要检定的表才设置
                {
                    meters[pos] = DeviceControl.Read_Fault((byte)(pos + 1), ID);
                }
            };
            DeviceThreadManager.Instance.Start();
            WaitWorkDone();
            return meters;
        }

        public int GetWcbFangXianIndex(PowerWay fx)
        {
            int readType = 0;
            switch (fx)
            {
                case PowerWay.正向有功:
                    readType = 0;
                    break;
                case PowerWay.正向无功:
                    readType = 1;
                    break;
                case PowerWay.反向有功:
                    readType = 0;
                    break;
                case PowerWay.反向无功:
                    readType = 1;
                    break;
                default:
                    break;
            }
            return readType;
        }
        public float[] DemoBasicErr(float MeterLevel, int maxWCnum)
        {
            MeterLevel *= 10000;//扩大10000倍

            float[] tmpWC = new float[maxWCnum];

            for (int e = 0; e < maxWCnum; e++)
            {
                tmpWC[e] = new Random().Next((int)(-MeterLevel), (int)MeterLevel);
                tmpWC[e] = tmpWC[e] / 20000F;
                int delayTime = 500 / MeterNumber;
                if (delayTime <= 0) delayTime = 1;
                Thread.Sleep(delayTime);

            }
            return tmpWC;
        }
        public float DemoErr(float maxErr)
        {
            Thread.Sleep(1);
            return new Random().Next((int)(-maxErr * 10000), (int)maxErr * 10000) / 10000F;
        }

        /// <summary>
        /// 加+-符号
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private string AddFlag(string data)
        {
            if (float.TryParse(data, out float fdata))
            {
                return fdata > 0 ? $"+{data}" : data;
            }
            else
            {
                return string.IsNullOrWhiteSpace(data) || data.IndexOf("-") > 0 ? data : $"+{data}";
            }
        }

        /// <summary>
        /// 修正数字加+-号
        /// </summary>
        /// <param name="data">要修正的数字</param>
        /// <param name="Priecision">修正精度</param>
        /// <returns>返回指定精度的带+-号的字符串</returns>
        public string AddFlag(float data, int Priecision)
        {
            string v = data.ToString($"F{Priecision}");
            return AddFlag(v);
        }
        /// <summary>
        /// 支持：重复性
        /// </summary>
        /// <param name="data"></param>
        /// <param name="space"></param>
        /// <returns></returns>
        public string GetWuChaRounding(float data, float space)
        {
            float avg = data;
            float hz = Number.GetHzz(avg, space);

            int hzPrecision = Common.GetPrecision(space.ToString());
            string HZNumber = hz.ToString(string.Format("F{0}", hzPrecision));
            if (hz != 0f) //化整值为0时，不加正负号//TODO:配置
                HZNumber = AddFlag(hz, hzPrecision);

            if (avg < 0) HZNumber = HZNumber.Replace('+', '-'); //平均值<0时，化整值需为负数

            //add yjt 20220701 根据平均值判断化整值的符号//TODO:删
            if (hz == 0)
            {
                if (avg < 0)
                {
                    HZNumber = "-" + HZNumber;
                }
                else
                {
                    HZNumber = "+" + HZNumber;
                }
            }


            return HZNumber;
        }

        private Dictionary<string, float[]> DicJianJu = null;
        /// <summary>
        /// 返回修正间距
        /// </summary>
        /// <IsWindage>是否是偏差</IsWindage> 
        /// <returns></returns>
        public float GetWuChaHzzJianJu(bool IsWindage, float meterLevel)
        {
            string Key = string.Format("Level{0}", meterLevel);
            //根据表精度及表类型生成主键
            if (DicJianJu == null)
            {
                DicJianJu = new Dictionary<string, float[]>
                {
                    { "Level0", new float[] { 0.001F, 0.0001F } },      //0级表标准表
                    { "Level0.01", new float[] { 0.001F, 0.0001F } },      //0.01级表标准表
                    { "Level0.02", new float[] { 0.002F, 0.0002F } },      //0.02级表标准表
                    { "Level0.02B", new float[] { 0.002F, 0.0002F } },      //0.02级表标准表
                    { "Level0.05", new float[] { 0.005F, 0.0005F } },      //0.05级表标准表
                    { "Level0.05B", new float[] { 0.005F, 0.0005F } },      //0.05级表标准表
                    { "Level0.1", new float[] { 0.01F, 0.001F } },         //0.1级表标准表
                    { "Level0.1B", new float[] { 0.01F, 0.001F } },         //0.1级表标准表
                    { "Level0.2B", new float[] { 0.02F, 0.002F } },         //0.2级标准表
                    { "Level0.2", new float[] { 0.02F, 0.004F } },          //0.2级表
                    { "Level0.2S", new float[] { 0.02F, 0.004F } },          //0.2级表
                    { "Level0.5", new float[] { 0.05F, 0.01F } },           //0.5级表
                    { "Level0.5S", new float[] { 0.05F, 0.01F } },           //0.5级表
                    { "Level1", new float[] { 0.1F, 0.02F } },              //1级表
                    { "Level1.5", new float[] { 0.2F, 0.04F } },           //2级表
                    { "Level2", new float[] { 0.2F, 0.04F } },               //2级表
                    { "Level3", new float[] { 0.2F, 0.04F } }               //3级表
                };
            }

            float[] JianJu;
            if (DicJianJu.ContainsKey(Key))
            {
                JianJu = DicJianJu[Key];
            }
            else
            {
                JianJu = new float[] { meterLevel * 0.1f, meterLevel * 0.1f };    //没有在字典中找到，则直接按2算
            }

            if (IsWindage)
                return JianJu[1];//标偏差
            else
                return JianJu[0];//普通误差
        }

        /// <summary>
        /// 解析费率时间
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        protected List<FL_Data> GetFlList(string data, bool IsCF)
        {
            //以防输错情况，把中文字符转成英文字符
            data = data.Replace("（", "(").Replace("）", ")").Replace("，", ",").Replace("：", ":");
            data = data.Replace("):", "),");
            string[] tem = data.Split(',');

            List<FL_Data> list = new List<FL_Data>();
            for (int i = 0; i < tem.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(tem[i])) continue;
                string[] d = tem[i].Split('(');
                if (d.Length < 2) continue;
                // 00:30(谷)
                FL_Data t = new FL_Data
                {
                    Time = d[0],
                    Type = (Cus_FeiLv)Enum.Parse(typeof(Cus_FeiLv), d[1].Substring(0, d[1].Length - 1))
                };
                if (IsCF || list.Count(item => item.Type == t.Type) == 0)  //费率重复的不添加
                {
                    list.Add(t);
                }
            }
            return list;

        }
        #endregion

        #region 雷电标准表
        /// <summary>
        ///  设置接线方式 
        /// </summary>
        /// <param name="BncCode">256输出端口</param>
        /// <param name="MetricIndex">脉冲类型</param>
        /// <param name="aseCode">相别</param>
        public void SetPuase(string BncCode, string MetricIndex, string aseCodex)
        {
            DeviceControl.SetPuase(BncCode, MetricIndex, aseCodex);
        }
        /// <summary>
        ///  设置常数
        /// </summary>
        /// <param name="MetricIndex"></param>
        /// <param name="Constant"></param>
        /// 
        public void SetConstant(string MetricIndex, float Constant)
        {
            DeviceControl.SetConstant(MetricIndex, Constant);
        }

        /// <summary>
        ///   读取标准表瞬时值
        /// </summary>
        /// 
        //public StandarMeterInfo ReadStMeterInfo()
        //{
        //   return DeviceControl.ReadStMeterInfo();
        //}
        #endregion

        #region 标准表
        /// <summary>
        /// 初始化标准表端口
        /// </summary>
        public void InitStdProtocol()
        {
            //需要先创建obj实例
            //然后反射回初始化方法，后面调用每个实例对应的方法
            //GetReflexObject
            string Error = "";
            for (int i = 0; i < MeterNumber; i++)
            {
                if (!MeterInfo[i].YaoJianYn)
                    continue;
                MeterInfo[i].Type = EquipmentData.DeviceManager.GetReflexObject("ZH311");
                MeterInfo[i].Obj = Activator.CreateInstance(MeterInfo[i].Type);
                System.Reflection.MethodInfo mInfo = MeterInfo[i].Type.GetMethod("InitSettingCom"); //获取当前方法
                int prot = Convert.ToInt32(MeterInfo[i].ProtInfo.Port);
                int MaxWaitTime = Convert.ToInt32(MeterInfo[i].ProtInfo.MaxTimePerByte);
                int WaitSencondsPerByte = Convert.ToInt32(MeterInfo[i].ProtInfo.MaxTimePerFrame);

                object[] s = new object[3] { prot, MaxWaitTime, WaitSencondsPerByte };
                int t = int.Parse(mInfo.Invoke(MeterInfo[i].Obj, s).ToString());  //接收调用返回值，判断调用是否成功  new object[1] {5}
                if (t != 0)
                {
                    Error += $"{i},";
                }

            }
            if (Error != "")
            {
                MessageAdd($"标准表：{Error}初始化失败", EnumLogType.错误信息);
            }
            else
            {
                MessageAdd("标准表初始化成功", EnumLogType.流程信息);
            }
        }
        /// <summary>
        ///读取标准表累积电量,kWh
        /// </summary>
        /// <returns >返回值float数组</returns>
        public float[] ReadStmEnergy()
        {
            if (IsDemo) return null;
            return DeviceControl.ReadEnergy();
        }
        /// <summary>
        ///1008H- 档位常数 读取与设置
        /// </summary>
        /// <param name="stdCmd">>0x10 读 ，0x13写</param>
        /// <param name="stdConst">常数</param>
        /// <param name="stdUIGear">电压电流挡位UA，ub，uc，ia，ib，ic</param>
        public bool StdGear(byte stdCmd, ref ulong stdConst, double[] stdUIGear)
        {
            if (IsDemo) return true;
            return DeviceControl.StdGear(stdCmd, ref stdConst, stdUIGear);
        }
        /// <summary>
        ///获得标准表的常数
        /// </summary>
        /// <param name="stdCmd">>0x10 读 ，0x13写</param>
        /// <param name="stdConst">常数</param>
        /// <param name="stdUIGear">电压电流挡位UA，ub，uc，ia，ib，ic</param>
        public ulong GetStaConst()
        {
            if (IsDemo) return 1000000;
            ulong stdConst = 1000000;
            double[] stdUIGear = new double[6];
            DeviceControl.StdGear(0x10, ref stdConst, stdUIGear);
            return stdConst;
        }

        /// <summary>
        ///1008H- 档位常数 读取与设置
        /// </summary>
        /// <param name="stdCmd">>0x10 读 ，0x13写</param>
        /// <param name="stdConst">常数</param>
        /// <param name="ua">A相电压当前档位</param>
        public bool StdGear(byte stdCmd, ulong stdConst, float ua, float ub, float uc, float ia, float ib, float ic)
        {
            if (IsDemo) return true;
            double[] stdUIGear = new double[] { ua, ub, uc, ia, ib, ic };
            return DeviceControl.StdGear(stdCmd, ref stdConst, stdUIGear);
        }

        /// <summary>
        /// 100cH-启停标准表累积电能
        /// </summary>
        /// <param name="startOrStopStd">字符’1’表示清零当前电能并开始累计（ascii 码读取）</param>
        public bool StartStdEnergy(int startOrStopStd)
        {
            if (IsDemo) return true;
            return DeviceControl.StartStdEnergy(startOrStopStd);
        }

        /// <summary>
        ///  100bH-模式设置--底层有问题
        /// </summary>
        /// <param name="stdCmd"> </param>
        /// <param name="strModeJxfs">自动手动模式标识字符’1’表示手动模式，字符’0’表示自动模式（ascii 码读取）
        ///  接线方式模式标识字符’1’表示3相4线制，字符’2’表示3相3线制（ascii 码读取）
        ///标准表模式标识字符’1’表示单相表，字符’3’表示三相表，写操作此位置填0（ascii 码读取）</param>
        public bool StdMode()
        {
            return DeviceControl.StdMode();
        }


        /// <summary>
        /// 初始化标准表
        /// </summary>
        /// <param name="ComNumber">端口号</param>
        /// <param name="MaxWaitTime">最大等待时间</param>
        /// <param name="WaitSencondsPerByte">帧最大等待时间</param>
        /// <returns></returns>
        public bool InitSettingCom(int ComNumber, string MaxWaitTime, string WaitSencondsPerByte)
        {
            return DeviceControl.InitSettingCom(ComNumber, MaxWaitTime, WaitSencondsPerByte);
        }

        /// <summary>
        /// 脉冲校准误差 字符串形式(10字节)W， 单位 分数值。
        /// 0.01525%，则0.01525% ×1000000 = 152.5，
        /// 则下发字符串 ”152.5”。
        /// </summary>
        ///  <param name="Error">误差板的值</param>
        /// <returns></returns>
        public bool SetPulseCalibration(double Error)
        {
            return DeviceControl.SetPulseCalibration(Error);
        }

        /// <summary>
        ///   设置脉冲
        /// </summary>
        /// <param name="pulseType">
        ///有功脉冲 设置字符’1’
        ///无功脉冲 设置字符’2’
        ///UA脉冲 设置字符’3’
        ///UB脉冲 设置字符’4’
        ///UC脉冲 设置字符’5’
        ///IA脉冲 设置字符’6’
        ///IB脉冲 设置字符’7’
        ///IC脉冲 设置字符’8’
        ///PA脉冲 设置字符’9’
        ///PB脉冲 设置字符’10’
        ///PC脉冲 设置字符’11’
        ///</param>
        /// <returns></returns>
        public bool SetPulseType(string pulseType)
        {
            if (IsDemo) return true;
            return DeviceControl.SetPulseType(pulseType);
        }
        #endregion

        #region 功耗板
        public bool Read_GH_Dissipation(int bwIndex, out float[] pd)
        {
            return DeviceControl.Read_GH_Dissipation(bwIndex, out pd);
        }
        #endregion

        #region 零线电流板
        //add yjt 20230103 新增零线电流控制启停
        /// <summary>
        /// 启停零线电流板
        /// </summary>
        /// <param name="startOrStopStd">字符‘1’开启，字符‘0’关闭</param>
        public bool StartZeroCurrent(int A_kz, int BC_kz)
        {
            if (IsDemo) return true;
            return DeviceControl.StartZeroCurrent(A_kz, BC_kz);
        }
        #endregion

        #region 接地故障
        /// <summary>
        /// 接地故障
        /// </summary>
        /// <param name="bwIndex"></param>
        /// <param name="typeA"></param>
        /// <param name="typeB"></param>
        /// <param name="typeC"></param>
        /// <param name="typeN"></param>
        /// <returns></returns>
        public bool SetJDGZContrnl(int bwIndex, int typeA, int typeB, int typeC, int typeN)
        {
            return DeviceControl.SetJDGZContrnl(bwIndex, typeA, typeB, typeC, typeN);
        }
        #endregion
        #endregion

        #region 冻结相关
        /// <summary>
        /// false-数组中有false值，true-数组全为true
        /// </summary>
        /// <param name="bValue"></param>
        /// <returns></returns>
        protected bool GetArrValue(bool[] bValue)
        {
            for (int i = 0; i < MeterNumber; i++)
            {
                if (MeterInfo[i].YaoJianYn)
                {
                    if (!bValue[i])
                        return false;
                }
            }
            return true;
        }

    }
    #endregion
}

