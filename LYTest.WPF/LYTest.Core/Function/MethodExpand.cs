using System;
using System.Collections.Generic;

namespace LYTest.Core.Function
{
    public static class MethodExpand
    {
        ///// <summary>
        ///// 将3字节转换为Float
        ///// </summary>
        ///// <param name="data">字节数组</param>
        ///// <param name="dotLen">小数点位数</param>
        ///// <returns></returns>
        //public static float ToSingle(this byte[] data, int dotLen)
        //{
        //    float f = data[0] << 16;
        //    f += data[1] << 8;
        //    f += data[2];

        //    f = (float)(f / Math.Pow(10, dotLen));
        //    return f;
        //}

        /// <summary>
        /// 十六进制字符串转字节数组
        /// </summary>
        /// <param name="data"></param>
        /// <param name="separatChar">字节码中的间隔字符</param>
        /// <returns></returns>
        public static byte[] ToArray(this string data, string separat)
        {
            data = data.Replace(separat, "");
            if (data.Length == 0) return new byte[0];

            int len = data.Length / 2;
            byte[] bs = new byte[len];
            for (int i = 0; i < len; i++)
            {
                bs[i] = Convert.ToByte(data.Substring(i * 2, 2), 16);
            }

            return bs;
        }

        ///// <summary>
        ///// 十六进制字符串转字节数组
        ///// </summary>
        ///// <param name="data"></param>
        //public static byte[] ToArray(this string data)
        //{
        //    return ToArray(data, "");
        //}

        ///// <summary>
        ///// 获取<see cref="Dictionary<TKey, TValue>"/>指定Key的Value,如果不存在则自动添加
        ///// </summary>
        ///// <typeparam name="TKey"></typeparam>
        ///// <typeparam name="TValue"></typeparam>
        ///// <param name="dic"></param>
        ///// <param name="key"></param>
        ///// <returns></returns>
        //public static TValue GetValue<TKey, TValue>(this Dictionary<TKey, TValue> dic, TKey key)
        //{

        //    if (dic.ContainsKey(key))
        //    {
        //        return dic[key];
        //    }
        //    else
        //    {
        //        Type t = typeof(TValue);
        //        TValue o = (TValue)Activator.CreateInstance(t);
        //        dic.Add(key, o);
        //        return o;

        //    }
        //}

        public static void RemoveAt<Tkey, TValue>(this Dictionary<Tkey, TValue> dic, Tkey key)
        {
            if (dic.ContainsKey(key))
            {
                dic.Remove(key);
            }
        }

        /// <summary>
        /// 填充数组元素为指定值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array"></param>
        /// <param name="value"></param>
        public static void Fill<T>(this T[] array, T value)
        {
            for (int i = 0; i < array.Length; i++)
                array[i] = value;
        }

    }
}
