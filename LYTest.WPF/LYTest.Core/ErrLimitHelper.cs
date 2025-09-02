using System;

namespace LYTest.Core
{
    public class ErrLimitHelper
    {

        public static string IBString = "";


        public ErrLimitHelper()
        {

        }


        ///// <summary>
        ///// 获取一个检定点的误差限
        ///// </summary>
        ///// <param name="WcLimitName">误差限名称</param>
        ///// <param name="GuiChengName">规程</param>
        ///// <param name="Dj">等级</param>
        ///// <param name="Yj">元件</param>
        ///// <param name="Hgq">互感器</param>
        ///// <param name="YouGong">是否有功</param>
        ///// <param name="Glys">功率因数</param>
        ///// <param name="xIb">电流倍数</param>
        ///// <returns></returns>
        //public string GetWcx(string WcLimitName
        //                   , string GuiChengName
        //                   , string Dj
        //                   , Enum.Cus_PowerYuanJian Yj
        //                   , bool Hgq
        //                   , bool YouGong
        //                   , string Glys
        //                   , string xIb)
        //{
        //    try
        //    {
        //        string _TmpWc = Wcx(xIb, GuiChengName, Dj, Yj, Glys, Hgq, YouGong);
        //        if (_TmpWc.IndexOf('.') == -1)
        //            _TmpWc = float.Parse(_TmpWc).ToString("F1");

        //        return string.Format("+{0}|-{1}", _TmpWc, _TmpWc);

        //    }
        //    catch
        //    {
        //        return "";
        //    }

        //}

        ///// <summary>
        ///// 获取偏差限值
        ///// </summary>
        ///// <param name="WcLimitName">误差限名称</param>
        ///// <param name="GuiChengName">规程名称</param>
        ///// <param name="Dj">等级</param>
        ///// <returns>返回偏差限值</returns>
        //public string getPcxValue(string WcLimitName, string GuiChengName, string Dj)
        //{
        //    string _TmpValue = "";
        //    _TmpValue = Pcx(Dj).ToString();

        //    return string.Format("+{0}|0", _TmpValue);

        //}


        ///// <summary>
        ///// 获取偏差限
        ///// </summary>
        ///// <param name="WcLimitName">误差限名称</param>
        ///// <param name="GuiChengName">规程名称</param>
        ///// <param name="Dj">等级</param>
        ///// <returns>返回偏差限值</returns>
        //public string getPcx(string WcLimitName, string GuiChengName, string Dj)
        //{
        //    string _TmpValue = "";

        //    _TmpValue = Pcx(Dj).ToString();

        //    return _TmpValue;

        //}


        ///// <summary>
        ///// 获取偏差限
        ///// </summary>
        ///// <param name="Dj">等级字符串不带S（0.2）</param>
        ///// <returns></returns>
        //public static float Pcx(string Dj)
        //{
        //    if (!Function.Number.IsNumeric(Dj))
        //        return 1F * 0.2F;
        //    return float.Parse(Dj) * 0.2F;
        //}

        /// <summary>
        /// 获取偏差限
        /// </summary>
        /// <param name="xIb">电流倍数</param>
        /// <param name="GuiChengName">规程名称</param>
        /// <param name="Dj">等级</param>
        /// <param name="Yj">元件</param>
        /// <param name="glys">功率因数</param>
        /// <param name="Hgq">互感器</param>
        /// <param name="active">有功无功</param>
        /// <returns></returns>
        public static string Pcx(string xIb, string GuiChengName, string Dj, string glys, bool Hgq)
        {
            switch (GuiChengName.ToUpper())
            {
                case "Q/GDW10827-2020":
                case "Q/GDW10364-2020":
                case "Q/GDW12175-2021":
                    {
                        return GetPCLimit_GBT17215(xIb, Dj,  glys, Hgq);
                    }
                default:
                    //TODO:是否需要依照其他的标准分割开？
                    //其他的标准依然用原来的:
                    return Pcx_NEW(Dj, xIb, glys, Hgq).ToString();

            }
        }

        /// <summary>
        /// 标准：GBT17215.321-2021————偏差,不区分互感器的误差限
        /// </summary>
        /// <param name="xIb"></param>
        /// <param name="Dj">等级</param>
        /// <param name="Yj">元件</param>
        /// <param name="glys">功率因数</param>
        /// <param name="Hgq">互感器</param>
        /// <param name="active">是否有功</param>
        /// <returns></returns>
        private static string GetPCLimit_GBT17215(string xIb, string Dj,  string glys, bool Hgq)
        {
            switch (Dj)
            {
                case "A":
                case "B":
                    return GetDeviationA(xIb, Hgq);
                case "C":
                    return GetDeviationC(xIb, glys, Hgq);
                case "D":
                    return GetDeviationD(xIb, glys, Hgq);
                case "E":
                    return GetDeviationE();
                default:
                    return Dj;
            }
        }

        //modify yjt 20230103 修改标准偏差的误差限
        /// <summary>
        /// 获取偏差限 新
        /// </summary>
        /// <param name="Dj">等级字符串不带S（0.2）</param>
        /// <returns></returns>
        public static float Pcx_NEW(string Dj, string xIb, string glys, bool hgq)
        {
            float pcLimit = 0;
            if (xIb.ToLower() == "ib") xIb = "1.0ib";
            float FloatCurrent = GetFloatCurrent(xIb, Dj, hgq, out float Imin, out float Itr, out float Imax, out _);
            switch (Dj)
            {
                case "0.2":
                case "0.2S":
                case "D":
                    {
                        if (glys == "1.0")
                        {
                            if (FloatCurrent >= Itr && FloatCurrent <= Imax)
                            {
                                pcLimit = 0.02f;
                            }
                            else if (FloatCurrent >= Imin && FloatCurrent < Itr)
                            {
                                pcLimit = 0.04f;
                            }
                        }
                        else
                        {
                            pcLimit = 0.03f;
                        }
                    }
                    break;
                case "0.5":
                case "0.5S":
                case "C":
                    {
                        if (glys == "1.0")
                        {
                            if (FloatCurrent >= Itr && FloatCurrent <= Imax)
                            {
                                pcLimit = 0.05f;
                            }
                            else if (FloatCurrent >= Imin && FloatCurrent < Itr)
                            {
                                pcLimit = 0.1f;
                            }
                        }
                        else
                        {
                            pcLimit = 0.06f;
                        }
                    }
                    break;
                case "1":
                case "1.0":
                case "2":
                case "2.0":
                case "B":
                case "A":
                    {
                        if (glys == "1.0")
                        {
                            if (FloatCurrent >= Itr && FloatCurrent <= Imax)
                            {
                                pcLimit = 0.1f;
                            }
                            else if (FloatCurrent >= Imin && FloatCurrent < Itr)
                            {
                                pcLimit = 0.15f;
                            }
                        }
                        else
                        {
                            pcLimit = 0.1f;
                        }
                    }
                    break;
                default:
                    pcLimit = 0.02F;
                    break;
            }
            return pcLimit;
        }

