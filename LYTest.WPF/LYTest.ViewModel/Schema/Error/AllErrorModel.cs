using LYTest.Core.Enum;
using LYTest.DAL;
using LYTest.ViewModel.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LYTest.ViewModel.Schema.Error
{
    /// <summary>
    /// 所有检定点集合
    /// </summary>
    public class AllErrorModel : ViewModelBase
    {
        private AsyncObservableCollection<ErrorCategory> categories = new AsyncObservableCollection<ErrorCategory>();

        public AsyncObservableCollection<ErrorCategory> Categories
        {
            get { return categories; }
            set { categories = value; }
        }

        public void Load(List<DynamicModel> paraValues)
        {
            for (int i = 0; i < Categories.Count; i++)
            {
                Categories[i].PointsChanged -= Category_PointsChanged;
                Categories[i].ErrorPoints.Clear();
            }
            Categories.Clear();

            //误差试验类型|功率方向|功率元件|功率因数|电流倍数|添加谐波|逆相序
            for (int i = 0; i < paraValues.Count; i++)
            {
                string stringPoint = paraValues[i].GetProperty("PARA_VALUE") as string;
                string[] arrayPara = stringPoint.Split('|');

                if (paraValues[i].GetProperty("PARA_NO") as string == ProjectID.初始固有误差)
                {
                    if (arrayPara.Length >= 4)
                    {
                        ErrorCategory category = Categories.FirstOrDefault(item => item.Fangxiang == arrayPara[0] && item.Component == arrayPara[1]);
                        if (category == null)
                        {
                            category = new ErrorCategory
                            {
                                Fangxiang = arrayPara[0],
                                Component = arrayPara[1]
                            };
                            category.PointsChanged += Category_PointsChanged;
                            Categories.Add(category);
                        }
                        ErrorModel errorPoint = category.ErrorPoints.FirstOrDefault(item => item.Current == arrayPara[3] && item.Factor == arrayPara[2]);
                        if (errorPoint == null)
                        {
                            category.ErrorPoints.Add(new ErrorModel
                            {
                                FangXiang = category.Fangxiang,
                                Component = category.Component,
                                Current = arrayPara[3],
                                Factor = arrayPara[2]
                            });
                        }
                    }
                }
                else
                {
                    if (arrayPara.Length >= 7)
                    {
                        ErrorCategory category = Categories.FirstOrDefault(item => item.Fangxiang == arrayPara[1] && item.Component == arrayPara[2] && item.ErrorType == arrayPara[0]);
                        if (category == null)
                        {
                            category = new ErrorCategory
                            {
                                ErrorType = arrayPara[0],
                                Fangxiang = arrayPara[1],
                                Component = arrayPara[2]
                            };
                            category.PointsChanged += Category_PointsChanged;
                            Categories.Add(category);
                        }
                        //误差点不会重复,如果想重复
                        //把这个为空的判断取消掉
                        //但是界面上处理,取消检定点的时候只会删除一个点
                        ErrorModel errorPoint = category.ErrorPoints.FirstOrDefault(item => item.Current == arrayPara[4] && item.Factor == arrayPara[3]);
                        if (errorPoint == null)
                        {
                            category.ErrorPoints.Add(new ErrorModel
                            {
                                FangXiang = category.Fangxiang,
                                Component = category.Component,
                                ErrorType = category.ErrorType,
                                Current = arrayPara[4],
                                Factor = arrayPara[3]
                            });
                        }
                    }
                }


            }
            for (int i = 0; i < Categories.Count; i++)
            {
                Categories[i].FlagLoad = true;
            }
        }

        public void AddCategory()
        {
            ErrorCategory category = new ErrorCategory();
            //if (Categories.Any(item => item.Fangxiang == ""))
            //{

            //}//TODO 这里其实需要自动的判断如功率方向等，应该自动添加没有的项目  

            category.PointsChanged += Category_PointsChanged;
            Categories.Add(category);
        }


        //private string GetFangXian(int index)
        //{
        //    string str = "";
        //    switch (index)
        //    {
        //        case 0:
        //            str = "正向有功";
        //            break;
        //        case 1:
        //            str = "反向有功";
        //            break;
        //        case 2:
        //            str = "正向无功";
        //            break;
        //        case 3:
        //            str = "反向无功";
        //            break;
        //        default:
        //            break;
        //    }
        //    return str;
        //}

        void Category_PointsChanged(object sender, EventArgs e)
        {
            PointsChanged?.Invoke(sender, e);
        }

        /// <summary>
        /// 检定点数量发生变化时,sender:变化的检定点.e:0:移除,1:添加
        /// </summary>
        public event EventHandler PointsChanged;
        private string lapCountIb = "2";
        /// <summary>
        /// 相对于Ib圈数
        /// </summary>
        public string LapCountIb
        {
            get { return lapCountIb; }
            set
            {
                if (lapCountIb != value)
                {
                    for (int i = 0; i < Categories.Count; i++)
                    {
                        Categories[i].LapCountIb = value;
                        for (int j = 0; j < Categories[i].ErrorPoints.Count; j++)
                        {
                            int.TryParse(Categories[i].LapCountIb, out int intLapCountIb);

                            float abc = 1;
                            if (!("H".Equals(Categories[i].ErrorPoints[j].Component))) abc = 0.34f;

                            if (!float.TryParse(Categories[i].ErrorPoints[j].Factor.ToUpper().Replace("L", "").Replace("C", ""), out float pf)) pf = 1.0f;

                            float xib = 1;
                            if (Categories[i].ErrorPoints[j].Current.IndexOf("Imax") >= 0)
                            {
                                if (string.IsNullOrWhiteSpace(Categories[i].ErrorPoints[j].Current.Replace("Imax", ""))) xib = 4;
                                else
                                {
                                    if (!float.TryParse(Categories[i].ErrorPoints[j].Current.Replace("Imax", ""), out float xItmp)) xItmp = 1;

                                    xib = xItmp * 4;
                                }
                            }
                            else if (Categories[i].ErrorPoints[j].Current.IndexOf("Ib") >= 0)
                            {
                                if (string.IsNullOrWhiteSpace(Categories[i].ErrorPoints[j].Current.Replace("Ib", ""))) xib = 1;
                                else
                                {
                                    if (!float.TryParse(Categories[i].ErrorPoints[j].Current.Replace("Ib", ""), out float xItmp)) xItmp = 1;

                                    xib = xItmp * 1;
                                }
                            }
                            else if (Categories[i].ErrorPoints[j].Current.IndexOf("Itr") >= 0)
                            {
                                if (string.IsNullOrWhiteSpace(Categories[i].ErrorPoints[j].Current.Replace("Itr", ""))) xib = 1;
                                else
                                {
                                    if (!float.TryParse(Categories[i].ErrorPoints[j].Current.Replace("Itr", ""), out float xItmp)) xItmp = 1;

                                    xib = xItmp * 1;
                                }
                            }
                            else
                            {
                                xib = 1;
                            }

                            int curPulses = (int)Math.Ceiling(abc * xib * intLapCountIb * pf);
                            if (curPulses < 1) curPulses = 1;

                            Categories[i].ErrorPoints[j].LapCountIb = curPulses.ToString();
                        }
                    }
                    SetPropertyValue(value, ref lapCountIb, "LapCountIb");
                }
            }
        }
        private string guichengMulti = "100";
        /// <summary>
        /// 规程误差限倍数
        /// </summary>
        public string GuichengMulti
        {
            get { return guichengMulti; }
            set
            {
                if (guichengMulti != value)
                {
                    SetPropertyValue(value, ref guichengMulti, "GuichengMulti");
                    for (int i = 0; i < Categories.Count; i++)
                    {
                        Categories[i].GuichengMulti = GuichengMulti;
                        for (int j = 0; j < Categories[i].ErrorPoints.Count; j++)
                        {
                            Categories[i].ErrorPoints[j].GuichengMulti = GuichengMulti;
                        }
                    }
                }
            }
        }

    }
}
