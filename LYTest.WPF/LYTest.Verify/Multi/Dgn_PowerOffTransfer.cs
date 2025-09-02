using LYTest.Core.Enum;
using LYTest.ViewModel.CheckController;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
 

namespace LYTest.Verify.Multi
{
    //  ///add lsj 20220218 停电转存试验
    //停电转存试验
    public class Dgn_PowerOffTransfer : VerifyBase
    {
        private string[] arrFreeze = null;
        public override void Verify()
        {
            base.Verify();
            if (Stop) return;
            bool[] result;
            string msg = "";
            DateTime dtFreezeTime;
            //for (int i = 1; i <= 2; i++)
            //{
            //    ClearItemData(string.Format("{0}{1:D2}", ItemKey, i));
            //}
            arrFreeze = new string[MeterNumber];
            //初始化设备
            if (!InitEquipment())
            {
                return;
            }

            if (Stop) return;
            //读取表地址和表号
            ReadMeterAddrAndNo();
            if (Stop) return;
            //身份认证
            Identity();
            MessageAdd("恢复电表时间", EnumLogType.提示信息);
            DateTime readTime = DateTime.Now;
            //写入电表时间
            MeterProtocolAdapter.Instance.WriteDateTime(readTime);
            MessageAdd("读取每月第1结算日", EnumLogType.提示信息);
            arrFreeze = MeterProtocolAdapter.Instance.ReadData("每月第1结算日");

            string strFreezeTime = arrFreeze[FirstIndex];
            for (int i = 0; i < MeterNumber; i++)
            {
                if (!MeterInfo[i].YaoJianYn) continue;

                if (arrFreeze[i] == null || arrFreeze[i] == "")
                    continue;

                if (strFreezeTime != arrFreeze[i])
                {
                    MessageAdd("有表位每月第1结算日不一致，试验终止", EnumLogType.提示信息);
                    Stop = true;
                    break;
                }
            }
            if (Stop) return;
            bool bReturnResult;
            for (int curNum = 1; curNum <= 2; curNum++)
            {
                if (Stop) return;
                Walk(PowerWay.正向有功);
                if (Stop) return;
                Walk(PowerWay.反向有功);
                if (Stop) return;
                FangXiang = PowerWay.正向有功;
                #region 将电表时间修改结算日前1分钟
                strFreezeTime = arrFreeze[FirstIndex];
                strFreezeTime = DateTime.Now.Year.ToString("D2") + curNum.ToString("D2") + strFreezeTime + "0000";
                dtFreezeTime = LYTest.Core.Function.DateTimes.FormatStringToDateTime(strFreezeTime);
                dtFreezeTime = dtFreezeTime.AddSeconds(-30);
                Identity();
                MessageAdd("将电表时间修改到结算日前30秒", EnumLogType.提示信息);
                result = MeterProtocolAdapter.Instance.WriteDateTime(dtFreezeTime);
                bReturnResult = true;
                msg = "";
                for (int i = 0; i < MeterNumber; i++)
                {
                    if (MeterInfo[i].YaoJianYn)
                    {
                        if (!result[i])
                        {
                            bReturnResult = false;
                            msg += (i + 1).ToString() + "号,";
                        }
                    }
                }
                if (!bReturnResult)
                {
                    MessageAdd("有电能表写表时间失败!请检查多功能协议配置、表接线或是表编程开关是否已经打开。\r\n当前检定中止", EnumLogType.提示信息);
                }
                MessageAdd("停电>>>>", EnumLogType.提示信息);
                PowerOff();
                if (curNum == 1)
                    MessageAdd("在00:00前停电，在00:30复电,正在等待", EnumLogType.提示信息);
                else
                    MessageAdd("在00:00前停电，在01:30复电,正在等待", EnumLogType.提示信息);

                int maxTime = 30 * 60 + (curNum - 1) * 3600;
                if (curNum == 1)
                    WaitTime("00:00前停电，在00:30复电", maxTime);
                else
                    WaitTime("00:00前停电，在01:30复电", maxTime);
                #endregion
                MessageAdd("复电>>>>", EnumLogType.提示信息);
                PowerOn();
                if (Stop) return;
                ReadEnergy(curNum);
                if (Stop) return;

            }
            Identity();
            MessageAdd("恢复电表时间", EnumLogType.提示信息);
            readTime = DateTime.Now;
            result = MeterProtocolAdapter.Instance.WriteDateTime(readTime);
            for (int i = 0; i < MeterNumber; i++)
            {
                if (MeterInfo[i].YaoJianYn)
                {
                    if (!result[i])
                    {
                        msg += (i + 1).ToString() + "号,";
                    }
                }
            }
            MessageAdd("停电转存实验完毕", EnumLogType.提示信息);
            //ControlResult(bResult);
        }

