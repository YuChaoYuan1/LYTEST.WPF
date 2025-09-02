namespace LYTest.CtrlCmd
{
    public enum MsgType
    {
        运行消息,
        空闲,
        正在运行,
        故障,
        /// <summary>
        /// 登录时发一次就可以
        /// </summary>
        心跳包,
        检定完成,
        /// <summary>
        /// 登录必须返回
        /// </summary>
        请求结果,
        /// <summary>
        /// n/N
        /// </summary>
        检定进度
    }
}
