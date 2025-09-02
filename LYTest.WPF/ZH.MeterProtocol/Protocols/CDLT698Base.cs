using LYTest.MeterProtocol.Packet;
using System;
using System.Threading;


namespace LYTest.MeterProtocol.Protocols
{
    public class CDLT698Base : ProtocolBase
    {

        /// <summary>
        ///  执行命令操作(有返回)
        /// </summary>
        /// <param name="cmd">控制码</param>
        /// <param name="dataAF">地址标识AF</param>
        /// <param name="pbAddr">服务器地址SA</param>
        /// <param name="dataCF">客户机地址CA</param>
        /// <param name="dataBuf">APDU</param>
        /// <param name="sequela">是否有后续帧</param>
        /// <param name="recvData">返回帧数据域</param>
        /// <param name="waitSecond">等待时间（毫秒）,本参数已弃用</param>
        /// <param name="bitSecond">字节间隔时间（毫秒）,本参数已弃用</param>
        /// <returns></returns>
        public bool ExeCommand(byte cmd, byte dataAF, byte[] pbAddr, byte dataCF, byte[] dataBuf, ref bool sequela,
                              ref byte[] recvData)
        {
            byte[] sendFrame = OrgFrame(cmd, dataAF, pbAddr, dataCF, dataBuf);
            byte[] recvFrame = new byte[0];
            if (ExeCommand(sendFrame, ref recvFrame))
            {
                if (CheckFrame(cmd, recvFrame, ref sequela, ref recvData))
                    return true;
            }
            return false;
        }

        /// <summary>
        ///  执行命令操作(有返回)
        /// </summary>
        /// <param name="sendFrame">发送命令帧</param>
        /// <param name="recvFrame">返回命令帧</param>
        /// <returns></returns>
        public bool ExeCommand(byte[] sendFrame, ref byte[] recvFrame)
        {
            MeterProtocolRecvPacket recvPacket = new MeterProtocolRecvPacket();
            MeterProtocolSendPacket sendPacket = new MeterProtocolSendPacket()
            {
                SendData = sendFrame,
                IsNeedReturn = (recvPacket != null)
            };

            Thread.Sleep(50);//做延时处理，不然有时候会导致485报文发送失败2015年9月22日 13:23:43
            bool b = base.SendData(sendPacket, recvPacket);
            if (b && recvPacket.RecvData != null)
            {
                recvFrame = recvPacket.RecvData;
                return true;
            }

            return false;
        }

        #region  //读取 地址

