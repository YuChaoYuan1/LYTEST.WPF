using System;
using ZH.SocketModule.Packet;

namespace ZH
{

    #region ZH1113误差板

    #region ZH1113误差板 继电器 控制 指令
    /// <summary>
    /// 01H—切换单相、三相继电器
    /// </summary>
    internal class ZH1113_RequestContrnlTypePacket : ZH3001SendPacket
    {
        public int ContrnlType = 1;
        public byte Data = 1;
        private byte BwNum = 0xFF;

        public ZH1113_RequestContrnlTypePacket()
            : base()
        { }

        //       01H—切换单相、三相继电器
        //    发送：68H+ID+SD+LEN+01H+NUM+DATA+CS  （DATA=2bytes）
        //DATA内容：01 01—2bytes 输出继电器吸合脉冲
        //            02 01—2bytes输出继电器断开脉冲
        //例如：
        //吸合  68   13  01  09  01  01    01 01  1B
        //断开  68   13  01  09  01  01    02 01  18  
        public void SetPara(int data0, byte bwNum, byte data1)
        {
            ContrnlType = data0;
            BwNum = bwNum;
            Data = data1;
        }
        //public void Set_Cmd(byte cmd)
        //{
        //    __Cmd = cmd;
        //}
        protected override byte[] GetBody()
        {
            ByteBuffer buf = new ByteBuffer();
            buf.Initialize();
            buf.Put(0x01);          //命令 
            buf.Put(BwNum);
            buf.Put(Convert.ToByte(ContrnlType));
            buf.Put(Data);
            return buf.ToByteArray();
        }
    }
    /// <summary>
    /// 01H—切换单相、三相继电器 返回指令
    /// </summary>
    internal class ZH1113_RequestContrnlTypeReplyPacket : ZH3001RecvPacket
    {
        protected override void ParseBody(byte[] data)
        {
            if (data == null || data.Length != 3)
                ReciveResult = RecvResult.DataError;
            else
            {
                if (data[0] == 0x81)
                    ReciveResult = RecvResult.OK;
                else
                    ReciveResult = RecvResult.Unknow;
            }
        }
    }
    #endregion

    #region ZH1113误差板 电动推杆压表入座和取表出座 指令
    /// <summary>
    /// 02H—电动推杆压表入座和取表出座
    /// </summary>
    internal class ZH1113_RequestElectricmachineryContrnlPacket : ZH3001SendPacket
    {
        public int ContrnlType = 1;
        public byte BwNum = 0xFF;
        public ZH1113_RequestElectricmachineryContrnlPacket()
            : base()
        { }

        //       02H—电动推杆压表入座和取表出座
        //    发送：68H+ID+SD+LEN+02H+NUM+DATA+CS  （DATA=1bytes）
        //     DATA内容： 00—压表入座
        //             01—取表出座
        //    例如：
        //    压表入座：68   13  01  08  02  01    00    19
        //    取表出座：68   13  01  08  02  01    01    18

        public void SetPara(int contrnlType, byte bwNum)
        {
            ContrnlType = contrnlType;
            BwNum = bwNum;
        }
        protected override byte[] GetBody()
        {
            ByteBuffer buf = new ByteBuffer();
            buf.Initialize();
            buf.Put(0x02);          //命令 
            buf.Put(BwNum);
            buf.Put(Convert.ToByte(ContrnlType));
            return buf.ToByteArray();
        }
    }
    /// <summary>
    /// 02H—电动推杆压表入座和取表出座 返回指令
    /// </summary>
    internal class ZH1113_RequestElectricmachineryContrnlReplyPacket : ZH3001RecvPacket
    {
        protected override void ParseBody(byte[] data)
        {
            if (data == null || data.Length != 3)
                ReciveResult = RecvResult.DataError;
            else
            {
                if (data[0] == 0x82)
                    ReciveResult = RecvResult.OK;
                else
                    ReciveResult = RecvResult.Unknow;
            }
        }
    }
    #endregion

