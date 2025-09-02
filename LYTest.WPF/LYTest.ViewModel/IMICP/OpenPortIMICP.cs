using LYTest.DAL;
using LYTest.DAL.Config;
using LYTest.Mis.Common;
using LYTest.Mis.IMICP;
using LYTest.Utility.Log;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;

namespace LYTest.ViewModel.IMICP
{
    class OpenPortIMICP : EquipmentData
    {
        private static int 上报频率 = int.Parse(ConfigHelper.Instance.OperatingConditionsUpdataF);
        private static readonly string 上报Url = string.Format("tcp://{0}:{1}", ConfigHelper.Instance.OperatingConditionsIp.Trim(), ConfigHelper.Instance.OperatingConditionsProt.Trim());
        public void openFWQ()
        {
            StateApi();
        }

        public static void ReceData(string Tstr)
        {
            string str = Tstr.Substring(0, 4);
            if (str == "12.8")
            {
                string table = Tstr.Substring(4);
                ShanXiTable recv = JsonHelper.反序列化字符串<ShanXiTable>(table);
                上报频率 = int.Parse(recv.frequency);

                //查询之前的配置信息
                DynamicModel model = DALManager.ApplicationDbDal.GetByID(EnumAppDbTable.T_CONFIG_PARA_VALUE.ToString(), string.Format("CONFIG_NO = '{0}'", "05002"));
                if (model != null)
                {
                    string[] strData = model.GetProperty("CONFIG_VALUE").ToString().Split('|');
                    strData[3] = 上报频率.ToString();

                    model.SetProperty("CONFIG_NO", model.GetProperty("CONFIG_NO").ToString());
                    model.SetProperty("CONFIG_VALUE", String.Join("|", strData));
                    DALManager.ApplicationDbDal.Update(EnumAppDbTable.T_CONFIG_PARA_VALUE.ToString(), string.Format("ID={0}", 49), model, new List<string> { "CONFIG_NAME", "CONFIG_VALUE" });
                }
                LogManager.AddMessage(string.Format("上报频率改变为{0}", 上报频率));

                //Thread thread = new Thread(Button14_Click);
                //thread.Start();
            }
        }

        #region 服务器

        string ip = "";
        int port = 44309;
        private HttpServer httpServer = null;

