using System;

namespace LYTest.Mis.GuoJin
{
    //快速瞬变脉冲群试验
    public class M_QT_TRANPULSE_MET_CONC : M_QT_CONC_Basic
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
        /// 负载电流升降
        /// </summary>
        public string LOAD_LIFT { get; set; }
        /// <summary>
        /// 相序
        /// </summary>
        public string REVERSE_ORDER { get; set; }
        /// <summary>
        /// 电压不平衡
        /// </summary>
        public string VOL_UNBALANCE { get; set; }
        /// <summary>
        /// 谐波类型
        /// </summary>
        public string HARM_TYPE { get; set; }
        /// <summary>
        /// 相位角度
        /// </summary>
        public string PHASE_TYPE { get; set; }
        /// <summary>
        /// 温度系数
        /// </summary>
        public string TEM_COEFFI { get; set; }
        /// <summary>
        /// 检验脉冲数（圈数）
        /// </summary>
        public string DETECT_CIRCLE { get; set; }
        /// <summary>
        /// 采样次数
        /// </summary>
        public string SIMPLING { get; set; }
        /// <summary>
        /// 变差
        /// </summary>
        public string VARIA_ERR { get; set; }
        /// <summary>
        /// 变差取整
        /// </summary>
        public string INT_VARIA_ERR { get; set; }
        /// <summary>
        /// 变差限
        /// </summary>
        public string VALUE_ABS { get; set; }

        /// <summary>
        /// 试验结果
        /// </summary>
        public string DETECT_RESULT { get; set; }
    }
}