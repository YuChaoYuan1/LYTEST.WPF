using System.Collections.Generic;

namespace LYTest.Mis
{
    /// <summary>
    /// 黑龙江下载表信息帮助类
    /// </summary>
    public class HeiLongJianDownHelper
    {
        /// <summary>
        /// 系统id编号
        /// </summary>
        public int systemID;
        /// <summary>
        /// 托盘编号列表
        /// </summary>

        public List<string> TrayNumList;

        /// <summary>
        /// 获取参数类型
        /// </summary>
        public int codeType = 1;


        /// <summary>
        /// 同步获取还是异步获取
        /// </summary>
        public bool applyType = true;
    }
}
