using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using ZH.SocketModule.Packet;
using ZH;
using ZH.Enum;
using ZH.Struct;

namespace E_ZH311
{
    #region ZH311标准表





    #region ZH311表联机指令
    /// <summary>
    /// 表联机/脱机请求包
    /// </summary>
    internal class ZH311_RequestLinkPacket : ZH311SendPacket
    {
        public bool IsLink = true;

        public ZH311_RequestLinkPacket()
            : base()
        { }
        protected override byte[] GetBody()
        {
            ByteBuffer buf = new ByteBuffer();
            buf.Initialize();
            buf.Put(0x10);          //命令 
            buf.Put(0x10);
            buf.Put(0x00);
            return buf.ToByteArray();
        }
    }
    /// <summary>
    /// ZH311表联机 返回指令
    /// </summary>
    internal class ZH311_RequestLinkReplyPacket : ZH311RecvPacket
    {
        protected override void ParseBody(byte[] data)
        {
            if (data == null || data.Length < 4)
                ReciveResult = RecvResult.DataError;
            else
            {
                if (data[0] == 0x90)
                    ReciveResult = RecvResult.OK;
                else
                    ReciveResult = RecvResult.Unknow;
            }
        }
    }
    #endregion



    #region 启停功能部分
    /// <summary>
    /// 标准表联机/脱机请求包
    /// </summary>
    internal class ZH3130_RequestStartStopacket : ZH311SendPacket
    {
        public byte[] byteData = new byte[0];
        public ZH3130_RequestStartStopacket()
            : base()
        { }

        public void SetPara(string strData)
        {
            byteData = Encoding.ASCII.GetBytes(strData);
        }
        protected override byte[] GetBody()
        {
            ByteBuffer buf = new ByteBuffer();
            buf.Initialize();
            //buf.Put(0xA0);          //命令 
            //buf.Put(0x02);
            buf.Put(byteData);
            buf.Put(0x0D);
            return buf.ToByteArray();
        }

        public override string GetPacketResolving()
        {
            string strResolve = " 。";
            return strResolve;
        }
    }
    /// <summary>
    /// 标准表，联机返回指令
    /// </summary>
    internal class ZH3130_RequestStartStoReplyPacket : ZH311RecvPacket
    {
        protected override void ParseBody(byte[] data)
        {
            if (data == null || data.Length != 8)
                ReciveResult = RecvResult.DataError;
            else
            {
                if (data[0] == 0x50)
                    ReciveResult = RecvResult.OK;
                else
                    ReciveResult = RecvResult.Unknow;
            }
        }
        public override string GetPacketResolving()
        {
            string strResolve = "返回：" + ReciveResult.ToString();
            return strResolve;
        }
    }
    #endregion


    #region 模式设置
    /// <summary>
    /// 标准表联机/脱机请求包
    /// </summary>
    internal class ZH3130_RequestSetModepacket : ZH311SendPacket
    {
        public byte[] _autoOrManuaDatal = new byte[0];
        public byte[] _connectionModeData = new byte[0];
        public byte[] _tableData = new byte[0];


        public ZH3130_RequestSetModepacket()
            : base()
        { }

        public void SetPara(string autoOrManuaDatal, string connectionModeData, string tableData)
        {
            _autoOrManuaDatal = Encoding.ASCII.GetBytes(autoOrManuaDatal);
            _connectionModeData = Encoding.ASCII.GetBytes(connectionModeData);
            _tableData = Encoding.ASCII.GetBytes(tableData);

        }
        protected override byte[] GetBody()
        {
            ByteBuffer buf = new ByteBuffer();
            buf.Initialize();
            //buf.Put(0xA0);          //命令 
            //buf.Put(0x02);
            buf.Put(_autoOrManuaDatal);
            buf.Put(_connectionModeData);
            buf.Put(_tableData);
            buf.Put(0x0D);
            return buf.ToByteArray();
        }

