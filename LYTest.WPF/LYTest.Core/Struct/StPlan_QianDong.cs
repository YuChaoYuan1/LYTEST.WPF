using LYTest.Core.Enum;
using LYTest.Core.Function;
using System;

namespace LYTest.Core.Struct
{
    /// <summary>
    /// 潜动项目结构
    /// </summary>
    [Serializable()]
    public class StPlan_QianDong
    {
        /// <summary>
        /// 项目编号
        /// </summary>
        public string ItemId;

        /// <summary>
        /// <summary>
        /// 获取潜动时间和潜动电流
        /// </summary>
        /// <param name="GuiChengName">规程名称</param>
        /// <param name="clfs">测量方式</param>
        /// <param name="U">电压</param>
        /// <param name="Ib">电流 Ib(Imax)</param>
        /// <param name="Dj">等级 有功（无功）</param>
        /// <param name="MeterConst">常数 有功（无功）</param>
        /// <param name="znq">止逆器</param>
        /// <param name="hgq">接线方式,直接,间接</param>   
        /// <param name="IbX">电流倍数</param>   
        /// <param name="UbX">电压倍数</param>   
        /// <param name="PowerWay">功率方向</param> 
        /// <param name="TimeX">潜动时间</param> 
        /// <param name="CreepIb">返回的潜动电流</param> 
        /// <param name="CheckTime">返回的潜动时间</param> 
        public static void CheckTimeAndIb(string GuiChengName, WireMode clfs, float U, string Ib, string Dj, string MeterConst, bool znq, bool hgq,
            float IbX, float UbX, PowerWay PowerWay, float TimeX, ref float CreepIb, ref float CheckTime)
        {
            float ib = Number.GetCurrentByIb("Ib", Ib, hgq);

            if (ib == 0F)
            {
                ib = float.Parse(Ib.Substring(0, Ib.IndexOf("(")));
            }
            float qIb;
            if ("IR46" == GuiChengName || GuiChengName == "Q/GDW10827-2020" || GuiChengName == "Q/GDW10364-2020")
            {
                qIb = 0;
            }
            else if ("JJG596-2012" == GuiChengName)
            {
                qIb = 0;
            }
            else
            {
                qIb = GetQiDongCurrent(GuiChengName, ib, Dj, znq, hgq, PowerWay);
            }

            if (IbX == 0F)
            {
                CreepIb = 0F;
            }
            else
            {
                CreepIb = IbX * qIb;
            }

            if (TimeX == 0)
            {
                if ("IR46" == GuiChengName || GuiChengName == "Q/GDW10827-2020" || GuiChengName == "Q/GDW10364-2020")
                    //2021-4-25 更改Function.Number.GetCurrentByIb("Imax", Ib) 为 Function.Number.GetCurrentByIb("Imin", Ib)
                    CheckTime = GetQianDongTime(GuiChengName, U, UbX, Number.GetCurrentByIb("Imin", Ib, hgq), Dj, Number.GetBcs(MeterConst, PowerWay), clfs, PowerWay);
                else if ("JJG596-2012" == GuiChengName)
                    CheckTime = GetQianDongTime(GuiChengName, U, UbX, Number.GetCurrentByIb("Imax", Ib, hgq), Dj, Number.GetBcs(MeterConst, PowerWay), clfs, PowerWay);
                else if ("Q/GDW12175-2021" == GuiChengName)
                    CheckTime = GetQianDongTime(GuiChengName, U, UbX, Number.GetCurrentByIb("Imin", Ib, hgq), Dj, Number.GetBcs(MeterConst, PowerWay), clfs, PowerWay);
                else
                    CheckTime = GetQianDongTime(GuiChengName, U, UbX, qIb, Dj, Number.GetBcs(MeterConst, PowerWay), clfs, PowerWay);
            }
            else
            {
                CheckTime = TimeX;
            }

        }




