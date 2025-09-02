using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LYTest.Mis.IMICP.IMICPDataTables
{
    /// <summary>
    /// 通用属性
    /// </summary>
    public class CommonDatas
    {
        /// <summary>
        /// 条形码
        /// </summary>
        /// 
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string barCode { get; set; }


        /// <summary>
        /// 系统编号
        /// </summary>
        /// 
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string sysNo { get; set; }

        /// <summary>
        /// 检定任务号
        /// </summary>
        /// 
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string taskNo { get; set; }

        /// <summary>
        ///试验方案名称
        /// </summary>
        /// 
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string veriSch { get; set; }
        /// <summary>
        ///设备分类
        /// </summary>
        /// 
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string devCls { get; set; }
        /// <summary>
        ///任务状态
        /// </summary>
        /// 
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string taskStatus { get; set; }



        /// <summary>
        ///是否自动施封
        /// </summary>
        /// 
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string autoSealFlag { get; set; }
        /// <summary>
        ///任务类型
        /// </summary>
        /// 
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string taskCateg { get; set; }
        /// <summary>
        ///任务优先级
        /// </summary>
        /// \
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string taskPri { get; set; }
        /// <summary>
        ///检定方式
        /// </summary>
        /// 
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string testMode { get; set; }
        /// <summary>
        ///ERP物料代码
        /// </summary>
        /// 
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string erpBatchNo { get; set; }


        /// <summary>
        ///型号
        /// </summary>
        /// 
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string devModel { get; set; }
        /// <summary>
        ///新设备码
        /// </summary>
        /// 
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string equipCodeNew { get; set; }
        /// <summary>
        ///检定设备状态
        /// </summary>
        /// 
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string veriDevStat { get; set; }
        /// <summary>
        ///到货批次号
        /// </summary>
        /// 
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string arrBatchNo { get; set; }

        /// <summary>
        /// 获取类型，三选一，到货批次号=01/设备码=02/条形码串=03
        /// </summary>
        /// 
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string type { get; set; }

        /// <summary>
        /// 条形码串，逗号分割
        /// </summary>
        /// 
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string barCodes { get; set; }

        /// <summary>
        /// 设备码
        /// </summary>
        /// 
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string devCodeNo { get; set; }

        /// <summary>
        /// 检定任务号
        /// </summary>
        /// 
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string veriTaskNo { get; set; }

        /// <summary>
        /// 01-检定任务完成02-分拣任务完成03-复拣任务完成04-出入库任务完成05-盘点任务完成
        /// </summary>
        /// 
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string flag { get; set; }


        /// <summary>
        /// 是否合格
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string isQualified { get; set; }
        /// <summary>
        /// 箱条码
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string boxBarCode { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string devBoxDate { get; set; }

    }

    public class RecvData
    {
        /// <summary>
        /// 成功标志
        /// </summary>
        public string resultFlag { get; set; }

        /// <summary>
        /// 错误信息
        /// </summary>
        public string errorInfo { get; set; }

        public VeriTask veriTask;
    }
    public class RecvGetEquipDETData
    {
        /// <summary>
        /// 成功标志
        /// </summary>
        public string resultFlag { get; set; }

        /// <summary>
        /// 错误信息
        /// </summary>
        public string errorInfo { get; set; }

        public InOutWhDtl inOutWhDtl;

    }
}
