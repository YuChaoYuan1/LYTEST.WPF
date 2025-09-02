using LYTest.Core.Enum;
using LYTest.ViewModel.CheckController;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LYTest.MeterProtocol.Enum;
using LYTest.MeterProtocol.Protocols.DLT698.Enum;

namespace LYTest.Verify.EventLog
{
    /// <summary>
    /// 事件跟随上报
    /// </summary>
    public class FollowAndReport : VerifyBase
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


            //SwitchChannel(Cus_ChannelType.通讯载波);


            //设置数据的时候需要485发送
            //SwitchChannel(Cus_ChannelType.通讯485);
            MessageAdd("正在开始电表允许跟随上报", EnumLogType.提示与流程信息);
            //1:修改4300允许主动上报
            MeterProtocolAdapter.Instance.WriteData("允许跟随上报", "1");
            MessageAdd("正在设置上报方式为485上报", EnumLogType.提示与流程信息);
            //2：设置4300，上报通道数为1
            MeterProtocolAdapter.Instance.WriteData("上报通道", "1");
            MessageAdd("正在设置校时事件的上报方式为主动上报", EnumLogType.提示与流程信息);
            //3:对应事件的上报方式
            MeterProtocolAdapter.Instance.WriteData("校时上报方式", "1");
            MessageAdd("正在设置校时事件发生时上报", EnumLogType.提示与流程信息);
            //4: 开启对应事件的上报标识
            MeterProtocolAdapter.Instance.WriteData("校时上报标识", "1");


                MessageAdd($"正在表位对时", EnumLogType.提示与流程信息);
                //5：对时
                DateTime readTime = DateTime.Now;  //读取GPS时间
                if (Stop) return;
                MeterProtocolAdapter.Instance.WriteDateTime(readTime);
            MessageAdd($"正在读取数据", EnumLogType.提示与流程信息);

            List<string> LstOad2 = new List<string>
                {
                    "40010200", //通信地址
                };
                Dictionary<int, object[]> DicObj2 = MeterProtocolAdapter.Instance.ReadData(LstOad2, EmSecurityMode.ClearText, EmGetRequestMode.GetRequestNormalList);

            for (int i = 0; i < MeterNumber; i++)
            {
                if (!MeterInfo[i].YaoJianYn) continue;
                ResultDictionary["结论"][i] = "不合格";
                if (DicObj2[i]!=null)
                {
                    for (int j = 0; j < DicObj2[i].Length; j++)
                    {
                        if (Array.IndexOf(DicObj2[i],"30160200")>=0)
                        {
                            ResultDictionary["结论"][i] = "合格";
                            ResultDictionary["上报事件对象标识符"][i] = "30160200";
                            ResultDictionary["上报事件对象名称"][i] = "电能表校时事件";
                            break;
                        }
                    }
                }
             
            }
            MessageAdd("正在恢复上报方式为载波上报", EnumLogType.提示与流程信息);
            //2：设置4300，上报通道数为1
            MeterProtocolAdapter.Instance.WriteData("上报通道", "9");
            //关闭上报
            MessageAdd("正在开始关闭跟随上报", EnumLogType.提示与流程信息);
            MeterProtocolAdapter.Instance.WriteData("允许跟随上报", "0");
            RefUIData("上报事件对象标识符");
            RefUIData("上报事件对象名称");
            RefUIData("结论");
            return;

            //    MessageAdd("正在对时", EnumLogType.提示与流程信息);
            ////5：对时
            //DateTime readTime = DateTime.Now;  //读取GPS时间
            //if (Stop) return;
            //MeterProtocolAdapter.Instance.WriteDateTime(readTime);

            //这里需要读取载波的数据了





            //MessageAdd("正在关闭校时事件发生时上报", EnumLogType.提示与流程信息);
            //MeterProtocolAdapter.Instance.WriteData("校时上报标识", "0");





            //return;
            //以下属于调试部分
            //5:

