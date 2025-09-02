using LYTest.Core.Enum;
using LYTest.Core;
using LYTest.ViewModel.CheckController;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LYTest.Verify.CostControl
{
    /// <summary>
    /// ESAM数据回抄
    /// </summary>
   public  class FK_Encryption_ReadEsam : VerifyBase
    {

        /// <summary>
        /// 重写基类测试方法
        /// </summary>
        /// <param name="ItemNumber">检定方案序号</param>
        public override void Verify()
        {
            base.Verify();


            for (int i = 0; i < MeterNumber; i++)
            {
                if (!MeterInfo[i].YaoJianYn) continue;
                ResultDictionary["当前项目"][i] = "数据回抄";
            }
            RefUIData("当前项目");
            bool[] bResult = new bool[MeterNumber];


            //初始化设备
            if (Stop) return;

            if (!PowerOn(OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, 0, 0, 0,Cus_PowerYuanJian.H, FangXiang, "1.0"))
            {
                MessageAdd("升源失败,退出检定", EnumLogType.提示信息);
                return ;
            }

                //获取所有表的表号
                if (Stop) return;
            ReadMeterAddrAndNo();

            if (Stop) return;

            //modify yjt 20220415 修改身份认证为不必要，传入参数为false
            //Identity();
            //modify yjt 20220415 修改身份认证为不必要，传入参数为false
            Identity(true);

            if (Stop) return;
            SendCostCommand(Cus_EncryptionTrialType.ESAM数据回抄);

            for (int i = 0; i < MeterNumber; i++)
            {
                if (Stop) return;
                if (!MeterInfo[i].YaoJianYn) continue;
                if (EncryptionThread.WorkThreads[i] != null)
                {
                    ResultDictionary["检定信息"][i] = EncryptionThread.WorkThreads[i].curKeyInfo.ToString();
                    ResultDictionary["结论"][i] = ConstHelper.合格;
                    bResult[i] = true;
                }
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
            ResultNames = new string[] { "当前项目", "检定信息", "结论" };
            return true;
        }
    }
}
