using LYTest.Core;
using LYTest.ViewModel.CheckController;
using System;
using System.Collections.Generic;
using LYTest.MeterProtocol.Protocols.DLT698.Enum;

namespace LYTest.Verify.Multi
{

    //  ///add lsj 20220218 费率时段检测实验
    /// <summary>
    /// 费率时段检测实验  读取时段信息
    /// </summary>
    public class Dgn_ReadPeriod : VerifyBase
    {
        //Doto 费率时段 需要获取检定参数  结论判断条件未写   modify yjt 20220303 结论已写
        //modify yjt 20220303 设置-结论配置修改 “运行时区|运行时段|第一套时区表数据|第二套时区表数据|第一套第1日时段表数据|第二天第1日时段表数据|标准时段表数据”
        //为 “运行时区|运行时段|第一套时区表数据|第二套时区表数据|第一套第1日时段表数据|第二套第1日时段表数据|标准日时段表数据”
        public override void Verify()
        {
            //add yjt 20220305 新增日志提示
            MessageAdd("费率时段检测实验检定开始...", EnumLogType.流程信息);

            //获取检定参数
            string[] FLDataArry = ReplaceSymbol(Test_Value).Split(','); // 检定参数 英文逗号分割

            //add  yjt 20220303 新增检定参数拼接为标准日时段表数据
            string bzsj = "";
            for (int i = 0; i < FLDataArry.Length; i++)
            {
                bzsj += FLDataArry[i];
            }

            //add  yjt 20220303 新增标准日时段表数据的结论
            for (int i = 0; i < MeterNumber; i++)
            {
                if (!MeterInfo[i].YaoJianYn) continue;
                ResultDictionary["标准日时段表数据"][i] = bzsj;
            }
            RefUIData("标准日时段表数据");

            //add  yjt 20220303 新增停止
            if (Stop) return;

            //更新一下电能表数据
            //MeterDataHelper.Instance.Init();
            ////更新多功能协议
            //Adapter.Instance.UpdateMeterProtocol();
            //升源 只升电压 读数据
            PowerOn();

            //add yjt 20220415 新增升源等待时间
            WaitTime("正在升源", 8);

            MessageAdd("正在进行费率时段检查实验", EnumLogType.提示信息);
            string[] arrStatusZone = new string[MeterNumber];
            string[] arrStatusPeriod = new string[MeterNumber];
            //获取参数

            //显示时区时段

            //add  yjt 20220303 新增停止
            if (Stop) return;

            //add yjt 20220320 新增演示模式
            if (IsDemo) //演示模式
            {
                for (int i = 0; i < MeterNumber; i++)
                {
                    if (!MeterInfo[i].YaoJianYn) continue;

                    ResultDictionary["运行时区"][i] = "第一套";
                    ResultDictionary["运行时段"][i] = "第一套";
                    ResultDictionary["第一套时区表数据"][i] = "010101";
                    ResultDictionary["第二套时区表数据"][i] = "010101";
                    ResultDictionary["第一套第1日时段表数据"][i] = bzsj;
                    ResultDictionary["第二套第1日时段表数据"][i] = bzsj;
                    ResultDictionary["结论"][i] = ConstHelper.合格;
                }

                RefUIData("运行时区");
                RefUIData("运行时段");
                RefUIData("第一套时区表数据");
                RefUIData("第二套时区表数据");
                RefUIData("第一套第1日时段表数据");
                RefUIData("第二套第1日时段表数据");
                RefUIData("结论");
            }
            else
            {
                string[] strReadData;
                //第一块要检表 FirstIndex
                #region DLT645-2007
                if (OneMeterInfo.MD_ProtocolName.IndexOf("645") != -1)
                {
                    MessageAdd("读取状态运行字3", EnumLogType.提示信息);
                    strReadData = MeterProtocolAdapter.Instance.ReadData("电表运行状态字3");
                    for (int i = 0; i < MeterNumber; i++)
                    {
                        //判断是否要检
                        if (!MeterInfo[i].YaoJianYn) continue;

                        if (!string.IsNullOrEmpty(strReadData[i]))
                        {
                            int warningValue = Convert.ToInt32(strReadData[i], 16);

                            if ((warningValue & 0x01) == 0x01)
                                arrStatusPeriod[i] = "第二套";
                            else
                                arrStatusPeriod[i] = "第一套";

                            if ((warningValue & 0x20) == 0x20)
                                arrStatusZone[i] = "第二套";
                            else
                                arrStatusZone[i] = "第一套";
                        }
                        else
                        {
                            arrStatusPeriod[i] = "第一套";
                        }

                        //判断是否读取到时区时段 刷新时区时段
                        if (string.IsNullOrEmpty(strReadData[i]))
                            ResultDictionary["运行时区"][i] = "未读到";
                        else
                            ResultDictionary["运行时区"][i] = arrStatusZone[i];

                        if (string.IsNullOrEmpty(strReadData[i]))
                        {
                            ResultDictionary["运行时段"][i] = "未读到";
                        }
                        else
                        {
                            ResultDictionary["运行时段"][i] = arrStatusPeriod[i];
                        }
                    }
                    //add  yjt 20220303 新增运行时区和运行时段的结论
                    RefUIData("运行时区");
                    RefUIData("运行时段");

                    //add  yjt 20220303 新增停止
                    if (Stop) return;

                    MessageAdd("读取第一套时区表数据", EnumLogType.提示信息);
                    strReadData = MeterProtocolAdapter.Instance.ReadData("第一套时区表数据");
                    for (int i = 0; i < MeterNumber; i++)
                    {
                        if (!MeterInfo[i].YaoJianYn) continue;
                        if (string.IsNullOrWhiteSpace(strReadData[i]))
                            ResultDictionary["第一套时区表数据"][i] = "未读到";
                        else
                            ResultDictionary["第一套时区表数据"][i] = strReadData[i];
                    }

                    RefUIData("第一套时区表数据");

                    //add  yjt 20220303 新增停止
                    if (Stop) return;

                    MessageAdd("读取第二套时区表数据", EnumLogType.提示信息);
                    strReadData = MeterProtocolAdapter.Instance.ReadData("第二套时区表数据");

                    for (int i = 0; i < MeterNumber; i++)
                    {
                        if (!MeterInfo[i].YaoJianYn) continue;
                        if (string.IsNullOrWhiteSpace(strReadData[i]))
                            ResultDictionary["第二套时区表数据"][i] = "未读到";
                        else
                            ResultDictionary["第二套时区表数据"][i] = strReadData[i];
                    }
                    RefUIData("第二套时区表数据");

                    //add  yjt 20220303 新增停止
                    if (Stop) return;

                    MessageAdd("读取第一套第1日时段表数据", EnumLogType.提示信息);
                    strReadData = MeterProtocolAdapter.Instance.ReadData("第一套第1日时段数据");
                    for (int i = 0; i < MeterNumber; i++)
                    {
                        if (!MeterInfo[i].YaoJianYn) continue;
                        ResultDictionary["第一套第1日时段表数据"][i] = GetFeiLvInfo(strReadData[i]);

                        //add  yjt 20220303 新增费率时段检查实验的结论 645未测试
                        if (arrStatusPeriod[i] == "第一套")
                        {
                            if (EqualsPeriod(ResultDictionary["第一套第1日时段表数据"][i], bzsj))
                            {
                                //    if (periods == rst.Value.Substring(0, periods.Length) || periods.Length < 1)
                                ResultDictionary["结论"][i] = ConstHelper.合格;
                                //    else
                                //        ResultDictionary["第一套时区表数据"][i] = ConstHelper.不合格;
                            }
                            else
                            {
                                ResultDictionary["结论"][i] = ConstHelper.不合格;
                            }
                        }
                    }
                    RefUIData("第一套第1日时段表数据");

                    //add  yjt 20220303 新增停止
                    if (Stop) return;

                    MessageAdd("读取第二套第1日时段表数据", EnumLogType.提示信息);
                    strReadData = MeterProtocolAdapter.Instance.ReadData("第二套第1日时段数据");
                    for (int i = 0; i < MeterNumber; i++)
                    {
                        if (!MeterInfo[i].YaoJianYn) continue;
                        ResultDictionary["第二套第1日时段表数据"][i] = GetFeiLvInfo(strReadData[i]);

                        //add  yjt 20220303 新增费率时段检查实验的结论 645未测试
                        if (arrStatusPeriod[i] == "第二套")
                        {
                            if (EqualsPeriod(ResultDictionary["第二套第1日时段表数据"][i], bzsj))
                            {
                                //    if (periods == rst.Value.Substring(0, periods.Length) || periods.Length < 1)
                                ResultDictionary["结论"][i] = ConstHelper.合格;
                                //    else
                                //        ResultDictionary["第一套时区表数据"][i] = ConstHelper.不合格;
                            }
                            else
                            {
                                ResultDictionary["结论"][i] = ConstHelper.不合格;
                            }
                        }
                    }
                    RefUIData("第二套第1日时段表数据");
                }
                #endregion
                else if (OneMeterInfo.MD_ProtocolName.IndexOf("698") != -1)
                {
                    for (int i = 0; i < MeterNumber; i++)
                    {
                        if (!MeterInfo[i].YaoJianYn) continue;
                        arrStatusPeriod[i] = "第一套";
                        arrStatusZone[i] = "第一套";
                        ResultDictionary["运行时区"][i] = arrStatusZone[i];
                        ResultDictionary["运行时段"][i] = arrStatusPeriod[i];
                    }

                    //add  yjt 20220303 新增运行时区和运行时段的结论
                    RefUIData("运行时区");
                    RefUIData("运行时段");

                    //add  yjt 20220303 新增停止
                    if (Stop) return;

                    //modify yjt 20220415 修改身份认证为不必要，传入参数为false
                    //Identity();
                    //modify yjt 20220415 修改身份认证为不必要，传入参数为false
                    Identity(false); //身份认证

                    if (Stop) return;

                    MessageAdd("读取第一套时区表数据", EnumLogType.提示信息);

                    List<string> LstOad = new List<string>
                    {
                        "40140200"
                    };
                    Dictionary<int, object[]> DicObj = MeterProtocolAdapter.Instance.ReadData(LstOad, EmSecurityMode.ClearTextRand, EmGetRequestMode.GetRequestNormalList);

                    strReadData = new string[MeterNumber];
                    for (int i = 0; i < MeterNumber; i++)
                    {
                        if (!MeterInfo[i].YaoJianYn || !DicObj.ContainsKey(i) || DicObj[i] == null) continue;

                        strReadData[i] = "";
                        for (int j = 0; j < DicObj[i].Length / 3; j++)
                        {
                            strReadData[i] += DicObj[i][j * 3].ToString().PadLeft(2, '0') + DicObj[i][j * 3 + 1].ToString().PadLeft(2, '0') + DicObj[i][j * 3 + 2].ToString().PadLeft(2, '0');
                        }
                        ResultDictionary["第一套时区表数据"][i] = strReadData[i];
                    }
                    RefUIData("第一套时区表数据");

                    //add  yjt 20220303 新增停止
                    if (Stop) return;

                    MessageAdd("读取第二套时区表数据", EnumLogType.提示信息);
                    LstOad = new List<string>
                    {
                        "40150200"
                    };
                    DicObj = MeterProtocolAdapter.Instance.ReadData(LstOad, EmSecurityMode.ClearTextRand, EmGetRequestMode.GetRequestNormalList);

                    for (int i = 0; i < MeterNumber; i++)
                    {
                        if (!MeterInfo[i].YaoJianYn || !DicObj.ContainsKey(i) || DicObj[i] == null) continue;
                        strReadData[i] = "";
                        for (int j = 0; j < DicObj[i].Length / 3; j++)
                        {
                            strReadData[i] += DicObj[i][j * 3].ToString().PadLeft(2, '0') + DicObj[i][j * 3 + 1].ToString().PadLeft(2, '0') + DicObj[i][j * 3 + 2].ToString().PadLeft(2, '0');
                        }
                        ResultDictionary["第二套时区表数据"][i] = strReadData[i];

                    }
                    RefUIData("第二套时区表数据");

                    //add  yjt 20220303 新增停止
                    if (Stop) return;

                    MessageAdd("读取第一套第1日时段表数据", EnumLogType.提示信息);
                    LstOad = new List<string>
                    {
                        "40160201"
                    };
                    DicObj = MeterProtocolAdapter.Instance.ReadData(LstOad, EmSecurityMode.ClearTextRand, EmGetRequestMode.GetRequestNormalList);

                    for (int i = 0; i < MeterNumber; i++)
                    {
                        if (!MeterInfo[i].YaoJianYn || !DicObj.ContainsKey(i) || DicObj[i] == null) continue;
                        strReadData[i] = "";
                        for (int j = 0; j < DicObj[i].Length / 3; j++)
                        {
                            for (int k = 0; k < 3; k++)
                            {
                                strReadData[i] += DicObj[i][j * 3 + k].ToString().PadLeft(2, '0').Replace("165", "00");
                            }
                        }
                        ResultDictionary["第一套第1日时段表数据"][i] = GetFeiLvInfo(strReadData[i]);

                        //add  yjt 20220303 新增费率时段检查实验的结论
                        if (arrStatusPeriod[i] == "第一套")
                        {
                            if (EqualsPeriod(ResultDictionary["第一套第1日时段表数据"][i], bzsj))
                            {
                                //    if (periods == rst.Value.Substring(0, periods.Length) || periods.Length < 1)
                                ResultDictionary["结论"][i] = ConstHelper.合格;
                                //    else
                                //        ResultDictionary["第一套时区表数据"][i] = ConstHelper.不合格;
                            }
                            else
                            {
                                ResultDictionary["结论"][i] = ConstHelper.不合格;
                            }
                        }

                        //刷新主界面是否合格
                    }
                    RefUIData("第一套第1日时段表数据");

                    //add  yjt 20220303 新增停止
                    if (Stop) return;

                    MessageAdd("读取第二套第1日时段表数据", EnumLogType.提示信息);
                    LstOad = new List<string>
                {
                    "40170201"
                };
                    DicObj = MeterProtocolAdapter.Instance.ReadData(LstOad, EmSecurityMode.ClearTextRand, EmGetRequestMode.GetRequestNormalList);

                    for (int i = 0; i < MeterNumber; i++)
                    {

                        if (!MeterInfo[i].YaoJianYn || !DicObj.ContainsKey(i) || DicObj[i] == null) continue;

                        strReadData[i] = "";
                        for (int j = 0; j < DicObj[i].Length / 3; j++)
                        {
                            for (int k = 0; k < 3; k++)
                            {
                                strReadData[i] += DicObj[i][j * 3 + k].ToString().PadLeft(2, '0').Replace("165", "00");
                            }
                        }
                        ResultDictionary["第二套第1日时段表数据"][i] = GetFeiLvInfo(strReadData[i]);

                        //delete yjt 20220509 删除第二套的时段表的验证
                        ////add  yjt 20220303 新增费率时段检查实验的结论
                        //if (arrStatusPeriod[i] == "第二套")
                        //{
                        //    if (ResultDictionary["第二套第1日时段表数据"][i] == bzsj)
                        //    {
                        //        //    if (periods == rst.Value.Substring(0, periods.Length) || periods.Length < 1)
                        //        ResultDictionary["结论"][i] = ConstHelper.合格;
                        //        //    else
                        //        //        ResultDictionary["第一套时区表数据"][i] = ConstHelper.不合格;
                        //    }
                        //    else
                        //    {
                        //        ResultDictionary["结论"][i] = ConstHelper.不合格;
                        //    }
                        //}
                        //刷新主界面是否合格
                    }
                    RefUIData("第二套第1日时段表数据");
                }
            }

            //add  yjt 20220303 新增停止
            if (Stop) return;

            //add  yjt 20220303 新增费率时段检查实验的结论
            RefUIData("结论");

            //add yjt 20200304 新增检定完成提示信息
            MessageAdd("检定完成", EnumLogType.提示信息);
            //add yjt 20220305 新增日志提示
            MessageAdd("费率时段检查实验检定结束...", EnumLogType.流程信息);
        }

