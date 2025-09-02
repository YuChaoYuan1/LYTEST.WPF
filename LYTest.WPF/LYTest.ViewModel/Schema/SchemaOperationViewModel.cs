using LYTest.DAL;
using LYTest.Utility.Log;
using LYTest.ViewModel.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace LYTest.ViewModel.Schema
{

    /// <summary>
    /// 方案操作视图模型
    /// </summary>
    public class SchemaOperationViewModel : ViewModelBase
    {
        public SchemaOperationViewModel()
        {
            LoadSchemas();
        }

        /// <summary>
        /// 被选择的方案
        /// </summary>
        public DynamicViewModel SelectedSchema
        {
            get => GetProperty(new DynamicViewModel(0));
            set => SetProperty(value);
        }

        /// <summary>
        ///所有方案名称列表
        /// </summary>
        public AsyncObservableCollection<DynamicViewModel> SchemasALL { get; } = new AsyncObservableCollection<DynamicViewModel>();


        /// <summary>
        /// 方案名称列表
        /// </summary>
        public AsyncObservableCollection<DynamicViewModel> Schemas { get; } = new AsyncObservableCollection<DynamicViewModel>();

        /// <summary>
        /// 加载方案列表
        /// </summary>
        private void LoadSchemas()
        {
            // 从数据库加载能被使用的方案
            List<DynamicModel> models = DALManager.SchemaDal.GetList(EnumAppDbTable.T_SCHEMA_INFO.ToString(), $"SCHEMA_METER_TYPE='{EquipmentData.Equipment.EquipmentType}' AND SCHEMA_TEST_TYPE='{EquipmentData.Equipment.MeterType}' AND SCHEMA_ENABLED='1' ");

            //对方案根据名称进行一个排序
            models.Sort((a, b) => a.GetProperty("SCHEMA_NAME").ToString().CompareTo(b.GetProperty("SCHEMA_NAME").ToString()));
            // 清空视窗模型中的方案列表，并重新录入
            Schemas.Clear();
            for (int i = 0; i < models.Count; i++)
            {
                Schemas.Add(new DynamicViewModel(models[i], i));
            }
            // 清空方案设置页面中的所有方案，从数据库中重新加载
            SchemasALL.Clear();
            models = DALManager.SchemaDal.GetList(EnumAppDbTable.T_SCHEMA_INFO.ToString(), $"SCHEMA_METER_TYPE='{EquipmentData.Equipment.EquipmentType}' AND SCHEMA_TEST_TYPE='{EquipmentData.Equipment.MeterType}'");
            for (int i = 0; i < models.Count; i++)
            {
                SchemasALL.Add(new DynamicViewModel(models[i], i));
            }
        }

        /// <summary>
        /// 方案名校验
        /// </summary>
        /// <remarks>
        /// 这里需要严格判断方案名是否符合规范，
        /// 可能会出现三相台添加单相台方案的情况，
        /// 这种情况下添加设备型号检索会出现bug漏掉已经添加的方案，
        /// 导致重名且功能相同的方案被添加。
        /// </remarks>
        private bool CheckSchemaName(string schemaName)
        {
            //名称校验
            if (string.IsNullOrEmpty(schemaName))
            {
                LogManager.AddMessage("方案名无效,方案名不允许为空", EnumLogSource.用户操作日志, EnumLevel.Warning);
                MessageBox.Show("方案名无效,方案名不允许为空", "警告", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            // 这里需要判断方案名是否符合规范
            if (DALManager.SchemaDal.GetCount(EnumAppDbTable.T_SCHEMA_INFO.ToString(), string.Format("schema_name ='{0}'", schemaName)) > 0)
            {
                List<DynamicModel> schemas = DALManager.SchemaDal.GetModels(EnumAppDbTable.T_SCHEMA_INFO.ToString(), string.Format("schema_name ='{0}'", schemaName));
                foreach (DynamicModel schema in schemas)
                {
                    // 如果存在台体型号和检定表类型都重合的方案，则认为是重名
                    if (schema.GetProperty("SCHEMA_METER_TYPE").ToString().Equals(EquipmentData.Equipment.EquipmentType, StringComparison.CurrentCultureIgnoreCase) &&
                        schema.GetProperty("SCHEMA_TEST_TYPE").ToString().Equals(EquipmentData.Equipment.MeterType, StringComparison.CurrentCultureIgnoreCase))
                    {
                        LogManager.AddMessage("方案名无效,方案名已存在", EnumLogSource.用户操作日志, EnumLevel.Warning);
                        MessageBox.Show("方案名无效,方案名已存在", "警告", MessageBoxButton.OK, MessageBoxImage.Error);
                        return false;
                    }
                }
                // 否则仅抛出警告但不阻止添加
                LogManager.AddMessage("同名方案名已存在,但仍可继续操作。", EnumLogSource.用户操作日志, EnumLevel.Warning);
            }
            return true;
        }
        /// <summary>
        /// 方案重命名
        /// </summary>
        public void RenameSchema()
        {
            if (!CheckSchemaName(NewName))
            {
                return;
            }
            if (MessageBox.Show("确认对方案重命名?", "重命名", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                string origionalName = SelectedSchema.GetProperty("SCHEMA_NAME") as string;
                DynamicModel model = new DynamicModel();
                model.SetProperty("SCHEMA_NAME", NewName);
                DALManager.SchemaDal.Update(EnumAppDbTable.T_SCHEMA_INFO.ToString(), string.Format("schema_name = '{0}'", origionalName), model, new List<string> { "SCHEMA_NAME" });
                LoadSchemas();

                DynamicViewModel modelTemp = EquipmentData.SchemaModels.Schemas.FirstOrDefault(item => item.GetProperty("SCHEMA_NAME") as string == origionalName);
                if (modelTemp != null)
                {
                    modelTemp.SetProperty("SCHEMA_NAME", NewName);
                }
            }
        }
        #region 添加方案

        public string NewName
        {
            get => GetProperty("");
            set => SetProperty(value);
        }

        public string SchemaType
        {
            get => GetProperty("");
            set => SetProperty(value);
        }

        /// <summary>
        /// 添加方案
        /// </summary>
        public void AddSchema()
        {
            if (!CheckSchemaName(NewName))
            {
                return;
            }
            //【方案标注】如果不需要方案分类，这里需要修改
            //if (string.IsNullOrEmpty(SchemaType))
            //{
            //    LogManager.AddMessage("请选择新建的检定方案类型.", EnumLogSource.用户操作日志, EnumLevel.WarningSpeech);
            //    return;
            //}
            DynamicModel model = new DynamicModel();
            model.SetProperty("SCHEMA_NAME", NewName);
            model.SetProperty("SCHEMA_METER_TYPE", EquipmentData.Equipment.EquipmentType);
            model.SetProperty("SCHEMA_TEST_TYPE", EquipmentData.Equipment.MeterType);
            model.SetProperty("SCHEMA_ENABLED", "1");



            //{     //EquipmentData.Equipment.MeterType
            //      //EquipmentData.Equipment.EquipmentType

            //model.SetProperty("SCHEMA_TYPE", SchemaType);
            DALManager.SchemaDal.Insert(EnumAppDbTable.T_SCHEMA_INFO.ToString(), model);
            LoadSchemas();

            DynamicViewModel modelTemp = Schemas.FirstOrDefault(item => item.GetProperty("SCHEMA_NAME") as string == NewName);
            if (modelTemp != null)
            {
                EquipmentData.SchemaModels.Schemas.Add(modelTemp);
            }
        }

        /// <summary>
        /// 添加方案
        /// </summary>
        public void AddDownSchema()
        {
            if (!CheckSchemaName(NewName))
            {
                return;
            }
            //【方案标注】如果不需要方案分类，这里需要修改
            //if (string.IsNullOrEmpty(SchemaType))
            //{
            //    LogManager.AddMessage("请选择新建的检定方案类型.", EnumLogSource.用户操作日志, EnumLevel.WarningSpeech);
            //    return;
            //}
            DynamicModel model = new DynamicModel();
            model.SetProperty("SCHEMA_NAME", NewName);
            model.SetProperty("SCHEMA_METER_TYPE", EquipmentData.Equipment.EquipmentType);
            model.SetProperty("SCHEMA_TEST_TYPE", EquipmentData.Equipment.MeterType);
            //add yjt zxg 20220426 新增
            model.SetProperty("SCHEMA_ENABLED", "1");
            //{     //EquipmentData.Equipment.MeterType
            //      //EquipmentData.Equipment.EquipmentType

            //model.SetProperty("SCHEMA_TYPE", SchemaType);
            DALManager.SchemaDal.Insert(EnumAppDbTable.T_SCHEMA_INFO.ToString(), model);


            LoadSchemas();

            DynamicViewModel modelTemp = Schemas.FirstOrDefault(item => item.GetProperty("SCHEMA_NAME") as string == NewName);
            //if (modelTemp != null)
            //{
            //    EquipmentData.SchemaModels.Schemas.Add(modelTemp);
            //}
        }
        #endregion
        #region 拷贝方案
        /// 拷贝方案
        /// <summary>
        /// 拷贝方案
        /// </summary>
        public void CopySchema()
        {
            if (!CheckSchemaName(NewName))
            {
                return;
            }
            #region 获取旧方案信息
            int oldId = (int)(SelectedSchema.GetProperty("ID"));
            List<DynamicModel> models = DALManager.SchemaDal.GetList(EnumAppDbTable.T_SCHEMA_PARA_VALUE.ToString(), string.Format("SCHEMA_ID={0}", oldId));
            #endregion
            #region 插入新方案导数据库
            int newId = 0;
            SelectedSchema.SetProperty("SCHEMA_NAME", NewName);
            DALManager.SchemaDal.Insert(EnumAppDbTable.T_SCHEMA_INFO.ToString(), SelectedSchema.GetDataSource());
            DynamicModel newModel = DALManager.SchemaDal.GetByID(EnumAppDbTable.T_SCHEMA_INFO.ToString(), string.Format("schema_name='{0}'", NewName));
            if (newModel != null)
            {
                newId = (int)(newModel.GetProperty("ID"));
                for (int i = 0; i < models.Count; i++)
                {
                    models[i].SetProperty("SCHEMA_ID", newId);
                    DALManager.SchemaDal.Insert(EnumAppDbTable.T_SCHEMA_PARA_VALUE.ToString(), models[i]);
                }
            }
            else
            {
                LogManager.AddMessage("方案拷贝失败!!!", EnumLogSource.用户操作日志, EnumLevel.Error);
            }
            #endregion
            LoadSchemas();

            DynamicViewModel modelTemp = Schemas.FirstOrDefault(item => item.GetProperty("SCHEMA_NAME") as string == NewName);
            if (modelTemp != null)
            {
                EquipmentData.SchemaModels.Schemas.Add(modelTemp);
            }
        }
        #endregion
        /// <summary>
        /// 删除方案
        /// </summary>
        public void DeleteSchema()
        {
            int oldId = (int)(SelectedSchema.GetProperty("ID"));
            DALManager.SchemaDal.Delete(EnumAppDbTable.T_SCHEMA_INFO.ToString(), string.Format("ID={0}", oldId));
            LoadSchemas();

            DynamicViewModel modelTemp = EquipmentData.SchemaModels.Schemas.FirstOrDefault(item => (int)item.GetProperty("ID") == oldId);
            if (modelTemp != null)
            {
                EquipmentData.SchemaModels.Schemas.Remove(modelTemp);
            }
        }
        /// <summary>
        /// 是否正在保存方案
        /// </summary>
        private bool doingSet = false;
        /// <summary>
        /// 保存方案设置
        /// </summary>
        public void SetSchemaValue()
        {
            if (doingSet)
                return;
            doingSet = true;
            try
            {
                for (int i = 0; i < SchemasALL.Count; i++)
                {
                    DynamicViewModel dynamic = SchemasALL[i];
                    int oldId = (int)dynamic.GetProperty("ID");
                    //string name = dynamic.GetProperty("ID");

                    DynamicModel model = new DynamicModel();
                    model.SetProperty("SCHEMA_NAME", dynamic.GetProperty("SCHEMA_NAME"));
                    model.SetProperty("SCHEMA_METER_TYPE", dynamic.GetProperty("SCHEMA_METER_TYPE"));
                    model.SetProperty("SCHEMA_TEST_TYPE", dynamic.GetProperty("SCHEMA_TEST_TYPE"));
                    model.SetProperty("SCHEMA_ENABLED", dynamic.GetProperty("SCHEMA_ENABLED"));

                    DALManager.SchemaDal.Update(EnumAppDbTable.T_SCHEMA_INFO.ToString(), string.Format("ID={0}", oldId), model, new List<string>
                { "SCHEMA_NAME" ,"SCHEMA_METER_TYPE","SCHEMA_TEST_TYPE","SCHEMA_ENABLED"});
                }
                MessageBox.Show("保存成功");

            }
            catch (Exception ex)
            {
                MessageBox.Show("保存失败" + ex.ToString());
            }
            LoadSchemas();
            doingSet = false;
        }

        public void RefreshCurrrentSchema()
        {
            OnPropertyChanged("SelectedSchema");
        }
        public void EndWaitRefreshSchema()
        {
            OnPropertyChanged("EndWait");
        }

        #region 导入方案
        /// <summary>
        /// 导入方案名称列表
        /// </summary>
        public AsyncObservableCollection<DynamicViewModel> ImportSchemas { get; } = new AsyncObservableCollection<DynamicViewModel>();

        private DynamicViewModel importselectedSchema = new DynamicViewModel(0);

        public DynamicViewModel ImportSelectedSchema
        {
            get { return importselectedSchema; }
            set
            {
                SetPropertyValue(value, ref importselectedSchema, "ImportSelectedSchema");
                RefImport(ImportSelectedSchema);
            }
        }

        /// <summary>
        /// 项目列表
        /// </summary>
        public AsyncObservableCollection<DynamicViewModel> ImportItemList { get; } = new AsyncObservableCollection<DynamicViewModel>();


        /// <summary>
        /// 导入方案名称
        /// </summary>
        public string ImportName
        {
            get => GetProperty("");
            set => SetProperty(value);
        }

        /// <summary>
        /// 导入数据库路径
        /// </summary>
        private string ImportPath;
        /// <summary>
        /// 获取导入方案的列表
        /// </summary>
        /// <param name="Path"></param>
        public void GetImportShema(string Path)
        {
            ImportPath = Path;
            //获取方案列表
            string connString = string.Format(@"Provider=Microsoft.Jet.OLEDB.4.0;Data Source={0}", ImportPath);
            GeneralDal schemaDal = new GeneralDal(connString);
            List<DynamicModel> models = schemaDal.GetList(EnumAppDbTable.T_SCHEMA_INFO.ToString(), $"SCHEMA_METER_TYPE='{EquipmentData.Equipment.EquipmentType}' AND SCHEMA_TEST_TYPE='{EquipmentData.Equipment.MeterType}' AND SCHEMA_ENABLED='1'");
            ImportSchemas.Clear();
            for (int i = 0; i < models.Count; i++)
            {
                ImportSchemas.Add(new DynamicViewModel(models[i], i));
            }
        }
        /// <summary>
        /// 刷新UI，显示导入的方案检定项目列表
        /// </summary>
        /// <param name="ImportSelectedSchema"></param>
        private void RefImport(DynamicViewModel ImportSelectedSchema)
        {
            if (ImportSelectedSchema == null) return;
            ImportName = ImportSelectedSchema.GetProperty("SCHEMA_NAME") as string;
            int SchemaId = int.Parse(ImportSelectedSchema.GetProperty("ID").ToString());
            string connString = string.Format(@"Provider=Microsoft.Jet.OLEDB.4.0;Data Source={0}", ImportPath);
            GeneralDal schemaDal = new GeneralDal(connString);
            List<DynamicModel> models = schemaDal.GetList(EnumAppDbTable.T_SCHEMA_PARA_VALUE.ToString(), string.Format("SCHEMA_ID={0} and (CODE_ENABLED <> '0' or CODE_ENABLED is null) order by para_index", SchemaId));
            ImportItemList.Clear();
            for (int i = 0; i < models.Count; i++)                   //12013
            {
                ImportItemList.Add(new DynamicViewModel(models[i], i));
            }
        }
        ///// <summary>
        ///// 导入方案
        ///// </summary>
        public void ImportSchema()
        {
            #region 同方案重复添加检测
            List<DynamicModel> schemas = DALManager.SchemaDal.GetList(EnumAppDbTable.T_SCHEMA_INFO.ToString(), $"SCHEMA_NAME ='{ImportName}' AND SCHEMA_METER_TYPE='{EquipmentData.Equipment.EquipmentType}' AND SCHEMA_TEST_TYPE='{EquipmentData.Equipment.MeterType}'");
            if (schemas.Count > 0)
            {
                foreach (DynamicModel model in schemas)
                {
                    if (model.GetProperty("SCHEMA_ENABLED").Equals("1"))
                    {
                        LogManager.AddMessage("导入方案失败，已经存在相同的方案了。", EnumLogSource.用户操作日志, EnumLevel.Error);
                        MessageBox.Show("导入方案失败，已经存在相同的方案了。", "警告", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }
                LogManager.AddMessage("导入方案失败，没有导入该方案的权限。", EnumLogSource.用户操作日志, EnumLevel.Error);
                MessageBox.Show("导入方案失败，没有导入该方案的权限。", "警告", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            #endregion
            #region 导入方案合法性检测
            if (!ImportSelectedSchema.GetProperty("SCHEMA_METER_TYPE").ToString().Equals(EquipmentData.Equipment.EquipmentType, StringComparison.CurrentCultureIgnoreCase) ||
                !ImportSelectedSchema.GetProperty("SCHEMA_TEST_TYPE").ToString().Equals(EquipmentData.Equipment.MeterType, StringComparison.CurrentCultureIgnoreCase))
            {
                LogManager.AddMessage("导入方案失败,导入的方案与当前设备型号或检定表类型不匹配!", EnumLogSource.用户操作日志, EnumLevel.Error);
                MessageBox.Show("导入方案失败,导入的方案与当前设备型号或检定表类型不匹配!", "警告", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            #endregion
            #region 插入SCHEMA_INFO到数据库
            if (DALManager.SchemaDal.Insert(EnumAppDbTable.T_SCHEMA_INFO.ToString(), ImportSelectedSchema.GetDataSource()) <= 0)
            {
                LogManager.AddMessage($"导入方案失败,插入数据库{EnumAppDbTable.T_SCHEMA_INFO.ToString()}的词条数量为0！", EnumLogSource.用户操作日志, EnumLevel.Error);
                MessageBox.Show($"导入方案失败,插入数据库{EnumAppDbTable.T_SCHEMA_INFO.ToString()}的词条数量为0！", "警告", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            #endregion
            #region 插入SCHEMA_PARA_VALUE到数据库
            int newId = 0;
            try
            {
                DynamicModel newModel = DALManager.SchemaDal.GetByID(EnumAppDbTable.T_SCHEMA_INFO.ToString(), $"SCHEMA_NAME ='{ImportName}' AND SCHEMA_METER_TYPE='{EquipmentData.Equipment.EquipmentType}' AND SCHEMA_TEST_TYPE='{EquipmentData.Equipment.MeterType}'");
                if (newModel != null)
                {
                    // 获取自增型ID
                    newId = (int)(newModel.GetProperty("ID"));
                    for (int i = 0; i < ImportItemList.Count; i++)
                    {
                        ImportItemList[i].SetProperty("SCHEMA_ID", newId);
                        if (DALManager.SchemaDal.Insert(EnumAppDbTable.T_SCHEMA_PARA_VALUE.ToString(), ImportItemList[i].GetDataSource()) <= 0)
                        {
                            throw new Exception($"试验点[{ImportItemList[i].GetProperty("PARA_NAME").ToString()}]插入失败!");
                        }
                    }
                    MessageBox.Show("导入方案完成!");

                }
                else
                {
                    throw new Exception("未找到对应的方案信息!");
                }
            }
            catch (Exception ex)
            {
                // 发生插入失败时，删除所有已插入的方案信息
                LogManager.AddMessage($"方案导入失败!\n错误信息：{ex.Message}", EnumLogSource.用户操作日志, EnumLevel.Error);
                MessageBox.Show($"方案导入失败!\n错误信息：{ex.Message}", "警告", MessageBoxButton.OK, MessageBoxImage.Error);
                DALManager.SchemaDal.Delete(EnumAppDbTable.T_SCHEMA_INFO.ToString(), $"SCHEMA_NAME ='{ImportName}'");
                DALManager.SchemaDal.Delete(EnumAppDbTable.T_SCHEMA_PARA_VALUE.ToString(), $"SCHEMA_NAME ='{ImportName}'");
            }
            #endregion
            LoadSchemas();
        }


        #endregion
    }

}
