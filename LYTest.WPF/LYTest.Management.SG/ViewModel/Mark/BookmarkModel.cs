using LYTest.DataManager.SG.ViewModel.Mark;
using System;

namespace LYTest.DataManager.SG.Mark.ViewModel
{
    class BookmarkModel : ViewModelBase
    {
        public BookmarkModel(string nameOriginal)
        {
            Name = nameOriginal;
            //避免出现重复的书签名,用VY将数据序号隔开
            string bookmarkName = nameOriginal.Replace("VY", "^").Split('^')[0];
            bool isValid = true;
            if (string.IsNullOrEmpty(bookmarkName))
            {
                isValid = false;
            }
            string bookmarkTemp = bookmarkName.Replace("VX", "^");
            string[] arrayTemp = bookmarkTemp.Split('^');
            if (arrayTemp.Length < 2 || arrayTemp.Length > 4)
            {
                isValid = false;
            }
            else
            {
                if (arrayTemp.Length == 3)
                {
                    #region 台体信息书签
                    if (arrayTemp[0] == "EquipmentInfo")
                    {
                        DeviceName = "检定台";
                        CheckPointName = "台体基本信息";
                        ResultName = arrayTemp[1];
                        TestValue = "";
                        string strFormat = arrayTemp[2];
                        Enum.TryParse(strFormat, out format);
                    }
                    else
                    {
                        isValid = false;
                    }
                    #endregion
                }
                else if (arrayTemp.Length == 4 && arrayTemp[1] == "MeterInfo")
                {
                    #region 表信息书签
                    DeviceName = "电能表";
                    CheckPointName = "表基本信息";
                    ResultName = arrayTemp[2];
                    TestValue = "";
                    string strFormat = arrayTemp[3];
                    Enum.TryParse(strFormat, out format);
                    #endregion
                }
                else if(arrayTemp.Length==2)
                {
                    #region 一种检定类型的总结论
                    DeviceName = arrayTemp[0];
                    ItemKey = arrayTemp[1];
                    CheckPointName = DAL.SchemaFramework.GetItemName(arrayTemp[1]);
                    ResultName = "总结论";
                    TestValue = "";
                    Format = EnumFormat.无;
                    #endregion
                }
                else if (arrayTemp.Length >= 3)
                {
                    #region 检定结论书签
                    string temp = "";
                    DeviceName = arrayTemp[0] == null ? "" : arrayTemp[0];
                    ItemKey = arrayTemp[1];
                    string[] noArray = arrayTemp[1].Split('_');
                    #region 获取检定点名称
                    DAL.DynamicModel itemModel = DAL.DALManager.SchemaDal .GetByID(DAL.EnumAppDbTable.T_SCHEMA_PARA_VALUE.ToString(), string.Format("para_key='{0}'", arrayTemp[1]));
                    if (itemModel != null)
                    {
                        temp = itemModel.GetProperty("PARA_NAME") as string;
                    }
                    else
                    {
                        temp = "";
                    }
                    #endregion
                    if (string.IsNullOrEmpty(temp))
                    {
                        if (temp.Contains("仍"))
                        {

                        }
                        temp = DAL.SchemaFramework.GetItemName(noArray[0]);
                    }
                    if (string.IsNullOrEmpty(temp))
                    {
                        isValid = false;
                    }
                    else
                    {
                        CheckPointName = temp;
                    }
                    if (string.IsNullOrEmpty(arrayTemp[2]))
                    {
                        isValid = false;
                    }
                    else
                    {
                        ResultName = arrayTemp[2];
                    }
                    if (arrayTemp.Length == 4)
                    {
                        string strFormat = arrayTemp[3];
                        Enum.TryParse(strFormat, out format);
                    }
                    #endregion
                }
                else
                {
                    isValid = false;
                }
                if (!isValid)
                {
                    DeviceName = "不识别";
                    CheckPointName = "不识别";
                    ResultName = "不识别";
                    TestValue = "";
                }
            }
        }

        private string deviceName = "";
        /// <summary>
        /// 设备名称
        /// </summary>
        public string DeviceName
        {
            get { return deviceName; }
            set
            {
                if (value == null)
                {
                    SetPropertyValue("", ref deviceName, "DeviceName");
                }
                else
                {
                    SetPropertyValue(value, ref deviceName, "DeviceName");
                }
            }
        }

        private string name;
        /// <summary>
        /// 书签名称
        /// </summary>
        public string Name
        {
            get { return name; }
            set { SetPropertyValue(value, ref name, "Name"); }
        }

        private string checkPointName;
        /// <summary>
        /// 检定点名称
        /// </summary>
        public string CheckPointName
        {
            get { return checkPointName; }
            set { SetPropertyValue(value, ref checkPointName, "CheckPointName"); }
        }
        private string resultName;
        /// <summary>
        /// 结论名称
        /// </summary>
        public string ResultName
        {
            get { return resultName; }
            set { SetPropertyValue(value, ref resultName, "ResultName"); }
        }
        private EnumFormat format;
        /// <summary>
        /// 结论对应的格式
        /// </summary>
        public EnumFormat Format
        {
            get { return format; }
            set { SetPropertyValue(value, ref format, "Format"); }
        }

        private string testValue;
        /// <summary>
        /// 书签测试值
        /// </summary>
        public string TestValue
        {
            get { return testValue; }
            set { SetPropertyValue(value, ref testValue, "TestValue"); }
        }

        private string itemKey;

        public string ItemKey
        {
            get { return itemKey; }
            set { SetPropertyValue(value, ref itemKey, "ItemKey"); }
        }
    }
}
