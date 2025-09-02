using LYTest.DAL;
using LYTest.DAL.DataBaseView;
using LYTest.Utility.Log;
using LYTest.ViewModel.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LYTest.ViewModel.CheckInfo
{
    public class DataBaseDisplayViewModel : ViewModelBase
    {

        #region 属性

        /// <summary>
        /// 检定点编号
        /// </summary>
        public string ParaNo { get; set; }
        private ViewIdClass viewIds = new ViewIdClass();

        public ViewIdClass ViewIds
        {
            get { return viewIds; }
            set { SetPropertyValue(value, ref viewIds, "ViewIds"); }
        }


        public FieldModelView fKField;
        /// <summary>
        /// 当前的辅助结论视图
        /// </summary>
        public FieldModelView FKField
        {
            get { return fKField; }
            set
            {
                SetPropertyValue(value, ref fKField, "FKField");
            }
        }


        public FieldModelView pKField;
        /// <summary>
        /// 当前的主结论视图
        /// </summary>
        public FieldModelView PKField
        {
            get { return pKField; }
            set
            {
                SetPropertyValue(value, ref pKField, "PKField");
            }
        }

        private ObservableCollection<string> fieldNames = new ObservableCollection<string>();
        /// <summary>
        /// 字段名称
        /// </summary>
        public ObservableCollection<string> FieldNames
        {
            get { return fieldNames; }
            set { SetPropertyValue(value, ref fieldNames, "FieldNames"); }
        }

        private string tableName;
        /// <summary>
        /// 当前的表
        /// </summary>
        public string TableName
        {
            get { return tableName; }
            set
            {
                SetPropertyValue(value, ref tableName, "TableName");
                LoadTableFields();
            }
        }
        private ObservableCollection<FieldModelView> fieldsPK = new ObservableCollection<FieldModelView>();
        /// <summary>
        /// 字段列表
        /// </summary>
        public ObservableCollection<FieldModelView> FieldsPK
        {
            get { return fieldsPK; }
            set { SetPropertyValue(value, ref fieldsPK, "FieldsPK"); }
        }


        /// <summary>
        /// 表名称
        /// </summary>
        public ObservableCollection<string> TableNames { get; set; }

        #endregion                      

        #region 视图移动
        /// <summary>
        /// 主结论视图往上移
        /// </summary>
        public void PKMoveUp()
        {
            if (PKField != null)
            {
                for (int i = 0; i < FieldsPK.Count; i++)
                {
                    if (FieldsPK[i] == PKField)
                    {
                        if (i == 0)
                        {
                            break;
                        }
                        else
                        {
                            FieldsPK.Move(i, i - 1);
                            break;
                        }
                    }
                }
            }
        }
        /// <summary>
        /// 主结论视图往下移
        /// </summary>
        public void PKMoveDown()
        {
            if (PKField != null)
            {
                for (int i = 0; i < FieldsPK.Count; i++)
                {
                    if (FieldsPK[i] == PKField)
                    {
                        if (i == FieldsPK.Count - 1)
                        {
                            break;
                        }
                        else
                        {
                            FieldsPK.Move(i, i + 1);
                            break;
                        }
                    }
                }
            }
        }

        public void DeletePK()
        {
            if (PKField != null)
            {
                for (int i = 0; i < FieldsPK.Count; i++)
                {
                    if (FieldsPK[i] == PKField)
                    {
                        FieldsPK.Remove(PKField);
                        break;
                    }
                }
            }
        }
        #endregion


        #region 载入数据
        /// <summary>
        /// 加载所有表的所有列名称
        /// </summary>
        private void LoadTableFields()
        {
            List<FieldModel> fieldModels = DALManager.MeterDbDal.GetFields(TableName);
            IEnumerable<string> list = from item in fieldModels select item.FieldName;
            FieldNames.Clear();
            for (int i = 0; i < list.Count(); i++)
            {
                FieldNames.Add(list.ElementAt(i));
            }
        }

        /// <summary>
        /// 添加新的主结论视图
        /// </summary>
        public void AddPKField()
        {
            FieldsPK.Add(new FieldModelView());
        }


         /// <summary>
         ///构造函数初始化
         /// </summary>
        public DataBaseDisplayViewModel()
        {
            TableNames = new ObservableCollection<string>(DALManager.MeterDbDal.GetTableNames());
        }


        /// <summary>
        /// 加载字段视图
        /// </summary>
        public void LoadFieldView()
        {
            FieldsPK.Clear();
            if (ViewIds.SelectedUnit == null)
            {
                return;
            }
            TableDisplayModel displayModel = ResultViewHelper.GetTableDisplayModel(ViewIds.SelectedUnit.ViewId, true);

            #region 主结论
            if (displayModel == null)
            {
                LogManager.AddMessage(string.Format(" {0} 的显示视图为空.", ViewIds.SelectedUnit.ViewId), EnumLogSource.用户操作日志, EnumLevel.Warning);
                return;
            }
            for (int i = 0; i < displayModel.ColumnModelList.Count; i++)
            {
                ColumnDisplayModel model = displayModel.ColumnModelList[i];
                FieldsPK.Add(new FieldModelView { ViewName = model.DisplayName, FieldName = model.Field });
            }
            TableName = displayModel.TableName;
            OnPropertyChanged("Fields");
            #endregion
        }
        #endregion



        #region 添加视图编号
        private string viewIdAdd;

        public string ViewIdAdd
        {
            get { return viewIdAdd; }
            set { SetPropertyValue(value, ref viewIdAdd, "ViewIdAdd"); }
        }
        private string viewNameAdd;

        public string ViewNameAdd
        {
            get { return viewNameAdd; }
            set { SetPropertyValue(value, ref viewNameAdd, "ViewNameAdd"); }
        }
        public void AddResultViewId()
        {
            #region 判断是否存在
            int intTemp = ViewIds.ViewUnits.Count(item => item.ViewId == ViewIdAdd);
            if (intTemp > 0)
            {
                LogManager.AddMessage(string.Format("视图编号:{0}已经使用,请修改编号后再添加.", ViewIdAdd), EnumLogSource.用户操作日志, EnumLevel.Tip);
                return;
            }
            intTemp = ViewIds.ViewUnits.Count(item => item.ViewName == ViewNameAdd);
            if (intTemp > 0)
            {
                LogManager.AddMessage(string.Format("视图名称:{0}已经使用,请修改名称后再添加.", ViewNameAdd), EnumLogSource.用户操作日志, EnumLevel.Tip);
                return;
            }
            #endregion
            CodeTree.CodeTreeNode nodeResultView = CodeTree.CodeTreeViewModel.Instance.GetCodeByEnName("MeterResultViewId", 1);
            if (nodeResultView != null)
            {
                nodeResultView.AddCode();
                CodeTree.CodeTreeNode nodeAdd = nodeResultView.Children[nodeResultView.Children.Count - 1];
                nodeAdd.CODE_TYPE = "";
                nodeAdd.CODE_VALUE = ViewIdAdd;
                nodeAdd.CODE_NAME = ViewNameAdd;
                nodeResultView.SaveCode();
                Dictionary<string, string> dictionaryTemp = CodeDictionary.GetLayer2("MeterResultViewId");
                dictionaryTemp.Add(ViewNameAdd, viewIdAdd);
                ViewIds.ViewUnits.Add(new ViewIdClass.ViewUnit()
                {
                    ViewId = ViewIdAdd,
                    ViewName = ViewNameAdd
                });
            }
        }
        #endregion

        /// <summary>
        /// 保存视图检定点
        /// </summary>
        public void SaveFieldView()
        {
            if (ViewIds.SelectedUnit == null)
            {
                return;
            }


            #region 更新结论视图
            DynamicModel viewModel = new DynamicModel();
            viewModel.SetProperty("AVR_VIEW_ID", ViewIds.SelectedUnit.ViewId);
            viewModel.SetProperty("AVR_CHECK_NAME", ViewIds.SelectedUnit.ViewName);
            viewModel.SetProperty("AVR_TABLE_NAME", TableName);
            string fieldsString = string.Empty;
            string displayString = string.Empty;
            //主结论项显示名称
            IEnumerable<string> stringTemp1 = from item in FieldsPK select item.FieldName;
            fieldsString = string.Join(",", stringTemp1);

            IEnumerable<string> stringTemp2 = from item in FieldsPK select item.ViewName;
            displayString = string.Join(",", stringTemp2);
            viewModel.SetProperty("AVR_COL_NAME", fieldsString);
            viewModel.SetProperty("AVR_COL_SHOW_NAME", displayString);
            string where = string.Format("AVR_VIEW_ID = '{0}'", ViewIds.SelectedUnit.ViewId);
            if (DALManager.ApplicationDbDal.GetCount(EnumAppDbTable.T_VIEW_CONFIG.ToString(), where) > 0)
            {
                LogManager.AddMessage(string.Format("更新 {0} 的显示视图.", ViewIds.SelectedUnit.ViewName), EnumLogSource.数据库存取日志, EnumLevel.Tip);
                DALManager.ApplicationDbDal.Update(EnumAppDbTable.T_VIEW_CONFIG.ToString(), where, viewModel, new List<string> { "AVR_TABLE_NAME", "AVR_COL_NAME", "AVR_COL_SHOW_NAME" });
            }
            else
            {
                LogManager.AddMessage(string.Format("插入 {0} 的显示视图.", ViewIds.SelectedUnit.ViewId), EnumLogSource.数据库存取日志, EnumLevel.Tip);
                DALManager.ApplicationDbDal.Insert(EnumAppDbTable.T_VIEW_CONFIG.ToString(), viewModel);
            }
            #endregion

            #region 更新检定点视图编号
            DynamicModel modelTemp = SchemaFramework.GetParaFormat(ParaNo);
            if (modelTemp != null)
            {
                modelTemp.SetProperty("RESULT_VIEW_ID", ViewIds.SelectedUnit.ViewId);
                int updateCount = DALManager.ApplicationDbDal.Update(EnumAppDbTable.T_SCHEMA_PARA_FORMAT.ToString(), string.Format("para_no='{0}'", ParaNo), modelTemp, new List<string> { "RESULT_VIEW_ID" });
                if (updateCount > 0)
                {
                    LogManager.AddMessage(string.Format("更新检定点 {0} 的显示视图成功.", modelTemp.GetProperty("PARA_NAME")), EnumLogSource.数据库存取日志, EnumLevel.Tip);
                }
                else
                {
                    LogManager.AddMessage(string.Format("更新检定点 {0} 的显示视图失败.", modelTemp.GetProperty("PARA_NAME")), EnumLogSource.数据库存取日志, EnumLevel.Warning);
                }
            }
            #endregion

        }




    }

    /// <summary>
    /// 结论视图编号类
    /// </summary>
    public class ViewIdClass : ViewModelBase
    {
        public ViewIdClass()
        {
            Dictionary<string, string> dictionaryTemp = CodeDictionary.GetLayer2("MeterResultViewId");
            ViewUnits.Clear();
            foreach (string key in dictionaryTemp.Keys)
            {
                ViewUnits.Add(new ViewUnit
                {
                    ViewId = dictionaryTemp[key],
                    ViewName = key
                });
            }
        }

        public class ViewUnit : ViewModelBase
        {
            private string viewId = "01";

            public string ViewId
            {
                get { return viewId; }
                set { SetPropertyValue(value, ref viewId, "ViewId"); }
            }
            private string viewName;

            public string ViewName
            {
                get { return viewName; }
                set { SetPropertyValue(value, ref viewName, "ViewName"); }
            }

        }
        private AsyncObservableCollection<ViewUnit> viewUnits = new AsyncObservableCollection<ViewUnit>();

        public AsyncObservableCollection<ViewUnit> ViewUnits
        {
            get { return viewUnits; }
            set { SetPropertyValue(value, ref viewUnits, "ViewUnits"); }
        }
        private ViewUnit selectedUnit;

        public ViewUnit SelectedUnit
        {
            get { return selectedUnit; }
            set { SetPropertyValue(value, ref selectedUnit, "SelectedUnit"); }
        }

    }


    public class FieldModelView : ViewModelBase
    {
        private string fieldName = "";
        /// 字段名称
        /// <summary>
        /// 字段名称
        /// </summary>
        public string FieldName
        {
            get { return fieldName; }
            set
            {
                SetPropertyValue(value, ref fieldName, "FieldName");
            }
        }
        private string viewName = "";
        /// 显示名称
        /// <summary>
        /// 显示名称
        /// </summary>
        public string ViewName
        {
            get { return viewName; }
            set { SetPropertyValue(value, ref viewName, "ViewName"); }
        }
        private string fkKey;
        /// 辅助编号
        /// <summary>
        /// 辅助编号
        /// </summary>
        public string FkKey
        {
            get { return fkKey; }
            set { SetPropertyValue(value, ref fkKey, "FkKey"); }
        }
    }
}
