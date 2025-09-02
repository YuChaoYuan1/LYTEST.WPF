using LYTest.DAL;
using System.Collections.ObjectModel;

namespace LYTest.ViewModel.Menu
{
    /// <summary>
    /// 目录的一些特性
    /// </summary>
    public class MenuConfigItem : ViewModelBase
    {
        private string menuName;
        /// <summary>
        /// 目录名称
        /// </summary>
        public string MenuName
        {
            get { return menuName; }
            set { SetPropertyValue(value, ref menuName, "MenuName"); }
        }
        private string menuClass;
        /// <summary>
        /// 目录对应的类名称
        /// </summary>
        public string MenuClass
        {
            get { return menuClass; }
            set { SetPropertyValue(value, ref menuClass, "MenuClass"); }
        }

        private ImageItem menuImage;
        /// <summary>
        /// 目录图片
        /// </summary>
        public ImageItem MenuImage
        {
            get { return menuImage; }
            set { SetPropertyValue(value, ref menuImage, "MenuImage"); }
        }
        private bool isValid;
        /// <summary>
        /// 是否有效
        /// </summary>
        public bool IsValid
        {
            get { return isValid; }
            set { SetPropertyValue(value, ref isValid, "IsValid"); }
        }


        private EnumMenuType menuType = EnumMenuType.打开页面;
        /// <summary>
        /// 是否为设备控制
        /// </summary>
        public EnumMenuType MenuType
        {
            get { return menuType; }
            set { SetPropertyValue(value, ref menuType, "MenuType"); }
        }
        private EnumCheckingEnable checkingEnable = EnumCheckingEnable.可用;
        /// <summary>
        /// 检定时是否可用
        /// </summary>
        public EnumCheckingEnable CheckingEnable
        {
            get { return checkingEnable; }
            set { SetPropertyValue(value, ref checkingEnable, "CheckingEnable"); }
        }

        private EnumUserVisible userCode = EnumUserVisible.所有用户可见;
        /// <summary>
        /// 用户可见编号
        /// </summary>
        public EnumUserVisible UserCode
        {
            get { return userCode; }
            set { SetPropertyValue(value, ref userCode, "UserCode"); }
        }


        private EnumMainMenu isMainMenu = EnumMainMenu.是;
        /// <summary>
        /// 是否为主目录
        /// </summary>
        public EnumMainMenu IsMainMenu
        {
            get { return isMainMenu; }
            set { SetPropertyValue(value, ref isMainMenu, "IsMainMenu"); }
        }
        private EnumMenuCategory menuCategory = EnumMenuCategory.常用;
        /// <summary>
        /// 目录类别
        /// </summary>
        public EnumMenuCategory MenuCategory
        {
            get { return menuCategory; }
            set { SetPropertyValue(value, ref menuCategory, "MenuCategory"); }
        }
        private bool flagChanged = false;
        /// <summary>
        /// 修改标记
        /// </summary>
        public bool FlagChanged
        {
            get { return flagChanged; }
            set { SetPropertyValue(value, ref flagChanged, "FlagChanged"); }
        }
        protected internal override void OnPropertyChanged(string propertyName)
        {
            base.OnPropertyChanged(propertyName);
            if (propertyName != "FlagChanged")
            {
                FlagChanged = true;
            }
        }
        private ObservableCollection<ImageItem> images = new ObservableCollection<ImageItem>();
        /// <summary>
        /// 图片列表
        /// </summary>
        public ObservableCollection<ImageItem> Images
        {
            get { return images; }
            set { SetPropertyValue(value, ref images, "Images"); }
        }
        /// <summary>
        /// 目录构造函数
        /// </summary>
        /// <param name="modelTemp"></param>
        public MenuConfigItem(DynamicModel modelTemp)
        {
            MenuName = modelTemp.GetProperty("MENU_NAME") as string;
            MenuClass = modelTemp.GetProperty("MENU_CLASS") as string;
            SortId = modelTemp.GetProperty("SORT_ID") as string;
            string fileName = modelTemp.GetProperty("MENU_IMAGE") as string;
            string validFlag = modelTemp.GetProperty("VALID_FLAG") as string;
            if (validFlag != "0")
            {
                isValid = true;
            }
            string sourceTemp = modelTemp.GetProperty("MENU_DATASOURCE") as string;
            if (sourceTemp == "1")
            {
                MenuType = EnumMenuType.设备控制;
            }
            string checkEnableTemp = modelTemp.GetProperty("MENU_CHECK_ENABLE") as string;
            if (checkEnableTemp == "1")
            {
                CheckingEnable = EnumCheckingEnable.不可用;
            }
            string userVisibleCode = modelTemp.GetProperty("MENU_USER_VISIBLE") as string;
            if (userVisibleCode == "1")
            {
                UserCode = EnumUserVisible.普通用户不可见;
            }
            else if (userVisibleCode == "2")
            {
                UserCode = EnumUserVisible.超级用户可见;
            }
            string codeMain = modelTemp.GetProperty("MENU_MAIN") as string;
            if (codeMain == "0")
            {
                IsMainMenu = EnumMainMenu.否;
            }
            string categoryTemp = modelTemp.GetProperty("MENU_CATEGORY") as string;
            switch (categoryTemp)
            {
                case "1":
                    MenuCategory = EnumMenuCategory.设备控制;
                    break;
                case "2":
                    MenuCategory = EnumMenuCategory.方案;
                    break;
                case "3":
                    MenuCategory = EnumMenuCategory.配置;
                    break;
                case "4":
                    MenuCategory = EnumMenuCategory.其它;
                    break;
                default:
                    MenuCategory = EnumMenuCategory.常用;
                    break;
            }
        }
        /// <summary>
        /// 获取模型
        /// </summary>
        /// <returns></returns>
        public DynamicModel GetModel()
        {
            DynamicModel modelTemp = new DynamicModel();
            modelTemp.SetProperty("MENU_NAME", MenuName);
            modelTemp.SetProperty("MENU_CLASS", MenuClass);
            modelTemp.SetProperty("SORT_ID", SortId);
            if (MenuImage != null)
            {
                modelTemp.SetProperty("MENU_IMAGE", MenuImage.ImageName);
            }
            modelTemp.SetProperty("VALID_FLAG", IsValid ? "1" : "0");
            modelTemp.SetProperty("MENU_DATASOURCE", ((int)MenuType).ToString());
            modelTemp.SetProperty("MENU_CHECK_ENABLE", ((int)CheckingEnable).ToString());
            modelTemp.SetProperty("MENU_USER_VISIBLE", ((int)UserCode).ToString());
            modelTemp.SetProperty("MENU_MAIN", ((int)IsMainMenu).ToString());
            modelTemp.SetProperty("MENU_CATEGORY", ((int)MenuCategory).ToString());
            return modelTemp;
        }

        private string sortId;
        /// <summary>
        /// 排序编号
        /// </summary>
        public string SortId
        {
            get { return sortId; }
            set { SetPropertyValue(value, ref sortId, "SortId"); }
        }

    }
}
