using LYTest.ViewModel.CheckController;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LYTest.MeterProtocol.Protocols.DLT698.Enum;

namespace LYTest.Verify.EventLog
{
    public class TimeErr : VerifyBase
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
            if (Stop) return;
            ReadMeterAddrAndNo();
            //Identity(false);
            //读取触发前次数
            string[] QCount = ReadEventLogInfo("事件");

            //DateTime readTime = DateTime.Now.AddDays(3000);  //读取GPS时间
            //if (Stop) return;
            //MeterProtocolAdapter.Instance.WriteDateTime(readTime);


            //string[] HCount = ReadEventLogInfo("事件产生后");
            for (int i = 0; i < MeterNumber; i++)
            {
                if (!MeterInfo[i].YaoJianYn) continue;
                if (!string.IsNullOrEmpty(QCount[i]))
                {
                    ResultDictionary["结论"][i] = "合格";
                }
                else
                { 
                    ResultDictionary["结论"][i] = "不合格";
                }

            }
            RefUIData("结论");
        }

        private string[] ReadEventLogInfo(string name)
        {
            //int[] eventCount = new int[MeterNumber];//次数
            string[] eventTimes = MeterProtocolAdapter.Instance.ReadData("时钟故障总次数");
            //List<string> oad = new List<string>() { "30110200" };
            //Dictionary<int, List<object>> dicObj = new Dictionary<int, List<object>>();
            //List<string> rcsd = new List<string>();
            //Dictionary<int, Dictionary<string, List<object>>> dic = MeterProtocolAdapter.Instance.ReadRecordData(oad, EmSecurityMode.ClearTextRand, EmGetRequestMode.GetRequestRecord, 1, rcsd, ref dicObj);


            for (int i = 0; i < MeterNumber; i++)
            {
                if (!MeterInfo[i].YaoJianYn) continue;
                //ResultDictionary[name + "事件发生时刻"][i] = timeStart[i];
                //ResultDictionary[name + "事件结束时刻"][i] = timeEnd[i];

                //if (!string.IsNullOrEmpty(eventTimes[i]))
                //{
                //    eventCount[i] = int.Parse(eventTimes[i]);
                //}
                ResultDictionary[name + "总次数"][i] = eventTimes[i].ToString();
            }
            RefUIData(name + "总次数");

            return eventTimes;
        }


        /// <summary>
        /// 检定参数是否合法
        /// </summary>
        /// <returns></returns>
        protected override bool CheckPara()
        {
            ResultNames = new string[] { "事件总次数",   "结论" };
            return true;
        }
    }
}
