using LYTest.DAL.Config;

namespace LYTest.ViewModel.CheckInfo
{
    /// <summary>
    /// 台体信息视图
    /// </summary>
    public class EquipmentViewModel : ViewModelBase
    {

        /// <summary>
        /// 是否是演示模式
        /// </summary>
        public bool IsDemo { 
            get; 
            set;
        }

        //public string Conn_Type = ConfigHelper.Instance.Conn_Type;

        /// <summary>
        /// 启动和潜动的时候同步进行通讯协议检查试验
        /// </summary>
        public bool IsSame = ConfigHelper.Instance.IsSame;
        /// <summary>
        /// 合格的项目是否再次检定
        /// </summary>
        public bool Is_Ok_Second_Test = ConfigHelper.Instance.Is_Ok_Second_Test;


        private string equipmentType = ConfigHelper.Instance.EquipmentType;

        /// <summary>
        /// 台体类型
        /// </summary>
        public string EquipmentType
        {
            get
            {
                return equipmentType;
            }
            set
            {
                SetPropertyValue(value, ref equipmentType, "EquipmentType");
            }
        }

        public bool AutoLogion = ConfigHelper.Instance.AutoLogin;

        private int meterCount = ConfigHelper.Instance.MeterCount;
        /// 表位数量
        /// <summary>
        /// 表位数量
        /// </summary>
        public int MeterCount
        {
            get { return meterCount; }
            set
            {
                SetPropertyValue(value, ref meterCount, "MeterCount");
                CheckController.MeterProtocolAdapter.Instance.SetBwCount(value);
            }
        }


        private string meterType = ConfigHelper.Instance.MeterType;
        private string verifyModel = ConfigHelper.Instance.VerifyModel;


        /// <summary>
        /// 程序检测类型(终端，电能表)
        /// </summary>
        public string MeterType
        {
            get { return meterType; }
            set { SetPropertyValue(value, ref meterType, "MeterType"); }
        }
        /// <summary>
        /// 检定模式-自动模式--手动模式
        /// </summary>
        public string VerifyModel
        {
            get { return verifyModel; }
            set { SetPropertyValue(value, ref verifyModel, "VerifyModel"); }
        }




        /// <summary>
        /// 台体编号
        /// </summary>
        public string ID
        {
            get => GetProperty(ConfigHelper.Instance.EquipmentNo);
            set => SetProperty(value);
        }

        /// <summary>
        /// 登录时显示的文本
        /// </summary>
        public string TextLogin
        {
            get => GetProperty("");
            set => SetProperty(value);
        }

        /// <summary>
        /// 登入时候，进度条进度
        /// </summary>
        public int ProgressBarValue
        {
            get => GetProperty(0);
            set => SetProperty(value);
        }
        /// <summary>
        /// 登录时记录版本号，上传用
        /// </summary>
        public string Version { get; set; }


    }
}
