using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LYTest.ViewModel.Debug
{
    public class SocketViewModel : ViewModelBase
    {
        public void ConnSocket()
        {
            //初始化与厚达通讯 模块。
            //string HouDaIP = ConfigHelper.Instance.Marketing_IP;
            //int HouDaPort = int.Parse(ConfigHelper.Instance.Marketing_Prot);
            //string XianTiCode = "";
            //int iHdPort = Convert.ToInt32(HouDaPort);
            //EquipmentData.HouTcpCommunication.ConnectServer();

            //发送自检指令
            Mis.Houda.XmlMsg xml = new Mis.Houda.XmlMsg
            {
                Status = "2"
            };
            xml.headMsg.ToRecive = "Main";
            xml.headMsg.CmdType = "1";
            xml.headMsg.CurrentTime = string.Empty;
            xml.headMsg.Command = "2001";
            EquipmentData.HouTcpCommunication.SendNetworkMessage(xml);
        }
        public void SendData()
        {
            //发送自检指令
            Mis.Houda.XmlMsg xml = new Mis.Houda.XmlMsg
            {
                Status = "2"
            };
            xml.headMsg.ToRecive = "Main";
            xml.headMsg.CmdType = "1";
            xml.headMsg.CurrentTime = DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss");
            xml.headMsg.Command = "2001";
            //EquipmentData.HouTcpCommunication.SendNetworkMessage(xml);
            if (EquipmentData.HouTcpCommunication != null)
                EquipmentData.HouTcpCommunication.SendNetworkMessage(xml);
        }

        public void SendTest()
        {

            if (EquipmentData.Controller.IsChecking == false)
            {
                //EquipmentData.CallMsg("VerifyCompelate");
                //EquipmentData.HouTcpCommunication.
                //EquipmentData.HouTcpCommunication. NetworkClientPatam.TcpClient.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, ReceiveFrameCallBack, NetworkClientPatam.TcpClient);

            }
        }

    }
}
