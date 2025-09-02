using LYTest.ViewModel.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LYTest.ViewModel.Log
{

    /// <summary>
    /// 日志集合 【备注001:日志总类，添加时间2021-07-21还不完善，后期在做修改。】
    /// </summary>
    public class LogViewModelS : ViewModelBase
    {
       //设备日志绑定可以不改变，终端日志在写一个类来存放，分别绑定
        private List<LogViewModel> logViews;//日志列表

        public List<LogViewModel> LogViews
        {
            get { return logViews; }
            set { SetPropertyValue(value, ref logViews, "LogViews"); }
        }


        public LogViewModelS()
        {
            logViews = new List<LogViewModel>();
            logViews.Add(new LogViewModel() { Name="设备日志"});  //第一个为设备日志
            for (int i = 1; i < EquipmentData.Equipment.MeterCount+1; i++)  //终端日志
            {
                logViews.Add(new LogViewModel() { Name="终端"+i.ToString().PadLeft(2,'0')});
            }
        }

    }
}
