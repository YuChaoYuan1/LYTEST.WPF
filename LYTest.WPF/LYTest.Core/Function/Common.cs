using LYTest.Core.Enum;
using System;

namespace LYTest.Core.Function
{
    public class Common
    {
        /// <summary>
        /// 计算电压电流相位
        /// </summary>
        /// <param name="Clfs">测量方式：
        ///   三相四线 = 0,
        ///   三相三线 = 1,
        ///   二元件跨相90 = 2,
        ///   二元件跨相60 = 3,
        ///   三元件跨相90 = 4,
        ///   单相 = 5,        /// </param>
        /// <param name="Glfx">功率方向</param>
        /// <param name="Glyj">功率元件</param>
        /// <param name="glys">功率因数</param>
        /// <param name="bln_Nxx">逆相序</param>
        /// <returns></returns>
        public static float[] GetPhiGlys(WireMode Clfs, PowerWay Glfx, Cus_PowerYuanJian Glyj, string glys, Cus_PowerPhase bln_Nxx)
        {
            float[] sng_Phi = new float[7];

            int intClfs = (int)Clfs;
            int intFX = (int)Glfx;



            if (Clfs == WireMode.三相四线 || Clfs == WireMode.单相) //三相四线 或 单相
            {
                if (Glfx == PowerWay.正向有功 || Glfx == PowerWay.反向有功)   //正向有功，反向有功
                    intClfs = 0;
                else
                    intClfs = 1;                //无功
            }
            else if (Clfs == WireMode.三相三线) //三相三线 = 1
            {
                if (Glfx == PowerWay.正向有功 || Glfx == PowerWay.反向有功)   //正向有功，反向有功
                    intClfs = 2;
                else//无功
                    intClfs = 3;
            }

            //电压电流相位
            float sngAngle = GetGlysAngle(intClfs, glys);
            //sngAngle = (int)sngAngle;
            sng_Phi[6] = sngAngle;
            if (Clfs == WireMode.三相四线) //三相四线
            {
                #region 三相四线角度
                sng_Phi[0] = 0;           //Ua
                sng_Phi[1] = 240;         //Ub
                sng_Phi[2] = 120;         //Uc
                sng_Phi[3] = sng_Phi[0] - sngAngle;
                sng_Phi[4] = sng_Phi[1] - sngAngle;
                sng_Phi[5] = sng_Phi[2] - sngAngle;


                //如果是反向要将电流角度反过来
                if (glys.IndexOf('-') == -1)
                {
                    if (intFX == 2 || intFX == 4) //反向
                    {
                        sng_Phi[3] = sng_Phi[3] + 180;
                        sng_Phi[4] = sng_Phi[4] + 180;
                        sng_Phi[5] = sng_Phi[5] + 180;
                        sng_Phi[6] = sng_Phi[6] + 180;
                    }
                }



                #endregion
            }
            else if (Clfs == WireMode.三相三线)
            {
                sng_Phi[0] = 0;           //Ua
                sng_Phi[2] = 120;         //Uc
                sng_Phi[3] = sng_Phi[0] - sngAngle - 30;
                sng_Phi[5] = sng_Phi[2] - sngAngle - 30;
                sng_Phi[0] = 30 - 30;
                sng_Phi[2] = 90 - 30;

                if (Glyj != Cus_PowerYuanJian
.H)
                {
                    sng_Phi[3] = sng_Phi[0] - sngAngle;
                    sng_Phi[5] = sng_Phi[2] - sngAngle;
                }

                //如果是反向要将电流角度反过来
                if (intFX == 2 || intFX == 4) //反向
                {
                    sng_Phi[3] = sng_Phi[3] + 180;
                    sng_Phi[5] = sng_Phi[5] + 180;
                    sng_Phi[6] = sng_Phi[6] + 180;
                }
            }
            else if (Clfs == WireMode.二元件跨相90)
            {
                #region 三元件跨相90°无功表角度
                sng_Phi[0] = 0;           //Ua
                sng_Phi[1] = 240;         //Ub
                sng_Phi[2] = 120;         //Uc
                sng_Phi[3] = sng_Phi[0] - sngAngle;
                sng_Phi[4] = sng_Phi[1] - sngAngle;
                sng_Phi[5] = sng_Phi[2] - sngAngle;
                #endregion
            }
            else
            {
                #region 单相表
                sng_Phi[0] = 0;         //Ua
                sng_Phi[3] = sng_Phi[0] - sngAngle;

                //如果是反向要将电流角度反过来                
                if (intFX == 2 || intFX == 4)
                {
                    sng_Phi[3] = sng_Phi[3] + 180;
                    sng_Phi[6] = sng_Phi[6] + 180;
                }
                #endregion

            }

            sng_Phi[0] = TrimAngle(sng_Phi[0]);
            sng_Phi[1] = TrimAngle(sng_Phi[1]);
            sng_Phi[2] = TrimAngle(sng_Phi[2]);
            sng_Phi[3] = TrimAngle(sng_Phi[3]);
            sng_Phi[4] = TrimAngle(sng_Phi[4]);
            sng_Phi[5] = TrimAngle(sng_Phi[5]);
            sng_Phi[6] = TrimAngle(sng_Phi[6]);

            if (bln_Nxx == Cus_PowerPhase.电压逆相序)
            {
                float tmp = sng_Phi[1];
                sng_Phi[1] = sng_Phi[2];
                sng_Phi[2] = tmp;
            }
            else if (bln_Nxx == Cus_PowerPhase.电流逆相序)
            {
                float tmp = sng_Phi[3];
                sng_Phi[3] = sng_Phi[4];
                sng_Phi[4] = tmp;
            }
            else if (bln_Nxx == Cus_PowerPhase.逆相序)
            {
                float tmp = sng_Phi[0];
                sng_Phi[0] = sng_Phi[1];
                sng_Phi[1] = tmp;

                tmp = sng_Phi[3];
                sng_Phi[3] = sng_Phi[4];
                sng_Phi[4] = tmp;
            }

            // 确保 正向无功1.0
            if (Glfx == PowerWay.正向无功 && (glys == "1.0L" || glys=="1.0"))
            {
                sng_Phi[3] += 0.5f;
                sng_Phi[4] += 0.5f;
                sng_Phi[5] += 0.5f;
            }
            else if (Glfx == PowerWay.正向无功 && glys == "1.0C")
            {
                sng_Phi[3] -= 0.5f;
                sng_Phi[4] -= 0.5f;
                sng_Phi[5] -= 0.5f;
            }
            else if (Glfx == PowerWay.反向无功 && (glys == "1.0L" || glys=="1.0"))
            {
                sng_Phi[3] += 0.5f;
                sng_Phi[4] += 0.5f;
                sng_Phi[5] += 0.5f;
            }
            else if (Glfx == PowerWay.反向无功 && glys == "1.0C")
            {
                sng_Phi[3] -= 0.5f;
                sng_Phi[4] -= 0.5f;
                sng_Phi[5] -= 0.5f;
            }

            return sng_Phi;
        }

