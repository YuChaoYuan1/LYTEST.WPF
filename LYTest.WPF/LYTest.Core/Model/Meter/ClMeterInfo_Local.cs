using LYTest.Core.Enum;
using System;

namespace LYTest.Core.Model.Meter
{
    #region =========== 电能表基本信息 ===========
    /// <summary>
    /// 电能表基本信息
    /// </summary>
    [Serializable]
    public class ClMeterInfo_Local

    {
        public bool _isCheck;
        public bool IsCheck
        {
            get { return _isCheck; }
            set { _isCheck = value; }
        }
        private int _MeterId = 0;
        /// <summary>
        /// 表位号
        /// </summary>
        public int MeterId
        {
            get
            {
                return this._MeterId;
            }
            set
            {
                this._MeterId = value;
            }
        }

        private string _GuidId = string.Empty;
        /// <summary>
        /// 表全局唯一ID
        /// </summary>
        public string GuidId
        {
            get
            {
                return this._GuidId;
            }
            set
            {
                this._GuidId = value;
            }
        }

        private string _AssetNum = string.Empty;
        /// <summary>
        /// 资产编号
        /// </summary>
        public string AssetNum
        {
            get
            {
                return this._AssetNum;
            }
            set
            {
                this._AssetNum = value;
            }
        }

        private string _BarCode = string.Empty;
        /// <summary>
        /// 条形码
        /// </summary>
        public string BarCode
        {
            get
            {
                return this._BarCode;
            }
            set
            {
                this._BarCode = value;
            }
        }

        private string _ManuCode = string.Empty;
        /// <summary>
        /// 出厂编号
        /// </summary>
        public string ManuCode
        {
            get
            {
                return this._ManuCode;
            }
            set
            {
                this._ManuCode = value;
            }
        }

        private string _MeterAddress = string.Empty;
        /// <summary>
        /// 表地址
        /// </summary>
        public string MeterAddress
        {
            get
            {
                return this._MeterAddress;
            }
            set
            {
                this._MeterAddress = value;
            }
        }

        private string _BatchId = string.Empty;
        /// <summary>
        /// 批次ID
        /// </summary>
        public string BatchId
        {
            get
            {
                return this._BatchId;
            }
            set
            {
                this._BatchId = value;
            }
        }

        private string _MeterConst = string.Empty;
        /// <summary>
        /// 表有功常数
        /// </summary>
        public string MeterConst
        {
            get
            {
                return this._MeterConst;
            }
            set
            {
                this._MeterConst = value;
            }
        }

        private string _MeterClass = string.Empty;
        /// <summary>
        /// 表有功等级 
        /// </summary>
        public string MeterClass
        {
            get
            {
                return this._MeterClass;
            }
            set
            {
                this._MeterClass = value;
            }
        }

        private string _MeterQConst = string.Empty;
        /// <summary>
        /// 表无功常数
        /// </summary>
        public string MeterQConst
        {
            get
            {
                return this._MeterQConst;
            }
            set
            {
                this._MeterQConst = value;
            }
        }

        private string _MeterQClass = string.Empty;
        /// <summary>
        /// 表无功等级 
        /// </summary>
        public string MeterQClass
        {
            get
            {
                return this._MeterQClass;
            }
            set
            {
                this._MeterQClass = value;
            }
        }

        private EmMeterType _MeterType = EmMeterType.Electronics_21;
        /// <summary>
        /// 表类型(0:电子式 1:智能本地费控 2:智能远程费控)
        /// </summary>
        public EmMeterType MeterType
        {
            get
            {
                return this._MeterType;
            }
            set
            {
                this._MeterType = value;
            }
        }

        private string _MeterType_Code = string.Empty;
        /// <summary>
        /// 码值字符串表示的表类型(如: 30800000.21)
        /// </summary>
        public string MeterType_Code
        {
            get
            {
                return this._MeterType_Code;
            }
            set
            {
                this._MeterType_Code = value;
            }
        }

