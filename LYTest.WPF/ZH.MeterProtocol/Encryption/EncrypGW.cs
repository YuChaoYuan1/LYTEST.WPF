using LYTest.DAL.Config;
using LYTest.MeterProtocol.Enum;
using LYTest.MeterProtocol.Struct;
using System;
using System.Diagnostics;
using System.Runtime.ExceptionServices;
using System.Text;

namespace LYTest.MeterProtocol.Encryption
{
    /// <summary>
    /// 功能描述：加密机操作类.
    /// </summary>
    public class EncrypGW
    {
        private static readonly object LockAP = new object();

        /// <summary>
        /// 0:国网电科院服务器，2：国网企业服务器
        /// </summary>
        private static EncryConnMode _SoftType = EncryConnMode.服务器版;

        /// <summary>
        /// 连接状态
        /// </summary>
        private static bool IsLink { set; get; }

        /// <summary>
        /// 操作失败信息
        /// </summary>
        public static string LostMessage { get; protected set; }

        public static bool BC = false;

        private static string IsNull(string str)
        {
            if (str == null)
            {
                return "Null";
            }
            return str;
        }

        /// <summary>
        /// 连接加密机
        /// </summary>
        /// <returns></returns>
        public static bool Link()
        {
            string ip = ConfigHelper.Instance.Dog_IP;
            int port = int.Parse(ConfigHelper.Instance.Dog_Prot);
            string usbKey = ConfigHelper.Instance.Dog_key;
            _SoftType = ConfigHelper.Instance.Dog_ConnectType == "服务器版" ? EncryConnMode.服务器版 : EncryConnMode.直连密码机版;
            LYTest.Utility.Log.LogManager.AddMessage("正在联接加密机...", LYTest.Utility.Log.EnumLogSource.设备操作日志, LYTest.Utility.Log.EnumLevel.Information);
            bool result = false;
            lock (LockAP)
            {
                try
                {
                    for (int i = 0; i < 1; i++)
                    {
                        //获取登录服务器的权限
                        if (_SoftType == EncryConnMode.服务器版)
                        {
                            int r = EncrypAPIGW.OpenUsbkey();
                            if (r == 0) //成功
                            {
                                r = EncrypAPIGW.LgServer(ip, port, usbKey.Length, usbKey);//连接加密机
                                if (r == 0)
                                {
                                    result = true;
                                    break;
                                }

                                LYTest.Utility.Log.LogManager.AddMessage($"第{ i + 1}次，连接加密机操作失败，请检查网络是否正常!\r\n{GetErrMsg(r)}", LYTest.Utility.Log.EnumLogSource.设备操作日志, LYTest.Utility.Log.EnumLevel.Error);
                            }
                            else
                            {
                                LYTest.Utility.Log.LogManager.AddMessage($"第{ i + 1}次，打开UsbKey操作失败：请检查UsbKey是否插入!\r\n{GetErrMsg(r)}", LYTest.Utility.Log.EnumLogSource.设备操作日志, LYTest.Utility.Log.EnumLevel.Error);
                            }
                        }
                        else if (_SoftType == EncryConnMode.直连密码机版)
                        {
                            int r = EncrypAPIGW.ConnectDevice(ip, port.ToString(), "30"); //连接加密机
                            if (r == 0)
                            {
                                result = true;
                                break;
                            }
                            LYTest.Utility.Log.LogManager.AddMessage($"第{ i + 1}次，连接加密机操作失败，请检查网络是否正常!\r\n{GetErrMsg(r)}", LYTest.Utility.Log.EnumLogSource.设备操作日志, LYTest.Utility.Log.EnumLevel.Error);
                        }


                    }
                }
                catch (Exception ex)
                {
                    Utility.Log.LogManager.AddMessage($"连接加密机出错\n\r{ex.Message}", Utility.Log.EnumLogSource.设备操作日志, Utility.Log.EnumLevel.Error);
                }
            }
            //错误消息处理
            if (result)
            {
                IsLink = true;
                result = true;
                Utility.Log.LogManager.AddMessage($"加密机连接成功", Utility.Log.EnumLogSource.设备操作日志, Utility.Log.EnumLevel.Information);
            }
            return result;
        }

        /// <summary>
        /// 断开加密机
        /// </summary>
        /// <returns></returns>
        public static bool UnLink()
        {
            lock (LockAP)
            {

                int r = EncrypAPIGW.LgoutServer();
                if (IsLink & r != 0)
                {
                    LYTest.Utility.Log.LogManager.AddMessage($"断开加密机操作失败！\r\n{GetErrMsg(r)}", LYTest.Utility.Log.EnumLogSource.设备操作日志, LYTest.Utility.Log.EnumLevel.Error);
                    return false;
                }
                else
                {
                    IsLink = false;
                    return true;
                }

            }

        }

        public static bool CheckIsLink()
        {
            int rst = 0;
            int iKeyState = 1;
            int iOperateMode = 1;
            string cESAMID = "0000000000000000";
            string cBrdCstAddr = "C0AA";
            string cAGSEQ = "00000001";
            string cBrdCstData = $"07010040007F001C{DateTime.Now.Year:X4}{DateTime.Now.Month:X2}{DateTime.Now.Day:X2}{DateTime.Now.Hour:X2}{DateTime.Now.Minute:X2}{DateTime.Now.Second:X2}";

            //读取加密机信息
            lock (LockAP)
            {
                StringBuilder sid = new StringBuilder(300);
                StringBuilder attachData = new StringBuilder(100);
                StringBuilder data = new StringBuilder(100);
                StringBuilder mac = new StringBuilder(100);
                rst = EncrypAPIGW.Obj_Meter_Formal_GetGrpBrdCstData(iKeyState, iOperateMode, cESAMID, cBrdCstAddr, cAGSEQ, cBrdCstData, sid, attachData, data, mac);

                string outSID = sid.ToString().Replace("\0", "");
                string outAttachData = attachData.ToString().Replace("\0", "");
                string outData = data.ToString().Replace("\0", "");
                string outMac = mac.ToString().Replace("\0", "");
            }
            return rst == 0;
        }

        ///创建随机数函数
        /// <summary>
        /// 功能描述：用于产生随机数，也可以不调用本函数自己产生随机数,可以不用联机调用
        /// </summary>
        /// <param name="outRand">randLen字节随机数</param>
        /// <returns>0 成功，其他 失败</returns>
        public static bool Create_Rand(int randLen, ref string outRand)
        {
            string str = "01234567890123456789012345678901234567890123456789";
            outRand = str.Substring(0, randLen);
            return true;

        }

        /// <summary>
        /// 远程身份认证，09密钥有效，13密钥有效，698无效
        /// </summary>
        /// <param name="keyState">0:公钥认证，1:私钥认证</param>
        /// <param name="meterNo">表号</param>
        /// <param name="rand">随机数1</param>
        /// <param name="endata">密文1</param>
        /// <returns></returns>
        [HandleProcessCorruptedStateExceptions]
        public static bool IdentityAuthentication(int keyState, string meterNo, ref string rand, ref string endata)
        {
            //分开实现
            StringBuilder outRand = new StringBuilder(16);      //随机数
            StringBuilder outEndata = new StringBuilder(16);    //密文
            int rst = 0;
            rand = "";
            endata = "";


            if (!IsLink) return false;

            //读取加密机认证信息
            lock (LockAP)
            {
                try
                {
                    if (BC) //带有编程键
                    {
                        //09密钥
                        string div = keyState == 1 ? meterNo.PadLeft(16, '0') : "0000000000000001";

                        rst = EncrypAPIGW.IdentityAuthentication(keyState, div, outRand, outEndata, "ZH");
                        rand = outRand.ToString().Replace("\0", "");
                        endata = outEndata.ToString().Replace("\0", "");
                        LostMessage = GetErrMsg(rst);
                    }
                    else
                    {
                        //13密钥
                        string div = meterNo.PadLeft(16, '0');

                        rst = EncrypAPIGW.Meter_Formal_IdentityAuthentication(keyState, div, outRand, outEndata);
                        rand = outRand.ToString().Replace("\0", "");
                        endata = outEndata.ToString().Replace("\0", "");
                        LostMessage = GetErrMsg(rst);
                    }
                }
                catch (System.Exception ex)
                {
                    string error = "加密库调用异常";
                    error += "\r\n" + "表号:" + IsNull(meterNo);
                    error += "\r\n" + ex.ToString();
                    LYTest.Utility.Log.LogManager.AddMessage(error, LYTest.Utility.Log.EnumLogSource.设备操作日志, LYTest.Utility.Log.EnumLevel.Error);
                    return false;
                }

            }
            return rst == 0;
        }

        /// <summary>
        /// 获取跳合闸密文
        /// </summary>
        /// <param name="putRand">8位随机数</param>
        /// <param name="putDiv">16位分散因子，“0000”+表号</param>
        /// <param name="putEsamNo">16Esam序列号，复位信息的后8字节,字符型,长度16</param>
        /// <param name="putData">跳闸或合闸控制命令明文</param>
        /// <param name="outEndata">20字节密文</param>
        /// <returns></returns>
        [HandleProcessCorruptedStateExceptions]
        public static bool UserControl(int flag, string putRand, string putDiv, string putEsamNo, string putData, out string outEndata)
        {
            StringBuilder endata = new StringBuilder(500);                //20字节密文
            outEndata = string.Empty;
            int rst = 0;
            string rand = putRand;
            string div = putDiv.PadLeft(16, '0');
            string esamNo = putEsamNo;
            string data = putData;

            if (!IsLink) return false;

            lock (LockAP)
            {
                string msg = "";
                try
                {
                    if (!BC)
                    {
                        rst = EncrypAPIGW.Meter_Formal_UserControl(flag, rand, div, esamNo, data, endata);
                        outEndata = endata.ToString().Replace("\0", "");
                        msg = GetErrMsg(rst);
                    }
                    else
                    {
                        rst = EncrypAPIGW.UserControl(flag, rand, div, esamNo, data, endata);
                        outEndata = endata.ToString().Replace("\0", "");

                        msg = GetErrMsg(rst);
                    }
                }
                catch (System.Exception ex)
                {
                    string error = "加密库调用异常";
                    error += "\r\n" + "ESAM序列号:" + IsNull(putEsamNo);
                    error += "\r\n" + "随机数:" + IsNull(putRand);
                    error += "\r\n" + "分散因子:" + IsNull(putDiv);
                    error += "\r\n" + "跳闸或合闸控制命令明文:" + IsNull(putData);
                    error += "\r\n" + ex.ToString();
                    LYTest.Utility.Log.LogManager.AddMessage(error, LYTest.Utility.Log.EnumLogSource.设备操作日志, LYTest.Utility.Log.EnumLevel.Error);
                    return false;
                }

            }

            return rst == 0;
        }

