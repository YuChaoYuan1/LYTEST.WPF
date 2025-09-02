using LYTest.DAL;
using LYTest.Utility.Log;
using LYTest.ViewModel.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace LYTest.ViewModel.InputPara
{
    /// <summary>
    /// 参数录入数据模型
    /// </summary>
    public class InputParaViewModel : ViewModelBase
    {
        private AsyncObservableCollection<InputParaUnit> allUnits = new AsyncObservableCollection<InputParaUnit>();
        /// <summary>
        /// 所有列的集合
        /// </summary>
        public AsyncObservableCollection<InputParaUnit> AllUnits
        {
            get { return allUnits; }
            set { SetPropertyValue(value, ref allUnits, "AllUnits"); }
        }


        public InputParaViewModel()
        {


            InitializeUnits();

        }




        //防止不停的排序
        private bool flagLoaded = false;
        /// <summary>
        /// 初始化
        /// </summary>
        private void InitializeUnits()
        {

            #region 从数据库加载所有列
            AllUnits.Clear();  //清空之前所有列
            List<FieldModel> fields = DALManager.MeterTempDbDal.GetFields("T_TMP_METER_INFO");  //先获得所有列的名称
            foreach (FieldModel fieldModel in fields)
            {
                InputParaUnit unit = new InputParaUnit()
                {
                    FieldName = fieldModel.FieldName,
                    IsDisplayMember = false,
                    IsSame = false,
                    DisplayName = "",
                    CodeType = "",
                    ValueType = EnumValueType.编码名称
                };
                unit.PropertyChanged += (sender, e) =>
                {
                    if (e.PropertyName == "Index")
                    {
                        if (flagLoaded)
                        {
                            if (sender is InputParaUnit unitTemp)
                            {
                                if (!unitTemp.IsDisplayMember)
                                {
                                    unitTemp.Index = "999";
                                }
                                AllUnits.Sort(item => item.Index);
                            }
                        }
                    }
                };
                AllUnits.Add(unit);
            }
            #endregion
            LoadParaUnits();
            flagLoaded = true;

        }

        /// <summary>
        /// 加载字段显示集合【加载参数录入界面的信息】
        /// </summary>
        private void LoadParaUnits()
        {
            string stringUnits = "";

            string value = "900";
            DynamicModel modelTemp = DALManager.ApplicationDbDal.GetByID(EnumAppDbTable.T_VIEW_CONFIG.ToString(), $"AVR_VIEW_ID='{value}'");
            if (modelTemp == null)
            {
                return;
            }
            else
            {
                stringUnits = modelTemp.GetProperty("AVR_COL_SHOW_NAME") as string;
            }
            if (string.IsNullOrEmpty(stringUnits))
            {
                return;
            }
            string[] arrayUnits = stringUnits.Split(',');
            for (int i = 0; i < arrayUnits.Length; i++)
            {
                #region 解析字段显示
                string stringParaUnit = arrayUnits[i];
                if (string.IsNullOrEmpty(stringParaUnit))
                {
                    break;
                }
                string[] arrayInputPara = stringParaUnit.Split('|');
                if (arrayInputPara.Length < 9)
                {
                    continue;
                }




                InputParaUnit paraUnit = AllUnits.FirstOrDefault(item => item.FieldName == arrayInputPara[1]);
                if (paraUnit != null)
                {
                    bool.TryParse(arrayInputPara[0], out bool boolTemp);
                    if (boolTemp)
                    {
                        paraUnit.IsDisplayMember = true;
                        paraUnit.Index = (i + 1).ToString().PadLeft(3, '0');
                    }
                    bool.TryParse(arrayInputPara[6], out boolTemp);
                    paraUnit.IsNecessary = boolTemp;
                    //是否显示|字段名称|显示名称|参数对应的编码类型|是否具有相同的值|值的类型
                    paraUnit.DisplayName = arrayInputPara[2];
                    paraUnit.CodeType = arrayInputPara[3];
                    paraUnit.CodeName = CodeDictionary.GetNameLayer1(arrayInputPara[3]);
                    bool.TryParse(arrayInputPara[4], out boolTemp);
                    paraUnit.IsSame =  boolTemp;
                    EnumValueType valueTypeTemp = EnumValueType.编码名称;
                    Enum.TryParse(arrayInputPara[5], out valueTypeTemp);
                    paraUnit.ValueType = valueTypeTemp;
                    paraUnit.DefaultValue = arrayInputPara[7];
                    bool.TryParse(arrayInputPara[8], out boolTemp);
                    paraUnit.IsNewValue = boolTemp;
                }
                #endregion
            }
            AllUnits.Sort(item => item.Index);

        }



        /// <summary>
        /// 保存到数据库
        /// </summary>
        public void SaveParaUnits()
        {
            List<string> listUnits = new List<string>();
            List<string> displayNames = new List<string>();
            List<string> fieldNames = new List<string>();
            #region 获取要保存的名称
            foreach (InputParaUnit paraUnit in AllUnits)
            {
                if (paraUnit.IsDisplayMember)
                {
                    if (string.IsNullOrEmpty(paraUnit.DisplayName))
                    {
                        MessageBox.Show(string.Format("字段:{0}的显示名称还没有配置,不执行保存操作", paraUnit.FieldName));
                        return;
                    }
                    displayNames.Add(paraUnit.DisplayName);
                    fieldNames.Add(paraUnit.FieldName);
                }
                string codeTypeTemp = CodeDictionary.GetCodeLayer1(paraUnit.CodeName);
                paraUnit.CodeType = codeTypeTemp;
                //字段名称|显示名称|参数对应的编码类型|是否具有相同的值|值的类型
                string stringTemp = string.Format("{5}|{0}|{1}|{2}|{3}|{4}|{6}|{7}|{8}", paraUnit.FieldName, paraUnit.DisplayName, codeTypeTemp, paraUnit.IsSame, paraUnit.ValueType, paraUnit.IsDisplayMember, paraUnit.IsNecessary, paraUnit.DefaultValue, paraUnit.IsNewValue);
                listUnits.Add(stringTemp);
            }
            #endregion
            #region 保存到数据库
            //DynamicModel viewModel1 = new DynamicModel();
            DynamicModel viewModel2 = new DynamicModel();
            //viewModel1.SetProperty("PK_VIEW_NO", "42");
            viewModel2.SetProperty("AVR_VIEW_ID", "900");
            //viewModel1.SetProperty("AVR_TABLE_NAME", "METER_INFO");
            viewModel2.SetProperty("AVR_TABLE_NAME", "METER_INFO");
            //viewModel1.SetProperty("AVR_COL_NAME", string.Join(",", fieldNames));
            viewModel2.SetProperty("AVR_COL_NAME", string.Join(",", fieldNames));
            //viewModel1.SetProperty("AVR_COL_SHOW_NAME", string.Join(",", displayNames));
            viewModel2.SetProperty("AVR_COL_SHOW_NAME", string.Join(",", listUnits));
            //string where1 = string.Format("PK_VIEW_NO = '{0}'", "42");
            string where2 = string.Format("AVR_VIEW_ID = '{0}'", "900");
            #region 表信息视图
            //if (DALManager.ApplicationDbDal.GetCount("dsptch_dic_view", where1) > 0)
            //{
            //    LogManager.AddMessage("更新表信息显示视图.", EnumLogSource.数据库存取日志);
            //    DALManager.ApplicationDbDal.Update("DSPTCH_DIC_VIEW", where1, viewModel1, new List<string> { "AVR_TABLE_NAME", "AVR_COL_NAME", "AVR_COL_SHOW_NAME" });
            //}
            //else
            //{
            //    LogManager.AddMessage("插入表信息显示视图.", EnumLogSource.数据库存取日志);
            //    DALManager.ApplicationDbDal.Insert("DSPTCH_DIC_VIEW", viewModel1);
            //}
            #endregion
            #region 参数录入视图
            if (DALManager.ApplicationDbDal.GetCount(EnumAppDbTable.T_VIEW_CONFIG.ToString(), where2) > 0)
            {
                LogManager.AddMessage("更新参数录入显示视图.", EnumLogSource.数据库存取日志);
                DALManager.ApplicationDbDal.Update(EnumAppDbTable.T_VIEW_CONFIG.ToString(), where2, viewModel2, new List<string> { "AVR_TABLE_NAME", "AVR_COL_NAME", "AVR_COL_SHOW_NAME" });
            }
            else
            {
                LogManager.AddMessage("插入参数录入显示视图.", EnumLogSource.数据库存取日志);
                DALManager.ApplicationDbDal.Insert(EnumAppDbTable.T_VIEW_CONFIG.ToString(), viewModel2);
            }
            #endregion
            #endregion
        }
    }
}
