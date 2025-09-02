
namespace LYTest.ViewModel.ProtConfig
{
    /// <summary>
    /// 串口服务器类
    /// </summary>
    public class ServerItem : ViewModelBase
    {
        private string name;

        public string Name
        {
            get { return name; }
            set { SetPropertyValue(value, ref name, "Name"); }
        }
        private string address;

        public string Address
        {
            get { return address; }
            set { SetPropertyValue(value, ref address, "Address"); }
        }
        public override string ToString()
        {
            return Name;
        }
        private bool flagChanged;
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
    }
}