        #region Function
        /// <summary>
        /// 获取起动电流
        /// </summary>
        /// <param name="GuiChengName">规程名称(JJG307-1988)</param>
        /// <param name="Dl">标定电流值</param>
        /// <param name="MeterLevel">表等级【有功等级（无功等级）】</param>
        /// <param name="Znq">止逆器</param>
        /// <param name="Hgq">互感器</param>
        /// <param name="pw">有无功</param>
        /// <returns>起动电流值</returns>
        public static float GetQiDongCurrent(string GuiChengName, float Dl, string MeterLevel, bool Znq, bool Hgq, PowerWay pw)
        {
            string[] dj = Number.GetDj(MeterLevel); //等级

            if (pw == PowerWay.正向有功 || pw == PowerWay.反向有功)
                MeterLevel = dj[0].Trim();
            else
                MeterLevel = dj[1].Trim();

            float ibX;
            switch (GuiChengName.ToUpper())
            {
                case "JJG307-1988":
                    {
                        #region
                        switch (MeterLevel)
                        {
                            case "0.1":
                                {
                                    ibX = 0.002F;
                                    break;
                                }
                            case "0.2":
                                {
                                    ibX = 0.0025F;
                                    break;
                                }
                            case "0.5":
                                {
                                    if (Znq)
                                        ibX = 0.008F;
                                    else
                                        ibX = 0.003F;
                                    break;
                                }
                            case "1.0":
                                {
                                    if (Znq)
                                        ibX = 0.009F;
                                    else
                                        ibX = 0.004F;
                                    break;
                                }
                            case "2.0":
                                {
                                    if (!Znq)
                                        ibX = 0.005F;
                                    else
                                        ibX = 0.01F;
                                    break;
                                }
                            case "3.0":
                                {
                                    if (!Znq)
                                        ibX = 0.01F;
                                    else
                                        ibX = 0.015F;
                                    break;
                                }
                            default:
                                ibX = 0.002F;
                                break;
                        }
                        #endregion
                        break;
                    }
                case "JJG307-2006":
                    {
                        #region
                        switch (MeterLevel)
                        {
                            case "0.5":
                                {
                                    ibX = 0.002F;
                                    break;
                                }
                            case "1.0":
                                {
                                    if (Znq)
                                        ibX = 0.003F;
                                    else if (Hgq)
                                        ibX = 0.002F;
                                    else
                                        ibX = 0.005F;
                                    break;
                                }
                            case "2.0":
                                {

                                    if (!Znq)
                                        if (Hgq)
                                            ibX = 0.003F;
                                        else
                                            ibX = 0.005F;
                                    else if (Hgq)
                                        ibX = 0.003F;
                                    else
                                        ibX = 0.005F;
                                    break;
                                }
                            case "3.0":
                                {
                                    if (Hgq)
                                        ibX = 0.005F;
                                    else
                                        ibX = 0.01F;
                                    break;
                                }
                            default:
                                ibX = 0.002F;
                                break;
                        }
                        #endregion
                        break;
                    }
                case "JJG596-1999":
                    {
                        #region
                        switch (MeterLevel)
                        {
                            case "0.02":
                                {
                                    ibX = 0.0002F;
                                    break;
                                }
                            case "0.05":
                                {
                                    ibX = 0.0005F;
                                    break;
                                }
                            case "0.1":
                                {
                                    ibX = 0.001F;
                                    break;
                                }
                            case "0.2":
                                {
                                    ibX = 0.001F;
                                    break;
                                }
                            case "0.5":
                                {
                                    ibX = 0.001F;
                                    break;
                                }
                            case "1.0":
                                {
                                    if (Hgq)
                                        ibX = 0.002F;
                                    else
                                        ibX = 0.004F;
                                    break;
                                }
                            case "2.0":
                                {
                                    if (Hgq)
                                        ibX = 0.003F;
                                    else
                                        ibX = 0.005F;
                                    break;
                                }
                            default:
                                ibX = 0.0002F;
                                break;

                        }
                        #endregion
                        break;
                    }
                case "JJG596-2012":
                    {
                        #region
                        switch (MeterLevel)
                        {
                            case "0.2":
                            case "0.5":
                            case "0.2S":
                            case "0.5S":
                                {
                                    ibX = 0.001F;
                                    break;
                                }
                            case "1":
                            case "1.0":
                                {
                                    if (!Hgq)
                                        ibX = 0.004F;
                                    else
                                        ibX = 0.002F;
                                    break;
                                }
                            case "2":
                            case "2.0":
                                {
                                    if (!Hgq)
                                        ibX = 0.005F;
                                    else
                                        ibX = 0.003F;
                                    break;
                                }
                            case "3.0":
                                {
                                    if (!Hgq)
                                        ibX = 0.01F;
                                    else
                                        ibX = 0.005F;
                                    break;
                                }
                            default:
                                ibX = 0.002F;
                                break;

                        }
                        #endregion
                        break;
                    }
                case "IR46":  //modey zjl20200806
                    {
                        #region
                        switch (MeterLevel)
                        {
                            case "1.0":

                                {
                                    ibX = 0.05F;
                                    break;
                                }

                            case "2.0":
                                {

                                    ibX = 0.04F;
                                    break;
                                }

                            case "3.0":
                            case "4.0":
                                {
                                    if (!Hgq)
                                        ibX = 0.004F;
                                    else
                                        ibX = 0.002F;
                                    break;
                                }
                            //2021-4-24 添加
                            //case "C":
                            case "A":
                                {
                                    ibX = 0.04F;
                                    break;
                                }
                            case "B":
                                {
                                    ibX = 0.04F;
                                    break;
                                }
                            case "C":
                                {
                                    if (!Hgq)
                                        ibX = 0.04F;
                                    else
                                        ibX = 0.02F;
                                    break;
                                }
                            case "D":
                                {
                                    if (!Hgq)
                                        ibX = 0.04F;
                                    else
                                        ibX = 0.02F;
                                    break;
                                }
                            default:
                                ibX = 0.04F;
                                break;

                        }
                        #endregion
                        break;
                    }
                default:
                    {
                        ibX = 0;
                        break;
                    }

            }

            return ibX * Dl;
        }


