using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LYTest.ViewModel.CodeTree
{
    /// <summary>
    /// 用户权限的枚举
    /// </summary>
    public enum EnumPermission
    {
        //普通用户可选中 = 0,
        //普通用户可修改 = 1,
        //普通用户可增加 = 2,
        //普通用户可增删改 = 3,

        //管理员可选中 = 10,
        //管理员可修改 = 11,
        //管理员可增加 = 12,
        //管理员可增删改 = 13,

        //仅超级用户可操作 = 20,

        普通用户可操作 = 0,
        管理员可操作 = 10,
        超级用户可操作 = 20,
    }
}
