using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LYTest.Mis.MDS.Table
{
    //负载电流升降变差试验结论
    public class MT_VARIATION_MET_CONC : MT_MET_CONC_Base
    {
        /// <summary>
        /// 功率方向
        /// </summary>
        public string BOTH_WAY_POWER_FLAG { get; set; }
        /// <summary>
        /// 电流负载
        /// </summary>
        public string LOAD_CURRENT { get; set; }
        /// <summary>
        /// 电流相别
        /// </summary>
        public string IABC { get; set; }
        /// <summary>
        /// 功率因数
        /// </summary>
        public string PF { get; set; }
        /// <summary>
        /// 检验圈数
        /// </summary>
        public string DETECT_CIRCLE { get; set; }
        /// <summary>
        /// 采样次数
        /// </summary>
        public string SIMPLING { get; set; }
        /// <summary>
        /// 升降电流等待时间
        /// </summary>
        public string WAIT_TIME { get; set; }
        /// <summary>
        /// 升流误差1
        /// </summary>
        public string UP_ERR1 { get; set; }
        /// <summary>
        /// 升流误差2
        /// </summary>
        public string UP_ERR2 { get; set; }
        /// <summary>
        /// 升流误差平均
        /// </summary>
        public string AVG_UP_ERR { get; set; }
        /// <summary>
        /// 升流化整误差
        /// </summary>
        public string INT_UP_ERR { get; set; }
        /// <summary>
        /// 降流误差1
        /// </summary>
        public string DOWN_ERR1 { get; set; }
        /// <summary>
        /// 降流误差2
        /// </summary>
        public string DOWN_ERR2 { get; set; }
        /// <summary>
        /// 降流误差平均
        /// </summary>
        public string AVG_DOWN_ERR { get; set; }
        /// <summary>
        /// 降流化整误差
        /// </summary>
        public string INT_DOWN_ERR { get; set; }
        /// <summary>
        /// 升降变差
        /// </summary>
        public string VARIATION_ERR { get; set; }
        /// <summary>
        /// 升降化整变差
        /// </summary>
        public string INT_VARIATION_ERR { get; set; }


        //以下为新加入

        /// <summary>
        /// 电压负载
        /// </summary>
        public string LOAD_VOLTAGE { get; set; }
    }
}
