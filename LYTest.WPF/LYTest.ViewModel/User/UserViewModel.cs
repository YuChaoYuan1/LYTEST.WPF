using LYTest.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LYTest.ViewModel.User
{
    public class UserViewModel : ViewModelBase
    {
        private static UserViewModel instance;
        public static UserViewModel Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new UserViewModel();
                }
                return instance;
            }
        }

        /// <summary>
        /// 获得所有用户
        /// </summary>
        /// <param name="strWizard"></param>
        /// <returns></returns>
        public List<string> GetList(string strWizard)
        {
            List<string> users = new List<string>();
            users = DALManager.ApplicationDbDal.GetDistinct(EnumAppDbTable.T_USER_INFO.ToString(), "USER_NAME", string.Format("USER_NAME like '%{0}%'", strWizard));
            return users;

            //users = DALManager.ApplicationDbDal.GetDistinct(EnumAppDbTable.Info_User.ToString(), "chrUserName", string.Format("chrUserName like '%{0}%'", strWizard));
            //if (users.Count == 0)
            //{
            //    users = DALManager.ApplicationDbDal.GetDistinct(EnumAppDbTable.Info_User.ToString(), "chrUserCode", string.Format("chrUserCode like '%{0}%'", strWizard));
            //}
            //return users;
        }


        public List<string> GetNormalUsers()
        {
            List<string> users = new List<string>();
            users = DALManager.ApplicationDbDal.GetDistinct(EnumAppDbTable.T_USER_INFO.ToString(), "USER_NAME", "USER_POWER<>'2'");
            return users;
        }
        private DynamicViewModel currentUser;

        public DynamicViewModel CurrentUser
        {
            get { return currentUser; }
            set { SetPropertyValue(value, ref currentUser, "CurrentUser"); }
        }

        private int step;
        /// 当前步骤:0:登录;1:选择前往哪个界面;2:修改密码;3:删除用户;4:添加用户
        /// <summary>
        /// 当前步骤:0:登录;1:选择前往哪个界面;2:修改密码;3:删除用户;4:添加用户
        /// </summary>
        public int Step
        {
            get { return step; ; }
            set { SetPropertyValue(value, ref step, "Step"); }
        }

        public bool Login(string userNameOrNo, string password)
        {
            if (userNameOrNo == "admin")
            {
                // if (password == DateTime.Now.ToString("ddHHmm"))
                if (password == "123456")
                {
                    CurrentUser = new DynamicViewModel(0);
                    CurrentUser.SetProperty("USER_NAME", "admin");
                    CurrentUser.SetProperty("USER_POWER", "2");
                    Step = 1;
                    EquipmentData.LastCheckInfo.OnPropertyChanged("TestPerson");
                    return true;
                }
                else
                {
                    return false;
                }
            }

            DynamicModel model = DALManager.ApplicationDbDal.GetByID(EnumAppDbTable.T_USER_INFO.ToString(), string.Format("(USER_ID='{0}' or USER_NAME='{0}') and USER_PASSWORD='{1}' ", userNameOrNo, password));
            if (model != null)
            {
                CurrentUser = new DynamicViewModel(model, 0);
                Step = 1;
                EquipmentData.LastCheckInfo.OnPropertyChanged("TestPerson");
                return true;
            }
            else
            {
                Step = 0;
                return false;
            }
        }
        public bool AddUser(string userName, string userCode, string password, string userPermission)
        {
            DynamicModel model = new DynamicModel();
            model.SetProperty("USER_ID", userCode);
            model.SetProperty("USER_NAME", userName);
            model.SetProperty("USER_PASSWORD", password);
            model.SetProperty("USER_POWER", userPermission);
            return DALManager.ApplicationDbDal.Insert(EnumAppDbTable.T_USER_INFO.ToString(), model) > 0;
        }
        public bool DeleteUser(string userName)
        {
            return DALManager.ApplicationDbDal.Delete(EnumAppDbTable.T_USER_INFO.ToString(), string.Format("USER_NAME='{0}' ", userName)) > 0;
        }
        public bool IsExist(string userNameOrNo)
        {
            return DALManager.ApplicationDbDal.GetCount(EnumAppDbTable.T_USER_INFO.ToString(), string.Format("USER_ID='{0}' or USER_NAME='{0}' ", userNameOrNo)) > 0;
        }
        public bool UpdatePassword(string userName, string password)
        {
            DynamicModel model = new DynamicModel();
            model.SetProperty("USER_PASSWORD", password);
            return DALManager.ApplicationDbDal.Update
(EnumAppDbTable.T_USER_INFO.ToString(), string.Format("USER_NAME='{0}' ", userName), model, new List<string> { "USER_PASSWORD" }) > 0;
        }
    }
}