        /// <summary>
        /// 清零（先私钥，再公钥）
        /// </summary>
        /// <param name="iState">状态</param>
        /// <param name="meterNo">表号</param>
        /// <param name="cRand">8位随机数</param>
        /// <param name="cEsamNo">16位ESAM序列号,复位信息的后8字节</param>
        /// <param name="str_PutKeyinfo1">远程控制密钥密钥信息明文</param>
        /// <param name="outKey1">64位远程控制密钥密文</param>
        /// <param name="outKeyinfo1">8位远程控制密钥信息</param>
        /// <returns></returns>
        [HandleProcessCorruptedStateExceptions]
        public static bool ClearKeyInfo(int iState, string meterNo, string cRand, string cEsamNo, string cKeyinfo3, out string outKey1, out string outKeyinfo1)
        {
            StringBuilder key2 = new StringBuilder(100);              //输出的密文
            StringBuilder keyinfo2 = new StringBuilder(20);           //输出密钥信息
            outKey1 = string.Empty;
            outKeyinfo1 = string.Empty;
            int rst = 0;
            if (!IsLink) return false;

            string strbd_PutRand = cRand;
            string strbd_PutEsamNo = cEsamNo;
            string strbd_PutKeyinfo3 = cKeyinfo3;

            string div = iState == 1 ? meterNo.PadLeft(16, '0') : "0000000000000001";
            //清零
            lock (LockAP)
            {
                string msg = "";
                try
                {
                    if (!BC)
                    {
                        string div1 = meterNo.PadLeft(16, '0');
                        rst = EncrypAPIGW.ClearKeyInfo(iState, strbd_PutRand, div1, strbd_PutEsamNo, strbd_PutKeyinfo3, key2, keyinfo2);

                        outKey1 = key2.ToString().Replace("\0", "");
                        outKeyinfo1 = keyinfo2.ToString().Replace("\0", "");

                        msg = GetErrMsg(rst);
                    }
                    else
                    {
                        rst = EncrypAPIGW.ClearKeyInfo(iState, strbd_PutRand, div, strbd_PutEsamNo, strbd_PutKeyinfo3, key2, keyinfo2);
                        outKey1 = key2.ToString().Replace("\0", "");
                        outKeyinfo1 = keyinfo2.ToString().Replace("\0", "");

                        msg = GetErrMsg(rst);
                    }
                }
                catch (System.Exception ex)
                {
                    string error = "加密库调用异常";
                    error += "\r\n" + "ESAM序列号:" + IsNull(cEsamNo);
                    error += "\r\n" + "随机数:" + IsNull(cRand);
                    error += "\r\n" + "表号:" + IsNull(meterNo);
                    error += "\r\n" + "密文:" + IsNull(cKeyinfo3);
                    error += "\r\n" + ex.ToString();
                    LYTest.Utility.Log.LogManager.AddMessage(error, LYTest.Utility.Log.EnumLogSource.设备操作日志, LYTest.Utility.Log.EnumLevel.Error);
                    return false;
                }

            }
            return rst == 0;

        }

        /// <summary>
        /// 密钥下装
        /// </summary>
        /// <param name="meterNo">表号</param>
        /// <param name="putRand">16位随机数</param>
        /// <param name="putEsamNo">16位ESAM序列号,复位信息的后8字节</param>
        /// <param name="putKeyinfo1">主控密钥密钥信息明文</param>
        /// <param name="putKeyinfo2">远程密钥密钥信息明文</param>
        /// <param name="putKeyinfo3">二类参数设置密钥信息明文</param>
        /// <param name="putKeyinfo4">远程身份认证密钥信息明文</param>
        /// <param name="key1">主控密钥密钥密文</param>
        /// <param name="keyinfo1">主控密钥信息</param>
        /// <param name="key2">远程控制密钥密文</param>
        /// <param name="keyinfo2">远程控制密钥信息</param>
        /// <param name="key3">二类参数设置密钥密文</param>
        /// <param name="keyinfo3">二类参数设置密钥信息</param>
        /// <param name="key4">远程身份认证密钥密文</param>
        /// <param name="keyinfo4">远程身份认证密钥信息</param>
        /// <returns></returns>
        [HandleProcessCorruptedStateExceptions]
        public static bool KeyUpdate(string meterNo, string putRand, string putEsamNo, string putKeyinfo1, string putKeyinfo2, string putKeyinfo3, string putKeyinfo4,
            out string key1, out string keyinfo1, out string key2, out string keyinfo2, out string key3, out string keyinfo3, out string key4, out string keyinfo4)
        {
            StringBuilder outKey1 = new StringBuilder(64);              //主控密钥密钥密文
            StringBuilder outKeyinfo1 = new StringBuilder(8);           //主控密钥信息
            StringBuilder outKey2 = new StringBuilder(64);              //远程控制密钥密文
            StringBuilder outKeyinfo2 = new StringBuilder(8);           //远程控制密钥信息
            StringBuilder outKey3 = new StringBuilder(64);              //二类参数设置密钥密文
            StringBuilder outKeyinfo3 = new StringBuilder(8);           //二类参数设置密钥信息
            StringBuilder outKey4 = new StringBuilder(64);              //远程身份认证密钥密文
            StringBuilder outKeyinfo4 = new StringBuilder(8);           //远程身份认证密钥信息

            //string strbd_PutRand = putRand;
            //string strbd_PutEsamNo = putEsamNo;
            //string strbd_PutKeyinfo1 = putKeyinfo1;
            //string strbd_PutKeyinfo2 = putKeyinfo2;
            //string strbd_PutKeyinfo3 = putKeyinfo3;
            //string strbd_PutKeyinfo4 = putKeyinfo4;
            int rst = 0;
            if (!IsLink)
            {
                key1 = string.Empty;
                keyinfo1 = string.Empty;
                key2 = string.Empty;
                keyinfo2 = string.Empty;
                key3 = string.Empty;
                keyinfo3 = string.Empty;
                key4 = string.Empty;
                keyinfo4 = string.Empty;
                return false;
            }
            //读取加密机信息
            try
            {
                lock (LockAP)
                {
                    string div = meterNo.PadLeft(16, '0');

                    rst = EncrypAPIGW.KeyUpdate(0, putRand, div, putEsamNo, putKeyinfo1, putKeyinfo2, putKeyinfo3,
                        putKeyinfo4, outKey1, outKeyinfo1, outKey2, outKeyinfo2, outKey3, outKeyinfo3, outKey4, outKeyinfo4);
                }
            }
            catch (System.Exception ex)
            {
                string error = "加密库调用异常";
                error += "\r\n" + "ESAM序列号:" + IsNull(putEsamNo);
                error += "\r\n" + "随机数:" + IsNull(putRand);
                error += "\r\n" + "表号:" + IsNull(meterNo);
                error += "\r\n" + "putKeyinfo1:" + IsNull(putKeyinfo1);
                error += "\r\n" + "putKeyinfo2:" + IsNull(putKeyinfo2);
                error += "\r\n" + "putKeyinfo3:" + IsNull(putKeyinfo3);
                error += "\r\n" + "putKeyinfo4:" + IsNull(putKeyinfo4);
                error += "\r\n" + ex.ToString();
                LYTest.Utility.Log.LogManager.AddMessage(error, LYTest.Utility.Log.EnumLogSource.设备操作日志, LYTest.Utility.Log.EnumLevel.Error);
                rst = 1;
            }
            key1 = outKey1.ToString().Replace("\0", "");                //主控密钥密钥密文
            keyinfo1 = outKeyinfo1.ToString().Replace("\0", "");        //主控密钥信息
            key2 = outKey2.ToString().Replace("\0", "");                //远程控制密钥密文
            keyinfo2 = outKeyinfo2.ToString().Replace("\0", "");        //远程控制密钥信息
            key3 = outKey3.ToString().Replace("\0", "");                //二类参数设置密钥密文
            keyinfo3 = outKeyinfo3.ToString().Replace("\0", "");        //二类参数设置密钥信息
            key4 = outKey4.ToString().Replace("\0", "");                //远程身份认证密钥密文
            keyinfo4 = outKeyinfo4.ToString().Replace("\0", "");        //远程身份认证密钥信息
            return rst == 0;
        }
        [HandleProcessCorruptedStateExceptions]
        public static bool KeyUpdate(ref StKeyUpdateInfo keyUpdateInfo)
        {
            int rst = 0;

            StringBuilder strb_OutKey1 = new StringBuilder(64);              //主控密钥密钥密文
            StringBuilder strb_OutKeyinfo1 = new StringBuilder(8);           //主控密钥信息
            StringBuilder strb_OutKey2 = new StringBuilder(64);              //远程控制密钥密文
            StringBuilder strb_OutKeyinfo2 = new StringBuilder(8);           //远程控制密钥信息
            StringBuilder strb_OutKey3 = new StringBuilder(64);              //二类参数设置密钥密文
            StringBuilder strb_OutKeyinfo3 = new StringBuilder(8);           //二类参数设置密钥信息
            StringBuilder strb_OutKey4 = new StringBuilder(64);              //远程身份认证密钥密文
            StringBuilder strb_OutKeyinfo4 = new StringBuilder(8);           //远程身份认证密钥信息


            if (!IsLink)
            {
                keyUpdateInfo.主控密钥密文 = string.Empty;
                keyUpdateInfo.主控密钥信息 = string.Empty;
                keyUpdateInfo.远程密钥密文 = string.Empty;
                keyUpdateInfo.远程密钥信息 = string.Empty;
                keyUpdateInfo.参数密钥密文 = string.Empty;
                keyUpdateInfo.参数密钥信息 = string.Empty;
                keyUpdateInfo.身份密钥密文 = string.Empty;
                keyUpdateInfo.身份密钥信息 = string.Empty;
                return false;
            }

            //读取加密机信息
            lock (LockAP)
            {

                try
                {
                    rst = EncrypAPIGW.KeyUpdate(0, keyUpdateInfo.MeterRand, keyUpdateInfo.MeterDiv, keyUpdateInfo.MeterEsamNo, keyUpdateInfo.主控密钥明文, keyUpdateInfo.远程密钥明文, keyUpdateInfo.参数密钥明文,
                     keyUpdateInfo.身份密钥明文, strb_OutKey1, strb_OutKeyinfo1, strb_OutKey2, strb_OutKeyinfo2, strb_OutKey3, strb_OutKeyinfo3, strb_OutKey4, strb_OutKeyinfo4);
                }
                catch (System.Exception ex)
                {
                    string error = "加密库调用异常";
                    error += "\r\n" + ex.ToString();
                    LYTest.Utility.Log.LogManager.AddMessage(error, LYTest.Utility.Log.EnumLogSource.设备操作日志, LYTest.Utility.Log.EnumLevel.Error);
                    return false;
                }
                keyUpdateInfo.主控密钥密文 = strb_OutKey1.ToString().Replace("\0", "");                        //主控密钥密钥密文
                keyUpdateInfo.主控密钥信息 = strb_OutKeyinfo1.ToString().Replace("\0", "");                    //主控密钥信息
                keyUpdateInfo.远程密钥密文 = strb_OutKey2.ToString().Replace("\0", "");                        //远程控制密钥密文
                keyUpdateInfo.远程密钥信息 = strb_OutKeyinfo2.ToString().Replace("\0", "");                    //远程控制密钥信息
                keyUpdateInfo.参数密钥密文 = strb_OutKey3.ToString().Replace("\0", "");                        //二类参数设置密钥密文
                keyUpdateInfo.参数密钥信息 = strb_OutKeyinfo3.ToString().Replace("\0", "");                    //二类参数设置密钥信息
                keyUpdateInfo.身份密钥密文 = strb_OutKey4.ToString().Replace("\0", "");                        //远程身份认证密钥密文
                keyUpdateInfo.身份密钥信息 = strb_OutKeyinfo4.ToString().Replace("\0", "");                    //远程身份认证密钥信息
            }

            keyUpdateInfo.GetKeyInfoSucc = rst == 0;
            return rst == 0;
        }

