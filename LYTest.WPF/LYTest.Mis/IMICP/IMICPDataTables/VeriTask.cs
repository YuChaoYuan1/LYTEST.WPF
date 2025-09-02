using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LYTest.Mis.IMICP.IMICPDataTables
{
    /// <summary>
    /// 6.1 检定台任务信息获取接口
    /// </summary>
    public class VeriTask:CommonDatas
    {
        /// <summary>
        /// 检定方案标识
        /// </summary>
        public string trialSchId { get; set; }  //	
        /// <summary>
        /// 试验方案名称
        /// </summary>
        public string veriSch { get; set; } //	
        /// <summary>
        /// 设备分类
        /// </summary>
        public string devCls { get; set; }  //	
        /// <summary>
        /// 任务状态
        /// </summary>
        public string taskStatus { get; set; }  //	
        /// <summary>
        /// 复检方案标识
        /// </summary>
        public string rckSchId { get; set; }    //	
        /// <summary>
        /// 检定任务号
        /// </summary>
        public string taskNo { get; set; }  //	
        /// <summary>
        /// 任务下发时间
        /// </summary>
        public string taskIssuTime { get; set; }    //	
        /// <summary>
        /// 是否自动施封
        /// </summary>
        public string autoSealFlag { get; set; }    //	
        /// <summary>
        /// 任务类型
        /// </summary>
        public string taskCateg { get; set; }   //	
        /// <summary>
        /// 任务优先级
        /// </summary>
        public string taskPri { get; set; } //	
        /// <summary>
        /// 检定方式
        /// </summary>
        public string testMode { get; set; }    //	
        /// <summary>
        /// 
        /// </summary>
        public string erpBatchNo { get; set; }  //	ERP物料代码
        /// <summary>
        /// 
        /// </summary>
        public string devNum { get; set; }  //	设备数量
        /// <summary>
        /// 
        /// </summary>
        public string tPileNum { get; set; }    //	总垛数
        /// <summary>
        /// 
        /// </summary>
        public string devModel { get; set; }    //	型号
        /// <summary>
        /// 
        /// </summary>
        public string equipCodeNew { get; set; }    //	新设备码
        /// <summary>
        /// 
        /// </summary>
        public string veriDevStat { get; set; } //	设备状态码
        /// <summary>
        /// 
        /// </summary>
        public string arrBatchNo { get; set; }	//	到货批次号
    }
}
