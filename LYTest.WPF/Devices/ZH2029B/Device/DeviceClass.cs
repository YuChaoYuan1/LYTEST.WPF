using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using CLOU.SocketModule.Packet;
using CLOU;
using CLOU.Enum;
using CLOU.Struct;

namespace E_CL2029B
{
    #region CL2029B时序板 联机指令
    /// <summary>
    /// 2029B联机/脱机请求包
    /// </summary>
    internal class CL2029B_RequestLinkPacket : CL2029BSendPacket
    {
        public bool IsLink = true;

        public CL2029B_RequestLinkPacket()
            : base(true)
        { }

        /*
         * 81 30 PCID 08 C1 01 00 CS
         */
        protected override byte[] GetBody()
        {
            ByteBuffer buf = new ByteBuffer();
            buf.Initialize();
            buf.Put(0xA3);          //命令 
            buf.Put(0x02);
            buf.Put(0x01);
            buf.Put(0x01);
            buf.Put(0x01);
            buf.Put(0x02);

            return buf.ToByteArray();
        }
    }

    /// <summary>
    /// 2029B时序板，联机返回指令
    /// </summary>
    internal class Cl2029B_RequestLinkReplyPacket : CL2029BRecvPacket
    {
        protected override void ParseBody(byte[] data)
        {
            if (data == null || data.Length != 6)
                ReciveResult = RecvResult.DataError;
            else
            {
                if (data[0] == 0x50)
                    ReciveResult = RecvResult.OK;
                else
                    ReciveResult = RecvResult.Unknow;
            }
        }
    }
    #endregion

    #region CL2029B设置警示灯命令
    /// <summary>
    /// 2029B设置警示灯请求包
    /// </summary>
    internal class CL2029B_RequestSetLightPacket : CL2029BSendPacket
    {
        public bool IsLink = true;
        /// <summary>
        /// 警示灯类型
        /// </summary>
        private int iLightType = 0;

        public CL2029B_RequestSetLightPacket()
            : base(false)
        { }

        public void SetPara(int iType)
        {
            this.iLightType = iType;
        }
        /*
         * 81 42 PCID 0B A3 02 01 01 0x xx CS
         */
        protected override byte[] GetBody()
        {
            ByteBuffer buf = new ByteBuffer();
            buf.Initialize();
            buf.Put(0xA3);          //命令 
            buf.Put(0x02);
            buf.Put(0x01);
            buf.Put(0x01);
            buf.Put(0x01);
            buf.Put(Convert.ToByte(iLightType));

            return buf.ToByteArray();
        }
    }
    /// <summary>
    /// 2029B时序板设置警示灯返回指令
    /// </summary>
    internal class Cl2029B_RequestSetLightReplyPacket : CL2029BRecvPacket
    {
        protected override void ParseBody(byte[] data)
        {
            if (data == null || data.Length != 3)
                ReciveResult = RecvResult.DataError;
            else
            {
                if (data[0] == 0xA0)
                    ReciveResult = RecvResult.OK;
                else
                    ReciveResult = RecvResult.Unknow;
            }
        }
    }
    #endregion

    #region 2029B切换继电器
    /// <summary>
    /// 2029B切换继电器请求包
    /// </summary>
    internal class CL2029B_RequestSetSwitchPacket : CL2029BSendPacket
    {
        public bool IsLink = true;
        /// <summary>
        /// 继电器类型
        /// </summary>
        private bool bSwitchType = false;

        public CL2029B_RequestSetSwitchPacket()
            : base(false)
        { }

        public void SetPara(bool bType)
        {
            this.bSwitchType = bType;
        }
        /*
         * 81 42 PCID 0B A3 02 01 01 0x xx CS
         */
        protected override byte[] GetBody()
        {
            ByteBuffer buf = new ByteBuffer();
            buf.Initialize();
            buf.Put(0xA3);          //命令 
            buf.Put(0x01);
            buf.Put(0x01);
            buf.Put(0x01);
            buf.Put(0x01);
            if (bSwitchType)
                buf.Put(0x03);
            else
                buf.Put(0x00);

            return buf.ToByteArray();
        }
    }


    /// <summary>
    /// 2029B时序板设置警示灯返回指令
    /// </summary>
    internal class CL2029B_RequestSetSwitchReplayPacket : CL2029BRecvPacket
    {
        protected override void ParseBody(byte[] data)
        {
            if (data == null || data.Length < 3)
                ReciveResult = RecvResult.DataError;
            else
            {
                if (data[0] == 0xA0 || data[0] == 0x50)
                    ReciveResult = RecvResult.OK;
                else
                    ReciveResult = RecvResult.Unknow;
            }
        }
    }

    #endregion

