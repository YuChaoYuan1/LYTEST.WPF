using LYTest.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LYTest.DataManager
{
    public class ParaInfoViewModel : ViewModelBase
    {
        private string paraNo;
        /// <summary>
        /// 参数项编号
        /// </summary>
        public string ParaNo
        {
            get { return paraNo; }
            set
            {
                SetPropertyValue(value, ref paraNo, "ParaNo");
                LoadParaInfo();
            }
        }

        private string categoryName;
        /// <summary>
        /// 检定点大类名称
        /// </summary>
        public string CategoryName
        {
            get { return categoryName; }
            set { SetPropertyValue(value, ref categoryName, "CategoryName"); }
        }

        private string itemName;
        /// <summary>
        /// 检定点名称
        /// </summary>
        public string ItemName
        {
            get { return itemName; }
            set { SetPropertyValue(value, ref itemName, "ItemName"); }
        }

        private bool containProjectName = true;
        /// <summary>
        /// 检定点名称是否包含项目名称
        /// </summary>
        public bool ContainProjectName
        {
            get { return containProjectName; }
            set { SetPropertyValue(value, ref containProjectName, "ContainProjectName"); }
        }

        private CheckParaViewModel checkParaCurrent;
        /// <summary>
        /// 当前要配置的检定参数
        /// </summary>
        public CheckParaViewModel CheckParaCurrent
        {
            get { return checkParaCurrent; }
            set
            {
                SetPropertyValue(value, ref checkParaCurrent, "CheckParaCurrent");
            }
        }


        private AsyncObservableCollection<CheckParaViewModel> checkParas = new AsyncObservableCollection<CheckParaViewModel>();
        /// <summary>
        /// 检定参数列表
        /// </summary>
        public AsyncObservableCollection<CheckParaViewModel> CheckParas
        {
            get { return checkParas; }
            set { SetPropertyValue(value, ref checkParas, "CheckParas"); }
        }
        /// <summary>
        /// 加载参数信息
        /// </summary>
        public void LoadParaInfo()
        {
            CheckParas.Clear();
            CategoryName = SchemaFramework.GetItemName(ParaNo);
            ItemName = SchemaFramework.GetItemName(ParaNo);
            ClassName = "";
            DynamicModel model = DALManager.ApplicationDbDal.GetByID(EnumAppDbTable.T_SCHEMA_PARA_FORMAT.ToString(), string.Format("PARA_NO='{0}'", paraNo));
            if (model == null) return;
            ClassName = model.GetProperty("CHECK_CLASS_NAME") as string;
            string displayNames = model.GetProperty("PARA_VIEW") as string;
            if (displayNames == null)
            {
                LoadFlag = true;
                return;
            }
            string[] arrayDisplayName = displayNames.Split('|');
            string enumTypes = model.GetProperty("PARA_P_CODE") as string;
            enumTypes = enumTypes ?? "";
            string[] arrayEnumType = enumTypes.Split('|');
            string keyRules = model.GetProperty("PARA_KEY_RULE") as string;
            keyRules = keyRules ?? "";
            string[] arrayKeyRule = keyRules.Split('|');
            string nameRules = model.GetProperty("PARA_VIEW_RULE") as string;
            if (nameRules == null)
            {
                nameRules = "";
            }
            string[] arrayNameRule = nameRules.Split('|');
            string defaultValues = model.GetProperty("DEFAULT_VALUE") as string;
            defaultValues = defaultValues ?? "";
            string[] arrayDefaultValue = defaultValues.Split('|');

            string strTemp = model.GetProperty("PARA_NAME_RULE") as string;
            ContainProjectName = (strTemp != null && strTemp.Trim() == "0") ? false : true;

            for (int i = 0; i < arrayDisplayName.Length; i++)
            {
                if (string.IsNullOrEmpty(arrayDisplayName[0]))
                {
                    continue;
                }
                CheckParaViewModel checkPara = new CheckParaViewModel();
                checkPara.ParaDisplayName = arrayDisplayName[i];
                if (arrayEnumType.Length > i)
                {
                    checkPara.ParaEnumType = arrayEnumType[i];
                    if ((ParaNo == "04023" || ParaNo == "14002") && i == 0)    //通讯协议检查
                    {
                        checkPara.ParaEnumType = "DataFlagNames";
                        checkPara.CodeName = "数据标识列表";
                    }
                    else
                    {
                        checkPara.CodeName = CodeDictionary.GetNameLayer1(checkPara.ParaEnumType);
                    }
                }
                if (arrayKeyRule.Length > i)
                {
                    bool.TryParse(arrayKeyRule[i], out bool boolTemp);
                    checkPara.IsKeyMember = boolTemp;
                }
                if (arrayNameRule.Length > i)
                {
                    bool.TryParse(arrayNameRule[i], out bool boolTemp);
                    checkPara.IsNameMember = boolTemp;
                }
                if (arrayDefaultValue.Length > i)
                {
                    checkPara.DefaultValue = arrayDefaultValue[i];
                }
                CheckParas.Add(checkPara);
            }
            LoadFlag = true;
        }

        private bool loadFlag;

        public bool LoadFlag
        {
            get { return loadFlag; }
            set { SetPropertyValue(value, ref loadFlag, "LoadFlag"); }
        }

        private string className;
        /// <summary>
        /// 检定点对应的方法名称
        /// </summary>
        public string ClassName
        {
            get { return className; }
            set { SetPropertyValue(value, ref className, "ClassName"); }
        }

    }
}
