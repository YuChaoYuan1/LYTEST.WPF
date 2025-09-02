using LYTest.Core.Enum;
using LYTest.Core.Function;
using LYTest.MeterProtocol.Protocols.DLT698.Enum;
using LYTest.ViewModel;
using LYTest.ViewModel.CheckController;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LYTest.Verify.QualityModule
{
    /// <summary>
    /// 电压暂升电压暂降和短时中断
    /// </summary>
    /// 结论数据：事件名称|超限阈值|延时判定时间|A相测量结果1|B相测量结果1|C相测量结果1|A相测量结果2|B相测量结果2|C相测量结果2
    /// 方案参数：模组事件
    public class DeviationvoltageDropTemporaryRiseShorttermInterruption : VerifyBase
    {
        事件类型 类型;
        Dictionary<string, string> oad = new Dictionary<string, string>() {
                { "A相电压暂降事件","30490700"}  ,
                { "B相电压暂降事件","30490800"}  ,
                { "C相电压暂降事件","30490900"}  ,
                { "A相电压暂升事件","304A0700"}  ,
                { "B相电压暂升事件","304A0800"}  ,
                { "C相电压暂升事件","304A0900"}  ,
                { "A相电压中断事件","30490700"}  ,
                { "B相电压中断事件","30490700"}  ,
                { "C相电压中断事件","30490700"}  ,
            };
        public override void Verify()
        {
            base.Verify();

            // ResultNames = new string[] { "事件名称", "超限阈值", "延时判定时间", "A相测量结果1", "B相测量结果1", "C相测量结果1", "A相测量结果2", "B相测量结果2", "C相测量结果2", "结论" };
            //
            ResultDictionary["事件名称"].Fill(类型.ToString());
            RefUIData("事件名称");
            MeterLogicalAddressType = MeterLogicalAddressEnum.电能质量模组;
            
            string name = "";
            switch (类型)
            {
                case 事件类型.电压暂升:
                    name = "电压暂升事件";
                    break;
                case 事件类型.电压暂降:
                    name = "电压暂降事件";
                    break;
                case 事件类型.电压中断:
                    name = "电压中断事件";
                    break;
                default:
                    break;
            }
            if (!PowerOn(OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, 0, 0, 0, Cus_PowerYuanJian.H, PowerWay.正向有功, "1.0"))
            {
                MessageAdd("升源失败,退出检定", EnumLogType.提示信息);
                return;
            }

            WaitTime("等待30s,剩余", 30);

            string[] readData = MeterProtocolAdapter.Instance.ReadData(name);
            var addata = MeterProtocolAdapter.Instance.ReadData("质量模组电压值");
            for (int i = 0; i < MeterNumber; i++)
            {
                if (!MeterInfo[i].YaoJianYn) continue;
                if (!string.IsNullOrWhiteSpace(readData[i]) && readData[i].Length > 2)
                {
                    var 判定时间 = int.Parse(readData[i].Substring(readData[i].Length - 2));
                    var 阈值 = float.Parse(readData[i].Substring(0, readData[i].Length - 2));
                    ResultDictionary["延时判定时间"][i] = 判定时间.ToString();
                    ResultDictionary["超限阈值"][i] = (阈值 / 100f).ToString("F2") + "%";
                }
            }
            RefUIData("延时判定时间");
            RefUIData("超限阈值");


            //string[] t = MeterProtocolAdapter.Instance.ReadData("电压暂降事件");    //900010
            //t = MeterProtocolAdapter.Instance.ReadData("电压暂升事件"); //1100010
            //t = MeterProtocolAdapter.Instance.ReadData("电压中断事件");   //100010

            MessageAdd("开始读取事件发生前数据", EnumLogType.提示信息);
            Dictionary<int, Dictionary<int, dataFormat>> EnevtData = new Dictionary<int, Dictionary<int, dataFormat>>();
            EnevtData.Add(1, new Dictionary<int, dataFormat>());
            EnevtData.Add(2, new Dictionary<int, dataFormat>());
            
            EnevtData[1] = 读取数据(name, 1);
            
            switch (类型)
            {
                case 事件类型.电压暂升:
                    AC_VoltageSagSndInterruption(30 * 100, 30 * 100, 3, -20, "1", "1", "1");
                    break;
                case 事件类型.电压暂降:
                    AC_VoltageSagSndInterruption(30 * 100, 30 * 100, 3, 20, "1", "1", "1");
                    break;
                case 事件类型.电压中断:
                    AC_VoltageSagSndInterruption(35 * 100, 50 * 100, 2, 92, "1", "1", "1");
                    break;
                default:
                    break;
            }
            WaitTime("正在模拟" + 类型, 240);

            //WaitTime("等待事件发生", 50);
            var addatas = MeterProtocolAdapter.Instance.ReadData("质量模组电压值");
            EnevtData[2] = 读取数据(name, 2);

            ResultDictionary["结论"].Fill("不合格");

            for (int i = 0; i < MeterNumber; i++)
            {
                if (!MeterInfo[i].YaoJianYn) continue;
                if (EnevtData[1].ContainsKey(i) && EnevtData[2].ContainsKey(i))
                {
                    if (EnevtData[2][i].次数 > EnevtData[1][i].次数)
                    {
                        ResultDictionary["结论"][i] = "合格";
                    }
                }
            }
            RefUIData("结论");   //TODO 结论判断还得写一下
        }

        class dataFormat
        {
            public string 事件记录序号;
            public string 事件发生时间;
            public string 事件结束时间;
            public string 事件发生时刻电压;
            public string 事件发生时刻电流;
            public string 事件发生时刻功率因数;
            public string 事件发生时刻频率;
            public string 事件结束时刻电压;
            public string 事件结束时刻电流;
            public string 事件结束时刻功率因数;
            public string 事件结束时刻频率;

            public int 次数 = 0;
            public new string ToString()
            {
                return $"{事件记录序号},{事件发生时间},{事件结束时间},{事件发生时刻电压},{事件发生时刻电流},{事件发生时刻功率因数}," +
                    $"{事件发生时刻频率},{事件结束时刻电压},{事件结束时刻电流},{事件结束时刻功率因数},{事件结束时刻频率}";
            }

        }
        private Dictionary<int, dataFormat> 读取数据(string Name, int Index)
        {
            var ReadList = oad.Where(x => x.Key.EndsWith(Name));
            Dictionary<int, dataFormat> valuePairs = new Dictionary<int, dataFormat>();
            //Dictionary<string, dataFormat> 冻结数据 = new Dictionary<string, dataFormat>();
            foreach (var o in ReadList)
            {
                MessageAdd($"开始读取【{o.Key}】冻结数据");
                var dic = 读取冻结数据(o.Value);

                for (int i = 0; i < MeterNumber; i++)
                {
                    if (!MeterInfo[i].YaoJianYn) continue;
                    if (dic.ContainsKey(i))
                    {
                        var Item = dic[0];
                        dataFormat data = new dataFormat();
                        if (Item != null)
                        {
                            data.事件记录序号 = GetData(Item, "20220200");
                            data.事件发生时间 = GetData(Item, "201E0200");
                            data.事件结束时间 = GetData(Item, "20200200");
                            data.事件发生时刻电压 = GetData(Item, "20002200");
                            data.事件发生时刻电流 = GetData(Item, "20012500");
                            data.事件发生时刻功率因数 = GetData(Item, "200A2200");
                            data.事件发生时刻频率 = GetData(Item, "200F2200");
                            data.事件结束时刻电压 = GetData(Item, "20008200");
                            data.事件结束时刻电流 = GetData(Item, "20018500");
                            data.事件结束时刻功率因数 = GetData(Item, "200A8200");
                            data.事件结束时刻频率 = GetData(Item, "200F8200");
                            string name = o.Key.Substring(0, 2);
                            ResultDictionary[name + "测量结果" + Index][i] = data.ToString();
                            if (string.IsNullOrWhiteSpace(data.事件记录序号))
                            {
                                ResultDictionary["结论"][i] = "不合格";
                            }
                            else
                            {
                                data.次数 = int.Parse(data.事件记录序号);
                            }
                        }
                        if (!valuePairs.ContainsKey(i))
                        {
                            valuePairs.Add(i, data);
                        }
                    }
                    else
                    {
                        ResultDictionary["结论"][i] = "不合格";
                    }
                }
            }
            RefUIData("A相测量结果" + Index);
            RefUIData("B相测量结果" + Index);
            RefUIData("C相测量结果" + Index);
            return valuePairs;
        }

        private string GetData(Dictionary<string, List<object>> sourData, string ItemNo, int Index = 0)
        {
            if (sourData.ContainsKey(ItemNo))
            {
                if (sourData[ItemNo].Count > Index)
                {
                    return sourData[ItemNo][Index].ToString();
                }
                else
                {
                    return "";
                }
            }
            else
            {
                return "";
            }

        }

        private string GetEnevetName(string No)
        {
            string Name = "";
            switch (No)
            {
                case "20220200":
                    Name = "事件记录序号";
                    break;
                case "201E0200":
                    Name = "事件发生时间";
                    break;
                case "20200200":
                    Name = "事件结束时间";
                    break;
                case "33000200":
                    Name = "事件上报状态";
                    break;
                case "20002200":
                    Name = "电压";
                    break;
                case "20012500":
                    Name = "电流";
                    break;
                case "200A2200":
                    Name = "功率因数";
                    break;
                case "200F2200":
                    Name = "电网频率";
                    break;
                case "20008200":
                    Name = "事件结束后电压";
                    break;
                case "20018500":
                    Name = "事件结束后电流";
                    break;
                case "200A8200":
                    Name = "事件结束后功率因数";
                    break;
                case "200F8200":
                    Name = "事件结束后频率";
                    break;
                default:
                    break;
            }
            return Name;
        }

        private void 测试日志(Dictionary<string, object> 数据)
        {
            foreach (var item in 数据)
            {
                Console.Write($"======================【{item.Key}】======================\r\n");
                var data = (Dictionary<int, Dictionary<string, List<object>>>)item.Value;
                if (data.Count > 0)
                {
                    foreach (var v in data[0])
                    {
                        string Name = GetEnevetName(v.Key);
                        Console.Write($"==【{v.Key}】【{Name}】\r\n");
                        int index = 1;
                        foreach (var s in v.Value)
                        {
                            Console.Write($"{index++}:{s}\r\n");
                            if (v.Key == "20220200" || v.Key == "201E0200")
                            {
                                MessageAdd($"【{Name}】==【{s}】", EnumLogType.流程信息);
                            }
                        }
                        Console.Write($"==========分割=================\r\n");
                    }
                }

                //foreach (var v in item.Value[0])
                //{

                //}
            }

        }


        private Dictionary<int, Dictionary<string, List<object>>> 读取冻结数据(string oada)
        {
            List<string> oad = new List<string> { oada };
            Dictionary<int, List<object>> dicObj = new Dictionary<int, List<object>>();
            List<string> rcsd = new List<string>();
            return MeterProtocolAdapter.Instance.ReadRecordData(oad, EmSecurityMode.ClearTextRand, EmGetRequestMode.GetRequestRecord, 1, rcsd, ref dicObj);
        }

        protected override bool CheckPara()
        {
            switch (Test_Value)
            {
                case "电压暂升":
                    类型 = 事件类型.电压暂升;
                    break;
                case "电压暂降":
                    类型 = 事件类型.电压暂降;
                    break;
                case "电压中断":
                    类型 = 事件类型.电压中断;
                    break;
                default:
                    break;
            }

            ResultNames = new string[] { "事件名称", "超限阈值", "延时判定时间", "A相测量结果1", "B相测量结果1", "C相测量结果1", "A相测量结果2", "B相测量结果2", "C相测量结果2", "结论" };
            //"事件名称|超限阈值|延时判定时间|A相测量结果1|B相测量结果1|C相测量结果1|A相测量结果2|B相测量结果2|C相测量结果2|结论"



            return true;
        }
        enum 事件类型
        {
            电压暂升,
            电压暂降,
            电压中断
        }
    }
    public class DeviationvoltageDropTemporaryRiseShorttermInterruption2 : VerifyBase
    {
        float wcLimit = 5.0f;//误差限
        public override void Verify()
        {
            base.Verify();
            MeterLogicalAddressType = MeterLogicalAddressEnum.电能质量模组;
            //AC_VoltageSagSndInterruption(100, 50, 1, 85, "1", "1", "1");
            //if (!PowerOn(OneMeterInfo.MD_UB , OneMeterInfo.MD_UB , OneMeterInfo.MD_UB , 0, 0, 0, Cus_PowerYuanJian.H, PowerWay.正向有功, "1.0"))
            //{
            //    MessageAdd("升源失败,退出检定", EnumLogType.提示信息);
            //    return;
            //}


            List<string> oad = new List<string> { "30490700" };
            Dictionary<int, List<object>> dicObj = new Dictionary<int, List<object>>();
            List<string> rcsd = new List<string>();
            Dictionary<int, Dictionary<string, List<object>>> dic = MeterProtocolAdapter.Instance.ReadRecordData(oad, EmSecurityMode.ClearTextRand, EmGetRequestMode.GetRequestRecord, 1, rcsd, ref dicObj);

            string[] t = MeterProtocolAdapter.Instance.ReadData("电压暂降事件");
            t = MeterProtocolAdapter.Instance.ReadData("电压暂升事件");
            t = MeterProtocolAdapter.Instance.ReadData("电压中断事件");


            MessageAdd("电能质量模组电压波动试验检定开始...", EnumLogType.提示信息);
            for (int j = 0; j < MeterNumber; j++)
            {
                if (!MeterInfo[j].YaoJianYn) continue;
                ResultDictionary["误差限%"][j] = "±" + wcLimit.ToString();
            }
            RefUIData("误差限%");

            if (string.IsNullOrWhiteSpace(OneMeterInfo.MD_MeterNo) || string.IsNullOrWhiteSpace(OneMeterInfo.MD_PostalAddress))   //没有表号的情况获取一下
            {
                MessageAdd("正在获取所有表的表地址和表号", EnumLogType.流程信息);
                ReadMeterAddrAndNo();
                if (!IsDemo)
                {
                    UpdateMeterProtocol();//更新电表命令
                }
            }

            MeterLogicalAddressType = MeterLogicalAddressEnum.电能质量模组;
            //电压暂降
            if (!PowerOn(OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, 0, 0, 0, Cus_PowerYuanJian.H, PowerWay.正向有功, "1.0"))
            {
                MessageAdd("升源失败,退出检定", EnumLogType.提示信息);
                return;
            }
            WaitTime("稳定源状态等待模组读取", 300);


            if (!PowerOn(OneMeterInfo.MD_UB * 0.89f, OneMeterInfo.MD_UB * 0.89f, OneMeterInfo.MD_UB * 0.89f, 0, 0, 0, Cus_PowerYuanJian.H, PowerWay.正向有功, "1.0"))
            {
                MessageAdd("升源失败,退出检定", EnumLogType.提示信息);
                return;
            }
            WaitTime("电压暂降-维持500周波", 300);
            string[] readdata = MeterProtocolAdapter.Instance.ReadData("电压数据块");
            string[] readEvendata = MeterProtocolAdapter.Instance.ReadData("电压暂降事件");
            for (int j = 0; j < MeterNumber; j++)
            {
                if (!MeterInfo[j].YaoJianYn) continue;
                //ResultDictionary["电压暂降误差"][j] =(float.Parse(readdata[j])- EquipmentData.StdInfo.Ub).ToString();
                ResultDictionary["电压暂降误差"][j] = readdata[j];//TODO 需要与标准表值比较，同时判断记录型数据是否发生
            }
            RefUIData("电压暂降误差");

            //电压暂升
            if (!PowerOn(OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, 0, 0, 0, Cus_PowerYuanJian.H, PowerWay.正向有功, "1.0"))
            {
                MessageAdd("升源失败,退出检定", EnumLogType.提示信息);
                return;
            }
            WaitTime("稳定源状态等待模组读取", 300);


            if (!PowerOn(OneMeterInfo.MD_UB * 1.12f, OneMeterInfo.MD_UB * 1.12f, OneMeterInfo.MD_UB * 1.12f, 0, 0, 0, Cus_PowerYuanJian.H, PowerWay.正向有功, "1.0"))
            {
                MessageAdd("升源失败,退出检定", EnumLogType.提示信息);
                return;
            }
            WaitTime("电压暂升-维持500周波", 300);
            string[] readdata2 = MeterProtocolAdapter.Instance.ReadData("电压数据块");
            string[] readEvendata2 = MeterProtocolAdapter.Instance.ReadData("电压暂升事件");
            for (int j = 0; j < MeterNumber; j++)
            {
                if (!MeterInfo[j].YaoJianYn) continue;
                //ResultDictionary["电压暂降误差"][j] =(float.Parse(readdata[j])- EquipmentData.StdInfo.Ub).ToString();
                ResultDictionary["电压暂升误差"][j] = readdata2[j];//TODO 需要与标准表值比较，同时判断记录型数据是否发生
            }
            RefUIData("电压暂升误差");

            //电压暂升
            if (!PowerOn(OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, 0, 0, 0, Cus_PowerYuanJian.H, PowerWay.正向有功, "1.0"))
            {
                MessageAdd("升源失败,退出检定", EnumLogType.提示信息);
                return;
            }
            WaitTime("稳定源状态等待模组读取", 300);


            if (!PowerOn(OneMeterInfo.MD_UB * 1.12f, OneMeterInfo.MD_UB * 1.12f, OneMeterInfo.MD_UB * 1.12f, 0, 0, 0, Cus_PowerYuanJian.H, PowerWay.正向有功, "1.0"))
            {
                MessageAdd("升源失败,退出检定", EnumLogType.提示信息);
                return;
            }
            WaitTime("电压短时中断-维持500周波", 300);
            string[] readdata3 = MeterProtocolAdapter.Instance.ReadData("电压数据块");
            string[] readEvendata3 = MeterProtocolAdapter.Instance.ReadData("电压中断事件");
            for (int j = 0; j < MeterNumber; j++)
            {
                if (!MeterInfo[j].YaoJianYn) continue;
                //ResultDictionary["电压暂降误差"][j] =(float.Parse(readdata[j])- EquipmentData.StdInfo.Ub).ToString();
                ResultDictionary["电压短时中断误差"][j] = readdata3[j];//TODO 需要与标准表值比较，同时判断记录型数据是否发生
            }
            RefUIData("电压短时中断误差");


            for (int j = 0; j < MeterNumber; j++)
            {
                if (!MeterInfo[j].YaoJianYn) continue;
                ResultDictionary["结论"][j] = "合格";
            }
            RefUIData("结论");
        }



        protected override bool CheckPara()
        {
            ResultNames = new string[] { "误差限%", "电压暂降误差", "电压暂升误差", "电压短时中断误差", "平均值", "化整值", "结论" };
            //



            return true;
        }
    }
}
