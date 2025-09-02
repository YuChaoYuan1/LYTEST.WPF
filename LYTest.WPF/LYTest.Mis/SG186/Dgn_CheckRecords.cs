namespace LYTest.Mis.SG186
{
    /// <summary>
    /// 电能表检定多功能检定项目
    /// </summary>
    public struct Dgn_CheckRecords
    {
        /// <summary> 
        ///项目记录标识
        ///本记录的唯一标识号, 由校表台系统填写,具体填写不做要求
        /// </summary>
        public string Id { get; set; }

        /// <summary> 
        ///记录标识 
        ///该字段为外键字段
        ///</summary> 
        public string Read_id { get; set; }

        /// <summary> 
        ///电能表标识 
        ///电能表编号
        ///</summary> 
        public string Meter_id { get; set; }

        /// <summary> 
        ///费率时段检查
        ///本处要求读出电能表内实际时段，例如平07：00-08：00，并与规定时段比较
        /// </summary> 
        public string Ar_ts_chk { get; set; }

        /// <summary> 
        ///费率时段检查结论
        ///1合格，0不合格
        /// </summary> 
        public string TS_CHK_CONC_CODE { get; set; }

        /// <summary> 
        ///由电源供电的时钟试验(s)1
        /// </summary> 
        public string DAILY_TIMING_ERR1 { get; set; }

        /// <summary> 
        ///由电源供电的时钟试验(s)2
        /// </summary> 
        public string DAILY_TIMING_ERR2 { get; set; }

        /// <summary> 
        ///由电源供电的时钟试验(s)3
        /// </summary> 
        public string DAILY_TIMING_ERR3 { get; set; }

        /// <summary> 
        ///由电源供电的时钟试验(s)4
        /// </summary> 
        public string DAILY_TIMING_ERR4 { get; set; }

        /// <summary> 
        ///由电源供电的时钟试验(s)5
        /// </summary> 
        public string DAILY_TIMING_ERR5 { get; set; }

        /// <summary> 
        ///由电源供电的时钟试验(s)6
        /// </summary> 
        public string DAILY_TIMING_ERR6 { get; set; }

        /// <summary> 
        ///由电源供电的时钟试验(s)7
        /// </summary> 
        public string DAILY_TIMING_ERR7 { get; set; }

        /// <summary> 
        ///由电源供电的时钟试验(s)8
        /// </summary> 
        public string DAILY_TIMING_ERR8 { get; set; }

        /// <summary> 
        ///由电源供电的时钟试验(s)9
        /// </summary> 
        public string DAILY_TIMING_ERR9 { get; set; }

        /// <summary> 
        ///由电源供电的时钟试验(s)0
        /// </summary> 
        public string DAILY_TIMING_ERR10 { get; set; }

        /// <summary> 
        ///由电源供电的时钟试验平均值
        /// </summary> 
        public string DAILY_TIMING_ERR_AVG { get; set; }

        /// <summary> 
        ///由电源供电的时钟试验化整值
        /// </summary> 
        public string DAILY_TIMING_ERR_INT { get; set; }

        /// <summary> 
        ///需量示数误差(负荷点Imax)标准值
        /// </summary> 
        public string DE_STD_IMAX { get; set; }

        /// <summary> 
        ///需量示数误差(负荷点Imax)实际值
        /// </summary> 
        public string DE_IMAX { get; set; }

        /// <summary> 
        ///需量示数误差(负荷点Imax)
        /// </summary> 
        public string DEMAND_READING_ERR { get; set; }

        /// <summary> 
        ///需量示数误差(负荷点Imax)化整值
        /// </summary> 
        public string DE_INT_IMAX { get; set; }

        /// <summary> 
        ///需量示数误差(负荷点Ib)标准值
        /// </summary> 
        public string DE_STD_IB { get; set; }

        /// <summary> 
        ///需量示数误差(负荷点Ib)实际值
        /// </summary> 
        public string DE_IB_ACT { get; set; }

        /// <summary> 
        ///需量示数误差(负荷点Ib)
        /// </summary> 
        public string DE_IB { get; set; }

        /// <summary> 
        ///需量示数误差(负荷点Ib)化整值
        /// </summary> 
        public string DE_IB_INT { get; set; }

        /// <summary> 
        ///需量示数误差(负荷点0.1Ib)标准值
        /// </summary> 
        public string DE_P1IB_STD { get; set; }

        /// <summary> 
        ///需量示数误差(负荷点0.1Ib)实际值
        /// </summary> 
        public string DE_P1IB_ACT { get; set; }

        /// <summary> 
        ///需量示数误差(负荷点0.1Ib)
        /// </summary> 
        public string DE_P1IB { get; set; }

        /// <summary> 
        ///需量示数误差(负荷点0.1Ib)化整值
        /// </summary> 
        public string DE_P1IB_INT { get; set; }

        /// <summary> 
        ///需量选定周期(负荷点Ib)
        /// </summary> 
        public string SEL_PERIOD { get; set; }

        /// <summary> 
        ///需量实测周期(负荷点Ib)
        /// </summary> 
        public string DMD_PERIOD_IB { get; set; }

        /// <summary> 
        ///需量周期误差(负荷点Ib)
        /// </summary> 
        public string DE_PERIOD_IB { get; set; }

        /// <summary> 
        ///需量周期误差(负荷点Ib)化整值
        /// </summary> 
        public string DE_PERIOD_IB_INT { get; set; }

        /// <summary> 
        ///(电压中断ΔU=100%，t=1s) 变化前电量
        /// </summary> 
        public string BF_PQ { get; set; }

        /// <summary> 
        ///(电压中断ΔU=100%，t=1s) 变化后电量
        /// </summary> 
        public string AF_PQ { get; set; }

        /// <summary> 
        ///(电压中断ΔU=100%，t=1s) 结论
        ///1合格，0不合格
        /// </summary> 
        public string CONC { get; set; }

        /// <summary> 
        ///(电压中断ΔU=100%，t=20ms) 变化前电量
        /// </summary> 
        public string BF_PQ_U100T20MS { get; set; }

        /// <summary> 
        ///(电压中断ΔU=100%，t=20ms) 变化后电量
        /// </summary> 
        public string AF_PQ_U100T20MS { get; set; }

        /// <summary> 
        ///(电压中断ΔU=100%，t=20ms) 结论
        ///1合格，0不合格
        /// </summary> 
        public string CONC_U100T20MS { get; set; }

        /// <summary> 
        ///(电压降落ΔU=50%，t=1min) 变化前电量
        /// </summary> 
        public string BF_PQ_U50T1M { get; set; }

        /// <summary> 
        ///(电压降落ΔU=50%，t=1min) 变化后电量
        /// </summary> 
        public string AF_PQ_U50T1M { get; set; }

        /// <summary> 
        ///(电压降落ΔU=50%，t=1min) 结论
        ///1合格，0不合格
        /// </summary> 
        public string CONCLUSION_U50T1M { get; set; }

        /// <summary> 
        ///通讯接口测试结论
        ///1合格，0不合格
        /// </summary> 
        public string CI_CHK_CONC_CODE { get; set; }

        /// <summary> 
        ///无功存储器检查
        ///01合格、02不合格
        /// </summary> 
        public string RP_MEMORY_CHK { get; set; }

        /// <summary> 
        ///其它存储器检查
        ///01合格、02不合格
        /// </summary> 
        public string OTHER_MEMORY_CHK { get; set; }

        /// <summary> 
        ///GPS对时
        ///01是、02否
        /// </summary> 
        public string GPS_CALIBRATE_FLAG { get; set; }

        /// <summary> 
        ///费率时段投切误差结论
        ///1合格，0不合格
        /// </summary> 
        public string TS_ERR_CONC_CODE { get; set; }

        /// <summary>
        /// 一次投切类型
        /// </summary>
        public string CHANGE1_TYPE { get; set; }

        /// <summary>
        /// 二次投切类型
        /// </summary>
        public string CHANGE2_TYPE { get; set; }

        /// <summary>
        /// 一次投切误差
        /// </summary>
        public string CHANGE1_ERR { get; set; }

        /// <summary>
        /// 二次投切误差
        /// </summary>
        public string CHANGE2_ERR { get; set; }

        /// <summary>
        /// 一次投切误差化整
        /// </summary>
        public string INT_CHANGE1_ERR { get; set; }

        /// <summary>
        /// 二次投切误差化整
        /// </summary>
        public string INT_CHANGE2_ERR { get; set; }

        /// <summary>
        /// 需量示值误差结论
        /// </summary>
        public string DE_ERR_CONC { get; set; }

        /// <summary>
        /// 需量周期误差结论
        /// </summary>
        public string DMD_PERIOD_ERR_CONC { get; set; }

        /// <summary>
        /// 由电源供电的时钟试验结论
        /// </summary>
        public string DAILY_TIMING_ERR_CONC { get; set; }
    }
}
