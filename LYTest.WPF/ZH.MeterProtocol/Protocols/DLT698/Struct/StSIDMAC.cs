using System.Collections.Generic;

namespace LYTest.MeterProtocol.Protocols.DLT698.Struct
{
    public struct StSIDMAC
    {
        /// <summary>
        /// 安全标示
        /// </summary>
        public string SID { get; set; }
        /// <summary>
        /// 附加数据
        /// </summary>

        public string AttachData { get; set; }

        /// <summary>
        /// 明文或密文数据<OAD,数据>
        /// </summary>
        public Dictionary<string, List<string>> Data { get; set; }

        /// <summary>
        /// MAC
        /// </summary>
        public string MAC { get; set; }

        /// <summary>
        /// 随机数
        /// </summary>
        public string Rand { get; set; }
    }
}