        /// <summary>
        /// 比较时段，参数格式2 00:00(谷)连续无分割符
        /// </summary>
        /// <param name="p1">表内时段</param>
        /// <param name="p2">方案配置的时段</param>
        /// <returns></returns>
        private bool EqualsPeriod(string v, string bzsj)
        {
            if (string.IsNullOrWhiteSpace(v) || string.IsNullOrWhiteSpace(bzsj)) return false;
            if (v.Length < 8 || bzsj.Length < 8) return false;

            if (v.Length < bzsj.Length)
            {
                if (string.IsNullOrWhiteSpace(bzsj.Replace(v, "").Replace(v.Substring(v.Length - 8, 8), ""))) return true;
            }
            else if (v.Length > bzsj.Length)
            {
                if (string.IsNullOrWhiteSpace(v.Replace(bzsj, "").Replace(bzsj.Substring(bzsj.Length - 8, 8), ""))) return true;
            }
            else
            {
                if (v.Equals(bzsj)) return true;
            }
            return false;
        }

        /// <summary>
        /// 检定参数是否合法
        /// </summary>
        /// <returns></returns>
        protected override bool CheckPara()
        {
            //ResultNames = new string[] { "运行时区", "运行时段", "第一套时区表数据","第二套时区表数据", "第一套第1日时区表数据", "第二套第1日时区表数据", "标准时区表数据", "结论" };
            //modify yjt 20220303 修改检定参数错误字
            ResultNames = new string[] { "运行时区", "运行时段", "第一套时区表数据", "第二套时区表数据", "第一套第1日时段表数据", "第二套第1日时段表数据", "标准日时段表数据", "结论" };

            return true;
        }


