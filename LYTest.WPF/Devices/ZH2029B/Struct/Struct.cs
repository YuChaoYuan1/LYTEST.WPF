using System;
using System.Collections.Generic;
using System.Text;

namespace CLOU.Struct
{


    /// <summary>
    /// 功能描述：载波协议信息
    /// 作    者：vs
    /// 编写日期：2010-09-06
    /// 修改记录：
    ///         修改日期：		     
    ///         修改  人：樊江凯           
    ///         修改内容：添加路由标识     
    ///
    /// </summary>
    [Serializable()]
    public struct StCarrierInfo
    {
        /// <summary>
        /// 载波名称
        /// </summary>
        public string CarrierName;
        /// <summary>
        /// 通讯介质
        /// </summary>
        public string CarrierType;
        /// <summary>
        /// 抄表器类型
        /// </summary>
        public string RdType;
        /// <summary>
        /// 通讯方式
        /// </summary>
        public string CommuType;
        /// <summary>
        /// 波特率
        /// </summary>
        public string BaudRate;
        /// <summary>
        /// 通讯端口
        /// </summary>
        public string Comm;
        /// <summary>
        /// 命令延时(ms)
        /// </summary>
        public string CmdTime;
        /// <summary>
        /// 字节延时(ms)
        /// </summary>
        public string ByteTime;
        /// <summary>
        /// 路由标识
        /// <para>0表示通信模块带路由或工作在路由模式，1表示通信模块不带路由或工作在旁路模式。</para>
        /// </summary>
        public byte RouterID;

        /// <summary>
        /// 返回载波设备信息
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("载波名称:{0} 通讯介质:{1} 抄表器类型:{2} 通讯方式:{3} 波特率:{4} 通讯端口:{5} 命令延时(ms):{6} 字节延时(ms):{7} 路由标识:{8} ", CarrierName, CarrierType, RdType, CommuType, BaudRate, Comm, CmdTime, ByteTime, RouterID);
        }
    }

    /// <summary>
    /// 电压参数
    /// </summary>
    public struct UIPara
    {
        /// <summary>
        /// 
        /// </summary>
        public double Ua;
        /// <summary>
        /// 
        /// </summary>
        public double Ub;
        /// <summary>
        /// 
        /// </summary>
        public double Uc;
        /// <summary>
        /// 
        /// </summary>
        public double Ia;
        /// <summary>
        /// 
        /// </summary>
        public double Ib;
        /// <summary>
        /// 
        /// </summary>
        public double Ic;
    }
    /// <summary>
    /// 
    /// </summary>
    public struct PhiPara
    {
        /// <summary>
        /// 
        /// </summary>
        public double PhiUa;
        /// <summary>
        /// 
        /// </summary>
        public double PhiUb;
        /// <summary>
        /// 
        /// </summary>
        public double PhiUc;
        /// <summary>
        /// 
        /// </summary>
        public double PhiIa;
        /// <summary>
        /// 
        /// </summary>
        public double PhiIb;
        /// <summary>
        /// 
        /// </summary>
        public double PhiIc;
    }
   
    /// <summary>
    /// 误差板功耗数据读取，用于计算功耗
    /// </summary>
    public struct stGHPram
    {
        /// <summary>
        /// 表位号
        /// </summary>
        public int MeterIndex;
        /// <summary>
        /// A相电压回路电流，单相电压回路电流值
        /// </summary>
        public float AU_Ia_or_I;
        /// <summary>
        /// B相电压回路电流，单相电流1回路电压值
        /// </summary>
        public float BU_Ib_or_L1_U;
        /// <summary>
        /// C相电压回路电流，单相电流2回路电压值
        /// </summary>
        public float CU_Ic_or_L2_U;
        /// <summary>
        /// A相电流回路电压
        /// </summary>
        public float AI_Ua;
        /// <summary>
        /// B相电流回路电压
        /// </summary>
        public float BI_Ub;
        /// <summary>
        /// C相电流回路电压
        /// </summary>
        public float CI_Uc;
        /// <summary>
        /// A相电压回路相位角，单相电压回路相位角
        /// </summary>
        public float AU_Phia_or_Phi;
        /// <summary>
        /// B相电压回路相位角
        /// </summary>
        public float BU_Phib;
        /// <summary>
        /// C相电压回路相位角
        /// </summary>
        public float CU_Phic;
    }
}
