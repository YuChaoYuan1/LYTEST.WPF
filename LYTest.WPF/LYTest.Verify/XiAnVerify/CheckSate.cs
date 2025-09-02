using LYTest.Core.Struct;
using LYTest.ViewModel.CheckController;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LYTest.Verify.XiAnVerify
{
    public  class CheckSate : VerifyBase
    {

        string ItemId = string.Empty;

        //string DataName = ""; //数据项名称
        //string BsCode = ""; //标识编码
        //int Len = 0; //长度
        //int DecimalLen = 0; // 小数位数
        //string DataFormat = ""; //数据格式
        //string DXGN = "读"; //功能读还是写
        //string TipsStr = ""; //写入示例
        /// <summary>
        /// 重写方案转换
        /// </summary>
        private StPlan_ConnProtocol CurPlan;

        public override void Verify()
        {
            base.Verify();
            for (int i = 0; i < MeterNumber; i++)
            {
                //if (!meterInfo[i].YaoJianYn) continue;
                ResultDictionary["当前项目"][i] = CurPlan.Name;
                ResultDictionary["项目编号"][i] = CurPlan.PrjID;
                ResultDictionary["设定值"][i] = CurPlan.WriteContent;
                ResultDictionary["数据标识"][i] = CurPlan.Code698;
                ResultDictionary["子项目编号"][i] = CurPlan.ChildrenItemId;
            }

            RefUIData("当前项目");
            RefUIData("项目编号");
            RefUIData("设定值");
            RefUIData("数据标识");
            RefUIData("子项目编号");
        }

        /// <summary>
        /// 检定参数是否合法
        /// </summary>
        /// <returns></returns>
        protected override bool CheckPara()
        {
            string[] tem = Test_Value.Split('|');
            if (tem.Length != 12) return false;
            CurPlan = new StPlan_ConnProtocol();
            int t = 0;


            CurPlan.Name = tem[0];            //数据项名称
            if (CurPlan.Name == "第一套时区表")
            {
                CurPlan.Name = "第一套时区表数据";//处理名称不规范部分
            }
            //CurPlan.Code645 = tem[1];          //标识编码645
            CurPlan.Code698 = tem[1];          //标识编码698

            int.TryParse(tem[2], out t);
            CurPlan.DataLen = t;      //长度
            t = 0;
            int.TryParse(tem[3], out t);
            CurPlan.PointIndex = t;          // 小数位数
            CurPlan.StrDataType = tem[4];    //数据格式
            CurPlan.OperType = tem[5];       //功能读还是写
            CurPlan.WriteContent = tem[6];
            if (CurPlan.Code645 == "")
            {
                return false;
            }

            CurPlan.PrjID = tem[7];//项目编号

            CurPlan.ChildrenItemId = tem[11];//子项目编号


            ResultNames = new string[] { "当前项目", "结论", "项目编号", "数据标识", "设定值", "读取值", "子项目编号" };

            
            return true;
        }
    }
}
