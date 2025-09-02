using LYTest.MeterProtocol.DataFlag;
using LYTest.Utility.Log;
using LYTest.ViewModel;
using System;
using System.IO;
using System.Windows;
using System.Windows.Data;
using System.Xml;

namespace LYTest.WPF.SG.View
{
    /// <summary>
    /// View_DataFlag.xaml 的交互逻辑
    /// </summary>
    public partial class View_DataFlag
    {
        public View_DataFlag()
        {
            InitializeComponent();
            Name = "数据标识";
            LoadXmlFromFile();
        }
        private XmlDataProvider DataFlagDict
        { get { return Resources["DataFlag"] as XmlDataProvider; } }

        /// <summary>
        /// 添加的数据标识的内容
        /// </summary>
        private DataFlagItem FlagItemTemp
        {
            get { return Resources["DataItemTemp"] as DataFlagItem; }
        }

        private readonly string filePath = string.Format(@"{0}\Xml\DataFlag.xml", Directory.GetCurrentDirectory());
        /// <summary>
        /// 从文件加载
        /// </summary>
        private void LoadXmlFromFile()
        {
            DataFlagDict.Source = new Uri(filePath, UriKind.Absolute);
        }


        //保存
        private void Click_Save(object sender, RoutedEventArgs e)
        {
            DataFlagDict.Document.Save(filePath);
            //CodeDictionary.LoadDataFlagNames();

            DataFlagS.LoadDataFlag();
            LogManager.AddMessage("保存成功", EnumLogSource.用户操作日志, EnumLevel.Tip);
        }

        //添加
        private void Click_Add(object sender, RoutedEventArgs e)
        {
            XmlDocument docTemp = DataFlagDict.Document;
            XmlNode nodeParent = docTemp.DocumentElement;
            for (int i = 0; i < nodeParent.ChildNodes.Count; i++)
            {
                try
                {
                    if (nodeParent.ChildNodes[i].Attributes["DataFlagDiName"].Value == FlagItemTemp.Name)
                    {
                        LogManager.AddMessage("该标识名称已经存在,请更改标识名称!!", EnumLogSource.用户操作日志, EnumLevel.Tip);
                        return;
                    }
                }
                catch
                { }
            }
            XmlElement dataItem = docTemp.CreateElement("R");
            XmlAttribute attributeName = docTemp.CreateAttribute("DataFlagDiName");
            dataItem.Attributes.Append(attributeName);
            dataItem.Attributes["DataFlagDiName"].Value = FlagItemTemp.Name;

            XmlAttribute attributeDataFlag = docTemp.CreateAttribute("DataFlagDi");
            dataItem.Attributes.Append(attributeDataFlag);
            dataItem.Attributes["DataFlagDi"].Value = FlagItemTemp.DataFlag;

            XmlAttribute attributeDataFlag698 = docTemp.CreateAttribute("DataFlagOi");
            dataItem.Attributes.Append(attributeDataFlag698);
            dataItem.Attributes["DataFlagOi"].Value = FlagItemTemp.DataFlag698;

            XmlAttribute attributeLength = docTemp.CreateAttribute("DataLength");
            dataItem.Attributes.Append(attributeLength);
            dataItem.Attributes["DataLength"].Value = FlagItemTemp.Length;

            XmlAttribute attributeDotNumber = docTemp.CreateAttribute("DataSmallNumber");
            dataItem.Attributes.Append(attributeDotNumber);
            dataItem.Attributes["DataSmallNumber"].Value = FlagItemTemp.DotLength;

            XmlAttribute attributeFormat = docTemp.CreateAttribute("DataFormat");
            dataItem.Attributes.Append(attributeFormat);
            dataItem.Attributes["DataFormat"].Value = FlagItemTemp.DataFormat;

            XmlAttribute ClassName = docTemp.CreateAttribute("ClassName");
            dataItem.Attributes.Append(ClassName);
            dataItem.Attributes["ClassName"].Value = FlagItemTemp.ClassName;

            XmlAttribute EmSecurityMode = docTemp.CreateAttribute("EmSecurityMode");
            dataItem.Attributes.Append(EmSecurityMode);
            dataItem.Attributes["EmSecurityMode"].Value = FlagItemTemp.EmSecurityMode;

            XmlAttribute Rights = docTemp.CreateAttribute("Rights");
            dataItem.Attributes.Append(Rights);
            dataItem.Attributes["Rights"].Value = FlagItemTemp.Rights;

            XmlAttribute Chip = docTemp.CreateAttribute("Chip");
            dataItem.Attributes.Append(Chip);
            dataItem.Attributes["Chip"].Value = FlagItemTemp.Chip;

            XmlAttribute SortNo = docTemp.CreateAttribute("SortNo");
            dataItem.Attributes.Append(SortNo);
            dataItem.Attributes["SortNo"].Value = FlagItemTemp.SortNo;
            //XmlAttribute readData = docTemp.CreateAttribute("ReadData");
            //dataItem.Attributes.Append(readData);
            //dataItem.Attributes["ReadData"].Value = flagItemTemp.ReadData;

            //XmlAttribute attributeDefault = docTemp.CreateAttribute("Default");
            //dataItem.Attributes.Append(attributeDefault);
            //dataItem.Attributes["Default"].Value = flagItemTemp.DataFormat;

            //DataFlag698
            //ReadData
            nodeParent.AppendChild(dataItem);
            LogManager.AddMessage("标识名称已添加,点击保存按钮后生效", EnumLogSource.用户操作日志, EnumLevel.Tip);
        }

    }

