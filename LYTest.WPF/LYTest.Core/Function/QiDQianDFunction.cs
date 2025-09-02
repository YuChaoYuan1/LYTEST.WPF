using System;

namespace LYTest.Core.Function
{
    /// <summary>
    /// 起动潜动相关计算函数
    /// </summary>
    public static class QiDQianDFunction
    {
        ///// <summary>
        ///// 海南获取起动电流
        ///// </summary>
        ///// <param name="ib">标定电流</param>
        ///// <param name="MeterLevel">表等级</param>
        ///// <param name="Hgq">互感器</param>
        ///// <param name="ywg">是否有功</param>
        ///// <returns></returns>
        //public static float GetQiDongCurrent(float ib, string MeterLevel, bool Hgq, Enum.Cus_Ywg ywg)
        //{
        //    string[] _Dj = Function.Number.GetDj(MeterLevel);

        //    if (ywg == Enum.Cus_Ywg.P)
        //    {
        //        MeterLevel = float.Parse(_Dj[0]).ToString("F1");
        //    }
        //    else
        //    {
        //        MeterLevel = float.Parse(_Dj[1]).ToString("F1");
        //    }

        //    float _xIb;
        //    switch (MeterLevel.ToLower().Replace("s", ""))
        //    {
        //        case "0.02":
        //            _xIb = 0.0002F;
        //            break;
        //        case "0.05":
        //            _xIb = 0.0005F;
        //            break;
        //        case "0.1":
        //            _xIb = 0.001F;
        //            break;
        //        case "0.2":
        //            _xIb = 0.001F;
        //            break;
        //        case "0.5":
        //            _xIb = 0.001f;
        //            break;
        //        case "1.0":
        //            _xIb = Hgq ? 0.002f : 0.004f;
        //            break;
        //        case "2.0":
        //            _xIb = Hgq ? 0.003F : 0.005F;
        //            break;
        //        default:
        //            _xIb = 0.002f;
        //            break;
        //    }
        //    return _xIb * ib;

        //}
        ///// <summary>
        ///// 海南获取潜动电流
        ///// </summary>
        ///// <param name="ib"></param>
        ///// <param name="hgq"></param>
        ///// <returns></returns>
        //public static float GetQianDongCurrent(float ib, bool hgq)
        //{
        //    return hgq ? 0.0002f * ib : 0f;
        //}


        /// <summary>
        /// 海南获取起动时间
        /// </summary>
        /// <param name="U">标定电压</param>
        /// <param name="qIb">起动电流值</param>
        /// <param name="MeterConst">表常数</param>
        /// <param name="_Clfs">测量方式</param>
        /// <returns></returns>
        public static float GetQiDongTime(float U, float qIb, int MeterConst, Enum.WireMode _Clfs)
        {
            float _m = 0;
            switch (_Clfs)
            {
                case Enum.WireMode.单相:
                    {
                        _m = 1F;
                        break;
                    }
                case Enum.WireMode.三相四线:
                    {
                        _m = 3F;
                        break;
                    }
                case Enum.WireMode.二元件跨相60:
                case Enum.WireMode.二元件跨相90:
                case Enum.WireMode.三元件跨相90:
                case Enum.WireMode.三相三线:
                    {
                        _m = (float)Math.Sqrt(3D);
                        break;
                    }
            }


            return 1.2F * 60F * 1000F / ((float)MeterConst * _m * U * qIb);

        }

        ///// <summary>
        ///// 海南获取潜动时间
        ///// </summary>
        ///// <param name="U"></param>
        ///// <param name="xU"></param>
        ///// <param name="qIb"></param>
        ///// <param name="imax"></param>
        ///// <param name="meterLevel"></param>
        ///// <param name="MeterConst"></param>
        ///// <param name="_Clfs"></param>
        ///// <param name="hgq"></param>
        ///// <param name="_Ywg"></param>
        ///// <returns></returns>
        //public static float GetQianDongTime(float U, float qIb, float imax, string meterLevel, int MeterConst, Enum.WireMode _Clfs, bool hgq, Enum.Cus_Ywg _Ywg)
        //{
        //    float xU = 1.15f;
        //    if (hgq)
        //    {
        //        return 5 * GetQiDongTime(xU * U, qIb, MeterConst, _Clfs);
        //    }

        //    float _m = 0;
        //    switch (_Clfs)
        //    {
        //        case Enum.WireMode.单相:
        //            {
        //                _m = 1F;
        //                break;
        //            }
        //        case Enum.WireMode.三相四线:
        //            {
        //                _m = 3F;
        //                break;
        //            }
        //        case Enum.WireMode.二元件跨相60:
        //        case Enum.WireMode.二元件跨相90:
        //        case Enum.WireMode.三元件跨相90:
        //        case Enum.WireMode.三相三线:
        //            {
        //                _m = (float)Math.Sqrt(3D);
        //                break;
        //            }
        //    }

