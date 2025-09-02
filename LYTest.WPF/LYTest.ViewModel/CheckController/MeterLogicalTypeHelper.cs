using LYTest.Core.Enum;
using LYTest.DAL.Config;

namespace LYTest.ViewModel.CheckController
{
    public class MeterLogicalTypeHelper
    {
        /// <summary>
        /// 获取某个试验在进入的时候切换到那个芯片
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public static MeterLogicalAddressEnum GetMeterLogicalType(string ID)
        {
            if (ID.IndexOf('_')>=0)  ID = ID.Substring(0, ID.IndexOf('_'));

            MeterLogicalAddressEnum meter = MeterLogicalAddressEnum.管理芯;
            if (!ConfigHelper.Instance.IsITOMeter)
            {
                return meter;
            }
            switch (ID)
            {
                case ProjectID.GPS对时:
                case ProjectID.电量清零:
                case ProjectID.钱包初始化:
                case ProjectID.身份认证_计量芯:
                case ProjectID.时钟示值误差:
                case ProjectID.电能示值组合误差:
                case ProjectID.定时冻结:
                case ProjectID.约定冻结:
                case ProjectID.瞬时冻结:
                case ProjectID.日冻结:
                case ProjectID.整点冻结:
                case ProjectID.分钟冻结:
                case ProjectID.小时冻结:
                case ProjectID.月冻结:
                case ProjectID.年冻结:
                case ProjectID.结算日冻结:
                case ProjectID.费控_电量清零:
                    meter = MeterLogicalAddressEnum.计量芯;
                    break;
                default:
                    break;
            }
            return meter;

        }
        /// <summary>
        /// 获取检定项目是否需要复位蓝牙表
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public static bool GetTestItemIsInitIot(string ID)
        {
            if (ID.IndexOf('_') >= 0) ID = ID.Substring(0, ID.IndexOf('_'));
            bool r = false;
            if (!ConfigHelper.Instance.IsITOMeter) return r;
            switch (ID)
            {
                case ProjectID.基本误差试验:
                case ProjectID.误差一致性:
                case ProjectID.误差变差:
                case ProjectID.负载电流升将变差:
                case ProjectID.日计时误差:
                case ProjectID.日计时误差_黑龙江:
                case ProjectID.初始固有误差:
                case ProjectID.初始固有误差试验:
                case ProjectID.起动试验:
                case ProjectID.起动试验_黑龙江:
                case ProjectID.潜动试验:
                case ProjectID.潜动试验_黑龙江:
                case ProjectID.第N次谐波试验:
                case ProjectID.方顶波波形试验:
                case ProjectID.尖顶波波形改变:
                case ProjectID.脉冲群触发波形试验:
                case ProjectID.九十度相位触发波形试验:
                case ProjectID.半波整流波形试验:
                case ProjectID.频率改变:
                case ProjectID.电压改变:
                case ProjectID.负载不平衡试验:
                case ProjectID.辅助装置试验:
                case ProjectID.逆相序试验:
                case ProjectID.一相或两相电压中断试验:
                case ProjectID.高次谐波:
                case ProjectID.标准偏差试验:
                case ProjectID.接线检查:
                case ProjectID.接线检查_黑龙江:
                case ProjectID.负载电流快速改变:
                case ProjectID.工频耐压试验:
                    r = false;
                    break;
                default:
                    r = true;
                    break;
            }
            return r;
        }
    }
}