    /// <summary>
    /// 数据标识项
    /// </summary>
    public class DataFlagItem : ViewModelBase
    {
        private string name;
        /// <summary>
        /// 名称
        /// </summary>
        public string Name
        {
            get { return name; }
            set { SetPropertyValue(value, ref name, "Name"); }
        }
        private string dataFlag;
        /// <summary>
        /// 数据标识
        /// </summary>
        public string DataFlag
        {
            get { return dataFlag; }
            set { SetPropertyValue(value, ref dataFlag, "DataFlag"); }
        }
        private string dataFlag698;
        /// <summary>
        /// 数据标识 698
        /// </summary>
        public string DataFlag698
        {
            get { return dataFlag698; }
            set { SetPropertyValue(value, ref dataFlag698, "DataFlag698"); }
        }

        private string length;
        /// <summary>
        /// 数据长度
        /// </summary>
        public string Length
        {
            get { return length; }
            set { SetPropertyValue(value, ref length, "Length"); }
        }
        private string dotLength;
        /// <summary>
        /// 小数位数
        /// </summary>
        public string DotLength
        {
            get { return dotLength; }
            set
            {
                SetPropertyValue(value, ref dotLength, "DotLength");
            }
        }
        private string dataFormat;
        /// <summary>
        /// 数据格式
        /// </summary>
        public string DataFormat
        {
            get { return dataFormat; }
            set { SetPropertyValue(value, ref dataFormat, "DataFormat"); }
        }

        private string readData;
        /// <summary>
        /// 写入内容
        /// </summary>
        public string ReadData
        {
            get { return readData; }
            set { SetPropertyValue(value, ref readData, "ReadData"); }
        }

        private string funmction;
        /// <summary>
        /// 功能。读还是写
        /// </summary>
        public string Function
        {
            get { return funmction; }
            set { SetPropertyValue(value, ref funmction, "Function"); }
        }
        private bool isCheck;
        /// <summary>
        /// 是否选中
        /// </summary>
        public bool IsCheck
        {
            get { return isCheck; }
            set { SetPropertyValue(value, ref isCheck, "IsCheck"); }
        }
        private string chip;
        /// <summary>
        /// 所属的芯片，0管理芯片,1计量芯片，2-??其他扩展芯片
        /// </summary>
        public string Chip
        {
            get { return chip; }
            set { SetPropertyValue(value, ref chip, "Chip"); }
        }
        private string sortNo;
        /// <summary>
        /// 排序号
        /// </summary>
        public string SortNo
        {
            get { return sortNo; }
            set { SetPropertyValue(value, ref sortNo, "SortNo"); }
        }
        private string emSecurityMode;
        /// <summary>
        /// 安全模式
        /// </summary>
        public string EmSecurityMode
        {
            get { return emSecurityMode; }
            set { SetPropertyValue(value, ref emSecurityMode, "EmSecurityMode"); }
        }
        private string className;
        /// <summary>
        /// 所属类名称
        /// </summary>
        public string ClassName
        {
            get { return className; }
            set { SetPropertyValue(value, ref className, "ClassName"); }
        }

        private string rights;
        /// <summary>
        /// 注释
        /// </summary>
        public string Rights
        {
            get { return rights; }
            set { SetPropertyValue(value, ref rights, "Rights"); }
        }




    }
}
