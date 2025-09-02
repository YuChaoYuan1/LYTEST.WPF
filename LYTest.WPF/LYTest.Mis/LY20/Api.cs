//using LYTest.Core.Model.Meter;
//using LYTest.Core.Model.Schema;
//using LYTest.Mis.Common;
//using System;
//using System.Collections.Generic;
//using System.Windows.Forms;

//namespace LYTest.Mis.LY20
//{
//    public class Api : IMis
//    {

//        //private readonly bool Allow = false;
//        private readonly string ip;
//        private readonly int port;
//        private readonly string dataSource;
//        private readonly string userId;
//        private readonly string pwd;
//        private readonly string url;

//        public Api(string ip, int port, string dataSource, string userId, string pwd, string url)
//        {
//            this.ip = ip;
//            this.port = port;
//            this.dataSource = dataSource;
//            this.userId = userId;
//            this.pwd = pwd;
//            this.url = url;
//        }
//        //public bool Down(string stationId, ref TestMeterInfo[] meter, out string msg)
//        //{
//        //    msg = "";
//        //    return false;
//        //}
//        public bool Down(string barcode, ref TestMeterInfo meter)
//        {
//            throw new NotImplementedException();
//        }

//        public bool SchemeDown(string barcode, out string schemeName, out Dictionary<string, SchemaNode> Schema)
//        {
//            throw new NotImplementedException();
//        }

//        //public bool SchemeDown(TestMeterInfo meter, out string schemeName, out Dictionary<string, SchemaNode> Schema, out string msg)
//        //{
//        //    schemeName = "";
//        //    Schema = new Dictionary<string, SchemaNode>();
//        //    msg = "";
//        //    return false;
//        //}

//        public bool SchemeDown(TestMeterInfo barcode, out string schemeName, out Dictionary<string, SchemaNode> Schema)
//        {
//            throw new NotImplementedException();
//        }

//        public void ShowPanel(Control panel)
//        {
//            throw new NotImplementedException();
//        }

//        public bool Update(TestMeterInfo meter)
//        {
//            return false;
//        }

//        //public bool Update(TestMeterInfo[] meters, int maxtimems, out string msg)
//        //{
//        //    msg = "";
//        //    return false;
//        //}

//        public bool UpdateCompleted()
//        {
//            return true;
//        }

//        public void UpdateInit()
//        {

//        }

//        //public bool UploadVersion(string stationId, string version, out string msg)
//        //{
//        //    msg = "";
//        //    if (!Allow)
//        //    {
//        //        msg = "登录数据服务失败！";
//        //        return false;
//        //    }

//        //    return false;

//        //}
//    }
//}
