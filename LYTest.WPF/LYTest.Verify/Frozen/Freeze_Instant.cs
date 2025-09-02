using LYTest.Core.Function;
using LYTest.ViewModel.CheckController;
using System;
using System.Collections.Generic;
using System.Threading;
using LYTest.MeterProtocol.Protocols.DLT698.Enum;

namespace LYTest.Verify.Frozen
{
    /// <summary>
    /// 瞬时冻结检测
    /// </summary>
    public class Freeze_Instant : VerifyBase
    {

        float Xib = 1f;
        /// <summary>
        /// 瞬时冻结检定
        /// </summary>
        /// <param name="ItemNumber"></param>
        public override void Verify()
        {
            base.Verify();
            //升源
            if (!PowerOn())
            {
                RefNoResoult();
                MessageAdd("升源失败,退出检定",EnumLogType.错误信息);
                return;
            }
            //读冻结模式字
            MessageAdd("等待电表启动......", EnumLogType.提示信息);
            Thread.Sleep(5000);  //延时源稳定5S
            ReadMeterAddrAndNo();

            Identity(true);

            //判断物联表的情况进行一次电量清零--因为物联表一天只能进行三次瞬时冻结
            if (DAL.Config.ConfigHelper.Instance.IsITOMeter)
            {
                MessageAdd("开始电量清零--确定冻结触发", EnumLogType.提示信息);
                MeterLogicalAddressType = Core.Enum.MeterLogicalAddressEnum.计量芯;
                MeterProtocolAdapter.Instance.ClearEnergy();
                MeterLogicalAddressType = Core.Enum.MeterLogicalAddressEnum.管理芯;
                MeterProtocolAdapter.Instance.ClearEnergy();
                WaitTime("清零", 5);
            }



            //冻结处理
            MessageAdd("开始进行瞬时冻结", EnumLogType.提示信息);
            FreezeDeal();

            //恢复表时间为当前时间
            Identity(false);

            MessageAdd("开始读取GPS时间...", EnumLogType.提示信息);
            DateTime readTime = DateTime.Now; //读取GPS时间

            if (DAL.Config.ConfigHelper.Instance.IsITOMeter) MeterLogicalAddressType = Core.Enum.MeterLogicalAddressEnum.计量芯;

            bool[] bs = MeterProtocolAdapter.Instance.WriteDateTime(readTime);
            bool b = GetArrValue(bs);
            if (!b)
            {
                MessageAdd("写表时间失败!请检查多功能协议配置、表接线或是表编程开关是否已经打开。\r\n当前检定中止", EnumLogType.错误信息);
            }
            MessageAdd("瞬时冻结检定完毕", EnumLogType.提示信息);
        }


                /// <summary>
        /// 冻结处理
        /// </summary>
        /// <returns></returns>
        private void FreezeDeal()
        {
            //读取冻结前电量
            float[][] powers = new float[][] { new float[MeterNumber], new float[MeterNumber], new float[MeterNumber], new float[MeterNumber], new float[MeterNumber], new float[MeterNumber] };
            MessageAdd("正在读取当前电表总电量", EnumLogType.提示信息);
            ReadDL(false, 1, ref powers[0]);
            RefDJData(powers[0], "当前电表总电量");


            //循环触发冻结--依次读取上一次冻结电量 

            for (int i = 1; i < 4; i++)
            {
                if (!PowerOn(OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, Xib, Xib, Xib, Core.Enum.Cus_PowerYuanJian.H, Core.Enum.PowerWay.正向有功, "1.0"))
                {
                    RefNoResoult();
                    MessageAdd("升源失败,退出检定", EnumLogType.错误信息);
                    return;
                }
                WaitTime("走电量", 20);
                MessageAdd("正在设置冻结测试参数", EnumLogType.提示信息);
                PowerOn();
                WaitTime("正在关闭电流", 5);
                DoFreeze();
                WaitTime("触发一次瞬冻结",5);
            }


            for (int i = 1; i < 4; i++)
            {
                ReadDL(true, i, ref powers[i]);
                RefDJData(powers[i], $"上{i}次冻结电量");
            }

            if (Stop) return;

            //判断结论
            for (int k = 0; k < MeterNumber; k++)
            {
                if (!MeterInfo[k].YaoJianYn) continue;

                if (powers[1][k]> powers[2][k] && powers[2][k] > powers[3][k] && powers[3][k] > powers[0][k])
                    ResultDictionary["结论"][k] = "合格";
                else
                    ResultDictionary["结论"][k] = "不合格";

            }
            RefUIData("结论");

        }


