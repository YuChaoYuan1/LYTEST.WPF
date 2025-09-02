using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using SocketModule.Packet;
using E_CL309;
using Enum;
using Struct;

namespace ZH
{
    public class E_CL309
    {
        /// <summary>
        /// 重试次数
        /// </summary>
        public static int RETRYTIEMS = 1;
        /// <summary>
        /// 源控制端口
        /// </summary>
        private readonly StPortInfo[] m_PowerPort = null;
        readonly DriverBase driverBase = null;

        ///// <summary>
        ///// 是否加谐波
        ///// </summary>
        //private bool m_XieBo = false;

        ///// <summary>
        ///// 本次升源电压
        ///// </summary>
        //private float curU = 0;
        ///// <summary>
        ///// 本次升源电流
        ///// </summary>
        //private float curI = 0;

        private bool sendFlag = true;

        public E_CL309()
        {
            m_PowerPort = new StPortInfo[1];
            driverBase = new DriverBase();
        }

        #region IClass_Interface 成员

        public int InitSetting(int ComNumber, int MaxWaitTime, string IP, int RemotePort, int LocalStartPort)
        {
            try
            {
                m_PowerPort[0] = new StPortInfo
                {
                    m_Exist = 1,
                    m_IP = IP,
                    m_Port = ComNumber,
                    m_Port_IsUDPorCom = true,
                    m_Port_Setting = "38400,n,8,1"
                };
                driverBase.RegisterPort(ComNumber, m_PowerPort[0].m_Port_Setting, m_PowerPort[0].m_IP, RemotePort, LocalStartPort, MaxWaitTime, 100);
                return 0;
            }
            catch
            {
                return -1;
            }
        }