        private string _MeterModel = string.Empty;
        /// <summary>
        /// 表型号
        /// </summary>
        public string MeterModel
        {
            get
            {
                return this._MeterModel;
            }
            set
            {
                this._MeterModel = value;
            }
        }

        private string _MeterSort = string.Empty;
        /// <summary>
        /// 1-有功2-无功3-多功能表(30700000码表)
        /// </summary>
        public string MeterSort
        {
            get
            {
                return this._MeterSort;
            }
            set
            {
                this._MeterSort = value;
            }
        }

        private string _CommParam = string.Empty;
        /// <summary>
        /// 通信参数
        /// </summary>
        public string CommParam
        {
            get
            {
                return this._CommParam;
            }
            set
            {
                this._CommParam = value;
            }
        }

        private int _HwType = 0;
        /// <summary>
        /// 0-非红外,1-远红外,2-近红外
        /// </summary>
        public int HwType
        {
            get
            {
                return this._HwType;
            }
            set
            {
                this._HwType = value;
            }
        }

        private string _HwParam = string.Empty;
        /// <summary>
        /// 红外参数（如 1200,E,8,1）
        /// </summary>
        public string HwParam
        {
            get
            {
                return this._HwParam;
            }
            set
            {
                this._HwParam = value;
            }
        }

        private EmCarrierType _CarrierType = EmCarrierType.Null;
        /// <summary>
        /// 载波类型(0:非载波  1: 晓程 2: 东软 3: 鼎新 4: 瑞斯康)
        /// </summary>
        public EmCarrierType CarrierType
        {
            get
            {
                return this._CarrierType;
            }
            set
            {
                this._CarrierType = value;
            }
        }

        private string _CarrierParam = string.Empty;
        /// <summary>
        /// 载波参数
        /// </summary>
        public string CarrierParam
        {
            get
            {
                return this._CarrierParam;
            }
            set
            {
                this._CarrierParam = value;
            }
        }

        private string _Voltage = string.Empty;
        /// <summary>
        /// 额定电压(单相220,三相3*220/380)
        /// </summary> 
        public string Voltage
        {
            get
            {
                return this._Voltage;
            }
            set
            {
                this._Voltage = value;
            }
        }

        private string _Current = string.Empty;
        /// <summary>
        /// 基本电流(如:10(60))
        /// </summary>
        public string Current
        {
            get
            {
                return this._Current;
            }
            set
            {
                this._Current = value;
            }
        }

        private string _Frequency = string.Empty;
        /// <summary>
        /// 额定频率(如: 50)
        /// </summary>
        public string Frequency
        {
            get
            {
                return this._Frequency;
            }
            set
            {
                this._Frequency = value;
            }
        }

        private EmWireMode _LinkType = EmWireMode.WM_APhase;
        /// <summary>
        /// 接线方式(单相,三相三线)
        /// </summary>
        public EmWireMode LinkType
        {
            get
            {
                return this._LinkType;
            }
            set
            {
                this._LinkType = value;
            }
        }

        private EmCommProtCode _EmCommProtCode = EmCommProtCode.Procotol645;
        /// <summary>
        /// 规约类型
        /// </summary>
        public EmCommProtCode EmCommProtCode
        {
            get
            {
                return this._EmCommProtCode;
            }
            set
            {
                this._EmCommProtCode = value;
            }
        }
        private string _LinkType_Code = string.Empty;
        /// <summary>
        /// 码值字符串表示的接线方式(如:16700000.1-单相)
        /// </summary>
        public string LinkType_Code
        {
            get
            {
                return this._LinkType_Code;
            }
            set
            {
                this._LinkType_Code = value;
            }
        }

        private EmPolar _ACPolar = EmPolar.Anode;
        /// <summary>
        /// 极性(共阴/共阳)
        /// </summary>
        public EmPolar ACPolar
        {
            get
            {
                return this._ACPolar;
            }
            set
            {
                this._ACPolar = value;
            }
        }

