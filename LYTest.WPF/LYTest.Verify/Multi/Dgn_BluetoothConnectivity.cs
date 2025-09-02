using LYTest.ViewModel.CheckController;

namespace LYTest.Verify.Multi
{
    ////  ///add lsj 20220218 蓝牙连接
    //蓝牙连接
    class Dgn_BluetoothConnectivity : VerifyBase
    {
        public override void Verify()
        {

          //  //首先创建一个蓝牙对象
          //  BluetoothClient Blueclient = new BluetoothClient();
          //  //用一个键值对保存收索到的蓝牙地址
          //  Dictionary<string, BluetoothAddress> deviceAddresses = new Dictionary<string, BluetoothAddress>();
          //  //设置蓝牙无线电状态
          //  BluetoothRadio BlueRadio = BluetoothRadio.PrimaryRadio;
          //  BlueRadio.Mode = RadioMode.Connectable   //可连接的
          ////监听服务，即监听附近的蓝牙设备并保存
          //   BluetoothDeviceInfo[] Devices = Blueclient.DiscoverDevices();
          //  //将监听到的服务添加到deviceAddresses保存。并与蓝牙的名字相对应。
          //  divceAddresses[device.DeviceName] = device.DeviceAddress;
          //  //连接蓝牙
          //  //设置匹配码,从txtPwd获取，并去掉前后空白字符Trim（）
          //  Blueclient.SetPin(DeviceAddress, txtPwd.Text.Trim())
          // //连接蓝牙，对不同的设备有不同的服务类型，要匹配
          //   Blueclient.Connect(DeviceAddress, BluetoothSevice.SerialPort);
          //  //发送信号
          //  //获取蓝牙数据流
          //  System.IO.Stream stream = Blueclient.GetStream();
          //  //有string型信号message
          //  //将string型信号message转换成字节数组，再发送
          //  byte[] by = System.Text.Encoding.ASCII.GetBytes(message);
          //  stream.Write(by, 0, by.Length);

        }
        /// <summary>
        /// 检定参数是否合法
        /// </summary>
        /// <returns></returns>
        protected override bool CheckPara()
        {
            ResultNames = new string[] { "检定项目", "结论" };
            return true;
        }
    }
}
