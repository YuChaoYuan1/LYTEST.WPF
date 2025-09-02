using LYTest.Core.Enum;
using LYTest.ViewModel;
using LYTest.ViewModel.CheckController;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace LYTest.Verify.AccurateTest
{

    //  ///add lsj 20220218 预热检定
    /// <summary>
    /// 预热检定控制器.按方案预定电流电压让表运行。
    /// 误差板显示收到的脉冲数量。
    /// 无结论判定,预热不需要保存检定数据
    /// </summary>
    class PreHeat : VerifyBase
    {

        #region----------检定控制----------
        /// <summary>
        /// 开始预热检定,预热不需要检定数据
        /// </summary>
        public override void Verify()
        {
            string value = Test_Value;
            string[] bb = value.Split('|');
            base.Verify();                                                  //调用基类方法，进行数据清理以及默认数据挂接处理

            for (int i = 0; i < MeterNumber; i++)
            {
                if (!MeterInfo[i].YaoJianYn) continue;
                ResultDictionary["检定项目"][i] = $"预热检定{value}";
            }
            RefUIData("检定项目");

            if (!InitEquipment()) return;                                   //初始化设备参数
            if (!Power_On()) return;

            StartTime = DateTime.Now;                                     //记录预热开始时间

            int preHeatTime = int.Parse(bb[2]) * 60;                      //计算总预热时间
            while (true)
            {
                if (Stop) break;
                if (VerifyPassTime >= preHeatTime)
                {
                    CheckOver = true;
                    break;
                }
                //每一秒刷新一次数据
                Thread.Sleep(1000);
                float PastTime = VerifyPassTime / 60F;
                if (!IsMeterDebug)
                {
                    for (int i = 0; i < MeterNumber; i++)
                    {
                        if (!MeterInfo[i].YaoJianYn) continue;
                        ResultDictionary["检定进度"][i] = $"预热时间{preHeatTime / 60}分，已经预热{ PastTime:f2}分";
                    }
                    RefUIData("检定进度");
                    MessageAdd($"预热时间{preHeatTime / 60}分，已经预热{ PastTime:f2}分", EnumLogType.提示信息);
                }
                else
                {
                    MessageAdd("预热调表中...", EnumLogType.提示信息);
                }
            }
            return;
        }
        #endregion

        #region 初始化设备参数
        /// <summary>
        /// 初始化设备参数
        /// </summary>
        /// <returns></returns>
        private bool InitEquipment()
        {
            if (IsDemo) return true;
            string value = Test_Value;
            string[] arrayParameters = value.Split('|');
            MessageAdd("正在设置预热参数", EnumLogType.提示信息);
            bool ret =InitPara_WarmUp(GetPowerWay(arrayParameters[0]));//doto初始化预热参数
            string result = ret ? "成功" : "失败";
            MessageAdd($"设置台体预热参数{result}", EnumLogType.提示信息);
            if (!ret)
            {
                MessageAdd("设置台体预热参数失败", EnumLogType.提示信息);
                return false;
            }
            if (Stop) return true;
            Thread.Sleep(100);
            return true;
        }
        #endregion
        /// <summary>
        /// 初始化预热参数:不带升源
        /// </summary>
        /// <returns></returns>
        public bool InitPara_WarmUp(PowerWay pd)
        {
            return true;
        }
        #region 控制源输出
        /// <summary>
        /// 控制源输出
        /// </summary>
        /// <returns>控源结果</returns>
        protected  bool Power_On()
        {
            float[] IbImax = OneMeterInfo.GetIb();
            string aa = Test_Value;
            string[] bb = aa.Split('|');

            float powerOutI = Core.Function.Number.GetCurrentByIb(bb[1], IbImax[0].ToString(), HGQ);
            Cus_PowerYuanJian ele = Cus_PowerYuanJian.H;
            //如果是单相，只输出A元
            if (EquipmentData.Equipment.EquipmentType == "单相台") ele = Cus_PowerYuanJian.A;
            MessageAdd(string.Format($"开始控制功率源输出:{OneMeterInfo.MD_UB}V {powerOutI}A"), EnumLogType.提示信息);
            return PowerOn(OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, powerOutI, powerOutI, powerOutI, ele, PowerWay.正向有功, "1.0");
        }

        #endregion
        /// <summary>
        /// 检定参数是否合法
        /// </summary>
        /// <returns></returns>
        protected override bool CheckPara()
        {
            ResultNames = new string[] { "检定项目", "检定进度" };
            return true;
        }

        public PowerWay GetPowerWay(string powerWay)
        {
            switch (powerWay)
            {
                case "正向有功":
                    return PowerWay.正向有功;
                case "反向有功":
                    return PowerWay.反向有功;
                case "正向无功":
                    return PowerWay.正向无功;
                case "反向无功":
                    return PowerWay.反向无功;
                default:
                    return PowerWay.正向有功;
            }
        }

    }
}
