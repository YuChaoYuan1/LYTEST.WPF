using LYTest.Core;
using LYTest.Core.Model.DnbModel.DnbInfo;
using LYTest.Core.Model.Meter;
using LYTest.Core.Model.Schema;
using LYTest.Mis.Common;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows.Forms;

namespace LyTest.Mis.Taida
{
    /// <summary>
    /// 天津泰达mis中间库
    /// </summary>
    public class Taida : OracleHelper, IMis
    {
        public bool SchemeDown(TestMeterInfo barcode, out string schemeName, out Dictionary<string, SchemaNode> Schema)
        {
            throw new NotImplementedException();
        }

        public Taida(string ip, int port, string dataSource, string userId, string pwd, string url)
        {
            this.Ip = ip;
            this.Port = port;
            this.DataSource = dataSource;
            this.UserId = userId;
            this.Password = pwd;
            this.WebServiceURL = url;
        }

        public bool Down(string barcode, ref TestMeterInfo meter)
        {
            if (string.IsNullOrEmpty(barcode)) return false;

            string sql = string.Format(@"select * from sf_dnbxy_sj where Txbm='{0}'", barcode);
            DataTable dt = ExecuteReader(sql);
            if (dt.Rows.Count <= 0)
            {
                sql = string.Format(@"select * from sf_dnbxy_sj where Ccbh='{0}'", barcode);
                dt = ExecuteReader(sql);
            }
            if (dt.Rows.Count <= 0)
            {
                //不存在条形码或出厂编号为(" + barcode + ")的记录"
                return false;
            }

            DataRow row = dt.Rows[0];

            meter.MD_BarCode = row["Txbm"].ToString().Trim();              //条形码
            meter.MD_AssetNo = row["jldh"].ToString().Trim();              //申请编号
            meter.MD_MadeNo = row["Ccbh"].ToString().Trim();              //出厂编号

            //表常数
            string strBcs = row["jlbcs"].ToString().Trim();
            string strBcswg = "";
            if (strBcswg != "")
                meter.MD_Constant = strBcs + "(" + strBcswg + ")";
            else
                meter.MD_Constant = strBcs;                //表常数
            //表等级                
            string strBdj = row["Zqddj"].ToString().Trim();

            string strBdjwg = row["Zqddj"].ToString().Trim();
            if (strBdjwg != "")     //假如有功表等级和无功表等级不一致
                meter.MD_Grane = strBdj + "(" + strBdjwg + ")";
            else
                meter.MD_Grane = strBdj;                    //表等级

            meter.MD_MeterModel = row["Xh"].ToString().Trim();      //表型号
            meter.DgnProtocol = null;   //通讯协议置空,由客户自已输入
            meter.MD_Factory = row["Sccj"].ToString().Trim();  //生产厂家
            meter.Seal1 = "";       //铅封1,暂时置空
            meter.Seal2 = "";       //铅封2,暂时置空
            meter.Seal3 = "";       //铅封3,暂时置空       
            return true;
        }

        public bool SchemeDown(string barcode, out string schemeName)
        {
            schemeName = "";
            //throw new NotImplementedException();
            return true;
        }

        public bool SchemeDown(string barcode, out string schemeName, out Dictionary<string, SchemaNode> Schema)
        {
            throw new NotImplementedException();
        }

        public void ShowPanel(Control panel)
        {
            throw new NotImplementedException();
        }

        public bool Update(TestMeterInfo meter)
        {

            #region 电能表检定记录
            CheckBasicData checkRecode;
            ProgressMeterCheckRecode(meter, out checkRecode);//meter
            if (!UpDataCheckRecords(checkRecode))
            {
                //string.Format("电能表[{0}]检定记录上传失败!", meter.Barcode);
                return false;
            }
            #endregion

            #region 电能表检定误差
            CheckErr[] checkErr;
            ProgressMeterErr(meter, out checkErr);
            if (!UpDataCheckErr(checkErr))
            {
                //string.Format("电能表[{0}]检定误差上传失败!", meter.ModeNo);
                return false;
            }
            #endregion

            return true;
        }

        public bool UpdateCompleted()
        {
            return true;
        }

        public void UpdateInit()
        {
            return;
        }



