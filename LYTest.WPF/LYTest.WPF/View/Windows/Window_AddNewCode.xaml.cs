using LYTest.DAL;
using LYTest.ViewModel.CodeTree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace LYTest.WPF.View.Windows
{
    /// <summary>
    /// Window_AddNewCode.xaml 的交互逻辑
    /// </summary>
    public partial class Window_AddNewCode : Window
    {
        public Window_AddNewCode(CodeTreeNode CodeNode)
        {
            InitializeComponent();
            nodeTemp = CodeNode;
            DataContext = CodeNode;
            Topmost = true;
        }


        private CodeTreeNode nodeTemp { get; set; }

        private void Click_Enter(object sender, RoutedEventArgs e)
        {
            if (CheckAndSaveCode(chkAutoCode.IsChecked == true))
            {
                DialogResult = true;
                Close();
            }
        }
        private bool CheckAndSaveCode(bool autoCode)
        {
            string nameTemp = textBoxName.Text;
            string valueTemp = textBoxValue.Text;
            int intTemp = 0;

            if (nodeTemp.CODE_TYPE == "CurrentValue" || nodeTemp.CODE_TYPE == "ConstantValue" || nodeTemp.CODE_TYPE == "AccuracyClass")
            {
                nameTemp = nameTemp.Replace("（", "(").Replace("）", ")");
            }

            if (string.IsNullOrEmpty(nameTemp))
            {
                MessageBox.Show("新添加的编码名称不能为空!!");
                return false;
            }
            if (string.IsNullOrEmpty(valueTemp) && !autoCode)
            {
                MessageBox.Show("新添加的编码值不能为空!!");
                return false;
            }

            if (!int.TryParse(valueTemp, out intTemp) && !autoCode)
            {
                MessageBox.Show("新添加的编码值必须为数字!!");
            }
            else
            {
                CodeTreeNode nodeTemp1 = nodeTemp.Children.FirstOrDefault(item => item.CODE_NAME == nameTemp);
                CodeTreeNode nodeTemp2 = nodeTemp.Children.FirstOrDefault(item => item.CODE_VALUE == valueTemp);
                if (nodeTemp1 != null)
                {
                    MessageBox.Show("编码名称已经存在,请输入新的值!!");
                    return false;
                }

                if (autoCode)
                {
                    for (int i = 0; i < 99; i++)
                    {
                        valueTemp = (++intTemp).ToString();
                        nodeTemp2 = nodeTemp.Children.FirstOrDefault(item => item.CODE_VALUE == valueTemp);
                        if (nodeTemp2 == null) break;
                    }
                }
                else
                {
                    if (nodeTemp2 != null)
                    {
                        MessageBox.Show("编码值已经存在,请输入新的值!!");
                        return false;
                    }
                }

                nodeTemp.Children.Add(new CodeTreeNode()
                {
                    CODE_LEVEL = nodeTemp.CODE_LEVEL + 1,
                    CODE_PARENT = nodeTemp.CODE_TYPE,
                    CODE_NAME = nameTemp,
                    CODE_VALUE = valueTemp,
                    CODE_ENABLED = true,
                    FlagChanged = true
                });
                nodeTemp.SaveCode();
                Dictionary<string, string> dictionary = CodeDictionary.GetLayer2(nodeTemp.CODE_TYPE);
                dictionary.Add(nameTemp, valueTemp);
                return true;

            }
            return false;
        }

        private void Click_Quit(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void DeleteItem_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            if (btn != null)
            {
                var node = btn.DataContext as CodeTreeNode;
                if (node != null)
                {
                    if (nodeTemp.DeleteCodeOfCodeAndIdAndName(node.ID.ToString(), nodeTemp.CODE_TYPE, node.CODE_NAME))
                    {
                        Dictionary<string, string> dictionary = CodeDictionary.GetLayer2(nodeTemp.CODE_TYPE);
                        dictionary.Remove(node.CODE_NAME);
                    }
                }
            }
        }

    }
}
