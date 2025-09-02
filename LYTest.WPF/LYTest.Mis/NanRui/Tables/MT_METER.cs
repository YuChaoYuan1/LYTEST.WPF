using System.Collections.Generic;

namespace LYTest.Mis.NanRui.Tables
{
    /// <summary>
    /// 电能表资产信息
    /// </summary>
    public class MT_METER
    {
        public MT_METER()
        {
            MT_DATECT_OUT_EQUIP = new MT_DATECT_OUT_EQUIP();
            dss = new Dictionary<string, string>();
            dss.Add("SYS_NO", "");
            dss.Add("DETECT_TASK_NO", "");
            dss.Add("BOX_BAR_CODE", "");
            dss.Add("EQUIP_NUM", "1");
            dss.Add("EQUIP_NOS", "");
            dss.Add("CHECK_FLAG", "");
            dss.Add("CHECK_TIME", "");
            dss.Add("ALARM_INFO", "");
        }
        /// <summary>
        /// 电能表标识
        /// </summary>
        public string METER_ID { get; set; }
        /// <summary>
        /// 物料号
        /// </summary>
        public string ERP_BATCH_NO { get; set; }
        /// <summary>
        /// 条形码 
        /// </summary>
        public string BAR_CODE { get; set; }
        /// <summary>
        /// 资产编号
        /// </summary>
        public string ASSET_NO { get; set; }
        /// <summary>
        /// 出厂编号
        /// </summary>
        public string MADE_NO { get; set; }
        /// <summary>
        /// 缴费终端生产批次
        /// </summary>
        public string LOT_NO { get; set; }
        /// <summary>
        /// 供电单位编号
        /// </summary>
        public string ORG_NO { get; set; }
        /// <summary>
        /// 供电单位类别
        /// </summary>
        public string ORG_TYPE { get; set; }
        /// <summary>
        /// 产权单位
        /// </summary>
        public string PR_ORG { get; set; }
        /// <summary>
        /// 所在单位
        /// </summary>
        public string BELONG_DEPT { get; set; }
        /// <summary>
        /// 订货合同编号,用来与订货合同实体进行关联
        /// </summary>
        public string CONTRACT_ID { get; set; }
        /// <summary>
        /// 用户自己编的合同编号
        /// </summary>
        public string CONTRACT_NO { get; set; }
        /// <summary>
        /// 到货接收的ID
        /// </summary>
        public string RCV_ID { get; set; }
        /// <summary>
        /// 到货的批次号
        /// </summary>
        public string ARRIVE_BATCH_NO { get; set; }
        /// <summary>
        /// 地区代码
        /// </summary>
        public string AREA_CODE { get; set; }
        /// <summary>
        /// 存放区分类
        /// </summary>
        public string STORE_AREA_SORT { get; set; }
        /// <summary>
        /// 库房标识
        /// </summary>
        public string WH_ID { get; set; }
        /// <summary>
        /// 库区标识
        /// </summary>
        public string WH_AREA_ID { get; set; }
        /// <summary>
        /// 存放区标识
        /// </summary>
        public string STORE_AREA_ID { get; set; }
        /// <summary>
        /// 与储位建立关联
        /// </summary>
        public string STORE_LOC_ID { get; set; }
        /// <summary>
        /// 设备所在托盘的条码
        /// </summary>
        public string PALLET_BAR_CODE { get; set; }
        /// <summary>
        /// 箱设备所在周转箱的条码
        /// </summary>
        public string BOX_BAR_CODE { get; set; }
        /// <summary>
        /// 类别
        /// </summary>
        public string SORT_CODE { get; set; }
        /// <summary>
        /// 设备类型
        /// </summary>
        public string TYPE_CODE { get; set; }
        /// <summary>
        /// 设备型号
        /// </summary>
        public string MODEL_CODE { get; set; }
        /// <summary>
        /// 接线方式
        /// </summary>
        public string WIRING_MODE { get; set; }
        /// <summary>
        /// 电压类型
        /// </summary>
        public string VOLT_CODE { get; set; }
        /// <summary>
        /// 标定电流
        /// </summary>
        public string RATED_CURRENT { get; set; }
        /// <summary>
        /// 过载倍数
        /// </summary>
        public string OVERLOAD_FACTOR { get; set; }

