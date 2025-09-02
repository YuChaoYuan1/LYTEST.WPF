using System;

namespace LYTest.Core.Model.DnbModel.DnbInfo
{
    /// <summary>
    /// 规约一致性数据
    /// </summary>
    [Serializable()]
    public class MeterDLTData : MeterBase
    {
        public MeterDLTData()
        {
            Value = "";
        }

        /// <summary>
        /// 数据项名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 项目ID  关联方案表Scheme_DLTData索引
        /// </summary>
        public string ItemID = "";
        /// <summary>
        /// 读写值  当读时保存值，当写时填“成功”、“失败”
        /// </summary>
        public string Value { get; set; }
        /// <summary>
        /// 5数据标识
        /// </summary>
        public string DataFlag { get; set; }

        /// <summary>
        /// 6标识意义
        /// </summary>
        public string FlagMsg { get; set; }

        /// <summary>
        /// 7长度
        /// </summary>
        public string DataLen { get; set; }

        /// <summary>
        /// 8格式
        /// </summary>
        public string DataFormat { get; set; }


        /// <summary>
        /// 10对比值
        /// </summary>
        public string StandardValue { get; set; }

        /// <summary>
        /// 11对比条件
        /// </summary>
        public string Condition { get; set; }
        /// <summary>
        /// 13结论Y/N
        /// </summary>
        public string Result { get; set; }
        /// <summary>
        /// 进度
        /// </summary>
        public string Progress { get; set; }

        //ADD JX 黑龙江哈尔滨流水线添加


        /// <summary>
        /// 设定值
        /// </summary>
        public string SetValue { get; set; }
        /// <summary>
        /// 读取值
        /// </summary>
        public string GetVavlue { get; set; }


        /// <summary>
        /// 子项目编号
        /// </summary>
        public string ChildredItemID { get; set; }
    }
}
