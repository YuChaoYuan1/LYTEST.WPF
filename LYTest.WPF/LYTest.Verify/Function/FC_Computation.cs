using LYTest.Core.Enum;
using LYTest.ViewModel.CheckController;
using System;
using System.Collections.Generic;
using System.Threading;

namespace LYTest.Verify.Function
{
    /// <summary>
    /// 计量功能
    /// add lsj 20220725
    /// </summary>
    class FC_Computation : VerifyBase
    {
        private string[] charP = null;
        private string[] charQ1 = null;
        private string[] charQ2 = null;
        private string[] strJSR = null;
        public override void Verify()
        {
            base.Verify();
            if (Stop) return;
            string[] paras = Test_Value.Split('|');

            int sumNum = int.Parse(paras[3]);
            charP = new string[MeterNumber];
            charQ1 = new string[MeterNumber];
            charQ2 = new string[MeterNumber];
            //初始化设备
            if (!InitEquipment()) return;
            if (Stop) return;
            ReadMeterAddrAndNo();
            if (Stop) return;
            Identity();
            if (Stop) return;
            MessageAdd("恢复电表时间", EnumLogType.提示信息);
            MessageAdd("开始读取GPS时间...", EnumLogType.提示信息);
            DateTime readTime = DateTime.Now;//读取GPS时间
            MeterProtocolAdapter.Instance.WriteDateTime(readTime);
            if (Stop) return;
            MessageAdd("读取每月第1结算日", EnumLogType.提示信息);
            strJSR = MeterProtocolAdapter.Instance.ReadData("每月第1结算日");
            string fTime = strJSR[FirstIndex];
            for (int i = 0; i < MeterNumber; i++)
            {
                if (!MeterInfo[i].YaoJianYn) continue;
                if (strJSR[i] == null || strJSR[i] == "") continue;

                if (fTime != strJSR[i])
                {
                    MessageAdd("有表位每月第1结算日不一致，试验终止", EnumLogType.错误信息);
                    Stop = true;
                    break;
                }
            }

            if (Stop) return;
            //读取组合有功特征字
            if (paras[0] == "是")
            {
                MessageAdd("读取组合有功特征字", EnumLogType.提示信息);
                string[] readData = MeterProtocolAdapter.Instance.ReadData("有功组合方式特征字");

                #region 有功组合方式特征字
                for (int i = 0; i < MeterNumber; i++)
                {
                    if (!MeterInfo[i].YaoJianYn) continue;
                    if (readData[i] == null || readData[i] == "") continue;

                    byte cBit = Convert.ToByte(readData[i], 16);
                    if (OneMeterInfo.DgnProtocol.ProtocolName == "CDLT698")
                        cBit = Core.Function.Funs.BitRever(cBit);

                    charP[i] = "";

                    if ((cBit & 0x01) == 0x01)
                        charP[i] = "(P+)";
                    if ((cBit & 0x02) == 0x02)
                        charP[i] += "-(P+)";
                    if ((cBit & 0x04) == 0x04)
                        charP[i] += "+(P-)";
                    if ((cBit & 0x08) == 0x08)
                        charP[i] += "-(P-)";

                    string tmp = Convert.ToString(cBit, 2).PadLeft(8, '0');

                    if (tmp.Substring(tmp.Length - 1, 1) == "1")
                    {
                        charP[i] = "(P+)";
                    }

                    if (tmp.Substring(tmp.Length - 3, 1) == "1")
                    {
                        if (charP[i].Length == 0)
                            charP[i] = "(P-)";
                        else
                            charP[i] += "+(P-)";
                    }
                    if (tmp.Substring(tmp.Length - 2, 1) == "1")
                    {
                        charP[i] += "-(P+)";
                    }

                    if (tmp.Substring(tmp.Length - 4, 1) == "1")
                    {
                        charP[i] += "-(P-)";
                    }
                    charP[i] = charP[i].Trim('-');
                }
                #endregion
            }

            if (Stop) return;
            //读取组合无功1/2特征字
            if (paras[1] == "是")
            {
                MessageAdd("读取组合无功1特征字", EnumLogType.提示信息);
                string[] readData = MeterProtocolAdapter.Instance.ReadData("无功组合方式1特征字");
                #region 无功组合方式1特征字
                for (int i = 0; i < MeterNumber; i++)
                {
                    if (!MeterInfo[i].YaoJianYn) continue;

                    if (readData[i] == null || readData[i] == "") continue;

                    byte cBit = Convert.ToByte(readData[i], 16);
                    if (OneMeterInfo.DgnProtocol.ProtocolName == "CDLT698")
                        cBit = Core.Function.Funs.BitRever(cBit);

                    charQ1[i] = "";
                    if ((cBit & 0x01) == 0x01)
                        charQ1[i] = "(Q1)";

                    if ((cBit & 0x04) == 0x04)
                        charQ1[i] += "+(Q2)";

                    if ((cBit & 0x10) == 0x10)
                        charQ1[i] += "+(Q3)";

                    if ((cBit & 0x40) == 0x40)
                        charQ1[i] += "+(Q4)";

                    if ((cBit & 0x02) == 0x02)
                        charQ1[i] += "-(Q1)";

                    if ((cBit & 0x08) == 0x08)
                        charQ1[i] += "-(Q2)";

                    if ((cBit & 0x20) == 0x20)
                        charQ1[i] += "-(Q3)";
                    else if ((cBit & 0x80) == 0x80)
                        charQ1[i] += "-(Q4)";

                    charQ1[i] = charQ1[i].Trim('+');
                }
                #endregion
                if (Stop) return;
                MessageAdd("读取组合无功2特征字", EnumLogType.提示信息);
                readData = MeterProtocolAdapter.Instance.ReadData("无功组合方式2特征字");
                #region 无功组合方式2特征字
                for (int i = 0; i < MeterNumber; i++)
                {
                    if (!MeterInfo[i].YaoJianYn) continue;

                    if (readData[i] == null || readData[i] == "") continue;

                    byte cBit = Convert.ToByte(readData[i], 16);

                    if (OneMeterInfo.DgnProtocol.ProtocolName == "CDLT698")
                        cBit = Core.Function.Funs.BitRever(cBit);

                    charQ2[i] = "";
                    if ((cBit & 0x01) == 0x01)
                        charQ2[i] = "(Q1)";

                    if ((cBit & 0x04) == 0x04)
                        charQ2[i] += "+(Q2)";

                    if ((cBit & 0x10) == 0x10)
                        charQ2[i] += "+(Q3)";

                    if ((cBit & 0x40) == 0x40)
                        charQ2[i] += "+(Q4)";

                    if ((cBit & 0x02) == 0x02)
                        charQ2[i] += "-(Q1)";

                    if ((cBit & 0x08) == 0x08)
                        charQ2[i] += "-(Q2)";

                    if ((cBit & 0x20) == 0x20)
                        charQ2[i] += "-(Q3)";

                    else if ((cBit & 0x80) == 0x80)
                        charQ2[i] += "-(Q4)";

                    charQ2[i] = charQ2[i].Trim('+');
                }
                #endregion
            }

            bool[] bResult = ReadEnergy(paras[0], paras[1], 0);

            int seconds = (int)(Convert.ToSingle(paras[2]) * 60f);
            for (int curNum = 1; curNum <= sumNum; curNum++)
            {
                if (Stop) return;
                //有功走字
                if (paras[0] == "是")
                {
                    if (Stop) return;
                    Walk(seconds, PowerWay.正向有功);
                    if (Stop) return;
                    Walk(seconds, PowerWay.反向有功);
                }

                //无功走字
                if (paras[1] == "是")
                {
                    if (Stop) return;
                    Walk(seconds, PowerWay.第一象限无功);
                    if (Stop) return;
                    Walk(seconds, PowerWay.第二象限无功);
                    if (Stop) return;
                    Walk(seconds, PowerWay.第三象限无功);
                    if (Stop) return;
                    Walk(seconds, PowerWay.第四象限无功);
                }

                #region 将电表时间修改结算日前1分钟
                if (Stop) return;
                Identity(true);

                if (Stop) return;
                MessageAdd("将电表时间修改到结算日前30秒", EnumLogType.提示信息);
                string ft = strJSR[FirstIndex]; //结算日
                ft = string.Format("{0:D2}{1:D2}{2}0000", DateTime.Now.Year, curNum, ft);
                DateTime frzTime = LYTest.Core.Function.DateTimes.FormatStringToDateTime(ft).AddSeconds(-30);

                bool[] result = MeterProtocolAdapter.Instance.WriteDateTime(frzTime);
                bool br = true;
                for (int i = 0; i < MeterNumber; i++)
                {
                    if (MeterInfo[i].YaoJianYn)
                    {
                        if (!result[i])
                            br = false;
                    }
                }
                if (!br) MessageAdd("有电能表写表时间失败!请检查多功能协议配置、表接线或是表编程开关是否已经打开。\r\n当前检定中止", EnumLogType.错误信息);


                WaitTime(string.Format("第{0}次运行过每月第1结算日", curNum), 120);

                #endregion
                if (Stop) return;
                bResult = ReadEnergy(paras[0], paras[1], curNum);

            }

            if (Stop) return;
            Identity(false);

            if (Stop) return;
            MessageAdd("恢复电表时间", EnumLogType.提示信息);
            MessageAdd("开始读取GPS时间...", EnumLogType.提示信息);
            readTime = DateTime.Now;
            //读取GPS时间
            MeterProtocolAdapter.Instance.WriteDateTime(readTime);

            //刷新结论
            for (int i = 0; i < MeterNumber; i++)
            {
                if (!MeterInfo[i].YaoJianYn) continue;

                ResultDictionary["结论"][i] = bResult[i] ? "合格" : "不合格";
            }
            RefUIData("结论");
        }

