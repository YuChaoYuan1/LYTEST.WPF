using LYTest.Core.Enum;
using LYTest.Core;
using LYTest.Core.Model.Meter;
using LYTest.Core.Struct;
using LYTest.ViewModel;
using LYTest.ViewModel.CheckController;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LYTest.Verify.HEBVerify
{
    /// <summary>
    /// 参数验证
    /// 只记录当前方向结论   --  未完成 --读取或写入完成后处理结论,没有写入内容，没有698
    /// </summary>
    public class ParameterValidation : VerifyBase
    {
        string ItemId = string.Empty;

        //string DataName = ""; //数据项名称
        //string BsCode = ""; //标识编码
        //int Len = 0; //长度
        //int DecimalLen = 0; // 小数位数
        //string DataFormat = ""; //数据格式
        //string DXGN = "读"; //功能读还是写
        //string TipsStr = ""; //写入示例
        /// <summary>
        /// 重写方案转换
        /// </summary>
        private StPlan_ConnProtocol CurPlan;


        public override void Verify()
        {
            base.Verify();
            for (int i = 0; i < MeterNumber; i++)
            {
                //if (!meterInfo[i].YaoJianYn) continue;
                ResultDictionary["当前项目"][i] = CurPlan.Name;
                ResultDictionary["项目编号"][i] = CurPlan.PrjID;
                ResultDictionary["设定值"][i] = CurPlan.WriteContent;
                ResultDictionary["数据标识"][i] = CurPlan.Code698;
                ResultDictionary["子项目编号"][i] = CurPlan.ChildrenItemId;
            }

            RefUIData("当前项目");
            RefUIData("项目编号");
            RefUIData("设定值");
            RefUIData("数据标识");
            RefUIData("子项目编号");


            if (EquipmentData.StdInfo.Ua < 40)
            {
                if (!PowerOn())
                {
                    //MessageAdd("升源失败,退出检定", EnumLogType.提示信息);
                    return;
                }
                WaitTime("升源成功", 5);
                MessageAdd("开始检定", EnumLogType.提示信息);
            }

            MessageAdd($"【{CurPlan.Name}】开始检定",EnumLogType.提示信息);
            MessageAdd("开始检定", EnumLogType.提示信息);
            if (Stop) return;
            UpdateMeterProtocol();//更新电表命令
            if (!IsDemo)
            {
                if (OneMeterInfo.MD_MeterNo == null || OneMeterInfo.MD_MeterNo == "" || OneMeterInfo.MD_PostalAddress == null || OneMeterInfo.MD_PostalAddress == "")   //没有表号的情况获取一下
                {
                    //MessageAdd("正在获取所有表的表地址", EnumLogType.提示信息);
                    //ReadMeterAddrAndNo();
                }
            }

            if (Stop) return;
            MessageAdd($"【{CurPlan}】开始检定", EnumLogType.提示信息);
            if (CurPlan.OperType == "读")
            {
                string[] readdata = new string[MeterNumber];
                if (IsDemo)
                {
                    for (int i = 0; i < MeterNumber; i++)
                        readdata[i] = " ";
                }
                else
                {
                    if (CurPlan.Name == "身份认证有效时长")
                    {
                        Identity();
                    }
                    readdata = MeterProtocolAdapter.Instance.ReadData(CurPlan.Name);
                }
                DoReadConnProtocolData(readdata);
            }
            else
            {
                if (Stop) return;
                ReadMeterAddrAndNo();
                if (Stop) return;
                Identity(false);
                if (Stop) return;
                string writedata = "";
                string[] arrData = new string[MeterNumber];

                if (OneMeterInfo.DgnProtocol.ClassName == "CDLT6452007")
                {

                    if (CurPlan.Name == "第一套时区表数据" || CurPlan.Name == "第二套时区表数据" ||
                        CurPlan.Name == "第一套第1日时段数据" || CurPlan.Name == "第一套第2日时段数据" || CurPlan.Name == "第一套第3日时段数据" || CurPlan.Name == "第一套第4日时段数据" ||
                        CurPlan.Name == "第二套第1日时段数据" || CurPlan.Name == "第二套第2日时段数据" || CurPlan.Name == "第二套第3日时段数据" || CurPlan.Name == "第二套第4日时段数据")
                    {
                        writedata = CurPlan.WriteContent;
                        while (writedata.Length < CurPlan.DataLen * 2)
                        {
                            writedata += writedata.Substring(writedata.Length - 6, 6);
                        }
                    }
                    else if (CurPlan.Name == "通信地址" || CurPlan.Name == "表号" || CurPlan.Name == "资产编号")
                    {
                        for (int i = 0; i < MeterNumber; i++)
                        {
                            TestMeterInfo meter = MeterInfo[i];
                            if (!meter.YaoJianYn) continue;

                            string cont = CurPlan.WriteContent;
                            int index = CurPlan.WriteContent.IndexOf("按照条码号后");
                            if (index != -1)
                            {
                                int sub = int.Parse(CurPlan.WriteContent.Replace("按照条码号后", "").Replace("位设置", ""));
                                if (meter.MD_BarCode.Length > sub)
                                    cont = meter.MD_BarCode.Substring(meter.MD_BarCode.Length - sub, sub).PadLeft(12, '0');
                            }
                            if (CurPlan.WriteContent.IndexOf("按照条码设置") != -1)
                                cont = meter.MD_BarCode;

                            arrData[i] = cont;
                        }
                    }
                    else if (CurPlan.Name == "第二套阶梯电价" || CurPlan.Name == "第一套阶梯电价")
                    {
                        string[] ss = CurPlan.WriteContent.Split(',');
                        writedata = "";
                        foreach (string s in ss)
                        {
                            writedata += (float.Parse(s) * 10000).ToString().PadLeft(8, '0');
                        }
                    }
                    else if (CurPlan.Name == "第二套费率电价" || CurPlan.Name == "第一套费率电价")
                    {
                        string[] ss = CurPlan.WriteContent.Split(',');
                        writedata = "";
                        foreach (string s in ss)
                        {
                            writedata += (float.Parse(s) * 10000).ToString().PadLeft(8, '0');
                        }
                        int len = 32;   //数据块是32个
                        if (ss.Length < len && ss.Length > 0)//补全32个
                        {
                            for (int i = 0; i < len - ss.Length; i++)
                            {
                                writedata += (float.Parse(ss[ss.Length - 1]) * 10000).ToString().PadLeft(8, '0');
                            }
                        }

                    }
                    else
                    {
                        int intLen = CurPlan.DataLen;
                        if (CurPlan.Name.IndexOf("自动循环显示第") == 0 || CurPlan.Name.IndexOf("按键循环显示第") == 0) //自动循环显示第1屏,按键循环显示第1屏
                        {
                            intLen = 5;
                            writedata = FormatWriteData(CurPlan.WriteContent, CurPlan.StrDataType, intLen, CurPlan.PointIndex);
                            writedata = writedata.Substring(2, 8) + writedata.Substring(0, 2);
                        }
                        else
                        {
                            writedata = FormatWriteData(CurPlan.WriteContent, CurPlan.StrDataType, intLen, CurPlan.PointIndex);
                        }
                    }

                    if (CurPlan.Name == "第一套费率电价")   //第一套不能写，需要先把他切换成备用套
                    {
                        DateTime dateGPS = DateTime.Now.AddDays(-7);//写入之前的时间
                        MeterProtocolAdapter.Instance.WriteDateTime(dateGPS);
                    }
                    //MessageAdd("设置" + CurPlan.Name, EnumLogType.提示信息);
                    if (CurPlan.Name == "通信地址" || CurPlan.Name == "表号" || CurPlan.Name == "资产编号")
                        MeterProtocolAdapter.Instance.WriteData(CurPlan.Name, arrData);
                    else
                        MeterProtocolAdapter.Instance.WriteData(CurPlan.Name, writedata);

                    if (CurPlan.Name == "第一套费率电价")   //第一套不能写，需要先把他切换成备用套
                    {
                        DateTime dateGPS = DateTime.Now;//恢复时间
                        MeterProtocolAdapter.Instance.WriteDateTime(dateGPS);
                    }

                }
                else if (OneMeterInfo.DgnProtocol.ClassName == "CDLT698")
                {
                    MessageAdd("写" + CurPlan.Name, EnumLogType.提示信息);
                    MeterProtocolAdapter.Instance.WriteData(CurPlan.Name, CurPlan.WriteContent);
                }

                MessageAdd("读取" + CurPlan.Name, EnumLogType.提示信息);
                string[] readdata = MeterProtocolAdapter.Instance.ReadData(CurPlan.Name);

                DoReadConnProtocolData(readdata);
            }

            //CheckOver = true;

        }


        /// <summary>
        /// 检定参数是否合法
        /// </summary>
        /// <returns></returns>
        protected override bool CheckPara()
        {
            string[] tem = Test_Value.Split('|');
            if (tem.Length != 12) return false;
            CurPlan = new StPlan_ConnProtocol();
            int t = 0;


            CurPlan.Name = tem[0];            //数据项名称
            if (CurPlan.Name == "第一套时区表")
            {
                CurPlan.Name = "第一套时区表数据";//处理名称不规范部分
            }
            //CurPlan.Code645 = tem[1];          //标识编码645
            CurPlan.Code698 = tem[1];          //标识编码698

            int.TryParse(tem[2], out t);
            CurPlan.DataLen = t;      //长度
            t = 0;
            int.TryParse(tem[3], out t);
            CurPlan.PointIndex = t;          // 小数位数
            CurPlan.StrDataType = tem[4];    //数据格式
            CurPlan.OperType = tem[5];       //功能读还是写
            CurPlan.WriteContent = tem[6];
            if (CurPlan.Code645 == "")
            {
                return false;
            }

            CurPlan.PrjID = tem[7];//项目编号

            CurPlan.ChildrenItemId = tem[11];//子项目编号


            ResultNames = new string[] { "当前项目", "结论", "项目编号", "数据标识", "设定值","读取值", "子项目编号" };

            //判断是不是双协议的电表，是的话强制使用645协议进行
            if (IsDoubleProtocol)
            {
                string ProtocalName = "CDLT6452007";
                TemClassName = new string[MeterNumber];
                for (int i = 0; i < MeterNumber; i++)
                {
                    if (!MeterInfo[i].YaoJianYn) continue;
                    TemClassName[i] = MeterInfo[i].MD_ProtocolName;//保存旧的协议
                    MeterInfo[i].MD_ProtocolName = ProtocalName;  //修改协议进行加密解密
                }
            }
            return true;
        }
        string[] TemClassName;

        /// <summary>
        /// 格式化写字符串
        /// </summary>
        /// <param name="data"></param>
        /// <param name="format"></param>
        /// <param name="len"></param>
        /// <param name="pointindex"></param>
        /// <returns></returns>
        private string FormatWriteData(string data, string format, int len, int pointindex)
        {
            if (data == "" || data == null) return "";
            string formatdata = data;
            bool isNum = true;           //判断读取的数据是不是数字
            List<char> splitChar = new List<char>(new char[] { '.', 'N' });
            for (int i = 0; i < format.Length; i++)
            {
                if (!splitChar.Contains(format[i]))
                {
                    isNum = false;
                    break;
                }
            }
            if (pointindex != 0)
            {
                if (isNum)
                {
                    int left = len * 2 - pointindex;
                    int right = pointindex;
                    formatdata = float.Parse(formatdata).ToString();
                    string[] newdata = formatdata.Split('.');
                    if (newdata.Length == 1)
                    {
                        if (newdata[0].Length <= left)
                        {
                            newdata[0] = newdata[0].PadLeft(left, '0');
                        }
                        else
                        {
                            newdata[0] = newdata[0].Substring(0, left);
                        }
                        formatdata = newdata[0] + "".PadRight(right, '0');
                    }
                    else
                    {
                        if (newdata[0].Length <= left)
                        {
                            newdata[0] = newdata[0].PadLeft(left, '0');
                        }
                        else
                        {
                            newdata[0] = newdata[0].Substring(0, left);
                        }
                        if (newdata[1].Length <= right)
                        {
                            newdata[1] = newdata[1].PadRight(right, '0');
                        }
                        else
                        {
                            newdata[1] = newdata[1].Substring(0, right);
                        }
                        formatdata = newdata[0] + newdata[1];
                    }
                }
                else
                {
                    formatdata = formatdata.Replace(".", "");
                    formatdata = formatdata.Replace("-", "");
                    if (formatdata.Length <= len * 2)
                    {
                        formatdata = formatdata.PadRight(len * 2, '0');
                    }
                    else
                    {
                        formatdata = formatdata.Substring(0, len * 2);
                    }
                }
            }
            else
            {
                if (formatdata.Length <= len * 2)
                {
                    formatdata = formatdata.PadLeft(len * 2, '0');
                }
                else
                {
                    formatdata = formatdata.Substring(0, len * 2);
                }
            }

            return formatdata;
        }

        private bool IsNumber(string str)
        {
            return System.Text.RegularExpressions.Regex.IsMatch(str, @"^[+-]?\d*[.]?\d*$");
        }

        /// <summary>
        /// 执行读的结果
        /// </summary>
        protected void DoReadConnProtocolData(string[] readdata)
        {

            try
            {
                //结论数据
                for (int i = 0; i < MeterNumber; i++)
                {
                    if (Stop) return;
                    TestMeterInfo meter = MeterInfo[i];
                    if (!meter.YaoJianYn) continue;
                    if (readdata[i] == null) {
                        
                        readdata[i] = "";
                        ResultDictionary["结论"][i] =  ConstHelper.不合格;
                    } 
                    //挂数据
                    if ((CurPlan.Name == "资产编号" || CurPlan.Code645 == "04000403") && meter.DgnProtocol.ClassName == "CDLT6452007") //资产管理编码
                    {
                        if (readdata[i].Length > 0)
                            readdata[i] = GetASCII(readdata[i]);
                    }
                    if (CurPlan.Code645 != null)
                    {

                        if (CurPlan.Code645 == "040604FF" || CurPlan.Code645 == "040605FF")
                        {
                            if (readdata[i].Length > 0)
                                readdata[i] = readdata[i].Replace(".", "");
                        }
                        if (CurPlan.Code645.IndexOf("04000503") != -1 || CurPlan.Code645.IndexOf("04000501") != -1) //电能表运行状态字
                        {
                            if (readdata[i] == null || readdata[i] == "") continue;

                            byte run = Convert.ToByte(readdata[i].Substring(0, 2), 16);
                            string tmp = Convert.ToString(run, 2);
                            tmp = tmp.PadLeft(8, '0');
                            if (readdata[i].Length / 2 > 0)
                            {
                                run = Convert.ToByte(readdata[i].Substring(2, 2), 16);

                                string tmp1 = Convert.ToString(run, 2);
                                tmp1 = tmp1.PadLeft(8, '0');
                                tmp += tmp1;
                            }
                            readdata[i] = tmp;
                        }
                        //if (this.CurPlan.Code645 == "04050101" || this.CurPlan.Code645 == "04050102" || this.CurPlan.Code645 == "04050103" || this.CurPlan.Code645 == "04050104" || this.CurPlan.Code645 == "04050201" || this.CurPlan.Code645 == "04050202" || this.CurPlan.Code645 == "04050203" || this.CurPlan.Code645 == "04050204" )


                        if (CurPlan.Code645 == "070001FF") //资产管理编码
                        {
                            if (readdata[i].Length == 0)
                                readdata[i] = "0005";
                        }
                    }
                    //if (CurPlan.Name.IndexOf("两套时区表切换时间") != -1 || CurPlan.Name.IndexOf("两套日时段表切换时间") != -1)
                    //{
                    //    ResultDictionary["检定信息"][i] = "00000000000000";
                    //}
                    //else {

                        ResultDictionary["读取值"][i] = readdata[i].ToString();
                    //}

                    
                    

                    if (CurPlan.Name.IndexOf("第一套费率电价") != -1 || CurPlan.Name.IndexOf("第二套费率电价") != -1)
                    {
                        if (readdata[i].Length == 9)
                        {
                            readdata[i] = double.Parse(readdata[i]).ToString();
                        }
                    }
                    ResultDictionary["结论"][i] = (readdata[i] != "" && readdata[i] != "-1") ? ConstHelper.合格 : ConstHelper.不合格;
                  

                    if (ResultDictionary["结论"][i] == ConstHelper.不合格)
                    {
                        NoResoult[i] = "没有读取到数据";
                    }

                    if (CurPlan.WriteContent != null && ResultDictionary["结论"][i] == ConstHelper.合格)
                    {
                        if (CurPlan.WriteContent.Length > 0)
                        {
                            string strContent = CurPlan.WriteContent;
                            int intIndex = CurPlan.WriteContent.IndexOf("按照条码号后");
                            if (intIndex != -1)
                            {
                                int intSub = int.Parse(CurPlan.WriteContent.Replace("按照条码号后", "").Replace("位设置", ""));
                                if (meter.MD_BarCode.Length > intSub)
                                    strContent = meter.MD_BarCode.Substring(meter.MD_BarCode.Length - intSub, intSub).PadLeft(12, '0');
                            }
                            if (CurPlan.WriteContent.IndexOf("按照条码设置") != -1)
                                strContent = meter.MD_BarCode;
                            if (CurPlan.Code645 != null)
                            {
                                if (CurPlan.Code645 == "04010000" || CurPlan.Code645 == "04020000" || CurPlan.Code645 == "04010001" || CurPlan.Code645 == "04010002" || CurPlan.Code645 == "04010003" || CurPlan.Code645 == "04010004"
                                 || CurPlan.Code645 == "04020001" || CurPlan.Code645 == "04020002" || CurPlan.Code645 == "04020003" || CurPlan.Code645 == "04020004")
                                {
                                    ResultDictionary["结论"][i] = (readdata[i].IndexOf(strContent) == 0) ? ConstHelper.合格 : ConstHelper.不合格;
                                    if (ResultDictionary["结论"][i] == ConstHelper.不合格)
                                    {
                                        readdata[i] = this.GetPeriodInfo(readdata[i]);
                                        ResultDictionary["结论"][i] = (readdata[i].IndexOf(strContent) == 0) ? ConstHelper.合格 : ConstHelper.不合格;
                                    }

                                }
                                else if (this.CurPlan.Code645.IndexOf("04000503") != -1)
                                {
                                    string a = readdata[i].Substring(15, 1);
                                    string b = strContent.Substring(5, 1);
                                    string a2 = readdata[i].Substring(10, 1);
                                    string b2 = strContent.Substring(12, 1);
                                    string a3 = readdata[i].Substring(3, 1);
                                    string b3 = strContent.Substring(20, 1);
                                    ResultDictionary["结论"][i] = (a == b && a2 == b2 && a3 == b3) ? ConstHelper.合格 : ConstHelper.不合格;
                                }
                                else if (this.CurPlan.Code645.IndexOf("04000501") != -1)
                                {
                                    ResultDictionary["结论"][i] = (readdata[i].Substring(13, 1) == strContent.Substring(5, 1)) ? ConstHelper.合格 : ConstHelper.不合格;
                                }
                                else if (this.CurPlan.Code645.IndexOf("040401") != -1 || this.CurPlan.Code645.IndexOf("040402") != -1)//循环显示的
                                {
                                    ResultDictionary["结论"][i] = (readdata[i] == strContent) ? ConstHelper.合格 : ConstHelper.不合格;
                                    if (ResultDictionary["结论"][i] == ConstHelper.不合格)
                                    {
                                        //12345678,12345678,01
                                        if (readdata[i].Length > 0)
                                        {
                                            string[] str = readdata[i].Split(',');
                                            if (str.Length > 1)
                                            {
                                                str[str.Length - 1] = str[str.Length - 1].PadLeft(2, '0');
                                            }
                                            //写入内容
                                            string[] str2 = subStringByCount(CurPlan.WriteContent.Replace(",", ""), 8);
                                            if (str2.Length > 1)
                                            {
                                                str2[str2.Length - 1] = str2[str2.Length - 1].PadLeft(2, '0');
                                            }
                                            if (string.Join(",", str) == string.Join(",", str2))
                                            {
                                                ResultDictionary["结论"][i] = ConstHelper.合格;
                                            }
                                        }
                                    }

                                }
                            }
                       
                            else
                            {
                                ResultDictionary["结论"][i] = (readdata[i] == strContent) ? ConstHelper.合格 : ConstHelper.不合格;
                            }

                            if (this.CurPlan.Code645 == "04001004" || this.CurPlan.Code645 == "04001005" || (this.CurPlan.Code645 == "04001003" | this.CurPlan.Code645 == "04001002") || this.CurPlan.Code645 == "04001001")
                            {
                                if (readdata[i].Length == 9)
                                {
                                    readdata[i] = readdata[i].Substring(0, 6) + "." + readdata[i].Substring(7, 2);
                                    double num3 = Convert.ToDouble(readdata[i]);
                                    double num4 = Convert.ToDouble(this.CurPlan.WriteContent);
                                    ResultDictionary["结论"][i] = (num3 - num4 == 0.0) ? ConstHelper.合格 : ConstHelper.不合格;
                                }
                            }

                            if (ResultDictionary["结论"][i] == ConstHelper.不合格)
                            {
                                try
                                {
                                    ResultDictionary["结论"][i] = string.Format(readdata[i], this.CurPlan.StrDataType) == strContent ? ConstHelper.合格 : ConstHelper.不合格; ;
                                    if (CurPlan.Name.IndexOf("两套时区表切换时间") != -1 || CurPlan.Name.IndexOf("两套日时段表切换时间") != -1)
                                    {

                                        if (readdata[i] == "00000000000000")
                                        {
                                            ResultDictionary["结论"][i] = ConstHelper.合格;
                                        }

                                    }
                                }
                                catch (Exception)
                                {

                                    throw;
                                }
                            }
                            if (ResultDictionary["结论"][i] == ConstHelper.不合格)
                            {
                                if (CurPlan.Name.IndexOf("第一套费率电价") != -1 || CurPlan.Name.IndexOf("第二套费率电价") != -1)
                                {
                                    ResultDictionary["结论"][i] = CurPlan.WriteContent.Replace(".", "").Replace(",", "") == readdata[i] ? ConstHelper.合格 : ConstHelper.不合格;
                                }
                                else if (CurPlan.Name.IndexOf("两套费率电价切换时间") != -1 && CurPlan.OperType == "写")
                                {
                                    ResultDictionary["结论"][i] = readdata[i] == "000000000000FF" || readdata[i] == "0000000000" ? ConstHelper.合格 : ConstHelper.不合格;
                                }
                            }
                            if (ResultDictionary["结论"][i] == ConstHelper.不合格)
                            {
                                if (IsNumber(readdata[i]) && IsNumber(strContent))
                                {
                                    double num3 = Convert.ToDouble(readdata[i]);
                                    double num4 = Convert.ToDouble(this.CurPlan.WriteContent);
                                    ResultDictionary["结论"][i] = (num3 - num4 == 0.0) ? ConstHelper.合格 : ConstHelper.不合格;
                                }
                            }

                            if (ResultDictionary["结论"][i] == ConstHelper.不合格)
                            {
                                if (CurPlan.Name.IndexOf("日期时间") != -1 && CurPlan.OperType == "读")
                                {
                                    DateTime dateTime = DateTime.Now;
                                    DateTime GetTime = new DateTime(Convert.ToInt32(readdata[i].Substring(0, 4)), Convert.ToInt32(readdata[i].Substring(4, 2)), Convert.ToInt32(readdata[i].Substring(6, 2)),Convert.ToInt32(readdata[i].Substring(8, 2)), Convert.ToInt32(readdata[i].Substring(10, 2)), Convert.ToInt32(readdata[i].Substring(12, 2)));
                                    double SetNum = Convert.ToDouble(CurPlan.WriteContent);
                                    if (dateTime>GetTime.AddSeconds(SetNum)|| dateTime < GetTime.AddSeconds(-SetNum)) {
                                        ResultDictionary["结论"][i] = ConstHelper.不合格;
                                        ResultDictionary["读取值"][i] = GetTime.ToString();
                                    }
                                    else
                                    {
                                        ResultDictionary["结论"][i] = ConstHelper.合格;
                                        ResultDictionary["读取值"][i] = GetTime.ToString();
                                    }
                                }
                            }
                        }
                    }
                }
                RefUIData("读取值");
                RefUIData("结论");
               
            }
            catch (Exception ex)
            {

                MessageAdd(ex.ToString(), EnumLogType.错误信息);
                TryStopTest();
            }




            if (IsDoubleProtocol) //双协议电表
            {
                for (int i = 0; i < MeterNumber; i++)
                {
                    if (!MeterInfo[i].YaoJianYn) continue;
                    if (string.IsNullOrEmpty(TemClassName[i])) continue;
                    MeterInfo[i].MD_ProtocolName = TemClassName[i];  //恢复成原来的协议
                }
                UpdateMeterProtocol();//每次开始检定，更新一下电表协议
            }
        }

        public string[] subStringByCount(string text, int count)
        {
            int start_index = 0;//开始索引
            int end_index = count - 1;//结束索引

            double count_value = 1.0 * text.Length / count;
            double newCount = Math.Ceiling(count_value);//向上取整，只有有小数就取整，比如3.14，结果4

            List<string> list = new List<string>();
            for (int i = 0; i < newCount; i++)
            {
                //如果end_index大于字符长度，则添加剩下字符串
                if (end_index > text.Length - 1)
                {
                    list.Add(text.Substring(start_index));
                    break;
                }
                else
                {
                    list.Add(text.Substring(start_index, count));

                    start_index += count;
                    end_index += count;
                }
            }
            return list.ToArray();
        }
        private string GetPeriodInfo(string strValue)
        {
            string text = "";
            while (strValue.Length >= 6)
            {
                string str = strValue.Substring(strValue.Length - 6, 6);
                text += str;
                strValue = strValue.Substring(0, strValue.Length - 6);
            }
            return text;
        }
    }
}
