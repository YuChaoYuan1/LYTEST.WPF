namespace LYTest.Mis.HeBei20.Tables
{
    /// <summary>
    /// 影响量试验
    /// </summary>
    public class MT_INFLUENCE_QTY_MET_CONC : MT_MET_CONC_Base
    {
        /// <summary>
        /// 0：无效 1：有效
        /// </summary>
        public string IS_VALID { get; set; }
        /// <summary>
        /// 结论
        /// </summary>
        public string CHK_CONC_CODE { get; set; }


        private string _EFFECT_TEST_ITEM;
        /// <summary>
        /// 影响量试验项目 ,1个中文3个长度
        /// </summary>
        public string EFFECT_TEST_ITEM
        {
            get { return _EFFECT_TEST_ITEM; }
            set
            {
                if (value.Length > 5)
                    _EFFECT_TEST_ITEM = value.Substring(0, 5);
                else
                    _EFFECT_TEST_ITEM = value;
            }
        }


        //新加

        /// <summary>
        /// 供电单位
        /// </summary>
        public string OPC_NO { get; set; }

        /// <summary>
        /// 地区代码
        /// </summary>
        public string AREA_CODE { get; set; }
    }
}
