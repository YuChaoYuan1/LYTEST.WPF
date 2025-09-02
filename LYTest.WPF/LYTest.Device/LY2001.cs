using LY.SocketModule.Packet;
using LYTest.Device.SocketModule;
using LYTest.Device.Struct;
using System;
using System.Collections.Generic;

namespace LYTest.Device
{
    public class LY2001 : DriverBase
    {
        #region 通用属性方法

        /// <summary>
        /// 获取1-n的版本
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="ver"></param>
        /// <param name="frms"></param>
        /// <returns></returns>
        public int GetVersion(int pos, out string ver, out string[] frms)
        {
            ver = "";
            frms = new string[1];


            ZH1104_RequestVersionPacket rc2 = new ZH1104_RequestVersionPacket(pos);
            ZH1104_RequestVersionReplyPacket recv2 = new ZH1104_RequestVersionReplyPacket();
            frms[0] = BitConverter.ToString(rc2.GetPacketData()).Replace("-", "");
            if (SendPacketWithRetry(rc2, recv2))
            {
                ver = recv2.Version;
                return recv2.ReciveResult == RecvResult.OK ? 0 : 2; ;
            }
            else
            {
                return 1;
            }


        }


        #endregion



        /// <summary>
        /// 温度台控制温控板及电压
        /// </summary>
        /// <param name="meterType">温控板类型-1单相-2三相直接-3三相互感</param>
        /// <param name="ub">电压1=57.7--2=100--3=200</param>
        /// <returns></returns>
        public int TemperaturePowerOn(int meterType, float ub)
        {
            byte[] data = new byte[8];
            byte meter = 0;
            byte u = 0;
            switch (meterType)
            {
                case 1:
                    meter = 4;
                    break;
                case 2:
                    meter = 5;
                    break;
                case 3:
                    meter = 6;
                    break;
                default:
                    break;
            }

            if (0 < ub && ub < 60) u = 15;
            else if (60 < ub && ub < 120) u = 16;
            else if (120 < ub && ub < 240) u = 17;


            //long value = 0;

            if (meter != 0 && u != 0)
            {
                data = BitConverter.GetBytes(0x00000000 | (1 << meter) | (1 << u));

            }

            Array.Reverse(data);
            SendContrnlTypePacket send = new SendContrnlTypePacket();
            RecvContrnlTypeReplyPacket reslut = new RecvContrnlTypeReplyPacket();
            send.SetPara("6000", data);
            if (SendPacketWithRetry(send, reslut))
            {
                return reslut.ReciveResult == RecvResult.OK ? 0 : 1;
            }
            else
            {
                return 1;
            }


        }

        /// <summary>
        /// 设置三色灯
        /// </summary>
        /// <returns></returns>
        public bool SetTricolorLamp(int colorNum)
        {
            long LightColor = 1;
            SendContrnlTypePacket send = new SendContrnlTypePacket();
            RecvContrnlTypeReplyPacket reslut = new RecvContrnlTypeReplyPacket();

            byte[] data = new byte[2] { (byte)(LightColor + colorNum), 1 };
            send.SetPara("6001", data);
            if (SendPacketWithRetry(send, reslut))
            {
                return reslut.ReciveResult == RecvResult.OK;
            }
            else
            {
                return false;
            }


        }

        /// <summary>
        /// 独立控制继电器
        /// </summary>
        /// <param name="number">通道号</param>
        /// <param name="open">0关闭，1输出</param>
        /// <returns></returns>
        public int MultiControl(byte number, byte open)
        {
            byte[] data = new byte[2] { number, open };//1-34

            SendContrnlTypePacket send = new SendContrnlTypePacket();
            RecvContrnlTypeReplyPacket reslut = new RecvContrnlTypeReplyPacket();
            send.SetPara("6001", data);
            if (SendPacketWithRetry(send, reslut))
            {
                return reslut.ReciveResult == RecvResult.OK ? 0 : 1;
            }
            else
            {
                return 1;
            }

        }
        /// <summary>
        /// 多通道多功能板控制
        /// </summary>
        /// <param name="bitNumber">通道号</param>
        /// <returns></returns>
        public int MultiControls(List<int> bitNumber)
        {
            long value = 0;
            for (int i = 0; i < bitNumber.Count; i++)
            {
                if (bitNumber[i] == 0) continue;
                value |= 1L << (bitNumber[i] - 1);
            }
            byte[] data = BitConverter.GetBytes(value);
            Array.Reverse(data);
            SendContrnlTypePacket send = new SendContrnlTypePacket();
            RecvContrnlTypeReplyPacket reslut = new RecvContrnlTypeReplyPacket();
            send.SetPara("6000", data);
            if (SendPacketWithRetry(send, reslut))
            {
                return reslut.ReciveResult == RecvResult.OK ? 0 : 1;
            }
            else
            {
                return 1;
            }


        }