        private EmLinkType _AccessType = EmLinkType.ZhiJieAcc;
        /// <summary>
        /// 接入方式(是否经互感器)
        /// </summary>
        public EmLinkType AccessType
        {
            get
            {
                return this._AccessType;
            }
            set
            {
                this._AccessType = value;
            }
        }

        private string _AccessType_Code = string.Empty;
        /// <summary>
        /// 码值字符串表示的介入方式(如:经互感器接入-37200000.2)
        /// </summary>
        public string AccessType_Code
        {
            get
            {
                return this._AccessType_Code;
            }
            set
            {
                this._AccessType_Code = value;
            }
        }

        private int _IsBackStop = 0;
        /// <summary>
        /// 有无止逆器(0-无,1-有)
        /// </summary>
        public int IsBackStop
        {
            get
            {
                return this._IsBackStop;
            }
            set
            {
                this._IsBackStop = value;
            }
        }

        private EmTestMode _CheckType = EmTestMode.Null;
        /// <summary>
        /// 检查类别(抽检\年检)
        /// </summary>
        public EmTestMode CheckType
        {
            get
            {
                return this._CheckType;
            }
            set
            {
                this._CheckType = value;
            }
        }

        private string _CheckType_Code = string.Empty;
        /// <summary>
        /// 码值字符串表示的检测类别(如:抽检验收-60003000.10)
        /// </summary>
        public string CheckType_Code
        {
            get
            {
                return this._CheckType_Code;
            }
            set
            {
                this._CheckType_Code = value;
            }
        }

        private string _ManuFactur = string.Empty;
        /// <summary>
        /// 制造商
        /// </summary>
        public string ManuFactur
        {
            get
            {
                return this._ManuFactur;
            }
            set
            {
                this._ManuFactur = value;
            }
        }

        private string _ManuDate = string.Empty;
        /// <summary>
        /// 出厂日期
        /// </summary>
        public string ManuDate
        {
            get
            {
                return this._ManuDate;
            }
            set
            {
                this._ManuDate = value;
            }
        }

        private string _ExamUnit = string.Empty;
        /// <summary>
        /// 送检单位
        /// </summary>
        public string ExamUnit
        {
            get
            {
                return this._ExamUnit;
            }
            set
            {
                this._ExamUnit = value;
            }
        }

        private string _CertCode = string.Empty;
        /// <summary>
        /// 证书编号
        /// </summary>
        public string CertCode
        {
            get
            {
                return this._CertCode;
            }
            set
            {
                this._CertCode = value;
            }
        }

        private string _SealNo1 = string.Empty;
        /// <summary>
        /// 铅封号1
        /// </summary>
        public string SealNo1
        {
            get
            {
                return this._SealNo1;
            }
            set
            {
                this._SealNo1 = value;
            }
        }

        private string _SealNo2 = string.Empty;
        /// <summary>
        /// 铅封号2
        /// </summary>
        public string SealNo2
        {
            get
            {
                return this._SealNo2;
            }
            set
            {
                this._SealNo2 = value;
            }
        }

        private string _SealNo3 = string.Empty;
        /// <summary>
        /// 铅封号3
        /// </summary>
        public string SealNo3
        {
            get
            {
                return this._SealNo3;
            }
            set
            {
                this._SealNo3 = value;
            }
        }

        private string _TrayNo = string.Empty;
        /// <summary>
        /// 所属台体编号;表当前所在台体上: =0时不在任何台体上
        /// </summary>
        public string TrayNo
        {
            get
            {
                return this._TrayNo;
            }
            set
            {
                this._TrayNo = value;
            }
        }

        private string _SaveDate = string.Empty;
        /// <summary>
        /// 检定保存时间
        /// </summary>
        public string SaveDate
        {
            get
            {
                return this._SaveDate;
            }
            set
            {
                this._SaveDate = value;
            }
        }