        /// <summary>
        /// 检定参数是否合法
        /// </summary>
        /// <returns></returns>
        protected override bool CheckPara()
        {


            ResultNames = new string[] {"当前电表总电量", "上1次冻结电量", "上2次冻结电量", "上3次冻结电量", "结论" };
            
            try
            {
                Xib = Number.GetCurrentByIb("0.5imax",OneMeterInfo.MD_UA, HGQ);
            }
            catch (Exception ex)
            {
                MessageAdd("数据校验失败\r\n" + ex.ToString(), EnumLogType.错误信息);
                RefNoResoult();
                return false;
            }
            return true;
        }
        /// <summary>
        /// 触发一次冻结
        /// </summary>
        private void DoFreeze()
        {
            if (DAL.Config.ConfigHelper.Instance.IsITOMeter) MeterLogicalAddressType = Core.Enum.MeterLogicalAddressEnum.管理芯;
            bool[] rst = new bool[MeterNumber];
            if (OneMeterInfo.DgnProtocol.ProtocolName == "CDLT6452007")
            {
                rst = MeterProtocolAdapter.Instance.FreezeCmd("99999999");

            }
            else if (OneMeterInfo.DgnProtocol.ProtocolName == "CDLT698")
            {
                rst = MeterProtocolAdapter.Instance.InstantFreeze698();
            }
            bool bResult = GetArrValue(rst);
            if (!bResult)
            {
                //Stop = true;
                //TryStopTest();
                MessageAdd("瞬时冻结失败!请检查多功能协议配置、表接线或是表编程开关是否已经打开。\r\n当前检定中止", EnumLogType.错误信息);
            }
        }
        /// <summary>
        /// 读取电量
        /// </summary>
        /// <param name="isSpecial">true=冻结电量，false=实际电量</param>
        /// <param name="times">冻结电量次数</param>
        /// <param name="powers">存储所有表位电量</param>
        /// <returns></returns>
        private void ReadDL(bool isSpecial, int times, ref float[] powers)
        {

            string[] dicEnergy = new string[MeterNumber];
            if (isSpecial)
            {
                if (OneMeterInfo.DgnProtocol.ProtocolName == "CDLT6452007")
                {
                    Dictionary<int, float[]> tmp = MeterProtocolAdapter.Instance.ReadSpecialEnergy(6, times);
                    for (int i = 0; i < MeterNumber; ++i)
                    {
                        if (!MeterInfo[i].YaoJianYn) continue;
                        dicEnergy[i] = tmp[i][0].ToString();
                    }
                }
                else if (OneMeterInfo.DgnProtocol.ProtocolName == "CDLT698")
                {

                    if (DAL.Config.ConfigHelper.Instance.IsITOMeter) MeterLogicalAddressType = Core.Enum.MeterLogicalAddressEnum.管理芯;

                    List<string> oad = new List<string> { "50000200" };
                    Dictionary<int, List<object>> dicObj = new Dictionary<int, List<object>>();
                    List<string> rcsd = new List<string>();
                    Dictionary<int, Dictionary<string, List<object>>> dic = MeterProtocolAdapter.Instance.ReadRecordData(oad, EmSecurityMode.ClearTextRand, EmGetRequestMode.GetRequestRecord, times, rcsd, ref dicObj);
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
            }
            else
            {
                dicEnergy = MeterProtocolAdapter.Instance.ReadData("(当前)正向有功总电能");

                if (OneMeterInfo.DgnProtocol.ProtocolName == "CDLT698")
                {
                    for (int i = 0; i < MeterNumber; i++)
                    {
                        if (!MeterInfo[i].YaoJianYn) continue;
                        if (dicEnergy[i] != null) dicEnergy[i] = dicEnergy[i].Split(',')[0];
                    }
                }
            }

            if (Stop) return;
            for (int j = 0; j < MeterNumber; j++)
            {
                //强制停止
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


        private void RefDJData(float[] data,string name)
        {

            for (int j = 0; j < MeterNumber; j++)
            {
                if (!MeterInfo[j].YaoJianYn) continue;
                ResultDictionary[name][j] = data[j].ToString();
 
            }
            RefUIData(name);
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
