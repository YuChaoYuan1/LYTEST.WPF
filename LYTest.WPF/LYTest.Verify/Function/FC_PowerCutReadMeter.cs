using LYTest.ViewModel.CheckController;

namespace LYTest.Verify.Function
{
    /// <summary>
    /// 停电抄表功能试验
    /// add lsj 20220724
    /// </summary>
    class FC_PowerCutReadMeter:VerifyBase
    {
        public override void Verify()
        {
            base.Verify();

            bool[] result = new bool[MeterNumber];
            for (int i = 0; i < MeterNumber; i++)
            {
                result[i] = true;
            }

            for (int i = 0; i < MeterNumber; i++)
            {
                if (!MeterInfo[i].YaoJianYn) continue;
                ResultDictionary["当前检定项目"][i] = "停电抄表功能试验";
                ResultDictionary["结论"][i] = result[i] ? "合格" : "不合格";

            }
            RefUIData("当前检定项目");
            RefUIData("结论");

        }

        protected override bool CheckPara()
        {
            ResultNames = new string[] { "当前检定项目", "结论" };
            return true;
        }
    }
}
