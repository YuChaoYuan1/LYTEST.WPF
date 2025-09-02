using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LYTest.ViewModel.Schema
{
    internal class JJG596Points202X : JJGPoints
    {
        internal override List<string> BalanceActiveClass1(bool Imax4Ib, string errFRAR)
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
            errPoint.Add(string.Join("|", "基本误差", errFRAR, "H", "1.0", "10Itr", "否", "否", "2", "100"));
            errPoint.Add(string.Join("|", "基本误差", errFRAR, "H", "0.5L", "10Itr", "否", "否", "2", "100"));
            errPoint.Add(string.Join("|", "基本误差", errFRAR, "H", "0.8C", "10Itr", "否", "否", "2", "100"));
            errPoint.Add(string.Join("|", "基本误差", errFRAR, "H", "1.0", "1Itr", "否", "否", "2", "100"));
            errPoint.Add(string.Join("|", "基本误差", errFRAR, "H", "0.5L", "1Itr", "否", "否", "2", "100"));
            errPoint.Add(string.Join("|", "基本误差", errFRAR, "H", "0.8C", "1Itr", "否", "否", "2", "100"));
            errPoint.Add(string.Join("|", "基本误差", errFRAR, "H", "1.0", "Imin", "否", "否", "2", "100"));
            return errPoint;
        }

        internal override List<string> BalanceActiveClass2(bool Imax4Ib, string errFRAR)
        {
            List<string> errPoint = new List<string>();
            errPoint.Add(string.Join("|", "基本误差", errFRAR, "H", "1.0", "Imax", "否", "否", "2", "100"));
            errPoint.Add(string.Join("|", "基本误差", errFRAR, "H", "0.5L", "Imax", "否", "否", "2", "100"));
            //errPoint.Add(string.Join("|", "基本误差", errFRAR, "H", "0.8C", "Imax", "否", "否", "2", "100"));
            if (Imax4Ib)
            {
                errPoint.Add(string.Join("|", "基本误差", errFRAR, "H", "1.0", "0.5Imax", "否", "否", "2", "100"));
                errPoint.Add(string.Join("|", "基本误差", errFRAR, "H", "0.5L", "0.5Imax", "否", "否", "2", "100"));
                //errPoint.Add(string.Join("|", "基本误差", errFRAR, "H", "0.8C", "0.5Imax", "否", "否", "2", "100"));
            }
            errPoint.Add(string.Join("|", "基本误差", errFRAR, "H", "1.0", "10Itr", "否", "否", "2", "100"));
            errPoint.Add(string.Join("|", "基本误差", errFRAR, "H", "0.5L", "10Itr", "否", "否", "2", "100"));
            //errPoint.Add(string.Join("|", "基本误差", errFRAR, "H", "0.8C", "10Itr", "否", "否", "2", "100"));
            errPoint.Add(string.Join("|", "基本误差", errFRAR, "H", "1.0", "1Itr", "否", "否", "2", "100"));
            errPoint.Add(string.Join("|", "基本误差", errFRAR, "H", "0.5L", "1Itr", "否", "否", "2", "100"));
            //errPoint.Add(string.Join("|", "基本误差", errFRAR, "H", "0.8C", "1Itr", "否", "否", "2", "100"));
            //errPoint.Add(string.Join("|", "基本误差", errFRAR, "H", "1.0", "Imin", "否", "否", "2", "100"));
            return errPoint;
        }

        internal override List<string> BalanceReactiveClass2(bool Imax4Ib, string errFRAR)
        {
            return base.BalanceReactiveClass2(Imax4Ib, errFRAR);
        }

        internal override List<string> ImbalanceActiveClass1(bool Imax4Ib, string errFRAR, string Ln)
        {
            List<string> errPoint = new List<string>();
            errPoint.Add(string.Join("|", "基本误差", errFRAR, Ln, "1.0", "Imax", "否", "否", "2", "100"));
            errPoint.Add(string.Join("|", "基本误差", errFRAR, Ln, "0.5L", "Imax", "否", "否", "2", "100"));
            errPoint.Add(string.Join("|", "基本误差", errFRAR, Ln, "1.0", "10Itr", "否", "否", "2", "100"));
            errPoint.Add(string.Join("|", "基本误差", errFRAR, Ln, "0.5L", "10Itr", "否", "否", "2", "100"));
            errPoint.Add(string.Join("|", "基本误差", errFRAR, Ln, "0.5L", "1Itr", "否", "否", "2", "100"));
            errPoint.Add(string.Join("|", "基本误差", errFRAR, Ln, "1.0", "1Itr", "否", "否", "2", "100"));
            return errPoint;
        }

        internal override List<string> ImbalanceActiveClass2(bool Imax4Ib, string errFRAR, string Ln)
        {
            return ImbalanceActiveClass1(Imax4Ib, errFRAR, Ln);
        }

        internal override List<string> ImbalanceReactiveClass2(bool Imax4Ib, string errFRAR, string Ln)
        {
            return base.ImbalanceReactiveClass2(Imax4Ib, errFRAR, Ln);
        }

        internal override List<string> TransformerBalanceActiveClass02S(bool Imax4Ib, string errFRAR)
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
            errPoint.Add(string.Join("|", "基本误差", errFRAR, "H", "1.0", "10Itr", "否", "否", "2", "100"));
            errPoint.Add(string.Join("|", "基本误差", errFRAR, "H", "0.5L", "10Itr", "否", "否", "2", "100"));
            errPoint.Add(string.Join("|", "基本误差", errFRAR, "H", "0.8C", "10Itr", "否", "否", "2", "100"));
            errPoint.Add(string.Join("|", "基本误差", errFRAR, "H", "1.0", "1Itr", "否", "否", "2", "100"));
            errPoint.Add(string.Join("|", "基本误差", errFRAR, "H", "0.5L", "1Itr", "否", "否", "2", "100"));
            errPoint.Add(string.Join("|", "基本误差", errFRAR, "H", "0.8C", "1Itr", "否", "否", "2", "100"));
            errPoint.Add(string.Join("|", "基本误差", errFRAR, "H", "1.0", "Imin", "否", "否", "2", "100"));
            return errPoint;
        }

        internal override List<string> TransformerBalanceActiveClass1(bool Imax4Ib, string errFRAR)
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
            errPoint.Add(string.Join("|", "基本误差", errFRAR, "H", "1.0", "10Itr", "否", "否", "2", "100"));
            errPoint.Add(string.Join("|", "基本误差", errFRAR, "H", "0.5L", "10Itr", "否", "否", "2", "100"));
            errPoint.Add(string.Join("|", "基本误差", errFRAR, "H", "0.8C", "10Itr", "否", "否", "2", "100"));
            errPoint.Add(string.Join("|", "基本误差", errFRAR, "H", "1.0", "1Itr", "否", "否", "2", "100"));
            errPoint.Add(string.Join("|", "基本误差", errFRAR, "H", "0.5L", "1Itr", "否", "否", "2", "100"));
            errPoint.Add(string.Join("|", "基本误差", errFRAR, "H", "0.8C", "1Itr", "否", "否", "2", "100"));
            errPoint.Add(string.Join("|", "基本误差", errFRAR, "H", "1.0", "Imin", "否", "否", "2", "100"));
            return errPoint;
        }

        internal override List<string> TransformerBalanceActiveClass2(bool Imax4Ib, string errFRAR)
        {
            List<string> errPoint = new List<string>();
            errPoint.Add(string.Join("|", "基本误差", errFRAR, "H", "1.0", "Imax", "否", "否", "2", "100"));
            errPoint.Add(string.Join("|", "基本误差", errFRAR, "H", "0.5L", "Imax", "否", "否", "2", "100"));
            //errPoint.Add(string.Join("|", "基本误差", errFRAR, "H", "0.8C", "Imax", "否", "否", "2", "100"));
            if (Imax4Ib)
            {
                errPoint.Add(string.Join("|", "基本误差", errFRAR, "H", "1.0", "0.5Imax", "否", "否", "2", "100"));
                errPoint.Add(string.Join("|", "基本误差", errFRAR, "H", "0.5L", "0.5Imax", "否", "否", "2", "100"));
                //errPoint.Add(string.Join("|", "基本误差", errFRAR, "H", "0.8C", "0.5Imax", "否", "否", "2", "100"));
            }
            errPoint.Add(string.Join("|", "基本误差", errFRAR, "H", "1.0", "10Itr", "否", "否", "2", "100"));
            errPoint.Add(string.Join("|", "基本误差", errFRAR, "H", "0.5L", "10Itr", "否", "否", "2", "100"));
            //errPoint.Add(string.Join("|", "基本误差", errFRAR, "H", "0.8C", "10Itr", "否", "否", "2", "100"));
            errPoint.Add(string.Join("|", "基本误差", errFRAR, "H", "1.0", "1Itr", "否", "否", "2", "100"));
            errPoint.Add(string.Join("|", "基本误差", errFRAR, "H", "0.5L", "1Itr", "否", "否", "2", "100"));
            //errPoint.Add(string.Join("|", "基本误差", errFRAR, "H", "0.8C", "1Itr", "否", "否", "2", "100"));
            //errPoint.Add(string.Join("|", "基本误差", errFRAR, "H", "1.0", "Imin", "否", "否", "2", "100"));
            return errPoint;
        }

        internal override List<string> TransformerBalanceReactiveClass1(bool Imax4Ib, string errFRAR)
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
            errPoint.Add(string.Join("|", "基本误差", errFRAR, "H", "1.0", "0.01Ib", "否", "否", "2", "100"));
            return errPoint;
        }
        internal override List<string> TransformerBalanceReactiveClass2(bool Imax4Ib, string errFRAR)
        {
            return base.TransformerBalanceReactiveClass2(Imax4Ib, errFRAR);
        }

        internal override List<string> TransformerImbalanceActiveClass02S(bool Imax4Ib, string errFRAR, string Ln)
        {
            return ImbalanceActiveClass1(Imax4Ib, errFRAR, Ln);
        }

        internal override List<string> TransformerImbalanceActiveClass1(bool Imax4Ib, string errFRAR, string Ln)
        {
            return ImbalanceActiveClass1(Imax4Ib, errFRAR, Ln);
        }

        internal override List<string> TransformerImbalanceActiveClass2(bool Imax4Ib, string errFRAR, string Ln)
        {
            return ImbalanceActiveClass1(Imax4Ib, errFRAR, Ln);
        }

        internal override List<string> TransformerImbalanceReactiveClass1(bool Imax4Ib, string errFRAR, string Ln)
        {
            List<string> errPoint = new List<string>();
            errPoint.Add(string.Join("|", "基本误差", errFRAR, Ln, "1.0", "Imax", "否", "否", "2", "100"));
            errPoint.Add(string.Join("|", "基本误差", errFRAR, Ln, "0.5L", "Imax", "否", "否", "2", "100"));
            errPoint.Add(string.Join("|", "基本误差", errFRAR, Ln, "1.0", "Ib", "否", "否", "2", "100"));
            errPoint.Add(string.Join("|", "基本误差", errFRAR, Ln, "0.5L", "Ib", "否", "否", "2", "100"));
            errPoint.Add(string.Join("|", "基本误差", errFRAR, Ln, "0.25L", "Ib", "否", "否", "2", "100"));
            errPoint.Add(string.Join("|", "基本误差", errFRAR, Ln, "0.5L", "0.1Ib", "否", "否", "2", "100"));
            errPoint.Add(string.Join("|", "基本误差", errFRAR, Ln, "1.0", "0.05Ib", "否", "否", "2", "100"));
            return errPoint;
        }
        internal override List<string> TransformerImbalanceReactiveClass2(bool Imax4Ib, string errFRAR, string Ln)
        {
            return base.TransformerImbalanceReactiveClass2(Imax4Ib, errFRAR, Ln);
        }
    }
}
