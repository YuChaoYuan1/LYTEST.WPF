using LY.SocketModule.Packet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LY.SocketModule.Sock
{
    interface IConnection
    {


        /// <summary>
        /// 连接名称
        /// </summary>
        string PortName { get; }
        /// <summary>
        /// 最大等待时间
        /// </summary>
        int Timeout { set; get; }

        bool Open();


        bool Close();

        /// <summary>
        /// 更新端口信息
        /// </summary>
        /// <param name="setting"></param>
        /// <returns></returns>
        bool UpdateBaudrate(string setting);



        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="vData">要发送的数据</param>
        /// <param name="IsReturn">量否需要回复</param>
        /// <param name="WaiteTime">发送后等待时间</param>
        /// <returns>发送是否成功</returns>
        bool SendData(SendPacket sPacke, RecvPacket rPacket);
    }
}
