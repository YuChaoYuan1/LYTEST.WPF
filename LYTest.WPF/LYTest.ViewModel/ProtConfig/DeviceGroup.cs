using LYTest.ViewModel.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LYTest.ViewModel.ProtConfig
{
    public class DeviceGroup : ViewModelBase
    {
        // <!--名称 服务器编号  设备型号 串口参数  开始端口 端口间隔  端口数量 帧最大间隔  字节最大间隔 是否编辑  删除-->

        private AsyncObservableCollection<DeviceItem> deviceItems = new AsyncObservableCollection<DeviceItem>();
        /// <summary>
        /// 设备列表
        /// </summary>                      
        public AsyncObservableCollection<DeviceItem> DeviceItems
        {
            get { return deviceItems; }
            set { SetPropertyValue(value, ref deviceItems, "DeviceItems"); }
        }



        private string name ;
        /// <summary>
        /// 名称
        /// </summary>
        public string Name
        {
            get { return name; }
            set { SetPropertyValue(value, ref name, "Name"); }
        }

        private bool isExist;
        /// <summary>
        /// 端口是否存在
        /// </summary>
        public bool IsExist
        {
            get { return isExist; }
            set { SetPropertyValue(value, ref isExist, "IsExist"); }
        }

       



        public override string ToString()
        {
            string str = "";
            for (int i = 0; i < DeviceItems.Count; i++)
            {
                str += DeviceItems[i].ToString()+"$";
            }
            str= str.TrimEnd('$');
            return str;
        }


    }
}
