using System;
using System.Threading;
using LYTest.MeterProtocol.Packet;

namespace LYTest.MeterProtocol.Protocols
{
    /// <summary>
    /// 国产电能表645协议基类，适用1997、2007版本,均可继承于它
    /// </summary>
    public class CDLT645 : ProtocolBase
    {

        /// <summary>
        /// 执行命令操作(有返回)
        /// </summary>
        /// <param name="byt_Addr">地址</param>
        /// <param name="cmd">命令字</param>
        /// <param name="data">数据域</param>
        /// <param name="sequela">是否有后续帧</param>
        /// <param name="revData">返回帧数据域</param>
        /// <param name="waitSecond">等待时间（毫秒）,本参数已弃用</param>
        /// <param name="int_BitSecond">字节间隔时间（毫秒）,本参数已弃用</param>
        /// <returns></returns>
        public bool ExeCommand(byte[] addr, byte cmd, byte[] data, ref bool sequela, ref byte[] revData)
        {
            byte[] frame = OrgFrame(addr, cmd, data);

            MeterProtocolRecvPacket recvPacket = new MeterProtocolRecvPacket();
            MeterProtocolSendPacket sendPacket = new MeterProtocolSendPacket()
            {
                SendData = frame,
                IsNeedReturn = (recvPacket != null)
            };

            bool result = base.SendData(sendPacket, recvPacket);
            if (cmd == 0x11 && result == false)
            {
                result = base.SendData(sendPacket, recvPacket);
            }
            if (result)
            {
                if (CheckFrame(cmd, recvPacket.RecvData, ref sequela, ref revData))
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
        /// <param name="revData">返回帧数据域</param>
        /// <param name="retryTime">重试次数</param>
        /// <param name="waitSecond">等待时间（毫秒）,本参数已弃用</param>
        /// <param name="bitSecond">字节间隔时间（毫秒）,本参数已弃用</param>
        /// <returns></returns>
        public bool ExeCommand(byte[] addr, byte cmd, byte[] data, ref bool sequela, ref byte[] revData, int retryTime)
        {
            byte[] frame = OrgFrame(addr, cmd, data);
            MeterProtocolRecvPacket recvPacket = new MeterProtocolRecvPacket();
            MeterProtocolSendPacket sendPacket = new MeterProtocolSendPacket()
            {
                SendData = frame,
                IsNeedReturn = true
            };

            for (int i = 0; i < retryTime; i++)
            {
                bool result = base.SendData(sendPacket, recvPacket);
                if (result)
                {
                    if (CheckFrame(cmd, recvPacket.RecvData, ref sequela, ref revData))
                        return true;
                }
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
        /// <param name="revAddr">返回帧地址域</param>
        /// <param name="revData">返回帧数据域</param>
        /// <param name="waitSecond">等待时间（毫秒）本参数已弃用</param>
        /// <param name="bitSecond">字节间隔时间（毫秒）本参数已弃用</param>
        /// <returns></returns>
        public bool ExeCommand(byte[] addr, byte cmd, byte[] data, ref bool sequela, ref byte[] revData, ref byte[] revAddr)
        {
            byte[] frame = OrgFrame(addr, cmd, data);
            MeterProtocolRecvPacket recvPacket = new MeterProtocolRecvPacket();
            MeterProtocolSendPacket sendPacket = new MeterProtocolSendPacket()
            {
                FrameMean = FrameMean,
                SendData = frame,
            };

            bool result = SendData(sendPacket, recvPacket);//SendFrame(byt_Frame, int_WaitSecond, int_BitSecond);
            if (result)
            {
                if (CheckFrame(cmd, recvPacket.RecvData, ref sequela, ref revAddr, ref revData))
                    return true;
            }
            return false;
        }


        /// <summary>
        /// 执行命令操作(有返回)
        /// </summary>
        /// <param name="sendata">发送数据</param>
        /// <param name="re">返回数据包</param>
        /// <param name="retryTime">重试次数</param>
        /// <returns></returns>
        public bool ExeCommand(byte[] sendata, MeterProtocolRecvPacket re, int retryTime)
        {
            MeterProtocolRecvPacket recvPacket = new MeterProtocolRecvPacket();
            MeterProtocolSendPacket sendPacket = new MeterProtocolSendPacket()
            {
                SendData = sendata,
                IsNeedReturn = true
            };

            for (int i = 0; i < retryTime; i++)
            {
                if (base.SendData(sendPacket, recvPacket))
                {
                    re.RecvData = recvPacket.RecvData;
                    //byte[] getData = recvPacket.RecvData;
                    return true;
                }
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
            string str = protocolInfo.Setting;
            protocolInfo.Setting = "9600,e,8,1";
            MeterProtocolRecvPacket recvPacket = new MeterProtocolRecvPacket();
            MeterProtocolSendPacket sendPacket = new MeterProtocolSendPacket()
            {
                SendData = sendFrame,
                IsNeedReturn = recvPacket != null
            };

            Thread.Sleep(50);//做延时处理，不然有时候会导致485报文发送失败2015年9月22日 13:23:43
            bool b = base.SendData(sendPacket, recvPacket);
            protocolInfo.Setting = str;
            if (b && recvPacket.RecvData != null)
            {
                recvFrame = recvPacket.RecvData;
                return true;
            }
            return false;
        }

        /// <summary>
        /// 执行命令操作(无返回)
        /// </summary>
        /// <param name="addr">地址</param>
        /// <param name="cmd">命令字</param>
        /// <param name="data">数据域</param>
        /// <returns></returns>
        public bool ExeCommand(byte[] addr, byte cmd, byte[] data)
        {
            byte[] frame = OrgFrame(addr, cmd, data);
            MeterProtocolSendPacket sendPacket = new MeterProtocolSendPacket()
            {
                SendData = frame,
            };
            MeterProtocolRecvPacket recvPacket = null;//回复包设置为NULL，则不需要回复
            return SendData(sendPacket, recvPacket);
        }

        /// <summary>
        /// 645组帧
        /// </summary>
        /// <param name="addr">地址域</param>
        /// <param name="cmd">命令</param>
        /// <param name="data">数据域</param>
        /// <returns></returns>
        private byte[] OrgFrame(byte[] addr, byte cmd, byte[] data)
        {

            byte len = (byte)data.Length;
            byte[] frame = new byte[len + 12 + protocolInfo.FECount];  //68H(1)+地址(6)+68H(1)+控制码(1)+Len(1)+Data(Len)+ChkSum(1)+16H(1)  
            if (protocolInfo.FECount > 0)
                for (int i = 0; i < protocolInfo.FECount; i++)
                    frame[i] = 0xFE;
            frame[protocolInfo.FECount + 0] = 0x68;
            Array.Copy(addr, 0, frame, protocolInfo.FECount + 1, 6);
            frame[protocolInfo.FECount + 7] = 0x68;
            frame[protocolInfo.FECount + 8] = cmd;
            frame[protocolInfo.FECount + 9] = len;

            for (int i = 0; i < len; i++)
                frame[protocolInfo.FECount + 10 + i] = Convert.ToByte((data[i] + 0x33) % 256);

            for (int i = 0; i < len + 10; i++)
                frame[protocolInfo.FECount + 10 + len] += frame[protocolInfo.FECount + i];

            frame[protocolInfo.FECount + 11 + len] = 0x16;
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

            if (revFrame == null || revFrame.Length <= 0)//没有返回数据！
                return false;
            int start = Array.IndexOf(revFrame, (byte)0x68);
            //返回帧不完整！没有帧头
            if (start < 0 || start > revFrame.Length || start + 12 > revFrame.Length) //没有68开头 长度是否足够一帧 是否完整
                return false;

            if (revFrame[start + 7] != 0x68)        //找不到第二个68,返回帧不完整！
                return false;

            int len = revFrame[start + 9];
            if (start + 12 + len != revFrame.Length)//帧的长度是否与实际长度一样
                return false;

            byte chksum = 0;
            for (int i = start; i < start + len + 10; i++)
                chksum += revFrame[i];
            if (revFrame[start + len + 10] != chksum)       //校验码不正确
                return false;

            if (revFrame[start + len + 11] != 0x16)       //没有16结束
                return false;

            Array.Resize(ref addr, 6);
            Array.Copy(revFrame, start + 1, addr, 0, 6);
            //cmd
            Array.Resize(ref revData, len);    //数据域长度
            if (len > 0)
            {
                Array.Copy(revFrame, start + 10, revData, 0, len);
                for (int i = 0; i < revData.Length; i++)
                    revData[i] -= 0x33;
            }

            if ((revFrame[start + 8] & 0x1f) != cmd)//返回帧命令与下发帧不一致！
                return false;

            //是否有后续帧
            if ((revFrame[start + 8] & 0x20) == 0x20)
                sequela = true;
            else
                sequela = false;

            //是否返回操作成功     第7Bit是1则是返回，第6bit是0=成功，1=失败
            if ((revFrame[start + 8] & 0x80) == 0x80 && (revFrame[start + 8] & 0x40) == 0x00)
                return true;
            else
                return false;
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
            byte[] addr = new byte[6];
            return CheckFrame(cmd, revFrame, ref sequela, ref addr, ref revData);
        }

        public override byte[] ReadData(byte[] data)
        {
            MeterProtocolRecvPacket recvPacket = new MeterProtocolRecvPacket();
            ExeCommand(data, recvPacket, 3);
            return recvPacket.RecvData;
        }


    }
}
