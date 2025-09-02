using LYTest.DAL;
using LYTest.Utility.Log;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace LYTest.DataManager.SG
{
    public class CodeTreeNode : ViewModelBase
    {
        public CodeTreeNode()
        {
            PropertyChanged += CodeTreeNode_PropertyChanged;
        }

        private void CodeTreeNode_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != "Parent" && e.PropertyName != "ID" && e.PropertyName != "FlagChanged")
            {
                FlagChanged = true;
            }
        }

        private DynamicModel GetModel()
        {
            DynamicModel modelTemp = new DynamicModel();
            modelTemp.SetProperty("ID", ID);
            modelTemp.SetProperty("CODE_CN_NAME", CODE_NAME);

            //string s = CODE_VALUE;
            //if (CODE_VALUE!=null)
            //{
            // s = CODE_VALUE.PadLeft(4, '0');
            //}
            //modelTemp.SetProperty("CODE_VALUE", s);

            modelTemp.SetProperty("CODE_VALUE", CODE_VALUE);
            modelTemp.SetProperty("CODE_EN_NAME", CODE_TYPE);
            modelTemp.SetProperty("CODE_ENABLED", CODE_ENABLED ? "1" : "0");
            modelTemp.SetProperty("CODE_LEVEL", CODE_LEVEL.ToString());
            modelTemp.SetProperty("CODE_CATEGORY", CODE_CATEGORY);
            modelTemp.SetProperty("CODE_PARENT", CODE_PARENT);
            modelTemp.SetProperty("CODE_PERMISSION", ((int)CodePermission).ToString());

            return modelTemp;
        }

        public CodeTreeNode(DynamicModel modelTemp)
        {
            ID = (int)(modelTemp.GetProperty("ID"));
            CODE_NAME = modelTemp.GetProperty("CODE_CN_NAME") as string;
            CODE_VALUE = modelTemp.GetProperty("CODE_VALUE") as string;
            CODE_TYPE = modelTemp.GetProperty("CODE_EN_NAME") as string;
            string validFlag = modelTemp.GetProperty("CODE_ENABLED") as string;
            if (validFlag == "0")
            {
                CODE_ENABLED = false;
            }
            else
            {
                CODE_ENABLED = true;
            }
            string levelTemp = modelTemp.GetProperty("CODE_LEVEL") as string;
            if (!int.TryParse(levelTemp, out code_level))
            {
                CODE_LEVEL = 1;
            }
            CODE_CATEGORY = modelTemp.GetProperty("CODE_CATEGORY") as string;
            CODE_PARENT = modelTemp.GetProperty("CODE_PARENT") as string;

            string strPermission = modelTemp.GetProperty("CODE_PERMISSION") as string;
            if (int.TryParse(strPermission, out int intTemp))
            {
                CodePermission = (EnumPermission)intTemp;
            }

            PropertyChanged += CodeTreeNode_PropertyChanged;
        }

        private int id = 0;
        /// <summary>
        /// 编码在数据库中的编号,新添加的编号为0
        /// </summary>
        public int ID
        {
            get { return id; }
            set { SetPropertyValue(value, ref id, "ID"); }
        }
        private string code_type;
        /// <summary>
        /// 编码英文名称
        /// </summary>
        public string CODE_TYPE
        {
            get { return code_type; }
            set { SetPropertyValue(value, ref code_type, "CODE_TYPE"); }
        }
        private string code_name;
        /// <summary>
        /// 编码中文名称
        /// </summary>
        public string CODE_NAME
        {
            get { return code_name; }
            set { SetPropertyValue(value, ref code_name, "CODE_NAME"); }
        }
        private string code_value;
        /// <summary>
        /// 编码值
        /// </summary>
        public string CODE_VALUE
        {
            get { return code_value; }
            set { SetPropertyValue(value, ref code_value, "CODE_VALUE"); }
        }
        private int code_level;
        /// <summary>
        /// 编码层数
        /// </summary>
        public int CODE_LEVEL
        {
            get { return code_level; }
            set { SetPropertyValue(value, ref code_level, "CODE_LEVEL"); }
        }

        private EnumPermission codePermission = EnumPermission.超级用户可操作;
        /// <summary>
        /// 编码修改权限
        /// </summary>
        public EnumPermission CodePermission
        {
            get { return codePermission; }
            set { SetPropertyValue(value, ref codePermission, "CodePermission"); }
        }

        private string code_parent;
        /// <summary>
        /// 父节点英文名称
        /// </summary>
        public string CODE_PARENT
        {
            get { return code_parent; }
            set { SetPropertyValue(value, ref code_parent, "CODE_PARENT"); }
        }
        private string code_category;
        /// <summary>
        /// 编码类别
        /// </summary>
        public string CODE_CATEGORY
        {
            get { return code_category; }
            set { SetPropertyValue(value, ref code_category, "CODE_CATEGORY"); }
        }

        private bool code_enabled;
        /// <summary>
        /// 是否有效
        /// </summary>
        public bool CODE_ENABLED
        {
            get { return code_enabled; }
            set { SetPropertyValue(value, ref code_enabled, "CODE_ENABLED"); }
        }

        private bool flagChanged = false;
        /// <summary>
        /// 被更改标记
        /// </summary>
        public bool FlagChanged
        {
            get { return flagChanged; }
            set
            {
                SetPropertyValue(value, ref flagChanged, "FlagChanged");
            }
        }

        /// <summary>
        /// 子节点
        /// </summary>
        public AsyncObservableCollection<CodeTreeNode> Children { get; } = new AsyncObservableCollection<CodeTreeNode>();


        private CodeTreeNode parent;

        public CodeTreeNode Parent
        {
            get { return parent; }
            set { SetPropertyValue(value, ref parent, "Parent"); }
        }
        
        /// <summary>
        /// 递归获取节点所有的ID
        /// </summary>
        /// <param name="nodeTemp">要获取id的节点</param>
        /// <returns></returns>
        private List<int> GetIdList(CodeTreeNode nodeTemp)
        {
            List<int> listTemp = new List<int>();
            if (nodeTemp.ID != 0)
            {
                listTemp.Add(nodeTemp.ID);
                for (int i = 0; i < nodeTemp.Children.Count; i++)
                {
                    List<int> idChildList = GetIdList(nodeTemp.Children[i]);
                    if (idChildList.Count > 0)
                    {
                        listTemp.AddRange(idChildList);
                    }
                }
            }
            return listTemp;
        }
        
        /// <summary>
        /// 获取要编辑的模型列表
        /// </summary>
        /// <param name="codeNode">要编辑的节点</param>
        /// <returns></returns>
        private List<DynamicModel> GetEditModels(CodeTreeNode codeNode)
        {
            List<DynamicModel> modelList = new List<DynamicModel>();
            if (codeNode.FlagChanged && codeNode.ID != 0)
            {
                modelList.Add(codeNode.GetModel());
                codeNode.FlagChanged = false;
            }
            for (int i = 0; i < codeNode.Children.Count; i++)
            {
                modelList.AddRange(GetEditModels(codeNode.Children[i]));
            }
            return modelList;
        }
        /// <summary>
        /// 迭代重命名
        /// </summary>
        /// <param name="nodeToRename"></param>
        private void EditChildren(CodeTreeNode nodeToRename)
        {
            for (int i = 0; i < nodeToRename.Children.Count; i++)
            {
                nodeToRename.Children[i].CODE_PARENT = nodeToRename.CODE_TYPE;
                EditChildren(nodeToRename.Children[i]);
            }
        }

    }
}
