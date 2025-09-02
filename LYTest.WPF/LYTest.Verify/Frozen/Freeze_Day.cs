using LYTest.Core.Function;
using LYTest.ViewModel.CheckController;
using System;
using System.Collections.Generic;
using System.Threading;
using LYTest.MeterProtocol.Protocols.DLT698.Enum;

namespace LYTest.Verify.Frozen
{

    /// <summary>
    /// 日冻结检定
    /// </summary>
    public class Freeze_Day : VerifyBase
    {
        float Xib = 1f;
        /// <summary>
        /// 日冻结检定
        /// </summary>
        /// <param name="ItemNumber"></param>
        public override void Verify()
        {
            base.Verify();
            MessageAdd("日冻结实验开始...", EnumLogType.提示信息);
            if (Stop) return;

            MessageAdd("正在升源...", EnumLogType.提示信息);
            if (!PowerOn())
            {
                MessageAdd("源输出失败", EnumLogType.提示信息);
                return;
            }
            MessageAdd("等待电表启动......", EnumLogType.提示信息);
            Thread.Sleep(5000);  //延时源稳定5S
            ReadMeterAddrAndNo();

            MessageAdd("开始冻结处理", EnumLogType.提示信息);
            FreezeDeal();

            #region -----------------恢复电表时间---------------
            //恢复表时间为当前时间
            Identity();
            MessageAdd("开始读取GPS时间...", EnumLogType.提示信息);
            DateTime readTime = DateTime.Now; //读取GPS时间

            MessageAdd("开始写入GPS时间...", EnumLogType.提示信息);
            if (DAL.Config.ConfigHelper.Instance.IsITOMeter) MeterLogicalAddressType = Core.Enum.MeterLogicalAddressEnum.计量芯;
            bool[] bs = MeterProtocolAdapter.Instance.WriteDateTime(readTime);
            bool b = GetArrValue(bs);
            if (!b)     MessageAdd("写表时间失败!请检查多功能协议配置、表接线或是表编程开关是否已经打开。\r\n当前检定中止", EnumLogType.错误信息);
            #endregion

            MessageAdd("日冻结检定完毕", EnumLogType.提示信息);

        }

