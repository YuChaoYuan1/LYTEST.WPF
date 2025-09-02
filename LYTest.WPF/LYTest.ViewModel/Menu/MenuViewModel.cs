using LYTest.DAL;
using LYTest.Utility.Log;
using LYTest.ViewModel.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Media.Imaging;

namespace LYTest.ViewModel.Menu
{
    /// <summary>
    /// 目录配置视图
    /// </summary>
    public class MenuViewModel : ViewModelBase
    {
        public MenuViewModel()
        {
            LoadCollections();
            LoadMenus();
        }

        /// <summary>
        /// 目录集合
        /// </summary>
        public AsyncObservableCollection<MenuConfigItem> Menus { get; } = new AsyncObservableCollection<MenuConfigItem>();

        #region 列的数据源
        /// <summary>
        /// 图片列表
        /// </summary>
        public ObservableCollection<ImageItem> Images { get; } = new ObservableCollection<ImageItem>();


        public ObservableCollection<EnumCheckingEnable> Enables { get; } = new ObservableCollection<EnumCheckingEnable>();


        public ObservableCollection<EnumMainMenu> YesNoCollecion { get; } = new ObservableCollection<EnumMainMenu>();

        public ObservableCollection<EnumMenuCategory> Categories { get; } = new ObservableCollection<EnumMenuCategory>();


        public ObservableCollection<EnumMenuType> MenuTypes { get; } = new ObservableCollection<EnumMenuType>();


        public ObservableCollection<EnumUserVisible> UserTypes { get; } = new ObservableCollection<EnumUserVisible>();

        #endregion

        /// <summary>
        /// 从数据库加载目录
        /// </summary>
        public void LoadMenus()
        {
            flagSort = false;

            Menus.Clear();
            List<DynamicModel> models = DALManager.ApplicationDbDal.GetList(EnumAppDbTable.T_MENU_VIEW.ToString());
            IEnumerable<DynamicModel> modelsTemp = models.OrderBy(item => item.GetProperty("SORT_ID"));  //根据编号排序
            foreach (DynamicModel modelTemp in modelsTemp)
            {
                string imageName = modelTemp.GetProperty("MENU_IMAGE") as string;  //获得图片名字

                MenuConfigItem itemTemp = new MenuConfigItem(modelTemp)
                {
                    Images = Images,
                    MenuImage = Images.FirstOrDefault(item => item.ImageName == imageName),
                    FlagChanged = false
                };
                Menus.Add(itemTemp);
                itemTemp.PropertyChanged += (sender, e) =>
                {
                    if (flagSort && e.PropertyName == "SortId")
                    {
                        Menus.Sort(item => item.SortId);
                    }
                };
            }

            flagSort = true;
        }
        private void LoadCollections()
        {
            Images.Clear();
            string[] fileNames = Directory.GetFiles(string.Format(@"{0}\Images", Directory.GetCurrentDirectory()));
            for (int i = 0; i < fileNames.Length; i++)
            {
                try
                {
                    int indexTemp = fileNames[i].LastIndexOf('\\');
                    string fileName = fileNames[i].Substring(indexTemp + 1);
                    BitmapImage imageTemp = new BitmapImage(new Uri(fileNames[i], UriKind.Absolute));
                    ImageItem itemTemp = new ImageItem()
                    {
                        ImageName = fileName,
                        ImageControl = imageTemp
                    };
                    Images.Add(itemTemp);
                }
                catch
                { }
            }
            #region 加载集合
            Enables.Clear();
            Array arrayTemp = Enum.GetValues(typeof(EnumCheckingEnable));
            for (int i = 0; i < arrayTemp.Length; i++)
            {
                Enables.Add((EnumCheckingEnable)(arrayTemp.GetValue(i)));
            }
            YesNoCollecion.Clear();
            arrayTemp = Enum.GetValues(typeof(EnumMainMenu));
            for (int i = 0; i < arrayTemp.Length; i++)
            {
                YesNoCollecion.Add((EnumMainMenu)(arrayTemp.GetValue(i)));
            }
            Categories.Clear();
            arrayTemp = Enum.GetValues(typeof(EnumMenuCategory));
            for (int i = 0; i < arrayTemp.Length; i++)
            {
                Categories.Add((EnumMenuCategory)(arrayTemp.GetValue(i)));
            }
            MenuTypes.Clear();
            arrayTemp = Enum.GetValues(typeof(EnumMenuType));
            for (int i = 0; i < arrayTemp.Length; i++)
            {
                MenuTypes.Add((EnumMenuType)(arrayTemp.GetValue(i)));
            }
            UserTypes.Clear();
            arrayTemp = Enum.GetValues(typeof(EnumUserVisible));
            for (int i = 0; i < arrayTemp.Length; i++)
            {
                UserTypes.Add((EnumUserVisible)(arrayTemp.GetValue(i)));
            }
            #endregion
        }
        /// <summary>
        /// 保存目录到数据库
        /// </summary>
        public void SaveMenus()
        {
            for (int i = 0; i < Menus.Count; i++)
            {
                if (!Menus[i].FlagChanged)
                {
                    continue;
                }
                DynamicModel modelTemp = Menus[i].GetModel();
                string menuName = modelTemp.GetProperty("MENU_NAME") as string;
                int updateCount = DALManager.ApplicationDbDal.Update(EnumAppDbTable.T_MENU_VIEW.ToString(),
                    $"MENU_NAME='{menuName}'", Menus[i].GetModel(), new List<string>() { "MENU_IMAGE", "MENU_CLASS",
                    "VALID_FLAG", "MENU_DATASOURCE", "MENU_CHECK_ENABLE", "MENU_USER_VISIBLE", "MENU_MAIN", "MENU_CATEGORY", "SORT_ID" });
                if (updateCount == 1)
                {
                    LogManager.AddMessage($"更新目录:{menuName}的相关信息成功", EnumLogSource.数据库存取日志);
                    Menus[i].FlagChanged = false;
                }
                else
                {
                    LogManager.AddMessage($"更新目录:{menuName}的相关信息失败:没有执行更改", EnumLogSource.数据库存取日志, EnumLevel.Warning);
                }
            }
        }

        private bool flagSort = false;
    }
}
