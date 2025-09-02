

namespace LYTest.ViewModel.Device
{
    /// 串口服务器视图模型
    /// <summary>
    /// 串口服务器视图模型
    /// </summary>
    public class ComServerViewModel : ViewModelBase
    {
        private int index;
        /// <summary>
        /// 串口服务器序号,从0开始
        /// </summary>
        public int Index
        {
            get { return index; }
            set
            {
                SetPropertyValue(value, ref index, "Index");
                OnPropertyChanged("Name");
            }
        }
        public string Name
        {
            get { return ServerAddress; }
        }

        private string serverAddress;
        /// <summary>
        /// 对应于串口服务器的地址
        /// </summary>
        public string ServerAddress
        {
            get { return serverAddress; }
            set
            {
                SetPropertyValue(value, ref serverAddress, "ServerAddress");
                OnPropertyChanged("Name");
            }
        }
        /// 设备构造函数
        /// <summary>
        /// 设备构造函数
        /// </summary>
        /// <param name="configString"></param>
        public ComServerViewModel()
        {
            Units.Clear();
            for (int i = 0; i < 64; i++)
            {
                Units.Add(new PortUnit
                {
                    Index = i + 1,
                });
            }
        }

        private Model.AsyncObservableCollection<PortUnit> units = new Model.AsyncObservableCollection<PortUnit>();
        /// 串口服务器端口列表
        /// <summary>
        /// 串口服务器端口列表
        /// </summary>
        public Model.AsyncObservableCollection<PortUnit> Units
        {
            get { return units; }
            set { SetPropertyValue(value, ref units, "Units"); }
        }
    }
}
