using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LYTest.DataManager.SG
{
    public class UiInterface
    {
        
        private static SynchronizationContext uiDispatcher = null;
        /// <summary>
        /// 界面线程,只允许设置一次
        /// </summary>
        public static SynchronizationContext UiDispatcher
        {
            get { return uiDispatcher; }
            set
            {
                if (uiDispatcher == null)
                {
                    uiDispatcher = value;
                }
            }
        }
    }
}
