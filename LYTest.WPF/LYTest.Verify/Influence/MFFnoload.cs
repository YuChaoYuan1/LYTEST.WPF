using LYTest.Core;
using LYTest.Core.Enum;
using LYTest.ViewModel;
using LYTest.ViewModel.CheckController;
using System;
using System.Threading;

namespace LYTest.Verify.Influence
{
    /// <summary>
    /// 外部工频磁场（无负载条件） 试验
    /// </summary>
    /// 方案数据："电压倍数","持续时间(秒)"
    /// 结论数据："电压倍数"，"方案时间(秒）"，"实际时间(秒）"，"结论"
    /// 浸入试验方式； 磁感应强度为 0.5mT(400A/m)
    /// 试验时间： 20τ ， τ 为启动试验时间
    public class MFFnoload : VerifyBase
    {
        //磁场源电流为10A

        /// <summary>
        /// 持续时间(秒)
        /// </summary>
        float TheoryTime = 60;

        /// <summary>
        /// 电流倍数
        /// </summary>
        float UbX = 1.15f;

        int _trayArg = 90; //托盘角度
        int _coilArg = 0; //线圈角度
        //float _magneticCurrent = 5f; //磁场强度电流


        /// <summary>
        /// 磁场强度电流，最大电流6A
        /// </summary>
        float _magneticCurrent = 5f;

        public override void Verify()
        {
            base.Verify();

            #region 上传误差参数
            for (int i = 0; i < MeterNumber; i++)
            {
                if (!MeterInfo[i].YaoJianYn) continue;
                ResultDictionary["电压倍数"][i] = UbX.ToString("F2");
                ResultDictionary["方案时间(秒)"][i] = TheoryTime.ToString("F2");
                ResultDictionary["实际时间(秒)"][i] = "";
                ResultDictionary["结论"][i] = "";
            }
            RefUIData("电压倍数");
            RefUIData("方案时间(秒)");
            RefUIData("实际时间(秒)");
            RefUIData("结论");
            #endregion

            if (Stop) return;
            SetBluetoothModule(6);

            if (Stop) return;
            MessageAdd("正在升源...", EnumLogType.提示信息);
            if (!PowerOn(OneMeterInfo.MD_UB * UbX, OneMeterInfo.MD_UB * UbX, OneMeterInfo.MD_UB * UbX, 0, 0, 0, Cus_PowerYuanJian.H, PowerWay.正向有功, "1.0"))
            {
                return;
            }

            StartTime = DateTime.Now.AddSeconds(-VerifyConfig.Dgn_PowerSourceStableTime);

            if (Stop) return;
            MessageAdd("正在角度...", EnumLogType.提示信息);
            InitEquipment2(_trayArg, _coilArg);


            // 磁场升6A的电流  ，只升A相电流
            DeviceControl.PowerOn2(WireMode.三相四线, 0, 0, 0, _magneticCurrent, 0, 0, 0, 0, 0, 0, 0, 0);
            Thread.Sleep(3000);

            if (Stop) return;
            MessageAdd("正在启动误差版...", EnumLogType.提示信息);
            int[] meterconst = MeterHelper.Instance.MeterConst(true);
            if (!SetErrorData(0x01, 0x00, meterconst[0], 2))
            {
                MessageAdd("误差板启动失败...", EnumLogType.提示信息);
                return;
            }


            DateTime timeBegin = DateTime.Now;
            while (true)
            {

                MessageAdd($"潜动时间{TheoryTime:F2}秒，已经经过{DateTime.Now.Subtract(timeBegin).TotalSeconds:F2}秒", EnumLogType.提示信息);

                if (Stop) break;

                if (DateTime.Now.Subtract(timeBegin).TotalSeconds > TheoryTime)
                {
                    MessageAdd("时间已到，退出检定", EnumLogType.提示信息);
                    break;
                }

                if (Stop) break;
                if (!ReadErrorData(out float[] curWC))    //读取误差
                {
                    continue;
                }

                if (curWC[0] != 0)
                {
                    break;
                }

                if (Stop) break;
                Thread.Sleep(1000);
            }


            if (Stop) return;

            ResultDictionary["实际时间(秒)"][0] = DateTime.Now.Subtract(timeBegin).TotalSeconds.ToString("F4");

            ReadErrorData(out float[] curWC1);

            if (curWC1[0] != 0)
            {
                ResultDictionary["结论"][6] = ConstHelper.不合格;
            }
            else
            {
                ResultDictionary["结论"][0] = ConstHelper.合格;

            }
            RefUIData("实际时间(秒)");
            RefUIData("结论");


            MessageAdd("正在复位磁场...", EnumLogType.提示信息);
            DeviceControl.PowerOn2(WireMode.三相四线, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);


            MessageAdd("正在复位磁场位置...", EnumLogType.提示信息);
            InitEquipment2(0, 0);


            PowerOn();
            //WaitTime("检定完成，关闭电流", 5);
            MessageAdd("检定完成", EnumLogType.提示信息);
        }


