using System;

namespace LYTest.MeterProtocol.Struct
{
    /// <summary>
    /// 密钥信息
    /// </summary>
    [Serializable()]
    public struct StKeyUpdateInfo
    {
        /// <summary>
        /// 本地表
        /// </summary>
        public bool LocalMeter { get; set; }
        /// <summary>
        /// 是否获取密文成功
        /// </summary>
        public bool GetKeyInfoSucc { get; set; }
        /// <summary>
        /// 是否公钥状态下更新
        /// </summary>
        public bool UpdateKeyPublic { get; set; }
        /// <summary>
        /// 分散因子
        /// </summary>
        public string MeterDiv { get; set; }

        /// <summary>
        /// 随机数
        /// </summary>
        public string MeterRand { get; set; }

        /// <summary>
        /// ESAM序列号
        /// </summary>
        public string MeterEsamNo { get; set; }

        public string 主控密钥明文 { get; set; }

        public string 远程密钥明文 { get; set; }

        public string 参数密钥明文 { get; set; }

        public string 身份密钥明文 { get; set; }

        public string 主控密钥密文 { get; set; }

        public string 主控密钥信息 { get; set; }

        public string 远程密钥密文;

        public string 远程密钥信息;

        public string 参数密钥密文;

        public string 参数密钥信息;

        public string 身份密钥密文;

        public string 身份密钥信息;
    }
}
