using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Threading;
using ZH.Enum;
using ZH.Struct;
using ZH;
using ZH.SocketModule.Packet;

namespace ZH
{
    [InterfaceType(ComInterfaceType.InterfaceIsIDispatch),
     ComVisible(true)]
    public interface IClass_Interface
    {
        /// <summary>
        /// 初始化设备通讯参数UDP
        /// </summary>
        /// <param name="ComNumber">端口号</param>
        /// <param name="MaxWaitTme">最长等待时间</param>
        /// <param name="WaitSencondsPerByte">帧字节间隔时间</param>
        /// <param name="IP">Ip地址</param>
        /// <param name="RemotePort">远程端口</param>
        /// <param name="LocalStartPort">本地端口</param>
        /// <returns>是否注册成功</returns>
        [DispId(1)]
        int InitSetting(int ComNumber, int MaxWaitTime, int WaitSencondsPerByte, string IP, int RemotePort, int LocalStartPort);
        /// <summary>
        /// 注册Com 口
        /// </summary>
        /// <param name="ComNumber"></param>
        /// <param name="strSetting"></param>
        /// <param name="maxWaittime"></param>
        /// <returns></returns>
        [DispId(2)]
        int InitSettingCom(int ComNumber, int MaxWaitTime, int WaitSencondsPerByte);
        /// <summary>
        /// 连接
        /// </summary>
        /// <param name="FrameAry"></param>
        /// <returns></returns>
        [DispId(3)]
        int Connect(byte meterN0, out string[] FrameAry);

        /// <summary>
        /// 脱机指令没有
        /// </summary>
        /// <param name="FrameAry">输出上行报文</param>
        /// <returns></returns>
        [DispId(4)]
        int DisConnect(out string[] FrameAry);

        /// <summary>
        /// 控制表位继电器
        /// </summary>
        /// <param name="contrnlType"></param>
        /// <param name="bwNum"></param>
        /// <param name="FrameAry"></param>
        /// <returns></returns>
        [DispId(5)]
        int ContnrlBw(int contrnlType, byte bwNum, byte data, byte cmd, out string[] FrameAry);

        /// <summary>
        /// 电机控制
        /// </summary>
        /// <param name="contrnlType"></param>
        /// <param name="bwNum"></param>
        /// <param name="FrameAry"></param>
        /// <returns></returns>
        [DispId(6)]
        int ElectricmachineryContrnl(int contrnlType, byte bwNum, out string[] FrameAry);


        /// <summary>
        /// 设置标准常数
        /// </summary>
        /// <param name="enlarge"></param>
        /// <param name="Constant"></param>
        /// <param name="qs"></param>
        /// <param name="bwNum"></param>
        /// <param name="FrameAry"></param>
        /// <returns></returns>
        [DispId(7)]
        int SetStandardConstantQs(int enlarge, int Constant, int qs, byte bwNum, out string[] FrameAry);

        /// <summary>
        /// 设置被检常数
        /// </summary>
        /// <param name="enlarge"></param>
        /// <param name="Constant"></param>
        /// <param name="fads"></param>
        /// <param name="qs"></param>
        /// <param name="bwNum"></param>
        /// <param name="FrameAry"></param>
        /// <returns></returns>
        [DispId(8)]
        int SetBJConstantQs(int enlarge, int Constant, int fads, int qs, byte bwNum, out string[] FrameAry);

        /// <summary>
        /// 读取计算值
        /// </summary>
        /// <param name="readType"></param>
        /// <param name="FrameAry"></param>
        /// <returns></returns>
        [DispId(9)]
        int ReadData(int readType, byte bwNum, out string[] OutWcData, out int OutBwNul, out int OutGroup, out int OutWcNul, out string[] FrameAry);

        /// <summary>
        /// 启动计算
        /// </summary>
        /// <param name="start"></param>
        /// <param name="FrameAry"></param>
        /// <returns></returns>
        [DispId(10)]
        int Start(int start, byte bwNum, out string[] FrameAry);

        /// <summary>
        /// 停止计算
        /// </summary>
        /// <param name="stop"></param>
        /// <param name="FrameAry"></param>
        /// <returns></returns>
        [DispId(11)]
        int Stop(int stop, byte bwNum, out string[] FrameAry);

