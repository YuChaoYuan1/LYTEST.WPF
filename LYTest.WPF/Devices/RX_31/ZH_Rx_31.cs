using ZH.Rx_31.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZH
{
    public class ZH_Rx_31
    {
        /// <summary>
        /// 格式 "COM19"
        /// </summary>
        public string PortName = "";
        readonly Rx_31.Helper.CommunicationHelper ComHelper = new Rx_31.Helper.CommunicationHelper();

        public ZH_Rx_31(string portName)
        {
            ComHelper.Port = portName;
            ComHelper.Baudrate = "57600,n,8,1";
            ComHelper.OpenPort();
        }
        public ZH_Rx_31()
        {
            
        }

        public int InitSettingCom(int ComNumber, int MaxWaitTime, int WaitSencondsPerByte)
        {

            ComHelper.Port = "COM"+ComNumber.ToString();
            ComHelper.Baudrate = "57600,n,8,1";
            ComHelper.OpenPort();
            return 0;

        }

        /// <summary>
        /// 联机
        /// </summary>
        /// <returns></returns>
        public int Connect(out string[] FrameAry)
        {
            FrameAry = new string[1];
            var result = 1;
            try
            {
                ComHelper.SendDataHex("A6 00 00 00 00 A6");//==>A3 00 00 A3

                int maxwait = 5;
                while (maxwait > 0)
                {
                    System.Threading.Thread.Sleep(300);
                    if (ComHelper.ReceivedText.Replace(" ", "") == "A30000A3")
                    {
                        result = 0;
                        break;
                    }
                    else
                    {
                        result = -1;
                    }
                    maxwait--;
                }
         
            }
            catch 
            {

            }
            return result;

        }

        public float[] ReadStMeterInfo()
        {
            float[] resoule = new float[23];
            Rx_31.Model.StandarMeterInfo model = new Rx_31.Model.StandarMeterInfo();
            try
            {
              
                var Voltage = ReadMeterInstantaneous(MetricsType.V);
                resoule[0] = Voltage[0];
                resoule[1] = Voltage[1];
                resoule[2] = Voltage[2];

                var Current = ReadMeterInstantaneous(MetricsType.A);
                model.Ia = Current[0];
                model.Ib = Current[1];
                model.Ic = Current[2];
                resoule[3] = Current[0];
                resoule[4] = Current[1];
                resoule[5] = Current[2];

                var Pw = ReadMeterInstantaneous(MetricsType.W);
                model.Pa = Pw[0];
                model.Pb = Pw[1];
                model.Pc = Pw[2];
                resoule[6] = Pw[0];
                resoule[7] = Pw[1];
                resoule[8] = Pw[2];

                var Qw = ReadMeterInstantaneous(MetricsType.VAR);
                model.Qa = Qw[0];
                model.Qb = Qw[1];
                model.Qc = Qw[2];
                resoule[9] = Qw[0];
                resoule[10] = Qw[1];
                resoule[11] = Qw[2];


                var Sw = ReadMeterInstantaneous(MetricsType.VA);
                model.Sa = Sw[0];
                model.Sb = Sw[1];
                model.Sc = Sw[2];
                resoule[12] = Sw[0];
                resoule[13] = Sw[1];
                resoule[14] = Sw[2];

                var hz = ReadMeterInstantaneous(MetricsType.Hz);
                model.Freq = hz[0];
                resoule[15] = hz[0];

                var pf = ReadMeterInstantaneous(MetricsType.PF);
                model.COS = pf[0];
                resoule[16] = pf[0];

                var doP = ReadMeterInstantaneous(MetricsType.doP);
                resoule[17] = doP[0];
                model.Phi_Ua = doP[0];
                if (model.Phi_Ua < 0)
                {
                    resoule[17] = model.Phi_Ua + 360;
                    model.Phi_Ua += 360;
                }

                resoule[18] = doP[1];
                model.Phi_Ub = doP[1];
                if (model.Phi_Ub < 0)
                {
                    resoule[18] = model.Phi_Ub + 360;
                    model.Phi_Ub += 360;
                }
                resoule[19] = doP[2];
                model.Phi_Uc = doP[2];
                if (model.Phi_Uc < 0)
                {
                    resoule[19] = model.Phi_Uc + 360;
                    model.Phi_Uc += 360;
                }

                var oP = ReadMeterInstantaneous(MetricsType.oP);

                model.Phi_Ia = oP[0] + model.Phi_Ua;
                resoule[20] = oP[0] + model.Phi_Ua;

                if (model.Phi_Ia < 0)
                {
                    resoule[20] = model.Phi_Ia + 360;
                    model.Phi_Ia += 360;
                }
                model.Phi_Ib = oP[1] + model.Phi_Ub;
                resoule[21] = oP[1] + model.Phi_Ub;
                if (model.Phi_Ib < 0)
                {
                    resoule[21] = model.Phi_Ib + 360;
                    model.Phi_Ib += 360;
                }
                resoule[22] = oP[2] + model.Phi_Uc;
                model.Phi_Ic = oP[2] + model.Phi_Uc;
                if (model.Phi_Ic < 0) {
                    resoule[22] = model.Phi_Ic + 360;
                    model.Phi_Ic += 360;
                } 


            }
            catch 
            {


            }
            return resoule;
        }



        public Rx_31.Model.StandarMeterInfo ReadStMeterInfo2()
        {
            Rx_31.Model.StandarMeterInfo model = new Rx_31.Model.StandarMeterInfo();
            try
            {
                var Voltage = ReadMeterInstantaneous(MetricsType.V);
                model.Ua = Voltage[0];
                model.Ub = Voltage[1];
                model.Uc = Voltage[2];
                var Current = ReadMeterInstantaneous(MetricsType.A);
                model.Ia = Current[0];
                model.Ib = Current[1];
                model.Ic = Current[2];

                var Pw = ReadMeterInstantaneous(MetricsType.W);
                model.Pa = Pw[0];
                model.Pb = Pw[1];
                model.Pc = Pw[2];


                var Qw = ReadMeterInstantaneous(MetricsType.VAR);
                model.Qa = Qw[0];
                model.Qb = Qw[1];
                model.Qc = Qw[2];


                var Sw = ReadMeterInstantaneous(MetricsType.VA);
                model.Sa = Sw[0];
                model.Sb = Sw[1];
                model.Sc = Sw[2];
                var hz = ReadMeterInstantaneous(MetricsType.Hz);
                model.Freq = hz[0];

                var pf = ReadMeterInstantaneous(MetricsType.PF);
                model.COS = pf[0];

                var doP = ReadMeterInstantaneous(MetricsType.doP);

                model.Phi_Ua = doP[0];
                if (model.Phi_Ua < 0) model.Phi_Ua += 360;
                model.Phi_Ub = doP[1];
                if (model.Phi_Ub < 0) model.Phi_Ub += 360;
                model.Phi_Uc = doP[2];
                if (model.Phi_Uc < 0) model.Phi_Uc += 360;

                var oP = ReadMeterInstantaneous(MetricsType.oP);

                model.Phi_Ia = oP[0] + model.Phi_Ua;
                if (model.Phi_Ia < 0) model.Phi_Ia += 360;
                model.Phi_Ib = oP[1] + model.Phi_Ub;
                if (model.Phi_Ib < 0) model.Phi_Ib += 360;
                model.Phi_Ic = oP[2] + model.Phi_Uc;
                if (model.Phi_Ic < 0) model.Phi_Ic += 360;


            }
            catch 
            {


            }
            return model;
        }





        /// <summary>
        /// 读取标准表瞬时值
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        private float[] ReadMeterInstantaneous(MetricsType index)
        {
            float[] Values = new float[5];//PhaseA|PhaseB|PhaseC|Reserved|Net
            try
            {
                var ttt = (int)index;
                string BasicSend = "A6 2E 00 04 00{0} FF FD ";
               string SendData = MakeSendData(string.Format(BasicSend,Convert.ToString( ((int)index),16).PadLeft(2, '0')));
               // string SendData = MakeSendData(string.Format(BasicSend,"00"));

                ComHelper.SendDataHex(SendData);//==>A3 00 00 A3

                int maxwait = 5;
                while (maxwait > 0)
                {
                    System.Threading.Thread.Sleep(300);
                    if (ComHelper.ReceivedText != string.Empty)
                    {
                        var replay = ComHelper.ReceivedText.Replace(" ","");
                        if (replay.Length == 52)
                        {
                            var analyze = MakeStvalue(replay.Substring(8, 40));
                            Values[0] = analyze[0];
                            Values[1] = analyze[1];
                            Values[2] = analyze[2];
                            Values[3] = analyze[3];
                            Values[4] = analyze[4];
                            break;
                        }
                    }
               
                    maxwait--;
                }

            }
            catch 
            {


            }

            return Values;
        }
      



        /// <summary>
        /// 1D设置方式
        /// </summary>
        /// <param name="BncCode">256输出端口</param>
        /// <param name="MetricIndex">脉冲类型</param>
        /// <param name="aseCode">相别</param>
        public void SetPuase(string BncCode ,string MetricIndex,string aseCode)
        {
            string strhead = "A61D0003";
            string strBncCode = BncCode.PadLeft(2, '0');
            string strBncDate = MetricIndex.PadLeft(2, '0');
            string phaseCode = aseCode.PadLeft(2, '0');
            string strsend = MakeSendData(strhead + strBncCode + strBncDate + phaseCode);
            ComHelper.SendDataHex(strsend);
            int maxwait = 5;
            while (maxwait > 0)
            {
                System.Threading.Thread.Sleep(300);
                if (ComHelper.ReceivedText != string.Empty)
                {
                    //ACh 1Dh 0002h 000Ah xxxxh
                    var replay = ComHelper.ReceivedText.Split(' ');
                    if (replay.Length == 26)
                    {
                        //var analyze = makeStvalue(ComHelper.ReceivedText.Replace(" ", "").Substring(8, 40));
                        
                        break;
                    }
                }

                maxwait--;
            }
        }
        /// <summary>
        /// 设置常数
        /// </summary>
        /// <param name="MetricIndex"></param>
        /// <param name="Constant"></param>
        public void SetConstant(string MetricIndex ,float Constant)
        {
            byte[] byteConstant=  FloatToBytesTMS320(1/Constant);
            string strhead= "A6320007";
            string strMetricIndex ="00"+ MetricIndex;
            string strStatus = "00";
            string strConstant = byteConstant[0].ToString("x2") + byteConstant[1].ToString("x2") + byteConstant[2].ToString("x2") + byteConstant[3].ToString("x2");
            string strsend = MakeSendData(strhead + strMetricIndex + strStatus + strConstant);
            ComHelper.SendDataHex(strsend);
            int maxwait = 5;
            while (maxwait > 0)
            {
                System.Threading.Thread.Sleep(300);
                if (ComHelper.ReceivedText != string.Empty)
                {
                    //A3h 32h 0004h Pulse OutputConstant ofgiven MetricIndex
                    var replay = ComHelper.ReceivedText.Split(' ');
                    if (replay.Length == 26)
                    {
                        //var analyze = makeStvalue(ComHelper.ReceivedText.Replace(" ", "").Substring(8, 40));

                        break;
                    }
                }

                maxwait--;
            }
          
        }




        /// <summary>
        /// 停止走字
        /// </summary>
        public void StopAccumulating(   )
        {
            //A6h 09h 0000h 00AFh
            string str = "A609000000AF";
            string strsend = MakeSendData(str);
            ComHelper.SendDataHex(strsend);
            int maxwait = 5;
            while (maxwait > 0)
            {
                System.Threading.Thread.Sleep(300);
                if (ComHelper.ReceivedText != string.Empty)
                {
                    //ACh 1Dh 0002h 000Ah xxxxh
                    var replay = ComHelper.ReceivedText.Split(' ');
                    if (replay.Length == 26)
                    {
                        //var analyze = makeStvalue(ComHelper.ReceivedText.Replace(" ", "").Substring(8, 40));

                        break;
                    }
                }

                maxwait--;
            }
        }
        /// <summary>
        /// 开始走字
        /// </summary>
        /// <param name="MetricIndex"></param>
        public void StartAccumulating()
        {
            //A6 08 0003 06 0000
            string str = "A6080003010000";
            string strsend = MakeSendData(str);
            ComHelper.SendDataHex(strsend);
            int maxwait = 5;
            while (maxwait > 0)
            {
                System.Threading.Thread.Sleep(300);
                if (ComHelper.ReceivedText != string.Empty)
                {
                    //ACh 1Dh 0002h 000Ah xxxxh
                    var replay = ComHelper.ReceivedText.Split(' ');
                    if (replay.Length == 26) 
                    {
                        //var analyze = makeStvalue(ComHelper.ReceivedText.Replace(" ", "").Substring(8, 40));

                        break;
                    }
                }

                maxwait--;
            }
        }
        /// <summary>
        /// 读取走字电量
        /// </summary>
        /// <param name="MetricIndex"></param>
        public float[] ReadAccumulating()
        {
            //A6h 2Fh 0002h Accumulating Metric Index xxxxh
            string strhead = "A62F0002";
            
            string strBncDate ="00" +"00";
            float[] Values = new float[5];
            string strsend = MakeSendData(strhead  + strBncDate );
            ComHelper.SendDataHex(strsend);
            int maxwait = 5;
            //System.Threading.Thread.Sleep(1000);
            while (maxwait > 0)
            {
                System.Threading.Thread.Sleep(300);
                if (ComHelper.ReceivedText != string.Empty)
                {
                    //ACh 1Dh 0002h 000Ah xxxxh
                    var replay = ComHelper.ReceivedText.Replace(" ","");
                    string strCmd = replay.Substring(2, 2);
                    if (replay.Length == 52 || replay.Length == 104)
                    {
                        if (strCmd == "2F")
                        {
                            var a = MakeStvalue(replay.Substring(8, 40));
                            Values[0] = a[0] / 1000;
                            Values[1] = a[1] / 1000;
                            Values[2] = a[2] / 1000;
                            Values[3] = a[3] / 1000;
                            Values[4] = a[4] / 1000;
                        }
                       //string ss= ComHelper.ReceivedText.Replace(" ", "");
                        break;
                    }
                }

                maxwait--;
            }
           return  Values;
        }


        // <summary>
        /// IEEE浮点转
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static byte[] FloatToBytesTMS320(float value)
        {

            byte[] ret = new byte[4];

            uint uValue;
            uint mantissa;
            uint exponent;
            bool negMantissa; // 





            //float data = value;

            /////int data2 =300;

            ////MessageBox.Show("EEPROM Data: " + Convert.ToString(data2, 16));
            //int nValue = 0;


            //int nSign;
            //if (data >= 0)
            //{
            //    nSign = 0x00;

            //}
            //else
            //{
            //    nSign = 0x01;

            //    data = data * (-1);
            //}
            //int nHead = (int)data;
            //float fTail = data % 1;
            //String str = Convert.ToString(nHead, 2);
            //int nHead_Length = str.Length;

            //nValue = nHead;

            //int nShift = nHead_Length;
            //while (nShift < 24)   // (nHead_Length + nShift < 23)
            //{
            //    if ((fTail * 2) >= 1)
            //    {
            //        nValue = (nValue << 1) | 0x00000001;
            //    }
            //    else
            //    {
            //        nValue = (nValue << 1);
            //    }
            //    fTail = (fTail * 2) % 1;
            //    nShift++;
            //}

            //int nExp = nHead_Length - 1 + 127;
            //nExp = nExp << 23;
            //nValue = nValue & 0x7FFFFF;
            //nValue = nValue | nExp;
            //nSign = nSign << 31;
            //nValue = nValue | nSign;

            //int data1, data2, data3, data4;
            //data1 = nValue & 0x000000FF;
            //data2 = (nValue & 0x0000FF00) >> 8;
            //data3 = (nValue & 0x00FF0000) >> 16;
            //data4 = (nValue >> 24) & 0x000000FF;

            //if (data == 0)
            //{
            //    data1 = 0x00;
            //    data2 = 0x00;
            //    data3 = 0x00;
            //    data4 = 0x00;
            //}

            //ret[0]=Convert.ToByte(data1);
            //ret[1] = Convert.ToByte(data2); ;
            //ret[2] = Convert.ToByte(data3); ;
            //ret[3] = Convert.ToByte(data4); ;


            //return ret;
            
            uValue =BitConverter.ToUInt32(BitConverter.GetBytes(value), 0); // 
           //byteData.CopyTo(BitConverter.GetBytes(value), 4);
            //uValue = BitConverter.ToUInt32(BitConverter.GetBytes(value), 0); // 
            exponent = (uValue >> 23) & 0xFF;
            mantissa = uValue & 0x7FFFFF;
            negMantissa = (uValue & 0x80000000) != 0;
            // Special cases
            if (exponent == 0xFF) // NAN
            {
                exponent = 0x7F;
                mantissa = (uint)(negMantissa ? 0 : 0x7FFFFF); // 
            }
            else if (exponent == 0) // 
            {
                exponent = 0x80;
                negMantissa = false;
                mantissa = 0;
            }
            else
            {
                if (negMantissa) // 
                {
                    if (mantissa != 0x0)
                    {
                        mantissa = ~mantissa + 1; // 
                        exponent -= 0x7f;
                    }
                    else
                        exponent -= 0x80;
                }
                else
                    exponent -= 0x7f;
            }
            // Pack number into bytes
            ret[0] = (byte)exponent;
            ret[1] = (byte)((mantissa >> 16) & 0x7f);
            if (negMantissa)
                ret[1] |= (byte)0x80;
            ret[2] = (byte)((mantissa >> 8) & 0xff);
            ret[3] = (byte)(mantissa & 0xff);

            return ret;
        }

        #region TOOL
        private  string MakeSendData(string sendData)
        {
            string result = sendData;

            sendData = sendData.Replace(" ", "");
            byte[] x = new byte[sendData.Length / 2];
            for (int k = 0; k < sendData.Length; k += 2)
            {
                x[k / 2] = (byte)Convert.ToInt32(sendData.Substring(k, 1) + sendData.Substring(k + 1, 1), 16);
            }
            var chkmun = CalcCheckNum(x);
            result += Convert.ToString(chkmun, 16).PadLeft(4, '0');

            return result;
        }
        private  int CalcCheckNum(byte[] sendata)
        {
            int result = 0;
            for (int i = 0; i < sendata.Length; i++)
            {
                result += sendata[i] & 0xff;
            }


            return result;
        }
        public  List<float> MakeStvalue(string code)
        {
            List<float> result = new List<float>();
            //7F7F FF FF7F7F FFFF0B 4E 4B 18 0B 4E 4B 20 00 04 B3 F5 7F 7FFF FF 7F7F FF FFFF 7F FF F6 7F 7F FFFF 7F 7F FF FF 7F 7F FF FF 0B 4E 4D 2E 0B 4E 4D 2F FF 78 CF 2A 00 04 B1 F9 FF4C 7A A9 

            //F76DCCCD F541 478DF968F0DF 7F7F FFFF80000000    0C 9B
            var codelist = code;
            codelist = codelist.Replace(" ", "");
            for (int i = 0; i < codelist.Length; i += 8)
            {
                byte[] xx = new byte[4];//F3 36 30 DB
                //(codelist[i]+ codelist[i+1])
                //var kk = codelist[i];
                //int sg = Convert.ToInt32((codelist.Substring(i, 1) + codelist.Substring(i + 1, 1)).ToString(), 16);
                xx[0] = (byte)Convert.ToInt32((codelist.Substring(i, 1) + codelist.Substring(i + 1, 1)).ToString(), 16);
                xx[1] = (byte)Convert.ToInt32((codelist.Substring(i, 1 + 2) + codelist.Substring(i + 3, 1)).ToString(), 16);
                xx[2] = (byte)Convert.ToInt32((codelist.Substring(i, 1 + 4) + codelist.Substring(i + 5, 1)).ToString(), 16);
                xx[3] = (byte)Convert.ToInt32((codelist.Substring(i, 1 + 6) + codelist.Substring(i + 7, 1)).ToString(), 16);

                //if (xx[0] == 0x80 || xx[0] == 0x81)
                //{
                //    result.Add("0");
                //    continue;
                //}

                //if ((xx[0] == 0x7f) && (xx[1] == 0x7f) && (xx[2] == 0xff) && (xx[3] == 0xff))
                //{
                //    result.Add("FLT_MAX");
                //    continue;

                //}
                //var mantissa=


                float fy = ByteToFloatTMS320(xx, 0, 4)[0];


                result.Add(fy);

            }


            return result;

        }
        public  float[] ByteToFloatTMS320(byte[] data, int startPos, int count)
        {
            if (0 == count || 0 != count % 4)
                throw new Exception("待转换的字节数必须是4的倍数");

            if (startPos + count > data.Length)
                throw new Exception("待转换的数组长度不够");

            float[] ret = new float[count / 4];

            for (int i = 0; i < ret.Length; i++)
            {
                int offset = i * 4 + startPos;

                //指数-128、-127
                if ((0x80 == data[offset + 0]) || (0x81 == data[offset + 0]))
                {
                    ret[i] = 0.0F;
                    continue;
                }

                //指数127，最大值
                if ((0x7F == data[offset + 0])
                    && (0x7F == data[offset + 1])
                    && (0xFF == data[offset + 2])
                    && (0xFF == data[offset + 3]))
                {
                    ret[i] = float.MaxValue;
                    continue;
                }

                uint mantissa = (((uint)data[offset + 1] & 0x7F) << 16) |
                               (((uint)data[offset + 2] & 0xFF) << 8) |
                               ((uint)data[offset + 3] & 0xFF);

                bool negMantissa = (data[offset + 1] & 0x80) != 0;

                if ((0x7F == (byte)data[offset + 0]) && negMantissa && (0 == mantissa))
                {
                    ret[i] = float.MinValue;
                    continue;
                }

                uint value;
                uint exponent = (byte)(data[offset + 0] & 0xFF);

                if (false == negMantissa)
                {
                    value = (((exponent + 0x7F) << 23) | mantissa) & 0x7FFFFFFF;
                }
                else if (0 != mantissa)
                {
                    value = ((~mantissa) + 1) & 0x7FFFFF;
                    value |= 0x80000000 | ((exponent + 0x7F) << 23);
                }
                else
                {
                    value = 0x80000000 | ((exponent + 0x80) << 23);
                }


                ret[i] = BitConverter.ToSingle(BitConverter.GetBytes(value), 0);
            }

            for (int i = 0; i < data.Length; i++)
            {
                data[i] = 0;
            }
            return ret;
        }
        #endregion
    }
}
