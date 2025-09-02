using LYTest.DAL;
using LYTest.ViewModel.Model;
using System.Collections.Generic;
using System.Linq;

namespace LYTest.ViewModel.Schema
{
    /// <summary>
    /// 检定点参数信息视图模型
    /// </summary>
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

        /// <summary>
        /// 检定点大类名称
        /// </summary>
        public string CategoryName
        {
            get => GetProperty("");
            set => SetProperty(value);
        }

        /// <summary>
        /// 检定点名称
        /// </summary>
        public string ItemName
        {
            get => GetProperty("");
            set => SetProperty(value);
        }

        /// <summary>
        /// 检定点名称是否包含项目名称
        /// </summary>
        public bool ContainProjectName
        {
            get => GetProperty(true);
            set => SetProperty(value);
        }

        /// <summary>
        /// 当前要配置的检定参数
        /// </summary>
        public CheckParaViewModel CheckParaCurrent
        {
            get => GetProperty<CheckParaViewModel>();
            set => SetProperty(value);

        }

        /// <summary>
        /// 检定参数列表
        /// </summary>
        public AsyncObservableCollection<CheckParaViewModel> CheckParas { get; } = new AsyncObservableCollection<CheckParaViewModel>();

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
            ContainProjectName = strTemp == null || strTemp.Trim() != "0";

            for (int i = 0; i < arrayDisplayName.Length; i++)
            {
                if (string.IsNullOrEmpty(arrayDisplayName[0]))
                {
                    continue;
                }
                CheckParaViewModel checkPara = new CheckParaViewModel
                {
                    ParaDisplayName = arrayDisplayName[i]
                };
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
                    if (bool.TryParse(arrayKeyRule[i], out bool b))
                    {
                        checkPara.IsKeyMember = b;
                    }
                }
                if (arrayNameRule.Length > i)
                {
                    if (bool.TryParse(arrayNameRule[i], out bool b))
                    {
                        checkPara.IsNameMember = b;
                    }
                }
                if (arrayDefaultValue.Length > i)
                {
                    checkPara.DefaultValue = arrayDefaultValue[i];
                }
                CheckParas.Add(checkPara);
            }
            LoadFlag = true;
        }

        public bool LoadFlag
        {
            get => GetProperty(false);
            set => SetProperty(value);
        }

        /// <summary>
        /// 检定点对应的方法名称
        /// </summary>
        public string ClassName
        {
            get => GetProperty("");
            set => SetProperty(value);
        }

        #region 增删改查操作
        //列名称:PARA_NO,PARA_P_CODE,PARA_VIEW,PARA_CATEGORY_NO,PARA_ITEM_NO,PARA_NAME,PARA_KEY_RULE
        /// <summary>
        /// 获取当前的模型
        /// </summary>
        /// <returns></returns>
        private DynamicModel GetCurrentModel()
        {
            DynamicModel model = new DynamicModel();
            if (CheckParas.Count == 0)
            {
                model.SetProperty("PARA_NO", ParaNo);
                model.SetProperty("PARA_P_CODE", null);
                model.SetProperty("PARA_VIEW", null);
                model.SetProperty("PARA_NAME", ItemName);
                model.SetProperty("PARA_KEY_RULE", null);
                model.SetProperty("PARA_VIEW_RULE", null);
                model.SetProperty("DEFAULT_VALUE", null);
                model.SetProperty("CHECK_CLASS_NAME", ClassName);
                model.SetProperty("PARA_NAME_RULE", "1");
                return model;
            }
            IEnumerable<string> displayNames = from item in CheckParas select item.ParaDisplayName;
            IEnumerable<string> enumTypes = from item in CheckParas select CodeDictionary.GetCodeLayer1(item.CodeName);
            IEnumerable<bool> keyRules = from item in CheckParas select item.IsKeyMember;
            IEnumerable<bool> nameRules = from item in CheckParas select item.IsNameMember;
            IEnumerable<string> defaultValues = from item in CheckParas select item.DefaultValue;
            string itemNo = "";
            if (ParaNo.Length == 5)
            {
                itemNo = paraNo.Substring(2, 3);
            }
            model.SetProperty("PARA_NO", ParaNo);
            model.SetProperty("PARA_P_CODE", string.Join("|", enumTypes));
            model.SetProperty("PARA_VIEW", string.Join("|", displayNames));
            model.SetProperty("PARA_NAME", ItemName);
            model.SetProperty("PARA_KEY_RULE", string.Join("|", keyRules));
            model.SetProperty("PARA_VIEW_RULE", string.Join("|", nameRules));
            model.SetProperty("DEFAULT_VALUE", string.Join("|", defaultValues));
            model.SetProperty("CHECK_CLASS_NAME", ClassName);
            model.SetProperty("PARA_NAME_RULE", ContainProjectName ? "1" : "0");
            return model;
        }
        /// <summary>
        /// 保存参数配置
        /// </summary>
        public void SaveParaInfo()
        {
            if (paraNo == null || paraNo == "") return;
            DynamicModel model = GetCurrentModel();
            string where = string.Format("PARA_NO='{0}'", ParaNo);
            if (DALManager.ApplicationDbDal.GetCount(EnumAppDbTable.T_SCHEMA_PARA_FORMAT.ToString(), where) > 0)
            {
                DALManager.ApplicationDbDal.Update(EnumAppDbTable.T_SCHEMA_PARA_FORMAT.ToString(), where, model, new List<string> { "PARA_P_CODE", "PARA_VIEW", "PARA_NAME", "PARA_KEY_RULE", "PARA_VIEW_RULE", "DEFAULT_VALUE", "CHECK_CLASS_NAME", "PARA_NAME_RULE" });
            }
            else
            {
                DALManager.ApplicationDbDal.Insert(EnumAppDbTable.T_SCHEMA_PARA_FORMAT.ToString(), model);
            }
            LoadParaInfo();
        }
        public void AddNewCheckPara()
        {
            CheckParas.Add(new CheckParaViewModel());
        }
        /// <summary>
        /// 删除当前参数
        /// </summary>
        public void DeleteCheckPara()
        {
            if (CheckParaCurrent == null) return;
            for (int i = 0; i < CheckParas.Count; i++)
            {
                if (CheckParas[i].ParaDisplayName == CheckParaCurrent.ParaDisplayName)
                {
                    CheckParaCurrent = null;
                    CheckParas.RemoveAt(i);
                    break;
                }
            }
        }
        /// <summary>
        /// 当前参数项上移
        /// </summary>
        public void MoveUpCheckPara()
        {
            if (CheckParaCurrent == null) return;
            for (int i = 0; i < CheckParas.Count; i++)
            {
                if (CheckParas[i] == CheckParaCurrent && i > 0)
                {
                    CheckParas.Move(i, i - 1);
                    break;
                }
            }
        }
        /// <summary>
        /// 当前参数项下移
        /// </summary>
        public void MoveDownCheckPara()
        {
            if (CheckParaCurrent == null) return;
            for (int i = 0; i < CheckParas.Count; i++)
            {
                if (CheckParas[i] == CheckParaCurrent && i < CheckParas.Count - 1)
                {
                    CheckParas.Move(i, i + 1);
                    break;
                }
            }
        }
        #endregion
    }
}