        /// <summary>
        /// 冻结处理
        /// </summary>
        /// <returns></returns>
        private void FreezeDeal()
        {
            int num = Convert.ToInt16(Test_Value);//冻结次数

            if (num == 0) num = 1;

            float[][] freezeDL = new float[MeterNumber][];//冻结电量
            for (int i = 0; i < MeterNumber; i++)
            {
                freezeDL[i] = new float[num];
            }

            #region ----------------读取冻结前数据------------------
            if (Stop) return;
            MessageAdd("日冻结前读取当前电表总电量", EnumLogType.提示信息);
            float[] freezeQDL = ReadDL();//冻结前表电量

            if (OneMeterInfo.DgnProtocol.ProtocolName == "CDLT6452007")
            {
                MessageAdd("开始进行日冻结", EnumLogType.提示信息);

                bool[] bReturn = MeterProtocolAdapter.Instance.FreezeCmd("99990000");
                bool bResult = GetArrValue(bReturn);
                if (!bResult) MessageAdd("日冻结失败!请检查多功能协议配置、表接线或是表编程开关是否已经打开。\r\n当前检定中止", EnumLogType.错误信息);
            }
            if (Stop) return;
            MessageAdd("进行走字120S，请稍候......", EnumLogType.提示信息);

            if (!PowerOn(OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, Xib, Xib, Xib, Core.Enum.Cus_PowerYuanJian.H, Core.Enum.PowerWay.正向有功, "1.0"))
            {
                RefNoResoult();
                MessageAdd("升源失败,退出检定", EnumLogType.流程信息);
                return;
            }

            for (int i = 0; i < num; i++)
            {
                if (Stop) return;
                WaitTime("走电量", 30);

                if (Stop) return;
                Identity();
                DateTime dt = DateTime.Parse(DateTime.Now.ToString("yyyy/MM/dd 23:59:55")).AddDays(i + 1);
                MessageAdd("设置电表时间为日冻结时间前" + dt.ToString("yyyyMMdd HH:mm:ss"), EnumLogType.提示信息);
               if(DAL.Config. ConfigHelper.Instance.IsITOMeter)     MeterLogicalAddressType = Core.Enum.MeterLogicalAddressEnum.计量芯;
                MeterProtocolAdapter.Instance.WriteDateTime(dt);

                if (Stop) return;
                WaitTime("等待第[" + (i + 1) + "]冻结产生", 30);
            }

            if (Stop) return;
            PowerOn();
            MessageAdd("等待电表启动......", EnumLogType.提示信息);
            Thread.Sleep(5000);  //延时源稳定5S

            for (int i = 0; i < num; i++)
            {
                if (Stop) return;
                MessageAdd("读取上" + (i + 1) + "次冻结总电量", EnumLogType.提示信息);
              
                float[] powers = ReadFreeze(i + 1);

                for (int j = 0; j < MeterNumber; j++)
                {
                    freezeDL[j][i] = powers[j];
                }
            }

            //}
            #endregion

            #region -------------------日冻结处理----------------
            if (Stop) return;
            MessageAdd("日冻结后读取当前电表总电量", EnumLogType.提示信息);
            float[] freezeHDL = ReadDL();


            if (Stop) return;
            int sum = int.Parse(Test_Value);
            if (sum < 1) sum = 1;
            //上报检定数据
            for (int i = 0; i < MeterNumber; i++)
            {
                if (!MeterInfo[i].YaoJianYn) continue;

                string dl = "";
                foreach (float f in freezeDL[i])
                {
                    dl = f.ToString("F2") + "," + dl;
                }
                dl = dl.TrimEnd(',');

                ResultDictionary["试验前电量"][i] = freezeQDL[i].ToString();
                for (int j = 0; j < sum; j++)
                {
                    ResultDictionary["第1次冻结电量"][i] = dl.Split(',')[j].ToString();
                }
                ResultDictionary["试验后电量"][i] = freezeHDL[i].ToString();
            }

            RefUIData("试验前电量");
            for (int j = 0; j < sum; j++)
            {
                RefUIData("第1次冻结电量");
            }
            RefUIData("试验后电量");

            #endregion
            #region --------------------处理结论------------------
            for (int j = 0; j < MeterNumber; j++)
            {
                if (!MeterInfo[j].YaoJianYn) continue;

                string rst1 = "合格";
                if (freezeDL[j].Length > 1)
                {
                    for (int f = 0; f < freezeDL[j].Length - 1; f++)
                    {
                        if (freezeDL[j][f] < freezeDL[j][f + 1])
                        {
                            rst1 = "不合格";
                            break;
                        }
                    }
                }
                else
                {
                    if (freezeDL[j][0] <= freezeQDL[j])
                        rst1 = "不合格";
                }
                ResultDictionary["结论"][j] = rst1;
            }
            RefUIData("结论");
            #endregion

        }

        /// <summary>
        /// 读取电量[(当前)正向有功总电能]
        /// </summary>
        private float[] ReadDL()
        {
            string[] dicEnergy = MeterProtocolAdapter.Instance.ReadData("(当前)正向有功总电能");

            if (OneMeterInfo.DgnProtocol.ProtocolName == "CDLT698")
            {
                for (int i = 0; i < MeterNumber; i++)
                {
                    if (!MeterInfo[i].YaoJianYn) continue;
                    if (dicEnergy[i] != null) dicEnergy[i] = dicEnergy[i].Split(',')[0];
                }
            }

            float[] powers = new float[MeterNumber];
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

            return powers;


        }