        /// <summary>
        /// 获取误差限
        /// </summary>
        /// <param name="xIb">电流倍数</param>
        /// <param name="GuiChengName">规程名称</param>
        /// <param name="Dj">等级</param>
        /// <param name="Yj">元件</param>
        /// <param name="glys">功率因数</param>
        /// <param name="Hgq">互感器</param>
        /// <param name="active">是否有功</param>
        /// <returns></returns>
        public static string Wcx(string xIb, string GuiChengName, string Dj, Enum.Cus_PowerYuanJian Yj, string glys, bool Hgq, bool active)
        {
            switch (GuiChengName.ToUpper())
            {
                case "JJG596-1999":
                    {
                        return GetLimit_JJG596_1999(xIb, Dj, Yj, glys, Hgq, active);
                    }
                case "JJG307-1988":
                    {
                        return GetGy(xIb, "JJG307-1988", Dj, Yj, glys, Hgq, active);
                    }
                case "JJG307-2006":
                    {
                        return GetGy(xIb, "JJG307-2006", Dj, Yj, glys, Hgq, active);
                    }
                case "JJG596-2012":
                    {
                        return GetLimit_JJG596_2012(xIb, Dj, Yj, glys, Hgq);
                    }
                case "IR46":
                    {
                        return GetLimit_IR46(xIb, Dj,  glys, Hgq);
                    }
                case "Q/GDW10827-2020":
                case "Q/GDW10364-2020":
                case "Q/GDW12175-2021":
                    {
                        return GetLimit_GBT17215(xIb, Dj, glys, Hgq, active);
                    }
                default:
                    return Dj;
            }
        }

        ///// <summary>
        ///// 获取影响量误差限
        ///// </summary>
        ///// <param name="ItemName">试验名称</param>           
        ///// <param name="Dj">等级</param>        
        ///// <param name="glys">功率因数</param>        
        ///// <returns></returns>
        //public static string getLimit_Effect(string ItemName, string Dj, string glys)
        //{
        //    switch (Dj)
        //    {
        //        case "1.0":
        //            {
        //                switch (ItemName)
        //                {
        //                    case "电压影响+10%":
        //                    case "电压影响-10%":
        //                        {
        //                            switch (glys)
        //                            {
        //                                case "1.0":
        //                                    return "0.7";
        //                                case "0.5L":
        //                                    return "1.0";
        //                                default:
        //                                    return "0.7";
        //                            }
        //                        }
        //                    case "电压影响+15%":
        //                    case "电压影响-20%":
        //                        {
        //                            return "2.1";
        //                        }
        //                    case "频率影响+2%":
        //                    case "频率影响-2%":
        //                        {
        //                            switch (glys)
        //                            {
        //                                case "1.0":
        //                                    return "0.5";
        //                                case "0.5L":
        //                                    return "0.7";
        //                                default:
        //                                    return "0.5";
        //                            }
        //                        }
        //                    case "逆相序影响":
        //                        {
        //                            return "1.5";
        //                        }
        //                    case "电压不平衡影响":
        //                        {
        //                            return "2.0";
        //                        }
        //                    case "电压电流线路中的谐波分量影响":
        //                        {
        //                            return "0.8";
        //                        }
        //                    case "交流电流线路中次谐波分量影响":
        //                        {
        //                            return "3.0";
        //                        }
        //                    case "交流电流线路中奇次谐波分量影响":
        //                        {
        //                            return "3.0";
        //                        }
        //                    case "交流电流线路中直流和偶次谐波分量影响":
        //                        {
        //                            return "3.0";
        //                        }
        //                    default:
        //                        return "3.0";
        //                }
        //            }
        //        case "0.5S":
        //            {
        //                switch (ItemName)
        //                {
        //                    case "电压影响+10%":
        //                    case "电压影响-10%":
        //                        {
        //                            switch (glys)
        //                            {
        //                                case "1.0":
        //                                    return "0.2";
        //                                case "0.5L":
        //                                    return "0.4";
        //                                default:
        //                                    return "0.5";
        //                            }
        //                        }
        //                    case "电压影响+15%":
        //                    case "电压影响-20%":
        //                        {
        //                            return "0.6";
        //                        }
        //                    case "频率影响+2%":
        //                    case "频率影响-2%":
        //                        {
        //                            return "0.2";
        //                        }
        //                    case "逆相序影响":
        //                        {
        //                            return "0.1";
        //                        }
        //                    case "电压不平衡影响":
        //                        {
        //                            return "1.0";
        //                        }
        //                    case "电压电流线路中的谐波分量影响":
        //                        {
        //                            return "0.5";
        //                        }
        //                    case "交流电流线路中次谐波分量影响":
        //                        {
        //                            return "1.5";
        //                        }
        //                    case "交流电流线路中奇次谐波分量影响":
        //                        {
        //                            return "1.5";
        //                        }
        //                    case "交流电流线路中直流和偶次谐波分量影响":
        //                        {
        //                            return "1.5";
        //                        }
        //                }
        //            }
        //            break;
        //        case "0.2S":
        //            {
        //                switch (ItemName)
        //                {
        //                    case "电压影响+10%":
        //                    case "电压影响-10%":
        //                        {
        //                            switch (glys)
        //                            {
        //                                case "1.0":
        //                                    return "0.1";
        //                                case "0.5L":
        //                                    return "0.2";
        //                                default:
        //                                    return "0.1";
        //                            }
        //                        }
        //                    case "电压影响+15%":
        //                    case "电压影响-20%":
        //                        {
        //                            return "0.3";
        //                        }
        //                    case "频率影响+2%":
        //                    case "频率影响-2%":
        //                        {
        //                            return "0.1";
        //                        }
        //                    case "逆相序影响":
        //                        {
        //                            return "0.05";
        //                        }
        //                    case "电压不平衡影响":
        //                        {
        //                            return "0.5";
        //                        }
        //                    case "电压电流线路中的谐波分量影响":
        //                        {
        //                            return "0.4";
        //                        }
        //                    case "交流电流线路中次谐波分量影响":
        //                        {
        //                            return "0.4";
        //                        }
        //                    case "交流电流线路中奇次谐波分量影响":
        //                        {
        //                            return "0.4";
        //                        }
        //                    case "交流电流线路中直流和偶次谐波分量影响":
        //                        {
        //                            return "0.4";
        //                        }
        //                    default:
        //                        return "0.4";
        //                }
        //            }
        //        default:
        //            {
        //                switch (ItemName)
        //                {
        //                    case "电压影响+10%":
        //                    case "电压影响-10%":
        //                        {
        //                            switch (glys)
        //                            {
        //                                case "1.0":
        //                                    return "0.7";
        //                                case "0.5L":
        //                                    return "1.0";
        //                                default:
        //                                    return "0.7";
        //                            }
        //                        }
        //                    case "电压影响+15%":
        //                    case "电压影响-20%":
        //                        {
        //                            return "2.1";
        //                        }
        //                    case "频率影响+2%":
        //                    case "频率影响-2%":
        //                        {
        //                            switch (glys)
        //                            {
        //                                case "1.0":
        //                                    return "0.5";
        //                                case "0.5L":
        //                                    return "0.7";
        //                                default:
        //                                    return "0.5";
        //                            }
        //                        }
        //                    case "逆相序影响":
        //                        {
        //                            return "1.5";
        //                        }
        //                    case "电压不平衡影响":
        //                        {
        //                            return "2.0";
        //                        }
        //                    case "电压电流线路中的谐波分量影响":
        //                        {
        //                            return "0.8";
        //                        }
        //                    case "交流电流线路中次谐波分量影响":
        //                        {
        //                            return "3.0";
        //                        }
        //                    case "交流电流线路中奇次谐波分量影响":
        //                        {
        //                            return "3.0";
        //                        }
        //                    case "交流电流线路中直流和偶次谐波分量影响":
        //                        {
        //                            return "3.0";
        //                        }
        //                    default:
        //                        return "3.0";
        //                }
        //            }

        //    }
        //    return "1.0";
        //}

