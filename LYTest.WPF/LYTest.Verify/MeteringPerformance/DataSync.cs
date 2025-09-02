using LYTest.Core.Function;
using LYTest.ViewModel.CheckController;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LYTest.Verify.MeteringPerformance
{
    /// <summary>
    /// 数据同步
    /// </summary>
    public class DataSync : VerifyBase
    {

        float xib = 1f;
        float ub = 220f;
        string glys = "1.0";

        public override void Verify()
        {
            base.Verify();
            PowerOn();
            WaitTime("正在升源", 5);
            ReadMeterAddrAndNo();
            Identity(false);

            MessageAdd("开始电量清零", EnumLogType.提示信息);

            MeterLogicalAddressType = Core.Enum.MeterLogicalAddressEnum.计量芯;
            MeterProtocolAdapter.Instance.ClearEnergy();
            if (DAL.Config.ConfigHelper.Instance.IsITOMeter)//物联表的情况
            {
                MeterLogicalAddressType = Core.Enum.MeterLogicalAddressEnum.管理芯;
                MeterProtocolAdapter.Instance.ClearEnergy();
            }

            float[][] value = new float[5][];
            float[][] value2 = new float[5][];

            //需要在两种情况下测试
            //for (int i = 0; i < 2; i++)
            //{
            MessageAdd("开始升源", EnumLogType.提示信息);
            PowerOn(ub, ub, ub, xib, xib, xib, Core.Enum.Cus_PowerYuanJian.H, Core.Enum.PowerWay.正向有功, glys);

            WaitTime("当前状态下运行", 10);
            MessageAdd("开始读取管理芯瞬时数据", EnumLogType.提示信息);
            MeterLogicalAddressType = Core.Enum.MeterLogicalAddressEnum.管理芯;
            //功率因数--电压--电流--频率--有功功率--不差过百分20
            //读取模组时间--电能值-分钟冻结数据
            value[0] = GetValue("电压数据块", "管理芯");
            value[1] = GetValue("电流数据块", "管理芯");
            value[2] = GetValue("瞬时总有功功率", "管理芯");
            value[3] = GetValue("总功率因数", "管理芯");

            MessageAdd("开始读取计量芯瞬时数据", EnumLogType.提示信息);
            MeterLogicalAddressType = Core.Enum.MeterLogicalAddressEnum.计量芯;
            value2[0] = GetValue("电压数据块", "计量芯");
            value2[1] = GetValue("电流数据块", "计量芯");
            value2[2] = GetValue("瞬时总有功功率", "计量芯");
            value2[3] = GetValue("总功率因数", "计量芯");
            PowerOn();
            WaitTime("关闭电流", 5);

            MessageAdd("开始读取管理芯数据", EnumLogType.提示信息);
            //value = MeterProtocolAdapter.Instance.ReadData("(当前)组合有功总电能");//1.36
            MeterLogicalAddressType = Core.Enum.MeterLogicalAddressEnum.管理芯;
            value[4] = GetValue("(当前)组合有功总电能", "管理芯");

            DateTime[] date1 = MeterProtocolAdapter.Instance.ReadDateTime();
            for (int index = 0; index < MeterNumber; index++)
            {
                if (!MeterInfo[index].YaoJianYn || date1[index] == null) continue;
                ResultDictionary["管理芯时间"][index] = date1[index].ToString();
            }
            RefUIData("管理芯时间");
            MessageAdd("开始读取计量芯数据", EnumLogType.提示信息);
            MeterLogicalAddressType = Core.Enum.MeterLogicalAddressEnum.计量芯;
            value2[4] = GetValue("(当前)组合有功总电能", "计量芯");

            DateTime[] date2 = MeterProtocolAdapter.Instance.ReadDateTime();
            for (int index = 0; index < MeterNumber; index++)
            {
                if (!MeterInfo[index].YaoJianYn || date1[index] == null) continue;
                ResultDictionary["计量芯时间"][index] = date1[index].ToString();
            }
            RefUIData("计量芯时间");
            //判断结论

            for (int index = 0; index < MeterNumber; index++)
            {
                if (!MeterInfo[index].YaoJianYn) continue;
                ResultDictionary["结论"][index] = "合格";
                for (int j = 0; j < value.GetLength(0); j++)
                {
                    //没有读取到值的情况为不合格
                    if (value[j][index] == -1 || value2[j][index] == -1)
                    {
                        ResultDictionary["结论"][index] = "不合格";
                        continue;
                    }
                    //计量芯和管理芯片误差超出20%
                    if (value[j][index] > value2[j][index] * 1.2 || value[j][index] < value2[j][index] * 0.8)
                    {
                        ResultDictionary["结论"][index] = "不合格";
                        continue;
                    }
                }
                //判断时间差
                if (date2[index] == null || date1[index] == null)
                {
                    ResultDictionary["结论"][index] = "不合格";
                }
                TimeSpan ts = date2[index].Subtract(date1[index]);
                if (ts.TotalSeconds > 60)
                {
                    ResultDictionary["结论"][index] = "不合格";
                }
            }
            RefUIData("结论");
        }

        /// <summary>
        /// 读取电表指定值
        /// </summary>
        /// <param name="name">数据项名称</param>
        /// <param name="type">管理芯--计量芯</param>
        /// <returns></returns>
        private float[] GetValue(string name, string type)
        {
            float[] value = new float[MeterNumber];
            string[] tem = MeterProtocolAdapter.Instance.ReadData(name);
            for (int i = 0; i < MeterNumber; i++)
            {
                if (!MeterInfo[i].YaoJianYn) continue;
                if (tem[i] == null || tem[i] == "") tem[i] = "-1";
                value[i] = float.Parse(tem[i]);
            }

            string UIname = "";
            switch (name)
            {
                case "电压数据块":
                    UIname =  "电压";
                    break;
                case "电流数据块":
                    UIname = "电流";
                    break;
                case "瞬时总有功功率":
                    UIname = "功率";
                    break;
                case "总功率因数":
                    UIname = "功率因数";
                    break;
                case "(当前)组合有功总电能":
                    UIname = "总电能";
                    break;
                default:
                    break;
            }
            UIname = type+ UIname;
            for (int i = 0; i < MeterNumber; i++)
            {
                if (!MeterInfo[i].YaoJianYn) continue;
                ResultDictionary[ UIname][i] = tem[i];
            }
            RefUIData(UIname);
            return value;
        }

        /// <summary>
        /// 检定参数是否合法
        /// </summary>
        /// <returns></returns>
        protected override bool CheckPara()
        {
            //ResultNames = new string[] { "计量芯数据", "管理芯数据", "结论" };

            ResultNames = new string[] { "管理芯电压", "管理芯电流", "管理芯功率", "管理芯功率因数", "管理芯总电能", "管理芯时间",
             "计量芯电压", "计量芯电流", "计量芯功率", "计量芯功率因数", "计量芯总电能", "计量芯时间","结论"};


            
            try
            {
                string[] data = Test_Value.Split('|');

                xib = Number.GetCurrentByIb(data[1], OneMeterInfo.MD_UA, HGQ);
                ub = float.Parse(data[0].TrimEnd('%')) * OneMeterInfo.MD_UB/100;
                glys = data[2];
            }
            catch
            {
                return false;
            }


            return true;
        }


        #region 旧

        //public override void Verify()
        //{
        //    base.Verify();

        //    //将计量时间修改到每天
        //    //读取管理芯片时间，判断是否一起修改了
        //    //恢复时间

        //    //MeterLogicalAddressType = Core.Enum.MeterLogicalAddressEnum.计量芯;
        //    ReadMeterAddrAndNo();
        //    Identity(false);

        //    bool[] result = new bool[MeterNumber];
        //    for (int i = 0; i < MeterNumber; i++)
        //    {
        //        //表位的检定数据
        //        if (!meterInfo[i].YaoJianYn)
        //            continue;
        //        DateTime time = DateTime.Now.AddDays(1);
        //        MessageAdd("正在向计量芯中写入数据......", EnumLogType.提示信息);
        //        result[i] = MeterProtocolAdapter.Instance.WriteDateTime(time, i);
        //        if (result[i] == false)
        //        {
        //            MessageAdd($"表位{i + 1}第一次写入时间失败,开始重复写入", EnumLogType.流程信息);
        //            MeterProtocolAdapter.Instance.WriteDateTime(time, i);
        //        }
        //    }

        //    MessageAdd("读取计量芯数据", EnumLogType.提示信息);
        //    DateTime[] dates = MeterProtocolAdapter.Instance.ReadDateTime();
        //    for (int i = 0; i < MeterNumber; i++)
        //    {
        //        if (!meterInfo[i].YaoJianYn || dates[i] == null) continue;
        //        ResultDictionary["计量芯数据"][i] = dates[i].ToString();
        //    }
        //    RefUIData("计量芯数据");


        //    MeterLogicalAddressType = Core.Enum.MeterLogicalAddressEnum.管理芯;
        //    System.Threading.Thread.Sleep(50);
        //    MessageAdd("开始读取计量芯数据", EnumLogType.提示信息);
        //    DateTime[] dates2 = MeterProtocolAdapter.Instance.ReadDateTime();
        //    for (int i = 0; i < MeterNumber; i++)
        //    {
        //        if (!meterInfo[i].YaoJianYn || dates2[i] == null) continue;
        //        ResultDictionary["管理芯数据"][i] = dates2[i].ToString();
        //    }
        //    RefUIData("管理芯数据");

        //    for (int i = 0; i < MeterNumber; i++)
        //    {
        //        if (!meterInfo[i].YaoJianYn) continue;

        //        if (dates2[i] == null || dates[i] == null)
        //        {
        //            ResultDictionary["结论"][i] = "不合格";
        //        }

        //        TimeSpan ts = dates2[i].Subtract(dates[i]);
        //        if (ts.TotalSeconds < 300)
        //        {
        //            ResultDictionary["结论"][i] = "合格";
        //        }
        //    }
        //    RefUIData("结论");


        //    MessageAdd("正在恢复芯片时间", EnumLogType.提示信息);
        //    //MeterLogicalAddressType = Core.Enum.MeterLogicalAddressEnum.计量芯;

        //    result = new bool[MeterNumber];
        //    for (int i = 0; i < MeterNumber; i++)
        //    {
        //        //表位的检定数据
        //        if (!meterInfo[i].YaoJianYn)
        //            continue;
        //        DateTime time = DateTime.Now;
        //        result[i] = MeterProtocolAdapter.Instance.WriteDateTime(time, i);
        //        if (result[i] == false)
        //        {
        //            MessageAdd($"表位{i + 1}第一次写入时间失败,开始重复写入", EnumLogType.流程信息);
        //            MeterProtocolAdapter.Instance.WriteDateTime(time, i);
        //        }
        //    }

        //}

        ///// <summary>
        ///// 检定参数是否合法
        ///// </summary>
        ///// <returns></returns>
        //protected override bool CheckPara()
        //{
        //    ResultNames = new string[] { "计量芯数据", "管理芯数据", "结论" };
        //    return true;
        //}

        #endregion

    }
}
