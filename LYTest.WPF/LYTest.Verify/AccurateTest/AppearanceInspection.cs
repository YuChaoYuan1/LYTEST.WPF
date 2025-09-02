//using LYTest.Verify.Helper;
using LYTest.Core;
using LYTest.ViewModel.CheckController;

namespace LYTest.Verify.AccurateTest
{
    //add zjl  20220216 外观检测功能
    /// <summary>
    /// 外观检查
    /// 
    /// </summary>
    class AppearanceInspection : VerifyBase
    {
         
        public override void Verify()
        {
            //add yjt 20220305 新增日志提示
            MessageAdd("外观检查试验检定开始...", EnumLogType.流程信息);

            MessageAdd("正在外观检测试验...", EnumLogType.提示信息);
            for (int Num = 0; Num < MeterNumber; Num++)
            {
                if (!MeterInfo[Num].YaoJianYn) continue;

                //add yjt 20220320 新增演示模式数据和结论
                ResultDictionary["当前项目"][Num] = "外观检查";

                ResultDictionary["检定信息"][Num] = "外观检查";
                ResultDictionary["不合格原因"][Num] = "";
                ResultDictionary["结论"][Num] = ConstHelper.合格;
            }
            RefUIData("当前项目");
            RefUIData("检定信息");
            RefUIData("不合格原因");
            RefUIData("结论");


            //add yjt 20220305 新增日志提示
            MessageAdd("检定结束", EnumLogType.提示信息);
            MessageAdd("外观检查试验检定结束...", EnumLogType.流程信息);
        }
        /// <summary>
        /// 检定参数是否合法
        /// </summary>
        /// <returns></returns>
        protected override bool CheckPara()
        {

            ResultNames = new string[] { "当前项目","检定信息", "不合格原因", "结论" };
            return true;
        }
    }
}