namespace LYTest.DAL
{
    /// 应用程序数据库中表名称
    /// <summary>
    /// 应用程序数据库中表名称
    /// </summary>
    public enum EnumAppDbTable
    {


        /// <summary>
        /// 用户信息表
        /// </summary>
        T_USER_INFO,

        /// <summary>
        /// 最后一次信息表
        /// </summary>
        T_LAST_INFO,

         /// <summary>
         /// 树节点结果配置数据表
         /// </summary>
        T_CODE_TREE,

        /// <summary>
        /// 页面配置【参数录入界面，详细结论界面等】
        /// </summary>
        T_VIEW_CONFIG,

         /// <summary>
         /// 界面显示配置
         /// </summary>
        T_MENU_VIEW,

        /// <summary>
        /// 已配置方案列表
        /// </summary>
       T_SCHEMA_INFO,

        ///// <summary>
        ///// 方案参数值
        ///// </summary>
       T_SCHEMA_PARA_VALUE,

        /// 方案参数表
        /// </summary>
        T_SCHEMA_PARA_FORMAT,

        /// <summary>
        /// 系统配置项的数据源及默认数据
        /// </summary>
        T_CONFIG_PARA_FORMAT,
        /// <summary>
        /// 系统配置项当前设置的数据
        /// </summary>
        T_CONFIG_PARA_VALUE,


         /// <summary>
         ///设备配置信息(标准表源误差板一些端口)
         /// </summary>
        T_DEVICE_CONFIG,

        /// <summary>
        /// 时间统计表
        /// </summary>
       TIME_STATISTICS,

        #region 旧



        /// <summary>
        /// 检定点视图
        /// </summary>
        //DSPTCH_DIC_VIEW,   
        /// <summary>
        /// 台体信息
        /// </summary>
        //DSPTCH_EQUIP_INFO,  --这个是对每个检定台的数据进行分类。。可以不需要
        /// <summary>
        /// 结论表中检定点Key对应的字段
        /// </summary>
        //DSPTCH_TABLE_FIELD,
        ///// <summary>
        ///// 方案列表
        ///// </summary>
        ////SCHEMA_INFO,
        ///// <summary>
        ///// 方案参数表
        ///// </summary>
        ////SCHEMA_PARA_FORMAT,
        ///// <summary>
        ///// 方案参数值
        ///// </summary>
        //SCHEMA_PARA_VALUE,
        //CONFIG_PARA_FORMAT,
        //CONFIG_PARA_VALUE,
        //LAST_STATE_INFO,
        //Info_User,
        /// <summary>
        /// 时间统计表
        /// </summary>
        // TIME_STATISTICS,
        //CODE_TREE,
        //MIS_UPLOAD_CONFIG,

        //=====================================================================================
        //=====================================================================================

        #endregion

    }
}
