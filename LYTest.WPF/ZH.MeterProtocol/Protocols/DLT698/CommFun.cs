using LYTest.MeterProtocol.Protocols.ApplicationLayer;
using System;
using System.Collections.Generic;
using System.Text;

namespace LYTest.MeterProtocol.Protocols.DLT698
{
    public class CommFun
    {

        public static ushort[] fcstab ={  
            #region fcstab
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
            #endregion
         };

        /// <summary>
        /// 字节数组转16进制字符串
        /// </summary>
        /// <param name="data">字节数组</param>
        /// <returns>16进制字符串</returns>
        public static string BytesToHexStr(byte[] data)
        {
            string v = "";
            for (int i = 0; i < data.Length; i++)
                v += data[i].ToString("X2");

            return v;
        }

        /// <summary>
        /// 截取数组到另一个数组
        /// </summary>
        /// <param name="SourceArray">源数组</param>
        /// <param name="StartIndex">源起始索引</param>
        /// <param name="DesStartIndex">目标数组起始索引</param>
        /// <param name="length">截取长度</param>
        /// <returns>数组</returns>
        public static byte[] SubBytes(byte[] SourceArray, int StartIndex, int DesStartIndex, int length)
        {
            if (SourceArray.Length - StartIndex < DesStartIndex + length)
            {
                length = SourceArray.Length - StartIndex;
            }
            byte[] DesArray = new byte[length + DesStartIndex];
            Array.Copy(SourceArray, StartIndex, DesArray, DesStartIndex, length);
            return DesArray;

        }

        /// <summary>
        /// CRC校验
        /// </summary>
        /// <param name="data">字节数组</param>
        /// <param name="count">数组长度</param>
        /// <returns></returns>
        public static ushort CRC16(byte[] data, int count)
        {
            ushort fcs = 0xffff;
            int i = 0;
            while (count > 0)
            {
                count -= 1;
                fcs = (ushort)((fcs >> 8) ^ fcstab[(fcs ^ data[i]) & 0xff]);
                i += 1;
            }
            fcs = (ushort)(fcs ^ 0xffff);
            return fcs;
        }

        /// <summary>
        /// 16进制字符串转换为指定长度的字节数组
        ///     【"123" => {0x01,0x23}】
        /// </summary>
        /// <param name="data">16进制字符串</param>
        /// <param name="len">数组长度</param>
        /// <returns>字节数组</returns>
        public static byte[] BytesFrom(string data, int len)
        {
            byte[] bs = new byte[len];
            data = data.Substring(data.Length - 2 * len).PadLeft(len * 2, '0');
            for (int i = 0; i < len; i++)
            {
                bs[i] = Convert.ToByte(data.Substring(i * 2, 2), 16);
            }
            return bs;
        }

        /// <summary>
        /// 反转字符串  "1234"=>"3412"
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string Reverse(string str)
        {
            StringBuilder sb = new StringBuilder();
            if (str.Length % 2 != 0)
                str = "0" + str;

            for (int i = str.Length - 2; i >= 0; i -= 2)
            {
                sb.Append(str.Substring(i, 2));
            }
            return sb.ToString();

        }

        /// <summary>
        /// 组可变字节串传输格式,（标示符+长度域+内容域）
        /// </summary>
        /// <param name="str">串</param>
        /// <returns></returns>
        public static string GroupByteString(string str)
        {
            if (string.IsNullOrEmpty(str)) return "";
            string RetData = string.Empty; ;
            if (str.Length % 2 != 0)
            {
                str = "0" + str;
            }
            int len = str.Length / 2;
            if (len > 0x7F)
            {
                RetData += "82" + len.ToString("X").PadLeft(4, '0');

            }
            else
            {
                RetData += len.ToString("X").PadLeft(2, '0');
            }
            RetData += str;
            return RetData;
        }

