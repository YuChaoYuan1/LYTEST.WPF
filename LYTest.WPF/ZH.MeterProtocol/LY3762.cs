using LYTest.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using LYTest.MeterProtocol.Device;
using LYTest.MeterProtocol.Protocols;
using LYTest.MeterProtocol.SocketModule.Packet;
using LYTest.MeterProtocol.Struct;

namespace LYTest.MeterProtocol
{
    public class LY3762 : ProtocolBase
    {

        #region 载波
        /// <summary>
        /// 打包645成376.2
        /// </summary>
        /// <param name="Frame645">645完整帧</param>
        public void Packet645To3762Frame(byte[] Frame645, out byte[] RFrame645)
        {
            RFrame645 = null;
            if (App.CarrierInfo.CarrierType.IndexOf("2041") != -1)
            {
                if (App.CarrierInfo.IsRoute) //带路由模式
                {
                    string[] FrameAry = new string[1];

                    CL3762_RequestAFN02Packet res = new CL3762_RequestAFN02Packet(Frame645);
                    CL3762_RequestAFN02ReplayPacket req = new CL3762_RequestAFN02ReplayPacket();
                    FrameAry[0] = BytesToString(res.GetPacketData());
                    if (SendPacketWithRetry("0", res, req))//
                    {
                        RFrame645 = req.Frame645;
                    }
                }
                else
                {
                    string[] FrameAry = new string[1];
                    CL3762_RequestAFN13Packet sa13 = new CL3762_RequestAFN13Packet(Frame645);
                    CL3762_RequestAFN13ReplayPacket ra13 = new CL3762_RequestAFN13ReplayPacket();
                    FrameAry[0] = BytesToString(sa13.GetPacketData());
                    if (SendPacketWithRetry("0", sa13, ra13))
                    {
                        RFrame645 = ra13.Frame645;
                    }
                }
            }
            else
            {
                Comm_SendPacket sa = new Comm_SendPacket
                {
                    bytSendByte = Frame645
                };
                Comm_RecvPacket ra = new Comm_RecvPacket();
                if (SendPacketWithRetry("0", sa, ra))//
                {
                    int s = -1;
                    for (int i = 0; i < ra.RecvData.Length; i++)
                    {
                        if (ra.RecvData[i] == 0x68)
                        {
                            s = i;
                            break;
                        }
                    }

                    //int s = Array.IndexOf(ra.RecvData, 0x68);
                    if (s >= 0)       //没有帧起始码 0x68
                    {
                        int l = ra.RecvData.Length - s;

                        RFrame645 = new byte[l];
                        Array.Copy(ra.RecvData, s, RFrame645, 0, l);
                    }
                }
            }
        }
        /// <summary>
        /// 初始化命令
        /// </summary>
        /// <param name="fn">Fn</param>
        public void PacketToCarrierInit(int fn, int bwIndex)
        {
            CL3762_RequestAFN01Packet sa01 = new CL3762_RequestAFN01Packet(fn);
            CL3762_RequestAFN01ReplayPacket ra01 = new CL3762_RequestAFN01ReplayPacket();
            string name = "COM_" + bwIndex;
            SendPacketWithRetry(name, sa01, ra01);
        }

        /// <summary>
        /// 不带数据域，返回确认/否认帧的命令
        /// </summary>
        /// <param name="byt_AFN">AFN</param>
        /// <param name="int_Fn">Fn</param>
        public void PacketToCarrierInit(byte afn, int fn, int bwIndex)
        {
            CL3762_RequestAFN01Packet sa01 = new CL3762_RequestAFN01Packet(afn, fn);
            CL3762_RequestAFN01ReplayPacket ra01 = new CL3762_RequestAFN01ReplayPacket();
            SendPacketWithRetry(bwIndex.ToString(), sa01, ra01);//

        }
        /// <summary>
        /// 控制命令
        /// </summary>
        /// <param name="fn">Fn</param>
        /// <param name="data">数据域</param>
        public void PacketToCarrierCtr(int fn, byte[] data, int bwIndex)
        {
            if (App.CarrierInfo.CarrierType.IndexOf("2041") != -1)
            {
                CL3762_RequestAFN05Packet sa05 = new CL3762_RequestAFN05Packet(fn, data);
                CL3762_RequestAFN05ReplayPacket ra05 = new CL3762_RequestAFN05ReplayPacket();
                SendPacketWithRetry(bwIndex.ToString(), sa05, ra05);

            }
        }
        /// <summary>
        /// 添加载波从节点
        /// </summary>
        /// <param name="fn">Fn</param>
        /// <param name="data">电表地址，反转</param>
        public void PacketToCarrierAddAddr(int fn, byte[] data, int bwIndex)
        {
            CL3762_RequestAFN11Packet sa11 = new CL3762_RequestAFN11Packet(fn, data);
            CL3762_RequestAFN11ReplayPacket ra11 = new CL3762_RequestAFN11ReplayPacket();
            SendPacketWithRetry(bwIndex.ToString(), sa11, ra11);
        }
        /// <summary>
        /// 查询载波从节点
        /// </summary>
        /// <param name="fn">Fn</param>
        /// <param name="data">电表地址，反转</param>
        public Dictionary<string, DataUnit> CarrierGetNodesAddr(int start, int count)
        {
            Q3762_RequestAFN10Packet s = new Q3762_RequestAFN10Packet(2, start, count);
            Q3762_RequestAFN10ReplayPacket r = new Q3762_RequestAFN10ReplayPacket();
            SendPacketWithRetry("0", s, r);
            return r.Data;
        }

        public override Dictionary<string, string> ReadHplcID()
        {
            CL3762_RequestAFN10ReplayPacket req = new CL3762_RequestAFN10ReplayPacket();
            CL3762_RequestAFN10Packet res = new CL3762_RequestAFN10Packet(112);


            Thread.Sleep(1000);//做延时处理，不然有时候会导致485报文发送失败2015年9月22日 13:23:43
            base.SendData("0", res, req);

            return req.Data;
        }

        #endregion


        private bool SendPacketWithRetry(string PortName, SendPacket sp, RecvPacket rp)
        {
            for (int i = 0; i < 2; i++)
            {
                if (SendData(PortName, sp, rp) == true)
                {
                    return true;
                }
                Thread.Sleep(10);
            }

            return false;
        }
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
    }
}