    #region ZH111305H—设置标准常数 指令
    /// <summary>
    /// 05H—设置标准常数
    /// </summary>
    internal class ZH1113_RequestSetConstantQsPacket : ZH3001SendPacket
    {
        public int _ConstantType = 0;
        public int _Constant = 400000;
        public int _fdbs = 0;
        public byte BwNum = 0xFF;

        public ZH1113_RequestSetConstantQsPacket()
            : base()
        { }

        //        发送：68H+ID+SD+LEN+05H+NUM+DATA+CS  （DATA=7bytes）
        //      Data内容：
        //  标准常数类型: 1个字节，00—标准电能常数,01—标准时钟频率(预设500KHZ)
        //常数值：  4个字节表示；
        //         放大倍数：2个字节表示  //正数表示放大，负数表示缩小

        public void SetPara(int enlarge, int Constant, int fdbs, byte bwNum)
        {
            _ConstantType = enlarge;
            _Constant = Constant;
            BwNum = bwNum;
            _fdbs = fdbs;
        }
        protected override byte[] GetBody()
        {
            ByteBuffer buf = new ByteBuffer();
            buf.Initialize();
            buf.Put(0x05);          //命令 
            buf.Put(BwNum);
            buf.Put(Convert.ToByte(_ConstantType));
            buf.PutInt(_Constant);

            buf.PutInt2(_fdbs);
            return buf.ToByteArray();
        }
    }
    /// <summary>
    /// 05H—设置标准常数
    /// </summary>
    internal class ZH1113_RequestSetConstantQsReplyPacket : ZH3001RecvPacket
    {
        protected override void ParseBody(byte[] data)
        {
            if (data == null || data.Length != 3)
                ReciveResult = RecvResult.DataError;
            else
            {
                if (data[0] == 0x85)
                    ReciveResult = RecvResult.OK;
                else
                    ReciveResult = RecvResult.Unknow;
            }
        }
    }
    #endregion

    #region ZH111305H—设置被检常数 指令
    /// <summary>
    /// 06H---设置被检常数
    /// </summary>
    internal class ZH1113_RequestSetBJConstantQsPacket : ZH3001SendPacket
    {
        public int _ConstantType = 0;
        public int _Constant = 1200;
        public int Fdbs = 100;
        public int _qs = 1;
        public byte BwNum = 0xFF;

        public ZH1113_RequestSetBJConstantQsPacket()
            : base()
        { }
        //06H---设置被检常数
        //   （6组被检 ：00-正向有功；01-正向无功；02-反向有功；03-反向无功；04-日计时；05-需量）
        //发送：68H+ID+SD+LEN+06H+NUM+ DATA+CS （data=11bytes）
        //      DATA内容：
        //被检常数组别：1个字节；
        //00— 正向有功；
        //01—	正向无功；
        //02—	反向有功；
        //03—	反向无功；
        //04—日计时时钟频率
        //05—需量
        //被检表常数: 4个字节表示
        //         常数放大倍数：2个字节表示  //正数表示放大，负数表示缩小
        //         检定圈数：4个字节表示


        public void SetPara(int enlarge, int Constant, int fads, int qs, byte bwNum)
        {
            _ConstantType = enlarge;
            _Constant = Constant;
            Fdbs = fads;
            _qs = qs;
            BwNum = bwNum;
        }
        protected override byte[] GetBody()
        {
            ByteBuffer buf = new ByteBuffer();
            buf.Initialize();
            buf.Put(0x06);          //命令 
            buf.Put(BwNum);
            buf.Put(Convert.ToByte(_ConstantType));
            buf.PutInt(_Constant);
            buf.PutInt2_S(Fdbs);
            buf.PutInt(_qs);
            return buf.ToByteArray();
        }
    }
    /// <summary>
    /// 06H---设置被检常数
    /// </summary>
    internal class ZH1113_RequestSetBJConstantQsReplyPacket : ZH3001RecvPacket
    {
        protected override void ParseBody(byte[] data)
        {
            if (data == null || data.Length != 3)
                ReciveResult = RecvResult.DataError;
            else
            {
                if (data[0] == 0x86)
                    ReciveResult = RecvResult.OK;
                else
                    ReciveResult = RecvResult.Unknow;
            }
        }
    }
    #endregion

