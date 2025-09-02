using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;

namespace LYTest.DataManager.ViewModel
{
    /// <summary>
    /// 显示消息
    /// </summary>
    public class MessageDisplay : ViewModelBase
    {
        private static MessageDisplay instance = null;

        public static MessageDisplay Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new MessageDisplay();
                }
                return instance;
            }
        }
        public void Initialize()
        {
            timer.Elapsed += (sender, e) =>
              {
                  IsVisible = false;
                  timer.Stop();
              };
        }
        /// <summary>
        /// 消息显示时间10秒
        /// </summary>
        private Timer timer = new Timer(10000);
        private string message;

        public string Message
        {
            get { return message; }
            set
            {
                SetPropertyValue(value, ref message, "Message");
                IsVisible = true;
                timer.Stop();
                timer.Start();
            }
        }


        private bool isVisible = false;

        public bool IsVisible
        {
            get { return isVisible; }
            set
            {
                SetPropertyValue(value, ref isVisible, "IsVisible");
            }
        }
        public void StopTimer()
        {
            timer.Stop();
            timer.Dispose();
        }

        private string finishedInfo;

        public string FinishedInfo
        {
            get { return finishedInfo; }
            set { SetPropertyValue(value, ref finishedInfo, "FinishedInfo"); }
        }

    }
}
