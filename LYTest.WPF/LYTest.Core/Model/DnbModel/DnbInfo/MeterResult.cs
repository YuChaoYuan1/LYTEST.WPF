using System;

namespace LYTest.Core.Model.DnbModel.DnbInfo
{

    [Serializable()]
    public class MeterResult : MeterBase
    {

        ///// <summary>
        ///// 4结论ID
        ///// </summary>
        //public string Mr_chrRstId = "";
        ///// <summary>
        ///// 5结论名称
        ///// </summary>
        //public string Mr_chrRstName = "";
        ///// <summary>
        ///// 6结论  0：默认，-1：不合格，1：合格
        ///// </summary>
        //public string Mr_chrRstValue = "";
        ///// <summary>
        ///// 7备注
        ///// </summary>
        //public string Mr_chrNote = "";

        public MeterResult() : base()
        {
            ResultKey = "";
            ResultName = "";
            Result = "";
            Note = "";
        }

        /// <summary>
        /// 4结论ID
        /// </summary>
        public string ResultKey { get; set; }

        /// <summary>
        /// 5结论名称
        /// </summary>
        public string ResultName { get; set; }

        /// <summary>
        /// 6结论[--, 不合格, 合格]
        /// </summary>
        public string Result { get; set; }

        /// <summary>
        /// 7备注
        /// </summary>
        public string Note { get; set; }


    }
}