        private static string GetLimit_JJG596_1999(string xIb, string Dj, Enum.Cus_PowerYuanJian Yj, string glys, bool Hgq, bool active)
        {
            if (active)
            {
                switch (Dj)
                {
                    #region
                    case "0.02":
                        return Getdz002(xIb, Yj, glys);
                    case "0.05":
                        return Getdz005(xIb, Yj, glys);
                    case "0.1":
                        return Getdz01(xIb, Yj, glys);
                    case "0.2":
                        if ((int)Yj == 1)  ///合元
                        {
                            return Getdz02(xIb, glys, Hgq);
                        }
                        else
                        {
                            if (glys == "1.0")
                                return "0.3";
                            else
                                return "0.4";
                        }
                    case "0.5":
                        if ((int)Yj == 1)  ///合元
                        {
                            return Getdz05(xIb, glys, Hgq);
                        }
                        else
                        {
                            if (glys == "1.0")
                                return "0.6";
                            else
                                return "1.0";
                        }
                    case "1":
                    case "1.0":
                        if ((int)Yj == 1)  ///合元
                        {
                            return Getdz10(xIb, glys, Hgq);
                        }
                        else
                        {
                            if (glys == "1.0")
                                return "2.0";
                            else
                                return "2.0";
                        }
                    case "2":
                    case "2.0":
                        if ((int)Yj == 1)  //合元
                        {
                            return Getdz20(xIb, glys, Hgq);
                        }
                        else
                        {
                            if (glys == "1.0")
                                return "3.0";
                            else
                                return "3.0";
                        }
                    case "3":
                    case "3.0":
                        return Getdz30(xIb, glys, Hgq);

                    default:
                        return Dj;

                        #endregion
                }
            }
            else
            {
                return Dj;
            }
        }

        private static string GetLimit_JJG596_2012(string xIb, string Dj, Enum.Cus_PowerYuanJian Yj, string glys, bool Hgq)
        {
            switch (Dj)
            {
                #region
                case "0.02":
                    return Getdz002(xIb, Yj, glys);
                case "0.05":
                    return Getdz005(xIb, Yj, glys);
                case "E":
                case "0.1":
                    return Getdz01(xIb, Yj, glys);
                case "D":
                case "0.2":
                case "0.2S":
                    if ((int)Yj == 1)  ///合元
                    {
                        return Getdz02(xIb, glys, Hgq);
                    }
                    else
                    {
                        if (glys == "1.0")
                            return "0.3";
                        else
                            return "0.4";
                    }
                case "C":
                case "0.5":
                case "0.5S":
                    if ((int)Yj == 1)  ///合元
                    {
                        return Getdz05(xIb, glys, Hgq);
                    }
                    else
                    {
                        if (glys == "1.0")
                            return "0.6";
                        else
                            return "1.0";
                    }
                case "B":
                case "1":
                case "1.0":
                case "1S":
                case "1.0S":
                    if ((int)Yj == 1)  ///合元
                    {
                        return Getdz10(xIb, glys, Hgq);
                    }
                    else
                    {
                        if (glys == "1.0")
                            return "2.0";
                        else
                            return "2.0";
                    }
                case "A":
                case "2":
                case "2.0":
                    if ((int)Yj == 1)  ///合元
                    {
                        return Getdz20(xIb, glys, Hgq);
                    }
                    else
                    {
                        if (glys == "1.0")
                            return "3.0";
                        else
                            return "3.0";
                    }
                case "3":
                case "3.0":
                    return Getdz30(xIb, glys, Hgq);

                default:
                    return Dj;

                    #endregion
            }
        }

        private static string GetLimit_IR46(string xIb, string Dj,  string glys, bool Hgq)
        {
            switch (Dj)
            {
                #region
                case "A":
                    return GetdzA(xIb, Hgq);
                case "B":
                    return GetdzB(xIb, Hgq);
                case "C":
                    return GetdzC(xIb, glys, Hgq);
                case "D":
                    return GetdzD(xIb,  glys, Hgq);
                case "1":
                case "1.0":
                    return Getdz10(xIb, glys, Hgq);
                case "2":
                case "2.0":
                    return Getdz20(xIb, glys, Hgq);
                case "3":
                case "3.0":
                    return Getdz30(xIb, glys, Hgq);

                default:
                    return Dj;

                    #endregion
            }
        }

        /// <summary>
        /// 标准：GBT17215.321-2021————初始固有误差
        /// </summary>
        /// <param name="xIb"></param>
        /// <param name="Dj">等级</param>
        /// <param name="Yj">元件</param>
        /// <param name="glys">功率因数</param>
        /// <param name="Hgq">互感器</param>
        /// <param name="active">是否有功</param>
        /// <returns></returns>
        private static string GetLimit_GBT17215(string xIb, string Dj, string glys, bool Hgq, bool active)
        {
            //有功
            if (active)
            {
                switch (Dj)
                {
                    case "A":
                    case "B":
                        return GetActiveA(xIb, active);
                    case "C":
                        return GetActiveC(xIb, glys, active);
                    case "D":
                        return GetActiveD(xIb, glys, active);
                    case "E":
                        return GetActiveE(xIb, glys, active);

                    default:
                        return Dj;
                }
            }
            else//无功
            {
                switch (Dj)
                {
                    case "A":
                        return GetReactiveA(xIb, Dj, Hgq);//TODO:有功等级
                    case "2":
                        return GetReactive20(xIb, glys, Hgq);
                    case "3":
                        return GetReactive30(xIb, glys, Hgq);
                    default:
                        return Dj;
                }
            }
        }

        #region GBT17215.321-2021

        #region 误差
        /// <summary>
        /// A级有功
        /// </summary>
        /// <param name="xIb"></param>
        /// <param name="Yj"></param>
        /// <param name="glys"></param>
        /// <param name="active"></param>
        /// <returns></returns>
        private static string GetActiveA(string xIb, bool hgq)
        {
            if (xIb.ToLower() == "ib") xIb = "1.0ib";
            float FloatCurrent = GetFloatCurrent(xIb, "A", hgq, out _, out float Itr, out float Imax, out float Ib);
            string wcLimit;
            if (FloatCurrent >= Itr && FloatCurrent <= Imax)
            {
                wcLimit = "1.0";
            }
            else if (FloatCurrent >= (float)0f && FloatCurrent < Itr)
            {
                wcLimit = "1.5";
            }
            else
            {
                wcLimit = (1.5 * ((float)0f / FloatCurrent)).ToString();
            }

            if (xIb.ToLower().IndexOf("ib") != -1)
            {
                if (FloatCurrent >= Ib && FloatCurrent <= Imax)
                {
                    wcLimit = "1.0";
                }
                else
                {
                    wcLimit = "1.5";
                }
            }
            return wcLimit;
        }

        /// <summary>
        /// C级有功
        /// </summary>
        /// <param name="xIb"></param>
        /// <param name="Yj"></param>
        /// <param name="glys"></param>
        /// <param name="active"></param>
        /// <returns></returns>
        private static string GetActiveC(string xIb, string glys, bool hgq)
        {
            if (xIb.ToLower() == "ib") xIb = "1.0ib";
            float FloatCurrent = GetFloatCurrent(xIb, "C", hgq, out float Imin, out float Itr, out float Imax, out _);
            string wcLimit = "0.5";
            if (FloatCurrent >= Itr && FloatCurrent <= Imax)
            {
                if (glys == "0.5L" || glys == "0.8C")
                {
                    wcLimit = "0.6";
                }
                else if (glys == "1.0")
                {
                    wcLimit = "0.5";
                }
            }
            else if (FloatCurrent >= Imin && FloatCurrent < Itr)
            {
                wcLimit = "1.0";
            }
            else if (FloatCurrent < Imin && FloatCurrent >= (0.04 * Itr))
            {
                wcLimit = (1.0f * (Imin / FloatCurrent)).ToString();
            }

            return wcLimit;
        }

