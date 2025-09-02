using LYTest.Core.Enum;
using LYTest.Core.Function;
using LYTest.ViewModel;
using LYTest.ViewModel.CheckController;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LYTest.Verify.QualityModule
{
    /// <summary>
    /// 电能质量模组三相不平衡试验检定//废弃
    /// </summary>
    public class DeviationThreePhaseUnbalance : VerifyBase
    {
        //百分之多少的电压
        string UnImbalance = "0.0";
        float UnImbalanceA = 1.0f;
        float UnImbalanceB = 1.0f;
        float UnImbalanceC = 1.0f;
        //百分之多少的电流
        string InImbalance = "0.0";
        float InImbalanceA = 1.0f;
        float InImbalanceB = 1.0f;
        float InImbalanceC = 1.0f;
        float UwcLimit = 0.01f;//误差限
        float IwcLimit = 0.01f;//误差限
        float xIb;
        public override void Verify()
        {
            base.Verify();
            MessageAdd("电能质量模组三相电流不平衡试验检定开始...", EnumLogType.提示信息);
            for (int j = 0; j < MeterNumber; j++)
            {
                if (!MeterInfo[j].YaoJianYn) continue;
                ResultDictionary["电压误差限%"][j] = "±" + UwcLimit.ToString();
                ResultDictionary["电流误差限%"][j] = "±" + IwcLimit.ToString();
            }
            RefUIData("电压误差限%");
            RefUIData("电流误差限%");
            xIb = Number.GetCurrentByIb("ib", OneMeterInfo.MD_UA, HGQ);//计算电流
            if (!PowerOn(OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, 0, 0, 0, Cus_PowerYuanJian.H, PowerWay.正向有功, "1.0"))
            {
                MessageAdd("升源失败,退出检定", EnumLogType.提示信息);
                return;
            }

            if (string.IsNullOrWhiteSpace(OneMeterInfo.MD_MeterNo) || string.IsNullOrWhiteSpace(OneMeterInfo.MD_PostalAddress))   //没有表号的情况获取一下
            {
                MessageAdd("正在获取所有表的表地址和表号", EnumLogType.流程信息);
                ReadMeterAddrAndNo();
                if (!IsDemo)
                {
                    UpdateMeterProtocol();//更新电表命令
                }
            }

            MeterLogicalAddressType = MeterLogicalAddressEnum.电能质量模组;
            #region 电压
            if (!PowerOn(OneMeterInfo.MD_UB* UnImbalanceA, OneMeterInfo.MD_UB* UnImbalanceB, OneMeterInfo.MD_UB* UnImbalanceC, 0, 0, 0, Cus_PowerYuanJian.H, PowerWay.正向有功, "1.0"))
            {
                MessageAdd("升源失败,退出检定", EnumLogType.提示信息);
                return;
            }
            CalculationProcess("150周波读取", 3, true);
            CalculationProcess("一分钟读取", 60, true);
            #endregion

            #region 电流
            if (!PowerOn(OneMeterInfo.MD_UB, OneMeterInfo.MD_UB , OneMeterInfo.MD_UB, xIb *InImbalanceA, xIb * InImbalanceB, xIb * InImbalanceC, Cus_PowerYuanJian.H, PowerWay.正向有功, "1.0"))
            {
                MessageAdd("升源失败,退出检定", EnumLogType.提示信息);
                return;
            }
            CalculationProcess("150周波读取", 3, false);
            CalculationProcess("一分钟读取", 60, false);
            #endregion
            for (int j = 0; j < MeterNumber; j++)
            {
                if (!MeterInfo[j].YaoJianYn) continue;
                ResultDictionary["结论"][j] = "合格";
            }
            RefUIData("结论");
        }

        private void CalculationProcess(string name, int WaitTimeSecond,bool IsVoltageOrElectricCurrent)
        {
            string oad = "电压不平衡(零序不平衡)";
            if (!IsVoltageOrElectricCurrent)
            {
                oad = "电流不平衡(零序不平衡)";
            }
            string[] readdata = MeterProtocolAdapter.Instance.ReadData(oad);
            WaitTime(name, WaitTimeSecond);
            if (IsVoltageOrElectricCurrent) {
                LoadResultDictionary("电压间隔测量值", readdata);
                LoadStdInfoResultDictionary("电压间隔标准值", ("电压ABC:" + EquipmentData.StdInfo.Ua + "-" + EquipmentData.StdInfo.Ub + "-" + EquipmentData.StdInfo.Uc));
                LoadResultDictionary("电压三相不平衡度相对误差", readdata);
                RefUIData("电压间隔测量值");
                RefUIData("电压间隔标准值");
                RefUIData("电压三相不平衡度相对误差");
            }
            else
            {
                LoadResultDictionary("电流间隔测量值", readdata);
                LoadStdInfoResultDictionary("电流间隔标准值", ("电压ABC:" + EquipmentData.StdInfo.Ia + "-" + EquipmentData.StdInfo.Ib + "-" + EquipmentData.StdInfo.Ic));
                LoadResultDictionary("电流三相不平衡度相对误差", readdata);
                RefUIData("电流间隔测量值");
                RefUIData("电流间隔标准值");
                RefUIData("电流三相不平衡度相对误差");
            }
            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name">结论名称</param>
        /// <param name="DecimalPlaces">小数位</param>
        private void LoadResultDictionary(string name, string[] Value)
        {
            for (int j = 0; j < MeterNumber; j++)
            {
                if (!MeterInfo[j].YaoJianYn) continue;
                ResultDictionary[name][j] = ResultDictionary[name][j].ToString() + Value[j] + "|";
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name">结论名称</param>
        /// <param name="DecimalPlaces">小数位</param>
        private void LoadStdInfoResultDictionary(string name, string Value)
        {
            for (int j = 0; j < MeterNumber; j++)
            {
                if (!MeterInfo[j].YaoJianYn) continue;
                ResultDictionary[name][j] = ResultDictionary[name][j].ToString() + Value + "|";
            }
        }


        protected override bool CheckPara()
        {
            string str = Test_Value;
            if (string.IsNullOrWhiteSpace(str)) return false;
            string[] strList = str.Split(',');
            UnImbalance = strList[0];
            UwcLimit = 0.15f;
            switch (UnImbalance)
            {
                case "0.0":
                    UnImbalanceA = 1.0f;
                    UnImbalanceB = 1.0f;
                    UnImbalanceC = 1.0f;
                    break;
                case "5.05":
                    UnImbalanceA = 0.73f;
                    UnImbalanceB = 0.80f;
                    UnImbalanceC = 0.87f;
                    break;
                case "4.95":
                    UnImbalanceA = 1.52f;
                    UnImbalanceB = 1.40f;
                    UnImbalanceC = 1.28f;
                    break;
            }
            InImbalance = strList[1];
            IwcLimit = 0.1f;
            switch (InImbalance)
            {
                case "0.0":
                    InImbalanceA = 1.0f;
                    InImbalanceB = 1.0f;
                    InImbalanceC = 1.0f;
                    break;
                case "5.05":
                    InImbalanceA = 0.73f;
                    InImbalanceB = 0.80f;
                    InImbalanceC = 0.87f;
                    break;
                case "4.95":
                    InImbalanceA = 1.52f;
                    InImbalanceB = 1.40f;
                    InImbalanceC = 1.28f;
                    break;
            }
            ResultNames = new string[] {  "电压误差限%", "电压间隔测量值", "电压间隔标准值", "电压三相不平衡度相对误差", "电流误差限%", "电流间隔测量值", "电流间隔标准值", "电流三相不平衡度相对误差", "平均值", "化整值", "结论" };
            return true;
        }
    }
}