        /// <summary>
        /// 解析返回的数据域(A-XDR编码)为指定类型的数据
        /// </summary>
        /// <param name="data">数据域</param>
        /// <param name="CodeType">数据类型</param>
        /// <param name="len">长度</param>
        /// <param name="FloatCount">小数点位数</param>
        /// <returns></returns>
        public static object CalcuteDataInfo(DataInfos obInfo, byte[] data)
        {
            string dataStr = BytesToHexStr(data);
            object ob = null;
            switch (obInfo.DataTypeName.Trim())
            {
                case "double-long":
                    data = BytesFrom(dataStr, 4);
                    Array.Reverse(data);
                    ob = BitConverter.ToInt32(data, 0);
                    break;
                case "double-long-unsigned":
                    data = BytesFrom(dataStr, 4);
                    Array.Reverse(data);
                    ob = BitConverter.ToUInt32(data, 0);
                    break;
                case "integer":
                    data = BytesFrom(dataStr, 2);
                    Array.Reverse(data);
                    ob = BitConverter.ToInt16(data, 0);
                    break;
                case "long":
                    data = BytesFrom(dataStr, 2);
                    Array.Reverse(data);
                    ob = BitConverter.ToInt16(data, 0);
                    break;
                case "unsigned":
                    ob = BitConverter.ToUInt16(data, 0);
                    break;
                case "long-unsigned":
                    data = BytesFrom(dataStr, 2);
                    Array.Reverse(data);
                    ob = BitConverter.ToUInt16(data, 0);
                    break;
                case "long64":
                    data = BytesFrom(dataStr, 8);
                    Array.Reverse(data);
                    ob = BitConverter.ToInt64(data, 0);
                    break;
                case "long64-unsigned":
                    data = BytesFrom(dataStr, 8);
                    Array.Reverse(data);
                    ob = BitConverter.ToUInt64(data, 0);
                    break;
                case "octet-string":
                case "OAD":
                case "enum":
                    ob = dataStr;
                    break;
                case "float32":
                    ob = dataStr.PadLeft(8, '0');
                    break;
                case "float64":
                    ob = dataStr.PadLeft(16, '0');
                    break;
                case "visible-string":
                    ob = AsciiToString(data);
                    break;
                case "bool":
                    ob = data[0] == 0xff;
                    break;
                case "date_time_s":
                    ob = Convert.ToUInt16(dataStr.Substring(0, 4), 16).ToString().PadLeft(4, '0');
                    ob += Convert.ToSByte(dataStr.Substring(4, 2), 16).ToString().PadLeft(2, '0');
                    ob += Convert.ToSByte(dataStr.Substring(6, 2), 16).ToString().PadLeft(2, '0');
                    ob += Convert.ToSByte(dataStr.Substring(8, 2), 16).ToString().PadLeft(2, '0');
                    ob += Convert.ToSByte(dataStr.Substring(10, 2), 16).ToString().PadLeft(2, '0');
                    ob += Convert.ToSByte(dataStr.Substring(12, 2), 16).ToString().PadLeft(2, '0');
                    break;
            }
            if (obInfo.FloatCount > 0)
            {
                string ob1 = ob.ToString().PadLeft(data.Length * 2, '0');
                ob = ob1.Insert(ob1.Length - obInfo.FloatCount, ".");//插入小数点

            }

            return ob;
        }

        /// <summary>
        ///   //解析返回的数据域(A-XDR编码)为指定类型的数据
        /// </summary>
        /// <param name="data">数据域</param>
        /// <param name="CodeType">数据类型</param>
        /// <param name="len">长度</param>
        /// <param name="FloatCount">小数点位数</param>
        /// <returns></returns>
        public static object CalcuteDataInfo(int dataType, byte[] data)
        {
            string str = BytesToHexStr(data);
            object ob = null;

