using System;
using System.Collections.Generic;
using System.Xml;
using System.IO;

namespace LYTest.Core
{
    [Serializable()]
    /// <summary>
    /// 多功能通信协议配置模型
    /// </summary>
    public class DgnProtocolInfo
    {

        #region 构造函数
        /// <summary>
        /// 多功能通信协议配置模型
        /// </summary>
        public DgnProtocolInfo() : this("")
        { }

        /// <summary>
        /// 构造函数，根据协议名称获取通信协议
        /// </summary>
        /// <param name="ProtocolName">协议名称</param>
        public DgnProtocolInfo(string protocolName)
        {
            #region 协议模型结构部分
            ProtocolName = protocolName;
            DllFile = "";
            ClassName = "";
            Setting = "";
            UserID = "";
            VerifyPasswordType = 0;
            WritePassword = "";
            WriteClass = "";
            ClearDemandPassword = "";
            ClearDemandClass = "";
            ClearDLPassword = "";
            ClearDLClass = "";
            TariffOrderType = "1234";
            DateTimeFormat = "";
            SundayIndex = 0;
            FECount = 0;
            ClockPL = 1;
            DataFieldPassword = false;
            BlockAddAA = false;
            ConfigFile = "";
            DgnPras = new Dictionary<string, string>();
            HaveProgrammingkey = false;
            DecimalDigits = 2;
            //Loading = false;
            #endregion

            Load(ProtocolName);
        }

        #endregion

        #region-------------------协议模型结构部分----------------------


        /// <summary>
        /// 协议名称
        /// </summary>
        public string ProtocolName = "";

        /// <summary>
        /// 协议库名称
        /// </summary>
        public string DllFile = "";

        /// <summary>
        /// 协议类
        /// </summary>
        public string ClassName = "";

        /// <summary>
        /// 通信参数
        /// </summary>
        public string Setting = "";

        /// <summary>
        /// 用户代码
        /// </summary>
        public string UserID = "";

        /// <summary>
        /// 验证密码类型
        /// </summary>
        public int VerifyPasswordType = 0;
        #region 密码
        /// <summary>
        /// 一类写操作密码
        /// </summary>
        public string WritePassword = "";

        /// <summary>
        /// 一类写操作密码等级
        /// </summary>
        public string WriteClass = "";

        /// <summary>
        /// 二类写操作密码/写密码
        /// </summary>
        public string WritePassword2 = "";

        /// <summary>
        /// 二类写操作密码等级/写等级
        /// </summary>
        public string WriteClass2 = "";

        private string clearDemandPassword = "";

        /// <summary>
        /// 清需量密码
        /// </summary>
        public string ClearDemandPassword
        {
            get { return this.clearDemandPassword; }
            set
            {
                this.clearDemandPassword = value;
            }

        }

        /// <summary>
        /// 清需量密码等级
        /// </summary>
        public string ClearDemandClass = "";

        /// <summary>
        /// 清电量密码
        /// </summary>
        public string ClearDLPassword = "";

        /// <summary>
        /// 清电量密码等级
        /// </summary>
        public string ClearDLClass = "";

        /// <summary>
        /// 清事件密码
        /// </summary>
        public string ClearEventPassword = "";

        /// <summary>
        /// 清事件等级
        /// </summary>
        public string ClearEventClass = "";

        /// <summary>
        /// 拉合闸密码
        /// </summary>
        public string RelayPassword = "";

        /// <summary>
        /// 拉合闸等级
        /// </summary>
        public string RelayClass = "";
        #endregion
        /// <summary>
        /// 费率排序（峰平谷尖2341）
        /// </summary>
        public string TariffOrderType = "2341";
        /// <summary>
        /// 日期时间格式
        /// </summary>
        public string DateTimeFormat = "";
        /// <summary>
        /// 星期天序号
        /// </summary>
        public int SundayIndex = 0;
        /// <summary>
        /// 下发帧的唤醒符个数
        /// </summary>
        public int FECount = 0;

        /// <summary>
        /// 时钟频率
        /// </summary>
        public float ClockPL = 1;

        /// <summary>
        /// 数据域是否包含密码
        /// </summary>
        public bool DataFieldPassword = false;

        /// <summary>
        /// 写块操作是否加AA
        /// </summary>
        public bool BlockAddAA = false;
        /// <summary>
        /// 配置文件
        /// </summary>
        public string ConfigFile = "";

