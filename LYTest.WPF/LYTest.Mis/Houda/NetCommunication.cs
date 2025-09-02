using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;

namespace LYTest.Mis.Houda
{
    /// <summary>
    /// 线程中是非传递
    /// </summary>
    /// <param name="value"></param>
    public delegate void MessageDelegateBool(bool value);
    /// <summary>
    /// 网络通讯参数
    /// </summary>
    public class SocketPortParameter
    {
        /// <summary>
        /// 客户端 
        /// </summary>
        public Socket TcpClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        /// <summary>
        /// 数据长度
        /// </summary>
        public int BufferLength = 0;
        /// <summary>
        /// 数据
        /// </summary>
        public string DataValue = string.Empty;
        /// <summary>
        /// 数据发送缓冲区
        /// </summary>
        public LinkedList<NetworkMessage> NetworkMessageLinkedList = new LinkedList<NetworkMessage>();
        /// <summary>
        ///  数据发送缓冲区重复发送缓存区
        /// </summary>
        public LinkedList<NetworkMessage> NetworkMessageLinkedListResponsion = new LinkedList<NetworkMessage>();
        /// <summary>
        /// 补救次数
        /// </summary>
        public int RepairNumber = 0;
        /// <summary>
        /// 接受数据线程
        /// </summary>
        public Thread ClientReceiveThread = null;
        /// <summary>
        /// 
        /// </summary>
        public MessageDelegateBool LogonStateDelegate = null;

    }


    /// <summary>
    /// 网络通信类
    /// </summary>
    public class NetworkMessage : EventArgs
    {
        //信息的属性
        /// <summary>
        /// 通信协议属性
        /// </summary>
        public NetworkMessageAttribute MessageAttribute = NetworkMessageAttribute.heartbeat;
        /// <summary>
        ///  读取或者设置的xml格式的字符串
        /// </summary>
        public string xmlDoc = string.Empty;
        /// <summary>
        /// 分解协议的值，或者协议合成的值
        /// </summary>
        public Hashtable ValueHashtable = new Hashtable();
        /// <summary>
        /// 数据缓冲区
        /// </summary>
        public byte[] Buffer = new byte[1024];
        /// <summary>
        /// 固定参数的分解协议的值，或者协议合成的值
        /// </summary>
        public ArrayList ValueArray = new ArrayList();
        /// <summary>
        /// 此命令是否装置确认信息
        /// </summary>
        public bool WaitForValidate = false;

    }


    /// <summary>
    /// 通信协议属性
    /// </summary>
    public enum NetworkMessageAttribute : uint
    {
        /// <summary>
        /// 登陆(装置编号) 客户机主动请求
        /// </summary>
        登陆9998 = 0x00010,
        /// <summary>
        /// 心跳
        /// </summary>
        heartbeat,
        /// <summary>
        /// 自检命令
        /// </summary>
        自检2001,
        /// <summary>
        /// 测试通知  服务器发给 客户机的指令
        /// </summary>
        测试通知1004,

        /// <summary>
        /// 测试结果通知  客户机发给服务器 的指令
        /// </summary>
        测试结果通知2005,
        /// <summary>
        /// 台体控制指令
        /// </summary>
        台体控制指令1006,

        任务设置7006,

        /// <summary>
        /// 查找表位
        /// </summary>
       // 查找表位1109,
        /// <summary>
        /// 查找表位返回
        /// </summary>
       // 查找表位返回2109,

        /// <summary>
        /// 应答指令
        /// </summary>
        应答指令9999,

        /// <summary>
        /// 台体状态发生改变
        /// </summary>

        电测台体状态变化2008,

    }
}

