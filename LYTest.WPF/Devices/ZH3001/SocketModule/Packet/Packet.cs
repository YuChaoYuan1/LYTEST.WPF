using System;
using System.Collections.Generic;
using System.Text;

namespace ZH.SocketModule.Packet
{
    /// <summary>
    /// 数据包基类，提供最基本的数据包操作
    /// </summary>
    abstract class Packet
    {

        /// <summary>
        /// 默认实现 ，返回类命名空间名称
        /// </summary>
        /// <returns></returns>
        public virtual string GetPacketName()
        {
            return this.GetType().FullName;
        }

        /// <summary>
        /// 获取包的解析
        /// </summary>
        /// <returns></returns>
        public virtual string GetPacketResolving()
        {
            return "没有实现解析";
        }
        /// <summary>
        /// 发送后等待返回时间（ms）
        /// </summary>
        /// <returns></returns>
        public virtual int WaiteTime()
        {
            return 200;
        }

        public byte[] GetBytesDot4(double data)
        {
            byte[] buf = new byte[5];
            int datax4;
            if (data * 10000 > int.MaxValue) datax4 = 0;
            else if (data < int.MinValue) datax4 = 0;
            else datax4 = Convert.ToInt32(Math.Abs(data) * 10000);
            buf[0] = (byte)((datax4 >> 24) & 0xFF);
            buf[1] = (byte)((datax4 >> 16) & 0xFF);
            buf[2] = (byte)((datax4 >> 8) & 0xFF);
            buf[3] = (byte)(datax4 & 0xFF);
            buf[4] = (byte)Convert.ToSByte(-4);
            return buf;
        }
    }
}
