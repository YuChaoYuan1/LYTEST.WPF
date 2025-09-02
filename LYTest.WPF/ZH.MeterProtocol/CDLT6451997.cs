using LYTest.Core;
using LYTest.MeterProtocol.Protocols;
using LYTest.MeterProtocol.Protocols.DLT698.Struct;
using System;
using System.Collections.Generic;

namespace LYTest.MeterProtocol
{
    /// <summary>
    /// DL/T645-1997通信协议
    /// </summary>
    public class CDLT6451997 : CDLT645
    {

        #region -------------------接口成员-----------------------------

        /// <summary>
        /// 通信测试
        /// </summary>
        /// <remarks>
        /// 测试类型，0=按配置文件测试,1=测试C032, 2=测试C033，3=测试C034, 4=测试901f 其他=测试9010 
        /// </remarks>
        /// <returns></returns>
        public override bool ComTest()
        {

            byte[] data = new byte[2];

            switch (CommTestType)
            {
                case 1:
                    data[0] = 0x32;
                    data[1] = 0xc0;
                    break;
                case 2:
                    data[0] = 0x33;
                    data[1] = 0xc0;
                    break;
                case 3:
                    data[0] = 0x34;
                    data[1] = 0xc0;
                    break;
                case 4:
                    data[0] = 0x1f;
                    data[1] = 0x90;
                    break;
                default:
                    data[0] = 0x10;
                    data[1] = 0x90;
                    break;
            }
            byte[] revData = new byte[0];
            bool sequela = false;
            return ExeCommand(GetAddressByte(), 0x01, data, ref sequela, ref revData);

        }

        /// <summary>
        /// 广播校时
        /// </summary>
        /// <param name="str_DateTime">日期时间</param>
        /// <returns></returns>
        public override bool BroadcastTime(DateTime broadCaseTime)
        {
            string dateTime = broadCaseTime.ToString("yyMMddHHmmss");

            byte[] addr = new byte[] { 0x99, 0x99, 0x99, 0x99, 0x99, 0x99 };
            byte[] data = new byte[6];
            byte[] tmp = BitConverter.GetBytes(Convert.ToInt64("0x" + dateTime, 16));
            Array.Copy(tmp, data, 6);

            for (int i = 0; i < 6; i++)
                data[5 - i] = Convert.ToByte(dateTime.Substring(i * 2, 2), 16);

            return ExeCommand(addr, 0x08, data);

        }

        /// <summary>
        /// 读取电量(分费率读取)
        /// </summary>
        /// <param name="energyType">功率类型，</param>
        /// <param name="tariffType">费率类型，</param>
        /// <returns>返回电量</returns>
        public override float ReadEnergy(byte energyType, byte tariffType)
        {
            float value = -1F; //电量值
            int pDirect = energyType;

            if (pDirect < 0 || pDirect > 7) return value;
            if (tariffType < 0 || tariffType > 63) return value;


            bool isBlock = false;
            if (ReadEnergyType == 1) isBlock = true;

            int index = protocolInfo.TariffOrderType.IndexOf(tariffType.ToString());      //按设置的费率排序取出电量

            string dataFlag = GetEnergyID(isBlock, pDirect, index);      //取出电量的标识符
            if (isBlock)
            {
                float[] sng_EnergyArry = ReadDataBlock( dataFlag, 4, 2);

                if (sng_EnergyArry.Length > 0)
                {
                    if (tariffType == 0)        //总电量
                    {
                        if (sng_EnergyArry.Length > 0)      //解电量块有数据
                        {
                            value = sng_EnergyArry[0];
                        }
                    }
                    else                                //尖峰平谷电量
                    {
                        if (index < sng_EnergyArry.Length)
                        {
                            Array.Resize(ref sng_EnergyArry, 5);
                            value = sng_EnergyArry[index];
                        }
                    }
                }
            }
            else
                return ReadData( dataFlag, 4, 2);
            return value;
        }
        public override FloatE ReadEnergyR(byte energyType, byte tariffType)
        {
            float value = ReadEnergyR(energyType, tariffType);
            return Convert.ToDecimal(value);
        }
        public override float ReadEnergyExtension(byte energyType, byte tariffType, int DecimalDigits)
        {
            return ReadEnergy(energyType, tariffType);
        }
        public override FloatE ReadEnergyExtensionR(byte energyType, byte tariffType, int DecimalDigits)
        {
            float value = ReadEnergyR(energyType, tariffType);
            return Convert.ToDecimal(value);
        }

        /// <summary>
        /// 读取电量(所有费率读取)
        /// </summary>
        /// <param name="int_Type">读取类型读取类型 0=按配置文件测试, 1=块读，2=分项读</param>
        /// <param name="ept_PDirect">功率类型，</param>
        /// <param name="sng_Energy">返回电量</param>
        /// <returns></returns>
        /// 
        public override float[] ReadEnergy(byte energyType)
        {
            float[] energy = new float[0];
            int pDirect = energyType;//(int)ept_PDirect;

            if (pDirect < 0 || pDirect > 7) return energy;

            bool isBlock = false;
            if (ReadEnergyType == 1) isBlock = true;

            if (isBlock)
            {
                string dataFlag = GetEnergyID(isBlock, pDirect, 0);      //取出电量的标识符
                float[] sng_EnergyArry = ReadDataBlock( dataFlag, 4, 2);
                if (sng_EnergyArry.Length > 0)
                {
                    if (sng_EnergyArry.Length > 0)
                    {
                        Array.Resize(ref energy, 5);
                        Array.Resize(ref sng_EnergyArry, 5);
                        energy[0] = sng_EnergyArry[0];
                        for (int i = 1; i < sng_EnergyArry.Length; i++)
                        {
                            int int_Index = Convert.ToInt16(protocolInfo.TariffOrderType.Substring(i - 1, 1));      //按设置的费率排序取出电量
                            energy[int_Index] = sng_EnergyArry[i];
                        }
                    }
                }
            }
            else
            {
                Array.Resize(ref energy, 5);

                string dataFlag = GetEnergyID(isBlock, pDirect, 0);      //取出电量总的标识符
                energy[0] = ReadData( dataFlag, 4, 2);
                if (energy[0] != -1)
                {
                    for (int i = 1; i < 5; i++)
                    {
                        int int_Index = protocolInfo.TariffOrderType.IndexOf(i.ToString()) + 1;      //按设置的费率排序取出电量

                        dataFlag = GetEnergyID(isBlock, pDirect, int_Index);      //取出电量总的标识符
                        energy[i] = ReadData( dataFlag, 4, 2);
                    }
                }

                return energy;

            }
            return energy;
        }

