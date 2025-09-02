using LYTest.DAL.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LYTest.ViewModel.CheckController
{
    public class VerifyConfig
    {
        private static bool updateKeyAndUpdateData = false;

        public static bool UpdateKeyAndUpdateData
        {
            get { return updateKeyAndUpdateData; }
            set { updateKeyAndUpdateData = value; }
        }
        /// <summary>
        /// 自动模式
        /// </summary>
        public static string VerifyModel = ConfigHelper.Instance.VerifyModel;

        #region 地区信息
        /// <summary>
        ///地区名称
        /// </summary>
        public static string AreaName = ConfigHelper.Instance.AreaName;
        /// <summary>
        /// 误差限比例
        /// </summary>
        public static string ErrorRatio = ConfigHelper.Instance.ErrorRatio;
        #endregion

        #region 检定信息
        /// <summary>
        /// 常数模式：是：固定常数；否：自动常数
        /// </summary>
        public static bool FixedConstant = ConfigHelper.Instance.FixedConstant;
        /// <summary>
        /// 挡位模式：是：自动挡位；否：手动挡位
        /// </summary>
        public static bool AutoGear = ConfigHelper.Instance.AutoGear;
        /// <summary>
        /// 标准表固定常数
        /// </summary>
        public static ulong StdConst = ConfigHelper.Instance.Std_Const;
        /// <summary>
        ///功率源稳定时间
        /// </summary>
        public static int Dgn_PowerSourceStableTime = ConfigHelper.Instance.Dgn_PowerSourceStableTime;
        /// <summary>
        ///  写操作时提示
        /// </summary>
        public static bool Dgn_WriteTips = ConfigHelper.Instance.Dgn_WriteTips;
        /// <summary>
        ///  走字电量输入方式--true:自动读取  false：手动输入
        /// </summary>
        public static bool Dgn_ZZStartElectricQuantityModel = ConfigHelper.Instance.Dgn_ZZStartElectricQuantityModel;
        /// <summary>
        /// 走字试验前电表清零
        /// </summary>
        public static bool Dgn_ZZTestClear = ConfigHelper.Instance.Dgn_ZZTestClear;
        /// <summary>
        ///  清零前对时
        /// </summary>
        public static bool Dgn_ClearFrontTiming = ConfigHelper.Instance.Dgn_ClearFrontTiming;
        /// <summary>
        /// 检定有效期
        /// </summary>
        public static string TestEffectiveTime = ConfigHelper.Instance.TestEffectiveTime;

        #endregion

        #region 检定配置
        /// <summary>
        /// 误差计算取值数
        /// </summary>
        public static int ErrorCount = ConfigHelper.Instance.ErrorCount;
        /// <summary>
        /// 最大处理时间
        /// </summary>
        public static int MaxHandleTime = ConfigHelper.Instance.MaxHandleTime;
        /// <summary>
        /// 误差个数最大数
        /// </summary>
        public static int ErrorMax = ConfigHelper.Instance.ErrorMax;
        /// <summary>
        ///平均值小数位数
        /// </summary>
        public static int PjzDigit = ConfigHelper.Instance.PjzDigit;
        /// <summary>
        /// 误差起始采集次数(这个就是前面几个误差不要)
        /// </summary>
        public static int ErrorStartCount = ConfigHelper.Instance.ErrorStartCount;
        /// <summary>
        /// 跳差判断倍数
        /// </summary>
        public static float JumpJudgment = ConfigHelper.Instance.JumpJudgment;
        /// <summary>
        /// 温度
        /// </summary>
        public static float Temperature = ConfigHelper.Instance.Temperature;
        /// <summary>
        /// 湿度
        /// </summary>
        public static float Humidity = ConfigHelper.Instance.Humidity;
        //temperature   humidity
        /// <summary>
        /// 偏差计算取值数
        /// </summary>
        public static int PcCount = ConfigHelper.Instance.PcCount;
        /// <summary>
        /// 是否使用时间来计算误差圈数
        /// </summary>
        public static bool IsTimeWcLapCount = ConfigHelper.Instance.IsTimeWcLapCount;
        /// <summary>
        /// 出一个误差最小时间
        /// </summary>
        public static float WcMinTime = ConfigHelper.Instance.WcMinTime;

        /// <summary>
        /// 是否自动压接表位(是，检定开始前会压接表位，检定结束会抬起表位)
        /// </summary>
        public static bool IsMete_Press = ConfigHelper.Instance.IsMete_Press;

        /// <summary>
        /// 表位压接等待时间
        /// </summary>
        public static int Mete_Press_Time = ConfigHelper.Instance.Mete_Press_Time;
        /// <summary>
        /// 不合格率报警(百分比)，并停止密钥更新
        /// </summary>
        public static int FailureRate = ConfigHelper.Instance.FailureRate;

        /// <summary>
        ///  加密解密方式--根据表协议(什么表用什么)-根据698协议(都用698)，根据645协议(都用645)
        /// </summary>
        public static string Test_CryptoModel = ConfigHelper.Instance.Test_CryptoModel;

        /// <summary>
        /// 功耗表位选择
        /// </summary>
        public static string SelectBiaoWei = ConfigHelper.Instance.SelectBiaoWei;
        /// <summary>
        /// B相电压补偿
        /// </summary>
        public static float VbCompensation = ConfigHelper.Instance.VbCompensation;

        /// <summary>
        /// 密钥更新模式-全部更新-只更新合格表位-弹出对话框等待
        /// </summary>
        public static string KeyUpdataModel = ConfigHelper.Instance.Test_KeyUpdataModel;

        /// <summary>
        /// 是否开启二次巡检
        /// </summary>
        public static bool IsCheckAgin = ConfigHelper.Instance.IsCheckAgin;

        /// <summary>
        /// 不合格报警数量（会停止检定）
        /// </summary>
        public static int MaxErrorNumber = ConfigHelper.Instance.MaxErrorNumber;

        /// <summary>
        /// 不合格表位是否跳出
        /// </summary>
        public static bool UnqualifiedJumpOutOf = ConfigHelper.Instance.UnqualifiedJumpOutOf;

        /// <summary>
        /// 是否耐压
        /// </summary>
        public static bool Test_InsulationModel
        {
            get
            {
                return "是" == ConfigHelper.Instance.Test_InsulationModel;
            }
        }
        /// <summary>
        /// 快速试验
        /// </summary>
        public static bool Test_QuickModel
        {
            get
            {
                return ConfigHelper.Instance.Test_QuickModel;
            }
        }
        #endregion

        #region 营销接口

        /// <summary>
        /// 营销接口类型
        /// </summary>
        public static string Marketing_Type = ConfigHelper.Instance.Marketing_Type;

        /// <summary>
        /// 营销下载标识--条形码  出厂编号 表位号
        /// </summary>
        public static string Marketing_DewnLoadNumber = ConfigHelper.Instance.Marketing_DewnLoadNumber;

        /// <summary>
        /// 营销系统IP地址
        /// </summary>
        public static string Marketing_IP = ConfigHelper.Instance.Marketing_IP;

        /// <summary>
        /// 营销系统端口号
        /// </summary>
        public static string Marketing_Prot = ConfigHelper.Instance.Marketing_Prot;

        /// <summary>
        /// 营销系统数据源--就是表名吧应该
        /// </summary>
        public static string Marketing_DataSource = ConfigHelper.Instance.Marketing_DataSource;

        /// <summary>
        /// 营销——数据库用户名
        /// </summary>
        public static string Marketing_UserName = ConfigHelper.Instance.Marketing_UserName;

        /// <summary>
        ///营销——数据库密码
        /// </summary>
        public static string Marketing_UserPassWord = ConfigHelper.Instance.Marketing_UserPassWord;
        /// <summary>
        /// WebService路径
        /// </summary>
        public static string Marketing_WebService = ConfigHelper.Instance.Marketing_WebService;

        /// <summary>
        /// 上传时时数据
        /// </summary>
        public static bool Marketing_UpData = ConfigHelper.Instance.Marketing_UpData;

        #endregion

        #region 加密机
        /// <summary>
        /// 加密机类型
        /// </summary>
        public static string Dog_Type = ConfigHelper.Instance.Dog_Type;
        /// <summary>
        /// 加密机IP
        /// </summary>
        public static string Dog_IP = ConfigHelper.Instance.Dog_IP;
        /// <summary>
        /// 加密机端口
        /// </summary>
        public static string Dog_Prot = ConfigHelper.Instance.Dog_Prot;
        /// <summary>
        /// 加密机密钥
        /// </summary>
        public static string Dog_key = ConfigHelper.Instance.Dog_key;
        /// <summary>
        /// 加密机认证类型--公钥--私钥
        /// </summary>
        public static string Dog_CheckingType = ConfigHelper.Instance.Dog_CheckingType;
        /// <summary>
        /// 加密机-是否进行密码机服务器连接
        /// </summary>
        public static bool Dog_IsPassWord = ConfigHelper.Instance.Dog_IsPassWord;
        /// <summary>
        /// 加密机连接模式---服务器版-直连加密机版
        /// </summary>
        public static string Dog_ConnectType = ConfigHelper.Instance.Dog_ConnectType;
        /// <summary>
        /// 加密机超时时间
        /// </summary>
        public static string Dog_Overtime = ConfigHelper.Instance.Dog_Overtime;
        #endregion

        public static string SetControl_No = ConfigHelper.Instance.SetControl_No;
        public static string SetControl_BenthNo = ConfigHelper.Instance.SetControl_BenthNo;

        /// <summary>
        /// 显示功能 测试方式:true 读电量
        /// </summary>
        public static bool Test_Fun_Show_Model
        {
            get
            {
                return true;//TODO:加到配置
            }
        }
        /// <summary>
        /// 单步或连续检定完是否关电压，true关，人工台多功能台不关
        /// </summary>
        public static bool Test_Finished_OffPower
        {
            get
            {
                return false;//TODO:加到配置
            }
        }
        public static bool Test_ZouZi_HalfwayMode
        {
            get
            {
                return true;//TODO:加到配置
            }
        }

        /// <summary>
        /// 内存变量，是走字类试验
        /// </summary>
        public static bool Test_ZouZi_Control = false;
    }
}
