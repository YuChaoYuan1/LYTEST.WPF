using System.Collections.Generic;
using System.Text;
using LYTest.MeterProtocol.Protocols.DLT698;
using LYTest.MeterProtocol.Protocols.DLT698.ApplicationLayer.ApplicationLayerStructure;
using LYTest.MeterProtocol.Protocols.DLT698.Enum;
using LYTest.MeterProtocol.Protocols.DLT698.Struct;

namespace LYTest.MeterProtocol.Protocols.ApplicationLayer
{
    /// <summary>
    /// 设置属性APDU部分报文 
    /// </summary>
    public class SetDataAPDU : APDU
    {
        /// <summary>
        /// 明文设置结构
        /// </summary>
        SetByClearStructure.SetApduFrameClearText ClearText;

        /// <summary>
        /// 明文+MAC设置结构
        /// </summary>
        SetByClearMacStructure.SetApduClearMac ClearMacText;

        /// <summary>
        /// 密文+MAC设置结构
        /// </summary>
        SetByCipherMacStructure.SetApduCipherMac CipherText;
        /// <summary>
        /// 明文设置属性应用层报文
        /// </summary>
        StringBuilder ClearSetDataStr = null;

        /// <summary>
        /// 明文+MAC设置属性应用层报文
        /// </summary>
        StringBuilder ClearAndMacSetDataStr = null;

        /// <summary>
        /// 密文+MAC设置属性应用层报文
        /// </summary>
        StringBuilder CiphAndMacSetDataStr = null;


        /// <summary>
        /// 对象属性标识
        /// </summary>
        public List<string> OAD { get; set; }
        public SetDataAPDU(List<string> oad, StSIDMAC sidmac, EmSecurityMode mode)
            : base(mode)
        {
            SidMac = sidmac;
            OAD = oad;
            ObjectInfos = ObjectInfosManage.Instance();
        }
        public SetDataAPDU() { }

        public override string SetDataAPDU_Frame()
        {
            switch (SecurityMode)
            {
                case EmSecurityMode.ClearText:
                    return ClearSetDataApduFrame();

                case EmSecurityMode.ClearTextMac:
                    return ClearAndMacSetDataApdu();

                case EmSecurityMode.CiphertextMac:
                    return CiphAndMacSetDataApdu();

                default:
                    return string.Empty;
            }

        }

        /// <summary>
        /// 明文 设置 组应用程报文
        /// </summary>
        /// <returns></returns>
        private string ClearSetDataApduFrame()
        {

            ClearText = new SetByClearStructure.SetApduFrameClearText
            {
                SetRequest = ((int)EmServieType.SET_Request).ToString().PadLeft(2, '0'),
                DataInfo = new List<SetByClearStructure.DataInfos>()
            };
            if (OAD.Count > 1)
            {
                ClearText.SetRequestMode = ((int)EmSetRequestMode.SetRequestNormalList).ToString().PadLeft(2, '0');
                ClearText.OadNum = OAD.Count.ToString().PadLeft(2, '0');
            }
            else
            {
                ClearText.SetRequestMode = ((int)EmSetRequestMode.SetRequestNormal).ToString().PadLeft(2, '0');
                ClearText.OadNum = string.Empty;
            }
            ClearText.PIID = "01";
            for (int i = 0; i < OAD.Count; i++)
            {
                obatt = ObjectInfos.AttributeInfos.Find(e => e.Oad == OAD[i]);
                SetByClearStructure.DataInfos dataIOnfo = new SetByClearStructure.DataInfos
                {
                    OAD = OAD[i],
                    Data = CommFun.GroupDataToSend(obatt, SidMac.Data[OAD[i]])
                };
                ClearText.DataInfo.Add(dataIOnfo);
            }

            ClearText.TimeFlag = "00";

            return GroupClearText();


        }


