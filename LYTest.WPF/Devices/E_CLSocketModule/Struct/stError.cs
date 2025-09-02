using E_CLSocketModule.Enum;
using System;
using System.Collections.Generic;
using System.Text;

namespace E_CLSocketModule.Struct
{
    /// <summary>
    /// 读取的误差数据
    /// </summary>
    public struct stError
    {
        /// <summary>
        /// 误差值
        /// </summary>
        public string szError;

        /// <summary>
        /// 标识当前属于第几次误差
        /// </summary>
        public int Index;

        /// <summary>
        /// 表位号
        /// </summary>
        public int MeterIndex;
        /// <summary>
        /// 状态类型
        /// </summary>
        public int MeterConst;
        /// <summary>
        /// 电流回路状态,0x00表示第一个电流回路，0x01表示第二个电流回路
        /// </summary>
        public Cus_EmBothIRoadType iType;
        /// <summary>
        /// 电压回路状态,0x00表示直接接入式电表电压回路选择，0x01表示互感器接入式电表电压回路选择，0x02表示本表位无电表接入
        /// </summary>
        public Cus_EmBothVRoadType vType;
        /// <summary>
        /// 通讯口状态,0x00表示选择第一路普通485通讯；0x01表示选择第二路普通485通讯；0x02表示选择红外通讯；
        /// </summary>
        public int ConnType;

        /*
         * 状态类型分为四种：接线故障状态（Bit0）、预付费跳闸状态（Bit1）、报警信号状态（Bit2）、对标状态（Bit3）的参数
         * 分别由一个字节中的Bit0、Bit1、Bit2、Bit3标示，为1则表示该表位有故障/跳闸/报警/对标完成，为0则表示正常/正常/正常/未完成对标。
        */
        /// <summary>
        /// 接线故障状态,为true则表示该表位有故障,false为正常
        /// </summary>
        public bool statusTypeIsOnErr_Jxgz;

        /// <summary>
        /// 预付费跳闸状态,为true则表示该表位跳闸,false为正常
        /// </summary>
        public bool statusTypeIsOnErr_Yfftz;

        /// <summary>
        /// 报警信号状态,为true则表示该表位报警,false为正常
        /// </summary>
        public bool statusTypeIsOnErr_Bjxh;

        /// <summary>
        /// 对标状态,为true则表示该表位对标完成,false为未完成对标
        /// </summary>
        public bool statusTypeIsOnOver_Db;
        /// <summary>
        /// 温度过高故障状态（false：正常；true：故障）。温度过高时，会自动短接隔离继电器
        /// </summary>
        public bool statusTypeIsOnErr_Temp;
        /// <summary>
        /// 光电信号状态（false：未挂表；true：已挂表）
        /// </summary>
        public bool statusTypeIsOn_HaveMeter;

        /// <summary>
        /// 表位上限限位状态（false：未就位；true：就位）
        /// </summary>
        public bool statusTypeIsOn_PressUpLimit;

        /// <summary>
        /// 表位下限限位状态（false：未就位；true：就位）
        /// </summary>
        public bool statusTypeIsOn_PressDownLimt;
        /// <summary>
        /// true :读到数据 FALSE ：没有读到
        /// </summary>
        public bool statusReadFlog;
    }
}
