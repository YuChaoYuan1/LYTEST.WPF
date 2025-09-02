using System;

namespace LYTest.Core.Model.DnbModel.DnbInfo
{
    /// <summary>
    /// 特殊检定项目结构 影响量
    /// </summary>
    [Serializable()]
    public class MeterSpecialErr : MeterError
    {

        public MeterSpecialErr()
        {
            Ub = "0|0|0";
            Ib = "0|0|0";
            Phase = "1.0";
            ErrLimit = "0|0";
            ErrValue = "";
            PowerWay = 1;
        }

        /// <summary>
        /// 43项目名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 方案信息
        /// </summary>
        public string TestValue { get; set; }

        /// <summary>
        /// 5功率方向1-P+，2-P-,3-Q+，4-Q-
        /// </summary>
        public int PowerWay { get; set; }

        /// <summary>
        /// Ua|Ub|Uc(试验电压值，非倍数)
        /// </summary>
        public string Ub { get; set; }
        /// <summary>
        /// Ia|Ib|Ic(试验电流，非倍数)
        /// </summary>
        public string Ib { get; set; }
        /// <summary>
        /// Ua,Ub,Uc|Ia,Ib,Ic(相位)（目前填写功率因数值）
        /// </summary>
        public string Phase { get; set; }//"0,240,120|0,240,120";

        /// <summary>
        /// 频率
        /// </summary>
        public string Pl = "50";
        /// <summary>
        /// 22频率倍数
        /// </summary>
        public int PlX = 1;
        /// <summary>
        /// 20    0-正相序，1-逆相序
        /// </summary>
        public int IsNxx = 0;
        /// <summary>
        /// 19   0-不加谐波，1-加谐波
        /// </summary>
        public int ContaintXieBo = 0;

        /// <summary>
        /// 31A相电压（根据方案倍数换算的值）
        /// </summary>
        public string Ua { get; set; }
        /// <summary>
        /// 32B相电压（根据方案倍数换算的值）
        /// </summary>
        public string AVR_VOT_B { get; set; }
        /// <summary>
        /// 33C相电压（根据方案倍数换算的值）
        /// </summary>
        public string AVR_VOT_C { get; set; }
        /// <summary>
        /// 34A相电流
        /// </summary>
        public string AVR_CUR_A { get; set; }
        /// <summary>
        /// 35B相电流
        /// </summary>
        public string AVR_CUR_B { get; set; }
        /// <summary>
        /// 36C相电流
        /// </summary>
        public string AVR_CUR_C { get; set; }
        /// <summary>
        /// 8A相电流倍数的字符串（IB、IMAX）例如：0.5IB、1.2IMAX
        /// </summary>
        public string AVR_CUR_A_MULTIPLE_STRING { get; set; }

        /// <summary>
        /// 10B相电流倍数的字符串（IB、IMAX）
        /// </summary>
        public string AVR_CUR_B_MULTIPLE_STRING { get; set; }

        /// <summary>
        /// 12C相电流倍数的字符串（IB、IMAX）
        /// </summary>
        public string AVR_CUR_C_MULTIPLE_STRING { get; set; }
        /// <summary>
        /// 37A相电压夹角
        /// </summary>
        public float UaPhi = 1F;
        /// <summary>
        /// 38B相电压夹角
        /// </summary>
        public float UbPhi = 1F;
        /// <summary>
        /// 39C相电压夹角
        /// </summary>
        public float UcPhi = 1F;
        /// <summary>
        /// 40A相电流夹角
        /// </summary>
        public float IaPhi = 1F;
        /// <summary>
        /// 41B相电流夹角
        /// </summary>
        public float IbPhi = 1F;
        /// <summary>
        /// 42C相电流夹角
        /// </summary>
        public float IcPhi = 1F;

        /// <summary>
        /// 误差限（上限|下限）
        /// </summary>
        public string ErrLimit { get; set; }

        /// <summary>
        /// 15.误差上限
        /// </summary>
        public string ErrLimitUp { get; set; }

        /// <summary>
        /// 误差下限
        /// </summary>
        public string ErrLimitDown { get; set; }

        /// <summary>
        /// 参比点误差值（误差一|误差二|...|误差平均|误差化整）
        /// </summary>
        public string Error1 = "";

        /// <summary>
        /// 带影响量误差值（误差一|误差二|...|误差平均|误差化整）
        /// </summary>
        public string Error2 = "";

        ///// <summary>
        ///// 高次谐波影响量误差值集合（误差一|误差二|...|误差平均|误差化整）
        ///// </summary>
        //public string[] ErrList { get; set; }
        //public List<HarmonicItem> ErrList { get; set; }

        /// <summary>
        /// 23误差化整值
        /// </summary>
        public string ErrInt = "";
        /// <summary>
        /// 24误差平均值
        /// </summary>
        public string ErrAvg = "";

        /// <summary>
        /// 28变差值
        /// </summary>
        public string ErrValue { get; set; }


        /// <summary>
        /// 29负载点类型，默认0，非基准点，1：基准点。
        /// </summary>
        public string CHR_BASE_LOAD_FLAG { get; set; }

        /// <summary>
        /// 30标准点编号[1,N] 
        /// </summary>
        public string AVR_BASE_LOAD_NO { get; set; }


        /// <summary>
        /// 14A电压倍数
        /// </summary>
        public string AVR_VOT_A_MULTIPLE { get; set; }
        /// <summary>
        /// 14B电压倍数
        /// </summary>
        public string AVR_VOT_B_MULTIPLE { get; set; }

        /// <summary>
        /// 15C电压倍数
        /// </summary>
        public string AVR_VOT_C_MULTIPLE { get; set; }

        /// <summary>
        /// 9B电流倍数
        /// </summary>
        public string AVR_CUR_A_MULTIPLE { get; set; }
        /// <summary>
        /// 9B电流倍数
        /// </summary>
        public string AVR_CUR_B_MULTIPLE { get; set; }

        /// <summary>
        /// 11C电流倍数
        /// </summary>
        public string AVR_CUR_C_MULTIPLE { get; set; }

        /// <summary>
        /// 打印报表数据-结论|平均值|化整值|误差1|误差2
        /// </summary>
        public string PrintData
        {
            get
            {
                string value = Result;
                string[] arr = Error2.Split('|');
                if (arr.Length == 4)
                {
                    value += "|" + arr[2];
                    value += "|" + arr[3];
                    value += "|" + arr[0];
                    value += "|" + arr[1];
                }
                else
                {
                    value += "||||";
                }
                return value;
            }
        }
    }
}
