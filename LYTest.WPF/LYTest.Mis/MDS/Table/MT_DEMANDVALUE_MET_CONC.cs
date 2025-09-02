using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LYTest.Mis.MDS.Table
{

    /// <summary>
    /// 需量示值误差结论
    /// </summary>
    public class MT_DEMANDVALUE_MET_CONC : MT_MET_CONC_Base
    {


        /// <summary>
        /// 需量周期时间
        /// </summary>
        public string DEMAND_PERIOD { get; set; }

        /// <summary>
        /// 滑差时间
        /// </summary>
        public string DEMAND_TIME { get; set; }

        /// <summary>
        /// 滑差次数
        /// </summary>
        public string DEMAND_INTERVAL { get; set; }

        private string _REAL_DEMAND = "";
        /// <summary>
        /// 实际需量
        /// </summary>
        public string REAL_DEMAND
        {
            get { return _REAL_DEMAND; }
            set
            {
                if (value.Length > 8)
                    _REAL_DEMAND = value.Substring(0, 8);
                else
                    _REAL_DEMAND = value;
            }
        }

        /// <summary>
        /// 实际周期
        /// </summary>
        public string REAL_PERIOD { get; set; }

        /// <summary>
        /// 控制方式
        /// </summary>
        public string CONTROL_METHOD { get; set; }

        /// <summary>
        /// 功率因数
        /// </summary>
        public string PF { get; set; }


        /// <summary>
        /// 误差上限
        /// </summary>
        public string ERR_UP { get; set; }

        /// <summary>
        /// 结论
        /// </summary>
        public string CHK_CONC_CODE { get; set; }

        /// <summary>
        /// 误差下线
        /// </summary>
        public string ERR_DOWM { get; set; }

        /// <summary>
        /// 电流负载
        /// </summary>
        public string LOAD_CURRENT { get; set; }

        /// <summary>
        /// 需量示值误差
        /// </summary>
        public string DEMAND_VALUE_ERR { get; set; }

        /// <summary>
        /// 标准表需量值
        /// </summary>
        public string DEMAND_STANDARD { get; set; }

        /// <summary>
        /// 需量示值误差限
        /// </summary>
        public string DEMAND_VALUE_ERR_ABS { get; set; }

        /// <summary>
        /// 需量清零结果
        /// </summary>
        public string CLEAR_DATA_RST { get; set; }

        /// <summary>
        /// 示值误差结论
        /// </summary>
        public string VALUE_CONC_CODE { get; set; }


        /// <summary>
        /// 功率方向
        /// </summary>
        public string BOTH_WAY_POWER_FLAG { get; set; }


        /// <summary>
        /// 电压
        /// </summary>
        public string VOLTAGE { get; set; }








    }
}
