using System;

namespace LYTest.Core.Struct
{
    /// <summary>
    /// 启动结构体
    /// </summary>
    [Serializable()]
    public struct StPlan_QiDong
    {
        /// <summary>
        /// 项目编号
        /// </summary>
        public string ItemId;
        /// <summary>
        /// 功率方向
        /// </summary>
        public Enum.PowerWay PowerFangXiang;
        /// <summary>
        /// 电流倍数(数字)单位(倍)(如果为0则根据规程计算起动电流倍数)
        /// </summary>
        public float FloatxIb;
        /// <summary>
        /// 电压倍数
        /// </summary>
        public float FloatXUb;
        /// <summary>
        /// 起动时间（秒），起动时间为规程计算时间(为0)，如果非零则直接为起动时间（非规程计算）
        /// </summary>
        public float xTime;

        /// <summary>
        /// 实际试验时间（秒）
        /// </summary>
        public float CheckTime;
        /// <summary>
        /// 起动电流值(单位A)
        /// </summary>
        public float FloatIb;
        /// <summary>
        /// 默认是否合格0-不默认，1-默认，默认合格情况下，则不会做起动试验
        /// </summary>
        public int DefaultValue;

        /// <summary>
        /// 根据规程计算起动时间和起动电流
        /// </summary>
        /// <param name="GuiChengName">规程名称</param>
        /// <param name="clfs">测量方式</param>
        /// <param name="U">电压</param>
        /// <param name="Ib">电流字符串Ib(Imax)</param>
        /// <param name="Dj">等级1.0(2.0)</param>
        /// <param name="MeterConst">常数 有功(无功)</param>
        /// <param name="znq">止逆器</param>
        /// <param name="hgq">互感器</param>
        public void CheckTimeAndIb(string GuiChengName, Enum.WireMode clfs, float U, string Ib, string Dj, string MeterConst, bool znq, bool hgq)
        {
            Enum.Cus_Ywg _Ywg;
            if (this.PowerFangXiang == Enum.PowerWay.正向有功 || this.PowerFangXiang == Enum.PowerWay.反向有功)
                _Ywg = Enum.Cus_Ywg.P;
            else
                _Ywg = Enum.Cus_Ywg.Q;

            float _ib = 0F;

            try
            {
                switch (GuiChengName.ToUpper())
                {
                    case "IR46":
                    case "Q/GDW12175-2021":
                    case "Q/GDW10827-2020":
                    case "Q/GDW10364-2020":
                        _ib = Function.Number.GetCurrentByIb("Itr", Ib, hgq);
                        break;
                    default:
                        _ib = Function.Number.GetCurrentByIb("Ib", Ib, hgq);
                        break;
                }
            }
            catch
            {

            }

            if (_ib == 0F)
            {
                _ib = float.Parse(Ib.Substring(0, Ib.IndexOf("(")));
            }
            if (this.FloatxIb == 0)         //如果方案电流倍数为0，则使用规程起动电流
            {
                this.FloatIb = Function.QiDQianDFunction.GetQiDongCurrent(GuiChengName, _ib, Dj, znq, hgq, _Ywg);

            }
            else   //反之则直接使用方案电流倍数乘以标定电流
            {
                this.FloatIb = this.FloatxIb * _ib;
            }

            if (this.xTime == 0F)           //如果方案起动时间为0，则使用规程起动时间
            {
                this.CheckTime = Function.QiDQianDFunction.GetQiDongTime(GuiChengName, U, this.FloatIb, Function.Number.GetBcs(MeterConst, PowerFangXiang), clfs);

            }
            else
            {
                this.CheckTime = this.xTime;
            }
        }

        /// <summary>
        /// 重写ToString函数，返回启动描述
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("{0}起动", PowerFangXiang.ToString());
        }




    }
}
