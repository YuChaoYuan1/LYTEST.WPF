using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace LYTest.Mis.Houda
{
    /// <summary>
    /// 与厚达公司 通讯消息包体
    /// </summary>
    public class XmlMsg : EventArgs
    {
        public string strSysCode = string.Empty;

        /// <summary>
        /// 台体状态  0-空闲； 1-检测中； 2-暂停；3-停止；4-完成； 5-关机中；8-不合格率报警9-异常
        /// </summary>
        public string DeviceStart = string.Empty;
        //消息头
        public HeadMsg headMsg = new HeadMsg();
        /// <summary>
        /// 通信协议属性
        /// </summary>
        public NetworkMessageAttribute MessageAttribute = NetworkMessageAttribute.heartbeat;

        //消息体
        public string Body { get; set; }

        /// <summary>
        /// 自检指令状态
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// 测试结果指令 错误指令
        /// </summary>
        public string Err { get; set; }

        public string Des { get; set; }

        /// <summary>
        /// 设备ID
        /// </summary>
        public string DeskId { get; set; }

        /// <summary>
        /// 操作类型
        /// </summary>
        public string OP { get; set; }

        /// <summary>
        /// 任务id  1-设置检定数量,准备检定；2-修改检定数量;3-开始检定
        /// </summary>
        public string Task { get; set; }

        /// <summary>
        /// 检定数量
        /// </summary>
        public string Num { get; set; }

        /// <summary>
        /// 查询表位是否有表 时使用  “机器人编号”
        /// </summary>
        public string RobotId { get; set; }

        /// <summary>
        /// 用于查询表位有没有 表时 使用
        /// </summary>
        public int[] Positionns { get; set; }

        /// <summary>
        /// 返回表位 是否有电表 10代表无表、9代表有表
        /// </summary>
        public string[] BwHaveMeterStatus { get; set; }


        /// <summary>
        /// 合成消息体
        /// </summary>
        /// <param name="bHaveBody"></param>
        /// <returns></returns>
        public string ComposeXml(bool bHaveBody)
        {
            string strValue = string.Empty;
            if (bHaveBody)
            {
                headMsg.CurrentTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                switch (headMsg.Command) //发送给主控平台的9997 ping     4010台体状态
                {
                    case "9998":  //登入指令
                        {
                            //<BODY><ID>DG01</ID><PASS>DG01</PASS></BODY>
                            Body = "<BODY>" + "<ID>" + strSysCode + "</ID>" + "<PASS>" + strSysCode + "</PASS>" + "</BODY>";
                            MessageAttribute = NetworkMessageAttribute.登陆9998;
                        }
                        break;
                    case "2008":  //电测台体状态变化
                        {
                            //< BODY >< DESKID > 台体编号 </ DESKID >< STATUS > 台体状态<STATUS></ BODY >
                            Body = "<BODY>" + "<DESKID>" + strSysCode + "</DESKID>" + "<STATUS>" + DeviceStart + "</STATUS>" + "</BODY>";
                            MessageAttribute = NetworkMessageAttribute.电测台体状态变化2008;
                        }
                        break;
                        
                    case "2001":
                        {
                            Body = "<BODY>" + "<STATUS>" + Status + "</STATUS>" + "</BODY>";
                            MessageAttribute = NetworkMessageAttribute.自检2001;
                        }
                        break;
                    case "2005":   //检定完成
                        {
                            Body = "<BODY>" + "<ERR>" + Err + "</ERR>" + "<DES>" + Des + "</DES>" + "</BODY>";
                            MessageAttribute = NetworkMessageAttribute.测试结果通知2005;
                        }
                        break;
                }
                
                strValue = "<MSG>" + headMsg.ComposeHead() + Body + "</MSG>";
            }
            else
            {
                strValue = "<MSG>" + headMsg.ComposeHead() + "</MSG>";
            }
            return strValue;
        }

        /// <summary>
        /// 解析返回的数据
        /// </summary>
        /// <param name="strXmlValue"></param>
        public void AnalysisXml(string strXmlValue)
        {
            //<MSG><HEAD>
            //<FROM>KL01</FROM >
            //<TO>Main</TO>
            //<TIME>2015-12-14 13:42:01</TIME >
            //<SEQ>1</SEQ>
            //<CMD>9998</CMD>
            //<TYPE>1</TYPE>
            //</HEAD></MSG>
            headMsg.FromStart = GetXmlNodeStringValue("//MSG/HEAD/FROM", strXmlValue);    //发起方
            headMsg.ToRecive = GetXmlNodeStringValue("//MSG/HEAD/TO", strXmlValue);      //接收方
            headMsg.CurrentTime = GetXmlNodeStringValue("//MSG/HEAD/TIME", strXmlValue);   //时间戳
            headMsg.Seq = GetXmlNodeStringValue("//MSG/HEAD/SEQ", strXmlValue);           // 帧序号
            headMsg.Command = GetXmlNodeStringValue("//MSG/HEAD/CMD", strXmlValue);       //指令号
            headMsg.CmdType = GetXmlNodeStringValue("//MSG/HEAD/TYPE", strXmlValue);      //指令类型
            //
            switch (headMsg.Command)
            {
                case "9999": //应答指令
                    {
                        MessageAttribute = NetworkMessageAttribute.应答指令9999;
                    }
                    break;
                case "1006":
                case "3019":
                case "5004":// 主控发送给我们控制指令  0-不执行； 1-暂停检定； 2-停止检定；3-继续检定； 9-关闭计算机
                    {
                        //电测台体控制指令
                        DeskId = GetXmlNodeStringValue("//MSG/BODY/DESKID", strXmlValue);

                        OP = GetXmlNodeStringValue("//MSG/BODY/OP", strXmlValue);

                        MessageAttribute = NetworkMessageAttribute.台体控制指令1006;
                    }
                    break;
                case "7005": //关闭检定台计算机PC
                    OP ="9";
                    MessageAttribute = NetworkMessageAttribute.台体控制指令1006;
                    break;
                case "1004": //开始检定
                    {
                        MessageAttribute = NetworkMessageAttribute.测试通知1004;
                    }
                    break;
                case "7006": //任务设置
                    OP = GetXmlNodeStringValue("//MSG/BODY/STATUS", strXmlValue);    //操作（1-设置检定数量,准备检定；2-修改检定数量;3-开始检定）
                    Task = GetXmlNodeStringValue("//MSG/BODY/TASK", strXmlValue); //任务ID
                    Num = GetXmlNodeStringValue("//MSG/BODY/NUM", strXmlValue);  //检定
                    MessageAttribute = NetworkMessageAttribute.任务设置7006;
                    break;
                default:
                    break;
            }
        }
        /// <summary>
        /// 解析表位数据
        /// </summary>
        /// <param name="xmlNodeName">节点关键字</param>
        /// <param name="strXmlValue">Xml 数据</param>
        /// <returns></returns>
        private int[] GetXmlNodeBWStringValue(string xmlNodeName, string strXmlValue)
        {

            int BwNum = 0;
            List<int> listBw = new List<int>();
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(strXmlValue);
            //读所有节点表
            XmlNamespaceManager xnm = new XmlNamespaceManager(doc.NameTable);
            //读取节点值
            XmlNode node = doc.SelectSingleNode(xmlNodeName, xnm);

            foreach (XmlNode nd in node.ChildNodes)
            {
                if (nd.Name.Equals("M"))
                {
                    BwNum = Convert.ToInt32(nd.InnerText);
                    listBw.Add(BwNum);
                }
            }
            //读取节点值

            return listBw.ToArray();
        }

        /// <summary>
        /// 解析Xml数据
        /// </summary>
        /// <param name="strXmlValue"></param>
        private string GetXmlNodeStringValue(string xmlNodeName, string strXmlValue)
        {

            string returnString = string.Empty;
            XmlDocument doc = new XmlDocument();
            strXmlValue = strXmlValue.TrimEnd();
            strXmlValue = strXmlValue.Trim('\0');
            if (strXmlValue.Length > 0)
            {
                doc.LoadXml(strXmlValue);
                //读所有节点表
                XmlNamespaceManager xnm = new XmlNamespaceManager(doc.NameTable);
                //读取节点值
                XmlNode node = doc.SelectSingleNode(xmlNodeName, xnm);
                //读取节点值
                returnString = node.InnerText;
            }

            return returnString;
        }

    }

    /// <summary>
    /// 消息头
    /// </summary>
    public class HeadMsg
    {
        //<FROM>发起方</ FROM > 
        public string FromStart { get; set; }

        //<TO>接收方</TO> 
        public string ToRecive { get; set; }

        //<TIME>时间戳</ TIME > 
        public string CurrentTime { get; set; }

        //<SEQ>帧序号</ SEQ>
        public string Seq { get; set; }

        //<CMD>1109</ CMD >
        public string Command { get; set; }

        //<TYPE>指令类型</TYPE> 
        public string CmdType { get; set; }


        /// <summary>
        /// 合成消息头
        /// </summary>
        /// <returns></returns>
        public string ComposeHead()
        {
            return "<HEAD>" + "<FROM>" + FromStart + "</FROM>"
                + "<TO>" + ToRecive + "</TO>" + "<TIME>" + CurrentTime + "</TIME>"
                + "<SEQ>" + Seq + "</SEQ>" + "<CMD>" + Command + "</CMD>"
                + "<TYPE>" + CmdType + "</TYPE>" + "</HEAD>";

        }

    }
}