        private void Walk(int seconds, PowerWay pWay)
        {
            string strGlys = "";
            switch (pWay)
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
            float xib = OneMeterInfo.GetIb()[0];
            if (PowerOn(OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, xib, xib, xib, Cus_PowerYuanJian.H, pWay, strGlys) == false)
            {
                MessageAdd("控制源输出失败", EnumLogType.提示信息);
                return;
            }
            Thread.Sleep(300);

            WaitTime(Enum.GetName(typeof(PowerWay), pWay) + "走字", seconds);

            if (Stop) return;
            PowerOn();
            Thread.Sleep(5000);
            return;
        }

        private bool[] ReadEnergy(string paramP, string paramQ, int curTime)
        {
            bool[] bResult = new bool[MeterNumber];
            for (int i = 0; i < MeterNumber; i++)
            {
                bResult[i] = true;
            }

            string bitFun = "";
            //if (param == "组合有功")     //+有功组合 -无功组合
            //    bitFun = "111000000";
            //else if (param == "组合无功")     //-有功组合 +无功组合
            //    bitFun = "000111111";


            // add 组合有功默认显示正向有功电量  组合无功默认显示反向有功

            //if (param == "组合有功")     //+有功组合 -无功组合
            //    bitFun = "100000000";
            //else if (param == "组合无功")     //-有功组合 +无功组合
            //    bitFun = "000100000";

            //modify
            if (paramP == "是" && paramQ == "是")          //+有功组合 +无功组合
                bitFun = "111111111";
            else if (paramP == "是" && paramQ == "否")     //+有功组合 -无功组合
                bitFun = "111000000";
            else if (paramP == "否" && paramQ == "是")     //-有功组合 +无功组合
                bitFun = "000111111";
            else if (paramP == "否" && paramQ == "否")     //-有功组合 -无功组合
                bitFun = "000000000";

            for (int i = 0; i <= 8; i++)
            {
                if (Stop) return bResult;

                if (bitFun[i] == '0') continue;
                PowerWay pw = (PowerWay)i;

                string msg;
                if (curTime == 0)
                    msg = string.Format("读取【当前{0}】电量", pw.ToString());
                else
                    msg = string.Format("读取【上{0}结算日{1}】电量", curTime, pw.ToString());
                MessageAdd(msg, EnumLogType.提示信息);
                //Dictionary<int, float[]> dicEnergy = MeterProtocolAdapter.Instance.ReadEnergysGJD((byte)i, curTime);
                Dictionary<int, float[]> dicEnergy = MeterProtocolAdapter.Instance.ReadEnergys((byte)i, curTime);
                for (int j = 0; j < MeterNumber; j++)
                {
                    if (!MeterInfo[j].YaoJianYn) continue;
                    if (dicEnergy.ContainsKey(j) == false) continue;
                    //string value = "||||";
                    //if (dicEnergy[j] != null && dicEnergy[j].Length >= 5)
                    //    value = string.Format("{0}|{1}|{2}|{3}|{4}", dicEnergy[j][0], dicEnergy[j][1], dicEnergy[j][2], dicEnergy[j][3], dicEnergy[j][4]);

                    string gl = "";
                    if (pw.ToString() == "正向无功")
                        gl = "组合无功1";
                    if (pw.ToString() == "反向无功")
                        gl = "组合无功2";

                    if (curTime == 0)
                    {
                        if (i == 0) //显示组合有功的数据
                        {
                            ResultDictionary["每月第一结算日"][j] = strJSR[j];
                            ResultDictionary["组合有功特征字"][j] = charP[j];
                            ResultDictionary["组合无功1特征字"][j] = charQ1[j];
                            ResultDictionary["组合无功2特征字"][j] = charQ2[j];
                            ResultDictionary["总"][j] = dicEnergy[0][0].ToString();
                            ResultDictionary["尖"][j] = dicEnergy[0][1].ToString();
                            ResultDictionary["峰"][j] = dicEnergy[0][2].ToString();
                            ResultDictionary["平"][j] = dicEnergy[0][3].ToString();
                            ResultDictionary["谷"][j] = dicEnergy[0][4].ToString();
                        }

                        MessageAdd(string.Format("第【{9}】表位 当前【{10}】结果：每月第一结算日：{0}，组合有功特征字：{1}，组合无功1特征字：{2}，组合无功2特征字：{3}，总：{4}，尖：{5}，峰：{6}，平：{7}，谷：{8}",
                            strJSR[j], charP[j], charQ1[j], charQ2[j], dicEnergy[j][0].ToString(), dicEnergy[j][1].ToString(), dicEnergy[j][2].ToString(), dicEnergy[j][3].ToString(), dicEnergy[j][4].ToString(), j,
                            gl != "" ? gl : pw.ToString()), EnumLogType.提示与流程信息);
                    }
                    else
                    {
                        ResultDictionary[$"上{curTime}次总"][j] = dicEnergy[0][0].ToString();
                        ResultDictionary[$"上{curTime}次尖"][j] = dicEnergy[0][1].ToString();
                        ResultDictionary[$"上{curTime}次峰"][j] = dicEnergy[0][2].ToString();
                        ResultDictionary[$"上{curTime}次平"][j] = dicEnergy[0][3].ToString();
                        ResultDictionary[$"上{curTime}次谷"][j] = dicEnergy[0][4].ToString();

                        MessageAdd(string.Format($"第【{j}】表位 上{curTime}次【{pw.ToString()}】结果：上{curTime}次总：{dicEnergy[j][0].ToString()}，" +
                            $"上{curTime}次尖：{dicEnergy[j][1].ToString()}，上{curTime}次峰：{dicEnergy[j][2].ToString()}，" +
                            $"上{curTime}次平：{dicEnergy[j][3].ToString()}，上{curTime}次谷：{dicEnergy[j][4].ToString()}"), EnumLogType.提示与流程信息);
                    }
                }

                if (curTime == 0)
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
                    RefUIData($"上{curTime}次总");
                    RefUIData($"上{curTime}次尖");
                    RefUIData($"上{curTime}次峰");
                    RefUIData($"上{curTime}次平");
                    RefUIData($"上{curTime}次谷");
                }
            }

