using LYTest.Core.Function;
using LYTest.MeterProtocol.Protocols.DLT698.Enum;
using LYTest.ViewModel.CheckController;
using System;
using System.Collections.Generic;

namespace LYTest.Verify.AccurateTest
{
    public class TemperatureError : VerifyBase
    {

        #region 属性参数
        /// <summary>
        /// 测量的相线
        /// </summary>
        MeasurePhaseLine PhaseLine = MeasurePhaseLine.A相;
        /// <summary>
        /// 温度
        /// </summary>
        float Temperature;
        /// <summary>
        /// 温度的保持时间--恒温时间
        /// </summary>
        int KeepTime;
        /// <summary>
        /// 误差线
        /// </summary>
        float ErrorLimit;
        /// <summary>
        /// 是否恢复温度
        /// </summary>
        bool IsRecoveryTemperature;

        /// <summary>
        /// 恢复的温度--规程65℃以下
        /// </summary>
        readonly float RecoveryTemperature = 65f;
        /// <summary>
        /// 端子号
        /// </summary>
        byte TerminalNum;
        /// <summary>
        /// 进线端子
        /// </summary>
        byte Terminal_Enter;
        /// <summary>
        /// 出线端子
        /// </summary>
        byte Terminal_Out;
        #endregion

        public override void Verify()
        {
            base.Verify();
            Terminal_Enter = (byte)(((int)PhaseLine - 1) * 2 + 1);
            Terminal_Out = (byte)(Terminal_Enter + 1);
            TerminalNum = (byte)((1 << (Terminal_Enter - 1)) | (1 << (Terminal_Out - 1)));
            for (int i = 0; i < MeterNumber; i++)
            {
                if (!MeterInfo[i].YaoJianYn) continue;
                ResultDictionary["端子1"][i] = Terminal_Enter.ToString();
                ResultDictionary["端子2"][i] = Terminal_Out.ToString();
                ResultDictionary["误差限"][i] = ErrorLimit + "℃";
                ResultDictionary["实验温度"][i] = Temperature.ToString();
            }
            RefUIData("端子1");
            RefUIData("端子2");
            RefUIData("误差限");
            RefUIData("实验温度");


            MessageAdd("正在设置电压继电器", EnumLogType.提示与流程信息);
            //这里控制LY2001多功能板切换继电器到对应的相线
            DeviceControl.TemperaturePowerOn((int)PhaseLine, OneMeterInfo.MD_UB);
            WaitTime("正在升源", 3);
            if (Stop) return;

            MessageAdd("设置端子温度到" + Temperature + "℃", EnumLogType.提示与流程信息);
            //这里设置LY3522温度控制板到指定温度
            float[] SetTemperatureData = new float[8];
            SetTemperatureData.Fill(Temperature);
            DeviceControl.SetTemperature(SetTemperatureData, TerminalNum);
            if (Stop) return;

            //这里回读温度控制板温度，判断是否到达指定温度--不一定需要
            WaitTime("恒温", KeepTime);
            if (Stop) return;

            MessageAdd("正在读取电表端子温度", EnumLogType.提示与流程信息);
            //这里读取电表对应端子的温度
            if (!IsDemo)
            {
                if (OneMeterInfo.MD_MeterNo == null || OneMeterInfo.MD_MeterNo == "" || OneMeterInfo.MD_PostalAddress == null || OneMeterInfo.MD_PostalAddress == "")   //没有表号的情况获取一下
                {
                    MessageAdd("正在获取所有表的表地址", EnumLogType.提示信息);
                    ReadMeterAddrAndNo();
                }
            }
            if (DAL.Config.ConfigHelper.Instance.IsITOMeter) MeterLogicalAddressType = Core.Enum.MeterLogicalAddressEnum.计量芯;
            Dictionary<int, object[]> DicObj = MeterProtocolAdapter.Instance.ReadData(new List<string> { "20100400" }, EmSecurityMode.ClearText, EmGetRequestMode.GetRequestNormalList);
            for (int i = 0; i < MeterNumber; i++)
            {
                if (!MeterInfo[i].YaoJianYn) continue;
                if (!DicObj.ContainsKey(i) || DicObj[i] == null || DicObj[i].Length < 1) continue;
                //TODO 这里对读回来的数据解析
                ResultDictionary["端子1实际温度"][i] = DicObj[i][0].ToString();
                ResultDictionary["端子2实际温度"][i] = DicObj[i][0].ToString();
            }
            RefUIData("端子1实际温度");
            RefUIData("端子2实际温度");
            if (Stop) return;


            //暂时用实验温度来
            //这里将读回来的温度和设定的温度来计算误差
            //float[] readTemperature=new float[0];
            //for (int i = 0; i < 3; i++)   //以防读取失败，读取3次
            //{
            //    DeviceControl.GetTemperature(out readTemperature, 0xff);
            //    if (readTemperature[Terminal_Enter - 1] >0 && readTemperature[Terminal_Out - 1] > 0) break;
            //}

            if (Stop) return;
            // 这里根据误差判断结论是否合格
            for (int i = 0; i < MeterNumber; i++)
            {
                if (!MeterInfo[i].YaoJianYn) continue;
                if (ResultDictionary["实际温度"][i] == null || ResultDictionary["实际温度"][i] == "")
                {
                    ResultDictionary["结论"][i] = "不合格";
                }
                else
                {
                    float err1 = Math.Abs(Temperature - float.Parse(ResultDictionary["端子1实际温度"][i]));
                    float err2 = Math.Abs(Temperature - float.Parse(ResultDictionary["端子1实际温度"][i]));


                    ResultDictionary["端子1误差"][i] = err1.ToString();
                    ResultDictionary["端子2误差"][i] = err2.ToString();

                    if (err1 < ErrorLimit && err2 < ErrorLimit)
                    {
                        ResultDictionary["结论"][i] = "合格";
                    }
                    else
                    {
                        ResultDictionary["结论"][i] = "不合格";
                    }
                }
            }
            RefUIData("端子1误差");
            RefUIData("端子2误差");
            RefUIData("结论");

            if (Stop) return;
            //恢复温度
            if (IsRecoveryTemperature)
            {
                //这里关闭温控板的温度，并且开启风扇
                if (Stop) return;
                MessageAdd("正在关闭温度", EnumLogType.提示与流程信息);
                DeviceControl.SetTemperature(new float[8], 0);
                MessageAdd("正在开启风扇", EnumLogType.提示与流程信息);
                DeviceControl.SetDevice(5, true);

                //读取温控板温度，。直到他的温度低于 RecoveryTemperature设定温度65
                if (Stop) return;
                int Errornum = 0;  //读取失败次数
                DateTime TmpTime1 = DateTime.Now;//检定开始时间，用于判断是否超时
                MessageAdd("正在降温,直到温度低于" + RecoveryTemperature + "℃", EnumLogType.提示信息);
                while (true)
                {
                    if (Errornum > 3) break;//3次读取失败就退出读取了
                    if (TimeSubms(DateTime.Now, TmpTime1) > 120 * 1000) //
                    {
                        MessageAdd("超出预计时间,降温失败", EnumLogType.错误信息);
                        break;
                    }
                    if (DeviceControl.GetTemperature(out float[] readTemperature, 0xff))
                    {
                        if (readTemperature[Terminal_Enter - 1] < RecoveryTemperature && readTemperature[Terminal_Out - 1] < RecoveryTemperature)
                        {
                            break;
                        }
                    }
                    else
                    {
                        Errornum++;
                    }
                }

                MessageAdd("正在关闭风扇", EnumLogType.提示与流程信息);
                DeviceControl.SetDevice(5, false);
            }
        }
        /// <summary>
        /// 检定参数是否合法
        /// </summary>
        /// <returns></returns>
        protected override bool CheckPara()
        {
            try
            {
                string[] data = Test_Value.Split('|');
                PhaseLine = (MeasurePhaseLine)Enum.Parse(typeof(MeasurePhaseLine), data[0]);
                Temperature = float.Parse(data[1].TrimEnd('℃'));
                KeepTime = int.Parse(data[2]);
                ErrorLimit = float.Parse(data[3]);
                IsRecoveryTemperature = data[4] == "是";


                ResultNames = new string[] { "端子1", "端子2", "误差限", "实验温度", "端子1实际温度", "端子2实际温度", "端子1误差", "端子2误差", "结论" };
                return true;
            }
            catch (Exception ex)
            {
                MessageAdd("参数验证失败，请检查该项目的检定参数\r\n" + ex.ToString(), EnumLogType.错误信息);
                return false;
            }
        }

        /// <summary>
        /// 测量的相线
        /// </summary>
        private enum MeasurePhaseLine
        {
            A相 = 1,
            B相 = 2,
            C相 = 3,
            N相 = 4
        }
    }
}
