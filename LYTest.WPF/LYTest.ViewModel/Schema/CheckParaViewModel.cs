using LYTest.DAL;

namespace LYTest.ViewModel.Schema
{
    /// <summary>
    /// 单个参数的视图模型
    /// </summary>
    public class CheckParaViewModel : ViewModelBase
    {
        private string paraDisplayName;
        /// <summary>
        /// 参数显示名称
        /// </summary>
        public string ParaDisplayName
        {
            get { return paraDisplayName; }
            set
            {
                SetPropertyValue(value, ref paraDisplayName, "ParaDisplayName");
            }
        }

        private string paraEnumType;
        /// <summary>
        /// 参数数据源
        /// </summary>
        public string ParaEnumType
        {
            get { return paraEnumType; }
            set
            {
                SetPropertyValue(value, ref paraEnumType, "ParaEnumType");
            }
        }

        private bool isKeyMember;
        /// <summary>
        /// 是否作为主键的成员
        /// </summary>
        public bool IsKeyMember
        {
            get { return isKeyMember; }
            set { SetPropertyValue(value, ref isKeyMember, "IsKeyMember"); }
        }

        private bool isNameMember;
        /// <summary>
        /// 是否作为显示名称
        /// </summary>
        public bool IsNameMember
        {
            get { return isNameMember; }
            set { SetPropertyValue(value, ref isNameMember, "IsNameMember"); }
        }

        private string codeValue;
        /// <summary>
        /// 编码英文名称
        /// </summary>
        public string CodeValue
        {
            get { return codeValue; }
            set
            {
                SetPropertyValue(value, ref codeValue, "codeValue");
            }
        }
        /// <summary>
        /// 编码值
        /// </summary>
        public string CodeId
        {
            get { return DAL.CodeDictionary.GetValueLayer2(ParaEnumType, codeValue); }
        }
        private string defaultValue;
        /// <summary>
        /// 默认值
        /// </summary>
        public string DefaultValue
        {
            get { return defaultValue; }
            set { SetPropertyValue(value, ref defaultValue, "DefaultVaule"); }
        }

        private string codeName;
        /// <summary>
        /// 编码中文名称
        /// </summary>
        public string CodeName
        {
            get { return codeName; }
            set
            {
                string temp = CodeDictionary.GetCodeLayer1(value);
                if ((!string.IsNullOrEmpty(temp)) && temp != ParaEnumType)
                {
                    ParaEnumType = temp;
                }
                SetPropertyValue(value, ref codeName, "CodeName");
            }
        }

    }
}