            switch (dataType)
            {
                case 3:     //bool
                    ob = data[0];
                    break;
                case 4:     //bit string
                    //data =BytesFrom(str,2);
                    //ob = Convert.ToInt32(str).ToString("X2");
                    ob = str;
                    break;
                case 5:     //double-long
                    data = BytesFrom(str, 4);
                    Array.Reverse(data);
                    ob = BitConverter.ToInt32(data, 0);
                    break;
                case 6:     //double-long-unsigned
                    data = BytesFrom(str, 4);
                    Array.Reverse(data);
                    ob = BitConverter.ToUInt32(data, 0);
                    break;
                case 15:    //integer
                    data = BytesFrom(str, 2);
                    Array.Reverse(data);
                    ob = BitConverter.ToInt16(data, 0);
                    break;
                case 16:    //long
                    data = BytesFrom(str, 2);
                    Array.Reverse(data);
                    ob = BitConverter.ToInt16(data, 0);
                    break;
                case 17:    //unsigned
                    ob = (int)data[0];
                    break;
                case 18:     //long-unsigned
                    data = BytesFrom(str, 2);
                    Array.Reverse(data);
                    ob = BitConverter.ToUInt16(data, 0);
                    break;
                case 20:     //long64
                    data = BytesFrom(str, 8);
                    Array.Reverse(data);
                    ob = BitConverter.ToInt64(data, 0);
                    break;
                case 21:    //long64-unsigned
                    data = BytesFrom(str, 8);
                    Array.Reverse(data);
                    ob = BitConverter.ToUInt64(data, 0);
                    break;
                case 9:     //octet-string
                case 81:    //OAD
                case 83:    //0x53 电气设备
                case 22:    //enum
                case 95:    //0x5F COMDCB 
                    ob = str;
                    break;
                case 23:    //float32
                    ob = str.PadLeft(8, '0');
                    break;
                case 24:    //float64
                    ob = str.PadLeft(16, '0');
                    break;
                case 10:    //visible-string
                    ob = AsciiToString(data);
                    break;

                case 28:    //DateTimeBCD 或 date_time_s
                    ob = Convert.ToUInt16(str.Substring(0, 4), 16).ToString().PadLeft(4, '0');
                    ob += Convert.ToSByte(str.Substring(4, 2), 16).ToString().PadLeft(2, '0');
                    ob += Convert.ToSByte(str.Substring(6, 2), 16).ToString().PadLeft(2, '0');
                    ob += Convert.ToSByte(str.Substring(8, 2), 16).ToString().PadLeft(2, '0');
                    ob += Convert.ToSByte(str.Substring(10, 2), 16).ToString().PadLeft(2, '0');
                    if (Convert.ToSByte(str.Substring(12, 2), 16).ToString().PadLeft(2, '0') == "-1")
                        ob += "00";
                    else
                        ob += Convert.ToSByte(str.Substring(12, 2), 16).ToString().PadLeft(2, '0');
                    break;

            }
            return ob;
        }

        /// <summary>
        /// 发送数据时，根据A-XDR编码规则组织数据内容
        /// </summary>
        /// <param name="oad">对象属性标示符</param>
        /// <param name="data">数据域</param>
        /// <returns>待下发的数据内容域</returns>
        public static string GroupDataToSend(ObjectAttributes obInfo, List<string> data)
        {
            StringBuilder RetData = new StringBuilder();
            if (!string.IsNullOrEmpty(obInfo.BigType))
            {
                RetData.Append(obInfo.BigType.PadLeft(2, '0'));
                RetData.Append(Convert.ToString(obInfo.ItemCount).PadLeft(2, '0'));
            }
            for (int i = 0; i < data.Count; i++)
            {
                string item = data[i].Replace(".", "");
                DataInfos info = obInfo.DataInfo[i];
                string type = info.DataTypeCode.ToString("X").PadLeft(2, '0');
                switch (info.DataTypeName.Trim())
                {
                    case "double-long":
                    case "double-long-unsigned":
                    case "integer":
                    case "long":
                    case "unsigned":
                    case "long-unsigned":
                    case "long64":
                    case "long64-unsigned":
                        item = Convert.ToInt64(item).ToString("X").PadLeft(info.DataLength * 2, '0');
                        break;

                    case "octet-string":
                        item = GroupByteString(item);
                        break;
                    case "float32":
                    case "OAD":
                        item = item.PadLeft(8, '0');
                        break;
                    case "enum":
                        item = item.PadLeft(2, '0');
                        break;
                    case "float64":
                        item = item.PadLeft(16, '0');
                        break;
                    case "visible-string":
                        item = StringToAscii(item);
                        break;
                    case "bool":
                        break;
                    default: break;
                }
                RetData.Append(type + item);

            }

            return RetData.ToString();

        }

