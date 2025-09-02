using LYTest.Core.Enum;
using System;

namespace LYTest.Core.Model.DnbModel.DnbInfo
{
    #region =========== 试验结果数据类 ===========
    /// <summary>
    /// 试验结果数据
    /// </summary>
    [Serializable]
    public class ClTrialData_Local
    {
        private string schemeId = string.Empty;
        public bool IsCheck = true;//2011-8-13
        private bool _Is485 = false;//485是否通讯上 2011-8-16
        /// <summary>
        /// 试验方案编号
        /// </summary>
        public string SchemeId
        {
            get
            {
                return schemeId;
            }
            set
            {
                schemeId = value;
            }
        }

        private string sourceParams = "";
        /// <summary>
        /// 控源参数
        /// </summary>
        public string SourceParams
        {
            get
            {
                return sourceParams;
            }
            set
            {
                sourceParams = value;
            }
        }

        private string itemParams = "";
        /// <summary>
        /// 项目参数
        /// </summary>
        public string ItemParams
        {
            get
            {
                return itemParams;
            }
            set
            {
                itemParams = value;
            }
        }
        private decimal itemId;
        /// <summary>
        /// 检定方案ID
        /// </summary>
        public decimal ItemId
        {
            get { return itemId; }
            set { itemId = value; }
        }
        /// <summary>
        ///参数验证子项ID
        /// </summary>
        public string ParamID { get; set; }

        private string guidId = "";
        /// <summary>
        /// 表全局唯一ID
        /// </summary>
        public string GuidId
        {
            get
            {
                return guidId;
            }
            set
            {
                guidId = value;
            }
        }

        private string meterNo = "";
        /// <summary>
        /// 表条码号
        /// </summary>
        public string MeterNo
        {
            get
            {
                return meterNo;
            }
            set
            {
                meterNo = value;
            }
        }

        private byte[] _picture = null;
        /// <summary>
        /// 表照片。
        /// </summary>
        public byte[] Picture
        {
            get { return _picture; }
            set { _picture = value; }
        }
        private int meterId = 0;
        /// <summary>
        /// 表位号
        /// </summary>
        public int MeterId
        {
            get
            {
                return meterId;
            }
            set
            {
                meterId = value;
            }
        }

        private int stationId = 0;
        /// <summary>
        /// 工位编号
        /// </summary>
        public int StationId
        {
            get
            {
                return stationId;
            }
            set
            {
                stationId = value;
            }
        }

        private string trialValue = "";
        /// <summary>
        /// 试验数据
        /// </summary>
        public string TrialValue
        {
            get
            {
                return trialValue;
            }
            set
            {
                trialValue = value;
            }
        }

        private string roundValue = "";
        /// <summary>
        /// 试验数据化整值
        /// </summary>
        public string RoundValue
        {
            get
            {
                return roundValue;
            }
            set
            {
                roundValue = value;
            }
        }

        private string pristineValues = "";
        /// <summary>
        /// 试验原始数据列表
        /// </summary>
        public string PristineValues
        {
            get
            {
                return pristineValues;
            }
            set
            {
                pristineValues = value;
            }
        }
        /// <summary>
        /// 485是否通讯上
        /// </summary>
        public bool Is485
        {
            get { return this._Is485; }
            set { this._Is485 = value; }
        }

        private EmTrialConclusion trialResult = EmTrialConclusion.CTG_Wait;
        /// <summary>
        /// 试验结论
        /// </summary>
        public EmTrialConclusion TrialResult
        {
            get
            {
                return trialResult;
            }
            set
            {
                trialResult = value;
            }
        }

        private EmTrialType trialType = EmTrialType.Null;
        /// <summary>
        /// 试验项目类型
        /// </summary>
        public EmTrialType TrialType
        {
            get
            {
                return trialType;
            }
            set
            {
                trialType = value;
            }
        }

        private string extentData = "";
        /// <summary>
        /// 扩展试验结果数据
        /// </summary>
        public string ExtentData
        {
            get
            {
                return extentData;
            }
            set
            {
                extentData = value;
            }
        }

        private string trialStartDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        /// <summary>
        /// 试验起始时间
        /// </summary>		
        public string TrialStartDate
        {
            get { return trialStartDate; }
            set { trialStartDate = value; }
        }
        private string saveDate = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        /// <summary>
        /// 试验数据保存日期(格式:yyyy-MM-dd)
        /// </summary>
        public string SaveDate
        {
            get
            {
                return saveDate;
            }
            set
            {
                saveDate = value;
            }
        }
        /// <summary>
        /// 箱条码
        /// </summary>
        public string BoxNo { get; set; }

        /// <summary>
        /// 是否是临检数据
        /// </summary>
        public bool IsSampleData
        {
            get;
            set;
        }

        /// <summary>
        /// 复检次数
        /// </summary>
        public int ReCheckCount
        {
            get;
            set;
        }

        /// <summary>
        /// 试验项目名称
        /// </summary>
        public string ItemName
        {
            get;
            set;
        }

        /// <summary>
        /// 表任务号
        /// </summary>
        public string Taskid
        {
            get;
            set;
        }
        /// <summary>
        /// 子项目编号(终端)
        /// </summary>
        public decimal SubItemNo
        {
            get;
            set;
        }
        /// <summary>
        /// 子项目名称(终端)
        /// </summary>
        public string SubItemName
        {
            get;
            set;
        }
    }
    #endregion
}
