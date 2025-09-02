using System;

namespace LYTest.Core.Function
{
    public class Funs
    {
        /// <summary>
        /// 将字节进行二进制反转，主要用于645与698特征字转换
        /// </summary>
        /// <param name="chr"></param>
        /// <returns></returns>
        public static byte BitRever(byte chr)
        {
            string bs = Convert.ToString(chr, 2).PadLeft(8, '0');
            string s = "";
            for (int i = 0; i < 8; i++)
            {
                s += bs[7 - i];
            }
            return Convert.ToByte(s, 2);
        }

        ///// <summary>
        ///// 将字节进行二进制反转，只要用于645与698特征字转换
        ///// </summary>
        ///// <param name="chr">转换的值</param>
        ///// <param name="totalWidth">二进制长度</param>
        ///// <returns></returns>
        //public static int BitRever(int chr, int totalWidth)
        //{
        //    string bs = Convert.ToString(chr, 2).PadLeft(totalWidth, '0');
        //    string s = "";
        //    for (int i = 0; i < totalWidth; i++)
        //    {
        //        s += bs[totalWidth - 1 - i];
        //    }
        //    return Convert.ToInt32(s, 2);
        //}
    }
}