        /// <summary>
        /// 字符串转为ASCII
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string StringToAscii(string str)
        {
            byte[] bytes = System.Text.Encoding.ASCII.GetBytes(str);
            return BytesToHexStr(bytes);

        }

        /// <summary>
        /// ASCII字节数组转化为字符串
        /// </summary>
        /// <param name="Ascii"></param>
        /// <returns></returns>
        public static string AsciiToString(byte[] Ascii)
        {
            return Encoding.ASCII.GetString(Ascii, 0, Ascii.Length);
        }

        /// <summary>
        /// 将DLT698协议
        /// </summary>
        /// <param name="data"></param>
        /// <param name="typeIndex"></param>
        /// <param name="result"></param>
        /// <param name="datalen">子数据的长度：数据类型 + 数据长度 + 数据量</param>
        /// <returns></returns>
        public static void GetData(byte[] data, int typeIndex, ref List<object> result, out int datalen)
        {
            datalen = 0;

            if (data.Length <= typeIndex) return;

            byte[] dataByte;
            int curIndex = typeIndex;
            datalen = 1;

            byte dataType = data[curIndex]; //数据类型

            ++curIndex;
            int len;
            switch (dataType)
            {
                case 0:
                    result.Add("");
                    return;
                case 1: //array
                case 2: //structure

                    #region structure
                    int arrayCount = data[curIndex];
                    if (data.Length < curIndex + arrayCount) return;

                    datalen++;
                    curIndex++;
                    for (int i = 0; i < arrayCount; i++)
                    {
                        GetData(data, curIndex, ref result, out int l);
                        datalen += l;
                        curIndex += l;
                    }
                    #endregion
                    break;
                case 3: //bool 1byte
                    if (data.Length < curIndex + 1) return;

                    dataByte = new byte[1];
                    Array.Copy(data, curIndex, dataByte, 0, 1);
                    result.Add(CommFun.CalcuteDataInfo(dataType, dataByte));
                    datalen += 1;
                    break;
                case 0x09:  //09  octet-string
                    len = data[curIndex];
                    if (data.Length < curIndex + len) return;
                    curIndex++;
                    dataByte = new byte[len];
                    Array.Copy(data, curIndex, dataByte, 0, len);
                    result.Add(CalcuteDataInfo(dataType, dataByte));
                    datalen += len + 1;

                    break;
                case 0x10: //16 long
                    if (data.Length < curIndex + 2) return;
                    dataByte = new byte[2];
                    Array.Copy(data, curIndex, dataByte, 0, 2);
                    result.Add(CommFun.CalcuteDataInfo(dataType, dataByte));
                    datalen += 2;
                    break;
                case 0x0A: //visible string
                    len = data[curIndex];
                    if (data.Length < curIndex + len) return;
                    curIndex++;
                    dataByte = new byte[len];
                    Array.Copy(data, curIndex, dataByte, 0, len);
                    result.Add(CalcuteDataInfo(dataType, dataByte));
                    datalen += len + 1;

                    break;

                case 0x16:  //22 enum
                    if (data.Length < curIndex + 1) return;
                    dataByte = new byte[1];
                    Array.Copy(data, curIndex, dataByte, 0, 1);
                    result.Add(CalcuteDataInfo(dataType, dataByte));
                    datalen += 1;

                    break;
                case 17:    //0x11 unsigned
                    if (data.Length < curIndex + 1) return;

                    dataByte = new byte[1];
                    Array.Copy(data, curIndex, dataByte, 0, 1);
                    result.Add(CalcuteDataInfo(dataType, dataByte));
                    datalen += 1;
                    break;

                case 18:    //long-unsigned (2 byte)
                    if (data.Length < curIndex + 2) return;

                    dataByte = new byte[2];
                    Array.Copy(data, curIndex, dataByte, 0, 2);
                    result.Add(CalcuteDataInfo(dataType, dataByte));
                    datalen += 2;
                    break;
                case 20: //long64 ,8byte
                case 21:// long64-unsigned,8byte
                    if (data.Length < curIndex + 8) return;
                    dataByte = new byte[8];
                    Array.Copy(data, curIndex, dataByte, 0, 8);
                    result.Add(CalcuteDataInfo(dataType, dataByte));
                    datalen += 8;
                    break;
                case 81:    //OAD
                case 83:    //0x53 电气设备
                case 5:     //double-long    
                case 6:     //double-long-unsigned (4 byte)
                    if (data.Length < curIndex + 4) return;

                    dataByte = new byte[4];
                    Array.Copy(data, curIndex, dataByte, 0, 4);
                    result.Add(CalcuteDataInfo(dataType, dataByte));
                    datalen += 4;
                    break;

                case 28:    //DateTimeBCD或date_time_s (7 byte)
                    if (data.Length < curIndex + 7) return;

                    dataByte = new byte[7];
                    Array.Copy(data, curIndex, dataByte, 0, 7);
                    result.Add(CalcuteDataInfo(dataType, dataByte));
                    datalen += 7;
                    break;

                case 4:     //bit-string
                    len = data[curIndex] / 8;
                    if (data.Length < curIndex + len) return;

                    curIndex += 1;
                    dataByte = new byte[len];
                    Array.Copy(data, curIndex, dataByte, 0, len);
                    result.Add(CalcuteDataInfo(dataType, dataByte));
                    datalen += len + 1;
                    break;
                case 91:    //0x5B  CSD 
                    if (data[curIndex] == 0x00) //OAD
                    {
                        if (data.Length < curIndex + 4) return;

                        curIndex += 1;
                        dataByte = new byte[4];
                        Array.Copy(data, curIndex, dataByte, 0, 4);
                        result.Add(CalcuteDataInfo(81, dataByte));
                        datalen += 5;
                    }
                    else //ROAD
                    {
                        dataByte = new byte[4];
                        if (data.Length < curIndex + 4) return;

                        curIndex += 1;
                        Array.Copy(data, curIndex, dataByte, 0, 4);
                        result.Add(CalcuteDataInfo(81, dataByte));
                        curIndex += 4;
                        datalen += 5;
                        int arrayCount1 = data[curIndex];
                        curIndex += 1;
                        datalen += 1;
                        for (int i = 0; i < arrayCount1; i++)
                        {
                            dataByte = new byte[4];
                            Array.Copy(data, curIndex, dataByte, 0, 4);
                            result[result.Count - 1] += "," + CalcuteDataInfo(81, dataByte);
                            curIndex += 4;
                            datalen += 4;

                        }


                    }
                    break;
                case 0x5F: //95 COMDCB
                    {
                        if (data.Length < curIndex + 5) return;

                        dataByte = new byte[5];
                        Array.Copy(data, curIndex, dataByte, 0, 5);
                        result.Add(CalcuteDataInfo(dataType, dataByte));
                        datalen += 5;
                    }
                    break;
                default:
                    break;

            }
            //return true;
        }

    }
}
