using LYTest.DAL.Config;
using LYTest.Mis.Common;

namespace LYTest.Mis
{
    public class MISFactory
    {
        public static IMis mis = null;
        static object Lock=new object();
        public static IMis Create(string UpDownUri = "MDS")
        {
            lock (Lock)
            {
                if (mis != null) return mis;
                string type;
                string ip;
                int port;
                string dataSource;
                string userId;
                string pwd;
                string url;
                string sysno;
                if (UpDownUri == "MDS")
                {
                    type = ConfigHelper.Instance.MDS_Type;
                    ip = ConfigHelper.Instance.MDSProduce_IP;
                    port = int.Parse(ConfigHelper.Instance.MDSProduce_Prot);
                    dataSource = ConfigHelper.Instance.MDSProduce_DataSource;
                    userId = ConfigHelper.Instance.MDSProduce_UserName;
                    pwd = ConfigHelper.Instance.MDSProduce_UserPassWord;
                    url = ConfigHelper.Instance.MDS_WebService;
                    sysno = ConfigHelper.Instance.MDS_SysNo;
                }
                else
                {
                    type = ConfigHelper.Instance.Marketing_Type;
                    ip = ConfigHelper.Instance.MarketingProduce_IP;
                    port = int.Parse(ConfigHelper.Instance.MarketingProduce_Prot);
                    dataSource = ConfigHelper.Instance.MarketingProduce_DataSource;
                    userId = ConfigHelper.Instance.MarketingProduce_UserName;
                    pwd = ConfigHelper.Instance.MarketingProduce_UserPassWord;
                    url = ConfigHelper.Instance.Marketing_WebService;
                    sysno = ConfigHelper.Instance.Marketing_SysNo;
                }
                switch (type)
                {
                    case "厚达":
                        mis = new Houda.Houda(ip, port, dataSource, userId, pwd, url);
                        break;
                    case "新疆生产调度平台":
                        mis = new NanRui.NanRui(ip, port, dataSource, userId, pwd, url, sysno);
                        break;
                    case "生产调度平台":
                        mis = new NanRui.NanRui(ip, port, dataSource, userId, pwd, url, sysno);
                        break;
                    case "东软SG186":
                        mis = new SG186.SG186(ip, port, dataSource, userId, pwd, url);
                        break;
                    case "黑龙江调度平台":
                        mis = new HEB.HeiLongJian(ip, port, dataSource, userId, pwd, url);
                        break;
                    case "西安调度平台":
                        mis = new XiAn.XiAnProject(ip, port, dataSource, userId, pwd, url);
                        break;
                    //add yjt 20230131 合并张工河北代码SG186LX
                    case "朗新SG186":
                        mis = new SG186LX.SG186(ip, port, dataSource, userId, pwd, url);
                        break;
                    //add yjt 20221121 国金源富接口
                    case "国金源富接口":
                        mis = new GuoJin.GuoJin(ip, port, dataSource, userId, pwd, url);
                        break;
                    case "天津MIS接口":
                        mis = new TianJin.TianJin(ip, port, dataSource, userId, pwd, url);
                        break;
                    case "河北营销2.0":
                        mis = new HeBei20.HeBei20(ip, port, dataSource, userId, pwd, url);
                        break;
                    //case "新疆隆元主控":
                    //    mis = new LY20.Api(ip, port, dataSource, userId, pwd, url);
                    //    break;
                    case "上海营销2.0":
                        mis = new ShangHai20.ShangHai20(ip, port, dataSource, userId, pwd, url, sysno);
                        break;
                    case "智慧计量工控平台":
                        //mis = new IMICP.SmartmeteringIndustrialPlatform(ip, port, dataSource, userId, pwd, url, sysno);
                        mis = new WLMQ.MDS(ip, port, sysno);
                        break;
                    case "智慧计量工控平台_新疆":
                        mis = new ZHGK_XinJiang.MDS(ip, port, sysno);
                        break;
                    default:
                        break;
                }
                return mis;
            }
        }
    }
}
