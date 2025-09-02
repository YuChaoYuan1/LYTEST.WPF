using LYTest.MeterProtocol.Protocols.DLT698.Enum;
using LYTest.ViewModel.CheckController;
using System;
using System.Collections.Generic;

namespace LYTest.Verify.LoadRecord
{
    /// <summary>
    /// 负荷记录还有
    /// </summary>
    public class LoadRecord : VerifyBase
    {
        //List<dataType> datas = new List<dataType>();
        float runTime = 15;

        public override void Verify()
        {
            base.Verify();

            if (!PowerOn())
            {
                ErrorRefResoult();
                MessageAdd("升源失败,退出检定", EnumLogType.错误信息);
                return;
            }



            for (int i = 0; i < MeterNumber; i++)
            {
                if (!MeterInfo[i].YaoJianYn) continue;
                ResultDictionary["间隔时间"][i] = runTime + "分钟";
            }
            RefUIData("间隔时间");
            WaitTime("功率源稳定", 5);
            ReadMeterAddrAndNo();

            float[][] powers = new float[][] { new float[MeterNumber], new float[MeterNumber], new float[MeterNumber], new float[MeterNumber], new float[MeterNumber] };
            MessageAdd("冻结前上一次冻结总电量", EnumLogType.提示信息);
            ReadDL(true, ref powers[0]);
            RefDJData(powers[0], "冻结前上一次冻结总电量");

            MessageAdd("冻结前电量", EnumLogType.提示信息);
            ReadDL(false, ref powers[1]);
            RefDJData(powers[1], "冻结前电量");
            bool flag = false;
            //判断冻结电量是否与当前电量相等
            for (int j = 0; j < powers[0].Length; j++)
            {
                if (powers[0][j] == powers[1][j])// && flt_DL[0][j] != 0
                {
                    flag = true;
                    break;
                }
            }
            if (flag)
            {
                MessageAdd("进行走字30S，请稍候......", EnumLogType.提示信息);
                float Xib = Core.Function.Number.GetCurrentByIb("0.5imax", OneMeterInfo.MD_UA, HGQ);
                //升源
                if (!PowerOn(OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, Xib, Xib, Xib, Core.Enum.Cus_PowerYuanJian.H, Core.Enum.PowerWay.正向有功, "1.0"))
                {
                    ErrorRefResoult();
                    MessageAdd("升源失败,退出检定", EnumLogType.错误信息);
                    return;
                }
                WaitTime("走电量", 30);
                PowerOn();
                WaitTime("正在关闭电流", 5);
            }

            WaitTime("等待记录产生", (int)(runTime * 60));
            MessageAdd("分钟冻结后冻结后电量", EnumLogType.提示信息);
            ReadDL(false, ref powers[2]);
            RefDJData(powers[2], "冻结后电量");


            MessageAdd("分钟冻结后读取上一次冻结总电量", EnumLogType.提示信息);
            ReadDL(true, ref powers[3]);
            RefDJData(powers[3], "冻结后上一次冻结总电量");


            for (int j = 0; j < MeterNumber; j++)
            {
                if (!MeterInfo[j].YaoJianYn) continue;
                ResultDictionary["结论"][j] = (powers[2][j] > powers[0][j]) ? "合格" : "不合格";
                MessageAdd(string.Format("{0:F2}|{1:F2}|{2:F2}", powers[0][j], powers[3][j], powers[2][j]), EnumLogType.提示信息);
            }
            RefUIData("结论");
        }
        /// <summary>
        /// 读取电量
        /// </summary>
        /// <param name="isSpecial">true=冻结电量，false=实际电量</param>
        /// <param name="powers">存储所有表位电量</param>
        /// <returns></returns>
        private void ReadDL(bool isSpecial, ref float[] powers)
        {
            string[] dicEnergy = new string[MeterNumber];
            if (isSpecial)
            {
                List<string> oad = new List<string> { "50020200" };
                Dictionary<int, List<object>> dicObj = new Dictionary<int, List<object>>();
                List<string> rcsd = new List<string>();
                Dictionary<int, Dictionary<string, List<object>>> dic = MeterProtocolAdapter.Instance.ReadRecordData(oad, EmSecurityMode.ClearTextRand, EmGetRequestMode.GetRequestRecord, 1, rcsd, ref dicObj);
                for (int i = 0; i < MeterNumber; ++i)
                {
                    dicEnergy[i] = "";
                    if (MeterInfo[i].YaoJianYn && dic.ContainsKey(i) && dic[i] != null)
                    {
                        if (dic[i].ContainsKey("00100200") || dic[i].ContainsKey("00100201")) //(当前)正向有功总电能
                        {
                            if (dic[i].ContainsKey("00100200")) //(当前)正向有功总电能
                            {
                                if (dic[i]["00100200"].Count <= 0) continue;
                                dicEnergy[i] = dic[i]["00100200"][0].ToString();
                                //dicEnergy[i] = (Convert.ToSingle(dicEnergy[i]) / 100).ToString("f2");

                                //modify yjt 20220302 修改获取冻结电量两位小数不四舍五入
                                dicEnergy[i] = (Math.Floor((Convert.ToSingle(dicEnergy[i]) / 100) * 100) / 100).ToString();
                            }
                            else if (dic[i].ContainsKey("00100201")) //(当前)正向有功总电能
                            {
                                if (dic[i]["00100201"].Count <= 0) continue;
                                dicEnergy[i] = dic[i]["00100201"][0].ToString();
                                dicEnergy[i] = (Math.Floor((Convert.ToSingle(dicEnergy[i]) / 100) * 100) / 100).ToString();
                            }
                        }
                        else
                        {
                            if (dic[i].ContainsKey("00100400")) //(当前)正向有功总电能
                            {
                                if (dic[i]["00100400"].Count <= 0) continue;
                                dicEnergy[i] = dic[i]["00100400"][0].ToString();
                                //dicEnergy[i] = (Convert.ToSingle(dicEnergy[i]) / 10000).ToString("f4");

                                //modify yjt 20220302 修改获取冻结电量两位小数不四舍五入
                                dicEnergy[i] = (Math.Floor((Convert.ToSingle(dicEnergy[i]) / 10000) * 100) / 100).ToString();
                            }
                            else if (dic[i].ContainsKey("00100401")) //(当前)正向有功总电能
                            {
                                if (dic[i]["00100401"].Count <= 0) continue;
                                dicEnergy[i] = dic[i]["00100401"][0].ToString();
                                //dicEnergy[i] = (Convert.ToSingle(dicEnergy[i]) / 10000).ToString("f4");

                                //modify yjt 20220302 修改获取冻结电量两位小数不四舍五入
                                dicEnergy[i] = (Math.Floor((Convert.ToSingle(dicEnergy[i]) / 10000) * 100) / 100).ToString();
                            }
                        }
                    }
                }
            }
            else
            {
                dicEnergy = MeterProtocolAdapter.Instance.ReadData("(当前)正向有功总电能");
                for (int i = 0; i < MeterNumber; i++)
                {
                    if (dicEnergy[i] != null) dicEnergy[i] = dicEnergy[i].Split(',')[0];
                }

            }

            for (int j = 0; j < MeterNumber; j++)
            {
                if (!MeterInfo[j].YaoJianYn) continue;

                if (string.IsNullOrEmpty(dicEnergy[j]))
                {
                    dicEnergy = MeterProtocolAdapter.Instance.ReadData("(当前)正向有功总电能");
                    for (int i = 0; i < MeterNumber; i++)
                    {
                        if (dicEnergy[i] != null) dicEnergy[i] = dicEnergy[i].Split(',')[0];
                    }
                    continue;
                }
                else
                {
                    powers[j] = Convert.ToSingle(dicEnergy[j]);
                }

            }
            if (Stop) return;
            for (int j = 0; j < MeterNumber; j++)
            {
                if (!MeterInfo[j].YaoJianYn) continue;

                if (string.IsNullOrEmpty(dicEnergy[j]))
                {
                    MessageAdd($"表位{j + 1}返回的数据不符合要求", EnumLogType.提示信息);
                    continue;
                }
                else
                {
                    powers[j] = Convert.ToSingle(dicEnergy[j]);
                }

            }

        }

