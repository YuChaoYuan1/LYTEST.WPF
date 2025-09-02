using LYTest.Core.Enum;
using LYTest.Core;
using LYTest.Core.Model.Meter;
using LYTest.ViewModel.CheckController;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LYTest.Verify.CostControl
{
    /// <summary>
    /// 解除保电
    /// </summary>
    class FK_Encryption_DisPower : VerifyBase
    {
        public override void Verify()
        {
            base.Verify();

            for (int i = 0; i < MeterNumber; i++)
            {
                if (!MeterInfo[i].YaoJianYn) continue;
                ResultDictionary["当前项目"][i] = "保电解除";
            }
            RefUIData("当前项目");

            //初始化设备
            MessageAdd("正在升源...", EnumLogType.提示信息);
            if (!PowerOn(OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, 0, 0, 0, Core.Enum.Cus_PowerYuanJian.H, FangXiang, "1.0"))
            {
                MessageAdd("升源失败,退出检定", EnumLogType.提示信息);
                return;
            }
            if (Stop) return;

            //获取所有表的表地址
  
            ReadMeterAddrAndNo();

            if (Stop) return;
            bool[] bResult = new bool[MeterNumber];
            for (int i = 0; i < MeterNumber; i++)
                bResult[i] = true;

            //modify yjt 20220415 修改身份认证为不必要，传入参数为false
            //Identity();
            //modify yjt 20220415 修改身份认证为不必要，传入参数为false
            Identity(true);

            SendCostCommand(Cus_EncryptionTrialType.解除保电);
            for (int i = 0; i < MeterNumber; i++)
            {
                TestMeterInfo meter = MeterInfo[i];
                if (!meter.YaoJianYn) continue;

                bResult[i] = EncryptionThread.WorkThreads[i].RemoteControlResult;

                ResultDictionary["检定信息"][i] = bResult[i] ? "√" : "×";

                if (bResult[i])
                {
                    bResult[i] = true;
                }
                else
                {
                    bResult[i] = false;
                    NoResoult[i] = "远程拉合闸失败";
                }
            }


            for (int i = 0; i < MeterNumber; i++)
            {
                TestMeterInfo meter = MeterInfo[i];
                if (!meter.YaoJianYn) continue;


                ResultDictionary["结论"][i] = bResult[i] ? ConstHelper.合格 : ConstHelper.不合格;
            }

            RefUIData("检定信息");
            RefUIData("结论");
            MessageAdd("检定完成", EnumLogType.提示信息);


        }


        /// <summary>
        /// 检定参数是否合法
        /// </summary>
        /// <returns></returns>
        protected override bool CheckPara()
        {
            ResultNames = new string[] { "当前项目","检定信息", "结论" };
            return true;
        }
    }
}
