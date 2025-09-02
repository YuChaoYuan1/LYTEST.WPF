using LYTest.Core.Enum;
using LYTest.Core.Function;
using LYTest.ViewModel.CheckController;
using System;
using System.Collections.Generic;
using System.Threading;
using LYTest.MeterProtocol.Protocols.DLT698.Enum;

namespace LYTest.Verify.Function
{

    /// <summary>
    /// 最大需量功能试验
    /// add lsj 20220725
    /// </summary>
    class FC_MaxDemand : VerifyBase
    {
        private string[] PChar = null;
        private string[] Q1Char = null;
        private string[] Q2Char = null;
        private string[] arrFreeze = null;

        public override void Verify()
        {
            base.Verify();
            string[] param = Test_Value.Split('|');

            PChar = new string[MeterNumber];
            Q1Char = new string[MeterNumber];
            Q2Char = new string[MeterNumber];
            arrFreeze = new string[MeterNumber];

            //初始化设备
            if (!InitEquipment()) return;

            if (Stop) return;
            ReadMeterAddrAndNo();

            if (Stop) return;
            Identity();

            if (Stop) return;
            MessageAdd("恢复电表时间", EnumLogType.提示信息);


            MessageAdd("开始读取GPS时间...", EnumLogType.提示信息);
            DateTime readTime = DateTime.Now; //读取GPS时间
            MeterProtocolAdapter.Instance.WriteDateTime(readTime);

            if (Stop) return;
            MessageAdd("读取每月第1结算日", EnumLogType.提示信息);
            arrFreeze = MeterProtocolAdapter.Instance.ReadData("每月第1结算日");

            if (Stop) return;
            MessageAdd("读取最大需量周期", EnumLogType.提示信息);
            string[] arrDemand = MeterProtocolAdapter.Instance.ReadData("最大需量周期");

            if (Stop) return;
            Identity(false);

            if (Stop) return;
            MessageAdd("设置最大需量周期", EnumLogType.提示信息);
            MeterProtocolAdapter.Instance.WriteData("最大需量周期", "05");

            string time = arrFreeze[FirstIndex]; //每月第1结算日
            for (int i = 0; i < MeterNumber; i++)
            {
                if (!MeterInfo[i].YaoJianYn) continue;
                if (arrFreeze[i] == null || arrFreeze[i] == "") continue;

                if (time != arrFreeze[i])
                {
                    MessageAdd("有表位每月第1结算日不一致，试验终止", EnumLogType.错误信息);
                    Stop = true;
                    break;
                }
            }
            if (Stop) return;

            if (param[0] == "是")  //组合有功
            {
                if (Stop) return;
                MessageAdd("读取组合有功特征字", EnumLogType.提示信息);
                string[] readData = MeterProtocolAdapter.Instance.ReadData("有功组合方式特征字");

                for (int i = 0; i < MeterNumber; i++)
                {
                    if (!MeterInfo[i].YaoJianYn) continue;
                    if (readData[i] == null || readData[i] == "") continue;

                    byte chr = Convert.ToByte(readData[i], 16);

                    if (OneMeterInfo.DgnProtocol.ProtocolName == "CDLT698")
                        chr = Core.Function.Funs.BitRever(chr);

                    PChar[i] = "";

                    if ((chr & 0x01) == 0x01) PChar[i] = "(P+)";
                    if ((chr & 0x04) == 0x04) PChar[i] += "+(P-)";
                    if ((chr & 0x02) == 0x02) PChar[i] += "-(P+)";
                    if ((chr & 0x08) == 0x08) PChar[i] += "-(P-)";

                    PChar[i] = PChar[i].Trim('-');
                }
            }

            if (param[1] == "是") //组合无功
            {
                if (Stop) return;
                MessageAdd("读取组合无功1特征字", EnumLogType.提示信息);
                string[] readData = MeterProtocolAdapter.Instance.ReadData("无功组合方式1特征字");
                for (int i = 0; i < MeterNumber; i++)
                {
                    if (!MeterInfo[i].YaoJianYn) continue;
                    if (readData[i] == null || readData[i] == "") continue;

                    byte chr = Convert.ToByte(readData[i], 16);
                    if (OneMeterInfo.DgnProtocol.ProtocolName == "CDLT698")
                        chr = Core.Function.Funs.BitRever(chr);

                    Q1Char[i] = "";
                    if ((chr & 0x01) == 0x01) Q1Char[i] = "(Q1)";
                    if ((chr & 0x04) == 0x04) Q1Char[i] += "+(Q2)";
                    if ((chr & 0x10) == 0x10) Q1Char[i] += "+(Q3)";
                    if ((chr & 0x40) == 0x40) Q1Char[i] += "+(Q4)";
                    if ((chr & 0x02) == 0x02) Q1Char[i] += "-(Q1)";
                    if ((chr & 0x08) == 0x08) Q1Char[i] += "-(Q2)";
                    if ((chr & 0x20) == 0x20) Q1Char[i] += "-(Q3)";
                    if ((chr & 0x80) == 0x80) Q1Char[i] += "-(Q4)";

                    Q1Char[i] = Q1Char[i].Trim('+');
                }

                if (Stop) return;
                MessageAdd("读取组合无功2特征字", EnumLogType.提示信息);
                readData = MeterProtocolAdapter.Instance.ReadData("无功组合方式2特征字");
                for (int i = 0; i < MeterNumber; i++)
                {
                    if (!MeterInfo[i].YaoJianYn) continue;
                    if (readData[i] == null || readData[i] == "") continue;

                    byte chr = Convert.ToByte(readData[i], 16);
                    if (OneMeterInfo.DgnProtocol.ProtocolName == "CDLT698")
                        chr = Core.Function.Funs.BitRever(chr);

                    Q2Char[i] = "";
                    if ((chr & 0x01) == 0x01) Q2Char[i] = "(Q1)";
                    if ((chr & 0x04) == 0x04) Q2Char[i] += "+(Q2)";
                    if ((chr & 0x10) == 0x10) Q2Char[i] += "+(Q3)";
                    if ((chr & 0x40) == 0x40) Q2Char[i] += "+(Q4)";
                    if ((chr & 0x02) == 0x02) Q2Char[i] += "-(Q1)";
                    if ((chr & 0x08) == 0x08) Q2Char[i] += "-(Q2)";
                    if ((chr & 0x20) == 0x20) Q2Char[i] += "-(Q3)";
                    if ((chr & 0x80) == 0x80) Q2Char[i] += "-(Q4)";

                    Q2Char[i] = Q2Char[i].Trim('+');
                }
            }

            if (param[0] == "是")
            {
                if (Stop) return;
                Walk(PowerWay.正向有功);

                if (Stop) return;
                Walk(PowerWay.反向有功);
            }
            if (Stop) return;
            if (param[1] == "是")
            {
                Walk(PowerWay.第一象限无功);
                if (Stop) return;
                Walk(PowerWay.第二象限无功);
                if (Stop) return;
                Walk(PowerWay.第三象限无功);
                if (Stop) return;
                Walk(PowerWay.第四象限无功);
            }

            //读取当前需量  
            if (Stop) return;
            ReadDemand(param[0],  0);
            #region 将电表时间修改结算日前1分钟

            if (Stop) return;
            Identity();

            if (Stop) return;
            MessageAdd("将电表时间修改到结算日前30秒", EnumLogType.提示信息);
            string time1 = arrFreeze[FirstIndex];
            time1 = string.Format("{0}{1:D2}{2}0000", DateTime.Now.Year, 1, time1);
            DateTime freezeTime = Core.Function.DateTimes.FormatStringToDateTime(time1);
            freezeTime = freezeTime.AddSeconds(-30);
            bool[] times = MeterProtocolAdapter.Instance.WriteDateTime(freezeTime);

            //-----
            for (int i = 0; i < MeterNumber; i++)
            {
                if (MeterInfo[i].YaoJianYn)
                {
                    if (times[i] == false)
                        MessageAdd(string.Format("表位{0}写表时间失败!", i), EnumLogType.错误信息);
                }
            }

            WaitTime(string.Format("第{0}次运行过每月第1结算日", 1), 120);

            #endregion
            if (Stop) return;
            bool[] bResult = ReadDemand(param[0], 1);

            if (Stop) return;
            Identity(false);
            MessageAdd("恢复电表时间", EnumLogType.提示信息);



            if (Stop) return;
            MessageAdd("开始读取GPS时间...", EnumLogType.提示信息);
            readTime = DateTime.Now; //读取GPS时间

            if (Stop) return;
            bool[] result = MeterProtocolAdapter.Instance.WriteDateTime(readTime);

            if (Stop) return;
            MessageAdd("恢复最大需量周期", EnumLogType.提示信息);

            for (int i = 0; i < MeterNumber; i++)
            {
                if (MeterInfo[i].YaoJianYn)
                {
                    if (arrDemand[i] == "5")
                        arrDemand[i] = "15";
                }
            }
            MeterProtocolAdapter.Instance.WriteData("最大需量周期", arrDemand);

            for (int i = 0; i < MeterNumber; i++)
            {
                if (MeterInfo[i].YaoJianYn)
                {
                    if (!result[i]) bResult[i] = false;
                }
            }
            //刷新结论
            for (int i = 0; i < MeterNumber; i++)
            {
                if (!MeterInfo[i].YaoJianYn) continue;

                ResultDictionary["结论"][i] = bResult[i] ? "合格" : "不合格";
            }
            RefUIData("结论");
        }

