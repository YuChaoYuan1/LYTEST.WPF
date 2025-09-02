using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace CLOU.Enum
{
    /// <summary>
    /// ����Ԫ��
    /// </summary>
    [ComVisible(true)]
    public enum Cus_PowerYuanJiang
    {
        /// <summary>
        /// ����ġ�δ��ֵ��
        /// 
        /// </summary>
        Error = 0,

        /// <summary>
        /// ��Ԫ
        /// </summary>
        H = 1 ,

        /// <summary>
        /// AԪ
        /// </summary>
        A = 2,

        /// <summary>
        /// BԪ
        /// </summary>
        B = 3,

        /// <summary>
        /// CԪ
        /// </summary>
        C = 4,
    }

    /// <summary>
    /// ��ʾ������
    /// </summary>
    public enum Cus_LightType
    {
        /// <summary>
        /// 
        /// </summary>
        OFF = 0,
        /// <summary>
        /// 
        /// </summary>
        RED = 1,
        /// <summary>
        /// 
        /// </summary>
        YELLOW = 2,
        /// <summary>
        /// 
        /// </summary>
        GREE = 4
    }

}
