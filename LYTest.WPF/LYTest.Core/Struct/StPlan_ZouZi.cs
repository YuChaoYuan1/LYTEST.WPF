using LYTest.Core.Enum;
using System;
using System.Collections.Generic;

namespace LYTest.Core.Struct
{
    /// <summary>
    /// 走字方案项目
    /// </summary>
    [Serializable()]
    public struct StPlan_ZouZi
    {

        private string _PrjID;
        /// <summary>
        /// 项目编号
        /// </summary>
        public string PrjID
        {
            get
            {
                return _PrjID;
            }
            set
            {
                _PrjID = value;
                this.PowerFangXiang = (PowerWay)int.Parse(_PrjID.Substring(0, 1));
                this.PowerYj = (Cus_PowerYuanJian)int.Parse(_PrjID.Substring(1, 1));
                //this.FeiLv = (Comm.Enum.Cus_FeiLv)int.Parse(_PrjID.Substring(6, 1));
            }
        }

        /// <summary>
        /// 功率方向
        /// </summary>
        public PowerWay PowerFangXiang;

        /// <summary>
        /// 元件
        /// </summary>
        public Cus_PowerYuanJian PowerYj;
        /// <summary>
        /// 检定方法
        /// </summary>
        public Cus_ZouZiMethod ZouZiMethod;

        /// <summary>
        /// 电流倍数(Imax)
        /// </summary>
        public string xIb;
        /// <summary>
        /// 获取转化后的电流倍数
        /// </summary>
        public string GetxIb
        {
            get
            {
                string xib = "00";
                switch (xIb.ToUpper())
                {
                    case "IMAX":
                        xib = "01";
                        break;
                    case "0.5IMAX":
                        xib = "02";
                        break;
                    case "3.0IB":
                        xib = "03";
                        break;
                    case "2.0IB":
                        xib = "04";
                        break;
                    case "1.5IB":
                        xib = "05";
                        break;
                    case "0.5(IMAX-IB)":
                        xib = "06";
                        break;
                    case "1.0IB":
                        xib = "07";
                        break;
                    case "0.8IB":
                        xib = "08";
                        break;
                    case "0.5IB":
                        xib = "09";
                        break;
                    case "0.2IB":
                        xib = "10";
                        break;
                    case "0.1IB":
                        xib = "11";
                        break;
                    case "0.05IB":
                        xib = "12";
                        break;
                    case "0.02IB":
                        xib = "13";
                        break;
                    case "0.01IB":
                        xib = "14";
                        break;
                    default:
                        break;

                }
                return xib;
            }
        }

        /// <summary>
        /// 功率因数
        /// </summary>
        public string Glys;
        /// <summary>
        /// 获取转化后的功率因数
        /// </summary>
        public string GetGlys
        {
            get
            {
                string glys = "00";
                switch (Glys.TrimStart('-').ToUpper())
                {
                    case "1.0":
                        glys = "01";
                        break;
                    case "0.5L":
                        glys = "02";
                        break;
                    case "0.8C":
                        glys = "03";
                        break;
                    case "0.5C":
                        glys = "04";
                        break;
                    case "0.8L":
                        glys = "05";
                        break;
                    case "0.25L":
                        glys = "06";
                        break;
                    case "0.25C":
                        glys = "07";
                        break;
                    default:
                        break;

                }
                return glys;
            }
        }
        /// <summary>
        /// 项目费率，只有在CreateFA后才生效
        /// </summary>
        public Enum.Cus_FeiLv FeiLv;
        /// <summary>
        /// 获取itemkey值
        /// </summary>
        public string ItemKey
        {
            get
            {
                return $"{(int)PowerFangXiang}{(int)PowerYj}{GetGlys}{GetxIb}{(int)FeiLv}";
            }
        }
        /// <summary>
        /// 开始时间，只有在加载方案后生效
        /// </summary>
        public string StartTime;

        /// <summary>
        /// 走字时间，只有在加载方案后生效
        /// </summary>
        public float UseMinutes;

        /// <summary>
        /// 费率
        /// </summary>
        public string FeiLvString;
        /// <summary>
        /// 是否做组合误差，不做为0，做为1
        /// </summary>
        public string ZuHeWc;
        /// <summary>
        /// 走字项目
        /// </summary>
        public List<StPrjFellv> ZouZiPrj;
        /// <summary>
        /// 走字项目描述
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            try
            {
                return $"{PowerFangXiang}走字:费率{FeiLv}";
            }
            catch
            {
                return $"{PowerFangXiang}走字";
            }
        }


        /// <summary>
        /// 走字分项
        /// </summary>
        public struct StPrjFellv
        {
            /// <summary>
            /// 走字费率
            /// </summary>
            public Cus_FeiLv FeiLv;
            /// <summary>
            /// 费率起始时间
            /// </summary>
            public string StartTime;
            /// <summary>
            /// 走字时长
            /// </summary>
            public string ZouZiTime;

        }

    }
}