        /// <summary>
        /// 注册Com 口
        /// </summary>
        /// <param name="ComNumber">端口号</param>
        /// <param name="MaxWaitTime">最长等待时间</param>
        /// <param name="WaitSencondsPerByte">字节延时时间</param>
        /// <returns></returns>
        public int InitSettingCom(int ComNumber, int MaxWaitTime, int WaitSencondsPerByte)
        {
            m_PowerPort[0] = new StPortInfo
            {
                m_Exist = 1,
                m_IP = "",
                m_Port = ComNumber,
                m_Port_IsUDPorCom = false,
                m_Port_Setting = "38400,n,8,1"
            };
            driverBase.RegisterPort(ComNumber, "38400,n,8,1", MaxWaitTime, WaitSencondsPerByte);
            return 0;
        }
        /// <summary>
        /// 连接
        /// </summary>
        /// <param name="FrameAry">出参</param>
        /// <returns></returns>
        public int Connect(out string[] FrameAry)
        {
            CL309_RequestLinkPacket rc2 = new CL309_RequestLinkPacket();
            Cl309_RequestLinkReplyPacket recv2 = new Cl309_RequestLinkReplyPacket();
            FrameAry = new string[1];
            try
            {
                FrameAry[0] = BytesToString(rc2.GetPacketData());
                if (sendFlag)
                {
                    if (SendPacketWithRetry(m_PowerPort[0], rc2, recv2))
                    {
                        int ReValue = recv2.ReciveResult == RecvResult.OK ? 0 : 2;
                        return ReValue;
                    }
                    else
                    {
                        return 1;
                    }
                }
                else
                {
                    return 0;
                }
            }
            catch (Exception)
            {
                return -1;
            }
        }
        /// <summary>
        /// 脱机指令没有
        /// </summary>
        /// <param name="FrameAry">输出上行报文</param>
        /// <returns></returns>
        public int DisConnect(out string[] FrameAry)
        {
            FrameAry = new string[1];
            return 0;
        }
        /// <summary>
        /// 升源
        /// </summary>
        /// <param name="clfs">测量方式</param>
        /// <param name="glfx">功率方向</param>
        /// <param name="strGlys">功率因数</param>
        /// <param name="sng_xUb_A">A相电压</param>
        /// <param name="sng_xUb_B">B相电压</param>
        /// <param name="sng_xUb_C">C相电压</param>
        /// <param name="sng_xIb_A">A相电流</param>
        /// <param name="sng_xIb_B">B相电流</param>
        /// <param name="sng_xIb_C">C相电流</param>
        /// <param name="element">功率元件、H元、A元、B元、C元</param>
        /// <param name="sng_Freq">频率</param>
        /// <param name="bln_IsNxx">是否为逆相序</param>
        /// <returns></returns>
        public int PowerOn(int clfs, int glfx, string strGlys,
            float sng_xUb_A, float sng_xUb_B, float sng_xUb_C,
            float sng_xIb_A, float sng_xIb_B, float sng_xIb_C,
            int element, float sng_Freq, bool bln_IsNxx, out string[] FrameAry)
        {

            FrameAry = new string[1];

            try
            {
                #region 源参
                byte myclfs = GetClfs((Cus_Clfs)clfs, (Cus_PowerFangXiang)glfx);
                byte byt_XWKG = 63;

                if (myclfs > 1 && myclfs < 7) byt_XWKG &= 0x2D;   //三相三线 去掉B相
                if ((Cus_PowerYuanJiang)element == Cus_PowerYuanJiang.A)
                    byt_XWKG &= 0xf;                  //去掉BC相
                else if ((Cus_PowerYuanJiang)element == Cus_PowerYuanJiang.B)
                    byt_XWKG &= 0x17;                  //去掉AC相
                else if ((Cus_PowerYuanJiang)element == Cus_PowerYuanJiang.C)
                    byt_XWKG &= 0x27;                  //去掉AB相
                #endregion

                #region
                //string strinfo = "控制源输出";
                if (((sng_xUb_A == sng_xUb_B) && (sng_xUb_A == sng_xUb_C) &&
                    (sng_xIb_A == sng_xIb_B) && (sng_xIb_A == sng_xIb_C)) ||
                    ((Cus_PowerYuanJiang)element != Cus_PowerYuanJiang.H))
                {
                    //单独控制一相或是三相电流电压全部相同,采用35指令输出
                    UIPara tagUI = new UIPara();
                    PhiPara tagP = new PhiPara();
                    CL309_RequestPowerOnPacket rcpower = new CL309_RequestPowerOnPacket();
                    tagUI.Ua = sng_xUb_A;
                    tagUI.Ub = sng_xUb_B;
                    tagUI.Uc = sng_xUb_C;
                    tagUI.Ia = sng_xIb_A;
                    tagUI.Ib = sng_xIb_B;
                    tagUI.Ic = sng_xIb_C;

                    //tagP.PhiIa = sng_IaPhi;
                    //tagP.PhiIb = sng_IbPhi;
                    //tagP.PhiIc = sng_IcPhi;
                    //tagP.PhiUa = sng_UaPhi;
                    //tagP.PhiUb = sng_UbPhi;
                    //tagP.PhiUc = sng_UcPhi;


                    rcpower.SetPara(tagUI, tagP, (Cus_PowerYuanJiang)element, strGlys, sng_Freq, (int)myclfs, false, bln_IsNxx);
                    Cl309_RequestPowerOnReplyPacket recv2 = new Cl309_RequestPowerOnReplyPacket();
                    FrameAry[0] = BytesToString(rcpower.GetPacketData());
                    if (sendFlag)
                    {
                        if (!SendPacketWithRetry(m_PowerPort[0], rcpower, recv2))
                        {
                            return 1;
                        }
                        else
                        {
                            return recv2.ReciveResult == RecvResult.OK ? 0 : 2;
                        }
                    }
                    else
                    {
                        return 0;
                    }


                }
                else
                {
                    //各相电流电压角度可不相同
                    CL309_RequestPowerOnPacket rcpower309 = new CL309_RequestPowerOnPacket();
                    UIPara tagUI = new UIPara
                    {
                        Ua = sng_xUb_A,
                        Ub = sng_xUb_B,
                        Uc = sng_xUb_C,
                        Ia = sng_xIb_A,
                        Ib = sng_xIb_B,
                        Ic = sng_xIb_C
                    };
                    PhiPara tagP = new PhiPara();
                    //tagP.PhiIa = sng_IaPhi;
                    //tagP.PhiIb = sng_IbPhi;
                    //tagP.PhiIc = sng_IcPhi;
                    //tagP.PhiUa = sng_UaPhi;
                    //tagP.PhiUb = sng_UbPhi;
                    //tagP.PhiUc = sng_UcPhi;
                    rcpower309.SetPara(tagUI, tagP, (Cus_PowerYuanJiang)element, strGlys, sng_Freq, (int)myclfs, false, bln_IsNxx);

                    Cl309_RequestPowerOnReplyPacket recv2 = new Cl309_RequestPowerOnReplyPacket();
                    FrameAry[0] = BytesToString(rcpower309.GetPacketData());
                    if (sendFlag)
                    {
                        if (!SendPacketWithRetry(m_PowerPort[0], rcpower309, recv2))
                        {
                            return 1;
                        }
                        else
                        {
                            return recv2.ReciveResult == RecvResult.OK ? 0 : 2;
                        }
                    }
                    else
                    {
                        return 0;
                    }


                }
                #endregion
            }
            catch (Exception)
            {
                return -1;
            }
        }
        /// <summary>
        /// 升源（单条指令实现）
        /// </summary>
        /// <param name="jxfs">接线方式</param>
        /// <param name="Ua">A相电压</param>
        /// <param name="Ub">B相电压</param>
        /// <param name="Uc">C相电压</param>
        /// <param name="Ia">A相电压</param>
        /// <param name="Ib">B相电压</param>
        /// <param name="Ic">C相电压</param>
        /// <param name="PhiUa">A相电压角度</param>
        /// <param name="PhiUb">B相电压角度</param>
        /// <param name="PhiUc">C相电压角度</param>
        /// <param name="PhiIa">A相电流角度</param>
        /// <param name="PhiIb">B相电流角度</param>
        /// <param name="PhiIc">C相电流角度</param>
        /// <param name="Freq">频率</param>
        /// <param name="on">升源标志</param>
        /// <param name="FrameAry"></param>
        /// <returns></returns>
        public int PowerOn(
            double Ua, double Ub, double Uc,
            double Ia, double Ib, double Ic,
            double PhiUa, double PhiUb, double PhiUc,
            double PhiIa, double PhiIb, double PhiIc,
            double Freq, out string[] FrameAry)
        {
            return PowerOnFree(Ua, Ub, Uc, Ia, Ib, Ic, PhiUa, PhiUb, PhiUc, PhiIa, PhiIb, PhiIc, Freq, out FrameAry);
        }
        /// <summary>
        /// 自由升源
        /// </summary>
        /// <param name="Ua">A相电压</param>
        /// <param name="Ub">B相电压</param>
        /// <param name="Uc">C相电压</param>
        /// <param name="Ia">A相电流</param>
        /// <param name="Ib">B相电流</param>
        /// <param name="Ic">C相电流</param>
        /// <param name="PhiUa">A相电压角度</param>
        /// <param name="PhiUb">B相电压角度</param>
        /// <param name="PhiUc">C相电压角度</param>
        /// <param name="PhiIa">A相电流角度</param>
        /// <param name="PhiIb">B相电流角度</param>
        /// <param name="PhiIc">C相电流角度</param>
        /// <param name="Hz">频率</param>
        /// <param name="FrameAry">输出上行报文</param>
        /// <returns></returns>
        public int PowerOnFree(double Ua, double Ub, double Uc, double Ia, double Ib, double Ic, double PhiUa, double PhiUb, double PhiUc, double PhiIa, double PhiIb, double PhiIc, double Hz, out string[] FrameAry)
        {
            FrameAry = new string[1];
            try
            {
                UIPara tagUI = new UIPara();
                PhiPara tagP = new PhiPara();
                tagUI.Ua = Ua;
                tagUI.Ub = Ub;
                tagUI.Uc = Uc;
                tagUI.Ia = Ia;
                tagUI.Ib = Ib;
                tagUI.Ic = Ic;
                tagP.PhiUa = PhiUa;
                tagP.PhiUb = PhiUb;
                tagP.PhiUc = PhiUc;
                tagP.PhiIa = PhiIa;
                tagP.PhiIb = PhiIb;
                tagP.PhiIc = PhiIc;

                CL309_RequestPowerOnPacket rcpower = new CL309_RequestPowerOnPacket();
                Cl309_RequestPowerOnReplyPacket recv = new Cl309_RequestPowerOnReplyPacket();
                rcpower.SetPara(tagUI, tagP, Cus_PowerYuanJiang.H, "1.0", (float)Hz, 0, false, false);//仅更新标志用
                rcpower.SetPara(tagUI, tagP, Hz);//实际
                FrameAry[0] = BytesToString(rcpower.GetPacketData());
                if (sendFlag)
                {
                    if (!SendPacketWithRetry(m_PowerPort[0], rcpower, recv))
                    {
                        return 1;
                    }
                    else
                    {
                        return recv.ReciveResult == RecvResult.OK ? 0 : 2;
                    }
                }
                else
                {
                    return 0;
                }
            }
            catch (Exception)
            {
                return -1;
            }
        }

