using LYTest.Utility.Log;
using System;
using System.Collections.Generic;

namespace LYTest.DAL.Config
{
    /// <summary>
    /// 配置信息管理
    /// </summary>
    public class ConfigHelper
    {
        private static ConfigData instance = null;

        public static ConfigData Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new ConfigData(DALManager.ApplicationDbDal);
                }
                return instance;
            }
        }

    }
}
