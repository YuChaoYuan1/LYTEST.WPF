using System;
using System.Collections.Generic;

namespace LYTest.Device
{
    internal class Fun
    {
        public static byte[] GetBytesDot4(double data)
        {
            byte[] buf = new byte[5];
            int datax4 = data * 10000 > int.MaxValue ? 0 : (data < int.MinValue ? 0 : Convert.ToInt32(Math.Abs(data) * 10000));
            buf[0] = (byte)((datax4 >> 24) & 0xFF);
            buf[1] = (byte)((datax4 >> 16) & 0xFF);
            buf[2] = (byte)((datax4 >> 8) & 0xFF);
            buf[3] = (byte)(datax4 & 0xFF);
            buf[4] = (byte)Convert.ToSByte(-4);
            return buf;
        }

        /// <summary>
        /// 计算检验码[帧头不进入检验范围]
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static byte GetChkSum(List<byte> data, int startPos, int length)
        {
            byte ck = 0;
            for (int i = startPos; i < length; i++)
            {
                ck ^= data[i];
            }
            return ck;
        }

        public static byte GetChkSum(byte[] data,int startPos)
        {
            List<byte> list = new List<byte>();
            list.AddRange(data);
            return GetChkSum(list, startPos, list.Count);
        }
    }
}
