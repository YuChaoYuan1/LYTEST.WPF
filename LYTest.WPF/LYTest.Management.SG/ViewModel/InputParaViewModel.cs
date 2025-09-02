using LYTest.DAL;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LYTest.DataManager.SG
{
    public class InputParaViewModel : ViewModelBase
    {
        /// <summary>
        /// 所有列的集合
        /// </summary>
        public AsyncObservableCollection<InputParaUnit> AllUnits { get; } = new AsyncObservableCollection<InputParaUnit>();



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
            //【标注003】
            //string value = "888";
            //if (EquipmentData.Equipment.MeterType!="终端")
            //{
            //    value = "889";
            //}
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
                    paraUnit.IsSame = boolTemp;
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

    }
}