        /// <summary>
        /// 从十六进制转换成二进制
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string ConvertTo2From16(string data)
        {
            string d = "";
            for (int i = data.Length - 1; i >= 0; i--)
            {
                byte tmp = Convert.ToByte(data.Substring(i, 1), 16);
                d += Convert.ToString(tmp, 2).PadLeft(4, '0');
            }
            return d;
        }

        /// <summary>
        /// 计算功率因数
        /// </summary>
        /// <param name="clfs">0-P2,P4有功，1-P2,P4无功，2-P3有功，4-P3无功</param>
        /// <param name="glys">功率因数</param>
        /// <returns></returns>
        private static float GetGlysAngle(int clfs, string glys)
        {
            double pha = 0;
            double ys;  //因素 ，如1.0
            float PI = 3.14159f;
            glys = glys.Trim();
            string sLC = glys.Substring(glys.Length - 1, 1);//感容性，如C,L
            if (sLC.ToUpper() == "C" || sLC.ToUpper() == "L")
                ys = double.Parse(glys.Substring(0, glys.Length - 1));
            else
                ys = double.Parse(glys);

            if (clfs == 0 || clfs == 2)      //有功
            {
                if (ys > 0 && ys <= 1)
                    pha = Math.Atan(Math.Sqrt(1 - ys * ys) / ys);
                else if (ys < 0 && ys >= -1)
                    pha = Math.Atan(Math.Sqrt(1 - ys * ys) / ys) + PI;
                else if (ys == 0)
                    pha = PI / 2;
            }
            else
            {
                if (ys > -1 && ys < 1)
                    pha = Math.Atan(ys / Math.Sqrt(1 - ys * ys));
                else if (ys == -1)
                    pha = -PI * 0.5f;
                else if (ys == 1)
                    pha = PI * 0.5f;
            }
            pha = pha * 180 / PI;


            if (clfs == 2 && sLC.ToUpper() == "C")
                pha = 360 - pha;
            else if ((clfs == 1 || clfs == 3) && sLC.ToUpper() == "C")
                pha = 360 - pha - 180;
            else if (sLC.ToUpper() == "C")
                pha = 360 - pha;


            if (pha < 0) pha = 360 + pha;
            if (pha >= 360) pha -= Math.Floor(pha / 360) * 360;
            pha = Math.Round(pha, 4);



            return (float)pha;
        }

