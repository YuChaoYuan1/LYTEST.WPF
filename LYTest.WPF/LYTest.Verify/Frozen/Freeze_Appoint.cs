using LYTest.Core.Function;
using LYTest.ViewModel.CheckController;
using System;
using System.Collections.Generic;
using System.Threading;
using LYTest.MeterProtocol.Protocols.DLT698.Enum;

namespace LYTest.Verify.Frozen
{
    /// <summary>
    /// 约定冻结检定 (1两套时区表切换冻结,2两套日时段表切换冻结,3两套费率电价切换冻结,4两套阶梯切换冻结)
    /// </summary>
    public class Freeze_Appoint : VerifyBase
    {
        /// <summary>
        /// 约定冻结检定
        /// </summary>
        /// <param name="ItemNumber"></param>
        public override void Verify()
        {
            bool[] AllResult = new bool[MeterNumber];
            for (int i = 0; i < AllResult.Length; i++)
            {
                AllResult[i] = true;
            }
            base.Verify();
            if (!PowerOn())
            {
                MessageAdd("源输出失败", EnumLogType.提示信息);
                RefNoResoult();
                return;
            }
            MessageAdd("等待电表启动......", EnumLogType.提示信息);
            Thread.Sleep(5000);    //延时5S

            if (Stop) return;
            ReadMeterAddrAndNo();

            int iCount;
            if (OneMeterInfo.FKType == 1) //如果是本地表多两项选择。，多了费率电价表切换和阶梯电价表切换
                iCount = 5;
            else
                iCount = 3;

            for (int i = 1; i < iCount; i++)//iCount=[1,4]
            {
                bool[] rstTmp = new bool[MeterNumber];

                FreezeDeal(i, ref rstTmp);

                for (int j = 0; j < rstTmp.Length; j++)
                {

                    if (!MeterInfo[j].YaoJianYn) continue;
                    AllResult[j] &= rstTmp[j];
                }
                if (Stop) return;
            }

            //恢复表时间为当前时间
            if (Stop) return;
            Identity();
            if (Stop) return;
            MessageAdd("开始读取GPS时间...", EnumLogType.提示信息);
            DateTime readTime = DateTime.Now; //读取GPS时间
            if (Stop) return;
            bool[] bReturn = MeterProtocolAdapter.Instance.WriteDateTime(readTime);
            if (!GetArrValue(bReturn))
                MessageAdd("写时间失败!请检查多功能协议配置、表接线或是表编程开关是否已经打开。\r\n当前检定中止", EnumLogType.错误信息);

            //结论
            for (int j = 0; j < MeterNumber; j++)
            {
                if (!MeterInfo[j].YaoJianYn) continue;
                ResultDictionary["结论"][j] = AllResult[j] ? "合格" : "不合格";
            }
            RefUIData("结论");
            MessageAdd("约定冻结检定完毕", EnumLogType.提示信息);
        }
        /// <summary>
        ///  获得bool数组中的统计合格数据
        /// </summary>
        /// <param name="format"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        protected void MsgDisplay(bool[] arr, string format, params object[] args)
        {
            for (int i = 0; i < MeterNumber; i++)
            {
                if (MeterInfo[i].YaoJianYn)
                {
                    if (!arr[i])
                        MessageAdd(string.Format("表位{0}:", i + 1) + string.Format(format, args), EnumLogType.错误信息);
                }
            }
        }
        /// <summary>
        /// 切换约定冻结项
        /// </summary>
        /// <param name="type">类型</param>
        /// <param name="result">结果</param>
        /// <returns></returns>
        private void FreezeDeal(int type, ref bool[] result)
        {
            #region  --------------局部变量---------------
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = true;
            }
            //维数 0=冻结前上一次冻结电量，1=冻结前当前表电量，2=冻结后当前表电量，3=冻结后上一次冻结电量
            float[][] powers = new float[][] { new float[MeterNumber], new float[MeterNumber], new float[MeterNumber], new float[MeterNumber] };
            #endregion

