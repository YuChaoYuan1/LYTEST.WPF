namespace LYTest.Core.Enum
{ /// <summary>
  /// 通讯选择
  /// </summary>
    public enum Cus_LightSelect
    {
        一对一模式485通讯 = 0,
        奇数表位485通讯 = 1,
        偶数表位485通讯 = 2,
        一对一模式红外通讯 = 3,
        奇数表位红外通讯 = 4,
        偶数表位红外通讯 = 5,
        切换到485总线 = 6       //电科院协议用
    }
}
