using LYTest.MeterProtocol.Enum;
using LYTest.MeterProtocol.Struct;

namespace LYTest.MeterProtocol
{
    public class App
    {
        /// <summary>
        ///  载波协议信息
        /// </summary>
        public static CarrierWareInfo CarrierInfo { get; set; } = new CarrierWareInfo();
        /// <summary>
        /// 当前载波配置
        /// </summary>
        public static CarrierWareInfo[] CarrierInfos = null;
        /// <summary>
        /// 载波当前表位
        /// </summary>
        public static int Carrier_Cur_BwIndex = 0;

        /// <summary>
        /// 通讯类型
        /// </summary>
        public static Cus_ChannelType g_ChannelType = Cus_ChannelType.通讯485;

        public static LY3762 LY3762 { get; set; }

        /// <summary>
        /// 通讯协议--主要用来载波组帧时候判断使用
        /// </summary>
        public static string Protocols = "698";

        /// <summary>
        ///  表内逻辑地址---管理芯片05，计量芯片15--其他扩展模组地址不一样，多了一位，后续补充
        /// </summary>
        public static string LogicalAddress = "05";


        //add yjt jz 20220822 新增参数
        #region 2022.06.11 载波修改——JX
        /// <summary>
        /// 查询芯片添加子节点用的
        /// </summary>
        public static int MeterNumber = 60;
        #endregion
    }
}