    #region 读取电压短路  电流开路的状态值
    // 03H 指令

    internal class ZH1113_RequestReadFaultPacket : ZH3001SendPacket
    {
        public byte _Reg = 1;
        public byte BwNum = 0xFF;

        public ZH1113_RequestReadFaultPacket()
            : base()
        { }
        public void SetPara(byte reg, byte bwNum)
        {
            _Reg = reg;
            BwNum = bwNum;
        }
        // 例如：68 13 FE 08 03 01 00 E7   //1号表位
        protected override byte[] GetBody()
        {
            ByteBuffer buf = new ByteBuffer();
            buf.Initialize();
            buf.Put(_Reg);          //命令 
            buf.Put(BwNum);
            buf.Put(0);
            return buf.ToByteArray();
        }
    }
    internal class ZH1113_RequestReadFaultReplyPacket : ZH3001RecvPacket
    {
        public byte OutBwNum = 0;
        public byte u_result = 0; //电压短路标标志00正常，01短路，02继电器不工作
        public byte i_result = 0; //电流短路标标志，00正在，01旁路成功，02旁路继电器不工作
        public byte dj_result = 0;  //电机行程标志，00电机行程不确定，01电机行程在上取表出座位置，02电机行程在下，压表入座位置
        public byte gb_result = 0; //挂表状态标志，00没挂表，01以挂表
        public byte ct_result = 0; //CT电量过载标志，00正常，01过载(北京改造线科陆CT)
        public byte tz_result = 0; //跳匝指示灯标志 00正常，01以输出跳匝信号
        public byte wd_result = 0;  //二级设备温度板电流过载标志，00没过载，01过载(北京改造线，互感表)
        public byte ny_result = 0;//0x00-在阈值范围内；0x01-超过阈值

        protected override void ParseBody(byte[] data)
        {
            if ((data == null) || (data.Length < 9))
                ReciveResult = RecvResult.DataError;
            else
            {
                if (data[0] == 0x83)
                {
                    ReciveResult = RecvResult.OK;

                    ByteBuffer buf = new ByteBuffer(data);
                    buf.Get(); //0x83
                    OutBwNum = Convert.ToByte(buf.Get()); //表位

                    u_result = Convert.ToByte(buf.Get());
                    i_result = Convert.ToByte(buf.Get());
                    dj_result = Convert.ToByte(buf.Get());
                    gb_result = Convert.ToByte(buf.Get());
                    ct_result = Convert.ToByte(buf.Get());
                    tz_result = Convert.ToByte(buf.Get());
                    wd_result = Convert.ToByte(buf.Get());
                    //耐压漏电标志
                    if (data.Length > 9)
                        ny_result = buf.Get();

                }
                else
                    ReciveResult = RecvResult.Unknow;
            }
        }
    }
    #endregion


    #region ZH1113读取计算值 指令
    /// <summary>
    /// 07H读取计算值
    /// </summary>
    internal class ZH1113_RequestReadDataPacket : ZH3001SendPacket
    {
        public int _readType = 1;
        public byte BwNum = 0xFF;

        public ZH1113_RequestReadDataPacket()
            : base()
        { }

        //       发送：68H+ID+SD+LEN+07H+NUM+ DATA+CS （data=1byte）
        //Data内容：
        //组别: 1个字节(00--正向有功，01--正向无功，02--反向有功，03--反向无功，04--日计时误差，
        public void SetPara(int readType, byte bwNum)
        {
            _readType = readType;
            BwNum = bwNum;
        }
        protected override byte[] GetBody()
        {
            ByteBuffer buf = new ByteBuffer();
            buf.Initialize();
            buf.Put(0x07);          //命令 
            buf.Put(BwNum);
            buf.Put(Convert.ToByte(_readType));
            return buf.ToByteArray();
        }
    }
    /// <summary>
    /// 07H读取计算值
    /// </summary>
    internal class ZH1113_RequestReadDataReplyPacket : ZH3001RecvPacket
    {

