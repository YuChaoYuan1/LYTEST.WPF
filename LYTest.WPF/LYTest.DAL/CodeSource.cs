using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LYTest.DAL
{
    /// <summary>
    /// 编码数据源
    /// </summary>
    public class CodeSource
    {
        /// <summary>
        /// 编码列表,程序启动时从数据库加载到内存中
        /// </summary>
        private static List<DynamicModel> codeModels = new List<DynamicModel>();
        public static void Initialize()
        {
            codeModels = DALManager.ApplicationDbDal.GetList(EnumAppDbTable.T_CODE_TREE.ToString());
        }
        /// <summary>
        /// 获取所有的编码
        /// </summary>
        /// <returns></returns>
        public static List<DynamicModel> GetAllModels()
        {
            return codeModels;
        }
    }
}
