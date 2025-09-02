using Gurux.Common;
using System;

namespace ZH.Rx_31.Helper
{

    public class CommunicationHelper
    {
        readonly Gurux.Serial.GXSerial Cl = new Gurux.Serial.GXSerial();
        public string meterNumber = "";
        public string ReceivedText = "";
        public string Port { get; set; }
        public string Baudrate { get; set; }
        public void OpenPort()
        {
            try
            {
                string[] arrybaudRate = Baudrate.Split(',');
                Cl.PortName = Port;
                Cl.BaudRate = int.Parse(arrybaudRate[0]);
                Cl.DataBits = int.Parse(arrybaudRate[2]);
                if (arrybaudRate[1] == "e")
                {
                    Cl.Parity = System.IO.Ports.Parity.Even;
                }
                else
                {
                    Cl.Parity = System.IO.Ports.Parity.None;

                }
                Cl.StopBits = System.IO.Ports.StopBits.One;
                Cl.OnReceived -= new ReceivedEventHandler(this.OnReceived);
                Cl.OnReceived += new ReceivedEventHandler(this.OnReceived);

                Cl.OnError -= new ErrorEventHandler(this.gxSerial1_OnError);
                Cl.OnError += new ErrorEventHandler(this.gxSerial1_OnError);
                if (Cl.IsOpen == false)
                {
                    Cl.Open();

                }
            }
            catch
            {
                //Test.Utility.Helper.PublicHelper.WriteErrorMsg(ex.Message);
            }

        }
        public void OpenPort(string port, string baudrate)
        {
            try
            {
                string[] arrybaudRate = baudrate.Split(',');
                Cl.PortName = port;
                Cl.BaudRate = int.Parse(arrybaudRate[0]);
                Cl.DataBits = int.Parse(arrybaudRate[2]);
                if (arrybaudRate[1] == "e")
                {
                    Cl.Parity = System.IO.Ports.Parity.Even;
                }
                else
                {
                    Cl.Parity = System.IO.Ports.Parity.None;

                }
                Cl.StopBits = System.IO.Ports.StopBits.One;
                Cl.OnReceived -= new ReceivedEventHandler(this.OnReceived);
                Cl.OnReceived += new ReceivedEventHandler(this.OnReceived);

                Cl.OnError -= new ErrorEventHandler(this.gxSerial1_OnError);
                Cl.OnError += new ErrorEventHandler(this.gxSerial1_OnError);
                if (Cl.IsOpen == false)
                {
                    Cl.Open();

                }
            }
            catch
            {

            }

        }
        public void ClosePort()
        {
            try
            {
                Cl.Close();
            }
            catch
            {
                //Helper.PublicHelper.WriteErrorMsg(Ex.Message);
            }
        }
        //private void OnReceivedFun(object sender, ReceiveEventArgs e)
        //{

        //    try
        //    {

        //    }
        //    catch (Exception Ex)
        //    {

        //    }
        //}
        private void OnReceived(object sender, ReceiveEventArgs e)
        {
            try
            {
                // Byte array received from GXSerial, and must be changed to chars.

                ReceivedText += GXCommon.ToHex((byte[])e.Data);


                //ReceivedText += System.Text.Encoding.ASCII.GetString((byte[])e.Data);
                //Helper.PublicHelper.WriteLogMsg(ReceivedText, meterNumber, "收");

            }
            catch
            {
                //Helper.PublicHelper.WriteErrorMsg(Ex.Message);
            }
        }
        public void SendData(string code)
        {
            ReceivedText = "";
            Cl.Send(code);

            //Helper.PublicHelper.WriteLogMsg(code, meterNumber, "发");

        }

        public void SendDataHex(string code)
        {
            ReceivedText = "";
            if (Cl.IsOpen)
            {
                Cl.Send(GXCommon.HexToBytes(code));
            }
            else
            {
                Cl.Open();
                Cl.Send(GXCommon.HexToBytes(code));
            }
            Gurux.Common.ReceiveParameters<byte[]> p = new Gurux.Common.ReceiveParameters<byte[]>()
            {
                WaitTime = Convert.ToInt32(5000),
                Count = Convert.ToInt32(0)
            };

            //Helper.PublicHelper.WriteLogMsg(code, meterNumber, "发");

        }
        public void SendDataSynchronous(string code, out string reply)
        {
            try
            {
                reply = "";
                Cl.OnReceived -= new ReceivedEventHandler(this.OnReceived);
                lock (Cl.Synchronous)
                {
                    Gurux.Common.ReceiveParameters<byte[]> p = new Gurux.Common.ReceiveParameters<byte[]>()
                    {
                        WaitTime = Convert.ToInt32(1000),
                        Eop = '\r',
                    };
                    Cl.Send(code);
                    Cl.Send('\r');

                    //Helper.PublicHelper.WriteLogMsg(code, meterNumber, "发");

                    if (Cl.Receive(p))
                    {
                        // p.Reply;  返回
                        reply = Convert.ToString(p.Reply);
                        // Helper.PublicHelper.WriteLogMsg(reply, meterNumber, "收");

                    }
                }
            }
            catch
            {
                reply = "";
                // Helper.PublicHelper.WriteErrorMsg(Ex.Message);
            }
        }
        private void gxSerial1_OnError(object sender, Exception ex)
        {
            try
            {
                Cl.Close();
                // Helper.PublicHelper.WriteErrorMsg(ex.Message);

            }
            catch
            {
                // Helper.PublicHelper.WriteErrorMsg(Ex.Message);
            }
        }
    }
}
