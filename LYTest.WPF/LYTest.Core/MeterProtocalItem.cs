//using System;
//using System.Data;

//namespace LYTest.Core
//{
//    /// <summary>
//    /// 功能描述：数据标识信息
//    /// </summary>
//    [Serializable()]
//    public class MeterProtocalItem
//    {

//        public MeterProtocalItem(DataRow row)
//        {
//            Name = row["name"].ToString();
//            DataFlag645 = row["dlt64507Id"].ToString();
//            DataFlag698 = row["dlt698Id"].ToString();
//            Length645 = Convert.ToInt32(row["dlt64507Len"]);
//            Dot645 = Convert.ToInt32(row["dlt64507Dot"]);
//            Format645 = row["dlt64507Format"].ToString();
//            Mode = Convert.ToInt32(row["dlt698Mode"]);
//            Dot698 = Convert.ToInt32(row["dlt698Dot"]);
//        }
//        //TODO 这里怕影响到之前的功能，先不对之前功能进行删除,后续测试完毕在删除
//        public MeterProtocalItem(DI dI)
//        {
//            //TODO 这里目前还有所属芯片类型等参数没有弄过来,后续根据需求添加
//            Name = dI.DataFlagDiName;
//            DataFlag645 = dI.DataFlagDi;
//            DataFlag698 = dI.DataFlagOi;
//            Length645 = int.Parse(dI.DataLength);
//            Dot645 = int.Parse(dI.DataSmallNumber);
//            Format645 = dI.DataFormat;
//            Mode = byte.Parse(dI.EmSecurityMode);
//            Dot698 = int.Parse(dI.DataSmallNumber); //TODO 原来协议里面有分645的位数和698的小数位数,这里目前没有区分,后续需要核对一下


//        }

//        /// <summary>
//        /// 数据标识名称
//        /// </summary>
//        public string Name { get; set; }
//        /// <summary>
//        /// 数据标识 645
//        /// </summary>
//        public string DataFlag645 { get; set; }

//        /// <summary>
//        /// 数据标识 698
//        /// </summary>
//        public string DataFlag698 { get; set; }

//        /// <summary>
//        /// 数据长度
//        /// </summary>
//        public int Length645 { get; set; }

//        /// <summary>
//        /// 小数位
//        /// </summary>
//        public int Dot645 { get; set; }
//        /// <summary>
//        /// 数据格式
//        /// </summary>
//        public string Format645 { get; set; }

//        /// <summary>
//        /// dlt698Mode
//        /// </summary>
//        public int Mode { get; set; }

//        /// <summary>
//        /// 小数位
//        /// </summary>
//        public int Dot698 { get; set; }
//    }
//}
