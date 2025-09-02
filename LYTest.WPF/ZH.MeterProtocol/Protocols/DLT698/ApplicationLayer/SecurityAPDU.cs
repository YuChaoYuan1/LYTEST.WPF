using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LYTest.MeterProtocol.Protocols.ApplicationLayer;
using LYTest.MeterProtocol.Protocols.DLT698.Enum;

namespace LYTest.MeterProtocol.Protocols.DLT698.ApplicationLayer
{
    public class SecurityAPDU : APDU
    {


        private bool ParseSidMac(byte[] dataInfo, byte[] Frame, ref List<object> data, int lenBytes, int len)
        {
            int MacFlag = 3 + lenBytes + len;//是否包含验证信息
            string dataInfoStr = CommFun.BytesToHexStr(dataInfo);
            data.Add(dataInfoStr);

            if (Frame.Length <= MacFlag) return false;

            if (Frame[MacFlag] == 0x01)//含有数据验证信息
            {
                int MacLength = Frame[MacFlag + 2];//Mac长度
                byte[] MacBytes = CommFun.SubBytes(Frame, MacFlag + 2 + 1, 0, MacLength);
                string Mac = CommFun.BytesToHexStr(MacBytes);
                data.Add(Mac);
                return true;
            }
            else
                return true;
        }

        /// <summary>
        /// 解析安全传输
        /// </summary>
        /// <param name="Frame"></param>
        /// <param name="ErrCode"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public override bool ParseSecurityResponse(byte[] Frame, ref int ErrCode, ref List<object> data, ref List<object> reportdata)
        {
            ErrCode = 0;
            data.Clear();
            int lenBytes = 0;//长度域字节数
            if (Frame[1] == 0x02)//DAR
            {
                ErrCode = Frame[2];
                return false;
            }
            int len;
            if (Frame[2] > 0x80)
            {
                lenBytes = Frame[2] & 0x7f;
                byte[] LenArray = CommFun.SubBytes(Frame, 3, 0, lenBytes); ;//长度域
                string s = "";
                foreach (byte b in LenArray)
                    s += b.ToString("X2");

                len = Convert.ToInt32(s, 16);
            }
            else
            {
                len = Frame[2];
            }

            byte[] dataInfo = CommFun.SubBytes(Frame, 3 + lenBytes, 0, len);//明文或密文数据
            if (Frame[1] == 00)//明文模式
            {
                if (EmServieType.GET_Response == (EmServieType)dataInfo[0])//读取请求响应
                {
                    return new ReadDataAPDU().ParseGetFrame(dataInfo, ref data, ref reportdata);
                }
                else if (EmServieType.SET_Response == (EmServieType)dataInfo[0]) //设置明文响应
                {
                    if (ParseSidMac(dataInfo, Frame, ref data, lenBytes, len))
                    {
                        return new SetDataAPDU().ParseSetFrame(dataInfo, ref ErrCode);
                    }
                    else
                    {
                        return new SetDataAPDU().ParseSetFrame(dataInfo, ref ErrCode);
                    }
                }
                else if (EmServieType.REPORT_Notification == (EmServieType)dataInfo[0])
                {
                    return new ReadDataAPDU().ParseNotificationFrame(dataInfo, ref data);
                }
                else//操作响应
                {
                    if (ParseSidMac(dataInfo, Frame, ref data, lenBytes, len))
                    {
                        return new OperationAPDU().ParseActionFrame(dataInfo, ref ErrCode);
                    }
                    else
                    {
                        return new OperationAPDU().ParseActionFrame(dataInfo, ref ErrCode);
                    }
                }
            }
            else//密文模式
            {
                return ParseSidMac(dataInfo, Frame, ref data, lenBytes, len);
            }
        }

        /// <summary>
        /// 解析安全传输
        /// </summary>
        /// <param name="Frame"></param>
        /// <param name="ErrCode"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public override bool ParseSecurityResponse(byte[] Frame, ref int ErrCode, ref List<object> data, ref Dictionary<string, List<object>> DicObj)
        {
            ErrCode = 0;
            data.Clear();
            int lenBytes = 0;//长度域字节数
            byte[] LenArray;
            byte[] dataInfo;
            if (Frame[1] == 0x02)//DAR
            {
                ErrCode = Frame[2];
                return false;
            }

            int len;
            if (Frame[2] > 0x80)
            {
                lenBytes = Frame[2] & 0x7f;
                LenArray = CommFun.SubBytes(Frame, 3, 0, lenBytes); ;//长度域
                string s = "";
                foreach (byte b in LenArray)
                    s += b.ToString("X2");

                len = Convert.ToInt32(s, 16);
            }
            else
            {
                len = Frame[2];
            }
            dataInfo = CommFun.SubBytes(Frame, 3 + lenBytes, 0, len);//明文或密文数据
            if (Frame[1] == 00)//明文模式
            {
                if (EmServieType.GET_Response == (EmServieType)dataInfo[0])//读取请求响应
                {
                    return new ReadDataAPDU().ParseReadFrame(dataInfo, ref data, ref DicObj);
                }
                else if (EmServieType.SET_Response == (EmServieType)dataInfo[0]) //设置明文响应
                {
                    if (ParseSidMac(dataInfo, Frame, ref data, lenBytes, len))
                    {
                        return new SetDataAPDU().ParseSetFrame(dataInfo, ref ErrCode);
                    }
                    else
                    {
                        return new SetDataAPDU().ParseSetFrame(dataInfo, ref ErrCode);
                    }
                }
                else//操作响应
                {
                    if (ParseSidMac(dataInfo, Frame, ref data, lenBytes, len))
                    {
                        return new OperationAPDU().ParseActionFrame(dataInfo, ref ErrCode);
                    }
                    else
                    {
                        return new OperationAPDU().ParseActionFrame(dataInfo, ref ErrCode);
                    }
                }
            }
            else//密文模式
            {
                return ParseSidMac(dataInfo, Frame, ref data, lenBytes, len);
            }
        }
    }
}