        /// <summary>
        /// 读取需量(分费率读取)
        /// </summary>
        /// <param name="int_Type">读取类型0=按配置文件测试,1=块读，2=分项读</param>
        /// <param name="ept_PDirect">功率类型，</param>
        /// <param name="ett_TariffType">费率类型，</param>
        /// <param name="sng_Demand">返回需量</param>
        /// <returns></returns>
        public override float ReadDemand(byte energyType, byte tariffType)
        {
            float sng_Demand = -1F;
            int int_PDirect = energyType;//(int)ept_PDirect;

            if (int_PDirect < 0 || int_PDirect > 7)
            {
                return sng_Demand;
            }
            if (tariffType < 0 || tariffType > 63)
            {
                //m_str_LostMessage = "读取需量指定费率类型超出范围[0-4]";
                return sng_Demand;
            }

            bool isBlock = false;
            if (ReadDemandType == 1) isBlock = true;

            int int_Index = protocolInfo.TariffOrderType.IndexOf(tariffType.ToString()) + 1;      //按设置的费率排序取出需量

            string dataFlag = GetDemandID(isBlock, int_PDirect, int_Index);      //取出需量的标识符
            if (isBlock)
            {
                float[] sng_DemandArry = ReadDataBlock( dataFlag, 3, 4);

                if (sng_DemandArry.Length > 0)
                {
                    if (tariffType == 0)        //总需量
                    {
                        if (sng_DemandArry.Length > 0)      //解需量块有数据
                        {
                            sng_Demand = sng_DemandArry[0];
                        }
                    }
                    else                                //尖峰平谷需量
                    {
                        if (int_Index < sng_DemandArry.Length)
                        {
                            sng_Demand = sng_DemandArry[int_Index];
                        }
                    }
                }
            }
            else
                return ReadData( dataFlag, 3, 4);
            return sng_Demand;
        }

        /// <summary>
        /// 读取需量(所有费率读取)
        /// </summary>
        /// <param name="int_Type">读取类型0=按配置文件测试,1=块读，2=分项读</param>
        /// <param name="ept_PDirect">功率类型，0=P+ 1=P- 2=Q+ 3=Q- 4=Q1 5=Q2 6=Q3 7=Q4</param>
        /// <param name="sng_Demand">返回需量</param>
        /// <returns></returns>
        public override float[] ReadDemand(byte energyType)
        {
            float[] sng_Demand = new float[0];
            int int_PDirect = energyType;//(int)ept_PDirect;

            if (int_PDirect < 0 || int_PDirect > 7)
            {
                return sng_Demand;
            }
            bool bln_Block = false;
            if (ReadDemandType == 1) bln_Block = true;

            if (bln_Block)
            {
                string str_ID = GetDemandID(bln_Block, int_PDirect, 0);      //取出需量的标识符
                float[] sng_DemandArry = ReadDataBlock( str_ID, 4, 2);
                if (sng_DemandArry.Length > 0)
                {
                    Array.Resize(ref sng_Demand, 5);
                    sng_Demand[0] = sng_DemandArry[0];
                    for (int i = 1; i < sng_DemandArry.Length; i++)
                    {
                        int int_Index = Convert.ToInt16(protocolInfo.TariffOrderType.Substring(i - 1, 1));      //按设置的费率排序取出需量
                        sng_Demand[int_Index] = sng_DemandArry[i];
                    }
                }
            }
            else
            {
                Array.Resize(ref sng_Demand, 5);
                string str_ID = GetDemandID(bln_Block, int_PDirect, 0);      //取出需量总的标识符
                sng_Demand[0] = ReadData( str_ID, 4, 2);
                if (sng_Demand[0] != -1)
                {
                    for (int i = 1; i < 5; i++)
                    {
                        int int_Index = protocolInfo.TariffOrderType.IndexOf(i.ToString()) + 1;      //按设置的费率排序取出需量

                        str_ID = GetDemandID(bln_Block, int_PDirect, int_Index);      //取出需量费率的标识符
                        sng_Demand[i] = ReadData( str_ID, 4, 2);
                    }
                }


            }
            return sng_Demand;
        }

        /// <summary>
        /// 读日期时间
        /// </summary>
        /// <param name="int_Type">读取类型 0=按配置文件测试, 1=分项操作,2=块操作C01f，3=块操作C012</param>
        /// <param name="str_DateTime">返回时间,如果读取失败则返回1900年1月1日</param>
        /// <returns></returns>
        public override DateTime ReadDateTime()
        {
            string dateTime = "1900/01/01 00:00:00";
            bool bln_Block = true;
            if (ReadDateTimeType == 1) bln_Block = false;

            if (bln_Block)
            {
                byte[] dataFlag = new byte[2];
                dataFlag[1] = 0xC0;
                if (ReadDateTimeType == 3)
                    dataFlag[0] = 0x12;
                else
                    dataFlag[0] = 0x1f;

                byte[] revData = new byte[0];
                bool sequela = false;
                bool result = ExeCommand(GetAddressByte(), 0x01, dataFlag, ref sequela, ref revData);
                if (result)
                {
                    if (revData.Length >= 6 + 2)        //大于等于6，6字节可能不包含周， +2是标识符2字节
                    {
                        if (revData[0] == dataFlag[0] && revData[1] == dataFlag[1])
                        {
                            Array.Reverse(revData);
                            string str_Tmp = BitConverter.ToString(revData, 0, revData.Length - 2).Replace("-", "");
                            str_Tmp = str_Tmp.TrimStart(new char[] { 'A' });        //去掉AA
                            str_Tmp = str_Tmp.TrimEnd(new char[] { 'A' });
                            str_Tmp = str_Tmp.Substring(6, 6) + str_Tmp.Substring(0, 6);
                            string dateFormat = protocolInfo.DateTimeFormat;
                            if (str_Tmp.Substring(0, 2) != "20") dateFormat = dateFormat.Replace("YYYY", "YY");
                            string[] str_Para = new string[] { "YY", "MM", "DD", "HH", "FF", "SS" };
                            for (int i = 0; i < 6; i++)
                            {
                                int int_Index = dateFormat.IndexOf(str_Para[i]);
                                str_Para[i] = str_Tmp.Substring(int_Index, 2);
                            }
                            dateTime = string.Format("{0}-{1}-{2} {3}:{4}:{5}"
                                  , str_Para[0]
                                  , str_Para[1]
                                  , str_Para[2]
                                  , str_Para[3]
                                  , str_Para[4]
                                  , str_Para[5]
                                  );
                        }
                    }
                }
            }
            else
            {
                ReadDateTimeSubItem(ref dateTime);
            }
            if (IsDate(dateTime))
                return DateTime.Parse(dateTime);
            else
                return DateTime.MinValue;
        }

        /// <summary>
        /// 检测字符串是否为日期类型
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsDate(string str)
        {
            if (DateTime.TryParse(str, out _))
                return true;
            else
                return false;
        }

        /// <summary>
        /// 读地址
        /// </summary>
        /// <returns></returns>
        public override string ReadAddress()
        {
            string addr = string.Empty;
            bool r = CptReadAddress(ref addr);
            if (!r)
                DetectAddress(ref addr);  //再用探测指令，即用通用地址AA读取
            return addr;
        }

