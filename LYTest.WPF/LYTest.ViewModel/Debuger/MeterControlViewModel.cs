using LYTest.ViewModel.CheckController;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LYTest.ViewModel.Debug
{
    public class MeterControlViewModel : ViewModelBase
    {
        private string address = "";
        /// <summary>
        /// 开始编号
        /// </summary>
        public string Address
        {
            get { return address; }
            set
            { SetPropertyValue(value, ref address, "Address"); }
        }
        private int meterNo = 1;
        /// <summary>
        /// 开始编号
        /// </summary>
        public int MeterNo
        {
            get { return meterNo; }
            set
            { SetPropertyValue(value, ref meterNo, "MeterNo"); }
        }

        public void ReadAddress()
        {

            Utility.TaskManager.AddDeviceAction(() =>
            {
                EquipmentData.Controller.UpdateMeterProtocol();
                Address = MeterProtocolAdapter.Instance.ReadAddress(meterNo - 1);
            });

        }

        public void Set_HG()
        {
            Utility.Log.LogManager.AddMessage("正在切换到互感式");
            //EquipmentData.DeviceManager.Hgq_Set(0);
            Utility.TaskManager.AddDeviceAction(() =>
            {
                EquipmentData.DeviceManager.Hgq_Set(0);
            });

        }
        public void Set_ZJ()
        {
            Utility.Log.LogManager.AddMessage("正在切换到直接式");
            //EquipmentData.DeviceManager.Hgq_Set(1);
            Utility.TaskManager.AddDeviceAction(() =>
            {
                EquipmentData.DeviceManager.Hgq_Set(1);
            });

        }
        public void Set_Off()
        {
            Utility.Log.LogManager.AddMessage("正在关闭互感器电机");
            //EquipmentData.DeviceManager.Hgq_Set(1);
            Utility.TaskManager.AddDeviceAction(() =>
            {
                EquipmentData.DeviceManager.Hgq_Off2();
            });

        }


    }
}
