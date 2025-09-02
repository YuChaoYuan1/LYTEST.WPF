using LYTest.Core;
using LYTest.Core.Enum;
using LYTest.Core.Function;
using LYTest.ViewModel;
using LYTest.ViewModel.CheckController;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace LYTest.Verify.Multi
{
    ///add lsj 20220218 测量及监测误差试验
    /// <summary>    
    /// 测量及监测误差试验
    /// </summary>
    public class Dgn_MeasureAndMonitor : VerifyBase
    {
        private Dictionary<int, string> testScheme = new Dictionary<int, string>();
        public override void Verify()
        {
            base.Verify();

            string[] jg = new string[MeterNumber];
            jg.Fill("合格");

            //string[] resultKeys = new string[MeterNumber];
            //string[] resultKeyTmps = new string[MeterNumber];

            if (!PowerOn(OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, 0, 0, 0, Cus_PowerYuanJian.H, FangXiang, "1.0"))
            {
                MessageAdd("升源失败,退出检定", EnumLogType.提示信息);
                return;
            }

            //获取检定状态 
            if (Stop) return;
            //身份认证
            ReadMeterAddrAndNo();

            if (Stop) return;

            //delete yjt 20230109 不需要身份认证
            //Identity(true);

            if (Stop) return;
            //for (int i = 5; i < 11; i++)

            for (int i = 1; i < 11; i++)
            {
                //add yjt 20220812 获取检定项名称
                //获取检定项名称
                string name = GetName(testScheme[i]);
                MessageAdd("正在进行" + name, EnumLogType.提示与流程信息);

                //升源输出
                if (Stop) return;
                PowerOn(testScheme[i]);
                if (Stop) return;

                //等待源输出稳定
                MessageAdd("等待功率源稳定....", EnumLogType.提示与流程信息);
                //Thread.Sleep(App.UserSetting.PowerOnWaitTime * 1000);  //todo多功能检定时间

                if (Stop) return;
                //首先读取电表的值
                string[] meterData;
                MessageAdd("读取电表的参数值....", EnumLogType.提示与流程信息);
                if (!ReadMeterData(testScheme[i], out meterData)) continue; //读取表电压，电流，功率，功率因数

                if (Stop) return;
                //再次读取标准表的值
                MessageAdd("读取标准表数据....", EnumLogType.提示与流程信息);

                //计算相应的误差
                MessageAdd("开始计算误差数据....", EnumLogType.提示与流程信息);
                for (int j = 0; j < MeterNumber; j++)
                {
                    if (!MeterInfo[j].YaoJianYn) continue;

                    string mValue = meterData[j];   //表值
                    //Trace.Assert(false, $"1_{mValue}");
                    //获取相应的读取标准值 并且计算误差值
                    string stdValue = GetStdAndWcData(testScheme[i], mValue, EquipmentData.StdInfo, out _, out string wcValue);
                    ResultDictionary[name][j] = mValue + "|" + stdValue + "|" + wcValue;
                    //Trace.Assert(false, $"2_{mValue}|{stdValue}|{wcValue}");

                    float level = MeterLevel(MeterInfo[j]);

                    //Trace.Assert(false, $"3_{level}");
                    //string result = string.Empty;
                    //for (int k = 0; k < new List<double>().Count; k++)
                    //{
                    //Trace.Assert(false, $"4_不会进来");
                    //检定结果 节点结论
                    //if (result.Equals("不合格")) break;

                    //if (double.IsNaN(new List<double>()[k]))
                    //    new List<double>()[k] = 0;


                    if (jg[j] != ConstHelper. 不合格)
                        jg[j] = Math.Abs(Convert.ToSingle(wcValue)) < level ? ConstHelper.合格 : ConstHelper.不合格;

                    ResultDictionary["结论"][j] = jg[j];

                    //}
                    Thread.Sleep(200);
                }

                RefUIData(name);
                RefUIData("结论");

                if (Stop) return;
                //通知刷型数据。
                MessageAdd("开始关源....", EnumLogType.提示与流程信息);
                //关源
                if (!PowerOn(OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, 0, 0, 0, Cus_PowerYuanJian.H, FangXiang, "1.0"))
                {
                    MessageAdd("升源失败,退出检定", EnumLogType.提示与流程信息);
                    return;
                }

                //for (int o = 0; o < 5; o++)
                //{
                //    Thread.Sleep(1000);
                //}
                if (Stop) return;

            }
            //存储相应的数据到多功能模块
            MessageAdd("开始存储数据....", EnumLogType.提示信息);
            for (int i = 0; i < MeterNumber; i++)//判定总结论
            {
                if (!MeterInfo[i].YaoJianYn) continue;
                //检定结果 最终结论

                if (jg[i] == "不合格")
                {
                    ResultDictionary["结论"][i] = "不合格";
                }
                else
                {
                    ResultDictionary["结论"][i] = "合格";
                }
            }
            RefUIData("结论");

            MessageAdd("测量及检测误差实验检定完毕", EnumLogType.提示信息);
            Thread.Sleep(200);
        }

        /// <summary>
        /// 检定参数是否合法
        /// </summary>
        /// <returns></returns>
        protected override bool CheckPara()
        {
            //float aa = 1.0f;
            //string aastr = aa.ToString("0.0");
            LoadScheme();
            ResultNames = new string[] { "120%Un", "100%Un", "60%Un", "120%Imax", "1000%Itr", "100%Imin", "120%Un_120%Imax_1", "100%Un_1000%Itr_1", "100%Un_4%Itr_1", "05L", "结论" };
            //ResultNames = new string[] { "120%Un", "100%Un", "60%Un", "120%Imax", "100%Ib", "5%Ib", "120%Un/120%Ib/" + aastr, "100%Un/100%Ib/" + aastr, "100%Un/0.4%Ib/" + aastr, aastr, "结论" };
            //ResultNames = new string[] { "120%Un", "100%Un", "60%Un", "120%Imax", "100%Ib", "5%Ib", "120%Un_120%Ib_1.0", "100%Un/100%Ib/1.0", "100%Un/0.4%Ib/1.0", "1.0", "结论" };
            //ResultNames = new string[] { "120%Un", "100%Un", "60%Un", "120%Imax", "100%Ib", "5%Ib", "120%Un120%IbP", "100%Un100%IbP", "100%Un零点4%IbP", "C", "结论" };
            //ResultNames = new string[] { "120%Un", "100%Un", "60%Un", "120%Imax", "100%Ib", "5%Ib", "120%Un/120%Ib/1.0", "100%Un/100%Ib/1.0", "100%Un/0.4%Ib/1.0", "1.0", "结论" };


            return true;
        }


        private string GetStdAndWcData(string param, string meterValue, ViewModel.Monitor.StdInfoViewModel tagParm, out List<double> wcList, out string wcValue)
        {
            string value = string.Empty;
            wcList = new List<double>();
            wcValue = string.Empty;
            string[] parms = param.Split('|');
            if (parms.Length <= 0 || string.IsNullOrEmpty(meterValue))
                return string.Empty;

            switch (parms[0])
            {
                case "Un":
                    {
                        if (tagParm.Ua == 0) return string.Empty;

                        string[] strUns = meterValue.Split(',');

                        //UA
                        float wc0 = (Convert.ToSingle(strUns[0]) - tagParm.Ua) / tagParm.Ua;
                        wcList.Add(wc0);
                        wcValue = wc0.ToString("F2");
                        value = tagParm.Ua.ToString("F4");
                        if (strUns.Length > 1 && strUns[1] != "FFF.F")
                        {
                            float wc1 = (Convert.ToSingle(strUns[1]) - tagParm.Ub) / tagParm.Ub;//UB
                            float wc2 = (Convert.ToSingle(strUns[2]) - tagParm.Uc) / tagParm.Uc;//UC
                            //添加出参
                            wcList.Add(wc1);
                            wcList.Add(wc2);

                            wcValue += "," + wc1.ToString("F2") + "," + wc2.ToString("F2");//误差值
                            value += "," + tagParm.Ub.ToString("F4") + "," + tagParm.Uc.ToString("F4");//标准值
                        }
                    }
                    break;
                case "Imax":
                case "Ib":
                case "Itr":
                case "Imin":
                    {
                        if (tagParm.Ia == 0) return string.Empty;

                        string[] curs = meterValue.Split(',');

                        //UA
                        float wc0 = (Convert.ToSingle(curs[0]) - tagParm.Ia) / tagParm.Ia;
                        if (double.IsNaN(wc0)) wc0 = 0;

                        wcList.Add(wc0);
                        value = tagParm.Ia.ToString("F4");
                        wcValue = wc0.ToString("F2");

                        if (curs.Length > 1 && curs[1] != "FFF.F")
                        {
                            float wc1 = (Convert.ToSingle(curs[1]) - tagParm.Ib) / tagParm.Ib;//UB
                            float wc2 = (Convert.ToSingle(curs[2]) - tagParm.Ic) / tagParm.Ic;//UC

                            if (double.IsNaN(wc1)) wc1 = 0;
                            if (double.IsNaN(wc2)) wc2 = 0;

                            //添加出参
                            wcList.Add(wc1);
                            wcList.Add(wc2);

                            wcValue += "," + wc1.ToString("F2") + "," + wc2.ToString("F2");//误差值
                            value += "," + tagParm.Ib.ToString("F4") + "," + tagParm.Ic.ToString("F4");//标准值
                        }
                    }
                    break;
                case "P":
                    {
                        if (tagParm.P == 0f) return string.Empty;


                        float fPwc = -100;
                        //if() 

                        float stmP = tagParm.P / 1000;

                        if (meterValue != "0")
                            fPwc = (Convert.ToSingle(meterValue) - stmP) / stmP;

                        if (double.IsNaN(fPwc)) fPwc = 0;
                        wcList.Add(fPwc);
                        wcValue = fPwc.ToString("F2");
                        value = stmP.ToString("F4");
                    }
                    break;
                case "C":
                    {
                        float fCOSwc = Convert.ToSingle(meterValue) - tagParm.PF;
                        //float fCOSwc = Convert.ToSingle(meterValue) - (float)Math.Cos(tagParm.PF);
                        wcList.Add(fCOSwc);
                        wcValue = fCOSwc.ToString("F2");

                        value = Math.Cos(tagParm.PF).ToString("f2");
                    }
                    break;
                default:
                    break;
            }

            return value;
        }


        private bool ReadMeterData(string para, out string[] outValue)
        {
            outValue = new string[MeterNumber];
            string[] parms = para.Split('|');
            if (parms.Length <= 0) return false;

            switch (parms[0])
            {
                case "Un":
                    outValue = MeterProtocolAdapter.Instance.ReadData("电压数据块");

                    break;
                case "Imax":
                case "Ib":
                case "Itr":
                case "Imin":
                    outValue = MeterProtocolAdapter.Instance.ReadData("电流数据块");
                    break;
                case "P":
                    outValue = MeterProtocolAdapter.Instance.ReadData("瞬时总有功功率");

                    break;
                case "C":
                    outValue = MeterProtocolAdapter.Instance.ReadData("总功率因数");
                    break;
                default:
                    break;
            }
            return true;
        }

        /// <summary>
        /// 升源
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        private bool PowerOn(string param)
        {
            CheckOver = false;

            float volt = OneMeterInfo.MD_UB;
            float curr = 0f;
            float testI = 0f;
            string glys = "1.0";

            string[] ps = param.Split('|');
            if (ps.Length <= 0) return false;

            float[] IbImax = OneMeterInfo.GetIb();
            //float testI = Number.GetCurrentByIb(ps[0], OneMeterInfo.MD_UA, HGQ);   //获取走字的电流
            //switch (ps[0])
            //{
            //    case "Un":
            //        volt = Convert.ToSingle(ps[1]) / 100 * OneMeterInfo.MD_UB;
            //        break;
            //    case "Imax":
            //        volt = OneMeterInfo.MD_UB;
            //        curr = Convert.ToSingle(ps[1]) / 100 * IbImax[1];
            //        break;
            //    case "Ib":
            //        volt = OneMeterInfo.MD_UB;
            //        curr = Convert.ToSingle(ps[1]) / 100 * IbImax[0];
            //        break;
            //    case "Itr":
            //        volt = OneMeterInfo.MD_UB;
            //        curr = Convert.ToSingle(ps[1]) / 100 * IbImax[0];
            //        break;
            //    case "Imin":
            //        volt = OneMeterInfo.MD_UB;
            //        curr = Convert.ToSingle(ps[1]) / 100 * IbImax[0];//Imin值暂时用的Ib值                    
            //        break;
            //    case "P":
            //        volt = Convert.ToSingle(ps[1]) / 100 * OneMeterInfo.MD_UB;
            //        curr = Convert.ToSingle(ps[2]) / 100 * IbImax[0];
            //        if (ps[4].Equals("Imax"))
            //        {
            //            curr = Convert.ToSingle(ps[2]) / 100 * IbImax[1];
            //        }
            //        glys = ps[3];
            //        break;
            //    case "C":
            //        glys = ps[1];
            //        curr = IbImax[0];
            //        break;
            //    default:
            //        break;
            //}

            switch (ps[0])
            {
                case "Un":
                    volt = Convert.ToSingle(ps[1]) / 100 * OneMeterInfo.MD_UB;
                    break;
                case "Imax":
                case "Ib":
                case "Itr":
                case "Imin":
                    volt = OneMeterInfo.MD_UB;
                    testI = Number.GetCurrentByIb(ps[0], OneMeterInfo.MD_UA, HGQ);   //获取走字的电流
                    curr = Convert.ToSingle(ps[1]) / 100 * testI;
                    break;
                case "P":
                    volt = Convert.ToSingle(ps[1]) / 100 * OneMeterInfo.MD_UB;
                    testI = Number.GetCurrentByIb(ps[4], OneMeterInfo.MD_UA, HGQ);   //获取走字的电流
                    curr = Convert.ToSingle(ps[2]) / 100 * testI;
                    glys = ps[3];
                    break;
                case "C":
                    glys = ps[1];
                    curr = IbImax[0];
                    break;
                default:
                    break;
            }

            if (PowerOn(volt, volt, volt, curr, curr, curr, Cus_PowerYuanJian.H, FangXiang, glys) == false)//
            {
                MessageAdd("控制源输出失败", EnumLogType.提示信息);
                return false;
            }

            return true;
        }




        //获取检定项名称
        private string GetName(string name)
        {
            //if (OneMeterInfo.MD_JJGC == "IR46")
            //{
            switch (name)
            {
                case "Un|120":
                    return "120%Un";
                case "Un|100":
                    return "100%Un";
                case "Un|60":
                    return "60%Un";
                case "Imax|120":
                    return "120%Imax";
                case "Itr|1000":
                    return "1000%Itr";
                case "Imin|100":
                    return "100%Imin";
                case "P|120|120|1.0|Imax":
                    return "120%Un_120%Imax_1";
                case "P|100|1000|1.0|Itr":
                    return @"100%Un_1000%Itr_1";
                case "P|100|4|1.0|Itr":
                    return "100%Un_4%Itr_1";
                case "C|0.5L":
                    return "05L";
                default:
                    return "";
            }
            //}
            //else
            //{
            //switch (name)
            //{
            //    case "Un|120":
            //        return "120%Un";
            //    case "Un|100":
            //        return "100%Un";
            //    case "Un|60":
            //        return "60%Un";
            //    case "Imax|120":
            //        return "120%Imax";
            //    case "Ib|100":
            //        return "100%Ib";
            //    case "Ib|5":
            //        return "5%Ib";
            //    case "P|120|120|1.0|Imax":
            //        return "120%Un/120%Ib/1.0";
            //    case "P|100|100|1.0|Ib":
            //        return "100%Un/100%Ib/1.0";
            //    case "P|100|0.4|1.0|Ib":
            //        return "100%Un/0.4%Ib/1.0";
            //    case "C|1.0":
            //        return "1.0";
            //    default:
            //        return "";
            //}
            //}


            //switch (name)
            //{
            //    case "Un|120":
            //        return "120%Un";
            //    case "Un|100":
            //        return "100%Un";
            //    case "Un|60":
            //        return "60%Un";
            //    case "Imax|120":
            //        return "120%Imax";
            //    case "Ib|100":
            //        return "100%Ib";
            //    case "Ib|5":
            //        return "5%Ib";
            //    case "P|120|120|1.0|Imax":
            //        return "120%Un120%IbP";
            //    case "P|100|100|1.0|Ib":
            //        return "100%Un100%IbP";
            //    case "P|100|0.4|1.0|Ib":
            //        return "100%Un零点4%IbP";
            //    case "C|1.0":
            //        return "C";
            //    default:
            //        return "";
            //}
        }

        //add yjt 20220811 新增
        private void LoadScheme()
        {
            string Qddl = "0.4";

            for (int j = 0; j < MeterNumber; j++)
            {
                if (!MeterInfo[j].YaoJianYn) continue;

                string[] level = Number.GetDj(OneMeterInfo.MD_Grane);
                string Bdj = level[base.IsYouGong ? 0 : 1].ToString();  //当前表的等级

                if (OneMeterInfo.MD_ConnectionFlag != "直接式")
                {

                    switch (Bdj)
                    {
                        case "0.2":
                        case "0.2S":
                            Qddl = "0.1";
                            break;
                        case "0.5":
                        case "0.5S":
                            Qddl = "0.1";
                            break;
                        case "1":
                            Qddl = "0.2";
                            break;
                        default:
                            Qddl = "0.4";
                            break;
                    }
                }
                break;
            }

            //if (OneMeterInfo.MD_JJGC == "IR46")
            //{
            for (int i = 1; i < 11; i++)
            {
                switch (i)
                {
                    case 1:
                        testScheme.Add(i, "Un|120");
                        break;
                    case 2:
                        testScheme.Add(i, "Un|100");
                        break;
                    case 3:
                        testScheme.Add(i, "Un|60");
                        break;
                    case 4:
                        testScheme.Add(i, "Imax|120");
                        break;
                    case 5:
                        testScheme.Add(i, "Itr|1000");
                        break;
                    case 6:
                        testScheme.Add(i, "Imin|100");
                        break;
                    case 7:
                        testScheme.Add(i, "P|120|120|1.0|Imax");
                        break;
                    case 8:
                        testScheme.Add(i, "P|100|1000|1.0|Itr");
                        break;
                    case 9:
                        testScheme.Add(i, "P|100|4|1.0|Itr");
                        break;
                    case 10:
                        testScheme.Add(i, "C|0.5L");
                        break;
                    default:
                        break;
                }
            }
            //}
            //else
            //{
            //for (int i = 1; i < 11; i++)
            //{
            //    switch (i)
            //    {
            //        case 1:
            //            testScheme.Add(i, "Un|120");
            //            break;
            //        case 2:
            //            testScheme.Add(i, "Un|100");
            //            break;
            //        case 3:
            //            testScheme.Add(i, "Un|60");
            //            break;
            //        case 4:
            //            testScheme.Add(i, "Imax|120");
            //            break;
            //        case 5:
            //            testScheme.Add(i, "Ib|100");
            //            break;
            //        case 6:
            //            testScheme.Add(i, "Ib|5");
            //            break;
            //        case 7:
            //            testScheme.Add(i, "P|120|120|1.0|Imax");
            //            break;
            //        case 8:
            //            testScheme.Add(i, "P|100|100|1.0|Ib");
            //            break;
            //        case 9:
            //            testScheme.Add(i, "P|100|" + Qddl + "|1.0|Ib");
            //            break;
            //        case 10:
            //            testScheme.Add(i, "C|0.5L");
            //            break;
            //        default:
            //            break;
            //    }
            //}
            //}
        }
    }

}