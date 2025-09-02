using System.Collections.Generic;
using System.Text;
using LYTest.MeterProtocol.Protocols.ApplicationLayer;
using LYTest.MeterProtocol.Protocols.DLT698.ApplicationLayer.ApplicationLayerStructure;
using LYTest.MeterProtocol.Protocols.DLT698.Enum;
using LYTest.MeterProtocol.Protocols.DLT698.Struct;

namespace LYTest.MeterProtocol.Protocols.DLT698.ApplicationLayer
{
    public class OperationAPDU : APDU
    {

        /// <summary>
        /// 明文操作结构
        /// </summary>
        OperationByClearStructure.OperationApduClearText ClearText;

        /// <summary>
        /// 明文+MAC操作结构
        /// </summary>
        OperationByClearMacStructure.OperationApduClearMac ClearMacText;

        /// <summary>
        /// 密文+MAC操作结构
        /// </summary>
        OperationByCipherMacStructure.OperationApduCipherMac CipherText;
        /// <summary>
        /// 明文操作应用层报文
        /// </summary>
        StringBuilder ClearActionStr = null;

        /// <summary>
        /// 明文+MAC操作应用层报文
        /// </summary>
        StringBuilder ClearAndMacActionStr = null;

        /// <summary>
        /// 密文+MAC操作应用层报文
        /// </summary>
        StringBuilder CiphAndMacActionStr = null;

        /// <summary>
        /// 对象方法描述符
        /// </summary>
        public List<string> OMD { get; set; }
        public OperationAPDU(List<string> omd, StSIDMAC sidmac, EmSecurityMode mode)
            : base(mode)
        {
            OMD = omd;
            SidMac = sidmac;
            ObjectInfos = ObjectInfosManage.Instance();

        }
        public OperationAPDU() { }
        public override string OperationAPDU_Frame()
        {
            switch (SecurityMode)
            {
                case EmSecurityMode.ClearText:
                    return ClearOperationApduFrame();

                case EmSecurityMode.ClearTextMac:
                    return ClearMacOperationApduFrame();

                case EmSecurityMode.CiphertextMac:
                    return CipherMacOperationApduFrame();

                default:
                    return string.Empty;
            }
        }

        /// <summary>
        /// 明文操作对象方法
        /// </summary>
        /// <returns></returns>
        private string ClearOperationApduFrame()
        {
            obatt = ObjectInfos.AttributeInfos.Find(e => e.Oad.ToLowerInvariant() == OMD[0].ToLowerInvariant());
            if (obatt == null)
            {
                return string.Empty;
            }
            ClearText = new OperationByClearStructure.OperationApduClearText
            {
                ActionRequest = ((int)EmServieType.ACTION_Request).ToString().PadLeft(2, '0')
            };
            if (OMD.Count == 1)
            {
                ClearText.ActionRequestMode = ((int)EmOperationMode.ActionRequest).ToString().PadLeft(2, '0');
            }
            else
            {
                ClearText.ActionRequestMode = ((int)EmOperationMode.ActionRequestList).ToString().PadLeft(2, '0');
            }
            ClearText.PIID = "01";
            ClearText.Num = OMD.Count.ToString().PadLeft(2, '0');
            ClearText.DataInfo = new List<OperationByClearStructure.DataInfos>();
            for (int i = 0; i < OMD.Count; i++)
            {
                obatt = ObjectInfos.AttributeInfos.Find(e => e.Oad == OMD[i]);
                OperationByClearStructure.DataInfos dataInfo = new OperationByClearStructure.DataInfos
                {
                    OMD = OMD[i],
                    Parameters = CommFun.GroupDataToSend(obatt, SidMac.Data[OMD[i]])
                };
                ClearText.DataInfo.Add(dataInfo);
            }
            ClearText.TimeFlag = "00";
            return GroupClearText();

        }
        /// <summary>
        /// 明文+MAC 操作 对象方法
        /// </summary>
        /// <returns></returns>
        private string ClearMacOperationApduFrame()
        {
            string ClearText = SidMac.Data[OMD[0]][0];
            ClearActionStr = new StringBuilder();
            ClearActionStr.Append(SidMac.Data[OMD[0]]);
            ClearMacText = new OperationByClearMacStructure.OperationApduClearMac
            {
                SecurtiyRequest = "10",//安全传输请求
                ClearMode = "00",  //明文传输
                ClearText = CommFun.GroupByteString(ClearText),//明文
                DataValidateMode = ((int)EmDataValidationMode.SID_MAC).ToString().PadLeft(2, '0'),//数据验证模式
                ValidationInfo = SidMac.SID + CommFun.GroupByteString(SidMac.AttachData) + CommFun.GroupByteString(SidMac.MAC)//验证信息
            };
            return GroupClearMacText();
        }
        /// <summary>
        /// 密文+MAC 操作对象方法
        /// </summary>
        /// <returns></returns>
        private string CipherMacOperationApduFrame()
        {
            string Cipher = SidMac.Data[OMD[0]][0];
            CipherText = new OperationByCipherMacStructure.OperationApduCipherMac
            {
                SecurtiyRequest = "10",//安全传输模式
                CipherMode = "01",//密文传输
                CipherText = CommFun.GroupByteString(Cipher),//密文数据
                DataValidateMode = ((int)EmDataValidationMode.SID_MAC).ToString().PadLeft(2, '0'),//验证模式
                ValidationInfo = SidMac.SID + CommFun.GroupByteString(SidMac.AttachData) + CommFun.GroupByteString(SidMac.MAC)//验证信息
            };
            return GroupCipherMacText();
        }

