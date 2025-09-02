using LYTest.Utility.Log;
using System;
using System.IO;
using System.Windows;
using System.Windows.Data;
using System.Xml;

namespace LYTest.WPF.SG.View
{
    /// <summary>
    /// AgreementConfig.xaml 的交互逻辑
    /// </summary>
    public partial class View_AgreementConfig
    {
        public View_AgreementConfig()
        {
            InitializeComponent();
            Name = "协议配置";
            DockStyle.IsFloating = false;
            LoadXmlFromFile();
            LoadXmlrCarrierFromFile();  //载入载波协议xml文件

            Loaded -= View_AgreementConfig_Loaded;
            Loaded += View_AgreementConfig_Loaded;
        }

        private void View_AgreementConfig_Loaded(object sender, RoutedEventArgs e)
        {

            XmlDocument docTemp = ProviderProtocol.Document;
            XmlNodeList nodeListTemp = docTemp.SelectNodes("DgnProtocol/Protocols");
            if (nodeListTemp != null)
            {
                XmlNodeList nodeList = docTemp.SelectNodes("DgnProtocol/DefaultProtocols/R");
                foreach (XmlNode nodeTemp in nodeList)
                {
                    try
                    {
                        if (nodeTemp.InnerXml.IndexOf("DecimalDigits") == -1)
                        {
                            XmlElement node = docTemp.CreateElement("DecimalDigits");
                            node.InnerText = "2";
                            nodeTemp.AppendChild(node);
                        }
                    }
                    catch
                    { }

                    foreach (XmlNode subTemp in nodeTemp.ChildNodes)
                    {
                        try
                        {
                            if (subTemp.Name == "FeiLvId")
                            {
                                var item = subTemp.Attributes.GetNamedItem("ShenGu");
                                if (item == null)
                                {
                                    var attr = docTemp.CreateAttribute("ShenGu");
                                    attr.InnerText = "5";
                                    subTemp.Attributes.Append(attr);
                                }
                            }
                        }
                        catch
                        { }
                    }
                }
                nodeList = docTemp.SelectNodes("DgnProtocol/Protocols/R");
                foreach (XmlNode nodeTemp in nodeList)
                {
                    try
                    {
                        if (nodeTemp.InnerXml.IndexOf("DecimalDigits") == -1)
                        {
                            XmlElement node = docTemp.CreateElement("DecimalDigits");
                            node.InnerText = "2";
                            nodeTemp.AppendChild(node);
                        }
                    }
                    catch
                    { }

                    foreach (XmlNode subTemp in nodeTemp.ChildNodes)
                    {
                        try
                        {
                            if (subTemp.Name == "FeiLvId")
                            {
                                var item = subTemp.Attributes.GetNamedItem("ShenGu");
                                if (item == null)
                                {
                                    var attr = docTemp.CreateAttribute("ShenGu");
                                    attr.InnerText = "5";
                                    subTemp.Attributes.Append(attr);
                                }
                            }
                        }
                        catch
                        { }
                    }
                }
            }
        }

        /// <summary>
        /// 设置默认选中项
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DockControlDisposable_Loaded(object sender, RoutedEventArgs e)
        {
            if (listBox.Items.Count > 0)
                listBox.SelectedIndex = 0;
            if (list_ZB.Items.Count > 0)
                list_ZB.SelectedIndex = 0;

        }



        #region 协议部分

        private XmlDataProvider ProviderProtocol
        { get { return Resources["ProviderMeterProtocol"] as XmlDataProvider; } }

        /// <summary>
        /// 路径
        /// </summary>
        private readonly string filePath = string.Format(@"{0}\xml\AgreementConfig.xml", Directory.GetCurrentDirectory());



        private void LoadXmlFromFile()
        {
            ProviderProtocol.Source = new Uri(filePath, UriKind.Absolute);
        }

        /// <summary>
        /// 删除当前协议
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Del_Click(object sender, RoutedEventArgs e)
        {

            listBox.SelectedItem = (e.OriginalSource as FrameworkElement).DataContext;//先选中该项

            //先判断是否选中项，并且选中项是xml节点
            if (listBox.SelectedItem != null && listBox.SelectedItem is XmlNode node)
            {
                XmlNode elementTemp = node;    //类型转换
                if (elementTemp.ParentNode != null && elementTemp.Name == "R") //判断是否有父节点，并且是数据节点
                {
                    elementTemp.ParentNode.RemoveChild(elementTemp);   //删除节点
                }
            }
        }