        /// <summary>
        /// 执行命令操作(有返回)
        /// </summary>
        /// <param name="addr">地址</param>
        /// <param name="cmd">命令字</param>
        /// <param name="data">数据域</param>
        /// <param name="sequela">是否有后续帧</param>
        /// <param name="revAddr">返回帧地址域</param>
        /// <param name="revData">返回帧数据域</param>
        /// <returns></returns>
        public bool ExeCommand(byte[] addr, byte cmd, byte[] data, ref bool sequela,
                               ref byte[] revAddr, ref byte[] revData)
        {
            byte[] sendFrame = OrgFrame(addr, cmd, data);
            MeterProtocolRecvPacket recvPacket = new MeterProtocolRecvPacket();
            MeterProtocolSendPacket sendPacket = new MeterProtocolSendPacket()
            {
                SendData = sendFrame,
            };

            bool b = SendData(sendPacket, recvPacket);
            if (b)
            {
                if (CheckFrame(cmd, recvPacket.RecvData, ref sequela, ref revAddr, ref revData))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// 执行命令操作(有返回)
        /// </summary>
        /// <param name="addr">地址</param>
        /// <param name="cmd">命令字</param>
        /// <param name="data">数据域</param>
        /// <param name="sequela">是否有后续帧</param>
        /// <param name="recvData">返回帧数据域</param>
        /// <param name="waitSecond">等待时间（毫秒）,本参数已弃用</param>
        /// <param name="bitSecond">字节间隔时间（毫秒）,本参数已弃用</param>
        /// <returns></returns>
        public bool ExeCommand(byte[] addr, byte cmd, byte[] data, ref bool sequela,
                               ref byte[] recvData)
        {
            byte[] sendFrame = OrgFrame(addr, cmd, data);
            MeterProtocolRecvPacket recvPacket = new MeterProtocolRecvPacket();
            MeterProtocolSendPacket sendPacket = new MeterProtocolSendPacket()
            {
                SendData = sendFrame,
                IsNeedReturn = (recvPacket != null)
            };

            bool b = base.SendData(sendPacket, recvPacket);
            if (cmd == 0x11 && b == false)
            {
                b = base.SendData(sendPacket, recvPacket);
            }
            if (b)
            {
                if (CheckFrame(cmd, recvPacket.RecvData, ref sequela, ref recvData))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// 698组帧
        /// </summary>
        /// <param name="addr">地址域</param>
        /// <param name="cmd">命令</param>
        /// <param name="data">数据域</param>
        /// <returns></returns>
        private byte[] OrgFrame(byte[] addr, byte cmd, byte[] data)
        {

            byte len = (byte)data.Length;
            int k = 0;
            byte[] frame = new byte[len + 12 + protocolInfo.FECount];
            //加入FE换醒符
            if (protocolInfo.FECount > 0)
                for (int i = 0; i < protocolInfo.FECount; i++)
                    frame[k += i] = 0xFE;

            frame[k++] = 0x68; //起始字符
            Array.Copy(addr, 0, frame, k, 6);

            frame[k += 6] = 0x68;
            frame[k += 1] = cmd;
            frame[k += 1] = len;
            for (int i = 0; i < len; i++)
                frame[k += 1] = Convert.ToByte((data[i] + 0x33) % 256);

            //计算校验码
            k += 1;
            for (int i = 0; i < len + 10; i++)
                frame[k] += frame[protocolInfo.FECount + i];

            frame[k += 1] = 0x16;
            return frame;
        }


        /// <summary>
        /// 解析返回帧
        /// </summary>
        /// <param name="cmd">下发命令字</param>
        /// <param name="revFrame">返回数据帧</param>
        /// <param name="sequela">是否有后续帧</param>
        /// <param name="addr">返回地址域</param>
        /// <param name="revData">返回数据域</param>
        /// <returns></returns>
        private bool CheckFrame(byte cmd, byte[] revFrame, ref bool sequela, ref byte[] addr, ref byte[] revData)
        {

            if (revFrame == null || revFrame.Length <= 0) return false;
            //没有68开头 长度是否足够一帧 是否完整
            int index = Array.IndexOf(revFrame, (byte)0x68);
            if (index < 0 || index > revFrame.Length || index + 12 > revFrame.Length) return false;


            //找不到第二个68
            if (revFrame[index + 7] != 0x68) return false;


            //数据长度与实际长度不一致
            int len = revFrame[index + 9];
            if (index + 12 + len != revFrame.Length) return false;


            //校验码不正确
            byte chk = 0;
            for (int i = index; i < index + len + 10; i++)
                chk += revFrame[i];
            if (revFrame[index + len + 10] != chk) return false;

            //没有16结束
            if (revFrame[index + len + 11] != 0x16) return false;


            Array.Resize(ref addr, 6);
            Array.Copy(revFrame, index + 1, addr, 0, 6);
            //cmd
            Array.Resize(ref revData, len);    //数据域长度
            if (len > 0)
            {
                Array.Copy(revFrame, index + 10, revData, 0, len);
                for (int i = 0; i < revData.Length; i++)
                    revData[i] -= 0x33;
            }

            //返回帧命令与下发帧不一致！
            if ((revFrame[index + 8] & 0x1f) != cmd) return false;


            //是否有后续帧
            if ((revFrame[index + 8] & 0x20) == 0x20)
                sequela = true;
            else
                sequela = false;

            //是否返回操作成功     第7Bit是1则是返回，第6bit是0=成功，1=失败
            if ((revFrame[index + 8] & 0x80) == 0x80 && (revFrame[index + 8] & 0x40) == 0x00)
                return true;
            else
                return false;
        }
        #endregion

        /// <summary>
        /// 解析返回帧
        /// </summary>
        /// <param name="revFrame">返回数据帧</param>
        /// <param name="sequela">是否有后续帧</param>
        /// <param name="addr">返回地址域</param>
        /// <param name="revData">返回数据域</param>
        /// <returns></returns>
        private bool CheckFrame(byte[] revFrame,  ref byte[] addr, ref byte[] revData)
        {

            if (revFrame == null || revFrame.Length <= 0) return false;

            //没有68开头 长度是否足够一帧 是否完整
            int index = Array.IndexOf(revFrame, (byte)0x68);
            if (index < 0 || index > revFrame.Length || index + 12 > revFrame.Length) return false;

            //数据长度与实际长度不一致！
            int len = revFrame[index + 1];
            if (index + 2 + len != revFrame.Length) return false;

            //没有16结束
            if (revFrame[index + len + 1] != 0x16) return false;


            Array.Resize(ref addr, 6);
            Array.Copy(revFrame, index + 5, addr, 0, 6);
            //cmd
            Array.Resize(ref revData, len - 15);    //数据域长度
            if (len > 0)
            {
                Array.Copy(revFrame, index + 14, revData, 0, len - 15);
            }
            return true;

        }

        /// <summary>
        /// 解析返回帧
        /// </summary>
        /// <param name="cmd">下发命令字</param>
        /// <param name="revFrame">解板帧</param>
        /// <param name="sequela">是否有后续帧</param>
        /// <param name="revData">返回数据域</param>
        /// <returns></returns>
        private bool CheckFrame(byte cmd, byte[] revFrame, ref bool sequela, ref byte[] revData)
        {
            byte[] byt_Addr = new byte[6];
            if (cmd == 0x43)
            { return CheckFrame(revFrame, ref byt_Addr, ref revData); }
            return CheckFrame(cmd, revFrame, ref sequela, ref byt_Addr, ref revData);
        }

        #region 组标准帧

        /// <summary>
        /// DLT698组帧,长度必须小于256个
        /// </summary>
        /// <param name="ControlByte">控制码</param>
        /// <param name="af">地址标识AF</param>
        /// <param name="addr">服务器地址SA</param>
        /// <param name="ca">客户机地址CA</param>
        /// <param name="dataBuf"></param>
        /// <returns></returns>
        public byte[] OrgFrame(byte ControlByte, byte af, byte[] addr, byte ca, byte[] dataBuf)
        {
            int Laddress = 6; //表地址长度
            // LL(2)+ControlByte(1)+ dataAF(1)+ pbAddr.Length+dataCF(1)+HCS(2)+dataBuf.Length+FCS(2)
            int LL = 9 + Laddress + dataBuf.Length; //计算数据长度
            byte[] frame = new byte[LL + 2];
            frame[0] = 0x68;//帧头
            frame[1] = Convert.ToByte(LL);//长度1
            frame[2] = 0x00;//长度2
            frame[3] = ControlByte;//控制码
            frame[4] = af;
            for (int i = 0; i < Laddress; i++)
            {
                frame[i + 5] = addr[i];
            }

            frame[5 + Laddress] = ca;
            byte[] checkbyte = new byte[5 + Laddress];
            for (int i = 0; i < checkbyte.Length; i++)
            {
                checkbyte[i] = frame[i + 1];
            }
            long ch = CheckCrc16(checkbyte, 5 + Laddress);
            frame[6 + Laddress] = (byte)(ch & 0xff);         //帧头检验1
            frame[7 + Laddress] = (byte)((ch >> 8) & 0xff);  //帧头检验2

            if (dataBuf != null)
            {
                for (int i = 0; i < dataBuf.Length; i++)
                {
                    frame[8 + Laddress + i] = dataBuf[i];
                }
                checkbyte = new byte[7 + Laddress + dataBuf.Length];
                for (int i = 0; i < checkbyte.Length; i++)
                {
                    checkbyte[i] = frame[i + 1];
                }

                long checkcs = CheckCrc16(checkbyte, 7 + Laddress + dataBuf.Length);
                frame[8 + Laddress + dataBuf.Length] = (byte)(checkcs & 0xff);
                frame[9 + Laddress + dataBuf.Length] = (byte)((checkcs >> 8) & 0xff);
                frame[10 + Laddress + dataBuf.Length] = 0x16;

            }
            else
            {
                frame[11] = 0x16;
            }

            return frame;
        }

        /// <summary>
        /// 计算校验输入： pbyt - 数据，iLen - 长度输出： pbytRes1 - 结果的第一字节（先发），pbytRes2 - 第二字节
        /// </summary>
        /// <param name="fcs"></param>
        /// <param name="cp"></param>
        /// <param name="len"></param>
        /// <returns></returns>
        public static long PPPfcs16(long fcs, byte[] cp, int len)
        {
            int i = 0;
            while (len-- > 0)
            {
                fcs = (fcs >> 8) ^ fcstab[(fcs ^ cp[i++]) & 0xff];
            }
            return (fcs);
        }

        public static long CheckCrc16(byte[] pbyt, int iLen)
        {
            long trialfcs;
            long PPPINITFCS16 = 0xffff;
            trialfcs = PPPfcs16(PPPINITFCS16, pbyt, iLen);
            trialfcs ^= 0xffff;
            return trialfcs;
        }

        #endregion

        #region 校验因子

        //FCS lookup table as calculated by genfcstab.
        public static int[] fcstab = new int[256] {
            0x0000, 0x1189, 0x2312, 0x329b, 0x4624, 0x57ad, 0x6536, 0x74bf,
            0x8c48, 0x9dc1, 0xaf5a, 0xbed3, 0xca6c, 0xdbe5, 0xe97e, 0xf8f7,
            0x1081, 0x0108, 0x3393, 0x221a, 0x56a5, 0x472c, 0x75b7, 0x643e,
            0x9cc9, 0x8d40, 0xbfdb, 0xae52, 0xdaed, 0xcb64, 0xf9ff, 0xe876,
            0x2102, 0x308b, 0x0210, 0x1399, 0x6726, 0x76af, 0x4434, 0x55bd,
            0xad4a, 0xbcc3, 0x8e58, 0x9fd1, 0xeb6e, 0xfae7, 0xc87c, 0xd9f5,
            0x3183, 0x200a, 0x1291, 0x0318, 0x77a7, 0x662e, 0x54b5, 0x453c,
            0xbdcb, 0xac42, 0x9ed9, 0x8f50, 0xfbef, 0xea66, 0xd8fd, 0xc974,
            0x4204, 0x538d, 0x6116, 0x709f, 0x0420, 0x15a9, 0x2732, 0x36bb,
            0xce4c, 0xdfc5, 0xed5e, 0xfcd7, 0x8868, 0x99e1, 0xab7a, 0xbaf3,
            0x5285, 0x430c, 0x7197, 0x601e, 0x14a1, 0x0528, 0x37b3, 0x263a,
            0xdecd, 0xcf44, 0xfddf, 0xec56, 0x98e9, 0x8960, 0xbbfb, 0xaa72,
            0x6306, 0x728f, 0x4014, 0x519d, 0x2522, 0x34ab, 0x0630, 0x17b9,
            0xef4e, 0xfec7, 0xcc5c, 0xddd5, 0xa96a, 0xb8e3, 0x8a78, 0x9bf1,
            0x7387, 0x620e, 0x5095, 0x411c, 0x35a3, 0x242a, 0x16b1, 0x0738,
            0xffcf, 0xee46, 0xdcdd, 0xcd54, 0xb9eb, 0xa862, 0x9af9, 0x8b70,
            0x8408, 0x9581, 0xa71a, 0xb693, 0xc22c, 0xd3a5, 0xe13e, 0xf0b7,
            0x0840, 0x19c9, 0x2b52, 0x3adb, 0x4e64, 0x5fed, 0x6d76, 0x7cff,
            0x9489, 0x8500, 0xb79b, 0xa612, 0xd2ad, 0xc324, 0xf1bf, 0xe036,
            0x18c1, 0x0948, 0x3bd3, 0x2a5a, 0x5ee5, 0x4f6c, 0x7df7, 0x6c7e,
            0xa50a, 0xb483, 0x8618, 0x9791, 0xe32e, 0xf2a7, 0xc03c, 0xd1b5,
            0x2942, 0x38cb, 0x0a50, 0x1bd9, 0x6f66, 0x7eef, 0x4c74, 0x5dfd,
            0xb58b, 0xa402, 0x9699, 0x8710, 0xf3af, 0xe226, 0xd0bd, 0xc134,
            0x39c3, 0x284a, 0x1ad1, 0x0b58, 0x7fe7, 0x6e6e, 0x5cf5, 0x4d7c,
            0xc60c, 0xd785, 0xe51e, 0xf497, 0x8028, 0x91a1, 0xa33a, 0xb2b3,
            0x4a44, 0x5bcd, 0x6956, 0x78df, 0x0c60, 0x1de9, 0x2f72, 0x3efb,
            0xd68d, 0xc704, 0xf59f, 0xe416, 0x90a9, 0x8120, 0xb3bb, 0xa232,
            0x5ac5, 0x4b4c, 0x79d7, 0x685e, 0x1ce1, 0x0d68, 0x3ff3, 0x2e7a,
            0xe70e, 0xf687, 0xc41c, 0xd595, 0xa12a, 0xb0a3, 0x8238, 0x93b1,
            0x6b46, 0x7acf, 0x4854, 0x59dd, 0x2d62, 0x3ceb, 0x0e70, 0x1ff9,
            0xf78f, 0xe606, 0xd49d, 0xc514, 0xb1ab, 0xa022, 0x92b9, 0x8330,
            0x7bc7, 0x6a4e, 0x58d5, 0x495c, 0x3de3, 0x2c6a, 0x1ef1, 0x0f78
        };
        #endregion

    }
}