        protected override bool CheckPara()
        {
            // 电流倍数，误差限，变差限
            string[] data = Test_Value.Split('|');

            UbX = 1.15f;
            TheoryTime = 180f;
            _trayArg = 0;
            _coilArg = 0;
            _magneticCurrent = 5f;
            if (data.Length >= 3)
            {
                if (float.TryParse(data[0], out float f))
                    UbX = f;

                if (float.TryParse(data[1], out float f1))
                    TheoryTime = f1;

                if (int.TryParse(data[2], out int f2))
                    _trayArg = f2;
                if (int.TryParse(data[3], out int f3))
                    _coilArg = f3;
                if (float.TryParse(data[4], out float f4))
                    _magneticCurrent = f4;
            }

            if (_magneticCurrent > 6)
                _magneticCurrent = 6;

            //"电压倍数"，"方案时间(秒）"，"实际时间(秒）"，"结论"
            ResultNames = new string[] { "电压倍数", "方案时间(秒)", "实际时间(秒)", "结论" };
            return true;
        }


        /// <summary>
        /// 读取并处理数据[演示版无效]
        /// </summary>
        /// <param name="verifyTimes"></param>
        private int ReadAndDealData()
        {

            StError[] arrTagError = ReadWcbData(GetYaoJian(), 6);
            if (Stop) return 0;

            //当所有表位均为不合格时,检定完毕
            CheckOver = true;
            for (int i = 0; i < MeterNumber; i++)
            {
                if (arrTagError[i] == null) continue;
                if (!MeterInfo[i].YaoJianYn) continue;

                int.TryParse(arrTagError[i].szError, out int intTemp);

                if (intTemp > 0)
                    return 1;
                else
                    return 0;
            }
            return 0;
        }

        ///// <summary>
        ///// 调整磁场表
        ///// </summary>
        ///// <param name="face">0-面1，1-面2，2-面3</param>
        ///// <returns></returns>
        //public bool InitEquipment2(int face)
        //{
        //    if (IsDemo) return true;

        //    if (face == 0) // 面1
        //    {
        //        EquipmentData.DeviceManager.LY2001_7000HSet(0, 0); // 正面
        //        EquipmentData.DeviceManager.LY2001_7000HSet(1, 0); // 正面
        //    }
        //    else if (face == 1) // 面2
        //    {
        //        EquipmentData.DeviceManager.LY2001_7000HSet(0, 90); // 顶面
        //    }
        //    else if (face == 2) // 面3
        //    {
        //        EquipmentData.DeviceManager.LY2001_7000HSet(1, 90); // 侧面
        //    }

        //    WaitTime("正在调整磁场检测面", 5);

        //    DateTime timeS = DateTime.Now;
        //    while (DateTime.Now.Subtract(timeS).TotalMilliseconds < 120 * 1000)
        //    {
        //        //读取线圈角度
        //        EquipmentData.DeviceManager.LY2001_7001HGet(out float angle1, out float angle2);
        //        if (face == 0 && Math.Abs(angle1) < 5)
        //            break;
        //        else if (face == 1 && Math.Abs(angle1 - 90) < 10)
        //            break;
        //        else if (face == 2 && Math.Abs(angle2 - 90) < 10)
        //            break;


        //        Thread.Sleep(1000);
        //    }


        //    return true;
        //}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="face">0-面1，1-面2，2-面3</param>
        /// <returns></returns>
        public bool InitEquipment2(int trayArg, int coilArg)
        {
            if (IsDemo) return true;

            if (trayArg > 90) trayArg = 90;
            if (trayArg < -90) trayArg = -90;
            if (coilArg > 90) coilArg = 90;
            if (coilArg < -90) coilArg = -90;

            EquipmentData.DeviceManager.LY2001_7000HSet(0, coilArg); // 线圈
            Thread.Sleep(500);
            EquipmentData.DeviceManager.LY2001_7000HSet(1, trayArg); // 托盘


            WaitTime("正在调整检测面", 5);

            DateTime timeS = DateTime.Now;
            while (DateTime.Now.Subtract(timeS).TotalMilliseconds < 60 * 1000)
            {
                //读取线圈角度
                EquipmentData.DeviceManager.LY2001_7001HGet(out float angle1, out float angle2);
                if (Math.Abs(angle1 - coilArg) < 5 && Math.Abs(angle2 - trayArg) < 5)
                    break;

                Thread.Sleep(1000);
            }


            return true;
        }

    }
}
