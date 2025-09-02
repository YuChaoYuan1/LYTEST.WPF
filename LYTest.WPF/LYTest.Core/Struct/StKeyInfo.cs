using System;

namespace LYTest.Core.Struct
{
    /// <summary>
    /// 密钥信息
    /// </summary>
    [Serializable()]
    public struct StKeyInfo
    {
        /// <summary>
        /// 密钥状态:00,测试状态,01:正式状态
        /// </summary>
        public byte keyStatus { get; set; }

        /// <summary>
        /// 密钥更新方式:本地方式:00;远程方式01;
        /// </summary>
        public byte UpdateType { get; set; }

        /// <summary>
        /// 密钥标识:密钥更新的标识信息
        /// </summary>
        public byte KeyCode { get; set; }

        /// <summary>
        /// 密钥版本:测试密钥为初始片本00,正式密钥文每变更一次,
        /// 片本增加一次,只有版本号大于现有ESAM中的密钥版本号才能进行密钥更新
        /// </summary>
        public byte KeyVer { get; set; }
        /// <summary>
        /// 秘钥更新的API入参0x5AH字节的数据
        /// </summary>
        /// <returns></returns>
        public string EsamCoreInfo { get; set; }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("{0:D2}{1:D2}{2:D2}{3:D2}", keyStatus, UpdateType, KeyCode, KeyVer);
        }
    }
}
