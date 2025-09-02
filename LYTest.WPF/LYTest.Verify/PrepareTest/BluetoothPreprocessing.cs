using LYTest.DAL.Config;
using LYTest.ViewModel.CheckController;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LYTest.Verify.PrepareTest
{
    /// <summary>
    /// 蓝牙预处理
    /// </summary>
    public class BluetoothPreprocessing : VerifyBase
    {
        public override void Verify()
        {
            base.Verify();

            try
            {


                PowerOn();
                WaitTime("正在升源", 8);
                MessageAdd("正在复位蓝牙光电模块", EnumLogType.提示信息);
                MessageAdd("正在复位蓝牙光电模块...", EnumLogType.流程信息);
                bool[] resoult = MeterProtocolAdapter.Instance.IOTMete_Reset();
                for (int i = 0; i < MeterNumber; i++)
                {
                    if (!MeterInfo[i].YaoJianYn) continue;
                    ResultDictionary["模块复位"][i] = resoult[i] ? "成功" : "失败";
                }
                RefUIData("模块复位");
                MessageAdd("正在蓝牙连接物联表...", EnumLogType.提示信息);
                MessageAdd("正在蓝牙连接物联表...", EnumLogType.流程信息);
                //这里还需要获取地址来连接蓝牙表
                string[] address = new string[MeterNumber];
                for (int i = 0; i < MeterNumber; i++)
                {
                    if (!MeterInfo[i].YaoJianYn) continue;
                    address[i] = MeterInfo[i].MD_PostalAddress;
                }


                //最多进行三次重连
                for (int index = 0; index < 5; index++)
                {
                    resoult = MeterProtocolAdapter.Instance.IOTMete_Connect(address, ConfigHelper.Instance.Bluetooth_Ping);
                    string err = "";
                    bool t = true;
                    for (int i = 0; i < MeterNumber; i++)
                    {
                        if (!MeterInfo[i].YaoJianYn) continue;
                        if (resoult[i]==false)
                        {
                            t = false;
                            err += $"【{i+1}】";
                        }
                    }
                    if (t) break;
                    MessageAdd($"表位：{err}第{index+1}次蓝牙连接失败,开始重连", EnumLogType.流程信息);
                }

                //TODO 这里如果连接失败该怎么办
                for (int i = 0; i < MeterNumber; i++)
                {
                    if (!MeterInfo[i].YaoJianYn) continue;
                    ResultDictionary["蓝牙连接"][i] = resoult[i] ? "成功" : "失败";
                }
                RefUIData("蓝牙连接");
                MessageAdd("正在进行蓝牙预处理...", EnumLogType.提示信息);
                MessageAdd("正在进行蓝牙预处理...", EnumLogType.流程信息);
                //这里没办法判断预处理状态-所以进行一次预处理
                //TODO 如果这里预处理失败怎么办
                resoult = MeterProtocolAdapter.Instance.IOTMete_Pretreatment();
                for (int i = 0; i < MeterNumber; i++)
                {
                    if (!MeterInfo[i].YaoJianYn) continue;
                    ResultDictionary["蓝牙预处理"][i] = resoult[i] ? "成功" : "失败";
                }
                RefUIData("蓝牙预处理");
                WaitTime("正在进行蓝牙预处理", 35);
                MessageAdd("正在查询蓝牙预处理状态...", EnumLogType.提示信息);
                MessageAdd("正在查询蓝牙预处理状态...", EnumLogType.流程信息);
                resoult = MeterProtocolAdapter.Instance.IOTMete_PretreatmentSelect(GetYaoJian());
                for (int i = 0; i < MeterNumber; i++)
                {
                    if (!MeterInfo[i].YaoJianYn) continue;
                    ResultDictionary["预处理状态"][i] = resoult[i] ? "成功" : "失败";
                }
                RefUIData("预处理状态");
                for (int i = 0; i < MeterNumber; i++)
                {
                    if (!MeterInfo[i].YaoJianYn) continue;
                    if (ResultDictionary["模块复位"][i] == "成功" && ResultDictionary["蓝牙连接"][i] == "成功"
                        && ResultDictionary["蓝牙预处理"][i] == "成功" && ResultDictionary["预处理状态"][i] == "成功")
                    {
                        ResultDictionary["结论"][i] = "合格";
                    }
                    else
                    {
                        ResultDictionary["结论"][i] = "不合格";
                    }
                }
                RefUIData("结论");
                MessageAdd("检定完成", EnumLogType.提示信息);
            }
            catch (Exception ex)
            {
                for (int i = 0; i < MeterNumber; i++)
                {
                    if (!MeterInfo[i].YaoJianYn) continue;
                    ResultDictionary["结论"][i] = "不合格";
                }
                RefUIData("结论");
                MessageAdd("蓝牙预处理检定异常" + ex.ToString(), EnumLogType.错误信息);
            }
        }

        /// <summary>
        /// 检定参数是否合法
        /// </summary>
        /// <returns></returns>
        protected override bool CheckPara()
        {
            ResultNames = new string[] { "模块复位", "蓝牙连接", "蓝牙预处理", "预处理状态", "结论" };
            //电流开路状态|485通讯状态|状态字|时钟脉冲状态|电能脉冲状态
            return true;
        }
    }
}
