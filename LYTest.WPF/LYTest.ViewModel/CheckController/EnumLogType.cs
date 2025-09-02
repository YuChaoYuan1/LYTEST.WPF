using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LYTest.ViewModel.CheckController
{
    /// <summary>
    /// 日志类型
    /// </summary>
    public enum EnumLogType
    {
        /// <summary>
        /// 异常的数据--必定会保存和显示出来
        /// </summary>
        错误信息 = 1,
        /// <summary>
        /// 仅显示在下发提示栏-没开始保存的情况不会保存该日志-不会添加到日志表格中
        /// </summary>
        提示信息 = 2,
        /// <summary>
        ///过程中所有的数据--会保存数据库-开启状态下才会显示
        /// </summary>
        详细信息 = 3,
        /// <summary>
        /// 正常的检定流程日志--默认保存
        /// </summary>
        流程信息 = 4,
        /// <summary>
        /// 即显示流程日志也显示提示信息
        /// </summary>
        提示与流程信息 = 5,
    }
}
