using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LYTest.ViewModel.Debug
{
    /// <summary>
    /// 多功能板
    /// </summary>
    public class MultiBoardViewModel : ViewModelBase
    {

        private int dColor = 0;
        /// <summary>
        /// 开始编号
        /// </summary>
        public int DColor
        {
            get { return dColor; }
            set
            { SetPropertyValue(value, ref dColor, "DColor"); }
        }
        private int dType = 0;
        /// <summary>
        /// 开始编号
        /// </summary>
        public int DType
        {
            get { return dType; }
            set
            { SetPropertyValue(value, ref dType, "DType"); }
        }
        /// <summary>
        /// 切换三色灯
        /// </summary>
        public void SetSSD()
        {

            Utility.TaskManager.AddDeviceAction(() =>
            {
                //TODO 不知道这个类干嘛的，没有任何应用
                //EquipmentData.DeviceManager.SetEquipmentThreeColor(DColor + 18, DType);              
                EquipmentData.DeviceManager.SetEquipmentThreeColor(Core.Enum.EmLightColor.灭, DType);
            });

        }

    }
}
