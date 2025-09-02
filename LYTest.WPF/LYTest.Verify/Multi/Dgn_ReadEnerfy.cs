using LYTest.Core.Enum;
using LYTest.ViewModel;
using LYTest.ViewModel.CheckController;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LYTest.Verify.Multi
{

    //add lsj 20220426 读取电量试验
    // 读取电量试验
    public class Dgn_ReadEnerfy : VerifyBase
    {
        public override void Verify()
        {
            //获取当前检定项的参数默认值
            string value = Test_Value;
            string[] para = value.Split('|');

            for (int i = 0; i < MeterNumber; i++)
            {
                if (!MeterInfo[i].YaoJianYn) continue;
                ResultDictionary["功率方向"][i] = para[0];
                ResultDictionary["费率类型"][i] = para[1];
            }
            RefUIData("功率方向");
            RefUIData("费率类型");
            base.Verify();
            //升源
            if (!CheckVoltage())
            {
                return;
            }
            //读取表地址表号
            ReadMeterAddrAndNo();

            MessageAdd("开始读取电量", EnumLogType.提示信息);
            ReadDl(para);
            MessageAdd("读取电量完毕", EnumLogType.提示信息);

        }
        /// <summary>
        /// 读取电量操作
        /// </summary>
        /// <param name="para">读取参数:总尖峰平谷|P+P-Q+Q-Q1Q2Q3Q4</param>
        /// <returns></returns>
        public bool ReadDl(string[] para)
        {
            if (Stop) return false;

            float[] dicEnergy = MeterProtocolAdapter.Instance.ReadEnergy((byte)GetPowerWay(para), (byte)GetCus(para));
            MessageAdd("开始处理电量数据", EnumLogType.提示信息);
            for (int j = 0; j < MeterNumber; j++)
            {
                if (!MeterInfo[j].YaoJianYn) continue;
                ResultDictionary["电量"][j] = dicEnergy[j].ToString();
                ResultDictionary["结论"][j] = "合格";

            }
            RefUIData("电量");
            RefUIData("结论");
            MessageAdd("电量读取完毕", EnumLogType.提示信息);
            return true;
        }
        private PowerWay GetPowerWay(string[] para)
        {
            PowerWay power = PowerWay.组合有功;
            switch (para[0])
            {
                case "正向有功":
                    power = PowerWay.正向有功;
                    break;
                case "正向无功":
                    power = PowerWay.正向无功;
                    break;
                case "反向无功":
                    power = PowerWay.反向无功;
                    break;
                case "反向有功":
                    power = PowerWay.反向有功;
                    break;
            }
            return power;
        }
        private Cus_FeiLv GetCus(string[] para)
        {
            Cus_FeiLv cus = Cus_FeiLv.总;
            switch (para[1])
            {
                case "尖":
                    cus = Cus_FeiLv.尖;
                    break;
                case "峰":
                    cus = Cus_FeiLv.峰;
                    break;
                case "平":
                    cus = Cus_FeiLv.平;
                    break;
                case "谷":
                    cus = Cus_FeiLv.谷;
                    break;
                case "深谷":
                    cus = Cus_FeiLv.深谷;
                    break;
            }
            return cus;
        }
        protected override bool CheckPara()
        {
            ResultNames = new string[] { "功率方向", "费率类型", "电量", "结论" };
            return true;
        }

    }
}
