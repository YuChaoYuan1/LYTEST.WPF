using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LYTest.Mis.MDS.Table
{
    //基本误差试验结论
    public class MT_BASICERR_MET_CONC : MT_MET_CONC_Base
    {
 

        /// <summary>
        /// 功率方向
        /// </summary>
        public string BOTH_WAY_POWER_FLAG { get; set; }

        /// <summary>
        /// 电流相别
        /// </summary>
        public string IABC { get; set; }

        /// <summary>
        /// 电流负载
        /// </summary>
        public string LOAD_CURRENT { get; set; }

        /// <summary>
        /// 电压比值
        /// </summary>
        public string LOAD_VOLTAGE { get; set; }

        /// <summary>
        /// 频率
        /// </summary>
        public string FREQ { get; set; }

        /// <summary>
        /// 功率因数
        /// </summary>
        public string PF { get; set; }

        /// <summary>
        /// 检验脉冲数（圈数）
        /// </summary>
        public string DETECT_CIRCLE { get; set; }

        /// <summary>
        /// 采样次数（误差次数）
        /// </summary>
        public string SIMPLING { get; set; }

        /// <summary>
        /// 实际误差
        /// </summary>
        public string ERROR { get; set; }

        /// <summary>
        /// 平均值
        /// </summary>
        public string AVE_ERR { get; set; }

        /// <summary>
        /// 误差化整值
        /// </summary>
        public string INT_CONVERT_ERR { get; set; }

        /// <summary>
        ///误差上限
        /// </summary>
        public string ERR_UP { get; set; }

        /// <summary>
        ///误差下限
        /// </summary>
        public string ERR_DOWN { get; set; }

    }
}