        /// <summary>
        /// 比对两个数据的值，True-两数组值相同，否则为False
        /// </summary>
        /// <param name="f1"></param>
        /// <param name="f2"></param>
        /// <returns></returns>
        private bool CompareDemand(float[] f1, float[] f2)
        {
            if (f1 == null) return false;
            if (f1.Length == 0) return false;
            if (f1.Length != f2.Length) return false;

            for (int i = 0; i < f1.Length; i++)
            {
                if (Math.Abs(f1[i] - f2[i]) > 0.01)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// 走字
        /// </summary>
        /// <param name="pw"></param>
        /// <returns></returns>
        private void Walk(PowerWay pw)
        {
            string strGlys = "";
            switch (pw)
            {
                case PowerWay.正向有功:
                    strGlys = "1.0";
                    break;
                case PowerWay.反向有功:
                    strGlys = "-1.0";
                    break;
                case PowerWay.第一象限无功:
                    strGlys = "0.5L";
                    break;
                case PowerWay.第二象限无功:
                    strGlys = "0.8C";
                    break;
                case PowerWay.第三象限无功:
                    strGlys = "-0.8C";
                    break;
                case PowerWay.第四象限无功:
                    strGlys = "-0.5L";
                    break;
            }

            if (Stop) return;
            //float xib = OneMeterInfo.GetIb()[0];
            float xib = Number.GetCurrentByIb("0.5imax", OneMeterInfo.MD_UA, HGQ);  //获取走字的电流
            if (PowerOn(OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, xib, xib, xib, Cus_PowerYuanJian.H, pw, strGlys) == false)
            {
                MessageAdd("控制源输出失败", EnumLogType.提示信息);
                return;
            }
            Thread.Sleep(300);

            WaitTime("走" + pw + "电量", 360);

            if (Stop) return;
            PowerOn();
            //PowerOff();
            Thread.Sleep(10000);
        }

        readonly Dictionary<int, float[]> _Demand0 = new Dictionary<int, float[]>();      //当前需量
        readonly Dictionary<int, float[]> _Demand1 = new Dictionary<int, float[]>();      //上1结算日需量

        private bool[] ReadDemand(string paramP, int curNum)
        {
            bool[] bResult = new bool[MeterNumber];
            for (int i = 0; i < MeterNumber; i++)
                bResult[i] = true;


            //if(paramP == "是")
            //{
            //    glfx = PowerWay.正向有功;
            //}
            //else
            //{
            //    glfx = PowerWay.正向无功;
            //}




            //0101FF00--(当前)正向有功最大需量及发生时间数据块  费率1-费率63
            //0101FF01--(上1结算日)正向有功总最大需量及发生时间
            //0101FF02--(上2结算日)正向有功总最大需量及发生时间
            //0101FF03--(上3结算日)正向有功总最大需量及发生时间

            //0102FF00--(当前)反向有功最大需量及发生时间数据块
            //0102FF01--(上1结算日)反向有功总最大需量及发生时间
            //0102FF02--(上2结算日)反向有功总最大需量及发生时间
            //0102FF03--(上3结算日)反向有功总最大需量及发生时间

            PowerWay glfx = PowerWay.正向有功;
            string[] dicDemand = new string[MeterNumber];
            for (int q = 0; q < 2; q++)
            {

                if (paramP == "是")
                {
                    if (q == 0)
                    {
                        glfx = PowerWay.正向有功;
                    }
                    else if (q == 1)
                    {
                        glfx = PowerWay.反向有功;
                    }
                }

                if (curNum == 0)
                    MessageAdd(string.Format("读取【当前{0}】需量", glfx), EnumLogType.提示与流程信息);
                else
                    MessageAdd(string.Format("读取【上{0}结算日{1}】需量", curNum, glfx), EnumLogType.提示与流程信息);

                //读取(当前)正向有功最大需量及发生时间
                if (curNum == 0)
                {
                    if (Stop) return bResult;
                    if (glfx == PowerWay.正向有功)
                    {
                        dicDemand = MeterProtocolAdapter.Instance.ReadData("(当前)正向有功最大需量及发生时间数据块");
                    }
                    else if (glfx == PowerWay.反向有功)
                    {
                        dicDemand = MeterProtocolAdapter.Instance.ReadData("(当前)反向有功总最大需量及发生时间数据块");
                    }

                    if (OneMeterInfo.DgnProtocol.ProtocolName == "CDLT698")
                    {
                        for (int i = 0; i < MeterNumber; i++)
                        {
                            if (string.IsNullOrEmpty(dicDemand[i])) continue;

                            string[] ds = dicDemand[i].Split(',');
                            if (ds.Length >= 8)
                                dicDemand[i] = string.Format("{0},{1},{2},{3},{4}",
                                    (Convert.ToSingle(ds[0]) / 10000).ToString("f4"),
                                    (Convert.ToSingle(ds[2]) / 10000).ToString("f4"),
                                    (Convert.ToSingle(ds[4]) / 10000).ToString("f4"),
                                    (Convert.ToSingle(ds[6]) / 10000).ToString("f4"),
                                    (Convert.ToSingle(ds[8]) / 10000).ToString("f4"));
                        }
                    }

                    for (int i = 0; i < MeterNumber; i++)
                    {
                        if (string.IsNullOrEmpty(dicDemand[i])) continue;
                        _Demand0[i] = new float[5];
                        string[] ds = dicDemand[i].Split(',');
                        if (ds.Length >= 5)
                        {
                            _Demand0[i][0] = Convert.ToSingle(ds[0]);
                            _Demand0[i][1] = Convert.ToSingle(ds[1]);
                            _Demand0[i][2] = Convert.ToSingle(ds[2]);
                            _Demand0[i][3] = Convert.ToSingle(ds[3]);
                            _Demand0[i][4] = Convert.ToSingle(ds[4]);
                        }
                    }
                }
                //读取(上N结算日)正向有功总最大需量及发生时间数据块
                else            //上N次
                {
                    #region 222
                    if (Stop) return bResult;
                    if (OneMeterInfo.DgnProtocol.ProtocolName == "CDLT6452007")
                    {
                        if (glfx == PowerWay.正向有功)
                            dicDemand = MeterProtocolAdapter.Instance.ReadData(string.Format("(上{0}结算日)正向有功总最大需量及发生时间数据块", curNum));
                        else if (glfx == PowerWay.反向有功)
                            dicDemand = MeterProtocolAdapter.Instance.ReadData(string.Format("(上{0}结算日)反向有功总最大需量及发生时间数据块", curNum));
                    }
                    else if (OneMeterInfo.DgnProtocol.ProtocolName == "CDLT698")
                    {

                        List<string> oad = new List<string>() { "50050200" };
                        Dictionary<int, List<object>> dicObj = new Dictionary<int, List<object>>();
                        List<string> rcsd = new List<string>() { "10100200", "10200200" };
                        Dictionary<int, Dictionary<string, List<object>>> dic = MeterProtocolAdapter.Instance.ReadRecordData(oad, EmSecurityMode.ClearTextRand, EmGetRequestMode.GetRequestRecord, 1, rcsd, ref dicObj);
                        for (int i = 0; i < MeterNumber; ++i)
                        {
                            dicDemand[i] = "";
                            if (MeterInfo[i].YaoJianYn && dic.ContainsKey(i) && dic[i] != null)
                            {
                                if (dic[i].ContainsKey("10100200") && glfx == PowerWay.正向有功)  //00100200正向有功：总,尖,峰,平,谷
                                {
                                    dicDemand[i] += (Convert.ToSingle(dic[i]["10100200"][0]) / 10000f).ToString("f4") + ",";
                                    dicDemand[i] += (Convert.ToSingle(dic[i]["10100200"][2]) / 10000f).ToString("f4") + ",";
                                    dicDemand[i] += (Convert.ToSingle(dic[i]["10100200"][4]) / 10000f).ToString("f4") + ",";
                                    dicDemand[i] += (Convert.ToSingle(dic[i]["10100200"][6]) / 10000f).ToString("f4") + ",";
                                    dicDemand[i] += (Convert.ToSingle(dic[i]["10100200"][8]) / 10000f).ToString("f4") + ",";
                                }

                                if (dic[i].ContainsKey("10200200") && glfx == PowerWay.反向有功)  //00200200反向有功：总,尖,峰,平,谷
                                {
                                    dicDemand[i] += (Convert.ToSingle(dic[i]["10200200"][0]) / 10000f).ToString("f4") + ",";
                                    dicDemand[i] += (Convert.ToSingle(dic[i]["10200200"][2]) / 10000f).ToString("f4") + ",";
                                    dicDemand[i] += (Convert.ToSingle(dic[i]["10200200"][4]) / 10000f).ToString("f4") + ",";
                                    dicDemand[i] += (Convert.ToSingle(dic[i]["10200200"][6]) / 10000f).ToString("f4") + ",";
                                    dicDemand[i] += (Convert.ToSingle(dic[i]["10200200"][8]) / 10000f).ToString("f4") + ",";

                                }

                            }
                            dicDemand[i] = dicDemand[i].TrimEnd(',');
                        }
                    }
                    #endregion 222
                }

                for (int i = 0; i < MeterNumber; i++)
                {
                    _Demand1[i] = new float[5];
                    if (string.IsNullOrEmpty(dicDemand[i])) continue;
                    string[] ds = dicDemand[i].Split(',');
                    if (ds.Length >= 5)
                    {
                        _Demand1[i][0] = Convert.ToSingle(ds[0]);
                        _Demand1[i][1] = Convert.ToSingle(ds[1]);
                        _Demand1[i][2] = Convert.ToSingle(ds[2]);
                        _Demand1[i][3] = Convert.ToSingle(ds[3]);
                        _Demand1[i][4] = Convert.ToSingle(ds[4]);
                    }
                }
                for (int j = 0; j < MeterNumber; j++)
                {
                    if (!MeterInfo[j].YaoJianYn) continue;

                    if (dicDemand[j] == "") continue;

                    if (dicDemand[j] != null)
                    {
                        if (dicDemand[j].Length >= 5)
                        {
                            string[] ps = dicDemand[j].Split(',');

                            if (curNum == 0)
                            {
                                if (q == 0)
                                {
                                    ResultDictionary["每月第一结算日"][j] = arrFreeze[j];
                                    ResultDictionary["组合有功特征字"][j] = PChar[j];
                                    ResultDictionary["组合无功1特征字"][j] = Q1Char[j];
                                    ResultDictionary["组合无功2特征字"][j] = Q2Char[j];
                                    ResultDictionary["总"][j] = ps[0].ToString();
                                    ResultDictionary["尖"][j] = ps[1].ToString();
                                    ResultDictionary["峰"][j] = ps[2].ToString();
                                    ResultDictionary["平"][j] = ps[3].ToString();
                                    ResultDictionary["谷"][j] = ps[4].ToString();
                                }

                                MessageAdd(string.Format("第【{9}】表位 当前【{10}】结果：每月第一结算日：{0}，组合有功特征字：{1}，组合无功1特征字：{2}，组合无功2特征字：{3}，总：{4}，尖：{5}，峰：{6}，平：{7}，谷：{8}",
                                arrFreeze[j], PChar[j], Q1Char[j], Q2Char[j], ps[0].ToString(), ps[1].ToString(), ps[2].ToString(), ps[3].ToString(), ps[4].ToString(), j,
                                glfx.ToString()), EnumLogType.提示与流程信息);
                            }
                            else
                            {
                                if (q == 0)
                                {
                                    ResultDictionary[$"上{curNum}次总"][j] = ps[0].ToString();
                                    ResultDictionary[$"上{curNum}次尖"][j] = ps[1].ToString();
                                    ResultDictionary[$"上{curNum}次峰"][j] = ps[2].ToString();
                                    ResultDictionary[$"上{curNum}次平"][j] = ps[3].ToString();
                                    ResultDictionary[$"上{curNum}次谷"][j] = ps[4].ToString();
                                }

                                MessageAdd(string.Format($"第【{j}】表位 上{curNum}次【{glfx}】结果：上{curNum}次总：{ps[0]}，" +
                                    $"上{curNum}次尖：{ps[1]}，上{curNum}次峰：{ps[2]}，上{curNum}次平：{ps[3]}，" +
                                    $"上{curNum}次谷：{ps[4]}"), EnumLogType.提示与流程信息);
                            }
                        }
                    }
                }
                if (curNum == 0)
                {
                    RefUIData("每月第一结算日");
                    RefUIData("组合有功特征字");
                    RefUIData("组合无功1特征字");
                    RefUIData("组合无功2特征字");
                    RefUIData("总");
                    RefUIData("尖");
                    RefUIData("峰");
                    RefUIData("平");
                    RefUIData("谷");
                }
                else
                {
                    RefUIData($"上{curNum}次总");
                    RefUIData($"上{curNum}次尖");
                    RefUIData($"上{curNum}次峰");
                    RefUIData($"上{curNum}次平");
                    RefUIData($"上{curNum}次谷");
                }
            }

            if (curNum == 0) return bResult;

            for (int i = 0; i < MeterNumber; i++)
            {
                if (!MeterInfo[i].YaoJianYn) continue;

                if (_Demand0.Count > i && _Demand1.Count > i)
                    if (!CompareDemand(_Demand0[i], _Demand1[i]))
                        bResult[i] = false;

                if (paramP == "是")
                {
                    ResultDictionary["正向有功"][i] = bResult[i] ? "合格" : "不合格";
                    ResultDictionary["反向有功"][i] = bResult[i] ? "合格" : "不合格";

                    RefUIData("正向有功");
                    RefUIData("反向有功");
                }

                ResultDictionary["结论"][i] = bResult[i] ? "合格" : "不合格";
            }


            return bResult;
        }
        protected override bool CheckPara()
        {
            ResultNames = new string[] { "每月第一结算日", "组合有功特征字", "组合无功1特征字", "组合无功2特征字", "总", "尖", "峰", "平", "谷", "上1次总", "上1次尖", "上1次峰", "上1次平", "上1次谷", "正向有功", "反向有功", "结论" };
            return true;
        }
        /// <summary>
        /// 初始化设备参数,计算每一块表需要检定的圈数
        /// </summary>
        private bool InitEquipment()
        {
            MessageAdd("开始升电压...", EnumLogType.提示信息);
            if (Stop) return false;
            if (!PowerOn())
            {
                MessageAdd("升电压失败! ", EnumLogType.提示信息);
                return false;
            }
            WaitTime("升源成功，等待源稳定", 5);
            return true;
        }
    }
}