        /// <summary>
        /// D级有功
        /// </summary>
        /// <param name="xIb"></param>
        /// <param name="Yj"></param>
        /// <param name="glys"></param>
        /// <param name="active"></param>
        /// <returns></returns>
        private static string GetActiveD(string xIb, string glys, bool hgq)
        {
            if (xIb.ToLower() == "ib") xIb = "1.0ib";
            float FloatCurrent = GetFloatCurrent(xIb, "D", hgq, out float Imin, out float Itr, out float Imax, out _);
            string wcLimit = "0.2";
            if (FloatCurrent >= Itr && FloatCurrent <= Imax)
            {
                if (glys == "0.5L" || glys == "0.8C")
                {
                    wcLimit = "0.3";
                }
                else if (glys == "1.0")
                {
                    wcLimit = "0.2";
                }
            }
            else if (FloatCurrent >= Imin && FloatCurrent < Itr)
            {
                if (glys == "0.5L" || glys == "0.8C")
                {
                    wcLimit = "0.5";
                }
                else if (glys == "1.0")
                {
                    wcLimit = "0.4";
                }

            }
            else if (FloatCurrent < Imin && FloatCurrent >= (0.04 * Itr))
            {
                wcLimit = (0.4f * (Imin / FloatCurrent)).ToString();
            }
            return wcLimit;
        }

        /// <summary>
        /// E级有功
        /// </summary>
        /// <param name="xIb"></param>
        /// <param name="Yj"></param>
        /// <param name="glys"></param>
        /// <param name="active"></param>
        /// <returns></returns>
        private static string GetActiveE(string xIb, string glys, bool hgq)
        {
            if (xIb.ToLower() == "ib") xIb = "1.0ib";
            float FloatCurrent = GetFloatCurrent(xIb, "E", hgq, out float Imin, out float Itr, out float Imax, out _);
            string wcLimit = "0.1";
            if (FloatCurrent >= Itr && FloatCurrent <= Imax)
            {
                if (glys == "0.5L" || glys == "0.8C")
                {
                    wcLimit = "0.15";
                }
                else if (glys == "1.0")
                {
                    wcLimit = "0.1";
                }
            }
            else if (FloatCurrent >= Imin && FloatCurrent < Itr)
            {
                if (glys == "0.5L" || glys == "0.8C")
                {
                    wcLimit = "0.25";
                }
                else if (glys == "1.0")
                {
                    wcLimit = "0.2";
                }

            }
            else if (FloatCurrent < Imin && FloatCurrent >= (0.04 * Itr))
            {
                wcLimit = (0.2f * (Imin / FloatCurrent)).ToString();
            }
            return wcLimit;
        }


        /// <summary>
        /// A级无功
        /// </summary>
        /// <param name="xIb"></param>
        /// <param name="Yj"></param>
        /// <param name="glys"></param>
        /// <param name="active"></param>
        /// <returns></returns>
        private static string GetReactiveA(string xIb, string mclass, bool hgq)
        {
            if (xIb.ToLower() == "ib") xIb = "1.0ib";
            float FloatCurrent = GetFloatCurrent(xIb, mclass, hgq, out float Imin, out float Itr, out float Imax, out float Ib);
            string wcLimit;
            if (FloatCurrent >= Itr && FloatCurrent <= Imax)
            {
                wcLimit = "2.0";
            }
            else if (FloatCurrent >= Imin && FloatCurrent < Itr)
            {
                wcLimit = "2.5";
            }
            else
            {
                wcLimit = (1.5 * (Imin / FloatCurrent)).ToString();
            }

            if (xIb.ToLower().IndexOf("ib") != -1)
            {
                if (FloatCurrent >= Ib && FloatCurrent <= Imax)
                {
                    wcLimit = "2.0";
                }
                else
                {
                    wcLimit = "2.5";
                }
            }
            return wcLimit;
        }

        /// <summary>
        /// 无功2级
        /// </summary>
        /// <param name="xIb"></param>
        /// <param name="glys"></param>
        /// <param name="Hgq"></param>
        /// <returns></returns>
        private static string GetReactive20(string xIb, string glys, bool Hgq)
        {
            if (xIb.ToLower() == "ib") xIb = "1.0ib";

            string wcLimit;
            if (glys == "1.0")
            {
                if (Hgq)
                {
                    if (xIb.ToLower().IndexOf("imax") == -1 && xIb.ToLower().IndexOf("imax-ib") == -1 && float.Parse(xIb.ToLower().Replace("ib", "")) < 0.05F)
                        wcLimit = "2.5";
                    else
                        wcLimit = "2.0";
                }
                else
                {
                    if (xIb.ToLower().IndexOf("imax") == -1 && xIb.ToLower().IndexOf("imax-ib") == -1 && float.Parse(xIb.ToLower().Replace("ib", "")) < 0.1F)
                        wcLimit = "2.5";
                    else
                        wcLimit = "2.0";
                }
            }
            else
            {
                if (Hgq)
                {
                    if (xIb.ToLower().IndexOf("imax") == -1 && xIb.ToLower().IndexOf("imax-ib") == -1 && float.Parse(xIb.ToLower().Replace("ib", "")) < 0.1F)
                        wcLimit = "2.5";
                    else
                        wcLimit = "2.0";
                }
                else
                {
                    if (xIb.ToLower().IndexOf("imax") == -1 && xIb.ToLower().IndexOf("imax-ib") == -1 && float.Parse(xIb.ToLower().Replace("ib", "")) < 0.2F)
                        wcLimit = "2.5";
                    else
                        wcLimit = "2.0";
                }
            }

            return wcLimit;
        }

        /// <summary>
        /// 无功3级
        /// </summary>
        /// <param name="xIb"></param>
        /// <param name="glys"></param>
        /// <param name="Hgq"></param>
        /// <returns></returns>
        private static string GetReactive30(string xIb, string glys, bool Hgq)
        {
            if (xIb.ToLower() == "ib") xIb = "1.0ib";
            if (xIb.ToLower().IndexOf("in") >= 0)
            {
                xIb = xIb.ToLower().Replace("in", "ib");
                if (xIb.ToLower() == "in") xIb = "1.0ib";
            }
            if (glys == "1" || glys.ToUpper() == "1L" || glys.ToUpper() == "1C") glys = "1.0";

            string wcLimit;
            float xI = 0;
            if (xIb.ToLower().IndexOf("imax") == -1)
                xI = float.Parse(xIb.ToLower().Replace("ib", ""));

            if (Hgq)
            {
                if (xI >= 0.05F && glys == "1.0")
                {
                    wcLimit = "3.0";
                }
                else if (xI >= 0.1F && glys == "0.5L" || glys == "0.5C" || glys == "0.5")
                {
                    wcLimit = "3.0";
                }
                else
                {
                    wcLimit = "4.0";
                }
            }
            else
            {
                if (xI >= 0.1F && glys == "1.0")
                {
                    wcLimit = "3.0";
                }
                else if (xI >= 0.2F && glys == "0.5L" || glys == "0.5C" || glys == "0.5")
                {
                    wcLimit = "3.0";
                }
                else
                {
                    wcLimit = "4.0";
                }
            }

            return wcLimit;
        }

        #endregion

        #region 偏差
        private static string GetDeviationA(string xIb, bool hgq)
        {
            if (xIb.ToLower() == "ib") xIb = "1.0ib";
            float FloatCurrent = GetFloatCurrent(xIb, "A", hgq, out float Imin, out float Itr, out _, out _);
            string wcLimit;
            if (FloatCurrent >= Imin && FloatCurrent < Itr)
            {
                wcLimit = "0.15";
            }
            else
            {
                wcLimit = "0.1";
            }
            return wcLimit;
        }

