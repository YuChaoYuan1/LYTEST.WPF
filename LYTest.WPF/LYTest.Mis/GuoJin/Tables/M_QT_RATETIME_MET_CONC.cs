using System;

namespace LYTest.Mis.GuoJin
{
    //费率和时段试验
    public class M_QT_RATETIME_MET_CONC : M_QT_CONC_Basic
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
        /// 检测项目分项
        /// </summary>
        public string TEST_ITEMS_SHARE { get; set; }

        /// <summary>
        /// 试验结果
        /// </summary>
        public string DETECT_RESULT { get; set; }

        ///// <summary>
        ///// 检定分项
        ///// </summary>
        //public string TEST_SUB { get; set; }

        ///// <summary>
        ///// 两套时区表切换时间
        ///// </summary>
        //public string CHANGE_DATE { get; set; }
        ///// <summary>
        ///// 两套日时段切换时间
        ///// </summary>
        //public string CHANGE_DATE2 { get; set; }
        ///// <summary>
        ///// 约定冻结数据模式字
        ///// </summary>
        //public string TIME_VALUE { get; set; }
        ///// <summary>
        ///// 当前正向有功电量
        ///// </summary>
        //public string FORWARD_POWER { get; set; }
        ///// <summary>
        ///// 切换前
        ///// </summary>
        //public string TIME_BEFORE { get; set; }
        ///// <summary>
        ///// 切换后
        ///// </summary>
        //public string TIME_AFTER { get; set; }
        ///// <summary>
        ///// 恢复后
        ///// </summary>
        //public string REVERT_AFTER { get; set; }
    }
}