        //    string[] _Dj = Function.Number.GetDj(meterLevel);

        //    if (_Ywg == Enum.Cus_Ywg.P)
        //    {
        //        meterLevel = float.Parse(_Dj[0]).ToString("F1");
        //    }
        //    else
        //    {
        //        meterLevel = float.Parse(_Dj[1]).ToString("F1");
        //    }
        //    int cs = meterLevel == "1.0" ? 600 : 480;

        //    return cs * (float)Math.Pow(10, 6) / (MeterConst * _m * U * xU * imax);
        //}

        /// <summary>
        /// 获取起动电流
        /// </summary>
        /// <param name="GuiChengName">规程名称(JJG307-1988)</param>
        /// <param name="Dl">标定电流值</param>
        /// <param name="MeterLevel">表等级【有功等级（无功等级）】</param>
        /// <param name="Znq">止逆器</param>
        /// <param name="Hgq">互感器</param>
        /// <param name="_Ywg">有无功</param>
        /// <returns>起动电流值</returns>
        public static float GetQiDongCurrent(string GuiChengName, float Dl, string MeterLevel, bool Znq, bool Hgq, Enum.Cus_Ywg _Ywg)
        {
            string[] _Dj = Function.Number.GetDj(MeterLevel);

            if (_Ywg == Enum.Cus_Ywg.P)
            {
                MeterLevel = _Dj[0].Trim();
            }
            else
            {
                MeterLevel = _Dj[1].Trim();
            }

            float _xIb;
            switch (GuiChengName.ToUpper())
            {
                case "JJG307-1988":
                    {
                        #region
                        switch (MeterLevel)
                        {
                            case "0.1":
                                {
                                    _xIb = 0.002F;
                                    break;
                                }
                            case "0.2":
                                {
                                    _xIb = 0.0025F;
                                    break;
                                }
                            case "0.5":
                                {
                                    if (Znq)
                                        _xIb = 0.008F;
                                    else
                                        _xIb = 0.003F;
                                    break;
                                }
                            case "1.0":
                                {
                                    if (!Znq)
                                        _xIb = 0.004F;
                                    else
                                        _xIb = 0.009F;
                                    break;
                                }
                            case "2.0":
                                {
                                    if (!Znq)
                                        _xIb = 0.005F;
                                    else
                                        _xIb = 0.01F;
                                    break;
                                }
                            case "3.0":
                                {
                                    if (!Znq)
                                        _xIb = 0.01F;
                                    else
                                        _xIb = 0.015F;
                                    break;
                                }
                            default:
                                _xIb = 0.002F;
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
                                    _xIb = 0.002F;
                                    break;
                                }
                            case "1.0":
                                {
                                    if (Znq)
                                        _xIb = 0.003F;
                                    else if (Hgq)
                                        _xIb = 0.002F;
                                    else
                                        _xIb = 0.005F;
                                    break;
                                }
                            case "2.0":
                                {

                                    if (!Znq)
                                        if (Hgq)
                                            _xIb = 0.003F;
                                        else
                                            _xIb = 0.005F;
                                    else
                                        if (Hgq)
                                        _xIb = 0.003F;
                                    else
                                        _xIb = 0.005F;
                                    break;
                                }
                            case "3.0":
                                {
                                    if (Hgq)
                                        _xIb = 0.005F;
                                    else
                                        _xIb = 0.01F;
                                    break;
                                }
                            default:
                                _xIb = 0.002F;
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
                                    _xIb = 0.0002F;
                                    break;
                                }
                            case "0.05":
                                {
                                    _xIb = 0.005F;
                                    break;
                                }
                            case "0.1":
                                {
                                    _xIb = 0.001F;
                                    break;
                                }
                            case "0.2":
                                {
                                    _xIb = 0.001F;
                                    break;
                                }
                            case "0.5":
                                {
                                    _xIb = 0.001F;
                                    break;
                                }
                            case "1.0":
                                {
                                    if (!Hgq)
                                        _xIb = 0.004F;
                                    else
                                        _xIb = 0.002F;
                                    break;
                                }
                            case "2.0":
                                {
                                    if (!Hgq)
                                        _xIb = 0.005F;
                                    else
                                        _xIb = 0.003F;
                                    break;
                                }
                            default:
                                _xIb = 0.0002F;
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
                            case "D":
                            case "0.2":
                            case "0.2S":
                            case "C":
                            case "0.5":
                            case "0.5S":
                                {
                                    _xIb = 0.001F;
                                    break;
                                }
                            case "B":
                            case "1":
                            case "1.0":
                                {
                                    if (!Hgq)
                                        _xIb = 0.004F;
                                    else
                                        _xIb = 0.002F;
                                    break;
                                }
                            case "A":
                            case "2":
                            case "2.0":
                                {
                                    if (!Hgq)
                                        _xIb = 0.005F;
                                    else
                                        _xIb = 0.003F;
                                    break;
                                }
                            case "3.0":
                                {
                                    if (!Hgq)
                                        _xIb = 0.01F;
                                    else
                                        _xIb = 0.005F;
                                    break;
                                }
                            default:
                                _xIb = 0.002F;
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
                            case "A":
                            case "2":
                            case "2.0":
                                {
                                    _xIb = 0.05F;
                                    break;
                                }
                            case "B":
                            case "1":
                            case "1.0":
                                {
                                    _xIb = 0.04F;
                                    break;
                                }
                            case "C":
                            case "0.5":
                            case "0.5S":
                                {
                                    if (!Hgq)
                                        _xIb = 0.04F;
                                    else
                                        _xIb = 0.02F;
                                    break;
                                }
                            case "D":
                            case "0.2":
                            case "0.2S":
                                {
                                    if (!Hgq)
                                        _xIb = 0.04F;
                                    else
                                        _xIb = 0.02F;
                                    break;
                                }
                            case "E":
                            case "0.1":
                            case "0.1S":
                                _xIb = 0.02F;
                                break;
                            default:
                                _xIb = 0.04F;
                                break;

                        }
                        #endregion
                        break;
                    }
                case "Q/GDW10827-2020":
                case "Q/GDW10364-2020":
                case "Q/GDW12175-2021":
                    {
                        #region
                        switch (MeterLevel)
                        {
                            case "A":
                            case "B":
                                {
                                    _xIb = 0.04F;
                                    break;
                                }
                            case "C":
                                {
                                    if (!Hgq)
                                        _xIb = 0.04F;
                                    else
                                        _xIb = 0.02F;
                                    break;
                                }
                            case "D":
                                {
                                    if (!Hgq)
                                        _xIb = 0.04F;
                                    else
                                        _xIb = 0.02F;
                                    break;
                                }
                            case "E":
                                _xIb = 0.02F;
                                break;
                            default:
                                _xIb = 0.04F;
                                break;

                        }
                        #endregion
                        break;
                    }
                default:
                    {
                        _xIb = 0;
                        break;
                    }

            }

