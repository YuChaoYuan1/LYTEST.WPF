using LYTest.ViewModel.CheckController;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LYTest.Verify
{
    public class Test : VerifyBase
    {
        //public static string str { get; set; }
        public override void Verify()
        {
            base.Verify();

            string[] str = new string[12];
            RefUIData("检定节点编号", "列名", str);//str数值
            MessageAdd("获得结论", EnumLogType.流程信息); //提示信息
            MessageAdd("获得结论", EnumLogType.提示信息); //提示信息--显示在最下面的，临时查看
            if (Stop) return;

            MessageAdd("123", EnumLogType.流程信息);

            //MessageAdd($1, EnumLogType.流程信息);


            //MessageAdd($1, EnumLogType.流程信息, EnumLogType.流程信息);
            //MessageAdd\((.*)true\)
            //MessageAdd\((.*)\);
            //MessageAdd($1,EnumLogType.流程信息);
        }

        /// <summary>
        /// 检定参数是否合法
        /// </summary>
        /// <returns></returns>
        protected override bool CheckPara()
        {
            ResultNames = new string[] { "1", "2", "3"};
            return true;
        }
    }
}