        /// <summary>
        /// 读取时段
        /// </summary>
        /// <returns></returns>
        public override string[] ReadPeriodTime()
        {
            string[] str_PTime = new string[0];
            float sng_PCount = 0;
            bool bln_Result = false;
            if (ReadPeriodTimeType == 2 || ReadPeriodTimeType == 3)
                sng_PCount = ReadData("c312", 1, 0);     //读取时段数


            if (ReadPeriodTimeType == 1 || ReadPeriodTimeType == 3)
            {
                string[] str_Data = ReadDataBlock1("c33f", 3);     //读取时段数
                if (ReadPeriodTimeType == 1)
                    sng_PCount = str_Data.Length;

                if (bln_Result)
                {
                    if (str_Data.Length >= sng_PCount)
                    {
                        Array.Resize(ref str_PTime, Convert.ToInt16(sng_PCount));
                        for (int i = 0; i < Convert.ToInt16(sng_PCount); i++)
                            str_PTime[i] = str_Data[i];
                    }
                    else
                    {
                        Array.Resize(ref str_PTime, Convert.ToInt16(str_Data.Length));
                        for (int i = 0; i < str_Data.Length; i++)
                            str_PTime[i] = str_Data[i];
                    }
                }
            }
            else
            {
                string[] str_Tmp = new string[Convert.ToInt16(sng_PCount)];
                for (int i = 0; i < Convert.ToInt16(sng_PCount); i++)
                {      //读取每个时段
                    str_Tmp[i] = ReadData("C3" + Convert.ToString(31 + i), 3);
                }
                if (bln_Result) str_PTime = str_Tmp;
            }
            return str_PTime;
        }


        public override string ReadData(string str_ID, int int_Len)
        {
            string value = string.Empty;
            byte[] byt_ID = BitConverter.GetBytes(Convert.ToInt16("0x" + str_ID, 16));
            byte[] byt_MyRevData = new byte[0];
            bool bln_Sequela = false;
            bool r = ExeCommand(GetAddressByte(), 0x01, byt_ID, ref bln_Sequela, ref byt_MyRevData);
            if (r)
            {
                if (byt_MyRevData[0] == byt_ID[0] && byt_MyRevData[1] == byt_ID[1])
                {
                    byte[] arr = new byte[int_Len];
                    Array.Copy(byt_MyRevData, 2, arr, 0, int_Len);
                    Array.Reverse(arr);
                    value = BitConverter.ToString(arr, 0, int_Len);
                    value = value.Replace("-", "");
                    // return true;
                }
            }
            return value;
        }

        public override float ReadData(string str_ID, int len, int int_Dot)
        {
            float value = 0F;
            byte[] byt_ID = BitConverter.GetBytes(Convert.ToInt16("0x" + str_ID, 16));
            byte[] byt_MyRevData = new byte[0];
            bool bln_Sequela = false;
            bool r = ExeCommand(GetAddressByte(), 0x01, byt_ID, ref bln_Sequela, ref byt_MyRevData);
            if (r)
            {
                if (byt_MyRevData[0] == byt_ID[0] && byt_MyRevData[1] == byt_ID[1])
                {
                    byte[] arr = new byte[len];
                    Array.Copy(byt_MyRevData, 2, arr, 0, len);
                    Array.Reverse(arr);   //倒序
                    string str = BitConverter.ToString(arr, 0, len);
                    str = str.Replace("-", "");
                    value = Convert.ToSingle(str) / Convert.ToSingle(Math.Pow(10, int_Dot));
                }
            }
            return value;
        }

        public override string[] ReadDataBlock(string str_ID, int int_Len)
        {
            return ReadDataBlock1(str_ID, int_Len);
        }

        public override float[] ReadDataBlock(string str_ID, int int_Len, int int_Dot)
        {
            float[] sng_Value = new float[0];
            byte[] byt_ID = BitConverter.GetBytes(Convert.ToInt16("0x" + str_ID, 16));
            byte[] byt_MyRevData = new byte[0];
            bool bln_Sequela = false;
            bool bln_Result = ExeCommand(GetAddressByte(), 0x01, byt_ID, ref bln_Sequela, ref byt_MyRevData);
            if (bln_Result)
            {

                if (byt_MyRevData[0] == byt_ID[0] && byt_MyRevData[1] == byt_ID[1])
                {
                    byte[] byt_TmpValue = new byte[byt_MyRevData.Length - 2];
                    Array.Copy(byt_MyRevData, 2, byt_TmpValue, 0, byt_MyRevData.Length - 2);
                    Array.Reverse(byt_TmpValue);
                    string str_Tmp = BitConverter.ToString(byt_TmpValue, 0);
                    str_Tmp = str_Tmp.Replace("-", "");

                    str_Tmp = str_Tmp.TrimStart(new char[] { 'A' });        //去掉AA
                    str_Tmp = str_Tmp.TrimEnd(new char[] { 'A' });

                    if (str_Tmp.IndexOf("AA") > 0)      //防止各项数据项目用AA隔开
                    {
                        string[] str_Para = str_Tmp.Split(new string[] { "AA" }, StringSplitOptions.None);
                        if (int_Len * str_Para.Length * 2 + (str_Para.Length - 1) * 2 == str_Tmp.Length)
                        {
                            Array.Resize(ref sng_Value, str_Para.Length);
                            for (int i = 0; i < str_Para.Length; i++)
                                sng_Value[i] = Convert.ToSingle(str_Para[i]) / Convert.ToSingle(Math.Pow(10, int_Dot));
                            Array.Reverse(sng_Value);
                        }
                    }

                    int int_Count = str_Tmp.Length / (int_Len * 2);
                    Array.Resize(ref sng_Value, int_Count);
                    for (int i = 0; i < int_Count; i++)
                        sng_Value[i] = Convert.ToSingle(str_Tmp.Substring(i * int_Len * 2, int_Len * 2)) / Convert.ToSingle(Math.Pow(10, int_Dot));
                    Array.Reverse(sng_Value);
                }
            }
            return sng_Value;
        }



