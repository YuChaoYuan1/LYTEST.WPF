using LYTest.ViewModel.CheckController;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LYTest.Verify.ElectricalTest
{
    /// <summary>
    /// 工频耐压试验
    /// </summary>
    public class InsulationVerify : VerifyBase
    {

        #region 属性

        /// <summary>
        /// 耐压类型名称
        /// </summary>
        string TestName = "";
        /// <summary>
        /// 耐压电压值
        /// </summary>
        int NY_U = 2000;
        /// <summary>
        /// 耐压时间
        /// </summary>
        int Time = 60;
        /// <summary>
        /// 耐压仪漏电流
        /// </summary>
        int NY_I = 30;
        /// <summary>
        /// 测试板漏电流
        /// </summary>
        int CS_I = 30;
        /// <summary>
        /// 耐压仪上升时间
        /// </summary>
        int NY_UpTime = 10;
        /// <summary>
        /// 耐压仪下降时间
        /// </summary>
        int NY_DownTime = 10;

        #endregion


        public override void Verify()
        {
            //add yjt 20220305 新增日志提示
            MessageAdd("工频耐压试验检定开始...", EnumLogType.流程信息);

            base.Verify();

            for (int i = 0; i < MeterNumber; i++)
            {
                if (!MeterInfo[i].YaoJianYn) continue;
                ResultDictionary["检定项目名称"][i] = TestName;
                ResultDictionary["结论"][i] = "合格";
            }
            RefUIData("检定项目名称");
            RefUIData("结论");

            //DeviceControl.Ainuo_Start(0x01);


            //等待，读取测试板数据


            //DeviceControl.Ainuo_Start(0x02);

            //MessageAdd("获得结论", false); //提示信息
            //MessageAdd("获得结论", EnumLogType.提示信息); //提示信息--显示在最下面的，临时查看
            if (Stop) return;

            //add yjt 20220305 新增日志提示
            MessageAdd("检定完成...", EnumLogType.提示信息);
            MessageAdd("工频耐压试验检定结束...", EnumLogType.流程信息);
        }

        /// <summary>
        /// 检定参数是否合法
        /// </summary>
        /// <returns></returns>
        protected override bool CheckPara()
        {
            string[] tem = Test_Value.Split('|');
            TestName = tem[0];
            int.TryParse(tem[1], out NY_U);
            int.TryParse(tem[2], out Time);
            int.TryParse(tem[3], out NY_I);
            int.TryParse(tem[4], out NY_UpTime);
            int.TryParse(tem[5], out NY_DownTime);
            int.TryParse(tem[6], out CS_I);
            ResultNames = new string[] { "检定项目名称", "漏电流", "标准值", "结论" };
            return true;
        }
    }
}
