using LYTest.DataManager.SG.Mark.ViewModel;
using Word = Microsoft.Office.Interop.Word;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using LYTest.DataManager.SG.ViewModel.Meters;
using LYTest.DataManager.SG.ViewModel;
using LYTest.DataManager.SG.ViewModel.Mark;
using LYTest.Core;

namespace LYTest.DataManager.SG
{
    /// <summary>
    /// 报表帮助类:将电能表的数据打印到模板中,并另存到目标文件夹
    /// </summary>
    class ReportHelper
    {
        private static Word.Application wordApp = null;
        /// <summary>
        /// office应用程序
        /// </summary>
        public static Word.Application WordApp
        {
            get
            {
                if (wordApp == null)
                {
                    wordApp = new Word.Application();
                }
                return wordApp;
            }
        }
        /// <summary>
        /// 打印报表
        /// </summary>
        /// <param name="meters">表信息</param>
        public static void PrintReport(DynamicViewModel meter)
        {
            #region 复制到临时文件
            string fileName = string.Format(@"{0}\ReportTemplate\{1}", Directory.GetCurrentDirectory(), Properties.Settings.Default.ReportPath);
            string destPath = string.Format(@"{0}\Reports", Directory.GetCurrentDirectory());
            string destFileName = string.Format(@"{0}\temp.doc", destPath);
            try
            {
                if (!Directory.Exists(destPath))
                {
                    Directory.CreateDirectory(destPath);
                }
                File.Copy(fileName, destFileName, true);
            }
            catch (Exception e)
            {
                MessageBox.Show(string.Format(@"复制报表模板到文件:{0}失败,失败原因:{1}", destFileName, e.Message));
            }
            #endregion
            #region 打开word文件
            //防止用户关闭word,重新创建
            try
            {
                WordApp.Documents.Add(destFileName);
                if (Properties.Settings.Default.IsPreview)
                {
                    WordApp.Visible = true;
                }
            }
            catch
            {
                wordApp = new Word.Application();
                WordApp.Documents.Add(destFileName);
                if (Properties.Settings.Default.IsPreview)
                {
                    WordApp.Visible = true;
                }
            }
            #endregion
            #region 加载并解析所有书签
            List<int> indexList = new List<int>();
            Word.Document document = WordApp.ActiveDocument;
            List<BookmarkModel> bookmarks = new List<BookmarkModel>();
            foreach (Word.Bookmark bookmark in document.Bookmarks)
            {
                BookmarkModel markModel = new BookmarkModel(bookmark.Name);
                bookmarks.Add(markModel);
                int indexTemp = 0;
                if (int.TryParse(markModel.DeviceName.Replace("表", ""), out indexTemp))
                {
                    if (!indexList.Contains(indexTemp))
                    {
                        indexList.Add(indexTemp);
                    }
                }
            }
            int maxIndex = indexList.Max();
            #endregion
            #region 解析所有书签并赋值
            string meterPk = meter.GetProperty("PK_LNG_METER_ID") as string;
            string equipNo = meter.GetProperty("AVR_DEVICE_ID") as string;
            DynamicViewModel equipInfo = Equipments.Instance.FindEquipInfo(equipNo);
            OneMeterResult meterResultTemp = new OneMeterResult(meterPk, false);
            foreach (BookmarkModel bookmark in bookmarks)
            {
                if (bookmark.DeviceName == "不识别")
                {
                    continue;
                }
                else if (bookmark.DeviceName == "检定台")
                {
                    #region 检定台信息
                    string valueTemp = equipInfo.GetProperty(bookmark.ResultName) as string;
                    SetBookmarkValue(document, bookmark.Name, valueTemp, bookmark.Format);
                    #endregion
                }
                else if (bookmark.DeviceName == "电能表")
                {
                    #region 电表信息
                    InputParaUnit paraUnit = MetersViewModel.ParasModel.AllUnits.FirstOrDefault(item => item.DisplayName == bookmark.ResultName);
                    object objTemp = null;
                    string valueTemp = "";
                    if (paraUnit != null)
                    {
                        objTemp = meter.GetProperty(paraUnit.FieldName);
                    }
                    if (objTemp != null)
                    {
                        valueTemp = objTemp.ToString();
                    }
                    SetBookmarkValue(document, bookmark.Name, valueTemp, bookmark.Format);
                    #endregion
                }
                else
                {
                    #region 解析检定结论
                    int indexTemp = 0;
                    if (int.TryParse(bookmark.DeviceName.Replace("表", ""), out indexTemp))
                    {
                        indexTemp = indexTemp - 1;
                    }
                    if (bookmark.ResultName == "总结论")
                    {
                        SetBookmarkValue(document, bookmark.Name, GetResultSummaray(meterResultTemp, bookmark.ItemKey), bookmark.Format);
                    }
                    else
                    {
                        DynamicViewModel modelTemp = GetResultModel(meterResultTemp, bookmark.ItemKey);
                        //书签的值
                        string markValueTemp = "";
                        if (modelTemp != null)
                        {
                            markValueTemp = modelTemp.GetProperty(bookmark.ResultName) as string;
                        }
                        //转到书签并给书签赋值
                        SetBookmarkValue(document, bookmark.Name, markValueTemp, bookmark.Format);
                    }
                    #endregion
                }
            }
            #endregion
            if (!Properties.Settings.Default.IsPreview)
            {
                try
                {
                    document.PrintOut();
                }
                catch (Exception e)
                {
                    MessageBox.Show(string.Format("打印报表文件出错:{0}", e.Message));
                }
                document.Close(Word.WdSaveOptions.wdDoNotSaveChanges);
            }
        }