        /// <summary>
        /// 协议参数列表，KEY值为协议测试项目ID，并非多功能试验项目ID
        /// </summary>
        public Dictionary<string, string> DgnPras;

        /// <summary>
        /// 区别有无编程键，false：无，true：有
        /// </summary>
        public bool HaveProgrammingkey = false;
        /// <summary>
        /// 电量小数位2/4
        /// </summary>
        public int DecimalDigits = 2;

        private bool _Loading = false;
        /// <summary>
        /// 标志检查（只读），如果loading为假表示加载协议失败！
        /// </summary>
        public bool Loading
        {
            get
            {
                return _Loading;
            }
        }

        #endregion



        #region ---------下面部分为协议文件操作配置部分-------------------- 

        /// <summary>
        /// 电能表制造厂家
        /// </summary>
        public string Factory { get; set; }

        /// <summary>
        /// 电能表型号
        /// </summary>
        public string ModelNo { get; set; }

        /// <summary>
        /// 根据协议名称加载协议信息
        /// </summary>
        /// <param name="ProtocolName"></param>
        public void Load(string ProtocolName)
        {
            LoadXmlData(ProtocolName, "", "");
        }
        /// <summary>
        /// 加载协议信息，调用该函数的前提是要么协议名称有值，要么制造厂家和表型号有值
        /// </summary>
        /// <returns></returns>
        public bool Load()
        {
            if (this.ProtocolName == "" || (this.Factory == "" && this.ModelNo == ""))
            {
                return false;
            }

            this.LoadXmlData(this.ProtocolName, this.Factory, this.ModelNo);

            return true;
        }


