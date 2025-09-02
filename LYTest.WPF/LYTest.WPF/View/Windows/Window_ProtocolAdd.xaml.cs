using LYTest.DAL;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Xml;

namespace LYTest.WPF.View.Windows
{
    /// <summary>
    /// Window_ProtocolAdd.xaml 的交互逻辑
    /// </summary>
    public partial class Window_ProtocolAdd : Window
    {

        //private static Window_ProtocolAdd instance = null;

        //public static Window_ProtocolAdd Instance
        //{
        //    get
        //    {
        //        if (instance == null)
        //        {
        //            instance = new Window_ProtocolAdd();
        //        }
        //        return instance;
        //    }
        //}
        //private new void Show()
        //{

        //    Dispatcher.Invoke(new Action(() =>
        //    {
        //        try
        //        {
        //            Visibility = Visibility.Visible;
        //            Show();
        //        }
        //        catch
        //        { }
        //    }));
        //}

        public List<DynamicModel> models;
        public Window_ProtocolAdd(List<DynamicModel> model)
        {
            InitializeComponent();
            models = model;
            //Visibility = Visibility.Collapsed;
            LoadXmlFromFile();
            Set_Checled();
            //Sort(flagItems);
            this.Topmost = true;
            //1:可以获取是否已经有了，默认选中
            //2:搜索功能
            //3：全选功能
            //4:写入内容有可能也有了

        }

        readonly ObservableCollection<DataFlagItem> flagItems = new ObservableCollection<DataFlagItem>();
        readonly ObservableCollection<DataFlagItem> soures = new ObservableCollection<DataFlagItem>();



        private readonly string filePath = string.Format(@"{0}\Xml\DataFlag.xml", Directory.GetCurrentDirectory());

        private XmlNode nodeDataFlags = null;
        /// <summary>
        /// 从文件加载
        /// </summary>
        private void LoadXmlFromFile()
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(filePath);
            nodeDataFlags = doc.DocumentElement;
            foreach (XmlNode n in nodeDataFlags)
            {
                if (n.Attributes["DataFlagDiName"] == null || n.Attributes["DataFlagDiName"].Value == "") continue;
                if (flagItems.Count(item => item.Name == n.Attributes["DataFlagDiName"].Value) == -1) continue;

                DataFlagItem data = new DataFlagItem
                {
                    Name = n.Attributes["DataFlagDiName"].Value,
                    DataFlag = n.Attributes["DataFlag"] != null ? n.Attributes["DataFlag"].Value : "",
                    DataFlag698 = n.Attributes["DataFlag698"] != null ? n.Attributes["DataFlag698"].Value : "",
                    Length = n.Attributes["DataLength"] != null ? n.Attributes["DataLength"].Value : "",
                    DataFormat = n.Attributes["DataFormat"] != null ? n.Attributes["DataFormat"].Value : "",
                    ReadData = n.Attributes["ReadData"] != null ? n.Attributes["ReadData"].Value : "",
                    DotLength = n.Attributes["DataSmallNumber"] != null ? n.Attributes["DataSmallNumber"].Value : "",
                    IsCheck = false,
                    Function = "读"
                };

                flagItems.Add(data);
                soures.Add(data);
            }
            protocolItem.ItemsSource = flagItems;
        }

        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //Visibility = Visibility.Collapsed;
            this.Close();
        }

        /// <summary>
        /// 设置选中的项目
        /// </summary>
        private void Set_Checled()
        {
            for (int i = 0; i < models.Count; i++)
            {
                if (!(models[i].GetProperty("PARA_VALUE") is string str2)) continue;
                string name = str2.Split('|')[0];
                if (name == "") continue;
                DataFlagItem item1 = flagItems.First(item => item.Name == name);
                if (item1 != null)
                {
                    item1.IsCheck = true;
                    item1.ReadData = str2.Split('|')[6];
                }
            }
        }

        // 搜索
        private void OnSearch(object sender, RoutedEventArgs e)
        {
            string txt = txt_Search.Text;
            var T = soures.ToList();
            var a = T.Where(item => item.Name.IndexOf(txt) != -1 || item.DataFlag.IndexOf(txt) != -1 || item.DataFlag698.IndexOf(txt) != -1);

            flagItems.Clear();
            foreach (var item in a)
            {
                flagItems.Add(item);
            }
            //Sort(flagItems);
        }

        ///// <summary>
        ///// 对当前的进行一个排序
        ///// </summary>
        //private void Sort(ObservableCollection<DataFlagItem> Items)
        //{
        //    return;
            //var T = Items.ToList();
            //var a = T.Where(item => item.IsCheck == true);
            //var b = T.Where(item => item.IsCheck == false);
            //Items.Clear();
            //foreach (var item in a)
            //{
            //    Items.Add(item);
            //}
            //foreach (var item in b)
            //{
            //    Items.Add(item);
            //}
        //}

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //Sort(flagItems);
            //string txt = txt_Search.Text;
            //var T = soures.ToList();
            //var a = T.Where(item => item.IsCheck==true);
            //var b = T.Where(item => item.IsCheck == false);

            //flagItems.Clear();
            //foreach (var item in a)
            //{
            //    flagItems.Add(item);
            //}
            //foreach (var item in b)
            //{
            //    flagItems.Add(item);
            //}

        }

        //public bool dialogResult = false;

        /// <summary>
        /// 选中的项目
        /// </summary>

        public ObservableCollection<DataFlagItem> selectItems = new ObservableCollection<DataFlagItem>();
        private void Btn_Add(object sender, RoutedEventArgs e)
        {
            selectItems.Clear();
            var a = soures.Where(item => item.IsCheck == true);
            foreach (var item in a)
            {
                selectItems.Add(item);
            }
            DialogResult = true;
        }

        ////选中
        //private void MenuI_IsCheck(object sender, RoutedEventArgs e)
        //{
        //    //if (protocolItem.SelectedItems == null) return;
        //    //foreach (var item in protocolItem.SelectedItems)
        //    //{

        //    //}
        //}

        //取消选中
        private void MenuI_IsClose(object sender, RoutedEventArgs e)
        {

        }
    }


}
