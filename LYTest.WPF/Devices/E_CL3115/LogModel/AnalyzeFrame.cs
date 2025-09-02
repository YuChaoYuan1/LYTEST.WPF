using CLOU.Struct;
using System;
using System.Collections.Generic;
using System.Text;

namespace CLOU.LogModel
{
    public class AnalyzeFrame
    {
        #region 数据定义
        #region Page0的所有Group
        /// <summary>
        /// 第0页第1组 字符串格式：数据编号|数据类型|数组长度|字节数|倍数|说明
        /// </summary>
        private static readonly string[] page0_Group0 = { "D0000|Char|1|12|1|SERIAL", "D0001|Char|1|5|1|Ver", "D0002|Char|1|11|1|DEVICE", "D0003|Char|1|7|1|CLT", "D0004|UINT1|1|1|1|波特率", "D0005|UINT1|1|1|1|接线方式", "D0006|UINT1|6|1|1|校准状态", "D0007|UINT1|1|1|1|交流测量方式" };
        private static readonly string[] page0_Group1 = { "D0010|UINT1|1|4|1|时间(毫秒)", "D0011|UINT1|1|1|1|时间(秒)", "D0012|UINT1|1|1|1|时间(分)", "D0013|UINT1|1|1|1|时间(时)", "D0014|UINT1|1|1|1|时间(日)", "D0015|UINT1|1|1|1|时间(月)", "D0016|UINT1|1|1|1|时间(年)", "" };
        private static readonly string[] page0_Group2 = { "D0020|UINT4|1|4|1|本机脉冲常数1", "D0021|UINT4|1|4|1|本机脉冲常数2", "D0022|UINT4|1|4|1|本机脉冲常数3", "D0023|UINT4|1|4|1|脉冲数", "D0024|UINT1|1|1|1|S值次数", "D0025|UINT1|1|1|1|电能误差计算次数", "D0026|UINT1|1|1|1|FFT数据通道设定", "D0027|UINT2|1|2|1|采样平滑时间(S)" };