        public static XmlNode NodeProtocols = null;
        /// <summary>
        /// 加载XML文档
        /// </summary>
        /// <param name="protocolname"></param>
        /// <param name="factroy"></param>
        /// <param name="size"></param>
        private void LoadXmlData(string protocolname, string factory, string size)
        {

            if (protocolname == "" && (factory == "" || size == ""))
                return;

            // protocolname = "DLT645-2007-Default";
            XmlControl _XmlNode = new XmlControl(string.Format(Directory.GetCurrentDirectory() + "\\Xml\\AgreementConfig.xml"));
            if (_XmlNode == null || _XmlNode.Count() == 0) return;

            System.Xml.XmlNode _FindXmlNode = null;

            if (protocolname != "")
                _FindXmlNode = XmlControl.FindSencetion(_XmlNode.RootNode(), XmlControl.XPath(string.Format("R,Name,{0}", protocolname)));

            //_FindXmlNode = XmlControl.FindSencetion(_XmlNode.RootNode(), "//R[@Name='DLT645-2007-Default']");

            if (_FindXmlNode == null) return;

            #region--------------加载协议文件信息---------------------

            this.ProtocolName = protocolname;         //协议名称 

            this.Factory = "";

            this.ModelNo = "";

            //add zxg yjt 20220311 新增协议库名称获取
            ProtocolName = XmlControl.GetValue(_FindXmlNode, "ClassName");//协议库名称

            this.DllFile = XmlControl.GetValue(_FindXmlNode, "ClassName");  //协议库名称
            this.ClassName = XmlControl.GetValue(_FindXmlNode, "ClassName");  //说使用协议类名称
            this.Setting = XmlControl.GetValue(_FindXmlNode, "Setting"); //通信参数
            this.UserID = XmlControl.GetValue(_FindXmlNode, "UserID");  //用户名
            this.VerifyPasswordType = int.Parse(XmlControl.GetValue(_FindXmlNode, "VerifyPasswordType"));//验证类型

            XmlNode nodeFeiLv = _FindXmlNode.SelectSingleNode("FeiLvId");
            //费率类型
            if (nodeFeiLv.Attributes["ShenGu"] == null)
            {
                TariffOrderType = nodeFeiLv.Attributes["Jian"].Value + nodeFeiLv.Attributes["Feng"].Value + nodeFeiLv.Attributes["Ping"].Value + nodeFeiLv.Attributes["Gu"].Value + "5";
            }
            else
            {
                TariffOrderType = nodeFeiLv.Attributes["Jian"].Value + nodeFeiLv.Attributes["Feng"].Value + nodeFeiLv.Attributes["Ping"].Value + nodeFeiLv.Attributes["Gu"].Value + nodeFeiLv.Attributes["ShenGu"].Value;
            }
            this.DateTimeFormat = XmlControl.GetValue(_FindXmlNode, "DateTimeFormat"); //日期格式
            this.SundayIndex = int.Parse(XmlControl.GetValue(_FindXmlNode, "SundayIndex")); //星期天表示
            this.ClockPL = 1F; //时钟频率
            this.FECount = int.Parse(XmlControl.GetValue(_FindXmlNode, "FECount"));     //唤醒FE个数
            this.DataFieldPassword = bool.Parse(XmlControl.GetValue(_FindXmlNode, "DataFieldPassword"));  //数据域是否包含密码
            this.BlockAddAA = bool.Parse(XmlControl.GetValue(_FindXmlNode, "BlockAddAA")); //写数据块是否加AA    
            this.ConfigFile = ""; //配置文件    
            this.HaveProgrammingkey = bool.Parse(XmlControl.GetValue(_FindXmlNode, "HaveProgrammingkey"));//有无编程键

            if (string.IsNullOrWhiteSpace(XmlControl.GetValue(_FindXmlNode, "DecimalDigits")))
                DecimalDigits = 2;
            else
                DecimalDigits = int.Parse(XmlControl.GetValue(_FindXmlNode, "DecimalDigits"));


            //有编程键
            XmlNodeList nodeWithKey;
            if (HaveProgrammingkey)
            {
                nodeWithKey = _FindXmlNode.SelectNodes("OperationsHaveKey/Operation");

                foreach (XmlNode item in nodeWithKey)
                {
                    switch (item.Attributes["Name"].Value)
                    {
                        case "Write":
                            WritePassword = item.Attributes["Password"].Value;
                            WriteClass = item.Attributes["Class"].Value;
                            break;
                        case "ClearEnergy":
                            ClearDLPassword = item.Attributes["Password"].Value;
                            ClearDLClass = item.Attributes["Class"].Value;
                            break;
                        case "ClearDemmand":
                            ClearDemandPassword = item.Attributes["Password"].Value;
                            ClearDemandClass = item.Attributes["Class"].Value;
                            break;
                        case "ClearEvent":
                            ClearEventPassword = item.Attributes["Password"].Value;
                            ClearEventClass = item.Attributes["Class"].Value;
                            break;
                        case "SwitchControl":
                            RelayPassword = item.Attributes["Password"].Value;
                            RelayClass = item.Attributes["Class"].Value;
                            break;
                        default:
                            break;
                    }
                }

            }
            else
            {
                //无编程键
                nodeWithKey = _FindXmlNode.SelectNodes("OperationsNoKey/Operation");

                foreach (XmlNode item in nodeWithKey)
                {
                    switch (item.Attributes["Name"].Value)
                    {
                        case "WriteLevel1":
                            WritePassword = item.Attributes["Password"].Value;
                            WriteClass = item.Attributes["Class"].Value;
                            break;
                        case "WriteLevel2":
                            WritePassword2 = item.Attributes["Password"].Value;
                            WriteClass2 = item.Attributes["Class"].Value;
                            break;
                        case "ClearMeter":
                            ClearDLPassword = item.Attributes["Password"].Value;
                            ClearDLClass = item.Attributes["Class"].Value;
                            break;
                        case "ClearDemmand":
                            ClearDemandPassword = item.Attributes["Password"].Value;
                            ClearDemandClass = item.Attributes["Class"].Value;
                            break;
                        case "ClearEvent":
                            ClearEventPassword = item.Attributes["Password"].Value;
                            ClearEventClass = item.Attributes["Class"].Value;
                            break;
                        case "SwitchControl":
                            RelayPassword = item.Attributes["Password"].Value;
                            RelayClass = item.Attributes["Class"].Value;
                            break;
                        default:
                            break;
                    }
                }
            }


            _FindXmlNode = XmlControl.FindSencetion(_FindXmlNode, XmlControl.XPath("Prjs"));          //转到项目数据节点

            this._Loading = true;                //改写加载标志，表示加载协议成功

            this.DgnPras = new Dictionary<string, string>();

            if (_FindXmlNode == null) return;

            for (int i = 0; i < _FindXmlNode.ChildNodes.Count; i++)
            {
                this.DgnPras.Add(_FindXmlNode.ChildNodes[i].Attributes["ID"].Value, _FindXmlNode.ChildNodes[i].ChildNodes[0].Value);        //加入ID，值
            }

            if (this.DgnPras.Count == 0) return;



            #endregion




        }
        #endregion
    }
}
