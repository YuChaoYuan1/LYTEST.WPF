using System;

namespace LYTest.Core.Model.DnbModel.DnbInfo
{
    [Serializable()]
    public class MeterErrAccordBase : MeterBase
    {
        public MeterErrAccordBase()
        {
            PrjID = "";
            Name = "";
            Result = ConstHelper.不合格;
            PF = "";
            IbX = "";
            UbX = "";
            Limit = "";
            PulseCount = "0";
            Freq = "";
            Data1 = "";
            Data2 = "";
            Error = "";
            ErrAver = "";
            SubItemId = "";
        }
        /// <summary>
        /// 误差项目ID
        /// </summary>
        public string PrjID { get; set; }

        /// <summary>
        /// 项目名称描述
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 检定点结论合格/不合格
        /// </summary>
        public string Result { get; set; }
        /// <summary>
        /// 功率因数1.0，0.5L等
        /// </summary>
        public string PF { get; set; }

        /// <summary>
        /// 电流倍数Imax,3.0Ib等
        /// </summary>
        public string IbX { get; set; }

        /// <summary>
        /// 电压倍数[1,0.5,0.8,1.2] 
        /// </summary>
        public string UbX { get; set; }

        /// <summary>
        /// 误差限   上线|下线
        /// </summary>
        public string Limit { get; set; }

        /// <summary>
        /// 圈数
        /// </summary>
        public string PulseCount { get; set; }

        /// <summary>
        /// 频率
        /// </summary>
        public string Freq { get; set; }

        /// <summary>
        /// 误差值1(第一次测量):误差一|误差二|...|误差平均|误差化整
        /// </summary>	
        public string Data1 { get; set; }

        /// <summary>
        /// 误差值2(第二次测量):误差一|误差二|...|误差平均|误差化整
        /// </summary>	
        public string Data2 { get; set; }

        /// <summary>
        /// 差值
        /// </summary>
        public string Error { get; set; }

        /// <summary>
        /// 样品均值                只对误差一致性有效    
        /// </summary>
        public string ErrAver { get; set; }

        /// <summary>
        /// 误差一致性的子项目编号，值为：{0}{1}，误差一致性类型，子项目序号+1
        /// </summary>
        public string SubItemId { get; set; }
    }
}
