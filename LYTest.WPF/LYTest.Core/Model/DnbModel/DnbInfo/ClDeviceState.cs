using System.Collections.Generic;

namespace LYTest.Core.Model.DnbModel.DnbInfo
{

    public class ClDeviceState
    {

        public ClDeviceState()
        {
            this.Device = new List<Device>();
        }
        /// <summary>
        /// 专机编号
        /// </summary>
        public string ID { get; set; }

        /// <summary>
        /// 专机状态
        /// </summary>
        public string State { get; set; }

        public List<Device> Device { get; } = new List<Device>();
    }

    public class Device
    {
        /// <summary>
        /// 设备编号
        /// </summary>
        public string DeviceId { get; set; }

        /// <summary>
        /// 设备状态
        /// </summary>
        public string DeviceState { get; set; }
    }
}
