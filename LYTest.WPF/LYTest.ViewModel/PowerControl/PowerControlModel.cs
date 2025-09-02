using LYTest.ViewModel.Device;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LYTest.ViewModel.PowerControl
{
    public class PowerControlModel : ViewModelBase
    {
        #region 自由升源信息
        private double ua = 57.7;
        public double Ua
        {
            get { return ua; }
            set
            {
                SetPropertyValue(value, ref ua, "Ua");
                SetU(Ua);
                if (!isSetXValue)
                {
                    TemUa = Ua;
                    Zoom = 1;
                }
            }
        }
        private double phaseUa;
        public double PhaseUa
        {
            get { return phaseUa; }
            set { SetPropertyValue(value, ref phaseUa, "PhaseUa"); }
        }
        private double ub = 57.7;
        public double Ub
        {
            get { return ub; }
            set
            {
                SetPropertyValue(value, ref ub, "Ub");
                SetU(Ub);
                if (!isSetXValue)
                {
                    TemUb = Ub;
                    Zoom = 1;
                }
            }
        }
        private double phaseUb = 240;
        public double PhaseUb
        {
            get { return phaseUb; }
            set { SetPropertyValue(value, ref phaseUb, "PhaseUb"); }
        }
        private double uc = 57.7;
        public double Uc
        {
            get { return uc; }
            set
            {
                SetPropertyValue(value, ref uc, "Uc");
                SetU(Uc);
                if (!isSetXValue)
                {
                    TemUc = Uc;
                    Zoom = 1;
                }
            }
        }
        private double phaseUc = 120;
        public double PhaseUc
        {
            get { return phaseUc; }
            set { SetPropertyValue(value, ref phaseUc, "PhaseUc"); }
        }
        private double ia;
        public double Ia
        {
            get { return ia; }
            set
            {
                SetPropertyValue(value, ref ia, "Ia");
                SetI(Ia);
                if (!isSetXValue)
                {
                    TemIa = Ia;
                    Zoom = 1;
                }

            }
        }
        private double phaseIa;
        public double PhaseIa
        {
            get { return phaseIa; }
            set
            {
                SetPropertyValue(value, ref phaseIa, "PhaseIa");
                SetPhiIa(PhaseIa);
                if (!isSetXValue)
                {
                    TemPhiIa = PhaseIa;
                    Zoom = 1;
                }
            }
        }
        private double ib = 0;
        public double Ib
        {
            get { return ib; }
            set
            {
                SetPropertyValue(value, ref ib, "Ib");
                SetI(Ib);
                if (!isSetXValue)
                {
                    TemIb = Ib;
                    Zoom = 1;
                }
            }
        }
        private double phaseIb = 240;
        public double PhaseIb
        {
            get { return phaseIb; }
            set
            {
                SetPropertyValue(value, ref phaseIb, "PhaseIb");
                SetPhiIb(PhaseIb);
                if (!isSetXValue)
                {
                    TemPhiIb = PhaseIb;
                    Zoom = 1;
                }
            }
        }
        private double ic = 0;
        public double Ic
        {
            get { return ic; }
            set
            {
                SetPropertyValue(value, ref ic, "Ic");
                SetI(Ic);
                if (!isSetXValue)
                {
                    TemIc = Ic;
                    Zoom = 1;
                }
            }
        }
        private double phaseIc = 120;
        public double PhaseIc
        {
            get { return phaseIc; }
            set
            {
                SetPropertyValue(value, ref phaseIc, "PhaseIc");
                SetPhiIc(PhaseIc);
                if (!isSetXValue)
                {
                    TemPhiIc = PhaseIc;
                    Zoom = 1;
                }
            }
        }
        private float freq = 50;

        public float Freq
        {
            get { return freq; }
            set { SetPropertyValue(value, ref freq, "Freq"); }
        }


        private string setting = "三相四线";

        /// <summary>
        /// 测量发送
        /// </summary>
        public string Setting
        {
            get { return setting; }
            set
            {
                SetPropertyValue(value, ref setting, "Setting");
                switch (Setting)
                {
                    case "三相四线":
                        IsUA = true;
                        IsUB = true;
                        IsUC = true;
                        IsIA = true;
                        IsIB = true;
                        IsIC = true;
                        break;
                    case "三相三线":
                        IsUA = true;
                        IsUB = false;
                        IsUC = true;
                        IsIA = true;
                        IsIB = false;
                        IsIC = true;
                        break;
                    case "单相":
                        IsUA = true;
                        IsUB = false;
                        IsUC = false;
                        IsIA = true;
                        IsIB = false;
                        IsIC = false;
                        break;

                    default:
                        break;


                }

                GetAngle();

            }
        }



        private string glys = "1.0";

        /// <summary>
        /// 功率因数
        /// </summary>
        public string GLYS
        {
            get { return glys; }
            set { SetPropertyValue(value, ref glys, "GLYS"); GetAngle(); }
        }


        private string glfx = "正向有功";

        /// <summary>
        /// 功率方向
        /// </summary>
        public string GLFX
        {
            get { return glfx; }
            set { SetPropertyValue(value, ref glfx, "GLFX"); GetAngle(); }
        }

        public bool StmPulseSwitch
        {
            get => GetProperty(false);
            set => SetProperty(value);
        }

        #endregion


        private string path = Directory.GetCurrentDirectory() + "\\Ini\\PowerData.ini";

        public PowerControlModel()
        {
            Zoom = 1;
            //开始初始化获取默认值

            try
            {
                IniValue();
            }
            catch (Exception)
            {
            }

            if (double.TryParse(EquipmentData.MeterGroupInfo.FirstMeter.GetProperty("MD_UB")?.ToString(), out double ub))
            {
                Ua = ub;
                Ub = ub;
                Uc = ub;
            }
            else
            {
                Ua = 0;
                Ub = 0;
                Uc = 0;
            }
            Ia = 0;
            Ib = 0;
            Ic = 0;

            Task.Factory.StartNew(() => GetPowerStableState());

        }


        private void IniValue()
        {
            string str = "";
            str = Core.OperateFile.GetINI("data", "测量方式", path).Trim();
            if (str != "") Setting = str;
            str = Core.OperateFile.GetINI("data", "功率因数", path).Trim();
            if (str != "") GLYS = str;
            str = Core.OperateFile.GetINI("data", "功率方向", path).Trim();
            if (str != "") GLFX = str;
            str = Core.OperateFile.GetINI("data", "频率", path);
            Freq = str != "" ? int.Parse(str) : 50;


            IsReverseOrder = Core.OperateFile.GetINI("data", "逆相序", path).Trim() == "是" ? true : false;
            IsAllCheck = Core.OperateFile.GetINI("data", "统一设置", path).Trim() == "是" ? true : false;
            IsUA = Core.OperateFile.GetINI("data", "A相电压开关", path).Trim() == "是" ? true : false;
            Ua = double.Parse(Core.OperateFile.GetINI("data", "A相电压", path).Trim());
            PhaseUa = double.Parse(Core.OperateFile.GetINI("data", "A相电压相位", path).Trim());
            IsUB = Core.OperateFile.GetINI("data", "B相电压开关", path).Trim() == "是" ? true : false;
            Ub = double.Parse(Core.OperateFile.GetINI("data", "B相电压", path).Trim());
            PhaseUb = double.Parse(Core.OperateFile.GetINI("data", "B相电压相位", path).Trim());
            IsUC = Core.OperateFile.GetINI("data", "C相电压开关", path).Trim() == "是" ? true : false;
            Uc = double.Parse(Core.OperateFile.GetINI("data", "C相电压", path).Trim());
            PhaseUc = double.Parse(Core.OperateFile.GetINI("data", "C相电压相位", path).Trim());
            IsIA = Core.OperateFile.GetINI("data", "A相电流开关", path).Trim() == "是" ? true : false;
            Ia = double.Parse(Core.OperateFile.GetINI("data", "A相电流", path).Trim());
            PhaseIa = double.Parse(Core.OperateFile.GetINI("data", "A相电流相位", path).Trim());
            IsIB = Core.OperateFile.GetINI("data", "B相电流开关", path).Trim() == "是" ? true : false;
            Ib = double.Parse(Core.OperateFile.GetINI("data", "B相电流", path).Trim());
            PhaseIb = double.Parse(Core.OperateFile.GetINI("data", "B相电流相位", path).Trim());
            IsIC = Core.OperateFile.GetINI("data", "C相电流开关", path).Trim() == "是" ? true : false;
            Ic = double.Parse(Core.OperateFile.GetINI("data", "C相电流", path).Trim());
            PhaseIc = double.Parse(Core.OperateFile.GetINI("data", "C相电流相位", path).Trim());

        }

        public void SaveValue()
        {
            Core.OperateFile.WriteINI("data", "测量方式", Setting.ToString(), path);
            Core.OperateFile.WriteINI("data", "功率因数", GLYS, path);
            Core.OperateFile.WriteINI("data", "功率方向", GLFX, path);
            Core.OperateFile.WriteINI("data", "频率", Freq.ToString(), path);
            Core.OperateFile.WriteINI("data", "逆相序", isReverseOrder ? "是" : "否", path);
            Core.OperateFile.WriteINI("data", "统一设置", IsAllCheck ? "是" : "否", path);
            Core.OperateFile.WriteINI("data", "A相电压开关", IsUA ? "是" : "否", path);
            Core.OperateFile.WriteINI("data", "A相电压", Ua.ToString(), path);
            Core.OperateFile.WriteINI("data", "A相电压相位", PhaseUa.ToString(), path);
            Core.OperateFile.WriteINI("data", "B相电压开关", IsUB ? "是" : "否", path);
            Core.OperateFile.WriteINI("data", "B相电压", Ub.ToString(), path);
            Core.OperateFile.WriteINI("data", "B相电压相位", PhaseUb.ToString(), path);
            Core.OperateFile.WriteINI("data", "C相电压开关", IsUC ? "是" : "否", path);
            Core.OperateFile.WriteINI("data", "C相电压", Uc.ToString(), path);
            Core.OperateFile.WriteINI("data", "C相电压相位", PhaseUc.ToString(), path);
            Core.OperateFile.WriteINI("data", "A相电流开关", IsIA ? "是" : "否", path);
            Core.OperateFile.WriteINI("data", "A相电流", Ia.ToString(), path);
            Core.OperateFile.WriteINI("data", "A相电流相位", PhaseIa.ToString(), path);
            Core.OperateFile.WriteINI("data", "B相电流开关", IsIB ? "是" : "否", path);
            Core.OperateFile.WriteINI("data", "B相电流", Ib.ToString(), path);
            Core.OperateFile.WriteINI("data", "B相电流相位", PhaseIb.ToString(), path);
            Core.OperateFile.WriteINI("data", "C相电流开关", IsIC ? "是" : "否", path);
            Core.OperateFile.WriteINI("data", "C相电流", Ic.ToString(), path);
            Core.OperateFile.WriteINI("data", "C相电流相位", PhaseIc.ToString(), path);
        }
        private bool isAllCheck = true;

        /// <summary>
        /// 统一设置
        /// </summary>
        public bool IsAllCheck
        {
            get { return isAllCheck; }
            set { SetPropertyValue(value, ref isAllCheck, "IsAllCheck"); }
        }

        private bool isReverseOrder = true;

        /// <summary>
        /// 是否逆相序
        /// </summary>
        public bool IsReverseOrder
        {
            get { return isReverseOrder; }
            set
            {
                SetPropertyValue(value, ref isReverseOrder, "IsReverseOrder");

                //double t;
                //t = PhaseUb;
                //PhaseUb = PhaseUc;
                //PhaseUc = t;
                //t = PhaseIb;
                //PhaseIb = PhaseIc;
                //PhaseIc = t;
                GetAngle();
            }
        }
        #region 是否选中

        private bool isUA = true;
        public bool IsUA
        {
            get { return isUA; }
            set
            {
                SetPropertyValue(value, ref isUA, "IsUA");
                EquipmentData.StdInfo.IsVisibleUA = IsUA;
            }
        }
        private bool isUB = true;
        public bool IsUB
        {
            get { return isUB; }
            set
            {
                SetPropertyValue(value, ref isUB, "IsUB");
                EquipmentData.StdInfo.IsVisibleUB = IsUB;
            }
        }
        private bool isUC = true;
        public bool IsUC
        {
            get { return isUC; }
            set
            {
                SetPropertyValue(value, ref isUC, "IsUC");
                EquipmentData.StdInfo.IsVisibleUC = IsUC;
            }
        }
        private bool isIA = true;
        public bool IsIA
        {
            get { return isIA; }
            set
            {
                SetPropertyValue(value, ref isIA, "IsIA");
                EquipmentData.StdInfo.IsVisibleIA = IsIA;
            }
        }
        private bool isIB = true;
        public bool IsIB
        {
            get { return isIB; }
            set
            {
                SetPropertyValue(value, ref isIB, "IsIB");
                EquipmentData.StdInfo.IsVisibleIB = IsIB;
            }
        }

        private bool isIC = true;
        public bool IsIC
        {
            get { return isIC; }
            set
            {
                SetPropertyValue(value, ref isIC, "IsIC");
                EquipmentData.StdInfo.IsVisibleIC = IsIC;
            }
        }


        #endregion




        /// <summary>
        /// 升源
        /// </summary>
        public void PowerOnFree()
        {
            if (!EquipmentData.DeviceManager.Devices.ContainsKey(DeviceName.功率源)) return;
            //实际升源的值
            double P_ua = Ua;
            double P_ub = Ub;
            double P_uc = Uc;
            double P_ia = Ia;
            double P_ib = Ib;
            double P_ic = Ic;

            int jxfs = 0;
            if (Setting == "三相四线")
            {
                jxfs = 0;
            }
            else if (Setting == "三相三线")
            {
                jxfs = 1;
            }
            else if (Setting == "单相")
            {
                jxfs = 5;
            }

            if (!IsUA) P_ua = 0;
            if (!IsUB) P_ub = 0;
            if (!IsUC) P_uc = 0;
            if (!IsIA) P_ia = 0;
            if (!IsIB) P_ib = 0;
            if (!IsIC) P_ic = 0;

            try
            {
                SaveValue();
            }
            catch { }
            EquipmentData.StdInfo.IsPowerOkVisible = false;
            //IsAgainPower = true;

            EquipmentData.DeviceManager.PowerOn(jxfs, (float)P_ua, (float)P_ub, (float)P_uc, (float)P_ia, (float)P_ib, (float)P_ic, PhaseUa, PhaseUb, PhaseUc, PhaseIa, PhaseIb, PhaseIc, Freq, 1);
            
            
            if (StmPulseSwitch)
            {
                bool wg = GLFX.Contains("无功");
                EquipmentData.DeviceManager.SetPulseType(wg ? "32" : "31");
            }
        }


        //bool IsAgainPower = false;
        /// <summary>
        /// 获取功率源稳定状态
        /// </summary>
        private void GetPowerStableState()
        {
            float tem = GLFX.IndexOf("有功") != -1 ? EquipmentData.StdInfo.P : EquipmentData.StdInfo.Q;
            while (true)
            {
                System.Threading.Thread.Sleep(1000);
                float tem2 = GLFX.IndexOf("有功") != -1 ? EquipmentData.StdInfo.P : EquipmentData.StdInfo.Q;
                if (tem2 > tem * 0.95 && tem2 < tem * 1.05) //判断功率波动--这个就是稳定了 --不判断等于的--等于应该只有0
                {
                    EquipmentData.StdInfo.IsPowerOkVisible = true;
                }
                else
                {
                    EquipmentData.StdInfo.IsPowerOkVisible = false;
                    tem = tem2;
                }
            }
            //需要判断电压--电流
            //上下的波动
            //一直判断
            //while (true)
            //{
            //    while (!IsAgainPower)//这个是关源的情况---一直等待升源
            //    {
            //        System.Threading.Thread.Sleep(1000);
            //    }

            //    //源一升起来--就开始判断他是否稳定了
            //    float tem = GLFX.IndexOf("有功") != -1 ? EquipmentData.StdInfo.P : EquipmentData.StdInfo.Q;
            //    while (IsAgainPower)
            //    {
            //        System.Threading.Thread.Sleep(1000);
            //        float tem2 = GLFX.IndexOf("有功") != -1 ? EquipmentData.StdInfo.P : EquipmentData.StdInfo.Q;
            //        if (tem2 > tem * 0.95 && tem2 < tem * 1.05) //判断功率波动--这个就是稳定了 --不判断等于的--等于应该只有0
            //        {
            //            EquipmentData.StdInfo.IsPowerOkVisible = true;
            //            break;
            //        }
            //        else
            //        {
            //            EquipmentData.StdInfo.IsPowerOkVisible = false;
            //            tem = tem2;
            //        }
            //    }
            //}
        }


        /// <summary>
        /// 关源
        /// </summary>
        public void PowerOffFree()
        {
            EquipmentData.DeviceManager.PowerOff();
            EquipmentData.StdInfo.IsPowerOkVisible = false;
            //IsAgainPower = false;
        }



        /// <summary>
        /// 计算角度
        /// </summary>
        private void GetAngle()
        {
            int intClfs = 0;
            if (Setting == "三相四线")
                intClfs = 0;
            else if (Setting == "三相三线")
                intClfs = 1;
            else if (Setting == "单相")
                intClfs = 5;
            int intFX = 1;
            switch (GLFX)
            {
                case "正向有功":
                    intFX = 1;
                    break;
                case "反向有功":
                    intFX = 2;
                    break;
                case "正向无功":
                    intFX = 3;
                    break;
                case "反向无功":
                    intFX = 4;
                    break;
                default:
                    break;
            }

            if (intClfs == 0 || intClfs == 5) //三相四线 或 单相
            {
                if (intFX == 1 || intFX == 2)   //正向有功，反向有功
                    intClfs = 0;
                else
                    intClfs = 1;                //无功
            }
            else if (intClfs == 1) //三相三线 = 1
            {
                if (intFX == 1 || intFX == 2)   //正向有功，反向有功
                    intClfs = 2;
                else//无功
                    intClfs = 3;
            }
            float sngAngle = GetGlysAngle(intClfs, GLYS);

            float[] sng_Phi = new float[7];
            sng_Phi[6] = sngAngle;
            if (Setting == "三相四线") //三相四线
            {
                #region 三相四线角度
                sng_Phi[0] = 0;           //Ua
                sng_Phi[1] = 240;         //Ub
                sng_Phi[2] = 120;         //Uc
                sng_Phi[3] = sng_Phi[0] - sngAngle;
                sng_Phi[4] = sng_Phi[1] - sngAngle;
                sng_Phi[5] = sng_Phi[2] - sngAngle;


                //如果是反向要将电流角度反过来
                if (GLYS.IndexOf('-') == -1)
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
            else if (Setting == "三相三线")
            {
                #region 三相三线角度

                sng_Phi[0] = 0;           //Ua
                sng_Phi[2] = 120;         //Uc
                sng_Phi[3] = sng_Phi[0] - sngAngle - 30;
                sng_Phi[5] = sng_Phi[2] - sngAngle - 30;
                sng_Phi[0] = 30 - 30;
                sng_Phi[2] = 90 - 30;

                //if (Glyj != Cus_PowerYuanJian.H)
                if (!IsUA || !IsUC)   //不是合元的情况
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
            float sngPhiTmp;
            if (IsReverseOrder)
            {
                sngPhiTmp = sng_Phi[1];
                sng_Phi[1] = sng_Phi[2];
                sng_Phi[2] = sngPhiTmp;

                sngPhiTmp = sng_Phi[4];
                sng_Phi[4] = sng_Phi[5];
                sng_Phi[5] = sngPhiTmp;
            }

            PhaseUa = sng_Phi[0];
            PhaseUb = sng_Phi[1];
            PhaseUc = sng_Phi[2];
            PhaseIa = sng_Phi[3];
            PhaseIb = sng_Phi[4];
            PhaseIc = sng_Phi[5];


        }
        private static float TrimAngle(double angle)
        {
            float f = (float)angle;
            if (angle > 0)
                f = (float)(angle % 360);
            else if (angle < 0)
                while (f < 0 && f < 360)
                    f += 360;

            return f;
        }

        /// <summary>
        /// 计算功率因数
        /// </summary>
        /// <param name="clfs">0-P2,P4有功，1-P2,P4无功，2-P3有功，4-P3无功</param>
        /// <param name="glys">功率因数</param>
        /// <returns></returns>
        private float GetGlysAngle(int clfs, string glys)
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
            if (pha >= 360) pha = pha - (pha / 360) * 360;
            pha = Math.Round(pha, 4);
            return (float)pha;
        }




        bool IsSet = false;
        private void SetU(double U)
        {
            if (!isAllCheck) return;
            if (IsSet) return;
            IsSet = true;
            Ua = U;
            Ub = U;
            Uc = U;
            System.Threading.Thread.Sleep(10);
            IsSet = false;
        }

        private void SetI(double I)
        {
            if (!isAllCheck) return;
            if (IsSet) return;
            IsSet = true;
            Ia = I;
            Ib = I;
            Ic = I;
            System.Threading.Thread.Sleep(10);
            IsSet = false;

        }
        private void SetPhiIa(double phiI)
        {
            if (!isAllCheck) return;
            if (IsSet) return;
            IsSet = true;
            PhaseIa = TrimAngle(phiI);
            System.Threading.Thread.Sleep(10);
            IsSet = false;

        }
        private void SetPhiIb(double phiI)
        {
            if (!isAllCheck) return;
            if (IsSet) return;
            IsSet = true;
            PhaseIb = TrimAngle(phiI);
            System.Threading.Thread.Sleep(10);
            IsSet = false;

        }
        private void SetPhiIc(double phiI)
        {
            if (!isAllCheck) return;
            if (IsSet) return;
            IsSet = true;
            PhaseIc = TrimAngle(phiI);
            System.Threading.Thread.Sleep(10);
            IsSet = false;

        }


        #region 设置电流

        private double TemA;
        private double TemB;
        private double TemC;
        private double TemIa;
        private double TemIb;
        private double TemIc;
        private double TemPhiIa;
        private double TemPhiIb;
        private double TemPhiIc;
        private double TemUa;
        private double TemUb;
        private double TemUc;

        /// <summary>
        /// 是否调节值
        /// </summary>
        bool isSetXValue = false;

        private string adjustTypeMsg = "I";
        /// <summary>
        /// 调节类型：0电流幅值，1电流相位，2电压幅值，
        /// </summary>
        private int adjustType = 0;
        /// <summary>
        /// 调节类型：0电流幅值，1电流相位，2电压幅值，
        /// </summary>
        public int AdjustType
        {
            get { return adjustType; }
            set { adjustType = value; }
        }

        public void SetAdjustCurrent()
        {
            AdjustType = 0;
            adjustTypeMsg = "I";
            TemA = TemIa;
            TemB = TemIb;
            TemC = TemIc;
            Zoom = zoomI;
        }
        public void SetAdjustCurrentAngle()
        {
            AdjustType = 1;
            adjustTypeMsg = "φI";
            TemA = TemPhiIa;
            TemB = TemPhiIb;
            TemC = TemPhiIc;
            Zoom = zoomPhiI;
        }
        public void SetAdjustVoltage()
        {
            AdjustType = 2;
            adjustTypeMsg = "U";
            TemA = TemUa;
            TemB = TemUb;
            TemC = TemUc;
            Zoom = zoomU;
        }

        private double zoomI = 1;
        private double zoomPhiI = 0;
        private double zoomU = 1;

        private double zoom = 1;

        public double Zoom
        {
            get { return zoom; }
            set
            {
                zoom = value;
                ZoomTips = $"变化率【{Zoom:F4}】{(AdjustType == 1 ? "°" : "倍")}";
                ZoomTips2 = $"初值{adjustTypeMsg}a【{TemA}】 {adjustTypeMsg}b【{TemB}】 {adjustTypeMsg}c【{TemC}】";

            }
        }

        private string zoomTips;

        /// <summary>
        /// 缩放提示信息
        /// </summary>
        public string ZoomTips
        {
            get { return zoomTips; }
            set { SetPropertyValue(value, ref zoomTips, "ZoomTips"); }
        }
        private string zoomTips2;

        /// <summary>
        /// 缩放提示信息
        /// </summary>
        public string ZoomTips2
        {
            get { return zoomTips2; }
            set { SetPropertyValue(value, ref zoomTips2, "ZoomTips2"); }
        }

        private string zoomX = "60.0000";
        /// <summary>
        /// 缩放输入值
        /// </summary>
        public string ZoomX
        {
            get { return zoomX; }
            set { SetPropertyValue(value, ref zoomX, "ZoomX"); }
        }



        private void SetValue()
        {
            //这里应该用原本的值来做--所以改变的时候需要存储之前的值
            isSetXValue = true;
            if (AdjustType == 0)
            {
                Ia = TemIa * zoomI;
                if (!isAllCheck) //统一设置的话设置IA就行了
                {
                    Ib = TemIb * zoomI;
                    Ic = TemIc * zoomI;
                }
            }
            else if (AdjustType == 2)
            {
                Ua = TemUa * zoomU;
                if (!isAllCheck)
                {
                    Ub = TemUb * zoomU;
                    Uc = TemUc * zoomU;
                }
            }
            else if (AdjustType == 1)
            {
                PhaseIa = TrimAngle(TemPhiIa + zoomPhiI);
                //if (!isAllCheck)
                {
                    PhaseIb = TrimAngle(TemPhiIb + zoomPhiI);
                    PhaseIc = TrimAngle(TemPhiIc + zoomPhiI);
                }
            }
            System.Threading.Thread.Sleep(10);
            isSetXValue = false;

        }

        private void Cvtzoom(double x, bool add)
        {
            if (AdjustType == 0)
            {
                if (add)
                    zoomI += x;
                else
                    zoomI = x;

                Zoom = zoomI;
            }
            else if (AdjustType == 1)
            {
                if (add)
                    zoomPhiI += x;
                else
                    zoomPhiI = x;

                Zoom = zoomPhiI;
            }
            else if (AdjustType == 2)
            {
                if (add)
                    zoomU += x;
                else
                    zoomU = x;

                Zoom = zoomU;
            }
        }
        private void CvtzoomPhi(double x, bool add)
        {
            if (AdjustType == 1)
            {
                if (add)
                    zoomPhiI += x;
                else
                    zoomPhiI = x;

                Zoom = zoomPhiI;
            }
        }

        public void Add_001()
        {
            Cvtzoom(0.0001, true);

            SetValue();
        }
        public void Cut_001()
        {
            Cvtzoom(-0.0001, true);

            SetValue();
        }
        public void Add_01()
        {
            Cvtzoom(0.001, true);

            SetValue();
        }
        public void Cut_01()
        {
            Cvtzoom(-0.001, true);
            SetValue();
        }
        public void Add_1()
        {
            Cvtzoom(0.01, true);

            SetValue();
        }
        public void Cut_1()
        {
            Cvtzoom(-0.01, true);
            SetValue();
        }
        public void Add_10()
        {
            Cvtzoom(0.1, true);

            SetValue();
        }
        public void Cut_10()
        {
            Cvtzoom(-0.1, true);
            SetValue();
        }
        public void Add_100()
        {
            CvtzoomPhi(1, true);

            SetValue();
        }
        public void Cut_100()
        {
            CvtzoomPhi(-1, true);
            SetValue();
        }
        public void Add_X()
        {
            if (double.TryParse(ZoomX, out double addx))
            {
                CvtzoomPhi(addx, true);

                SetValue();
            }
        }
        public void Cut_X()
        {
            if (double.TryParse(ZoomX, out double addx))
            {
                CvtzoomPhi(-1 * addx, true);
                SetValue();
            }
        }

        public void Set_120()
        {
            Cvtzoom(1.2, false);
            SetValue();
        }
        public void Set_100()
        {
            Cvtzoom(1, false);
            SetValue();
        }
        public void Set_80()
        {
            Cvtzoom(0.8, false);
            SetValue();
        }
        public void Set_60()
        {
            Cvtzoom(0.6, false);
            SetValue();
        }
        #endregion

    }
}
