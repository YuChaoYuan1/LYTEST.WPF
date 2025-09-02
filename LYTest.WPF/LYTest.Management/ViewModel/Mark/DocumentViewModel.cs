using Word = Microsoft.Office.Interop.Word;
using System.Windows;
using LYTest.DataManager.ViewModel;
using System;
using System.IO;
using System.Linq;

namespace LYTest.DataManager.Mark.ViewModel
{
    class DocumentViewModel : ViewModelBase
    {
        public DocumentViewModel()
        {
            ResultMaker.EventAddBookmark += Maker_EventAddBookmark;
            MeterInfoMaker.EventAddBookmark += Maker_EventAddBookmark;
            EquipInfoMaker.EventAddBookmark += Maker_EventAddBookmark;
        }

        /// <summary>
        /// 添加书签
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Maker_EventAddBookmark(object sender, System.EventArgs e)
        {
            try
            {
                string nameTemp = sender as string;
                if (!string.IsNullOrEmpty(nameTemp))
                {
                    if (WordDocument != null)
                    {
                        string temp = "待替换";
                        WordDocument.Application.Selection.Text = temp;
                        object range = WordDocument.Application.Selection;

                        string name1=nameTemp;
                        int indexTemp = 0;
                        while (true)
                        {
                            BookmarkModel modelTemp = Bookmarks.FirstOrDefault(item => item.Name == name1);
                            if (modelTemp == null)
                            {
                                break;
                            }
                            else
                            {
                                indexTemp++;
                                name1 = string.Format("{0}VY{1}", nameTemp, indexTemp);
                            }
                        }

                        WordDocument.Bookmarks.Add(name1, ref range);
                        Bookmarks.Add(new BookmarkModel(name1));
                        WordApp.Activate();
                    }
                    else
                    {
                        MessageBox.Show("插入书签之前请先打开要编辑的报表模板!!", "请先打开报表模板", MessageBoxButton.OK);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageDisplay.Instance.Message = string.Format("获取报表模板失败:{0}", ex.Message);
            }
        }

        #region Word文档相关
        private static Word.Application wordApp = new Word.Application();
        /// <summary>
        /// Word应用程序
        /// </summary>
        public static Word.Application WordApp
        {
            get
            {
                return wordApp;
            }
        }
        /// <summary>
        /// Word文档
        /// </summary>
        public Word.Document WordDocument
        {
            get
            {
                try
                {
                    return WordApp.ActiveDocument;
                }
                catch
                {
                    OpenTemplate();
                    return WordApp.ActiveDocument;
                }
            }
        }
        /// <summary>
        /// 加载文档的所有书签
        /// </summary>
        public void OpenTemplate()
        {
            string fileName = string.Format(@"{0}\ReportTemplate\{1}", Directory.GetCurrentDirectory(), Properties.Settings.Default.ReportPath);
            try
            {
                MessageDisplay.Instance.Message = string.Format("正在打开报表模板,文件路径为:{0}", fileName);
                WordApp.Documents.Open(fileName);
                WordApp.Visible = true;
                //WordApp.ActiveWindow.WindowState = Word.WdWindowState.wdWindowStateNormal;
            }
            catch
            {
                try
                {
                    wordApp = new Word.Application();
                    WordApp.Documents.Open(fileName);
                    WordApp.Visible = true;
                    WordApp.ActiveWindow.WindowState = Word.WdWindowState.wdWindowStateNormal;
                }
                catch (Exception ex)
                {
                    MessageDisplay.Instance.Message = string.Format("打开报表模板失败:{0}", ex.Message);
                }
            }

            Bookmarks.Clear();

            Word.Document documentTemp = WordApp.ActiveDocument;

            if (documentTemp != null)
            {
                foreach (Word.Bookmark bookmark in documentTemp.Bookmarks)
                {
                    Bookmarks.Add(new BookmarkModel(bookmark.Name));
                }
            }
        }

        /// <summary>
        /// 书签列表
        /// </summary>
        public AsyncObservableCollection<BookmarkModel> Bookmarks { get; } = new AsyncObservableCollection<BookmarkModel>();

        #endregion

        #region 检定结论相关
        private ResultBookmarkMaker resultMaker = new ResultBookmarkMaker();
        /// <summary>
        /// 检定结论书签制作器
        /// </summary>
        public ResultBookmarkMaker ResultMaker
        {
            get { return resultMaker; }
            set { SetPropertyValue(value, ref resultMaker, "ResultMaker"); }
        }
        #endregion

        #region 表信息
        private MeterBookmarkMaker meterInfoMaker = new MeterBookmarkMaker();
        /// <summary>
        /// 表信息书签制作器
        /// </summary>
        public MeterBookmarkMaker MeterInfoMaker
        {
            get { return meterInfoMaker; }
            set { SetPropertyValue(value, ref meterInfoMaker, "MeterInfoMaker"); }
        }
        #endregion

        #region 台体信息
        private EquipmentBookmarkMaker equipInfoMaker = new EquipmentBookmarkMaker();
        /// <summary>
        /// 台体信息书签制作器
        /// </summary>
        public EquipmentBookmarkMaker EquipInfoMaker
        {
            get { return equipInfoMaker; }
            set { SetPropertyValue(value, ref equipInfoMaker, "EquipInfoMaker"); }
        }
        #endregion

        #region 删除书签
        /// <summary>
        /// 删除书签
        /// </summary>
        /// <param name="model"></param>
        public void DeleteBookmark(BookmarkModel model)
        {
            try
            {
                foreach (Word.Bookmark mark in WordDocument.Bookmarks)
                {
                    if (mark.Name == model.Name)
                    {
                        mark.Range.Delete();
                        Bookmarks.Remove(model);
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                MessageDisplay.Instance.Message = string.Format("获取报表模板失败:{0}", e.Message);
            }
        }
        public void ClearBookmarks()
        {
            try
            {
                foreach (Word.Bookmark mark in WordDocument.Bookmarks)
                {
                    mark.Range.Delete();
                }
                Bookmarks.Clear();
            }
            catch (Exception e)
            {
                MessageDisplay.Instance.Message = string.Format("获取报表模板失败:{0}", e.Message);
            }
        }
        /// <summary>
        /// 选中书签,并在Word控件上面显示
        /// </summary>
        /// <param name="model"></param>
        public void SelectBookmark(BookmarkModel model)
        {
            try
            {
                foreach (Word.Bookmark mark in WordDocument.Bookmarks)
                {
                    if (mark.Name == model.Name)
                    {
                        mark.Range.Select();
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                MessageDisplay.Instance.Message = string.Format("获取报表模板失败:{0}", e.Message);
            }
        }
        #endregion
    }
}