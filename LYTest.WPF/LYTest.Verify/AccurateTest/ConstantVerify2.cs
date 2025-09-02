using LYTest.Core;
using LYTest.Core.Enum;
using LYTest.Core.Function;
using LYTest.Core.Model;
using LYTest.Core.Model.DnbModel.DnbInfo;
using LYTest.Core.Model.Meter;
using LYTest.Core.Struct;
using LYTest.DAL.Config;
using LYTest.ViewModel;
using LYTest.ViewModel.CheckController;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace LYTest.Verify.AccurateTest
{
    ///<summary>
    /// 1：升源，只用电压
    /// 2：如果费用不是总，就要写时间进去-写对应方案费率时间
    /// 3：启动标准表--重置电量,启动脉冲计数
    /// ---判断：试验是中途停止：读低度，判断==止码，计算剩余电量，提示框继续未完试验，还是重新试验
    /// ---如果重新试验：
    /// 4：判断：如果手动录入起码，否则自动读取起码
    /// ---如果继续未完试验：---中途停止时读底度
    /// 5：升源-升电流
    /// 6：循环读取标准表电量
    /// 7：判断是时间还是电量到额定值--
    /// 8：关电流
    /// 9：再次读取误差版脉冲计数
    /// 10：再读取标准表,读取止码
    /// 11：写时间(对时)---不是总
    /// 12：对比判断
    /// 13：停止误差版
    /// </summary>
    class ConstantVerify2 : VerifyBase
    {
        private readonly string HalfwayCacheFile = "HalfwayDataCache.temp";
        StPlan_ZouZi curPoint = new StPlan_ZouZi();

        /// <summary>
        /// 标准表累积电量
        /// </summary>
        private float _StarandMeterDl = 0F;

        /// <summary>
        /// 标准表累积电量
        /// </summary>
        private float _StarandMeterDlm = 0F;
        private readonly decimal[] _HalfwayMeterPulse = new decimal[MeterInfo.Length];
        private readonly bool bluetoothPulses = true;//计不计算蓝牙脉冲

        private byte mc = 0x06;//脉冲计数

        private float demoEnergy = 0;


        /// <summary>
        /// 走字
        /// </summary>
        public override void Verify()
        {
            int resetITO = 0;
            int curstep = 0;
            try
            {
                MessageAdd("走字试验检定开始...", EnumLogType.流程信息);

                //基类确定检定ID
                base.Verify();
                #region 初始化工作

                Cus_ZouZiMethod _ZZMethod;
                StPlan_ZouZi _curPoint = curPoint;
                FangXiang = _curPoint.PowerFangXiang;
                //把方案时间分转化为秒
                int _MaxTestTime = (int)(_curPoint.UseMinutes * 60);
                _ZZMethod = _curPoint.ZouZiMethod;
                //设置误差计算器参数
                string[] arrData = new string[0];    //数据数组
                string strDesc = string.Empty;       //描述信息

                CheckOver = false;
                //获取走字的电流
                float testI = Number.GetCurrentByIb(curPoint.xIb, OneMeterInfo.MD_UA, HGQ);

                //初始化相关的电能表走字数据
                InitZZData(testI.ToString());

                if (Stop) return;
                #endregion

                #region //第一步升压,不升电流,因为电表只有在 升源后，才能进行通讯
                if (!CheckVoltageAndCurrent())
                {
                    return;
                }

                if (Stop) return;


                ulong constants = EquipmentData.DeviceManager.GetStdConst(testI);

                ConfigHelper.Instance.AutoGearTemp = false;

                StdGear(0x13, constants, OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, testI, testI, testI);
                //新标准表换档时间太慢，以下不进分支的话不够
                for (int i = 0; i < 3; i++)
                {
                    if (Stop) return;
                    Thread.Sleep(1000);
                }
                //表少也可能不够

                VerifyConfig.Test_ZouZi_Control = true;

                if (Stop) return;
                #endregion

                #region //第二步 如果不是总费率的话，需要 将电能表改成费率时间
                if (_curPoint.FeiLv != Cus_FeiLv.总 && !IsDemo)
                {
                    if (Stop) return;
                    ReadMeterAddrAndNo();

                    if (Stop) return;
                    Identity(false);
                    string tempDate = $"{DateTime.Now:MM-dd}"; //"07-01";
                    if (string.IsNullOrWhiteSpace(_curPoint.StartTime))
                    {
                        MessageAdd("正在读取表内费率时段", EnumLogType.提示信息);
                        int dayN = 1;
                        string[] dayCounts = MeterProtocolAdapter.Instance.ReadData("日时段表数");
                        int dayCount = 2;
                        for (int i = 0; i < dayCounts.Length; i++)
                        {
                            if (!string.IsNullOrWhiteSpace(dayCounts[i]))
                            {
                                int.TryParse(dayCounts[i], out dayCount);
                                break;
                            }
                        }

                        bool findall = false;
                        for (int i = 0; i < dayCount; i++)
                        {
                            string[] readDatas = MeterProtocolAdapter.Instance.ReadData($"第一套第{i + 1}日时段数据");
                            for (int j = 0; j < readDatas.Length; j++)
                            {
                                if (!string.IsNullOrWhiteSpace(readDatas[j]))
                                {
                                    string temp = readDatas[j];
                                    int x = 4;
                                    while (x + 2 <= temp.Length)
                                    {
                                        FL_Data fld = new FL_Data
                                        {
                                            Time = $"{temp.Substring(x - 4, 2)}:{temp.Substring(x - 2, 2)}",
                                            Type = (Cus_FeiLv)int.Parse(temp.Substring(x, 2))
                                        };
                                        if (fld.Type == _curPoint.FeiLv)
                                        {
                                            _curPoint.StartTime = fld.Time;
                                            findall = true;
                                            break;
                                        }
                                        x += 6;
                                    }

                                    if (findall)
                                    {
                                        dayN = i + 1;
                                        break;
                                    }
                                    break;
                                }
                                if (findall) break;
                            }
                            if (findall) break;
                        }

                        string[] readtzs = MeterProtocolAdapter.Instance.ReadData("第一套时区表数据");
                        bool findtz = false;
                        for (int i = 0; i < readtzs.Length; i++)
                        {
                            if (!string.IsNullOrWhiteSpace(readtzs[i]))
                            {
                                string temp = readtzs[i];
                                int x = 4;
                                while (x + 2 <= temp.Length)
                                {
                                    if (dayN == int.Parse(temp.Substring(x, 2)))
                                    {
                                        tempDate = $"{temp.Substring(x - 4, 2)}-{temp.Substring(x - 2, 2)}";
                                        findtz = true;
                                        break;
                                    }
                                    x += 6;
                                }
                                break;
                            }
                            if (findtz) break;
                        }

                    }

                    if (string.IsNullOrWhiteSpace(_curPoint.StartTime))
                    {
                        string msg = string.Format("当前使用的费率时段没有【{0}】时段", _curPoint.FeiLv) + "\r\n";
                        MessageAdd(msg, EnumLogType.错误信息);
                        return;
                    }

                    if (Stop) return;


                    string time = _curPoint.StartTime;
                    DateTime dt = DateTime.Now;
                    int hh = int.Parse(_curPoint.StartTime.Substring(0, 2));
                    int mm = int.Parse(_curPoint.StartTime.Substring(3, 2));
                    int MM = int.Parse(tempDate.Substring(0, 2));
                    int dd = int.Parse(tempDate.Substring(3, 2));
                    dt = new DateTime(dt.Year, MM, dd, hh, mm, 0);
                    bool[] t = MeterProtocolAdapter.Instance.WriteDateTime(dt);

                    if (Stop) return;

                    for (int i = 0; i < t.Length; i++)
                    {
                        if (MeterInfo[i].YaoJianYn)
                        {
                            if (!t[i])
                            {
                                t[i] = MeterProtocolAdapter.Instance.WriteDateTime(dt, i);
                                if (!t[i])
                                {
                                    MessageAdd($"表位{i + 1}切换时段失败！", EnumLogType.错误信息);
                                }
                                //return;
                            }
                        }
                    }
                }
                #endregion

                #region // 第三步，读取起始电量
                if (Stop) return;

                if (ReadMeterEnergys(true) == false)
                {
                    MessageAdd("读取起始电量失败,本项检定将终止", EnumLogType.错误信息);
                    return;
                }
                #endregion

                if (Stop) return;

                #region ---判断中途走字
                if (VerifyConfig.Test_ZouZi_HalfwayMode)
                {
                    if (System.IO.File.Exists(HalfwayCacheFile))
                    {
                        string[] lines = System.IO.File.ReadAllLines(HalfwayCacheFile);
                        if (lines.Length >= MeterNumber)
                        {
                            bool consistent = true;
                            for (int i = 0; i < MeterNumber; i++)
                            {
                                string[] data = lines[i].Split(',');
                                if (!MeterInfo[i].YaoJianYn)
                                {
                                    if (!"false".Equals(data[0]))
                                    {
                                        consistent = false;
                                        break;
                                    }
                                }
                                else
                                {
                                    if (data.Length >= 9)
                                    {
                                        if (!MeterInfo[i].MD_PostalAddress.Equals(data[1]))
                                        {
                                            consistent = false;
                                            break;
                                        }
                                    }
                                }
                            }
                            if (consistent)
                            {
                                if (DialogResult.Yes == MessageBox.Show("检测到走字未完中途停止，选择\r\n\r\n\t是：继续剩余走字\r\n\r\n\t否：重新走字\r\n", "选择提示", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1, MessageBoxOptions.ServiceNotification))
                                {
                                    for (int i = 0; i < MeterNumber; i++)
                                    {
                                        string[] data = lines[i].Split(',');
                                        if (!MeterInfo[i].YaoJianYn) continue;
                                        ResultDictionary["起码"][i] = data[2];

                                        _HalfwayMeterPulse[i] = decimal.Parse(data[5]);
                                        ResultDictionary["标准表脉冲"][i] = data[6];
                                        _StarandMeterDlm = float.Parse(data[7]);
                                    }
                                    RefUIData("起码");
                                }
                            }

                        }
                    }
                }
                #endregion

                #region //第四步，发送走字指令，开始走字
                // 2:(启动标准表--重置电量)
                MessageAdd("启动标准表--重置电量", EnumLogType.提示信息);
                if (!IsDemo)
                {
                    if (bluetoothPulses)
                    {
                        SetBluetoothModule(mc);
                    }
                    StartStdEnergy(31);
                    if (Stop) return;
                    MessageAdd("启动误差板脉冲计数", EnumLogType.提示信息);
                    StartWcb(mc, 0xff); //启动误差板脉冲计数
                    #endregion

                    #region //第五步，升走字电流
                    if (Stop) return;
                    MessageAdd("正在升电流...", EnumLogType.提示信息);
                    if (!PowerOn(OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, testI, testI, testI, curPoint.PowerYj, curPoint.PowerFangXiang, curPoint.Glys))
                    {
                        MessageAdd("升源失败,退出检定", EnumLogType.错误信息);
                        return;
                    }
                }
                //add yjt 20220520 新增等待时间的获取
                int timeWaitSource = DAL.Config.ConfigHelper.Instance.Dgn_PowerSourceStableTime;
                if (IsDemo)
                {
                    timeWaitSource = 1;
                }

                if (Stop) return;
                #endregion
                curstep = 5;
                #region //第六步，控制 执行步骤
                string stroutmessage = string.Empty;
                double deduct = CalckWhOfSeconds(1.5f, testI, curPoint.PowerYj, curPoint.Glys);
                double maxSeconds = CalcSecondsOfkWh(_curPoint.UseMinutes, testI, curPoint.PowerYj, curPoint.Glys) * 1.2;
                DateTime startTime = DateTime.Now.AddSeconds(-timeWaitSource);   //检定开始时间,减掉源等待时间
                _StarandMeterDl = 0;                        //标准表电量
                DateTime lastTime = DateTime.Now.AddSeconds(-timeWaitSource - 1);

                while (true)
                {
                    Thread.Sleep(1000);
                    if (Stop) return;
                    float _PastTime = (float)DateTimes.DateDiff(startTime);

                    if (TimeSubms(DateTime.Now, startTime) > maxSeconds * 1000)//超出最大处理时间
                    {
                        MessageAdd("超出最大处理时间,正在退出...", EnumLogType.提示信息);
                        break;
                    }

                    #region
                    if (!IsDemo)
                    {
                        //记录标准表电量
                        float pSum = 0;
                        if (IsYouGong)
                        {
                            pSum = Math.Abs(EquipmentData.StdInfo.P);
                        }
                        else
                        {
                            pSum = Math.Abs(EquipmentData.StdInfo.Q);
                        }

                        float pastSecond = (float)(DateTime.Now - lastTime).TotalMilliseconds;
                        lastTime = DateTime.Now;
                        _StarandMeterDl += pastSecond * pSum / 3600 / 1000 / 1000;
                        if (arrData.Length < MeterNumber)
                        {
                            arrData = new string[MeterNumber];
                        }
                        StError[] errors = ReadWcbData(GetYaoJian(), mc);
                        for (int i = 0; i < errors.Length; i++)
                        {
                            if (errors[i] == null) continue;
                            arrData[i] = errors[i].szError;

                        }

                        pastSecond = (float)(DateTime.Now - lastTime).TotalMilliseconds;
                        lastTime = DateTime.Now;
                        _StarandMeterDl += pastSecond * pSum / 3600 / 1000 / 1000;
                        int xb = 9;
                        if (!IsYouGong)
                            xb = 10;

                        //读标准标电量kWh
                        float[] bzbdl = ReadStmEnergy();

                        if (bzbdl != null && bzbdl.Length > xb && bzbdl[xb] != 0)
                        {
                            _StarandMeterDl = bzbdl[xb];
                        }
                        if (_StarandMeterDl < 0)
                        {
                            _StarandMeterDl = Math.Abs(_StarandMeterDl);
                        }
                    }
                    else
                    {
                        //模拟电量
                        _StarandMeterDl = _PastTime * OneMeterInfo.MD_UB * testI / 3600000F;
                        //同步模拟脉冲数
                    }
                    //如果电量达到设定，停止
                    if (_StarandMeterDl + _StarandMeterDlm >= _curPoint.UseMinutes - deduct)
                    {
                        CheckOver = true;
                    }

                    // add yjt 20220327 新增演示模式直接合格
                    if (IsDemo)
                    {
                        CheckOver = true;
                    }
                    //如果脉冲数达到设定，也停止
                    decimal flt_C = 0;
                    if (arrData != null && arrData.Length > 0)
                    {
                        decimal.TryParse(arrData[FirstIndex], out flt_C);
                    }
                    flt_C = (flt_C + _HalfwayMeterPulse[FirstIndex]) / OneMeterInfo.GetBcs()[0];
                    if (flt_C >= Convert.ToDecimal(_curPoint.UseMinutes - deduct))
                    {
                        CheckOver = true;
                    }

                    stroutmessage = string.Format("方案设置走字电量：{0}度，已经走字：{1}度", _curPoint.UseMinutes, (_StarandMeterDl + _StarandMeterDlm).ToString("F5"));
                    #endregion

                    #region 更新数据
                    //缓存数据
                    for (int i = 0; i < MeterNumber; i++)
                    {
                        TestMeterInfo _meterInfo = MeterInfo[i];
                        if (!_meterInfo.YaoJianYn)
                        {
                            continue;
                        }

                        //"表脉冲", "标准表脉冲"
                        if (arrData != null && arrData.Length > i)
                        {
                            ResultDictionary["表脉冲"][i] = (decimal.Parse(arrData[i]) + _HalfwayMeterPulse[i]).ToString("F0");
                        }

                        //modify yjt 20220520 修改演示模式下的标准表脉冲
                        if (IsDemo)
                        {
                            ResultDictionary["标准表脉冲"][i] = (_StarandMeterDl * _meterInfo.GetBcs()[0] + _meterInfo.GetBcs()[0]).ToString("F2");
                        }
                        else
                        {
                            ResultDictionary["标准表脉冲"][i] = ((_StarandMeterDl + _StarandMeterDlm) * _meterInfo.GetBcs()[0]).ToString("F2");
                        }
                    }
                    RefUIData("表脉冲");
                    RefUIData("标准表脉冲");

                    #endregion
                    MessageAdd(stroutmessage, EnumLogType.提示信息);
                    if (CheckOver)
                    {
                        break;
                    }
                }
                #endregion
                curstep = 6;
                #region //第七步,升压,不升电流
                MessageAdd("正在升源...", EnumLogType.提示信息);
                if (!PowerOn(OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, 0, 0, 0, curPoint.PowerYj, curPoint.PowerFangXiang, curPoint.Glys))
                {
                    MessageAdd("升源失败,退出检定", EnumLogType.错误信息);
                    return;
                }
                if (Stop) return;
                #endregion
                curstep = 7;
                #region //第八步 读终止时的电量或脉冲数

                if (!IsDemo)
                {
                    if (arrData.Length < MeterNumber)
                    {
                        arrData = new string[MeterNumber];
                    }
                    StError[] errors = ReadWcbData(GetYaoJian(), mc);
                    bool[] TwoReadWc = new bool[MeterNumber];
                    TwoReadWc.Fill(false);
                    for (int i = 0; i < errors.Length; i++)
                    {
                        if (errors[i] == null) continue;
                        arrData[i] = errors[i].szError;
                        if (arrData[i] == "0") TwoReadWc[i] = true;
                    }
                    //有表位没用读取到脉冲，二次读取
                    if (Array.IndexOf(TwoReadWc, true) != -1)
                    {
                        errors = ReadWcbData(TwoReadWc, mc);
                        for (int i = 0; i < TwoReadWc.Length; i++)
                        {
                            if (errors[i] == null) continue;
                            if (!TwoReadWc[i]) continue;
                            arrData[i] = errors[i].szError;
                        }
                    }


                    int xb = 9;

                    if (!IsYouGong)
                        xb = 10;

                    //读标准标电量kWh
                    float[] bzbdl = ReadStmEnergy();

                    if (bzbdl != null && bzbdl.Length > xb && bzbdl[xb] != 0)
                    {
                        _StarandMeterDl = bzbdl[xb];
                    }

                    //_StarandMeterDl = float.Parse((float.Parse(readEnrgyZh311()[xb]) / 3600 / 1000).ToString("0.0000"));

                    if (_StarandMeterDl < 0)
                    {
                        _StarandMeterDl = Math.Abs(_StarandMeterDl);
                    }
                }
                else
                {
                    if (arrData.Length < MeterNumber)
                    {
                        arrData = new string[MeterNumber];
                    }
                    for (int i = 0; i < MeterNumber; i++)
                    {
                        if (!MeterInfo[i].YaoJianYn) continue;

                        arrData[i] = (curPoint.UseMinutes * MeterInfo[i].GetBcs()[0]).ToString("F0");
                        _StarandMeterDl = curPoint.UseMinutes - _StarandMeterDlm;
                    }
                }
                if (Stop) return;

                //if (_ZZMethod != Cus_ZouZiMethod.计读脉冲法)
                {
                    if (bluetoothPulses && ConfigHelper.Instance.IsITOMeter)
                    {
                        OutITOTestModelInitIto();
                        ++resetITO;
                    }
                    if (ReadMeterEnergys(false) == false)
                    {
                        MessageAdd("读取终止电表量失败", EnumLogType.错误信息);
                        return;
                    }
                }

                //缓存数据
                for (int i = 0; i < MeterNumber; i++)
                {
                    TestMeterInfo meter = MeterInfo[i];
                    if (!meter.YaoJianYn)
                    {
                        continue;
                    }
                    //"表脉冲", "标准表脉冲"
                    if (arrData != null && arrData.Length > i)
                    {
                        ResultDictionary["表脉冲"][i] = (decimal.Parse(arrData[i]) + _HalfwayMeterPulse[i]).ToString("F0");
                        ResultDictionary["标准表脉冲"][i] = ((_StarandMeterDl + _StarandMeterDlm) * meter.GetBcs()[0]).ToString("F4");
                    }

                }
                RefUIData("表脉冲");
                RefUIData("标准表脉冲");

                #endregion
                curstep = 8;
                #region //第九步将 电表时间改成 计算机当前时间
                if (_curPoint.FeiLv != Cus_FeiLv.总 && !IsDemo)
                {
                    MessageAdd("正在修改表时间为当前时间..", EnumLogType.提示信息);
                    MeterProtocolAdapter.Instance.WriteDateTime(GetYaoJian());
                }
                #endregion
                curstep = 9;
                #region 第十步，关源,检定基类处理
                //isSc = EquipHelper.Instance.PowerOn(0, 0, _curPoint.PowerYj, _curPoint.PowerFangXiang, _curPoint.Glys, IsYouGong, false);
                //if (isSc == false)
                //{
                //    Stop = true;
                //}

                #endregion

                #region //第十一步，计算误差
                try
                {
                    ControlZZResult(_curPoint, _ZZMethod, arrData, curPoint.ItemKey);
                }
                catch
                {
                    MessageAdd("计算走字误差时出现错误", EnumLogType.提示信息);
                }
                try
                {
                    ControlResult();
                }
                catch
                {
                }
                #endregion

                curstep = 12;
                StopWcb(mc, 0xff); //停止误差板计数
                MessageAdd("走字试验检定结束...", EnumLogType.流程信息);
            }
            finally
            {
                VerifyConfig.Test_ZouZi_Control = false;
                DAL.Config.ConfigHelper.Instance.AutoGearTemp = true;
                StdGear(0x13, 0, 0, 0, 0, 0, 0, 0);
                if (VerifyConfig.Test_ZouZi_HalfwayMode && !IsDemo)
                {
                    if (curstep < 12)
                    {
                        #region //第12步,升压,不升电流
                        MessageAdd("正在升源...", EnumLogType.提示信息);
                        if (!PowerOn(OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, 0, 0, 0, curPoint.PowerYj, curPoint.PowerFangXiang, curPoint.Glys))
                        {

                        }
                        #endregion

                        #region //第13步 读终止时的电量或脉冲数
                        string[] pulses = new string[MeterNumber];
                        StError[] readErrs = ReadWcbDataLast(GetYaoJian(), mc);
                        {
                            bool[] TwoReadWc = new bool[MeterNumber];
                            TwoReadWc.Fill(false);
                            for (int i = 0; i < readErrs.Length; i++)
                            {
                                if (readErrs[i] == null) continue;
                                pulses[i] = readErrs[i].szError;
                                if (pulses[i] == "0") TwoReadWc[i] = true;
                            }
                            //有表位没用读取到脉冲，二次读取
                            if (Array.IndexOf(TwoReadWc, true) != -1)
                            {
                                readErrs = ReadWcbDataLast(TwoReadWc, mc);
                                for (int i = 0; i < TwoReadWc.Length; i++)
                                {
                                    if (readErrs[i] == null) continue;
                                    if (!TwoReadWc[i]) continue;
                                    pulses[i] = readErrs[i].szError;
                                }
                            }


                            int xb = 9;

                            if (!IsYouGong)
                                xb = 10;

                            //读标准标电量kWh
                            float[] bzbdl = ReadStmEnergy();

                            if (bzbdl != null && bzbdl.Length > xb && bzbdl[xb] != 0)
                            {
                                _StarandMeterDl = bzbdl[xb];
                            }

                            if (_StarandMeterDl < 0)
                            {
                                _StarandMeterDl = Math.Abs(_StarandMeterDl);
                            }
                        }

                        //if (curPoint.ZouZiMethod != Cus_ZouZiMethod.计读脉冲法)
                        {
                            if (bluetoothPulses && ConfigHelper.Instance.IsITOMeter && resetITO == 0) OutITOTestModelInitIto();
                            if (ReadMeterEnergys(false) == false)
                            {
                            }
                        }

                        //缓存数据
                        for (int i = 0; i < MeterNumber; i++)
                        {
                            TestMeterInfo meter = MeterInfo[i];
                            if (!meter.YaoJianYn)
                                continue;

                            //"表脉冲", "标准表脉冲"
                            if (pulses != null && pulses.Length > i)
                            {
                                decimal.TryParse(pulses[i], out decimal pulse);
                                ResultDictionary["表脉冲"][i] = (pulse + _HalfwayMeterPulse[i]).ToString("F0");
                                ResultDictionary["标准表脉冲"][i] = ((_StarandMeterDl + _StarandMeterDlm) * meter.GetBcs()[0]).ToString("F4");
                            }

                        }

                        #endregion

                        bool all = true;
                        for (int i = 0; i < MeterNumber; i++)
                        {
                            TestMeterInfo meter = MeterInfo[i];
                            if (!meter.YaoJianYn) continue;
                            if (ResultDictionary["结论"][i] != ConstHelper.合格)
                            {
                                all = false;
                                break;
                            }
                        }
                        if (!all && curstep < 12)
                        {
                            StringBuilder builder = new StringBuilder();
                            for (int i = 0; i < MeterNumber; i++)
                            {
                                TestMeterInfo meter = MeterInfo[i];
                                if (!meter.YaoJianYn)
                                {
                                    builder.AppendLine("false");
                                }
                                else
                                {
                                    //要检,表地址,起码,止码,表码差,表脉冲,标准表脉冲,标准电量,当前时间
                                    builder.Append("true,").Append(meter.MD_PostalAddress).Append(",").Append(ResultDictionary["起码"][i]).Append(",").Append(ResultDictionary["止码"][i]).Append(",").Append(ResultDictionary["表码差"][i]).Append(",").Append(ResultDictionary["表脉冲"][i]).Append(",").Append(ResultDictionary["标准表脉冲"][i]).Append(",").Append(_StarandMeterDl + _StarandMeterDlm).Append(",").AppendLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                                }
                            }
                            System.IO.File.WriteAllText(HalfwayCacheFile, builder.ToString());
                        }
                        else
                        {
                            if (System.IO.File.Exists(HalfwayCacheFile))
                                System.IO.File.Delete(HalfwayCacheFile);
                        }
                    }
                    else
                    {
                        if (System.IO.File.Exists(HalfwayCacheFile))
                            System.IO.File.Delete(HalfwayCacheFile);
                    }
                }
            }
        }

        protected override bool CheckPara()
        {
            string[] _Prams = Test_Value.Split('|');
            if (_Prams.Length < 8) return false;
            //StPlan_ZouZi curPoint = new StPlan_ZouZi();
            curPoint.FeiLv = (Cus_FeiLv)Enum.Parse(typeof(Cus_FeiLv), _Prams[5]);
            curPoint.FeiLvString = _Prams[5];
            curPoint.Glys = _Prams[2];
            curPoint.PowerFangXiang = (PowerWay)Enum.Parse(typeof(PowerWay), _Prams[0]);
            curPoint.PowerYj = (Cus_PowerYuanJian)Enum.Parse(typeof(Cus_PowerYuanJian), _Prams[1]);
            curPoint.xIb = _Prams[3];
            curPoint.ZouZiMethod = (Cus_ZouZiMethod)Enum.Parse(typeof(Cus_ZouZiMethod), _Prams[4]);
            if (string.IsNullOrWhiteSpace(_Prams[6]) && string.IsNullOrWhiteSpace(_Prams[7]))
            {
                MessageAdd($"方案配置的走字电量和走字时间都是空，至少输入一个值！", EnumLogType.错误信息);
                return false;
            }
            float kWh = 0;
            float min = 0;
            if (!string.IsNullOrWhiteSpace(_Prams[6]) && !float.TryParse(_Prams[6], out kWh))
            {
                MessageAdd($"方案配置的走字电量格式错误：{_Prams[6]}，应输入数字，不输单位", EnumLogType.错误信息);
                return false;
            }
            if (!string.IsNullOrWhiteSpace(_Prams[7]) && !float.TryParse(_Prams[7], out min))
            {
                MessageAdd($"方案配置的走字时间格式错误：{_Prams[7]}，应输入数字，不输单位", EnumLogType.错误信息);
                return false;
            }
            string dufen;
            if (!string.IsNullOrWhiteSpace(_Prams[7]) && min > 0)
            {
                curPoint.UseMinutes = min;
                dufen = _Prams[7] + "分";
            }
            else if (!string.IsNullOrWhiteSpace(_Prams[6]) && kWh > 0)
            {
                curPoint.UseMinutes = kWh;
                dufen = _Prams[6] + "度";
            }
            else
            {
                MessageAdd($"方案配置的走字电量和走字时间都是0，至少输入一个有效值！", EnumLogType.错误信息);
                return false;
            }

            curPoint.ZouZiPrj = new List<StPlan_ZouZi.StPrjFellv>() {
                new StPlan_ZouZi.StPrjFellv()
                {
                    FeiLv= (Cus_FeiLv)Enum.Parse(typeof(Cus_FeiLv), _Prams[5]),
                    StartTime="",
                    ZouZiTime=dufen
                }
            };
            curPoint.ZuHeWc = "0";

            if (_Prams.Length > 8)
            {
                if (_Prams[8].IndexOf(':') >= 0 || _Prams[8].IndexOf('：') >= 0)
                {
                    curPoint.StartTime = _Prams[8].Replace("：", ":");
                }
            }

            bool Result = true;
            TestType testMethod = TestType.默认;
            string[] powerDirect = new string[1];
            //取当前检定方案中的所有功率方向
            powerDirect[0] = ((int)(PowerWay)Enum.Parse(typeof(PowerWay), _Prams[0])).ToString();
            //检测每一个检定方向下的费率
            for (int i = 0; i < powerDirect.Length; i++)
            {
                string[] feilv = new string[1];

                int zNum = 0;
                //取当前功率方向下的费率时段
                feilv[0] = ((int)((Cus_FeiLv)Enum.Parse(typeof(Cus_FeiLv), _Prams[5]))).ToString();
                //当走字方式为总与分同时做时，要求每一个方向只有一个总且第一个费率必须为总，
                if (testMethod == TestType.总与分费率同时做)
                {
                    if (feilv[0] != ((int)Cus_FeiLv.总).ToString())
                    {
                        MessageAdd("当走字方式为[" + testMethod.ToString() + "]时，第一个走字试验方案必须为[总]", EnumLogType.提示信息);
                        Result = false;
                        break;
                    }

                    for (int k = 0; k < feilv.Length; k++)
                    {

                        if (feilv[k] == ((int)Cus_FeiLv.总).ToString())
                            zNum++;
                        if (zNum > 1)
                        {
                            MessageAdd("当走字方式为[" + testMethod.ToString() + "]时，每一个功率方向允许有且仅允许有一个总费率方向方案", EnumLogType.提示信息);
                            return false;
                        }
                    }
                }
                else if (testMethod == TestType.自动检定总时段内的所有分费率)
                {
                    if (feilv[0] != ((int)Cus_FeiLv.总).ToString()) //第一个不为总
                    {
                        Result = false;
                        break;
                    }
                }
                else
                {
                    Result = true;
                }
            }
            //StPlan_ZouZi _curPoint = (StPlan_ZouZi)CurPlan;
            if (curPoint.ZouZiMethod == Cus_ZouZiMethod.基本走字法 && MeterHelper.Instance.TestMeterCount < 3)
            {
                MessageAdd("基本走字法至少要求有三块以上被检表!", EnumLogType.提示信息);
                return false;
            }

            ResultNames = new string[] { "起码", "止码", "表码差", "表脉冲", "标准表脉冲", "误差", "结论", "不合格原因" };
            if (curPoint.PowerFangXiang == PowerWay.正向无功 || curPoint.PowerFangXiang == PowerWay.反向无功)
            {
                mc = 0x07;
            }
            return Result;

        }


        #region 读取起码、止码、写表时间
        /// <summary>
        /// 读取电表能信息 
        /// </summary>
        /// <param name="isStartEnergy"></param>
        /// <returns></returns>
        private bool ReadMeterEnergys(bool isStartEnergy)
        {
            MessageAdd($"正在读取{(isStartEnergy ? "起始" : "结束")}电量...", EnumLogType.提示信息);


            if (DAL.Config.ConfigHelper.Instance.IsITOMeter) VerifyBase.MeterLogicalAddressType = MeterLogicalAddressEnum.管理芯;

            FloatE[] energys = new FloatE[MeterNumber];
            if (IsDemo)
            {
                for (int i = 0; i < MeterNumber; i++)
                {
                    Random rd = new Random();

                    if (demoEnergy == 0)
                    {
                        int itemz = rd.Next(3, 5);
                        double itemx = rd.NextDouble();
                        demoEnergy = float.Parse(((float)(itemz + itemx)).ToString("F2"));
                        while (demoEnergy.ToString().Length < 4)
                        {
                            itemx = rd.NextDouble();
                            demoEnergy = float.Parse(((float)(itemz + itemx)).ToString("F2"));
                        }
                        if (demoEnergy.ToString().Length < 4)
                        {
                            itemx = rd.NextDouble();
                            demoEnergy = float.Parse(((float)(itemz + itemx)).ToString("F2"));
                        }
                    }

                    if (isStartEnergy)
                    {
                        energys[i] = Convert.ToDecimal(Math.Round(demoEnergy, 2));
                    }
                    else
                    {
                        energys[i] = Convert.ToDecimal(Math.Round(demoEnergy + curPoint.UseMinutes, 2));
                    }
                }
                if (!isStartEnergy) demoEnergy += curPoint.UseMinutes;
            }
            else
            {
                //energys = MeterProtocolAdapter.Instance.ReadEnergy((byte)curPoint.PowerFangXiang, (byte)curPoint.FeiLv);
                byte xiangxian1 = 0;
                byte xiangxian2 = 0;
                if (curPoint.PowerFangXiang == PowerWay.正向无功 || curPoint.PowerFangXiang == PowerWay.反向无功)
                {
                    if (curPoint.PowerFangXiang == PowerWay.正向无功)
                    {
                        xiangxian1 = (byte)PowerWay.第一象限无功;
                        xiangxian2 = (byte)PowerWay.第二象限无功;
                    }
                    else if (curPoint.PowerFangXiang == PowerWay.反向无功)
                    {
                        xiangxian1 = (byte)PowerWay.第三象限无功;
                        xiangxian2 = (byte)PowerWay.第四象限无功;
                    }

                    FloatE[] energys1 = MeterProtocolAdapter.Instance.ReadEnergyR(xiangxian1, (byte)curPoint.FeiLv);
                    FloatE[] energys2 = MeterProtocolAdapter.Instance.ReadEnergyR(xiangxian2, (byte)curPoint.FeiLv);

                    for (int i = 0; i < MeterNumber; i++)
                    {
                        if (!MeterInfo[i].YaoJianYn) continue;
                        energys[i] = energys1[i] + energys2[i];
                    }
                }
                else
                {
                    energys = MeterProtocolAdapter.Instance.ReadEnergyR((byte)curPoint.PowerFangXiang, (byte)curPoint.FeiLv);
                }
            }

            for (int i = 0; i < MeterNumber; i++)
            {
                TestMeterInfo meter = MeterInfo[i];
                if (!meter.YaoJianYn) continue;

                if (energys[i] < 0)
                    energys[i] = MeterProtocolAdapter.Instance.ReadEnergyR((byte)curPoint.PowerFangXiang, (byte)curPoint.FeiLv, i);

                if (energys[i] < 0)
                {
                    MessageAdd($"表位号{ i + 1}没有读到电量值({energys[i]})", EnumLogType.提示信息);
                    continue;
                }

                if (isStartEnergy)
                    meter.MeterZZErrors[curPoint.ItemKey].PowerStart = energys[i];
                else
                    meter.MeterZZErrors[curPoint.ItemKey].PowerEnd = energys[i];

                ResultDictionary["起码"][i] = meter.MeterZZErrors[curPoint.ItemKey].PowerStart?.ToString();
                ResultDictionary["止码"][i] = meter.MeterZZErrors[curPoint.ItemKey].PowerEnd?.ToString();
            }
            RefUIData("起码");
            RefUIData("止码");
            return true;

        }


        #endregion

        /// <summary>
        /// 初始化检定数据
        /// </summary>
        /// <param name="I">走字电流</param>
        /// <returns></returns>
        private void InitZZData(string I)
        {
            for (int i = 0; i < MeterNumber; i++)
            {
                TestMeterInfo meter = MeterInfo[i];
                if (!meter.YaoJianYn) continue;
                meter.MeterZZErrors.RemoveAt(curPoint.ItemKey);

                MeterZZError zzError = new MeterZZError
                {
                    IbXString = curPoint.xIb,
                    PowerWay = Convert.ToInt32(curPoint.PowerFangXiang).ToString(),

                    GLYS = curPoint.Glys,
                    IbX = I,
                    YJ = Convert.ToInt32(curPoint.PowerYj).ToString(),

                    PowerError = "--",
                    Result = ConstHelper.默认结论,
                    STMEnergy = (curPoint.UseMinutes * OneMeterInfo.GetBcs()[0]).ToString(),
                    PrjID = curPoint.PrjID,
                    TimeStart = curPoint.StartTime,
                };

                switch (curPoint.FeiLv)
                {
                    case Cus_FeiLv.尖:
                        zzError.Fl = "尖";
                        break;
                    case Cus_FeiLv.峰:
                        zzError.Fl = "峰";
                        break;
                    case Cus_FeiLv.平:
                        zzError.Fl = "平";
                        break;
                    case Cus_FeiLv.谷:
                        zzError.Fl = "谷";
                        break;
                    case Cus_FeiLv.深谷:
                        zzError.Fl = "深谷";
                        break;
                    default:
                        zzError.Fl = "总";
                        break;
                }


                meter.MeterZZErrors.Add(curPoint.ItemKey, zzError);
            }
            MessageAdd("清理上一次检定数据完毕...", EnumLogType.提示信息);
        }

        /// <summary>
        /// 计算总的结论
        /// </summary>
        protected void ControlResult()
        {
            for (int bw = 0; bw < MeterNumber; bw++)
            {
                TestMeterInfo curMeter = MeterInfo[bw];
                if (!curMeter.YaoJianYn) continue;
                MeterResult r = new MeterResult
                {
                    Result = ConstHelper.合格
                };
                if (curMeter.MeterZZErrors[curPoint.ItemKey].Result == ConstHelper.不合格)
                    r.Result = ConstHelper.不合格;
                else
                {
                    //检测当前方向下的其它点是否合格
                    if (!IsTheSamePowerPDHeGe(curMeter))
                        r.Result = ConstHelper.不合格;
                }
            }
        }

        /// <summary>
        /// 是否相同方向下的所有当前检定项目都合格
        /// </summary>
        /// <param name="curMeter"></param>
        /// <returns></returns>
        private bool IsTheSamePowerPDHeGe(TestMeterInfo curMeter)
        {
            bool isAllItemOk = true;
            foreach (string strKey in curMeter.MeterZZErrors.Keys)
            {
                //当前功率方向
                if (curMeter.MeterZZErrors[strKey].Result == ConstHelper.不合格)
                {
                    //PowerWay thisPointFX = (PowerWay)int.Parse(curMeter.MeterZZErrors[strKey].PrjID.Substring(0, 1));
                    PowerWay thisPointFX = (PowerWay)int.Parse(strKey.Substring(0, 1));
                    if (curPoint.PowerFangXiang == thisPointFX)
                    {
                        isAllItemOk = false;
                        break;
                    }
                }
            }
            return isAllItemOk;
        }


        /// <summary>
        /// 计算 走字结果
        /// </summary>
        /// <param name="plan">当前检定点</param>
        /// <param name="_ZZMethod">走字方式</param>
        /// <param name="arrData"></param>
        /// <param name="strKey"></param>
        private void ControlZZResult(StPlan_ZouZi plan, Cus_ZouZiMethod _ZZMethod, string[] arrData, string strKey)
        {
            bool isAllHeGe = true;
            MessageAdd("正在计算走字结果", EnumLogType.提示信息);

            for (int i = 0; i < MeterNumber; i++)
            {
                TestMeterInfo meter = MeterInfo[i];
                if (!meter.YaoJianYn) continue;

                MeterZZError err = meter.MeterZZErrors[strKey];
                //新申明一个走字误差，用于保存计算返回结果
                float level = MeterLevel(meter); //当前表的等级
                //设置计算参数
                ErrorLimit limit = new ErrorLimit
                {
                    IsSTM = false,
                    UpLimit = level,
                    DownLimit = -level
                };

                MeterZZError tmp;
                if (_ZZMethod == Cus_ZouZiMethod.校核常数)
                {

                    decimal.TryParse(arrData[i], out decimal refkWh);
                    refkWh += _HalfwayMeterPulse[i];
                    if (curPoint.PowerFangXiang.ToString().IndexOf("有功") != -1)
                    {
                        refkWh /= meter.GetBcs()[0];
                    }
                    else
                    {
                        if (meter.GetBcs().Length > 1)
                            refkWh /= meter.GetBcs()[1];
                        else
                            refkWh /= meter.GetBcs()[0];
                    }

                    ZZError m_Context = new ZZError(limit);

                    tmp = (MeterZZError)m_Context.SetWuCha3(err.PowerStart, err.PowerEnd, refkWh);//0.1

                }
                else//标准表法，基本走字法
                {
                    limit.DownLimit = -level * 1.0F;
                    limit.UpLimit = level * 1.0F;


                    ZZError m_Context = new ZZError(limit);

                    decimal.TryParse(arrData[i], out decimal p);
                    p = (p + _HalfwayMeterPulse[i]) / meter.GetBcs()[0]; //表脉冲数/常数=电量

                    if (!bluetoothPulses && ConfigHelper.Instance.IsITOMeter)
                    {
                        tmp = (MeterZZError)m_Context.SetWuCha(plan.FeiLv == Cus_FeiLv.总 ? 1 : 0, err.PowerStart, err.PowerEnd, Convert.ToDecimal(_StarandMeterDl + _StarandMeterDlm));
                    }
                    else
                    {
                        tmp = (MeterZZError)m_Context.SetWuCha(plan.FeiLv == Cus_FeiLv.总 ? 1 : 0, err.PowerStart, err.PowerEnd, Convert.ToDecimal(_StarandMeterDl + _StarandMeterDlm));
                        if (tmp.Result == ConstHelper.不合格)
                        {
                            tmp = (MeterZZError)m_Context.SetWuCha(plan.FeiLv == Cus_FeiLv.总 ? 1 : 0, 0, p, Convert.ToDecimal(_StarandMeterDl + _StarandMeterDlm));
                            if (tmp.Result == ConstHelper.不合格)
                            {
                                tmp = (MeterZZError)m_Context.SetWuCha(plan.FeiLv == Cus_FeiLv.总 ? 1 : 0, err.PowerStart, err.PowerEnd, Convert.ToDecimal(_StarandMeterDl + _StarandMeterDlm));
                            }
                        }
                    }

                    if (IsDemo && tmp.Result == ConstHelper.不合格)
                    {
                        tmp = (MeterZZError)m_Context.SetWuCha(plan.FeiLv == Cus_FeiLv.总 ? 1 : 0, 0, p, Convert.ToDecimal(0.00005f + curPoint.UseMinutes));
                    }
                }

                err.WarkPower = tmp.WarkPower;
                err.PowerError = tmp.PowerError;
                err.Result = tmp.Result;
                if (err.Result == ConstHelper.不合格 && isAllHeGe)
                {
                    isAllHeGe = false;
                    NoResoult[i] = "误差超出误差限";
                }
                ResultDictionary["表码差"][i] = err.PowerEnd - err.PowerStart;//err.WarkPower;
                ResultDictionary["误差"][i] = err.PowerError;
                ResultDictionary["结论"][i] = err.Result;

            }
            for (int i = 0; i < MeterNumber; i++)
            {
                if (!MeterInfo[i].YaoJianYn) continue;
                //起码和止码一样或者表没有脉冲的情况为不合格
                if (ResultDictionary["起码"][i] == ResultDictionary["止码"][i]
                    || (ResultDictionary["表脉冲"][i] == "0" && !(!bluetoothPulses && ConfigHelper.Instance.IsITOMeter)))
                {
                    ResultDictionary["结论"][i] = ConstHelper.不合格;
                }
            }

            RefUIData("表码差");
            RefUIData("误差");
            RefUIData("结论");

        }
    }


    enum TestType
    {
        默认 = 0,
        /// <summary>
        /// 总与分同时做是指当有总费率及其它分费率在一起时。先读取总费率的起码，然后走分费率。最后再读取总费率的止码。
        /// </summary>
        总与分费率同时做 = 1,
        /// <summary>
        /// 此种走字方式是指只走总时读取所有分费率起码，总走完后再读取所有分费率止码。
        /// 此种方式应用于总走字时间较长，同时跨几个分费率的情况
        /// </summary>
        自动检定总时段内的所有分费率 = 2
    }

    /// <summary>
    /// 走字误差计算器--标准表法
    /// </summary>
    class ZZError
    {
        readonly ErrorLimit ErrLimit;
        public ZZError(ErrorLimit wuChaDeal)
        {
            ErrLimit = wuChaDeal;
        }
        /// <summary>
        /// 第一个参数：是否是总费率: 1-是, 0-不是
        /// 第二个参数：起码,止码,标准电量,标准准确度
        /// </summary>
        /// <param name="total"></param>
        /// <param name="sPower"></param>
        /// <param name="ePower"></param>
        /// <param name="refPower"></param>
        /// <returns></returns>
        public MeterBase SetWuCha(int total, FloatE sPower, FloatE ePower, FloatE refPower)
        {
            if (total > 0)
            {
                return SetWuCha1(sPower, ePower, refPower);
            }
            else
                return SetWuCha2(sPower, ePower, refPower);

        }


        /// <summary>
        /// 标准表法/电能表常数试验法误差计算
        /// </summary>
        /// <param name="sPower">被检表起码</param>
        /// <param name="ePower">被检表止码</param>
        /// <param name="refPower">标准表[头表]电量</param>
        /// <returns></returns>
        private MeterBase SetWuCha1(FloatE sPower, FloatE ePower, FloatE refPower)
        {
            MeterZZError rst = new MeterZZError();

            //修正标准表电量

            decimal stmPower = Convert.ToDecimal(Math.Round(refPower.Value, 6));

            rst.PowerStart = sPower;
            rst.PowerEnd = ePower;
            rst.WarkPower = ePower - sPower;

            decimal err;
            if (stmPower == 0) err = -100;
            else err = (decimal.Parse(rst.WarkPower) - stmPower) / stmPower * 100;

            rst.PowerError = err.ToString("F4");
            decimal tmperr = decimal.Parse(rst.PowerError);
            if (tmperr != 0 && err > 0) //误差大于零且不等于0时，误差加+符号
                rst.PowerError = "+" + rst.PowerError;
            //计算方法参见JJG56-1999 4.4.2
            if (tmperr <= Convert.ToDecimal(ErrLimit.UpLimit) && tmperr >= Convert.ToDecimal(ErrLimit.DownLimit))
                rst.Result = ConstHelper.合格;
            else
                rst.Result = ConstHelper.不合格;

            return rst;
        }

        /// <summary>
        /// 计算分费率走字误差
        /// </summary>
        /// <param name="sPower">被检表起码</param>
        /// <param name="ePower">被检表止码</param>
        /// <param name="stmPower"></param>
        /// <returns></returns>
        private MeterZZError SetWuCha2(FloatE sPower, FloatE ePower, FloatE stmPower)
        {
            MeterZZError rst = new MeterZZError
            {
                PowerStart = sPower,
                PowerEnd = ePower,
                WarkPower = ePower - sPower
            };

            // |费率码差-总码差| * 10 ^ MeterPrecision <=2 参见JJG596-1999 4.4.4
            //float err = (float)((float.Parse(rst.WarkPower) - stmPower) * Math.Pow(10, MeterPrecision));

            decimal err = (ePower - sPower - stmPower) / stmPower * 100;

            rst.PowerError = err.ToString("F4");
            if (Math.Abs(err) <= Convert.ToDecimal(ErrLimit.UpLimit))
            {
                rst.Result = ConstHelper.合格;
            }
            else
            {
                rst.Result = ConstHelper.不合格;
            }

            return rst;
        }


        /// <summary>
        /// 校核常数
        /// </summary>
        /// <param name="sPower">被检表起码</param>
        /// <param name="ePower">被检表止码</param>
        /// <param name="pulsePower">标准表[头表]电量</param>
        /// <returns></returns>
        public MeterBase SetWuCha3(FloatE sPower, FloatE ePower, FloatE pulsePower)
        {
            MeterZZError rst = new MeterZZError();

            FloatE diffPower = ePower - sPower;

            rst.PowerStart = sPower;
            rst.PowerEnd = ePower;
            rst.WarkPower = diffPower;

            decimal err;
            if (pulsePower == 0f)
                err = -100;
            else
                err = (pulsePower - diffPower) / diffPower * 100;

            rst.PowerError = err.ToString("F4");
            //decimal tmperr = decimal.Parse(rst.PowerError);
            if (err != 0 && err > 0) //误差大于零且不等于0时，误差加+符号
                rst.PowerError = "+" + rst.PowerError;
            //计算方法参见JJG56-1999 4.4.2
            if (err <= Convert.ToDecimal(ErrLimit.UpLimit) && err >= Convert.ToDecimal(ErrLimit.DownLimit))
                rst.Result = ConstHelper.合格;
            else
                rst.Result = ConstHelper.不合格;

            return rst;
        }

    }
}