            ////1:修改4300允许主动上报
            //MeterProtocolAdapter.Instance.WriteData("允许主动上报", "1");
            ////2：设置4300，上报通道数为1
            //MeterProtocolAdapter.Instance.WriteData("上报通道", "9");
            ////3:对应事件的上报方式
            //MeterProtocolAdapter.Instance.WriteData("校时上报方式", "0");

            ////4: 开启对应事件的上报标识
            //MeterProtocolAdapter.Instance.WriteData("校时上报标识", "1");

            ////5：对时
            // //readTime = DateTime.Now.AddDays(-2);  //读取GPS时间
            //if (Stop) return;
            ////MeterProtocolAdapter.Instance.WriteDateTime(readTime);


            //SwitchChannel(Cus_ChannelType.通讯485);

            //MeterProtocolAdapter.Instance.WriteData("允许跟随上报", "1");

            //MeterProtocolAdapter.Instance.WriteData("跟随上报状态字", "FFFFFFFF");

            //List<string> LstOad = new List<string>
            //{
            //    "20150500",
            //    "20150400",
            //    "43000A00",
            //    "202f0200",
            //    "43000800",
            //    "43000700",
            //    "30160400",
            //    "30160800",
            //    "30160900",


            //    "302e0600",
            //    "302e0700",
            //    "302e0400",
            //    "302e0500",
            //    "302e0200",

            //    "30160b00",
            //    "30160800",
            //    "30160B00"
            //};




            //MeterProtocolAdapter.Instance.WriteData("校时上报", "1");





            //SendCostCommand(Cus_EncryptionTrialType.远程拉闸);

            //string[] strReadData = MeterProtocolAdapter.Instance.ReadData("电表运行状态字3");


            //SendCostCommand(Cus_EncryptionTrialType.远程合闸);
            //Dictionary<int, object[]> DicObj = MeterProtocolAdapter.Instance.ReadData(LstOad, EmSecurityMode.ClearTextRand, EmGetRequestMode.GetRequestNormalList);


            //return;

            //MeterProtocolAdapter.Instance.WriteData("允许主动上报", "1");





            //string[] eventTimes = MeterProtocolAdapter.Instance.ReadData("校时总次数");
            ////DateTime readTime = DateTime.Now.AddDays(-1);  //读取GPS时间
            ////if (Stop) return;
            ////MeterProtocolAdapter.Instance.WriteDateTime(readTime);
            //List<string> oad = new List<string>() { "33000200" };
            //Dictionary<int, List<object>> dicObj = new Dictionary<int, List<object>>();
            //List<string> rcsd = new List<string>();
            //Dictionary<int, Dictionary<string, List<object>>> dic = MeterProtocolAdapter.Instance.ReadRecordData(oad, EmSecurityMode.ClearTextRand, EmGetRequestMode.GetRequestRecord, 0, rcsd, ref dicObj); 

            //允许跟随上报
            //允许主动上报
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

        /// <summary>
        /// 设置上报数据的方式
        /// </summary>
        internal void SetUpDataType()
        {
            MessageAdd("正在开始电表允许上报", EnumLogType.流程信息);
            //1:修改4300允许主动上报
            MeterProtocolAdapter.Instance.WriteData("允许主动上报", "1");
            MessageAdd("正在设置上报方式为载波上报", EnumLogType.流程信息);
            //2：设置4300，上报通道数为1
            MeterProtocolAdapter.Instance.WriteData("上报通道", "9");
            MessageAdd("正在设置校时事件的上报方式为主动上报", EnumLogType.流程信息);
            //3:对应事件的上报方式
            MeterProtocolAdapter.Instance.WriteData("校时上报方式", "0");
            MessageAdd("正在设置校时事件发生时上报", EnumLogType.流程信息);
            //4: 开启对应事件的上报标识
            MeterProtocolAdapter.Instance.WriteData("校时上报标识", "1");
        }

    }
}
