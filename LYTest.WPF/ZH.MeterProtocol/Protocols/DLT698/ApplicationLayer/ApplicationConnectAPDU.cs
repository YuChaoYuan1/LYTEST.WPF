using System;
using System.Collections.Generic;
using System.Text;
using LYTest.MeterProtocol.Protocols.ApplicationLayer;
using LYTest.MeterProtocol.Protocols.DLT698.ApplicationLayer.ApplicationLayerStructure;
using LYTest.MeterProtocol.Protocols.DLT698.Enum;
using LYTest.MeterProtocol.Protocols.DLT698.Struct;

namespace LYTest.MeterProtocol.Protocols.DLT698.ApplicationLayer
{
    /// <summary>
    /// 应用连接APDU
    /// </summary>
    public class ApplicationConnectAPDU : APDU
    {
        /// <summary>
        /// 认证进制信息
        /// </summary>
        private StConnectMechanismInfo ConnectInfo { get; set; }

        /// <summary>
        /// 应用连接请求结构
        /// </summary>
        private AppConnectionStructure.ConnectionRequestText ConnectText;

        /// <summary>
        /// 应用连接请求APDU帧
        /// </summary>
        private StringBuilder AppConnectStr = null;

        public ApplicationConnectAPDU(StConnectMechanismInfo connectInfo, EmSecurityMode mode)
            : base(mode)
        {
            ConnectInfo = connectInfo;

        }
        public ApplicationConnectAPDU() { }
        /// <summary>
        /// 应用连接请求APDU帧
        /// </summary>
        /// <returns></returns>
        public override string AppConnect()
        {
            AppConnectStr = new StringBuilder();
            ConnectText = new AppConnectionStructure.ConnectionRequestText
            {
                ConnectRequest = ((int)EmServieType.CONNECT_Request).ToString().PadLeft(2, '0'),
                PIID = "00",
                ProtocolVersion = "0010",
                ProtocolConformance = "FFFFFFFFFFFFFFFF",
                FunctionConformance = "FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF",
                ClientSendMaxFrameSize = "0400",
                ClientReceiveMaxFrameSize = "0400",
                ClientReceiveMaxFrameFormSize = "01",
                ClientHandleMaxApduSize = "0400",
                OutTime = "00000064"
            };

            ConnectText.PIID = "02";
            ConnectText.ProtocolVersion = "0016";
            ConnectText.ProtocolConformance = "FFFFFFFFC0000000";
            ConnectText.FunctionConformance = "FFFEC400000000000000000000000000";
            ConnectText.ClientSendMaxFrameSize = "0200";
            ConnectText.ClientReceiveMaxFrameSize = "0200";
            ConnectText.ClientReceiveMaxFrameFormSize = "01";
            ConnectText.ClientHandleMaxApduSize = "07D0";
            ConnectText.OutTime = "00001C20";
            ConnectText.ConnectMechanismInfos = ((int)ConnectInfo.ConnectInfo).ToString().PadLeft(2, '0') +
                CommFun.GroupByteString(ConnectInfo.SessionData.SessionData) + CommFun.GroupByteString(ConnectInfo.SessionData.MAC);

            ConnectText.TimeFlag = "00";

            return GroupConnectApduFrame();
        }
        private string GroupConnectApduFrame()
        {
            AppConnectStr = new StringBuilder();
            AppConnectStr.Append(ConnectText.ConnectRequest);
            AppConnectStr.Append(ConnectText.PIID);
            AppConnectStr.Append(ConnectText.ProtocolVersion);
            AppConnectStr.Append(ConnectText.ProtocolConformance);
            AppConnectStr.Append(ConnectText.FunctionConformance);
            AppConnectStr.Append(ConnectText.ClientSendMaxFrameSize);
            AppConnectStr.Append(ConnectText.ClientReceiveMaxFrameSize);
            AppConnectStr.Append(ConnectText.ClientReceiveMaxFrameFormSize);
            AppConnectStr.Append(ConnectText.ClientHandleMaxApduSize);
            AppConnectStr.Append(ConnectText.OutTime);
            AppConnectStr.Append(ConnectText.ConnectMechanismInfos);
            AppConnectStr.Append(ConnectText.TimeFlag);

            return AppConnectStr.ToString();

        }


        /// <summary>
        /// 解析应用连接数据
        /// </summary>
        /// <param name="Frame">帧</param>
        /// <param name="ErrCode">错误代码</param>
        /// <param name="data">数据内容</param>
        /// <returns>执行成功与否</returns>
        public override bool ParseConnectionFrame(byte[] Frame, ref int ErrCode, ref List<object> data)
        {
            ErrCode = 0;
            data = new List<object>();
            byte[] RN;
            byte[] SecurityData;
            if (Frame[71] == 0)//允许建立应用连接
            {
                if (Frame[72] == 0x01)//存在附加信息
                {
                    RN = new byte[Frame[73]];
                    if (Frame.Length > 74 + RN.Length)
                    {
                        SecurityData = new byte[Frame[74 + RN.Length]];
                        Array.Copy(Frame, 74, RN, 0, Frame[73]);
                        Array.Copy(Frame, 75 + RN.Length, SecurityData, 0, Frame[74 + RN.Length]);
                        data.Add(CommFun.BytesToHexStr(RN));
                        data.Add(CommFun.BytesToHexStr(SecurityData));
                    }
                    else
                    {
                        ErrCode = Frame[71];
                        return false;
                    }
                }
                return true;
            }
            else
            {
                ErrCode = Frame[71];
                return false;
            }
        }
    }
}
