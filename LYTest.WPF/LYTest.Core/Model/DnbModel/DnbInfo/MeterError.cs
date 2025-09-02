using System;

namespace LYTest.Core.Model.DnbModel.DnbInfo
{
    [Serializable()]
    public class MeterError : MeterBase
    {
        public MeterError() : base()
        {
            PrjNo = "";
            WCType = 0;
            YJ = "H";
            Result = ConstHelper.默认结论;
            GLYS = "";
            GLFX = "";
            IbX = "";
            IbXString = "";
            WCData = "";
            Remark = "";
            WCHZ = "";
            WCValue = "";
            WCPC = "";
            WcMore = "";
            Limit = "";
            BPHUpLimit = "";
            BPHDownLimit = "";
            Circle = "2";
            HasBPH = false;
            PHData = "";
            PHVag = "";
            PHHZ = "";
            BPHWc = "";
            BPHHZ = "";


        }

        /// <summary>
        /// 误差项目ID
        /// </summary>
        public string PrjNo { get; set; }

        /// <summary>
        /// 误差类别
        /// </summary>
        public int WCType { get; set; }

        /// <summary>
        /// 元件
        /// </summary>
        public string YJ { get; set; }

        /// <summary>
        /// 项目结论[合格,不合格]
        /// </summary>
        public string Result { get; set; }

        /// <summary>
        /// 功率因数[1.0, 0.5L]
        /// </summary>
        public string GLYS { get; set; }

        /// <summary>
        /// 检定方向
        /// </summary>
        public string GLFX { get; set; }
        /// <summary>
        /// 额定电流的倍数
        /// </summary>
        public string IbX { get; set; }
        /// <summary>
        /// 9.额定电流IB的倍数的字符串（IB、IMAX）
        /// </summary>
        public string IbXString { get; set; }

        /// <summary>
        /// 误差值 误差一|误差二|...|误差平均|误差化整
        /// </summary>
        public string WCData { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }

        /// <summary>
        /// 误差化整值
        /// </summary>
        public string WCHZ { get; set; }
        /// <summary>
        /// 误差值
        /// </summary>
        public string WCValue { get; set; }

        /// <summary>
        /// 误差比例_黑龙江添加使用
        /// </summary>
        public string WCRatio { get; set; }

        /// <summary>
        /// 误差个数_黑龙江添加使用
        /// </summary>
        public string WCNumber { get; set; }

        /// <summary>
        /// 小数位数_黑龙江添加使用
        /// </summary>
        public string DecimalPlace { get; set; }

        /// <summary>
        /// 13.偏差值
        /// </summary>
        public string WCPC { get; set; }
        /// <summary>
        /// 14.更多次误差  更多次数误差，格式：误差10#误差11#...#...，以“#”分隔
        /// </summary>
        public string WcMore { get; set; }
        /// <summary>
        /// 15.误差限[上线|下线]
        /// </summary>
        public string Limit { get; set; }

        /// <summary>
        /// 16.不平衡负载与平衡负载时的上限 , 不加“+”号
        /// </summary>
        public string BPHUpLimit { get; set; }

        /// <summary>
        /// 17.不平衡负载与平衡负载时的下限
        /// </summary>
        public string BPHDownLimit { get; set; }

        /// <summary>
        /// 18.圈数
        /// </summary>
        public string Circle { get; set; }

        /// <summary>
        /// 19.不平衡负载与平衡负载时误差之差试验。0：没有，1：有
        /// </summary>
        public bool HasBPH { get; set; }
        /// <summary>
        /// 20.平衡负载时误差原始值，以#分隔
        /// </summary>
        public string PHData { get; set; }

        /// <summary>
        /// 21.平衡负载时误差平均值
        /// </summary>
        public string PHVag { get; set; }

        /// <summary>
        /// 22.平衡负载时误差平均值化整
        /// </summary>
        public string PHHZ { get; set; }

        /// <summary>
        /// 23.不平衡负载与平衡负载时误差之差（平均值的差）
        /// </summary>
        public string BPHWc { get; set; }

        /// <summary>
        /// 24.不平衡负载与平衡负载时误差之差化整
        /// </summary>
        public string BPHHZ { get; set; }

    }




}
