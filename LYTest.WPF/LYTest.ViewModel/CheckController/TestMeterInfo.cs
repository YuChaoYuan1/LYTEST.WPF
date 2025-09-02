using LYTest.Core;
using LYTest.Core.Model.Meter;
using System;
using System.Collections.Generic;
using LYTest.MeterProtocol.Comm;
using LYTest.MeterProtocol.Protocols.DgnProtocol;

namespace LYTest.ViewModel.CheckController
{
    /// <summary>
    /// 电表帮助类
    /// </summary>
    public class MeterHelper : SingletonBase<MeterHelper>
    {
        /// <summary>
        /// 电能表类型列表，每种类型一个元素。每个元素中包括当前类型电能表的表位
        /// </summary>
        private readonly Dictionary<string, string> m_MeterTypeList = new Dictionary<string, string>();
        /// <summary>
        /// 电能表协议分类，每种类型一个元素.其中包括当前协议类型的表位号
        /// </summary>
        private readonly Dictionary<string, string> m_MeterProtocolList = new Dictionary<string, string>();
        /// <summary>
        /// 电能表所使用的协议列表
        /// </summary>
        private DgnProtocolInfo[] m_MeterProtocols = null;
        /// <summary>
        /// 所有电能表的协议
        /// </summary>
        /// <returns></returns>
        public DgnProtocolInfo[] GetAllProtocols()
        {
            return m_MeterProtocols;
        }

        /// <summary>
        /// 初始化电表协议
        /// </summary>
        /// <returns></returns>
        public bool Init()
        {
            m_MeterTypeList.Clear();
            int MeterNumber = EquipmentData.Equipment.MeterCount;//表位数量
            m_MeterProtocols = new DgnProtocolInfo[MeterNumber];
            for (int i = 0; i < MeterNumber; i++)
            {
                //根据每个表的协议名称，载入对应的表协议 
                m_MeterProtocols[i] = new DgnProtocolInfo(VerifyBase.MeterInfo[i].MD_ProtocolName);
                if (VerifyBase.IsDoubleProtocol)
                {
                    m_MeterProtocols[i].Setting = "9600,e,8,1";
                }

                VerifyBase.MeterInfo[i].DgnProtocol = m_MeterProtocols[i];

                if (!VerifyBase.MeterInfo[i].YaoJianYn) continue;

                //VerifyBase.meterInfo[i].DgnProtocol.Load();

                string key1 = string.Format("{0}_{1}", VerifyBase.MeterInfo[i].MD_Grane, VerifyBase.MeterInfo[i].MD_Constant);
                if (m_MeterTypeList.ContainsKey(key1))
                    m_MeterTypeList[key1] += "|" + i.ToString();
                else
                    m_MeterTypeList.Add(key1, i.ToString());

                //协议
                string key2 = VerifyBase.MeterInfo[i].DgnProtocol.ProtocolName;
                if (!string.IsNullOrEmpty(key2))
                {
                    if (m_MeterProtocolList.ContainsKey(key2))
                        m_MeterProtocolList[key2] += "|" + i.ToString();
                    else
                        m_MeterProtocolList.Add(key2, i.ToString());
                }

            }
            return true;
        }


        /// <summary>
        /// 初始化载波协议
        /// </summary>
        /// <returns></returns>
        public bool InitCarrier()
        {
            for (int i = 0; i < EquipmentData.Equipment.MeterCount; i++)
            {
                if (!VerifyBase.MeterInfo[i].YaoJianYn) continue;
                LYTest.MeterProtocol.App.CarrierInfos[i] = CarrierList.Load(VerifyBase.MeterInfo[i].MD_CarrName);
            }
            //LYTest.MeterProtocol.App.Protocols=VerifyBase.OneMeterInfo.

            return true;
        }

        /// <summary>
        /// 获取所有电能表的通讯地址
        /// </summary>
        /// <returns></returns>
        public string[] GetMeterAddress()
        {
            if (VerifyBase.MeterInfo == null)
                throw new Exception("需要在检定前设置被检表数据");

            List<string> list = new List<string>();

            for (int i = 0; i < VerifyBase.MeterInfo.Length; i++)
            {
                if (!string.IsNullOrWhiteSpace(VerifyBase.MeterInfo[i].MD_PostalAddress))
                    list.Add(VerifyBase.MeterInfo[i].MD_PostalAddress);
                else
                    list.Add(VerifyBase.MeterInfo[i].MD_MadeNo);
            }
            return list.ToArray();
        }

        public ComPortInfo[] GetComPortInfo()
        {
            int len = VerifyBase.MeterInfo.Length;
            ComPortInfo[] comPorts = new ComPortInfo[len];
            for (int i = 0; i < len; i++)
            {
                comPorts[i] = VerifyBase.MeterInfo[i].ProtInfo;
            }
            return comPorts;
        }


        #region ----------表类型管理----------


        /// <summary>
        /// 返回电能表类型数量
        /// </summary>
        public int TypeCount
        {
            get
            {
                return m_MeterTypeList.Count;
            }
        }

        /// <summary>
        /// 取要检表的数量
        /// </summary>
        public int TestMeterCount
        {
            get
            {
                int mcount = 0;
                for (int i = 0; i < TypeCount; i++)
                {
                    mcount += MeterType(i).Length;
                }
                return mcount;
            }
        }