        public string SelectIp()
        {
            string AddressIP = string.Empty;
            foreach (IPAddress _IPAddress in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
            {
                if (_IPAddress.AddressFamily.ToString() == "InterNetwork")
                {
                    AddressIP = _IPAddress.ToString();
                }
            }
            return AddressIP;
        }
        ///开启服务器
        public async void StateApi()
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(ConfigHelper.Instance.DevControllServerPort))
                {
                    port = Convert.ToInt32(ConfigHelper.Instance.DevControllServerPort);
                }
                ip = Convert.ToString(ConfigHelper.Instance.DevControllServerIP).Replace(" ", "");
                httpServer = new HttpServer(ip, port);
                await httpServer.StartHttpServer();
                LogManager.AddMessage("服务器开启成功IP：" + ip + "端口：" + port, EnumLogSource.服务器日志, EnumLevel.Information);
                LogManager.AddMessage(string.Format("服务器开启成功   IP:{0},端口：{1}", ip, port));
            }
            catch (Exception ex)
            {
                LogManager.AddMessage("服务器开启失败" + ex.Message, EnumLogSource.服务器日志, EnumLevel.Information);
                LogManager.AddMessage(string.Format("服务器开启失败：{0}  ", ex.Message));
            }
        }

        /// <summary>
        /// 停止服务器
        /// </summary>
        public async void EndApi()
        {
            try
            {
                ip = SelectIp();
                httpServer = new HttpServer(ip, port);
                await httpServer.CloseHttpServer();

            }
            catch
            {

            }
        }
        #endregion

        #region 工况数据上传


        string taskno = "";
        static string plantNo = ConfigHelper.Instance.PlantNO;



        /// <summary>
        /// 13.2设备事件上报接口  button11_Click
        /// </summary>
        /// <param name="plantEventType">事件类型</param>
        /// <param name="veriltemParaNo">试验项编号</param>
        public void EventEscalation(string plantEventType, string veriltemParaNo)
        {
            try
            {
                //判断任务号是否是一个
                for (int i = 0; i < EquipmentData.MeterGroupInfo.Meters.Count; i++)
                {

                }
                taskno = EquipmentData.MeterGroupInfo.Meters[0].GetProperty("MD_TASK_NO").ToString();  ///任务号
                //获取电表任务信息
                //0101:开机；
                //0102:关机；
                //0103:开始预热；
                //0104:结束预热；
                //0105:任务开始；
                //0106:任务结束；
                //0107:暂停；
                //0108:暂停结束;
                //0109:开始检修；
                //0110:结束检修；
                //0111:异常停机；
                //0112:异常停机结束；
                //0119:开始试验项；
                //0120:结束试验项；

                string eventTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                //string devSeatStatusList = null;

                string topic = "GK_JD_PLAN_EVENT_TOPIC";
                string topicName = "设备事件上报接口";

                JObject joSend = new JObject();
                joSend.Add("plantNo", plantNo);//设备编号
                joSend.Add("taskNo", taskno);//任务编号
                joSend.Add("plantEventType", plantEventType);//事件类型
                joSend.Add("eventTime", eventTime);//事件发生时间  
                joSend.Add("veriltemParaNo", veriltemParaNo);//试验项项参数编码  //项目编号

                JArray jaValues = new JArray();

                JObject joValue = new JObject();
                joValue.Add("devSeatNo", "0");//表位编号
                joValue.Add("statusCode", EquipmentData.StdInfo.Ua.ToString());//表位状态
                joValue.Add("barCode", EquipmentData.StdInfo.Ua.ToString());//条码

                jaValues.Add(joValue);
                joSend.Add("devSeatStatusList", jaValues);//事件类型   


                string jsonSend = joSend.ToString();

                KafkaClient kafka = new KafkaClient(上报Url);
                bool ret = kafka.SendToKafka(topic, jsonSend, 上报Url);
                if (ret)
                {
                    LogManager.AddMessage(string.Format("13.2---上报数据成功 Url:{0},topic:{1},接口描述:{2},上报数据Json内容:\r\n{3}", 上报Url, topic, topicName, jsonSend), 13);
                }
                else
                {

                }
            }
            catch
            {

            }

        }


        /// <summary>
        /// 13.4   完成
        /// </summary>
        /// <param name="curWkStatusCode">当前状态 </param>
        /// <param name="previousState">上一状态</param>
        public void WorkingStatus(string curWkStatusCode, string previousState)
        {
            try
            {
                //01:预热；02:空闲；03:离线；04:运行；05:告警；06:自校准
                string startTime = DateTime.Now.AddSeconds(-2).ToString("yyyy-MM-dd HH:mm:ss");

                string receiveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");


                string topic = "GK_JD_PLAN_WORKING_STATUS_TOPIC";
                string topicName = "设备工作状态信息上报接口";

                JObject joSend = new JObject();
                joSend.Add("plantNo", plantNo);//设备编号
                joSend.Add("curWkStatusCode", curWkStatusCode);//当前状态
                joSend.Add("startTime", startTime);//当前状态开始时间
                joSend.Add("previousState", previousState);//上一状态
                joSend.Add("receiveTime", receiveTime);//数据上报时间

                string jsonSend = joSend.ToString();
                KafkaClient kafka = new KafkaClient(上报Url);
                bool ret = kafka.SendToKafka(topic, jsonSend, 上报Url);
                if (ret)
                {
                    LogManager.AddMessage(string.Format("13.4---上报数据成功 Url:{0},topic:{1},接口描述:{2},上报数据Json内容:\r\n{3}", 上报Url, topic, topicName, jsonSend), 13);
                }
                else
                {

                }
            }
            catch
            {

            }


        }
        /// <summary>
        /// 13.6 不用传 告警  button 13
        /// </summary> 
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void AlarmData(string warnInfo, string warnSuggest)
        {
            try
            {
                //string curWkStatusCode = "04";//01:预热；02:空闲；03:离线；04:运行；05:告警；06:自校准
                string warnCode = "001";
                string startTime = DateTime.Now.AddSeconds(-2).ToString("yyyy-MM-dd HH:mm:ss");
                string warnLevel = "02";//01:紧急；02:一般；03:提示
                string plantWarnType = "02";//01;心跳；02; 网络;03; 功能;04; 安全;05; 检定质量 06:故障
                string epiPos = "";
                string receiveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                List<string> lis = new List<string>();
                lis.Add("01");

                string topic = "GK_JD_PLAN_ALARM_INFO_TOPIC";
                string topicName = "设备告警信息上报接口";

                JObject joSend = new JObject();
                joSend.Add("plantNo", plantNo);//设备编号
                joSend.Add("warnCode", warnCode);//告警代码
                joSend.Add("startTime", startTime);//当前状态开始时间
                joSend.Add("warnLevel", warnLevel);//告警等级
                joSend.Add("plantWarnType", plantWarnType);//告警类型
                joSend.Add("warnInfo", warnInfo);//告警描述
                joSend.Add("warnSuggest", warnSuggest);//处理建议
                joSend.Add("epiPos", epiPos);//告警表位

                joSend.Add("receiveTime", receiveTime);//数据上报时间


                string jsonSend = joSend.ToString();
                KafkaClient kafka = new KafkaClient(上报Url);
                bool ret = kafka.SendToKafka(topic, jsonSend, 上报Url);
                if (ret)
                {
                    LogManager.AddMessage(string.Format("13.6---上报数据成功 Url:{0},topic:{1},接口描述:{2},上报数据Json内容:\r\n{3}", 上报Url, topic, topicName, jsonSend), 13);
                }
                else
                {
                }
            }
            catch
            {

            }

        }

        //private DispatcherTimer timer;

        /// <summary>
        /// 13.8  完成
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        /// <param name="frequency"></param>
        public void Updata(string ip, string port, string frequency)
        {
            LogManager.AddMessage(string.Format("默认上报频率：{0}", 上报频率.ToString()));

            Thread thread = new Thread(new ThreadStart(YUNXING138));
            thread.IsBackground = true;
            thread.Start();
        }

        private void YUNXING138()
        {
            while (true)
            {
                string topic = "GK_JD_OUTPUT_INFO_TOPIC";
                string topicName = "设备输出信息上报接口";
                try
                {
                    string machNo = "";//专机编号
                    string devSeatNo = "01"; //
                    string occurTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                    JObject joSend = new JObject();
                    joSend.Add("plantNo", plantNo);//设备编号
                    joSend.Add("machNo", machNo);//专机编号
                    joSend.Add("devSeatNo", devSeatNo);//表位编号
                    joSend.Add("occurTime", occurTime);//上报时间
                    #region 上报数据
                    JArray jaValues = new JArray();

                    JObject joValue = new JObject();
                    joValue.Add("outParaName", "voltageA");//电压
                    joValue.Add("outParaValue", EquipmentData.StdInfo.Ua.ToString());//电压A
                    jaValues.Add(joValue);

                    joValue = new JObject();
                    joValue.Add("outParaName", "voltageB");//电压
                    joValue.Add("outParaValue", EquipmentData.StdInfo.Ub.ToString());//电压B
                    jaValues.Add(joValue);

                    joValue = new JObject();
                    joValue.Add("outParaName", "voltageC");//电压
                    joValue.Add("outParaValue", EquipmentData.StdInfo.Uc.ToString());//电压C
                    jaValues.Add(joValue);

                    joValue = new JObject();
                    joValue.Add("outParaName", "currentA");//电流
                    joValue.Add("outParaValue", Convert.ToDecimal(Decimal.Parse(EquipmentData.StdInfo.Ia.ToString(), System.Globalization.NumberStyles.Float)));//电流A
                    jaValues.Add(joValue);


                    joValue = new JObject();
                    joValue.Add("outParaName", "currentB");//电流
                    joValue.Add("outParaValue", Convert.ToDecimal(Decimal.Parse(EquipmentData.StdInfo.Ib.ToString(), System.Globalization.NumberStyles.Float)));//电流B
                    jaValues.Add(joValue);

                    joValue = new JObject();
                    joValue.Add("outParaName", "currentC");//电流
                    joValue.Add("outParaValue", Convert.ToDecimal(Decimal.Parse(EquipmentData.StdInfo.Ic.ToString(), System.Globalization.NumberStyles.Float)));//电流C
                    jaValues.Add(joValue);

                    joValue = new JObject();
                    joValue.Add("outParaName", "phaseA");//相位
                    joValue.Add("outParaValue", EquipmentData.StdInfo.PhaseA.ToString());//相位
                    jaValues.Add(joValue);


                    joValue = new JObject();
                    joValue.Add("outParaName", "phaseB");//相位
                    joValue.Add("outParaValue", EquipmentData.StdInfo.PhaseB.ToString());//相位
                    jaValues.Add(joValue);

                    joValue = new JObject();
                    joValue.Add("outParaName", "phaseC");//相位
                    joValue.Add("outParaValue", EquipmentData.StdInfo.PhaseC.ToString());//相位
                    jaValues.Add(joValue);

                    joSend.Add("realTimeValues", jaValues);
                    joSend.Add("remarks", "");
                    #endregion
                    string jsonSend = joSend.ToString();

                    using (KafkaClient kafka = new KafkaClient(上报Url))
                    {
                        bool ret = kafka.SendToKafka(topic, jsonSend, 上报Url);
                        if (ret)
                        {
                            LogManager.AddMessage(string.Format("13.9---上报数据成功 Url:{0},topic:{1},接口描述:{2},上报数据Json内容:\r\n{3}", 上报Url, topic, topicName, jsonSend), 13);
                        }
                        else
                        {

                        }
                    }
                }
                catch (Exception ex)
                {
                    LogManager.AddMessage(ex.Message + "13.8-------------");
                }
                Thread.Sleep(上报频率 * 1000);
            }
        }

        /// <summary>
        /// 开机检查 13.9  上传数据不确定
        /// </summary>
        public static void BootCheck()
        {

            string itemNo = "01";
            string itemName = "温度";
            string itemType = "02";
            string machNo = "";//专机编号
            //string devSeatNo = "01";
            string startTime = DateTime.Now.AddMinutes(-1).ToString("yyyy-MM-dd HH:mm:ss");
            string finishTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            string checkConc = "01";

            string warnCode = "01";
            string warnLevel = "02";//01:紧急；02:一般；03:提示
            string plantWarnType = "03";//01;心跳；02; 网络;03; 功能;04; 安全;05; 检定质量 06:故障
            string warnInfo = "升压失败";
            string warnSuggest = "";
            string epiPos = "";
            string receiveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");


            string topic = "GK_JD_BOOT_CHECK_TOPIC";
            string topicName = "开机检查结果上报";

            JObject joSend = new JObject();
            joSend.Add("plantNo", plantNo);//设备编号
            joSend.Add("itemNo", itemNo);//参数编号
            joSend.Add("itemName", itemName);//参数名称
            joSend.Add("itemType", itemType);//参数类型
            joSend.Add("detectSn", "01");//顺序号
            joSend.Add("startTime", startTime);//开始时间
            joSend.Add("finishTime", finishTime);//完成时间
            joSend.Add("checkConc", checkConc);//检查结果
            joSend.Add("checkData", "");//检查数据

            if (checkConc == "02")
            {
                JArray ja_Warn = new JArray();
                JObject joWarn = new JObject();

                joWarn.Add("plantNo", plantNo);//设备编号
                joWarn.Add("machNo", machNo);//上报时间
                joWarn.Add("warnCode", warnCode);//告警代码
                joWarn.Add("startTime", startTime);//当前状态开始时间
                joWarn.Add("warnLevel", warnLevel);//告警等级
                joWarn.Add("plantWarnType", plantWarnType);//告警类型
                joWarn.Add("warnInfo", warnInfo);//告警描述
                joWarn.Add("warnSuggest ", warnSuggest);//处理建议
                joWarn.Add("epiPos", epiPos);//告警表位
                joWarn.Add("receiveTime ", receiveTime);//数据上报时间
                ja_Warn.Add(joWarn);
                joSend.Add("plantWarnList", ja_Warn);//专机告警信息
            }
            else
            {
                joSend.Add("plantWarnList", null);//专机告警信息
            }

            string jsonSend = joSend.ToString();
            using (KafkaClient kafka = new KafkaClient(上报Url))
            {
                bool ret = kafka.SendToKafka(topic, jsonSend, 上报Url);
                if (ret)
                {
                    LogManager.AddMessage(string.Format("13.9---上报数据成功 Url:{0},topic:{1},接口描述:{2},上报数据Json内容:\r\n{3}", 上报Url, topic, topicName, jsonSend), 13);
                }
                else
                {

                }
            }

        }
        #endregion
    }

}
