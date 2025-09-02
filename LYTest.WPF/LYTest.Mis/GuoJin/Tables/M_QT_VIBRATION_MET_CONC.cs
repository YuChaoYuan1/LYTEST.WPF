using System;

namespace LYTest.Mis.GuoJin
{
    //振动试验
    public class M_QT_VIBRATION_MET_CONC : M_QT_CONC_Basic
    {
        
        /// <summary>
        /// 负载电流
        /// </summary>
        public string LOAD_CURRENT { get; set; }
        /// <summary>
        /// 电压
        /// </summary>
        public string LOAD_VOLTAGE { get; set; }
        /// <summary>
        /// 功率方向
        /// </summary>
        public string BOTH_WAY_POWER_FLAG { get; set; }
        /// <summary>
        /// 电流相别
        /// </summary>
        public string IABC { get; set; }
        /// <summary>
        /// 频率
        /// </summary>
        public string FREQ { get; set; }
        /// <summary>
        /// 功率因数
        /// </summary>
        public string PF { get; set; }
        /// <summary>
        /// 电能表常数
        /// </summary>
        public string METER_CONST_CODE { get; set; }

        /// <summary>
        /// 试验结果
        /// </summary>
        public string DETECT_RESULT { get; set; }

        /// <summary>
        ///误差1
        /// </summary>
        public string ERROR1 { get; set; }
        /// <summary>
        ///误差
        /// </summary>
        public string ERROR { get; set; }
        /// <summary>
        ///误差2
        /// </summary>
        public string ERROR2 { get; set; }
        /// <summary>
        ///误差平均值
        /// </summary>
        public string AVE_ERR { get; set; }
        /// <summary>
        ///误差取整ERR_ABS
        /// </summary>
        public string INT_CONVERT_ERR { get; set; }
        /// <summary>
        ///误差限
        /// </summary>
        public string ERR_ABS { get; set; }
        /// <summary>
        ///有效标志：0：无效 1：有效
        /// </summary>
        public string IS_VALID { get; set; }
        /// <summary>
        ///频率范围
        /// </summary>
        public string RANGE_FREQUENCY { get; set; }
        /// <summary>
        ///变越频率
        /// </summary>
        public string IN_FREQUENCY { get; set; }
        /// <summary>
        ///扫描周期
        /// </summary>
        public string SCAN_CYCLE { get; set; }
        /// <summary>
        ///恒定振幅
        /// </summary>
        public string CON_AMPLITUDE { get; set; }
        /// <summary>
        ///恒定加速度
        /// </summary>
        public string CON_ACCELERA { get; set; }
        /// <summary>
        ///试验时长：用||间隔开 秒
        /// </summary>
        public string TEST_TIME { get; set; }
        /// <summary>
        ///每次间隔时长：用||间隔开
        /// </summary>
        public string ORY_TEST_TIME { get; set; }
        /// <summary>
        ///结论：01合格、02不合格 
        /// </summary>
        public string CHK_CONC_CODE { get; set; }
        /// <summary>
        ///供电单位编号
        /// </summary>
        public string ORG_NO { get; set; }
        /// <summary>
        ///缴费终端的地区代码
        /// </summary>
        public string AREA_CODE { get; set; }
        /// <summary>
        ///01输入值 ；02 选择
        /// </summary>
        public string RESULT_TYPE { get; set; }
        /// <summary>
        ///附件表关联标识
        /// </summary>
        public string ASSOCIATED_ID { get; set; }
        /// <summary>
        ///试验分项
        /// </summary>
        public string TEST_CATEGORIES2 { get; set; }
    }
}