        /// <summary>
        /// 添加新的协议
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Add_Click(object sender, RoutedEventArgs e)
        {
            XmlDocument docTemp = ProviderProtocol.Document;
            XmlNodeList nodeListTemp = docTemp.SelectNodes("DgnProtocol/Protocols");
            if (nodeListTemp != null)
            {
                XmlNode nodeParent = nodeListTemp[0];
                XmlNodeList nodeList = docTemp.SelectNodes("DgnProtocol/DefaultProtocols/R");
                foreach (XmlNode nodeTemp in nodeList)
                {
                    try
                    {
                        if (nodeTemp.Attributes["Name"].Value == "DLT645-2007-Default")
                        {
                            XmlNode nodeNew = nodeTemp.Clone();
                            nodeNew.Attributes["Name"].Value = "新建协议";
                            nodeParent.AppendChild(nodeNew);
                            break;
                        }
                    }
                    catch
                    { }
                }
            }
        }


        /// <summary>
        /// 保存配置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Btn_Save(object sender, RoutedEventArgs e)
        {
            ProviderProtocol.Document.Save(filePath);
            LogManager.AddMessage("表协议配置信息保存成功!!", EnumLogSource.用户操作日志, EnumLevel.Tip);
        }

        #endregion

        #region 载波部分

        private XmlDataProvider ProviderCarrierProtocol

        { get { return Resources["ProviderCarrierProtocol"] as XmlDataProvider; } }
        private readonly string ZB_filePath = string.Format(@"{0}\xml\CarrierConfig.xml", Directory.GetCurrentDirectory());

        /// <summary>
        /// 从文件加载
        /// </summary>
        private void LoadXmlrCarrierFromFile()
        {
            ProviderCarrierProtocol.Source = new Uri(ZB_filePath, UriKind.Absolute);
        }


        /// <summary>
        /// 添加新的载波协议
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ZB_Btn_Add(object sender, RoutedEventArgs e)
        {
            XmlDocument docTemp = ProviderCarrierProtocol.Document;
            XmlNodeList nodeListTemp = docTemp.SelectNodes("CarrierProtocol/Protocols");
            if (nodeListTemp != null)
            {
                XmlNode nodeParent = nodeListTemp[0];
                XmlNodeList nodeList = docTemp.SelectNodes("CarrierProtocol/DefaultProtocols/R");
                foreach (XmlNode nodeTemp in nodeList)
                {
                    try
                    {
                        if (nodeTemp.Attributes["Name"].Value == "新建载波协议")
                        {
                            XmlNode nodeNew = nodeTemp.Clone();
                            //nodeNew.Attributes["Name"].Value = "新建载波协议";
                            nodeParent.AppendChild(nodeNew);
                            break;
                        }
                    }
                    catch
                    { }
                }
            }
        }

        /// <summary>
        /// 删除载波协议
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ZB_Btn_Del(object sender, RoutedEventArgs e)
        {
            list_ZB.SelectedItem = (e.OriginalSource as FrameworkElement).DataContext;//先选中该项

            //先判断是否选中项，并且选中项是xml节点
            if (list_ZB.SelectedItem != null && list_ZB.SelectedItem is XmlNode node)
            {
                XmlNode elementTemp = node;    //类型转换
                if (elementTemp.ParentNode != null && elementTemp.Name == "R") //判断是否有父节点，并且是数据节点
                {
                    elementTemp.ParentNode.RemoveChild(elementTemp);   //删除节点
                }
            }
        }

        /// <summary>
        /// 载波保存
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ZB_Btn_Save(object sender, RoutedEventArgs e)
        {
            ProviderCarrierProtocol.Document.Save(ZB_filePath);
            LogManager.AddMessage("载波配置信息保存成功!!", EnumLogSource.用户操作日志, EnumLevel.Tip);
        }



        #endregion

        private void ListBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {

        }
    }
}
