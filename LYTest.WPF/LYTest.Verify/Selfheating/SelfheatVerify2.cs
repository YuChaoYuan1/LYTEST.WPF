using LYTest.Core;
using LYTest.Core.Function;
using LYTest.ViewModel.CheckController;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LYTest.Verify.Selfheating
{
    /// <summary>
    /// 自热试验
    /// </summary>


    public class SelfheatVerify2 : VerifyBase
    {
        /// <summary>
        /// 预热时间--分钟
        /// </summary>
        int PreheatTime = 120;

        /// <summary>
        /// 持续时间,分钟
        /// </summary>
        readonly int RunTime = 60;
        /// <summary>
        /// 误差限
        /// </summary>
        float Limet = 1f;

        /// <summary>
        /// 暂停的时间，免得出太多误差了,ms
        /// </summary>
        readonly int stopTime = 10000;

        /// <summary>
        /// 采用的判断时间,分钟
        /// </summary>
        readonly int okTime = 20;


        public override void Verify()
        {
            base.Verify();
            try
            {
                // 升电压
                if (!PowerOn())
                {
                    MessageAdd("升源失败", EnumLogType.错误信息);
                    RefNoResoult();
                    return;
                }
                WaitTime("正在升源", 5);

                WaitTime("正在预热中", PreheatTime * 60);
                ResultDictionary["结论"].Fill(ConstHelper.合格);
                for (int index = 0; index < 2; index++)
                {
                    string name = "10误差变化值";
                    int qs = 40;
                    string glys = "1.0";
                    string dlbs = "Imax";
                    if (index > 0)
                    {
                        name = "05L误差变化值";
                        qs = 20;
                        glys = "0.5L";
                        dlbs = "Imax";

                        PowerOn();
                        //WaitTime("第二次参数测试自热等待", 180);
                    }

                    if (Stop) return;
                    //预热完成,开始误差
                    MessageAdd("正在初始化误差参数", EnumLogType.提示信息);
                    SetBluetoothModule(0);
                    if (!ErrorInitEquipment(Core.Enum.PowerWay.正向有功, Core.Enum.Cus_PowerYuanJian.H, glys, dlbs, qs))//圈数大一点，保证误差准确
                    {
                        RefNoResoult();
                        MessageAdd("误差参数失败", EnumLogType.错误信息);
                        return;
                    }
                    ViewModel.EquipmentData.DeviceManager.SetPowerSafe(false);
                    if (Stop) return;
                    MessageAdd("正在启动误差版...", EnumLogType.提示信息);
                    if (!StartWcb(0, 0xff))
                    {
                        MessageAdd("误差板启动失败...", EnumLogType.提示信息);
                        return;
                    }
                    if (Stop) return;

                    MessageAdd("开始检定...", EnumLogType.提示信息);

                    DateTime TmpTime1 = DateTime.Now;//检定开始时间，用于判断是否超时
                    int MaxTime = RunTime * 60 * 1000;
                    int needTime = 60 * 60 * 1000;//规程至少1h
                    int OkTime = okTime * 60 * 1000;
                    string[] curWC = new string[MeterNumber];   //重新初始化本次误差
                    int[] curNum = new int[MeterNumber];        //当前读取的误差序号
                    int[] lastNum = new int[MeterNumber];                   //保存上一次误差的序号
                    lastNum.Fill(-1);
                    ErrorList[] errorLists = new ErrorList[MeterNumber];
                    bool[] IsOver = new bool[MeterNumber];//完成了
                    for (int i = 0; i < MeterNumber; i++)
                    {
                        if (!MeterInfo[i].YaoJianYn) IsOver[i] = true;//不需要检定的表就是true
                        errorLists[i] = new ErrorList();
                    }


                    //误差判断
                    while (true)
                    {

                        if (Stop) break;
                        double times = TimeSubms(DateTime.Now, TmpTime1) / 1000;
                        MessageAdd(string.Format("已经运行{0}秒", times.ToString("F0")), EnumLogType.提示信息);

                        if (TimeSubms(DateTime.Now, TmpTime1) > MaxTime) //超出最大处理时间
                        {
                            //NoResoult.Fill("超出最大处理时间");
                            MessageAdd("达到最大运行时间,正在退出...", EnumLogType.提示信息);
                            break;
                        }
                        //读取误差，存储起来
                        curWC.Fill("");
                        curNum.Fill(0);
                        if (!ReadWc(ref curWC, ref curNum, Core.Enum.PowerWay.正向有功))    //读取误差
                        {
                            continue;
                        }
                        if (Stop) break;
                        for (int i = 0; i < MeterNumber; i++)
                        {
                            if (Stop) return;
                            if (!MeterInfo[i].YaoJianYn || string.IsNullOrEmpty(curWC[i])) continue;     //表位不要检
                            if (lastNum[i] >= curNum[i]) continue;
                            lastNum[i] = curNum[i];//记录次数
                                                   //开始记录误差
                            if (!string.IsNullOrEmpty(curWC[i]) && curWC[i] != "999.99")//排除空值的情况
                            {
                                if (float.TryParse(curWC[i], out float s))
                                {
                                    errorLists[i].Value.Add(s);
                                }
                            }
                        }


                        //stroutmessage = string.Format("方案设置走字电量：{0}度，已经走字：{1}度", _curPoint.UseMinutes, _StarandMeterDl.ToString("F5"));

                        if (Stop) return;
                        //达到判断的时间了
                        if (TimeSubms(DateTime.Now, TmpTime1) > OkTime)
                        {
                             //len = 0;
                            //判断误差偏移值是否在范围内
                            for (int i = 0; i < MeterNumber; i++)
                            {
                                if (Stop) return;

                                if (!MeterInfo[i].YaoJianYn || IsOver[i]) continue;
                               int len = errorLists[i].Value.Count - 1;
                                if (len < 5) continue;
                                //取其中某些个误差计算差值
                                List<float> value = new List<float>
                                {
                                    errorLists[i].Value[len] - errorLists[i].Value[len / 5],
                                    errorLists[i].Value[len] - errorLists[i].Value[len / 4],
                                    errorLists[i].Value[len] - errorLists[i].Value[len / 3],
                                    errorLists[i].Value[len] - errorLists[i].Value[len / 2]
                                };
                                if (value.Count(x => x > Limet) <= 0)//超出误差限
                                {
                                    ResultDictionary[name][i] = value[0].ToString("f2");
                                    IsOver[i] = true;
                                    continue;
                                }
                                ResultDictionary[name][i] = value.Find(x => x > Limet).ToString();
                                while (errorLists[i].Value.Count > len - len / 4)//去掉其中的4分1
                                {
                                    errorLists[i].Value.RemoveAt(0);
                                }
                                OkTime += 5 * 60 * 1000;//加5分钟
                            }
                            RefUIData(name);
                            if (Array.IndexOf(IsOver, false) == -1)//全部都完成了
                            {
                                if (TimeSubms(DateTime.Now, TmpTime1) >= needTime)
                                {
                                    break;
                                }
                            }
                        }


                        System.Threading.Thread.Sleep(stopTime);//尽量少出误差吧
                    }

                }

                //判断结论

                for (int i = 0; i < MeterNumber; i++)
                {
                    if (!MeterInfo[i].YaoJianYn) continue;
                    if (string.IsNullOrEmpty(ResultDictionary["10误差变化值"][i]) || string.IsNullOrEmpty(ResultDictionary["05L误差变化值"][i]))
                    {
                        ResultDictionary["结论"][i] = Core.ConstHelper.不合格;
                        continue;
                    }
                    try
                    {
                        if (float.Parse(ResultDictionary["10误差变化值"][i]) > Limet || float.Parse(ResultDictionary["05L误差变化值"][i]) > Limet)
                        {
                            ResultDictionary["结论"][i] = Core.ConstHelper.不合格;
                            continue;
                        }
                    }
                    catch (Exception)
                    {

                        ResultDictionary["结论"][i] = Core.ConstHelper.不合格;
                    }

                }
                RefUIData("结论");

            }
            catch (Exception ex)
            {
                RefNoResoult();
                MessageAdd("检定异常\r\n" + ex.ToString(), EnumLogType.错误信息);
            }
        }



        /// <summary>
        /// 刷新不合格结论
        /// </summary>
        private void RefNoResoult()
        {
            for (int i = 0; i < MeterNumber; i++)
            {
                if (!MeterInfo[i].YaoJianYn) continue;
                ResultDictionary["结论"][i] = Core.ConstHelper.不合格;
            }
            RefUIData("结论");
        }

        /// <summary>
        /// 误差集合
        /// </summary>
        class ErrorList
        {
            public List<float> Value = new List<float>();
        }

        /// <summary>
        /// 检定参数是否合法
        /// </summary>
        /// <returns></returns>
        protected override bool CheckPara()
        {
            try
            {
                //一次误差变化值，二次误差变化值，结论
                string[] data = Test_Value.Split('|');
                if (string.IsNullOrEmpty(data[0]))
                {
                    MessageAdd("请设置预热时间", EnumLogType.错误信息);
                    return false;
                }
                PreheatTime = int.Parse(data[0]);
                //计算误差限
                Limet = MeterLevel(OneMeterInfo) * 0.1f;
                ResultNames = new string[] { "10误差变化值", "05L误差变化值", "结论" };
                return true;
            }
            catch (Exception ex)
            {
                MessageAdd("初始化参数失败\r\n" + ex.ToString(), EnumLogType.错误信息);
                return false;
            }

        }



    }
}
