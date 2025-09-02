using System;

namespace LYTest.Core.Model.DnbModel.DnbInfo
{
    //ADD WKW 20220527

    /// <summary>
    /// 电能示值组合误差
    /// </summary>
    [Serializable()]
    public class MeterEnergyError : MeterBase
    {
        /// <summary>
        /// 多功能项目ID	
        /// </summary>
        public string PrjID { get; set; }

        /// <summary>
        /// 走电前	----总|尖|峰|平|谷
        /// </summary>
        public string PowerOutBefore { get; set; }


        /// <summary>
        /// 走电后	----总|尖|峰|平|谷
        /// </summary>
        public string PowerOutAfter { get; set; }

        /// <summary>
        /// 走电前总
        /// </summary>
        public string PowerBeforeTotal { get; set; }

        /// <summary>
        /// 走电后总
        /// </summary>
        public string PowerAfterTotal { get; set; }

        /// <summary>
        /// 组合误差
        /// </summary>
        public string CombinationError { get; set; }

        /// <summary>
        /// 误差限
        /// </summary>
        public string WCRate { get; set; }

        /// <summary>
        /// 费率数
        /// </summary>
        public string Rate { get; set; }

        /// <summary>
        /// 试验时间
        /// </summary>
        public string TrialTime { get; set; }
        /// <summary>
        /// 电能增量
        /// </summary>
        public string EnergyIncrement { get; set; }

        /// <summary>
        /// 是否合格
        /// </summary>
        public string Result { get; set; }
    }
}