        ///3.3.12. 密钥更新函数
        /// <summary>
        /// 功能描述：用于电能表远程密钥更新，2013年标准电能表密钥更新本地表和远程表都采用通信方式完成，共20条密钥，分5次调用本函数，所得密钥分5次下发给电能表
        /// </summary>
        /// <param name="PutKeySum">密钥总条数，固定为20</param>
        /// <param name="PutKeystate">密钥状态，“00”，密钥恢复（需特殊授权），“01”密钥下装</param>
        /// <param name="PutKeyid">密钥编号，0x00-0x13，每次最多输出4条密钥“00010203”</param>
        /// <param name="PutRand">随机数2，电表身份认证成功返回，4字节</param>
        /// <param name="PutDiv">8字节分散因子，“0000”+表号</param>
        /// <param name="PutEsamNo">8字节ESAM序列号</param>
        /// <param name="PutChiplnfor">芯片发行信息文件数据，从电表ESAM抄读得到，005AH字节</param>
        /// <param name="OutData">出参，4*（4字节密钥信息+32字节密钥密文）+4字节MAC</param>
        /// <returns>0成功，其他失败</returns>
        [HandleProcessCorruptedStateExceptions]
        public static bool Meter_Formal_KeyUpdateV2(int PutKeySum, string PutKeystate, string PutKeyid, string PutRand, string PutDiv, string PutEsamNo, string PutChiplnfor, ref string OutData, ref string msg)
        {
            int rst = 0;
            OutData = "";
            if (!IsLink) return false;

            //读取加密机信息
            lock (LockAP)
            {
                try
                {
                    string str_m_Div = PutDiv.PadLeft(16, '0');
                    StringBuilder strOutData = new StringBuilder(500);
                    rst = EncrypAPIGW.Meter_Formal_KeyUpdateV2(PutKeySum, PutKeystate, PutKeyid, PutRand, str_m_Div, PutEsamNo, PutChiplnfor, strOutData);
                    OutData = strOutData.ToString().Replace("\0", "");
                    msg = GetErrMsg(rst);
                }
                catch (System.Exception ex)
                {
                    string error = "加密库调用异常";
                    error += "\r\n" + "密钥状态:" + IsNull(PutKeystate);
                    error += "\r\n" + "密钥编号:" + IsNull(PutKeyid);
                    error += "\r\n" + "随机数2:" + IsNull(PutRand);
                    error += "\r\n" + "分散因子:" + IsNull(PutDiv);
                    error += "\r\n" + "ESAM序列号:" + IsNull(PutEsamNo);
                    error += "\r\n" + "芯片发行信息文件数据:" + IsNull(PutChiplnfor);
                    error += "\r\n" + ex.ToString();
                    LYTest.Utility.Log.LogManager.AddMessage(error, LYTest.Utility.Log.EnumLogSource.设备操作日志, LYTest.Utility.Log.EnumLevel.Error);
                    return false;
                }
            }
            return rst == 0;
        }

        ///3.2.3. 一类参数 MAC 计算函数
        /// <summary>
        /// 功能描述：用于电能表一类参数MAC计算函数
        /// </summary>
        /// <param name="Flag">整形，0公钥状态，1私钥状态（需要特殊授权）</param>
        /// <param name="PutRand">输入的随机数2，字符型，长度8</param>
        /// <param name="PutDiv">输入的分散因子，字符型，长度16，“0000”+表号</param>
        /// <param name="PutApdu">一类参数设置的写ESAM命令头，字符型，长度10</param>
        /// <param name="PutData">输入的一类参数明文，字符型</param>
        /// <param name="OutEndata">输出的MAC数据，字符型，长度8</param>
        /// <returns>0 成功，其他 失败</returns>
        [HandleProcessCorruptedStateExceptions]
        public static bool ParameterUpDate(int Flag, string PutRand, string PutDiv, string PutApdu, string PutData, ref string OutEndata, ref string msg)
        {
            int rst = 0;
            if (!IsLink) return false;

            lock (LockAP)
            {

                try
                {
                    string str_m_Div = PutDiv.PadLeft(16, '0');
                    if (!BC)
                    {
                        StringBuilder strOutEndata = new StringBuilder(500);

                        rst = EncrypAPIGW.Meter_Formal_ParameterUpdate(Flag, PutRand, str_m_Div, PutApdu, PutData, strOutEndata);
                        OutEndata = strOutEndata.ToString().Replace("\0", "");
                        msg = GetErrMsg(rst);
                    }
                    else
                    {
                        StringBuilder strOutEndata = new StringBuilder(200);
                        int result = EncrypAPIGW.ParameterUpDate(Flag, PutRand, str_m_Div, PutApdu, PutData, strOutEndata);

                        OutEndata = strOutEndata.ToString().Replace("\0", "");

                        msg = GetErrMsg(result);
                    }
                }
                catch (System.Exception ex)
                {
                    string error = "加密库调用异常";
                    error += "\r\n" + "输入的随机数2:" + IsNull(PutRand);
                    error += "\r\n" + "输入的分散因子:" + IsNull(PutDiv);
                    error += "\r\n" + "输入的指令数据:" + IsNull(PutApdu);
                    error += "\r\n" + "输入的一类参数明文:" + IsNull(PutData);
                    error += "\r\n" + ex.ToString();
                    LYTest.Utility.Log.LogManager.AddMessage(error, LYTest.Utility.Log.EnumLogSource.设备操作日志, LYTest.Utility.Log.EnumLevel.Error);
                    return false;
                }

            }
            return rst == 0;
        }

        ///3.2.4. 二类参数加密函数
        /// <summary>
        /// 功能描述：用于远程二类参数设置加密
        /// </summary>
        /// <param name="Flag">整形，0公钥状态，1私钥状态（<font color='red'>需要特殊授权</font>）</param>
        /// <param name="PutRand">输入的随机数2，字符型，长度8</param>
        /// <param name="PutDiv">输入的分散因子，字符型，长度16，“0000”+表号</param>
        /// <param name="PutApdu">输入的指令数据，字符型，长度10</param>
        /// <param name="PutData">输入的二类参数明文，字符型</param>
        /// <param name="OutEndata">输出的密文，字符型</param>
        /// <returns>0成功，其他失败</returns>
        [HandleProcessCorruptedStateExceptions]
        public static bool ParameterElseUpDate(int Flag, string PutRand, string PutDiv, string PutApdu, string PutData, ref string OutEndata, ref string msg)
        {
            int rst = 0;
            if (!IsLink) return false;

            lock (LockAP)
            {

                try
                {
                    string str_m_Div = PutDiv.PadLeft(16, '0');
                    if (!BC)
                    {
                        StringBuilder strOutEndata = new StringBuilder(400);
                        rst = EncrypAPIGW.Meter_Formal_ParameterElseUpdate(Flag, PutRand, str_m_Div, PutApdu, PutData, strOutEndata);
                        OutEndata = strOutEndata.ToString().Replace("\0", "");
                        msg = GetErrMsg(rst);
                    }
                    else
                    {
                        StringBuilder strOutEndata = new StringBuilder(400);
                        rst = EncrypAPIGW.ParameterElseUpdate(Flag, PutRand, str_m_Div, PutApdu, PutData, strOutEndata);
                        OutEndata = strOutEndata.ToString().Replace("\0", "");

                        msg = GetErrMsg(rst);
                    }
                }
                catch (System.Exception ex)
                {
                    string error = "加密库调用异常";
                    error += "\r\n" + "输入的随机数2:" + IsNull(PutRand);
                    error += "\r\n" + "输入的分散因子:" + IsNull(PutDiv);
                    error += "\r\n" + "输入的指令数据:" + IsNull(PutApdu);
                    error += "\r\n" + "输入的二类参数明文:" + IsNull(PutData);
                    error += "\r\n" + ex.ToString();
                    LYTest.Utility.Log.LogManager.AddMessage(error, LYTest.Utility.Log.EnumLogSource.设备操作日志, LYTest.Utility.Log.EnumLevel.Error);
                    return false;
                }
            }
            return rst == 0;
        }
        [HandleProcessCorruptedStateExceptions]
        public static bool ParameterElseUpDateGPS(int Flag, string PutRand, string PutDiv, string PutApdu, string PutData, ref string OutEndata, ref string msg)
        {
            int rst = 0;
            if (!IsLink) return false;

            lock (LockAP)
            {

                try
                {
                    string str_m_Div = PutDiv.PadLeft(16, '0');
                    DateTime newTime = DateTime.Now;
                    int weekday = (int)newTime.DayOfWeek;
                    PutData = "0400010C" + newTime.ToString("yyMMdd") + "0" + weekday + newTime.ToString("HHmmss");

                    if (!BC)
                    {
                        StringBuilder strOutEndata = new StringBuilder(400);
                        rst = EncrypAPIGW.Meter_Formal_ParameterElseUpdate(Flag, PutRand, str_m_Div, PutApdu, PutData, strOutEndata);
                        OutEndata = strOutEndata.ToString().Replace("\0", "");
                        msg = GetErrMsg(rst);
                    }
                    else
                    {
                        StringBuilder strOutEndata = new StringBuilder(400);
                        rst = EncrypAPIGW.ParameterElseUpdate(Flag, PutRand, str_m_Div, PutApdu, PutData, strOutEndata);
                        OutEndata = strOutEndata.ToString().Replace("\0", "");

                        msg = GetErrMsg(rst);
                    }
                }
                catch (System.Exception ex)
                {
                    string error = "加密库调用异常";
                    error += "\r\n" + "输入的随机数2:" + IsNull(PutRand);
                    error += "\r\n" + "输入的分散因子:" + IsNull(PutDiv);
                    error += "\r\n" + "输入的指令数据:" + IsNull(PutApdu);
                    error += "\r\n" + "输入的二类参数明文:" + IsNull(PutData);
                    error += "\r\n" + ex.ToString();
                    LYTest.Utility.Log.LogManager.AddMessage(error, LYTest.Utility.Log.EnumLogSource.设备操作日志, LYTest.Utility.Log.EnumLevel.Error);
                    return false;
                }
            }
            return rst == 0;
        }

