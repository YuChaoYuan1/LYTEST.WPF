using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LYTest.Mis.IMICP.IMICPDataTables
{
    /// <summary>
    /// 检定综合结论 - 检定明细单
    /// </summary>
    public class VeriDtlFormList
    {
        /// <summary>
        ///设备分类
        /// </summary>
        public string devCls { get; set; }
        /// <summary>
        ///检定任务编号
        /// </summary>
        public string veriTaskNo { get; set; }
        /// <summary>
        ///设备单元编号
        /// </summary>
        public string plantElementNo { get; set; }
        /// <summary>
        ///专机编号
        /// </summary>
        public string machNo { get; set; }
        /// <summary>
        ///表位编号
        /// </summary>
        public string devSeatNo { get; set; }

        /// <summary>
        ///资产编号
        /// </summary>
        public string assetNo { get; set; }
        /// <summary>
        ///条形码
        /// </summary>
        public string barCode { get; set; }
        /// <summary>
        ///检定结果
        /// </summary>
        public string veriRslt { get; set; }
        /// <summary>
        ///检定人员
        /// </summary>
        public string veriStf { get; set; }
        /// <summary>
        ///检定部门
        /// </summary>
        public string veriDept { get; set; }
        /// <summary>
        ///检定日期
        /// </summary>
        public string veriDate { get; set; }
        /// <summary>
        ///故障原因
        /// </summary>
        public string faultReason { get; set; }
        /// <summary>
        ///核验人员
        /// </summary>
        public string checkStf { get; set; }
        /// <summary>
        ///试验人员
        /// </summary>
        public string trialStf { get; set; }
        /// <summary>
        ///设备档案编号，检定线台标识
        /// </summary>
        public string plantNo { get; set; }
        /// <summary>
        ///台体编号
        /// </summary>
        public string platformNo { get; set; }
        /// <summary>
        ///温度
        /// </summary>
        public string temp { get; set; }
        /// <summary>
        ///湿度
        /// </summary>
        public string humid { get; set; }
        /// <summary>
        ///核验日期
        /// </summary>
        public string checkDate { get; set; }
        /// <summary>
        ///一级故障原因
        /// </summary>
        public string frstLvFaultReason { get; set; }
        /// <summary>
        ///二级故障原因
        /// </summary>
        public string scndLvFaultReason { get; set; }
    }
}
