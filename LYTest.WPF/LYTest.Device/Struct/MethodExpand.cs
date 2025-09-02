using System;

namespace LYTest.Device.Struct
{
    public static class MethodExpand
    {

        /// <summary>
        /// 将16进字符串转换成字节数组，如 123456 =[0x12,0x34,0x56]
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static byte[] ToBytes(this string str)
        {
            str = str.Replace(" ", "");
            if ((str.Length % 2) != 0)
            {
                str += '0';
            }

            byte[] arr = new byte[str.Length / 2];
            for (int i = 0; i < arr.Length; i++)
            {
                arr[i] = Convert.ToByte(str.Substring(i * 2, 2), 16);
            }

            return arr;

        }
    }
}
