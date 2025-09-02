using LYTest.ViewModel.CheckController;
using System;

namespace LYTest.Verify.Function
{
    /// <summary>
    /// 时区时段功能试验
    /// add lsj 20220724
    /// </summary>
    class FC_ZonePeriod : VerifyBase
    {
        public override void Verify()
        {
            base.Verify();
            if (Stop) return;
            MessageAdd("开始时区时段功能试验...", EnumLogType.提示与流程信息);

            //初始化设备
            if (!InitEquipment()) return;

            if (Stop) return;
            ReadMeterAddrAndNo();

            //身份认证
            if (Stop) return;
            Identity();

            //---------时区切换-------------------------------------------------------------------------------------------------------
            if (Stop) return;
            MessageAdd("读取当前套时区表", EnumLogType.提示信息);
            string[] zoneData1 = MeterProtocolAdapter.Instance.ReadData("第一套时区表数据"); //当前套

            if (Stop) return;
            MessageAdd("读取备用套时区表", EnumLogType.提示信息);
            string[] zoneData2 = MeterProtocolAdapter.Instance.ReadData("第二套时区表数据"); //备用套

            //切换前两套时区表数据
            for (int j = 0; j < MeterNumber; j++)
            {
                if (!MeterInfo[j].YaoJianYn) continue;
                ResultDictionary["切换前当前套时区表数据"][j] = zoneData1[j];
                ResultDictionary["切换前备用套时区表数据"][j] = zoneData2[j];
            }
            RefUIData("切换前当前套时区表数据");
            RefUIData("切换前备用套时区表数据");

            if (Stop) return;
            MessageAdd("写入备用套时区表为030301", EnumLogType.提示信息);
            MeterProtocolAdapter.Instance.WriteData("第二套时区表数据", "030301");         //写入备用套数据

            if (Stop) return;
            DateTime time = DateTime.Now;
            MessageAdd("校准时间为" + time.ToString("yyyyMMddHHmmss"), EnumLogType.提示信息);
            MeterProtocolAdapter.Instance.WriteDateTime(time);   // 统一表对时

            if (Stop) return;
            MessageAdd("设置两套时区表切换时间", EnumLogType.提示信息);
            time = time.AddSeconds(20);
            MessageAdd("设置两套时区表切换时间为" + time.ToString("yyyyMMddHHmmss"), EnumLogType.提示信息);
            MeterProtocolAdapter.Instance.WriteData("两套时区表切换时间", time.ToString("yyyyMMddHHmmss"));

            WaitTime("倒计时", 60);

            //身份认证
            if (Stop) return;
            Identity(false);

            if (Stop) return;
            MessageAdd("读取当前套时区表", EnumLogType.提示信息);
            string[] zoneData3 = MeterProtocolAdapter.Instance.ReadData("第一套时区表数据"); //当前套

            if (Stop) return;
            MessageAdd("读取备用套时区表", EnumLogType.提示信息);
            string[] zoneData4 = MeterProtocolAdapter.Instance.ReadData("第二套时区表数据"); //备用套

            //切换后两套时区表数据
            for (int j = 0; j < MeterNumber; j++)
            {
                if (!MeterInfo[j].YaoJianYn) continue;
                ResultDictionary["切换后当前套时区表数据"][j] = zoneData3[j];
                ResultDictionary["切换后备用套时区表数据"][j] = zoneData4[j];
            }
            RefUIData("切换后当前套时区表数据");
            RefUIData("切换后备用套时区表数据");
            //恢复备用套数据
            MessageAdd("恢复当前套数据", EnumLogType.提示信息);
            MeterProtocolAdapter.Instance.WriteData("第二套时区表数据", zoneData1[FirstIndex].Replace(",", ""));         //写入备用套数据

            if (Stop) return;
            MessageAdd("设置两套时区表切换时间", EnumLogType.提示信息);
            time = DateTime.Now.AddSeconds(30);
            MeterProtocolAdapter.Instance.WriteData("两套时区表切换时间", time.ToString("yyyyMMddHHmmss"));

            WaitTime("倒计时", 60);
            //身份认证
            if (Stop) return;
            Identity(false);

            if (Stop) return;
            MessageAdd("读取当前套时区表", EnumLogType.提示信息);
            string[] zoneData5 = MeterProtocolAdapter.Instance.ReadData("第一套时区表数据"); //当前套

            if (Stop) return;
            MessageAdd("读取备用套时区表", EnumLogType.提示信息);
            string[] zoneData6 = MeterProtocolAdapter.Instance.ReadData("第二套时区表数据"); //备用套

            //切换前两套时区表数据
            for (int j = 0; j < MeterNumber; j++)
            {
                if (!MeterInfo[j].YaoJianYn) continue;

                ResultDictionary["恢复后当前套时区表数据"][j] = zoneData5[j];
                ResultDictionary["恢复后备用套时区表数据"][j] = zoneData6[j];
            }
            RefUIData("恢复后当前套时区表数据");
            RefUIData("恢复后备用套时区表数据");
            //---------时段切换-------------------------------------------------------------------------------------------------------

            if (Stop) return;
            MessageAdd("读取当前套第1日时段数据", EnumLogType.提示信息);
            string[] periodData1 = MeterProtocolAdapter.Instance.ReadData("第一套第1日时段数据"); //当前套

            if (Stop) return;
            MessageAdd("读取备用套第1日时段数据", EnumLogType.提示信息);
            string[] periodData2 = MeterProtocolAdapter.Instance.ReadData("第二套第1日时段数据"); //备用套

            //切换前两套时段第1日时段数据
            for (int j = 0; j < MeterNumber; j++)
            {
                if (!MeterInfo[j].YaoJianYn) continue;
                ResultDictionary["切换前当前套第1日时段数据"][j] = periodData1[j];
                ResultDictionary["切换前备用套第1日时段数据"][j] = periodData2[j];

            }
            RefUIData("切换前当前套第1日时段数据");
            RefUIData("切换前备用套第1日时段数据");
            if (Stop) return;
            MessageAdd("写入备用套第1日时段数据为" + "033001043001053001063001073001", EnumLogType.提示信息);
            MeterProtocolAdapter.Instance.WriteData("第二套第1日时段数据", "033001043001053001063001073001");         //写入备用套数据

            if (Stop) return;
            time = DateTime.Now;
            MessageAdd("校准时间为" + time.ToString("yyyyMMddHHmmss"), EnumLogType.提示信息);
            MeterProtocolAdapter.Instance.WriteDateTime(time);   // 统一表对时

            if (Stop) return;
            MessageAdd("设置两套时段表切换时间", EnumLogType.提示信息);
            time = time.AddSeconds(20);
            MeterProtocolAdapter.Instance.WriteData("两套日时段表切换时间", time.ToString("yyyyMMddHHmmss"));

            WaitTime("倒计时", 60);

            //身份认证
            if (Stop) return;
            Identity(false);

            if (Stop) return;
            MessageAdd("读取当前套时段表", EnumLogType.提示信息);
            string[] periodData3 = MeterProtocolAdapter.Instance.ReadData("第一套第1日时段数据"); //当前套

            if (Stop) return;
            MessageAdd("读取备用套时段表", EnumLogType.提示信息);
            string[] periodData4 = MeterProtocolAdapter.Instance.ReadData("第二套第1日时段数据"); //备用套

            //切换后两套时段第1日时段数据
            for (int j = 0; j < MeterNumber; j++)
            {
                if (!MeterInfo[j].YaoJianYn) continue;
                ResultDictionary["切换后当前套第1日时段数据"][j] = periodData3[j];
                ResultDictionary["切换后备用套第1日时段数据"][j] = periodData4[j];

            }
            RefUIData("切换后当前套第1日时段数据");
            RefUIData("切换后备用套第1日时段数据");
            if (Stop) return;
            MessageAdd("恢复备用套第1日时段数据", EnumLogType.提示信息);
            MeterProtocolAdapter.Instance.WriteData("第二套第1日时段数据", periodData1[FirstIndex].Replace(",", ""));         //写入备用套数据

            if (Stop) return;
            MessageAdd("设置两套时区表切换时间", EnumLogType.提示信息);
            time = DateTime.Now.AddSeconds(30);
            MeterProtocolAdapter.Instance.WriteData("两套日时段表切换时间", time.ToString("yyyyMMddHHmmss"));

            WaitTime("倒计时", 60);

            //身份认证
            //if (Stop) return;
            //Identity();

            if (Stop) return;
            MessageAdd("读取当前套第1日时段数据", EnumLogType.提示信息);
            string[] periodData5 = MeterProtocolAdapter.Instance.ReadData("第一套第1日时段数据"); //当前套

            if (Stop) return;
            MessageAdd("读取备用套第1日时段数据", EnumLogType.提示信息);
            string[] periodData6 = MeterProtocolAdapter.Instance.ReadData("第二套第1日时段数据"); //备用套

            //切换前两套时段第1日时段数据
            for (int j = 0; j < MeterNumber; j++)
            {
                if (!MeterInfo[j].YaoJianYn) continue;
                ResultDictionary["恢复后当前套第1日时段数据"][j] = periodData5[j];
                ResultDictionary["恢复后备用套第1日时段数据"][j] = periodData6[j];

            }
            RefUIData("恢复后当前套第1日时段数据");
            RefUIData("恢复后备用套第1日时段数据");

            //-----恢复电表时间------------------------------------------------------------------------------------------------------------

            if (Stop) return;
            MessageAdd("恢复电表时间", EnumLogType.提示信息);


            MessageAdd("开始读取GPS时间...", EnumLogType.提示信息);
            DateTime readTime = DateTime.Now;  //读取GPS时间
            MeterProtocolAdapter.Instance.WriteDateTime(readTime);

            bool[] arrResult = new bool[MeterNumber];
            for (int i = 0; i < MeterNumber; i++)
            {
                if (!MeterInfo[i].YaoJianYn) continue;
                arrResult[i] = true;
                if (string.IsNullOrEmpty(zoneData3[i]) || zoneData3[i].IndexOf("030301") < 0 ||
                    string.IsNullOrEmpty(periodData3[i]) || periodData3[i].IndexOf("033001043001053001063001073001") < 0)
                {
                    arrResult[i] = false;
                }
                ResultDictionary["结论"][i] = arrResult[i] ? "合格" : "不合格";
            }
            RefUIData("结论");
            MessageAdd("时区时段功能试验结束...", EnumLogType.提示与流程信息);
        }
        protected override bool CheckPara()
        {
            ResultNames = new string[] { "切换前当前套时区表数据", "切换前备用套时区表数据", "切换后当前套时区表数据", "切换后备用套时区表数据", "恢复后当前套时区表数据", "恢复后备用套时区表数据", "切换前当前套第1日时段数据", "切换前备用套第1日时段数据", "切换后当前套第1日时段数据", "切换后备用套第1日时段数据", "恢复后当前套第1日时段数据", "恢复后备用套第1日时段数据", "结论" };
            return true;
        }
        /// <summary>
        /// 初始化设备参数,计算每一块表需要检定的圈数
        /// </summary>
        private bool InitEquipment()
        {
            MessageAdd("开始升电压...", EnumLogType.提示信息);
            if (!PowerOn())
            {
                MessageAdd("升电压失败! ", EnumLogType.错误信息);
                return false;
            }
            WaitTime("升电压", 5);
            return true;
        }
    }
}