        private static string GetDeviationC(string xIb, string glys, bool hgq)
        {
            if (xIb.ToLower() == "ib") xIb = "1.0ib";
            float FloatCurrent = GetFloatCurrent(xIb, "C", hgq, out float Imin, out float Itr, out float Imax, out _);
            string wcLimit = "0.1";

            if (glys == "1.0")
            {
                if (FloatCurrent <= Itr && FloatCurrent <= Imax)
                {
                    wcLimit = "0.05";
                }
                else if (FloatCurrent <= Imin && FloatCurrent < Itr)
                {
                    wcLimit = "0.1";
                }
            }
            else if (glys == "0.5L" || glys == "0.8C")
            {
                if (FloatCurrent <= Itr && FloatCurrent <= Imax)
                {
                    wcLimit = "0.06";
                }
            }

            return wcLimit;
        }


        private static string GetDeviationD(string xIb, string glys, bool hgq)
        {
            if (xIb.ToLower() == "ib") xIb = "1.0ib";
            float FloatCurrent = GetFloatCurrent(xIb, "D", hgq, out float Imin, out float Itr, out float Imax, out _);
            string wcLimit = "0.4";

            if (glys == "1.0")
            {
                if (FloatCurrent <= Itr && FloatCurrent <= Imax)
                {
                    wcLimit = "0.02";
                }
                else if (FloatCurrent <= Imin && FloatCurrent < Itr)
                {
                    wcLimit = "0.04";
                }
            }
            else if (glys == "0.5L" || glys == "0.8C")
            {
                if (FloatCurrent <= Itr && FloatCurrent <= Imax)
                {
                    wcLimit = "0.03";
                }
            }

            return wcLimit;
        }


        private static string GetDeviationE()
        {
            //if (xIb.ToLower() == "ib") xIb = "1.0ib";
            //float FloatCurrent = getFloatCurrent(xIb, "E", hgq, out _, out _, out _, out _);
            string wcLimit = "0.2";



            return wcLimit;
        }
        #endregion

        #endregion

        #region
        /// <summary>
        /// 电子式0.02级
        /// </summary>
        /// <param name="xIb">电流倍数</param>
        /// <param name="Yj">元件</param>
        /// <param name="glys">功率因数</param>
        /// <returns></returns>
        private static string Getdz002(string xIb, Enum.Cus_PowerYuanJian Yj, string glys)
        {
            if (xIb.ToLower() == "ib")
            {
                xIb = "1.0ib";
            }
            string wcLimit;
            if (Yj == Enum.Cus_PowerYuanJian.H)
            {
                if (glys == "1.0")
                {
                    wcLimit = "0.02";
                    if (xIb.ToLower().IndexOf("imax") == -1 && xIb.ToLower().IndexOf("imax-ib") == -1 && float.Parse(xIb.ToLower().Replace("ib", "")) < 0.1F)
                        wcLimit = "0.04";
                }
                else
                {
                    wcLimit = "0.02";
                    if (xIb.ToLower().IndexOf("imax") == -1 && xIb.ToLower().IndexOf("imax-ib") == -1 && float.Parse(xIb.ToLower().Replace("ib", "")) < 0.5F)
                        wcLimit = "0.03";
                    else if (xIb.ToLower().IndexOf("imax") >= 0 || xIb.ToLower().IndexOf("imax-ib") >= 0 || float.Parse(xIb.ToLower().Replace("ib", "")) > 0.1F)
                        wcLimit = "0.03";
                    if (glys == "0.25L")
                        wcLimit = "0.04";
                    else if (glys == "0.5C")
                        wcLimit = "0.03";
                }
            }
            else
            {
                wcLimit = "0.03";
                if (glys != "1.0" && (xIb.ToLower().IndexOf("imax") == -1 && xIb.ToLower().IndexOf("imax-ib") == -1 && float.Parse(xIb.ToLower().Replace("ib", "")) < 0.5F))
                    wcLimit = "0.04";
            }
            return wcLimit;

        }

        /// <summary>
        /// 电子式0.05级
        /// </summary>
        /// <param name="xIb"></param>
        /// <param name="Yj"></param>
        /// <param name="glys"></param>
        /// <returns></returns>
        private static string Getdz005(string xIb, Enum.Cus_PowerYuanJian Yj, string glys)
        {
            if (xIb.ToLower() == "ib")
            {
                xIb = "1.0ib";
            }
            string wcLimit;
            if (Yj == Enum.Cus_PowerYuanJian.H)
            {
                if (glys == "1.0")
                {
                    wcLimit = "0.05";
                    if (xIb.ToLower().IndexOf("imax") == -1 && xIb.ToLower().IndexOf("imax-ib") == -1 && float.Parse(xIb.ToLower().Replace("ib", "")) < 0.1F)
                        wcLimit = "0.1";
                }
                else
                {
                    wcLimit = "0.05";
                    if (xIb.ToLower().IndexOf("imax") == -1 && xIb.ToLower().IndexOf("imax-ib") == -1 && float.Parse(xIb.ToLower().Replace("ib", "")) < 0.2F)
                        wcLimit = "0.15";
                    if (xIb.ToLower().IndexOf("imax") == -1 && xIb.ToLower().IndexOf("imax-ib") == -1 && float.Parse(xIb.ToLower().Replace("ib", "")) < 0.5F)
                        wcLimit = "0.075";
                    if (xIb.ToLower().IndexOf("imax") >= 0 || xIb.ToLower().IndexOf("imax-ib") >= 0 || float.Parse(xIb.ToLower().Replace("ib", "")) > 0.1F)
                        wcLimit = "0.075";
                    if (glys.ToUpper() == "0.5C")
                        wcLimit = "0.1";
                    if (glys.ToUpper() == "0.25L")
                        wcLimit = "0.15";
                }
            }
            else
            {
                wcLimit = "0.075";
                if (glys != "1.0" && (xIb.ToLower().IndexOf("imax") == -1 && xIb.ToLower().IndexOf("imax-ib") == -1 && float.Parse(xIb.ToLower().Replace("ib", "")) < 0.5F))
                    wcLimit = "0.1";
            }
            return wcLimit;
        }

        /// <summary>
        /// 电子式0.1级
        /// </summary>
        /// <param name="xIb"></param>
        /// <param name="Yj"></param>
        /// <param name="glys"></param>
        /// <returns></returns>
        private static string Getdz01(string xIb, Enum.Cus_PowerYuanJian Yj, string glys)
        {
            if (xIb.ToLower() == "ib") xIb = "1.0ib";

            string wcLimit;
            if (Yj == Enum.Cus_PowerYuanJian.H)
                if (glys == "1.0")
                {
                    wcLimit = "0.1";
                    if (xIb.ToLower().IndexOf("imax") == -1 && xIb.ToLower().IndexOf("imax-ib") == -1 && float.Parse(xIb.ToLower().Replace("ib", "")) < 0.1F)
                        wcLimit = "0.2";
                }
                else
                {
                    wcLimit = "0.1";
                    if (xIb.ToLower().IndexOf("imax") == -1 && xIb.ToLower().IndexOf("imax-ib") == -1 && float.Parse(xIb.ToLower().Replace("ib", "")) < 0.2F)
                        wcLimit = "0.3";
                    if (xIb.ToLower().IndexOf("imax") == -1 && xIb.ToLower().IndexOf("imax-ib") == -1 && float.Parse(xIb.ToLower().Replace("ib", "")) < 0.5F)
                        wcLimit = "0.15";
                    if (xIb.ToLower().IndexOf("imax") >= 0 || xIb.ToLower().IndexOf("imax-ib") >= 0 || float.Parse(xIb.ToLower().Replace("ib", "")) > 0.1F)
                        wcLimit = "0.15";
                    if (glys == "0.5C")
                        wcLimit = "0.2";
                    if (glys == "0.25L")
                        wcLimit = "0.3";
                }
            else
            {
                wcLimit = "0.15";
                if (glys != "1.0" && (xIb.ToLower().IndexOf("imax") == -1 && xIb.ToLower().IndexOf("imax-ib") == -1 && float.Parse(xIb.ToLower().Replace("ib", "")) < 0.5F))
                    wcLimit = "0.2";
            }
            return wcLimit;
        }