        /// <summary>
        /// 降源
        /// </summary>
        /// <param name="FrameAry">输出上行报文</param>
        /// <returns></returns>
        public int PowerOn_Off(out string[] FrameAry)
        {
            //谐波标志归0
            //m_XieBo = false;
            FrameAry = new string[2];
            try
            {
                //curU = 0F;
                //curI = 0F;
                CL309_RequestPowerOnPacket rco = new CL309_RequestPowerOnPacket();
                Cl309_RequestPowerOnReplyPacket rcorec = new Cl309_RequestPowerOnReplyPacket();
                UIPara uipara;
                PhiPara phipara;
                uipara.Ia = 0;
                uipara.Ib = 0;
                uipara.Ic = 0;
                uipara.Ua = 0;
                uipara.Ub = 0;
                uipara.Uc = 0;
                phipara.PhiIa = 0;
                phipara.PhiIb = 240;
                phipara.PhiIc = 120;
                phipara.PhiUa = 0;
                phipara.PhiUb = 240;
                phipara.PhiUc = 120;
                rco.SetPara(uipara, phipara, Cus_PowerYuanJiang.H, "1.0", 50, 7, false, false);//把上一次升源值归零

                ////第一步：设备各相各次谐波关
                CL309_RequestPowerXBZongSwitchPacket xbswth = new CL309_RequestPowerXBZongSwitchPacket(0x00);
                CL309_RequestPowerXBZongSwitchReplyPacket recxbswth = new CL309_RequestPowerXBZongSwitchReplyPacket();
                FrameAry[0] = BytesToString(xbswth.GetPacketData());
                if (SendPacketWithRetry(m_PowerPort[0], xbswth, recxbswth))
                {
                    if (recxbswth.ReciveResult != RecvResult.OK)
                    {
                        //
                    }
                }



                CL309_RequestPowerOffPacket rcoA = new CL309_RequestPowerOffPacket();
                Cl309_RequestPowerOffReplyPacket rcorecA = new Cl309_RequestPowerOffReplyPacket();
                rcoA.SetPara();
                FrameAry[1] = BytesToString(rcoA.GetPacketData());
                if (sendFlag)
                {
                    if (SendPacketWithRetry(m_PowerPort[0], rcoA, rcorecA))
                    {
                        return rcorecA.ReciveResult == RecvResult.OK ? 0 : 2;
                    }
                    else
                    {
                        return 1;
                    }
                }
                else
                {
                    return 0;
                }

            }
            catch (Exception)
            {

                return -1;
            }
        }

