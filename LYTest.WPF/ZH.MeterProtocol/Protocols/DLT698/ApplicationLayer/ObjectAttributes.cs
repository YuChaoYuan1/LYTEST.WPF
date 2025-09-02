using System.Collections.Generic;


namespace LYTest.MeterProtocol.Protocols.ApplicationLayer
{

    public class ObjectAttributes
    {
        /// <summary>
        /// 对象属性描述符
        /// </summary>
        public string Oad { get; set; }

        /// <summary>
        /// 数组或结构体类型
        /// </summary>
        public string BigType { get; set; }

        /// <summary>
        /// 数据元素个数
        /// </summary>
        public int ItemCount { get; set; }

        /// <summary>
        /// 数据信息
        /// </summary>
        public List<DataInfos> DataInfo { get; set; }



    }
    public struct DataInfos
    {
        /// <summary>
        /// 数据类型编码
        /// </summary>
        public int DataTypeCode { get; set; }

        /// <summary>
        /// 数据类型名称
        /// </summary>
        public string DataTypeName { get; set; }

        /// <summary>
        /// 是否含长度域
        /// </summary>
        public bool LengthFlag { get; set; }
        /// <summary>
        /// 数据长度(字节)
        /// </summary>
        public int DataLength { get; set; }

        /// <summary>
        ///  小数位数
        /// </summary>
        public int FloatCount { get; set; }


    }
}