        private string _WeekDate = string.Empty;
        /// <summary>
        /// 周检保存时间
        /// </summary>
        public string WeekDate
        {
            get
            {
                return this._WeekDate;
            }
            set
            {
                this._WeekDate = value;
            }
        }

        private int _MeterState = 0;
        /// <summary>
        /// 表状态，=0时，表在主控，=1时，表已下发至分台体检定中，=2时，检表完毕
        /// </summary>
        public int MeterState
        {
            get
            {
                return this._MeterState;
            }
            set
            {
                this._MeterState = value;
            }
        }

        private string _CheckHoman = string.Empty;
        /// <summary>
        /// 检定人
        /// </summary>
        public string CheckHoman
        {
            get
            {
                return this._CheckHoman;
            }
            set
            {
                this._CheckHoman = value;
            }
        }

        private string _ApproveHoman = string.Empty;
        /// <summary>
        /// 核定人
        /// </summary>
        public string ApproveHoman
        {

            get
            {
                return this._ApproveHoman;
            }
            set
            {
                this._ApproveHoman = value;
            }
        }

        private string _Manager = string.Empty;
        /// <summary>
        /// 主管
        /// </summary>
        public string Manager
        {
            get
            {
                return this._Manager;
            }
            set
            {
                this._Manager = value;
            }
        }

        private string _CheckTemperature = string.Empty;
        /// <summary>
        /// 检定时温度
        /// </summary>
        public string CheckTemperature
        {
            get
            {
                return this._CheckTemperature;
            }
            set
            {
                this._CheckTemperature = value;
            }
        }

        private string _CheckHumidity = string.Empty;
        /// <summary>
        /// 检定时湿度
        /// </summary>
        public string CheckHumidity
        {
            get
            {
                return this._CheckHumidity;
            }
            set
            {
                this._CheckHumidity = value;
            }
        }

        private EmTrialConclusion _CheckResult = EmTrialConclusion.CTG_Wait;
        /// <summary>
        /// 检定结果
        /// </summary>
        public EmTrialConclusion CheckResult
        {
            get
            {
                return this._CheckResult;
            }
            set
            {
                this._CheckResult = value;
            }
        }

        private int _IsTomis = 0;
        /// <summary>
        /// 是否上传到一体化平台(0表示已经上传成功,其它表示失败)
        /// </summary>
        public int IsTomis
        {
            get
            {
                return this._IsTomis;
            }
            set
            {
                this._IsTomis = value;
            }
        }

        private bool _Insulated = false;
        /// <summary>
        /// 是否需要隔离(true-是)
        /// </summary>
        public bool Insulated
        {
            get
            {
                return this._Insulated;
            }
            set
            {
                this._Insulated = value;
            }
        }

        private string _TaskId = string.Empty;
        /// <summary>
        /// 任务单编号
        /// </summary>
        public string TaskId
        {
            get
            {
                return this._TaskId;
            }
            set
            {
                this._TaskId = value;
            }
        }
        private string _SchemeId = string.Empty;
        /// <summary>
        /// 方案编号
        /// </summary>
        public string SchemeId
        {
            get
            {
                return this._SchemeId;
            }
            set
            {
                this._SchemeId = value;
            }
        }
        private string _BoxBar = string.Empty;
        /// <summary>
        /// 箱条码
        /// </summary>
        public string BoxBar
        {
            get
            {
                return this._BoxBar;
            }
            set
            {
                this._BoxBar = value;
            }
        }

        /// <summary>
        /// 对应中间库表唯一编号
        /// </summary>
        public string EquipId
        {
            get;
            set;
        }

        /// <summary>
        /// 跳闸方式
        /// </summary>
        public EmCloseMode CloseMode
        {
            get;
            set;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        public ClMeterInfo_Local()
        {
            EquipCatage = 1;
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
        /// 设备类别，01电能表、02互感器，03采集终端，04失压仪
        /// </summary>
        public int EquipCatage
        {
            get;
            set;
        }
    }
    #endregion
}
