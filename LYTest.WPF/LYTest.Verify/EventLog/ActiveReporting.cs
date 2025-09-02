using LYTest.ViewModel.CheckController;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LYTest.MeterProtocol.Enum;

namespace LYTest.Verify.EventLog
{
    /// <summary>
    /// 事件主动上报
    /// </summary>
    public class ActiveReporting : VerifyBase
    {
        public override void Verify()
        {
            base.Verify();
            if (!PowerOn())
            {
                MessageAdd("升电压失败! ", EnumLogType.提示信息);
                ErrorRefResoult();
                return;
            }
            WaitTime("升源", 5);
            if (Stop) return;
            ReadMeterAddrAndNo();
            Identity(false);

            SwitchChannel(Cus_ChannelType.通讯载波);//先组网


            //设置数据的时候需要485发送
            SwitchChannel(Cus_ChannelType.通讯485);
            MessageAdd("正在开始电表允许上报", EnumLogType.提示与流程信息);
            //1:修改4300允许主动上报
            MeterProtocolAdapter.Instance.WriteData("允许主动上报", "1");
            MessageAdd("正在设置上报方式为载波上报", EnumLogType.提示与流程信息);
            //2：设置4300，上报通道数为1
            MeterProtocolAdapter.Instance.WriteData("上报通道", "9");
            MessageAdd("正在设置校时事件的上报方式为主动上报", EnumLogType.提示与流程信息);
            //3:对应事件的上报方式
            MeterProtocolAdapter.Instance.WriteData("校时上报方式", "0");
            MessageAdd("正在设置校时事件发生时上报", EnumLogType.提示与流程信息);
            //4: 开启对应事件的上报标识
            MeterProtocolAdapter.Instance.WriteData("校时上报标识", "1");

            for (int i = 0; i < MeterNumber; i++)
            {
                if (!MeterInfo[i].YaoJianYn) continue;
                MessageAdd($"正在第【{i+1}】表位对时", EnumLogType.提示与流程信息);
                MeterProtocolAdapter.Instance.SetDataReceived(true);
                //5：对时
                DateTime readTime = DateTime.Now;  //读取GPS时间
                if (Stop) return;
                MeterProtocolAdapter.Instance.WriteDateTime(readTime, i);
                List<object> oad = new List<object>();
                //这里需要读取载波的数据了
                MessageAdd($"正在等待【{i+1}】表位主动上报", EnumLogType.提示与流程信息);
                while (true)//如果监听到了数据
                {
                    System.Threading.Thread.Sleep(500);
                    if (TimeSubms(DateTime.Now, readTime) > 60000) //1分钟没上报就推出
                    {
                        MessageAdd("超出最大处理时间,正在退出...", EnumLogType.提示信息);
                        break;
                    }
                  var  temoad = MeterProtocolAdapter.Instance.GetOADList();
                    if (temoad != null && temoad.Count > 0)
                    {
                        for (int index = 0; index < temoad.Count; index++)
                        {
                            oad.Add(temoad[index]);
                        }
                        break;
                    }
                }
                MeterProtocolAdapter.Instance.SetDataReceived(false);


                if (oad != null && oad.Count > 0 && oad.Contains("30160200"))
                {
                    ResultDictionary["结论"][i] = "合格";
                    ResultDictionary["上报事件对象标识符"][i] = "30160200";
                    ResultDictionary["上报事件对象名称"][i] = "电能表校时事件";
                }
                else
                {
                    ResultDictionary["结论"][i] = "不合格";
                }
                RefUIData("上报事件对象标识符");
                RefUIData("上报事件对象名称");
                RefUIData("结论");
            }
            MeterProtocolAdapter.Instance.SetDataReceived(true);
            //WaitTime("", 100);
            //关闭上报
            MessageAdd("正在关闭主动上报", EnumLogType.提示与流程信息);
            MeterProtocolAdapter.Instance.WriteData("允许主动上报", "0");

            //MessageAdd("正在关闭校时事件发生时上报", EnumLogType.提示与流程信息);
            //MeterProtocolAdapter.Instance.WriteData("校时上报标识", "0");


        }

        /// <summary>
        /// 检定参数是否合法
        /// </summary>
        /// <returns></returns>
        protected override bool CheckPara()
        {
            ResultNames = new string[] { "上报事件对象标识符", "上报事件对象名称", "结论" };
            return true;
        }
    }
}
