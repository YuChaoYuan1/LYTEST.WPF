using LYTest.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LYTest.DataManager
{
    public class SchemaNodeViewModel : ViewModelBase
    {
        private string name = "";
        /// <summary>
        /// 名称
        /// </summary>
        public string Name
        {
            get { return name; }
            set { SetPropertyValue(value, ref name, "Name"); }
        }
        private string paraNo;
        /// <summary>
        /// 编号
        /// </summary>
        public string ParaNo
        {
            get { return paraNo; }
            set
            {
                SetPropertyValue(value, ref paraNo, "ParaNo");
            }
        }
        private int schemarId;
        /// <summary>
        /// 方案编号
        /// </summary>
        public int SchemaId
        {
            get { return schemarId; }
            set
            {
                SetPropertyValue(value, ref schemarId, "SchemaId");
            }
        }

        /// <summary>
        /// 节点为第几层
        /// </summary>
        public int Level { get; set; }

        private AsyncObservableCollection<SchemaNodeViewModel> children = new AsyncObservableCollection<SchemaNodeViewModel>();
        /// <summary>
        /// 子节点
        /// </summary>
        public AsyncObservableCollection<SchemaNodeViewModel> Children
        {
            get { return children; }
            set
            {
                SetPropertyValue(value, ref children, "Children");
            }
        }
        /// <summary>
        /// 父节点
        /// </summary>
        public SchemaNodeViewModel ParentNode { get; set; }

        public int PointCount
        {
            get { return pointCount; }
            set { SetPropertyValue(value, ref pointCount, "PointCount"); }
        }
        private int pointCount = 0;

        private List<DynamicModel> paraValuesCurrent = new List<DynamicModel>();
        public List<DynamicModel> ParaValuesCurrent
        {
            get { return paraValuesCurrent; }
            set { paraValuesCurrent = value; }
        }
        private bool isTerminal;
        /// <summary>
        /// 是否为检定点
        /// </summary>
        public bool IsTerminal
        {
            get { return isTerminal; }
            set { SetPropertyValue(value, ref isTerminal, "IsTerminal"); }
        }

        /// <summary>
        /// 获取终端节点列表
        /// </summary>
        /// <param name="nodeTemp"></param>
        /// <returns></returns>
        private List<SchemaNodeViewModel> GetTerminalNodes(SchemaNodeViewModel nodeTemp)
        {
            List<SchemaNodeViewModel> nodesTemp = new List<SchemaNodeViewModel>();
            if (nodeTemp.IsTerminal)
            {
                nodesTemp.Add(nodeTemp);
            }
            for (int i = 0; i < nodeTemp.Children.Count; i++)
            {
                nodesTemp.AddRange(GetTerminalNodes(nodeTemp.Children[i]));
            }
            return nodesTemp;
        }

        private string viewNo;
        /// <summary>
        /// 显示视图编号
        /// </summary>
        public string ViewNo
        {
            get { return viewNo; }
            set
            {
                SetPropertyValue(value, ref viewNo, "ViewNo");
                OnPropertyChanged("HaveViewNo");
            }
        }
        
        private string sortNo;

        public string SortNo
        {
            get { return sortNo; }
            set { SetPropertyValue(value, ref sortNo, "SortNo"); }
        }

    }
}