        /// <summary>
        /// 打印报表
        /// </summary>
        /// <param name="meters">表信息</param>
        public static void PrintReport(List<DynamicViewModel> meter)
        {
            #region 复制到临时文件
            string fileName = string.Format(@"{0}\ReportTemplate\{1}", Directory.GetCurrentDirectory(), Properties.Settings.Default.ReportPath);
            string destPath = string.Format(@"{0}\Reports", Directory.GetCurrentDirectory());
            string destFileName = string.Format(@"{0}\temp.doc", destPath);
            try
            {
                if (!Directory.Exists(destPath))
                {
                    Directory.CreateDirectory(destPath);
                }
                File.Copy(fileName, destFileName, true);
            }
            catch (Exception e)
            {
                MessageBox.Show(string.Format(@"复制报表模板到文件:{0}失败,失败原因:{1}", destFileName, e.Message));
            }
            #endregion
            #region 打开word文件
            //防止用户关闭word,重新创建
            try
            {
                WordApp.Documents.Add(destFileName);
                if (Properties.Settings.Default.IsPreview)
                {
                    WordApp.Visible = true;
                }
            }
            catch
            {
                wordApp = new Word.Application();
                WordApp.Documents.Add(destFileName);
                if (Properties.Settings.Default.IsPreview)
                {
                    WordApp.Visible = true;
                }
            }
            #endregion
            #region 加载并解析所有书签
            List<int> indexList = new List<int>();
            Word.Document document = WordApp.ActiveDocument;
            List<BookmarkModel> bookmarks = new List<BookmarkModel>();
            int MeterIndexTemp = 0;
            foreach (Word.Bookmark bookmark in document.Bookmarks)
            {
                BookmarkModel markModel = new BookmarkModel(bookmark.Name);
                //bookmarks.Add(markModel);
                //int indexTemp = 0;
                //if (int.TryParse(markModel.DeviceName.Replace("表", ""), out indexTemp))
                //{
                //}
                string strMeterIndex = bookmark.Name.Replace("VX", "^").Split('^')[0];
                if (!string.IsNullOrEmpty(strMeterIndex) && strMeterIndex.Contains("表"))
                {
                    int.TryParse(strMeterIndex.Replace("表", ""), out MeterIndexTemp);
                    if (MeterIndexTemp <= meter.Count)
                    {
                        MeterIndexTemp = MeterIndexTemp - 1;
                    }
                    else
                    {
                        SetBookmarkValue(document, bookmark.Name, "/", markModel.Format);
                        continue;
                    }
                }

                string meterPk = meter[MeterIndexTemp].GetProperty("PK_LNG_METER_ID") as string;
                string equipNo = meter[MeterIndexTemp].GetProperty("AVR_DEVICE_ID") as string;
                DynamicViewModel equipInfo = Equipments.Instance.FindEquipInfo(equipNo);
                OneMeterResult meterResultTemp = new OneMeterResult(meterPk, false);

                if (markModel.DeviceName == "不识别")
                {
                    continue;
                }
                else if (markModel.DeviceName == "检定台")
                {
                    #region 检定台信息
                    string valueTemp = equipInfo.GetProperty(markModel.ResultName) as string;
                    SetBookmarkValue(document, bookmark.Name, valueTemp, markModel.Format);
                    #endregion
                }
                else if (markModel.DeviceName == "电能表")
                {
                    #region 电表信息
                    InputParaUnit paraUnit = MetersViewModel.ParasModel.AllUnits.FirstOrDefault(item => item.DisplayName == markModel.ResultName);
                    object objTemp = null;
                    string valueTemp = "";
                    if (paraUnit != null)
                    {
                        objTemp = meter[MeterIndexTemp].GetProperty(paraUnit.FieldName);
                    }
                    if (objTemp != null)
                    {
                        valueTemp = objTemp.ToString();
                    }
                    SetBookmarkValue(document, bookmark.Name, valueTemp, markModel.Format);
                    #endregion
                }
                else
                {
                    #region 解析检定结论
                    //int indexTemp = 0;
                    //if (int.TryParse(bookmark.DeviceName.Replace("表", ""), out indexTemp))
                    //{
                    //    indexTemp = indexTemp - 1;
                    //}
                    if (markModel.ResultName == "总结论")
                    {
                        SetBookmarkValue(document, bookmark.Name, GetResultSummaray(meterResultTemp, markModel.ItemKey), markModel.Format);
                    }
                    else
                    {
                        DynamicViewModel modelTemp = GetResultModel(meterResultTemp, markModel.ItemKey);
                        //书签的值
                        string markValueTemp = "";
                        if (modelTemp != null)
                        {
                            markValueTemp = modelTemp.GetProperty(markModel.ResultName) as string;
                        }
                        //转到书签并给书签赋值
                        SetBookmarkValue(document, bookmark.Name, markValueTemp, markModel.Format);
                    }
                    #endregion
                }
            }
            #endregion
            if (!Properties.Settings.Default.IsPreview)
            {
                try
                {
                    document.PrintOut();
                }
                catch (Exception e)
                {
                    MessageBox.Show(string.Format("打印报表文件出错:{0}", e.Message));
                }
                document.Close(Word.WdSaveOptions.wdDoNotSaveChanges);
            }
        }

