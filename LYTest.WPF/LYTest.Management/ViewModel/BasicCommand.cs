using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace LYTest.DataManager
{
    public class BasicCommand : ICommand
    {
        public bool CanExecute(object parameter)
        {
            return true;
        }
        /// <summary>
        /// 
        /// </summary>
        public event EventHandler CanExecuteChanged
        {
            add
            { }
            remove
            { }
        }
        /// <summary>
        /// 执行外部定义的动作
        /// </summary>
        /// <param name="parameter">方法名称,即CommandParameter的值,该方法必须定义成公有方法,不然程序会找不到该方法,无法执行操作.</param>
        public void Execute(object parameter)
        {
            if (CommandAction != null)
            {
                string stringPara = parameter as string;
                if (!string.IsNullOrEmpty(stringPara))
                {
                    try
                    {
                        CommandAction.Invoke(stringPara);
                    }
                    catch { }
                }
            }
        }
        /// <summary>
        /// 命令下发时要执行的动作
        /// </summary>
        public Action<string> CommandAction { get; set; }
    }
}
