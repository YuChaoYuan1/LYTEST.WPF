using LYTest.ViewModel.CheckController;
using System;
using System.Windows.Forms;

namespace LYTest.Verify.Multi
{
    /// <summary>
    /// 采用备用电源工作的时钟试验
    /// add lsj 20220718
    /// </summary>
    class DgnBYDYTimeError : VerifyBase
    {
        public override void Verify()
        {
            base.Verify();
            if (Stop) return;
            if (!PowerOn()) return;
            //add
            WaitTime("升电压", 5);
            if (Stop) return;
            //有无编程键
            if (OneMeterInfo.DgnProtocol.HaveProgrammingkey)
            {
                MessageBox.Show("请打开电能表编程开关后点击[确定]", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            if (Stop) return;
            ReadMeterAddrAndNo();

            if (Stop) return;
            Identity();
            string[] param = Test_Value.Split('|');
            int intTong = Convert.ToInt32(Convert.ToDouble(param[1]) * 60);
            int intDuan = Convert.ToInt32(Convert.ToDouble(param[2]) * 60 * 60);

            DateTime[] readDataQ = new DateTime[MeterNumber];
            DateTime[] readDataGPS = new DateTime[MeterNumber];
            DateTime[] readDataH = new DateTime[MeterNumber];

            //float[] floatiTime = new float[MeterNumber];

            if (Stop) return;
            MessageAdd("开始写表时间......", EnumLogType.提示信息);
            //modify
            DateTime readTime = DateTime.Now;
            //MeterProtocolAdapter.Instance.WriteDateTime(readTime);
            for (int i = 0; i < MeterNumber; i++)
            {
                if (!MeterInfo[i].YaoJianYn) continue;
                MeterProtocolAdapter.Instance.WriteDateTime(i);
                readTime = DateTime.Now;
                WaitTime("", 1);
            }


            // 演示模式 时钟示值误差
            if (IsDemo)
            {
                for (int i = 0; i < MeterNumber; i++)
                {
                    readDataQ[i] = readTime;
                    readDataH[i] = readTime;
                    readDataGPS[i] = readTime;
                }
            }
            else
            {
                if (Stop) return;
                //MessageAdd("开始给表连续供电......", EnumLogType.提示信息);
                //add
                MessageAdd("开始给表连续供电......", EnumLogType.流程信息);
                WaitTime( "给表连续供电", intTong);
                //WaitTime("给表连续供电",15);

                MessageAdd("开始读取断电前的时间......", EnumLogType.提示信息);

                for (int i = 0; i < MeterNumber; i++)
                {
                    if (!MeterInfo[i].YaoJianYn) continue;
                    readDataQ[i] = MeterProtocolAdapter.Instance.ReadDateTime(i);
                }

                //关源
                MessageAdd("开始关源操作......", EnumLogType.提示信息);
               PowerOff();

                if (Stop) return;
                MessageAdd("开始给表断电......", EnumLogType.提示信息);
                //add
                MessageAdd("开始给表断电......", EnumLogType.流程信息);

                WaitTime( "连续断电", intDuan);
                //WaitTime("连续断电", 30);

                if (Stop) return;
                MessageAdd("重新开始升源......", EnumLogType.提示信息);
                if (!PowerOn()) return;
                //add
                WaitTime("升电压", 5);

                if (Stop) return;
                MessageAdd("开始读取断电后时间......", EnumLogType.提示信息);

                for (int i = 0; i < MeterNumber; i++)
                {
                    if (!MeterInfo[i].YaoJianYn) continue;
                   
                    readDataH[i] = MeterProtocolAdapter.Instance.ReadDateTime(i);
                    readDataGPS[i] = DateTime.Now.AddSeconds(-1);
                    WaitTime("", 1);
                    if (Stop) return;

                    //if (readDataQ[i] == null) continue;
                    //if (readDataH[i] == null) continue;
                    //float iTime = 0f;
                    //DateTime meterTimeQ = readDataQ[i];
                    //DateTime meterTimeH = readDataH[i];
                    //DateTime GPSTimes = readDataGPS[i];

                    //iTime = (float)MeterProtocolAdapter.Instance.ReadDateTime(i).Subtract(DateTime.Now).TotalSeconds;

                    //float err = 1.5f;
                    //if (param != null && param.Length >= 0)
                    //{
                    //    float.TryParse(param[0], out err);
                    //}

                    //if (Math.Abs(iTime) > err)
                    //    ResultDictionary["结论"][i] = "不合格";
                    //else
                    //    ResultDictionary["结论"][i] = "合格";

                    //ResultDictionary["断电后电源恢复电能表时间"][i] = meterTimeH.ToString();
                    //ResultDictionary["标准时钟时间"][i] = GPSTimes.ToString();
                    //ResultDictionary["时间差"][i] = iTime.ToString("#0.00");

                    //RefUIData("断电后电源恢复电能表时间");
                    //RefUIData("标准时钟时间");
                    //RefUIData("时间差");
                    //RefUIData("结论");
                    //MessageAdd("采用备用电源工作的时钟试验完毕", EnumLogType.提示信息);
                }
            }
            if (Stop) return;
            for (int i = 0; i < MeterNumber; i++)
            {
                if (!MeterInfo[i].YaoJianYn) continue;

                if (readDataQ[i] == null) continue;
                if (readDataH[i] == null) continue;
                DateTime meterTimeH = readDataH[i];
                DateTime GPSTimes = readDataGPS[i];

                float iTime = (float)meterTimeH.Subtract(GPSTimes).TotalSeconds;

                float err = 1.5f;
                if (param != null && param.Length >= 0)
                {
                    float.TryParse(param[0], out err);
                }

                if (Math.Abs(iTime) > err)
                    ResultDictionary["结论"][i] = "不合格";
                else
                    ResultDictionary["结论"][i] = "合格";

                ResultDictionary["断电后电源恢复电能表时间"][i] = meterTimeH.ToString();
                ResultDictionary["标准时钟时间"][i] = GPSTimes.ToString();
                ResultDictionary["时间差"][i] = iTime.ToString("#0.00");
            }
            RefUIData("断电后电源恢复电能表时间");
            RefUIData("标准时钟时间");
            RefUIData("时间差");
            RefUIData("结论");
            MessageAdd("采用备用电源工作的时钟试验完毕", EnumLogType.提示信息);

        }
        /// <summary>
        /// 检定参数是否合法
        /// </summary>
        /// <returns></returns>
        protected override bool CheckPara()
        {
            ResultNames = new string[] { "断电后电源恢复电能表时间", "标准时钟时间", "时间差", "结论" };
            return true;
        }
    }
}