        public string[] OutstrWcData = new string[0];
        public int OutBwNul = 0;
        public int OutGroup = 0;
        public int OutWcNul = 0;
        protected override void ParseBody(byte[] data)
        {
            if ((data == null) || (data.Length != 24 && data.Length != 27))
                ReciveResult = RecvResult.DataError;
            else
            {
                if (data[0] == 0x87)
                {
                    string[] strwc = new string[5];
                    OutstrWcData = new string[5];
                    ReciveResult = RecvResult.OK;

                    ByteBuffer buf = new ByteBuffer(data);
                    buf.Get(); //0x87
                    OutBwNul = Convert.ToInt32(buf.Get()); //表位
                    //组别: 1个字节(00--有功，01--无功，04--日计时误差，05--需量，06--有功脉冲计数，07--无功
                    OutGroup = Convert.ToInt32(buf.Get());
                    if (data.Length == 24)
                    {
                        OutWcNul = Convert.ToInt32(buf.Get());
                    }
                    else
                    {
                        OutWcNul = buf.GetInt();
                    }

                    if (OutGroup == 0x06 || OutGroup == 0x07)
                    {
                        strwc[0] = Convert.ToString(buf.GetInt());
                        strwc[1] = Convert.ToString(buf.GetInt());
                        strwc[2] = "0";
                        strwc[3] = "0";
                        strwc[4] = "0";
                    }
                    else if (OutGroup == 0x08 || OutGroup == 0x09)
                    {
                        strwc[0] = (buf.GetInt() / 10000.0F).ToString("F3");
                        strwc[1] = (buf.GetInt() / 10000.0F).ToString("F3");
                        strwc[2] = (buf.GetInt() / 10000.0F).ToString("F3");
                        strwc[3] = (buf.GetInt() / 10000.0F).ToString("F3");
                        strwc[4] = (buf.GetInt() / 10000.0F).ToString("F3");
                    }
                    else
                    {
                        strwc[0] = (buf.GetInt() / 100000.00000F).ToString("#0.0000");
                        strwc[1] = (buf.GetInt() / 100000.00000F).ToString("#0.0000");
                        strwc[2] = (buf.GetInt() / 100000.00000F).ToString("#0.0000");
                        strwc[3] = (buf.GetInt() / 100000.00000F).ToString("#0.0000");
                        strwc[4] = (buf.GetInt() / 100000.00000F).ToString("#0.0000");
                    }


                    OutstrWcData = strwc;


                }
                else
                    ReciveResult = RecvResult.Unknow;
            }
        }
    }
    #endregion

    #region ZH1113误差板 设置表位显示屏 指令
    /// <summary>
    ///09H 设置表位显示屏
    /// </summary>
    internal class ZH1113_RequestSetBwNoPacket : ZH3001SendPacket
    {

        public byte BwNum = 0xFF;
        //public byte BwNum = 0x0B;


        public ZH1113_RequestSetBwNoPacket()
            : base()
        { }

        //       发送：68H+ID+SD+LEN+09H+NUM+ DATA+CS （DATA=1bytes,无意义，用00）
        //返回：68H+ID+SD+LEN+89H+NUM+K+CS

        public void SetPara(byte bwNum)
        {

            BwNum = bwNum;
        }
        protected override byte[] GetBody()
        {
            ByteBuffer buf = new ByteBuffer();
            buf.Initialize();
            buf.Put(0x09);          //命令 
            buf.Put(BwNum);
            buf.Put(0x00);
            return buf.ToByteArray();
        }
    }
    /// <summary>
    /// 09H 设置表位显示屏
    /// </summary>
    internal class ZH1113_RequestSetBwNoReplyPacket : ZH3001RecvPacket
    {
        protected override void ParseBody(byte[] data)
        {
            if (data == null || data.Length != 3)
                ReciveResult = RecvResult.DataError;
            else
            {
                if (data[0] == 0x89)
                    ReciveResult = RecvResult.OK;
                else
                    ReciveResult = RecvResult.Unknow;
            }
        }
    }
    #endregion

    #region ZH1113误差板 启动功能 指令
    /// <summary>
    ///0AH 启动功能
    /// </summary>
    internal class ZH1113_RequestStartPacket : ZH3001SendPacket
    {

