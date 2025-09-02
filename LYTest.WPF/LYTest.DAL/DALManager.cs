using System;
using System.IO;

namespace LYTest.DAL
{
    /// <summary>
    ///  数据存取层调用管理类,包含当前应用程序要用到的数据库存取类
    /// </summary>
    public class DALManager
    {
        private static GeneralDal inherentDbDal;
        /// <summary>
        /// 系统固有的属性
        /// </summary>
        public static GeneralDal InherentDbDal
        {
            get
            {
                if (inherentDbDal == null)
                {
                    if (8 == IntPtr.Size)
                    {
                        inherentDbDal = new GeneralDal($@"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={Directory.GetCurrentDirectory()}\DataBase\InherentAppData.mdb");
                    }
                    else
                    {
                        inherentDbDal = new GeneralDal($@"Provider=Microsoft.Jet.OLEDB.4.0;Data Source={Directory.GetCurrentDirectory()}\DataBase\InherentAppData.mdb");
                    }
                }
                return inherentDbDal;
            }
        }

        private static GeneralDal applicationDbDal;
        /// <summary>
        /// 应用程序数据库存取类
        /// </summary>
        public static GeneralDal ApplicationDbDal
        {
            get
            {
                if (applicationDbDal == null)
                {
                    if (8 == IntPtr.Size)
                    {
                        applicationDbDal = new GeneralDal($@"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={Directory.GetCurrentDirectory()}\DataBase\AppData.mdb");
                    }
                    else
                    {
                        string connString = string.Format(@"Provider=Microsoft.Jet.OLEDB.4.0;Data Source={0}\DataBase\AppData.mdb", Directory.GetCurrentDirectory());
                        applicationDbDal = new GeneralDal(connString);
                    }
                }
                return applicationDbDal;
            }
        }
        private static GeneralDal meterTempDbDal;
        /// <summary>
        /// 临时表数据存取类
        /// </summary>
        public static GeneralDal MeterTempDbDal
        {
            get
            {
                if (meterTempDbDal == null)
                {
                    if (8 == IntPtr.Size)
                    {
                        meterTempDbDal = new GeneralDal($@"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={Directory.GetCurrentDirectory()}\DataBase\TmpMeterData.mdb");
                    }
                    else
                    {
                        string connString = string.Format(@"Provider=Microsoft.Jet.OLEDB.4.0;Data Source={0}\Database\TmpMeterData.mdb", Directory.GetCurrentDirectory());

                        meterTempDbDal = new GeneralDal(connString);
                    }
                }
                return meterTempDbDal;
            }
        }
        private static GeneralDal meterDbDal;
        /// <summary>
        /// 正式表数据存取类
        /// </summary>
        public static GeneralDal MeterDbDal
        {
            get
            {
                if (meterDbDal == null)
                {
                    if (8 == IntPtr.Size)
                    {
                        meterDbDal = new GeneralDal($@"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={Directory.GetCurrentDirectory()}\DataBase\MeterData.mdb");
                    }
                    else
                    {
                        string connString = string.Format(@"Provider=Microsoft.Jet.OLEDB.4.0;Data Source={0}\Database\MeterData.mdb", Directory.GetCurrentDirectory());
                        meterDbDal = new GeneralDal(connString);
                    }
                }
                return meterDbDal;
            }
        }

        private static GeneralDal schemaDal;
        /// <summary>
        /// 应用程序数据库存取类
        /// </summary>
        public static GeneralDal SchemaDal
        {
            get
            {
                if (schemaDal == null)
                {
                    if (8 == IntPtr.Size)
                    {
                        schemaDal = new GeneralDal($@"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={Directory.GetCurrentDirectory()}\DataBase\SchemaData.mdb");
                    }
                    else
                    {
                        string connString = string.Format(@"Provider=Microsoft.Jet.OLEDB.4.0;Data Source={0}\DataBase\SchemaData.mdb", Directory.GetCurrentDirectory());
                        schemaDal = new GeneralDal(connString);
                    }
                }
                return schemaDal;
            }
        }
    }
}
