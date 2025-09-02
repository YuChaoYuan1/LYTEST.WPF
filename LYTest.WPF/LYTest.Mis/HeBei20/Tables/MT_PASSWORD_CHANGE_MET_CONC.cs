using System;
namespace LYTest.Mis.HeBei20.Tables
{
    /// <summary>
    /// 485密码修改
    /// </summary>
    [Serializable]
    public class MT_PASSWORD_CHANGE_MET_CONC : MT_MET_CONC_Base
    {
        /// <summary>
        /// A10       0：无效 1：有效
        /// </summary>
        public string IS_VALID { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string PASSWORD_LEVEL { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string OLD_PASSWORD { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string NEW_PASSWORD { get; set; }


    }
}

