using System;

namespace LYTest.Core.Model.DnbModel.DnbInfo
{
    /// <summary>
    /// 功耗检定数据
    /// </summary>
    [Serializable()]
    public class MeterPower : MeterBase
    {
        /// <summary>
        /// 功耗项目ID	
        /// </summary>
        public string PrjID { get; set; }
        /// <summary>
        /// 项目名称描述
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 项目值
        /// </summary>
        public string Value { get; set; }


        /// <summary>
        /// A相电压回路有功功率
        /// </summary>
        public string UaPowerP { get; set; }
        /// <summary>
        /// A相电压回路视在功率
        /// </summary>
        public string UaPowerS { get; set; }
        /// <summary>
        /// A相电流回路视在功率
        /// </summary>
        public string IaPowerS { get; set; }

        /// <summary>
        /// B相电压回路有功功率
        /// </summary>
        public string UbPowerP { get; set; }
        /// <summary>
        /// B相电压回路视在功率
        /// </summary>
        public string UbPowerS { get; set; }
        /// <summary>
        /// B相电流回路视在功率
        /// </summary>
        public string IbPowerS { get; set; }

        /// <summary>
        /// C相电压回路有功功率
        /// </summary>
        public string UcPowerP { get; set; }
        /// <summary>
        /// C相电压回路视在功率
        /// </summary>
        public string UcPowerS { get; set; }
        /// <summary>
        /// C相电流回路视在功率
        /// </summary>
        public string IcPowerS { get; set; }

        /// <summary>
        /// 方案信息
        /// </summary>
        public string TestValue { get; set; }

        /// <summary>
        /// 结论
        /// </summary>
        public string Result { get; set; }

        public string AVR_CUR_CIR_A_VOT { get; set; }
        public string AVR_CUR_CIR_B_VOT { get; set; }
        public string AVR_CUR_CIR_C_VOT { get; set; }
        public string AVR_CUR_CIR_A_CUR { get; set; }
        public string AVR_CUR_CIR_B_CUR { get; set; }
        public string AVR_CUR_CIR_C_CUR { get; set; }
        public string AVR_VOT_CIR_A_VOT { get; set; }
        public string AVR_VOT_CIR_B_VOT { get; set; }
        public string AVR_VOT_CIR_C_VOT { get; set; }
        public string AVR_VOT_CIR_A_CUR { get; set; }
        public string AVR_VOT_CIR_B_CUR { get; set; }
        public string AVR_VOT_CIR_C_CUR { get; set; }
        public string AVR_VOT_CIR_A_ANGLE { get; set; }
        public string AVR_VOT_CIR_B_ANGLE { get; set; }
        public string AVR_VOT_CIR_C_ANGLE { get; set; }

        public string AVR_CUR_CIR_S_LIMIT { get; set; }
        public string Mgh_chrISJL { get; set; }
        public string AVR_VOT_CIR_S_LIMIT { get; set; }
        public string Mgh_chrUSJL { get; set; }
        public string AVR_VOT_CIR_P_LIMIT { get; set; }
        public string Mgh_chrUPJL { get; set; }
        public string Mgh_chrJL { get; set; }
    }
}
