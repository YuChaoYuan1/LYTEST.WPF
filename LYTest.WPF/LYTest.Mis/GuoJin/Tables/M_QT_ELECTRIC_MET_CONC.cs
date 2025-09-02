using System;

namespace LYTest.Mis.GuoJin
{
    /// <summary>
    /// 电能量分项设置与累计存储试验
    /// </summary>
    public class M_QT_ELECTRIC_MET_CONC : M_QT_CONC_Basic
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
        /// 费率
        /// </summary>
        public string FEE { get; set; }

        /// <summary>
        /// 读取值
        /// </summary>
        public string READ_VALUE { get; set; }

        /// <summary>
        /// 试验结果
        /// </summary>
        public string DETECT_RESULT { get; set; }

        /// <summary>
        /// 组合有功特征字
        /// </summary>
        public string FEAT_VALUE { get; set; }

        /// <summary>
        /// 组合有功1特征字
        /// </summary>
        public string FEAT_VALUE1 { get; set; }

        /// <summary>
        /// 组合有功2特征字
        /// </summary>
        public string FEAT_VALUE2 { get; set; }

        /// <summary>
        /// 项目类别（计量功能、最大需量功能）
        /// </summary>
        public string TEST_TYPE { get; set; }

        /// <summary>
        /// 费率电量（总|尖|峰|平|谷）
        /// </summary>
        public string FEE_VALUE { get; set; }

        /// <summary>
        /// 结算次数
        /// </summary>
        public string STATE_POINT { get; set; }

        /// <summary>
        /// 第一结算日
        /// </summary>
        public string STATE_DATE { get; set; }
    }
}