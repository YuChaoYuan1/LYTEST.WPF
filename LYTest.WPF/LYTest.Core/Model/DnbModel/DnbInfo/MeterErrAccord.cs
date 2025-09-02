using System;
using System.Collections.Generic;

namespace LYTest.Core.Model.DnbModel.DnbInfo
{
    [Serializable()]
    public class MeterErrAccord : MeterBase
    {
        public MeterErrAccord()
        {
            PointList = new Dictionary<string, MeterErrAccordBase>();
            Result = ConstHelper.合格;
        }

        /// <summary>
        /// 误差一致性类型 1：误差一致性 2：误差变差 3：负载电流升降 4：电流过载
        /// </summary>
        public int Type { get; set; }

        /// <summary>
        /// 项目结论[合格,不合格]
        /// </summary>
        public string Result { get; set; }

        /// <summary>
        /// 进度值
        /// </summary>
        public string ProgressValue { get; set; }

        /// <summary>
        /// 该项目下包含的检定点
        /// </summary>
        public Dictionary<string, MeterErrAccordBase> PointList { get; private set; }
        /// <summary>
        /// 不合格原因
        /// </summary>
        public string Description { get; set; }
    }
}
