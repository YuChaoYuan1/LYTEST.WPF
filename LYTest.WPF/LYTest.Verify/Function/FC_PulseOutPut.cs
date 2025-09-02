using LYTest.Core.Enum;
using LYTest.ViewModel.CheckController;
using System;

namespace LYTest.Verify.Function
{
    /// <summary>
    /// 脉冲输出功能
    /// add lsj 20220724
    /// </summary>
    class FC_PulseOutPut : VerifyBase
    {
        public override void Verify()
        {
            base.Verify();
            if (Stop) return;

            //三相测试需量 单相无
            int iTestNum = 4;
            if (Clfs == WireMode.单相)
                iTestNum = 3;

            if (Stop) return;
            bool[] arrResult = new bool[MeterNumber];

            for (int i = 0; i < MeterNumber; i++)
            {
                if (!MeterInfo[i].YaoJianYn) continue;
                ResultDictionary["电能脉冲输出"][i] = "合格";
                ResultDictionary["秒脉冲输出"][i] = "合格";
                ResultDictionary["投切脉冲输出"][i] = "合格";
                if (iTestNum == 4)
                {
                    ResultDictionary["需量脉冲输出"][i] = "合格";
                }
            }
            if (Stop) return;
            //电表选择时钟通道 0日计时、1需量、2时段投切
            if (!PowerOn())
            {
                throw new Exception("控制源输出失败！");
            }
            WaitTime("升源成功，等待源稳定", 5);
            bool[] rjsResult = new bool[MeterNumber];   //日计时
            bool[] sdtqResult = new bool[MeterNumber];  //时段切换
            bool[] xlResult = new bool[MeterNumber];    //需量
            for (int i = 0; i < MeterNumber; i++)
            {
                rjsResult[i] = true;
                sdtqResult[i] = true;
                xlResult[i] = true;
            }

            if (!IsDemo)
            {
                rjsResult = MeterProtocolAdapter.Instance.SetPulseCom(0);//日计时
                sdtqResult = MeterProtocolAdapter.Instance.SetPulseCom(2);//时段投切试验
                if (iTestNum == 4)
                {
                    xlResult = MeterProtocolAdapter.Instance.SetPulseCom(1);//需量
                }
            }

            for (int i = 0; i < MeterNumber; i++)
            {
                if (!MeterInfo[i].YaoJianYn) continue;
                for (int k = 1; k <= iTestNum; k++)
                {
                    switch (k)
                    {
                        case 1:
                            break;
                        case 2:
                            if (!rjsResult[i])
                            {
                                ResultDictionary["秒脉冲输出"][i] = "不合格";
                            }
                            break;
                        case 3:
                            if (!sdtqResult[i])
                            {
                                ResultDictionary["投切脉冲输出"][i] = "不合格";
                            }
                            break;
                        case 4:
                            if (!xlResult[i])
                            {
                                ResultDictionary["需量脉冲输出"][i] = "不合格";
                            }
                            break;
                        default:
                            break;
                    }

                }
                if (rjsResult[i] && sdtqResult[i] && xlResult[i])
                    arrResult[i] = true;
                else
                    arrResult[i] = false;
            }

            if (Stop) return;
            for (int i = 0; i < MeterNumber; i++)
            {
                if (!MeterInfo[i].YaoJianYn) continue;
                ResultDictionary["电能脉冲输出"][i] = "合格";
                ResultDictionary["秒脉冲输出"][i] = rjsResult[i] ? "合格" : "不合格";
                ResultDictionary["投切脉冲输出"][i] = sdtqResult[i] ? "合格" : "不合格";
                if (iTestNum == 4)
                {
                    ResultDictionary["需量脉冲输出"][i] = xlResult[i] ? "合格" : "不合格";
                }
                ResultDictionary["结论"][i] = arrResult[i] ? "合格" : "不合格";

            }
            RefUIData("电能脉冲输出");
            RefUIData("秒脉冲输出");
            RefUIData("投切脉冲输出");
            if (Clfs != WireMode.单相)
                RefUIData("需量脉冲输出");
            RefUIData("结论");


        }
        /// <summary>
        /// 单三相都刷新  以后需要修改
        /// </summary>
        /// <returns></returns>
        protected override bool CheckPara()
        {
            //    if (Clfs == WireMode.单相)
            //    {
            //        ResultNames = new string[] { "电能脉冲输出", "秒脉冲输出", "时段投切输出", "需量脉冲输出", "结论" };

            //    }电能脉冲输出|秒脉冲输出|投切脉冲输出|需量脉冲输出
            ResultNames = new string[] { "电能脉冲输出", "秒脉冲输出", "投切脉冲输出", "需量脉冲输出", "结论" };
            return true;
        }
    }
}
