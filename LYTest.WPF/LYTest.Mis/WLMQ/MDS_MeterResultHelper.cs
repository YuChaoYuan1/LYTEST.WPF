using ICInterface.Base_ICStructure;
using ICInterface.Meter_ICStructure;
using ICInterface.Meter_ICStructure.ItemResultStructure;
using LYTest.Core.Enum;
using LYTest.Core.Model.Meter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LYTest.Mis.WLMQ
{

    public class MDS_MeterResultHelper
    {
        public class ResultConfig
        {
            /// <summary>
            /// 系统编号
            /// </summary>
            public string SysNo;
            /// <summary>
            /// 地区代码
            /// </summary>
            public string AreaCode;
            /// <summary>
            /// 设备类别
            /// </summary>
            public string DevCls;
            /// <summary>
            /// 设备标识
            /// </summary>
            public string DevId;
            /// <summary>
            ///  供电所编号
            /// </summary>
            public string PsOrgNo;
            /// <summary>
            /// 电能表综合结论标识
            /// </summary>

            public string VeriMeterRsltId;


        }
        /// <summary>
        /// 配置项
        /// </summary>
        private ResultConfig RConfig;

        /// <summary>
        /// 标准代码
        /// </summary>
        public StdDicCode StdDicCode = null;
        public MDS_MeterResultHelper(ResultConfig resultConfig)
        {
            RConfig = resultConfig;
        }
        /// <summary>
        ///  初始化标准代码
        /// </summary>
        /// <param name="stdDicCode"></param>
        /// <param name="IsForceInit">是否强制初始化</param>
        public void InitStddic(StdDicCode stdDicCode, bool IsForceInit)
        {
            if (StdDicCode == null || IsForceInit)
            {
                StdDicCode = stdDicCode;
            }
        }


        #region 试验项目  //影响量试验 功耗试验 需量清零 标准偏差  费率时段电能市值误差  远程控制
        public void 外观检查(DETedTestData dETedTestData, TestMeterInfo meter, Dictionary<string, Dictionary<string, Dictionary<string, string>>> Results, Dictionary<string, VeriDisqualReasonList> 不合格列表)
        {
            //01  铭牌
            //02  液晶屏
            //03  指示灯
            int index = 1;
            if (Results.ContainsKey(ProjectID.外观检查))
            {
                foreach (var item in Results[ProjectID.外观检查].Values)
                {

                    if (string.IsNullOrWhiteSpace(item["结论"])) continue;
                    string DETECT_CONTENT = "02";
                    switch (item["识别区域"])
                    {
                        case "液晶显示屏":
                            DETECT_CONTENT = "02";
                            break;
                        case "铭牌":
                            DETECT_CONTENT = "01";
                            break;
                        case "指示灯":
                            DETECT_CONTENT = "03";
                            break;
                        default:
                            break;
                    }
                    VIntuitMeterConc meterConc = new VIntuitMeterConc()
                    {
                        areaCode = RConfig.AreaCode,
                        assetNo = meter.MD_BarCode,
                        barCode = meter.MD_BarCode,
                        checkConc = GetResultCode(item["结论"]),
                        chkCont = DETECT_CONTENT,
                        devCls = RConfig.DevCls,
                        devId = RConfig.DevId,
                        devSeatNo = item["表位号"],
                        machNo = item["检测系统编号"],
                        plantElementNo = item["检测系统编号"],
                        plantNo = item["检测系统编号"],
                        psOrgNo = RConfig.PsOrgNo,
                        sn = "1",
                        sysNo = RConfig.SysNo,
                        validFlag = GetCodeValue("validFlag", "有效"),
                        veriCaliDate = GetTime(meter.VerifyDate),
                        veriPointSn = index.ToString(),
                        veriTaskNo = meter.MD_TaskNo,
                        veriMeterRsltId = RConfig.VeriMeterRsltId,
                    };
                    if (dETedTestData.vIntuitMeterConc == null) dETedTestData.vIntuitMeterConc = new List<VIntuitMeterConc>();
                    dETedTestData.vIntuitMeterConc.Add(meterConc);

                    index++;
                    if (item["结论"] != "合格" && !不合格列表.ContainsKey(ProjectID.外观检查))
                    {
                        不合格列表.Add(ProjectID.外观检查, new VeriDisqualReasonList()
                        {
                            barCode = meter.MD_BarCode,
                            disqualReason = GetErrCode(ProjectID.外观检查),
                            veriTaskNo = meter.MD_TaskNo,
                            sysNo = RConfig.SysNo,
                            veriUnitNo = item["检测系统编号"],
                        });
                    }
                }
            }
        }

        public void 交流耐压试验(DETedTestData dETedTestData, TestMeterInfo meter, Dictionary<string, Dictionary<string, Dictionary<string, string>>> Results, Dictionary<string, VeriDisqualReasonList> 不合格列表)
        {
            int index = 1;
            string ItemNo = ProjectID.工频耐压试验;
            if (Results.ContainsKey(ItemNo))
            {
                foreach (var item in Results[ItemNo].Values)
                {
                    if (string.IsNullOrWhiteSpace(item["结论"])) continue;
                    VVoltMeterConc meterConc = new VVoltMeterConc()
                    {
                        areaCode = RConfig.AreaCode,
                        assetNo = meter.MD_BarCode,
                        barCode = meter.MD_BarCode,
                        checkConc = GetResultCode(item["结论"]),
                        devCls = RConfig.DevCls,
                        devId = RConfig.DevId,
                        devSeatNo = item["表位号"],
                        machNo = item["检测系统编号"],
                        plantElementNo = item["检测系统编号"],
                        plantNo = item["检测系统编号"],
                        psOrgNo = RConfig.PsOrgNo,
                        sn = "1",
                        sysNo = RConfig.SysNo,
                        validFlag = GetCodeValue("validFlag", "有效"),
                        veriCaliDate = GetTime(meter.VerifyDate),
                        veriPointSn = index.ToString(),
                        veriTaskNo = meter.MD_TaskNo,
                        leakCurrentLimit = item["耐压仪漏电流"],
                        positionLeakLimit = item["标准值"],
                        testDate = GetTime(meter.VerifyDate),
                        testTime = item["耐压时间"],
                        testVoltValue = item["耐压电压"],
                        veriMeterRsltId = RConfig.VeriMeterRsltId,
                        voltObj = "01"

                    };
                    if (dETedTestData.vVoltMeterConc == null) dETedTestData.vVoltMeterConc = new List<VVoltMeterConc>();
                    dETedTestData.vVoltMeterConc.Add(meterConc);
                    index++;
                    if (item["结论"] != "合格" && !不合格列表.ContainsKey(ItemNo))
                    {
                        不合格列表.Add(ItemNo, new VeriDisqualReasonList()
                        {
                            barCode = meter.MD_BarCode,
                            disqualReason = GetErrCode(ItemNo),
                            veriTaskNo = meter.MD_TaskNo,
                            sysNo = RConfig.SysNo,
                            veriUnitNo = item["检测系统编号"],
                        });
                    }
                }
            }
        }

        public void 基本误差(DETedTestData dETedTestData, TestMeterInfo meter, Dictionary<string, Dictionary<string, Dictionary<string, string>>> Results, Dictionary<string, VeriDisqualReasonList> 不合格列表)
        {
            int index = 1;
            string ItemNo = ProjectID.基本误差试验;
            if (Results.ContainsKey(ItemNo))
            {
                foreach (var item in Results[ItemNo].Values)
                {
                    if (string.IsNullOrWhiteSpace(item["结论"])) continue;
                    VBasicerrMeterConc meterConc = new VBasicerrMeterConc()
                    {
                        areaCode = RConfig.AreaCode,
                        assetNo = meter.MD_BarCode,
                        barCode = meter.MD_BarCode,
                        checkConc = GetResultCode(item["结论"]),
                        devCls = RConfig.DevCls,
                        devId = RConfig.DevId,
                        devSeatNo = item["表位号"],
                        machNo = item["检测系统编号"],
                        plantElementNo = item["检测系统编号"],
                        plantNo = item["检测系统编号"],
                        psOrgNo = RConfig.PsOrgNo,
                        sn = "1",
                        sysNo = RConfig.SysNo,
                        validFlag = GetCodeValue("validFlag", "有效"),
                        veriCaliDate = GetTime(meter.VerifyDate),
                        veriPointSn = index.ToString(),
                        veriTaskNo = meter.MD_TaskNo,
                        veriMeterRsltId = RConfig.VeriMeterRsltId,


                        actlError = item["误差1"] + "|" + item["误差2"],
                        aveErr = item["平均值"],
                        errDown = item["误差下限"],
                        errUp = item["误差上限"],
                        trialFreq = meter.MD_Frequency.ToString(),
                        simpling = "2",
                        bothWayPowerFlag = GetCodeValue("powerFlag", item["功率方向"]),
                        pf = GetCodeValue("meterTestPowerFactor", item["功率因数"]),
                        curPhaseCode = 功率元件转换(item["功率元件"]),
                        detectCircle = item["误差圈数"],
                        intConvertErr = item["化整值"],
                        loadCurrent = GetCodeValue("meterTestCurLoad", item["电流倍数"]),
                        loadVoltage = GetCodeValue("meterTestVolt", "100%Un"),
                    };
                    if (dETedTestData.vBasicerrMeterConc == null) dETedTestData.vBasicerrMeterConc = new List<VBasicerrMeterConc>();
                    dETedTestData.vBasicerrMeterConc.Add(meterConc);
                    index++;
                    if (item["结论"] != "合格" && !不合格列表.ContainsKey(ItemNo))
                    {
                        不合格列表.Add(ItemNo, new VeriDisqualReasonList()
                        {
                            barCode = meter.MD_BarCode,
                            disqualReason = GetErrCode(ItemNo),
                            veriTaskNo = meter.MD_TaskNo,
                            sysNo = RConfig.SysNo,
                            veriUnitNo = item["检测系统编号"],
                        });
                    }
                }
            }
        }

        public void 电能表常数试验(DETedTestData dETedTestData, TestMeterInfo meter, Dictionary<string, Dictionary<string, Dictionary<string, string>>> Results, Dictionary<string, VeriDisqualReasonList> 不合格列表)
        {
            int index = 1;
            string ItemNo = ProjectID.电能表常数试验;
            if (Results.ContainsKey(ItemNo))
            {
                var levs = GetMeterGrade(meter.MD_Grane);
                foreach (var item in Results[ItemNo].Values)
                {
                    if (string.IsNullOrWhiteSpace(item["结论"])) continue;
                    VConstMeterConc meterConc = new VConstMeterConc()
                    {
                        areaCode = RConfig.AreaCode,
                        assetNo = meter.MD_BarCode,
                        barCode = meter.MD_BarCode,
                        checkConc = GetResultCode(item["结论"]),
                        //chkCont = DETECT_CONTENT,
                        devCls = RConfig.DevCls,
                        devId = RConfig.DevId,
                        devSeatNo = item["表位号"],
                        machNo = item["检测系统编号"],
                        plantElementNo = item["检测系统编号"],
                        plantNo = item["检测系统编号"],
                        psOrgNo = RConfig.PsOrgNo,
                        sn = "1",
                        sysNo = RConfig.SysNo,
                        validFlag = GetCodeValue("validFlag", "有效"),
                        veriCaliDate = GetTime(meter.VerifyDate),
                        veriPointSn = index.ToString(),
                        veriTaskNo = meter.MD_TaskNo,
                        veriMeterRsltId = RConfig.VeriMeterRsltId,

                        actlError = item["误差"],
                        bothWayPowerFlag = GetCodeValue("powerFlag", item["功率方向"]),
                        constConcCode = "",
                        constErr = item["误差"],
                        controlMethod = GetCodeValue("meterTestCtrlMode", item["走字试验方法类型"]),
                        diffReading = item["表码差"],
                        divideElectricQuantity = item["走字电量(度)"],
                        endReading = item["止码"],
                        errDown = (Convert.ToDouble(levs[0]) * -1.0).ToString("0.0"),
                        errUp = (Convert.ToDouble(levs[0]) * 1.0).ToString("0.0"),
                        feeStartTime = "",
                        irLastReading = "0",
                        loadCurrent = GetCodeValue("meterTestCurLoad", item["电流倍数"]),
                        pf = GetCodeValue("meterTestPowerFactor", item["功率因数"]),
                        qualifiedPules = "",
                        rate = item["费率"],
                        readType = GetREAD_TYPE_CODE(item["功率方向"], item["费率"]),
                        realPules = item["表脉冲"],
                        rv = GetCodeValue("meterTestVolt", "100%Un"),
                        standardReading = string.IsNullOrWhiteSpace(item["标准表脉冲"]) ? "0" : (Convert.ToSingle(item["标准表脉冲"]) / GetBcs(meter.MD_Constant)[0]).ToString("F4"),
                        startReading = item["起码"],
                        valueConcCode = item["误差"],
                    };
                    if (!string.IsNullOrWhiteSpace(meterConc.valueConcCode) && meterConc.valueConcCode.Length > 8)
                    {
                        meterConc.valueConcCode = meterConc.valueConcCode.Substring(0, 8);
                    }
                    if (dETedTestData.vConstMeterConc == null) dETedTestData.vConstMeterConc = new List<VConstMeterConc>();
                    dETedTestData.vConstMeterConc.Add(meterConc);
                    index++;
                    if (item["结论"] != "合格" && !不合格列表.ContainsKey(ItemNo))
                    {
                        不合格列表.Add(ItemNo, new VeriDisqualReasonList()
                        {
                            barCode = meter.MD_BarCode,
                            disqualReason = GetErrCode(ItemNo),
                            veriTaskNo = meter.MD_TaskNo,
                            sysNo = RConfig.SysNo,
                            veriUnitNo = item["检测系统编号"],
                        });
                    }
                }
            }
        }

        public void 起动试验(DETedTestData dETedTestData, TestMeterInfo meter, Dictionary<string, Dictionary<string, Dictionary<string, string>>> Results, Dictionary<string, VeriDisqualReasonList> 不合格列表)
        {
            int index = 1;
            string ItemNo = ProjectID.起动试验;
            if (Results.ContainsKey(ItemNo))
            {
                foreach (var item in Results[ItemNo].Values)
                {
                    if (string.IsNullOrWhiteSpace(item["结论"])) continue;
                    VStartingMeterConc meterConc = new VStartingMeterConc()
                    {
                        areaCode = RConfig.AreaCode,
                        assetNo = meter.MD_BarCode,
                        barCode = meter.MD_BarCode,
                        checkConc = GetResultCode(item["结论"]),
                        //chkCont = DETECT_CONTENT,
                        devCls = RConfig.DevCls,
                        devId = RConfig.DevId,
                        devSeatNo = item["表位号"],
                        machNo = item["检测系统编号"],
                        plantElementNo = item["检测系统编号"],
                        plantNo = item["检测系统编号"],
                        psOrgNo = RConfig.PsOrgNo,
                        sn = "1",
                        sysNo = RConfig.SysNo,
                        validFlag = GetCodeValue("validFlag", "有效"),
                        veriCaliDate = GetTime(meter.VerifyDate),
                        veriPointSn = index.ToString(),
                        veriTaskNo = meter.MD_TaskNo,
                        veriMeterRsltId = RConfig.VeriMeterRsltId,

                        current = "0",
                        esamNo = "",
                        testTime = "",
                        theorTime = "",
                    };
                    if (float.TryParse(item["试验电流"], out float curtmp))
                    {
                        meterConc.current = curtmp.ToString("F4");
                    }
                    if (!string.IsNullOrWhiteSpace(item["标准试验时间"]))
                    {
                        float.TryParse(item["标准试验时间"], out float timetmp);
                        meterConc.theorTime = (timetmp * 60).ToString("F0");
                    }
                    if (!string.IsNullOrWhiteSpace(item["实际运行时间"]))
                    {
                        float.TryParse(item["实际运行时间"].Replace("分", ""), out float timetmp);
                        meterConc.testTime = (timetmp * 60).ToString("F0");
                    }
                    if (dETedTestData.vStartingMeterConc == null) dETedTestData.vStartingMeterConc = new List<VStartingMeterConc>();
                    dETedTestData.vStartingMeterConc.Add(meterConc);
                    index++;
                    if (item["结论"] != "合格" && !不合格列表.ContainsKey(ItemNo))
                    {
                        不合格列表.Add(ItemNo, new VeriDisqualReasonList()
                        {
                            barCode = meter.MD_BarCode,
                            disqualReason = GetErrCode(ItemNo),
                            veriTaskNo = meter.MD_TaskNo,
                            sysNo = RConfig.SysNo,
                            veriUnitNo = item["检测系统编号"],
                        });
                    }
                }
            }
        }

        public void 潜动试验(DETedTestData dETedTestData, TestMeterInfo meter, Dictionary<string, Dictionary<string, Dictionary<string, string>>> Results, Dictionary<string, VeriDisqualReasonList> 不合格列表)
        {
            int index = 1;
            string ItemNo = ProjectID.潜动试验;
            if (Results.ContainsKey(ItemNo))
            {
                foreach (var item in Results[ItemNo].Values)
                {
                    if (string.IsNullOrWhiteSpace(item["结论"])) continue;
                    VCreepingMeterConc meterConc = new VCreepingMeterConc()
                    {
                        areaCode = RConfig.AreaCode,
                        assetNo = meter.MD_BarCode,
                        barCode = meter.MD_BarCode,
                        checkConc = GetResultCode(item["结论"]),
                        devCls = RConfig.DevCls,
                        devId = RConfig.DevId,
                        devSeatNo = item["表位号"],
                        machNo = item["检测系统编号"],
                        plantElementNo = item["检测系统编号"],
                        plantNo = item["检测系统编号"],
                        psOrgNo = RConfig.PsOrgNo,
                        sn = "1",
                        sysNo = RConfig.SysNo,
                        validFlag = GetCodeValue("validFlag", "有效"),
                        veriCaliDate = GetTime(meter.VerifyDate),
                        veriPointSn = index.ToString(),
                        veriTaskNo = meter.MD_TaskNo,
                        veriMeterRsltId = RConfig.VeriMeterRsltId,

                        bothWayPowerFlag = GetCodeValue("powerFlag", item["功率方向"]),
                        constConcCode = "",
                        loadCurrent = GetCodeValue("meterTestCurLoad", "0"),
                        loadVoltage = GetCodeValue("meterTestVolt", $"{item["潜动电压"].TrimEnd('%')}%Un"),
                        pules = "0",
                        realTestTime = "0",
                        testCircleNum = "1",
                        testTime = "0",
                        volt = GetCodeValue("meterTestVolt", $"{item["潜动电压"].TrimEnd('%')}%Un"),
                    };
                    if (!string.IsNullOrWhiteSpace(item["标准试验时间"]))
                    {
                        float.TryParse(item["标准试验时间"], out float timetmp);
                        meterConc.testTime = (timetmp * 60).ToString("F0");
                    }
                    if (!string.IsNullOrWhiteSpace(item["实际运行时间"]))
                    {
                        float.TryParse(item["实际运行时间"].Replace("分", ""), out float timetmp);
                        meterConc.realTestTime = (timetmp * 60).ToString("F0");
                    }
                    if (dETedTestData.vCreepingMeterConc == null) dETedTestData.vCreepingMeterConc = new List<VCreepingMeterConc>();
                    dETedTestData.vCreepingMeterConc.Add(meterConc);
                    index++;
                    if (item["结论"] != "合格" && !不合格列表.ContainsKey(ItemNo))
                    {
                        不合格列表.Add(ItemNo, new VeriDisqualReasonList()
                        {
                            barCode = meter.MD_BarCode,
                            disqualReason = GetErrCode(ItemNo),
                            veriTaskNo = meter.MD_TaskNo,
                            sysNo = RConfig.SysNo,
                            veriUnitNo = item["检测系统编号"],
                        });
                    }
                }
            }
        }

        public void 计度器总电能示值组合误差(DETedTestData dETedTestData, TestMeterInfo meter, Dictionary<string, Dictionary<string, Dictionary<string, string>>> Results, Dictionary<string, VeriDisqualReasonList> 不合格列表)
        {
            int index = 1;
            string ItemNo = ProjectID.电能示值组合误差;
            if (Results.ContainsKey(ItemNo))
            {
                foreach (var item in Results[ItemNo].Values)
                {
                    if (string.IsNullOrWhiteSpace(item["结论"])) continue;
                    int count = 0;
                    if (!string.IsNullOrWhiteSpace(item["试验前费率电量"]))
                    {
                        count = item["试验前费率电量"].Split(',').Length;
                    }
                    VHutCombinaMeterConc meterConc = new VHutCombinaMeterConc()
                    {
                        areaCode = RConfig.AreaCode,
                        assetNo = meter.MD_BarCode,
                        barCode = meter.MD_BarCode,
                        checkConc = GetResultCode(item["结论"]),
                        //chkCont = DETECT_CONTENT,
                        devCls = RConfig.DevCls,
                        devId = RConfig.DevId,
                        devSeatNo = item["表位号"],
                        machNo = item["检测系统编号"],
                        plantElementNo = item["检测系统编号"],
                        plantNo = item["检测系统编号"],
                        psOrgNo = RConfig.PsOrgNo,
                        sn = "1",
                        sysNo = RConfig.SysNo,
                        validFlag = GetCodeValue("validFlag", "有效"),
                        veriCaliDate = GetTime(meter.VerifyDate),
                        veriPointSn = index.ToString(),
                        veriTaskNo = meter.MD_TaskNo,
                        veriMeterRsltId = RConfig.VeriMeterRsltId,


                        bothWayPowerFlag = GetCodeValue("powerFlag", "正向有功"),
                        loadCurrent = GetCodeValue("meterTestCurLoad", item["走字电流"]),
                        pf = GetCodeValue("meterTestPowerFactor", "1.0"),
                        feeRatio = "总",
                        controlWay = GetCodeValue("meterTestCtrlMode", "标准表法"),
                        irTime = count.ToString(),
                        irReading = item["总电量差值"],
                        errUp = (0.01 * (count - 1)).ToString("f2"),
                        errDown = (-0.01 * (count - 1)).ToString("f2"),
                        voltage = GetCodeValue("meterTestVolt", "100%Un"),
                        totalReadingErr = item["组合误差"],
                        totalIncrement = item["总电量差值"],
                        valueConcCode = item["组合误差"],


                        //eleIncrement = "",
                        //endValue = "",
                        //feeValue = "",
                        //flatIncrement = "",
                        otherIncrementOne = "",
                        otherIncrementTwo = "",
                        //peakIncrement = "",
                        //sharpIncrement = "",
                        //startValue = "",
                        //sumerAllIncrement = "",
                        //valleyIncrement = "",
                    };

                    bool isNullData = string.IsNullOrWhiteSpace(item["费率电量差值"]) || string.IsNullOrWhiteSpace(item["试验前费率电量"]) || string.IsNullOrWhiteSpace(item["试验后费率电量"])
                   || string.IsNullOrWhiteSpace(item["试验后费率电量"]);

                    费率电量结构 试验前电量 = new 费率电量结构();
                    费率电量结构 试验后电量 = new 费率电量结构();
                    费率电量结构 费率差值 = new 费率电量结构();
                    if (!isNullData)
                    {
                        float[] tmp_费率差值 = item["费率电量差值"].Split(',').Select(x => float.Parse(x)).ToArray();
                        float[] tmp_试验前电量 = item["试验前费率电量"].Split(',').Select(x => float.Parse(x)).ToArray();
                        float[] tmp_试验后电量 = item["试验后费率电量"].Split(',').Select(x => float.Parse(x)).ToArray();
                        string[] names = item["费率数段(英文逗号,间隔)"].Split(',');
                        for (int i = 0; i < names.Length; i++)
                        {
                            int Index = -1;
                            if (names[i].IndexOf("尖") != -1)
                                Index = 0;
                            else if (names[i].IndexOf("峰") != -1)
                                Index = 1;
                            else if (names[i].IndexOf("平") != -1)
                                Index = 2;
                            else if (names[i].IndexOf("谷") != -1 && names[i].IndexOf("深") == -1)
                                Index = 3;
                            else if (names[i].IndexOf("深谷") != -1)
                                Index = 4;
                            if (Index >= 0)
                            {
                                if (item["结论"] == "合格")
                                {
                                    试验前电量.电量[Index] = tmp_试验前电量[Index];
                                    试验后电量.电量[Index] = tmp_试验后电量[Index];
                                    费率差值.电量[Index] = tmp_费率差值[Index];
                                }
                                else
                                {
                                    试验前电量.电量[Index] = (tmp_试验前电量.Length - 1) >= Index ? tmp_试验前电量[Index] : 0;
                                    试验后电量.电量[Index] = (tmp_试验后电量.Length - 1) >= Index ? tmp_试验后电量[Index] : 0;
                                    费率差值.电量[Index] = (tmp_费率差值.Length - 1) >= Index ? tmp_费率差值[Index] : 0;
                                }
                            }
                        }

                        meterConc.sharpIncrement = 费率差值.电量[0].ToString("F4");
                        meterConc.peakIncrement = 费率差值.电量[1].ToString("F4");
                        meterConc.flatIncrement = 费率差值.电量[2].ToString("F4");
                        meterConc.valleyIncrement = 费率差值.电量[3].ToString("F4");
                        meterConc.sumerAllIncrement = 费率差值.总.ToString("F4");
                    }
                    if (count > 4)//有深谷的
                    {
                        meterConc.feeValue = "尖|峰|平|谷|深谷|总";
                    }
                    else
                    {
                        meterConc.feeValue = "尖|峰|平|谷|总";
                    }
                    meterConc.startValue = $"{string.Join("|", 试验前电量.电量)}|{item["试验前总电量"]}";
                    meterConc.endValue = $"{string.Join("|", 试验后电量.电量)}|{item["试验后总电量"]}";
                    meterConc.eleIncrement = $"{string.Join("|", 费率差值.电量)}|{item["总电量差值"]}";

                    if (dETedTestData.vHutCombinaMeterConc == null) dETedTestData.vHutCombinaMeterConc = new List<VHutCombinaMeterConc>();

                    dETedTestData.vHutCombinaMeterConc.Add(meterConc);
                    index++;
                    if (item["结论"] != "合格" && !不合格列表.ContainsKey(ItemNo))
                    {
                        不合格列表.Add(ItemNo, new VeriDisqualReasonList()
                        {
                            barCode = meter.MD_BarCode,
                            disqualReason = GetErrCode(ItemNo),
                            veriTaskNo = meter.MD_TaskNo,
                            sysNo = RConfig.SysNo,
                            veriUnitNo = item["检测系统编号"],
                        });
                    }
                }
            }
        }

        public void 日计时误差(DETedTestData dETedTestData, TestMeterInfo meter, Dictionary<string, Dictionary<string, Dictionary<string, string>>> Results, Dictionary<string, VeriDisqualReasonList> 不合格列表)
        {
            int index = 1;
            string ItemNo = ProjectID.日计时误差;
            if (Results.ContainsKey(ItemNo))
            {
                foreach (var item in Results[ItemNo].Values)
                {
                    if (string.IsNullOrWhiteSpace(item["结论"])) continue;
                    VDayerrMeterConc meterConc = new VDayerrMeterConc()
                    {
                        areaCode = RConfig.AreaCode,
                        assetNo = meter.MD_BarCode,
                        barCode = meter.MD_BarCode,
                        checkConc = GetResultCode(item["结论"]),
                        //chkCont = DETECT_CONTENT,
                        devCls = RConfig.DevCls,
                        devId = RConfig.DevId,
                        devSeatNo = item["表位号"],
                        machNo = item["检测系统编号"],
                        plantElementNo = item["检测系统编号"],
                        plantNo = item["检测系统编号"],
                        psOrgNo = RConfig.PsOrgNo,
                        sn = "1",
                        sysNo = RConfig.SysNo,
                        validFlag = GetCodeValue("validFlag", "有效"),
                        veriCaliDate = GetTime(meter.VerifyDate),
                        veriPointSn = index.ToString(),
                        veriTaskNo = meter.MD_TaskNo,
                        veriMeterRsltId = RConfig.VeriMeterRsltId,

                        actlError = $"{item["误差1"]}|{item["误差2"]}|{item["误差3"]}|{item["误差4"]}|{item["误差5"]}",
                        errAbs = item["误差限(s/d)"]?.Replace("±", ""),
                        intConvertErr = item["化整值"],
                        secPiles = "1",
                        simpling = "5",
                        testAvgErr = item["平均值"],
                        testTime = "60",
                    };
                    if (double.TryParse(item["平均值"], out double err))
                    {
                        meterConc.testAvgErr = err.ToString("F5");
                    }
                    if (dETedTestData.vDayerrMeterConc == null) dETedTestData.vDayerrMeterConc = new List<VDayerrMeterConc>();

                    dETedTestData.vDayerrMeterConc.Add(meterConc);
                    index++;
                    if (item["结论"] != "合格" && !不合格列表.ContainsKey(ItemNo))
                    {
                        不合格列表.Add(ItemNo, new VeriDisqualReasonList()
                        {
                            barCode = meter.MD_BarCode,
                            disqualReason = GetErrCode(ItemNo),
                            veriTaskNo = meter.MD_TaskNo,
                            sysNo = RConfig.SysNo,
                            veriUnitNo = item["检测系统编号"],
                        });
                    }
                }
            }
        }

        public void 规约一致性检查(DETedTestData dETedTestData, TestMeterInfo meter, Dictionary<string, Dictionary<string, Dictionary<string, string>>> Results, Dictionary<string, VeriDisqualReasonList> 不合格列表)
        {
            int index = 1;
            string ItemNo = ProjectID.通讯协议检查试验;
            if (Results.ContainsKey(ItemNo))
            {
                foreach (var item in Results[ItemNo].Values)
                {
                    if (string.IsNullOrWhiteSpace(item["结论"])) continue;
                    VStandardMeterConc meterConc = new VStandardMeterConc()
                    {
                        areaCode = RConfig.AreaCode,
                        assetNo = meter.MD_BarCode,
                        barCode = meter.MD_BarCode,
                        checkConc = GetResultCode(item["结论"]),
                        devCls = RConfig.DevCls,
                        devId = RConfig.DevId,
                        devSeatNo = item["表位号"],
                        machNo = item["检测系统编号"],
                        plantElementNo = item["检测系统编号"],
                        plantNo = item["检测系统编号"],
                        psOrgNo = RConfig.PsOrgNo,
                        sn = "1",
                        sysNo = RConfig.SysNo,
                        validFlag = GetCodeValue("validFlag", "有效"),
                        veriCaliDate = GetTime(meter.VerifyDate),
                        veriPointSn = index.ToString(),
                        veriTaskNo = meter.MD_TaskNo,
                        veriMeterRsltId = RConfig.VeriMeterRsltId,

                        chkBasis = "",// item["标识编码"],
                        dataFlag = item["标识编码"],
                        readValue = item["检定信息"],
                        settingValue = item["标准数据"],
                    };
                    if (item["检定信息"].Length > 250)
                    {
                        meterConc.readValue = item["检定信息"].Substring(0, 250);
                    }
                    if (dETedTestData.vStandardMeterConc == null) dETedTestData.vStandardMeterConc = new List<VStandardMeterConc>();
                    dETedTestData.vStandardMeterConc.Add(meterConc);
                    index++;
                    if (item["结论"] != "合格" && !不合格列表.ContainsKey(ItemNo))
                    {
                        不合格列表.Add(ItemNo, new VeriDisqualReasonList()
                        {
                            barCode = meter.MD_BarCode,
                            disqualReason = GetErrCode(ItemNo),
                            veriTaskNo = meter.MD_TaskNo,
                            sysNo = RConfig.SysNo,
                            veriUnitNo = item["检测系统编号"],
                        });
                    }
                }
            }
        }

        public void 误差变差试验(DETedTestData dETedTestData, TestMeterInfo meter, Dictionary<string, Dictionary<string, Dictionary<string, string>>> Results, Dictionary<string, VeriDisqualReasonList> 不合格列表)
        {
            int index = 1;
            string ItemNo = ProjectID.误差变差;
            if (Results.ContainsKey(ItemNo))
            {
                foreach (var item in Results[ItemNo].Values)
                {
                    if (string.IsNullOrWhiteSpace(item["结论"])) continue;
                    VErrorMeterConc meterConc = new VErrorMeterConc()
                    {
                        areaCode = RConfig.AreaCode,
                        assetNo = meter.MD_BarCode,
                        barCode = meter.MD_BarCode,
                        checkConc = GetResultCode(item["结论"]),
                        devCls = RConfig.DevCls,
                        devId = RConfig.DevId,
                        devSeatNo = item["表位号"],
                        machNo = item["检测系统编号"],
                        plantElementNo = item["检测系统编号"],
                        plantNo = item["检测系统编号"],
                        psOrgNo = RConfig.PsOrgNo,
                        sn = "1",
                        sysNo = RConfig.SysNo,
                        validFlag = GetCodeValue("validFlag", "有效"),
                        veriCaliDate = GetTime(meter.VerifyDate),
                        veriPointSn = index.ToString(),
                        veriTaskNo = meter.MD_TaskNo,
                        veriMeterRsltId = RConfig.VeriMeterRsltId,

                        actlError = item["变差值"],
                        avgOnceErr = item["第一次平均值"],
                        avgTwiceErr = item["第二次平均值"],
                        bothWayPowerFlag = GetCodeValue("powerFlag", "正向有功"),
                        errDown = item["误差下限"],
                        errUp = item["误差上限"],
                        intOnceErr = item["第一次化整值"],
                        intTwiceErr = item["第二次化整值"],
                        loadCurrent = GetCodeValue("meterTestCurLoad", "1.0Ib"),
                        onceErr = item["第一次误差1"] + "|" + item["第一次误差2"],
                        pf = GetCodeValue("meterTestPowerFactor", item["功率因数"]),
                        pules = item["检定圈数"],
                        simpling = "2",
                        twiceErr = item["第二次误差1"] + "|" + item["第二次误差2"],
                    };
                    if (dETedTestData.vErrorMeterConc == null) dETedTestData.vErrorMeterConc = new List<VErrorMeterConc>();
                    dETedTestData.vErrorMeterConc.Add(meterConc);
                    index++;
                    if (item["结论"] != "合格" && !不合格列表.ContainsKey(ItemNo))
                    {
                        不合格列表.Add(ItemNo, new VeriDisqualReasonList()
                        {
                            barCode = meter.MD_BarCode,
                            disqualReason = GetErrCode(ItemNo),
                            veriTaskNo = meter.MD_TaskNo,
                            sysNo = RConfig.SysNo,
                            veriUnitNo = item["检测系统编号"],
                        });
                    }
                }
            }
        }

        public void 误差一致性试验(DETedTestData dETedTestData, TestMeterInfo meter, Dictionary<string, Dictionary<string, Dictionary<string, string>>> Results, Dictionary<string, VeriDisqualReasonList> 不合格列表)
        {
            int index = 1;
            string ItemNo = ProjectID.误差一致性;
            if (Results.ContainsKey(ItemNo))
            {
                foreach (var item in Results[ItemNo].Values)
                {
                    if (string.IsNullOrWhiteSpace(item["结论"])) continue;
                    string xib = item["电流倍数"];
                    if (xib == "Ib") xib = "1.0Ib";
                    else if (xib == "10Itr") xib = "10.0Itr";
                    else if (xib == "1Itr" || xib == "1.0Itr") xib = "Itr";
                    else if (xib == "1In" || xib == "1.0In") xib = "In";

                    VConsistMeterConc meterConc = new VConsistMeterConc()
                    {
                        areaCode = RConfig.AreaCode,
                        assetNo = meter.MD_BarCode,
                        barCode = meter.MD_BarCode,
                        checkConc = GetResultCode(item["结论"]),
                        //chkCont = DETECT_CONTENT,
                        devCls = RConfig.DevCls,
                        devId = RConfig.DevId,
                        devSeatNo = item["表位号"],
                        machNo = item["检测系统编号"],
                        plantElementNo = item["检测系统编号"],
                        plantNo = item["检测系统编号"],
                        psOrgNo = RConfig.PsOrgNo,
                        sn = "1",
                        sysNo = RConfig.SysNo,
                        validFlag = GetCodeValue("validFlag", "有效"),
                        veriCaliDate = GetTime(meter.VerifyDate),
                        veriPointSn = index.ToString(),
                        veriTaskNo = meter.MD_TaskNo,
                        veriMeterRsltId = RConfig.VeriMeterRsltId,

                        actlError = item["误差1"] + "|" + item["误差2"],
                        allAvgError = item["平均值"],
                        simpling = "2",
                        loadCurrent = GetCodeValue("meterTestCurLoad", xib),
                        errDown = item["误差下限"],
                        errUp = item["误差上限"],
                        pf = GetCodeValue("meterTestPowerFactor", item["功率因数"]),
                        pules = item["检定圈数"],
                        testAvgErr = item["平均值"],
                        intConvertErr = item["化整值"],
                        bothWayPowerFlag = GetCodeValue("powerFlag", "正向有功"),
                        allError = item["误差1"] + "|" + item["误差2"],
                        allIntError = item["样品均值"],
                        avgOnceErr = "",
                        avgTwiceErr = "",
                        onceErr = "",
                        twiceErr = "",
                        intOnceErr = "",
                        intTwiceErr = "",

                    };
                    if (float.TryParse(meterConc.intConvertErr, out float HZ1))
                    {
                        if (Math.Abs(HZ1) < 0.00001)
                        {
                            meterConc.intConvertErr = meterConc.intConvertErr.Replace("+", "").Replace("-", "");
                        }
                    }
                    if (dETedTestData.vConsistMeterConc == null) dETedTestData.vConsistMeterConc = new List<VConsistMeterConc>();
                    dETedTestData.vConsistMeterConc.Add(meterConc);
                    index++;
                    if (item["结论"] != "合格" && !不合格列表.ContainsKey(ItemNo))
                    {
                        不合格列表.Add(ItemNo, new VeriDisqualReasonList()
                        {
                            barCode = meter.MD_BarCode,
                            disqualReason = GetErrCode(ItemNo),
                            veriTaskNo = meter.MD_TaskNo,
                            sysNo = RConfig.SysNo,
                            veriUnitNo = item["检测系统编号"],
                        });
                    }
                }
            }
        }

        public void 负载电流升降变差试验(DETedTestData dETedTestData, TestMeterInfo meter, Dictionary<string, Dictionary<string, Dictionary<string, string>>> Results, Dictionary<string, VeriDisqualReasonList> 不合格列表)
        {
            int index = 1;
            string ItemNo = ProjectID.负载电流升将变差;
            if (Results.ContainsKey(ItemNo))
            {
                foreach (var item in Results[ItemNo].Values)
                {
                    if (string.IsNullOrWhiteSpace(item["结论"])) continue;

                    List<string> Names = new List<string>() { "01Ib", "Ib", "Imax" };
                    for (int i = 0; i < Names.Count; i++)
                    {
                        string xib = item["电流点" + (i + 1).ToString()];
                        if (xib == "Ib") xib = "1.0Ib";
                        else if (xib == "10Itr") xib = "10.0Itr";
                        else if (xib == "1Itr" || xib == "1.0Itr") xib = "Itr";
                        else if (xib == "1In" || xib == "1.0In") xib = "In";

                        VVariationMeterConc meterConc = new VVariationMeterConc()
                        {
                            areaCode = RConfig.AreaCode,
                            assetNo = meter.MD_BarCode,
                            barCode = meter.MD_BarCode,
                            checkConc = GetResultCode(item["结论"]),
                            devCls = RConfig.DevCls,
                            devId = RConfig.DevId,
                            devSeatNo = item["表位号"],
                            machNo = item["检测系统编号"],
                            plantElementNo = item["检测系统编号"],
                            plantNo = item["检测系统编号"],
                            psOrgNo = RConfig.PsOrgNo,
                            sn = "1",
                            sysNo = RConfig.SysNo,
                            validFlag = GetCodeValue("validFlag", "有效"),
                            veriCaliDate = GetTime(meter.VerifyDate),
                            veriPointSn = index.ToString(),
                            veriTaskNo = meter.MD_TaskNo,
                            veriMeterRsltId = RConfig.VeriMeterRsltId,

                            bothWayPowerFlag = GetCodeValue("powerFlag", "正向有功"),
                            loadCurrent = GetCodeValue("meterTestCurLoad", xib),
                            pf = GetCodeValue("meterTestPowerFactor", "1.0"),
                            curPhaseCode = 功率元件转换("H"),

                            detectCircle = item["Imax检定圈数"],
                            downErr1 = item[Names[i] + "下降误差1"],
                            downErr2 = item[Names[i] + "下降误差2"],
                            upErr1 = item[Names[i] + "上升误差1"],
                            upErr2 = item[Names[i] + "上升误差2"],
                            avgDownErr = item[Names[i] + "下降平均值"],
                            intDownErr = item[Names[i] + "下降化整值"],
                            variationErr = item[Names[i] + "差值"],
                            intVariationErr = item[Names[i] + "差值"],
                            avgUpErr = item[Names[i] + "上升平均值"],
                            intUpErr = item[Names[i] + "上升化整值"],
                            simpling = "2",
                            waitTime = Names[i] == "Imax" ? "120" : "0",
                        };
                        if (dETedTestData.vVariationMeterConc == null) dETedTestData.vVariationMeterConc = new List<VVariationMeterConc>();
                        dETedTestData.vVariationMeterConc.Add(meterConc);
                        index++;
                    }
                    if (item["结论"] != "合格" && !不合格列表.ContainsKey(ItemNo))
                    {
                        不合格列表.Add(ItemNo, new VeriDisqualReasonList()
                        {
                            barCode = meter.MD_BarCode,
                            disqualReason = GetErrCode(ItemNo),
                            veriTaskNo = meter.MD_TaskNo,
                            sysNo = RConfig.SysNo,
                            veriUnitNo = item["检测系统编号"],
                        });
                    }
                }
            }
        }

        public void 时段投切试验(DETedTestData dETedTestData, TestMeterInfo meter, Dictionary<string, Dictionary<string, Dictionary<string, string>>> Results, Dictionary<string, VeriDisqualReasonList> 不合格列表)
        {
            int index = 1;
            string ItemNo = ProjectID.时段投切;
            if (Results.ContainsKey(ItemNo))
            {
                foreach (var item in Results[ItemNo].Values)
                {
                    if (string.IsNullOrWhiteSpace(item["结论"])) continue;
                    VTsMeterConc meterConc = new VTsMeterConc()
                    {
                        veriMeterRsltId = RConfig.VeriMeterRsltId,
                        areaCode = RConfig.AreaCode,
                        psOrgNo = RConfig.PsOrgNo,
                        devId = RConfig.DevId,
                        assetNo = meter.MD_BarCode,
                        barCode = meter.MD_BarCode,
                        veriTaskNo = meter.MD_TaskNo,
                        devCls = RConfig.DevCls,
                        sysNo = RConfig.SysNo,
                        plantNo = item["检测系统编号"],
                        plantElementNo = item["检测系统编号"],
                        machNo = item["检测系统编号"],
                        devSeatNo = item["表位号"],
                        veriCaliDate = GetTime(meter.VerifyDate),
                        sn = "1",
                        validFlag = GetCodeValue("validFlag", "有效"),
                        veriPointSn = index.ToString(),
                        rate = "",
                        tsStartTime = "",
                        tsRealTime = "",
                        tsWay = "",
                        //tsErrConcCode = item["投切误差"],
                        errAbs = "",

                        rv = GetCodeValue("meterTestVolt", "100%Un"),
                        startTime = "",
                        errDowm = "",
                        errUp = "",
                        checkConc = GetResultCode(item["结论"]),
                    };
                    if (dETedTestData.vTsMeterConc == null) dETedTestData.vTsMeterConc = new List<VTsMeterConc>();
                    dETedTestData.vTsMeterConc.Add(meterConc);
                    index++;
                    if (item["结论"] != "合格" && !不合格列表.ContainsKey(ItemNo))
                    {
                        不合格列表.Add(ItemNo, new VeriDisqualReasonList()
                        {
                            barCode = meter.MD_BarCode,
                            disqualReason = GetErrCode(ItemNo),
                            veriTaskNo = meter.MD_TaskNo,
                            sysNo = RConfig.SysNo,
                            veriUnitNo = item["检测系统编号"],
                        });
                    }
                }
            }
        }

        public void 测量重复性(DETedTestData dETedTestData, TestMeterInfo meter, Dictionary<string, Dictionary<string, Dictionary<string, string>>> Results, Dictionary<string, VeriDisqualReasonList> 不合格列表)
        {
            int index = 1;
            string ItemNo = ProjectID.重复性;
            if (Results.ContainsKey(ItemNo))
            {
                foreach (var item in Results[ItemNo].Values)
                {
                    if (string.IsNullOrWhiteSpace(item["结论"])) continue;
                    VMeasureRptMeterConc meterConc = new VMeasureRptMeterConc()
                    {
                        areaCode = RConfig.AreaCode,
                        assetNo = meter.MD_BarCode,
                        barCode = meter.MD_BarCode,
                        checkConc = GetResultCode(item["结论"]),
                        devCls = RConfig.DevCls,
                        devId = RConfig.DevId,
                        devSeatNo = item["表位号"],
                        machNo = item["检测系统编号"],
                        plantElementNo = item["检测系统编号"],
                        plantNo = item["检测系统编号"],
                        psOrgNo = RConfig.PsOrgNo,
                        sn = "1",
                        sysNo = RConfig.SysNo,
                        validFlag = GetCodeValue("validFlag", "有效"),
                        veriCaliDate = GetTime(meter.VerifyDate),
                        veriPointSn = index.ToString(),
                        veriTaskNo = meter.MD_TaskNo,
                        veriMeterRsltId = RConfig.VeriMeterRsltId,

                        rv = GetCodeValue("meterTestVolt", "100%Un"),
                        bothWayPowerFlag = GetCodeValue("powerFlag", item["功率方向"]),
                        loadCurrent = GetCodeValue("meterTestCurLoad", item["电流倍数"]),
                        pf = GetCodeValue("meterTestPowerFactor", item["功率因数"]),
                        simpling = item["误差值"].Split(',').Length.ToString(),
                        deviationLimt = item["偏差"],
                    };
                    if (dETedTestData.vMeasureRptMeterConc == null) dETedTestData.vMeasureRptMeterConc = new List<VMeasureRptMeterConc>();
                    dETedTestData.vMeasureRptMeterConc.Add(meterConc);
                    index++;
                    if (item["结论"] != "合格" && !不合格列表.ContainsKey(ItemNo))
                    {
                        不合格列表.Add(ItemNo, new VeriDisqualReasonList()
                        {
                            barCode = meter.MD_BarCode,
                            disqualReason = GetErrCode(ItemNo),
                            veriTaskNo = meter.MD_TaskNo,
                            sysNo = RConfig.SysNo,
                            veriUnitNo = item["检测系统编号"],
                        });
                    }
                }
            }
        }

        public void 密钥下装(DETedTestData dETedTestData, TestMeterInfo meter, Dictionary<string, Dictionary<string, Dictionary<string, string>>> Results, Dictionary<string, VeriDisqualReasonList> 不合格列表)
        {
            int index = 1;

            string ItemNo = ProjectID.密钥更新_预先调试;
            if (Results.ContainsKey(ProjectID.密钥更新))
            {
                ItemNo = ProjectID.密钥更新;
            }
            if (Results.ContainsKey(ItemNo))
            {
                foreach (var item in Results[ItemNo].Values)
                {
                    if (string.IsNullOrWhiteSpace(item["结论"])) continue;
                    VEsamMeterConc meterConc = new VEsamMeterConc()
                    {
                        areaCode = RConfig.AreaCode,
                        assetNo = meter.MD_BarCode,
                        barCode = meter.MD_BarCode,
                        checkConc = GetResultCode(item["结论"]),
                        //chkCont = DETECT_CONTENT,
                        devCls = RConfig.DevCls,
                        devId = RConfig.DevId,
                        devSeatNo = item["表位号"],
                        machNo = item["检测系统编号"],
                        plantElementNo = item["检测系统编号"],
                        plantNo = item["检测系统编号"],
                        psOrgNo = RConfig.PsOrgNo,
                        sn = "1",
                        sysNo = RConfig.SysNo,
                        validFlag = GetCodeValue("validFlag", "有效"),
                        veriCaliDate = GetTime(meter.VerifyDate),
                        veriPointSn = index.ToString(),
                        veriTaskNo = meter.MD_TaskNo,
                        veriMeterRsltId = RConfig.VeriMeterRsltId,


                        rv = GetCodeValue("meterTestVolt", "100%Un"),
                        keyType = GetCodeValue("secretKeyType", "身份认证密钥"),
                        keyStatus = GetCodeValue("secretKeyStatus", "正式密钥"),
                        keyNum = "",
                        keyVer = ""
                    };
                    if (dETedTestData.vEsamMeterConc == null) dETedTestData.vEsamMeterConc = new List<VEsamMeterConc>();
                    dETedTestData.vEsamMeterConc.Add(meterConc);
                    index++;
                    if (item["结论"] != "合格" && !不合格列表.ContainsKey(ItemNo))
                    {
                        不合格列表.Add(ItemNo, new VeriDisqualReasonList()
                        {
                            barCode = meter.MD_BarCode,
                            disqualReason = GetErrCode(ItemNo),
                            veriTaskNo = meter.MD_TaskNo,
                            sysNo = RConfig.SysNo,
                            veriUnitNo = item["检测系统编号"],
                        });
                    }
                }
            }
         


        }

        public void 剩余电量递减度试验(DETedTestData dETedTestData, TestMeterInfo meter, Dictionary<string, Dictionary<string, Dictionary<string, string>>> Results, Dictionary<string, VeriDisqualReasonList> 不合格列表)
        {
            int index = 1;
            string ItemNo = ProjectID.剩余电量递减准确度;
            if (Results.ContainsKey(ItemNo))
            {
                foreach (var item in Results[ItemNo].Values)
                {
                    if (string.IsNullOrWhiteSpace(item["结论"])) continue;
                    VEqMeterConc meterConc = new VEqMeterConc()
                    {
                        areaCode = RConfig.AreaCode,
                        assetNo = meter.MD_BarCode,
                        barCode = meter.MD_BarCode,
                        checkConc = GetResultCode(item["结论"]),
                        devCls = RConfig.DevCls,
                        devId = RConfig.DevId,
                        devSeatNo = item["表位号"],
                        machNo = item["检测系统编号"],
                        plantElementNo = item["检测系统编号"],
                        plantNo = item["检测系统编号"],
                        psOrgNo = RConfig.PsOrgNo,
                        sn = "1",
                        sysNo = RConfig.SysNo,
                        validFlag = GetCodeValue("validFlag", "有效"),
                        veriCaliDate = GetTime(meter.VerifyDate),
                        veriPointSn = index.ToString(),
                        veriTaskNo = meter.MD_TaskNo,
                        veriMeterRsltId = RConfig.VeriMeterRsltId,

                        totalEq = "",
                        surplusEq = "",
                        currElecPrice = "",
                        loadCurrent = GetCodeValue("meterTestCurLoad", item["电流倍数"]),
                        pf = GetCodeValue("meterTestPowerFactor", item["功率因素"]),

                    };
                    if (dETedTestData.vEqMeterConc == null) dETedTestData.vEqMeterConc = new List<VEqMeterConc>();
                    dETedTestData.vEqMeterConc.Add(meterConc);
                    index++;
                    if (item["结论"] != "合格" && !不合格列表.ContainsKey(ItemNo))
                    {
                        不合格列表.Add(ItemNo, new VeriDisqualReasonList()
                        {
                            barCode = meter.MD_BarCode,
                            disqualReason = GetErrCode(ItemNo),
                            veriTaskNo = meter.MD_TaskNo,
                            sysNo = RConfig.SysNo,
                            veriUnitNo = item["检测系统编号"],
                        });
                    }
                }
            }
        }

        public void 通讯测试(DETedTestData dETedTestData, TestMeterInfo meter, Dictionary<string, Dictionary<string, Dictionary<string, string>>> Results, Dictionary<string, VeriDisqualReasonList> 不合格列表)
        {
            int index = 1;
            string ItemNo = ProjectID.通讯测试;
            if (Results.ContainsKey(ItemNo))
            {
                foreach (var item in Results[ItemNo].Values)
                {
                    if (string.IsNullOrWhiteSpace(item["结论"])) continue;
                    VComminicatMeterConc meterConc = new VComminicatMeterConc()
                    {
                        areaCode = RConfig.AreaCode,
                        assetNo = meter.MD_BarCode,
                        barCode = meter.MD_BarCode,
                        checkConc = GetResultCode(item["结论"]),
                        devCls = RConfig.DevCls,
                        devId = RConfig.DevId,
                        devSeatNo = item["表位号"],
                        machNo = item["检测系统编号"],
                        plantElementNo = item["检测系统编号"],
                        plantNo = item["检测系统编号"],
                        psOrgNo = RConfig.PsOrgNo,
                        sn = "1",
                        sysNo = RConfig.SysNo,
                        validFlag = GetCodeValue("validFlag", "有效"),
                        veriCaliDate = GetTime(meter.VerifyDate),
                        veriPointSn = index.ToString(),
                        veriTaskNo = meter.MD_TaskNo,
                        veriMeterRsltId = RConfig.VeriMeterRsltId,
                    };
                    if (dETedTestData.vComminicatMeterConc == null) dETedTestData.vComminicatMeterConc = new List<VComminicatMeterConc>();
                    dETedTestData.vComminicatMeterConc.Add(meterConc);
                    index++;
                    if (item["结论"] != "合格" && !不合格列表.ContainsKey(ItemNo))
                    {
                        不合格列表.Add(ItemNo, new VeriDisqualReasonList()
                        {
                            barCode = meter.MD_BarCode,
                            disqualReason = GetErrCode(ItemNo),
                            veriTaskNo = meter.MD_TaskNo,
                            sysNo = RConfig.SysNo,
                            veriUnitNo = item["检测系统编号"],
                        });
                    }
                }
            }
        }

        public void 安全认证试验(DETedTestData dETedTestData, TestMeterInfo meter, Dictionary<string, Dictionary<string, Dictionary<string, string>>> Results, Dictionary<string, VeriDisqualReasonList> 不合格列表)
        {
            int index = 1;
            string ItemNo = ProjectID.密钥更新;
            if (Results.ContainsKey(ItemNo))
            {
                foreach (var item in Results[ItemNo].Values)
                {
                    if (string.IsNullOrWhiteSpace(item["结论"])) continue;
                    VEsamSecMeterConc meterConc = new VEsamSecMeterConc()
                    {
                        areaCode = RConfig.AreaCode,
                        assetNo = meter.MD_BarCode,
                        barCode = meter.MD_BarCode,
                        checkConc = GetResultCode(item["结论"]),
                        //chkCont = DETECT_CONTENT,
                        devCls = RConfig.DevCls,
                        devId = RConfig.DevId,
                        devSeatNo = item["表位号"],
                        machNo = item["检测系统编号"],
                        plantElementNo = item["检测系统编号"],
                        plantNo = item["检测系统编号"],
                        psOrgNo = RConfig.PsOrgNo,
                        sn = "1",
                        sysNo = RConfig.SysNo,
                        validFlag = GetCodeValue("validFlag", "有效"),
                        veriCaliDate = GetTime(meter.VerifyDate),
                        veriPointSn = index.ToString(),
                        veriTaskNo = meter.MD_TaskNo,
                        veriMeterRsltId = RConfig.VeriMeterRsltId,
                        esamNo = "",
                    };
                    if (dETedTestData.vEsamSecMeterConc == null) dETedTestData.vEsamSecMeterConc = new List<VEsamSecMeterConc>();
                    dETedTestData.vEsamSecMeterConc.Add(meterConc);
                    index++;
                    if (item["结论"] != "合格" && !不合格列表.ContainsKey(ProjectID.身份认证))
                    {
                        不合格列表.Add(ProjectID.身份认证, new VeriDisqualReasonList()
                        {
                            barCode = meter.MD_BarCode,
                            disqualReason = GetErrCode(ProjectID.身份认证),
                            veriTaskNo = meter.MD_TaskNo,
                            sysNo = RConfig.SysNo,
                            veriUnitNo = item["检测系统编号"],
                        });
                    }
                }
            }
        }

        public void 预置参数设置(DETedTestData dETedTestData, TestMeterInfo meter, Dictionary<string, Dictionary<string, Dictionary<string, string>>> Results, Dictionary<string, VeriDisqualReasonList> 不合格列表)
        {
            int index = 1;
            string ItemNo = ProjectID.预置内容设置;
            if (Results.ContainsKey(ItemNo))
            {
                foreach (var item in Results[ItemNo].Values)
                {
                    if (string.IsNullOrWhiteSpace(item["结论"])) continue;
                    VPreParamMeterConc meterConc = new VPreParamMeterConc()
                    {
                        areaCode = RConfig.AreaCode,
                        assetNo = meter.MD_BarCode,
                        barCode = meter.MD_BarCode,
                        checkConc = GetResultCode(item["结论"]),
                        devCls = RConfig.DevCls,
                        devId = RConfig.DevId,
                        devSeatNo = item["表位号"],
                        machNo = item["检测系统编号"],
                        plantElementNo = item["检测系统编号"],
                        plantNo = item["检测系统编号"],
                        psOrgNo = RConfig.PsOrgNo,
                        sn = "1",
                        sysNo = RConfig.SysNo,
                        validFlag = GetCodeValue("validFlag", "有效"),
                        veriCaliDate = GetTime(meter.VerifyDate),
                        veriPointSn = index.ToString(),
                        veriTaskNo = meter.MD_TaskNo,
                        veriMeterRsltId = RConfig.VeriMeterRsltId,

                        dataName = item["数据项名称"],
                        dataId = item["标识编码"],
                        controlCode = "",
                        dataBlockFlag = "",
                        delayWaitTime = "",
                        loginPwd = "",
                        dataFormat = item["数据格式"],
                        standardValue = item["标准数据"],
                    };
                    if (dETedTestData.vPreParamMeterConc == null) dETedTestData.vPreParamMeterConc = new List<VPreParamMeterConc>();
                    dETedTestData.vPreParamMeterConc.Add(meterConc);
                    index++;
                    if (item["结论"] != "合格" && !不合格列表.ContainsKey(ItemNo))
                    {
                        不合格列表.Add(ItemNo, new VeriDisqualReasonList()
                        {
                            barCode = meter.MD_BarCode,
                            disqualReason = GetErrCode(ItemNo),
                            veriTaskNo = meter.MD_TaskNo,
                            sysNo = RConfig.SysNo,
                            veriUnitNo = item["检测系统编号"],
                        });
                    }
                }
            }
        }

        public void 费率时段和功能(DETedTestData dETedTestData, TestMeterInfo meter, Dictionary<string, Dictionary<string, Dictionary<string, string>>> Results, Dictionary<string, VeriDisqualReasonList> 不合格列表)
        {
            int index = 1;
            string ItemNo = ProjectID.费率时段检查;
            if (Results.ContainsKey(ItemNo))
            {
                foreach (var item in Results[ItemNo].Values)
                {
                    if (string.IsNullOrWhiteSpace(item["结论"])) continue;
                    VFeeMeterConc meterConc = new VFeeMeterConc()
                    {
                        areaCode = RConfig.AreaCode,
                        assetNo = meter.MD_BarCode,
                        barCode = meter.MD_BarCode,
                        checkConc = GetResultCode(item["结论"]),
                        //chkCont = DETECT_CONTENT,
                        devCls = RConfig.DevCls,
                        devId = RConfig.DevId,
                        devSeatNo = item["表位号"],
                        machNo = item["检测系统编号"],
                        plantElementNo = item["检测系统编号"],
                        plantNo = item["检测系统编号"],
                        psOrgNo = RConfig.PsOrgNo,
                        sn = "1",
                        sysNo = RConfig.SysNo,
                        validFlag = GetCodeValue("validFlag", "有效"),
                        veriCaliDate = GetTime(meter.VerifyDate),
                        veriPointSn = index.ToString(),
                        veriTaskNo = meter.MD_TaskNo,
                        veriMeterRsltId = RConfig.VeriMeterRsltId,

                        //pf = GetCodeValue("meterTestPowerFactor", item["功率因素"]),
                        //loadCurrent = GetCodeValue("meterTestCurLoad", item["电流倍数"]),
                        //bothWayPowerFlag = GetCodeValue("powerFlag", item["功率方向"]),
                        //curPhaseCode = 功率元件转换(item["功率元件"]),
                    };
                    if (dETedTestData.vFeeMeterConc == null) dETedTestData.vFeeMeterConc = new List<VFeeMeterConc>();
                    dETedTestData.vFeeMeterConc.Add(meterConc);
                    index++;
                    if (item["结论"] != "合格" && !不合格列表.ContainsKey(ItemNo))
                    {
                        不合格列表.Add(ItemNo, new VeriDisqualReasonList()
                        {
                            barCode = meter.MD_BarCode,
                            disqualReason = GetErrCode(ItemNo),
                            veriTaskNo = meter.MD_TaskNo,
                            sysNo = RConfig.SysNo,
                            veriUnitNo = item["检测系统编号"],
                        });
                    }
                }
            }
        }

        public void 费控试验(DETedTestData dETedTestData, TestMeterInfo meter, Dictionary<string, Dictionary<string, Dictionary<string, string>>> Results, Dictionary<string, VeriDisqualReasonList> 不合格列表)
        {
            //int index = 1;
            //string ItemNo = ProjectID.外观检查;
            //if (Results.ContainsKey(ItemNo))
            //{
            //    foreach (var item in Results[ItemNo].Values)
            //    {
            //        if (string.IsNullOrWhiteSpace(item["结论"])) continue;
            //        //VIntuitMeterConc meterConc = new VIntuitMeterConc()
            //        //{
            //        //    areaCode = RConfig.AreaCode,
            //        //    assetNo = meter.MD_BarCode,
            //        //    barCode = meter.MD_BarCode,
            //        //    checkConc = GetResultCode(item["结论"]),
            //        //    chkCont = DETECT_CONTENT,
            //        //    devCls = RConfig.DevCls,
            //        //    devId = RConfig.DevId,
            //        //    devSeatNo = item["表位号"],
            //        //    machNo = item["检测系统编号"],
            //        //    plantElementNo = item["检测系统编号"],
            //        //    plantNo = item["检测系统编号"],
            //        //    psOrgNo = RConfig.PsOrgNo,
            //        //    sn = "1",
            //        //    sysNo = RConfig.SysNo,
            //        //    validFlag = "01",
            //        //    veriCaliDate = GetTime(meter.VerifyDate),
            //        //    veriPointSn = index.ToString(),
            //        //    veriTaskNo = meter.MD_TaskNo,
            //        //};
            //        //dETedTestData.vIntuitMeterConc.Add(meterConc);
            //        index++;
            //        if (item["结论"] != "合格" && !不合格列表.ContainsKey(ItemNo))
            //        {
            //            不合格列表.Add(ItemNo, new VeriDisqualReasonList()
            //            {
            //                barCode = meter.MD_BarCode,
            //                disqualReason = GetErrCode(ItemNo),
            //                veriTaskNo = meter.MD_TaskNo,
            //                sysNo = RConfig.SysNo,
            //                veriUnitNo = item["检测系统编号"],
            //            });
            //        }
            //    }
            //}
        }

        public void 预置参数检查(DETedTestData dETedTestData, TestMeterInfo meter, Dictionary<string, Dictionary<string, Dictionary<string, string>>> Results, Dictionary<string, VeriDisqualReasonList> 不合格列表)
        {
            int index = 1;
            string ItemNo = ProjectID.预置内容检查;
            if (Results.ContainsKey(ItemNo))
            {
                foreach (var item in Results[ItemNo].Values)
                {
                    if (string.IsNullOrWhiteSpace(item["结论"])) continue;
                    VPreCheckMeterConc meterConc = new VPreCheckMeterConc()
                    {
                        areaCode = RConfig.AreaCode,
                        assetNo = meter.MD_BarCode,
                        barCode = meter.MD_BarCode,
                        checkConc = GetResultCode(item["结论"]),
                        devCls = RConfig.DevCls,
                        devId = RConfig.DevId,
                        devSeatNo = item["表位号"],
                        machNo = item["检测系统编号"],
                        plantElementNo = item["检测系统编号"],
                        plantNo = item["检测系统编号"],
                        psOrgNo = RConfig.PsOrgNo,
                        sn = "1",
                        sysNo = RConfig.SysNo,
                        validFlag = GetCodeValue("validFlag", "有效"),
                        veriCaliDate = GetTime(meter.VerifyDate),
                        veriPointSn = index.ToString(),
                        veriTaskNo = meter.MD_TaskNo,
                        veriMeterRsltId = RConfig.VeriMeterRsltId,

                        dataName = item["数据项名称"],
                        dataId = item["标识编码"],
                        controlCode = "",
                        dataBlockFlag = "",
                        realValue = item["检定信息"],
                        deterUpperLimit = "",
                        deterLowerLimit = "",
                        dataFormat = item["数据格式"],
                        standardValue = item["标准数据"],
                    };
                    if (dETedTestData.vPreCheckMeterConc == null) dETedTestData.vPreCheckMeterConc = new List<VPreCheckMeterConc>();
                    dETedTestData.vPreCheckMeterConc.Add(meterConc);
                    index++;
                    if (item["结论"] != "合格" && !不合格列表.ContainsKey(ItemNo))
                    {
                        不合格列表.Add(ItemNo, new VeriDisqualReasonList()
                        {
                            barCode = meter.MD_BarCode,
                            disqualReason = GetErrCode(ItemNo),
                            veriTaskNo = meter.MD_TaskNo,
                            sysNo = RConfig.SysNo,
                            veriUnitNo = item["检测系统编号"],
                        });
                    }
                }
            }
        }

        public void 需量示值误差(DETedTestData dETedTestData, TestMeterInfo meter, Dictionary<string, Dictionary<string, Dictionary<string, string>>> Results, Dictionary<string, VeriDisqualReasonList> 不合格列表)
        {
            int index = 1;
            string ItemNo = ProjectID.需量示值误差;
            if (Results.ContainsKey(ItemNo))
            {
                foreach (var item in Results[ItemNo].Values)
                {
                    if (string.IsNullOrWhiteSpace(item["结论"])) continue;
                    VDemandvalMeterConc meterConc = new VDemandvalMeterConc()
                    {
                        areaCode = RConfig.AreaCode,
                        assetNo = meter.MD_BarCode,
                        barCode = meter.MD_BarCode,
                        checkConc = GetResultCode(item["结论"]),
                        //chkCont = DETECT_CONTENT,
                        devCls = RConfig.DevCls,
                        devId = RConfig.DevId,
                        devSeatNo = item["表位号"],
                        machNo = item["检测系统编号"],
                        plantElementNo = item["检测系统编号"],
                        plantNo = item["检测系统编号"],
                        psOrgNo = RConfig.PsOrgNo,
                        sn = "1",
                        sysNo = RConfig.SysNo,
                        validFlag = GetCodeValue("validFlag", "有效"),
                        veriCaliDate = GetTime(meter.VerifyDate),
                        veriPointSn = index.ToString(),
                        veriTaskNo = meter.MD_TaskNo,
                        veriMeterRsltId = RConfig.VeriMeterRsltId,

                        demandPeriod = item["需量周期"],
                        demandTime = item["滑差时间"],
                        demandInterval = item["滑差次数"],
                        realDemand = item["实际需量"]?.Replace(" ", ""),
                        realPeriod = item["需量周期"],
                        demandValueErr = item["需量误差"],
                        demandStandard = item["标准需量"]?.Replace(" ", ""),
                        demandValueErrAbs = "",
                        clearDataRst = "02",
                        valueConcCode = GetResultCode(item["结论"]),

                        bothWayPowerFlag = GetCodeValue("powerFlag", item["功率方向"]),
                        rv = GetCodeValue("meterTestVolt", "100%Un"),
                        loadCurrent = GetCodeValue("meterTestCurLoad", item["电流"]),
                        pf = GetCodeValue("meterTestPowerFactor", "1.0"),
                        controlMethod = "01",
                        errDowm = "",
                        demandMeter = item["实际需量"]?.Replace(" ", ""),
                        errUp = "",
                        intConvertErr = "",
                    };
                    if (double.TryParse(item["误差下限"], out double err2))
                    {
                        meterConc.errDowm = Math.Round(err2, 4).ToString();
                    }
                    if (double.TryParse(item["误差上限"], out double err))
                    {
                        meterConc.errUp = Math.Round(err, 4).ToString();
                    }
                    if (dETedTestData.vDemandvalMeterConc == null) dETedTestData.vDemandvalMeterConc = new List<VDemandvalMeterConc>();
                    dETedTestData.vDemandvalMeterConc.Add(meterConc);
                    index++;
                    if (item["结论"] != "合格" && !不合格列表.ContainsKey(ItemNo))
                    {
                        不合格列表.Add(ItemNo, new VeriDisqualReasonList()
                        {
                            barCode = meter.MD_BarCode,
                            disqualReason = GetErrCode(ItemNo),
                            veriTaskNo = meter.MD_TaskNo,
                            sysNo = RConfig.SysNo,
                            veriUnitNo = item["检测系统编号"],
                        });
                    }
                }
            }
        }

        public void 时钟示值误差(DETedTestData dETedTestData, TestMeterInfo meter, Dictionary<string, Dictionary<string, Dictionary<string, string>>> Results, Dictionary<string, VeriDisqualReasonList> 不合格列表)
        {
            int index = 1;
            string ItemNo = ProjectID.时钟示值误差;
            if (Results.ContainsKey(ItemNo))
            {
                foreach (var item in Results[ItemNo].Values)
                {
                    if (string.IsNullOrWhiteSpace(item["结论"])) continue;
                    VClockValueMeterConc meterConc = new VClockValueMeterConc()
                    {
                        areaCode = RConfig.AreaCode,
                        assetNo = meter.MD_BarCode,
                        barCode = meter.MD_BarCode,
                        checkConc = GetResultCode(item["结论"]),
                        //chkCont = DETECT_CONTENT,
                        devCls = RConfig.DevCls,
                        devId = RConfig.DevId,
                        devSeatNo = item["表位号"],
                        machNo = item["检测系统编号"],
                        plantElementNo = item["检测系统编号"],
                        plantNo = item["检测系统编号"],
                        psOrgNo = RConfig.PsOrgNo,
                        sn = "1",
                        sysNo = RConfig.SysNo,
                        validFlag = GetCodeValue("validFlag", "有效"),
                        veriCaliDate = GetTime(meter.VerifyDate),
                        veriPointSn = index.ToString(),
                        veriTaskNo = meter.MD_TaskNo,
                        veriMeterRsltId = RConfig.VeriMeterRsltId,

                        timeErr = item["时间差"],
                        writeDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                        meterDate = "",
                        meterValue = "",
                        stdDate = "",
                        stdValue = "",
                    };
                    if (DateTime.TryParse(item["GPS时间"], out DateTime stdDate))
                    {
                        meterConc.stdDate = stdDate.ToString("yyyy-MM-dd");
                        meterConc.stdValue = stdDate.ToString("HH:mm:ss");
                    }
                    else
                    {
                        meterConc.stdDate = "";
                        meterConc.stdValue = "";
                    }
                    if (DateTime.TryParse(item["被检表时间"], out DateTime meterDate))
                    {
                        meterConc.meterDate = meterDate.ToString("yyyy-MM-dd HH:mm:ss");
                        meterConc.meterValue = meterDate.ToString("HH:mm:ss");
                    }
                    else
                    {
                        meterConc.meterDate = "";
                        meterConc.meterValue = "";
                    }
                    if (dETedTestData.vClockValueMeterConc == null) dETedTestData.vClockValueMeterConc = new List<VClockValueMeterConc>();
                    dETedTestData.vClockValueMeterConc.Add(meterConc);
                    index++;
                    if (item["结论"] != "合格" && !不合格列表.ContainsKey(ItemNo))
                    {
                        不合格列表.Add(ItemNo, new VeriDisqualReasonList()
                        {
                            barCode = meter.MD_BarCode,
                            disqualReason = GetErrCode(ItemNo),
                            veriTaskNo = meter.MD_TaskNo,
                            sysNo = RConfig.SysNo,
                            veriUnitNo = item["检测系统编号"],
                        });
                    }
                }
            }
        }

        public void 电能表清零(DETedTestData dETedTestData, TestMeterInfo meter, Dictionary<string, Dictionary<string, Dictionary<string, string>>> Results, Dictionary<string, VeriDisqualReasonList> 不合格列表)
        {
            int index = 1;
            string ItemNo = ProjectID.电量清零;
            if (Results.ContainsKey(ItemNo))
            {
                foreach (var item in Results[ItemNo].Values)
                {
                    if (string.IsNullOrWhiteSpace(item["结论"])) continue;
                    VResetEqMeterConc meterConc = new VResetEqMeterConc()
                    {
                        areaCode = RConfig.AreaCode,
                        assetNo = meter.MD_BarCode,
                        barCode = meter.MD_BarCode,
                        checkConc = GetResultCode(item["结论"]),
                        //chkCont = DETECT_CONTENT,
                        devCls = meter.Other4,
                        devId = RConfig.DevId,
                        devSeatNo = item["表位号"],
                        machNo = item["检测系统编号"],
                        plantElementNo = item["检测系统编号"],
                        plantNo = item["检测系统编号"],
                        psOrgNo = RConfig.PsOrgNo,
                        sn = "1",
                        sysNo = RConfig.SysNo,
                        validFlag = GetCodeValue("validFlag", "有效"),
                        veriCaliDate = GetTime(meter.VerifyDate),
                        veriPointSn = index.ToString(),
                        veriTaskNo = meter.MD_TaskNo,
                        veriMeterRsltId = RConfig.VeriMeterRsltId,

                        //eq = item["清零前电量"],
                        resetEq = item["检定数据"],
                    };

                    if (dETedTestData.vResetEqMeterConc == null) dETedTestData.vResetEqMeterConc = new List<VResetEqMeterConc>();
                    dETedTestData.vResetEqMeterConc.Add(meterConc);
                    index++;
                    if (item["结论"] != "合格" && !不合格列表.ContainsKey(ItemNo))
                    {
                        不合格列表.Add(ItemNo, new VeriDisqualReasonList()
                        {
                            barCode = meter.MD_BarCode,
                            disqualReason = GetErrCode(ItemNo),
                            veriTaskNo = meter.MD_TaskNo,
                            sysNo = RConfig.SysNo,
                            veriUnitNo = item["检测系统编号"],
                        });
                    }
                }
            }
        }
        public void GPS对时(DETedTestData dETedTestData, TestMeterInfo meter, Dictionary<string, Dictionary<string, Dictionary<string, string>>> Results, Dictionary<string, VeriDisqualReasonList> 不合格列表)
        {
            int index = 1;
            string ItemNo = ProjectID.GPS对时;
            if (Results.ContainsKey(ItemNo))
            {
                foreach (var item in Results[ItemNo].Values)
                {
                    if (string.IsNullOrWhiteSpace(item["结论"])) continue;
                    VTimingMeterConc meterConc = new VTimingMeterConc()
                    {
                        areaCode = RConfig.AreaCode,
                        assetNo = meter.MD_BarCode,
                        barCode = meter.MD_BarCode,
                        checkConc = GetResultCode(item["结论"]),
                        //chkCont = DETECT_CONTENT,
                        devCls = RConfig.DevCls,
                        devId = RConfig.DevId,
                        devSeatNo = item["表位号"],
                        machNo = item["检测系统编号"],
                        plantElementNo = item["检测系统编号"],
                        plantNo = item["检测系统编号"],
                        psOrgNo = RConfig.PsOrgNo,
                        sn = "1",
                        sysNo = RConfig.SysNo,
                        validFlag = GetCodeValue("validFlag", "有效"),
                        veriCaliDate = GetTime(meter.VerifyDate),
                        veriPointSn = index.ToString(),
                        veriTaskNo = meter.MD_TaskNo,
                        veriMeterRsltId = RConfig.VeriMeterRsltId,
                    };

                    if (dETedTestData.vTimingMeterConc == null) dETedTestData.vTimingMeterConc = new List<VTimingMeterConc>();
                    dETedTestData.vTimingMeterConc.Add(meterConc);
                    index++;
                    if (item["结论"] != "合格" && !不合格列表.ContainsKey(ItemNo))
                    {
                        不合格列表.Add(ItemNo, new VeriDisqualReasonList()
                        {
                            barCode = meter.MD_BarCode,
                            disqualReason = GetErrCode(ItemNo),
                            veriTaskNo = meter.MD_TaskNo,
                            sysNo = RConfig.SysNo,
                            veriUnitNo = item["检测系统编号"],
                        });
                    }
                }
            }
        }
        #endregion

        #region 总结论
        public void 检定综合结论(MeterVeriResult meterVeriResult, TestMeterInfo meter, Dictionary<string, Dictionary<string, Dictionary<string, string>>> Results, Dictionary<string, VeriDisqualReasonList> 不合格列表, string 检定员, string 核验员)
        {
            //初始化结论
            if (meterVeriResult.veriDtlFormList == null) meterVeriResult.veriDtlFormList = new List<VeriDtlFormList>();
            if (meterVeriResult.meterVeriCompConc == null) meterVeriResult.meterVeriCompConc = new List<MeterVeriCompConc>();


            string DeviceNo = "";
            string MeterNo = "";


            if (Results.ContainsKey(ProjectID.通讯测试))
            {
                DeviceNo = Results[ProjectID.通讯测试][ProjectID.通讯测试]["检测系统编号"];
                MeterNo = Results[ProjectID.通讯测试][ProjectID.通讯测试]["表位号"];
            }
            VeriDtlFormList veriDtlFormList = new VeriDtlFormList()
            {
                assetNo = meter.MD_BarCode,
                barCode = meter.MD_BarCode,
                checkDate = GetTime(meter.ExpiryDate),
                checkStf = 核验员,
                devCls = RConfig.DevCls,
                devSeatNo = MeterNo,
                faultReason = "",
                frstLvFaultReason = "",
                humid = "50",
                machNo = DeviceNo,
                plantElementNo = DeviceNo,
                plantNo = DeviceNo,
                platformNo = DeviceNo,
                scndLvFaultReason = "",
                temp = "20",
                trialStf = 检定员,
                veriDate = GetTime(meter.VerifyDate),
                veriDept = "",
                veriRslt = GetResultCode(meter.Result),
                veriStf = 检定员,
                veriTaskNo = meter.MD_TaskNo,
            };
            MeterVeriCompConc meterVeriCompConc = new MeterVeriCompConc()
            {
                veriTaskNo = meter.MD_TaskNo,
                areaCode = RConfig.AreaCode,
                psOrgNo = RConfig.PsOrgNo,
                barCode = meter.MD_BarCode,
                //intuitConcCode = 获取总结论(Results, ProjectID.外观检查),
                basicErrorConcCode = 获取总结论(Results, ProjectID.基本误差试验),
                constConcCode = 获取总结论(Results, ProjectID.电能表常数试验),
                creepingConcCode = 获取总结论(Results, ProjectID.潜动试验),
                dayerrConcCode = 获取总结论(Results, ProjectID.日计时误差),
                //voltConcCode = 获取总结论(Results, ProjectID.工频耐压试验),
                standardConcCode = 获取总结论(Results, ProjectID.通讯协议检查试验),
                errorConcCode = 获取总结论(Results, ProjectID.误差变差),
                consistConcCode = 获取总结论(Results, ProjectID.误差一致性),
                variationConcCode = 获取总结论(Results, ProjectID.负载电流升将变差),
                tsConcCode = 获取总结论(Results, ProjectID.时段投切),
                runingConcCode = 获取总结论(Results, ProjectID.电能表常数试验),
                valueConcCode = 获取总结论(Results, ProjectID.需量示值误差),
                keyConcCode = 获取总结论(Results, Results.ContainsKey(ProjectID.密钥更新) ? ProjectID.密钥更新 : ProjectID.密钥更新_预先调试),
                settingConcCode = 获取总结论(Results, ProjectID.预置内容设置),
                esamConcCode = 获取总结论(Results, Results.ContainsKey(ProjectID.密钥更新) ? ProjectID.密钥更新 : ProjectID.密钥更新_预先调试),

                surplusConcCode = 获取总结论(Results, ProjectID.剩余电量递减准确度),
                resetEqMetConcCode = 获取总结论(Results, ProjectID.电量清零),
                timingMetConcCode = 获取总结论(Results, ProjectID.GPS对时),
                comminicateMetConcCode = 获取总结论(Results, ProjectID.通讯测试),
                addressMetConcCode = 获取总结论(Results, ProjectID.通讯测试),
                startingConcCode = 获取总结论(Results, ProjectID.起动试验),
                paraSetMetConc = 获取总结论(Results, ProjectID.预置内容设置),
                paraReadMetConcCode = 获取总结论(Results, ProjectID.预置内容检查),
                presetPara1CheckConcCode = "",
                hwVer = "",
                faultReason = "",
                rateTimeFuncConcCode = 获取总结论(Results, ProjectID.费率时段检查),
                eleChkConcCode = 获取总结论(Results, ProjectID.通讯测试),
                meterErrorConcCode = 获取总结论(Results, ProjectID.电能示值组合误差),
                clockValueConcCode = 获取总结论(Results, ProjectID.时钟示值误差),
                keyRecoveryConcCode = 获取总结论(Results, Results.ContainsKey(ProjectID.密钥更新) ? ProjectID.密钥更新 : ProjectID.密钥更新_预先调试),
                presetContentSetConcCode = 获取总结论(Results, ProjectID.预置内容设置),
                presetContentCheckConcCode = 获取总结论(Results, ProjectID.预置内容检查),
                controlFuncConcCode = 获取总结论(Results, ProjectID.远程控制),
                preContenrChkConcCode = 获取总结论(Results, ProjectID.预置内容检查),
                swVer = "",
                keyUpdateConcCode = 获取总结论(Results, Results.ContainsKey(ProjectID.密钥恢复)? ProjectID.密钥恢复 : ProjectID.密钥恢复_预先调试),
                rs485ConcCode = 获取总结论(Results, ProjectID.通讯测试),
            };


            meterVeriResult.veriDtlFormList.Add(veriDtlFormList);
            meterVeriResult.meterVeriCompConc.Add(meterVeriCompConc);
            if (meter.Result != "合格" && 不合格列表.Count > 0)
            {
                if (meterVeriResult.veriDisqualReasonList == null) meterVeriResult.veriDisqualReasonList = new List<VeriDisqualReasonList>();
                meterVeriResult.veriDisqualReasonList.AddRange(不合格列表.Values.ToList());
            }

        }
        #endregion

        #region 内部方法

        private class 费率电量结构
        {
            /// <summary>
            /// 尖|峰|平|谷|深谷
            /// </summary>
            public float[] 电量 = new float[5];

            public float 总
            {
                get
                {
                    return 电量.Sum();
                }
            }
        }
        private string 获取总结论(Dictionary<string, Dictionary<string, Dictionary<string, string>>> Results, string ItemNo, string Defuvalue = "03")
        {
            if (Results.ContainsKey(ItemNo))
            {
                string result = "";
                foreach (var item in Results[ItemNo].Values)
                {
                    if (item["结论"] == "合格")
                    {
                        result = "合格";
                    }
                    else
                    {
                        result = "不合格";
                        break;
                    }
                }
                return GetResultCode(result);
            }
            else
            {
                return Defuvalue;
            }
        }

        private string GetTime(string result)
        {
            if (DateTime.TryParse(result, out DateTime time))
            {
                return time.ToString("yyyy-MM-dd HH:mm:ss");
            }
            return "";
        }



        /// <summary>
        /// 获取电表的等级
        /// </summary>
        /// <returns></returns>
        private string[] GetMeterGrade(string accuracy)
        {
            accuracy = accuracy.ToUpper().Replace("S", "");
            accuracy = accuracy.ToUpper().Replace("（", "(").Replace("）", ")").Replace(")", "");
            string[] dj = accuracy.Split('(');
            string[] levs;
            if (dj.Length == 1)
            {
                levs = new string[] { dj[0], dj[0] };
            }
            else
            {
                levs = new string[] { dj[0], dj[1] };
            }
            for (int i = 0; i < levs.Length; i++)
            {
                if (levs[0] == "A" || levs[0] == "B")
                {
                    levs[0] = "1.0";
                }
                else if (levs[0] == "C")
                {
                    levs[0] = "0.5";
                }
                else if (levs[0] == "D")
                {
                    levs[0] = "0.2";
                }
            }
            return levs;

        }

        private string GetREAD_TYPE_CODE(string BOTH_WAY_POWER_FLAG, string FEE_RATIO)
        {
            if (string.IsNullOrWhiteSpace(BOTH_WAY_POWER_FLAG) || string.IsNullOrWhiteSpace(FEE_RATIO)) return "";
            string Fx = "";
            switch (BOTH_WAY_POWER_FLAG)
            {
                case "正向有功":
                    Fx = "有功";
                    break;
                case "反向有功":
                    Fx = "有功反向";
                    break;
                case "正向无功":
                    Fx = "无功";
                    break;
                case "反向无功":
                    Fx = "无功反向";
                    break;
                default:
                    break;
            }

            string Value = "";
            switch (Fx + $"（{FEE_RATIO}）")
            {
                case "有功（总）":
                    Value = "11";
                    break;
                case "有功（尖）":
                    Value = "12";
                    break;
                case "有功（峰）":
                    Value = "13";
                    break;
                case "有功（平）":
                    Value = "15";
                    break;
                case "有功（谷）":
                    Value = "14";
                    break;
                case "有功（深谷）":
                    Value = "96";
                    break;
                case "无功（总）":
                    Value = "21";
                    break;
                case "无功（尖）":
                    Value = "22";
                    break;
                case "无功（峰）":
                    Value = "23";
                    break;
                case "无功（平）":
                    Value = "25";
                    break;
                case "无功（谷）":
                    Value = "24";
                    break;
                case "无功（深谷）":
                    Value = "97";
                    break;
                case "有功反向（总）":
                    Value = "41";
                    break;
                case "有功反向（尖）":
                    Value = "42";
                    break;
                case "有功反向（峰）":
                    Value = "43";
                    break;
                case "有功反向（平）":
                    Value = "45";
                    break;
                case "有功反向（谷）":
                    Value = "44";
                    break;
                case "有功反向（深谷）":
                    Value = "98";
                    break;
                case "无功反向（总）":
                    Value = "51";
                    break;
                case "无功反向（尖）":
                    Value = "52";
                    break;
                case "无功反向（峰）":
                    Value = "53";
                    break;
                case "无功反向（平）":
                    Value = "55";
                    break;
                case "无功反向（谷）":
                    Value = "54";
                    break;
                case "无功反向（深谷）":
                    Value = "99";
                    break;
                default:
                    break;
            }

            return Value;
        }

        private string 功率元件转换(string value)
        {
            string strYj;
            switch (value)
            {
                case "A":
                    strYj = "A";
                    break;
                case "B":
                    strYj = "B";
                    break;
                case "C":
                    strYj = "C";
                    break;
                case "H":
                    strYj = "ABC";
                    break;
                case "A元不平衡与平衡负载之差":
                case "B元不平衡与平衡负载之差":
                case "C元不平衡与平衡负载之差":
                    strYj = value;
                    break;
                default:
                    strYj = "ABC";
                    break;
            }
            return GetCodeValue("curPhase", strYj);
        }

        /// <summary>
        /// 获取电表的常数
        /// </summary>
        /// <param name="constr"></param>
        /// <returns></returns>
        public int[] GetBcs(string constr)
        {
            constr = constr.Replace("（", "(").Replace("）", ")");

            if (constr.Trim().Length < 1)
            {
                return new int[] { 1, 1 };
            }

            string[] arTmp = constr.Trim().Replace(")", "").Split('(');

            if (arTmp.Length == 1)
            {
                if (IsNumber(arTmp[0]))
                    return new int[] { int.Parse(arTmp[0]), int.Parse(arTmp[0]) };
                else
                    return new int[] { 1, 1 };
            }
            else
            {
                if (IsNumber(arTmp[0]) && IsNumber(arTmp[1]))
                    return new int[] { int.Parse(arTmp[0]), int.Parse(arTmp[1]) };
                else
                    return new int[] { 1, 1 };
            }
        }
        private bool IsNumber(string str)
        {
            if (string.IsNullOrWhiteSpace(str)) return false;
            return System.Text.RegularExpressions.Regex.IsMatch(str, @"^[+-]?\d*[.]?\d*$");
        }
        #endregion

        #region 标准代码
        /// <summary>
        /// 根据名称或者标准代码值
        /// </summary>
        /// <param name="CoreType">标准代码类型</param>
        /// <param name="Name">名称</param>
        /// <returns></returns>
        private string GetCodeValue(string CoreType, string Name)
        {
            if (StdDicCode != null)
            {
                return StdDicCode.GetCodeValue(CoreType, Name);

            }
            return "";
        }
        /// <summary>
        /// 根据标准代码值获取名称
        /// </summary>
        /// <param name="CoreType">标准代码类型</param>
        /// <param name="Value">值</param>
        /// <returns></returns>
        private string GetCodeName(string CoreType, string Value)
        {
            if (StdDicCode != null)
            {
                StdDicCode.GetCodeName(CoreType, Value);
            }
            return "";
        }
        /// <summary>
        /// 获得不合格原因编码
        /// </summary>
        /// <returns></returns>
        private string GetErrCode(string ItemNo)
        {
            string ErrorName = "";
            switch (ItemNo)
            {
                case ProjectID.电能表常数试验:
                    ErrorName = "电能表常数试验不合格";//0102010202
                    break;
                case ProjectID.起动试验:
                    ErrorName = "起动试验不合格";//0102010203
                    break;
                case ProjectID.潜动试验:
                    ErrorName = "潜动试验不合格";//0102010204
                    break;
                case ProjectID.电能示值组合误差:
                    ErrorName = "计度器总电能示值误差不合格";//0102010207
                    break;
                case ProjectID.日计时误差:
                    ErrorName = "日计时误差不合格";//0102010209
                    break;
                case ProjectID.通讯协议检查试验:
                    ErrorName = "通信规约一致性检查不合格";//01020109
                    break;
                case ProjectID.误差变差:
                    ErrorName = "误差变差试验不合格";//0102010212
                    break;
                case ProjectID.误差一致性:
                    ErrorName = "误差一致性试验不合格";//0102010213
                    break;
                case ProjectID.负载电流升将变差:
                    ErrorName = "负载电流升降变差试验不合格";//0102010214
                    break;
                case ProjectID.时段投切:
                    ErrorName = "存储单元异常";//010103
                    break;
                case ProjectID.密钥更新:
                    ErrorName = "密钥更新试验不合格";//0101090401
                    break;
                case ProjectID.剩余电量递减准确度:
                    ErrorName = "剩余电能量（金额）递减准确度不合格";//0101020113
                    break;
                case ProjectID.外观检查:
                case ProjectID.通讯测试:
                    ErrorName = "外观、标志、通电检查不合格";//0102010101
                    break;
                case ProjectID.身份认证:
                    ErrorName = "认证失败";//01010503
                    break;
                case ProjectID.工频耐压试验:
                    ErrorName = "交流电压试验不合格";//0101090202
                    break;
                case ProjectID.费率时段检查:
                    ErrorName = "费率和时段功能异常";//0102011012
                    break;
                case ProjectID.控制功能:
                    ErrorName = "远程控制试验不合格";//0102010805
                    break;
                case ProjectID.需量示值误差:
                    ErrorName = "需量示值超差";//0101100101
                    break;
                case ProjectID.时钟示值误差:
                    ErrorName = "时钟示值误差不合格";//0101060206
                    break;
                case ProjectID.电量清零:
                    ErrorName = "清零功能异常";//0102011007
                    break;
                case ProjectID.基本误差试验:
                    ErrorName = "基本误差超差";//0101020101
                    break;
                case ProjectID.预置内容设置:
                    ErrorName = "参数设置失败";//0101080302
                    break;
                case ProjectID.预置内容检查:
                    ErrorName = "预置参数错误";//0101080301
                    break;
                case ProjectID.GPS对时:
                    ErrorName = "校时失败";//0101060205
                    break;
                default:
                    break;
            }
            if (!string.IsNullOrWhiteSpace(ErrorName))
            {
                return GetCodeValue("meterFaultPhen", ErrorName);
            }
            return "";
        }
        private string GetResultCode(string result)
        {
            if (string.IsNullOrWhiteSpace(result))
            {
                return "03";
            }
            else
            {
                if (result == "合格")
                {
                    return "02";
                }
                else
                {
                    return "01";
                }
            }
        }
        #endregion
    }
}
