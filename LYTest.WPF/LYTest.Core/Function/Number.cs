using LYTest.Core.Enum;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace LYTest.Core.Function
{
    /// <summary>
    /// 有关数字计算的公共函数
    /// </summary>
    public class Number
    {
        /// <summary>
        /// 计算一个数组的平均值
        /// </summary>
        /// <param name="data">参与计算的数字</param>
        /// <param name="inviade">不参与计算的无效数字</param>
        /// <returns></returns>
        public static float GetAvgA(float[] data, float inviade)
        {
            if (data.Length <= 0) return 99.99F;

            float sum = 0;
            foreach (float f in data)
            {
                if (f != inviade)
                    sum += f;
            }

            return sum / data.Length;
        }

        /// <summary>
        /// 计算一个数组的平均值
        /// </summary>
        /// <param name="arrNumber">参与计算的数字，默认-999F不参与计算</param>
        /// <returns></returns>
        public static float GetAvgA(float[] arrNumber)
        {
            return GetAvgA(arrNumber, -999F);
        }

        /// <summary>
        /// 化整值计算
        /// </summary>
        /// <param name="data">要化整的数字</param>
        /// <param name="space">化整间距</param>
        /// <returns>化整后的浮点数</returns>
        public static float GetHzz(float data, float space)
        {
            float abs = Math.Abs(data);     //用于操作
            int flag = data > 0 ? 1 : -1;   //记录符号
            if (space != 1)                 //如果化整间距不为1,则直接将Number/Space后按1的方法化整
            {
                abs /= space;
            }

            int inte = (int)abs;        // 取得整数部分
            float dot = abs - inte;     // 得到小数部分
            if (dot > 0.5F)             //右边部分大于0.5，则整数部分++
            {
                inte++;
            }
            else if (dot == 0.5F && inte % 2 == 1)       //==0.5 且为奇数 ,则检测化整位
            {
                inte++;
            }
            //还原
            abs = flag * inte * space;
            return abs;
        }
        /// <summary>
        /// 获取指定功率方向下的等级
        /// </summary>
        /// <param name="meterClass">完整等级</param>
        /// <param name="active">有功/无功</param>
        /// <returns></returns>
        public static float GetMeterPdClass(string meterClass, bool active, out string pdClass)
        {
            string[] level = GetDj(meterClass);
            pdClass = level[active ? 0 : 1];

            float meterLevel;
            if (pdClass == "A")
            {
                meterLevel = 2.0F;
            }
            else if (pdClass == "B")
            {
                meterLevel = 1.0F;
            }
            else if (pdClass == "C")
            {
                meterLevel = 0.5F;
            }
            else if (pdClass == "D")
            {
                meterLevel = 0.2F;
            }
            else if (pdClass == "E")
            {
                meterLevel = 0.1F;
            }
            else
            {
                if (!float.TryParse(pdClass, out meterLevel))                  //当前表的等级
                {
                    meterLevel = 0.02F;
                }
            }

            return meterLevel;                   //当前表的等级
        }

        private static readonly Dictionary<string, float[]> DicJianJu = new Dictionary<string, float[]>
        {
            { "Level0", new float[] { 0.0005F, 0.00005F } },      //0级标准表
            { "Level0.01", new float[] { 0.001F, 0.0001F } },      //0.01级标准表
            { "Level0.02", new float[] { 0.002F, 0.0002F } },      //0.02级标准表
            { "Level0.02B", new float[] { 0.002F, 0.0002F } },      //0.02级标准表
            { "Level0.05", new float[] { 0.005F, 0.0005F } },      //0.05级标准表
            { "Level0.05B", new float[] { 0.005F, 0.0005F } },      //0.05级标准表
            { "Level0.1", new float[] { 0.01F, 0.001F } },         //0.1级表标准表
            { "Level0.1B", new float[] { 0.01F, 0.001F } },         //0.1级标准表
            { "Level0.2B", new float[] { 0.02F, 0.002F } },         //0.2级标准表
            { "Level0.2", new float[] { 0.02F, 0.004F } },          //0.2级表
            { "Level0.2S", new float[] { 0.02F, 0.004F } },          //0.2级表
            { "Level0.5", new float[] { 0.05F, 0.01F } },           //0.5级表
            { "Level0.5S", new float[] { 0.05F, 0.01F } },           //0.5级表
            { "Level1", new float[] { 0.1F, 0.02F } },              //1级表
            { "Level1.5", new float[] { 0.2F, 0.04F } },           //2级表
            { "Level2", new float[] { 0.2F, 0.04F } },               //2级表
            { "Level3", new float[] { 0.2F, 0.04F } }               //3级表
        };
        /// <summary>
        /// 返回修约间距
        /// </summary>
        /// <IsDeviation>是否是偏差</IsDeviation> 
        /// <param name="meterLevel">等级</param>
        /// <returns></returns>
        public static float GetRoundingSpace(bool IsDeviation, float meterLevel)
        {
            string Key = string.Format("Level{0}", meterLevel);

            float[] JianJu;
            if (DicJianJu.ContainsKey(Key))
            {
                JianJu = DicJianJu[Key];
            }
            else
            {
                JianJu = new float[] { meterLevel * 0.1f, meterLevel * 0.1f };    //没有在字典中找到，则直接按2算
            }

            if (IsDeviation)
                return JianJu[1];//标偏差
            else
                return JianJu[0];//普通误差
        }

        /// <summary>
        /// 修约、化整值计算
        /// </summary>
        /// <param name="data">要化整的数字</param>
        /// <param name="space">化整间距</param>
        /// <returns>化整后的浮点数</returns>
        public static string GetRounding(float data, float space)
        {
            float abs = Math.Abs(data);     //用于操作
            int flag = data > 0 ? 1 : -1;   //记录符号
            if (space != 1)                 //如果化整间距不为1,则直接将Number/Space后按1的方法化整
            {
                abs /= space;
            }

            int inte = (int)abs;        // 取得整数部分
            float dot = abs - inte;     // 得到小数部分
            if (dot > 0.5F)             //右边部分大于0.5，则整数部分++
            {
                inte++;
            }
            else if (dot == 0.5F && inte % 2 == 1)       //==0.5 且为奇数 ,则检测化整位
            {
                inte++;
            }
            //还原
            abs = inte * space;
            int hzPrecision = Common.GetPrecision(space.ToString());
            
            if (flag > 0) return $"+{abs.ToString($"F{hzPrecision}")}";
            else return $"-{abs.ToString($"F{hzPrecision}")}"; ;
        }


        /// <summary>
        /// 计算一组数据的标准偏差
        /// </summary>
        /// <param name="arrNumber">输入数据数组</param>
        /// <param name="inviade">其中不参与计算的值</param>
        /// <returns>返回一组数据的偏差值((未化整))</returns>
        public static float GetWindage(float[] arrNumber, float inviade)
        {
            int intCount = 0;    //要计算偏差的成员个数
            float Sum = 0F;                     //和，用于计算平均值
            float Windage = 0F;                 //辅助计算变量

            //计算平均值
            for (int i = 0; i < arrNumber.Length; i++)
            {
                if (arrNumber[i] != inviade)
                {
                    Sum += arrNumber[i];
                    intCount++;
                }
            }
            if (intCount == 1) return 0F;

            float Avg = Sum / intCount;
            //计算偏差
            for (int i = 0; i < intCount; i++)
            {
                if (arrNumber[i] != inviade)
                {
                    Windage += (float)Math.Pow((arrNumber[i] - Avg), 2);
                }
            }
            Windage /= (intCount - 1);
            return (float)Math.Sqrt(Windage);
        }

        /// <summary>
        /// 计算一组数据的标准偏差
        /// </summary>
        /// <param name="arrNumber">输入数据数组</param>
        /// <returns>返回一组数据的偏差值((未化整))</returns>
        public static float GetWindage(float[] arrNumber)
        {
            return GetWindage(arrNumber, -999F);
        }

        /// <summary>
        /// 计算相对误差值[(a-b)/a]
        /// </summary>
        /// <param name="a">比较参数</param>
        /// <param name="b">被比较参数</param>
        /// <returns>返回二个数字相差百分比[形如:(a-b)/b *100],适用于走字误差，需量误差等计算</returns>
        public static float GetRelativeWuCha(float a, float b)
        {
            if (b == 0) return 99F;
            return (a - b) / b * 100.0f;
        }

        /// <summary>
        /// 获取指定IB倍数的电流值（包括了R46的表）
        /// </summary>
        /// <param name="xIb">电流倍数Imax,1.0Ib</param>
        /// <param name="Current">电流参数1.5(6)</param>
        /// <param name="HGQ">是否经互感器</param>
        /// <returns></returns>

        public static float GetCurrentByIb(string xIb, string Current, bool HGQ)
        {
            float _Ib;
            float _Imax;
            if (Current.Contains("-"))
            {
                Regex _Reg = new Regex("(?<imin>[\\d\\.]+)\\-(?<itr>[\\d\\.]+)\\((?<imax>[\\d\\.]+)\\)");
                //    Regex _Reg = new Regex("(?<Imin>[\\d\\.]+)\\-(?<Itr>[\\d\\.]+)\\((?<Imax>[\\d\\.]+)\\)");
                Match _Match = _Reg.Match(Current);
                if (_Match.Groups["itr"].Value.Length < 1)
                    return 0F;
                //if (Current.IndexOf("(") >= 0 && Current.IndexOf(")") >= 0)

                float _Imin = float.Parse(_Match.Groups["imin"].Value);
                float _Itr = float.Parse(_Match.Groups["itr"].Value);
                _Imax = float.Parse(_Match.Groups["imax"].Value);

                if (HGQ == false)
                {
                    _Ib = 10 * _Itr;
                }
                else
                {
                    _Ib = 20 * _Itr;
                }
                if (xIb.ToLower().IndexOf("imin") >= 0)
                {
                    return _Imin;
                }
                else if (xIb.ToLower().IndexOf("itr") >= 0)
                {


                    if (HGQ == false) //直接接入
                    {
                        if (xIb.ToLower() == "itr")
                            return _Ib / 10;
                        else if (xIb.ToLower().IndexOf("itr") >= 0)
                            return _Ib * float.Parse(xIb.ToLower().Replace("itr", "")) / 10;
                        return 0F;
                    }
                    else //经互感器
                    {
                        if (xIb.ToLower() == "itr")
                            return _Ib / 20;
                        else if (xIb.ToLower().IndexOf("itr") >= 0)
                            return _Ib * float.Parse(xIb.ToLower().Replace("itr", "")) / 20;
                        return 0F;
                    }
                }
                if (xIb.ToLower() == "imax")
                    return _Imax;
                else if (xIb.ToLower().IndexOf("imax") >= 0 && xIb.ToLower().IndexOf("ib") == -1)
                {
                    if (xIb.ToLower().IndexOf("imax/√2") != -1)
                    {
                        return (float)(_Imax / Math.Sqrt(2));
                    }
                    return _Imax * float.Parse(xIb.ToLower().Replace("imax", ""));
                }
                else if (xIb.ToLower() == "ib")
                    return _Ib;
                else if (xIb.ToLower().IndexOf("ib") >= 0 && xIb.ToLower().IndexOf("imax") == -1)
                    return _Ib * float.Parse(xIb.ToLower().Replace("ib", ""));
                else if (xIb.ToLower().IndexOf("(imax-ib)") >= 0)
                    if (xIb.ToLower().IndexOf("/") >= 0)
                        return (_Imax - _Ib) / float.Parse(xIb.ToLower().Replace("(imax-ib)/", ""));
                    else
                        return (_Imax - _Ib) * float.Parse(xIb.ToLower().Replace("(imax-ib)", ""));
                else
                    return 0F;
            }
            else
            {
                Regex _Reg = new Regex("(?<ib>[\\d\\.]+)\\((?<imax>[\\d\\.]+)\\)");
                //    Regex _Reg = new Regex("(?<Imin>[\\d\\.]+)\\-(?<Itr>[\\d\\.]+)\\((?<Imax>[\\d\\.]+)\\)");
                Match _Match = _Reg.Match(Current);
                if (_Match.Groups["ib"].Value.Length < 1)
                    return 0F;
                //    _Imin = float.Parse(_Match.Groups["imin"].Value);
                _Ib = float.Parse(_Match.Groups["ib"].Value);
                _Imax = float.Parse(_Match.Groups["imax"].Value);
                if (xIb.ToLower().IndexOf("itr") >= 0)
                {
                    if (HGQ == false) //直接接入
                    {
                        if (xIb.ToLower() == "itr")
                            return _Ib / 10;
                        else if (xIb.ToLower().IndexOf("itr") >= 0)
                            return _Ib * float.Parse(xIb.ToLower().Replace("itr", "")) / 10;
                        return 0F;
                    }
                    else //经互感器
                    {
                        if (xIb.ToLower() == "itr")
                            return _Ib / 20;
                        else if (xIb.ToLower().IndexOf("itr") >= 0)
                            return _Ib * float.Parse(xIb.ToLower().Replace("itr", "")) / 20;
                        return 0F;
                    }
                }
                if (xIb.ToLower() == "imax")
                    return _Imax;
                else if (xIb.ToLower().IndexOf("imax") >= 0 && xIb.ToLower().IndexOf("ib") == -1)
                    return _Imax * float.Parse(xIb.ToLower().Replace("imax", ""));
                else if (xIb.ToLower() == "ib")
                    return _Ib;
                else if (xIb.ToLower().IndexOf("ib") >= 0 && xIb.ToLower().IndexOf("imax") == -1)
                    return _Ib * float.Parse(xIb.ToLower().Replace("ib", ""));
                else if (xIb.ToLower().IndexOf("(imax-ib)") >= 0)
                    if (xIb.ToLower().IndexOf("/") >= 0)
                        return (_Imax - _Ib) / float.Parse(xIb.ToLower().Replace("(imax-ib)/", ""));
                    else
                        return (_Imax - _Ib) * float.Parse(xIb.ToLower().Replace("(imax-ib)", ""));
                else if (xIb.ToLower() == "imin")
                {
                    if (HGQ) return _Ib / 25F;
                    else return _Ib / 20F;
                }
                else
                    return 0F;
            }


        }
        /// <summary>
        /// Ib,Itr
        /// </summary>
        /// <param name="Current"></param>
        /// <param name="IbItr"></param>
        /// <returns>Ib或Itr</returns>
        public static string GetCurrentBase(string Current, out float IbItr)
        {
            if (Current.Contains("-"))
            {
                Regex reg = new Regex("(?<imin>[\\d\\.]+)\\-(?<itr>[\\d\\.]+)\\((?<imax>[\\d\\.]+)\\)");
                Match match = reg.Match(Current);

                //if (Current.IndexOf("(") >= 0 && Current.IndexOf(")") >= 0)
                //float _Imin = float.Parse(_Match.Groups["imin"].Value);
                float itr = float.Parse(match.Groups["itr"].Value);
                //float _Imax = float.Parse(_Match.Groups["imax"].Value);
                IbItr = itr;
                return "Itr";
            }
            else
            {
                Regex reg = new Regex("(?<ib>[\\d\\.]+)\\((?<imax>[\\d\\.]+)\\)");
                Match match = reg.Match(Current);

                float ib = float.Parse(match.Groups["ib"].Value);
                //float _Imax = float.Parse(_Match.Groups["imax"].Value);
                IbItr = ib;
                return "Ib";
            }


        }
        /// <summary>
        /// 获取有功或无功常数值 
        /// </summary>
        /// <param name="ConstString">表常数 有功（无功）</param>
        /// <param name="pw">是否是有功</param>
        /// <returns>[有功，无功]</returns>
        public static int GetBcs(string ConstString, PowerWay pw)
        {
            ConstString = ConstString.Replace("（", "(").Replace("）", ")");

            if (ConstString.Trim().Length < 1) return 1;

            string[] arTmp = ConstString.Trim().Replace(")", "").Split('(');

            if (arTmp.Length == 1)
            {
                if (IsNumeric(arTmp[0]))
                    return int.Parse(arTmp[0]);
                else
                    return 1;
            }
            else
            {
                if (IsNumeric(arTmp[0]) && IsNumeric(arTmp[1]))
                {
                    if (pw == PowerWay.正向有功 || pw == PowerWay.反向有功)
                        return int.Parse(arTmp[0]);
                    else
                        return int.Parse(arTmp[1]);
                }
                else
                    return 1;
            }
        }
        ///// <summary>
        ///// 获取电流倍数算术值
        ///// </summary>
        ///// <param name="xIb">电流倍数字符串1.5Ib</param>
        ///// <param name="Current">电流参数1.5(6)</param>
        ///// <returns></returns>
        //public static float GetIbX(string xIb, string Current)
        //{
        //    Regex reg = new Regex("(?<ib>[\\d\\.]+)\\((?<imax>[\\d\\.]+)\\)");
        //    Match match = reg.Match(Current);
        //    if (match.Groups["ib"].Value.Length < 1)
        //        return 0F;

        //    float ib = float.Parse(match.Groups["ib"].Value);
        //    float imax = float.Parse(match.Groups["imax"].Value);
        //    float x = imax / ib;  //倍数
        //    if (xIb.ToLower() == "imax")
        //        return x;
        //    else if (xIb.ToLower().IndexOf("imax") >= 0 && xIb.ToLower().IndexOf("ib") == -1)
        //        return x * float.Parse(xIb.ToLower().Replace("imax", ""));
        //    else if (xIb.ToLower() == "ib")
        //        return 1F;
        //    else if (xIb.ToLower().IndexOf("ib") >= 0 && xIb.ToLower().IndexOf("imax") == -1)
        //        return 1F * float.Parse(xIb.ToLower().Replace("ib", ""));
        //    else if (xIb.ToLower().IndexOf("(imax-ib)") >= 0)
        //        if (xIb.ToLower().IndexOf("/") >= 0)
        //            return ((imax - ib) / float.Parse(xIb.ToLower().Replace("(imax-ib)/", ""))) / ib;
        //        else
        //            return ((imax - ib) * float.Parse(xIb.ToLower().Replace("(imax-ib)", ""))) / ib;
        //    else
        //        return 1F;
        //}

        ///// <summary>
        ///// 获取电流倍数算术值
        ///// </summary>
        ///// <param name="xIb">电流倍数字符串1.5Ib</param>
        ///// <param name="Current">电流参数1.5(6)</param>
        ///// <returns></returns>
        //public static float GetIbX(string xIb, string Current, bool HGQ)
        //{
        //    float _Imin = 0F;
        //    float _Iitr = 0F;
        //    float _Ib = 0F;
        //    float _Imax = 0F;
        //    if (Current.Contains("-"))
        //    {
        //        Regex _Reg1 = new Regex("(?<imin>[\\d\\.]+)\\-(?<itr>[\\d\\.]+)\\((?<imax>[\\d\\.]+)\\)");
        //        Match _Match1 = _Reg1.Match(Current);
        //        if (_Match1.Groups["itr"].Value.Length < 1)
        //            return 0F;
        //        _Imin = float.Parse(_Match1.Groups["imin"].Value);
        //        _Iitr = float.Parse(_Match1.Groups["itr"].Value);
        //        _Imax = float.Parse(_Match1.Groups["imax"].Value);
        //        float _BeiShu;
        //        float _MinBshu;
        //        if (HGQ)
        //        {
        //            _Ib = 10 * _Iitr;
        //            _BeiShu = _Imax / _Ib;
        //            _MinBshu = _Imin / _Ib;
        //            if (xIb.ToLower() == "imax")
        //                return _BeiShu;
        //            else if (xIb.ToLower().IndexOf("imax") >= 0 && xIb.ToLower().IndexOf("ib") == -1)
        //                return _BeiShu * float.Parse(xIb.ToLower().Replace("imax", ""));
        //            else if (xIb.ToLower() == "ib")
        //                return 1F;
        //            else if (xIb.ToLower().IndexOf("ib") >= 0 && xIb.ToLower().IndexOf("imax") == -1)
        //                return 1F * float.Parse(xIb.ToLower().Replace("ib", ""));
        //            else if (xIb.ToLower().IndexOf("(imax-ib)") >= 0)
        //                if (xIb.ToLower().IndexOf("/") >= 0)
        //                    return ((_Imax - _Ib) / float.Parse(xIb.ToLower().Replace("(imax-ib)/", ""))) / _Ib;
        //                else
        //                    return ((_Imax - _Ib) * float.Parse(xIb.ToLower().Replace("(imax-ib)", ""))) / _Ib;
        //            else if (xIb.ToLower() == "imin")
        //                return _MinBshu;
        //            else if (xIb.ToLower() == "itr")
        //                return 0.2F;
        //            else if (xIb.ToLower().IndexOf("itr") >= 0 && xIb.ToLower().IndexOf("ib") == -1 && xIb.ToLower().IndexOf("imax") == -1)
        //                return 0.2F * float.Parse(xIb.ToLower().Replace("itr", ""));
        //            else
        //                return 1F;

        //        }
        //        else
        //        {
        //            _Ib = 10 * _Iitr;
        //            _BeiShu = _Imax / _Ib;
        //            _MinBshu = _Imin / _Ib;
        //            if (xIb.ToLower() == "imax")
        //                return _BeiShu;
        //            else if (xIb.ToLower().IndexOf("imax") >= 0 && xIb.ToLower().IndexOf("ib") == -1)
        //                return _BeiShu * float.Parse(xIb.ToLower().Replace("imax", ""));
        //            else if (xIb.ToLower() == "ib")
        //                return 1F;
        //            else if (xIb.ToLower().IndexOf("ib") >= 0 && xIb.ToLower().IndexOf("imax") == -1)
        //                return 1F * float.Parse(xIb.ToLower().Replace("ib", ""));
        //            else if (xIb.ToLower().IndexOf("(imax-ib)") >= 0)
        //                if (xIb.ToLower().IndexOf("/") >= 0)
        //                    return ((_Imax - _Ib) / float.Parse(xIb.ToLower().Replace("(imax-ib)/", ""))) / _Ib;
        //                else
        //                    return ((_Imax - _Ib) * float.Parse(xIb.ToLower().Replace("(imax-ib)", ""))) / _Ib;
        //            else if (xIb.ToLower() == "imin")
        //                return _MinBshu;
        //            else if (xIb.ToLower() == "itr")
        //                return 0.1F;
        //            else if (xIb.ToLower().IndexOf("itr") >= 0 && xIb.ToLower().IndexOf("ib") == -1 && xIb.ToLower().IndexOf("imax") == -1)
        //                return 0.1F * float.Parse(xIb.ToLower().Replace("itr", ""));
        //        }

        //        if (xIb.ToLower() == "imax")
        //            return _BeiShu;
        //        else if (xIb.ToLower().IndexOf("imax") >= 0 && xIb.ToLower().IndexOf("ib") == -1)
        //            return _BeiShu * float.Parse(xIb.ToLower().Replace("imax", ""));
        //        else if (xIb.ToLower() == "ib")
        //            return 1F;
        //        else if (xIb.ToLower().IndexOf("ib") >= 0 && xIb.ToLower().IndexOf("imax") == -1)
        //            return 1F * float.Parse(xIb.ToLower().Replace("ib", ""));
        //        else if (xIb.ToLower().IndexOf("(imax-ib)") >= 0)
        //            if (xIb.ToLower().IndexOf("/") >= 0)
        //                return ((_Imax - _Ib) / float.Parse(xIb.ToLower().Replace("(imax-ib)/", ""))) / _Ib;
        //            else
        //                return ((_Imax - _Ib) * float.Parse(xIb.ToLower().Replace("(imax-ib)", ""))) / _Ib;
        //        else
        //            return 1F;
        //    }
        //    else
        //    {
        //        Regex _Reg = new Regex("(?<ib>[\\d\\.]+)\\((?<imax>[\\d\\.]+)\\)");
        //        Match _Match = _Reg.Match(Current);
        //        if (_Match.Groups["ib"].Value.Length < 1)
        //            return 0F;
        //        _Ib = float.Parse(_Match.Groups["ib"].Value);
        //        _Imax = float.Parse(_Match.Groups["imax"].Value);
        //        float _BeiShu = _Imax / _Ib;
        //        if (xIb.ToLower() == "imax")
        //            return _BeiShu;
        //        else if (xIb.ToLower().IndexOf("imax") >= 0 && xIb.ToLower().IndexOf("ib") == -1)
        //            return _BeiShu * float.Parse(xIb.ToLower().Replace("imax", ""));
        //        else if (xIb.ToLower() == "ib")
        //            return 1F;
        //        else if (xIb.ToLower().IndexOf("ib") >= 0 && xIb.ToLower().IndexOf("imax") == -1)
        //            return 1F * float.Parse(xIb.ToLower().Replace("ib", ""));
        //        else if (xIb.ToLower().IndexOf("(imax-ib)") >= 0)
        //            if (xIb.ToLower().IndexOf("/") >= 0)
        //                return ((_Imax - _Ib) / float.Parse(xIb.ToLower().Replace("(imax-ib)/", ""))) / _Ib;
        //            else
        //                return ((_Imax - _Ib) * float.Parse(xIb.ToLower().Replace("(imax-ib)", ""))) / _Ib;
        //        else
        //            return 1F;
        //    }

        //}

        /// <summary>
        /// 获取功率因数数值
        /// </summary>
        /// <param name="Glys">功率因数1.0,0.5L</param>
        /// <returns></returns>
        public static float GetGlysValue(string Glys)
        {
            if (!IsNumeric(Glys.Substring(Glys.Length - 1, 1)))
                Glys = Glys.Substring(0, Glys.Length - 1);
            return float.Parse(Glys);
        }
        private static Regex IsNumeric_Reg = null;
        /// <summary>
        /// 检测是否是数字
        /// </summary>
        /// <param name="sNumeric">要验证的字符串</param>
        /// <returns>是Y否N</returns>
        public static bool IsNumeric(string sNumeric)
        {
            if (sNumeric == null || sNumeric.Length == 0) return false;
            if (IsNumeric_Reg == null)
                IsNumeric_Reg = new Regex("^[\\+\\-]?[0-9]*\\.?[0-9]+$");
            return IsNumeric_Reg.Replace(sNumeric, "").Length == 0;
        }

        /// <summary>
        /// 检查是否为整型数字、包括负数
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool IsIntNumber(string value)
        {
            if (string.IsNullOrEmpty(value)) return false;
            Regex reg = new Regex("-?[0-9]+");
            return reg.Replace(value, "").Length == 0;
        }

        /// <summary>
        /// 返回等级，数组下标0=有功，1=无功
        /// </summary>
        /// <param name="accuracy">等级字符串1.0S(2.0)</param>
        /// <returns></returns>
        public static string[] GetDj(string accuracy)
        {
            accuracy = accuracy.ToUpper().Replace("S", "");
            accuracy = accuracy.ToUpper().Replace("（", "(").Replace("）", ")").Replace(")", "");
            string[] dj = accuracy.Split('(');

            if (dj.Length == 1)
                return new string[] { dj[0], dj[0] };
            else
                return new string[] { dj[0], dj[1] };
        }






        /// <summary>
        /// 拆分1.5(6)字样的参数
        /// </summary>
        /// <param name="str">要拆分的对象</param>
        /// <param name="bFirst">是否是取第一个参数，如果为False则取第二个参数</param>
        /// <returns>指定的数据</returns>

        public static float SplitKF(string str, bool bFirst)
        {
            string[] arr = GetDj(str);
            string MeterLevel;
            if (bFirst == true)
                MeterLevel = arr[0].Trim();
            else
                MeterLevel = arr[1].Trim();

            float dj;
            switch (MeterLevel)
            {
                case "A":
                    dj = 2.0F;
                    break;
                case "B":
                    dj = 1.0F;
                    break;
                case "C":
                    dj = 0.5F;
                    break;
                case "D":
                    dj = 0.2F;
                    break;
                default:
                    dj = float.Parse(MeterLevel);
                    break;
            }

            return bFirst ? float.Parse(dj.ToString()) : float.Parse(arr[1]);
        }


        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="arrList"></param>
        ///// <param name="IsUP"></param>
        //public static void PopDesc(ref int[] arrList, bool IsUP)
        //{
        //    float[] fltArray = ArrayConvert.ToSingle(arrList);
        //    PopDesc(ref fltArray, IsUP);
        //    int pos = 0;
        //    foreach (float v in fltArray)
        //    {
        //        arrList[pos] = (int)v;
        //        pos++;
        //    }

        //}


        /// <summary>
        /// 冒泡排序法
        /// </summary>
        /// <param name="arr">要排序的数驵</param>
        /// <param name="IsUp">升/降序</param>
        public static void PopDesc(ref float[] arr, bool IsUp)
        {

            for (int i = 0; i < arr.Length; i++)
            {
                for (int j = i; j < arr.Length; j++)
                {
                    if (IsUp)
                    {
                        if (arr[i] > arr[j])
                        {
                            float temp = arr[i];
                            arr[i] = arr[j];
                            arr[j] = temp;
                        }
                    }
                    else
                    {
                        if (arr[i] < arr[j])
                        {
                            float temp = arr[i];
                            arr[i] = arr[j];
                            arr[j] = temp;
                        }
                    }
                }
            }


        }

        ///// <summary>
        ///// 将字节进行二进制反转，主要用于645与698特征字转换
        ///// </summary>
        ///// <param name="chr"></param>
        ///// <returns></returns>
        //public static byte BitRever(byte chr)
        //{
        //    string bs = Convert.ToString(chr, 2).PadLeft(8, '0');
        //    string s = "";
        //    for (int i = 0; i < 8; i++)
        //    {
        //        s += bs[7 - i];
        //    }
        //    return Convert.ToByte(s, 2);
        //}

        /// <summary>
        /// 将字节进行二进制反转，只要用于645与698特征字转换
        /// </summary>
        /// <param name="chr">转换的值</param>
        /// <param name="totalWidth">二进制长度</param>
        /// <returns></returns>
        public static int BitRever(int chr, int totalWidth)
        {
            string bs = Convert.ToString(chr, 2).PadLeft(totalWidth, '0');
            string s = "";
            for (int i = 0; i < totalWidth; i++)
            {
                s += bs[totalWidth - 1 - i];
            }
            return Convert.ToInt32(s, 2);
        }

        /// <summary>
        /// 计算指定负载下的标准功率.(W)
        /// </summary>
        /// <param name="U">负载电压</param>
        /// <param name="I">负载电流</param>
        /// <param name="Clfs">测量方式</param>
        /// <param name="Yj">元件H，ABC</param>
        /// <param name="Glys">功率因数，0.5L</param>
        /// <param name="isP">true 有功，false 无功</param>
        /// <returns>标准功率</returns>
        public static float CalculatePower(float U, float I, WireMode Clfs, Cus_PowerYuanJian Yj, string Glys, bool isP)
        {
            float flt_GlysP = 1;
            float flt_GlysQ;
            if (isP)
            {
                float.TryParse(Glys.Replace("C", "").Replace("L", "").ToString(), out flt_GlysP);
                flt_GlysQ = (float)Math.Sqrt(1 - Math.Pow(flt_GlysP, 2));
            }
            else
            {
                float.TryParse(Glys.Replace("C", "").Replace("L", "").ToString(), out flt_GlysQ);
                flt_GlysP = (float)Math.Sqrt(1 - Math.Pow(flt_GlysP, 2));
            }
            float p = U * I * flt_GlysP;
            float q = U * I * flt_GlysQ;
            if (Cus_PowerYuanJian.H == Yj)
            {
                if (Clfs == WireMode.三相四线)
                {
                    p *= 3F;
                    q *= 3F;
                }
                else if (Clfs == WireMode.单相)
                {

                }
                else
                {
                    p *= 1.732F;
                    q *= 1.732F;
                }
            }
            return isP ? p : q;
        }
    }
}
