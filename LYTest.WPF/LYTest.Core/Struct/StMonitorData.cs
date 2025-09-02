using System;
using System.Runtime.InteropServices;

namespace LYTest.Core.Struct
{
    #region =========== 标准表监视功率源数据结构体 ===========
    /// <summary>
    /// 标准表监视功率源数据
    /// </summary>
    [Serializable, StructLayout(LayoutKind.Sequential)]
    public struct StMonitorData
    {
        /// <summary>
        /// 频率 (信号)
        /// </summary>
        public float Frequency;
        /// <summary>
        /// 是否过载
        /// </summary>
        public byte OverLoad;
        /// <summary>
        /// Va幅值
        /// </summary>
        public float UaValue;
        /// <summary>
        /// Vb幅值
        /// </summary>
        public float UbValue;
        /// <summary>
        /// Vc幅值
        /// </summary>
        public float UcValue;
        /// <summary>
        /// Ia幅值
        /// </summary>
        public float IaValue;
        /// <summary>
        /// Ib幅值
        /// </summary>
        public float IbValue;
        /// <summary>
        /// Ic幅值
        /// </summary>
        public float IcValue;
        /// <summary>
        /// Va相位
        /// </summary>
        public float UaPhase;
        /// <summary>
        /// Ia相位
        /// </summary>
        public float IaPhase;
        /// <summary>
        /// Vb相位
        /// </summary>
        public float UbPhase;
        /// <summary>
        /// Ib相位
        /// </summary>
        public float IbPhase;
        /// <summary>
        /// Vc相位
        /// </summary>
        public float UcPhase;
        /// <summary>
        /// Ic相位
        /// </summary>
        public float IcPhase;
        /// <summary>
        /// A相相角
        /// </summary>
        public float Aa;
        /// <summary>
        /// B相相角
        /// </summary>
        public float Ab;
        /// <summary>
        /// C相相角
        /// </summary>
        public float Ac;
        /// <summary>
        /// 功率相角
        /// </summary>
        public float PowerAngle;
        /// <summary>
        /// A相有功功率
        /// </summary>
        public float Pa;
        /// <summary>
        /// B相有功功率
        /// </summary>
        public float Pb;
        /// <summary>
        /// C相有功功率
        /// </summary>
        public float Pc;
        /// <summary>
        /// A相无功功率
        /// </summary>
        public float Qa;
        /// <summary>
        /// B相无功功率
        /// </summary>
        public float Qb;
        /// <summary>
        /// C相无功功率
        /// </summary>
        public float Qc;
        /// <summary>
        /// A相视在功率
        /// </summary>
        public float Sa;
        /// <summary>
        /// B相视在功率
        /// </summary>
        public float Sb;
        /// <summary>
        /// C相视在功率
        /// </summary>
        public float Sc;
        /// <summary>
        /// 总有功功率
        /// </summary>
        public float P;
        /// <summary>
        /// 总无功功率
        /// </summary>
        public float Q;
        /// <summary>
        /// 总视在功率
        /// </summary>
        public float S;
        /// <summary>
        /// 总有功功率因数
        /// </summary>
        public float Pcos;
        /// <summary>
        /// 总无功功率因数
        /// </summary>
        public float Qsin;
        /// <summary>
        /// A相有功功率因数
        /// </summary>
        public float ApCos;
        /// <summary>
        /// B相有功功率因数
        /// </summary>
        public float BpCos;
        /// <summary>
        /// C相有功功率因数
        /// </summary>
        public float CpCos;
    }
    #endregion
}
