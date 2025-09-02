using System;
using System.Runtime.InteropServices;

namespace LYTest.Core.Struct
{
    #region =========== 单元台体信息结构体 ===========
    /// <summary>
    /// 单元台体信息结构体
    /// </summary>
    [Serializable, StructLayout(LayoutKind.Sequential)]
    public struct StStationState
    {
        /// <summary>
        /// 工位是否处于工作状态(true-是,false-空闲)
        /// </summary>
        public bool IsWorking;
        /// <summary>
        /// 工位编号
        /// </summary>
        public int StationId;

        private string proccessValue;
        /// <summary>
        /// 进度信息
        /// </summary>
        public string ProccessValue
        {
            get { return proccessValue; }
            set { proccessValue = value; }
        }

        public string _AnnexText;
        /// <summary>
        /// 描述工位状态的信息
        /// </summary> 
        public string AnnexText
        {
            get { return _AnnexText; }
            set { _AnnexText = value; }
        }
    }
    #endregion
}
