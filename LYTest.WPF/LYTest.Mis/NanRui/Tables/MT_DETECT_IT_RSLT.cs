namespace LYTest.Mis.NanRui.Tables
{
    //互感器检定/校准综合结论
    public class MT_DETECT_IT_RSLT
    {
        /// <summary>
        /// A1
        /// </summary>
        public string DETECT_TASK_NO { get; set; }

        /// <summary>
        /// A6
        /// </summary>
        public string BAR_CODE { get; set; }

        /// <summary>
        /// A14
        /// </summary>
        public string WRITE_DATE { get; set; }

        /// <summary>
        /// A15           0-未处理（默认）；1-处理中；2-已处理
        /// </summary>
        public string HANDLE_FLAG { get; set; }

        /// <summary>
        /// A16
        /// </summary>
        public string HANDLE_DATE { get; set; }

        /// <summary>
        /// 耐压保持时间
        /// </summary>
        public string PUN_HOL_TIME { get; set; }

        /// <summary>
        /// 磁饱和裕度比差值150	
        /// </summary>
        public string MAR_RATIO { get; set; }

        /// <summary>
        /// 磁饱和裕度角差值150
        /// </summary>
        public string MAR_ANGLE { get; set; }

        /// <summary>
        /// 二次匝间绝缘强度试验时间	
        /// </summary>
        public string INSULATION_TIME { get; set; }

        /// <summary>
        /// 极性检查结论	STRING16	极性
        /// </summary>
        public string POLARITY { get; set; }

        /// <summary>
        /// 温度	
        /// </summary>
        public string TEMP { get; set; }

        /// <summary>
        /// 湿度
        /// </summary>
        public string HUMIDITY { get; set; }

        /// <summary>
        /// 检定人	
        /// </summary>
        public string TESTER_NO { get; set; }

        /// <summary>
        /// 校验人员
        /// </summary>
        public string CHECKER_NO { get; set; }

        /// <summary>
        /// 检定日期，格式yyyy-MM-dd HH:mm:ss，如2012-06-08 09:32:31
        /// </summary>
        public string CHK_DATE { get; set; }

        /// <summary>
        /// 有效日期	
        /// </summary>
        public string VALIDITY_DATE { get; set; }

        /// <summary>
        /// 外观检查试验结论，	见附录D：结论
        /// </summary>
        public string INTUITIVE_CONC_CODE { get; set; }

        /// <summary>
        /// 绝缘电阻试验结论	，	见附录D：结论
        /// </summary>
        public string INSULATION_CONC { get; set; }

        /// <summary>
        /// 耐压试验结论，见附录D：结论
        /// </summary>
        public string VOLT_CONC { get; set; }

        /// <summary>
        /// 磁饱和裕度试验结论	，	见附录D：结论
        /// </summary>
        public string MAR_CONC { get; set; }

        /// <summary>
        /// 二次匝间绝缘强度试验结论	，见附录D：结论
        /// </summary>
        public string INSU_STR_CONC { get; set; }

        /// <summary>
        /// 误差试验结论	，见附录D：结论
        /// </summary>
        public string CHK_CONC_CODE { get; set; }

        /// <summary>
        /// 检定总结论	，见附录D：结论
        /// </summary>
        public string CHK_CONC { get; set; }

        /// <summary>
        /// 绝缘电阻一次对二次及地	
        /// </summary>
        public string INSU_FST_GROUND { get; set; }

        /// <summary>
        /// 绝缘电阻二次对地
        /// </summary>
        public string INSU_SND_GROUND { get; set; }

        /// <summary>
        /// 一次绕组对二次绕组及地电压T
        /// </summary>
        public string FST_SND_VOLT { get; set; }

        /// <summary>
        /// 二次绕组对地电压
        /// </summary>
        public string SND_GROUND_VOLT { get; set; }

    }
}