        //PowerOn_Off

        /// <summary>
        /// 降电流 只更新电流 不更新电压
        /// </summary>
        /// <param name="FrameAry">输出上行报文</param>
        /// <returns></returns>
        public int PowerOffOnlyCurrent(out string[] FrameAry)
        {
            FrameAry = new string[1];
            try
            {
                CL309_RequestPowerOffPacket rcoA = new CL309_RequestPowerOffPacket();
                Cl309_RequestPowerOffReplyPacket rcorecA = new Cl309_RequestPowerOffReplyPacket();
                //不更新电压，只更新电流
                rcoA._bIsUpDateVoltage = false;
                rcoA.SetPara();
                FrameAry[0] = BytesToString(rcoA.GetPacketData());
                if (sendFlag)
                {
                    if (SendPacketWithRetry(m_PowerPort[0], rcoA, rcorecA))
                    {
                        return rcorecA.ReciveResult == RecvResult.OK ? 0 : 2;
                    }
                    else
                    {
                        return 1;
                    }
                }
                else
                {
                    return 0;
                }

            }
            catch (Exception)
            {

                return -1;
            }
        }
        /// <summary>
        /// 设置谐波参数
        /// </summary>
        /// <param name="Phase">相别A相电压 = 0,B相电压 = 1,C相电压 = 2,A相电流 = 3,B相电流 = 4,C相电流 = 5</param>
        /// <param name="int_XTSwitch">各相各次开关,数组元素值：0=不加谐波，1=加谐波，各次（0-31）</param>
        /// <param name="sng_Value">幅值（最高到32次）</param>
        /// <param name="sng_Phase">相位（最高到32次）</param>
        /// <param name="frameAry">输出上行报文</param>
        /// <returns></returns>
        public int SetHarmonic(int Phase, int[] int_XTSwitch, float[] sng_Value, float[] sng_Phase, out string[] frameAry)
        {
            frameAry = new string[1];
            int reValue;
            try
            {
                //1.发送一次谐波实际值
                CL309_RequestPowerXieBoPacket rc = new CL309_RequestPowerXieBoPacket();
                CL309_RequestPowerXieBoReplyPacket recv = new CL309_RequestPowerXieBoReplyPacket();
                List<string> listData = new List<string>();
                //两条指令 先发送相位   再发送幅值
                for (int i = 0; i < 2; i++)
                {

                    if (i == 0)
                    {
                        //相位
                        rc.SetPara(Convert.ToByte(Phase), Convert.ToByte(i), sng_Phase);
                    }
                    else if (i == 1)
                    {
                        //幅值
                        rc.SetPara(Convert.ToByte(Phase), Convert.ToByte(i), sng_Value);
                    }
                    listData.Add(BytesToString(rc.GetPacketData()));
                    if (sendFlag)
                    {
                        //若发送数据
                        if (SendPacketWithRetry(m_PowerPort[0], rc, recv))
                        {
                            reValue = recv.ReciveResult == RecvResult.OK ? 0 : 2;
                        }
                        else
                        {
                            reValue = 1;
                        }
                        System.Threading.Thread.Sleep(50);

                    }
                    else
                    {
                        reValue = 0;
                    }
                }
                //
                //发送分相谐波开关
                CL309_RequestPowerXBFenXiangSwitchPacket rcxb = new CL309_RequestPowerXBFenXiangSwitchPacket();

                CL309_RequestPowerXBFenXiangSwitchReplayPacket recvxb = new CL309_RequestPowerXBFenXiangSwitchReplayPacket();
                rcxb.SetPara(Convert.ToByte(Phase), int_XTSwitch);
                listData.Add(BytesToString(rcxb.GetPacketData()));
                if (sendFlag)
                {
                    if (SendPacketWithRetry(m_PowerPort[0], rcxb, recvxb))
                    {
                        reValue = recvxb.ReciveResult == RecvResult.OK ? 0 : 2;
                    }
                    else
                    {
                        reValue = 1;
                    }
                }
                else
                {
                    reValue = 0;
                }

                //
                if (listData.Count > 0)
                {
                    frameAry = listData.ToArray();
                }


            }
            catch (Exception)
            {

                return -1;
            }
            return reValue;
        }
        /// <summary>
        /// 打开谐波总开关
        /// </summary>
        /// <param name="FrameAry">输出上行报文</param>
        /// <returns></returns>
        public int SetHarmonicSwitch(out string[] FrameAry)
        {
            FrameAry = new string[1];
            try
            {
                //设置谐波打开
                CL309_RequestPowerXBZongSwitchPacket rc = new CL309_RequestPowerXBZongSwitchPacket(1);
                CL309_RequestPowerXBZongSwitchReplyPacket recv = new CL309_RequestPowerXBZongSwitchReplyPacket();
                FrameAry[0] = BytesToString(rc.GetPacketData());
                if (sendFlag)
                {
                    if (SendPacketWithRetry(m_PowerPort[0], rc, recv))
                    {
                        return recv.ReciveResult == RecvResult.OK ? 0 : 2;
                    }
                    else
                    {
                        return 1;
                    }
                }
                else
                {
                    return 0;
                }

            }
            catch (Exception)
            {

                return -1;
            }
        }
        /// <summary>
        /// 设置特殊输出命令
        /// </summary>
        /// <param name="Type">1电压跌落和短时中断2:电压逐渐变化3:逐渐关机和启动</param>
        /// <param name="FrameAry"></param>
        /// <returns></returns>
        public int SetSpecialOut(int Type, out string[] FrameAry)
        {
            FrameAry = new string[1];
            try
            {
                if (Type < 1 || Type > 3)
                    return 3;
                CL309_RequestFallOrStepUPacket rc = new CL309_RequestFallOrStepUPacket(Type - 1);
                CL309_RequestFallOrStepUPacketReplayPacket recv = new CL309_RequestFallOrStepUPacketReplayPacket();
                FrameAry[0] = BytesToString(rc.GetPacketData());
                if (sendFlag)
                {
                    if (SendPacketWithRetry(m_PowerPort[0], rc, recv))
                    {
                        return recv.ReciveResult == RecvResult.OK ? 0 : 2;
                    }
                    else
                    {
                        return 1;
                    }
                }
                else
                {
                    return 0;
                }
            }
            catch (Exception)
            {

                return -1;
            }
        }
        /// <summary>
        /// 设置标志位
        /// </summary>
        /// <param name="Flag"></param>
        /// <returns></returns>
        public int SetSendFlag(bool Flag)
        {
            sendFlag = Flag;
            return 0;
        }
        /// <summary>
        /// 解析下行报文
        /// </summary>
        /// <param name="MothedName">方法名称</param>
        /// <param name="ReFrameAry">下行报文</param>
        /// <param name="ReAry">解析的数据</param>
        /// <returns></returns>
        public int UnPacket(string MothedName, byte[] ReFrameAry, out string[] ReAry)
        {
            ReAry = new string[1];
            MothedName = MothedName.Replace(" ", "");
            int reValue;
            try
            {
                switch (MothedName)
                {
                    case "Connect":
                        {
                            // 连接 int Connect(out string[] FrameAry);
                            Cl309_RequestLinkReplyPacket recv = new Cl309_RequestLinkReplyPacket();
                            recv.ParsePacket(ReFrameAry);
                            ReAry[0] = recv.ReciveResult.ToString();
                            reValue = 0;
                        }
                        break;
                    case "DisConnect":
                        {
                            //   断开连机         int DisConnect(out string[] FrameAry);
                            reValue = 3;
                        }
                        break;
                    case "PowerOn":
                        {
                            // 升源        int PowerOn(int clfs, int glfx, string strGlys, Single sng_Ub_A, Single sng_Ub_B, Single sng_Ub_C, Single sng_Ib_A, Single sng_Ib_B, Single sng_Ib_C, int element, Single sng_Freq, bool bln_IsNxx, out string[] FrameAry);
                            Cl309_RequestPowerOnReplyPacket recv = new Cl309_RequestPowerOnReplyPacket();
                            recv.ParsePacket(ReFrameAry);
                            ReAry[0] = recv.ReciveResult.ToString();
                            reValue = 0;
                        }
                        break;
                    case "PowerOnFree":
                        {
                            //    自由升源    int PowerOnFree(double Ua, double Ub, double Uc,double Ia, double Ib, double Ic,double PhiUa, double PhiUb, double PhiUc,double PhiIa, double PhiIb, double PhiIc,float Hz,out string[] FrameAry);
                            Cl309_RequestPowerOnReplyPacket recv = new Cl309_RequestPowerOnReplyPacket();
                            recv.ParsePacket(ReFrameAry);
                            ReAry[0] = recv.ReciveResult.ToString();
                            reValue = 0;
                        }
                        break;
                    case "PowerOff":
                        {
                            //关源          int PowerOff(out string[] FrameAry);
                            Cl309_RequestPowerOffReplyPacket recv = new Cl309_RequestPowerOffReplyPacket();
                            recv.ParsePacket(ReFrameAry);
                            ReAry[0] = recv.ReciveResult.ToString();
                            reValue = 0;

                        }
                        break;
                    case "PowerOffOnlyCurrent":
                        {
                            //只关电流 int PowerOffOnlyCurrent(out string[] FrameAry);
                            Cl309_RequestPowerOffReplyPacket recv = new Cl309_RequestPowerOffReplyPacket();
                            recv.ParsePacket(ReFrameAry);
                            ReAry[0] = recv.ReciveResult.ToString();
                            reValue = 0;
                        }
                        break;
                    case "SetHarmonic":
                        {
                            //设置谐波参数         int SetHarmonic(int Phase, int[] int_XTSwitch, Single[] sng_Value, Single[] sng_Phase, out string[] frameAry);
                            CL309_RequestPowerXieBoReplyPacket recv = new CL309_RequestPowerXieBoReplyPacket();
                            recv.ParsePacket(ReFrameAry);
                            ReAry[0] = recv.ReciveResult.ToString();
                            reValue = 0;
                        }
                        break;
                    case "SetHarmonicSwitch":
                        {
                            //设置谐波总开关        int SetHarmonicSwitch(out string[] FrameAry);
                            CL309_RequestPowerXBZongSwitchReplyPacket recv = new CL309_RequestPowerXBZongSwitchReplyPacket();
                            recv.ParsePacket(ReFrameAry);
                            ReAry[0] = recv.ReciveResult.ToString();
                            reValue = 0;
                        }
                        break;
                    case "SetSpecialOut":
                        {
                            //设置特殊输出命令  int SetSpecialOut(int Type, out string[] FrameAry);
                            CL309_RequestFallOrStepUPacketReplayPacket recv = new CL309_RequestFallOrStepUPacketReplayPacket();
                            recv.ParsePacket(ReFrameAry);
                            ReAry[0] = recv.ReciveResult.ToString();
                            reValue = 0;
                        }
                        break;
                    default:
                        reValue = 3;
                        break;
                }
            }
            catch (Exception)
            {

                return -1;
            }
            return reValue;
        }



