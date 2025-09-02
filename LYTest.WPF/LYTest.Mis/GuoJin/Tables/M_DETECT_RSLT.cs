using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LYTest.Mis.GuoJin.Tables
{
    public class M_DETECT_RSLT : M_QT_CONC_Basic
    {
        /// <summary>
        /// 本表唯一主键
        /// </summary>
        public string READ_ID { get; set; }
        /// <summary>
        /// 设备ID
        /// </summary>
        public string EQUIP_ID { get; set; }

        /// <summary>
        /// 设备名称
        /// </summary>
        public string EQUIP_NAME { get; set; }

        /// <summary>
        /// 设备大类
        /// </summary>
        public string EQUIP_CATEG { get; set; }

        /// <summary>
        /// 设备小类
        /// </summary>
        public string EQUIP_TYPE { get; set; }

        /// <summary>
        /// 试验项名称
        /// </summary>
        public string ITEM_NAME { get; set; }

        /// <summary>
        /// 试验分组
        /// </summary>
        public string TEST_GROUP { get; set; }

        /// <summary>
        /// 系统编号
        /// </summary>
        public string SYS_NO { get; set; }

        /// <summary>
        /// 检定单元编号
        /// </summary>
        public string DETECT_UNIT_NO { get; set; }

        /// <summary>
        /// 表位编号
        /// </summary>
        public string POSITION_NO { get; set; }

        /// <summary>
        /// 数据来源
        /// </summary>
        public string DATA_SOURCE { get; set; }

        /// <summary>
        /// 数据类型
        /// </summary>
        public string DATA_TYPE { get; set; }

        /// <summary>
        /// 接入方式
        /// </summary>
        public string CON_MODE { get; set; }

        /// <summary>
        /// 有功等级
        /// </summary>
        public string AP_LEVEL { get; set; }

        /// <summary>
        /// 无功等级
        /// </summary>
        public string RP_LEVEL { get; set; }

        /// <summary>
        /// 相别
        /// </summary>
        public string IABC { get; set; }

        /// <summary>
        /// 功率因数
        /// </summary>
        public string PF { get; set; }


        /// <summary>
        /// 负载电流
        /// </summary>
        public string LOAD_CURRENT { get; set; }

        /// <summary>
        /// 功率方向
        /// </summary>
        public string BOTH_WAY_POWER_FLAG { get; set; }

        /// <summary>
        /// 检定员ID
        /// </summary>
        public string TEST_USER_ID { get; set; }

        /// <summary>
        /// 误差化整值
        /// </summary>
        public string INT_CONVERT_ERR { get; set; }

        /// <summary>
        /// 误差限值
        /// </summary>
        public string ERR_ABS { get; set; }

        /// <summary>
        /// 试验结果类型
        /// </summary>
        public string RESULT_TYPE { get; set; }

        /// <summary>
        /// 单位
        /// </summary>
        public string UNIT_MARK { get; set; }

        /// <summary>
        /// 分项结论
        /// </summary>
        public string DETECT_RESULT { get; set; }

        /// <summary>
        /// 指标项1
        /// </summary>
        public string DATA_ITEM1 { get; set; }

        /// <summary>
        /// 指标项2
        /// </summary>
        public string DATA_ITEM2 { get; set; }

        /// <summary>
        /// 指标项3
        /// </summary>
        public string DATA_ITEM3 { get; set; }

        /// <summary>
        /// 指标项4
        /// </summary>
        public string DATA_ITEM4 { get; set; }

        /// <summary>
        /// 指标项5
        /// </summary>
        public string DATA_ITEM5 { get; set; }

        /// <summary>
        /// 指标项6
        /// </summary>
        public string DATA_ITEM6 { get; set; }

        /// <summary>
        /// 指标项7
        /// </summary>
        public string DATA_ITEM7 { get; set; }

        /// <summary>
        /// 指标项8
        /// </summary>
        public string DATA_ITEM8 { get; set; }

        /// <summary>
        /// 指标项9
        /// </summary>
        public string DATA_ITEM9 { get; set; }

        /// <summary>
        /// 指标项10
        /// </summary>
        public string DATA_ITEM10 { get; set; }

        /// <summary>
        /// 指标项11
        /// </summary>
        public string DATA_ITEM11 { get; set; }

        /// <summary>
        /// 指标项12
        /// </summary>
        public string DATA_ITEM12 { get; set; }

        /// <summary>
        /// 指标项13
        /// </summary>
        public string DATA_ITEM13 { get; set; }

        /// <summary>
        /// 指标项14
        /// </summary>
        public string DATA_ITEM14 { get; set; }

        /// <summary>
        /// 指标项15
        /// </summary>
        public string DATA_ITEM15 { get; set; }

        /// <summary>
        /// 指标项16
        /// </summary>
        public string DATA_ITEM16 { get; set; }

        /// <summary>
        /// 指标项17
        /// </summary>
        public string DATA_ITEM17 { get; set; }

        /// <summary>
        /// 指标项18
        /// </summary>
        public string DATA_ITEM18 { get; set; }

        /// <summary>
        /// 指标项19
        /// </summary>
        public string DATA_ITEM19 { get; set; }

        /// <summary>
        /// 指标项20
        /// </summary>
        public string DATA_ITEM20 { get; set; }

        /// <summary>
        /// 检定总结论
        /// </summary>
        public string CHK_CONC_CODE { get; set; }
    }
}