        /// <summary>
        /// 对标
        /// </summary>
        /// <param name="index"></param>
        /// <param name="FrameAry"></param>
        /// <returns></returns>
        [DispId(12)]
        int Benchmarking(int index, byte bwNum, out string[] FrameAry);

        /// <summary>
        /// 取消队标
        /// </summary>
        /// <param name="index"></param>
        /// <param name="FrameAry"></param>
        /// <returns></returns>
        [DispId(13)]
        int RevBenchmarking(int index, byte bwNum, out string[] FrameAry);

        /// <summary>
        /// 查询对标状态
        /// </summary>
        /// <param name="index"></param>
        /// <param name="FrameAry"></param>
        /// <returns></returns>
        [DispId(14)]
        int SelectBenchmarking(int index, byte bwNum, out string[] FrameAry);

    }
    
    public class ZH1113 : IClass_Interface
    {
        /// <summary>
        /// 重试次数
        /// </summary>
        public static int RETRYTIEMS = 1;
        /// <summary>
        /// 源控制端口
        /// </summary>
        private readonly StPortInfo m_ErrorZh1113Port = null;

        private readonly DriverBase driverBase = null;

        //是否发送数据标志
        private bool sendFlag = true;
        /// <summary>
        /// 构造方法
        /// </summary>
        public ZH1113()
        {
            m_ErrorZh1113Port = new StPortInfo();
            driverBase = new DriverBase();
        }

        #region IClass_Interface 成员
        /// <summary>
        /// 初始化2018端口
        /// </summary>
        /// <param name="ComNumber">端口号</param>
        /// <param name="MaxWaitTime">帧命令最长等待时间</param>
        /// <param name="WaitSencondsPerByte">帧字节等待时间</param>
        /// <param name="IP">Ip地址</param>
        /// <param name="RemotePort">远程端口</param>
        /// <param name="LocalStartPort">本地端口</param>
        /// <returns></returns>
        public int InitSetting(int ComNumber, int MaxWaitTime, int WaitSencondsPerByte, string IP, int RemotePort, int LocalStartPort)
        {
            m_ErrorZh1113Port.m_Exist = 1;
            m_ErrorZh1113Port.m_IP = IP;
            m_ErrorZh1113Port.m_Port = ComNumber;
            m_ErrorZh1113Port.m_Port_IsUDPorCom = true;
            m_ErrorZh1113Port.m_Port_Setting = "19200,n,8,1";
            try
            {
                driverBase.RegisterPort(ComNumber, m_ErrorZh1113Port.m_Port_Setting, m_ErrorZh1113Port.m_IP, RemotePort, LocalStartPort, MaxWaitTime, WaitSencondsPerByte);
            }
            catch (Exception)
            {
                return 1;
            }

            return 0;
        }
        /// <summary>
        /// 初始化Com 口
        /// </summary>
        /// <param name="ComNumber"></param>
        /// <param name="MaxWaitTime"></param>
        /// <param name="WaitSencondsPerByte"></param>
        /// <returns></returns>
        public int InitSettingCom(int ComNumber, int MaxWaitTime, int WaitSencondsPerByte)
        {
            m_ErrorZh1113Port.m_Exist = 1;
            m_ErrorZh1113Port.m_IP = "";
            m_ErrorZh1113Port.m_Port = ComNumber;
            m_ErrorZh1113Port.m_Port_IsUDPorCom = false;
            m_ErrorZh1113Port.m_Port_Setting = "19200,n,8,1";
            try
            {
                driverBase.RegisterPort(ComNumber, "19200,n,8,1", MaxWaitTime, WaitSencondsPerByte);
            }
            catch (Exception)
            {

                return 1;
            }

            return 0;
        }

