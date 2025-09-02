using LYTest.Core;
using LYTest.MeterProtocol.Protocols;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LYTest.MeterProtocol
{
    public class CDLT6452007 : CDLT645
    {

        #region -----------------接口成员-----------------------

        /// <summary>
        /// 密钥下装通用指令
        /// </summary>
        /// <param name="cmd">命令字</param>
        /// <param name="data">数据域</param>
        /// <param name="sequela">是否有后续帧</param>
        /// <param name="revData">返回帧数据域</param>
        /// <returns></returns>
        public override bool UpdateRemoteEncryptionCommand(byte cmd, byte[] data, ref bool sequela, ref byte[] revData)
        {
            FrameMean = "密钥下装指令";
            // 兼容0x1C 、 0x03命令protocolInfo.WriteClass
            byte[] datas;
            if (cmd == 0x1C)
            {
                datas = new byte[data.Length + 4];
                Array.Copy(BitConverter.GetBytes(Convert.ToInt32("0x" + protocolInfo.WritePassword + "98", 16)), datas, 4);
                Array.Copy(data, 0, datas, 4, data.Length);
            }
            else
            {
                datas = new byte[data.Length];
                Array.Copy(data, datas, data.Length);
            }
            return ExeCommand(GetAddressByte(), cmd, datas, ref sequela, ref revData, 1);
        }

        /// <summary>
        /// 通信测试
        /// </summary>
        public override bool ComTest()
        {
            FrameMean = "通信测试";
            byte[] data;
            switch (CommTestType)
            {
                case 1:
                    data = BitConverter.GetBytes(Convert.ToInt32("0x04000402", 16));
                    break;
                case 2:
                    data = BitConverter.GetBytes(Convert.ToInt32("0x00010000", 16));
                    break;

                default:
                    data = BitConverter.GetBytes(Convert.ToInt32("0x04000401", 16)); // 读地址
                    break;
            }
            byte[] revData = new byte[0];
            bool sequela = false;
            return ExeCommand(GetAddressByte(), 0x11, data, ref sequela, ref revData);

        }

        /// <summary>
        /// 广播校时
        /// </summary>
        /// <param name="broadCaseTime">日期时间</param>
        /// <returns></returns>
        public override bool BroadcastTime(DateTime broadCaseTime)
        {
            FrameMean = "广播校时";
            string dateTime = broadCaseTime.ToString("yyMMddHHmmss");

            //功能：强制从站与主站时间同步
            //b)控制码：C=08H
            //c)数据域长度：L=06H
            //d)数据域：YYMMDDhhmmss(年.月.日.时.分.秒)
            byte[] addr = new byte[] { 0x99, 0x99, 0x99, 0x99, 0x99, 0x99 };
            byte[] data = new byte[6];
            byte[] tmp = BitConverter.GetBytes(Convert.ToInt64("0x" + dateTime, 16));
            Array.Copy(tmp, data, 6);
            return ExeCommand(addr, 0x08, data);
        }

        /// <summary>
        /// 读取冻结电量
        /// </summary>
        public override bool ReadSpecialEnergy(int dLType, int times, ref float[] curDL)
        {
            FrameMean = "读取冻结电量";
            string dataFlag = GetFreezeID(dLType, times);      //取出电量的标识符
            float[] energyArry = ReadDataBlock(dataFlag, 4, 2);
            if (energyArry.Length > 0)
            {
                curDL = energyArry;
            }
            return true;
        }

        /// <summary>
        /// 读取冻结电量冻结数据模式字   patternType  ：2,3,4,5,6
        /// </summary>
        /// <param name="type"></param>
        /// <param name="patternType"></param>
        /// <param name="word"></param>
        /// <returns></returns>
        public override bool ReadPatternWord(int type, int patternType, ref string word)
        {
            FrameMean = "读取冻结电量冻结数据模式字";
            string id = "0400090" + patternType.ToString();      //取出整点冻结数据模式字
            word = ReadData(id, 1).ToString();
            return true;
        }

        /// <summary>
        /// 写入冻结电量冻结数据模式字   type  ：2,3,4,5,6   data：
        /// </summary>
        /// <param name="type"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public override bool WritePatternWord(int type, string data) // 5整点
        {
            FrameMean = "写入冻结电量冻结数据模式字";
            //取出整点冻结数据模式字
            return WriteData("0400090" + type.ToString(), 1, data);
        }

        /// <summary>
        /// 时段表切换时间 freezeType 切换时间类型 1=两套时区表切换时间，2=两套日，3=两套费率电价切换时间，4=两套梯度切换时间,5=整点冻结起始时间 6=时间间隔
        /// </summary>
        /// <param name="freezeType"></param>
        /// <param name="freezeTime"></param>
        public override bool ReadFreezeTime(int freezeType, ref string freezeTime)
        {
            FrameMean = "读取冻结时间";
            string id = "";
            int length = 5;
            if (freezeType == 1)
                id = "04000106";
            else if (freezeType == 2)
                id = "04000107";
            else if (freezeType == 3)
                id = "04000108";
            else if (freezeType == 4)
                id = "04000109";
            else if (freezeType == 5)
                id = "04001201";
            else if (freezeType == 6)
            {
                id = "04001202";
                length = 1;
            }
            freezeTime = ReadData(id, length).ToString();

            return true;
        }

        /// <summary>
        /// 读取电量(分费率读取)
        /// </summary>
        /// <param name="ept_PDirect">功率类型</param>
        /// <param name="ett_TariffType">费率类型</param>
        /// <returns>返回电量</returns>
        public override float ReadEnergy(byte energyType, byte tariffType)
        {
            FrameMean = "读取电量";
            float energy = -1F;

            if (energyType < 0 || energyType > 8 || tariffType < 0 || tariffType > 63)
                return energy;

            bool block = false;
            if (ReadEnergyType == 0) block = true;

            int index = protocolInfo.TariffOrderType.IndexOf(tariffType.ToString()) + 1;      //按设置的费率排序取出电量

            string dataFlag = GetEnergyID(block, energyType, index);      //取出电量的标识符
            if (block)
            {
                float[] energyArry = ReadDataBlock(dataFlag, 4, 2);

                if (energyArry.Length > 0)
                {
                    if (tariffType == 0)        //总电量
                    {
                        if (energyArry.Length > 0)      //解电量块有数据
                            energy = energyArry[0];
                    }
                    else                                //尖峰平谷电量
                    {
                        if (index < energyArry.Length)
                            energy = energyArry[index];
                    }
                }
            }
            else
                return ReadData(dataFlag, 4, 2);

            return energy;
        }
        public override FloatE ReadEnergyR(byte energyType, byte tariffType)
        {
            FrameMean = "读取电量";
            FloatE energy = -1m;

            if (energyType < 0 || energyType > 8 || tariffType < 0 || tariffType > 63)
                return energy;

            bool block = false;
            if (ReadEnergyType == 0) block = true;

            int index = protocolInfo.TariffOrderType.IndexOf(tariffType.ToString()) + 1;      //按设置的费率排序取出电量

            string dataFlag = GetEnergyID(block, energyType, index);      //取出电量的标识符
            if (block)
            {
                FloatE[] energyArry = ReadDataBlockR(dataFlag, 4, 2);

                if (energyArry.Length > 0)
                {
                    if (tariffType == 0)        //总电量
                    {
                        if (energyArry.Length > 0)      //解电量块有数据
                            energy = energyArry[0];
                    }
                    else                                //尖峰平谷电量
                    {
                        if (index < energyArry.Length)
                            energy = energyArry[index];
                    }
                }
            }
            else
                return ReadDataR(dataFlag, 2);

            return energy;
        }
        public override float ReadEnergyExtension(byte energyType, byte tariffType, int DecimalDigits)
        {

            FrameMean = "读取电量";
            float energy = -1F;

            if (energyType < 0 || energyType > 8 || tariffType < 0 || tariffType > 63)
                return energy;

            bool block = false;
            if (ReadEnergyType == 0) block = true;

            int index = protocolInfo.TariffOrderType.IndexOf(tariffType.ToString()) + 1;      //按设置的费率排序取出电量

            string dataFlag = GetEnergyID(block, energyType, index, DecimalDigits);      //取出电量的标识符
            if (block)//不要块读
            {
                float[] energyArry = ReadDataBlock(dataFlag, 4, DecimalDigits);

                if (energyArry.Length > 0)
                {
                    if (tariffType == 0)        //总电量
                    {
                        if (energyArry.Length > 0)      //解电量块有数据
                            energy = energyArry[0];
                    }
                    else                                //尖峰平谷电量
                    {
                        if (index < energyArry.Length)
                            energy = energyArry[index];
                    }
                }
            }
            else
                return ReadData(dataFlag, 4, DecimalDigits);

            return energy;
        }

        public override FloatE ReadEnergyExtensionR(byte energyType, byte tariffType, int DecimalDigits)
        {

            FrameMean = "读取电量";
            decimal energy = -1m;

            if (energyType < 0 || energyType > 8 || tariffType < 0 || tariffType > 63)
                return energy;

            bool block = false;
            if (ReadEnergyType == 0) block = true;

            int index = protocolInfo.TariffOrderType.IndexOf(tariffType.ToString()) + 1;      //按设置的费率排序取出电量

            string dataFlag = GetEnergyID(block, energyType, index, DecimalDigits);      //取出电量的标识符
            if (block)//不要块读
            {
                FloatE[] energyArry = ReadDataBlockR(dataFlag, 4, DecimalDigits);

                if (energyArry.Length > 0)
                {
                    if (tariffType == 0)        //总电量
                    {
                        if (energyArry.Length > 0)      //解电量块有数据
                            energy = energyArry[0];
                    }
                    else                                //尖峰平谷电量
                    {
                        if (index < energyArry.Length)
                            energy = energyArry[index];
                    }
                }
            }
            else
                return ReadDataR(dataFlag, DecimalDigits);

            return energy;
        }
        /// <summary>
        /// 读取电量(所有费率读取)
        /// </summary>
        /// <param name="energyType">功率类型</param>
        /// <param name="freezeTimes">冻结次数</param>
        /// <returns></returns>
        public override float[] ReadEnergys(byte energyType, int freezeTimes)
        {
            FrameMean = "读取电量";
            float[] energy = new float[0];

            if (energyType < 0 || energyType > 8)//读取电量指定功率类型超出范围[0-7]
                return energy;

            string dataFlag = GetEnergyIDs(true, energyType, freezeTimes);      //取出电量的标识符                

            float[] energyArry = ReadDataBlock(dataFlag, 4, 2);
            if (energyArry.Length > 0)
            {
                if (energyArry.Length > 0)
                {
                    Array.Resize(ref energy, 5);
                    energy[0] = energyArry[0];
                    for (int i = 1; i < energyArry.Length; i++)
                    {
                        int index = Convert.ToInt16(protocolInfo.TariffOrderType.Substring(i - 1, 1));      //按设置的费率排序取出电量
                        energy[index] = energyArry[i];
                    }
                }
            }
            return energy;
        }

        /// <summary>
        /// 读取电量(所有费率读取)
        /// </summary>
        /// <param name="int_Type">读取类型0=块读，1=分项读</param>
        /// <param name="ept_PDirect">功率类型</param>
        /// <param name="sng_Energy">返回电量</param>
        /// <returns></returns>
        public override float[] ReadEnergy(byte energyType)
        {
            FrameMean = "读取电量";
            float[] energy = new float[0];

            int pDirect = energyType;
            if (pDirect < 0 || pDirect > 8)
                return energy;

            string dataFlag = GetEnergyID(true, pDirect, 0);      //取出电量的标识符
            float[] energyArry = ReadDataBlock(dataFlag, 4, 2);
            if (energyArry.Length > 0)
            {
                if (energyArry.Length > 0)
                {
                    Array.Resize(ref energy, energyArry.Length);
                    energy[0] = energyArry[0];
                    for (int i = 1; i < energyArry.Length; i++)
                    {
                        int index = Convert.ToInt16(protocolInfo.TariffOrderType.Substring(i - 1, 1));      //按设置的费率排序取出电量
                        energy[index] = energyArry[i];
                    }
                }
            }
            return energy;
        }

        public override float[] ReadEnergyExtension(byte energyType, int DecimalDigits)
        {
            FrameMean = "读取电量";
            float[] energy = new float[0];

            int pDirect = energyType;
            if (pDirect < 0 || pDirect > 8)
                return energy;

            string dataFlag = GetEnergyID(true, pDirect, 0, DecimalDigits);      //取出电量的标识符
            float[] energyArry = ReadDataBlock(dataFlag, 4, DecimalDigits);
            if (energyArry.Length > 0)
            {
                if (energyArry.Length > 0)
                {
                    Array.Resize(ref energy, energyArry.Length);
                    energy[0] = energyArry[0];
                    for (int i = 1; i < energyArry.Length; i++)
                    {
                        int index = Convert.ToInt16(protocolInfo.TariffOrderType.Substring(i - 1, 1));      //按设置的费率排序取出电量
                        energy[index] = energyArry[i];
                    }
                }
            }
            return energy;
        }

        /// <summary>
        /// 读取需量(分费率读取)
        /// </summary>
        /// <param name="int_Type">读取类型0=块读，1=分项读</param>
        /// <param name="ept_PDirect">功率类型</param>
        /// <param name="ett_TariffType">费率类型</param>
        /// <param name="sng_Demand">返回需量</param>
        /// <returns></returns>
        public override float ReadDemand(byte energyType, byte tariffType1)
        {
            FrameMean = "读取需量";

            int pw = energyType;
            int tariffType = tariffType1;

            float demand = -1F;
            //读取需量指定功率类型超出范围[0-7]
            if (pw < 0 || pw > 8)
                return demand;

            //读取需量指定费率类型超出范围[0-4]
            if (tariffType < 0 || tariffType > 63)
                return demand;

            bool block = false;
            if (ReadDemandType == 0) block = true;

            int int_Index = protocolInfo.TariffOrderType.IndexOf(tariffType.ToString()) + 1;      //按设置的费率排序取出需量

            string str_ID = GetDemandID(block, pw, int_Index);      //取出需量的标识符
            if (block)
            {
                string[] str_DemandArry = ReadDataBlock(str_ID, 8);

                if (str_DemandArry.Length > 0)
                {
                    if (tariffType == 0)        //总需量
                    {
                        if (str_DemandArry.Length > 0)      //解需量块有数据
                            demand = Convert.ToSingle(str_DemandArry[0].Substring(10, 6)) / 10000F;
                    }
                    else                                //尖峰平谷需量
                    {
                        if (int_Index < str_DemandArry.Length)
                            demand = Convert.ToSingle(str_DemandArry[int_Index].Substring(0, 8)) / 10000.0f;
                    }
                }

            }
            else
            {
                string str_Demand = ReadData(str_ID, 8);
                if (str_Demand != string.Empty)
                {
                    demand = Convert.ToSingle(str_Demand.Substring(10, 6)) / 10000.0f;
                }

            }

            return demand;
        }

        /// <summary>
        /// 读取需量(所有费率读取)
        /// </summary>
        /// <param name="int_Type">读取类型0=块读，1=分项读</param>
        /// <param name="ept_PDirect">功率类型</param>
        /// <param name="sng_Demand">返回需量</param>
        /// <returns></returns>
        public override float[] ReadDemand(byte energyType)
        {
            FrameMean = "读取需量";
            float[] sng_Demand = new float[0];

            int int_PDirect = energyType;// (int)ept_PDirect;
            if (int_PDirect < 0 || int_PDirect > 8)
            {
                //m_str_LostMessage = "读取需量指定功率类型超出范围[0-7]";
                return sng_Demand;
            }
            bool bln_Block = false;
            if (ReadDemandType == 0) bln_Block = true;

            if (bln_Block)
            {
                string str_ID = GetDemandID(bln_Block, int_PDirect, 0);      //取出需量的标识符
                string[] str_DemandArry = ReadDataBlock(str_ID, 8);
                if (str_DemandArry.Length > 0)
                {
                    if (str_DemandArry.Length > 0)
                    {
                        Array.Resize(ref sng_Demand, 5);
                        sng_Demand[0] = Convert.ToSingle(str_DemandArry[0].Substring(0, 8)) / 10000.0f;
                        for (int i = 1; i < str_DemandArry.Length; i++)
                        {
                            int int_Index = Convert.ToInt16(protocolInfo.TariffOrderType.Substring(i - 1, 1));      //按设置的费率排序取出需量
                            sng_Demand[int_Index] = Convert.ToSingle(str_DemandArry[i].Substring(0, 8)) / 10000.0f;
                        }
                    }

                }

            }
            else
            {
                Array.Resize(ref sng_Demand, 5);
                string dataFlag = GetDemandID(bln_Block, int_PDirect, 0);      //取出需量总的标识符
                string demand = ReadData(dataFlag, 8);
                if (demand != string.Empty)
                {
                    sng_Demand[0] = Convert.ToSingle(demand.Substring(0, 8)) / 10000.0f;
                    for (int i = 1; i < 5; i++)
                    {
                        int int_Index = protocolInfo.TariffOrderType.IndexOf(i.ToString()) + 1;      //按设置的费率排序取出需量
                        dataFlag = GetDemandID(bln_Block, int_PDirect, int_Index);      //取出需量费率的标识符
                        demand = ReadData(dataFlag, 8);
                        if (demand != string.Empty)
                            sng_Demand[i] = Convert.ToSingle(demand.Substring(0, 8)) / 10000.0f;
                        else
                            break;
                    }
                }

            }

            return sng_Demand;

        }


        /// <summary>
        /// 读日期时间
        /// </summary>
        /// <param name="int_Type">读取类型 </param>
        /// <param name="str_DateTime">返回时间 yy-mm-dd hh:mm:ss</param>
        /// <returns></returns>
        public override DateTime ReadDateTime()
        {
            FrameMean = "读取日期时间";
            string str_DateTime = string.Empty;

            bool bln_Sequela = false;
            byte[][] byt_ID = new byte[][]{new byte[]{0x01,0x01 ,0x00,0x04},
                                           new byte[]{0x02,0x01 ,0x00,0x04}};
            //------------读日期
            byte[] byt_RevData = new byte[0];
            bool bln_Result = ExeCommand(GetAddressByte(), 0x11, byt_ID[0], ref bln_Sequela, ref byt_RevData);
            byte[] byt_DateTime = new byte[6];
            if (bln_Result)
            {
                if (byt_RevData[0] == byt_ID[0][0] && byt_RevData[1] == byt_ID[0][1] && byt_RevData[2] == byt_ID[0][2] && byt_RevData[3] == byt_ID[0][3])
                {
                    byt_DateTime[2] = byt_RevData[5];
                    byt_DateTime[1] = byt_RevData[6];
                    byt_DateTime[0] = byt_RevData[7];

                    //----------读时间
                    byt_RevData = new byte[0];
                    bln_Result = ExeCommand(GetAddressByte(), 0x11, byt_ID[1], ref bln_Sequela, ref byt_RevData);
                    if (bln_Result)
                    {
                        if (byt_RevData[0] == byt_ID[1][0] && byt_RevData[1] == byt_ID[1][1] && byt_RevData[2] == byt_ID[1][2] && byt_RevData[3] == byt_ID[1][3])
                        {
                            byt_DateTime[5] = byt_RevData[4];
                            byt_DateTime[4] = byt_RevData[5];
                            byt_DateTime[3] = byt_RevData[6];
                            string str_Tmp = BitConverter.ToString(byt_DateTime).Replace("-", "");
                            string[] str_Para = new string[] { "YY", "MM", "DD", "HH", "FF", "SS" };
                            string dateTimeFormat = protocolInfo.DateTimeFormat;
                            if (str_Tmp.Substring(0, 2) != "20") dateTimeFormat = dateTimeFormat.Replace("YYYY", "YY");
                            if (str_Tmp.Length == 12) dateTimeFormat = dateTimeFormat.Replace("YYYY", "YY");

                            if (str_Tmp.Length < 14) dateTimeFormat = dateTimeFormat.Replace("WW", "");
                            for (int i = 0; i < 6; i++)
                            {
                                int int_Index = dateTimeFormat.IndexOf(str_Para[i]);
                                str_DateTime += str_Tmp.Substring(int_Index, 2);
                                if (i < 2)
                                    str_DateTime += "-";
                                else if (i == 2)
                                    str_DateTime += " ";
                                else if (i != 5)
                                    str_DateTime += ":";
                            }

                        }

                    }

                }

            }
            else
            {
                str_DateTime = "01-01-01 00:00:00";
            }

            return DateTime.Parse(str_DateTime);
        }

        /// <summary>
        /// 读地址
        /// </summary>
        /// <param name="int_Type">类型，0=强制读取地址，1=探测读取地址</param>
        /// <param name="str_Address"></param>
        /// <returns></returns>
        public override string ReadAddress()
        {
            FrameMean = "读取地址";
            string addr = string.Empty;

            bool r = CptReadAddress(ref addr);
            if (!r)
                DetectAddress(ref addr);

            return addr;
        }

        /// <summary>
        /// 读取时段
        /// </summary>
        /// <param name="int_Type">类型0=块读(hhmmNN)，1=块读(NNhhmm)</param>
        /// <param name="str_PTime">返回时段</param>
        /// <returns></returns>
        public override string[] ReadPeriodTime()
        {
            FrameMean = "读取时段";
            string[] str_PTime = new string[0];

            float sng_PCount = ReadData("04000203", 1, 0);     //读取时段数

            //04010001

            string[] str_Data = ReadDataBlock("04010001", 3);     //读取时段块

            if (str_Data.Length > 0)
            {
                if (str_Data.Length >= sng_PCount)
                {
                    Array.Resize(ref str_PTime, Convert.ToInt16(sng_PCount));
                    for (int i = 0; i < Convert.ToInt16(sng_PCount); i++)
                        if (ReadPeriodTimeType == 0)
                            str_PTime[i] = str_Data[i];
                        else
                            str_PTime[i] = str_Data[i].Substring(2, 4) + str_Data[i].Substring(0, 2);
                }
                else
                {
                    Array.Resize(ref str_PTime, Convert.ToInt16(str_Data.Length));
                    for (int i = 0; i < str_Data.Length; i++)
                        if (ReadPeriodTimeType == 0)
                            str_PTime[i] = str_Data[i];
                        else
                            str_PTime[i] = str_Data[i].Substring(2, 4) + str_Data[i].Substring(0, 2);
                }
                //return true;
            }

            return str_PTime;
        }

        /// <summary>
        /// 读取数据（数据型，数据项）
        /// </summary>
        /// <param name="dataFlag">标识符,两个字节</param>
        /// <param name="len">数据长度(字节数)</param>
        /// <param name="dot">小数位</param>
        /// <param name="bln_Reverse">解释方式，true=高低位对调，false=高低位正常</param>
        /// <param name="sng_Value">返回数据</param>
        /// <returns></returns>
        public override float ReadData(string dataFlag, int len, int dot)
        {
            FrameMean = "读取数据";
            float value = -1F;

            if (dataFlag.Length != 8) return value;

            byte[] dFlag = BitConverter.GetBytes(Convert.ToInt32("0x" + dataFlag, 16));
            byte[] revData = new byte[0];
            bool sequela = false;
            bool result = ExeCommand(GetAddressByte(), 0x11, dFlag, ref sequela, ref revData);
            if (result)
            {
                if (revData[0] == dFlag[0] && revData[1] == dFlag[1]
                    && revData[2] == dFlag[2] && revData[3] == dFlag[3])
                {
                    int actLen = revData.Length - 4;
                    byte[] byt_TmpValue = new byte[actLen];
                    Array.Copy(revData, 4, byt_TmpValue, 0, actLen);
                    Array.Reverse(byt_TmpValue);   //倒序
                    string tmp = BitConverter.ToString(byt_TmpValue, 0).Replace("-", "");
                    value = Convert.ToSingle(tmp) / Convert.ToSingle(Math.Pow(10, dot));
                }
            }

            return value;
        }
        public FloatE ReadDataR(string dataFlag, int dot)
        {
            FrameMean = "读取数据";
            FloatE value = -1m;

            if (dataFlag.Length != 8) return value;

            byte[] dFlag = BitConverter.GetBytes(Convert.ToInt32("0x" + dataFlag, 16));
            byte[] revData = new byte[0];
            bool sequela = false;
            bool result = ExeCommand(GetAddressByte(), 0x11, dFlag, ref sequela, ref revData);
            if (result)
            {
                if (revData[0] == dFlag[0] && revData[1] == dFlag[1]
                    && revData[2] == dFlag[2] && revData[3] == dFlag[3])
                {
                    int actLen = revData.Length - 4;
                    byte[] byt_TmpValue = new byte[actLen];
                    Array.Copy(revData, 4, byt_TmpValue, 0, actLen);
                    Array.Reverse(byt_TmpValue);   //倒序
                    string tmp = BitConverter.ToString(byt_TmpValue, 0).Replace("-", "");
                    value = (Convert.ToDecimal(tmp) / Convert.ToDecimal(Math.Pow(10, dot))).ToString($"F{dot}");
                }
            }

            return value;
        }

        /// <summary>
        /// 读取负荷记录（字符型，数据项）
        /// </summary>
        /// <param name="dataFlag">标识符,两个字节</param>
        /// <param name="len">数据长度(字节数)</param>
        /// <param name="bln_Reverse">解释方式，true=高低位对调，false=高低位正常</param>
        /// <param name="str_Value">返回数据</param>
        /// <returns></returns>
        public override string ReadData(string dataFlag, int len, string item)
        {
            FrameMean = "读取负荷记录";
            string value = string.Empty;

            if (dataFlag.Length != 8)
            {
                //m_str_LostMessage = "标识符不符合要求，不是4字节";
                return value;
            }
            byte[] dFlag = BitConverter.GetBytes(Convert.ToInt32("0x" + dataFlag, 16));
            byte[] tmp = GetBytesArry(6, item, true);
            byte[] read = new byte[tmp.Length + dFlag.Length];
            byte[] revData = new byte[0];
            bool sequela = false;
            Array.Copy(dFlag, 0, read, 0, dFlag.Length);
            Array.Copy(tmp, 0, read, 4, tmp.Length);

            bool result = ExeCommand(GetAddressByte(), 0x11, read, ref sequela, ref revData);
            if (result)
            {
                if (revData[0] == dFlag[0] && revData[1] == dFlag[1]
                    && revData[2] == dFlag[2] && revData[3] == dFlag[3])
                {
                    if (len != revData.Length - 4)
                        len = revData.Length - 4;
                    byte[] tmpValue = new byte[len];
                    Array.Copy(revData, 4, tmpValue, 0, len);
                    Array.Reverse(tmpValue);
                    value = BitConverter.ToString(tmpValue, 0, len).Replace("-", "");
                }


            }

            return value;
        }

        /// <summary>
        /// 读取数据（字符型，数据项）
        /// </summary>
        /// <param name="dataFlag">标识符,两个字节</param>
        /// <param name="len">数据长度(字节数)</param>
        /// <returns></returns>
        public override string ReadData(string dataFlag, int len)
        {
            FrameMean = "读取数据";
            string value = string.Empty;

            if (dataFlag.Length != 8) return value;

            byte[] dFlag = BitConverter.GetBytes(Convert.ToInt32("0x" + dataFlag, 16));
            byte[] revData = new byte[0];
            bool sequela = false;
            bool result = ExeCommand(GetAddressByte(), 0x11, dFlag, ref sequela, ref revData);
            if (result)
            {
                if (revData[0] == dFlag[0] && revData[1] == dFlag[1]
                    && revData[2] == dFlag[2] && revData[3] == dFlag[3])
                {
                    if (len < revData.Length - 4)
                        len = revData.Length - 4;
                    if (len > revData.Length - 4)
                        len = revData.Length - 4;
                    byte[] tmpValue = new byte[len];
                    Array.Copy(revData, 4, tmpValue, 0, len);
                    Array.Reverse(tmpValue);
                    value = BitConverter.ToString(tmpValue, 0, len).Replace("-", "");
                }

            }

            return value;
        }


        /// <summary>
        /// 读取数据（数据型，数据块）
        /// </summary>
        /// <param name="dataFlag">标识符,两个字节</param>
        /// <param name="len">数据长度(字节数)</param>
        /// <param name="dot">小数位</param>
        /// <returns></returns>
        public override float[] ReadDataBlock(string dataFlag, int len, int dot)
        {
            FrameMean = "读取数据";
            float[] value = new float[0];

            if (dataFlag.Length != 8) return value;// false;

            byte[] dFlag = BitConverter.GetBytes(Convert.ToInt32("0x" + dataFlag, 16));
            byte[] revData = new byte[0];
            bool sequela = false;
            bool result = ExeCommand(GetAddressByte(), 0x11, dFlag, ref sequela, ref revData);
            if (result)
            {

                if (revData[0] == dFlag[0] && revData[1] == dFlag[1] && revData[2] == dFlag[2] && revData[3] == dFlag[3])
                {
                    byte[] arr = new byte[revData.Length - 4];
                    Array.Copy(revData, 4, arr, 0, revData.Length - 4);
                    Array.Reverse(arr);
                    string tmp = BitConverter.ToString(arr, 0);
                    tmp = tmp.Replace("-", "");

                    tmp = tmp.TrimStart(new char[] { 'A' });        //去掉AA
                    tmp = tmp.TrimEnd(new char[] { 'A' });

                    if (tmp.IndexOf("AA") > 0)      //防止各项数据项目用AA隔开
                    {
                        string[] pp = tmp.Split(new string[] { "AA" }, StringSplitOptions.None);
                        if (len * pp.Length * 2 + (pp.Length - 1) * 2 == tmp.Length)
                        {
                            Array.Resize(ref value, pp.Length);
                            for (int i = 0; i < pp.Length; i++)
                                value[i] = Convert.ToSingle(pp[i]) / Convert.ToSingle(Math.Pow(10, dot));
                            Array.Reverse(value);
                        }
                    }

                    int count = tmp.Length / (len * 2);
                    Array.Resize(ref value, count);
                    for (int i = 0; i < count; i++)
                        value[i] = Convert.ToSingle(tmp.Substring(i * len * 2, len * 2)) / Convert.ToSingle(Math.Pow(10, dot));
                    Array.Reverse(value);
                }


            }

            return value;
        }
        public FloatE[] ReadDataBlockR(string dataFlag, int len, int dot)
        {
            FrameMean = "读取数据";
            FloatE[] value = new FloatE[0];

            if (dataFlag.Length != 8) return value;// false;

            byte[] dFlag = BitConverter.GetBytes(Convert.ToInt32("0x" + dataFlag, 16));
            byte[] revData = new byte[0];
            bool sequela = false;
            bool result = ExeCommand(GetAddressByte(), 0x11, dFlag, ref sequela, ref revData);
            if (result)
            {

                if (revData[0] == dFlag[0] && revData[1] == dFlag[1] && revData[2] == dFlag[2] && revData[3] == dFlag[3])
                {
                    byte[] arr = new byte[revData.Length - 4];
                    Array.Copy(revData, 4, arr, 0, revData.Length - 4);
                    Array.Reverse(arr);
                    string tmp = BitConverter.ToString(arr, 0);
                    tmp = tmp.Replace("-", "");

                    tmp = tmp.TrimStart(new char[] { 'A' });        //去掉AA
                    tmp = tmp.TrimEnd(new char[] { 'A' });

                    if (tmp.IndexOf("AA") > 0)      //防止各项数据项目用AA隔开
                    {
                        string[] pp = tmp.Split(new string[] { "AA" }, StringSplitOptions.None);
                        if (len * pp.Length * 2 + (pp.Length - 1) * 2 == tmp.Length)
                        {
                            Array.Resize(ref value, pp.Length);
                            for (int i = 0; i < pp.Length; i++)
                                value[i] = (Convert.ToDecimal(pp[i]) / Convert.ToDecimal(Math.Pow(10, dot))).ToString($"F{dot}");
                            Array.Reverse(value);
                        }
                    }

                    int count = tmp.Length / (len * 2);
                    Array.Resize(ref value, count);
                    for (int i = 0; i < count; i++)
                        value[i] = (Convert.ToDecimal(tmp.Substring(i * len * 2, len * 2)) / Convert.ToDecimal(Math.Pow(10, dot))).ToString($"F{dot}");
                    Array.Reverse(value);
                }

            }

            return value;
        }

        /// <summary>
        /// 读取数据（字符型，数据块）
        /// </summary>
        /// <param name="dataFlag">标识符,两个字节</param>
        /// <param name="len">数据长度(字节数)</param>
        /// <returns></returns>
        public override string[] ReadDataBlock(string dataFlag, int len)
        {
            FrameMean = "读取数据";
            string[] value = new string[0];

            if (dataFlag.Length != 8)//标识符不符合要求，不是4字节"
                return value;

            byte[] dFlag = BitConverter.GetBytes(Convert.ToInt32("0x" + dataFlag, 16));
            byte[] revData = new byte[0];
            bool sequela = false;
            bool result = ExeCommand(GetAddressByte(), 0x11, dFlag, ref sequela, ref revData);
            if (result)
            {
                if (revData[0] == dFlag[0] && revData[1] == dFlag[1] && revData[2] == dFlag[2] && revData[3] == dFlag[3])
                {
                    byte[] tmpValue = new byte[revData.Length - 4];
                    Array.Copy(revData, 4, tmpValue, 0, revData.Length - 4);
                    Array.Reverse(tmpValue);
                    string tmp = BitConverter.ToString(tmpValue, 0);
                    tmp = tmp.Replace("-", "");

                    tmp = tmp.TrimStart(new char[] { 'A' });        //去掉AA
                    tmp = tmp.TrimEnd(new char[] { 'A' });

                    if (tmp.IndexOf("AA") > 0)      //防止各项数据项目用AA隔开
                    {
                        string[] data = tmp.Split(new string[] { "AA" }, StringSplitOptions.None);
                        if (len * data.Length * 2 + (data.Length - 1) * 2 == tmp.Length)
                        {
                            Array.Resize(ref value, data.Length);
                            for (int i = 0; i < data.Length; i++)
                                value[i] = data[i];
                            Array.Reverse(value);
                        }
                    }

                    int count = tmp.Length / (len * 2);
                    Array.Resize(ref value, count);
                    for (int i = 0; i < count; i++)
                        value[i] = tmp.Substring(i * len * 2, len * 2);
                    Array.Reverse(value);
                }

            }

            return value;
        }

        /// <summary>
        /// 写地址
        /// </summary>
        /// <param name="int_Type">操作类型</param>
        /// <param name="address"></param>
        /// <returns></returns>
        public override bool WriteAddress(string address)
        {
            FrameMean = "写地址";
            //a)功能：设置某从站的通信地址，仅支持点对点通信。
            //b)控制码：C=15H
            //c)地址域：AA…AAH
            //d)数据域长度：L=06H
            //e)数据域：A0…A5（通信地址）
            //注：	本命令必须与编程键配合使用。

            byte[] uAddr = new byte[] { 0xAA, 0xAA, 0xAA, 0xAA, 0xAA, 0xAA };
            byte[] data = new byte[6];

            string tmp = "0x";
            if (address.Length > 12)
                tmp += address.Substring(0, 12);
            else if (address.Length == 0)
                tmp += "0";
            else
                tmp += address;

            byte[] byt_Tmp = BitConverter.GetBytes(Convert.ToInt64(tmp, 16));
            Array.Copy(byt_Tmp, data, 6);

            byte[] revData = new byte[0];
            bool sequela = false;
            return ExeCommand(uAddr, 0x15, data, ref sequela, ref revData);
        }
        /// <summary>
        /// 写地址
        /// </summary>
        /// <param name="int_Type">操作类型</param>
        /// <param name="address"></param>
        /// <returns></returns>
        public override bool WriteAddress(string address, out string rAddr)
        {
            rAddr = string.Empty;
            FrameMean = "写地址";
            //a)功能：设置某从站的通信地址，仅支持点对点通信。
            //b)控制码：C=15H
            //c)地址域：AA…AAH
            //d)数据域长度：L=06H
            //e)数据域：A0…A5（通信地址）
            //注：	本命令必须与编程键配合使用。

            byte[] uAddr = new byte[] { 0xAA, 0xAA, 0xAA, 0xAA, 0xAA, 0xAA };
            byte[] data = new byte[6];

            string tmp = "0x";
            if (address.Length > 12)
                tmp += address.Substring(0, 12);
            else if (address.Length == 0)
                tmp += "0";
            else
                tmp += address;

            byte[] byt_Tmp = BitConverter.GetBytes(Convert.ToInt64(tmp, 16));
            Array.Copy(byt_Tmp, data, 6);

            byte[] revData = new byte[0];
            bool sequela = false;
            byte[] revAddr = new byte[0];
            bool rst = ExeCommand(uAddr, 0x15, data, ref sequela, ref revData, ref revAddr);
            if (rst)
            {
                rAddr = BitConverter.ToString(revAddr.Reverse().ToArray()).Replace("-", "");
            }
            return rst;
        }

        /// <summary>
        /// 写日期时间
        /// </summary>
        /// <param name="int_Type">类型</param>
        /// <param name="dateTime">日期时间(YYYYMMDDhhmmss)</param>
        /// <returns></returns>
        public override bool WriteDateTime(string dateTime)
        {
            FrameMean = "写日期时间";
            string tmp = dateTime.Substring(2, 6) + "0" + Convert.ToString(GetWeekday(dateTime, protocolInfo.SundayIndex));
            bool result = WriteData("04000101", 4, tmp);
            if (result)
            {
                tmp = dateTime.Substring(8, 6);
                result = WriteData("04000102", 3, tmp);
            }

            return result;
        }
        public override bool WriteDateTime()
        {
            FrameMean = "写日期时间";
            DateTime dateTime = DateTime.Now.AddSeconds(1);
            string tmp = dateTime.ToString("yyyyMMddHHmmss").Substring(2, 6) + "0" + Convert.ToString(GetWeekday(dateTime.ToString("yyyyMMddHHmmss"), protocolInfo.SundayIndex));
            bool result = WriteData("04000101", 4, tmp);
            if (result)
            {
                tmp = dateTime.ToString("yyyyMMddHHmmss").Substring(8, 6);
                result = WriteData("04000102", 3, tmp);
            }

            return result;
        }

        /// <summary>
        /// 写操作
        /// </summary>
        /// <param name="int_Type"></param>
        /// <param name="periodTime"></param>
        /// <returns></returns>
        public override bool WritePeriodTime(string[] periodTime)
        {
            FrameMean = "写切换时间";
            bool bln_Result = WriteData("04010001", 3, periodTime);
            if (bln_Result)
                bln_Result = WriteData("04000203", 1, 0, Convert.ToSingle(periodTime.Length));
            return bln_Result;

        }

        /// <summary>
        /// 写费率1(字符型，数据项)
        /// </summary>
        /// <param name="dataFlag">标识符</param>
        /// <param name="value">写入数据</param>
        /// <returns></returns>
        public override bool WriteRatesPrice(string dataFlag, byte[] value)
        {
            FrameMean = "写费率";
            if (dataFlag.Length != 8) return false;

            byte[] dFlag = BitConverter.GetBytes(Convert.ToInt32("0x" + dataFlag, 16));

            int dataLen = 12;                                //标识符4字节,用户代码4字节， 密码4字节
            dataLen += value.Length;                             //数据
            byte[] data = new byte[dataLen];
            Array.Copy(dFlag, 0, data, 0, 4);

            byte[] tmp = BitConverter.GetBytes(Convert.ToInt32("0x" + protocolInfo.WritePassword + "99", 16));
            Array.Copy(tmp, 0, data, 4, 4);

            string str_Tmp = protocolInfo.UserID;
            if (str_Tmp.Length > 8) str_Tmp = str_Tmp.Substring(0, 8);

            tmp = BitConverter.GetBytes(Convert.ToInt32("0x" + str_Tmp, 16));
            Array.Copy(tmp, 0, data, 8, 4);

            Array.Copy(value, 0, data, 12, value.Length);

            byte[] revData = new byte[0];
            bool sequela = false;
            return ExeCommand(GetAddressByte(), 0x14, data, ref sequela, ref revData);
        }

        /// <summary>
        /// 写数据(字符型，数据项)
        /// </summary>
        /// <param name="dataFlag">数据标识</param>
        /// <param name="value">写入数据</param>
        /// <returns></returns>
        public override bool WriteData(string dataFlag, byte[] value)
        {
            FrameMean = "写数据";
            if (dataFlag.Length != 8) return false;

            byte[] data = new byte[12 + value.Length]; //标识符4字节 + 用户代码4字节 + 密码4字节 + 数据
            data[3] = Convert.ToByte(dataFlag.Substring(0, 2), 16);
            data[2] = Convert.ToByte(dataFlag.Substring(2, 2), 16);
            data[1] = Convert.ToByte(dataFlag.Substring(4, 2), 16);
            data[0] = Convert.ToByte(dataFlag.Substring(6, 2), 16);

            int tmp;
            int t = CheckStrIDType(dataFlag);  //获取参数分类 1类参数，2类参数
            if (t == 1)
            {
                tmp = Convert.ToInt32("0x" + protocolInfo.WritePassword + "99", 16);
            }
            else if (t == 2)
            {
                tmp = Convert.ToInt32("0x" + protocolInfo.WritePassword + "98", 16);
            }
            else
            {
                tmp = Convert.ToInt32("0x" + protocolInfo.WritePassword + protocolInfo.WriteClass, 16);
            }
            Array.Copy(BitConverter.GetBytes(tmp), 0, data, 4, 4);

            string userId = protocolInfo.UserID;
            if (userId.Length > 8) userId = userId.Substring(0, 8);

            tmp = Convert.ToInt32("0x" + userId, 16);
            Array.Copy(BitConverter.GetBytes(tmp), 0, data, 8, 4);

            Array.Copy(value, 0, data, 12, value.Length);

            byte[] revData = new byte[0];
            bool Sequela = false;
            return ExeCommand(GetAddressByte(), 0x14, data, ref Sequela, ref revData);

        }

        /// <summary>
        /// 写数据(数据型，数据项)
        /// </summary>
        /// <param name="dataFlag">标识符</param>
        /// <param name="valueLen">数据长度(字节数)</param>
        /// <param name="value">写入数据</param>
        /// <returns></returns>
        public override bool WriteData(string dataFlag, int valueLen, string value)
        {
            FrameMean = "写数据";
            byte[] data = new byte[valueLen];
            if (valueLen <= 8)                       //如果只写小于等于8字节的，可以直接转换
            {
                value = value.PadLeft(2 * valueLen, '0');
                Array.Copy(BitConverter.GetBytes(Convert.ToInt64("0x" + value, 16)), data, valueLen);
            }
            else
                data = GetBytesArry(valueLen, value, true);      //转换数据
            return WriteData(dataFlag, data);

        }

        /// <summary>
        /// 写数据(数据型，数据项)
        /// </summary>
        /// <param name="dataFlag">标识符</param>
        /// <param name="len">数据长度((块中每项字节数不于8字节))</param>
        /// <param name="dot">小数位</param>
        /// <param name="value">写入数据</param>
        /// <returns></returns>
        public override bool WriteData(string dataFlag, int len, int dot, float value)
        {
            FrameMean = "写数据";
            byte[] data = new byte[len];
            string tmp = Convert.ToInt64(value * Math.Pow(10, dot)).ToString();
            byte[] bTmp = BitConverter.GetBytes(Convert.ToInt64("0x" + tmp, 16));
            Array.Copy(bTmp, 0, data, 0, len);
            return WriteData(dataFlag, data);

        }

        /// <summary>
        /// 写数据(字符型，数据块)
        /// </summary>
        /// <param name="dataFlag">标识符,两个字节</param>
        /// <param name="len">数据长度</param>
        /// <param name="value">写入数据</param>
        /// <returns></returns>
        public override bool WriteData(string dataFlag, int len, string[] value)
        {
            FrameMean = "写数据";
            byte[] data = new byte[len * value.Length];
            for (int i = 0; i < value.Length; i++)
            {
                byte[] tmp;
                if (len <= 8)                       //如果只写小于等于8字节的，可以直接转换
                {
                    string sTmp = value[i];
                    if (sTmp.Length > len * 2)
                        sTmp = sTmp.Substring(sTmp.Length - len * 2);
                    else if (sTmp.Length < len * 2)
                        sTmp = "".PadLeft(len * 2 - value.Length, '0') + sTmp;
                    tmp = BitConverter.GetBytes(Convert.ToInt64("0x" + sTmp, 16));
                }
                else
                    tmp = GetBytesArry(len, value[i], true);      //转换数据

                Array.Copy(tmp, 0, data, i * len, len);

            }
            if (protocolInfo.BlockAddAA)
            {
                Array.Resize(ref data, data.Length + 1);
                data[data.Length - 1] = 0xaa;
            }
            return WriteData(dataFlag, data);

        }

        /// <summary>
        /// 写数据(数据型，数据块)
        /// </summary>
        /// <param name="dataFlag">标识符,两个字节</param>
        /// <param name="len">数据长度</param>
        /// <param name="dot">小数位</param>
        /// <param name="str_Value">写入数据</param>
        /// <returns></returns>
        public override bool WriteData(string dataFlag, int len, int dot, float[] value)
        {
            FrameMean = "写数据";
            byte[] data = new byte[len * value.Length];
            for (int i = 0; i < value.Length; i++)
            {
                string tmp = Convert.ToInt64(value[i] * Math.Pow(10, dot)).ToString();
                byte[] bTmp = new byte[len];
                if (len <= 8)                       //如果只写小于等于8字节的，可以直接转换
                {
                    byte[] byt_MyValue = BitConverter.GetBytes(Convert.ToInt64("0x" + tmp, 16));
                    Array.Copy(byt_MyValue, bTmp, len);
                }
                else
                    bTmp = GetBytesArry(len, tmp, true);      //转换数据
                Array.Copy(bTmp, 0, data, i * len, len);
            }
            if (protocolInfo.BlockAddAA)
            {
                Array.Resize(ref data, data.Length + 1);
                data[data.Length - 1] = 0xaa;
            }
            return WriteData(dataFlag, data);

        }

        /// <summary>
        /// 清空需量
        /// </summary>
        /// <returns></returns>
        public override bool ClearDemand()
        {
            FrameMean = "清空需量";
            //a)功能：当前最大需量及发生时间数据清零
            //b)控制码：C=19H
            //c)数据域长度：L=08H  password (4) + UserID(4)
            byte[] data = new byte[8];

            Array.Copy(BitConverter.GetBytes(Convert.ToInt32("0x" + protocolInfo.ClearDemandPassword + protocolInfo.ClearDemandClass, 16)), data, 4);
            string userID = "00000000" + protocolInfo.UserID;
            Array.Copy(BitConverter.GetBytes(Convert.ToInt32("0x" + userID.Substring(userID.Length - 8), 16)), 0, data, 4, 4);
            bool sequela = false;
            byte[] revData = new byte[0];
            return ExeCommand(GetAddressByte(), 0x19, data, ref sequela, ref revData);
        }

        /// <summary>
        /// 清空需量
        /// </summary>
        /// <param name="endata">密文</param>
        /// <returns></returns>
        public override bool ClearDemand(string endata)
        {
            FrameMean = "清空需量";
            //a)功能：当前最大需量及发生时间数据清零
            //b)控制码：C=19H
            //c)数据域长度：L=08H  password (4) + UserID(4)
            byte[] data = new byte[28];

            Array.Copy(BitConverter.GetBytes(Convert.ToInt32("0x" + protocolInfo.ClearDemandPassword + protocolInfo.ClearDemandClass, 16)), data, 4);
            string str_UserID = "00000000" + protocolInfo.UserID;
            Array.Copy(BitConverter.GetBytes(Convert.ToInt32("0x" + str_UserID.Substring(str_UserID.Length - 8), 16)), 0, data, 4, 4);
            Array.Copy(GetBytesArry(20, endata, false), 0, data, 8, 20);

            bool sequela = false;
            byte[] revData = new byte[0];
            return ExeCommand(GetAddressByte(), 0x19, data, ref sequela, ref revData);
        }

        /// <summary>
        /// 清空电量
        /// </summary>
        ///  <param name="int_Type">类型 </param>
        /// <returns></returns>
        public override bool ClearEnergy()
        {
            FrameMean = "清空电量";
            //a)功能：清空电能表内电能量、最大需量及发生时间、冻结量、事件记录、负荷记录等数据
            //b)控制码：C=1AH
            //c)数据域长度：L=08H  password (4) + UserID(4)

            byte[] data = new byte[8];
            Array.Copy(BitConverter.GetBytes(Convert.ToInt32("0x" + protocolInfo.ClearDLPassword + protocolInfo.ClearDLClass, 16)), data, 4);
            string userID = "00000000" + protocolInfo.UserID;
            Array.Copy(BitConverter.GetBytes(Convert.ToInt32("0x" + userID.Substring(userID.Length - 8), 16)), 0, data, 4, 4);
            bool sequela = false;
            byte[] revData = new byte[0];
            return ExeCommand(GetAddressByte(), 0x1A, data, ref sequela, ref revData);
        }

        /// <summary>
        /// 清空电量
        /// </summary>
        ///  <param name="endata">密文 </param>
        /// <returns></returns>
        public override bool ClearEnergy(string endata)
        {
            FrameMean = "清空电量";
            //a)功能：清空电能表内电能量、最大需量及发生时间、冻结量、事件记录、负荷记录等数据
            //b)控制码：C=1AH
            //c)数据域长度：L=08H  password (4) + UserID(4) + 密文(20)
            //C151112010C 8  3346  45CB    4FAB8642DD8955D6CA57
            byte[] data = new byte[28];
            Array.Copy(BitConverter.GetBytes(Convert.ToInt32("0x" + protocolInfo.ClearDLPassword + protocolInfo.ClearDLClass, 16)), data, 4);
            string userID = "00000000" + protocolInfo.UserID;
            Array.Copy(BitConverter.GetBytes(Convert.ToInt32("0x" + userID.Substring(userID.Length - 8), 16)), 0, data, 4, 4);
            Array.Copy(GetBytesArry(20, endata, false), 0, data, 8, 20);

            bool sequela = false;
            byte[] revData = new byte[0];
            return ExeCommand(GetAddressByte(), 0x1A, data, ref sequela, ref revData);
        }

        /// <summary>
        /// 钱包初始化
        /// </summary>
        ///  <param name="endata">密文 </param>
        /// <returns></returns>
        public override bool InitPurse(string endata)
        {
            FrameMean = "钱包初始化";
            //a)功能：钱包初始化            
            //c)数据域长度：L=08H  DI (4) + UserID(4) + 密文(16)

            byte[] data = new byte[24];
            Array.Copy(BitConverter.GetBytes(Convert.ToInt32("0x" + "070103FF", 16)), data, 4);
            string userID = "00000000" + protocolInfo.UserID;
            Array.Copy(BitConverter.GetBytes(Convert.ToInt32("0x" + userID.Substring(userID.Length - 8), 16)), 0, data, 4, 4);
            Array.Copy(GetBytesArry(16, endata, false), 0, data, 8, 16);

            bool sequela = false;
            byte[] revData = new byte[0];
            return ExeCommand(GetAddressByte(), 0x03, data, ref sequela, ref revData);
        }

        /// <summary>
        /// 清空事件记录
        /// </summary>
        /// <param name="int_Type">操作类型</param>
        /// <param name="dataFlag">事件清零内容 事件总清零=FFFFFFFF   分项事件清零=DI3DI2DI1FF</param>
        /// <returns></returns>
        public override bool ClearEventLog(string dataFlag)
        {
            FrameMean = "清空事件记录";
            //a)功能：清空电能表内存储的全部或某类事件记录数据。
            //b)控制码：C=1BH
            //c)数据域长度：L=0CH
            //1）事件总清零 PAOP0OP1OP2O＋C0C1C2C3＋FFFFFFFF；
            //2）分项事件清零 PAOP0OP1OP2O＋C0C1C2C3＋事件记录数据标识（DI0用FF表示）

            byte[] data = new byte[12];
            Array.Copy(BitConverter.GetBytes(Convert.ToInt32("0x" + protocolInfo.ClearDLPassword + protocolInfo.ClearDLClass, 16)), data, 4);
            string tmp = "00000000" + protocolInfo.UserID;
            Array.Copy(BitConverter.GetBytes(Convert.ToInt32("0x" + tmp.Substring(tmp.Length - 8), 16)), 0, data, 4, 4);
            tmp = "00000000" + dataFlag;
            Array.Copy(BitConverter.GetBytes(Convert.ToInt32("0x" + tmp.Substring(tmp.Length - 8), 16)), 0, data, 8, 4);
            bool sequela = false;
            byte[] revData = new byte[0];
            return ExeCommand(GetAddressByte(), 0x1B, data, ref sequela, ref revData);
        }

        /// <summary>
        /// 清空需量
        /// </summary>
        /// <param name="endata">密文</param>
        /// <returns></returns>
        public override bool ClearEventLog(string dataFlag, string endata)
        {
            FrameMean = "清空需量";
            //a)功能：当前最大需量及发生时间数据清零
            //b)控制码：C=19H
            //c)数据域长度：L=08H  password (4) + UserID(4)
            byte[] data = new byte[28];

            Array.Copy(BitConverter.GetBytes(Convert.ToInt32("0x" + protocolInfo.ClearDemandPassword + protocolInfo.ClearDemandClass, 16)), data, 4);
            string str_UserID = "00000000" + protocolInfo.UserID;
            Array.Copy(BitConverter.GetBytes(Convert.ToInt32("0x" + str_UserID.Substring(str_UserID.Length - 8), 16)), 0, data, 4, 4);
            Array.Copy(GetBytesArry(20, endata, false), 0, data, 8, 20);

            bool sequela = false;
            byte[] revData = new byte[0];
            return ExeCommand(GetAddressByte(), 0x1B, data, ref sequela, ref revData);
        }

        /// <summary>
        /// 设置多功能脉冲端子输出脉冲类型
        /// </summary>
        /// <param name="int_Type">操作类型  </param>
        /// <param name="pulse">端子输出脉冲类型</param>
        /// <returns></returns>
        public override bool SetPulseCom(byte pulse)
        {
            FrameMean = "设置多功能脉冲端子输出脉冲类型";
            //a)功能：设置多功能端子输出信号类别 00时钟，01需量 02 时段投切
            //b)控制码：C=1DH
            //c)数据域长度：L=01H
            bool sequela = false;
            byte[] revData = new byte[0];
            byte[] data = new byte[1];
            data[0] = pulse;
            return ExeCommand(GetAddressByte(), 0x1D, data, ref sequela, ref revData);
        }

        /// <summary>
        /// 冻结命令
        /// </summary>
        /// <param name="int_Type">操作类型 0=广播冻结，1=普通冻结</param>
        /// <param name="dateHour">冻结时间，MMDDhhmm(月.日.时.分)数据域99DDhhmm表示以月为周期定时冻结，9999hhmm表示以日为周期定时冻结，999999mm表示以小时为周期定时冻结，99999999为瞬时冻结。</param>
        /// <returns></returns>
        public override bool FreezeCmd(string dateHour)
        {
            FrameMean = "冻结命令";
            //a)功能：冻结电能表数据，冻结内容见冻结数据标识编码表。
            //b)控制码：C=16H
            //c)数据域长度：L=04H
            //d)数据域：MMDDhhmm(月.日.时.分)
            string tmp = dateHour;
            if (dateHour.Length > 8)
                tmp = dateHour.Substring(dateHour.Length - 8);
            else if (dateHour.Length < 8)
            {
                if (dateHour.Length % 2 == 0)
                    tmp = tmp.PadLeft(8 - dateHour.Length, '9');
                else
                    tmp = tmp.PadLeft(8 - dateHour.Length, '0');
            }
            byte[] data = BitConverter.GetBytes(Convert.ToInt32("0x" + tmp, 16));

            bool sequela = false;
            byte[] revData = new byte[0];
            return ExeCommand(GetAddressByte(), 0x16, data, ref sequela, ref revData);

        }

        /// <summary>
        /// 更改波特率
        /// </summary>
        /// <param name="int_Type">操作类型 0=</param>
        /// <param name="setting">波特率</param>
        /// <returns></returns>
        public override bool ChangeSetting(string setting)
        {
            FrameMean = "更改波特率";
            //a)功能：更改电能表当前通信速率为其它标准速率。
            //b)控制码：C=17H
            //c)数据域长度：L=01H

            //19200bps  9600bps	4800bps	2400bps	1200bps	600bps

            string[] para = setting.Split(new char[] { ',' });
            byte[] data = new byte[1];
            data[0] = GetSettingCode6452007(Convert.ToInt16(para[0]));
            if (data[0] == 0) return false;

            byte[] revData = new byte[0];
            bool sequela = false;
            bool result = ExeCommand(GetAddressByte(), 0x17, data, ref sequela, ref revData);
            if (result)
            {
                if (revData[0] != data[0])
                    return false;
                else
                    return true;
            }
            else
                return false;

        }

        /// <summary>
        /// 更改密码
        /// </summary>
        /// <param name="int_Type">操作类型 0=</param>
        /// <param name="clas">密码等级，即修改几级的密码</param>
        /// <param name="oldPws">旧密码,如果是更高等级修改本等级密码则需加更高等级，原密码则不包含等级</param>
        /// <param name="newPsw">新密码,不包含等级</param>
        /// <returns></returns>
        public override bool ChangePassword(int clas, string oldPws, string newPsw)
        {
            FrameMean = "更改密码";
            //a)功能：修改从站密码设置
            //b)控制码：C=18H
            //c)数据域长度：L=0CH
            //d)数据域：DIODI1DI2DI3＋PAOP0OP1OP2O＋PANP0NP1NP2N

            if (clas < 0 || clas > 9) return false;

            string[] pwdID = new string[]{"04000C01","04000C02","04000C03","04000C04","04000C05",
                                                  "04000C06","04000C07","04000C08","04000C09","04000C0A"};
            byte[] data = new byte[12];
            byte[] tmp = BitConverter.GetBytes(Convert.ToInt32("0x" + pwdID[clas], 16));
            Array.Copy(tmp, data, 4);

            if (oldPws.Length == 8)                 //包含等级，是当前修改等级的原密码，或更高等级密码
            {
                tmp = BitConverter.GetBytes(Convert.ToInt32("0x" + oldPws, 16));
            }
            else                                        //不包含等级，是当前修改等级的原密码
            {
                if (oldPws.Length > 6)
                    tmp = BitConverter.GetBytes(Convert.ToInt32("0x" + oldPws.Substring(0, 6) + "0" + clas.ToString(), 16));
                else
                    tmp = BitConverter.GetBytes(Convert.ToInt32("0x" + oldPws.PadLeft(6 - oldPws.Length, '0') + "0" + clas.ToString(), 16));
            }
            Array.Copy(tmp, 0, data, 4, 4);

            if (newPsw.Length == 8)
            {
                tmp = BitConverter.GetBytes(Convert.ToInt32("0x" + newPsw, 16));
            }
            else
            {
                if (newPsw.Length > 6)
                    tmp = BitConverter.GetBytes(Convert.ToInt32("0x" + newPsw.Substring(0, 6) + "0" + clas.ToString(), 16));
                else
                    tmp = BitConverter.GetBytes(Convert.ToInt32("0x" + newPsw.PadLeft(6 - newPsw.Length, '0') + "0" + clas.ToString(), 16));
            }
            Array.Copy(tmp, 0, data, 8, 4);

            bool sequela = false;
            byte[] revData = new byte[0];
            bool result = ExeCommand(GetAddressByte(), 0x18, data, ref sequela, ref revData);
            if (result)
            {
                if (tmp.Length == revData.Length)
                {
                    for (int i = 0; i < tmp.Length; i++)
                    {
                        if (tmp[i] != revData[i])
                            return false;
                    }
                    return true;
                }
                else
                    return false;
            }
            else
                return false;

        }
        #endregion

        /// <summary>
        /// DLT645参变量分类,1类参数，2类参数
        /// </summary>
        /// <param name="dataId">DLT645数据标识</param>
        /// <returns>1类,2类,3类</returns>
        private int CheckStrIDType(string dataId)
        {
            dataId = dataId.ToUpper();
            if (protocolInfo.HaveProgrammingkey)
            {
                return 4;//有编程键
            }

            int tp;
            if (DicIDType.ContainsKey(dataId))
            {
                tp = 1;
            }
            else if (dataId.IndexOf("040501", 0) >= 0)
            {
                tp = 1;
            }
            else if (dataId.IndexOf("040502", 0) >= 0)
            {
                tp = 1;
            }
            else if (dataId == "04001301")
            {
                tp = 3;
            }
            else
            {
                tp = 2;
            }
            return tp;
        }
        /// <summary>
        /// 参变量分类字典。key 协议标识 value 1类2类3类
        /// </summary>
        private static readonly Dictionary<string, int> DicIDType = new Dictionary<string, int> {
        {"04000108",1},{"04000109",1},{"04000306",1},{"04000307",1},
        {"04000402",1},{"0400040E",1},{"04001001",1},{"04001002",1},
        {"040501XX",1},{"040502XX",1},{"040604FF",1},{"040605FF",1}};


        #region --------------------私有函数---------------------------------

        /// <summary>
        /// 波特率特征字
        /// </summary>
        /// <param name="btl">波特率</param>
        /// <returns></returns>
        private byte GetSettingCode6452007(int btl)
        {
            //19200bps	9600bps	4800bps	2400bps	1200bps	600bps

            if (btl == 600)
                return 2;
            else if (btl == 1200)
                return 4;
            else if (btl == 2400)
                return 8;
            else if (btl == 4800)
                return 16;
            else if (btl == 9600)
                return 32;
            else if (btl == 9600)
                return 64;
            else
                return 0;
        }

        /// <summary>
        /// 强制地址
        /// </summary>
        /// <param name="address">返回地址</param>
        /// <returns></returns>
        private bool CptReadAddress(ref string address)
        {
            FrameMean = "探测地址";

            //a)功能：请求读电能表通信地址，仅支持点对点通信。
            //b)地址域：AA…AAH
            //c)控制码：C=13H
            //d)数据域长度：L=00H
            byte[] uAddr = new byte[] { 0xAA, 0xAA, 0xAA, 0xAA, 0xAA, 0xAA };
            byte[] revAddr = new byte[0];
            byte[] revData = new byte[0];
            bool sequela = false;
            bool result = ExeCommand(uAddr, 0x13, new byte[0], ref sequela, ref revData, ref revAddr);
            if (result)
            {
                if (Array.Equals(uAddr, revAddr))      //返回帧的地址域是否跟通用地址一样，一样则取数据域，不一样则就实际地址
                {
                    Array.Reverse(revData);
                    address = BitConverter.ToString(revData).Replace("-", "");
                    return true;
                }
                else     //返回帧的地址域不是广播地址,则是电能表实际地址
                {
                    Array.Reverse(revAddr);
                    address = BitConverter.ToString(revAddr).Replace("-", "");
                    return true;
                }
            }
            return result;

        }

        /// <summary>
        /// 探测地址
        /// </summary>
        /// <param name="address">返回地址</param>
        /// <returns></returns>
        private bool DetectAddress(ref string address)
        {
            FrameMean = "探测地址";
            //04 00	04 01
            byte[] uAddr = new byte[] { 0xAA, 0xAA, 0xAA, 0xAA, 0xAA, 0xAA };
            byte[][] data = new byte[][] { new byte[] { 0x01,0x04,0x00,0x04 },          //设备号
                                                   new byte[] {0x01,0x04,0x00,0x04}  };        //表号

            for (int i = 0; i < data.Length; i++)
            {
                byte[] revData = new byte[0];
                byte[] addr = new byte[6];
                bool sequela = false;
                bool result = ExeCommand(uAddr, 0x11, data[i], ref sequela, ref revData, ref addr);
                if (result)
                {
                    if (Array.Equals(uAddr, addr))      //返帧的地址与通用地址一样,用读回的数据作为地址下发
                    {
                        if (revData.Length >= 10)
                        {
                            Array.Copy(revData, 4, addr, 0, 6);       //取出数据域中的数据作为地址，并下发指令，验证是否是地址
                            result = ExeCommand(addr, 0x11, data[i], ref sequela, ref revData);
                            if (result)
                            {
                                address = BitConverter.ToString(addr).Replace("-", "");
                                return true;
                            }
                        }
                    }
                    else       //如果跟通用地址不一样则就是电能表返回的实际地址
                    {
                        Array.Reverse(addr);
                        address = BitConverter.ToString(addr).Replace("-", "");
                        return true;
                    }
                }
            }
            return false;

        }

        /// <summary>
        /// 换算电量标识符
        /// </summary>
        /// <param name="block">块操作</param>
        /// <param name="pDirect">功率类型，X 0=P+ 1=P- 2=Q+ 3=Q- 4=Q1 5=Q2 6=Q3 7=Q4
        /// 定义：0=组合 1=P+ 2=P- 3=Q+ 4=Q- 5=Q1 6=Q2 7=Q3 8=Q4</param>
        /// <param name="tariffType">费率类型，0=总，1=尖, 2=峰，3=平，4=谷</param>
        /// <param name="dot">2/4</param>
        /// <returns></returns>
        private string GetEnergyID(bool block, int pDirect, int tariffType, int dot = 2)
        {
            if (dot > 2)
            {
                string dire;
                if (pDirect == 0)
                {
                    dire = "8";
                }
                else
                {
                    dire = (pDirect - 1).ToString();
                }

                if (block)
                    return "006" + dire + "ff00";
                else
                    return "006" + dire + "0" + tariffType.ToString() + "00";
            }
            else
            {
                if (block)
                    return "000" + Convert.ToString(pDirect) + "ff00";
                else
                    return "000" + Convert.ToString(pDirect) + "0" + tariffType.ToString() + "00";
            }
        }

        /// <param name="type">读取类型0=块读，1=分项读</param>
        /// <param name="dLType">读取类型 1=剩余电量，2=透支电量，3=(上1次)定时冻结正向有功电能,4=(上1次)日冻结正向有功电能
        private string GetFreezeID(int dLType, int times)
        {
            //0 为块  1 为分项
            string ID = "";
            if (dLType == 3)//定时冻结
                ID = "050001" + times.ToString().PadLeft(2, '0');
            else if (dLType == 4)//日冻结
                ID = "050601" + times.ToString().PadLeft(2, '0');
            else if (dLType == 5)//整点冻结
                ID = "050401" + times.ToString().PadLeft(2, '0');
            else if (dLType == 6)//瞬时冻结
                ID = "050101" + times.ToString().PadLeft(2, '0');
            else if (dLType == 7)//两套时区表切换
                ID = "050201" + times.ToString().PadLeft(2, '0');
            else if (dLType == 8)//两套日时段表切换
                ID = "050301" + times.ToString().PadLeft(2, '0');
            else if (dLType == 9)//两套费率电价切换
                ID = "050501" + times.ToString().PadLeft(2, '0');
            else if (dLType == 10)//两套阶梯切换
                ID = "050701" + times.ToString().PadLeft(2, '0');
            return ID;

        }

        /// <summary>
        /// 换算电量标识符
        /// </summary>
        /// <param name="block">块操作</param>
        /// <param name="pDirect">功率类型，0=组合有功 1=P+ 2=P- 3=Q+ 4=Q- 5=Q1 6=Q2 7=Q3 8=Q4</param>
        /// <param name="tariffType">费率类型，0=总，1=尖, 2=峰，3=平，4=谷</param>
        /// <param name="freezeTimes">结算次数，为0时表示读取当前</param>
        /// <returns></returns>
        private string GetEnergyIDs(bool block, int pDirect, int freezeTimes)
        {
            if (block)
                return "000" + Convert.ToString(pDirect) + "ff" + Convert.ToString(freezeTimes, 16).PadLeft(2, '0');
            else
                return "000" + Convert.ToString(pDirect) + "00" + Convert.ToString(freezeTimes, 16).PadLeft(2, '0');

            //0000FF00-(当前)组合有功电能数据块
            //0000FF01-(上1结算日)组合有功电能数据块
            //0000FF02-(上2结算日)组合有功电能数据块
            //0000FF0C-(上12结算日)组合有功电能数据块
            //0001FF00-(当前)正向有功电能数据块
            //0001FF01-(上1结算日)正向有功电能数据块
            //0001FF02-(上2结算日)正向有功电能数据块
            //0001FF0C-(上12结算日)正向有功电能数据块
        }

        /// <summary>
        /// 换算需量标识符
        /// </summary>
        /// <param name="block">块操作</param>
        /// <param name="pDirect">功率类型，1=P+ 2=P- 3=Q+ 4=Q- 5=Q1 6=Q2 7=Q3 8=Q4</param>
        /// <param name="tariffType">费率类型，0=总，1=尖,2=峰，3=平，4=谷</param>
        /// <returns></returns>
        private string GetDemandID(bool block, int pDirect, int tariffType)
        {
            if (block)
                return "010" + Convert.ToString(pDirect) + "ff00";
            else
                return "010" + Convert.ToString(pDirect) + "0" + tariffType.ToString() + "00";
        }

        /// <summary>
        /// 把任意16进制字符串转换为指定长度的byte数组
        /// </summary>
        /// <param name="len">数组长度</param>
        /// <param name="value">要转换的字符串</param>
        /// <param name="reverse">true翻转，false不翻转</param>
        /// <returns></returns>
        private byte[] GetBytesArry(int len, string value, bool reverse)
        {
            byte[] data = new byte[len];
            string tmp;

            if (value.Length > len * 2)
                tmp = value.Substring(value.Length - len * 2);
            else if (value.Length < len * 2)
                tmp = value.PadLeft(len * 2 - value.Length, '0');
            else
                tmp = value;

            if (reverse)
            {
                for (int i = 0; i < len; i++)
                    data[len - 1 - i] = Convert.ToByte(tmp.Substring(i * 2, 2), 16);
            }
            else
            {
                for (int i = 0; i < len; i++)
                    data[i] = Convert.ToByte(tmp.Substring(i * 2, 2), 16);
            }
            return data;
        }

        /// <summary>
        /// 星期代码转换
        /// </summary>
        /// <param name="date">日期</param>
        /// <param name="sundayIndex">星期日序号</param>
        /// <returns></returns>
        private int GetWeekday(string date, int sundayIndex)
        {
            DateTime dateTime = new DateTime(Convert.ToInt16(date.Substring(2, 2)),
                                                 Convert.ToInt16(date.Substring(4, 2)),
                                                 Convert.ToInt16(date.Substring(6, 2)));

            int sysWeekday = (int)dateTime.DayOfWeek;
            if (sundayIndex == 1)
                return sysWeekday + 1;
            else if (sundayIndex == 7)
            {
                if (sysWeekday == 0)
                    return 7;
                else
                    return sysWeekday;
            }
            else
                return sysWeekday;
        }
        #endregion



        #region 物联网表--多芯表

        //发送数据，助阵
        public byte[] IOTMete_Framing(byte controlType, byte[] data)
        {
            List<byte> fram = new List<byte>
            {
                //帧头
                0x7E,
                0x7E,
                0x7E,
                0x5A
            };

            byte len = (byte)(0x06 + data.Length);
            //长度
            fram.Add(len);
            //功能码
            fram.Add(controlType);
            if (data.Length > 0)
            {
                //数据区
                fram.AddRange(data);
            }



            //校验位
            byte cs = 0;
            for (int i = 0; i < fram.Count; i++)
            {
                cs += fram[i];
            }
            fram.Add(cs);

            //帧尾
            fram.Add(0x7E);
            fram.Add(0xA5);
            return fram.ToArray();
        }

        /// <summary>
        /// 解析帧
        /// </summary>
        /// <param name="controlType"></param>
        /// <returns></returns>
        public byte IOTMete_ParseFrame(byte[] recvData)
        {
            string str = "";
            for (int i = 0; i < recvData.Length; i++)
            {
                str += recvData[i].ToString("x2");
            }
            str = str.ToUpper();
            if (recvData.Length < 9)
            {
                return 0x01;
            }
            if (str.IndexOf("7E7E7E5A") == -1)
            {
                return 0x01;
            }
            byte cs = 0x00;
            //判断校验位是否正确
            for (int i = 0; i < recvData.Length - 3; i++)
            {
                cs += recvData[i];
            }
            if (cs != recvData[recvData.Length - 3])
            {
                return 0x01;
            }
            if (recvData[recvData.Length - 4] == 0x00)
            {
                Console.WriteLine($"接收数据{str}--成功");
            }
            else
            {
                Console.WriteLine($"接收数据{str}--失败");
            }
            return recvData[recvData.Length - 4];
        }

        /// <summary>
        ///  物联网表--复位
        /// </summary>
        /// <returns></returns>
        public override bool IOTMete_Reset()
        {
            byte control = 0x00;
            byte[] data = new byte[0];
            byte[] recvData = new byte[0];
            byte[] sendData = IOTMete_Framing(control, data);
            ExeCommand(sendData, ref recvData);
            return IOTMete_ParseFrame(recvData) == 0x00;
        }
        /// <summary>
        ///物联网表-- 连接电表
        /// </summary>
        /// <param name="Address">表地址</param>
        /// <returns></returns>
        public override bool IOTMete_Connect(string Address, string ping)
        {
            byte control = 0x01;
            byte[] data;
            if (ping != "") //说明有密码比如123456
            {

                //把每一位补成俩位，加上48后在转成16进制--就是ascii
                string possword = "";
                for (int i = 0; i < ping.Length; i++)
                {
                    possword += (int.Parse(ping[i].ToString()) + 48).ToString("x");
                }
                data = GetBytesArry(possword.Length / 2 + 6, possword + Address, true);
            }
            else
            {
                data = GetBytesArry(6, Address, true);
            }
            byte[] recvData = new byte[0];
            byte[] sendData = IOTMete_Framing(control, data);
            ExeCommand(sendData, ref recvData);
            return IOTMete_ParseFrame(recvData) == 0x00;
        }
        /// <summary>
        ///  物联网表--预处理--需要等待35秒以上
        /// </summary>
        /// <returns></returns>
        public override bool IOTMete_Pretreatment()
        {
            byte control = 0x07;
            byte[] data = new byte[0];
            byte[] recvData = new byte[0];
            byte[] sendData = IOTMete_Framing(control, data);
            ExeCommand(sendData, ref recvData);
            return IOTMete_ParseFrame(recvData) == 0x00;
        }

        /// <summary>
        ///  物联网表--预处理-状态查询
        /// </summary>
        /// <returns></returns>
        public override bool IOTMete_PretreatmentSelect()
        {
            byte control = 0x08;
            byte[] data = new byte[0];
            byte[] recvData = new byte[0];
            byte[] sendData = IOTMete_Framing(control, data);
            ExeCommand(sendData, ref recvData);
            return IOTMete_ParseFrame(recvData) == 0x00;
        }
        /// <summary>
        ///  物联网表--切换到光脉冲
        /// </summary>
        /// <param name="pulseType">脉冲类型</param>
        /// <returns></returns>
        public override bool IOTMete_SetLightPulse(byte pulseType)
        {

            byte control = 0x09;
            byte[] data = new byte[2];
            data[0] = 0x00;
            data[01] = pulseType;
            byte[] recvData = new byte[0];
            byte[] sendData = IOTMete_Framing(control, data);
            ExeCommand(sendData, ref recvData);
            return IOTMete_ParseFrame(recvData) == 0x00;
        }
        /// <summary>
        ///   切换电表到检定模式(待测电能表蓝牙模式切换)：0x02--参数：脉冲类型，通道数量，发射频率，频段
        /// </summary>
        /// <param name="pulseType">脉冲类型</param>
        /// <param name="powerGear">功率挡位</param>
        /// <param name="powerGear">频段00全频段。01带内频段，02带外频段</param>
        /// <param name="powerGear">通道数量</param>
        /// <returns></returns>
        public override byte IOTMete_SetMeterTestModel(byte pulseType, byte powerGear, byte frequencyBand, byte passagewayCount)
        {
            byte control = 0x02;
            byte[] data = new byte[6];
            data[0] = 0x01;
            data[1] = pulseType;
            data[2] = powerGear;
            data[3] = frequencyBand;
            data[4] = 0x01;
            data[5] = passagewayCount;
            byte[] recvData = new byte[0];
            byte[] sendData = IOTMete_Framing(control, data);
            ExeCommand(sendData, ref recvData);
            return IOTMete_ParseFrame(recvData);
        }

        /// <summary>
        ///  切换转换器到检定模式(转换器蓝牙模式切换)---脉冲类型，发射功率，通信模式
        /// </summary>
        /// <param name="pulseType">脉冲类型</param>
        /// <param name="powerGear">功率挡位</param>
        /// <param name="model">模式--00普通检定模式--10脉冲跟随模式</param>
        /// <returns></returns>
        public override bool IOTMete_SetConverterModel(byte pulseType, byte powerGear, byte model)
        {
            byte control = 0x03;
            byte[] data = new byte[4];
            data[0] = 0x01;
            data[1] = pulseType;
            data[2] = powerGear;
            data[3] = model;
            byte[] recvData = new byte[0];
            byte[] sendData = IOTMete_Framing(control, data);
            ExeCommand(sendData, ref recvData);
            return IOTMete_ParseFrame(recvData) == 0x00;
        }

        #endregion
    }
}
