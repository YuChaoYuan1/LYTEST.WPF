using LYTest.Utility.Log;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace LYTest.WPF.SG.View.Windows
{
    /// <summary>
    /// Window_ThemeChoice.xaml 的交互逻辑
    /// </summary>
    public partial class Window_ThemeChoice : Window
    {
        private static Window_ThemeChoice instance = null;

        public static Window_ThemeChoice Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new Window_ThemeChoice();
                }
                return instance;
            }
        }
        private new void Show()
        {

            Dispatcher.Invoke(new Action(() =>
            {
                try
                {
                    Visibility = Visibility.Visible;
                    Show();
                }
                catch
                { }
            }));
        }

        private readonly List<string> list = new List<string>();

        public Window_ThemeChoice()
        {
            InitializeComponent();
            Visibility = Visibility.Collapsed;
            Topmost = true;

            string strPath = Directory.GetCurrentDirectory();
            string ThemeName = Core.OperateFile.GetINI("SkinList", "SkinCurrent", strPath + "\\Skin\\SkinData.ini", "默认");  //获得当前设计的颜色
            ThemePic1.Source = LoadPic(ThemeName);

            for (int i = 0; i < 30; i++)
            {
                string str = Core.OperateFile.GetINI("SkinList", i.ToString(), strPath + "\\Skin\\SkinData.ini").Trim();  //保存当前设计的颜色
                if (str != "")
                {
                    list.Add(str);
                }
            }
            comboBoxTheme.ItemsSource = list;
            comboBoxTheme.SelectedItem = ThemeName;
            //载入当前的主题到ThemePic1

        }

        private ImageSource LoadPic(string name)
        {
            ImageSource bmp;
            string strPath = Directory.GetCurrentDirectory();
            string path = strPath + "//Images//ThemeImage//" + name + ".png";
            if (File.Exists(path))
            {
                bmp = new BitmapImage(new Uri(path, UriKind.RelativeOrAbsolute));
                return bmp;
            }
            return null;


        }


        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        //保存
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (comboBoxTheme.SelectedItem.ToString().Trim() != "")
                {
                    string strPath = Directory.GetCurrentDirectory();
                    Core.OperateFile.WriteINI("SkinList", "SkinCurrent", comboBoxTheme.SelectedItem.ToString().Trim(), strPath + "\\Skin\\SkinData.ini");  //保存当前设计的颜色
                    LogManager.AddMessage("保存主题成功,重启程序生效", EnumLogSource.用户操作日志, EnumLevel.Warning);
                }
            }
            catch (Exception)
            {
                LogManager.AddMessage("保存失败", EnumLogSource.用户操作日志, EnumLevel.Warning);
            }
            Visibility = Visibility.Collapsed;
        }

        private void ComboBoxTheme_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //选中主题改变,切换ThemePic2的图片
            if (comboBoxTheme.SelectedItem == null)
            {
                return;
            }
            ThemePic2.Source = LoadPic(comboBoxTheme.SelectedItem.ToString());
        }


        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Visibility = Visibility.Collapsed;
        }
    }
}
