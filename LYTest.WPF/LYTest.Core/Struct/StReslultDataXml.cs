using System;

namespace LYTest.Core.Struct
{
    #region =========== 设备返回数据结构体 ===========
    /// <summary>
    /// 设备返回数据
    /// </summary>
    [Serializable]
    public struct StReslultDataXml
    {
        /// <summary>
        /// 是否上传成功
        /// </summary>
        public bool IsUpload;
        /// <summary>
        /// 表条码 
        /// </summary>
        public string MeterNo;
        /// <summary>
        /// 实验项目ID 
        /// </summary>
        public int ItemId;
        /// <summary>
        /// 是否合格
        /// </summary>
        public bool IsPass;
    }
    #endregion
}
