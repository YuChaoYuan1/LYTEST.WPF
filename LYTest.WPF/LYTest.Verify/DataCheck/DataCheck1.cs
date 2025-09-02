using LYTest.Core;
using LYTest.Core.Model.Meter;
using LYTest.ViewModel.CheckController;

namespace LYTest.Verify.ConnProtocolTest
{
    /// <summary>
    /// 预置参数检查1
    /// 方案：数据项，操作类型，标准数据
    /// 结论：数据项，操作类型，标准数据，表数据，结论
    /// 
    /// </summary>
    public class DataCheck1 : VerifyBase
    {
        string _dataName = "";

        /// <summary>
        /// 比对类型：表值 -- 标准值
        /// 0-不比对，1-相等,2-大于，3-小于，4-包含，5-被包含
        /// </summary>
        string _dataOperate = "不比对";

        string _standValue = "";

        public override void Verify()
        {
            MessageAdd("预置参数检查1开始...", EnumLogType.流程信息);

            base.Verify();
            for (int i = 0; i < MeterNumber; i++)
            {
                if (!MeterInfo[i].YaoJianYn) continue;
                ResultDictionary["数据项"][i] = _dataName;
                ResultDictionary["操作类型"][i] = _dataOperate;
                ResultDictionary["标准数据"][i] = _standValue;
                ResultDictionary["表数据"][i] = "";
                ResultDictionary["结论"][i] = "";
            }
            RefUIData("数据项");
            RefUIData("操作类型");
            RefUIData("标准数据");
            RefUIData("表数据");
            RefUIData("结论");

            if (!CheckVoltage())
            {
                return;
            }

            if (Stop) return;

            if (string.IsNullOrWhiteSpace(OneMeterInfo.MD_MeterNo) || string.IsNullOrWhiteSpace(OneMeterInfo.MD_PostalAddress))   //没有表号的情况获取一下
            {
                MessageAdd("正在获取所有表的表地址和表号", EnumLogType.流程信息);
                ReadMeterAddrAndNo();
                if (!IsDemo)
                {
                    UpdateMeterProtocol();//更新电表命令
                }
            }

            MessageAdd($"【{_dataName}】开始{_dataOperate}", EnumLogType.流程信息);

            if (Stop) return;

            string[] readdata = MeterProtocolAdapter.Instance.ReadData(_dataName);
            DoReadConnProtocolData(readdata);

            MessageAdd("预置参数检查1结束...", EnumLogType.提示与流程信息);
        }


        /// <summary>
        /// 检定参数是否合法
        /// </summary>
        /// <returns></returns>
        protected override bool CheckPara()
        {
            string[] arr = Test_Value.Split('|');
            if (arr.Length < 3) return false;

            _dataName = arr[0];
            _dataOperate = arr[1];
            _standValue = arr[2];

            ResultNames = new string[] { "数据项", "操作类型", "标准数据", "表数据", "结论" };

            return true;
        }


        /// <summary>
        /// 执行读的结果
        /// </summary>
        protected void DoReadConnProtocolData(string[] readdata)
        {
            //结论数据
            for (int i = 0; i < MeterNumber; i++)
            {
                if (Stop) return;
                TestMeterInfo meter = MeterInfo[i];
                if (!meter.YaoJianYn) continue;

                ResultDictionary["结论"][i] = string.IsNullOrEmpty(readdata[i]) ? ConstHelper.不合格 : ConstHelper.合格;
                ResultDictionary["表数据"][i] = readdata[i] ?? ""; ;


                if (ResultDictionary["结论"][i] == ConstHelper.不合格)
                {
                    NoResoult[i] = "没有读取到数据";
                    continue;
                }

                if (_dataOperate == "相等" && readdata[i] != _standValue)
                {
                    ResultDictionary["结论"][i] = ConstHelper.不合格;
                }
                else if (_dataOperate == "大于")
                {
                    if (float.TryParse(readdata[i], out float mv) && float.TryParse(_standValue, out float rv))
                    {
                        if (mv > rv)
                            continue;
                    }
                    ResultDictionary["结论"][i] = ConstHelper.不合格;

                }
                else if (_dataOperate == "小于")
                {
                    if (float.TryParse(readdata[i], out float mv) && float.TryParse(_standValue, out float rv))
                    {
                        if (mv < rv)
                            continue;
                    }
                    ResultDictionary["结论"][i] = ConstHelper.不合格;

                }
                else if (_dataOperate == "包含" && !readdata[i].Contains(_standValue))
                {
                    ResultDictionary["结论"][i] = ConstHelper.不合格;
                }
                else if (_dataOperate == "被包含" && !_standValue.Contains(readdata[i]))
                {
                    ResultDictionary["结论"][i] = ConstHelper.不合格;
                }
            }

            RefUIData("表数据");
            RefUIData("结论");

        }

    }
}