        public override string GetPacketResolving()
        {
            string strResolve = " 。";
            return strResolve;
        }
    }
    /// <summary>
    /// 标准表，联机返回指令
    /// </summary>
    internal class ZH3130_RequestSetModeReplyPacket : ZH311RecvPacket
    {
        protected override void ParseBody(byte[] data)
        {
            if (data == null || data.Length != 8)
                ReciveResult = RecvResult.DataError;
            else
            {
                if (data[0] == 0x50)
                    ReciveResult = RecvResult.OK;
                else
                    ReciveResult = RecvResult.Unknow;
            }
        }
        public override string GetPacketResolving()
        {
            string strResolve = "返回：" + ReciveResult.ToString();
            return strResolve;
        }
    }
    #endregion
    #region ZH311表联机指令
    /// <summary>
    /// 表数据请求包
    /// </summary>
    internal class ZH311_RequestDataPacket : ZH311SendPacket
    {
        private byte _cmd = 0x10;


        byte[] byteData = new byte[0];
        public ZH311_RequestDataPacket()
            : base()
        { }
        public void SetPara(byte Cmd, byte[] byteda)
        {
            _cmd = Cmd;
            byteData = byteda;
        }
        public void SetPara(byte Cmd, Dictionary<string, string> dictionaryData)
        {
            _cmd = Cmd;

            int dataLenght = 0;
            int dataIndex = 0;
            foreach (var dic in dictionaryData)
            {
                string strRegn = dic.Key;
                if (strRegn.Length == 4)
                {
                    dataLenght += dic.Key.Length / 2;
                }
                else
                { continue; }
                if (_cmd == 0x10) continue;
                dataLenght += dic.Value.Length / 2;
            }

            byteData = new byte[dataLenght];



            foreach (var dic in dictionaryData)
            {


                string strRegn = dic.Key;
                if (strRegn.Length == 4)
                {

                    strToToHexByte(dic.Key).CopyTo(byteData, dataIndex);
                    dataIndex += 2;
                }
                else
                { continue; }
                if (_cmd == 0x10) continue;


                strToToHexByte(dic.Value).CopyTo(byteData, dataIndex);
                dataIndex = dataIndex + dic.Value.Length / 2;


            }





            //byte[] strSend2 = new byte[10];
            //ASCIIEncoding charToASCII = new ASCIIEncoding();
            //Array.Copy(charToASCII.GetBytes(dic.ToString().ToCharArray()), 0, strSend2, 0, 10);
            //strSend2.CopyTo(byteData, 12);


        }




        protected override byte[] GetBody()
        {
            ByteBuffer buf = new ByteBuffer();
            buf.Initialize();
            buf.Put(_cmd);          //命令 
            buf.Put(byteData);

            return buf.ToByteArray();
        }

        private static byte[] strToToHexByte(string hexString)
        {
            hexString = hexString.Replace(" ", "");
            if ((hexString.Length % 2) != 0)
                hexString = hexString.Insert(hexString.Length - 1, 0.ToString());
            byte[] returnBytes = new byte[hexString.Length / 2];
            for (int i = 0; i < returnBytes.Length; i++)
                returnBytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
            return returnBytes;

        }
    }
    /// <summary>
    /// ZH311表 数据 返回指令
    /// </summary>
    internal class ZH311_RequestDataReplyPacket : ZH311RecvPacket
    {

        public byte[] revbyteData = new byte[0];
        protected override void ParseBody(byte[] data)
        {
            if (data == null)
                ReciveResult = RecvResult.DataError;
            else
            {
                if (data[0] == 0x90)
                {
                    ReciveResult = RecvResult.OK;
                    revbyteData = data;
                }
                else
                {
                    ReciveResult = RecvResult.Unknow;
                }
            }
        }
        public override string GetPacketResolving()
        {
            return base.GetPacketResolving();
        }
    }
    #endregion



    #endregion

}