        ///3.2.5. 第一套费率电价设置函数
        /// <summary>
        /// 功能描述：用于第一套费率参数MAC计算
        /// </summary>
        /// <param name="Flag">整形，0公钥状态，1私钥状态（需要特殊授权）</param>
        /// <param name="PutRand">输入的随机数2，字符型，长度8</param>
        /// <param name="PutDiv">输入的分散因子，字符型，长度16，“0000”+表号</param>
        /// <param name="PutApdu">输入的指令数据，字符型，长度10</param>
        /// <param name="PutData">输入的第一套费率参数明文，字符型</param>
        /// <param name="OutEndata">输出的明文和MAC，字符型</param>
        /// <returns>0 成功，其他 失败</returns>
        [HandleProcessCorruptedStateExceptions]
        public static bool ParameterUpDate1(int Flag, string PutRand, string PutDiv, string PutApdu, string PutData, ref string OutEndata, ref string msg)
        {
            int rst = 0;
            if (!IsLink) return false;

            lock (LockAP)
            {
                try
                {
                    string str_m_Div = PutDiv.PadLeft(16, '0');
                    if (!BC)
                    {
                        StringBuilder strOutEndata = new StringBuilder(400);

                        rst = EncrypAPIGW.Meter_Formal_ParameterUpdate1(Flag, PutRand, str_m_Div, PutApdu, PutData, strOutEndata);

                        OutEndata = strOutEndata.ToString().Replace("\0", "");
                        msg = GetErrMsg(rst);
                    }
                    else
                    {
                        StringBuilder strOutEndata = new StringBuilder(400);
                        rst = EncrypAPIGW.ParameterUpDate1(Flag, PutRand, str_m_Div, PutApdu, PutData, strOutEndata);
                        OutEndata = strOutEndata.ToString().Replace("\0", "");
                        msg = GetErrMsg(rst);
                    }
                }
                catch (System.Exception ex)
                {
                    string error = "加密库调用异常";
                    error += "\r\n" + "输入的随机数2:" + IsNull(PutRand);
                    error += "\r\n" + "输入的分散因子:" + IsNull(PutDiv);
                    error += "\r\n" + "输入的指令数据:" + IsNull(PutApdu);
                    error += "\r\n" + "输入的第一套费率参数明文:" + IsNull(PutData);
                    error += "\r\n" + ex.ToString();
                    LYTest.Utility.Log.LogManager.AddMessage(error, LYTest.Utility.Log.EnumLogSource.设备操作日志, LYTest.Utility.Log.EnumLevel.Error);
                    return false;
                }
            }
            return rst == 0;
        }

        ///3.2.6. 第二套费率电价设置函数
        /// <summary>
        /// 功能描述：该函数用于2009版规范中的第而套费率电价设置，也用于2013版规范中的备用套电价参数MAC计算
        /// </summary>
        /// <param name="Flag">整形，0公钥状态，1私钥状态（需要特殊授权）</param>
        /// <param name="PutRand">输入的随机数2，字符型，长度8</param>
        /// <param name="PutDiv">输入的分散因子，字符型，长度16，“0000”+表号</param>
        /// <param name="PutApdu">输入的指令数据，字符型，长度10</param>
        /// <param name="PutData">输入的第二套费率参数或者当前套电价参数明文，字符型</param>
        /// <param name="OutEndata">输出的明文和MAC，字符型</param>
        /// <returns>0 成功，其他 失败</returns>
        [HandleProcessCorruptedStateExceptions]
        public static bool ParameterUpDate2(int Flag, string PutRand, string PutDiv, string PutApdu, string PutData, ref string OutEndata, ref string msg)
        {
            int rst = 0;
            if (!IsLink) return false;

            lock (LockAP)
            {
                try
                {
                    string div = PutDiv.PadLeft(16, '0');
                    if (!BC)
                    {
                        StringBuilder endata = new StringBuilder(400);

                        rst = EncrypAPIGW.Meter_Formal_ParameterUpdate2(Flag, PutRand, div, PutApdu, PutData, endata);

                        OutEndata = endata.ToString().Replace("\0", "");
                        msg = GetErrMsg(rst);
                    }
                    else
                    {
                        StringBuilder strOutEndata = new StringBuilder(400);
                        rst = EncrypAPIGW.ParameterUpDate2(Flag, PutRand, div, PutApdu, PutData, strOutEndata);

                        msg = GetErrMsg(rst);
                    }
                }
                catch (System.Exception ex)
                {
                    string error = "加密库调用异常";
                    error += "\r\n" + "输入的随机数2:" + IsNull(PutRand);
                    error += "\r\n" + "输入的分散因子:" + IsNull(PutDiv);
                    error += "\r\n" + "输入的指令数据:" + IsNull(PutApdu);
                    error += "\r\n" + "输入的第二套费率参数或者当前套电价参数明文:" + IsNull(PutData);
                    error += "\r\n" + ex.ToString();
                    LYTest.Utility.Log.LogManager.AddMessage(error, LYTest.Utility.Log.EnumLogSource.设备操作日志, LYTest.Utility.Log.EnumLevel.Error);
                    return false;
                }


            }
            return rst == 0;
        }

        ///3.3.7. 钱包初始化
        /// <summary>
        /// 功能描述：用于本地费控电能表钱包数据MAC计算
        /// </summary>
        /// <param name="Flag">整型，0公钥状态</param>
        /// <param name="PutRand">随机数2，电表身份认证成功返回，4字节</param>
        /// <param name="PutDiv">分散因子，8字节，“0000”+表号</param>
        /// <param name="PutData">输入的数据明文，包含预置金额</param>
        /// <param name="OutData">输出的数据，预置金额+MAC1+"00000000"+MAC2</param>
        /// <returns>0 成功，其他 失败</returns>
        [HandleProcessCorruptedStateExceptions]
        public static bool Meter_Formal_InintPurse(int Flag, string PutRand, string PutDiv, string PutData, ref string OutData, ref string msg)
        {
            int rst = 0;

            if (!IsLink) return false;

            //读取加密机信息
            lock (LockAP)
            {
                string str_m_Div = PutDiv.PadLeft(16, '0');
                StringBuilder strOutEndata = new StringBuilder(400);
                try
                {
                    rst = EncrypAPIGW.Meter_Formal_InintPurse(Flag, PutRand, str_m_Div, PutData, strOutEndata);
                    OutData = strOutEndata.ToString().Replace("\0", "");
                    msg = GetErrMsg(rst);
                }
                catch (System.Exception ex)
                {
                    string error = "加密库调用异常";
                    error += "\r\n" + "随机数2:" + IsNull(PutRand);
                    error += "\r\n" + "分散因子:" + IsNull(PutDiv);
                    error += "\r\n" + "输入的数据明文:" + IsNull(PutData);
                    error += "\r\n" + ex.ToString();
                    LYTest.Utility.Log.LogManager.AddMessage(error, LYTest.Utility.Log.EnumLogSource.设备操作日志, LYTest.Utility.Log.EnumLevel.Error);
                    return false;
                }
            }
            return rst == 0;
        }

        ///3.3.8. 电表清零函数
        /// <summary>
        /// 功能描述:用于远程费控电能表清零
        /// </summary>
        /// <param name="Flag">整形，0公钥状态，1私钥状态（需要特殊授权）</param>
        /// <param name="PutRand">随机数2，电表身份认证成功返回，4字节</param>
        /// <param name="PutDiv">分散因子，8字节，“0000”+表号</param>
        /// <param name="PutData">入参，清零数据</param>
        /// <param name="OutData">20字节密文</param>
        /// <returns>0 成功，其他 失败</returns>
        [HandleProcessCorruptedStateExceptions]
        public static bool Meter_Formal_DataClear1(int Flag, string PutRand, string PutDiv, string PutData, ref string OutData, ref string msg)
        {
            int rst = 0;

            if (!IsLink) return false;

            //读取加密机信息
            lock (LockAP)
            {
                string str_m_Div = PutDiv.PadLeft(16, '0');
                StringBuilder strOutEndata = new StringBuilder(400);

                try
                {
                    rst = EncrypAPIGW.Meter_Formal_DataClear1(Flag, PutRand, str_m_Div, PutData, strOutEndata);
                    OutData = strOutEndata.ToString().Replace("\0", "");
                    msg = GetErrMsg(rst);
                }
                catch (System.Exception ex)
                {
                    string error = "加密库调用异常";
                    error += "\r\n" + "随机数2:" + IsNull(PutRand);
                    error += "\r\n" + "分散因子:" + IsNull(PutDiv);
                    error += "\r\n" + "入参:" + IsNull(PutData);
                    error += "\r\n" + ex.ToString();
                    LYTest.Utility.Log.LogManager.AddMessage(error, LYTest.Utility.Log.EnumLogSource.设备操作日志, LYTest.Utility.Log.EnumLevel.Error);
                    return false;
                }
            }
            return rst == 0;
        }

        ///3.3.9. 事件或需量清零函数
        /// <summary>
        /// 功能描述:用于电能表事件或者需量清零
        /// </summary>
        /// <param name="Flag">整形，0公钥状态，1私钥状态（需要特殊授权）</param>
        /// <param name="PutRand">随机数2，电表身份认证成功返回，4字节</param>
        /// <param name="PutDiv">分散因子，8字节，“0000”+表号</param>
        /// <param name="PutData">入参，清零数据</param>
        /// <param name="OutData">20字节密文</param>
        /// <returns>0 成功，其他 失败</returns>
        [HandleProcessCorruptedStateExceptions]
        public static bool Meter_Formal_DataClear2(int Flag, string PutRand, string PutDiv, string PutData, ref string OutData, ref string msg)
        {
            int rst = 0;

            if (!IsLink) return false;

            //读取加密机信息
            lock (LockAP)
            {
                string str_m_Div = PutDiv.PadLeft(16, '0');
                StringBuilder strOutEndata = new StringBuilder(400);
                try
                {
                    rst = EncrypAPIGW.Meter_Formal_DataClear2(Flag, PutRand, str_m_Div, PutData, strOutEndata);
                    OutData = strOutEndata.ToString().Replace("\0", "");
                    msg = GetErrMsg(rst);
                }
                catch (System.Exception ex)
                {
                    string error = "加密库调用异常";
                    error += "\r\n" + "随机数2:" + IsNull(PutRand);
                    error += "\r\n" + "分散因子:" + IsNull(PutDiv);
                    error += "\r\n" + "入参:" + IsNull(PutData);
                    error += "\r\n" + ex.ToString();
                    LYTest.Utility.Log.LogManager.AddMessage(error, LYTest.Utility.Log.EnumLogSource.设备操作日志, LYTest.Utility.Log.EnumLevel.Error);
                    return false;
                }

            }
            return rst == 0;
        }

        ///3.3.10. 红外认证函数
        /// <summary>
        /// 功能描述：用于获取红外认证密文和随机数2，红外认证前先进行红外查询
        /// </summary>
        /// <param name="Flag">0公钥状态</param>
        /// <param name="PudDiv">分散因子，8字节，“0000”+表号</param>
        /// <param name="PutEsamNo">8字节ESAM序列号，电能表红外查询命令返回</param>
        /// <param name="PutRand1">8字节随机数1，创建随机数函数返回</param>
        /// <param name="InRand1Endata">8字节随机数1密文，电能表红外查询命令返回</param>
        /// <param name="PutRand2">8字节随机数2，电能表红外查询命令返回</param>
        /// <param name="OutRand2Endata">返回8字节随机数2密文</param>
        /// <returns>0 成功，其他 失败</returns>
        public static bool Meter_Formal_InfraredAuth(int Flag, string PutDiv, string PutEsamNo, string PutRand1, string PutRand1Endata, string PutRand2, ref string OutRand2Endata, ref string msg)
        {
            int rst = 0;

            if (!IsLink) return false;

            //读取加密机信息
            lock (LockAP)
            {
                string div = PutDiv.PadLeft(16, '0');
                StringBuilder rand2Endata = new StringBuilder(400);
                rst = EncrypAPIGW.Meter_Formal_InfraredAuth(Flag, div, PutEsamNo, PutRand1, PutRand1Endata, PutRand2, rand2Endata);
                OutRand2Endata = rand2Endata.ToString().Replace("\0", "");
                msg = GetErrMsg(rst);
            }
            return rst == 0;
        }