        /// <summary>
        /// 更新电能表检定记录
        /// </summary>
        /// <param name="ckRecords">检定记录</param>
        /// <returns></returns>
        private bool UpDataCheckRecords(CheckBasicData ckRecords)
        {
            string sql = string.Format("update sf_dnbxy_sj set xylx='{0}',Tempture='{1}',Humiture='{2}',Starttest='{3}',Undercur='{4}',Xyrq=to_date('{5}','yyyy-MM-dd HH24:mi:ss'),Xyry='{6}',Hyry='{7}',Xyjl='{8}',Bjlx='{9}',BZSB='{10}'where Txbm = '{11}'",
                ckRecords.Xylx.Trim(), ckRecords.Tempture.Trim(), ckRecords.Humiture.Trim(), ckRecords.Starttest.Trim(), ckRecords.Undercur.Trim(),
                ckRecords.Xyrq, ckRecords.Xyry.Trim(), ckRecords.Hyry.Trim(), ckRecords.Xyjl.Trim(), ckRecords.Bjlx.Trim(), ckRecords.BZSB.Trim(),
                 ckRecords.Txbm.Trim());
            if (ExecuteNonQuery(sql) <= 0)
                return false;

            return true;
        }

        /// <summary>
        /// 更新电能表检定误差
        /// </summary>
        /// <param name="ckErr">误差检定记录</param>
        /// <param name="read_id"></param>
        /// <returns></returns>
        private bool UpDataCheckErr(CheckErr[] ckErr)
        {
            if (ckErr == null || ckErr.Length <= 0) return true;
            for (int i = 0; i < ckErr.Length; i++)
            {
                string sql = string.Format("insert into sf_dnbxy_jg values('{0}','{1}',to_date('{2}','yyyy-MM-dd HH24:mi:ss'),'{3}','{4}','{5}','{6}','{7}','{8}','{9}','{10}','{11}','{12}','{13}')",
                     ckErr[i].Txbm.Trim(), ckErr[i].Ccbh.Trim(), ckErr[i].Xyrq, ckErr[i].Error1.Trim(), ckErr[i].Error2.Trim(),
                    ckErr[i].Chaninct.Trim(), ckErr[i].Balance.Trim(), ckErr[i].Dlfd.Trim(), ckErr[i].Dyfd.Trim(), ckErr[i].Xw.Trim(), ckErr[i].Bz.Trim(), ckErr[i].Xylx.Trim(), ckErr[i].Xx.Trim(), ckErr[i].JLDH.Trim());
                if (ExecuteNonQuery(sql) <= 0)
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// 电能表检定误差
        /// </summary>
        /// id;              误差记录标识,本实体记录的唯一标识号, 由校表台系统填写,具体填写不做要求
        /// read_id;         记录标识,该字段为外键字段
        /// chk_type_c;      检定类别，01取样检验、02抽样检定/校准、03装用前检定或校准...
        /// pf;              功率因数，COSφ＝1、COSφ＝0.5L.......
        /// load_currt;      负载电流，Imax、Ib、0.1Ib、0.2Ib.....
        /// both_way_p;      正向有功/正向无功/反向有功/反向无功	
        /// orgnstderr;      标准偏差原始值
        /// err1;            误差1
        /// err2;            误差2	
        /// err3;            误差3
        /// err4;            误差4
        /// err5;            误差5
        /// ave_err;         平均误差
        /// int_conver;      化整误差
        /// std_err_in;      标准偏差化整值
        /// load_stats;      负荷状况,1：平衡负荷、2：不平衡负荷
        /// test_group;      测试元组,1:A相,2:B相,3:C相,4:合组，必须传入
        /// <param name="meter">电能表对象</param>
        /// <returns></returns>
        private void ProgressMeterErr(TestMeterInfo meter, out CheckErr[] checkErr)
        {

            checkErr = new CheckErr[meter.MeterErrors.Keys.Count];

            if (meter.MeterErrors.Keys.Count <= 0) return;

            string[] keys = new string[meter.MeterErrors.Keys.Count];
            meter.MeterErrors.Keys.CopyTo(keys, 0);

            for (int i = 0; i < keys.Length; i++)
            {
                string key = keys[i];

                if (key.Length == 3) continue;      //如果ID长度是3表示是大项目，则跳过

                MeterError meterErr = meter.MeterErrors[key];

                string[] strWc = meterErr.WcMore.Split('|');
                if (strWc.Length <= 2) continue;


                string sql = string.Format(@"SELECT CCBH FROM sf_dnbxy_sj WHERE Txbm='{0}'", meter.MD_BarCode.Trim());
                object o = ExecuteScalar(sql);

                if (o != null)
                    checkErr[i].Ccbh = o.ToString().Trim();
                else
                    checkErr[i].Ccbh = "";

                checkErr[i].Txbm = meter.MD_BarCode;               //条形码
                checkErr[i].JLDH = meter.MD_AssetNo;            //计量工单
                checkErr[i].Xyrq = meter.VerifyDate == "" ? DateTime.Now : DateTime.Parse(meter.VerifyDate);              //出厂日期

                checkErr[i].Xw = meterErr.GLYS;                 //误差记录标识,本实体记录的唯一标识号, 由校表台系统填写,具体填写不做要求
                checkErr[i].Xylx = "有功";                      //记录标识,该字段为外键字段
                checkErr[i].Dyfd = "100%";                      //检定类别,检定类别，01取样检验、02抽样检定/校准、03装用前检定或校准...
                checkErr[i].Dlfd = meterErr.IbX;                //负载电流,负载电流，Imax、Ib、0.1Ib、0.2Ib......	
                checkErr[i].Balance = "合元";                   //正反向有无功,正向有功/正向无功/反向有功/反向无功

                string tmpHi = key.Substring(0, 3);             //取出误差类别+功率方向+元件
                switch (int.Parse(tmpHi.Substring(1, 1)))
                {
                    case 1:
                        checkErr[i].Xx = "正相";
                        break;
                    case 2:
                        checkErr[i].Xx = "反相";
                        break;
                    case 3:
                        checkErr[i].Xx = "正相";
                        break;
                    case 4:
                        checkErr[i].Xx = "反相";
                        break;
                    default:
                        checkErr[i].Xx = int.Parse(tmpHi.Substring(1, 1)).ToString();
                        break;
                }                 //正反向有无功,正向有功/正向无功/反向有功/反向无功
                string[] wc = GetWc(meterErr.WcMore);
                string[] err = new string[5];
                for (int j = 0; j < err.Length; j++)
                {
                    if (wc.Length - 2 >= j)
                        err[j] = wc[j];
                    else
                        err[j] = "";
                }

                checkErr[i].Error1 = err[0];              //误差1
                checkErr[i].Error2 = err[1];              //误差2


                checkErr[i].Chaninct = wc[wc.Length - 1];        //化整误差
                checkErr[i].Bz = "";        //标准偏差化整值
            }
        }


        /// <summary>
        /// 获取检定记录
        /// read_id      记录标识
        /// send_sn      发送批号,根据样本记录可为空,此外留空
        /// app_no       申请编号，由营销系统获取
        /// chk_no       检定编号
        /// meter_id     电能表标识
        /// chk_type_c   检定类别,01取样检验、02抽样检定/校准、03装用前检定或校准
        /// checker_nm   检定人员
        /// chk_date     检定日期
        /// checker_no   核检人员
        /// chk_rec_dt   核验日期
        /// equip_cert   标准装置检定证书号,根据样本记录可为空,此外留空
        /// chk_deskno   检定台编号
        /// meter_loc    挂表位置
        /// humidity     温度
        /// temp         湿度
        /// chk_basis    检定依据
        /// chk_const    校核常数   
        /// chk_conc     检定结论,1合格,0，不合格
        /// cert_id      证书编号
        /// chk_validt   检定有效日期 
        /// chk_remark   检定说明,根据样本记录可为空,此外留空
        /// org_no       供电单位
        /// Recipientn   数据接收人姓名
        /// </summary>
        /// <param name="meter"></param>
        /// <returns></returns>
        private void ProgressMeterCheckRecode(TestMeterInfo meter, out CheckBasicData checkRecode)
        {
            checkRecode = new CheckBasicData
            {
                Xylx = "",                                   //校验类型
                Tempture = meter.Temperature,                //温度
                Humiture = meter.Humidity,                                //湿度
                Xyjl = meter.Result == ConstHelper.合格 ? "t" : "f",             //结论
                Bjlx = "",                          //表计类型
                BZSB = "",           //检定人员
                Starttest = "",     //启动
                Undercur = "",//潜动
                Xyry = meter.Checker1,
                Hyry = meter.Checker2,
                Txbm = meter.MD_BarCode,
                Xyrq = meter.VerifyDate
            };

        }


        /// <summary>
        /// 获取误差值
        /// </summary>
        /// <param name="strwc"></param>
        /// <returns></returns>
        private string[] GetWc(string strwc)
        {
            string[] wc = strwc.Split('|');
            string[] pWc = new string[7];
            pWc[5] = wc[wc.Length - 2];
            pWc[6] = wc[wc.Length - 1];
            for (int i = 0; i < pWc.Length - 2; i++)
            {
                if (i < wc.Length - 1)
                {
                    pWc[i] = wc[i];
                }
                else
                {
                    pWc[i] = wc[0];
                }
            }
            return pWc;
        }

     
    }

}
