using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LYTest.MeterProtocol.Protocols.ApplicationLayer;
using LYTest.MeterProtocol.Protocols.DLT698.ApplicationLayer.ApplicationLayerStructure;
using LYTest.MeterProtocol.Protocols.DLT698.Enum;

namespace LYTest.MeterProtocol.Protocols.DLT698.ApplicationLayer
{
    /// <summary>
    /// 读取对象属性APDU类
    /// </summary>
    public class ReadDataAPDU : APDU
    {
        /// <summary>
        /// 对象属性标识队列
        /// </summary>
        public List<string> OAD { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public List<string> CSD { get; set; }
        /// <summary>
        /// 随机数
        /// </summary>
        private readonly string Rand = string.Empty;

        /// <summary>
        /// 读取属性模式
        /// </summary>
        private EmGetRequestMode GetRequestMode { get; set; }


        /// <summary>
        /// 明文读取读取应用层报文
        /// </summary>
        private StringBuilder ReadDataAPDUStr = null;

        /// <summary>
        /// 明文rand读取读取应用层报文
        /// </summary>
        private StringBuilder ReadDataAPDURandStr = null;
        /// <summary>
        /// 明文读取属性的结构
        /// </summary>
        private ReadByClearTextStructure.ReadApduFrameClearText ClearText;

        /// <summary>
        /// 明文+RAND 读取属性结构
        /// </summary>
        private ReadByClearMacStructure.ReadApduClearMac ClearTextMac;



        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="oad">对象属性列表</param>
        /// <param name="mode">安全模式</param>
        public ReadDataAPDU(List<string> oad, EmSecurityMode mode, EmGetRequestMode RequestMode)
            : base(mode)
        {
            OAD = oad;
            GetRequestMode = RequestMode;
            CSD = new List<string>();

        }

        public ReadDataAPDU()
        {
            ObjectInfos = ObjectInfosManage.Instance();
            CSD = new List<string>();

        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="oad">对象属性标识队列</param>
        /// <param name="mode">安全模式</param>
        /// <param name="rand">随机数</param>
        public ReadDataAPDU(List<string> oad, EmSecurityMode mode, EmGetRequestMode RequestMode, string rand)
            : base(mode)
        {
            OAD = oad;
            Rand = rand;
            GetRequestMode = RequestMode;
            CSD = new List<string>();
        }
        /// <summary>
        /// 读取对象属性
        /// </summary>
        /// <returns>帧</returns>
        public override string ReadDataAPDU_Frame()
        {

            ReadDataAPDUStr = new StringBuilder();
            ClearText = new ReadByClearTextStructure.ReadApduFrameClearText
            {
                GetRequest = ((int)EmServieType.GET_Request).ToString().PadLeft(2, '0'),
                PIID = "01"
            };
            if (GetRequestMode == EmGetRequestMode.GetRequestNormal)//读取一个对象
            {
                ClearText.GetRequestMode = ((int)EmGetRequestMode.GetRequestNormal).ToString().PadLeft(2, '0');

            }
            else if (GetRequestMode == EmGetRequestMode.GetRequestNormalList)
            {
                ClearText.GetRequestMode = ((int)EmGetRequestMode.GetRequestNormalList).ToString().PadLeft(2, '0');
                ClearText.Num = Convert.ToString(OAD.Count).PadLeft(2, '0');
            }
            else if (GetRequestMode == EmGetRequestMode.GetRequestRecord)
            {
                ClearText.PIID = "00";
                ClearText.GetRequestMode = ((int)EmGetRequestMode.GetRequestRecord).ToString().PadLeft(2, '0');
                ClearText.Rsd = "09";  //selector9 上N记录数
                ClearText.Selector = RecordNo.ToString("X2"); //上1次记录，02-上2次记录
                ClearText.Rcsd = CSD.Count.ToString("X2");//所有关联属性
                ClearText.Csd = "";
                CSD.ForEach(e => ClearText.Csd += ("00" + e));

            }
            OAD.ForEach(e => ClearText.Oads += e);
            ClearText.Timeflag = "00"; //时间标签

            GroupReadDataByClearText();

            //明文
            if (SecurityMode == EmSecurityMode.ClearText)
            {
                return ReadDataAPDUStr.ToString();
            }
            //明文+rand
            else
            {
                ReadDataAPDURandStr = new StringBuilder();
                ClearTextMac = new ReadByClearMacStructure.ReadApduClearMac
                {
                    SecurtiyRequest = "10",
                    ClearMode = "00",
                    ClearLen = ((ReadDataAPDUStr.ToString()).Length / 2).ToString("X").PadLeft(2, '0'),
                    ClearText = ClearText,
                    DataValidateMode = ((int)EmDataValidationMode.RN).ToString().PadLeft(2, '0'),
                    ValidationInfo = CommFun.GroupByteString(Rand)
                };
                GroupReadDataByClearMac();
                return ReadDataAPDURandStr.ToString();

            }


        }

        /// <summary>
        /// 组明文读取属性报文s
        /// </summary>
        private void GroupReadDataByClearText()
        {
            ReadDataAPDUStr.Append(ClearText.GetRequest);
            ReadDataAPDUStr.Append(ClearText.GetRequestMode); //
            ReadDataAPDUStr.Append(ClearText.PIID);

            if (!string.IsNullOrEmpty(ClearText.Num))
            {
                ReadDataAPDUStr.Append(ClearText.Num);
            }
            ReadDataAPDUStr.Append(ClearText.Oads);

            if (!string.IsNullOrEmpty(ClearText.Rsd))
            {
                //if (ClearText.Oads == "50040200")
                //{
                //    ReadDataAPDUStr.Append("01");
                //}
                //else
                {
                    ReadDataAPDUStr.Append(ClearText.Rsd);
                }
            }
            if (!string.IsNullOrEmpty(ClearText.Selector))
            {
                ReadDataAPDUStr.Append(ClearText.Selector);
            }

            //CSD 个数
            if (!string.IsNullOrEmpty(ClearText.Rcsd))
            {
                //if (ClearText.Oads == "50040200")
                //{
                //    ReadDataAPDUStr.Append("20210200");
                //    string sss = "1C"+( (DateTime.Now.Year).ToString("X").PadLeft(4, '0') + DateTime.Now.Month.ToString("X").PadLeft(2, '0') + (DateTime.Now.Day+1).ToString("X").PadLeft(2, '0')) +"000000";
                //    //string strHex = "";
                //    //for (int k =0;k< sss.Length/2;k++)
                //    //{

                //    //    strHex = strHex+ int.Parse( sss.Substring(k * 2, 2)).ToString("X").PadLeft(2, '0');
                //    //}
                //    ReadDataAPDUStr.Append(sss);
                //    ReadDataAPDUStr.Append("02");
                //    ReadDataAPDUStr.Append("00");
                //    ReadDataAPDUStr.Append("20210200");                     
                //    ReadDataAPDUStr.Append(ClearText.Csd);
                //    ReadDataAPDUStr.Append(ClearText.Timeflag);
                //    return;
                //}
                //else
                {
                    ReadDataAPDUStr.Append(ClearText.Rcsd);
                }
            }
            //CSD值
            if (!string.IsNullOrEmpty(ClearText.Csd))
            {
                ReadDataAPDUStr.Append(ClearText.Csd);
            }
            ReadDataAPDUStr.Append(ClearText.Timeflag);
        }

        /// <summary>
        /// 组明文+Rand读取属性报文
        /// </summary>
        private void GroupReadDataByClearMac()
        {
            ReadDataAPDURandStr.Append(ClearTextMac.SecurtiyRequest);   //安全请求
            ReadDataAPDURandStr.Append(ClearTextMac.ClearMode);         //
            ReadDataAPDURandStr.Append(ClearTextMac.ClearLen);          //明文应用数据单元, 长度
            ReadDataAPDURandStr.Append(ReadDataAPDUStr.ToString());     //APDU
            ReadDataAPDURandStr.Append(ClearTextMac.DataValidateMode);  //
            ReadDataAPDURandStr.Append(ClearTextMac.ValidationInfo);
        }


        public override bool ParseNotificationFrame(byte[] frame, ref List<object> data)
        {
            //EmGetRequestMode mode = (EmGetRequestMode)frame[1];
            OAD = new List<string>();
            CSD = new List<string>();
            int FrameOadIndex = 3;
            FrameOadIndex += 1;  //读取多个对象包含OAD个数的域 
            for (int i = 0; i < frame[3]; i++)//OAD个数
            {
                if (!ParseReadNormal(frame, ref FrameOadIndex, ref data))//FrameOadIndex:每个oad 索引
                {
                    return false;
                }
            }
            return true;

        }
        /// <summary>
        /// 解析读取返回数据
        /// </summary>
        /// <param name="frame">帧</param>
        /// <param name="data">数据内容</param>
        /// <returns>执行成功与否</returns>
        public override bool ParseGetFrame(byte[] frame, ref List<object> data, ref List<object> reportdata)
        {
            EmGetRequestMode mode = (EmGetRequestMode)frame[1];
            OAD = new List<string>();
            CSD = new List<string>();
            int FrameOadIndex = 3;
            bool T = false;
            if (mode == EmGetRequestMode.GetRequestNormal)
            {
                T = ParseReadNormal(frame, ref FrameOadIndex, ref data);
            }
            else if (mode == EmGetRequestMode.GetRequestNormalList)//一次读取多个对象
            {
                FrameOadIndex += 1;  //读取多个对象包含OAD个数的域 
                for (int i = 0; i < frame[3]; i++)//OAD个数
                {
                    if (!ParseReadNormal(frame, ref FrameOadIndex, ref data))//FrameOadIndex:每个oad 索引
                    {
                        return false;
                    }
                }
                T = true;
            }
            else if (mode == EmGetRequestMode.GetRequestRecord)
            {
                byte[] oad = new byte[4];
                byte[] csd = new byte[4];
                //byte[] dataByte;
                byte[] csdByte;
                int index = 0;//数据内容起始索引85 01 01 40 01 02 00 01 09 06 12 00 00 00 00 39 00 00 
                int Count = 0;

                Array.Copy(frame, FrameOadIndex, oad, 0, 4);
                OAD.Add(CommFun.BytesToHexStr(oad));

                int csdCount = frame[FrameOadIndex + 4];
                csdByte = new byte[csdCount * 5];
                Array.Copy(frame, FrameOadIndex + 5, csdByte, 0, csdCount * 5);
                string dataStr = CommFun.BytesToHexStr(csdByte);
                for (int i = 0; i < csdCount; i++)
                {
                    CSD.Add(dataStr.Substring(i * 10 + 2, 8));
                }
                int recordCount = frame[FrameOadIndex + 4 + csdCount * 5 + 2];

                if (recordCount < 1)
                {
                    data = null;
                    return false;
                }

                ObjectAttributes obatt = ObjectInfos.AttributeInfos.Find(e => e.Oad == OAD[0]);
                if (obatt.ItemCount == 1)
                {

                    index = FrameOadIndex + 6;
                    if (obatt.DataInfo[0].LengthFlag)// 可变长度类型
                    {
                        index++;
                    }
                    byte[] dataByte = new byte[obatt.DataInfo[0].DataLength];
                    Array.Copy(frame, index, dataByte, 0, obatt.DataInfo[0].DataLength);
                    object da = CommFun.CalcuteDataInfo(obatt.DataInfo[0], dataByte);
                    data.Add(da);
                    FrameOadIndex = index + obatt.DataInfo[0].DataLength;

                    T = true;
                }
                else// 数组或结构体类型对象属性，包含多个元素
                {
                    index = FrameOadIndex + 8;//数组或结构内部子元素数据内容索引
                    foreach (var item in obatt.DataInfo)
                    {
                        byte[] dataByte;
                        if (item.LengthFlag)// 可变长度类型
                        {
                            index++;
                            dataByte = new byte[item.DataLength];
                            Array.Copy(frame, index, dataByte, 0, item.DataLength);
                        }
                        else//固定长度类型
                        {
                            dataByte = new byte[item.DataLength];
                            Array.Copy(frame, index, dataByte, 0, item.DataLength);
                        }

                        object da = CommFun.CalcuteDataInfo(item, dataByte);
                        data.Add(da);
                        Count++;
                        if (Count < obatt.DataInfo.Count) index = index + item.DataLength + 1;//最后一次后，不执行
                        FrameOadIndex = index + item.DataLength;
                    }
                    T = true;
                }
            }
            else
            {
                return false;
            }
            try
            {
                ///解析跟随上报数据
                if (FrameOadIndex < frame.Length && frame[FrameOadIndex] == 0x01)//跟随上报信息域
                {
                    for (int i = 0; i < frame[FrameOadIndex + 2]; i++)
                    {
                        if (!ParseReadNormal2(frame, ref FrameOadIndex, ref reportdata))//FrameOadIndex:每个oad 索引
                        {
                            return false;
                        }
                    }
                }
            }
            catch (Exception)
            { }

            return T;
        }

        /// <summary>
        /// 解析读取返回数据
        /// </summary>
        /// <param name="frame">帧</param>
        /// <param name="data">数据内容</param>
        /// <returns>执行成功与否</returns>
        public override bool ParseReadFrame(byte[] frame, ref List<object> data, ref Dictionary<string, List<object>> DicObj)
        {
            EmGetRequestMode mode = (EmGetRequestMode)frame[1];
            OAD = new List<string>();
            CSD = new List<string>();
            int FrameOadIndex = 3;
            if (mode == EmGetRequestMode.GetRequestNormal)
            {
                return ParseReadNormal(frame, ref FrameOadIndex, ref data);
            }
            else if (mode == EmGetRequestMode.GetRequestNormalList)//一次读取多个对象
            {
                FrameOadIndex += 1;  //读取多个对象包含OAD个数的域 
                for (int i = 0; i < frame[3]; i++)//OAD个数
                {
                    if (!ParseReadNormal(frame, ref FrameOadIndex, ref data))//FrameOadIndex:每个oad 索引
                    {
                        return false;
                    }
                }
                return true;
            }
            else if (mode == EmGetRequestMode.GetRequestRecord)
            {
                byte[] oad = new byte[4];
                //byte[] dataByte;
                byte[] csdByte;
                //int Count = 0;

                Array.Copy(frame, FrameOadIndex, oad, 0, 4);
                OAD.Add(CommFun.BytesToHexStr(oad));

                int csdLen = frame[FrameOadIndex + 4];      //csd长度
                csdByte = new byte[csdLen * 5];
                Array.Copy(frame, FrameOadIndex + 5, csdByte, 0, csdLen * 5);
                string dataStr = CommFun.BytesToHexStr(csdByte);
                for (int i = 0; i < csdLen; i++)
                {
                    string strCsd = dataStr.Substring(i * 10 + 2, 8);
                    CSD.Add(strCsd);

                    data.Add(strCsd);
                    DicObj.Add(strCsd, new List<object>());
                }

                int recordCount = frame[FrameOadIndex + 4 + csdLen * 5 + 2]; //记录数长度

                if (recordCount < 1)
                {
                    DicObj = null;
                    return true;
                }

                int index = FrameOadIndex + 4 + csdLen * 5 + 2 + 1;
                for (int i = 0; i < DicObj.Count; i++)
                {
                    List<object> t = new List<object>();
                    CommFun.GetData(frame, index, ref t, out int l);
                    DicObj[CSD[i]] = t;
                    index += l;
                }
                return true;

                //ObjectAttributes obatt = ObjectInfos.AttributeInfos.Find(e => e.Oad == OAD[0]);
                //if (obatt !=null  && obatt.ItemCount == 1)
                //{

                //    index = FrameOadIndex + 6;
                //    if (obatt.DataInfo[0].LengthFlag)// 可变长度类型
                //    {
                //        index = index + 1;
                //    }
                //    dataByte = new byte[obatt.DataInfo[0].DataLength];
                //    Array.Copy(frame, index, dataByte, 0, obatt.DataInfo[0].DataLength);
                //    object da = CommFun.CalcuteDataInfo(obatt.DataInfo[0], dataByte);
                //    data.Add(da);
                //    FrameOadIndex = index + obatt.DataInfo[0].DataLength;

                //    return true;
                //}
                //else// 数组或结构体类型对象属性，包含多个元素
                //{
                //    index = FrameOadIndex + 8;//数组或结构内部子元素数据内容索引
                //    foreach (var item in obatt.DataInfo)
                //    {
                //        if (item.LengthFlag)// 可变长度类型
                //        {
                //            index = index + 1;
                //            dataByte = new byte[item.DataLength];
                //            Array.Copy(frame, index, dataByte, 0, item.DataLength);
                //        }
                //        else//固定长度类型
                //        {
                //            dataByte = new byte[item.DataLength];
                //            Array.Copy(frame, index, dataByte, 0, item.DataLength);
                //        }

                //        object da = CommFun.CalcuteDataInfo(item, dataByte);
                //        data.Add(da);
                //        Count++;
                //        if (Count < obatt.DataInfo.Count) index = index + item.DataLength + 1;//最后一次后，不执行
                //        FrameOadIndex = index + item.DataLength;
                //    }
                //    return true;
                //}
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 解析数据
        /// </summary>
        /// <param name="frame"></param>
        /// <param name="data"></param>
        /// <param name="frameOadIndex"></param>
        /// <param name="oadIndex"></param>
        /// <returns></returns>
        private bool ParseReadNormal(byte[] frame, ref int frameOadIndex, ref List<object> data)
        {
            byte[] oad = new byte[4];
            int curIndex = frameOadIndex + 4;

            Array.Copy(frame, frameOadIndex, oad, 0, 4); //OAD
            OAD.Add(CommFun.BytesToHexStr(oad));

            if (frame[curIndex] != 0x01) //0x01:有数据，0x00:无数据 
                return false;

            curIndex += 1;
            CommFun.GetData(frame, curIndex, ref data, out int l);
            frameOadIndex += l + 5;
            return true;

        }

        private bool ParseReadNormal2(byte[] frame, ref int frameOadIndex, ref List<object> data)
        {
            byte[] oad = new byte[4];
            int curIndex = frameOadIndex + 7;

            Array.Copy(frame, frameOadIndex + 3, oad, 0, 4); //OAD
            OAD.Add(CommFun.BytesToHexStr(oad));

            if (frame[curIndex] != 0x01) //0x01:有数据，0x00:无数据 
                return false;

            curIndex += 1;
            CommFun.GetData(frame, curIndex, ref data, out int l);
            frameOadIndex += l + 5;
            return true;

        }
        //private void Parse

        ///// <summary>
        ///// 获取报文中指定类型的数据
        ///// </summary>
        ///// <param name="frame">报文</param>
        ///// <param name="typeIndex">指定类型下标</param>
        ///// <param name="data">返回数据</param>
        ///// <returns>返回当前数据及下标的长度和</returns>
        //private int GetData(byte[]frame,int typeIndex,out string data )
        //{
        //    data = "";
        //    if (frame.Length < typeIndex) return 0;

        //    int l = 0;
        //    List<object> list = new List<object>();
        //    CommFun.GetData(frame, typeIndex, ref list, out l);
        //    if (frame[typeIndex] == 0x06) //double_long_unsigned 4bytes
        //        dataLen = 4;
        //    else if (frame[typeIndex] == 0x06) //
        //        dataLen = 4;
        //}
    }
}