        /// <summary>
        /// 给书签设置值
        /// </summary>
        /// <param name="markName"></param>
        /// <param name="valueTemp"></param>
        private static void SetBookmarkValue(Word.Document document, string markName, string valueTemp, EnumFormat formatTemp = EnumFormat.无)
        {
            //转到书签并给书签赋值
            Word.Bookmark markTemp = null;
            foreach (Word.Bookmark mark in document.Bookmarks)
            {
                if (mark.Name == markName)
                {
                    markTemp = mark;
                    break;
                }
            }
            if (markTemp != null)
            {
                markTemp.Range.Select();
                markTemp.Range.Text = AnalysisText(valueTemp, formatTemp);
            }
        }

        /// <summary>
        /// 获取对应编号的检定结论
        /// </summary>
        /// <param name="meterResult"></param>
        /// <param name="itemKey"></param>
        /// <returns></returns>
        private static DynamicViewModel GetResultModel(OneMeterResult meterResult, string itemKey)
        {
            DynamicViewModel modelResult = null;
            for (int i = 0; i < meterResult.Categories.Count; i++)
            {
                modelResult = meterResult.Categories[i].ResultUnits.FirstOrDefault(item => item.GetProperty("项目号") as string == itemKey);
                if (modelResult != null)
                {
                    break;
                }
            }
            return modelResult;
        }
        /// <summary>
        /// 获取类别的总结论
        /// </summary>
        /// <param name="meterResult"></param>
        /// <param name="paraNo"></param>
        /// <returns></returns>
        private static string GetResultSummaray(OneMeterResult meterResult, string paraNo)
        {
            string stringResult = ConstHelper.合格;
            bool havePoint = false;
            for (int i = 0; i < meterResult.Categories.Count; i++)
            {
                var models = meterResult.Categories[i].ResultUnits.Where(item => (item.GetProperty("项目号") as string) != null && (item.GetProperty("项目号") as string).StartsWith(paraNo));
                if (models != null)
                {
                    foreach (DynamicViewModel modelTemp in models)
                    {
                        havePoint = true;
                        if ((modelTemp.GetProperty("结论") as string) == ConstHelper.不合格)
                        {
                            stringResult = ConstHelper.不合格;
                            return stringResult;
                        }
                        else if ((modelTemp.GetProperty("结论") as string) != ConstHelper.合格)
                        {
                            stringResult = "";
                        }
                    }
                }
            }
            if (!havePoint)
            {
                stringResult = "";
            }
            return stringResult;
        }

