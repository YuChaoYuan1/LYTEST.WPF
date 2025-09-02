using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LYTest.DataManager.ViewModel
{
    public class LoginModel : ViewModelBase
    {
        private string textLogin;
        /// <summary>
        /// 登录时显示的文本
        /// </summary>
        public string TextLogin
        {
            get { return textLogin; }
            set { SetPropertyValue(value, ref textLogin, "TextLogin"); }
        }

        private int progressBarValue;
        /// <summary>
        /// 登入时候，进度条进度
        /// </summary>
        public int ProgressBarValue
        {
            get { return progressBarValue; }
            set { SetPropertyValue(value, ref progressBarValue, "ProgressBarValue"); }
        }

        public bool IsClose;
    }
}