        /// <summary>
        /// 读取冻结数据
        /// </summary>
        /// <param name="num">上N次</param>
        /// <returns></returns>
        private float[] ReadFreeze(int num)
        {
            string[] dicEnergy = new string[MeterNumber];

            if (OneMeterInfo.DgnProtocol.ProtocolName == "CDLT6452007")
            {
                //(上1次)日冻结正向有功电能数据
                Dictionary<int, float[]> tmp = MeterProtocolAdapter.Instance.ReadSpecialEnergy(4, num);
                for (int i = 0; i < MeterNumber; ++i)
                {
                    if (!MeterInfo[i].YaoJianYn) continue;
                    dicEnergy[i] = tmp[i][0].ToString();
                }
            }
            else if (OneMeterInfo.DgnProtocol.ProtocolName == "CDLT698")
            {
                if (DAL.Config.ConfigHelper.Instance.IsITOMeter) MeterLogicalAddressType = Core.Enum.MeterLogicalAddressEnum.管理芯;
                List<string> oad = new List<string> { "50040200" };
                Dictionary<int, List<object>> dicObj = new Dictionary<int, List<object>>();
                List<string> rcsd = new List<string>();
                Dictionary<int, Dictionary<string, List<object>>> dic = MeterProtocolAdapter.Instance.ReadRecordData(oad, EmSecurityMode.ClearTextRand, EmGetRequestMode.GetRequestRecord, num, rcsd, ref dicObj);
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
                                //dicEnergy[i] = (Convert.ToSingle(dicEnergy[i]) / 10000).ToString("f2");

                                //modify yjt 20220302 修改获取冻结电量两位小数不四舍五入
                                dicEnergy[i] = (Math.Floor((Convert.ToSingle(dicEnergy[i]) / 10000) * 100) / 100).ToString();
                            }
                            else if (dic[i].ContainsKey("00100401")) //(当前)正向有功总电能
                            {
                                if (dic[i]["00100401"].Count <= 0) continue;
                                dicEnergy[i] = dic[i]["00100401"][0].ToString();
                                //dicEnergy[i] = (Convert.ToSingle(dicEnergy[i]) / 10000).ToString("f2");

                                //modify yjt 20220302 修改获取冻结电量两位小数不四舍五入
                                dicEnergy[i] = (Math.Floor((Convert.ToSingle(dicEnergy[i]) / 10000) * 100) / 100).ToString();
                            }
                        }
                    }
                }
            }

            float[] powers = new float[MeterNumber];
            for (int i = 0; i < MeterNumber; i++)
            {
                if (!MeterInfo[i].YaoJianYn) continue;

                if (string.IsNullOrEmpty(dicEnergy[i]))
                {
                    MessageAdd($"表位{i + 1}返回的数据不符合要求", EnumLogType.提示信息);
                    continue;
                }
                else
                {
                    powers[i] = Convert.ToSingle(dicEnergy[i]);
                }

            }

            return powers;
        }
        /// <summary>
        /// 检定参数是否合法
        /// </summary>
        /// <returns></returns>
        protected override bool CheckPara()
        {
            List<string> _list = new List<string>
            {
                "试验前电量",
                "试验后电量"
            };
            int num = int.Parse(Test_Value);
            if (num < 1) num = 1;
            for (int i = 0; i < num; i++)
            {
                _list.Add($"第{num}次冻结电量");
            }
            _list.Add("结论");
            ResultNames = _list.ToArray();
            try
            {
                Xib = Number.GetCurrentByIb("0.5imax", OneMeterInfo.MD_UA, HGQ);
            }
            catch (Exception ex)
            {
                MessageAdd("数据校验失败\r\n" + ex.ToString(), EnumLogType.错误信息);
                RefNoResoult();
                return false;
            }
            return true;
        }

        private void RefNoResoult()
        {

            for (int j = 0; j < MeterNumber; j++)
            {
                if (!MeterInfo[j].YaoJianYn) continue;
                ResultDictionary["结论"][j] = "不合格";

            }
            RefUIData("结论");
        }


    }
}