            string msg = string.Empty;
            string msg2 = string.Empty;

            switch (type)
            {
                case 1:
                    msg = "两套时区表切换时间";
                    break;
                case 2:
                    msg = "两套日时段表切换时间";
                    break;
                case 3:
                    msg = "两套费率电价切换时间";
                    break;
                case 4:
                    msg = "两套阶梯电价切换时间";
                    break;
            }
            switch (type)
            {
                case 1:
                    msg2 = "两套时区表切换";
                    break;
                case 2:
                    msg2 = "两套日时段表切换";
                    break;
                case 3:
                    msg2 = "两套费率电价切换";
                    break;
                case 4:
                    msg2 = "两套阶梯电价切换";
                    break;
            }
            MessageAdd(msg, EnumLogType.提示信息);

            if (Stop) return;
            MessageAdd("正在读取冻结前电表总电量......", EnumLogType.提示信息);
            ReadDL(false, type, 1, ref powers[0]);

            if (Stop) return;
            MessageAdd("正在读取上一次电表冻结总电量......", EnumLogType.提示信息);
            ReadDL(true, type, 1, ref powers[1]);

            //判断冻结电量是否与当前电量相等
            //bool xd = false;
            //for (int j = 0; j < powers[0].Length; j++)
            //{
            //    if (!MeterInfo[j].YaoJianYn) continue;
            //    if (powers[0][j] == powers[1][j])
            //    {
            //        xd = true;
            //        break;
            //    }
            //}
            MessageAdd("进行走字30S，请稍候......", EnumLogType.提示信息);
            //升源
           float Xib = Number.GetCurrentByIb("0.5imax", OneMeterInfo.MD_UA, HGQ);
            if (!PowerOn(Xib))
            {
                PowerOn(Xib);
            }

            WaitTime("走电量", 30);

            MessageAdd("正在设置冻结测试参数", EnumLogType.提示信息);
            PowerOn();

            //读冻结模式字
            MessageAdd("等待电表启动......", EnumLogType.提示信息);
            WaitTime("电表启动", 5);
            if (Stop) return;

            //当前年份+1；
            if (Stop) return;
            Identity();

            if (Stop) return;
            MessageAdd(string.Format("正在{0}......", msg), EnumLogType.提示信息); //设置两套表的切换时间
            bool[] bReturn = MeterProtocolAdapter.Instance.WriteData(msg, DateTime.Now.AddYears(type).ToString("yyyy") + "0101000000");
            MsgDisplay(bReturn, "切换{0}失败", msg);

            if (Stop) return;
            DateTime meterTime = new DateTime(DateTime.Now.AddYears(type - 1).Year, 12, 31, 23, 59, 58);
            bReturn = MeterProtocolAdapter.Instance.WriteDateTime(meterTime);
            MsgDisplay(bReturn, "校时失败!");

            if (Stop) return;
            WaitTime("延时", 60);

            if (Stop) return;
            MessageAdd("读取当前电表总电量", EnumLogType.提示信息);
            ReadDL(false, type, 1, ref powers[2]);

            if (Stop) return;
            MessageAdd("读取上一次冻结总电量", EnumLogType.提示信息);
            ReadDL(true, type, 1, ref powers[3]);