    #region CL2029B控制装置供电命令
    /// <summary>
    /// 2029B控制装置供电请求包
    /// </summary>
    internal class CL2029B_RequestControlPowerPacket : CL2029BSendPacket
    {
        public bool IsLink = true;
        /// <summary>
        /// 是否供电
        /// </summary>
        private bool bPowerType = false;

        public CL2029B_RequestControlPowerPacket()
            : base(false)
        { }

        public void SetPara(bool bType)
        {
            this.bPowerType = bType;
        }
        /*
         * 81 42 PCID 0B A3 01 01 02 0x xx CS
         */
        protected override byte[] GetBody()
        {
            ByteBuffer buf = new ByteBuffer();
            buf.Initialize();
            buf.Put(0xA3);          //命令 
            buf.Put(0x01);
            buf.Put(0x01);
            buf.Put(0x01);
            buf.Put(0x02);
            if (bPowerType)
                buf.Put(0x03);
            else
                buf.Put(0x00);

            return buf.ToByteArray();
        }
    }
    /// <summary>
    /// 2029B时序板控制装置供电返回指令
    /// </summary>
    internal class CL2029B_RequestControlPowerReplyPacket : CL2029BRecvPacket
    {
        protected override void ParseBody(byte[] data)
        {
            if (data == null || data.Length != 3)
                ReciveResult = RecvResult.DataError;
            else
            {
                if (data[0] == 0xA0)
                    ReciveResult = RecvResult.OK;
                else
                    ReciveResult = RecvResult.Unknow;
            }
        }
    }
    #endregion

    #region 2029B切换继电器 03 电流
    /// <summary>
    /// 2029B切换继电器请求包
    /// </summary>
    internal class CL2029B_SetSwitchCurrentPacket : CL2029BSendPacket
    {
        public bool IsLink = true;
        /// <summary>
        /// 继电器类型
        /// </summary>
        private bool bSwitchType = false;

        public CL2029B_SetSwitchCurrentPacket()
            : base(true)
        { }

        public void SetPara(bool bType)
        {
            this.bSwitchType = bType;
        }
        /*
         * 81 42 PCID 0B A3 02 01 01 0x xx CS
         */
        protected override byte[] GetBody()
        {
            ByteBuffer buf = new ByteBuffer();
            buf.Initialize();
            buf.Put(0xA3);          //命令 
            buf.Put(0x03);
            buf.Put(0x01);
            buf.Put(0x01);
            buf.Put(0x01);
            if (bSwitchType)
                buf.Put(0x01);
            else
                buf.Put(0x00);

            return buf.ToByteArray();
        }
    }


    /// <summary>
    /// 2029B时序板设置警示灯返回指令
    /// </summary>
    internal class CL2029B_SetSwitchCurrentReplayPacket : CL2029BRecvPacket
    {
        protected override void ParseBody(byte[] data)
        {
            if (data == null || data.Length < 3)
                ReciveResult = RecvResult.DataError;
            else
            {
                if (data[0] == 0xA0 || data[0] == 0x50)
                    ReciveResult = RecvResult.OK;
                else
                    ReciveResult = RecvResult.Unknow;
            }
        }
    }

    #endregion

    #region 2029B切换继电器 01 CT供电
    /// <summary>
    /// 2029B切换继电器请求包
    /// </summary>
    internal class CL2029B_SetSwitchCTPacket : CL2029BSendPacket
    {
        public bool IsLink = true;
        /// <summary>
        /// 继电器类型
        /// </summary>
        private bool bSwitchType = false;

        public CL2029B_SetSwitchCTPacket()
            : base(true)
        { }

        public void SetPara(bool bType)
        {
            this.bSwitchType = bType;
        }
        /*
         * 81 42 PCID 0B A3 02 01 01 0x xx CS
         */
        protected override byte[] GetBody()
        {
            ByteBuffer buf = new ByteBuffer();
            buf.Initialize();
            buf.Put(0xA3);          //命令 
            buf.Put(0x01);
            buf.Put(0x01);
            buf.Put(0x01);
            buf.Put(0x01);
            if (bSwitchType)
                buf.Put(0x01);
            else
                buf.Put(0x00);

            return buf.ToByteArray();
        }
    }


    /// <summary>
    /// 2029B时序板设置警示灯返回指令
    /// </summary>
    internal class CL2029B_SetSwitchCTReplayPacket : CL2029BRecvPacket
    {
        protected override void ParseBody(byte[] data)
        {
            if (data == null || data.Length < 3)
                ReciveResult = RecvResult.DataError;
            else
            {
                if (data[0] == 0x50)
                    ReciveResult = RecvResult.OK;
                else
                    ReciveResult = RecvResult.Unknow;
            }
        }
    }

    #endregion
}