        /// <summary>
        /// 获取起动时间,min
        /// </summary>
        /// <param name="GuiChengName">规程名称(JJG307-1988)</param>
        /// <param name="U">标定电压</param>
        /// <param name="qIb">起动电流值</param>
        /// <param name="meterConst">表常数</param>
        /// <param name="_Clfs">测量方式</param>
        /// <returns></returns>
        public static float GetQiDongTime(string GuiChengName, float U, float qIb, int meterConst, WireMode _Clfs)
        {
            float _m = 0;
            switch (_Clfs)
            {
                case WireMode.单相:
                    {
                        _m = 1F;
                        break;
                    }
                case WireMode.三相四线:
                    {
                        _m = 3F;
                        break;
                    }
                case WireMode.二元件跨相60:
                case WireMode.二元件跨相90:
                case WireMode.三元件跨相90:
                case WireMode.三相三线:
                    {
                        _m = (float)Math.Sqrt(3D);
                        break;
                    }
            }

            switch (GuiChengName.ToUpper())
            {
                case "JJG307-1988":
                    {
                        return 1.4F * 60F * 1000F / (meterConst * _m * U * qIb);
                    }
                case "JJG307-2006":
                    {
                        return 80F * 1000F / (meterConst * _m * U * qIb);
                    }
                case "JJG596-1999":
                    {
                        return 1.4F * 60F * 1000F / (meterConst * _m * U * qIb);
                    }
                case "JJG596-2012":
                    {
                        return 1.2F * 60F * 1000F / (meterConst * _m * U * qIb);
                    }
                case "IR46":
                    {
                        return 3.6F * 1000000F / (meterConst * _m * U * qIb) / 60;
                    }
                default:
                    {
                        return 1.2F * 60F * 1000F / (meterConst * _m * U * qIb);
                    }
            }
        }