        public byte BwNum = 0xFF;
        public int _start = 0;

        public ZH1113_RequestStartPacket()
            : base()
        { }

        //      发送：68H+ID+SD+LEN+0AH+NUM+ DATA+CS  （data=1字节）
        //        DATA内容：
        //组别：1个字节（00：正向有功，01：正向无功，02：反向有功，03：反向无功，04：日计时，05：需量， 06：正向有功脉冲计数， 07：正向无功脉冲计数， 08：反向有功脉冲计数，09 反向无功脉冲计数）


        public void SetPara(int start, byte bwNum)
        {
            _start = start;
            BwNum = bwNum;
        }
        protected override byte[] GetBody()
        {
            ByteBuffer buf = new ByteBuffer();
            buf.Initialize();
            buf.Put(0x0A);          //命令 
            buf.Put(BwNum);
            buf.Put(Convert.ToByte(_start));
            return buf.ToByteArray();
        }
    }
    /// <summary>
    /// 0AH 启动功能
    /// </summary>
    internal class ZH1113_RequestStartReplyPacket : ZH3001RecvPacket
    {
        protected override void ParseBody(byte[] data)
        {
            if (data == null || data.Length != 3)
                ReciveResult = RecvResult.DataError;
            else
            {
                if (data[0] == 0x8A)
                    ReciveResult = RecvResult.OK;
                else
                    ReciveResult = RecvResult.Unknow;
            }
        }
    }
    #endregion

    #region ZH1113误差板 停止当前功能 指令
    /// <summary>
    ///0BH 停止当前功能
    /// </summary>
    internal class ZH1113_RequestStopPacket : ZH3001SendPacket
    {

        public byte BwNum = 0xFF;
        public int _stop = 0;

        public ZH1113_RequestStopPacket()
            : base()
        { }

        //       发送：68H+ID+SD+LEN+0BH+NUM+ DATA+CS     （data=1byte）
        //DATA内容：
        //        组别：1个字节（00：正向有功，01：正向无功，02：反向有功，03：反向无功，04：日计时，05：需量， 06：正向有功脉冲计数， 07：正向无功脉冲计数，08：反向有功脉冲计数， 09：反向无功脉冲计数）


        public void SetPara(int stop, byte bwNum)
        {
            _stop = stop;
            BwNum = bwNum;
        }
        protected override byte[] GetBody()
        {
            ByteBuffer buf = new ByteBuffer();
            buf.Initialize();
            buf.Put(0x0B);          //命令 
            buf.Put(BwNum);
            buf.Put(Convert.ToByte(_stop));
            return buf.ToByteArray();
        }
    }
    /// <summary>
    /// 0BH 停止当前功能
    /// </summary>
    internal class ZH1113_RequestStopReplyPacket : ZH3001RecvPacket
    {
        protected override void ParseBody(byte[] data)
        {
            if (data == null || data.Length != 3)
                ReciveResult = RecvResult.DataError;
            else
            {
                if (data[0] == 0x8B)
                    ReciveResult = RecvResult.OK;
                else
                    ReciveResult = RecvResult.Unknow;
            }
        }
    }
    #endregion

    #region ZH1113误差板 对标 指令
    /// <summary>
    ///0CH对标
    /// </summary>
    internal class ZH1113_RequestBenchmarkingPacket : ZH3001SendPacket
    {

        public byte BwNum = 0xFF;
        public int _index = 01;

        public ZH1113_RequestBenchmarkingPacket()
            : base()
        { }

        //组数：1个字节（01:第一组,02:第二组）

        public void SetPara(int index, byte bwNum)
        {
            _index = index;
            BwNum = bwNum;
        }
        protected override byte[] GetBody()
        {
            ByteBuffer buf = new ByteBuffer();
            buf.Initialize();
            buf.Put(0x0C);          //命令 
            buf.Put(BwNum);
            buf.Put(Convert.ToByte(_index));
            return buf.ToByteArray();
        }
    }
    /// <summary>
    /// 0CH对标
    /// </summary>
    internal class ZH1113_RequestBenchmarkingReplyPacket : ZH3001RecvPacket
    {
        protected override void ParseBody(byte[] data)
        {
            if (data == null || data.Length != 3)
                ReciveResult = RecvResult.DataError;
            else
            {
                if (data[0] == 0x8C)
                    ReciveResult = RecvResult.OK;
                else
                    ReciveResult = RecvResult.Unknow;
            }
        }
    }
    #endregion

