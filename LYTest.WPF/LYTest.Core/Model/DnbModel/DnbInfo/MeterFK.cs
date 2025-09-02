using System;

namespace LYTest.Core.Model.DnbModel.DnbInfo
{
    /// <summary>
    /// 费控数据
    /// </summary>
    [Serializable()]
    public class MeterFK : MeterBase
    {
        public MeterFK() : this("")

        { }
        public MeterFK(string itemType) : base()
        {
            Group = "";
            ItemType = itemType;
            Result = "";
            Data = "";
            Name = "";
        }


        /// <summary>
        /// 4组别
        /// </summary>
        public string Group { get; set; }
        /// <summary>
        /// 5项目类型
        /// </summary>
        public string ItemType { get; set; }
        /// <summary>
        /// 6结论
        /// </summary>
        public string Result { get; set; }
        /// <summary>
        /// 7数据
        /// </summary>
        public string Data { get; set; }

        /// <summary>
        /// 项目名称描述
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 表地址
        /// </summary>
        public string MeterAddress { get; set; }

        /// <summary>
        /// 私钥密文
        /// </summary>
        public string PrivateKey { get; set; }

        //add yjt20220306
        /// <summary>
        /// 其他参数
        /// </summary>
        public string TestValue { get; set; }

        /// <summary>
        /// 电量
        /// </summary>
        public string Electricity { get; set; }

        /// <summary>
        /// 密钥下装状态
        /// </summary>
        public string PasswordState { get; set; }

        /// <summary>
        /// 私钥认证状态
        /// </summary>
        public string PrivateKeyState { get; set; }
    }
}