        /// <summary>
        /// 读取数据（字符型，数据块）
        /// </summary>
        /// <param name="dataFlag">标识符,两个字节</param>
        /// <param name="len">数据长度(字节数)</param>
        /// <returns></returns>
        private string[] ReadDataBlock1(string dataFlag, int len)
        {
            string[] value = new string[0];
            byte[] arrID = BitConverter.GetBytes(Convert.ToInt16("0x" + dataFlag, 16));
            byte[] revData = new byte[0];
            bool sequela = false;
            bool r = ExeCommand(GetAddressByte(), 0x01, arrID, ref sequela, ref revData);
            if (r)
            {
                if (revData[0] == arrID[0] && revData[1] == arrID[1])
                {
                    byte[] arr = new byte[revData.Length - 2];
                    Array.Copy(revData, 2, arr, 0, revData.Length - 2);
                    Array.Reverse(arr);

                    string tmp = BitConverter.ToString(arr, 0);
                    tmp = tmp.Replace("-", "");
                    tmp = tmp.TrimStart(new char[] { 'A' });        //去掉AA
                    tmp = tmp.TrimEnd(new char[] { 'A' });

                    if (tmp.IndexOf("AA") > 0)      //防止各项数据项目用AA隔开
                    {
                        string[] para = tmp.Split(new string[] { "AA" }, StringSplitOptions.None);
                        if (len * para.Length * 2 + (para.Length - 1) * 2 == tmp.Length)
                        {
                            Array.Resize(ref value, para.Length);
                            for (int i = 0; i < para.Length; i++)
                                value[i] = para[i];
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
        /// 写日期时间
        /// </summary>
        /// <param name="value">日期时间 yymmddhhffss</param>
        /// <returns></returns>
        public override bool WriteDateTime(string value)
        {
            string week = "0" + GetWeekday(value, protocolInfo.SundayIndex).ToString();
            if (WriteDateTimeType == 1)      //分项目操作
            {
                byte[] data = BitConverter.GetBytes(Convert.ToInt32("0x" + value.Substring(0, 6) + week, 16));
                bool r = WriteData("c010", data);
                if (r)
                {
                    byte[] tmp = BitConverter.GetBytes(Convert.ToInt32("0x" + value.Substring(6, 6), 16));
                    data = new byte[3];
                    Array.Copy(tmp, data, 3);

                    r = WriteData("c011", data);
                    return r;
                }
                return false;
            }
            else
            {
                string str_ID = "c0";
                if (WriteDateTimeType == 2)
                    str_ID += "1F";
                else
                    str_ID += "12";

                byte[] byt_Data;
                if (WriteDateTimeType == 3)
                {
                    byt_Data = new byte[6];
                    byte[] byt_Tmp = BitConverter.GetBytes(Convert.ToInt64("0x" + value.Substring(0, 12), 16));
                    Array.Copy(byt_Tmp, byt_Data, 6);
                }
                else
                {
                    byt_Data = new byte[7];
                    byte[] byt_Tmp = BitConverter.GetBytes(Convert.ToInt64("0x" + value.Substring(6, 6)
                                                                                + value.Substring(0, 6)
                                                                                + week, 16));
                    Array.Copy(byt_Tmp, byt_Data, 7);
                }

                if (protocolInfo.BlockAddAA)
                {
                    Array.Resize(ref byt_Data, byt_Data.Length + 1);
                    byt_Data[byt_Data.Length - 1] = 0xaa;
                }
                return WriteData(str_ID, byt_Data);
            }
        }

        /// <summary>
        /// 写地址(需要按键响应)
        /// </summary>
        /// <param name="address">写入地址</param>
        /// <returns></returns>
        /// 
        public override bool WriteAddress(string address)
        {
            byte[] addr = new byte[] { 0x99, 0x99, 0x99, 0x99, 0x99, 0x99 };
            byte[] data = new byte[6];

            string tmp = "0x";
            if (address.Length > 12)
                tmp += address.Substring(0, 12);
            else if (address.Length == 0)
                tmp += "0";
            else
                tmp += address;

            byte[] arr = BitConverter.GetBytes(Convert.ToInt64(tmp, 16));
            Array.Copy(arr, data, 6);

            byte[] revData = new byte[0];
            bool sequela = false;
            return ExeCommand(addr, 0x0a, data, ref sequela, ref revData);
        }

        /// <summary>
        /// 写时段
        /// </summary>
        /// <param name="data">时段组</param>
        /// <returns></returns>
        /// 
        public override bool WritePeriodTime(string[] data)
        {
            bool r = WriteData("c33f", 3, data);   //写时段组
            if (r)
            {
                r = WriteData("c312", 1, 0, Convert.ToSingle(data.Length));  //写时段数
            }
            return r;
        }

        /// <summary>
        /// 写数据(字符型，数据项)
        /// </summary>
        /// <param name="dataFlag">标识符,两个字节</param>
        /// <param name="data">写入数据</param>
        /// <returns></returns>
        public override bool WriteData(string dataFlag, byte[] data)
        {
            if (dataFlag.Length != 4) return false;

            byte[] byt_ID = BitConverter.GetBytes(Convert.ToInt16("0x" + dataFlag, 16));

            if (protocolInfo.VerifyPasswordType > 0)       //需要密码验证
            {
                if (!VerifyPassword(protocolInfo.VerifyPasswordType, protocolInfo.WritePassword))    //验证是否通过
                {
                    return false;
                }
            }

            int int_DataLen = 2;                                //标识符两字节
            if (protocolInfo.DataFieldPassword) int_DataLen += 4;      //是否带密码，4字节
            int_DataLen += data.Length;                             //数据
            byte[] byt_Data = new byte[int_DataLen];
            Array.Copy(byt_ID, 0, byt_Data, 0, 2);
            if (protocolInfo.DataFieldPassword)                //数据域包含密码
            {
                byte[] byt_Tmp = BitConverter.GetBytes(Convert.ToInt32("0x" + protocolInfo.WritePassword, 16));
                Array.Copy(byt_Tmp, 0, byt_Data, 2, 4);
                Array.Copy(data, 0, byt_Data, 6, data.Length);
            }
            else
                Array.Copy(data, 0, byt_Data, 2, data.Length);

            byte[] revData = new byte[0];
            bool sequela = false;
            return ExeCommand(GetAddressByte(), 0x04, byt_Data, ref sequela, ref revData);

        }

        /// <summary>
        /// 写数据(数据型，数据项)
        /// </summary>
        /// <param name="str_ID">标识符,两个字节</param>
        /// <param name="int_Len">数据长度(字节数)</param>
        /// <param name="int_Dot">小数位</param>
        /// <param name="sng_Value">写入数据</param>
        /// <returns></returns>
        public override bool WriteData(string str_ID, int int_Len, int int_Dot, float sng_Value)
        {
            if (str_ID.Length != 4)
                return false;

            byte[] byt_Data = new byte[int_Len];
            string str_Tmp = Convert.ToInt64(sng_Value * Math.Pow(10, int_Dot)).ToString();
            byte[] byt_Tmp = BitConverter.GetBytes(Convert.ToInt64("0x" + str_Tmp, 16));
            Array.Copy(byt_Tmp, 0, byt_Data, 0, int_Len);
            return WriteData(str_ID, byt_Data);

        }

        /// <summary>
        /// 写数据(字符型，数据块)
        /// </summary>
        /// <param name="str_ID">标识符,两个字节</param>
        /// <param name="int_Len">数据长度(块中每项字节数不于8字节)</param>
        /// <param name="str_Value">写入数据</param>
        /// <returns></returns>
        public override bool WriteData(string str_ID, int int_Len, string[] str_Value)
        {

            if (str_ID.Length != 4)
            {
                //m_str_LostMessage = "标识符不符合要求，不是2字节";
                return false;
            }
            byte[] byt_Data = new byte[int_Len * str_Value.Length];
            for (int i = 0; i < str_Value.Length; i++)
            {
                byte[] byt_Tmp = BitConverter.GetBytes(Convert.ToInt64("0x" + str_Value[i], 16));
                Array.Copy(byt_Tmp, 0, byt_Data, i * int_Len, int_Len);
            }
            if (protocolInfo.BlockAddAA)
            {
                Array.Resize(ref byt_Data, byt_Data.Length + 1);
                byt_Data[byt_Data.Length - 1] = 0xaa;
            }
            return WriteData(str_ID, byt_Data);

        }

        /// <summary>
        /// 写数据(字符型，数据块)
        /// </summary>
        /// <param name="str_ID">标识符,两个字节</param>
        /// <param name="int_Len">数据长度字节数</param>
        /// <param name="str_Value">写入数据</param>
        /// <returns></returns>
        /// 
        public override bool WriteData(string str_ID, int int_Len, string str_Value)
        {
            if (str_ID.Length != 4) return false;

            byte[] byt_Data = new byte[int_Len];
            byte[] byt_Tmp = BitConverter.GetBytes(Convert.ToInt64("0x" + str_Value, 16));
            Array.Copy(byt_Tmp, byt_Data, int_Len);
            return WriteData(str_ID, byt_Data);

        }

        /// <summary>
        /// 写数据(数据型，数据块)
        /// </summary>
        /// <param name="str_ID">标识符,两个字节</param>
        /// <param name="int_Len">数据长度(块中每项字节数不于8字节)</param>
        /// <param name="int_Dot">小数位</param>
        /// <param name="str_Value">写入数据</param>
        /// <returns></returns>
        public override bool WriteData(string str_ID, int int_Len, int int_Dot, float[] sng_Value)
        {

            if (str_ID.Length != 4) return false;

            byte[] byt_Data = new byte[int_Len * sng_Value.Length];

            for (int i = 0; i < sng_Value.Length; i++)
            {
                string str_Tmp = Convert.ToInt64(sng_Value[i] * Math.Pow(10, int_Dot)).ToString();
                byte[] byt_Tmp = BitConverter.GetBytes(Convert.ToInt64("0x" + str_Tmp, 16));
                Array.Copy(byt_Tmp, 0, byt_Data, i * int_Len, int_Len);
            }
            if (protocolInfo.BlockAddAA)
            {
                Array.Resize(ref byt_Data, byt_Data.Length + 1);
                byt_Data[byt_Data.Length - 1] = 0xaa;
            }
            return WriteData(str_ID, byt_Data);
        }

        /// <summary>
        /// 冻结命令
        /// </summary>
        /// <param name="int_Type">操作类型 0=</param>
        /// <param name="str_DateHour">冻结时间，MMDDhhmm(月.日.时.分)</param>
        /// <returns></returns>
        public override bool FreezeCmd(string str_DateHour)
        {
            return false;
        }

        /// <summary>
        /// 更改波特率
        /// </summary>
        /// <param name="int_Type">操作类型 0=</param>
        /// <param name="setting">波特率</param>
        /// <returns></returns>
        public override bool ChangeSetting(string setting)
        {
            string[] para = setting.Split(new char[] { ',' });
            byte[] data = new byte[1];
            data[0] = GetSettingCode6451997(Convert.ToInt16(para[0]));
            if (data[0] == 0) return false;

            data[0] |= 1;       //永久更改
            byte[] revData = new byte[0];
            bool sequela = false;
            bool result = ExeCommand(GetAddressByte(), 0x0c, data, ref sequela, ref revData);
            if (result)
            {
                if (revData[0] == 0xff)
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
            byte[] data = new byte[8];
            byte[] tmp = BitConverter.GetBytes(Convert.ToInt32("0x" + oldPws, 16));
            Array.Copy(tmp, 0, data, 0, 4);

            tmp = BitConverter.GetBytes(Convert.ToInt32("0x" + newPsw, 16));
            Array.Copy(tmp, 0, data, 4, 4);

            byte[] revData = new byte[0];
            bool sequela = false;
            return ExeCommand(GetAddressByte(), 0x0f, data, ref sequela, ref revData);

        }

        /// <summary>
        /// 清空需量
        /// </summary>
        /// <param name="int_Type">操作类型 0=配置文件，1=指令清需量，2=抄表日清需量(C01f),3=抄表日清需量(C012 7字节), 4=抄表日清需量(C012 6字节)</param>
        /// <returns></returns>
        public override bool ClearDemand()
        {
            if (ClearDemandType != 1)
            {
                return ClearDemandCBR();        //抄表日清需量
            }
            else                                        //指令清需量
            {
                byte[] byt_Data = BitConverter.GetBytes(Convert.ToInt32("0x" + protocolInfo.ClearDemandPassword + protocolInfo.ClearDemandClass, 16));
                byte[] byt_MyRevData = new byte[0];
                bool bln_Sequela = false;
                return ExeCommand(GetAddressByte(), 0x10, byt_Data, ref bln_Sequela, ref byt_MyRevData);
            }

        }

        /// <summary>
        /// 清空电量
        /// </summary>
        ///  <param name="int_Type">类型 0=配置文件，1=江苏三相多费率电子表清电量20节字，2=江苏三相多费率电子表清电量16节字,3=江苏0.5s级国产表，4=江苏单相表</param>
        /// <returns></returns>
        public override bool ClearEnergy()
        {

            bool result = false;

            byte[] psw = BitConverter.GetBytes(Convert.ToUInt64("0x" + protocolInfo.ClearDLPassword + protocolInfo.ClearDLClass, 16));
            byte[] revData = new byte[0];
            byte[] data;
            bool sequela = false;
            switch (ClearEnergyType)
            {
                case 1:                     //C119  20字节  

                    data = new byte[26];
                    data[0] = 0x19;
                    data[1] = 0xc1;
                    Array.Copy(psw, 0, data, 2, 4);
                    result = ExeCommand(GetAddressByte(), 0x04, data, ref sequela, ref revData);
                    break;
                case 2:                     //C119  16字节  

                    data = new byte[22];
                    data[0] = 0x19;
                    data[1] = 0xc1;
                    Array.Copy(psw, 0, data, 2, 4);
                    result = ExeCommand(GetAddressByte(), 0x04, data, ref sequela, ref revData);
                    break;

                case 3:
                    result = ExeCommand(GetAddressByte(), 0x09, psw, ref sequela, ref revData);

                    break;
                case 4:                     //江苏单相表
                    /* 2010-03-11 by Gqs 江苏单相表电量清零规则 ？？？*/
                    data = new byte[10];
                    data[0] = 0x19;
                    data[1] = 0xc1;
                    Array.Copy(psw, 0, data, 2, 4);
                    result = ExeCommand(GetAddressByte(), 0x04, data, ref sequela, ref revData);

                    break;
                case 5:                     //C119  4字节 
                    data = new byte[10];
                    data[0] = 0x19;
                    data[1] = 0xc1;
                    Array.Copy(psw, 0, data, 2, 4);
                    result = ExeCommand(GetAddressByte(), 0x04, data, ref sequela, ref revData);
                    break;
                default:
                    break;
            }
            return result;
        }

        /// <summary>
        /// 设置多功能脉冲端子输出脉冲类型
        /// </summary>
        /// <param name="int_Type">操作类型0=配置文件,  1=林洋(设置E831)</param>
        /// <param name="pulse">端子输出脉冲类型 0=秒脉冲，1=需量周期，2=时段投切。</param>
        /// <returns></returns>
        public override bool SetPulseCom(byte pulse)
        {

            byte[] data = new byte[1];
            if (pulse == 1)
                data[0] = 0x06;
            else if (pulse == 1)
                data[0] = 0x05;
            else
                data[0] = 0x00;
            return WriteData("e831", data);
        }

        #endregion



        #region-------------------私有成员-----------------------------

        /// <summary>
        /// 换算电量标识符
        /// </summary>
        /// <param name="block">块操作</param>
        /// <param name="pDirect">功率类型，1=P+ 2=P- 3=Q+ 4=Q- 5=Q1 6=Q2 7=Q3 8=Q4</param>
        /// <param name="tariffType">费率类型，0=总，1=尖,2=峰，3=平，4=谷</param>
        /// <returns></returns>
        private string GetEnergyID(bool block, int pDirect, int tariffType)
        {
            byte[] byt_ID = new byte[2];
            byt_ID[0] = Convert.ToByte(pDirect > 1 ? 0x91 : 0x90);
            if (block)
            {
                if (pDirect > 5)        //二三四限象没有按顺序
                {
                    if (pDirect == 6)
                        byt_ID[1] = 0x5f;
                    else if (pDirect == 7)
                        byt_ID[1] = 0x6f;
                    else
                        byt_ID[1] = 0x4f;
                }
                else
                    byt_ID[1] = Convert.ToByte(pDirect > 2 ? 0x1f + 0x10 * (pDirect - 3) : 0x1f + 0x10 * (pDirect - 1));
            }
            else
            {
                if (pDirect > 5)    //二三四限象没有按顺序
                {
                    if (pDirect == 6)
                        byt_ID[1] = 0x50;
                    else if (pDirect == 7)
                        byt_ID[1] = 0x60;
                    else
                        byt_ID[1] = 0x40;

                    byt_ID[1] += Convert.ToByte(tariffType);
                }
                else
                    byt_ID[1] = Convert.ToByte(pDirect > 1 ? 0x10 + 0x10 * (pDirect - 3) + tariffType : 0x10 + 0x10 * (pDirect - 1) + tariffType);
            }
            return BitConverter.ToString(byt_ID).Replace("-", "");
        }

        /// <summary>
        /// 换算需量标识符
        /// </summary>
        /// <param name="bln_Block">块操作</param>
        /// <param name="int_PDirect">功率类型，0=P+ 1=P- 2=Q+ 3=Q- 4=Q1 5=Q2 6=Q3 7=Q4</param>
        /// <param name="int_TariffType">费率类型，0=总，1=尖,2=峰，3=平，4=谷</param>
        /// <returns></returns>
        private string GetDemandID(bool bln_Block, int int_PDirect, int int_TariffType)
        {
            byte[] byt_ID = new byte[2];
            byt_ID[0] = Convert.ToByte(int_PDirect > 1 ? 0xA1 : 0xA0);
            if (bln_Block)
            {
                if (int_PDirect > 5)        //二三四限象没有按顺序
                {
                    if (int_PDirect == 6)
                        byt_ID[1] = 0x5f;
                    else if (int_PDirect == 7)
                        byt_ID[1] = 0x6f;
                    else
                        byt_ID[1] = 0x4f;
                }
                else
                    byt_ID[1] = Convert.ToByte(int_PDirect > 2 ? 0x1f + 0x10 * (int_PDirect - 3) : 0x1f + 0x10 * (int_PDirect - 1));
            }
            else
            {
                if (int_PDirect > 5)    //二三四限象没有按顺序
                {
                    if (int_PDirect == 6)
                        byt_ID[1] = 0x50;
                    else if (int_PDirect == 7)
                        byt_ID[1] = 0x60;
                    else
                        byt_ID[1] = 0x40;

                    byt_ID[1] += Convert.ToByte(int_TariffType);
                }
                else
                    byt_ID[1] = Convert.ToByte(int_PDirect > 2 ? 0x10 + 0x10 * (int_PDirect - 3) + int_TariffType : 0x10 + 0x10 * (int_PDirect - 1) + int_TariffType);
            }
            return BitConverter.ToString(byt_ID).Replace("-", "");
        }

        /// <summary>
        /// 星期代码转换
        /// </summary>
        /// <param name="str_Date">日期</param>
        /// <param name="int_SundayIndex">星期日序号</param>
        /// <returns></returns>
        private int GetWeekday(string str_Date, int int_SundayIndex)
        {
            DateTime dte_DateTime = new DateTime(Convert.ToInt16(str_Date.Substring(0, 4)),
                                                 Convert.ToInt16(str_Date.Substring(4, 2)),
                                                 Convert.ToInt16(str_Date.Substring(6, 2)));

            int int_SysWeekday = (int)dte_DateTime.DayOfWeek;
            if (int_SundayIndex == 1)
                return int_SysWeekday + 1;
            else if (int_SundayIndex == 7)
            {
                if (int_SysWeekday == 0)
                    return 7;
                else
                    return int_SysWeekday;
            }
            else
                return int_SysWeekday;
        }

        /// <summary>
        /// 抄表日清空需量
        /// </summary>
        /// <param name="int_Type">设置时间类型</param>
        /// <returns></returns>
        private bool ClearDemandCBR()
        {
            //------------读取抄表日-------------
            string str_CBR = ReadData("c117", 2);
            if (str_CBR.Length != 4) str_CBR = "0100";  //默认1号0时
                                                        //-----------设置电表运行在抄表日期时间----------------------
            DateTime dte_DateTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, Convert.ToInt16(str_CBR.Substring(0, 2)), Convert.ToInt16(str_CBR.Substring(2, 2)), 0, 0);
            dte_DateTime = dte_DateTime.AddSeconds(-5);
            string str_DateTime = dte_DateTime.ToString("yyMMddHHmmss");
            return WriteDateTime(str_DateTime);

        }

        /// <summary>
        /// 分项读取日期时间
        /// </summary>
        /// <param name="str_DateTime">返回日期时间</param>
        /// <returns></returns>
        private bool ReadDateTimeSubItem(ref string str_DateTime)
        {
            bool bln_Sequela = false;
            byte[][] byt_ID = new byte[][]{new byte[]{0x10,0xc0}, new byte[]{0x11,0xc0}};
            //------------读日期
            byte[] byt_RevData = new byte[0];
            bool bln_Result = ExeCommand(GetAddressByte(), 0x01, byt_ID[0], ref bln_Sequela, ref byt_RevData);
            byte[] byt_DateTime = new byte[6];
            if (bln_Result)
            {
                if (byt_RevData[0] == byt_ID[0][0] && byt_RevData[1] == byt_ID[0][1])
                {
                    byt_DateTime[2] = byt_RevData[3];
                    byt_DateTime[1] = byt_RevData[4];
                    byt_DateTime[0] = byt_RevData[5];

                    //----------读时间
                    byt_RevData = new byte[0];
                    bln_Result = ExeCommand(GetAddressByte(), 0x01, byt_ID[1], ref bln_Sequela, ref byt_RevData);
                    if (bln_Result)
                    {
                        if (byt_RevData[0] == byt_ID[1][0] && byt_RevData[1] == byt_ID[1][1])
                        {
                            byt_DateTime[5] = byt_RevData[2];
                            byt_DateTime[4] = byt_RevData[3];
                            byt_DateTime[3] = byt_RevData[4];
                            string str_Tmp = BitConverter.ToString(byt_DateTime).Replace("-", "");
                            string[] str_Para = new string[] { "YY", "MM", "DD", "HH", "FF", "SS" };
                            string datetimeformat = protocolInfo.DateTimeFormat;
                            if (datetimeformat.StartsWith("YYYY")) datetimeformat = datetimeformat.Substring(2);
                            if (datetimeformat.Length != str_Tmp.Length)
                            {

                                return false;
                            }
                            str_DateTime = string.Empty;
                            for (int i = 0; i < 6; i++)
                            {
                                int int_Index = datetimeformat.IndexOf(str_Para[i]);
                                str_DateTime += str_Tmp.Substring(int_Index, 2);
                                if (i < 2)
                                    str_DateTime += "-";
                                else if (i == 2)
                                    str_DateTime += " ";
                                else if (i != 5)
                                    str_DateTime += ":";
                            }
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else
                        return false;
                }
                else
                {
                    return false;
                }
            }
            else
                return false;
        }

        /// <summary>
        /// 波特率特征字
        /// </summary>
        /// <param name="int_BTL">波特率</param>
        /// <returns></returns>
        private byte GetSettingCode6451997(int int_BTL)
        {
            if (int_BTL == 300)
                return 2;
            else if (int_BTL == 600)
                return 4;
            else if (int_BTL == 2400)
                return 16;
            else if (int_BTL == 4800)
                return 32;
            else if (int_BTL == 9600)
                return 64;
            else
                return 0;
        }

        /// <summary>
        /// 探测地址
        /// </summary>
        /// <param name="address">返回地址</param>
        /// <returns></returns>
        private bool DetectAddress(ref string address)
        {
            byte[] uAddr = new byte[] { 0xAA, 0xAA, 0xAA, 0xAA, 0xAA, 0xAA };
            byte[][] data = new byte[][] { new byte[] { 0x34,0xc0 },          //设备号
                                                 new byte[] { 0x33,0xc0 } ,         //用户号
                                                 new byte[] { 0x32,0xc0 } ,         //表号
                                                 new byte[] { 0x83,0xe4 } ,         //江苏预付费表的通信地址
                                                 new byte[] { 0x1f,0x90 }           //P+所有电量
                                                };


            for (int i = 0; i < data.Length; i++)
            {
                byte[] revData = new byte[0];
                byte[] addr = new byte[6];
                bool sequela = false;
                bool result = ExeCommand(uAddr, 0x01, data[i], ref sequela, ref revData, ref addr);
                if (result)
                {
                    if (Array.Equals(uAddr, addr) && i < 3)      //返帧的地址与通用地址一样,用读回的数据作为地址下发
                    {
                        if (revData.Length >= 8)
                        {
                            Array.Copy(revData, 2, addr, 0, 6);       //取出数据域中的数据作为地址，并下发指令，验证是否是地址
                            result = ExeCommand(addr, 0x01, data[i], ref sequela, ref revData);
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
        /// 强制性读地址，需要按相应的按銉或开关配合
        /// </summary>
        /// <returns></returns>
        private bool CptReadAddress(ref string address)
        {
            byte[] data = new byte[0];
            byte[] uAddr = new byte[] { 0x99, 0x99, 0x99, 0x99, 0x99, 0x99 };
            byte[] revData = new byte[0];
            byte[] addr = new byte[6];
            bool sequela = false;
            bool result = ExeCommand(uAddr, 0x0D, data, ref sequela, ref revData, ref addr);
            if (result)
            {
                if (Array.Equals(uAddr, addr))      //返回帧的地址域是广播地址
                {
                    Array.Reverse(revData);
                    address = BitConverter.ToString(revData).Replace("-", "");
                    return true;
                }
                else     //返回帧的地址域不是广播地址,则是电能表实际地址
                {
                    Array.Reverse(addr);
                    address = BitConverter.ToString(addr).Replace("-", "");
                    return true;
                }
            }
            else
                return false;
        }

        /// <summary>
        /// 密码验证方式
        /// </summary>
        /// <param name="int_Type">验证类型，0=无验证，1=A型表0x05, 2=浩宁达表0x08
        /// 3=恒星表验证方式 0x14, 4=三星表验证方式 0x0E 5=爱拓利验证方式 C212 6=红相3000型645表验证方式 EF00 </param>
        /// <param name="str_Password">密码</param>
        /// <returns></returns>
        private bool VerifyPassword(int int_Type, string str_Password)
        {
            bool bln_Result = true;

            switch (int_Type)
            {
                case 1:                         //A型表验证方式
                    bln_Result = VerifyPassword_0x05(str_Password);
                    break;
                case 2:                         //浩宁达表验证方式 0x08
                    bln_Result = VerifyPassword_0x08(str_Password);
                    break;
                case 3:                         //恒星表验证方式 0x14
                    bln_Result = VerifyPassword_0x14(str_Password);
                    break;
                case 4:                         //三星表验证方式 0x0E
                    bln_Result = VerifyPassword_0x0E(str_Password);
                    break;
                case 5:                         //爱拓利验证方式 C212
                    bln_Result = VerifyPassword_C212(str_Password);
                    break;
                case 6:                         // 红相3000型645表验证方式 EF00 
                    bln_Result = VerifyPassword_EF00(str_Password);
                    break;
                default:
                    break;
            }

            return bln_Result;

        }

        /// <summary>
        /// A型表验证方式 0x05
        /// </summary>
        /// <param name="str_Password">密码</param>
        /// <returns></returns>
        private bool VerifyPassword_0x05(string str_Password)
        {
            //A型号表  05指令
            //数据域格式:  密码+编程有效时间(秒)

            byte[] byt_Data = new byte[5];              //密码(3)+密码等级(1)+编程有效时间(1)
            byte[] byt_Tmp = BitConverter.GetBytes(Convert.ToInt32("0x" + str_Password, 16));
            Array.Copy(byt_Tmp, 0, byt_Data, 2, 4);
            byt_Data[4] = 0x0A;
            byte[] byt_MyRevData = new byte[0];
            bool bln_Sequela = false;
            return ExeCommand(GetAddressByte(), 0x05, byt_Data, ref bln_Sequela, ref byt_MyRevData);

        }

        /// <summary>
        /// 浩宁达表验证方式 0x08
        /// </summary>
        /// <param name="str_Password">密码</param>
        /// <returns></returns>
        private bool VerifyPassword_0x08(string str_Password)
        {
            //浩宁达    08指令
            //数据域格式:   等级+密码(12位)
            byte[] byt_Data = new byte[7];              //密码(6)+密码等级(1)
            string str_Tmp = "00000000000000".Substring(0, 14 - str_Password.Length) + str_Password;
            byte[] byt_Tmp = BitConverter.GetBytes(Convert.ToUInt64("0x" + str_Tmp, 16));
            Array.Copy(byt_Tmp, 0, byt_Data, 0, 7);

            byte[] byt_MyRevData = new byte[0];
            bool bln_Sequela = false;
            return ExeCommand(GetAddressByte(), 0x08, byt_Data, ref bln_Sequela, ref byt_MyRevData);

        }

        /// <summary>
        /// 恒星表验证方式 0x14
        /// </summary>
        /// <param name="str_Password">密码</param>
        /// <returns></returns>
        private bool VerifyPassword_0x14(string str_Password)
        {
            //恒星    14指令
            //数据域格式:   等级+密码(6位)
            byte[] byt_Data = new byte[4];              //密码(3字节)+密码等级(1字节)
            byte[] byt_Tmp = BitConverter.GetBytes(Convert.ToUInt64("0x" + str_Password, 16));
            Array.Copy(byt_Tmp, 0, byt_Data, 0, 4);
            byte[] byt_MyRevData = new byte[0];
            bool bln_Sequela = false;
            return ExeCommand(GetAddressByte(), 0x14, byt_Data, ref bln_Sequela, ref byt_MyRevData);

        }

        /// <summary>
        /// 三星表验证方式 0x0E
        /// </summary>
        /// <param name="str_Password">密码</param>
        /// <returns></returns>
        private bool VerifyPassword_0x0E(string str_Password)
        {
            //宁波三星   0E指令
            //数据域格式:   等级+密码(6位)
            byte[] byt_Data = new byte[4];              //密码(3字节)+密码等级(1字节)
            byte[] byt_Tmp = BitConverter.GetBytes(Convert.ToUInt64("0x" + str_Password, 16));
            Array.Copy(byt_Tmp, 0, byt_Data, 0, 4);
            byte[] byt_MyRevData = new byte[0];
            bool bln_Sequela = false;
            return ExeCommand(GetAddressByte(), 0x0E, byt_Data, ref bln_Sequela, ref byt_MyRevData);

        }

        /// <summary>
        /// 爱拓利验证方式 C212
        /// </summary>
        /// <param name="str_Password">密码</param>
        /// <returns></returns>
        private bool VerifyPassword_C212(string str_Password)
        {
            //爱拓利  写密码到C212  
            //数据域格式:   密码(6位)+等级
            byte[] byt_Data = new byte[6];              //密码(3字节)+密码等级(1字节)
            byte[] byt_Tmp = BitConverter.GetBytes(Convert.ToUInt16("0xC212", 16));
            Array.Copy(byt_Tmp, 0, byt_Data, 0, 2);
            byt_Tmp = BitConverter.GetBytes(Convert.ToUInt64("0x" + str_Password, 16));
            Array.Copy(byt_Tmp, 0, byt_Data, 2, 4);

            byte[] byt_MyRevData = new byte[0];
            bool bln_Sequela = false;
            return ExeCommand(GetAddressByte(), 0x04, byt_Data, ref bln_Sequela, ref byt_MyRevData);

        }

        /// <summary>
        /// 红相3000型645表验证方式 EF00
        /// </summary>
        /// <param name="password">密码</param>
        /// <returns></returns>
        private bool VerifyPassword_EF00(string password)
        {
            //红相3000型645表,默认用户名 12个0    写用户名和密码到EF00
            //红相3000型645表 用这条指令登录后,写操作处理完后,需同样用这条指令退出.
            //数据域格式:   用户名(12位)+密码(6位)+等级
            byte[] data = new byte[12];              //标识符+用户名(6字节)+密码(3字节)+密码等级(1字节)
            byte[] tmp = BitConverter.GetBytes(Convert.ToUInt16("0xEF00", 16));
            Array.Copy(tmp, 0, data, 0, 2);
            string sTmp;
            if (protocolInfo.UserID.Length >= 12)
                sTmp = protocolInfo.UserID.Substring(0, 12);
            else
                sTmp = "0000000000000".Substring(0, 12 - protocolInfo.UserID.Length) + protocolInfo.UserID;

            tmp = BitConverter.GetBytes(Convert.ToUInt64("0x" + sTmp, 16));
            Array.Copy(tmp, 0, data, 2, 6);

            if (password.Length > 8)
                sTmp = password.Substring(0, 8);
            else
                sTmp = "0000000000000".Substring(0, 8 - password.Length) + password;

            tmp = BitConverter.GetBytes(Convert.ToUInt64("0x" + sTmp, 16));
            Array.Copy(tmp, 0, data, 8, 4);

            byte[] revData = new byte[0];
            bool bln_Sequela = false;
            return ExeCommand(GetAddressByte(), 0x04, data, ref bln_Sequela, ref revData);

        }
        #endregion


        /// <summary>
        /// 读取数据    698协议
        /// </summary>
        /// <param name="oad">对象操作符</param>
        /// <param name="mode">参数安全模式</param>
        /// <param name="LstObj">返回内容列表</param>
        public override bool ReadData(StPackParas DataInfo, ref List<object> LstObj, ref List<object> reportdata, ref int intErrCode)
        {
            LstObj = new List<object>();
            reportdata = new List<object>();
            intErrCode = 0;
            return true;
        }

        /// <summary>
        /// 读取记录数据    698协议
        /// </summary>
        /// <param name="oad">对象操作符</param>
        /// <param name="mode">参数安全模式</param>
        /// <param name="LstObj">返回内容列表</param>
        public override bool ReadRecordData(StPackParas DataInfo, ref List<object> LstCsd, ref Dictionary<string, List<object>> DicObj, int recordNo, List<string> rcsd, ref int intErrCode)
        {
            LstCsd = new List<object>();
            DicObj = new Dictionary<string, List<object>>();
            intErrCode = 0;
            return true;
        }

        /// <summary>
        /// 应用连接请求
        /// </summary>
        /// <param name="frameData">帧</param>
        /// <param name="MeterAdd">表地址</param>
        /// <param name="ConnectInfo">认证机制信息</param>
        /// <returns>帧长度</returns>
        public override bool AppConnection(StConnectMechanismInfo ConnectInfo, ref List<object> LstObj, ref List<object> reportdata, ref int intErrCode)
        {
            LstObj = new List<object>();
            reportdata = new List<object>();
            intErrCode = 0;
            return true;
        }

        /// <summary>
        /// 读取安全模式参数
        /// </summary>
        /// <returns></returns>
        public override bool SecurityParameter(byte state)
        {
            return true;
        }
    }
}