        public int WriteDatas(string RegAddr, byte[] data)
        {
            if (string.IsNullOrWhiteSpace(RegAddr) || RegAddr.Length != 4
                || data == null || data.Length == 0) return 2;

            SendContrnlTypePacket send = new SendContrnlTypePacket();
            RecvContrnlTypeReplyPacket reslut = new RecvContrnlTypeReplyPacket();
            send.SetPara(RegAddr, data);
            if (SendPacketWithRetry(send, reslut))
            {
                return reslut.ReciveResult == RecvResult.OK ? 0 : 1;
            }
            else
            {
                return 1;
            }

        }


        /// <summary>
        /// 设置工频磁场装置磁场线圈电机旋转
        /// </summary>
        /// <param name="dev">选择电机控制对象，0表示磁场线圈电机，1表示挂表座旋转电机。</param>
        /// <param name="angle">旋转角度，下发0表示复位到0°位置上，负数角度表示逆时针旋转，正数表示往顺时针方向旋转。挂表座旋转电机只在0~90°</param>
        /// <returns></returns>
        public int Fun7000H(byte dev, int angle)
        {
            byte[] data = new byte[3];
            data[0] = dev;

            int value = angle * 100;
            data[2] = (byte)(value & 0xFF);
            value >>= 8;
            data[1] = (byte)(value & 0xFF);

            SendContrnlTypePacket send = new SendContrnlTypePacket();
            RecvContrnlTypeReplyPacket reslut = new RecvContrnlTypeReplyPacket();
            send.SetPara("7000", data);
            if (SendPacketWithRetry(send, reslut))
            {
                return reslut.ReciveResult == RecvResult.OK ? 0 : 1;
            }
            else
            {
                return 1;
            }

        }

        /// <summary>
        /// 读取工频磁场装置磁场线圈电机角度
        /// </summary>
        /// <param name="angle1">磁场电机角度</param>
        /// <param name="angle2">挂表电机角度</param>
        /// <returns></returns>
        public int Fun7001H(out float angle1, out float angle2)
        {
            angle1 = 0;
            angle2 = 0;
            byte[] data = new byte[0];
            LY2001_ReadSendPacket req = new LY2001_ReadSendPacket();
            req.SetPara("7001", data);
            LY2001_ReadReplyPacket res = new LY2001_ReadReplyPacket();
            if (SendPacketWithRetry(req, res))
            {
                if (res.ReciveResult == RecvResult.OK)
                {
                    ushort d1 = Convert.ToUInt16(res.Data[2].ToString("X2") + res.Data[3].ToString("X2"), 16);
                    ushort d2 = Convert.ToUInt16(res.Data[4].ToString("X2") + res.Data[5].ToString("X2"), 16);
                    angle1 = (short)d1 / 100f;
                    angle2 = (short)d2 / 100f;
                }

                return res.ReciveResult == RecvResult.OK ? 0 : 1;
            }
            else
            {
                return 1;
            }
        }

    }
    #region 发送返回接口
    /// <summary>
    /// 发送命令
    /// </summary>
    internal class SendContrnlTypePacket : LY2001_SendPacket
    {
        /// <summary>
        /// 功能码。用字符串然后解析成16进制
        /// </summary>
        public string Cmd = "";
        /// <summary>
        /// 数据
        /// </summary>
        public byte[] Data;

        public byte ControlTyep = 0x10;
        public SendContrnlTypePacket()
            : base()
        { }

        /// <summary>
        /// 设置参数
        /// </summary>
        /// <param name="contrnlType">控制类型，读还是写0x10,0x13</param>
        /// <param name="cmd">命令码1002</param>
        /// <param name="bwNum">表位号</param>
        /// <param name="data1">数据</param>
        public void SetPara(string cmd, byte[] data1)
        {

            Cmd = cmd;
            Data = data1;
        }

        protected override byte[] GetBody()
        {
            ByteBuffer buf = new ByteBuffer();
            buf.Initialize();
            buf.Put(0x13); //cmd1002
            buf.Put(Cmd.ToBytes()); //cmd1002
            buf.Put(Data);
            return buf.ToByteArray();
        }

