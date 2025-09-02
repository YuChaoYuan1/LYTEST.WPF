using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LYTest.ViewModel.InputPara
{
    /// <summary>
    /// 参数录入单元【视图编号ID：888】
    /// </summary>
    public class InputParaUnit : ViewModelBase
    {
        private bool isDisplayMember;
        /// <summary>
        /// 作为要录入参数的成员显示
        /// </summary>
        public bool IsDisplayMember
        {
            get { return isDisplayMember; }
            set
            {
                if (!value)
                {
                    Index = "999";
                }
                SetPropertyValue(value, ref isDisplayMember, "IsDisplayMember");
            }
        }

        private string fieldName;
        /// <summary>
        /// 字段名称
        /// </summary>
        public string FieldName
        {
            get { return fieldName; }
            set { SetPropertyValue(value, ref fieldName, "FieldName"); }
        }
        private string displayName;
        /// <summary>
        /// 显示名称
        /// </summary>
        public string DisplayName
        {
            get { return displayName; }
            set { displayName = value; }
        }
        private bool isSame;
        /// <summary>
        /// 是否具有相同的值 【true的时候为上面复选框，否则在表格中】
        /// </summary>
        public bool IsSame
        {
            get { return isSame; }
            set { SetPropertyValue(value, ref isSame, "IsSame"); }
        }
        private string codeType = "";
        /// <summary>
        /// 参数对应的编码类型
        /// </summary>
        public string CodeType
        {
            get { return codeType; }
            set { SetPropertyValue(value, ref codeType, "CodeType"); }
        }

        private string codeName;
        /// <summary>
        /// 编码名称
        /// </summary>
        public string CodeName
        {
            get { return codeName; }
            set { SetPropertyValue(value, ref codeName, "CodeName"); }
        }

        private bool isNewValue = false;
        /// <summary>
        /// 更换新表时是否使用新的值
        /// </summary>
        public bool IsNewValue
        {
            get { return isNewValue; }
            set { SetPropertyValue(value, ref isNewValue, "IsNewValue"); }
        }


        private EnumValueType valueType = EnumValueType.编码名称;
        /// <summary>
        /// 值的类型
        /// </summary>
        public EnumValueType ValueType
        {
            get { return valueType; }
            set { SetPropertyValue(value, ref valueType, "ValueType"); }
        }

        private string defaultValue;
        /// <summary>
        /// 默认值
        /// </summary>
        public string DefaultValue
        {
            get { return defaultValue; }
            set { SetPropertyValue(value, ref defaultValue, "DefaultValue"); }
        }


        private bool isNecessary;
        /// <summary>
        /// 值为必须的
        /// </summary>
        public bool IsNecessary
        {
            get { return isNecessary; }
            set { SetPropertyValue(value, ref isNecessary, "IsNecessary"); }
        }


        private string index = "999";

        public string Index
        {
            get { return index; }
            set
            {
                SetPropertyValue(value, ref index, "Index");
            }
        }

    }
}