        private static float TrimAngle(float angle)
        {
            float f = angle;
            if (angle > 0)
                f = angle % 360;
            else if (angle < 0)
                while (f > 0 && f < 360)
                    f += 360;

            return f;
        }

        /// <summary>
        /// 获取指定数据的小数位数
        /// </summary>
        /// <param name="strNumber">数字字符串</param>
        /// <returns></returns>
        public static int GetPrecision(string strNumber)
        {
            if (!Number.IsNumeric(strNumber))
            {
                return 0;
            }
            int hzPrecision = strNumber.ToString().LastIndexOf('.');
            if (hzPrecision == -1)
            {
                //没有小数点，返回0
                hzPrecision = 0;
            }
            else
            {
                //有小数点
                hzPrecision = strNumber.ToString().Length - hzPrecision - 1;
            }
            return hzPrecision;

        }

        /// <summary>
        /// 从<paramref name="timezone"/>中查找指定<paramref name="month"/>和<paramref name="day"/>的NN
        /// </summary>
        /// <param name="timezone">MMddNN……MMddNN</param>
        /// <param name="month">1~12</param>
        /// <param name="day">1~31</param>
        /// <returns>NN,或0未找到</returns>
        public static byte GetPeriodNN(string timezone, byte month, byte day = 1)
        {
            if (timezone == null || month < 1 || month > 12 || day < 1 || day > 31)
                return 0;
            int len = timezone.Length / 6;
            byte[] nn = new byte[1232];
            for (int i = 0; i < len; i++)
            {
                nn[int.Parse(timezone.Substring(i * 6, 4))] = byte.Parse(timezone.Substring(i * 6 + 4, 2));
            }
            byte nntmp = 0;
            for (int m = 0; m < 1232 * 2; m++)
            {
                if (nn[m % 1232] != 0) nntmp = nn[m % 1232];
                nn[m % 1232] = nntmp;
            }
            return nn[int.Parse($"{month:D2}{day:D2}")];
        }
        /// <summary>
        /// 将<paramref name="timezone"/>转换为key:MMdd,value:NN的全集合
        /// </summary>
        /// <param name="timezone">MMddNN……MMddNN</param>
        /// <returns></returns>
        public static byte[] GetPeriodMMddMapNN(string timezone)
        {
            if (timezone == null)
                return new byte[0];
            int len = timezone.Length / 6;
            byte[] nn = new byte[1232];
            for (int i = 0; i < len; i++)
            {
                nn[int.Parse(timezone.Substring(i * 6, 4))] = byte.Parse(timezone.Substring(i * 6 + 4, 2));
            }
            byte nntmp = 0;
            for (int m = 0; m < 1232 * 2; m++)
            {
                if (nn[m % 1232] != 0) nntmp = nn[m % 1232];
                nn[m % 1232] = nntmp;
            }
            return nn;
        }

    }
}
