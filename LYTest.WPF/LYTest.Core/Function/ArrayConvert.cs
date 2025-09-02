using System;

namespace LYTest.Core.Function
{
    /// <summary>
    /// 提供转换数组的函数
    /// </summary>
    public class ArrayConvert
    {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="arrInput"></param>
        /// <returns></returns>
        public static float[] ToSingle(int[] arrInput)
        {
            Converter<int, float> myCon = new Converter<int, float>(Cint2Float);
            return Array.ConvertAll<int, float>(arrInput, myCon);
        }
        private static float Cint2Float(int v)
        {
            return float.Parse(v.ToString());
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="arrInput"></param>
        /// <returns></returns>
        public static float[] ToSingle(long[] arrInput)
        {
            Converter<long, float> myCon = new Converter<long, float>(CLong2Float);
            float[] _fl = Array.ConvertAll<long, float>(arrInput, myCon);
            return _fl;
        }
        private static float CLong2Float(long lngIn)
        {
            return float.Parse(lngIn.ToString());
        }
        /// <summary>
        /// Int数组---->字符串数组
        /// </summary>
        /// <param name="arrInput"></param>
        /// <returns></returns>
        public static string[] ToString(int[] arrInput)
        {
            Converter<int, string> myCon = new Converter<int, string>(CInt2string);
            string[] _fl = Array.ConvertAll<int, string>(arrInput, myCon);
            return _fl;

        }

        private static string CInt2string(int str)
        {
            return Convert.ToString(str);

        }
        /// <summary>
        /// Int数组---->字符串数组
        /// </summary>
        /// <param name="arrInput"></param>
        /// <returns></returns>
        public static int[] ToInt(string[] arrInput)
        {
            Converter<string, int> myCon = new Converter<string, int>(Cstring2Int);
            return Array.ConvertAll(arrInput, myCon);

        }

        private static int Cstring2Int(string str)
        {
            if (string.IsNullOrEmpty(str))
                str = "0";
            return Convert.ToInt32(str);

        }


        /// <summary>
        /// 将变体类型转换为Float
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static float[] ToSingle(object[] data)
        {
            Converter<object, float> myCon = new Converter<object, float>(ToFloat);
            return Array.ConvertAll(data, myCon);
        }
        /// <summary>
        /// 将变体类型转换为Float
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static decimal[] Todecimal(object[] data)
        {
            Converter<object, decimal> myCon = new Converter<object, decimal>(Toecimal);
            return Array.ConvertAll(data, myCon);
        }
        private static decimal Toecimal(object str)
        {
            if (decimal.TryParse(str.ToString(), out decimal res))
                return res;
            else
                return -999m;
        }
        private static float ToFloat(object str)
        {
            if (float.TryParse(str.ToString(), out float res))
                return res;
            else
                return -999f;
        }

        /// <summary>
        /// 将byte[]装换位字符串，fjk
        /// </summary>
        /// <param name="arrInput"></param>
        /// <returns></returns>
        public static string ToString(byte[] arrInput)
        {
            if (arrInput == null) return "";
            return BitConverter.ToString(arrInput);
        }


    }
}