    #region ZH1113误差板 取消对标 指令
    /// <summary>
    ///0DH 取消对标
    /// </summary>
    internal class ZH1113_RequestRevBenchmarkingPacket : ZH3001SendPacket
    {

        public byte BwNum = 0xFF;
        public int _index = 01;

        public ZH1113_RequestRevBenchmarkingPacket()
            : base()
        { }

        //组数：1个字节（01:第一组,02:第二组）

        public void SetPara(int index, byte bwNum)
        {
            _index = index;
            BwNum = bwNum;
        }
        protected override byte[] GetBody()
        {
            ByteBuffer buf = new ByteBuffer();
            buf.Initialize();
            buf.Put(0x0D);          //命令 
            buf.Put(BwNum);
            buf.Put(Convert.ToByte(_index));
            return buf.ToByteArray();
        }
    }
    /// <summary>
    /// 0DH 取消对标
    /// </summary>
    internal class ZH1113_RequestRevBenchmarkingReplyPacket : ZH3001RecvPacket
    {
        protected override void ParseBody(byte[] data)
        {
            if (data == null || data.Length != 3)
                ReciveResult = RecvResult.DataError;
            else
            {
                if (data[0] == 0x8D)
                    ReciveResult = RecvResult.OK;
                else
                    ReciveResult = RecvResult.Unknow;
            }
        }
    }
    #endregion

    #region ZH1113误差板 查询对标状态 指令
    /// <summary>
    ///0EH查询对标状态
    /// </summary>
    internal class ZH1113_RequestSelectBenchmarkingPacket : ZH3001SendPacket
    {

        public byte BwNum = 0xFF;
        public int _index = 01;

        public ZH1113_RequestSelectBenchmarkingPacket()
            : base()
        { }

        //     发送：68H+ID+SD+LEN+0EH+NUM+ DATA+CS
        //               组数：1个字节（01:第一组,02:第二组）
        //返回：68H+ID+SD+LEN+8EH+NUM+DATA+CS
        //              组数：1个字节（01:第一组,02:第二组）
        //              状态：1个字节（0：没有完成，1：完成对标）


        public void SetPara(int index, byte bwNum)
        {
            _index = index;
            BwNum = bwNum;
        }
        protected override byte[] GetBody()
        {
            ByteBuffer buf = new ByteBuffer();
            buf.Initialize();
            buf.Put(0x0E);          //命令 
            buf.Put(BwNum);
            buf.Put(Convert.ToByte(_index));
            return buf.ToByteArray();
        }
    }
    /// <summary>
    /// 0EH查询对标状态
    /// </summary>
    internal class ZH1113_RequestSelectBenchmarkingReplyPacket : ZH3001RecvPacket
    {
        protected override void ParseBody(byte[] data)
        {
            if (data == null || data.Length != 3)
                ReciveResult = RecvResult.DataError;
            else
            {
                if (data[0] == 0x8E)
                    ReciveResult = RecvResult.OK;
                else
                    ReciveResult = RecvResult.Unknow;
            }
        }
    }
    #endregion

    #region ZH1113误差板 16H 设置耐压测试漏电流阈值
    /// <summary>
    /// 
    /// </summary>
    internal class ZH1113_SetNaiYaLimitPacket : ZH3001SendPacket
    {
        public uint Data = 1;
        public byte BwNum = 0xFF;

        public ZH1113_SetNaiYaLimitPacket()
            : base()
        { }

        public void SetPara(byte bwNum, float data)
        {
            BwNum = bwNum;
            Data = (uint)(data * 100000);
        }

