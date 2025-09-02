using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LYTest.Mis.MDS.Table
{
    public class MT_DETECT_RSLT2 : MT_MET_CONC_Base
    {


        /// <summary>
        /// 外观检查结论
        /// </summary>
        public string INTUIT_CONC_CODE { get; set; }

        /// <summary>
        ///基本误差结论
        /// </summary>
        public string BASICERR_CONC_CODE { get; set; }

        /// <summary>
        /// 电能表常数试验结论
        /// </summary>
        public string CONST_CONC_CODE { get; set; }


        /// <summary>
        /// 起动试验结论
        /// </summary>
        public string STARTING_CONC_CODE { get; set; }
        /// <summary>
        /// 潜动试验结论
        /// </summary>
        public string CREEPING_CONC_CODE { get; set; }

        /// <summary>
        /// 日计时误差结论
        /// </summary>
        public string DAYERR_CONC_CODE { get; set; }

        /// <summary>
        /// 功率消耗结论
        /// </summary>
        public string POWER_CONC_CODE { get; set; }

        /// <summary>
        /// ELE_CHK_CONC_CODE
        /// </summary>
        public string ELE_CHK_CONC_CODE { get; set; }

        /// <summary>
        /// 测量重复性试验(新规)
        /// </summary>
        public string MEA_REP_CONC_CODE { get; set; }

        /// <summary>
        /// 交流电压试验结论
        /// </summary>
        public string VOLT_CONC_CODE { get; set; }

        /// <summary>
        /// 规约一致性检查结论
        /// </summary>
        public string STANDARD_CONC_CODE { get; set; }

        /// <summary>
        /// 载波通信性能试验结论
        /// </summary>
        public string WAVE_CONC_CODE { get; set; }

        /// <summary>
        /// 误差变差试验结论
        /// </summary>
        public string ERROR_CONC_CODE { get; set; }

        /// <summary>
        /// 误差一致性试验结论
        /// </summary>
        public string CONSIST_CONC_CODE { get; set; }

        /// <summary>
        /// 负载电流升降变差试验结论
        /// </summary>
        public string VARIATION_CONC_CODE { get; set; }

        /// <summary>
        /// 电流过载试验结论
        /// </summary>
        public string OVERLOAD_CONC_CODE { get; set; }

        /// <summary>
        /// 时段投切误差结论
        /// </summary>
        public string TS_CONC_CODE { get; set; }

        /// <summary>
        /// 走字试验结论
        /// </summary>
        public string RUNING_CONC_CODE { get; set; }

        /// <summary>
        /// MAX_DEMAND_ERROR_CONC_CODE
        /// </summary>
        public string MAX_DEMAND_ERROR_CONC_CODE { get; set; }

        /// <summary>
        /// 需量周期误差结论
        /// </summary>
        public string PERIOD_CONC_CODE { get; set; }

        /// <summary>
        /// 需量示值误差结论
        /// </summary>
        public string VALUE_CONC_CODE { get; set; }

        /// <summary>
        /// 密钥下装结论
        /// </summary>
        public string KEY_CONC_CODE { get; set; }

        /// <summary>
        /// METER_ERROR_CONC_CODE
        /// </summary>
        public string METER_ERROR_CONC_CODE { get; set; }

        /// <summary>
        /// TIME_ERROR_CONC_CODE
        /// </summary>
        public string TIME_ERROR_CONC_CODE { get; set; }

        /// <summary>
        /// 参数设置结论
        /// </summary>
        public string SETTING_CONC_CODE { get; set; }

        /// <summary>
        /// 费控安全认证试验结论
        /// </summary>
        public string ESAM_CONC_CODE { get; set; }

        /// <summary>
        /// 费控远程数据回抄试验结论
        /// </summary>
        public string REMOTE_CONC_CODE { get; set; }

        /// <summary>
        /// 费控保电试验结论
        /// </summary>
        public string EH_CONC_CODE { get; set; }

        /// <summary>
        /// 费控告警试验结论
        /// </summary>
        public string WARN_CONC_CODE { get; set; }

        /// <summary>
        /// 剩余电量递减试验结论
        /// </summary>
        public string SURPLUS_CONC_CODE { get; set; }

        /// <summary>
        /// 费控取消保电结论
        /// </summary>
        public string EC_CONC_CODE { get; set; }

        /// <summary>
        /// 费控取消告警结论
        /// </summary>
        public string WARN_CANCEL_CONC_CODE { get; set; }

        /// <summary>
        /// 费控合闸试验结论
        /// </summary>
        public string SWITCH_ON_CONC_CODE { get; set; }

        /// <summary>
        /// 费控拉闸结论
        /// </summary>
        public string SWITCH_OUT_CONC_CODE { get; set; }

        /// <summary>
        /// 电能表清零结论
        /// </summary>
        public string RESET_EQ_MET_CONC_CODE { get; set; }

        /// <summary>
        /// 最大需量清零
        /// </summary>
        public string RESET_DEMAND_MET_CONC_CODE { get; set; }

        /// <summary>
        /// 广播校时试验
        /// </summary>
        public string TIMING_MET_CONC_CODE { get; set; }

        /// <summary>
        /// 通讯测试
        /// </summary>
        public string COMMINICATE_MET_CONC_CODE { get; set; }

        /// <summary>
        /// 探测表地址
        /// </summary>
        public string ADDRESS_MET_CONC_CODE { get; set; }

        /// <summary>
        /// 多功能口测试
        /// </summary>
        public string MULTI_INTERFACE_MET_CONC_CODE { get; set; }

        /// <summary>
        /// 闰年切换
        /// </summary>
        public string LEAP_YEAR_MET_CONC { get; set; }

        /// <summary>
        /// 任意数据读取
        /// </summary>
        public string PARA_READ_MET_CONC_CODE { get; set; }

        /// <summary>
        /// 任意数据写入
        /// </summary>
        public string PARA_SET_MET_CONC { get; set; }

        /// <summary>
        /// 标准偏差
        /// </summary>
        public string DEVIATION_MET_CONC { get; set; }

        /// <summary>
        /// GPS校时
        /// </summary>
        public string GPS_CONC { get; set; }

        /// <summary>
        /// 检定人员
        /// </summary>
        public string DETECT_PERSON { get; set; }

        /// <summary>
        /// 审核人员
        /// </summary>
        public string AUDIT_PERSON { get; set; }



        /// <summary>
        /// 施封线处理标记
        /// </summary>
        public string SEAL_HANDLE_FLAG { get; set; }

        /// <summary>
        /// 施封线处理时间
        /// </summary>
        public string SEAL_HANDLE_DATE { get; set; }



        /// <summary>
        /// 报警功能
        /// </summary>
        public string INFLUENCE_QTY_CONC_CODE { get; set; }

        /// <summary>
        /// 计量功能试验结论
        /// </summary>
        public string ELE_ENERGY_FUNC_CONC_CODE { get; set; }

        /// <summary>
        /// 最大需量功能结论
        /// </summary>
        public string MAX_DEMAND_FUNC_CONC_CODE { get; set; }

        /// <summary>
        /// 费率时段功能结论
        /// </summary>
        public string RATE_TIME_FUNC_CONC_CODE { get; set; }

        /// <summary>
        /// 事件记录功能试验
        /// </summary>
        public string TIM_FUNC_CONC_CODE { get; set; }

        /// <summary>
        /// 脉冲输出功能
        /// </summary>
        public string PULSE_OUT_FUNC_CONC_CODE { get; set; }

        /// <summary>
        /// 控制功能结论 @C_B
        /// </summary>
        public string CONTROL_FUNC_CONC_CODE { get; set; }

        /// <summary>
        /// 预置内容设置结论 @C_B
        /// </summary>
        public string PRESET_CONTENT_SET_CONC_CODE { get; set; }

        /// <summary>
        /// 预置内容检查结论 @C_B
        /// </summary>
        public string PRESET_CONTENT_CHECK_CONC_CODE { get; set; }

        /// <summary>
        /// 温度
        /// </summary>
        public string TEMP { get; set; }

        /// <summary>
        /// 湿度
        /// </summary>
        public string HUMIDITY { get; set; }
        /// <summary>
        /// 预置参数检查1
        /// </summary>

        public string PRESET_PARAMET_CHECK_CONC_CODE { get; set; }

        /// <summary>
        /// 预置参数检查2
        /// </summary>
        public string PRESET_PARAM2_CHECK_CONC_CODE { get; set; }
        /// <summary>
        /// 预置参数检查3
        /// </summary>
        public string PRESET_PARAM3_CHECK_CONC_CODE { get; set; }

        /// <summary>
        /// 预置参数设置1
        /// </summary>
        public string PRESET_PARAMETER_SET_CONC_CODE { get; set; }
        /// <summary>
        /// 485密码修改
        /// </summary>
        public string PASSWORD_CHANGE_CONC_CODE { get; set; }
        /// <summary>
        /// 硬件版本
        /// </summary>
        public string HARD_VERSION { get; set; }

        public string CLOCK_VALUE_CONC_CODE { get; set; }

        public string DISPLAY_FUNC_CONC_CODE { get; set; }

        public string PURSE_INIT_CONC_CODE { get; set; }

        /// <summary>
        /// HPLC芯片ID认证
        /// </summary>
        public string HPLC_CERT_CONC_CODE { get; set; }


        //新加

        /// <summary>
        /// 红外通信测试结论
        /// </summary>
        public string INFRARED_TEST_CONC_CODE { get; set; }

        /// <summary>
        /// 可靠性验证试验
        /// </summary>
        public string RELIABILITY_CONC_CODE { get; set; }

        /// <summary>
        /// 动稳定试验
        /// </summary>
        public string MOVE_STABILITY_TEST_CONC_CODE { get; set; }

        /// <summary>
        /// 抗震性能试验
        /// </summary>
        public string ANTI_SEISMICTEST_CONC_CODE { get; set; }

        /// <summary>
        /// 预置参数设置2
        /// </summary>
        public string PRESET_PARAM2_SET_CONC_CODE { get; set; }

        /// <summary>
        /// 预置参数设置3
        /// </summary>
        public string PRESET_PARAM3_SET_CONC_CODE { get; set; }
    }
       /// <summary>
       /// 总结论
       /// </summary>
    public class MT_DETECT_RSLT : MT_MET_CONC_Base
    {

        ///// <summary>
        ///// 检定任务编号    
        ///// </summary>
        //public string DETECT_TASK_NO { get; set; }
        ///// <summary>
        ///// 设备类别      
        ///// </summary>
        //public string EQUIP_CATEG { get; set; }
        ///// <summary>
        ///// 系统编号      
        ///// </summary>
        //public string SYS_NO { get; set; }
        ///// <summary>
        ///// 检定线台编号  
        ///// </summary>
        //public string DETECT_EQUIP_NO { get; set; }
        ///// <summary>
        ///// 检定单元编号
        ///// </summary>
        //public string DETECT_UNIT_NO { get; set; }
        ///// <summary>
        ///// 表位编号
        ///// </summary>
        //public string POSITION_NO { get; set; }
        ///// <summary>
        ///// 设备条形码
        ///// </summary>
        //public string BAR_CODE { get; set; }
        ///// <summary>
        ///// 检定时间
        ///// </summary>
        //public string DETECT_DATE { get; set; }
        ///// <summary>
        ///// 检定总结论
        ///// </summary>
        //public string CONC_CODE { get; set; }
        /// <summary>
        /// 外观检查结论
        /// </summary>
        public string INTUIT_CONC_CODE { get; set; }
        /// <summary>
        /// 基本误差结论
        /// </summary>
        public string BASICERR_CONC_CODE { get; set; }
        /// <summary>
        /// 电能表常数试验结论
        /// </summary>
        public string CONST_CONC_CODE { get; set; }
        /// <summary>
        /// 起动试验结论    
        /// </summary>
        public string STARTING_CONC_CODE { get; set; }
        /// <summary>
        /// 潜动试验结论
        /// </summary>
        public string CREEPING_CONC_CODE { get; set; }
        /// <summary>
        ///   日计时误差结论
        /// </summary>
        public string DAYERR_CONC_CODE { get; set; }
        /// <summary>
        /// 功率消耗结论
        /// </summary>
        public string POWER_CONC_CODE { get; set; }
        /// <summary>
        /// 交流电压试验结论
        /// </summary>
        public string VOLT_CONC_CODE { get; set; }
        /// <summary>
        /// 规约一致性检查结论
        /// </summary>
        public string STANDARD_CONC_CODE { get; set; }
        /// <summary>
        /// 载波通信性能试验结论
        /// </summary>
        public string WAVE_CONC_CODE { get; set; }
        /// <summary>
        /// 误差变差试验结论
        /// </summary>
        public string ERROR_CONC_CODE { get; set; }
        /// <summary>
        /// 误差一致性试验结论
        /// </summary>
        public string CONSIST_CONC_CODE { get; set; }
        /// <summary>
        /// 负载电流升降变差试验结论
        /// </summary>
        public string VARIATION_CONC_CODE { get; set; }
        /// <summary>
        /// 电流过载试验结论
        /// </summary>
        public string OVERLOAD_CONC_CODE { get; set; }
        /// <summary>
        /// 时段投切误差结论
        /// </summary>
        public string TS_CONC_CODE { get; set; }
        /// <summary>
        /// 走字试验结论
        /// </summary>
        public string RUNING_CONC_CODE { get; set; }
        /// <summary>
        /// 需量周期误差结论
        /// </summary>
        public string PERIOD_CONC_CODE { get; set; }
        /// <summary>
        /// 需量示值误差结论
        /// </summary>
        public string VALUE_CONC_CODE { get; set; }
        /// <summary>
        /// 密钥下装结论
        /// </summary>
        public string KEY_CONC_CODE { get; set; }
        /// <summary>
        ///费控安全认证试验结论
        /// </summary>
        public string ESAM_CONC_CODE { get; set; }
        /// <summary>
        /// 费控远程数据回抄试验结论
        /// </summary>
        public string REMOTE_CONC_CODE { get; set; }
        /// <summary>
        /// 费控保电试验结论
        /// </summary>
        public string EH_CONC_CODE { get; set; }
        /// <summary>
        /// 费控取消保电结论
        /// </summary>
        public string EC_CONC_CODE { get; set; }
        /// <summary>
        /// 费控合闸试验结论
        /// </summary>
        public string SWITCH_ON_CONC_CODE { get; set; }
        /// <summary>
        /// 费控拉闸结论
        /// </summary>
        public string SWITCH_OUT_CONC_CODE { get; set; }
        /// <summary>
        /// 费控告警试验结论
        /// </summary>
        public string WARN_CONC_CODE { get; set; }
        /// <summary>
        /// 费控取消告警结论
        /// </summary>
        public string WARN_CANCEL_CONC_CODE { get; set; }
        /// <summary>
        ///  剩余电量递减试验结论
        /// </summary>
        public string SURPLUS_CONC_CODE { get; set; }
        /// <summary>
        /// 电能表清零结论
        /// </summary>
        public string RESET_EQ_MET_CONC_CODE { get; set; }
        /// <summary>
        /// 红外通信测试结论
        /// </summary>
        public string INFRARED_TEST_CONC_CODE { get; set; }
        /// <summary>
        ///  最大需量清零
        /// </summary>
        public string RESET_DEMAND_MET_CONC_CODE { get; set; }
        /// <summary>
        /// 广播校时试验
        /// </summary>
        public string TIMING_MET_CONC_CODE { get; set; }
        /// <summary>
        /// 通讯测试
        /// </summary>
        public string COMMINICATE_MET_CONC_CODE { get; set; }
        /// <summary>
        /// 探测表地址
        /// </summary>
        public string ADDRESS_MET_CONC_CODE { get; set; }
        /// <summary>
        /// 多功能口测试
        /// </summary>
        public string MULTI_INTERFACE_MET_CONC_CODE { get; set; }
        /// <summary>
        /// 闰年切换
        /// </summary>
        public string LEAP_YEAR_MET_CONC { get; set; }
        /// <summary>
        /// 任意数据读取
        /// </summary>
        public string PARA_READ_MET_CONC_CODE { get; set; }
        /// <summary>
        /// 任意数据写入
        /// </summary>
        public string PARA_SET_MET_CONC { get; set; }
        /// <summary>
        /// 参数设置结论
        /// </summary>
        public string SETTING_CONC_CODE { get; set; }
        /// <summary>
        /// 标准偏差
        /// </summary>
        public string DEVIATION_MET_CONC { get; set; }
        /// <summary>
        /// GPS校时
        /// </summary>
        public string GPS_CONC { get; set; }
        /// <summary>
        /// 检定人员
        /// </summary>
        public string DETECT_PERSON { get; set; }
        /// <summary>
        /// 审核人员
        /// </summary>
        public string AUDIT_PERSON { get; set; }
        ///// <summary>
        ///// 检定线写入时间
        ///// </summary>
        //public string WRITE_DATE { get; set; }
        /// <summary>
        /// 施封线处理标记
        /// </summary>
        public string SEAL_HANDLE_FLAG { get; set; }
        /// <summary>
        /// 施封线处理时间
        /// </summary>
        public string SEAL_HANDLE_DATE { get; set; }
        ///// <summary>
        /////  平台处理标记
        ///// </summary>
        //public string HANDLE_FLAG { get; set; }
        ///// <summary>
        /////  平台处理时间
        ///// </summary>
        //public string HANDLE_DATE { get; set; }
        /// <summary>
        ///  温度
        /// </summary>
        public string TEMP { get; set; }
        /// <summary>
        /// 湿度
        /// </summary>
        public string HUMIDITY { get; set; }
        /// <summary>
        /// 预置参数检查1
        /// </summary>
        public string PRESET_PARAMET_CHECK_CONC_CODE { get; set; }
        /// <summary>
        /// 485密码修改
        /// </summary>
        public string PASSWORD_CHANGE_CONC_CODE { get; set; }
        /// <summary>
        /// 预置参数设置1
        /// </summary>
        public string PRESET_PARAMETER_SET_CONC_CODE { get; set; }
        /// <summary>
        /// 可靠性验证试验
        /// </summary>
        public string RELIABILITY_CONC_CODE { get; set; }
        /// <summary>
        /// 动稳定试验
        /// </summary>
        public string MOVE_STABILITY_TEST_CONC_CODE { get; set; }
        /// <summary>
        /// 抗震性能试验
        /// </summary>
        public string ANTI_SEISMICTEST_CONC_CODE { get; set; }
        /// <summary>
        /// 预置参数检查2
        /// </summary>
        public string PRESET_PARAM2_CHECK_CONC_CODE { get; set; }
        /// <summary>
        /// 预置参数检查3
        /// </summary>
        public string PRESET_PARAM3_CHECK_CONC_CODE { get; set; }
        /// <summary>
        /// 预置参数设置2
        /// </summary>
        public string PRESET_PARAM2_SET_CONC_CODE { get; set; }
        /// <summary>
        /// 预置参数设置3
        /// </summary>
        public string PRESET_PARAM3_SET_CONC_CODE { get; set; }
        /// <summary>
        /// 硬件版本
        /// </summary>
        public string HARD_VERSION { get; set; }
        /// <summary>
        /// 故障类型
        /// </summary>
        public string FAULT_TYPE { get; set; }
        /// <summary>
        /// 事件记录功能
        /// </summary>
        public string EVENT_REC_FUNC_CONC_CODE { get; set; }
        /// <summary>
        /// 影响量试验
        /// </summary>
        public string INFLUENCE_QTY_CONC_CODE { get; set; }
        /// <summary>
        /// 辅助电源
        /// </summary>
        public string AUX_POW_CONC_CODE { get; set; }
        /// <summary>
        /// 报警功能
        /// </summary>
        public string ALARM_FUNC_CONC_CODE { get; set; }
        /// <summary>
        /// 费率和时段功能
        /// </summary>
        public string RATE_TIME_FUNC_CONC_CODE { get; set; }
        /// <summary>
        /// 电能计量功能
        /// </summary>
        public string ELE_ENERGY_FUNC_CONC_CODE { get; set; }
        /// <summary>
        /// 合格证有效截止日期
        /// </summary>
        public string EXPIRATION_DATE { get; set; }
        /// <summary>
        /// 测量重复性试验(新规)
        /// </summary>
        public string MEA_REP_CONC_CODE { get; set; }
        /// <summary>
        /// 计量装置证书编号
        /// </summary>
        public string CERT_NO { get; set; }
        /// <summary>
        /// 计量装置证书编号有效期
        /// </summary>
        public string CERT_NO_PERIOD { get; set; }
        ///// <summary>
        ///// 不合格原因
        ///// </summary>
        //public string UNPASS_REASON { get; set; }
        /// <summary>
        ///  通讯模块互换能力试验(新规)
        /// </summary>
        public string COMM_MODE_CHG_CONC_CODE { get; set; }
        /// <summary>
        ///  通讯模块接口带载能力测试（新规）
        /// </summary>
        public string COMM_MODE_INT_CONC_CODE { get; set; }
        /// <summary>
        ///  冻结功能
        /// </summary>
        public string FROZEN_FUNC_CONC_CODE { get; set; }
    }
}