        #endregion

        /// <summary>
        /// 字节数组转字符串
        /// </summary>
        /// <param name="bytesData"></param>
        /// <returns></returns>
        private string BytesToString(byte[] bytesData)
        {
            string strRevalue = string.Empty;
            if (bytesData == null || bytesData.Length < 1)
                return strRevalue;

            strRevalue = BitConverter.ToString(bytesData).Replace("-", "");

            return strRevalue;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stPort"></param>
        /// <param name="UDPorCOM">true UDP,false COM</param>
        /// <param name="sp"></param>
        /// <param name="rp"></param>
        /// <returns></returns>
        private bool SendPacketWithRetry(StPortInfo stPort, SendPacket sp, RecvPacket rp)
        {

            {
                for (int i = 0; i < RETRYTIEMS; i++)
                {
                    if (driverBase.SendData(stPort, sp, rp) == true)
                    {
                        return true;
                    }
                    System.Threading.Thread.Sleep(300);
                }
            }

            return false;
        }
        /// <summary>
        /// 转换当前要升源的测量方式
        /// 中的测试方式定义与检定器定义不一致。
        /// </summary>
        /// <param name="Clfs">测量方式0-7</param>
        /// <param name="pd">功率方向</param>
        /// <returns>测量方式</returns>
        private byte GetClfs(Cus_Clfs Clfs, Cus_PowerFangXiang pd)
        {
            /*   三相四线有功 = 0,
         三相四线无功 = 1,
         三相三线有功 = 2,
         三相三线无功 = 3,
         二元件跨相90 = 4,
         二元件跨相60 = 5,
         三元件跨相90 = 6,
             
        三相四线=0,
        三相三线=1,
        二元件跨相90=2,
        二元件跨相60=3,
        三元件跨相90=4,
        单相=5
             
             */
            bool IsYouGong = ((pd == Cus_PowerFangXiang.ZXP) || (pd == Cus_PowerFangXiang.FXP));
            byte clfs = (byte)Clfs;
            if (clfs == 5)                            //单相台统一划分为三相四线
            {
                clfs = 0;
            }
            clfs += 2;                              //先保证后面对齐
            if (clfs < 4)                             //处理前面没有对齐部分
            {
                if (clfs == 3)
                {
                    if (IsYouGong)
                    {
                        clfs--;
                    }
                }
                else
                {
                    clfs--;
                    if (IsYouGong)
                    {
                        clfs--;
                    }
                }
            }
            return clfs;
        }

        public int ClearOL(out string[] FrameAry)
        {
            FrameAry = new string[1];
            try
            {
                ClearOLPacket s = new ClearOLPacket();
                ClearOLReplayPacket r = new ClearOLReplayPacket();

                FrameAry[0] = BytesToString(s.GetPacketData());
                if (sendFlag)
                {
                    if (SendPacketWithRetry(m_PowerPort[0], s, r))
                    {
                        return r.ReciveResult == RecvResult.OK ? 0 : 2;
                    }
                    else
                    {
                        return 1;
                    }
                }
                else
                {
                    return 0;
                }
            }
            catch (Exception)
            {
                return -1;
            }
        }
    }
}