        /// <summary>
        /// 电子式0.2级
        /// </summary>
        /// <param name="xIb"></param>
        /// <param name="glys"></param>
        /// <param name="Hgq"></param>
        /// <returns></returns>
        private static string Getdz02(string xIb, string glys, bool Hgq)
        {
            float FloatCurrent = GetFloatCurrent(xIb, "0.2", Hgq, out _, out _, out _, out float Ib);

            string wcLimit;
            if (glys == "1.0")
                if (Hgq)
                {
                    if (FloatCurrent < 0.05 * Ib)
                        wcLimit = "0.4";
                    else
                        wcLimit = "0.2";
                }
                else
                {
                    if (FloatCurrent < 0.1 * Ib)
                        wcLimit = "0.4";
                    else
                        wcLimit = "0.2";
                }
            else if (glys.ToUpper() == "0.5C" || glys.ToUpper() == "0.25L")
                wcLimit = "0.5";
            else
            {
                if (Hgq)
                {
                    if (FloatCurrent < 0.1 * Ib)
                        wcLimit = "0.5";
                    else
                        wcLimit = "0.3";
                }
                else
                {
                    if (FloatCurrent < 0.2 * Ib)
                        wcLimit = "0.5";
                    else
                        wcLimit = "0.3";
                }
            }

            return wcLimit;
        }

        /// <summary>
        /// 电子式0.5级
        /// </summary>
        /// <param name="xIb"></param>
        /// <param name="glys"></param>
        /// <param name="Hgq"></param>
        /// <returns></returns>
        private static string Getdz05(string xIb, string glys, bool Hgq)
        {
            float FloatCurrent = GetFloatCurrent(xIb, "0.5", Hgq, out _, out _, out _, out float Ib);

            string limit;
            if (glys == "1.0")
                if (Hgq)
                {
                    if (FloatCurrent < 0.05 * Ib)
                        limit = "1.0";
                    else
                        limit = "0.5";
                }
                else
                {
                    if (FloatCurrent < 0.1 * Ib)
                        limit = "1.0";
                    else
                        limit = "0.5";
                }
            else if (glys.ToUpper() == "0.5C" || glys.ToUpper() == "0.25L")
                limit = "1.0";
            else
            {
                if (Hgq)
                {
                    if (FloatCurrent < 0.1 * Ib)
                        limit = "1.0";
                    else
                        limit = "0.6";
                }
                else
                {
                    if (FloatCurrent < 0.2 * Ib)
                        limit = "1.0";
                    else
                        limit = "0.6";
                }
            }

            return limit;
        }

        /// <summary>
        /// 电子式1级
        /// </summary>
        /// <param name="xIb"></param>
        /// <param name="glys"></param>
        /// <param name="Hgq"></param>
        /// <returns></returns>
        private static string Getdz10(string xIb, string glys, bool Hgq)
        {
            float FloatCurrent = GetFloatCurrent(xIb, "1", Hgq, out _, out _, out _, out float Ib);

            string wcLimit;
            if (glys == "1.0")
                if (Hgq)
                {
                    if (FloatCurrent < 0.05 * Ib)
                        wcLimit = "1.5";
                    else
                        wcLimit = "1.0";
                }
                else
                {
                    if (FloatCurrent < 0.1 * Ib)
                        wcLimit = "1.5";
                    else
                        wcLimit = "1.0";
                }
            else if (glys.ToUpper() == "0.5C")
                wcLimit = "2.5";
            else if (glys.ToUpper() == "0.25L")
                wcLimit = "3.5";
            else
            {
                if (Hgq)
                {
                    if (FloatCurrent < 0.1 * Ib)
                        wcLimit = "1.5";
                    else
                        wcLimit = "1.0";
                }
                else
                {
                    if (FloatCurrent < 0.2 * Ib)
                        wcLimit = "1.5";
                    else
                        wcLimit = "1.0";
                }
            }

            return wcLimit;
        }

        /// <summary>
        /// 电子式2级
        /// </summary>
        /// <param name="xIb"></param>
        /// <param name="glys"></param>
        /// <param name="Hgq"></param>
        /// <returns></returns>
        private static string Getdz20(string xIb, string glys, bool Hgq)
        {
            float FloatCurrent = GetFloatCurrent(xIb, "2", Hgq, out _, out _, out _, out float Ib);

            string wcLimit;
            if (glys == "1.0")
            {
                if (Hgq)
                {
                    if (FloatCurrent < 0.05 * Ib)
                        wcLimit = "2.5";
                    else
                        wcLimit = "2.0";
                }
                else
                {
                    if (FloatCurrent < 0.1 * Ib)
                        wcLimit = "2.5";
                    else
                        wcLimit = "2.0";
                }
            }
            else
            {
                if (Hgq)
                {
                    if (FloatCurrent < 0.1 * Ib)
                        wcLimit = "2.5";
                    else
                        wcLimit = "2.0";
                }
                else
                {
                    if (FloatCurrent < 0.2 * Ib)
                        wcLimit = "2.5";
                    else
                        wcLimit = "2.0";
                }
            }

            return wcLimit;
        }

        /// <summary>
        /// 电子式3级
        /// </summary>
        /// <param name="xIb"></param>
        /// <param name="glys"></param>
        /// <param name="Hgq"></param>
        /// <returns></returns>
        private static string Getdz30(string xIb, string glys, bool Hgq)
        {
            float FloatCurrent = GetFloatCurrent(xIb, "2", Hgq, out _, out _, out _, out float Ib);

            if (glys == "1" || glys.ToUpper() == "1L" || glys.ToUpper() == "1C") glys = "1.0";

            string wcLimit;

            if (Hgq)
            {
                if (FloatCurrent >= 0.05 * Ib && glys == "1.0")
                {
                    wcLimit = "3.0";
                }
                else if (FloatCurrent >= 0.1 * Ib && glys == "0.5L" || glys == "0.5C" || glys == "0.5")
                {
                    wcLimit = "3.0";
                }
                else
                {
                    wcLimit = "4.0";
                }
            }
            else
            {
                if (FloatCurrent >= 0.1 * Ib && glys == "1.0")
                {
                    wcLimit = "3.0";
                }
                else if (FloatCurrent >= 0.2 * Ib && glys == "0.5L" || glys == "0.5C" || glys == "0.5")
                {
                    wcLimit = "3.0";
                }
                else
                {
                    wcLimit = "4.0";
                }
            }

            return wcLimit;
        }