        #region 解析数据格式
        /// <summary>
        /// 根据数据格式解析文本
        /// </summary>
        /// <param name="valueTemp">要解析的文本</param>
        /// <param name="formatTemp">格式类型</param>
        /// <returns></returns>
        private static string AnalysisText(string valueTemp, EnumFormat formatTemp)
        {
            string valueResult = valueTemp;
            if (valueResult == ConstHelper.合格)
            {
                valueResult = "符合要求";
            }
            else if (valueResult == ConstHelper.不合格)
            {
                valueResult = "不符合要求";
            }
            else if (valueResult == "")
            {
                valueResult = "/";
            }

            switch (formatTemp)
            {
                case EnumFormat.年:
                case EnumFormat.月:
                case EnumFormat.日:
                    valueResult = AnalysisTimeText(valueTemp, formatTemp);
                    break;
                case EnumFormat.额定电流:
                case EnumFormat.最大电流:
                case EnumFormat.有功等级:
                case EnumFormat.无功等级:
                case EnumFormat.有功常数:
                case EnumFormat.无功常数:
                    valueResult = AnalysisCurrentText(valueTemp, formatTemp);
                    break;
                case EnumFormat.格式化电压:
                    valueResult = AnalysisVoltageText(valueTemp, formatTemp);
                    break;
            }
            return valueResult;
        }
        /// <summary>
        /// 解析日期格式
        /// </summary>
        /// <param name="valueTemp"></param>
        /// <param name="formatTemp"></param>
        /// <returns></returns>
        private static string AnalysisTimeText(string valueTemp, EnumFormat formatTemp)
        {
            DateTime timeTemp = DateTime.Now;
            if (DateTime.TryParse(valueTemp, out timeTemp))
            {
                switch (formatTemp)
                {
                    case EnumFormat.年:
                        return timeTemp.ToString("yyyy");
                    case EnumFormat.月:
                        return timeTemp.ToString("MM");
                    case EnumFormat.日:
                        return timeTemp.ToString("dd");
                }
            }
            return valueTemp;
        }
        /// <summary>
        /// 解析电流格式
        /// </summary>
        /// <param name="valueTemp"></param>
        /// <param name="formatTemp"></param>
        /// <returns></returns>
        private static string AnalysisCurrentText(string valueTemp, EnumFormat formatTemp)
        {
            if (valueTemp == null)
            {
                return "";
            }
            string[] arrayTemp = valueTemp.Split('(');
            if (arrayTemp.Length == 2)
            {
                if (formatTemp == EnumFormat.额定电流 || formatTemp == EnumFormat.有功等级 || formatTemp == EnumFormat.有功常数)
                {
                    return arrayTemp[0];
                }
                else
                {
                    return arrayTemp[1].Replace(")", "");
                }
            }
            return valueTemp;
        }

        /// <summary>
        /// 解析电压格式带最大电压
        /// </summary>
        /// <param name="valueTemp"></param>
        /// <param name="formatTemp"></param>
        /// <returns></returns>
        private static string AnalysisVoltageText(string valueTemp, EnumFormat formatTemp)
        {
            if (!string.IsNullOrEmpty(valueTemp))
            {
                if (valueTemp == "57.7")
                {
                    valueTemp = "57.7/100";
                }
                else if (valueTemp == "220")
                {
                    valueTemp = "220/380";
                }
            }
            return valueTemp;
        }
        #endregion
    }
}
