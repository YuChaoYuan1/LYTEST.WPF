using LYTest.Core.Enum;
using LYTest.Core.Function;
using System;

namespace LYTest.Core.Struct
{
    /// <summary>
    /// 误差点
    /// </summary>
    [Serializable()]
    public struct StPlan_WcPoint
    {
        /// <summary>
        /// 项目ID
        /// </summary>
        private string _PrjID;

        /// <summary>
        /// 项目ID
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
                this.Pc = int.Parse(_PrjID.Substring(0, 1)) == 2 ? 1 : 0;
                this.PowerFangXiang = (PowerWay)int.Parse(_PrjID.Substring(1, 1));
                this.PowerYuanJian = (Cus_PowerYuanJian)int.Parse(_PrjID.Substring(2, 1));
                this.XieBo = int.Parse(_PrjID.Substring(7, 1));
                this.XiangXu = int.Parse(_PrjID.Substring(8, 1));
            }
        }
        /// <summary>
        /// 项目名称
        /// </summary>
        public string PrjName;

        /// <summary>
        /// 内部编号
        /// </summary>
        public int PointId;

        /// <summary>
        /// 检定点顺序
        /// </summary>
        public int nCheckOrder;

        /// <summary>
        /// 功率元件
        /// </summary>
        public Cus_PowerYuanJian PowerYuanJian;

        /// <summary>
        /// 功率方向
        /// </summary>
        public PowerWay PowerFangXiang;

        /// <summary>
        /// 功率因数
        /// </summary>
        public string PowerYinSu;

        /// <summary>
        /// 误差类型
        /// </summary>
        public string ErrorType;

        /// <summary>
        /// 电压倍数
        /// </summary>
        public string PowerDianYa;
        /// <summary>
        /// 负载电流xIb 
        /// </summary>
        public string PowerDianLiu;

        /// <summary>
        /// 是否是偏差0不是1是
        /// </summary>
        public int Pc;

        /// <summary>
        /// 是否要检
        /// </summary>
        public bool IsCheck;

        /// <summary>
        /// 误差上限(默认值-999)
        /// </summary>
        public float ErrorShangXian;

        /// <summary>
        /// 误差下限（默认值-999）
        /// </summary>
        public float ErrorXiaXian;

        /// <summary>
        /// 检测圈数
        /// </summary>
        public int LapCount;

        /// <summary>
        /// 相序0-正相序，1-逆相序
        /// </summary>
        public int XiangXu;


        /// <summary>
        /// 谐波0-不加，1-加
        /// </summary>
        public int XieBo;

        /// <summary>
        /// 频率
        /// </summary>

        public float PL;

        /// <summary>
        /// 电压倍数(A相)
        /// </summary>
        public float xUa;

        /// <summary>
        /// 电压倍数（B相）
        /// </summary>
        public float xUb;

        /// <summary>
        /// 电压倍数（C相）
        /// </summary>
        public float xUc;
        /// <summary>
        /// 不平衡负载与平衡负载时误差之差标志 
        /// 0：没有，1：有
        /// </summary>
        public int Dif_Err_Flag
        {
            get;
            set;
        }


        #region 计算、设置检定点的检定圈数
        /// <summary>
        /// 计算获取需要检定圈数
        /// </summary>
        /// <param name="MinConst">当台被检表中的最小常数(数组，下标0=有功，下标1=无功)</param>
        /// <param name="MeConst">当前表的常数 有功(无功)</param>
        /// <param name="Current">电流参数(1.5(6))</param>
        /// <param name="CzIb">参照 电流</param>
        /// <param name="CzQS">参照圈数</param>
        public void SetLapCount(int[] MinConst, string MeConst, string Current, string CzIb, int CzQS, bool HGQ)
        {
            int _MeConst;

            int _MinConst;

            if (this.PowerFangXiang == PowerWay.正向有功 || this.PowerFangXiang == PowerWay.反向有功)
            {
                _MeConst = Number.GetBcs(MeConst, PowerFangXiang);
                _MinConst = MinConst[0];
            }
            else
            {
                _MeConst = Number.GetBcs(MeConst, PowerFangXiang);
                _MinConst = MinConst[1];
            }
            //6/(3600/((220*1.5*1*3*6400)/1000))
            float _Tqs = CzQS * (_MeConst / (float)_MinConst);

            _Tqs *= Number.GetCurrentByIb(PowerDianLiu, Current, HGQ) / Number.GetCurrentByIb(CzIb, Current, HGQ) * Number.GetGlysValue(this.PowerYinSu);
            if ((int)this.PowerYuanJian != 1)      //不是合元
                _Tqs /= 3;
            int _QS = (int)Math.Round(_Tqs, 0);
            if (_QS == 0)
                _QS = 1;
            LapCount = _QS;
        }

        public void SetLapCount2(float U, string I, WireMode JXFS, Cus_PowerYuanJian YJ, string MeConst, string GLYS, bool IsYouGong, bool HGQ, float time = 5)
        {
            //电压，电流，测量方式，元件，功率因数，有功无功，
            float xIb = Number.GetCurrentByIb(PowerDianLiu, I, HGQ);
            int meterConst;
            if (this.PowerFangXiang == PowerWay.正向有功 || this.PowerFangXiang == PowerWay.反向有功)
            {
                meterConst = Number.GetBcs(MeConst, PowerFangXiang);
            }
            else
            {
                meterConst = Number.GetBcs(MeConst, PowerFangXiang);
            }

            // 圈数计算方法
            float currentPower = Number.CalculatePower(U, xIb, JXFS, YJ, GLYS, IsYouGong);

            int onePulseTime = (int)Math.Ceiling(time / (1 / (currentPower * meterConst / 3600000)));
            int QS = onePulseTime;

            if (QS == 0)
                QS = 1;
            LapCount = QS;

        }


        ///// <summary>
        ///// 计算当前负载下本批表中脉冲常数最小的表跑一个脉冲需要的时间（ms）
        ///// </summary>
        ///// <param name="bYouGong">有功/无功</param>
        ///// <param name="OneKWHTime">一度电需要的时间(分)</param>
        ///// <returns>以毫秒为单位</returns>
        //protected int OnePulseNeedTime(bool bYouGong, float OneKWHTime, string MeConst)
        //{
        //    float minConst = 999999999;
        //    int onePulseTime = 99;
        //    int[] arrConst = MeterHelper.Instance.MeterConst(bYouGong);
        //    for (int i = 0; i < arrConst.Length; i++)
        //    {

        //        if (arrConst[i] < minConst)
        //            minConst = arrConst[i];

        //    }
        //    if (minConst == 999999999) return 1;
        //    onePulseTime = (int)Math.Ceiling(OneKWHTime * 60 / minConst * 1000);
        //    return onePulseTime;
        //}

        /// <summary>
        /// 获取当前被检点的误差限
        /// </summary>
        /// <param name="WcLimitName">误差限名称</param>
        /// <param name="GuiChengName">规程名称</param>
        /// <param name="Dj">等级1.0(2.0)</param>
        /// <param name="Hgq">是否经互感器接入</param>
        public void SetWcx(string GuiChengName, string Dj, bool Hgq)
        {
            string[] djArr = Number.GetDj(Dj);

            bool YouGong = true;

            if (((int)PowerFangXiang) > 2)
                YouGong = false;

            //if (WcLimitName == "规程误差限")
            //{
            if (this.Pc == 0)          //误差
            {
                string wcx = ErrLimitHelper.Wcx(this.PowerDianLiu, GuiChengName,
                    (int)this.PowerFangXiang > 2 ? djArr[1] : djArr[0],
                    this.PowerYuanJian, this.PowerYinSu, Hgq, YouGong);
                this.SetWcx(float.Parse(wcx), float.Parse($"-{wcx}"));       //设置误差限
            }
            else //偏差
            {
                string wcx = ErrLimitHelper.Pcx(this.PowerDianLiu, GuiChengName
                                , (int)this.PowerFangXiang > 2 ? djArr[1] : djArr[0]
                                , this.PowerYinSu, Hgq);
                this.SetWcx(float.Parse(wcx), 0F);     //设置误差限
            }
            //}

        }



        /// <summary>
        /// 设置误差限
        /// </summary>
        /// <param name="Max">上线</param>
        /// <param name="Min">下限</param>
        private void SetWcx(float Max, float Min)
        {
            if (this.ErrorShangXian == 0F)
                this.ErrorShangXian = Max;
            if (this.ErrorXiaXian == 0F)
                this.ErrorXiaXian = Min;
            WcxUpPercent = 1;
            WcxDownPercent = 1;
        }



        #endregion

        public override string ToString()
        {
            return PrjName + (Dif_Err_Flag == 1 ? " FHC" : "");
        }

        /// <summary>
        /// 统一误差上限比率（只读,默认100%）
        /// </summary>
        public float WcxUpPercent { get; set; }

        /// <summary>
        /// 项目编号(黑龙江使用)
        /// </summary>
        public string ItemId;

        /// <summary>
        /// 误差个数
        /// </summary>
        public int ErrorNumber;


        /// <summary>
        /// 小数位
        /// </summary>
        public string DecimalPlace;

        /// <summary>
        /// 统一误差下限比率（只读,默认100%）
        /// </summary>
        public float WcxDownPercent { get; set; }

    }
}