        /// <summary>
        /// 连接
        /// </summary>
        /// <param name="meterN0">表位号</param>
        /// <param name="FrameAry">出参</param>
        /// <returns></returns>
        public int Connect(byte meterN0, out string[] FrameAry)
        {
            //联机的时候默认为没有电压，第一次升源需要升电压。
            //ZH1113.g_WriteINI("D:\\Temp.ini", "U", "u", "XXXXXX");   //  by DBL 

            ZH1113_RequestSetBwNoPacket rc2 = new ZH1113_RequestSetBwNoPacket();
            ZH1113_RequestSetBwNoReplyPacket recv2 = new ZH1113_RequestSetBwNoReplyPacket();
            FrameAry = new string[1];
            try
            {
                rc2.SetPara(meterN0);
                FrameAry[0] = BytesToString(rc2.GetPacketData());
                if (sendFlag)
                {
                    if (SendPacketWithRetry(m_ErrorZh1113Port, rc2, recv2))
                    {
                        int ReValue = recv2.ReciveResult == RecvResult.OK ? 0 : 1;
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
        /// 控制表位继电器
        /// </summary>
        /// <param name="contrnlType"></param>
        /// <param name="bwNum"></param>
        /// <param name="FrameAry"></param>
        /// <returns></returns>
        public int ContnrlBw(int contrnlType, byte bwNum, byte data, byte cmd, out string[] FrameAry)
        {
            FrameAry = new string[1];
            ZH1113_RequestContrnlTypePacket rc2 = new ZH1113_RequestContrnlTypePacket();
            ZH1113_RequestContrnlTypeReplyPacket recv2 = new ZH1113_RequestContrnlTypeReplyPacket();
            try
            {
                if (bwNum == 0xFF) rc2.IsNeedReturn = false;
                else rc2.IsNeedReturn = true;

                rc2.SetPara(contrnlType, bwNum, data);
                int ReValue = 0;
                FrameAry[0] = BytesToString(rc2.GetPacketData());
                if (sendFlag)
                {
                    if (SendPacketWithRetry(m_ErrorZh1113Port, rc2, recv2))
                    {
                        ReValue = recv2.ReciveResult == RecvResult.OK ? 0 : 1;
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
        /// 电机控制
        /// </summary>
        /// <param name="contrnlType">00下行 01上行</param>
        /// <param name="bwNum"></param>
        /// <param name="FrameAry"></param>
        /// <returns></returns>
        public int ElectricmachineryContrnl(int contrnlType, byte bwNum, out string[] FrameAry)
        {
            ZH1113_RequestElectricmachineryContrnlPacket rc2 = new ZH1113_RequestElectricmachineryContrnlPacket();
            ZH1113_RequestElectricmachineryContrnlReplyPacket recv2 = new ZH1113_RequestElectricmachineryContrnlReplyPacket();
            rc2.SetPara(contrnlType, bwNum);
            FrameAry = new string[1];
            try
            {
                FrameAry[0] = BytesToString(rc2.GetPacketData());
                if (sendFlag)
                {
                    if (SendPacketWithRetry(m_ErrorZh1113Port, rc2, recv2))
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
        /// 设置标准常数
        /// </summary>
        /// <param name="enlarge">o电能类型（2个标准： 00-标准电能误差相关； 01-标准时钟日计时需量）</param>
        /// <param name="Constant">标准常数</param>
        /// <param name="sdbs">放大倍数-2缩小100倍</param>
        /// <param name="bwNum">表位FF</param>
        /// <param name="FrameAry"></param>
        /// <returns></returns>
        public int SetStandardConstantQs(int enlarge, int Constant, int sdbs, byte bwNum, out string[] FrameAry)
        {
            ZH1113_RequestSetConstantQsPacket rc2 = new ZH1113_RequestSetConstantQsPacket();
            ZH1113_RequestSetConstantQsReplyPacket recv2 = new ZH1113_RequestSetConstantQsReplyPacket();
            //rc2.IsNeedReturn = false;
            rc2.SetPara(enlarge, Constant, sdbs, bwNum);
            FrameAry = new string[1];
            try
            {

                FrameAry[0] = BytesToString(rc2.GetPacketData());
                if (sendFlag)
                {
                    if (bwNum == 0xFF)
                    {
                        SendPacketNotRevWithRetry(m_ErrorZh1113Port, rc2, recv2);
                        return 0;
                    }
                    else if (SendPacketWithRetry(m_ErrorZh1113Port, rc2, recv2))
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
        /// 设置被检常数
        /// </summary>
        /// <param name="enlarge">（6组被检 ：00-有功（含正向、反向，以下同）；01-无功（正向、反向，以下同）； 04-日计时；05-需量）</param>
        /// <param name="Constant"></param>
        /// <param name="fads"></param>
        /// <param name="qs"></param>
        /// <param name="bwNum"></param>
        /// <param name="FrameAry"></param>
        /// <returns></returns>
        public int SetBJConstantQs(int enlarge, int Constant, int fads, int qs, byte bwNum, out string[] FrameAry)
        {
            ZH1113_RequestSetBJConstantQsPacket rc2 = new ZH1113_RequestSetBJConstantQsPacket();
            ZH1113_RequestSetBJConstantQsReplyPacket recv2 = new ZH1113_RequestSetBJConstantQsReplyPacket();
            //rc2.IsNeedReturn = false;
            rc2.SetPara(enlarge, Constant, fads, qs, bwNum);
            FrameAry = new string[1];
            try
            {
                FrameAry[0] = BytesToString(rc2.GetPacketData());
                if (sendFlag)
                {
                    if (bwNum == 0xFF)
                    {
                        SendPacketNotRevWithRetry(m_ErrorZh1113Port, rc2, recv2);
                        return 0;
                    }
                    else if (SendPacketWithRetry(m_ErrorZh1113Port, rc2, recv2))
                    {
                        int ReValue = recv2.ReciveResult == RecvResult.OK ? 0 : 1;
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
        /// 读取表位电压短路和电流开路标志 
        /// </summary>
        /// <param name="reg">命令03读取表位电压短路和电流开路标志</param>
        /// <param name="bwNum">表位</param>
        /// <param name="OutResult">返回状态0:电压短路标志，00表示没短路，01表示短路；DATA1-电流开路标志，1:00表示没开路，01表示开路。</param>
        /// <param name="FrameAry"></param>
        /// <returns></returns>
        public int Read_Fault(byte reg, byte bwNum, out byte[] OutResult, out string[] FrameAry)
        {
            OutResult = new byte[8];
            FrameAry = new string[1];
            ZH1113_RequestReadFaultPacket rc = new ZH1113_RequestReadFaultPacket();
            ZH1113_RequestReadFaultReplyPacket recv = new ZH1113_RequestReadFaultReplyPacket();
            // 设置参数
            rc.SetPara(reg, bwNum);

            try
            {
                FrameAry[0] = BytesToString(rc.GetPacketData());
                if (sendFlag)
                {
                    int ReValue = 0;
                    if (SendPacketWithRetry(m_ErrorZh1113Port, rc, recv))
                    {
                        ReValue = recv.ReciveResult == RecvResult.OK ? 0 : 2;
                        OutResult[0] = recv.u_result;
                        OutResult[1] = recv.i_result;
                        OutResult[2] = recv.dj_result;
                        OutResult[3] = recv.gb_result;
                        OutResult[4] = recv.ct_result;
                        OutResult[5] = recv.tz_result;
                        OutResult[6] = recv.wd_result;
                        OutResult[7] = recv.ny_result;
                        return ReValue;
                    }
                    else
                        return 1;
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
        /// 读取计算值
        /// </summary>
        /// <param name="readType">00--有功；01--无功；04--日计时05--需量06--有功脉冲计数07--无功脉冲计数
        /// 08-有功启动实验脉冲时长（必须先设置有功误差参数，因为会同时做启动电流误差实验）V3.1新增 09-无功启动实验脉冲时长（必须先设置无功误差参数，因为会同时做启动电流误差实验） V3.1新增
        ///</param>
        /// <param name="FrameAry"></param>
        /// <returns></returns>
        public int ReadData(int readType, byte bwNum, out string[] OutWcData, out int OutBwNul, out int OutGroup, out int OutWcNul, out string[] FrameAry)
        {
            OutWcData = new string[0];
            OutBwNul = 0;
            OutGroup = 0;
            OutWcNul = 0;

            ZH1113_RequestReadDataPacket rc2 = new ZH1113_RequestReadDataPacket();
            ZH1113_RequestReadDataReplyPacket recv2 = new ZH1113_RequestReadDataReplyPacket();
            rc2.SetPara(readType, bwNum);



            FrameAry = new string[1];
            try
            {
                FrameAry[0] = BytesToString(rc2.GetPacketData());
                if (sendFlag)
                {

                    if (SendPacketWithRetry(m_ErrorZh1113Port, rc2, recv2))
                    {
                        int ReValue = recv2.ReciveResult == RecvResult.OK ? 0 : 1;
                        OutWcData = recv2.OutstrWcData;
                        OutBwNul = recv2.OutBwNul;
                        OutGroup = recv2.OutGroup;
                        OutWcNul = recv2.OutWcNul;
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
        /// 启动计算
        /// </summary>
        /// <param name="start"></param>
        /// <param name="FrameAry"></param>
        /// <returns></returns>
        public int Start(int start, byte bwNum, out string[] FrameAry)
        {
            ZH1113_RequestStartPacket rc2 = new ZH1113_RequestStartPacket();
            ZH1113_RequestStartReplyPacket recv2 = new ZH1113_RequestStartReplyPacket();
            //rc2.IsNeedReturn = false;

            //add zxg yjt 20220413 新增
            if (bwNum == 0xff)
            {
                rc2.IsNeedReturn = false;
            }

            rc2.SetPara(start, bwNum);
            FrameAry = new string[1];
            try
            {
                FrameAry[0] = BytesToString(rc2.GetPacketData());


                if (sendFlag)
                {
                    if (bwNum == 0xFF)
                    {
                        SendPacketNotRevWithRetry(m_ErrorZh1113Port, rc2, recv2);
                        return 0;
                    }
                    else if (SendPacketWithRetry(m_ErrorZh1113Port, rc2, recv2))
                    {
                        int ReValue = recv2.ReciveResult == RecvResult.OK ? 0 : 1;
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
        /// 停止计算
        /// </summary>
        /// <param name="stop"></param>
        /// <param name="FrameAry"></param>
        /// <returns></returns>
        public int Stop(int stop, byte bwNum, out string[] FrameAry)
        {
            ZH1113_RequestStopPacket rc2 = new ZH1113_RequestStopPacket();
            ZH1113_RequestStopReplyPacket recv2 = new ZH1113_RequestStopReplyPacket();
            //rc2.IsNeedReturn = false;
            rc2.SetPara(stop, bwNum);
            FrameAry = new string[1];
            if (bwNum == 0xff)
            {
                rc2.IsNeedReturn = false;
            }

            try
            {
                FrameAry[0] = BytesToString(rc2.GetPacketData());
                if (sendFlag)
                {
                    if (bwNum == 0xFF)
                    {
                        SendPacketNotRevWithRetry(m_ErrorZh1113Port, rc2, recv2);
                        return 0;
                    }
                    else if (SendPacketWithRetry(m_ErrorZh1113Port, rc2, recv2))
                    {
                        int ReValue = recv2.ReciveResult == RecvResult.OK ? 0 : 1;
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
        /// 对标
        /// </summary>
        /// <param name="index"></param>
        /// <param name="FrameAry"></param>
        /// <returns></returns>
        public int Benchmarking(int index, byte bwNum, out string[] FrameAry)
        {
            ZH1113_RequestBenchmarkingPacket rc2 = new ZH1113_RequestBenchmarkingPacket();
            ZH1113_RequestBenchmarkingReplyPacket recv2 = new ZH1113_RequestBenchmarkingReplyPacket();
            rc2.SetPara(index, bwNum);
            FrameAry = new string[1];
            try
            {
                FrameAry[0] = BytesToString(rc2.GetPacketData());
                if (sendFlag)
                {
                    if (SendPacketWithRetry(m_ErrorZh1113Port, rc2, recv2))
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
        /// 取消队标
        /// </summary>
        /// <param name="index"></param>
        /// <param name="FrameAry"></param>
        /// <returns></returns>
        public int RevBenchmarking(int index, byte bwNum, out string[] FrameAry)
        {
            ZH1113_RequestRevBenchmarkingPacket rc2 = new ZH1113_RequestRevBenchmarkingPacket();
            ZH1113_RequestRevBenchmarkingReplyPacket recv2 = new ZH1113_RequestRevBenchmarkingReplyPacket();
            rc2.SetPara(index, bwNum);
            FrameAry = new string[1];
            try
            {
                FrameAry[0] = BytesToString(rc2.GetPacketData());
                if (sendFlag)
                {
                    if (SendPacketWithRetry(m_ErrorZh1113Port, rc2, recv2))
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
        /// 查询对标状态
        /// </summary>
        /// <param name="index"></param>
        /// <param name="FrameAry"></param>
        /// <returns></returns>
        public int SelectBenchmarking(int index, byte bwNum, out string[] FrameAry)
        {
            ZH1113_RequestSelectBenchmarkingPacket rc2 = new ZH1113_RequestSelectBenchmarkingPacket();
            ZH1113_RequestSelectBenchmarkingReplyPacket recv2 = new ZH1113_RequestSelectBenchmarkingReplyPacket();
            rc2.SetPara(index, bwNum);
            FrameAry = new string[1];
            try
            {
                FrameAry[0] = BytesToString(rc2.GetPacketData());
                if (sendFlag)
                {
                    if (SendPacketWithRetry(m_ErrorZh1113Port, rc2, recv2))
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
        #endregion





        /// <summary>
        /// 设置发送标志位
        /// </summary>
        /// <param name="Flag"></param>
        /// <returns></returns>
        public int SetSendFlag(bool Flag)
        {
            sendFlag = Flag;
            return 0;
        }

        /// <summary>
        /// 发送命令
        /// </summary>
        /// <param name="stPort">端口号</param>
        /// <param name="sp">发送包</param>
        /// <param name="rp">接收包</param>
        /// <returns></returns>
        private bool SendPacketWithRetry(StPortInfo stPort, SendPacket sp, RecvPacket rp)
        {
            for (int i = 0; i < RETRYTIEMS; i++)
            {
                if (driverBase.SendData(stPort, sp, rp) == true)
                {
                    return true;
                }
                Thread.Sleep(100);
            }
            return false;
        }


        /// <summary>
        /// 发送命令 不带返回
        /// </summary>
        /// <param name="stPort">端口号</param>
        /// <param name="sp">发送包</param>
        /// <param name="rp">接收包</param>
        /// <returns></returns>
        private bool SendPacketNotRevWithRetry(StPortInfo stPort, SendPacket sp, RecvPacket rp)
        {
            driverBase.SendData(stPort, sp, rp);
            return true;
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
        /// <summary>
        /// 表位漏电流
        /// </summary>
        /// <param name="pos">1-n</param>
        /// <param name="data">mA</param>
        /// <returns></returns>
        public int SetInsulationLimit(int pos, float data)
        {
            ZH1113_SetNaiYaLimitPacket rc2 = new ZH1113_SetNaiYaLimitPacket();
            ZH1113_SetNaiYaLimitReplyPacket recv2 = new ZH1113_SetNaiYaLimitReplyPacket();

            try
            {
                rc2.SetPara((byte)pos, data);
                if (sendFlag)
                {
                    if (SendPacketWithRetry(m_ErrorZh1113Port, rc2, recv2))
                    {
                        return recv2.ReciveResult == RecvResult.OK ? 0 : 2;
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

        public int ReadInsulationCurrent(int pos, out float[] data)
        {
            data = new float[2];
            ZH1113_ReadNaiYaCurrentPacket rc2 = new ZH1113_ReadNaiYaCurrentPacket();
            ZH1113_ReadNaiYaCurrentReplyPacket recv2 = new ZH1113_ReadNaiYaCurrentReplyPacket();

            try
            {
                rc2.SetPara((byte)pos);
                if (sendFlag)
                {
                    if (SendPacketWithRetry(m_ErrorZh1113Port, rc2, recv2))
                    {
                        data[0] = recv2.overCurrent;
                        data[1] = recv2.Current;
                        return recv2.ReciveResult == RecvResult.OK ? 0 : 2;
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