        ///3.3.11. 数据回抄
        /// <summary>
        /// 功能描述:用于数据回抄MAC校验
        /// </summary>
        /// <param name="flag">0公钥状态</param>
        /// <param name="cRand">随机数1的高4字节</param>
        /// <param name="cDiv">分散因子，8字节，“0000”+表号</param>
        /// <param name="cApdu">命令头，5字节（04d686+起始地址+LEN，LEN为数据长度+0x0C）</param>
        /// <param name="cData">数据回抄返回的数据</param>
        /// <param name="cMac">4字节数据回抄返回的MAC</param>
        /// <returns>0成功，其他失败</returns>
        [HandleProcessCorruptedStateExceptions]
        public static bool Meter_Formal_MacCheck(int flag, string cRand, string cDiv, string cApdu, string cData, string cMac, ref string msg)
        {
            int rst = 0;

            if (!IsLink) return false;

            //读取加密机信息
            lock (LockAP)
            {
                string div = cDiv.PadLeft(16, '0');

                try
                {
                    rst = EncrypAPIGW.Meter_Formal_MacCheck(flag, cRand, div, cApdu, cData, cMac);
                    msg = GetErrMsg(rst);
                }
                catch (System.Exception ex)
                {
                    string error = "加密库调用异常";
                    error += "\r\n" + "分散因子:" + IsNull(cDiv);
                    error += "\r\n" + "随机数1:" + IsNull(cRand);
                    error += "\r\n" + "命令头:" + IsNull(cApdu);
                    error += "\r\n" + "数据回抄返回的数据:" + IsNull(cData);
                    error += "\r\n" + ex.ToString();
                    LYTest.Utility.Log.LogManager.AddMessage(error, LYTest.Utility.Log.EnumLogSource.设备操作日志, LYTest.Utility.Log.EnumLevel.Error);
                    return false;
                }

            }
            return rst == 0;
        }

        /// <summary>
        ///6.5.1.  主站会话协商  对称密码连接认证机制， 用于主站与设备进行会话协商时产生密文 1 和客户机签名 1，该过程在建立应用连接时完成。
        /// </summary>
        /// <param name="iKeyState"></param>
        /// <param name="cDiv"> 分散因子（ 8Byte）， iKeyState=0， cDiv 为芯片序列号；iKeyState=1， cDiv 为表号；</param>
        /// <param name="cASCTR"></param>
        /// <param name="cFLG">应用密钥产生标识， 1Byte，默认”01”;</param>
        /// <param name="cOutRandHost">主站随机数（ 16Byte）</param>
        /// <param name="cOutSessionInit">会话协商数据，建立应用连接中的密文 1；</param>
        /// <param name="cOutSign">协商数据签名(4Byte)，建立应用连接中的客户机签名 1；</param>
        /// <returns>返回 0 =成功 1= 失败</returns>

        [HandleProcessCorruptedStateExceptions]
        public static bool Obj_Meter_Formal_InitSession(int iKeyState, string cDiv, string cASCTR, string cFLG, out string outRandHost, out string outSessionInit, out string outSign)
        {
            int rst = 0;

            outRandHost = "";
            outSessionInit = "";
            outSign = "";
            if (!IsLink) return false;

            //读取加密机信息
            lock (LockAP)
            {
                string div = cDiv.PadLeft(16, '0');
                StringBuilder randHost = new StringBuilder(32);
                StringBuilder sessionInit = new StringBuilder(64);
                StringBuilder sign = new StringBuilder(8);
                try
                {
                    rst = EncrypAPIGW.Obj_Meter_Formal_InitSession(iKeyState, div, cASCTR.PadLeft(8, '0'), cFLG, randHost, sessionInit, sign);
                    outRandHost = randHost.ToString().Replace("\0", "");
                    outSessionInit = sessionInit.ToString().Replace("\0", "");
                    outSign = sign.ToString().Replace("\0", "");
                    string msg = GetErrMsg(rst);
                    if (rst != 0)
                    {
                        LYTest.Utility.Log.LogManager.AddMessage("【获取应用连接密文失败，请检查,错误信息为】" + msg, LYTest.Utility.Log.EnumLogSource.设备操作日志, LYTest.Utility.Log.EnumLevel.Error);
                    }
                }
                catch (System.Exception ex)
                {
                    string error = "加密库调用异常";
                    error += "\r\n" + "分散因子:" + IsNull(cDiv);
                    error += "\r\n" + "cASCTR:" + IsNull(cASCTR);
                    error += "\r\n" + "密钥标识:" + IsNull(cFLG);
                    error += "\r\n" + ex.ToString();
                    LYTest.Utility.Log.LogManager.AddMessage(error, LYTest.Utility.Log.EnumLogSource.设备操作日志, LYTest.Utility.Log.EnumLevel.Error);
                    return false;
                }

            }
            return rst == 0;
        }

        /// <summary>
        ///6.5.2.  主站会话协商验证
        /// </summary>   对称密码连接认证机制，用于主站验证设备会话协商时返回的数据，验证成功主站产生会话密钥。
        /// <param name="iKeyState"></param>
        /// <param name="cDiv">分散因子（ 8Byte）， iKeyState=0， cDiv 为芯片序列号；iKeyState=1， cDiv 为表号，表号不足 8 字节前补 0；</param>
        /// <param name="cRandHost">主站随机数 R1（ 16Byte）</param>
        /// <param name="cSessionData">终端返回的会话协商数据(48Byte)，建立应用连接中的密文 2；</param>
        /// <param name="cSign">终端返回的会话协商数据签名(4Byte) ，建立应用连接中的客户机签名 2；</param>
        /// <param name="cOutSessionKey">会话密钥</param>
        /// <returns></returns>
        [HandleProcessCorruptedStateExceptions]
        public static bool Obj_Meter_Formal_VerifySession(int iKeyState, string cDiv, string cRandHost, string cSessionData, string cSign, out string outSessionKey)
        {

            outSessionKey = "";
            int rst = 0;
            if (!IsLink) return false;

            //读取加密机信息
            lock (LockAP)
            {
                string div = cDiv.PadLeft(16, '0');
                StringBuilder sessionKey = new StringBuilder(352);

                try
                {
                    rst = EncrypAPIGW.Obj_Meter_Formal_VerifySession(iKeyState, div, cRandHost, cSessionData, cSign, sessionKey);
                    outSessionKey = sessionKey.ToString().Replace("\0", "");
                }
                catch (System.Exception ex)
                {
                    string error = "加密库调用异常";
                    error += "\r\n" + "分散因子:" + IsNull(cDiv);
                    error += "\r\n" + "会话密钥:" + IsNull(cSessionData);
                    error += "\r\n" + "主站随机数R1:" + IsNull(cRandHost);
                    error += "\r\n" + "数据签名:" + IsNull(cSign);
                    error += "\r\n" + ex.ToString();
                    LYTest.Utility.Log.LogManager.AddMessage(error, LYTest.Utility.Log.EnumLogSource.设备操作日志, LYTest.Utility.Log.EnumLevel.Error);
                    return false;
                }
            }
            return rst == 0;
        }

        /// <summary>
        /// 6.5.3.  抄读数据验证
        /// </summary> 主站验证设备返回的抄读数据，具体指抄读电能表返回的数据。
        /// <param name="iKeyState"></param>
        /// <param name="iOperateMode"></param>
        /// <param name="cMeterNo"></param>
        /// <param name="cRandHost">主站随机数(16Byte</param>
        /// <param name="cReadData">抄读数据</param>
        /// <param name="cMac">数据</param>
        /// <param name="cOutData">明文抄读数据， iOperateMode=1，为空</param>
        /// <returns></returns>
        public static bool Obj_Meter_Formal_VerifyReadData(int iKeyState, string iOperateMode, string cMeterNo, string cRandHost, string cReadData, string cMac, out string outData)
        {
            outData = "";
            int rst = 0;

            if (!IsLink) return false;
            //读取加密机信息
            lock (LockAP)
            {

                StringBuilder data = new StringBuilder(200);
                rst = EncrypAPIGW.Obj_Meter_Formal_VerifyReadData(iKeyState, iOperateMode, cMeterNo, cRandHost, cReadData, cMac, data);

                outData = data.ToString().Replace("\0", "");
            }
            return rst == 0;
        }

        /// <summary>
        ///6.5.4.  上报数据验证 
        /// </summary> 设备主动上报数据时，主站验证数据的合法性。
        /// <param name="iKeyState"></param>
        /// <param name="iOperateMode"></param>
        /// <param name="cMeterNo"></param>
        /// <param name="cRandT">终端随机数(12B)</param>
        /// <param name="cReportData">上报数据</param>
        /// <param name="cMac">MAC 数据</param>
        /// <param name="cOutData">明文数据， iOperateMode=1，为空</param>
        /// <param name="cOutRSPCTR">主动上报随机数</param>
        /// <returns></returns>
        public static bool Obj_Meter_Formal_VerifyReportData(int iKeyState, string iOperateMode, string cMeterNo, string cRandT, string cReportData, string cMac, out string outData, out string outRSPCTR)
        {
            outData = "";
            outRSPCTR = "";
            int rst = 0;

            if (!IsLink) return false;
            //读取加密机信息
            lock (LockAP)
            {
                StringBuilder data = new StringBuilder(100);
                StringBuilder rSPCTR = new StringBuilder(100);
                rst = EncrypAPIGW.Obj_Meter_Formal_VerifyReportData(iKeyState, iOperateMode, cMeterNo, cRandT, cReportData, cMac, data, rSPCTR);
                outData = data.ToString().Replace("\0", "");
                outRSPCTR = rSPCTR.ToString().Replace("\0", "");

            }
            return rst == 0;
        }

        /// <summary>
        ///6.5.5.  上报数据返回报文加密
        /// </summary> 用于设备主动上报主站返回帧数据加密计算。
        /// <param name="iKeyState"></param>
        /// <param name="iOperateMode"></param>
        /// <param name="cMeterNo"></param>
        /// <param name="RandHost"></param>
        /// <param name="cReportData"></param>
        /// <param name="OutSID"></param>
        /// <param name="OutAttachData"></param>
        /// <param name="cOutData"></param>
        /// <param name="ucOutMac"></param>
        /// <returns></returns>
        public static bool Obj_Meter_Formal_GetResponseData(int iKeyState, string iOperateMode, string cMeterNo, string cRandHost, string cReportData, out string outSID, out string outAttachData, out string outData, out string outMac)
        {

            outSID = "";
            outAttachData = "";
            outData = "";
            outMac = "";


            int rst = 0;
            if (!IsLink) return false;

            //读取加密机信息
            lock (LockAP)
            {
                StringBuilder sid = new StringBuilder(300);
                StringBuilder attachData = new StringBuilder(100);
                StringBuilder data = new StringBuilder(100);
                StringBuilder mac = new StringBuilder(100);
                rst = EncrypAPIGW.Obj_Meter_Formal_GetResponseData(iKeyState, iOperateMode, cMeterNo, cRandHost, cReportData, sid, attachData, data, mac);
                outSID = sid.ToString().Replace("\0", "");
                outAttachData = attachData.ToString().Replace("\0", "");
                outData = data.ToString().Replace("\0", "");
                outMac = mac.ToString().Replace("\0", "");
            }
            return rst == 0;
        }