        private string GetFeiLvInfo(string data)
        {
            string fl = "";
            while (data.Length >= 6)
            {
                string period = data.Substring(data.Length - 6, 6);
                period = period.Substring(0, 2) + ":" + period.Substring(2, 2) + "(" + GetFeiDlValue(int.Parse(period.Substring(4, 2))) + ")";
                fl = period + fl;

                data = data.Substring(0, data.Length - 6); //去除后面6字符
            }
            return fl;
        }
        private string GetFeiDlValue(int FeiLvID)
        {
            switch (FeiLvID)
            {
                case 1:
                    return "尖";
                case 2:
                    return "峰";
                case 3:
                    return "平";
                case 4:
                    return "谷";
                case 5:
                    return "深谷";
                default:
                    return "";
            }
        }

        private string ReplaceSymbol(string Value)
        {
            if (Value.Contains(" "))
            {
                Value = Value.Replace(" ", "");
            }
            if (Value.Contains("，"))
            {
                Value = Value.Replace("，", ",");
            }
            if (Value.Contains(";"))
            {
                Value = Value.Replace(";", ",");
            }
            if (Value.Contains("|"))
            {
                Value = Value.Replace("|", ",");
            }
            if (Value.Contains("-"))
            {
                Value = Value.Replace("-", ",");
            }
            return Value;
        }
    }
}