        ///// <summary>
        ///// 16进制的字符串放到字节数组中
        ///// </summary>
        ///// <param name="hexString"></param>
        ///// <returns></returns>
        //private byte[] HexStrrinToToHexByte(string hexString)
        //{
        //    hexString = hexString.Replace(" ", "");
        //    if ((hexString.Length % 2) != 0)
        //        hexString = hexString.Insert(hexString.Length - 1, 0.ToString());
        //    byte[] returnBytes = new byte[hexString.Length / 2];
        //    for (int i = 0; i < returnBytes.Length; i++)
        //        returnBytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
        //    return returnBytes;
        //}
    }
    /// <summary>
    /// 返回命令
    /// </summary>
    internal class RecvContrnlTypeReplyPacket : LY2001_RecvPacket
    {
        //public object OutData ;
        protected override void ParseBody(byte[] data)
        {
            if (data == null)
                ReciveResult = RecvResult.DataError;
            else
            {
                switch (data.Length)
                {
                    case 16:
                        ReciveResult = RecvResult.OK;
                        break;
                    case 9:
                        ReciveResult = data[4] == 0x4B ? RecvResult.OK : RecvResult.OrderFail;
                        break;
                    default:
                        ReciveResult = RecvResult.Unknow;
                        break;
                }

            }
        }
    }

    #endregion


    #region 读取数据
    /// <summary>
    /// 发送命令
    /// </summary>
    internal class LY2001_ReadSendPacket : LY2001_SendPacket
    {
        /// <summary>
        /// 功能码。用字符串然后解析成16进制
        /// </summary>
        public string Cmd = "";
        /// <summary>
        /// 数据
        /// </summary>
        public byte[] Data;

        public LY2001_ReadSendPacket()
            : base()
        { }

        /// <summary>
        /// 设置参数
        /// </summary>
        /// <param name="cmd">命令码1002</param>
        /// <param name="data">数据</param>
        public void SetPara(string cmd, byte[] data)
        {

            Cmd = cmd;
            Data = data;
        }

        protected override byte[] GetBody()
        {
            ByteBuffer buf = new ByteBuffer();
            buf.Initialize();
            buf.Put(0x10); //读
            buf.Put(Cmd.ToBytes()); //cmd1002
            if (Data.Length > 0)
                buf.Put(Data);
            return buf.ToByteArray();
        }

        ///// <summary>
        ///// 16进制的字符串放到字节数组中
        ///// </summary>
        ///// <param name="hexString"></param>
        ///// <returns></returns>
        //private byte[] HexStrrinToToHexByte(string hexString)
        //{
        //    hexString = hexString.Replace(" ", "");
        //    if ((hexString.Length % 2) != 0)
        //        hexString = hexString.Insert(hexString.Length - 1, 0.ToString());

        //    byte[] returnBytes = new byte[hexString.Length / 2];
        //    for (int i = 0; i < returnBytes.Length; i++)
        //        returnBytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
        //    return returnBytes;
        //}
    }
    /// <summary>
    /// 返回命令
    /// </summary>
    internal class LY2001_ReadReplyPacket : LY2001_RecvPacket
    {
        public byte[] Data { get; private set; }

        protected override void ParseBody(byte[] data)
        {
            if (data == null)
                ReciveResult = RecvResult.DataError;
            else
            {
                if (data[0] == 0x90)
                {
                    Data = new byte[data.Length - 1];
                    Array.Copy(data, 1, Data, 0, Data.Length);
                    ReciveResult = RecvResult.OK;
                }
                else
                    ReciveResult = RecvResult.OrderFail;
            }
        }
    }

    #endregion



    #region 读版本

    internal class ZH1104_RequestVersionPacket : LY2001_SendPacket
    {
        public bool IsLink = true;

        public ZH1104_RequestVersionPacket(int bw)
            : base()
        {
            //this.ToID = Convert.ToByte(bw);
        }

        protected override byte[] GetBody()
        {
            ByteBuffer buf = new ByteBuffer();
            buf.Initialize();
            buf.Put(0x10);
            buf.Put(0x30);
            buf.Put(0x02);
            return buf.ToByteArray();
        }

        public override string GetPacketResolving()
        {
            return "读版本";
        }
    }

    internal class ZH1104_RequestVersionReplyPacket : LY2001_RecvPacket
    {
        public string Version { get; private set; }
        protected override void ParseBody(byte[] data)
        {
            if (data == null || data.Length < 10)
                ReciveResult = RecvResult.DataError;
            else
            {
                if (data[4] == 0x90)
                {
                    Version = BitConverter.ToString(data, 7, 6).Replace("-", "");
                    ReciveResult = RecvResult.OK;
                }
                else
                    ReciveResult = RecvResult.Unknow;
            }
        }
        public override bool ParsePacket(List<byte> data)
        {
            if (data == null || data.Count < 10)
                ReciveResult = RecvResult.DataError;
            else
            {
                if (data[4] == 0x90)
                {
                    Version = BitConverter.ToString(data.ToArray(), 7, 6).Replace("-", "");
                    ReciveResult = RecvResult.OK;
                }
                else
                {
                    ReciveResult = RecvResult.Unknow;
                }
            }

            return true;
        }
        public override string GetPacketResolving()
        {
            return $"返回：{Version}";
        }
    }
    #endregion


