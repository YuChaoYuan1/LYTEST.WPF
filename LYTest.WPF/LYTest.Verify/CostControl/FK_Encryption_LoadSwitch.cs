using LYTest.Core.Enum;
using LYTest.ViewModel.CheckController;
using System;

namespace LYTest.Verify.CostControl
{
    ///add lsj 20220218    负荷开关实验

    ////负荷开关实验
    class FK_Encryption_LoadSwitch : VerifyBase
    {
        /// <summary>
        /// 重写基类测试方法
        /// </summary>
        /// <param name="ItemNumber">检定方案序号</param>
        public override void Verify()
        {
            base.Verify();
            if (Stop) return;
          
            //初始化设备
            if (!InitEquipment()) return;

            //获取所有表的表地址
            if (Stop) return;
            ReadMeterAddrAndNo();

            if (Stop) return;
            Identity();

            if (Stop) return;
            //将GPS时间写到表中
            MessageAdd( "开始写表时间......", EnumLogType.提示信息);
            DateTime readTime = DateTime.Now;
            MeterProtocolAdapter.Instance.WriteDateTime(readTime);

            bool[] bSumResult = new bool[MeterNumber];
            for (int curTime = 0; curTime < 5; curTime++)  //共做5次
            {
                for (int i = 0; i < MeterNumber; i++)
                    bSumResult[i] = true;

                if (Stop) return;
                Identity();

                if (OneMeterInfo.MD_ProtocolName.IndexOf("645")!=-1)
                    SendCostCommand(Cus_EncryptionTrialType.ESAM数据回抄);

                if (Stop) return;
                SendCostCommand(Cus_EncryptionTrialType.解除保电);

                if (Stop) return;
                SendCostCommand(Cus_EncryptionTrialType.远程拉闸);

                for (int i = 0; i < MeterNumber; i++)
                {
                  
                    if (!MeterInfo[i].YaoJianYn) continue;

                    if (EncryptionThread.WorkThreads[i].RemoteControlResult)
                    {
                        ResultDictionary["拉闸"][i] = "√";
                    }
                    else
                    {
                        ResultDictionary["拉闸"][i] = "×";
                        bSumResult[i] = false;
                    }
                }
                RefUIData("拉闸");
              

                WaitTime("拉闸等待时间", 10);
                if (Stop) return;
                MessageAdd("读取状态运行字3", EnumLogType.提示信息);
                string[] strReadData = MeterProtocolAdapter.Instance.ReadData("电表运行状态字3");
              
                for (int i = 0; i < MeterNumber; i++)
                {
                    if (!MeterInfo[i].YaoJianYn) continue;
                    if (strReadData[i] == null || strReadData[i] == "") continue;
                  
                    int chr = Convert.ToInt32(strReadData[i], 16);
                    if (OneMeterInfo.MD_ProtocolName.IndexOf("698") != -1)
                        chr = BitRever(chr, 16);
                    if ((chr & 0x40) == 0x40)
                    {
                        ResultDictionary["拉闸状态字"][i] = "√";
                    }
                    else
                    {
                        ResultDictionary["拉闸状态字"][i] = "×";
                        bSumResult[i] = false;
                    }
                }
                RefUIData("拉闸状态字");
              
                if (Stop) return;
                SendCostCommand(Cus_EncryptionTrialType.直接合闸);
                for (int i = 0; i < MeterNumber; i++)
                {
                 
                    if (!MeterInfo[i].YaoJianYn) continue;
                    if (EncryptionThread.WorkThreads[i].RemoteControlResult)
                    {
                        ResultDictionary["合闸"][i] = "√";
                    }
                    else
                    {
                        ResultDictionary["合闸"][i] = "×";
                        bSumResult[i] = false;
                    }
                }
                RefUIData("合闸");
              
                WaitTime("合闸等待时间", 10);

                if (Stop) return;
                MessageAdd("读取状态运行字3", EnumLogType.提示信息);
                strReadData = MeterProtocolAdapter.Instance.ReadData("电表运行状态字3");
              
                for (int i = 0; i < MeterNumber; i++)
                {
                   
                    if (!MeterInfo[i].YaoJianYn) continue;
                    if (strReadData[i] == null || strReadData[i] == "") continue;
                    int chr = Convert.ToInt32(strReadData[i], 16);
                    if (OneMeterInfo.MD_ProtocolName.IndexOf("698") != -1)
                        chr = BitRever(chr, 16);
                    if ((chr & 0x40) == 0x40)
                    {
                        ResultDictionary["合闸状态字"][i] = "×";
                        bSumResult[i] = false;
                    }
                    else
                    {
                        ResultDictionary["合闸状态字"][i] = "√";
                    }
                }
                RefUIData("合闸状态字");
               
                for (int i = 0; i < MeterNumber; i++)
                {
                    if (!MeterInfo[i].YaoJianYn) continue;
                    ResultDictionary["结论"][i] = bSumResult[i] ? "合格" :"不合格";
                }
                RefUIData("结论");
            }
          
        }
        /// <summary>
        /// 检定参数是否合法
        /// </summary>
        /// <returns></returns>
        protected override bool CheckPara()
        {
            ResultNames = new string[] { "拉闸", "拉闸状态字", "合闸", "合闸状态字","结论" };
            return true;
        }
        /// <summary>
        /// 初始化设备参数,计算每一块表需要检定的圈数
        /// </summary>
        /// <returns></returns>
        protected bool InitEquipment()
        {
            if (Stop) return false;
            if (IsDemo) return true;

            //if (!Helper.EquipHelper.Instance.InitPara_InitEncryption())//是否是密钥下装初始化
            //{
            //    Stop = true;
            //    return false;
            //}
            MessageAdd( "开始升电压...", EnumLogType.提示信息);
            if (Stop) return false;
            if (!PowerOn())
            {
                MessageAdd("升电压失败! ", EnumLogType.提示信息);
                return false;
            }

            return true;
        }
        /// <summary>
        /// 将字节进行二进制反转，只要用于645与698特征字转换
        /// </summary>
        /// <param name="chr">转换的值</param>
        /// <param name="totalWidth">二进制长度</param>
        /// <returns></returns>
        public int BitRever(int chr, int totalWidth)
        {
            string bs = Convert.ToString(chr, 2).PadLeft(totalWidth, '0');
            string s = "";
            for (int i = 0; i < totalWidth; i++)
            {
                s += bs[totalWidth - 1 - i];
            }
            return Convert.ToInt32(s, 2);
        }

    }
}