            for (int i = 0; i < MeterNumber; i++)
            {
                if (!MeterInfo[i].YaoJianYn) continue;

                if (paramP == "是")
                {
                    ResultDictionary["组合有功"][i] = bResult[i] ? "合格" : "不合格";
                    ResultDictionary["正向有功"][i] = bResult[i] ? "合格" : "不合格";
                    ResultDictionary["反向有功"][i] = bResult[i] ? "合格" : "不合格";
                    ResultDictionary["正向分项有功"][i] = bResult[i] ? "合格" : "不合格";
                    ResultDictionary["反向分项有功"][i] = bResult[i] ? "合格" : "不合格";

                    RefUIData("组合有功");
                    RefUIData("正向有功");
                    RefUIData("反向有功");
                    RefUIData("正向分项有功");
                    RefUIData("反向分项有功");
                }

                if (paramQ == "是")
                {
                    ResultDictionary["组合无功1"][i] = bResult[i] ? "合格" : "不合格";
                    ResultDictionary["组合无功2"][i] = bResult[i] ? "合格" : "不合格";
                    ResultDictionary["无功一象限"][i] = bResult[i] ? "合格" : "不合格";
                    ResultDictionary["无功二象限"][i] = bResult[i] ? "合格" : "不合格";
                    ResultDictionary["无功三象限"][i] = bResult[i] ? "合格" : "不合格";
                    ResultDictionary["无功四象限"][i] = bResult[i] ? "合格" : "不合格";

                    RefUIData("组合无功1");
                    RefUIData("组合无功2");
                    RefUIData("无功一象限");
                    RefUIData("无功二象限");
                    RefUIData("无功三象限");
                    RefUIData("无功四象限");
                }

                ResultDictionary["结论"][i] = bResult[i] ? "合格" : "不合格";
            }
            if (Stop) return bResult;

            return bResult;
        }
        protected override bool CheckPara()
        {
            ResultNames = new string[] { "每月第一结算日", "组合有功特征字", "组合无功1特征字", "组合无功2特征字", "总", "尖", "峰", "平", "谷", "上1次总", "上1次尖", "上1次峰", "上1次平", "上1次谷", "组合有功", "正向有功", "反向有功", "正向分项有功", "反向分项有功", "组合无功1", "组合无功2", "无功一象限", "无功二象限", "无功三象限", "无功四象限", "结论" };
            return true;
        }
        /// <summary>
        /// 初始化设备参数,计算每一块表需要检定的圈数
        /// </summary>
        /// <returns></returns>
        private bool InitEquipment()
        {
            MessageAdd("开始升电压...", EnumLogType.提示信息);
            if (Stop) return false;
            if (!PowerOn())
                MessageAdd("升电压失败! ", EnumLogType.提示信息);
            WaitTime("升源成功，等待源稳定", 5);
            return true;
        }


    }
}
