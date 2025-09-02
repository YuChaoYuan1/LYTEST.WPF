using LYTest.MeterProtocol.Protocols.DLT698;

namespace LY.Test
{
    internal class MainVM : ViewModelBase
    {

        public string? Input
        {
            get => GetPropertyValue("");
            set => SetPropertyValue(value);
        }

        public string? Output
        {
            get => GetPropertyValue("");
            set => SetPropertyValue(value);
        }

        public void Ansy()
        {
            if (Input == null) return;

            string? str = Input.Replace("-", "");
            byte[] bytes = new byte[str.Length / 2];
            for (int i = 0; i < bytes.Length; i++)
            {
                bytes[i] = Convert.ToByte(str.Substring(i * 2, 2), 16);
            }

            //StPackParas para = new()
            //{
            //    MeterAddr = "924230002874",
            //    SecurityMode = EmSecurityMode.ClearText,
            //    OD = ["00300400"],
            //    SidMac = new StSIDMAC(),
            //    GetRequestMode = EmGetRequestMode.GetRequestNormal
            //};

            //DataLinkLayer datalink = new(para.MeterAddr)
            //{
            //    APDU = new ReadDataAPDU(para.OD, EmSecurityMode.ClearText, para.GetRequestMode)
            //};
            //sendData = datalink.PackFrame(EmServieType.GET_Request);

            List<object> data = [];
            List<object> reportdata = [];
            int errCode = 0;
            DataLinkLayer datalink = new();
            datalink.ParseFrame(bytes, ref errCode, ref data, ref reportdata);
        }
    }
}
