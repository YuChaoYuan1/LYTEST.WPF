using System.Collections.Generic;
using System.Linq;

namespace LYTest.DAL.DataBaseView
{
    /// <summary>
    /// 数据表的显示模型
    /// </summary>
    public class TableDisplayModel
    {
        /// <summary>
        /// 显示模型键值
        /// </summary>
        public string ViewID { get; set; }
        public List<string> paraNoList = new List<string>();
        /// <summary>
        /// 检定点编号
        /// </summary>
        public List<string> ParaNoList { get { return paraNoList; } set { paraNoList = value; } }
        /// <summary>
        /// 表名称
        /// </summary>
        public string TableName { get; set; }
        private List<ColumnDisplayModel> modelList = new List<ColumnDisplayModel>();
        /// <summary>
        /// 要显示的所有列
        /// </summary>
        public List<ColumnDisplayModel> ColumnModelList{ get{ return modelList;} set {modelList=value;} }
        private List<FKDisplayConfigModel> fKDisplayModelList = new List<FKDisplayConfigModel>();
        /// <summary>
        /// 副检定项结论列表
        /// </summary>
        public List<FKDisplayConfigModel> FKDisplayModelList 
        {
            get { return fKDisplayModelList; }
            set { fKDisplayModelList = value; }
        }
        /// <summary>
        /// 获取结论名称列表
        /// </summary>
        /// <returns></returns>
        public List<string> GetDisplayNames()
        {
            List<string> namesResult = new List<string>();
            for (int i = 0; i < FKDisplayModelList.Count; i++)
            {
                namesResult.AddRange(FKDisplayModelList[i].DisplayNames);
            }
            var displayNames = from item in ColumnModelList select item.DisplayName;
            for (int i = 0; i < displayNames.Count(); i++)
            {
                namesResult.AddRange(displayNames.ElementAt(i).Split('|'));
            }
            return namesResult;
        }
    }
}