        /// <summary>
        ///6.5.6.  安全传输加密
        /// </summary> 用于对具体业务数据进行数据加密计算。
        /// <param name="iOperateMode"></param>
        /// <param name="cESAMID">此处可为空</param>
        /// <param name="cSessionKey">会话密钥</param>
        /// <param name="cTaskType">参数类型：4，安全模式参数、会话时效门限；5，电价、电价切换时间、费率时段、对时；8，拉闸任务；3，除上述操作外的数据加密，密钥更新、写 ESAM 操作和钱包操作数据下发通过此函数进行安全计算。</param>
        /// <param name="cTaskData">数据明文； NByte</param>
        /// <param name="cOutSID"></param>
        /// <param name="cOutAttachData"></param>
        /// <param name="cOutData"></param>
        /// <param name="cOutMAC"></param>
        /// <returns></returns>
        [HandleProcessCorruptedStateExceptions]
        public static bool Obj_Meter_Formal_GetSessionData(int iOperateMode, string esamID, string sessionKey, int taskType, string taskData, out string outSID, out string outAttachData, out string outData, out string outMAC)
        {
            int rst = 0;

            outSID = "";
            outAttachData = "";
            outData = "";
            outMAC = "";

            if (!IsLink) return false;

            //读取加密机信息
            lock (LockAP)
            {
                StringBuilder PutcOutSID1 = new StringBuilder(8);
                StringBuilder PutcOutAttachData1 = new StringBuilder(100);
                StringBuilder PutcOutData1 = new StringBuilder(5000);
                StringBuilder PutcOutMAC1 = new StringBuilder(8);

                try
                {
                    Debug.WriteLine("------------------------------------------");
                    Debug.WriteLine("函数：Obj_Meter_Formal_GetSessionData");
                    Debug.WriteLine(string.Format("入参iOperateMode：{0}", iOperateMode));
                    Debug.WriteLine(string.Format("入参cESAMID：{0}", esamID));
                    Debug.WriteLine(string.Format("入参cSessionKey：{0}", sessionKey));
                    Debug.WriteLine(string.Format("入参cTaskType：{0}", taskType));
                    Debug.WriteLine(string.Format("入参cTaskData：{0}", taskData));

                    rst = EncrypAPIGW.Obj_Meter_Formal_GetSessionData(iOperateMode, esamID, sessionKey, taskType, taskData, PutcOutSID1, PutcOutAttachData1, PutcOutData1, PutcOutMAC1);

                    outSID = PutcOutSID1.ToString().Replace("\0", "");
                    outAttachData = PutcOutAttachData1.ToString().Replace("\0", "");
                    outData = PutcOutData1.ToString().Replace("\0", "");
                    outMAC = PutcOutMAC1.ToString().Replace("\0", "");

                    Debug.WriteLine(string.Format("出参outSID：{0}", outSID));
                    Debug.WriteLine(string.Format("出参outAttachData：{0}", outAttachData));
                    Debug.WriteLine(string.Format("出参outData：{0}", outData));
                    Debug.WriteLine(string.Format("出参outMAC：{0}", outMAC));
                    Debug.WriteLine("");
                }
                catch (System.Exception ex)
                {
                    string error = "加密库调用异常";
                    error += "\r\n" + "ESAM序列号:" + IsNull(esamID);
                    error += "\r\n" + "会话密钥:" + IsNull(sessionKey);
                    error += "\r\n" + "数据:" + IsNull(taskData);
                    error += "\r\n" + ex.ToString();
                    LYTest.Utility.Log.LogManager.AddMessage(error, LYTest.Utility.Log.EnumLogSource.设备操作日志, LYTest.Utility.Log.EnumLevel.Error);
                    return false;
                }


            }
            return rst == 0;
        }

        [HandleProcessCorruptedStateExceptions]
        public static bool Obj_Meter_Formal_GetSessionDataGPS(int iOperateMode, string esamID, string sessionKey, int taskType, string taskData, out string outSID, out string outAttachData, out string outData, out string outMAC)
        {
            int rst = 0;

            outSID = "";
            outAttachData = "";
            outData = "";
            outMAC = "";

            if (!IsLink) return false;

            //读取加密机信息
            lock (LockAP)
            {
                StringBuilder PutcOutSID1 = new StringBuilder(8);
                StringBuilder PutcOutAttachData1 = new StringBuilder(100);
                StringBuilder PutcOutData1 = new StringBuilder(5000);
                StringBuilder PutcOutMAC1 = new StringBuilder(8);

                try
                {
                    DateTime newTime = DateTime.Now;
                    string year = newTime.Year.ToString("X").PadLeft(4, '0');
                    string month = newTime.Month.ToString("X").PadLeft(2, '0');
                    string day = newTime.Day.ToString("X").PadLeft(2, '0');
                    string hour = newTime.Hour.ToString("X").PadLeft(2, '0');
                    string min = newTime.Minute.ToString("X").PadLeft(2, '0');
                    string sec = newTime.Second.ToString("X").PadLeft(2, '0');

                    //"060101" + "40000200" + "1C" + year + month + day + hour + min + sec + "00"
                    taskData = string.Format("060101400002001C{0}{1}{2}{3}{4}{5}00", year, month, day, hour, min, sec);

                    Debug.WriteLine("------------------------------------------");
                    Debug.WriteLine("函数：Obj_Meter_Formal_GetSessionData");
                    Debug.WriteLine(string.Format("入参iOperateMode：{0}", iOperateMode));
                    Debug.WriteLine(string.Format("入参cESAMID：{0}", esamID));
                    Debug.WriteLine(string.Format("入参cSessionKey：{0}", sessionKey));
                    Debug.WriteLine(string.Format("入参cTaskType：{0}", taskType));
                    Debug.WriteLine(string.Format("入参cTaskData：{0}", taskData));

                    rst = EncrypAPIGW.Obj_Meter_Formal_GetSessionData(iOperateMode, esamID, sessionKey, taskType, taskData, PutcOutSID1, PutcOutAttachData1, PutcOutData1, PutcOutMAC1);

                    outSID = PutcOutSID1.ToString().Replace("\0", "");
                    outAttachData = PutcOutAttachData1.ToString().Replace("\0", "");
                    outData = PutcOutData1.ToString().Replace("\0", "");
                    outMAC = PutcOutMAC1.ToString().Replace("\0", "");

                    Debug.WriteLine(string.Format("出参outSID：{0}", outSID));
                    Debug.WriteLine(string.Format("出参outAttachData：{0}", outAttachData));
                    Debug.WriteLine(string.Format("出参outData：{0}", outData));
                    Debug.WriteLine(string.Format("出参outMAC：{0}", outMAC));
                    Debug.WriteLine("");
                }
                catch (System.Exception ex)
                {
                    string error = "加密库调用异常";
                    error += "\r\n" + "ESAM序列号:" + IsNull(esamID);
                    error += "\r\n" + "会话密钥:" + IsNull(sessionKey);
                    error += "\r\n" + "数据:" + IsNull(taskData);
                    error += "\r\n" + ex.ToString();
                    LYTest.Utility.Log.LogManager.AddMessage(error, LYTest.Utility.Log.EnumLogSource.设备操作日志, LYTest.Utility.Log.EnumLevel.Error);
                    return false;
                }


            }
            return rst == 0;
        }

        /// <summary>
        ///6.5.6.  安全传输加密
        /// </summary> 用于对具体业务数据进行数据加密计算。
        /// <param name="iOperateMode"></param>
        /// <param name="cESAMID">此处可为空</param>
        /// <param name="cSessionKey">会话密钥</param>
        /// <param name="cTaskType">参数类型：4，安全模式参数、会话时效门限；5，电价、电价切换时间、费率时段、对时；8，拉闸任务；3，除上述操作外的数据加密，密钥更新、写 ESAM 操作和钱包操作数据下发通过此函数进行安全计算。</param>
        /// <param name="cTaskData">数据明文； NByte</param>
        /// <param name="cOutSID"></param>
        /// <param name="cOutAttachData"></param>
        /// <param name="cOutData"></param>
        /// <param name="cOutMAC"></param>
        /// <returns></returns>
        [HandleProcessCorruptedStateExceptions]
        public static bool Obj_Meter_Formal_GetMeterSetData(int iOperateMode, string esam, string sessionKey, string taskData, out string outSID, out string outAttachData, out string outData, out string outMAC)
        {
            int rst = 0;

            outSID = "";
            outAttachData = "";
            outData = "";
            outMAC = "";

            if (!IsLink) return false;
            //读取加密机信息
            lock (LockAP)
            {
                StringBuilder PutcOutSID1 = new StringBuilder(8);
                StringBuilder PutcOutAttachData1 = new StringBuilder(100);
                StringBuilder PutcOutData1 = new StringBuilder(5000);
                StringBuilder PutcOutMAC1 = new StringBuilder(8);

                try
                {
                    rst = EncrypAPIGW.Obj_Meter_Formal_GetMeterSetData(iOperateMode, esam, sessionKey, taskData, PutcOutSID1, PutcOutAttachData1, PutcOutData1, PutcOutMAC1);

                    outSID = PutcOutSID1.ToString().Replace("\0", "");
                    outAttachData = PutcOutAttachData1.ToString().Replace("\0", "");
                    outData = PutcOutData1.ToString().Replace("\0", "");
                    outMAC = PutcOutMAC1.ToString().Replace("\0", "");
                }
                catch (System.Exception ex)
                {
                    string error = "加密库调用异常";
                    error += "\r\n" + "ESAM序列号:" + IsNull(esam);
                    error += "\r\n" + "会话密钥:" + IsNull(sessionKey);
                    error += "\r\n" + "数据:" + IsNull(taskData);
                    error += "\r\n" + ex.ToString();
                    LYTest.Utility.Log.LogManager.AddMessage(error, LYTest.Utility.Log.EnumLogSource.设备操作日志, LYTest.Utility.Log.EnumLevel.Error);
                    return false;
                }
            }
            return rst == 0;
        }
        /// <summary>
        ///6.5.7.  安全传输解密
        /// </summary>  用于电能表返回帧数据解密验证。
        /// <param name="iKeyState"></param>
        /// <param name="iOperateMode"></param>
        /// <param name="cESAMID"></param>
        /// <param name="cSessionKey"></param>
        /// <param name="cTaskData">数据</param>
        /// <param name="cMac">MAC 数据</param>
        /// <param name="cOutData">数据明文</param>
        /// <returns></returns>
        [HandleProcessCorruptedStateExceptions]
        public static bool Obj_Meter_Formal_VerifyMeterData(int iKeyState, int iOperateMode, string cESAMID, string cSessionKey, string cTaskData, string cMac, out string outData)
        {
            int rst = 0;

            outData = "";
            if (!IsLink) return false;

            //读取加密机信息
            lock (LockAP)
            {
                StringBuilder cOutData1 = new StringBuilder(400);
                Debug.WriteLine("------------------------------------------");
                Debug.WriteLine("函数：Obj_Meter_Formal_VerifyMeterData");
                Debug.WriteLine(string.Format("入参iKeyState：{0}", iKeyState));
                Debug.WriteLine(string.Format("入参iOperateMode：{0}", iOperateMode));
                Debug.WriteLine(string.Format("入参cESAMID：{0}", cESAMID));
                Debug.WriteLine(string.Format("入参cSessionKey：{0}", cSessionKey));
                Debug.WriteLine(string.Format("入参cTaskData：{0}", cTaskData));
                Debug.WriteLine(string.Format("入参cMac：{0}", cMac));
                try
                {
                    rst = EncrypAPIGW.Obj_Meter_Formal_VerifyMeterData(iKeyState, iOperateMode, cESAMID, cSessionKey, cTaskData, cMac, cOutData1);
                    outData = cOutData1.ToString().Replace("\0", "");
                    Debug.WriteLine(string.Format("出参outData：{0}", outData));
                    Debug.WriteLine("");
                }
                catch (System.Exception ex)
                {
                    string error = "加密库调用异常";
                    error += "\r\n" + "ESAM序列号:" + IsNull(cESAMID);
                    error += "\r\n" + "会话密钥:" + IsNull(cSessionKey);
                    error += "\r\n" + "数据:" + IsNull(cTaskData);
                    error += "\r\n" + "MAC数据:" + IsNull(cMac);
                    error += "\r\n" + ex.ToString();
                    LYTest.Utility.Log.LogManager.AddMessage(error, LYTest.Utility.Log.EnumLogSource.设备操作日志, LYTest.Utility.Log.EnumLevel.Error);
                    return false;
                }

            }
            return rst == 0;
        }

