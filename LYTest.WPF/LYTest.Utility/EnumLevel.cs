namespace LYTest.Utility.Log
{
    /// 异常信息的枚举
    /// <summary>
    /// 异常信息的枚举
    /// </summary>
    public enum EnumLevel
    {
        /// <summary>
        /// 显示信息
        /// </summary>
        Information=0,
        /// <summary>
        /// 告警
        /// </summary>
        Warning=1,
        /// <summary>
        /// 故障 --弹出窗体
        /// </summary>
        Error=2,
        /// <summary>
        /// 提示信息
        /// </summary>
        Tip = 3, //弹出窗体
        /// <summary>
        /// 故障-不弹出错误界面
        /// </summary>
        TipsError = 99,
        /// <summary>
        /// 提示信息 不弹出错误界面
        /// </summary>
        TipsTip = 98,
        ///// <summary>
        ///// 提示信息 不弹出错误界面
        ///// </summary>
        //TipsTip = 98,
        ///// <summary>
        ///// 信息带语音
        ///// </summary>
        //InformationSpeech = 90,
        ///// <summary>
        ///// 告警带语音
        ///// </summary>
        //WarningSpeech = 91,
        ///// <summary>
        ///// 故障带语音
        ///// </summary>
        //ErrorSpeech = 92,

    }
}
