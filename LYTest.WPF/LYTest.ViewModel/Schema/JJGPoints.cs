using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LYTest.ViewModel.Schema
{
    /// <summary>
    /// 规程选点
    /// </summary>
    internal class JJGPoints
    {
        #region 直接式 3、2、1
        /// <summary>
        /// 直接式单相、平衡负载有功2级
        /// </summary>
        /// <param name="Imax4Ib"></param>
        /// <param name="errFRAR">正向有功、反向有功、正向无功、反向无功</param>
        /// <returns></returns>
        internal virtual List<string> BalanceActiveClass2(bool Imax4Ib, string errFRAR)
        {
            List<string> errPoint = new List<string>();
            errPoint.Add(string.Join("|", "基本误差", errFRAR, "H", "1.0", "Imax", "否", "否", "2", "100"));
            errPoint.Add(string.Join("|", "基本误差", errFRAR, "H", "0.5L", "Imax", "否", "否", "2", "100"));
            if (Imax4Ib)
            {
                errPoint.Add(string.Join("|", "基本误差", errFRAR, "H", "1.0", "0.5Imax", "否", "否", "2", "100"));
                errPoint.Add(string.Join("|", "基本误差", errFRAR, "H", "0.5L", "0.5Imax", "否", "否", "2", "100"));
            }
            errPoint.Add(string.Join("|", "基本误差", errFRAR, "H", "1.0", "Ib", "否", "否", "2", "100"));
            errPoint.Add(string.Join("|", "基本误差", errFRAR, "H", "0.5L", "Ib", "否", "否", "2", "100"));
            errPoint.Add(string.Join("|", "基本误差", errFRAR, "H", "0.5L", "0.2Ib", "否", "否", "2", "100"));
            errPoint.Add(string.Join("|", "基本误差", errFRAR, "H", "1.0", "0.1Ib", "否", "否", "2", "100"));
            errPoint.Add(string.Join("|", "基本误差", errFRAR, "H", "0.5L", "0.1Ib", "否", "否", "2", "100"));
            errPoint.Add(string.Join("|", "基本误差", errFRAR, "H", "1.0", "0.05Ib", "否", "否", "2", "100"));
            return errPoint;
        }

        internal virtual List<string> BalanceActiveClass1(bool Imax4Ib, string errFRAR)
        {
            List<string> errPoint = new List<string>();
            errPoint.Add(string.Join("|", "基本误差", errFRAR, "H", "1.0", "Imax", "否", "否", "2", "100"));
            errPoint.Add(string.Join("|", "基本误差", errFRAR, "H", "0.5L", "Imax", "否", "否", "2", "100"));
            errPoint.Add(string.Join("|", "基本误差", errFRAR, "H", "0.8C", "Imax", "否", "否", "2", "100"));
            if (Imax4Ib)
            {
                errPoint.Add(string.Join("|", "基本误差", errFRAR, "H", "1.0", "0.5Imax", "否", "否", "2", "100"));
                errPoint.Add(string.Join("|", "基本误差", errFRAR, "H", "0.5L", "0.5Imax", "否", "否", "2", "100"));
                errPoint.Add(string.Join("|", "基本误差", errFRAR, "H", "0.8C", "0.5Imax", "否", "否", "2", "100"));
            }
            errPoint.Add(string.Join("|", "基本误差", errFRAR, "H", "1.0", "Ib", "否", "否", "2", "100"));
            errPoint.Add(string.Join("|", "基本误差", errFRAR, "H", "0.5L", "Ib", "否", "否", "2", "100"));
            errPoint.Add(string.Join("|", "基本误差", errFRAR, "H", "0.8C", "Ib", "否", "否", "2", "100"));
            errPoint.Add(string.Join("|", "基本误差", errFRAR, "H", "0.5L", "0.2Ib", "否", "否", "2", "100"));
            errPoint.Add(string.Join("|", "基本误差", errFRAR, "H", "0.8C", "0.2Ib", "否", "否", "2", "100"));
            errPoint.Add(string.Join("|", "基本误差", errFRAR, "H", "1.0", "0.1Ib", "否", "否", "2", "100"));
            errPoint.Add(string.Join("|", "基本误差", errFRAR, "H", "0.5L", "0.1Ib", "否", "否", "2", "100"));
            errPoint.Add(string.Join("|", "基本误差", errFRAR, "H", "0.8C", "0.1Ib", "否", "否", "2", "100"));
            errPoint.Add(string.Join("|", "基本误差", errFRAR, "H", "1.0", "0.05Ib", "否", "否", "2", "100"));
            return errPoint;
        }
        /// <summary>
        /// 直接式单相、平衡负载无功2、3级
        /// </summary>
        /// <param name="Imax4Ib"></param>
        /// <param name="errFRAR"></param>
        /// <returns></returns>
        internal virtual List<string> BalanceReactiveClass2(bool Imax4Ib, string errFRAR)
        {
            List<string> errPoint = new List<string>();
            errPoint.Add(string.Join("|", "基本误差", errFRAR, "H", "1.0", "Imax", "否", "否", "2", "100"));
            errPoint.Add(string.Join("|", "基本误差", errFRAR, "H", "0.5L", "Imax", "否", "否", "2", "100"));
            if (Imax4Ib)
            {
                errPoint.Add(string.Join("|", "基本误差", errFRAR, "H", "1.0", "0.5Imax", "否", "否", "2", "100"));
                errPoint.Add(string.Join("|", "基本误差", errFRAR, "H", "0.5L", "0.5Imax", "否", "否", "2", "100"));
            }
            errPoint.Add(string.Join("|", "基本误差", errFRAR, "H", "1.0", "Ib", "否", "否", "2", "100"));
            errPoint.Add(string.Join("|", "基本误差", errFRAR, "H", "0.5L", "Ib", "否", "否", "2", "100"));
            errPoint.Add(string.Join("|", "基本误差", errFRAR, "H", "0.25L", "Ib", "否", "否", "2", "100"));
            errPoint.Add(string.Join("|", "基本误差", errFRAR, "H", "0.5L", "0.2Ib", "否", "否", "2", "100"));
            errPoint.Add(string.Join("|", "基本误差", errFRAR, "H", "1.0", "0.1Ib", "否", "否", "2", "100"));
            errPoint.Add(string.Join("|", "基本误差", errFRAR, "H", "0.5L", "0.1Ib", "否", "否", "2", "100"));
            errPoint.Add(string.Join("|", "基本误差", errFRAR, "H", "1.0", "0.05Ib", "否", "否", "2", "100"));
            return errPoint;
        }

        /// <summary>
        /// 直接式不平衡负载有功2级
        /// </summary>
        /// <param name="Imax4Ib"></param>
        /// <param name="errFRAR"></param>
        /// <param name="Ln">H、A、B、C</param>
        /// <returns></returns>
        internal virtual List<string> ImbalanceActiveClass2(bool Imax4Ib, string errFRAR, string Ln)
        {
            List<string> errPoint = new List<string>();
            errPoint.Add(string.Join("|", "基本误差", errFRAR, Ln, "1.0", "Imax", "否", "否", "2", "100"));
            errPoint.Add(string.Join("|", "基本误差", errFRAR, Ln, "0.5L", "Imax", "否", "否", "2", "100"));
            errPoint.Add(string.Join("|", "基本误差", errFRAR, Ln, "1.0", "Ib", "否", "否", "2", "100"));
            errPoint.Add(string.Join("|", "基本误差", errFRAR, Ln, "0.5L", "Ib", "否", "否", "2", "100"));
            errPoint.Add(string.Join("|", "基本误差", errFRAR, Ln, "0.5L", "0.2Ib", "否", "否", "2", "100"));
            errPoint.Add(string.Join("|", "基本误差", errFRAR, Ln, "1.0", "0.1Ib", "否", "否", "2", "100"));
            return errPoint;
        }
        internal virtual List<string> ImbalanceActiveClass1(bool Imax4Ib, string errFRAR, string Ln)
        {
            List<string> errPoint = new List<string>();
            errPoint.Add(string.Join("|", "基本误差", errFRAR, Ln, "1.0", "Imax", "否", "否", "2", "100"));
            errPoint.Add(string.Join("|", "基本误差", errFRAR, Ln, "0.5L", "Imax", "否", "否", "2", "100"));
            errPoint.Add(string.Join("|", "基本误差", errFRAR, Ln, "1.0", "Ib", "否", "否", "2", "100"));
            errPoint.Add(string.Join("|", "基本误差", errFRAR, Ln, "0.5L", "Ib", "否", "否", "2", "100"));
            errPoint.Add(string.Join("|", "基本误差", errFRAR, Ln, "0.5L", "0.2Ib", "否", "否", "2", "100"));
            errPoint.Add(string.Join("|", "基本误差", errFRAR, Ln, "1.0", "0.1Ib", "否", "否", "2", "100"));
            return errPoint;
        }

        internal virtual List<string> ImbalanceReactiveClass2(bool Imax4Ib, string errFRAR, string Ln)
        {
            List<string> errPoint = new List<string>();
            errPoint.Add(string.Join("|", "基本误差", errFRAR, Ln, "1.0", "Imax", "否", "否", "2", "100"));
            errPoint.Add(string.Join("|", "基本误差", errFRAR, Ln, "0.5L", "Imax", "否", "否", "2", "100"));
            errPoint.Add(string.Join("|", "基本误差", errFRAR, Ln, "1.0", "Ib", "否", "否", "2", "100"));
            errPoint.Add(string.Join("|", "基本误差", errFRAR, Ln, "0.5L", "Ib", "否", "否", "2", "100"));
            errPoint.Add(string.Join("|", "基本误差", errFRAR, Ln, "0.5L", "0.2Ib", "否", "否", "2", "100"));
            errPoint.Add(string.Join("|", "基本误差", errFRAR, Ln, "1.0", "0.1Ib", "否", "否", "2", "100"));
            return errPoint;
        }
        #endregion

        #region 互感式 3、2、1、0.5S、0.2S
        internal virtual List<string> TransformerBalanceActiveClass2(bool Imax4Ib, string errFRAR)
        {
            List<string> errPoint = new List<string>();
            errPoint.Add(string.Join("|", "基本误差", errFRAR, "H", "1.0", "Imax", "否", "否", "2", "100"));
            errPoint.Add(string.Join("|", "基本误差", errFRAR, "H", "0.5L", "Imax", "否", "否", "2", "100"));
            errPoint.Add(string.Join("|", "基本误差", errFRAR, "H", "1.0", "Ib", "否", "否", "2", "100"));
            errPoint.Add(string.Join("|", "基本误差", errFRAR, "H", "0.5L", "Ib", "否", "否", "2", "100"));
            errPoint.Add(string.Join("|", "基本误差", errFRAR, "H", "0.5L", "0.1Ib", "否", "否", "2", "100"));
            errPoint.Add(string.Join("|", "基本误差", errFRAR, "H", "1.0", "0.05Ib", "否", "否", "2", "100"));
            errPoint.Add(string.Join("|", "基本误差", errFRAR, "H", "0.5L", "0.05Ib", "否", "否", "2", "100"));
            errPoint.Add(string.Join("|", "基本误差", errFRAR, "H", "1.0", "0.02Ib", "否", "否", "2", "100"));
            return errPoint;
        }
        internal virtual List<string> TransformerBalanceActiveClass1(bool Imax4Ib, string errFRAR)
        {
            List<string> errPoint = new List<string>();
            errPoint.Add(string.Join("|", "基本误差", errFRAR, "H", "1.0", "Imax", "否", "否", "2", "100"));
            errPoint.Add(string.Join("|", "基本误差", errFRAR, "H", "0.5L", "Imax", "否", "否", "2", "100"));
            errPoint.Add(string.Join("|", "基本误差", errFRAR, "H", "0.8C", "Imax", "否", "否", "2", "100"));
            errPoint.Add(string.Join("|", "基本误差", errFRAR, "H", "1.0", "Ib", "否", "否", "2", "100"));
            errPoint.Add(string.Join("|", "基本误差", errFRAR, "H", "0.5L", "Ib", "否", "否", "2", "100"));
            errPoint.Add(string.Join("|", "基本误差", errFRAR, "H", "0.8C", "Ib", "否", "否", "2", "100"));
            errPoint.Add(string.Join("|", "基本误差", errFRAR, "H", "0.5L", "0.1Ib", "否", "否", "2", "100"));
            errPoint.Add(string.Join("|", "基本误差", errFRAR, "H", "0.8C", "0.1Ib", "否", "否", "2", "100"));
            errPoint.Add(string.Join("|", "基本误差", errFRAR, "H", "1.0", "0.05Ib", "否", "否", "2", "100"));
            errPoint.Add(string.Join("|", "基本误差", errFRAR, "H", "0.5L", "0.05Ib", "否", "否", "2", "100"));
            errPoint.Add(string.Join("|", "基本误差", errFRAR, "H", "0.8C", "0.05Ib", "否", "否", "2", "100"));
            errPoint.Add(string.Join("|", "基本误差", errFRAR, "H", "1.0", "0.02Ib", "否", "否", "2", "100"));
            return errPoint;
        }
        internal virtual List<string> TransformerBalanceActiveClass02S(bool Imax4Ib, string errFRAR)
        {
            List<string> errPoint = new List<string>();
            errPoint.Add(string.Join("|", "基本误差", errFRAR, "H", "1.0", "Imax", "否", "否", "2", "100"));
            errPoint.Add(string.Join("|", "基本误差", errFRAR, "H", "0.5L", "Imax", "否", "否", "2", "100"));
            errPoint.Add(string.Join("|", "基本误差", errFRAR, "H", "0.8C", "Imax", "否", "否", "2", "100"));
            errPoint.Add(string.Join("|", "基本误差", errFRAR, "H", "1.0", "Ib", "否", "否", "2", "100"));
            errPoint.Add(string.Join("|", "基本误差", errFRAR, "H", "0.5L", "Ib", "否", "否", "2", "100"));
            errPoint.Add(string.Join("|", "基本误差", errFRAR, "H", "0.8C", "Ib", "否", "否", "2", "100"));
            errPoint.Add(string.Join("|", "基本误差", errFRAR, "H", "0.5L", "0.1Ib", "否", "否", "2", "100"));
            errPoint.Add(string.Join("|", "基本误差", errFRAR, "H", "0.8C", "0.1Ib", "否", "否", "2", "100"));
            errPoint.Add(string.Join("|", "基本误差", errFRAR, "H", "1.0", "0.05Ib", "否", "否", "2", "100"));
            errPoint.Add(string.Join("|", "基本误差", errFRAR, "H", "0.5L", "0.02Ib", "否", "否", "2", "100"));
            errPoint.Add(string.Join("|", "基本误差", errFRAR, "H", "0.8C", "0.02Ib", "否", "否", "2", "100"));
            errPoint.Add(string.Join("|", "基本误差", errFRAR, "H", "1.0", "0.01Ib", "否", "否", "2", "100"));
            return errPoint;
        }

        internal virtual List<string> TransformerBalanceReactiveClass2(bool Imax4Ib, string errFRAR)
        {
            List<string> errPoint = new List<string>();
            errPoint.Add(string.Join("|", "基本误差", errFRAR, "H", "1.0", "Imax", "否", "否", "2", "100"));
            errPoint.Add(string.Join("|", "基本误差", errFRAR, "H", "0.5L", "Imax", "否", "否", "2", "100"));
            errPoint.Add(string.Join("|", "基本误差", errFRAR, "H", "1.0", "Ib", "否", "否", "2", "100"));
            errPoint.Add(string.Join("|", "基本误差", errFRAR, "H", "0.5L", "Ib", "否", "否", "2", "100"));
            errPoint.Add(string.Join("|", "基本误差", errFRAR, "H", "0.25L", "Ib", "否", "否", "2", "100"));
            errPoint.Add(string.Join("|", "基本误差", errFRAR, "H", "0.5L", "0.1Ib", "否", "否", "2", "100"));
            errPoint.Add(string.Join("|", "基本误差", errFRAR, "H", "1.0", "0.05Ib", "否", "否", "2", "100"));
            errPoint.Add(string.Join("|", "基本误差", errFRAR, "H", "0.5L", "0.05Ib", "否", "否", "2", "100"));
            errPoint.Add(string.Join("|", "基本误差", errFRAR, "H", "1.0", "0.02Ib", "否", "否", "2", "100"));
            return errPoint;
        }
        internal virtual List<string> TransformerBalanceReactiveClass1(bool Imax4Ib, string errFRAR)
        {
            return TransformerBalanceReactiveClass2(Imax4Ib, errFRAR);
        }

        internal virtual List<string> TransformerImbalanceActiveClass2(bool Imax4Ib, string errFRAR, string Ln)
        {
            List<string> errPoint = new List<string>();
            errPoint.Add(string.Join("|", "基本误差", errFRAR, Ln, "1.0", "Imax", "否", "否", "2", "100"));
            errPoint.Add(string.Join("|", "基本误差", errFRAR, Ln, "0.5L", "Imax", "否", "否", "2", "100"));
            errPoint.Add(string.Join("|", "基本误差", errFRAR, Ln, "1.0", "Ib", "否", "否", "2", "100"));
            errPoint.Add(string.Join("|", "基本误差", errFRAR, Ln, "0.5L", "Ib", "否", "否", "2", "100"));
            errPoint.Add(string.Join("|", "基本误差", errFRAR, Ln, "0.5L", "0.1Ib", "否", "否", "2", "100"));
            errPoint.Add(string.Join("|", "基本误差", errFRAR, Ln, "1.0", "0.05Ib", "否", "否", "2", "100"));
            return errPoint;
        }
        internal virtual List<string> TransformerImbalanceActiveClass1(bool Imax4Ib, string errFRAR, string Ln)
        {
            List<string> errPoint = new List<string>();
            errPoint.Add(string.Join("|", "基本误差", errFRAR, Ln, "1.0", "Imax", "否", "否", "2", "100"));
            errPoint.Add(string.Join("|", "基本误差", errFRAR, Ln, "0.5L", "Imax", "否", "否", "2", "100"));
            errPoint.Add(string.Join("|", "基本误差", errFRAR, Ln, "1.0", "Ib", "否", "否", "2", "100"));
            errPoint.Add(string.Join("|", "基本误差", errFRAR, Ln, "0.5L", "Ib", "否", "否", "2", "100"));
            errPoint.Add(string.Join("|", "基本误差", errFRAR, Ln, "0.5L", "0.1Ib", "否", "否", "2", "100"));
            errPoint.Add(string.Join("|", "基本误差", errFRAR, Ln, "1.0", "0.05Ib", "否", "否", "2", "100"));
            return errPoint;
        }
        internal virtual List<string> TransformerImbalanceActiveClass02S(bool Imax4Ib, string errFRAR, string Ln)
        {
            List<string> errPoint = new List<string>();
            errPoint.Add(string.Join("|", "基本误差", errFRAR, Ln, "1.0", "Imax", "否", "否", "2", "100"));
            errPoint.Add(string.Join("|", "基本误差", errFRAR, Ln, "0.5L", "Imax", "否", "否", "2", "100"));
            errPoint.Add(string.Join("|", "基本误差", errFRAR, Ln, "1.0", "Ib", "否", "否", "2", "100"));
            errPoint.Add(string.Join("|", "基本误差", errFRAR, Ln, "0.5L", "Ib", "否", "否", "2", "100"));
            errPoint.Add(string.Join("|", "基本误差", errFRAR, Ln, "0.5L", "0.1Ib", "否", "否", "2", "100"));
            errPoint.Add(string.Join("|", "基本误差", errFRAR, Ln, "1.0", "0.05Ib", "否", "否", "2", "100"));
            return errPoint;
        }

        internal virtual List<string> TransformerImbalanceReactiveClass2(bool Imax4Ib, string errFRAR, string Ln)
        {
            List<string> errPoint = new List<string>();
            errPoint.Add(string.Join("|", "基本误差", errFRAR, Ln, "1.0", "Imax", "否", "否", "2", "100"));
            errPoint.Add(string.Join("|", "基本误差", errFRAR, Ln, "0.5L", "Imax", "否", "否", "2", "100"));
            errPoint.Add(string.Join("|", "基本误差", errFRAR, Ln, "1.0", "Ib", "否", "否", "2", "100"));
            errPoint.Add(string.Join("|", "基本误差", errFRAR, Ln, "0.5L", "Ib", "否", "否", "2", "100"));
            errPoint.Add(string.Join("|", "基本误差", errFRAR, Ln, "0.5L", "0.1Ib", "否", "否", "2", "100"));
            errPoint.Add(string.Join("|", "基本误差", errFRAR, Ln, "1.0", "0.05Ib", "否", "否", "2", "100"));
            return errPoint;
        }
        internal virtual List<string> TransformerImbalanceReactiveClass1(bool Imax4Ib, string errFRAR, string Ln)
        {
            return TransformerImbalanceReactiveClass2(Imax4Ib, errFRAR, Ln);
        }
        #endregion
    }
}
