using LYTest.Core.Model.Meter;
using LYTest.Core.Model.Schema;
using System.Collections.Generic;
using System.Windows.Forms;

namespace LYTest.Mis.Common
{
    public interface IMis
    {
        /// <summary>
        /// 显示面板
        /// </summary>
        void ShowPanel(Control panel);

        void UpdateInit();

        /// <summary>
        /// 上传检定记录
        /// </summary>
        /// <param name="meterList">数据对象集合</param>
        /// <returns></returns>
        bool Update(TestMeterInfo meter);

        /// <summary>
        /// 全部表上传完成
        /// <paramref name="taskNo">任务单号</paramref>
        /// </summary>
        bool UpdateCompleted();


        /// <summary>
        /// 下载表信息
        /// </summary>
        /// <param name="barCode">条码号</param>
        /// <param name="Item">被检表对象</param>
        /// <returns></returns>
        bool Down(string barcode, ref TestMeterInfo meter);

        /// <summary>
        /// 下载方案
        /// </summary>
        /// <param name="barcode"></param>
        /// <param name="schemeName"></param>
        /// <returns></returns>
        bool SchemeDown(string barcode, out string schemeName, out Dictionary<string, SchemaNode> Schema);
        /// <summary>
        /// 下载方案
        /// </summary>
        /// <param name="barcode"></param>
        /// <param name="schemeName"></param>
        /// <returns></returns>
        bool SchemeDown(TestMeterInfo barcode, out string schemeName, out Dictionary<string, SchemaNode> Schema);
    }
}