        /// <summary>
        /// 有功准确度等级
        /// </summary>
        public string AP_PRE_LEVEL_CODE { get; set; }

        /// <summary>
        /// 无功准确度等级
        /// </summary>
        public string RP_PRE_LEVER_CODE { get; set; }

        /// <summary>
        /// 示数位数，采用n.d形式。N为整数部分位数，d为小数部分位数
        /// </summary>
        public string METER_DIGITS { get; set; }
        /// <summary>
        /// 示数位数，采用n.d形式。N为整数部分位数，d为小数部分位数
        /// </summary>
        public string TS_DIGITS { get; set; }

        /// <summary>
        /// 常数
        /// </summary>
        public string CONST_CODE { get; set; }

        /// <summary>
        /// 无功常数
        /// </summary>
        public string RP_CONSTANT { get; set; }

        /// <summary>
        /// 制造单位
        /// </summary>
        public string MANUFACTURER { get; set; }

        /// <summary>
        /// 出厂日期
        /// </summary>
        public string MADE_DATE { get; set; }

        /// <summary>
        /// 设备单价
        /// </summary>
        public string EQIP_PRC { get; set; }

        /// <summary>
        /// 自身倍率
        /// </summary>
        public string SELF_FACTOR { get; set; }

        /// <summary>
        /// 是否双向计量
        /// </summary>
        public string BOTH_WAY_CALC { get; set; }

        /// <summary>
        /// 是否预付费
        /// </summary>
        public string PREPAY_FLAG { get; set; }

        /// <summary>
        /// 复费率表标志
        /// </summary>
        public string MULTIRATE_FALG { get; set; }

        /// <summary>
        /// 需量表标志
        /// </summary>
        public string DEMAND_METER_FALG { get; set; }

        /// <summary>
        /// 谐波计量标志
        /// </summary>
        public string HARMONIC_MEAS_FALG { get; set; }

        /// <summary>
        /// 是否阻逆
        /// </summary>
        public string CC_PREVENT_FLAG { get; set; }

        /// <summary>
        /// 脉冲常数
        /// </summary>
        public string PULSE_CONSTANT_CODE { get; set; }

        /// <summary>
        /// 脉冲幅值
        /// </summary>
        public string PULSE_AMPLITUDE_CODE { get; set; }

        /// <summary>
        /// 脉冲类别
        /// </summary>
        public string PULSE_SORT_CODE { get; set; }

        /// <summary>
        /// 频率
        /// </summary>
        public string FREQ_CODE { get; set; }

        /// <summary>
        /// 接入方式
        /// </summary>
        public string CON_MODE { get; set; }

        /// <summary>
        /// 指示数类型
        /// </summary>
        public string READING_TYPE_CODE { get; set; }

        /// <summary>
        /// 使用用途
        /// </summary>
        public string METER_USAGE { get; set; }

        /// <summary>
        /// 测量原理
        /// </summary>
        public string MEAS_THEORY { get; set; }

        /// <summary>
        /// 轴承结构
        /// </summary>
        public string BEARING_STRUS { get; set; }

        /// <summary>
        /// 通讯接口
        /// </summary>
        public string CI { get; set; }

        /// <summary>
        /// 继电器接点
        /// </summary>
        public string RELAY_JOINT { get; set; }

        /// <summary>
        /// 跳合闸产生方式
        /// </summary>
        public string ELEC_MEAS_DISP_FALG { get; set; }

        /// <summary>
        /// 电测量显示
        /// </summary>
        public string VL_FLAG { get; set; }

        /// <summary>
        /// 失压判断
        /// </summary>
        public string CL_FLAG { get; set; }

        /// <summary>
        /// 失流判断
        /// </summary>
        public string ANTI_PHASE_FLAG { get; set; }

        /// <summary>
        /// 逆相序判断
        /// </summary>
        public string SUPER_POWER_FLAG { get; set; }
        /// <summary>
        /// 超功率
        /// </summary>
        public string LOAD_CURVE_FLAG { get; set; }
        /// <summary>
        /// 负荷曲线
        /// </summary>
        public string POWEROFF_MR_FLAG { get; set; }
        /// <summary>
        /// 停电抄表
        /// </summary>
        public string INFRARED_FLAG { get; set; }

