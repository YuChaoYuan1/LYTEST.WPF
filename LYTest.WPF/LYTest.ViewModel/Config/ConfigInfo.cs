using LYTest.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LYTest.ViewModel.Config
{
    /// <summary>
    /// 配置单元的格式
    /// </summary>
    public class ConfigInfo : ViewModelBase
    {
        private string name;
        /// <summary>
        /// 配置名称
        /// </summary>
        public string Name
        {
            get { return name; }
            set { SetPropertyValue(value, ref name, "Name"); }
        }
        private string codeName;
        /// <summary>
        /// 配置的编码
        /// </summary>
        public string CodeName
        {
            get { return codeName; }
            set
            {
                SetPropertyValue(value, ref codeName, "CodeName");
                if (!string.IsNullOrEmpty(codeName))
                {
                    Code = CodeDictionary.GetCodeLayer1(codeName);
                }
            }
        }

        public string Code { get; set; }
        private string defaultValue;
        /// <summary>
        /// 默认值
        /// </summary>
        public string DefaultValue
        {
            get { return defaultValue; }
            set { SetPropertyValue(value, ref defaultValue, "DefaultValue"); }
        }
    }
}