        /// <summary>
        /// 获取一种类型的表
        /// </summary>
        /// <param name="iType">第几种类型，从0开始</param>
        /// <returns></returns>
        public string[] MeterType(int iType)
        {
            if (iType >= 0 && iType < TypeCount)
            {
                int i = 0;
                foreach (string strKey in m_MeterTypeList.Keys)
                {
                    if (i == iType)
                    {
                        return m_MeterTypeList[strKey].Split('|');
                    }
                    i++;
                }
            }
            return new string[] { };
        }

        #endregion

        #region ----------表数据统计----------
        /// <summary>
        /// 统计要检表列表
        /// </summary>
        /// <returns></returns>
        public bool[] GetYaoJian()
        {
            bool[] arrResult = new bool[VerifyBase.MeterInfo.Length];
            for (int i = 0; i < VerifyBase.MeterInfo.Length; i++)
            {
                arrResult[i] = VerifyBase.MeterInfo[i].YaoJianYn;
            }
            return arrResult;
        }
        /// <summary>
        /// 统计要检表列表
        /// </summary>
        /// <returns></returns>
        public bool[] GetYaoJianSave()
        {
            bool[] arrResult = new bool[VerifyBase.MeterInfo.Length];
            for (int i = 0; i < VerifyBase.MeterInfo.Length; i++)
            {
                arrResult[i] = VerifyBase.MeterInfo[i].YaoJianYn;
            }
            return arrResult;
        }

        /// <summary>
        /// 获取当前检定表中最小表常数
        /// </summary>
        /// <param name="bYouGong">是否是有功常数</param>
        /// <returns>当前被检表中最小表常数</returns>
        public float MeterConstMin(bool bYouGong)
        {
            float[] consts = new float[VerifyBase.MeterInfo.Length];
            for (int i = 0; i < VerifyBase.MeterInfo.Length; i++)
            {
                string constant = VerifyBase.MeterInfo[i].MD_Constant;
                if (!VerifyBase.MeterInfo[i].YaoJianYn)
                {
                    constant = VerifyBase.OneMeterInfo.MD_Constant;
                }
                if (constant.IndexOf("(") > 0)   //判断是否有分有功无功
                {
                    string[] str = constant.TrimEnd(')').Split('(');
                    if (bYouGong)
                    {
                        consts[i] = int.Parse(str[0]);
                    }
                    else
                    {
                        consts[i] = int.Parse(str[1]);

                    }
                }
                else
                {
                    consts[i] = float.Parse(constant);
                }
            }
            Core.Function.Number.PopDesc(ref consts, true);
            return consts[0];
        }
        /// <summary>
        /// 取当前检定所有表中最小表常数
        /// </summary>
        /// <returns>包括二个元素的数组，第一个元素为有功最小常数，第二个为无功</returns>
        public int[] MeterConstMin()
        {
            int[] mconst = new int[2];
            mconst[0] = (int)MeterConstMin(true);
            mconst[1] = (int)MeterConstMin(false);
            return mconst;
        }


        /// <summary>
        /// 获取所有表的常数
        /// </summary>
        /// <returns></returns>
        public int[] MeterConst(bool bYouGong)
        {
            List<int> cList = new List<int>();

            for (int i = 0; i < VerifyBase.MeterInfo.Length; i++)
            {
                TestMeterInfo m = VerifyBase.MeterInfo[i];
                if (!m.YaoJianYn)
                    m = VerifyBase.OneMeterInfo;

                int[] cs = m.GetBcs();
                int bcs = cs[bYouGong ? 0 : 1];
                cList.Add(bcs);
            }
            return cList.ToArray();
        }

        /// <summary>
        /// 取要检表的数量
        /// </summary>
        public int YaoJianMeterCount
        {
            get
            {
                int mcount = 0;
                for (int i = 0; i < TypeCount; i++)
                {
                    mcount += MeterType(i).Length;
                }
                return mcount;
            }
        }
        #endregion

        #region ----------协议类型管理----------
        /// <summary>
        /// 协议类型数量
        /// </summary>
        public int ProtocolCount
        {
            get { return m_MeterProtocolList.Count; }
        }

        /// <summary>
        /// 使用当前协议的电能表
        /// </summary>
        /// <param name="iType"></param>
        /// <returns></returns>
        public string[] ProtocolType(int iType)
        {
            if (iType >= 0 && iType < ProtocolCount)
            {
                int i = 0;
                foreach (string k in m_MeterProtocolList.Keys)
                {
                    if (i == iType)
                    {
                        return m_MeterProtocolList[k].Split('|');
                    }
                    i++;
                }
            }
            return new string[] { };
        }


        #endregion
        /// <summary>
        /// 检查总结论
        /// </summary>
        /// <returns></returns>
        public bool GetResult()
        {
            bool Result = true;
            for (int i = 0; i < VerifyBase.MeterInfo.Length; i++)
            {
                if (VerifyBase.MeterInfo[i].YaoJianYn)
                {
                    if (!EquipmentData.CheckResults.GetMeterResult(i))
                    {
                        Result = false;
                        break;
                    }

                }
            }
            return Result;
        }
    }



}