            return _xIb * Dl;
        }

        /// <summary>
        /// 获取起动时间,秒
        /// </summary>
        /// <param name="GuiChengName">规程名称(JJG307-1988)</param>
        /// <param name="U">标定电压</param>
        /// <param name="qIb">起动电流值</param>
        /// <param name="MeterConst">表常数</param>
        /// <param name="_Clfs">测量方式</param>
        /// <returns></returns>
        public static float GetQiDongTime(string GuiChengName, float U, float qIb, int MeterConst, Enum.WireMode _Clfs)
        {
            float _m = 0;
            switch (_Clfs)
            {
                case Enum.WireMode.单相:
                    {
                        _m = 1F;
                        break;
                    }
                case Enum.WireMode.三相四线:
                    {
                        _m = 3F;
                        break;
                    }
                case Enum.WireMode.二元件跨相60:
                case Enum.WireMode.二元件跨相90:
                case Enum.WireMode.三元件跨相90:
                case Enum.WireMode.三相三线:
                    {
                        _m = (float)Math.Sqrt(3D);
                        break;
                    }
            }

            switch (GuiChengName.ToUpper())
            {
                case "JJG307-1988":
                    {
                        return 1.4F * 60F * 1000F / ((float)MeterConst * _m * U * qIb) * 60;
                    }
                case "JJG307-2006":
                    {
                        return 80F * 1000F / ((float)MeterConst * _m * U * qIb) * 60;
                    }
                case "JJG596-1999":
                    {
                        return 1.4F * 60F * 1000F / ((float)MeterConst * _m * U * qIb) * 60;
                    }
                case "JJG596-2012":
                    {
                        return 1.2F * 60F * 1000F / ((float)MeterConst * _m * U * qIb) * 60;
                    }
                case "IR46":
                case "Q/GDW12175-2021":
                case "Q/GDW10827-2020":
                case "Q/GDW10364-2020":
                    {
                        return 3.6F * 1000000F / ((float)MeterConst * _m * U * qIb);
                    }
                default:
                    {
                        return 1.2F * 60F * 1000F / ((float)MeterConst * _m * U * qIb) * 60;
                    }
            }
        }

    }
}