        /// <summary>
        /// 电子式A级
        /// </summary>
        /// <param name="xIb"></param>
        /// <param name="glys"></param>
        /// <param name="Hgq"></param>
        /// <returns></returns>
        private static string GetdzA(string xIb, bool hgq)
        {
            if (xIb.ToLower() == "ib") xIb = "1.0ib";
            float FloatCurrent = GetFloatCurrent(xIb, "A", hgq, out float Imin, out float Itr, out _, out float Ib);
            string wcLimit;
            if (FloatCurrent >= Itr)
            {
                wcLimit = "2.0";
            }
            else if (FloatCurrent >= Imin && FloatCurrent < Itr)
            {
                wcLimit = "2.5";
            }
            else
            {
                wcLimit = (2.5 * (Imin / FloatCurrent)).ToString();
            }

            if (xIb.ToLower().IndexOf("ib") != -1)
            {
                if (FloatCurrent >= Ib && FloatCurrent <= (float)0f)
                {
                    wcLimit = "2.0";
                }
                else
                {
                    wcLimit = "2.5";
                }
            }
            return wcLimit;
        }
        /// <summary>
        /// 计算电流。等级完整格式或有功等级，用于计算Imin
        /// </summary>
        /// <param name="xIb"></param>
        /// <param name="mclass">等级，或有功等级</param>
        /// <param name="hgq"></param>
        /// <param name="Imin"></param>
        /// <param name="Itr"></param>
        /// <param name="Imax"></param>
        /// <param name="Ib"></param>
        /// <returns></returns>
        public static float GetFloatCurrent(string xIb, string mclass, bool hgq, out float Imin, out float Itr, out float Imax, out float Ib)
        {
            string activeClass;
            if (string.IsNullOrWhiteSpace(mclass)) activeClass = "A";
            else activeClass = mclass.Split('(')[0];

            string[] strCurrent = IBString.Split('-');

            string[] strCurrent1;
            if (strCurrent.Length == 1)
            {
                strCurrent1 = strCurrent[0].Split('(', '（');
                Ib = float.Parse(strCurrent1[0]);
                if (hgq) Itr = Ib * 0.05f;
                else Itr = Ib * 0.1f;

                if (hgq) Imin = Itr * 0.2f;
                else
                {
                    if (activeClass.IndexOf("A") >= 0 || activeClass.IndexOf("2") >= 0)
                        Imin = Itr * 0.5f;
                    else
                        Imin = Itr * 0.4f;
                }
            }
            else
            {
                Imin = float.Parse(strCurrent[0]);
                strCurrent1 = strCurrent[1].Split('(', '（');
                Itr = float.Parse(strCurrent1[0]);
                if (hgq) Ib = Itr * 20;
                else Ib = Itr * 10;
            }

            string strImax = strCurrent1[1].Replace(")", "").Replace("）", "");
            Imax = float.Parse(strImax);


            float FloatCurrent = 0;
            if (xIb.ToLower().IndexOf("itr") != -1)
            {
                if (xIb.ToLower() == "itr")
                {
                    FloatCurrent = Itr;
                }
                else
                {
                    FloatCurrent = float.Parse(xIb.ToLower().Replace("itr", "")) * Itr;
                }
            }
            else if (xIb.ToLower().IndexOf("imin") != -1)
            {
                if (xIb.ToLower() == "imin")
                {
                    FloatCurrent = Imin;
                }
                else
                {
                    FloatCurrent = float.Parse(xIb.ToLower().Replace("imin", "")) * Imin;
                }
            }
            else if (xIb.ToLower().IndexOf("imax") != -1)
            {
                if (xIb.ToLower() == "imax")
                {
                    FloatCurrent = Imax;
                }
                else if (xIb == "Imax/√2" || xIb == "imax/√2" || xIb == "IMAX/√2")
                {
                    FloatCurrent = (float)(Imax / Math.Sqrt(2));
                }
                else if (xIb.ToLower().IndexOf("imax-ib") != -1)
                {
                    if (xIb.ToLower() == "imax-ib")
                    {
                        FloatCurrent = Imax - Ib;
                    }
                    else
                    {
                        FloatCurrent = (Imax - Ib) * 0.5f;
                    }
                }
                else
                {
                    FloatCurrent = float.Parse(xIb.ToLower().Replace("imax", "")) * Imax;
                }
            }
            else if (xIb.ToLower().IndexOf("ib") != -1)
            {
                if (xIb.ToLower() == "ib")
                {
                    FloatCurrent = Ib;
                }
                else if (xIb.ToLower().IndexOf("imax-ib") != -1)
                {
                    if (xIb.ToLower() == "imax-ib")
                    {
                        FloatCurrent = Imax - Ib;
                    }
                    else
                    {
                        FloatCurrent = (Imax - Ib) * 0.5f;
                    }
                }
                else
                {
                    FloatCurrent = float.Parse(xIb.ToLower().Replace("ib", "")) * Ib;
                }
            }

            return FloatCurrent;
        }


        /// <summary>
        /// 电子式B级
        /// </summary>
        /// <param name="xIb"></param>
        /// <param name="glys"></param>
        /// <param name="Hgq"></param>
        /// <returns></returns>
        private static string GetdzB(string xIb, bool hgq)
        {
            if (xIb.ToLower() == "ib") xIb = "1.0ib";
            float FloatCurrent = GetFloatCurrent(xIb, "B", hgq, out float Imin, out float Itr, out float Imax, out float Ib);
            string wcLimit;
            if (FloatCurrent >= Itr && FloatCurrent <= Imax)
            {
                wcLimit = "1.0";
            }
            else if (FloatCurrent >= Imin && FloatCurrent < Itr)
            {
                wcLimit = "1.5";
            }
            else
            {
                wcLimit = (1.5 * (Imin / FloatCurrent)).ToString();
            }

            if (xIb.ToLower().IndexOf("ib") != -1)
            {
                if (FloatCurrent >= Ib && FloatCurrent <= Imax)
                {
                    wcLimit = "1.0";
                }
                else
                {
                    wcLimit = "1.5";
                }
            }
            return wcLimit;
        }

        /// <summary>
        /// 电子式C级
        /// </summary>
        /// <param name="xIb"></param>
        /// <param name="glys"></param>
        /// <param name="Hgq"></param>
        /// <returns></returns>
        private static string GetdzC(string xIb, string glys, bool hgq)
        {
            if (xIb.ToLower() == "ib") xIb = "1.0ib";
            float FloatCurrent = GetFloatCurrent(xIb, "C", hgq, out float Imin, out float Itr, out _, out float Ib);
            string wcLimit;
            if (FloatCurrent >= Itr)
            {
                if (glys == "1.0")
                {
                    wcLimit = "0.5";
                }
                else
                { wcLimit = "0.6"; }
            }
            else if (FloatCurrent >= Imin && FloatCurrent < Itr)
            {
                wcLimit = "1.0";
            }
            else
            {
                wcLimit = (1.0 * (Imin / FloatCurrent)).ToString();
            }

            if (xIb.ToLower().IndexOf("ib") != -1)
            {
                if (FloatCurrent >= Ib)
                {
                    if (glys == "1.0")
                    {
                        wcLimit = "0.5";
                    }
                    else
                    { wcLimit = "0.6"; }
                }
                else
                {
                    wcLimit = "1.0";
                }
            }

            return wcLimit;
        }