        /// <summary>
        /// 6.5.8. 广播数据加密
        /// </summary> 用于广播数据加密计算。
        /// <param name="iKeyState"></param>
        /// <param name="iOperateMode"></param>
        /// <param name="cESAMID">此处可为空</param>
        /// <param name="cBrdCstAddr">广播地址</param>
        /// <param name="AGSEQ">广播应用通信序列号， 4Byte</param>
        /// <param name="cBrdCstData">广播数据明文；</param>
        /// <param name="cOutSID"></param>
        /// <param name="cOutAttachData"></param>
        /// <param name="cOutData"></param>
        /// <param name="cOutMac"></param>
        /// <returns></returns>
        public static bool Obj_Meter_Formal_GetGrpBrdCstData(int iKeyState, int iOperateMode, string cESAMID, string cBrdCstAddr, string cAGSEQ, string cBrdCstData, out string outSID, out string outAttachData, out string outData, out string outMac)
        {
            int rst = 0;

            outSID = "";
            outAttachData = "";
            outData = "";
            outMac = "";
            if (!IsLink) return false;

            //读取加密机信息
            lock (LockAP)
            {
                StringBuilder sid = new StringBuilder(300);
                StringBuilder attachData = new StringBuilder(100);
                StringBuilder data = new StringBuilder(100);
                StringBuilder mac = new StringBuilder(100);
                rst = EncrypAPIGW.Obj_Meter_Formal_GetGrpBrdCstData(iKeyState, iOperateMode, cESAMID, cBrdCstAddr, cAGSEQ, cBrdCstData, sid, attachData, data, mac);

                outSID = sid.ToString().Replace("\0", "");
                outAttachData = attachData.ToString().Replace("\0", "");
                outData = data.ToString().Replace("\0", "");
                outMac = mac.ToString().Replace("\0", "");
            }
            return rst == 0;
        }

        /// <summary>
        ///6.5.9.  设置 ESAM 参数
        /// </summary> 用于设置表号、当前套电价文件、备用套电价文件、 ESAM 存储标识。
        /// <param name="iKeyState"></param>
        /// <param name="iOperateMode"></param>
        /// <param name="cESAMID"></param>
        /// <param name="cSessionKey"></param>
        /// <param name="cMeterNo">表号(8Byte)，不够 8Byte 前面填充 0</param>
        /// <param name="cESAMRand"></param>
        /// <param name="cData">4ByteOAD + 1Byte 内容 LEN + 内容</param>
        /// <param name="OutSID"></param>
        /// <param name="OutAddData"></param>
        /// <param name="OutData"></param>
        /// <param name="OutMAC"></param>
        /// <returns></returns>
        [HandleProcessCorruptedStateExceptions]
        public static bool Obj_Meter_Formal_SetESAMData(int iKeyState, int iOperateMode, string cESAMID, string cSessionKey, string cMeterNo, string cESAMRand, string cData, out string outSID, out string outAddData, out string outData, out string outMAC)
        {
            int rst = 0;

            outSID = "";
            outAddData = "";
            outData = "";
            outMAC = "";
            if (!IsLink) return false;

            //读取加密机信息
            lock (LockAP)
            {
                StringBuilder strOutSID = new StringBuilder(300);
                StringBuilder strcOutAttachData = new StringBuilder(100);
                StringBuilder strcOutData = new StringBuilder(100);
                StringBuilder strcOutMac = new StringBuilder(100);
                Trace.WriteLine("------------------------------------------");
                Trace.WriteLine("函数：Obj_Meter_Formal_SetESAMData");
                Trace.WriteLine(string.Format("入参iKeyState：{0}", iKeyState));
                Trace.WriteLine(string.Format("入参iOperateMode：{0}", iOperateMode));
                Trace.WriteLine(string.Format("入参cESAMID：{0}", cESAMID));
                Trace.WriteLine(string.Format("入参cSessionKey：{0}", cSessionKey));
                Trace.WriteLine(string.Format("入参cMeterNo：{0}", cMeterNo));
                Trace.WriteLine(string.Format("入参cESAMRand：{0}", cESAMRand));
                Trace.WriteLine(string.Format("入参cData：{0}", cData));
                try
                {
                    rst = EncrypAPIGW.Obj_Meter_Formal_SetESAMData(iKeyState, iOperateMode, cESAMID, cSessionKey, cMeterNo, cESAMRand, cData, strOutSID, strcOutAttachData, strcOutData, strcOutMac);

                    outSID = strOutSID.ToString().Replace("\0", "");
                    outAddData = strcOutAttachData.ToString().Replace("\0", "");
                    outData = strcOutData.ToString().Replace("\0", "");
                    outMAC = strcOutMac.ToString().Replace("\0", "");

                    Trace.WriteLine(string.Format("函数返回值：{0}", rst));
                    Trace.WriteLine(string.Format("出参outSID：{0}", outSID));
                    Trace.WriteLine(string.Format("出参outAttachData：{0}", outAddData));
                    Trace.WriteLine(string.Format("出参outData：{0}", outData));
                    Trace.WriteLine(string.Format("出参outMAC：{0}", outMAC));
                    Trace.WriteLine("");
                }
                catch (System.Exception ex)
                {
                    string error = "加密库调用异常";
                    error += "\r\n" + "ESAM序列号:" + IsNull(cESAMID);
                    error += "\r\n" + "会话密钥:" + IsNull(cSessionKey);
                    error += "\r\n" + "表号:" + IsNull(cMeterNo);
                    error += "\r\n" + "随机数:" + IsNull(cESAMRand);
                    error += "\r\n" + "OAD内容:" + IsNull(cData);
                    error += "\r\n" + ex.ToString();
                    LYTest.Utility.Log.LogManager.AddMessage(error, LYTest.Utility.Log.EnumLogSource.设备操作日志, LYTest.Utility.Log.EnumLevel.Error);
                    return false;
                }


            }
            return rst == 0;
        }

        /// <summary>
        /// 6.5.10. 钱包操作
        /// </summary>用于设置表号、当前套电价文件、备用套电价文件、 ESAM 存储标识。
        /// <param name="iOperateMode"></param>
        /// <param name="cESAMID"></param>
        /// <param name="cSessionKey"></param>
        /// <param name="cTaskType">任务序编号， 9，钱包初始化； 10，钱包充值； 11，钱包退费</param>
        /// <param name="cTaskData">数据明文，包含预置金额， 4Byte cOutSID</param>
        /// <param name="cOutSID"></param>
        /// <param name="cOutAttachData"></param>
        /// <param name="cOutData"></param>
        /// <param name="cOutMAC"></param>
        /// <returns></returns>
        [HandleProcessCorruptedStateExceptions]
        public static bool Obj_Meter_Formal_GetPurseData(int iOperateMode, string cESAMID, string cSessionKey, string cTaskType, string cTaskData, out string outSID, out string outAttachData, out string outData, out string outMAC)
        {
            int rst = 0;

            outSID = "";
            outAttachData = "";
            outData = "";
            outMAC = "";
            if (!IsLink) return false;

            //读取加密机信息
            lock (LockAP)
            {
                StringBuilder sid = new StringBuilder(300);
                StringBuilder attachData = new StringBuilder(100);
                StringBuilder data = new StringBuilder(100);
                StringBuilder mac = new StringBuilder(100);
                int taskType = int.Parse(cTaskType);
                try
                {
                    rst = EncrypAPIGW.Obj_Meter_Formal_GetPurseData(iOperateMode, cESAMID, cSessionKey, taskType, cTaskData, sid, attachData, data, mac);
                    outSID = sid.ToString().Replace("\0", "");
                    outAttachData = attachData.ToString().Replace("\0", "");
                    outData = data.ToString().Replace("\0", "");
                    outMAC = mac.ToString().Replace("\0", "");
                }
                catch (System.Exception ex)
                {
                    string error = "加密库调用异常";
                    error += "\r\n" + "ESAM序列号:" + IsNull(cESAMID);
                    error += "\r\n" + "会话密钥:" + IsNull(cSessionKey);
                    error += "\r\n" + "任务编号:" + IsNull(cTaskType);
                    error += "\r\n" + "数据明文:" + IsNull(cTaskData);
                    error += "\r\n" + ex.ToString();
                    LYTest.Utility.Log.LogManager.AddMessage(error, LYTest.Utility.Log.EnumLogSource.设备操作日志, LYTest.Utility.Log.EnumLevel.Error);
                    return false;
                }


            }
            return rst == 0;
        }

