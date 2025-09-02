using LYTest.Utility.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LYTest.Mis.Common
{
 public   class GKLogHelper
    {

        /// <summary>
        /// 写入日志
        /// </summary>
        /// <param name="Name">日志名称</param>
        /// <param name="Message">日志内容</param>
        public static void WriteLog(string Name,string Message)
        {
            LogManager.AddMessage(string.Format("调用接口{0}，上报数据\r\n{1}", Name, Message), 6);

        }
    }
}
