using LYTest.Core.Enum;
using LYTest.ViewModel.CheckController;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LYTest.Verify.EventLog
{
    /// <summary>
    /// 广播校时事件
    /// </summary>
    public class BroadcastTime : VerifyBase
    {
        public override void Verify()
        {
            base.Verify();

            if (!PowerOn())
            {
                MessageAdd("升电压失败! ", EnumLogType.提示信息);
                ErrorRefResoult();
                return;
            }
            if (Stop) return;
            ReadMeterAddrAndNo();
            Identity(false);
            MessageAdd("正在读取校时前次数", EnumLogType.提示信息);
            if (DAL.Config.ConfigHelper.Instance.IsITOMeter) VerifyBase.MeterLogicalAddressType = MeterLogicalAddressEnum.计量芯;
            string[] stateCount = MeterProtocolAdapter.Instance.ReadData("广播校时总次数");
            SetValue(stateCount, "事件发生前次数");

            MessageAdd("正在校时", EnumLogType.提示信息);
            MeterProtocolAdapter.Instance.WriteDateTime(GetYaoJian());

            MessageAdd("正在广播校时", EnumLogType.提示信息);
            MeterProtocolAdapter.Instance.BroadCastTime(DateTime.Now.AddSeconds(-100));
            MessageAdd("正在读取校后前次数", EnumLogType.提示信息);
            WaitTime("广播校时事件触发中", 10);
            string[] endCount = MeterProtocolAdapter.Instance.ReadData("广播校时总次数");
            SetValue(endCount, "事件发生后次数");
            MessageAdd("正在恢复时间", EnumLogType.提示信息);
            MeterProtocolAdapter.Instance.WriteDateTime(GetYaoJian());
            for (int i = 0; i < MeterNumber; i++)
            {
                if (!MeterInfo[i].YaoJianYn) continue;
                int s = 0;
                int e = 0;
                if (!string.IsNullOrEmpty(stateCount[i]))
                {
                    s = int.Parse(stateCount[i]);
                }
                if (!string.IsNullOrEmpty(stateCount[i]))
                {
                    e = int.Parse(endCount[i]);
                }
                if (e > s || (e == 10 && s == 10))//次数发生改变
                {
                    ResultDictionary["结论"][i] = "合格";
                }
                else
                { 
                    ResultDictionary["结论"][i] = "不合格";

                }
            }
            RefUIData("结论");


        }

        private void SetValue(string[] value, string name)
        {
            for (int i = 0; i < MeterNumber; i++)
            {
                if (!MeterInfo[i].YaoJianYn) continue;
                ResultDictionary[name][i] = value[i];
            }
            RefUIData(name);

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
