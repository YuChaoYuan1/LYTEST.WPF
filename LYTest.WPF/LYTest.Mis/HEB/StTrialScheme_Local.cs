using LYTest.Core.Enum;
using LYTest.Core.Struct;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace LYTest.Mis.HEB
{
    #region =========== 试验方案 ===========
    /// <summary>
    /// 试验方案
    /// </summary>
    [Serializable, StructLayout(LayoutKind.Sequential)]
    public struct StTrialScheme_Local
    {
        /// <summary>
        /// 主键ID,用于修改和删除标识
        /// </summary>
        public int ID
        {
            get;
            set;
        }

        /// <summary>
        /// 表批次号
        /// </summary>
        public string BatchId;
        /// <summary>
        /// 方案编号
        /// </summary>
        public string SchemeId;
        /// <summary>
        /// 方案名称
        /// </summary>
        public string SchemeName;
        /// <summary>
        /// 试验项目名称
        /// </summary>
        public string ItemName
        {
            get;
            set;
        }
        /// <summary>
        /// 工位类型
        /// </summary>
        public EmLocalStationType StationType;
        /// <summary>
        /// 工位编号
        /// </summary>
        public int StationId;
        /// <summary>
        /// 试验类型
        /// </summary>
        public EmTrialType TrialType;
        /// <summary>
        /// 控源参数
        /// </summary>
        public string SourceParams;
        /// <summary>
        /// 项目参数
        /// </summary>
        public string ItemParams;
        /// <summary>
        /// 方案顺序。
        /// </summary>
        public int TrialOrder;

        /// <summary>
        ///参数验证
        /// </summary>
        public List<StParameterCheck> ParamtrCheck;
        /// <summary>
        /// 终端通信方式 0：485，1：网口
        /// </summary>

        public int CommMode;
        /// <summary>
        /// 是否上传实验数据
        /// </summary>
        public bool IsUploadData
        { get; set; }
        /// <summary>
        /// 加密机参数
        /// </summary>
        public string EncryportServer;
    }
    #endregion
}
