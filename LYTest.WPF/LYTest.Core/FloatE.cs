namespace LYTest.Core
{
    /// <summary>
    /// 显示浮点数
    /// </summary>
    public class FloatE
    {
        /// <summary>
        /// 十进制数的字符串
        /// </summary>
        public string Data { get; set; }
        /// <summary>
        /// 十进制数
        /// </summary>
        public decimal Value { get; set; }
        /// <summary>
        /// 显示浮点数的字符串构造
        /// </summary>
        /// <param name="value">十进制数的字符串</param>
        public FloatE(string value)
        {
            Data = value;
            _ = decimal.TryParse(Data, out decimal m);
            Value = m;
        }

        public FloatE(decimal value)
        {
            Value = value;
            Data = Value.ToString();
        }

        public static implicit operator FloatE(string value)
        {
            return new FloatE(value);
        }

        public static implicit operator FloatE(decimal value)
        {
            return new FloatE(value);
        }

        public static implicit operator string(FloatE customType)
        {
            return customType?.Data;
        }

        public static implicit operator decimal(FloatE customType)
        {
            return customType.Value;
        }

        public override string ToString()
        {
            return Data;
        }

        /// <summary>
        /// 单向转换float,兼容过度
        /// </summary>
        /// <param name="customType"></param>
        public static implicit operator float(FloatE customType)
        {
            return (float)customType.Value;
        }

        public static FloatE operator +(FloatE r1, FloatE r2)
        {
            return r1?.Value + r2?.Value;
        }

        public static FloatE operator -(FloatE r1, FloatE r2)
        {
            return r1?.Value - r2?.Value;
        }

        public static FloatE operator *(FloatE r1, FloatE r2)
        {
            return r1?.Value * r2?.Value;
        }

        public static FloatE operator /(FloatE r1, FloatE r2)
        {
            return r1?.Value / r2?.Value;
        }

        public static bool operator <(FloatE r1, FloatE r2)
        {
            return r1?.Value < r2?.Value;
        }

        public static bool operator >(FloatE r1, FloatE r2)
        {
            return r1?.Value > r2?.Value;
        }

        public static bool operator <=(FloatE r1, FloatE r2)
        {
            return r1?.Value <= r2?.Value;
        }

        public static bool operator >=(FloatE r1, FloatE r2)
        {
            return r1?.Value >= r2?.Value;
        }

    }
}
