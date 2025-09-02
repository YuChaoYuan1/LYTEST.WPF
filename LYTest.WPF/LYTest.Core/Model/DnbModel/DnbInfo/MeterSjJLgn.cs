using System;

namespace LYTest.Core.Model.DnbModel.DnbInfo
{
    /// <summary>
    /// 事件记录
    /// </summary>
    [Serializable()]
    public class MeterSjJLgn : MeterBase
    {

        /// <summary>
        /// 项目ID	
        /// </summary>
        public string PrjID { get; set; }
        /// <summary>
        /// 项目名称描述
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 结论信息
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// 方案信息
        /// </summary>
        public string TestValue { get; set; }
        /// <summary>
        /// 结论
        /// </summary>
        public string Result { get; set; }




        /// <summary>
        /// 4.组类别
        /// </summary>
        public string GroupType { get; set; }

        /// <summary>
        /// 5.排序号
        /// </summary>
        public string ListNo { get; set; }

        /// <summary>
        /// 6.项目名：失压，断相
        /// </summary>
        public string ItemName { get; set; }

        /// <summary>
        /// 7.工况点编号
        /// </summary>
        public string StatusNo { get; set; }

        /// <summary>
        /// 8.分项目名称
        /// </summary>
        public string SubItemName { get; set; }

        /// <summary>
        /// 9.开始运行工况时间
        /// </summary>
        public string TestStartTime { get; set; }

        /// <summary>
        /// 10.工况结束时间
        /// </summary>
        public string TestEndTime { get; set; }

        /// <summary>
        /// 11.发生总次数
        /// </summary>
        public string SumTimes { get; set; }

        /// <summary>
        /// 12.总累计时间
        /// </summary>
        public string UseTime { get; set; }

        /// <summary>
        /// 13.发生时刻（仅有发生时刻的事件或最近一次发生时刻）
        /// </summary>
        public string RecordStartTime { get; set; }

        /// <summary>
        /// 14.结束时刻
        /// </summary>
        public string RecordEndTime { get; set; }

        /// <summary>
        /// 15.此工况点结论
        /// </summary>
        public string SubItemConc { get; set; }

        /// <summary>
        /// 16.结论Y/N
        /// </summary>
        //public string Result { get; set; }

        /// <summary>
        /// 17.A相总次数
        /// </summary>
        public string ASumTimes { get; set; }

        /// <summary>
        /// 18.A相总累计时间
        /// </summary>
        public string AUseTime { get; set; }

        /// <summary>
        /// 19.B相总次数
        /// </summary>
        public string BSumTimes { get; set; }

        /// <summary>
        /// 20.B相总累计时间
        /// </summary>
        public string BUseTime { get; set; }

        /// <summary>
        /// 21.C相总次数
        /// </summary>
        public string CSumTimes { get; set; }

        /// <summary>
        /// 22.C相总累计时间
        /// </summary>
        public string CUseTime { get; set; }

        /// <summary>
        /// 23.上N次记录【1-N】
        /// </summary>
        public string RecordNo { get; set; }

        /// <summary>
        /// 24.A相发生时刻
        /// </summary>
        public string ARecordStartTime { get; set; }

        /// <summary>
        /// 25.A相结束时间
        /// </summary>
        public string ARecordEndTime { get; set; }

        /// <summary>
        /// 26.A相发生时刻数据（不包括发生时刻）
        /// </summary>
        public string ARecordStartData { get; set; }

        /// <summary>
        /// 27.A相结束时刻数据（不包括结束时刻）
        /// </summary>
        public string ARecordEndData { get; set; }

        /// <summary>
        /// 28.A相事件期间数据（645-2007 中数据（增量）跟备案文件（发生、结束）不同）
        /// </summary>
        public string ARecordingData { get; set; }

        /// <summary>
        /// 29.B相发生时刻
        /// </summary>
        public string BRecordStartTime { get; set; }

        /// <summary>
        /// 30.B相结束时间
        /// </summary>
        public string BRecordEndTime { get; set; }

        /// <summary>
        /// 31.B相发生时刻数据（不包括发生时刻）
        /// </summary>
        public string BRecordStartData { get; set; }

        /// <summary>
        /// 32.B相结束时刻数据（不包括结束时刻）
        /// </summary>
        public string BRecordEndData { get; set; }

        /// <summary>
        /// 33.B相事件期间数据（645-2007 中数据（增量）跟备案文件（发生、结束）不同）
        /// </summary>
        public string BRecordingData { get; set; }

        /// <summary>
        /// 34.C相发生时刻
        /// </summary>
        public string CRecordStartTime { get; set; }

        /// <summary>
        /// 35.C相结束时间
        /// </summary>
        public string CRecordEndTime { get; set; }

        /// <summary>
        /// 36.C相发生时刻数据（不包括发生时刻）
        /// </summary>
        public string CRecordStartData { get; set; }

        /// <summary>
        /// 37.C相结束时刻数据（不包括结束时刻）
        /// </summary>
        public string CRecordEndData { get; set; }

        /// <summary>
        /// 38.C相事件期间数据（645-2007 中数据（增量）跟备案文件（发生、结束）不同）
        /// </summary>
        public string CRecordingData { get; set; }

        /// <summary>
        /// 39.操作者代码
        /// </summary>
        public string UserCode { get; set; }

        /// <summary>
        /// 40.操作标识
        /// </summary>
        public string DICode { get; set; }

        /// <summary>
        /// 41.其他记录数据
        /// </summary>
        public string RecordOther { get; set; }

        /// <summary>
        /// 42.标准最大不平衡率
        /// </summary>
        public string ImbalanceRatio { get; set; }

        /// <summary>
        /// 44.不合格原因
        /// </summary>
        public string DisReasion { get; set; }

    }
}