        private bool CompareEnergy(float[] f1, float[] f2)
        {
            if (f1 == null)
                return false;
            if (f1.Length == 0)
                return false;
            if (f1.Length != f2.Length)
                return false;
            for (int i = 0; i < f1.Length; i++)
            {
                if (Math.Abs(f1[i] - f2[i]) > 0.01)
                    return false;
            }
            return true;
        }
        private bool Walk(PowerWay fangxiang)
        {
            //string strGlys;
            switch (fangxiang)
            {
                case PowerWay.正向有功:
                    FangXiang = PowerWay.正向有功;
                    //strGlys = "1.0";
                    break;
                case PowerWay.反向有功:
                    FangXiang = PowerWay.反向有功;
                    //strGlys = "-1.0";
                    break;
                case PowerWay.第一象限无功:
                    FangXiang = PowerWay.正向无功;
                    //strGlys = "0.5L";
                    break;
                case PowerWay.第二象限无功:
                    FangXiang = PowerWay.正向无功;
                    //strGlys = "0.8C";
                    break;
                case PowerWay.第三象限无功:
                    FangXiang = PowerWay.反向无功;
                    //strGlys = "-0.8C";
                    break;
                case PowerWay.第四象限无功:
                    FangXiang = PowerWay.反向无功;
                    //strGlys = "-0.5L";
                    break;
            }
            float[] ib = OneMeterInfo.GetIb();
            if (PowerOn(OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, ib[0], ib[0], ib[0], Core.Enum.Cus_PowerYuanJian.H, PowerWay.正向有功, "1.0") == false)
            {
                MessageAdd("控制源输出失败", EnumLogType.提示信息);
                return false;
            }
            Thread.Sleep(300);
            WaitTime(Enum.GetName(typeof(PowerWay), fangxiang) + "走电量", 30);

            PowerOn(OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, 0, 0, 0, Core.Enum.Cus_PowerYuanJian.H, PowerWay.正向有功, "1.0");
            Thread.Sleep(300);
            return true;
        }

        //读取所以费率电量
        private bool[] ReadEnergy(int intTestTime)
        {
            string[] resultKey = new string[MeterNumber];
            object[] resultValue = new object[MeterNumber];
            Dictionary<int, float[]> dicEnergy = new Dictionary<int, float[]>();
            bool[] bResult = new bool[MeterNumber];
            for (int i = 0; i < MeterNumber; i++)
                bResult[i] = true;
            if (Stop) return bResult;
            MessageAdd("读取【当前组合有功】电量", EnumLogType.提示信息);
            Dictionary<int, float[]> dicEnergyZh=new Dictionary<int, float[]>();//= MeterProtocolAdapter.Instance.ReadEnergy(0x00, 0x00);//Todo:读取电量未写------------------------------------------------------------
            if (Stop) return bResult;
            MessageAdd("读取【上1结算日组合有功】电量", EnumLogType.提示信息);
            Dictionary<int, float[]> dicEnergyZhP1=new Dictionary<int, float[]>();// = MeterProtocolAdapter.Instance.ReadEnergys(0x00, 0x01);
            for (int i = 0; i < MeterNumber; i++)
            {
                if (!MeterInfo[i].YaoJianYn) continue;
                if (!CompareEnergy(dicEnergyZh[i], dicEnergyZhP1[i]))
                    bResult[i] = false;

                if (dicEnergy.ContainsKey(i) == false) continue;
               // 总结论
                //dgn.Value = "||||";
                //dgn.Value = string.Format("{0}|{1}", dicEnergyZh[i][0], dicEnergyZhP1[i][0]);
                ResultDictionary["组合有功总电量"][i] = string.Format("{0}", dicEnergyZh[i][0]);
                ResultDictionary["上一月组合有功总电量"][i] = string.Format("{0}", dicEnergyZhP1[i][0]);
                ResultDictionary["结论"][i] = bResult[i] ? "合格" : "不合格";
                if (Stop) return bResult;
            }
            RefUIData("组合有功总电量");
            RefUIData("上一月组合有功总电量");
            RefUIData("结论");
            return bResult;
         
        }
        /// <summary>
        /// 检定参数是否合法
        /// </summary>
        /// <returns></returns>
        protected override bool CheckPara()
        {
            ResultNames = new string[] { "组合有功总电量", "上一月组合有功总电量","组合有功总电量2","上一月组合有功总电量2", "结论" };
            return true;
        }
        /// <summary>
        /// 初始化设备参数,计算每一块表需要检定的圈数
        /// </summary>
        /// <returns></returns>
        private bool InitEquipment()
        {
            if (Stop) return false;
            MessageAdd("开始升电压...", EnumLogType.提示信息);
            if (Stop) return false;
            if (!PowerOn())
            {
               MessageAdd("升电压失败! ", EnumLogType.提示信息);
                return false;
            }
            if (Stop) return false;

            return true;
        }
    }
}
