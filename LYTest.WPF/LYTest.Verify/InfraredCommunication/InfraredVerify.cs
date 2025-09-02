using LYTest.Core.Enum;
using LYTest.ViewModel.CheckController;
using LYTest.MeterProtocol;

namespace LYTest.Verify.InfraredCommunication
{
    ///add lsj 20220218 红外通讯检定
    /// <summary>
    /// 功能描述：红外通讯检定
    public class InfraredVerify : VerifyBase
    {
        private readonly int _VerifyNum = 1;
        private readonly bool Is485 = false;

        /// <summary>
        /// 红外通讯实验
        /// </summary>
        public override void Verify()
        {
            string aa = Test_Value;
            string[] bb = aa.Split('|');
            base.Verify();
            for (int i = 0; i < MeterNumber; i++)
            {
                if (!MeterInfo[i].YaoJianYn) continue;
                ResultDictionary["项目名称"][i] = "红外通讯";
            }
            RefUIData("项目名称");
            MessageAdd("正在升电压...", EnumLogType.提示信息);
            PowerOn(OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, OneMeterInfo.MD_UB, 0, 0, 0, Core.Enum.Cus_PowerYuanJian.H, Core.Enum.PowerWay.正向有功, "1.0");
            //【红外数据比对检定】
            InfraredCheck();
            if (Is485) //DOTO是否进行485数据比对
            {
                MessageAdd("正在进行485数据读取...", EnumLogType.提示信息);
                string[] rev485D = MeterProtocolAdapter.Instance.ReadData(bb[0]);

                MessageAdd("正在进行红外数据比对试验...", EnumLogType.提示信息);
                for (int i = 0; i < MeterNumber; i++)
                {
                    if (Stop) break;
                    if (!MeterInfo[i].YaoJianYn) continue;

                    ResultDictionary["485抄读数据"][i] = rev485D[i].ToString();

                    if (ResultDictionary["红外抄读数据"][i] != ResultDictionary["485抄读数据"][i])
                        ResultDictionary["结论"][i] = "不合格";
                    else
                        ResultDictionary["结论"][i] = "合格";
                }
            }
            RefUIData("485抄读数据");
            RefUIData("红外抄读数据");
            RefUIData("结论");
            //【消息处理】
            MessageAdd("红外数据比对试验检定完毕。", EnumLogType.提示信息);
        }
        /// <summary>
        /// 红外数据比对试验检定
        /// </summary>
        private bool InfraredCheck()
        {
            string aa = Test_Value;
            string[] bb = aa.Split('|');
            App.g_ChannelType = LYTest.MeterProtocol.Enum.Cus_ChannelType.通讯红外;

            MessageAdd("正在切换红外通讯模式...", EnumLogType.提示信息);
            //SetSelectLightStatus(Cus_LightSelect.一对一模式红外通讯);

            MessageAdd("正在进行红外通讯...", EnumLogType.提示信息);

            string[] readData = new string[MeterNumber];
            for (int i = 0; i < MeterNumber; i++)
            {
                if (Stop) break;
                if (!MeterInfo[i].YaoJianYn) continue;
                MessageAdd("正在红外读取第" + (i + 1) + "表位...", EnumLogType.提示信息);
                for (int j = 0; j < _VerifyNum; j++)
                {
                    //【读召测数据】
                    readData[i] = MeterProtocolAdapter.Instance.ReadDataPos(bb[1], i);
                    if (!string.IsNullOrEmpty(readData[i]))
                        break;
                }
                if (string.IsNullOrEmpty(readData[i]))
                {
                    ResultDictionary["红外抄读数据"][i] = "";
                    ResultDictionary["结论"][i] = "不合格";
                }
                else
                {
                    ResultDictionary["红外抄读数据"][i] = readData[i];
                    ResultDictionary["结论"][i] = "合格";
                }
            }
            MessageAdd("正在切换485通讯模式...", EnumLogType.提示信息);
            //SetSelectLightStatus(Cus_LightSelect.一对一模式485通讯);
            App.g_ChannelType = LYTest.MeterProtocol.Enum.Cus_ChannelType.通讯485;

            return true;
        }

        //private void SetSelectLightStatus(Cus_LightSelect 一对一模式红外通讯)
        //{
        //}

        /// <summary>
        /// 检定参数是否合法
        /// </summary>
        /// <returns></returns>
        protected override bool CheckPara()
        {
            ResultNames = new string[] { "项目名称", "红外抄读数据", "485抄读数据", "结论" };
            return true;
        }


    }
}
