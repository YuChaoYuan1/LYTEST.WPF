using LYTest.ViewModel.CheckController;
using System;
using System.Threading;
namespace LYTest.Verify.Multi
{
    /// <summary>
    /// 闰年判断功能
    /// add lsj 20220718
    /// </summary>
    class Dgn_LeapYear : VerifyBase
    {
        public override void Verify()
        {
            base.Verify();
            for (int i = 0; i < MeterNumber; i++)
            {
                if (!MeterInfo[i].YaoJianYn) continue;
                ResultDictionary["检定项目"][i] = "闰年判断功能";
            }

            RefUIData("检定项目");
            if (!PowerOn())
            {
                MessageAdd("源输出失败", EnumLogType.提示信息);
                return;
            }
            DateTime LeapYear = new DateTime(2008, 02, 28, 23, 59, 55);
            bool[] Result = new bool[MeterNumber];

            ReadMeterAddrAndNo();
            Identity();
            MessageAdd("正在把表时间设置到2008-2-28 23:59:55", EnumLogType.提示与流程信息);

            bool[] isSc = MeterProtocolAdapter.Instance.WriteDateTime(LeapYear);
            bool bResult = true;
            for (int i = 0; i < MeterNumber; i++)
            {
                if (MeterInfo[i].YaoJianYn)
                {
                    if (!isSc[i])
                        bResult = false;
                }
            }
            if (!bResult)
            {
                MessageAdd("修改电表时间失败，是否打开编程开关?", EnumLogType.错误信息);
                return;
            }
            MessageAdd("等待6秒", EnumLogType.提示信息);
            Thread.Sleep(6000);
            MessageAdd("开始读取表时间...", EnumLogType.提示信息);
            DateTime[] readDateTime = MeterProtocolAdapter.Instance.ReadDateTime();
            //分析结果
            for (int i = 0; i < MeterNumber; i++)
            {
                Result[i] = (readDateTime[i].Month == 2 && readDateTime[i].Day == 29);

                //add
                ResultDictionary["检定数据"][i] = readDateTime[i].ToString("yyyy/MM/dd");
            }
            //add
            RefUIData("检定数据");

            //modify
            //Identity();
            Identity(false);
            MessageAdd("开始检测29日到3月1日跳转", EnumLogType.提示与流程信息);
            LeapYear = new DateTime(2008, 02, 29, 23, 59, 55);

            MeterProtocolAdapter.Instance.WriteDateTime(LeapYear);
            MessageAdd("等待6秒", EnumLogType.提示信息);
            Thread.Sleep(6000);
            MessageAdd("开始读取表时间...", EnumLogType.提示信息);
            readDateTime = MeterProtocolAdapter.Instance.ReadDateTime();

            for (int i = 0; i < MeterNumber; i++)
            {
                if (!MeterInfo[i].YaoJianYn) continue;
                Result[i] = (readDateTime[i].Month == 3 && readDateTime[i].Day == 1);
                ResultDictionary["结论"][i]  = Result[i] ? "合格" : "不合格";
            }
            RefUIData("结论");
            //GPS对时一次
            //modify
            //Identity();
            Identity(false);
            MessageAdd("开始读取GPS时间...", EnumLogType.提示信息);
            DateTime readTime = DateTime.Now;
            //读取GPS时间
            MeterProtocolAdapter.Instance.WriteDateTime(readTime);
        }
        /// <summary>
        /// 检定参数是否合法
        /// </summary>
        /// <returns></returns>
        protected override bool CheckPara()
        {
            //modify 
            //ResultNames = new string[] { "当前项目", "结论" };
            ResultNames = new string[] { "检定项目", "检定数据", "结论" };
            return true;
        }
    }
}
