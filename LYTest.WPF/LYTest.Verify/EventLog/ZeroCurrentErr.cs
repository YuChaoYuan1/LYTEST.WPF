using LYTest.Core.Enum;
using LYTest.Core.Function;
using LYTest.ViewModel.CheckController;

namespace LYTest.Verify.EventLog
{
    public class ZeroCurrentErr : VerifyBase
    {

        public override void Verify()
        {
            //当程序开始切换误差板继电器后，将不允许中间停止
            //为防止做其他功能时，继电器状态不正确
            base.Verify();
            if (Stop) return;

            if (Stop) return;

            PowerOn();
            WaitTime("等待源稳定...", 5);
            bool[] YJMeter = new bool[MeterNumber];
            bool[] bBiaoweiBz = new bool[MeterNumber];
            MessageAdd("切换表位旁路", EnumLogType.提示信息);

            ReadMeterAddrAndNo();

            Identity(false);

            //bBiaoweiBz.Fill(false);
            //bBiaoweiBz[0] = true;

            //for (int i = 0; i < MeterNumber; i++)
            //{
            //    TestMeterInfo m = meterInfo[i];
            //    YJMeter[i] = m.YaoJianYn & bBiaoweiBz[i];
            //}

            YJMeter.Fill(false);
            YJMeter[0] = true;
            bBiaoweiBz.Fill(true);
            ControlMeterRelay(bBiaoweiBz, 2);

            //ControlMeterRelay(YJMeter, 1);
            //ControlMeterRelay(ReversalBool(YJMeter), 2);
            //WaitTime("旁路除了一号表位的其他表位继电器...", 3);

            MeterProtocolAdapter.Instance.ClearEventLog("FFFFFFFF");
            WaitTime("正常清除记录", 5);
            //读取

            int[] stateCount = ReadEventLogInfo("事件发生前");


            StartEventLog();

            int[] endCount = ReadEventLogInfo("事件发生后");

            for (int i = 0; i < MeterNumber; i++)
            {
                if (!MeterInfo[i].YaoJianYn) continue;
                if (endCount[i] > stateCount[i] || i > 0)//除了一号表位--这里以后需要改成配置的
                {
                    ResultDictionary["结论"][i] = "合格";
                }
                else
                {
                    ResultDictionary["结论"][i] = "不合格";
                }
            }
            RefUIData("结论");

            if (Stop) return;
            //关闭零线电流板 A 01关闭 ，BC 00开启
            StartZeroCurrent(01, 00);
            WaitTime("关闭零线电流板...", 3);

            ControlMeterRelay(GetYaoJian(), 2);
            ControlMeterRelay(ReversalBool(GetYaoJian()), 1);

            MessageAdd("零线电流检测试验完成!", EnumLogType.提示信息);

        }
        private int[] ReadEventLogInfo(string name)
        {
            int[] resoult = new int[MeterNumber];
            string[] eventTimes = MeterProtocolAdapter.Instance.ReadData("零线电流异常总次数");


            //List<string> oad = new List<string> { "30400200" };
            //Dictionary<int, List<object>> dicObj = new Dictionary<int, List<object>>();
            //List<string> rcsd = new List<string>();
            //Dictionary<int, Dictionary<string, List<object>>> dic = MeterProtocolAdapter.Instance.ReadRecordData(oad, EmSecurityMode.ClearTextRand, EmGetRequestMode.GetRequestRecord, 1, rcsd, ref dicObj);


            for (int i = 0; i < MeterNumber; i++)
            {
                if (!MeterInfo[i].YaoJianYn) continue;
                if (int.TryParse(eventTimes[i], out int n))
                {
                    resoult[i] = n;
                }
                ResultDictionary[name + "次数"][i] = resoult[i].ToString();
            }
            RefUIData(name + "次数");
            return resoult;
        }

        /// <summary>
        /// 开始切换
        /// </summary>
        private void StartEventLog()
        {
            //获取电流值
            //开启零线电流板 A 00开启，BC 01关闭
            StartZeroCurrent(00, 01);
            WaitTime("开启零线电流板...", 3);

            MessageAdd("升电压和A相电流", EnumLogType.提示信息);
            float xIb = Number.GetCurrentByIb("Ib", OneMeterInfo.MD_UA, HGQ);
            PowerOn(OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, xIb * 0.3f, 0, 0, Cus_PowerYuanJian.H, PowerWay.正向有功, "1.0");

            WaitTime("零等待线电流异常", 65);

            //源按照正常输出
            if (Stop) return;
            PowerOn();
            WaitTime("恢复电压,等待功率因数超下限事件记录产生", 10);
        }
        /// <summary>
        /// bool数组反转
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        private bool[] ReversalBool(bool[] Bt)
        {
            bool[] t = new bool[Bt.Length];
            for (int i = 0; i < t.Length; i++)
            {
                t[i] = !Bt[i];
            }
            return t;
        }
        /// <summary>
        /// 检定参数是否合法
        /// </summary>
        /// <returns></returns>
        protected override bool CheckPara()
        {
            ResultNames = new string[] { "事件发生前次数", "事件发生后次数", "结论" };
            return true;
        }
    }
}