        private string GroupClearText()
        {
            ClearActionStr = new StringBuilder();
            ClearActionStr.Append(ClearText.ActionRequest);
            ClearActionStr.Append(ClearText.ActionRequestMode);
            ClearActionStr.Append(ClearText.PIID);
            if (ClearText.ActionRequestMode != "01") ClearActionStr.Append(ClearText.Num);
            ClearText.DataInfo.ForEach(e =>
            {
                ClearActionStr.Append(e.OMD);
                ClearActionStr.Append(e.Parameters);

            });
            ClearActionStr.Append(ClearText.TimeFlag);
            return ClearActionStr.ToString();
        }
        private string GroupClearMacText()
        {
            ClearAndMacActionStr = new StringBuilder();
            ClearAndMacActionStr.Append(ClearMacText.SecurtiyRequest);
            ClearAndMacActionStr.Append(ClearMacText.ClearMode);
            ClearAndMacActionStr.Append(ClearMacText.ClearText);
            ClearAndMacActionStr.Append(ClearMacText.DataValidateMode);
            ClearAndMacActionStr.Append(ClearMacText.ValidationInfo);
            return ClearAndMacActionStr.ToString();
        }
        private string GroupCipherMacText()
        {
            CiphAndMacActionStr = new StringBuilder();
            CiphAndMacActionStr.Append(CipherText.SecurtiyRequest);
            CiphAndMacActionStr.Append(CipherText.CipherMode);
            CiphAndMacActionStr.Append(CipherText.CipherText);
            CiphAndMacActionStr.Append(CipherText.DataValidateMode);
            CiphAndMacActionStr.Append(CipherText.ValidationInfo);
            return CiphAndMacActionStr.ToString();

        }

        /// <summary>
        /// 解析明文操作返回数据
        /// </summary>
        /// <param name="Frame">帧</param>
        /// <param name="ErrCode">错误代码</param>
        /// <returns>执行成功与否</returns>
        public override bool ParseActionFrame(byte[] Frame, ref int ErrCode)
        {
            EmOperationMode SetMode = (EmOperationMode)Frame[1];
            ErrCode = 0;
            if (SetMode == EmOperationMode.ActionRequest)
            {
                ErrCode = Frame[7];
                if (Frame[7] == 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else if (SetMode == EmOperationMode.ActionRequestList)
            {
                int Num = Frame[3];//OAD个数
                int NO = 8;//第一个OAD执行结果的索引
                while (Num > 0)
                {
                    if (Frame[NO] != 0)
                    {
                        ErrCode = Frame[NO];
                        return false;
                    }
                    ErrCode += Frame[NO];
                    NO += 5;//下一个OAD执行结果的索引
                    Num--;
                }

                if (ErrCode == 0)
                {
                    return true;
                }
            }
            else
            {
                return false;
            }
            return true;

        }

    }
}
