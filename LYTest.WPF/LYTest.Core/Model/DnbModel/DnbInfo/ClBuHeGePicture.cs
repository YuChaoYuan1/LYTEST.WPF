using System;

namespace LYTest.Core.Model.DnbModel.DnbInfo
{
    ///<METER_NO>表条码</METER_NO>
    //<STAION_ID>工位ID</STAION_ID>
    //<TASK_ID>任务号</TASK_ID>
    //<FILE_NAME>文件名称</FILE_NAME>
    //<SUFFIX>文件后缀名</SUFFIX>
    //<FILE_CONTENT>文件内容</FILE_CONTENT>

    [Serializable]
    public class ClBuHeGePicture
    {
        public string MeterNo { get; set; }

        public string StationId { get; set; }

        public string TaskId { get; set; }

        public string FileName { get; set; }

        public string Suffix { get; set; }

        public string FileContent { get; set; }

    }
}
