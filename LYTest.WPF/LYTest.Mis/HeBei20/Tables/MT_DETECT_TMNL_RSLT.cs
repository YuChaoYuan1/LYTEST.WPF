namespace LYTest.Mis.HeBei20.Tables
{
    //采集终端检定/校准综合结论
    public class MT_DETECT_TMNL_RSLT
    {
        /// <summary>
        /// A1
        /// </summary>
        public string DETECT_TASK_NO { get; set; }
        /// <summary>
        /// A2
        /// </summary>
        public string SYS_NO { get; set; }
        /// <summary>
        /// A3
        /// </summary>
        public string DETECT_EQUIP_NO { get; set; }
        /// <summary>
        /// 线体编号
        /// </summary>
        public string LINE_NO { get; set; }
        /// <summary>
        /// A5
        /// </summary>
        public string POSITION_NO { get; set; }
        /// <summary>
        /// A6
        /// </summary>
        public string BAR_CODE { get; set; }
        /// <summary>
        /// A7
        /// </summary>
        public string DETECT_DATE { get; set; }
        /// <summary>
        /// A12          01合格、02不合格 
        /// </summary>
        public string CONC_CODE { get; set; }
        /// <summary>
        /// 参数读取结论
        /// </summary>
        public string READ_CONC_CODE { get; set; }
        /// <summary>
        /// 参数设置结论
        /// </summary>
        public string SETTING_CONC_CODE { get; set; }
        /// <summary>
        /// 电源电压变化影响试验结论
        /// </summary>
        public string VOLT_CONC_CODE { get; set; }
        /// <summary>
        /// 交流模拟量误差结论
        /// </summary>
        public string SIMU_CONC_CODE { get; set; }
        /// <summary>
        /// 脉冲量采集结论
        /// </summary>
        public string PULSE_CONC_CODE { get; set; }
        /// <summary>
        /// 状态量采集结论
        /// </summary>
        public string STATE_CONC_CODE { get; set; }
        /// <summary>
        /// 电流回路反向结论
        /// </summary>
        public string CURRENT_CONC_CODE { get; set; }
        /// <summary>
        /// 电压回路断相结论
        /// </summary>
        public string LOST_CONC_CODE { get; set; }
        /// <summary>
        /// 电压回路失压结论
        /// </summary>
        public string LOW_CONC_CODE { get; set; }
        /// <summary>
        /// 电压相序异常结论
        /// </summary>
        public string EXCP_CONC_CODE { get; set; }
        /// <summary>
        /// 电压不平衡结论
        /// </summary>
        public string EXBALA_CONC_CODE { get; set; }
        /// <summary>
        /// 电流不平衡结论
        /// </summary>
        public string CURR_CONC_CODE { get; set; }
        /// <summary>
        /// 电压越上上限结论
        /// </summary>
        public string VOLT_OVERLOAD_CONC_CODE { get; set; }
        /// <summary>
        /// 电流越上上限结论
        /// </summary>
        public string OVERLOAD_CONC_CODE { get; set; }
        /// <summary>
        /// 视在功率越上上限结论
        /// </summary>
        public string POWEROVER_CONC_CODE { get; set; }
        /// <summary>
        /// 参数更改记录结论
        /// </summary>
        public string CHANGE_CONC_CODE { get; set; }
        /// <summary>
        /// 电能表时段费率变化结论
        /// </summary>
        public string FEE_CONC_CODE { get; set; }
        /// <summary>
        /// 电能表编程时间更改结论
        /// </summary>
        public string PROG_CONC_CODE { get; set; }
        /// <summary>
        /// 电能表编程次数更改结论
        /// </summary>
        public string PROGTIMES_CONC_CODE { get; set; }
        /// <summary>
        /// 电能表脉冲常数更改结论
        /// </summary>
        public string CONST_CONC_CODE { get; set; }
        /// <summary>
        /// 电能表抄表日更改结论
        /// </summary>
        public string METERDATE_CONC_CODE { get; set; }

        /// <summary>
        /// 有功总电量差动结论
        /// </summary>
        public string EQ_CONC_CODE { get; set; }
        /// <summary>
        /// 电能表最大需量清零结论
        /// </summary>
        public string MAX_CONC_CODE { get; set; }
        /// <summary>
        /// 电能表最大需量清零次数变更事件结论
        /// </summary>
        public string DEMAND_CONC_CODE { get; set; }
        /// <summary>
        /// 电能表电能量超差结论
        /// </summary>
        public string ELEC_CONC_CODE { get; set; }
        /// <summary>
        /// 电能表电池欠压结论
        /// </summary>
        public string BATTVOLT_CONC_CODE { get; set; }
        /// <summary>
        /// 电能表示度下降结论
        /// </summary>
        public string EQDOWN_CONC_CODE { get; set; }
        /// <summary>
        /// 电能表飞走结论
        /// </summary>
        public string FAST_CONC_CODE { get; set; }
        /// <summary>
        /// 电能表停走结论
        /// </summary>
        public string STOP_CONC_CODE { get; set; }
        /// <summary>
        /// 终端485抄表故障结论
        /// </summary>
        public string READERR_CONC_CODE { get; set; }
        /// <summary>
        /// 电能表时钟超差结论
        /// </summary>
        public string TIME_CONC_CODE { get; set; }
        /// <summary>
        /// 实时数据结论
        /// </summary>
        public string REAL_CONC_CODE { get; set; }
        /// <summary>
        /// 曲线数据结论
        /// </summary>
        public string CURVE_CONC_CODE { get; set; }
        /// <summary>
        /// 历史日数据结论
        /// </summary>
        public string HISDAY_CONC_CODE { get; set; }
        /// <summary>
        /// 历史月数据结论
        /// </summary>
        public string HISMONTH_CONC_CODE { get; set; }
        /// <summary>
        /// 对时功能结论
        /// </summary>
        public string TIMER_CONC_CODE { get; set; }
        /// <summary>
        /// 遥控功能结论
        /// </summary>
        public string REMOTE_CONC_CODE { get; set; }
        /// <summary>
        /// 功率下浮控制结论
        /// </summary>
        public string POWERDOWN_CONC_CODE { get; set; }
        /// <summary>
        /// 月电量控制测试结论
        /// </summary>
        public string MONEQ_CONC_CODE { get; set; }
        /// <summary>
        /// 购电控制结论
        /// </summary>
        public string BUY_CONC_CODE { get; set; }
        /// <summary>
        /// 时段控制测试结论
        /// </summary>
        public string PERION_CONC_CODE { get; set; }
        /// <summary>
        /// 厂休控结论
        /// </summary>
        public string FACTORY_CONC_CODE { get; set; }
        /// <summary>
        /// 营业报停控结论
        /// </summary>
        public string ELECON_CONC_CODE { get; set; }
        /// <summary>
        /// 终端停电结论
        /// </summary>
        public string ELECOFF_CONC_CODE { get; set; }
        /// <summary>
        /// 终端上电结论
        /// </summary>
        public string ELECTRIFY_CONC_CODE { get; set; }
        /// <summary>
        /// 保电剔除结论
        /// </summary>
        public string REMOVE_CONC_CODE { get; set; }
        /// <summary>
        /// 催费告警结论
        /// </summary>
        public string WARN_CONC_CODE { get; set; }
        /// <summary>
        /// 透明转发结论
        /// </summary>
        public string TRANSMIT_CONC_CODE { get; set; }
        /// <summary>
        /// 定时发送一类数据结论
        /// </summary>
        public string SNDREAL_CONC_CODE { get; set; }
        /// <summary>
        /// 定时发送二类数据结论
        /// </summary>
        public string SNDHIS_CONC_CODE { get; set; }
        /// <summary>
        /// 抄表试验结论
        /// </summary>
        public string DLB_CONC_CODE { get; set; }
        /// <summary>
        /// 参数恢复结论
        /// </summary>
        public string RESTORE_CONC_CODE { get; set; }
        /// <summary>
        /// 设置主站IP地址和端口结论
        /// </summary>
        public string SETIP_CONC_CODE { get; set; }
        /// <summary>
        /// 外观检查试验结论
        /// </summary>
        public string FACADE_CONC_CODE { get; set; }
        /// <summary>
        /// A14
        /// </summary>
        public string WRITE_DATE { get; set; }
        /// <summary>
        /// A16
        /// </summary>
        public string HANDLE_DATE { get; set; }
        /// <summary>
        /// A15           0-未处理（默认）；1-处理中；2-已处理
        /// </summary>
        public string HANDLE_FLAG { get; set; }
        /// <summary>
        /// DLB 2007抄表试验	，	DLB 2007抄表试验
        /// </summary>
        public string DLB_TZZS_CONC_CODE { get; set; }
        /// <summary>
        /// 功耗试验	，	功耗试验
        /// </summary>
        public string POWER_TEST { get; set; }
        /// <summary>
        /// 信道试验，信道试验
        /// </summary>
        public string CHANNEL_TEST { get; set; }
        /// <summary>
        /// 接地故障试验，接地故障试验
        /// </summary>
        public string GROUNDING_TEST { get; set; }
        /// <summary>
        /// 电压暂降和短时中断	，	电压暂降和短时中断
        /// </summary>
        public string DIPS_SHORT_TEST { get; set; }
        /// <summary>
        /// 连续通电稳定性
        /// </summary>
        public string CONTINUOUS_STABILITY { get; set; }
        /// <summary>
        /// 谐波分量基本误差
        /// </summary>
        public string HARMONIC_COMPONENT { get; set; }
        /// <summary>
        /// 检定人员
        /// </summary>
        public string DETECT_PERSON { get; set; }
        /// <summary>
        /// 审核人员	
        /// </summary>
        public string AUDIT_PERSON { get; set; }
        /// <summary>
        /// 铅封线处理标记，见附录D：处理标记
        /// </summary>
        public string SEAL_HANDLE_FLAG { get; set; }
        /// <summary>
        /// 铅封线处理时间	，处理时间,该域由铅封线系统填写
        /// </summary>
        public string SEAL_HANDLE_DATE { get; set; }
        /// <summary>
        /// 温度
        /// </summary>
        public string TEMP { get; set; }
        /// <summary>
        /// 湿度
        /// </summary>
        public string HUMIDITY { get; set; }
    }
}