    internal abstract class LY2001_SendPacket : SendPacket
    {

        public LY2001_SendPacket() { IsNeedReturn = true; }
        public LY2001_SendPacket(bool needReplay) { IsNeedReturn = needReplay; }

        /// <summary>
        /// 组帧
        /// </summary>
        /// <returns>完整的数据包内容</returns>
        public override byte[] GetPacketData()
        {
            ByteBuffer buf = new ByteBuffer();
            buf.Initialize();
            buf.Put(0x68);        //帧头
            buf.Put(0x01);              //发信节点
            buf.Put(0xFE);              //受信节点
            byte[] body = GetBody();
            if (body == null)
            {
                return null;
            }

            byte packetLength = (byte)(body.Length + 5);//帧头4字节+CS一字节
            buf.Put(packetLength);      //帧长度
            buf.Put(body);              //数据域 
            byte chkSum = Fun.GetChkSum(buf.ToByteArray(), 1);
            buf.Put(chkSum);
            return buf.ToByteArray();
        }

        protected abstract byte[] GetBody();
    }

    /// <summary>
    ///接收数据包基类
    /// </summary>
    internal abstract class LY2001_RecvPacket : RecvPacket
    {
        /// <summary>
        /// 包头
        /// </summary>
        protected byte PacketHead = 0x81;
        /// <summary>
        /// 发信节点
        /// </summary>
        protected byte MyID = 0x80;
        /// <summary>
        /// 受信节点
        /// </summary>
        protected byte ToID = 0x10;
        /// <summary>
        /// 解析数据包
        /// </summary>
        /// <param name="buf">缓冲区接收到的数据包内容</param>
        /// <returns>解析是否成功</returns>
        public override bool ParsePacket(List<byte> buf)
        {
            if (buf.Count < 9) return false;

            int h68 = buf.IndexOf(0x68);
            if (h68 < 0) return false;
            if (buf.Count < h68 + 8) return false;
            if (buf[h68 + 1] != 0xFE) return false;
            if (buf[h68 + 2] != 0x01) return false;

            byte len = buf[h68 + 3];
            if (h68 + len > buf.Count) return false;
            //buf[h68 + 4] // 功能码
            byte[] data = new byte[len - 6];
            buf.CopyTo(h68 + 5, data, 0, len - 6);

            byte ck1 = buf[h68 + len - 1]; //校验码
                                           //计算校验码
            byte ck = Fun.GetChkSum(buf, h68 + 1, len - 2);
            if (ck != ck1) return false;


            ParseBody(data);
            return true;
        }

        /// <summary>
        /// 解析数据域
        /// </summary>
        /// <param name="data">数据域</param>
        protected abstract void ParseBody(byte[] data);


        /// <summary>
        /// 单个字节由低位向高位取值，
        /// </summary>
        /// <param name="input">单个字节</param>
        /// <param name="index">起始0,1,2..7</param>
        /// <returns></returns>
        protected int GetbitValue(byte input, int index)
        {
            int value;
            value = index > 0 ? input >> index : input;
            return value &= 1;
        }

        ///// <summary>
        ///// 3字节转换为Float
        ///// </summary>
        ///// <param name="bytData"></param>
        ///// <param name="dotLen"></param>
        ///// <returns></returns>
        //protected float Get3ByteValue(byte[] bytData, int dotLen)
        //{
        //    float data = bytData[0] << 16;
        //    data += bytData[1] << 8;
        //    data += bytData[2];

        //    data = (float)(data / Math.Pow(10, dotLen));
        //    return data;
        //}

        /////<summary>
        ///// 替换byteSource目标位的值
        /////</summary>
        /////<param name="byteSource">源字节</param>
        /////<param name="location">替换位置(0-7)</param>
        /////<param name="value">替换的值(1-true,0-false)</param>
        /////<returns>替换后的值</returns>
        //protected byte ReplaceTargetBit(byte byteSource, short location, bool value)
        //{
        //    byte baseNum = (byte)(Math.Pow(2, location + 1) / 2);
        //    return ReplaceTargetBit(location, value, byteSource, baseNum);
        //}

        /////<summary>
        ///// 替换byteSource目标位的值
        /////</summary>
        /////<param name="location"></param>
        /////<param name="value">替换的值(1-true,0-false)</param>
        /////<param name="byteSource"></param>
        /////<param name="baseNum">与 基数(1,2,4,8,16,32,64,128)</param>
        /////<returns></returns>
        //private byte ReplaceTargetBit(short location, bool value, byte byteSource, byte baseNum)
        //{
        //    bool locationValue = GetbitValue(byteSource, location) == 1;
        //    if (locationValue != value)
        //    {
        //        return (byte)(value ? byteSource + baseNum : byteSource - baseNum);
        //    }
        //    return byteSource;
        //}
    }


}