        /// <summary>
        /// 红外抄表
        /// </summary>
        public string DOC_TYPE_CODE { get; set; }
        /// <summary>
        /// 建档类型
        /// </summary>
        public string LATEST_CHK_DATE { get; set; }
        /// <summary>
        /// 最近检定日期
        /// </summary>
        public string INST_DATE { get; set; }
        /// <summary>
        /// 安装日期
        /// </summary>
        public string BMV_DATE { get; set; }
        /// <summary>
        /// 拆除日期
        /// </summary>
        public string ROTATE_CYCLE { get; set; }
        /// <summary>
        /// 轮换周期
        /// </summary>
        public string DISCARD_REASON { get; set; }
        /// <summary>
        /// 报废原因
        /// </summary>
        public string DESCARD_DATE { get; set; }
        /// <summary>
        /// 报废日期
        /// </summary>
        public string PR_CODE { get; set; }
        /// <summary>
        /// 产权
        /// </summary>
        public string HANDOVER_DEPT { get; set; }
        /// <summary>
        /// 移交单位
        /// </summary>
        public string HANDOVER_DATE { get; set; }
        /// <summary>
        /// 移交日期
        /// </summary>
        public string CUR_STATUS_CODE { get; set; }
        /// <summary>
        /// 当前状态
        /// </summary>
        public string BORROW_FLAG { get; set; }
        /// <summary>
        /// 是否借用
        /// </summary>
        public string NEW_FLAG { get; set; }
        /// <summary>
        /// 新旧标志
        /// </summary>
        public string DOC_CREATOR_NO { get; set; }
        /// <summary>
        /// 建档人
        /// </summary>
        public string DOC_CREATOR_DATE { get; set; }
        /// <summary>
        /// 建档日期
        /// </summary>
        public string BAUDRATE_CODE { get; set; }
        /// <summary>
        /// 波特率
        /// </summary>
        public string METER_CLOSE_MODE { get; set; }
        /// <summary>
        /// 卡表跳闸方式
        /// </summary>
        public string REGISTER_MODE { get; set; }
        /// <summary>
        /// 计度器方式
        /// </summary>
        public string DISP_MODE { get; set; }
        /// <summary>
        /// 显示方式
        /// </summary>
        public string HARD_VER { get; set; }
        /// <summary>
        /// 硬件版本
        /// </summary>
        public string SOFT_VER { get; set; }
        /// <summary>
        /// 软件版本
        /// </summary>
        public string RS485_ROUTE_QTY { get; set; }
        /// <summary>
        /// RS485路数
        /// </summary>
        public string COMM_PROT_CODE { get; set; }
        /// <summary>
        /// 通讯规约
        /// </summary>
        public string COMM_MODE { get; set; }
        /// <summary>
        /// 通讯方式
        /// </summary>
        public string ATTACHEQUIP_TYPE_CODE { get; set; }
        /// <summary>
        /// 附属设备分类
        /// </summary>
        public string CARRIER_WAVE_ID { get; set; }
        /// <summary>
        /// 载波模块属性标识
        /// </summary>
        public string REMARK { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public string WRITE_DATE { get; set; }
        /// <summary>
        /// 平台写入时间
        /// </summary>
        public string HANDLE_FLAG { get; set; }
        /// <summary>
        /// 处理标记
        /// </summary>
        public string HANDLE_DATE { get; set; }
        /// <summary>
        /// 处理时间
        /// </summary>
        public string LATEST_DETECT_DATE { get; set; }
        public string CHIP_MANUFACTURER { get; set; }
        public string CHIP_MODEL_CODE { get; set; }
        /// <summary>
        /// 表相关的明细信息，包含任务单号和系统编号
        /// </summary>
        public MT_DATECT_OUT_EQUIP MT_DATECT_OUT_EQUIP { get; set; }
        /// <summary>
        /// 要写入webservices的数据
        /// </summary>
        public Dictionary<string,string> dss { get; set; }

      
    }
}
