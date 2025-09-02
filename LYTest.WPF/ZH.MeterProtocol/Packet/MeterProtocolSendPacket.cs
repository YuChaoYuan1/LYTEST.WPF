using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LYTest.MeterProtocol.Packet
{
    /// <summary>
    /// 电能表数据发送包基类
    /// </summary>
    public class MeterProtocolSendPacket :SocketModule.Packet.SendPacket
    {
        /// <summary>
        /// 命令桢意义
        /// </summary>
        public string FrameMean { get; set; }
        public byte[] SendData { get; set; }

        public override int WaiteTime()
        {
            return 3000;
        }
        public override byte[] GetPacketData()
        {
            return SendData;
        }

        public override string GetPacketResolving()
        {
            return FrameMean;
        }
        /// <summary>
        /// 打包645成载波，TODO:这里固定成了07
        /// </summary>
        public void PacketTo3762(out byte[] frame)
        {

            int k = 0;
            int len = SendData.Length;
            //这里会去掉换醒符FE
            for (int i = 0; i < len; i++)
            {
                if (SendData[i] == 0x68)
                {
                    k = i;
                    break;
                }
            }
            byte[] data = new byte[len - k];
            Array.Copy(SendData, k, data, 0, data.Length);
          //string str=  BitConverter.ToString(SendData);
            App.LY3762.Packet645To3762Frame(data, out frame);
        }
    }
}
