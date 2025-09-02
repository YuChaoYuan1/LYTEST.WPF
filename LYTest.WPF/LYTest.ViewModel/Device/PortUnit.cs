namespace LYTest.ViewModel.Device
{
    /// 端口单元
    /// <summary>
    /// 端口单元
    /// </summary>
    public class PortUnit : ViewModelBase
    {
        public bool IsUsed
        { get { return !string.IsNullOrEmpty(Name); } }

        private string name;
        /// <summary>
        /// 设备名
        /// </summary>
        public string Name
        {
            get { return name; }
            set
            {
                SetPropertyValue(value, ref name, "Name");
                OnPropertyChanged("DisplayName");
                OnPropertyChanged("IsUsed");
            }
        }

        private int index;
        /// 对应于2018的端口号
        /// <summary>
        /// 对应于2018的端口号
        /// </summary>
        public int Index
        {
            get { return index; }
            set { SetPropertyValue(value, ref index, "Index"); }
        }
        private bool newSend;
        /// 发出新数据
        /// <summary>
        /// 发出新数据
        /// </summary>
        public bool NewSend
        {
            get { return newSend; }
            set
            {
                SetPropertyValue(value, ref newSend, "NewSend");
            }
        }

        private bool newReceived;
        /// 收到新数据
        /// <summary>
        /// 收到新数据
        /// </summary>
        public bool NewReceived
        {
            get { return newReceived; }
            set
            {
                SetPropertyValue(value, ref newReceived, "NewReceived");
            }
        }
        /// 显示名称
        /// <summary>
        /// 显示名称
        /// </summary>
        public string DisplayName
        {
            get
            {
                if (string.IsNullOrEmpty(Name))
                {
                    return "";
                }
                else
                {
                    return string.Format("{0}-{1}", Name, DeviceIndex + 1);
                }
            }
        }
        private int deviceIndex;
        /// 设备序号
        /// <summary>
        /// 设备序号
        /// </summary>
        public int DeviceIndex
        {
            get { return deviceIndex; }
            set { SetPropertyValue(value, ref deviceIndex, "DeviceIndex"); }
        }

    }
}
