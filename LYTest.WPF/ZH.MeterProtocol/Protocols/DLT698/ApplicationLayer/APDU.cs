using System.Collections.Generic;
using LYTest.MeterProtocol.Protocols.DLT698.Enum;
using LYTest.MeterProtocol.Protocols.DLT698.Struct;

namespace LYTest.MeterProtocol.Protocols.ApplicationLayer
{
    /// <summary>
    /// 应用层
    /// </summary>
    public class APDU
    {

        /// <summary>
        /// 传输安全模式
        /// </summary>
        public EmSecurityMode SecurityMode { get; set; }

        /// <summary>
        /// 数据验证码
        /// </summary>
        public StSIDMAC SidMac { get; set; }

        /// <summary>
        /// 对象属性信息管理类
        /// </summary>
        public ObjectInfosManage ObjectInfos = null;

        /// <summary>
        /// 对象属性信息
        /// </summary>
        public ObjectAttributes obatt;

        /// <summary>
        /// 读取上N次记录
        /// </summary>
        public int RecordNo { get; set; }

        public APDU()
        { }

        public APDU(EmSecurityMode Mode)
        {
            SecurityMode = Mode;
        }
        /// <summary>
        /// 读数据组帧
        /// </summary>
        /// <returns></returns>
        public virtual string ReadDataAPDU_Frame()
        {
            return string.Empty;
        }
        /// <summary>
        /// 写数据组帧
        /// </summary>
        /// <returns></returns>
        public virtual string SetDataAPDU_Frame()
        {
            return string.Empty;

        }
        /// <summary>
        /// 操作组帧
        /// </summary>
        /// <returns></returns>
        public virtual string OperationAPDU_Frame()
        {
            return string.Empty;
        }
        /// <summary>
        /// 应用连接组帧
        /// </summary>
        /// <returns></returns>
        public virtual string AppConnect()
        {
            return string.Empty;
        }

        /// <summary>
        /// 解析明文读取返回数据
        /// </summary>
        /// <param name="Frame">帧</param>
        /// <param name="data">数据内容</param>
        /// <param name="reportdata">上报内容</param>
        /// <returns>执行成功与否</returns>
        public virtual bool ParseGetFrame(byte[] Frame, ref List<object> data, ref List<object> reportdata)
        {
            return true;

        }
        public virtual bool ParseNotificationFrame(byte[] Frame, ref List<object> data)
        { 
            return true;
        }

        public virtual bool ParseReadFrame(byte[] Frame, ref List<object> CsdData, ref Dictionary<string, List<object>> DicObj)
        {
            return true;
        }

        /// <summary>
        /// 解析明文设置返回数据
        /// </summary>
        /// <param name="Frame">帧</param>
        /// <param name="ErrCode">错误代码</param>
        /// <returns>执行成功与否</returns>
        public virtual bool ParseSetFrame(byte[] Frame, ref int ErrCode)
        {
            return true;

        }

        /// <summary>
        /// 解析明文操作返回数据
        /// </summary>
        /// <param name="Frame">帧</param>
        /// <param name="ErrCode">错误代码</param>
        /// <returns>执行成功与否</returns>
        public virtual bool ParseActionFrame(byte[] Frame, ref int ErrCode)
        {
            return true;

        }

        /// <summary>
        /// 解析应用连接数据
        /// </summary>
        /// <param name="Frame">帧</param>
        /// <param name="ErrCode">错误代码</param>
        /// <param name="data">数据内容</param>
        /// <returns>执行成功与否</returns>
        public virtual bool ParseConnectionFrame(byte[] Frame, ref int ErrCode, ref List<object> data)
        {
            return true;

        }

        /// <summary>
        /// 解析安全传输
        /// </summary>
        /// <param name="Frame"></param>
        /// <param name="ErrCode"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public virtual bool ParseSecurityResponse(byte[] Frame, ref int ErrCode, ref List<object> data, ref List<object> reportdata)
        {
            return true;
        }

        /// <summary>
        /// 解析安全传输
        /// </summary>
        /// <param name="Frame"></param>
        /// <param name="ErrCode"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public virtual bool ParseSecurityResponse(byte[] Frame, ref int ErrCode, ref List<object> data, ref Dictionary<string, List<object>> DicObj)
        {
            return true;
        }
    }
}
