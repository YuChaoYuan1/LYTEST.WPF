using LYTest.DAL;
using LYTest.DAL.DataBaseView;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace LYTest.DataManager
{
    /// <summary>
    /// 台体信息
    /// </summary>
    public class Equipments : ViewModelBase
    {
        private static Equipments instance;

        public static Equipments Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new Equipments();
                }
                return instance;
            }
        }
        private readonly TableDisplayModel displayModel = ResultViewHelper.GetTableDisplayModel("013", true);
        public void Initialize()
        {

        }

        /// <summary>
        /// 信息列表
        /// </summary>
        public AsyncObservableCollection<DynamicViewModel> Models { get; } = new AsyncObservableCollection<DynamicViewModel>();

        /// <summary>
        /// 创建新的台体信息
        /// </summary>
        /// <param name="equipmentNo"></param>
        public DynamicViewModel FindEquipInfo(string equipmentNo)
        {
            DynamicViewModel modelTemp = Models.FirstOrDefault(item => item.GetProperty(EquipNoName) as string == equipmentNo);
            if (modelTemp != null)
            {
                return modelTemp;
            }
            modelTemp = new DynamicViewModel(0);
            for (int j = 0; j < displayModel.ColumnModelList.Count; j++)
            {
                ColumnDisplayModel columnModel = displayModel.ColumnModelList[j];
                if (columnModel.Field == "AVR_DEVICE_ID")
                {
                    modelTemp.SetProperty(columnModel.DisplayName, equipmentNo);
                    continue;
                }
                string[] displayNames = columnModel.DisplayName.Split('|');
                for (int k = 0; k < displayNames.Length; k++)
                {
                    modelTemp.SetProperty(displayNames[k], "");
                }
            }
            Models.Add(modelTemp);
            return modelTemp;
        }
        /// <summary>
        /// 获取台体编号的显示名称
        /// </summary>
        /// <returns></returns>
        private string EquipNoName
        {
            get
            {
                ColumnDisplayModel columnModel = displayModel.ColumnModelList.Find(item => item.Field == "AVR_DEVICE_ID");
                return columnModel.DisplayName;
            }
        }

        public ReadOnlyCollection<string> ReguNames { get; } = new ReadOnlyCollection<string>(
 new string[] { "单-本地依据", "单-远程依据", "三-本地依据", "三-远程依据" }
 );
        public ReadOnlyCollection<string> ReguDefaultValues { get; } = new ReadOnlyCollection<string>(
 new string[] { "JJG596-2012《电子式交流电能表》JJG691-2014《多费率交流电能表》JJG1099-2014《预付费交流电能表》", "JJG596-2012《电子式交流电能表》JJG691-2014《多费率交流电能表》", "JJG596-2012《电子式交流电能表》JJG691-2014《多费率交流电能表》JJG1099-2014《预付费交流电能表》JJG569-2014《最大需量电能表》", "JJG596-2012《电子式交流电能表》JJG691-2014《多费率交流电能表》JJG569-2014《最大需量电能表》" }
 );

        private static SchemaViewModel schema;
        /// 检定方案
        /// <summary>
        /// 检定方案
        /// </summary>
        public static SchemaViewModel Schema
        {
            get
            {
                if (schema == null)
                {
                    schema = new SchemaViewModel();
                }
                return schema;
            }
        }

    }
}