        private void RefDJData(float[] data, string name)
        {

            for (int j = 0; j < MeterNumber; j++)
            {
                if (!MeterInfo[j].YaoJianYn) continue;
                ResultDictionary[name][j] = data[j].ToString();

            }
            RefUIData(name);
        }
        /// <summary>
        /// 检定参数是否合法
        /// </summary>
        /// <returns></returns>
        protected override bool CheckPara()
        {
            try
            {
                string[] data = Test_Value.Split('|');
                if (string.IsNullOrEmpty(data[0]))
                {
                    runTime = float.Parse(data[0]);
                }

                //string[] name = Test_Format.Split('|');
                //for (int i = 1; i < name.Length; i++)
                //{
                //    switch (name[i])
                //    {
                //        case "电压电流":
                //            datas.Add( dataType.电压);
                //            datas.Add(dataType.电流);
                //            break;
                //        case "功率":
                //            datas.Add(dataType.有功功率);
                //            datas.Add(dataType.无功功率);
                //            break;
                //        case "功率因数":
                //            datas.Add(dataType.功率因数);
                //            break;
                //        case "电能":
                //            datas.Add(dataType.正向有功电能);
                //            datas.Add(dataType.反向有功电能);
                //            break;
                //        default:
                //            break;
                //    }
                //}
                //间隔时间|冻结前上一次冻结总电量|冻结前电量|冻结后电量|冻结后上一次冻结总电量
                ResultNames = new string[] { "间隔时间", "冻结前上一次冻结总电量", "冻结前电量", "冻结后电量", "冻结后上一次冻结总电量", "结论" };
                return true;
            }
            catch (Exception ex)
            {
                MessageAdd("负荷记录参数错误：" + ex.ToString(), EnumLogType.错误信息);
                return false;
            }


        }
    }

    //enum dataType
    //{
    //    电压,
    //    电流,
    //    频率,
    //    有功功率,
    //    无功功率,
    //    功率因数,
    //    正向有功电能,
    //    反向有功电能

    //}

}