        /// <summary>
        /// 计算潜动时间,min
        /// </summary>
        /// <param name="GuiChengName">规程名称</param>
        /// <param name="U">标定电压</param>
        /// <param name="xU">电压倍数</param> 
        /// <param name="qIb">起动电流</param>
        /// <param name="MeterLevel">电能表常数</param>
        /// <param name="MeterConst">测量方式</param>
        /// <param name="wireMode">测量方式</param>
        /// <param name="pw">测量方式</param>
        /// <returns></returns>
        public static float GetQianDongTime(string GuiChengName, float U, float xU, float qIb, string MeterLevel, int MeterConst, WireMode wireMode, PowerWay pw)
        {
            float m = 0;
            switch (wireMode)
            {
                case WireMode.单相:
                    m = 1F;
                    break;
                case WireMode.三相四线:
                    m = 3F;
                    break;
                case WireMode.二元件跨相60:
                case WireMode.二元件跨相90:
                case WireMode.三元件跨相90:
                case WireMode.三相三线:
                    m = (float)Math.Sqrt(3D);
                    break;
            }
            string[] levels = Number.GetDj(MeterLevel);

            if (pw == PowerWay.正向有功 || pw == PowerWay.反向有功)
            {
                MeterLevel = levels[0].Trim();
            }
            else
            {
                MeterLevel = levels[1].Trim();
            }
            float fbase = 900;
            switch (MeterLevel)
            {
                case "0.2S":
                case "0.2":
                case "D":
                    fbase = 900F;
                    break;
                case "0.5S":
                case "0.5":
                case "C":
                case "1.0":
                case "1":
                case "B":
                    fbase = 600F;
                    break;
                case "A":
                case "2.0":
                case "2":
                    fbase = 480F;
                    break;
                default:
                    break;
            }
            switch (GuiChengName.ToUpper())
            {
                case "JJG307-1988":
                    return 10F * (GetQiDongTime(GuiChengName, U * xU, qIb, MeterConst, wireMode));
                case "JJG307-2006":
                    return 20F * 1000F / (MeterConst * m * U * xU * qIb * 0.25F);
                case "JJG596-1999":
                    return 10F * (GetQiDongTime(GuiChengName, U * xU, qIb, MeterConst, wireMode));
                case "JJG596-2012":
                    return fbase * 1000000F / (MeterConst * m * U * qIb);
                case "IR46":

                    float Errb = 0;
                    switch (MeterLevel)
                    {
                        case "3":
                        case "3.0":
                            Errb = 3.0F;
                            break;
                        case "A":
                        case "2":
                        case "2.0":
                            Errb = 2.5F;
                            break;
                        case "B":
                        case "1":
                        case "1S":
                        case "1.0":
                        case "1.0S":
                            Errb = 1.5F;
                            break;
                        case "C":
                        case "0.5":
                        case "0.5S":
                            Errb = 1.0F;
                            break;
                        case "D":
                        case "0.2":
                        case "0.2S":
                            Errb = 0.4F;
                            break;
                        case "E":
                        case "0.1":
                            Errb = 0.2F;
                            break;
                        default:
                            break;
                    }
                    return 100 * 1000F / (1.1F * Errb * MeterConst * m * U * qIb) * 60;
                case "Q/GDW12175-2021":
                case "Q/GDW10827-2020":
                case "Q/GDW10364-2020":
                    float Errb12175 = 0;
                    switch (MeterLevel)
                    {
                        case "A":
                        case "2":
                        case "2.0":
                        case "B":
                        case "1":
                        case "1S":
                        case "1.0":
                        case "1.0S":
                            Errb12175 = 1.5F;
                            break;
                        case "C":
                        case "0.5":
                        case "0.5S":
                            Errb12175 = 1.0F;
                            break;
                        case "D":
                        case "0.2":
                        case "0.2S":
                            Errb12175 = 0.4F;
                            break;
                    }
                    return 100 * 1000F / (1.1F * Errb12175 * MeterConst * m * U * qIb) * 60;
                default:
                    return 10F * (GetQiDongTime(GuiChengName, U * xU, qIb, MeterConst, wireMode));
            }
        }




        #endregion
    }
}