        /// <summary>
        ///6.5.11.  电能表对称密钥更新
        /// </summary> 用于对称密钥更新。
        /// <param name="iKeyState">密钥更新的目标状态， 1，代表更新到私钥， 0 代表恢复到初始密钥；</param>
        /// <param name="cESAMID"></param>
        /// <param name="cSessionKey"></param>
        /// <param name="cMeterNo">表号(8Bytes)</param>
        /// <param name="cKeyType">密钥类型，此处为 00，应用密钥</param>
        /// <param name="cOutSID"></param>
        /// <param name="cOutAttachData"></param>
        /// <param name="cOutTrmKeyData"></param>
        /// <param name="cOutMAC"></param>
        /// <returns></returns>
        [HandleProcessCorruptedStateExceptions]
        public static bool Obj_Meter_Formal_GetTrmKeyData(int iKeyState, string cESAMID, string cSessionKey, string cMeterNo, string cKeyType, out string outSID, out string outAttachData, out string outTrmKeyData, out string outMAC)
        {
            int rst = 0;

            outSID = "";
            outAttachData = "";
            outTrmKeyData = "";
            outMAC = "";

            if (!IsLink)
            {
                LYTest.Utility.Log.LogManager.AddMessage($"加密机联机状态:{IsLink}", LYTest.Utility.Log.EnumLogSource.设备操作日志, LYTest.Utility.Log.EnumLevel.Error);
                return false;
            }
            //读取加密机信息
            lock (LockAP)
            {
                StringBuilder sid = new StringBuilder(8);
                StringBuilder attachData = new StringBuilder(50);
                StringBuilder trmKeyData = new StringBuilder(5000);
                StringBuilder mac = new StringBuilder(9);

                try
                {
                    rst = EncrypAPIGW.Obj_Meter_Formal_GetTrmKeyData(iKeyState, cESAMID, cSessionKey, cMeterNo, cKeyType, sid, attachData, trmKeyData, mac);
                    if (rst == 0)
                    {
                        outSID = sid.ToString().Replace("\0", "");
                        outAttachData = attachData.ToString().Replace("\0", "");
                        outTrmKeyData = trmKeyData.ToString().Replace("\0", "");
                        outMAC = mac.ToString().Replace("\0", "");
                    }
                    else
                    {
                        string msg = GetErrMsg(rst);
                        LYTest.Utility.Log.LogManager.AddMessage($"{rst}:{msg}", LYTest.Utility.Log.EnumLogSource.设备操作日志, LYTest.Utility.Log.EnumLevel.Error);
                    }
                }
                catch (System.Exception ex)
                {
                    string error = "加密库调用异常";
                    error += "\r\n" + "ESAM序列号:" + IsNull(cESAMID);
                    error += "\r\n" + "会话密钥:" + IsNull(cSessionKey);
                    error += "\r\n" + "表号:" + IsNull(cMeterNo);
                    error += "\r\n" + "密钥类型:" + IsNull(cKeyType);
                    error += "\r\n" + ex.ToString();
                    LYTest.Utility.Log.LogManager.AddMessage(error, LYTest.Utility.Log.EnumLogSource.设备操作日志, LYTest.Utility.Log.EnumLevel.Error);
                    return false;
                }

            }
            return rst == 0;
        }

        /// <summary>
        /// 6.5.12. 电表对称密钥初始化 
        /// </summary> 用于对电能表对称密钥进行初始化，须先对密钥进行初始化，此函数暂时不需要使用。
        /// <param name="iKeyState"></param>
        /// <param name="cESAMID"></param>
        /// <param name="cSessionKey"></param>
        /// <param name="cMeterNo"></param>
        /// <param name="cKeyType"></param>
        /// <param name="outSID"></param>
        /// <param name="cOutAttachData"></param>
        /// <param name="cOutTrmKeyData"></param>
        /// <param name="cOutMAC"></param>
        /// <returns></returns>
        public static bool Obj_Meter_Formal_InitTrmKeyData(int iKeyState, string cESAMID, string cSessionKey, string cMeterNo, string cKeyType, out string outSID, out string outAttachData, out string outTrmKeyData, out string outMAC)
        {
            int rst = 0;
            outSID = "";
            outAttachData = "";
            outTrmKeyData = "";
            outMAC = "";

            if (!IsLink) return false;

            //读取加密机信息
            lock (LockAP)
            {
                StringBuilder sid = new StringBuilder(100);
                StringBuilder attachData = new StringBuilder(100);
                StringBuilder trmKeyData = new StringBuilder(100);
                StringBuilder mac = new StringBuilder(100);

                rst = EncrypAPIGW.Obj_Meter_Formal_InitTrmKeyData(iKeyState, cESAMID, cSessionKey, cMeterNo, cKeyType, sid, attachData, trmKeyData, mac);

                outSID = sid.ToString().Replace("\0", "");
                outAttachData = attachData.ToString().Replace("\0", "");
                outTrmKeyData = trmKeyData.ToString().Replace("\0", "");
                outMAC = mac.ToString().Replace("\0", "");
            }
            return rst == 0;
        }

        #region 物联表
        /// <summary>
        /// 主站会话协商（计量芯）
        /// </summary>
        /// <param name="iKeyState"></param>
        /// <param name="cTESAMID">ESAM 序列号(8Byte)</param>
        /// <param name="cDiv">表号(8Byte)，表号不足 8 字节前补 0</param>
        /// <param name="cASCTR">应用会话计数器，从芯片读取</param>
        /// <param name="cFLG">应用密钥产生标识，1Byte，默认”01”</param>
        /// <param name="cOutRandHost">主站随机数(16Byte)</param>
        /// <param name="cOutSessionInit">会话协商数据，建立应用连接中的密文 1；</param>
        /// <param name="cOutMac">协商数据签名(4Byte)，建立应用连接中的客户机签名 1</param>
        /// <returns>返回 0 成功,其他 失败</returns>
        public static bool Obj_JL_Formal_InitSession(int iKeyState, string cTESAMID, string cDiv, string cASCTR, string cFLG, out string cOutRandHost, out string cOutSessionInit, out string cOutMac)
        {
            cOutRandHost = "";
            cOutSessionInit = "";
            cOutMac = "";
            if (!IsLink) return false;
            //读取加密机信息
            lock (LockAP)
            {
                string div = cDiv.PadLeft(16, '0');
                string TESAMID = cTESAMID.PadLeft(16, '0');
                string ASCTR = cASCTR.PadLeft(8, '0');
                StringBuilder OutRandHost = new StringBuilder(32);
                StringBuilder OutSessionInit = new StringBuilder(64);
                StringBuilder OutMac = new StringBuilder(8);

                try
                {
                    int rst = EncrypAPIGW.Obj_JL_Formal_InitSession(iKeyState, TESAMID, div, ASCTR, cFLG, OutRandHost, OutSessionInit, OutMac);

                    if (rst != 0)
                    {
                        string msg = GetErrMsg(rst);
                        LYTest.Utility.Log.LogManager.AddMessage("【计量芯获取应用连接密文失败，请检查,错误信息为】" + msg, LYTest.Utility.Log.EnumLogSource.设备操作日志, LYTest.Utility.Log.EnumLevel.Error);
                    }
                    else
                    {
                        cOutRandHost = OutRandHost.ToString().Replace("\0", "");
                        cOutSessionInit = OutSessionInit.ToString().Replace("\0", "");
                        cOutMac = OutMac.ToString().Replace("\0", "");
                    }
                    return rst == 0;
                }
                catch (System.Exception ex)
                {
                    string error = "加密库调用异常";
                    error += "\r\n" + "分散因子:" + IsNull(cDiv);
                    error += "\r\n" + "会话密钥:" + IsNull(cOutSessionInit);
                    error += "\r\n" + "主站随机数R1:" + IsNull(cOutRandHost);
                    error += "\r\n" + "数据签名:" + IsNull(cOutMac);
                    error += "\r\n" + ex.ToString();
                    LYTest.Utility.Log.LogManager.AddMessage(error, LYTest.Utility.Log.EnumLogSource.设备操作日志, LYTest.Utility.Log.EnumLevel.Error);
                    return false;
                }
            }
        }
        /// <summary>
        /// 主站会话协商验证（计量芯）
        /// </summary>
        /// <param name="iKeyState"></param>
        /// <param name="cTESAMID">ESAM 序列号(8Byte)</param>
        /// <param name="cDiv">表号(8Byte)，表号不足 8 字节前补 0</param>
        /// <param name="cRandHost">主站随机数 R1（16Byte）</param>
        /// <param name="cSessionData">终端返回的会话协商数据(48Byte)，建立应用连接中的密文 2</param>
        /// <param name="cSign">终端返回的会话协商数据签名(4Byte) ，建立应用连接中的客户机签名 2</param>
        /// <param name="cOutSessionKey">会话密钥</param>
        /// <returns>返回 0 成功,其他 失败</returns>
        public static bool Obj_JL_Formal_VerifySession(int iKeyState, string cTESAMID, string Div, string RandHost, string SessionData, string Sign, out string OutSessionKey)
        {
            OutSessionKey = "";
            int rst = 0;
            if (!IsLink) return false;

            //读取加密机信息
            lock (LockAP)
            {
                string cdiv = Div.PadLeft(16, '0');
                string TESAMID = cTESAMID.PadLeft(16, '0');
                StringBuilder sessionKey = new StringBuilder(352);

                try
                {
                    rst = EncrypAPIGW.Obj_JL_Formal_VerifySession(iKeyState, TESAMID, cdiv, RandHost, SessionData, Sign, sessionKey);
                    OutSessionKey = sessionKey.ToString().Replace("\0", "");
                }
                catch (System.Exception ex)
                {
                    string error = "加密库调用异常";
                    error += "\r\n" + "分散因子:" + IsNull(cdiv);
                    error += "\r\n" + "会话密钥:" + IsNull(SessionData);
                    error += "\r\n" + "主站随机数R1:" + IsNull(RandHost);
                    error += "\r\n" + "数据签名:" + IsNull(Sign);
                    error += "\r\n" + ex.ToString();
                    LYTest.Utility.Log.LogManager.AddMessage(error, LYTest.Utility.Log.EnumLogSource.设备操作日志, LYTest.Utility.Log.EnumLevel.Error);
                    return false;
                }
            }
            return rst == 0;

        }

        #endregion

        /// <summary>
        /// 获取错误码的描述
        /// </summary>
        /// <param name="code">错误代码</param>
        /// <returns></returns>
        private static string GetErrMsg(int code)
        {

            string msg = "";
            if (code != 0)
            {
                switch (code)
                {
                    case 48:
                        msg = "无设备或设备无效";
                        break;
                    case 56:
                        msg = "创建socket 句柄失败";
                        break;
                    case 57:
                        msg = "连接服务器失败";
                        break;
                    case 64:
                        msg = "客户端发送数据失败";
                        break;
                    case 65:
                        msg = "客户端接收数据失败";
                        break;
                    case 100:
                        msg = "打开设备失败";
                        break;
                    case 160:
                        msg = " 连接密码机失败";
                        break;
                    case 161:
                        msg = "操作权限不够";
                        break;
                    case 162:
                        msg = "USBKey 不是操作员";
                        break;
                    case 163:
                        msg = "服务器发送数据失败";
                        break;
                    case 164:
                        msg = "服务端接收报文失败";
                        break;
                    case 165:
                        msg = "密码机加密数据失败";
                        break;
                    case 166:
                        msg = "密码机导出密钥失败";
                        break;
                    case 167:
                        msg = "密码机计算MAC 失败";
                        break;
                    case 168:
                        msg = " 服务器已断开连接";
                        break;
                    case 169:
                        msg = "数据无效";
                        break;
                    case 170:
                        msg = "密码机收发报文错误";
                        break;
                    case 171:
                        msg = "密码机故障";
                        break;
                    case 172:
                        msg = "数据库出错";
                        break;
                    case 1100:
                        msg = "系统认证错误";
                        break;
                    case 1107:
                        msg = "USBKey 权限不正确";
                        break;
                    default:
                        if (code >= 700 && code <= 712)
                        {
                            msg = "客户端导出密钥失败";
                        }
                        else if (code >= 800 && code <= 810)
                        {
                            msg = "计算MAC失败";
                        }
                        else if (code >= 900 && code <= 910)
                        {
                            msg = "加密数据失败";
                        }
                        else if (code >= 1000 && code <= 1010)
                        {
                            msg = "数据长度错";
                        }
                        else if (code >= 1108 && code <= 1111)
                        {
                            msg = "操作USBKey 失败";
                        }
                        else
                        {
                            msg = "未知其他错误";
                        }
                        break;
                }
                msg = "错误码：" + code + " 错误描述:" + msg + "。";
            }
            return msg;
        }
    }
}
