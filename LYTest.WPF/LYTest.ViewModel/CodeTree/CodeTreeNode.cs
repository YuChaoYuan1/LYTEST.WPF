using LYTest.DAL;
using LYTest.Utility.Log;
using LYTest.ViewModel.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace LYTest.ViewModel.CodeTree
{
    /// <summary>
    /// 编码的树形节点
    /// </summary>
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
                CodePermission = (EnumPermission)20;
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

        private AsyncObservableCollection<CodeTreeNode> children = new AsyncObservableCollection<CodeTreeNode>();
        /// <summary>
        /// 子节点
        /// </summary>
        public AsyncObservableCollection<CodeTreeNode> Children
        {
            get { return children; }
            set { SetPropertyValue(value, ref children, "Children"); }
        }

        private CodeTreeNode parent;

        public CodeTreeNode Parent
        {
            get { return parent; }
            set { SetPropertyValue(value, ref parent, "Parent"); }
        }
        /// <summary>
        /// 添加编码
        /// </summary>
        public void AddCode()
        {
            if (string.IsNullOrWhiteSpace(CODE_TYPE))
            {
                LogManager.AddMessage("当前是末级节点,并且没有英文名称,不允许添加子节点!!!", EnumLogSource.数据库存取日志, EnumLevel.Tip);
                return;
            }
            CodeTreeNode nodeNew = new CodeTreeNode()
            {
                CODE_LEVEL = CODE_LEVEL + 1,
                CODE_CATEGORY = CODE_CATEGORY,
                CODE_NAME = "新编码中文名称",
                CODE_VALUE = "新编码值",
                CODE_TYPE = "",
                CODE_PARENT = CODE_TYPE,
                CODE_ENABLED = true,
                Parent = this,
            };
            Children.Add(nodeNew);

            ////添加数据协议名称的时候为了方便从代码来添加，不要用
            //int index = 1251;
            //string name;
            //System.Data.DataTable dataTable = LYTest.MeterProtocol.MeterProtocal.Protocals;
            //for (int i = 0; i < dataTable.Rows.Count; i++)
            //{

            //    System.Data.DataRow Row = dataTable.Rows[i];
            //    name = Row.ItemArray[6].ToString();
            //    if (name == "(上5次)阶梯切换冻结记录")
            //    {

            //    }
            //    CodeTreeNode nodeNew = new CodeTreeNode()
            //    {
            //        CODE_LEVEL = CODE_LEVEL + 1,
            //        CODE_CATEGORY = CODE_CATEGORY,
            //        CODE_NAME = name,
            //        CODE_VALUE = index.ToString().PadLeft(4, '0'),
            //        CODE_TYPE = "",
            //        CODE_PARENT = CODE_TYPE,
            //        CODE_ENABLED = true,
            //        Parent = this,
            //    };
            //    Children.Add(nodeNew);
            //    index++;
            //}

            #region 生产厂家
            //            string[] str = new string[] {
            //"积成电子股份有限公司                      "
            //,"江苏华源仪器仪表有限公司                  "
            //,"江苏林洋电子股份有限公司                  "
            //,"江苏西欧电子有限公司                      "
            //,"江苏永泰电器有限公司                      "
            //,"牡丹江互感器厂                            "
            //,"南京智达电气有限公司                      "
            //,"宁波迦南电子有限公司                      "
            //,"宁波三星电气股份有限公司                  "
            //,"宁夏隆基宁光仪表有限公司                  "
            //,"青岛乾程电子科技有限公司                  "
            //,"上海金陵智能电表有限公司                  "
            //,"上海协同科技股份有限公司                  "
            //,"深圳市宝利达实业有限公司                  "
            //,"深圳市国电科技通信有限公司                "
            //,"深圳市航天泰瑞捷电子有限公司              "
            //,"深圳市精正达电力仪表有限公司              "
            //,"深圳市先行电气技术有限公司                "
            //,"深圳市友讯达科技发展有限公司              "
            //,"深圳长城开发科技股份有限公司              "
            //,"威胜集团有限公司                          "
            //,"潍坊五洲浩特电气有限公司                  "
            //,"日新电机(无锡)有限公司                    "
            //,"无锡威达电工仪表有限公司                  "
            //,"武汉高压研究所新技术公司华电电气技术有限公"
            //,"武汉盛帆电子股份有限公司                  "
            //,"西安亮丽仪器仪表有限责任公司              "
            //,"烟台东方威思顿电气有限公司                "
            //,"浙江八达电子仪表有限公司                  "
            //,"浙江恒业电子有限公司                      "
            //,"杭州华隆电力仪表有限公司                  "
            //,"浙江顺舟电力高技术有限公司                "
            //,"浙江万胜电力仪表有限公司                  "
            //,"浙江正泰仪器仪表有限责任公司              "
            //,"中电装备山东电子有限公司                  "
            //,"深圳市科陆电子科技股份有限公司            "
            //,"国电南瑞科技股份有限公司                  "
            //,"南京能瑞自动化设备股份有限公司            "
            //,"杭州海兴电力科技股份有限公司              "
            //,"南京新联电子股份有限公司                  "
            //,"深圳浩宁达仪表股份有限公司                "
            //,"华立仪表集团股份有限公司                  "
            //,"北京电研华源电力技术有限公司              "
            //,"浙江共同电子科技有限公司                  "
            //,"科大智能科技股份有限公司                  "
            //,"北京博纳电气股份有限公司                  "
            //,"晨泰集团有限公司                          "
            //,"长沙威胜信息技术有限公司                  "
            //,"东方电子股份有限公司                      "
            //,"北京和利时恒业科技有限公司                "
            //,"北京清华联电器制造有限公司                "
            //,"兰吉尔仪表系统（珠海）有限公司            "
            //,"天津市新巨升电子工业有限责任公司          "
            //,"北京华新电工设备有限公司                  "
            //,"从兴技术有限公司                          "
            //,"宁波三维电测设备有限公司                  "
            //,"北京四方电气设备有限公司                  "
            //,"北京新鸿基瑞程科技有限公司                "
            //,"天津三达电气有限公司                      "
            //,"保定天威保变电气股份有限公司              "
            //,"大连北方互感器集团有限公司                "
            //,"中山市国能电器有限公司                    "
            //,"广东四会互感器厂有限公司                  "
            //,"杭州电表厂                                "
            //,"保定市电力互感器厂                        "
            //,"衡阳变压器厂                              "
            //,"江苏靖江互感器厂有限公司                  "
            //,"北京瑞奇恩电器设备有限公司                "
            //,"任丘市长城互感器有限公司                  "
            //,"上海人民企业集团电器制造有限公司          "
            //,"天津市百利纽泰克电器有限公司              "
            //,"西安西电高压开关有限责任公司              "
            //,"江苏科兴电器有限公司                      "
            //,"镇江市丹高电器有限公司                    "
            //,"中山市泰峰电气有限公司                    "
            //,"南京巨冉电力设备制造有限公司              "
            //,"湖州新荣机电设备制造有限公司              "
            //,"湖南电力电磁电器厂                        "
            //,"保定凯宇电器有限公司                      "
            //,"北京华强诚信电器有限公司                  "
            //,"北京双成易电信工程有限公司                "
            //,"北京中电创微科技发展有限公司              "
            //,"滨州市华新电力科技有限公司                "
            //,"大连第二互感器集团有限公司                "
            //,"庆云县昆仑锁业暨电子有限公司              "
            //,"庆云昆泰电力器材有限公司                  "
            //,"山东锐进电力器材有限公司                  "
            //,"山东卓越电力器材有限公司                  "
            //,"北京恒源利通电力技术有限公司              "
            //,"山东盛海电器有限公司                      "
            //,"浙江宏睿通信技术有限公司                  "
            //,"其他供应商                                "
            //};
            //            int index = 19;
            //            for (int i = 0; i < str.Length; i++)
            //            {
            //                //添加生产厂家
            //                CodeTreeNode nodeNew = new CodeTreeNode()
            //                {
            //                    CODE_LEVEL = CODE_LEVEL + 1,
            //                    CODE_CATEGORY = CODE_CATEGORY,
            //                    CODE_NAME = str[i].Trim(),
            //                    CODE_VALUE = index.ToString().PadLeft(3, '0'),
            //                    CODE_TYPE = "",
            //                    CODE_PARENT = CODE_TYPE,
            //                    CODE_ENABLED = true,
            //                    Parent = this,
            //                };
            //                Children.Add(nodeNew);
            //                index++;
            //            }
            #endregion




        }
        /// <summary>
        /// 非终结点不允许删除
        /// </summary>
        public void DeleteCode()
        {
            List<int> idList = GetIdList(this);
            List<string> conditionList = new List<string>();
            for (int i = 0; i < idList.Count; i++)
            {
                conditionList.Add(string.Format("id={0}", idList[i]));
            }
            if (conditionList.Count > 0)
            {
                string where = string.Join(" or ", conditionList);
                int deleteCount = DALManager.ApplicationDbDal.Delete(EnumAppDbTable.T_CODE_TREE.ToString(), where);
                LogManager.AddMessage(string.Format("删除数据库中编码{0}及其所有子元素完成,共删除{1}条记录.", where, deleteCount));
            }
            if (Parent != null)
            {
                Parent.Children.Remove(this);
            }
        }
        public bool DeleteCodeOfMeterFactory(string ID)
        {
            string where = $"code_parent='MeterFactory' and id={ID}";
            int deleteCount = DALManager.ApplicationDbDal.Delete(EnumAppDbTable.T_CODE_TREE.ToString(), where);
            if (deleteCount > 0)
            {
                if (Children != null)
                {
                    Children.Remove(Children.Single(v => v.ID == int.Parse(ID)));
                }
            }
            LogManager.AddMessage(string.Format("删除数据库中编码{0}元素完成,共删除{1}条记录.", where, deleteCount));
            return deleteCount > 0;
        }
        public bool DeleteCodeOfCodeAndIdAndName(string ID, string CODE_TYPE, string CODE_NAME)
        {
            string where = $"code_parent='{CODE_TYPE}' and id={ID} and code_cn_name='{CODE_NAME}'";
            int deleteCount = DALManager.ApplicationDbDal.Delete(EnumAppDbTable.T_CODE_TREE.ToString(), where);
            if (deleteCount > 0)
            {
                if (Children != null)
                {
                    Children.Remove(Children.Single(v => v.ID == int.Parse(ID)));
                }
            }
            LogManager.AddMessage(string.Format("删除数据库中编码{0}元素完成,共删除{1}条记录.", where, deleteCount));
            return deleteCount > 0;
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
        /// 保存编码
        /// </summary>
        public bool  SaveCode()
        {
            #region 先将子节点的父元素更新
            if (string.IsNullOrWhiteSpace(this.CODE_PARENT))
            {
                LogManager.AddMessage("父节点的英文名称不能为空,保存失败!!!", EnumLogSource.数据库存取日志, EnumLevel.Tip);
                return false;

            }
            EditChildren(this);
            #endregion
            #region 先更新要编辑的点
            List<DynamicModel> modelList = GetEditModels(this);
            if (modelList.Count > 0)
            {
                List<string> fieldNames = new List<string>();
                if (modelList.Count > 0)
                {
                    fieldNames = modelList[0].GetAllProperyName();
                }
                fieldNames.Remove("ID");
                int updateCount = DALManager.ApplicationDbDal.Update(EnumAppDbTable.T_CODE_TREE.ToString(), "ID", modelList, fieldNames);
                LogManager.AddMessage(string.Format("编码:{0}编辑完成,共执行{1}条数据的更新", CODE_NAME, updateCount), EnumLogSource.数据库存取日志);


            }
            #endregion
            #region 再插入新增的点
            InsertCode(this);
            return true;
            #endregion
        }
        /// <summary>
        /// 插入编码,ID号为0的编码插入比较繁琐,具体流程如下
        /// </summary>
        private void InsertCode(CodeTreeNode nodeToInsert)
        {
            if (nodeToInsert.FlagChanged && nodeToInsert.ID == 0)
            {
                #region 步骤1:插入数据库
                DynamicModel modelNew = nodeToInsert.GetModel();
                int insertCount = DALManager.ApplicationDbDal.Insert(EnumAppDbTable.T_CODE_TREE.ToString(), modelNew);
                if (insertCount > 0)
                {
                    LogManager.AddMessage(string.Format("编码{0}插入数据库成功", nodeToInsert.CODE_NAME), EnumLogSource.数据库存取日志);
                }
                else
                {
                    LogManager.AddMessage(string.Format("编码{0}插入数据库失败", nodeToInsert.CODE_NAME), EnumLogSource.数据库存取日志, EnumLevel.Warning);
                    MessageBox.Show(string.Format("编码{0}插入失败", nodeToInsert.CODE_NAME));
                    return;
                }
                nodeToInsert.FlagChanged = false;
                #endregion
                #region 步骤2:获取刚插入的数据模型并更新编号
                DynamicModel modelTemp = DALManager.ApplicationDbDal.GetByID(EnumAppDbTable.T_CODE_TREE.ToString(), string.Format("CODE_PARENT='{0}' order by ID desc", nodeToInsert.CODE_PARENT));
                if (modelTemp != null)
                {
                    int idTemp = (int)(modelTemp.GetProperty("ID"));
                    nodeToInsert.ID = idTemp;
                }
                #endregion
            }
            for (int i = 0; i < nodeToInsert.Children.Count; i++)
            {
                InsertCode(nodeToInsert.Children[i]);
            }
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

        /// <summary>
        /// 当前用户权限
        /// </summary>
        public string CurrentPermission
        {
            get { return User.UserViewModel.Instance.CurrentUser.GetProperty("USER_POWER") as string; }
        }
    }
}