        protected override byte[] GetBody()
        {
            ByteBuffer buf = new ByteBuffer();
            buf.Initialize();
            buf.Put(0x16);          //命令 
            buf.Put(BwNum);
            buf.PutInt(Data);
            return buf.ToByteArray();
        }
    }

    internal class ZH1113_SetNaiYaLimitReplyPacket : ZH3001RecvPacket
    {
        protected override void ParseBody(byte[] data)
        {
            if (data == null || data.Length != 3)
                ReciveResult = RecvResult.DataError;
            else
            {
                if (data[0] == 0x96)
                    ReciveResult = RecvResult.OK;
                else
                    ReciveResult = RecvResult.Unknow;
            }
        }
    }
    #endregion

    #region ZH1113误差板 15H 读取耐压测试漏电电流值
    /// <summary>
    /// 
    /// </summary>
    internal class ZH1113_ReadNaiYaCurrentPacket : ZH3001SendPacket
    {
        public byte Data = 0;
        public byte BwNum = 0xFF;

        public ZH1113_ReadNaiYaCurrentPacket()
            : base()
        { }

        public void SetPara(byte bwNum)
        {
            BwNum = bwNum;
        }

        protected override byte[] GetBody()
        {
            ByteBuffer buf = new ByteBuffer();
            buf.Initialize();
            buf.Put(0x15);          //命令 
            buf.Put(BwNum);
            buf.Put(Data);
            return buf.ToByteArray();
        }
    }

    internal class ZH1113_ReadNaiYaCurrentReplyPacket : ZH3001RecvPacket
    {
        public float overCurrent;
        public float Current;

        protected override void ParseBody(byte[] data)
        {
            if (data == null || data.Length < 9)
                ReciveResult = RecvResult.DataError;
            else
            {
                if (data[0] == 0x95)
                {
                    ByteBuffer buf = new ByteBuffer(data);
                    buf.Get();
                    buf.Get();
                    overCurrent = buf.GetUInt() / 100000f;
                    Current = buf.GetUInt() / 100000f;
                    ReciveResult = RecvResult.OK;
                }
                else
                    ReciveResult = RecvResult.Unknow;
            }
        }
    }
    #endregion

    #region 14H—启停电流柱温度传感器功能及查询利旧的二级设备续流继电器状态及控制续流继电器
    /// <summary>
    /// 14H---控制续流继电器
    /// </summary>
    internal class ZH1113_RequestSetCurrentSwitchStatePacket : ZH3001SendPacket
    {
        public int switchState = 0;
        public int tempState = 0;
        public byte BwNum = 0xFF;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="switchState">0-旁路继电器无意义，1-使续流继电器闭合，2-消除续流继电器的锁定信号</param>
        /// <param name="tempState">0-温度检测无意义，1-启用电流柱温度检测，2-停用电流柱温度检测</param>
        /// <param name="bwNum">表位</param>
        public ZH1113_RequestSetCurrentSwitchStatePacket(int switchState, int tempState, byte bwNum)
            : base()
        {
            this.switchState = switchState;
            this.tempState = tempState;
            BwNum = bwNum;
        }

        protected override byte[] GetBody()
        {
            byte data = 0x00;
            if (switchState == 1)
                data |= 0b0000_1000;
            else if (switchState == 2)
                data |= 0b0000_0100;

            if (tempState == 1)
                data |= 0b0000_0001;
            else if (tempState == 2)
                data |= 0b0000_0010;

            ByteBuffer buf = new ByteBuffer();
            buf.Initialize();
            buf.Put(0x14);          //命令 
            buf.Put(BwNum);
            buf.Put(data);
            return buf.ToByteArray();
        }
    }
    /// <summary>
    /// 14H---控制续流继电器
    /// </summary>
    internal class ZH1113_RequestSetCurrentSwitchStateReplyPacket : ZH3001RecvPacket
    {
        protected override void ParseBody(byte[] data)
        {
            if (data == null || data.Length != 3)
                ReciveResult = RecvResult.DataError;
            else
            {
                if (data[0] == 0x94)
                    ReciveResult = RecvResult.OK;
                else
                    ReciveResult = RecvResult.Unknow;
            }
        }
    }
    #endregion


    #endregion



}
