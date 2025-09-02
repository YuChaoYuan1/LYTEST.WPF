using LYTest.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace LYTest.DataManager.SG
{
    public class CodeTreeViewModel : ViewModelBase
    {
        private static CodeTreeViewModel instance = null;
        public static CodeTreeViewModel Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new CodeTreeViewModel();
                }
                return instance;
            }
        }
        /// <summary>
        /// 初始化编码树
        /// </summary>
        public void InitializeTree()
        {
            List<DynamicModel> modelsAll = DALManager.ApplicationDbDal.GetList(EnumAppDbTable.T_CODE_TREE.ToString());

            CodeNodes.Clear();

            //先获得顶层节点
            IEnumerable<DynamicModel> modelsLevel = modelsAll.Where(item => item.GetProperty("CODE_LEVEL") as string == "1");
            //加载顶层节点下的子节点
            foreach (DynamicModel modelTemp in modelsLevel)
            {
                CodeTreeNode treeNodeTemp = new CodeTreeNode(modelTemp);
                LoadChildren(treeNodeTemp, modelsAll);
                CodeNodes.Add(treeNodeTemp);
            }

            InitializeDictionary();
        }

        /// <summary>
        /// 递归加载子节点
        /// </summary>
        /// <param name="nodeRoot">要获取子节点的节点</param>
        private void LoadChildren(CodeTreeNode nodeRoot, List<DynamicModel> modelsAll)
        {
            int levelChildren = nodeRoot.CODE_LEVEL + 1;
            string levelType = nodeRoot.CODE_TYPE;
            IEnumerable<DynamicModel> modelsChild = modelsAll.Where(item => (item.GetProperty("CODE_LEVEL") as string == levelChildren.ToString() && item.GetProperty("CODE_PARENT") as string == levelType));
            modelsChild = modelsChild.OrderBy(item => item.GetProperty("CODE_VALUE") as string);
            foreach (DynamicModel modelTemp in modelsChild)
            {
                CodeTreeNode nodeTemp = new CodeTreeNode(modelTemp)
                {
                    Parent = nodeRoot
                };
                LoadChildren(nodeTemp, modelsAll);
                nodeRoot.Children.Add(nodeTemp);
            }
        }

        /// <summary>
        /// 第一层的节点列表
        /// </summary>
        public AsyncObservableCollection<CodeTreeNode> CodeNodes { get; } = new AsyncObservableCollection<CodeTreeNode>();


        #region 编码查询
        public AsyncObservableCollection<CodeTreeNode> NodesResult { get; } = new AsyncObservableCollection<CodeTreeNode>();

        private string keyWord;
        /// <summary>
        /// 搜寻关键字
        /// </summary>
        public string KeyWord
        {
            get { return keyWord; }
            set { SetPropertyValue(value, ref keyWord, "KeyWord"); }
        }

        /// <summary>
        /// 查询节点
        /// </summary>
        public void SearchNodes()
        {
            if (string.IsNullOrEmpty(KeyWord))
            {
                MessageBox.Show("查询条件不能为空!!");
                return;
            }
            NodesResult.Clear();
            for (int i = 0; i < CodeNodes.Count; i++)
            {
                SearchNodeList(CodeNodes[i]);
            }
        }

        private void SearchNodeList(CodeTreeNode nodeTemp)
        {
            if ((nodeTemp.CODE_NAME != null && nodeTemp.CODE_NAME.Contains(KeyWord)) || (nodeTemp.CODE_TYPE != null && nodeTemp.CODE_TYPE.ToLower().Contains(KeyWord.ToLower())))
            {
                NodesResult.Add(nodeTemp);
            }
            for (int i = 0; i < nodeTemp.Children.Count; i++)
            {
                SearchNodeList(nodeTemp.Children[i]);
            }
        }
        #endregion

        #region 根据编码英文名获取编码
        public string GetCodeCnName(string enName, int layer)
        {
            CodeTreeNode nodeTemp = GetCodeByEnName(enName, layer);
            if (nodeTemp != null)
            {
                return nodeTemp.CODE_NAME;
            }
            return "";
        }
        /// <summary>
        /// 根据英文名称获取编号
        /// </summary>
        /// <param name="enName">英文名称</param>
        /// <param name="layer">层数</param>
        /// <returns></returns>
        public CodeTreeNode GetCodeByEnName(string enName, int layer)
        {
            for (int i = 0; i < CodeNodes.Count; i++)
            {
                CodeTreeNode nodeTemp = GetCodeByEnName(CodeNodes[i], enName, layer);
                if (nodeTemp != null)
                {
                    return nodeTemp;
                }
            }
            return null;
        }
        private CodeTreeNode GetCodeByEnName(CodeTreeNode nodeTemp, string enName, int layer)
        {
            if (nodeTemp.CODE_LEVEL > layer)
            { return null; }
            if (nodeTemp.CODE_TYPE == enName)
            {
                return nodeTemp;
            }
            else
            {
                for (int i = 0; i < nodeTemp.Children.Count; i++)
                {
                    CodeTreeNode nodeTemp1 = GetCodeByEnName(nodeTemp.Children[i], enName, layer);
                    if (nodeTemp1 != null)
                    {
                        return nodeTemp1;
                    }
                }
                return null;
            }
        }
        #endregion
        #region 根据编码中文名获取编码
        public string GetCodeEnName(string cnName, int layer)
        {
            CodeTreeNode nodeTemp = GetCodeByCnName(cnName, layer);
            if (nodeTemp != null)
            {
                return nodeTemp.CODE_TYPE;
            }
            return "";
        }
        /// <summary>
        /// 根据中文名称获取编号
        /// </summary>
        /// <param name="cnName">中文名称</param>
        /// <param name="layer">层数</param>
        /// <returns></returns>
        public CodeTreeNode GetCodeByCnName(string cnName, int layer)
        {
            for (int i = 0; i < CodeNodes.Count; i++)
            {
                CodeTreeNode nodeTemp = GetCodeByCnName(CodeNodes[i], cnName, layer);
                if (nodeTemp != null)
                {
                    return nodeTemp;
                }
            }
            return null;
        }
        private CodeTreeNode GetCodeByCnName(CodeTreeNode nodeTemp, string cnName, int layer)
        {
            if (nodeTemp.CODE_LEVEL > layer)
            { return null; }
            if (nodeTemp.CODE_NAME == cnName)
            {
                return nodeTemp;
            }
            else
            {
                for (int i = 0; i < nodeTemp.Children.Count; i++)
                {
                    CodeTreeNode nodeTemp1 = GetCodeByEnName(nodeTemp.Children[i], cnName, layer);
                    if (nodeTemp1 != null)
                    {
                        return nodeTemp1;
                    }
                }
                return null;
            }
        }
        #endregion

        #region 初始化CodeDictionary
        private void InitializeDictionary()
        {
            CodeDictionary.Clear();
            for (int i = 0; i < CodeNodes.Count; i++)
            {
                LoadDictionary(CodeNodes[i]);
            }
            // 【标注】通讯协议标识，暂时注释  
            //CodeDictionary.LoadDataFlagNames();
        }
        private void LoadDictionary(CodeTreeNode nodeTemp)
        {
            if (nodeTemp == null || nodeTemp.Children.Count == 0 || !nodeTemp.CODE_ENABLED)
            {
                return;
            }
            Dictionary<string, string> dictionaryTemp = new Dictionary<string, string>();
            for (int i = 0; i < nodeTemp.Children.Count; i++)
            {
                if (string.IsNullOrEmpty(nodeTemp.Children[i].CODE_NAME) || dictionaryTemp.ContainsKey(nodeTemp.Children[i].CODE_NAME) || !nodeTemp.Children[i].CODE_ENABLED)
                {
                    continue;
                }
                dictionaryTemp.Add(nodeTemp.Children[i].CODE_NAME, nodeTemp.Children[i].CODE_VALUE);
            }
            CodeDictionary.AddItem(nodeTemp.CODE_TYPE, nodeTemp.CODE_NAME, dictionaryTemp);

            for (int i = 0; i < nodeTemp.Children.Count; i++)
            {
                if (nodeTemp.Children[i].CODE_ENABLED)
                {
                    LoadDictionary(nodeTemp.Children[i]);
                }
            }
        }
        #endregion

    }
}
