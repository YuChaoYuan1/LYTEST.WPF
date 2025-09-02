using LYTest.DAL;
using LYTest.DAL.DataBaseView;
using LYTest.ViewModel.AccessControl;
using LYTest.ViewModel.CodeTree;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace LYTest.ViewModel.Schema
{
    public class FullTree : SchemaNodeViewModel
    {
        private static FullTree instance = null;
        public static FullTree Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new FullTree();
                }
                return instance;
            }
        }
        /// <summary>
        /// 初始化
        /// </summary>
        public void Initialize()
        {
            Children.Clear();
            //List<SchemaNodeViewModel> tem = new List<SchemaNodeViewModel>();
            Dictionary<string, SchemaNodeViewModel> tem = new Dictionary<string, SchemaNodeViewModel>();
            CodeTreeNode nodeSchemaRoot = CodeTreeViewModel.Instance.CodeNodes.FirstOrDefault(item => item.CODE_TYPE == "SchemaCategory");
            for (int i = 0; i < nodeSchemaRoot.Children.Count; i++)
            {
                SchemaNodeViewModel nodeTemp = GetNode(nodeSchemaRoot.Children[i]);
                if (nodeTemp != null)
                {
                    tem.Add(nodeTemp.Name, nodeTemp);
                    SchemaFramework.AddNewPair(nodeTemp.ParaNo, nodeTemp.Name);
                    SetParaNo(nodeTemp);
                }
            }
            List<string> list = new List<string>();
            for (int i = 0; i < 999; i++)
            {
                string str = Core.OperateFile.GetINI("Sort", i.ToString(), System.IO.Directory.GetCurrentDirectory() + "\\Ini\\ShcemaShor.ini").Trim();
                if (str != "")
                {
                    list.Add(str);
                }
            }

            for (int i = 0; i < list.Count; i++)
            {
                if (tem.ContainsKey(list[i]))
                {
                    Children.Add(tem[list[i]]);
                    tem.Remove(list[i]);
                }
            }

            foreach (var item in tem.Keys)
            {
                Children.Add(tem[item]);
            }
        }


        /// <summary>
        /// 获取方案节点
        /// </summary>
        /// <param name="codeNode"></param>
        /// <returns></returns>
        private SchemaNodeViewModel GetNode(CodeTreeNode codeNode)
        {
            if (!codeNode.CODE_ENABLED)
            {
                return null;
            }
            SchemaNodeViewModel schemaNode = new SchemaNodeViewModel
            {
                Name = codeNode.CODE_NAME
            };
            #region 设置当前节点的信息

            if (codeNode.CODE_LEVEL == 2)
            {
                schemaNode.ParaNo = codeNode.CODE_VALUE.PadLeft(2, '0');
            }
            else
            {
                schemaNode.ParaNo = codeNode.CODE_VALUE.PadLeft(3, '0');
            }

            schemaNode.Level = codeNode.CODE_LEVEL - 1;
            #endregion
            for (int i = 0; i < codeNode.Children.Count; i++)
            {
                SchemaNodeViewModel schemaNodeChild = GetNode(codeNode.Children[i]);
                if (schemaNodeChild != null)
                {
                    schemaNode.Children.Add(schemaNodeChild);
                    schemaNodeChild.ParentNode = schemaNode;
                }
            }
            if (schemaNode.Children.Count == 0)
            {
                schemaNode.IsTerminal = true;
            }
            else
            {
                schemaNode.IsTerminal = false;
            }
            return schemaNode;
        }
        /// <summary>
        /// 设置节点编号
        /// </summary>
        /// <param name="nodeTemp"></param>
        private void SetParaNo(SchemaNodeViewModel nodeTemp)
        {
            if (nodeTemp.ParentNode != null)
            {
                nodeTemp.ParaNo = nodeTemp.ParentNode.ParaNo + nodeTemp.ParaNo;
                SchemaFramework.AddNewPair(nodeTemp.ParaNo, nodeTemp.Name);
                if (nodeTemp.IsTerminal)
                {
                    nodeTemp.ViewNo = ResultViewHelper.GetParaNoView(nodeTemp.ParaNo);
                    DynamicModel modelTemp = SchemaFramework.GetParaFormat(nodeTemp.ParaNo);
                    if (modelTemp != null)
                    {
                        nodeTemp.SortNo = modelTemp.GetProperty("DEFAULT_SORT_NO") as string;     //排序的编号
                    }
                    if (string.IsNullOrEmpty(nodeTemp.SortNo))
                    {
                        nodeTemp.SortNo = "999";
                    }
                }
            }

            for (int i = 0; i < nodeTemp.Children.Count; i++)
            {
                SetParaNo(nodeTemp.Children[i]);
            }
        }

        public IEnumerable FilterAllCheck()
        {
            return Children.Where(x => x.Children.Any(a => RightsSchemaCategory.Categories.Any(b => b.Name == a.Name && b.ParaNo == a.ParaNo && (b.Rights & 0x01) == 0x01)))
                .Select(o =>
                {
                    var col = new Model.AsyncObservableCollection<SchemaNodeViewModel>(o.Children.Where(v => RightsSchemaCategory.Categories.Any(d => d.Name == v.Name && d.ParaNo == v.ParaNo && (d.Rights & 0x01) == 0x01)));
                    o.Children.Clear();
                    foreach (var c in col)
                        o.Children.Add(c);
                    return o;
                });
        }

        public IEnumerable FilterSampleCheck()
        {
            Initialize();
            return Children.Where(x => x.Children.Any(a => RightsSchemaCategory.Categories.Any(b => b.Name == a.Name && b.ParaNo == a.ParaNo && (b.Rights & 0x02) == 0x02)))
                .Select(o =>
                {
                    var col = new Model.AsyncObservableCollection<SchemaNodeViewModel>(o.Children.Where(v => RightsSchemaCategory.Categories.Any(d => d.Name == v.Name && d.ParaNo == v.ParaNo && (d.Rights & 0x02) == 0x02)));
                    o.Children.Clear();
                    foreach (var c in col)
                        o.Children.Add(c);
                    return o;
                });
        }

        public IEnumerable FilterFullCheck()
        {
            Initialize();
            return Children.Where(x => x.Children.Any(a => RightsSchemaCategory.Categories.Any(b => b.Name == a.Name && b.ParaNo == a.ParaNo && (b.Rights & 0x04) == 0x04)))
                .Select(o =>
                {
                    var col = new Model.AsyncObservableCollection<SchemaNodeViewModel>(o.Children.Where(v => RightsSchemaCategory.Categories.Any(d => d.Name == v.Name && d.ParaNo == v.ParaNo && (d.Rights & 0x04) == 0x04)));
                    o.Children.Clear();
                    foreach (var c in col)
                        o.Children.Add(c);

                    return o;
                });
        }
    }
}