        /// <summary>
        /// 电子式D级
        /// </summary>
        /// <param name="xIb"></param>
        /// <param name="glys"></param>
        /// <param name="Hgq"></param>
        /// <returns></returns>
        private static string GetdzD(string xIb,  string glys, bool hgq)
        {
            if (xIb.ToLower() == "ib") xIb = "1.0ib";
            float FloatCurrent = GetFloatCurrent(xIb, "D", hgq, out float Imin, out float Itr, out float Imax, out float Ib);
            string wcLimit;
            if (FloatCurrent >= Itr && FloatCurrent <= Imax)
            {
                if (glys == "1.0")
                {
                    wcLimit = "0.2";
                }
                else
                { wcLimit = "0.3"; }
            }
            else if (FloatCurrent >= Imin && FloatCurrent < Itr)
            {
                if (glys == "1.0")
                {
                    wcLimit = "0.4";
                }
                else
                { wcLimit = "0.5"; }
            }
            else
            {
                wcLimit = (0.4 * (Imin / FloatCurrent)).ToString();
            }

            if (xIb.ToLower().IndexOf("ib") != -1)
            {
                if (FloatCurrent >= Ib && FloatCurrent <= Imax)
                {
                    if (glys == "1.0")
                    {
                        wcLimit = "0.2";
                    }
                    else
                    { wcLimit = "0.3"; }
                }
                else
                {
                    if (glys == "1.0")
                    {
                        wcLimit = "0.4";
                    }
                    else
                    { wcLimit = "0.5"; }
                }
            }
            return wcLimit;
        }

        /// <summary>
        /// 获取感应式电能表的误差限
        /// </summary>
        /// <param name="xIb">电流倍数</param>
        /// <param name="GuiChengName">规程名称</param>
        /// <param name="Dj">等级</param>
        /// <param name="Yj">元件</param>
        /// <param name="glys">功率因数</param>
        /// <param name="Hgq">互感器</param>
        /// <param name="YouGong">有无功</param>
        /// <returns></returns>
        private static string GetGy(string xIb, string GuiChengName, string Dj, Enum.Cus_PowerYuanJian Yj, string glys, bool Hgq, bool YouGong)
        {
            if (xIb.ToLower() == "ib") xIb = "1.0ib";

            string wcLimit;
            if (YouGong)        //有功
            {
                if ((int)Yj == 1)      //合元
                {
                    if (glys == "1.0")
                    {
                        wcLimit = Dj;
                        if (GuiChengName.ToUpper() == "JJG307-2006" && Hgq)
                        {
                            if (xIb.ToLower().IndexOf("imax") == -1 && xIb.ToLower().IndexOf("imax-ib") == -1 && float.Parse(xIb.ToLower().Replace("ib", "")) < 0.05F)
                                wcLimit = (float.Parse(wcLimit) + 0.5F).ToString();
                        }
                        else if (xIb.ToLower().IndexOf("imax") == -1 && xIb.ToLower().IndexOf("imax-ib") == -1 && float.Parse(xIb.ToLower().Replace("ib", "")) < 0.1F)
                            wcLimit = (float.Parse(wcLimit) + 0.5F).ToString();
                    }
                    else if (glys.ToUpper() == "0.5C")
                    {
                        switch (Dj)
                        {
                            case "0.5":
                                wcLimit = "1.5";
                                break;
                            case "1.0":
                                wcLimit = "2.5";
                                break;
                            case "2.0":
                                wcLimit = "3.5";
                                break;
                            default:
                                wcLimit = "3.5";
                                break;
                        }
                    }
                    else if (glys.ToUpper() == "0.25L")
                    {
                        switch (Dj)
                        {
                            case "0.5":
                                wcLimit = "2.5";
                                break;
                            case "1.0":
                                wcLimit = "3.5";
                                break;
                            case "2.0":
                                wcLimit = "4.5";
                                break;
                            default:
                                wcLimit = "4.5";
                                break;
                        }
                    }
                    else
                    {
                        switch (Dj)
                        {
                            case "0.5":
                                wcLimit = "0.8";
                                break;
                            case "1.0":
                                wcLimit = "1.0";
                                break;
                            case "2.0":
                                wcLimit = "2.0";
                                break;
                            default:
                                wcLimit = "2.0";
                                break;
                        }
                        if (GuiChengName.ToUpper() == "JJG307-2006" && Hgq)
                        {
                            if (xIb.ToLower().IndexOf("imax") == -1 && xIb.ToLower().IndexOf("imax-ib") == -1 && float.Parse(xIb.ToLower().Replace("ib", "")) < 0.1F)
                                wcLimit = (float.Parse(wcLimit) + 0.5F).ToString();
                        }
                        else if (xIb.ToLower().IndexOf("imax") == -1 && xIb.ToLower().IndexOf("imax-ib") == -1 && float.Parse(xIb.ToLower().Replace("ib", "")) < 0.2F)
                            wcLimit = (float.Parse(wcLimit) + 0.5F).ToString();
                    }
                }
                else
                {
                    switch (Dj)
                    {
                        case "0.5":
                            wcLimit = "1.5";
                            break;
                        case "1.0":
                            wcLimit = "2.0";
                            break;
                        case "2.0":
                            wcLimit = "2.0";
                            break;
                        default:
                            wcLimit = "3.0";
                            break;
                    }
                    if (GuiChengName.ToUpper() == "JJG307-1988")
                        if (xIb.ToLower().IndexOf("imax") >= 0 || xIb.ToLower().IndexOf("imax-ib") >= 0 || float.Parse(xIb.ToLower().Replace("ib", "")) > 1F)
                            wcLimit = (float.Parse(wcLimit) + 1F).ToString();
                }
            }
            else//无功
            {
                if ((int)Yj == 1)           //合元
                {
                    if (glys == "1.0")
                    {
                        wcLimit = Dj;
                        if (GuiChengName.ToUpper() == "JJG307-2006" && Hgq)
                        {
                            if (xIb.ToLower().IndexOf("imax") == -1 && xIb.ToLower().IndexOf("imax-ib") == -1 && float.Parse(xIb.ToLower().Replace("ib", "")) < 0.1F)
                                wcLimit = (float.Parse(wcLimit) + 1.0F).ToString();
                        }
                        else if (xIb.ToLower().IndexOf("imax") == -1 && xIb.ToLower().IndexOf("imax-ib") == -1 && float.Parse(xIb.ToLower().Replace("ib", "")) < 0.2F)
                            wcLimit = (float.Parse(wcLimit) + 1.0F).ToString();
                    }
                    else if (glys.ToUpper() == "0.25C" || glys.ToUpper() == "0.25L")
                    {
                        wcLimit = (float.Parse(Dj) * 2F).ToString();
                    }
                    else
                    {
                        if (xIb.ToLower().IndexOf("imax") == -1 && xIb.ToLower().IndexOf("imax-ib") == -1 && float.Parse(xIb.ToLower().Replace("ib", "")) < 0.5F)
                            wcLimit = (float.Parse(Dj) + 2.0F).ToString();
                        else
                            wcLimit = Dj;

                        if (GuiChengName.ToUpper() == "JJG307-2006")
                        {
                            if (Hgq)
                            {
                                if (xIb.ToLower().IndexOf("imax") == -1 && xIb.ToLower().IndexOf("imax-ib") == -1 && float.Parse(xIb.ToLower().Replace("ib", "")) < 0.2F)
                                    wcLimit = (float.Parse(wcLimit) - 1F).ToString();
                                else if (xIb.ToLower().IndexOf("imax") == -1 && xIb.ToLower().IndexOf("imax-ib") == -1 && float.Parse(xIb.ToLower().Replace("ib", "")) < 0.5F)
                                    wcLimit = (float.Parse(wcLimit) - 2F).ToString();
                            }
                            else if (xIb.ToLower().IndexOf("imax") == -1 && xIb.ToLower().IndexOf("imax-ib") == -1 && float.Parse(xIb.ToLower().Replace("ib", "")) < 0.5F)
                                wcLimit = (float.Parse(wcLimit) - 1F).ToString();
                        }
                    }
                }
                else
                    wcLimit = (float.Parse(Dj) + 1.0F).ToString();
            }

            return wcLimit;

        }
        #endregion

    }
}
