using LYTest.Core;
using LYTest.Core.Model.Meter;
using LYTest.ViewModel.CheckController;
using System;
using System.Threading;
using LYTest.MeterProtocol.Enum;

namespace LYTest.Verify.CarrierTest
{
    public class CarrierVerify : VerifyBase
    {
        private readonly int VerifyNum = 3; //发送次数
        /// <summary>
        /// 标识符
        /// </summary>
        public string Code;
        /// <summary>
        /// 项目名
        /// </summary>
        public string CodeName;
        /// <summary>
        /// 项目编号 
        /// </summary>
        public string ItemId;
        /// <summary>
        /// 模块互换
        /// </summary>
        public bool ModuleSwaps = false;
        public override void Verify()
        {
            try
            {
                base.Verify();

                for (int i = 0; i < MeterNumber; i++)
                {
                    if (!MeterInfo[i].YaoJianYn) continue;
                    ResultDictionary["当前项目"][i] = CodeName;
                    ResultDictionary["项目编号"][i] = ItemId;
                }
                RefUIData("当前项目");
                RefUIData("项目编号");
                //默认合格情况不作检定
                string[] resultKey1 = new string[MeterNumber];

                UpdateMeterProtocol();

                if (!CheckVoltage())
                {
                    MessageAdd("升源失败,退出检定", EnumLogType.错误信息);
                    TryStopTest();
                    return;
                }
                WaitTime("正在升源", 5);
                if (Stop) return;
                //【消息处理】
                MessageAdd("正在进行载波试验检定...", EnumLogType.提示信息);
                ReadMeterAddrAndNo();
                MessageAdd("切换为载波通讯...", EnumLogType.提示与流程信息);
                LYTest.MeterProtocol.App.g_ChannelType = Cus_ChannelType.通讯载波;
                SwitchChannel(Cus_ChannelType.通讯载波);

                if (Stop) return;
                DateTime startTime = DateTime.Now;
                while (TimeSubms(DateTime.Now, startTime) <= 15 * 60 * 1000)
                {
                    if (Stop) break;

                    for (int t = 0; t < VerifyNum; t++) //重复次数
                    {
                        for (int i = 0; i < MeterNumber; i++)
                        {
                            if (Stop) break;
                            TestMeterInfo meter = MeterInfo[i];
                            if (!meter.YaoJianYn) continue;
                            if (ResultDictionary["结论"][i] == ConstHelper.合格) continue;

                            LYTest.MeterProtocol.App.Protocols = MeterInfo[i].DgnProtocol.ClassName;
                            LYTest.MeterProtocol.App.Carrier_Cur_BwIndex = i;
                            if (Stop) break;

                            MessageAdd($"第{ i + 1}表位 第{ t + 1}次载波试验...", EnumLogType.提示信息);
                            //Utility.Log.LogManager.AddMessage("第{ " + (i + 1) + "}表位 第{" + (t + 1) + "}次载波试验", Utility.Log.EnumLogSource.检定业务日志, Utility.Log.EnumLevel.Information);
                            string revData = "";

                            if (Stop) break;
                            revData = MeterProtocolAdapter.Instance.ReadDataPos(Code, i);
                            if (revData == "")
                            {
                                if (Stop) break;
                                Thread.Sleep(1000);
                                revData = MeterProtocolAdapter.Instance.ReadDataPos(Code, i);
                                if (Stop) break;
                            }

                            ResultDictionary["检定信息"][i] = revData;
                            ResultDictionary["读取值"][i] = revData;
                            if (string.IsNullOrEmpty(revData))
                            {
                                WaitTime("载波读取超时等待", 3);

                            }
                            if (Stop) break;
                            if (string.IsNullOrEmpty(revData))
                            {
                                ResultDictionary["结论"][i] = ConstHelper.不合格;
                            }
                            else
                            {
                                ResultDictionary["结论"][i] = ConstHelper.合格;
                            }
                            RefUIData("读取值");
                            RefUIData("检定信息");


                        }

                    }

                    bool fail = false;
                    for (int i = 0; i < MeterNumber; i++)
                    {
                        TestMeterInfo meter = MeterInfo[i];
                        if (!meter.YaoJianYn) continue;
                        if (ResultDictionary["结论"][i] == ConstHelper.不合格)
                        {
                            fail = true;
                            break;
                        }
                    }
                    if (!fail) break;
                    if (Stop) break;
                    Thread.Sleep(1000);
                }
                SwitchChannel(Cus_ChannelType.通讯485);
                RefUIData("结论");
                MessageAdd("载波试验检定完成", EnumLogType.提示信息);
            }
            catch (Exception ex)
            {
                TryStopTest();
                MessageAdd(ex.ToString(), EnumLogType.错误信息);
            }
        }
        /// <summary>
        /// 检定参数是否合法
        /// </summary>
        /// <returns></returns>
        protected override bool CheckPara()
        {
            string[] tem = Test_Value.Split('|');
            if (tem.Length < 3) return false;

            CodeName = tem[0];
            Code = tem[1];
            ItemId = tem[2];
            if (Code.Trim() == "") return false;

            if (Test_No == "19003")   //模块互换
            {
                ModuleSwaps = true;
            }
            ResultNames = new string[] { "当前项目", "检定信息", "读取值", "结论", "项目编号" };
            return true;
        }



    }
}
