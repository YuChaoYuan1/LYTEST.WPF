using LYTest.DAL.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LYTest.DAL
{
    /// <summary>
    /// 系统固有的属性配置信息管理
    /// </summary>
    public class InherentConfig
    {
        private static ConfigData instance = null;

        public static ConfigData Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new ConfigData(DALManager.InherentDbDal);
                }
                return instance;
            }
        }
    }
}