        private static readonly string[] page0_Group3 = { "D0030|UINT2|1|2|1|电能1指示", "|||||", "|||||", "|||||", "D0034|UINT1|1|1|1|电能1误差计算启动标志", "|||||", "|||||", "|||||" };
        private static readonly string[] page0_Group4 = { "D0040|SINT8|1|8|1|被检表1表常数", "D0041|SINT8|1|8|1|被检表2表常数", "|||||", "|||||", "|||||", "|||||", "|||||", "D0047|UINT1|1|1|1|ARM版显示界面" };
        private static readonly string[] page0_Group5 = { "|||||", "|||||", "|||||", "|||||", "|||||", "|||||", "|||||", "|||||" };
        private static readonly string[] page0_Group6 = { "|||||", "|||||", "|||||", "|||||", "|||||", "|||||", "|||||", "|||||" };
        private static readonly string[] page0_Group7 = { "|||||", "|||||", "|||||", "|||||", "|||||", "|||||", "|||||", "|||||" };
        #endregion
        #region Page1的所有Group
        private static readonly string[] page1_Group0 = { "", "", "", "", "", "", "", "" };
        private static readonly string[] page1_Group1 = { "", "", "", "", "", "", "", "" };
        private static readonly string[] page1_Group2 = { "", "", "", "", "", "", "", "" };
        private static readonly string[] page1_Group3 = { "", "", "", "", "", "", "", "" };
        private static readonly string[] page1_Group4 = { "", "", "", "", "", "", "", "" };
        private static readonly string[] page1_Group5 = { "", "", "", "", "", "", "", "" };
        private static readonly string[] page1_Group6 = { "", "", "", "", "", "", "", "" };
        private static readonly string[] page1_Group7 = { "", "", "", "", "", "", "", "" };
        #endregion
        #region Page2的所有Group
        /// <summary>
        /// 字符串格式：数据编号|数据类型|数组长度|字节数|倍数|说明
        /// </summary>
        private static readonly string[] page2_Group0 = { "D0200|INT4E1|1|5|1|C相电压幅值", "D0201|INT4E1|1|5|1|B相电压幅值", "D0202|INT4E1|1|5|1|A相电压幅值", "D0203|INT4E1|1|5|1|C相电流幅值", "D0204|INT4E1|1|5|1|B相电流幅值", "D0205|INT4E1|1|5|1|A相电流幅值", "D0206|UINT4|1|4|100000|频率", "D0207|UINT1|1|1|1|档位过载标志" };
        private static readonly string[] page2_Group1 = { "D0210|UINT1|1|1|1|C相电压当前档位", "D0211|UINT1|1|1|1|B相电压当前档位", "D0212|UINT1|1|1|1|A相电压当前档位", "D0213|UINT1|1|1|1|C相电流当前档位", "D0214|UINT1|1|1|1|B相电流当前档位", "D0215|UINT1|1|1|1|A相电流当前档位", "D0216|UINT4|1|4|1|真实常数", "D0217|SINT4|1|4|1|功率相角" };
        private static readonly string[] page2_Group2 = { "D0220|UINT4|1|4|10000|C相电压相位", "D0221|UINT4|1|4|10000|B相电压相位", "D0222|UINT4|1|4|10000|A相电压相位", "D0223|UINT4|1|4|10000|C相电流相位", "D0224|UINT4|1|4|10000|B相电流相位", "D0225|UINT4|1|4|10000|A相电流相位", "", "" };
        private static readonly string[] page2_Group3 = { "D0230|SINT4|1|4|1000|C相角度", "D0231|SINT4|1|4|1000|B相角度", "D0232|SINT4|1|4|1000|A相角度", "D0233|SINT4|1|4|10000|C相功率因数", "D0234|SINT4|1|4|10000|B相功率因数", "D0235|SINT4|1|4|10000|A相功率因数", "D0236|SINT4|1|4|10000|三相总有功率因数", "D0237|SINT4|1|4|10000|三相总无功率因数" };
        private static readonly string[] page2_Group4 = { "D0240|INT4E1|1|5|1|C相总有功功率", "D0241|INT4E1|1|5|1|B相总有功功率", "D0242|INT4E1|1|5|1|A相总有功功率", "D0243|INT4E1|1|5|1|总有功功率", "D0244|INT4E1|1|5|1|C相总无功功率", "D0245|INT4E1|1|5|1|B相总无功功率", "D0246|INT4E1|1|5|1|A相总无功功率", "D0247|INT4E1|1|5|1|总无功功率" };
        private static readonly string[] page2_Group5 = { "D0250|INT4E1|1|5|1|C相视在功率", "D0251|INT4E1|1|5|1|B相视在功率", "D0252|INT4E1|1|5|1|A相视在功率", "D0253|INT4E1|1|5|1|总视在功率", "D0254|SINT8|1|8|1|有功电能", "D0255|SINT8|1|8|1|无功电能", "D0256|SINT8|1|8|1|有功表常数", "D0257|SINT8|1|8|1|无功表常数" };
        private static readonly string[] page2_Group6 = { "", "", "D0262|SINT4|1|4|10000|电能1测量误差1", "", "", "", "", "D0267|UINT8|1|8|1|电能累计脉冲数" };
        private static readonly string[] page2_Group7 = { "", "", "", "", "", "", "", "" };
        #endregion
        #region Page3的所有Group
        private static readonly string[] page3_Group0 = { "D0300|UINT4|64|4|10000|C相电压各次谐波幅值", "D0301|UINT4|64|4|10000|B相电压各次谐波幅值", "D0302|UINT4|64|4|10000|A相电压各次谐波幅值", "D0303|UINT4|64|4|10000|C相电流各次谐波幅值", "D0304|UINT4|64|4|10000|B相电流各次谐波幅值", "D0305|UINT4|64|4|10000|A相电流各次谐波幅值", "", "" };
        private static readonly string[] page3_Group1 = { "", "", "", "", "", "", "", "" };
        private static readonly string[] page3_Group2 = { "", "", "", "", "", "", "", "" };
        private static readonly string[] page3_Group3 = { "", "", "D0332|SINT2|128|2|100|A相电压波形数据", "", "", "D0335|SINT2|128|2|100|B相电压波形数据", "", "" };
        private static readonly string[] page3_Group4 = { "", "", "D0342|SINT2|128|2|100|C相电压波形数据", "", "", "D0345|SINT2|128|2|100|A相电流波形数据", "", "" };
        private static readonly string[] page3_Group5 = { "", "", "D0352|SINT2|128|2|100|B相电流波形数据", "", "", "D0355|SINT2|128|2|100|C相电流波形数据", "", "" };
        private static readonly string[] page3_Group6 = { "", "", "D0262|SINT4|1|4|10000|电能1测量误差1", "", "", "", "", "" };
        private static readonly string[] page3_Group7 = { "", "", "", "", "", "", "", "" };
        #endregion
        #region Page4的所有Group
        private static readonly string[] page4_Group0 = { "", "", "", "", "", "", "", "" };
        private static readonly string[] page4_Group1 = { "", "", "", "", "", "", "", "" };
        private static readonly string[] page4_Group2 = { "", "", "", "", "", "", "", "" };
        private static readonly string[] page4_Group3 = { "", "", "", "", "", "", "", "" };
        private static readonly string[] page4_Group4 = { "", "", "", "", "", "", "", "" };
        private static readonly string[] page4_Group5 = { "", "", "", "", "", "", "", "" };
        private static readonly string[] page4_Group6 = { "", "", "", "", "", "", "", "" };
        private static readonly string[] page4_Group7 = { "", "", "", "", "", "", "", "" };
        #endregion
        #region Page5的所有Group
        private static readonly string[] page5_Group0 = { "", "", "", "", "", "", "", "" };
        private static readonly string[] page5_Group1 = { "", "", "", "", "", "", "", "" };
        private static readonly string[] page5_Group2 = { "", "", "", "", "", "", "", "" };
        private static readonly string[] page5_Group3 = { "", "", "", "", "", "", "", "" };
        private static readonly string[] page5_Group4 = { "", "", "", "", "", "", "", "" };
        private static readonly string[] page5_Group5 = { "", "", "", "", "", "", "", "" };
        private static readonly string[] page5_Group6 = { "", "", "", "", "", "", "", "" };
        private static readonly string[] page5_Group7 = { "", "", "", "", "", "", "", "" };
        #endregion
        #region Page6的所有Group
        private static readonly string[] page6_Group0 = { "", "", "", "", "", "", "", "" };
        private static readonly string[] page6_Group1 = { "", "", "", "", "", "", "", "" };
        private static readonly string[] page6_Group2 = { "", "", "", "", "", "", "", "" };
        private static readonly string[] page6_Group3 = { "", "", "", "", "", "", "", "" };
        private static readonly string[] page6_Group4 = { "", "", "", "", "", "", "", "" };
        private static readonly string[] page6_Group5 = { "", "", "", "", "", "", "", "" };
        private static readonly string[] page6_Group6 = { "", "", "", "", "", "", "", "" };
        private static readonly string[] page6_Group7 = { "", "", "", "", "", "", "", "" };
        #endregion
        #region Page7的所有Group
        private static readonly string[] page7_Group0 = { "", "", "", "", "", "", "", "" };
        private static readonly string[] page7_Group1 = { "", "", "", "", "", "", "", "" };
        private static readonly string[] page7_Group2 = { "", "", "", "", "", "", "", "" };
        private static readonly string[] page7_Group3 = { "", "", "", "", "", "", "", "" };
        private static readonly string[] page7_Group4 = { "", "", "", "", "", "", "", "" };
        private static readonly string[] page7_Group5 = { "", "", "", "", "", "", "", "" };
        private static readonly string[] page7_Group6 = { "", "", "", "", "", "", "", "" };
        private static readonly string[] page7_Group7 = { "", "", "", "", "", "", "", "" };
        #endregion
        #region Page0-Page7
        /// <summary>
        /// 第0页数据
        /// </summary>
        private static Dictionary<int, string[]> doPage0 = new Dictionary<int, string[]>() { { 0, page0_Group0 }, { 1, page0_Group1 }, { 2, page0_Group2 }, { 3, page0_Group3 }, { 4, page0_Group4 }, { 5, page0_Group5 }, { 6, page0_Group6 }, { 7, page0_Group7 } };
        /// <summary>
        /// 第1页数据
        /// </summary>
        private static Dictionary<int, string[]> doPage1 = new Dictionary<int, string[]>() { { 0, page1_Group0 }, { 1, page1_Group1 }, { 2, page1_Group2 }, { 3, page1_Group3 }, { 4, page1_Group4 }, { 5, page1_Group5 }, { 6, page1_Group6 }, { 7, page1_Group7 } };
        /// <summary>
        /// 第2页数据
        /// </summary>
        private static Dictionary<int, string[]> doPage2 = new Dictionary<int, string[]>() { { 0, page2_Group0 }, { 1, page2_Group1 }, { 2, page2_Group2 }, { 3, page2_Group3 }, { 4, page2_Group4 }, { 5, page2_Group5 }, { 6, page2_Group6 }, { 7, page2_Group7 } };
        /// <summary>
        /// 第3页数据
        /// </summary>
        private static Dictionary<int, string[]> doPage3 = new Dictionary<int, string[]>() { { 0, page3_Group0 }, { 1, page3_Group1 }, { 2, page3_Group2 }, { 3, page3_Group3 }, { 4, page3_Group4 }, { 5, page3_Group5 }, { 6, page3_Group6 }, { 7, page3_Group7 } };
        /// <summary>
        /// 第4页数据
        /// </summary>
        private static Dictionary<int, string[]> doPage4 = new Dictionary<int, string[]>() { { 0, page4_Group0 }, { 1, page4_Group1 }, { 2, page4_Group2 }, { 3, page4_Group3 }, { 4, page4_Group4 }, { 5, page4_Group5 }, { 6, page4_Group6 }, { 7, page4_Group7 } };
        /// <summary>
        /// 第5页数据
        /// </summary>
        private static Dictionary<int, string[]> doPage5 = new Dictionary<int, string[]>() { { 0, page5_Group0 }, { 1, page5_Group1 }, { 2, page5_Group2 }, { 3, page5_Group3 }, { 4, page5_Group4 }, { 5, page5_Group5 }, { 6, page5_Group6 }, { 7, page5_Group7 } };
        /// <summary>
        /// 第6页数据
        /// </summary>
        private static Dictionary<int, string[]> doPage6 = new Dictionary<int, string[]>() { { 0, page6_Group0 }, { 1, page6_Group1 }, { 2, page6_Group2 }, { 3, page6_Group3 }, { 4, page6_Group4 }, { 5, page6_Group5 }, { 6, page6_Group6 }, { 7, page6_Group7 } };
        /// <summary>
        /// 第7页数据
        /// </summary>
        private static Dictionary<int, string[]> doPage7 = new Dictionary<int, string[]>() { { 0, page7_Group0 }, { 1, page7_Group1 }, { 2, page7_Group2 }, { 3, page7_Group3 }, { 4, page7_Group4 }, { 5, page7_Group5 }, { 6, page7_Group6 }, { 7, page7_Group7 } };
        /// <summary>
        /// 所有页数据
        /// </summary>
        private static Dictionary<int, Dictionary<int, string[]>> doPages = new Dictionary<int, Dictionary<int, string[]>>() { { 0, doPage0 }, { 1, doPage1 }, { 2, doPage2 }, { 3, doPage3 }, { 4, doPage4 }, { 5, doPage5 }, { 6, doPage6 }, { 7, doPage7 } };
        #endregion
        #region Cmd
        /// <summary>
        /// 所有命令数据
        /// </summary>
        private static Dictionary<string, string> do_Cmds = new Dictionary<string, string>() { { "30", "成功" }, { "33", "出错" }, { "35", "系统忙" }, { "36", "禁止改写数据或存储器内容" }, { "C0", "询问对方是否已准备好接受新的命令" }, { "C3", "询问对方有无新的事件发生，或向对方交接令牌" }, { "C9", "询问设备型号、版本号、产品出厂串号" }, { "39", "回答设备型号、版本号、产品出厂串号" }, { "A0", "询问有关数据或参数" }, { "50", "传送有关数据或参数" }, { "A3", "改写有关数据或参数" }, { "A5", "询问有关数组或参数" }, { "55", "传送有关数组或参数" }, { "A6", "改写有关数组或参数" }, { "A9", "询问存储器内容" }, { "59", "回传存储器内容" }, { "AA", "改写存储器内容" }, { "90", "询问新密钥" }, { "60", "回传新密钥" }, { "93", "出示口令" }, { "95", "询问数据字典" }, { "65", "回传数据字典" } };
        private delegate string Fun();
        /// <summary>
        /// 所有命令函数
        /// </summary>
        private static Dictionary<string, Fun> doFuns = new Dictionary<string, Fun>() { { "30", Fun_30 }, { "33", Fun_33 }, { "A0", Fun_A0 }, { "50", Fun_50 }, { "A3", Fun_A3 }, { "A5", Fun_A5 }, { "55", Fun_55 },{"C9",Fun_C9},{"39",Fun_39} };
        #endregion
        #endregion
        public static int AnalyzeFrames(string[] frames,out string[] frameValues)
        {
            frameValues = null;
            if (frames == null) return 0;
            try
            {
                int len = frames.Length;
                frameValues = new string[len];
                for (int i = 0; i < len; i++)
                {
                    frameValues[i] = Analyze(frames[i]);
                }
            }
            catch
            {
                return -1;
            }
            return 0;
        }
        private static string Analyze(string frames)
        {
            try
            {
                string frameValues = string.Empty;
                frames = frames.Replace(" ", "");
                Frames = frames;
                int len = frames.Length;
                if (len < 6)
                {
                    frameValues = "result:长度不对";
                }
                if (!CheckChkSum())//校验码不对
                {
                    frameValues = "result:校验码不对";
                    return frameValues;
                }
                string top = GetString(1);//帧头
                string toID = GetString(1);//受信节点
                string myID = GetString(1);//发信节点
                len = 0;//帧长度
                int.TryParse(GetString(1), out len);
                string cmd = GetString(1);//命令码
                string content = string.Empty;
                Fun fun = doFuns[cmd];
                content = fun();
                frameValues = string.Format("result:成功;cmd:{0};content:{1}", GetCmdValue(cmd), content);
                return frameValues;
            }
            catch
            {
                return string.Format("result:错误;");
            }
        }
        #region 字符串处理
        private static string Frames;
        private static string GetString(int len)
        {
            len = len * 2;
            string tmp = Frames.Substring(0, len);
            Frames=Frames.Remove(0, len);
            return tmp;
        }
        private static string GetString_S(int len)
        {
            string tmp = GetString(len);
            StringBuilder sb = new StringBuilder();
            for (int i = tmp.Length-2; i >= 0; i -= 2)
            {
                sb.Append(tmp.Substring(i, 2));
            }
            return sb.ToString();
        }
        private static bool CheckChkSum()
        {
            int len = Frames.Length;
            string chkSum = Frames.Substring(len - 2);
            string tmp = Frames.Substring(2, len - 4);
            byte btChkSum = ChkSum.GetChkSumXor(HexToByte(tmp));
            return Convert.ToByte(chkSum,16) == btChkSum;
        }
        private static byte[] HexToByte(string msg)
        {
            msg = msg.Replace(" ", "");
            byte[] comBuffer = new byte[msg.Length / 2];
            for (int i = 0; i < msg.Length; i += 2)
            {
                comBuffer[i / 2] = (byte)Convert.ToByte(msg.Substring(i, 2),16);
            }
            return comBuffer;
        }
        private static string HexTo2(string msg)
        {
            string flag = Convert.ToString(Convert.ToInt32(msg,16), 2).PadLeft(8, '0');
            return flag;
        }
        private static StFrameInfo Split(string msg, char separator)
        {
            string[] value = msg.Split(separator);
            StFrameInfo info = new StFrameInfo();
            info.DataNo = value[0];
            info.DataType = value[1];
            info.ArrayLength =Convert.ToInt32(value[2]);
            info.ByteLength = Convert.ToInt32(value[3]);
            info.Multiple = Convert.ToInt32(value[4]);
            info.Explain = value[5];
            return info;
        }
        #endregion
        #region 各命令处理函数
        private static string Fun_A0()
        {
            StringBuilder value = new StringBuilder();
            string page = GetString(1);
            string group = GetString(1);
            List<string> listValue = GetDataValue(page, group);
            if (listValue != null && listValue.Count > 0)
            {
                foreach (string str in listValue)
                {
                    StFrameInfo info = Split(str, '|');
                    value.Append(info.Explain).Append(",");
                }
            }
            return value.ToString().Substring(0, value.Length-1);
        }
        private static string Fun_50()
        {
            StringBuilder value = new StringBuilder();
            string page = GetString(1);
            string group = GetString(1);
            string tmpGroup = HexTo2(group);
            string tmpData = string.Empty;
            Dictionary<int, string[]> doGroup = doPages[int.Parse(page)];

            for (int i = tmpGroup.Length - 1; i >= 0; i--)
            {
                if (tmpGroup[i] == '1')
                {
                    string data = GetString(1);
                    tmpData = HexTo2(data);
                    string[] doData = doGroup[tmpGroup.Length - 1 - i];
                    for (int j = tmpData.Length - 1; j >= 0; j--)
                    {
                        if (tmpData[j] == '1')
                        {
                            StFrameInfo info = Split(doData[tmpData.Length - 1 - j], '|');
                            value.Append(info.Explain).Append(":").Append(GetValue(info)).Append(",");
                        }
                    }
                }
            }

            //List<string> listValue = GetDataValue(page, group);
            //if (listValue != null && listValue.Count > 0)
            //{
            //    foreach (string str in listValue)
            //    {
            //        StFrameInfo info = Split(str, '|');
            //        value.Append(info.Explain).Append(":").Append(GetValue(info)).Append(",");
            //    }
            //}
            return value.ToString().Substring(0, value.Length - 1);
        }
        private static string Fun_A5()
        {
            StFrameInfo info = GetDataValue();
            int start = GetSInt(2);
            int len = GetSInt(1);
            return string.Format("{0}:数组下标开始:{1},数组长度:{2}", info.Explain, start, len/info.ByteLength);
        }
        private static string Fun_55()
        {
            StringBuilder value = new StringBuilder();
            StFrameInfo info = GetDataValue();
            int start = GetSInt(2);
            int len = GetSInt(1);
            value.Append(string.Format("{0}:数组下标开始:{1},数组长度:{2},数据:", info.Explain, start, len / info.ByteLength));
            for (int i = 0; i < len; i += info.ByteLength)
            {
                value.Append(GetValue(info)).Append(",");
            }
            return value.ToString().Substring(0, value.Length - 1);
        }
        private static string Fun_A3()
        {
            return Fun_50();
        }
        private static string Fun_30()
        {
            return "成功";
        }
        private static string Fun_33()
        {
            return "出错";
        }
        private static string Fun_C9()
        {
            return "询问设备型号、版本号、产品出厂串号";
        }
        public static string Fun_39()
        {
            string cltNo=GetChar(7);
            string deviceNo = GetChar(11);
            string verNo = GetChar(5);
            string number = GetChar(12);
            return string.Format("{0}{1}{2}{3}", cltNo, deviceNo, verNo, number);
        }
        #endregion
        #region
        private static List<string> GetDataValue(string page,string group)
        {
            string tmpGroup = HexTo2(group);
            string tmpData = string.Empty;
            Dictionary<int, string[]> doGroup = doPages[int.Parse(page)];
            List<string> listValue = new List<string>();
            for (int i = tmpGroup.Length - 1; i >= 0; i--)
            {
                if (tmpGroup[i] == '1')
                {
                    string data = GetString(1);
                    tmpData = HexTo2(data);
                    string[] doData = doGroup[tmpGroup.Length - 1-i];
                    for (int j = tmpData.Length-1; j >= 0; j--)
                    {
                        if (tmpData[j] == '1')
                        {
                            listValue.Add(doData[tmpData.Length-1-j]);
                        }
                    }
                }
            }
            return listValue;
        }
        private static StFrameInfo GetDataValue()
        {
            int page = GetSInt(1);
            int arry = GetSInt(1);
            int group = arry / 8;
            int data = arry % 8;
            Dictionary<int, string[]> doGroup = doPages[page];
            string[] doData = doGroup[group];
            StFrameInfo info = Split(doData[data], '|');
            return info;
        }
        private static string GetCmdValue(string key)
        {
            string value = string.Empty;
            if (do_Cmds.ContainsKey(key))
            {
                return do_Cmds[key];
            }
            return value;
        }
        #endregion
        #region 数据类型处理
        private static string GetValue(StFrameInfo info)
        {
            string value = string.Empty;
            switch (info.DataType.ToLower())
            { 
                case "uint1":
                case "uint2":
                case "uint3":
                case "uint4":
                    UInt32 uInt = GetUInt(info.ByteLength);
                    float fUInt = 0f;
                    if (info.Multiple > 1) fUInt = Convert.ToSingle(uInt / info.Multiple);
                    else fUInt = uInt;
                    value = fUInt.ToString();
                    break;
                case "sint1":
                case "sint2":
                case "sint3":
                case "sint4":
                    int sInt = GetSInt(info.ByteLength);
                    float fSInt = 0f;
                    if (info.Multiple > 1) fSInt = Convert.ToSingle(sInt / info.Multiple);
                    else fSInt = sInt;
                    value = fSInt.ToString();
                    break;
                case "char":
                    value = GetChar(info.ByteLength);
                    break;
                case "int4e1":
                    float int4E1 = GetInt4E1(info.ByteLength);
                    if (info.Multiple > 1) int4E1 = Convert.ToSingle(int4E1 / info.Multiple);
                    value = int4E1.ToString();
                    break;
                case "uint8":
                    UInt64 uInt8 = GetUInt8(info.ByteLength);
                    if (info.Multiple > 1) uInt8 = Convert.ToUInt64(uInt8 / (UInt32)info.Multiple);
                    value = uInt8.ToString();
                    break;
                case "sint8":
                    long sInt8 = GetSInt8(info.ByteLength);
                    if (info.Multiple > 1) sInt8 = Convert.ToInt64(sInt8 / info.Multiple);
                    value = sInt8.ToString();
                    break;
            }
            return value;
        }
        private static UInt32 GetUInt(int len)
        {
            string value = GetString_S(len);
            UInt32 uValue = Convert.ToUInt32(value, 16);
            return uValue;
        }
        private static UInt64 GetUInt8(int len)
        {
            string value = GetString_S(len);
            UInt64 uValue = Convert.ToUInt64(value, 16);
            return uValue;
        }
        private static int GetSInt(int len)
        {
            string value = GetString_S(len);
            int sValue = Convert.ToInt32(value, 16);
            return sValue;
        }
        private static long GetSInt8(int len)
        {
            string value = GetString_S(len);
            long sValue = Convert.ToInt64(value, 16);
            return sValue;
        }
        private static float GetInt4E1(int len)
        {
            string value = GetString_S(4);
            sbyte e = Convert.ToSByte(GetString(1),16);
            return Convert.ToSingle(Convert.ToInt32(value,16) * Math.Pow(10, e));
        }
        private static string GetChar(int len)
        {
            string value = GetString(len);
            StringBuilder sb=new StringBuilder();
            for (int i = 0; i < value.Length; i += 2)
            {
                sb.Append(Char.ConvertFromUtf32(Convert.ToInt32(value.Substring(i, 2),16)));
            }
            return sb.ToString();
        }
        #endregion
    }
}
