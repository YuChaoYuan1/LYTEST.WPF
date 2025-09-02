using LYTest.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LYTest.ViewModel.Time
{
    /// <summary>
    /// 单个检定点的时间管理
    /// </summary>
    public class TimeItem : ViewModelBase
    {
        private bool? isValid = null;
        /// <summary>
        /// 时间记录有效标记:
        /// null:初始状态,
        /// false:无效的时间统计-手动停止的不统计-没有传回结论的检定项不统计,
        /// true:有效的时间统计
        /// </summary>
        public bool? IsValid
        {
            get
            {
                return isValid;
            }
            set
            {
                if (isValid == null || isValid.Value)
                {
                    isValid = value;
                }
                if (isValid.HasValue && isValid.Value)
                {
                    SaveTimeItem();
                }
            }
        }
        /// <summary>
        /// 检定开始时间
        /// </summary>
        private DateTime timeBegin = new DateTime();
        /// <summary>
        /// 已用时间对应字符串
        /// </summary>
        public string StringPastTime
        {
            get { return GetTimeString(pastTime); }
        }
        private int pastTime = 0;
        /// <summary>
        /// 当前运行时间(秒)
        /// </summary>
        public int PastTime
        {
            get
            {
                if (isRunning)
                {
                    pastTime = (int)((DateTime.Now - timeBegin).TotalSeconds);
                }
                return pastTime;
            }
        }
        /// <summary>
        /// 总时间对应字符串
        /// </summary>
        public string StringTotalTime
        {
            get { return GetTimeString(totalTime); }
        }
        private int totalTime = 60;
        /// <summary>
        /// 项目预计总时间
        /// </summary>
        public int TotalTime
        {
            get { return totalTime; }
            set
            {
                SetPropertyValue(value, ref totalTime, "TotalTime");
                OnPropertyChanged("StringTotalTime");
            }
        }
        /// <summary>
        /// 更新一下运行时间
        /// </summary>
        public void Tick()
        {
            OnPropertyChanged("PastTime");
            OnPropertyChanged("StringPastTime");
            OnPropertyChanged("TextTooltip");
        }
        private bool isRunning = false;
        /// <summary>
        /// 开始统计
        /// </summary>
        public void Start()
        {
            timeBegin = DateTime.Now;
            isRunning = true;
            Tick();
        }
        /// <summary>
        /// 当前项结束时掉用,用来对时间进行统计
        /// </summary>
        public void FinishItem()
        {
            if (isRunning)
            {
                pastTime = (int)((DateTime.Now - timeBegin).TotalSeconds);
                isRunning = false;
                SaveTimeItem();
            }
        }
        private DynamicModel dbModel = null;
        private string where = "";
        /// <summary>
        /// 初始化信息
        /// </summary>
        public void Intialize()
        {
            isRunning = false;
            string[] conditonArray = new string[3];
            conditonArray[0] = string.Format("PARA_NO = '{0}'", CheckModel.ParaNo);
            string paraValue = CheckModel.ParaValue;
            if (paraValue == null)
            {
                conditonArray[1] = "PARA_VALUE is null";
            }
            else
            {
                conditonArray[1] = string.Format("PARA_VALUE ='{0}' ", paraValue);
            }
            string meterInfo = CheckModel.MeterInfo;
            if (meterInfo == null)
            {
                conditonArray[2] = "METER_INFO is null";
            }
            else
            {
                conditonArray[2] = string.Format("METER_INFO ='{0}' ", meterInfo);
            }
            where = string.Join(" and ", conditonArray);
            #region 加载项目的时间
            dbModel = DALManager.ApplicationDbDal.GetByID(EnumAppDbTable.TIME_STATISTICS.ToString(), where);
            if (dbModel != null)
            {
                string averageString = dbModel.GetProperty("AVERAGE_TIME") as string;
                int intTemp = 0;
                if (int.TryParse(averageString, out intTemp))
                {
                    TotalTime = intTemp;
                }
            }
            else
            {
                TotalTime = 60;
            }
            #endregion
        }
        private bool isSelected = true;
        /// <summary>
        /// 选中
        /// </summary>
        public bool IsSelected
        {
            get { return isSelected; }
            set { isSelected = value; }
        }
        /// <summary>
        /// 检定项信息
        /// </summary>
        public CheckModelTime CheckModel { get; set; }
        /// <summary>
        /// 保存当前项目的时间,只有在IsValid和IsFinished的时候才会执行保存
        /// 有的时候项目有效标记(收到结论通知)会晚于切换检定点
        /// </summary>
        private void SaveTimeItem()
        {
            //如果不是有效或者在检定中,不保存
            if (isValid == null || !isValid.Value || isRunning)
            {
                return;
            }
            #region 更新当前统计到数据库
            if (dbModel == null)
            {
                dbModel = new DynamicModel();
                dbModel.SetProperty("PARA_NO", CheckModel.ParaNo);
                dbModel.SetProperty("PARA_VALUE", CheckModel.ParaValue);
                dbModel.SetProperty("METER_INFO", CheckModel.MeterInfo);
                dbModel.SetProperty("LAST_TIME", pastTime.ToString());
                dbModel.SetProperty("AVERAGE_TIME", pastTime.ToString());
                dbModel.SetProperty("CHECK_TIMES", "1");
                TotalTime = pastTime;
            }
            else
            {
                string checkTimesString = dbModel.GetProperty("CHECK_TIMES") as string;
                int intTemp = 0;
                if (int.TryParse(checkTimesString, out intTemp))
                {
                    dbModel.SetProperty("CHECK_TIMES", (intTemp + 1).ToString());
                    dbModel.SetProperty("LAST_TIME", pastTime.ToString());
                    int averageTime = (TotalTime * intTemp + pastTime) / (intTemp + 1);
                    dbModel.SetProperty("AVERAGE_TIME", averageTime.ToString());
                    TotalTime = averageTime;
                }
            }
            //if (DALManager.ApplicationDbDal.GetCount(EnumAppDbTable.TIME_STATISTICS.ToString(), where) > 0)
            //{
            //    DALManager.ApplicationDbDal.Update(EnumAppDbTable.TIME_STATISTICS.ToString(), where, dbModel, new List<string>() { "CHECK_TIMES", "LAST_TIME", "AVERAGE_TIME" });
            //}
            //else
            //{
            //    DALManager.ApplicationDbDal.Insert(EnumAppDbTable.TIME_STATISTICS.ToString(), dbModel);
            //}
            #endregion

            isValid = null;
        }

        public static string GetTimeString(int seconds)
        {
            TimeSpan timeSpan = new TimeSpan(0, 0, seconds);
            return timeSpan.ToString();
        }

        public string TextTooltip
        {
            get { return string.Format("分项预计时间:{0},已过去:{1}", StringTotalTime, StringPastTime); }
        }
    }
}