        /// <summary>
        /// 明文+MAC 设置 组应用程报文
        /// </summary>
        /// <returns></returns>
        private string ClearAndMacSetDataApdu()
        {
            string ClearStr = ClearSetDataApduFrame();//包含明文部分          
            ClearMacText = new SetByClearMacStructure.SetApduClearMac
            {
                SecurtiyRequest = "10",//安全传输请求
                ClearMode = "00",  //明文传输
                ClearText = CommFun.GroupByteString(ClearStr),//明文长度+明文；
                DataValidateMode = ((int)EmDataValidationMode.SID_MAC).ToString().PadLeft(2, '0'),//数据验证模式
                ValidationInfo = SidMac.SID + CommFun.GroupByteString(SidMac.AttachData) + CommFun.GroupByteString(SidMac.MAC)//验证信息
            };
            return GroupClearMacFrame();
        }



        /// <summary>
        /// 密文+MAC 设置 组应用程报文
        /// </summary>
        /// <returns></returns>
        private string CiphAndMacSetDataApdu()
        {
            string Cipher = SidMac.Data[OAD[0]][0];
            CipherText = new SetByCipherMacStructure.SetApduCipherMac
            {
                SecurtiyRequest = "10",//安全传输请求
                CipherMode = "01",//密文模式
                CipherText = CommFun.GroupByteString(Cipher),//密文数据
                DataValidateMode = ((int)EmDataValidationMode.SID_MAC).ToString().PadLeft(2, '0'),//验证模式
                ValidationInfo = SidMac.SID + CommFun.GroupByteString(SidMac.AttachData) + CommFun.GroupByteString(SidMac.MAC)//验证信息
            };


            return GroupCiphMacFrame();
        }

        private string GroupClearText()
        {
            ClearSetDataStr = new StringBuilder();
            ClearSetDataStr.Append(ClearText.SetRequest);
            ClearSetDataStr.Append(ClearText.SetRequestMode);
            ClearSetDataStr.Append(ClearText.PIID);
            if (!string.IsNullOrEmpty(ClearText.OadNum)) { ClearSetDataStr.Append(ClearText.OadNum); }
            ClearText.DataInfo.ForEach(e =>
            {
                ClearSetDataStr.Append(e.OAD);
                ClearSetDataStr.Append(e.Data);
            }
                );
            ClearSetDataStr.Append(ClearText.TimeFlag);
            return ClearSetDataStr.ToString();

        }

        private string GroupClearMacFrame()
        {
            ClearAndMacSetDataStr = new StringBuilder();
            ClearAndMacSetDataStr.Append(ClearMacText.SecurtiyRequest);
            ClearAndMacSetDataStr.Append(ClearMacText.ClearMode);
            ClearAndMacSetDataStr.Append(ClearMacText.ClearText);
            ClearAndMacSetDataStr.Append(ClearMacText.DataValidateMode);
            ClearAndMacSetDataStr.Append(ClearMacText.ValidationInfo);
            return ClearAndMacSetDataStr.ToString();

        }
        private string GroupCiphMacFrame()
        {
            CiphAndMacSetDataStr = new StringBuilder();
            CiphAndMacSetDataStr.Append(CipherText.SecurtiyRequest);
            CiphAndMacSetDataStr.Append(CipherText.CipherMode);
            CiphAndMacSetDataStr.Append(CipherText.CipherText);
            CiphAndMacSetDataStr.Append(CipherText.DataValidateMode);
            CiphAndMacSetDataStr.Append(CipherText.ValidationInfo);
            return CiphAndMacSetDataStr.ToString();

        }

        /// <summary>
        /// 解析明文设置返回数据
        /// </summary>
        /// <param name="Frame">帧</param>
        /// <param name="ErrCode">错误代码</param>
        /// <returns>执行成功与否</returns>
        public override bool ParseSetFrame(byte[] Frame, ref int ErrCode)
        {
            EmSetRequestMode SetMode = (EmSetRequestMode)Frame[1];
            ErrCode = 0;
            if (SetMode == EmSetRequestMode.SetRequestNormal)
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
            else if (SetMode == EmSetRequestMode.SetRequestNormalList)
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
