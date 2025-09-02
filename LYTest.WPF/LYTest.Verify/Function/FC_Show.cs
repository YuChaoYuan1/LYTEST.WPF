using LYTest.Core;
using LYTest.ViewModel.CheckController;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LYTest.Verify.Function
{
    public class FC_Show : VerifyBase
    {
        //add yjt 20220304 新增显示功能（默认成功）文件夹 LYTest.Verify/Function

        /// <summary>
        /// 显示功能
        /// </summary>
        public override void Verify()
        {
            //add yjt 20220305 新增日志提示
            MessageAdd("显示功能试验检定开始...", EnumLogType.流程信息);

            //开始检定
            MessageAdd("开始做显示功能", EnumLogType.提示信息);
            base.Verify();

            //默认合格
            if (IsDemo)
            {
                for (int i = 0; i < MeterNumber; i++)
                {
                    if (!MeterInfo[i].YaoJianYn) continue;
                    ResultDictionary["显示功能数据内容"][i] = "00000201";
                    ResultDictionary["标准内容"][i] = "00000201";
                    ResultDictionary["结论"][i] = ConstHelper.合格;
                }
                RefUIData("显示功能数据内容");
                RefUIData("标准内容");
                RefUIData("结论");
            }
            else
            {
                List<string[]> dataFlagList = new List<string[]>();//每屏所有表
                string[] param = Test_Value.Split(',');
                string[] dataFlagStd = new string[param.Length];
                string[] dataName = new string[param.Length];

                for (int i = 0; i < param.Length; i++)
                {
                    if (param[i] == "") continue;
                    if (OneMeterInfo.DgnProtocol.ClassName == "CDLT698")
                        dataFlagStd[i] = param[i].Split('|')[2];
                    else
                        dataFlagStd[i] = param[i].Split('|')[1];

                    dataName[i] = param[i].Split('|')[0];
                }

                if (!CheckVoltage())
                {
                    return;
                }
                if (Stop) return;
                MessageAdd("读取自动循环显示屏数", EnumLogType.提示信息);
                string[] AutoScreenCount = MeterProtocolAdapter.Instance.ReadData("自动循环显示屏数");

                if (Stop) return;
                MessageAdd("读取自动循环显示屏显示数据项", EnumLogType.提示信息);
                int[] screenCount = ToInt(AutoScreenCount);
                //TODO:加配置
                if (VerifyConfig.Test_Fun_Show_Model || screenCount == null || screenCount.All(v => v == 0)
                    || AutoScreenCount == null || AutoScreenCount.All(v => string.IsNullOrWhiteSpace(v)))//如果全null，读电量判断，兰吉尔645不支持读循显，多数厂家采用读电量
                {
                    string[] readdata = MeterProtocolAdapter.Instance.ReadData(dataName[0]);
                    bool all = true;
                    for (int i = 0; i < MeterNumber; i++)
                    {
                        if (!MeterInfo[i].YaoJianYn) continue;

                        bool rst1 = true;
                        if (string.IsNullOrWhiteSpace(readdata[i]) || readdata[i] == "-1")
                        {
                            rst1 = false;
                            all = false;
                        }

                        ResultDictionary["显示功能数据内容"][i] = readdata[i];
                        ResultDictionary["标准内容"][i] = string.Join("|", dataFlagStd);
                        ResultDictionary["结论"][i] = rst1 ? ConstHelper.合格 : ConstHelper.不合格;
                    }

                    if (all == false)
                    {
                        readdata = MeterProtocolAdapter.Instance.ReadData(dataName[0]);
                        for (int i = 0; i < MeterNumber; i++)
                        {
                            if (!MeterInfo[i].YaoJianYn) continue;
                            if (ResultDictionary["结论"][i] == ConstHelper.合格) continue;

                            bool rst1 = true;
                            if (string.IsNullOrWhiteSpace(readdata[i]) || readdata[i] == "-1")
                            {
                                rst1 = false;
                            }

                            ResultDictionary["显示功能数据内容"][i] = readdata[i];
                            ResultDictionary["标准内容"][i] = string.Join("|", dataFlagStd);
                            ResultDictionary["结论"][i] = rst1 ? ConstHelper.合格 : ConstHelper.不合格;
                        }
                    }
                }
                else
                {
                    int maxAuto = GetMax(screenCount);
                    for (int i = 1; i <= maxAuto; i++)
                    {
                        if (Stop) return;
                        MessageAdd("读取自动循环显示第" + i + "/" + maxAuto + "屏显示数据项", EnumLogType.提示信息);
                        string[] ss = MeterProtocolAdapter.Instance.ReadData("自动循环显示第" + i.ToString() + "屏");
                        MessageAdd("读取自动循环显示第" + i + "/" + maxAuto + "屏显示数据项完成", EnumLogType.提示信息);
                        for (int j = 0; j < ss.Length; j++)
                        {
                            if (!string.IsNullOrEmpty(ss[j]) && ss[j].Length > 8)
                                ss[j] = ss[j].Substring(0, 8);
                        }
                        dataFlagList.Add(ss);
                    }

                    if (Stop) return;
                    MessageAdd("正在处理数据...", EnumLogType.提示信息);

                    for (int i = 0; i < MeterNumber; i++)
                    {
                        if (!MeterInfo[i].YaoJianYn) continue;

                        bool rst1 = true;
                        string reason = "";
                        string[] OneMeterIDs = GetDataFlag(dataFlagList, i);
                        if (screenCount[i] >= param.Length)
                        {
                            for (int j = 0; j < param.Length; j++)
                            {
                                if (-1 == Array.IndexOf(OneMeterIDs, dataFlagStd[j]))
                                {
                                    rst1 = false;
                                    reason = "自动循环显示屏显示数据项或顺序与配置不匹配";
                                    break;
                                }
                            }
                        }
                        else
                        {
                            rst1 = false;
                            reason = "自动循环显示屏数" + screenCount[i] + "不等于配置屏数" + param.Length;
                        }

                        ResultDictionary["显示功能数据内容"][i] = string.Join("|", OneMeterIDs);
                        ResultDictionary["标准内容"][i] = string.Join("|", dataFlagStd);
                        ResultDictionary["结论"][i] = rst1 ? ConstHelper.合格 : ConstHelper.不合格;
                    }
                }
                RefUIData("显示功能数据内容");
                RefUIData("标准内容");
                RefUIData("结论");
            }

            MessageAdd("检定完成", EnumLogType.提示信息);

            //add yjt 20220305 新增日志提示
            MessageAdd("显示功能试验检定结束...", EnumLogType.流程信息);
        }

        /// <summary>
        /// Int数组---->字符串数组
        /// </summary>
        /// <param name="arrInput"></param>
        /// <returns></returns>
        private int[] ToInt(string[] arrInput)
        {
            Converter<string, int> myCon = new Converter<string, int>(Cstring2Int);
            return Array.ConvertAll(arrInput, myCon);

        }

        private int Cstring2Int(string str)
        {
            if (string.IsNullOrEmpty(str))
                str = "0";
            return Convert.ToInt32(str);

        }

        private int GetMax(int[] screenCount)
        {
            int max = 0;
            foreach (int item in screenCount)
            {
                if (item > max)
                {
                    max = item;
                }
            }
            return max;
        }

        private string[] GetDataFlag(List<string[]> list, int bw)
        {
            List<string> lst = new List<string>();
            int lstCount = list.Count;
            for (int i = 0; i < lstCount; i++)
            {
                lst.Add((list[i])[bw]);
            }
            return lst.ToArray();
        }

        protected override bool CheckPara()
        {
            ResultNames = new string[] { "显示功能数据内容", "标准内容", "结论" };
            return true;
        }
    }
}