            //上报检定数据
            for (int j = 0; j < MeterNumber; j++)
            {
                if (!MeterInfo[j].YaoJianYn) continue;

                if (Math.Abs(powers[2][j] - powers[3][j]) < 0.01)
                    result[j] = true;
                else
                    result[j] = false;
                ResultDictionary[msg2 + "冻结前电表总电量"][j] = powers[0][j].ToString();
                ResultDictionary[msg2 + "冻结前上一次冻结总电量"][j] = powers[1][j].ToString();
                ResultDictionary[msg2 + "冻结后当前电表总电量"][j] = powers[2][j].ToString();
                ResultDictionary[msg2 + "冻结后上一次冻结总电量"][j] = powers[3][j].ToString();
            }
            RefUIData(msg2 + "冻结前电表总电量");
            RefUIData(msg2 + "冻结前上一次冻结总电量");
            RefUIData(msg2 + "冻结后当前电表总电量");
            RefUIData(msg2 + "冻结后上一次冻结总电量");

        }
        /// <summary>
        /// 检定参数是否合法
        /// </summary>
        /// <returns></returns>
        protected override bool CheckPara()
        {

            List<string> list = new List<string>();
            if (OneMeterInfo.FKType == 1)
            {
                list.Add("两套时区表切换冻结前电表总电量");
                list.Add("两套时区表切换冻结前上一次冻结总电量");
                list.Add("两套时区表切换冻结后当前电表总电量");
                list.Add("两套时区表切换冻结后上一次冻结总电量");

                list.Add("两套日时段表切换冻结前电表总电量");
                list.Add("两套日时段表切换冻结前上一次冻结总电量");
                list.Add("两套日时段表切换冻结后当前电表总电量");
                list.Add("两套日时段表切换冻结后上一次冻结总电量");

                list.Add("两套费率电价切换冻结前电表总电量");
                list.Add("两套费率电价切换冻结前上一次冻结总电量");
                list.Add("两套费率电价切换冻结后当前电表总电量");
                list.Add("两套费率电价切换冻结后上一次冻结总电量");

                list.Add("两套阶梯电价切换冻结前电表总电量");
                list.Add("两套阶梯电价切换冻结前上一次冻结总电量");
                list.Add("两套阶梯电价切换冻结后当前电表总电量");
                list.Add("两套阶梯电价切换冻结后上一次冻结总电量");
            }
            else
            {
                list.Add("两套时区表切换冻结前电表总电量");
                list.Add("两套时区表切换冻结前上一次冻结总电量");
                list.Add("两套时区表切换冻结后当前电表总电量");
                list.Add("两套时区表切换冻结后上一次冻结总电量");

                list.Add("两套日时段表切换冻结前电表总电量");
                list.Add("两套日时段表切换冻结前上一次冻结总电量");
                list.Add("两套日时段表切换冻结后当前电表总电量");
                list.Add("两套日时段表切换冻结后上一次冻结总电量");
            }
            list.Add("结论");
            ResultNames = list.ToArray();
            return true;
        }
        /// <summary>
        /// 读取电量
        /// </summary>
        /// <param name="isSpecial">true=冻结电量，false=实际电量</param>
        /// <param name="type">冻结事件类型</param>
        /// <param name="times">冻结次数</param>
        /// <param name="powers"></param>
        /// <returns></returns>
        private void ReadDL(bool isSpecial, int type, int times, ref float[] powers)
        {
            string[] dicEnergy = new string[MeterNumber];
            if (isSpecial)
            {
                if (OneMeterInfo.DgnProtocol.ProtocolName == "CDLT6452007")
                {
                    if (type == 1)
                        type = 7;//两套时区表切换
                    else if (type == 2)
                        type = 8;//两套日时段表切换
                    else if (type == 3)
                        type = 9;//两套费率电价切换
                    else if (type == 4)
                        type = 10;//两套阶梯切换
                    Dictionary<int, float[]> tmp = MeterProtocolAdapter.Instance.ReadSpecialEnergy(type, times);
                    for (int i = 0; i < MeterNumber; ++i)
                    {
                        if (!MeterInfo[i].YaoJianYn) continue;
                        dicEnergy[i] = tmp[i][0].ToString();
                    }
                }
                else if (OneMeterInfo.DgnProtocol.ProtocolName == "CDLT698")
                {
                    List<string> oad = new List<string>();
                    if (type == 1)
                        oad.Add("50080200");//两套时区表切换
                    else if (type == 2)
                        oad.Add("50090200"); //两套日时段表切换
                    else if (type == 3)
                        oad.Add("500A0200"); //两套费率电价切换
                    else if (type == 4)
                        oad.Add("500B0200");//两套阶梯切换
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
