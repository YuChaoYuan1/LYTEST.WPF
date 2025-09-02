using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LYTest.ViewModel.Config
{
    /// <summary>
    /// 单个配置数据模型
    /// </summary>
    public class ConfigUnit : ViewModelBase
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
        private bool isCombo;
        /// <summary>
        /// 是否为下拉可选
        /// </summary>
        public bool IsCombo
        {
            get { return isCombo; }
            set { SetPropertyValue(value, ref isCombo, "IsCombo"); }
        }
        private string configValue;
        /// <summary>
        /// 配置的值
        /// </summary>
        public string ConfigValue
        {
            get { return configValue; }
            set { SetPropertyValue(value, ref configValue, "ConfigValue"); }
        }
        private string code;
        public string Code
        {
            get { return code; }
            set { SetPropertyValue(value, ref code, "Code"); }
        }
    }
}
