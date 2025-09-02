using LYTest.Core.Enum;
using LYTest.Core.Function;
using LYTest.Core;
using LYTest.Core.Model.DnbModel.DnbInfo;
using LYTest.Core.Model.Meter;
using LYTest.Core.Model.Schema;
using LYTest.Mis.Common;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows.Forms;

namespace LYTest.Mis.SG186
{
    //add yjt 20220222 新增整个东软SG186接口（整个SG186文件夹）
    public class SG186 : OracleHelper, IMis
    {
        public SG186(string ip, int port, string dataSource, string userId, string pwd, string url)
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

            meter = new TestMeterInfo();
            //modify yjt 20220417 修改从中间库获取的表数据的排序
            //string sql = string.Format(@"select * from d_meter where bar_code='{0}'", barcode);
            //modify yjt 20220417 修改从中间库获取的表数据的排序
            string sql = string.Format(@"select * from d_meter where bar_code='{0}' order by read_id desc", barcode);
            DataTable dt = ExecuteReader(sql);
            if (dt.Rows.Count <= 0)
            {
                sql = string.Format(@"select * from d_meter where made_no='{0}'", barcode);
                dt = ExecuteReader(sql);
            }
            if (dt.Rows.Count <= 0)
            {
                //不存在条形码或出厂编号为(" + barcode + ")的记录
                return false;
            }

            DataRow row = dt.Rows[0];

            meter.Meter_ID = row["METER_ID"].ToString().Trim();        //表唯一编号
            meter.MD_BarCode = row["BAR_CODE"].ToString().Trim();      //条形码
            meter.MD_AssetNo = row["ASSET_NO"].ToString().Trim();      //申请编号 --资产编号
            meter.MD_MadeNo = row["MADE_NO"].ToString().Trim();        //出厂编号
            meter.MD_TaskNo = row["APP_NO"].ToString().Trim(); //检定任务单，申请编号,任务编号

            //表类别（不需要）
            //string bnb = GetValue("ammeter_sort_code", row["SORT_CODE"].ToString().Trim());

            //表类型
            meter.MD_MeterType = GetValue("ammeter_type_code", row["TYPE_CODE"].ToString().Trim());
            if (!string.IsNullOrEmpty(meter.MD_MeterType) && meter.MD_MeterType.IndexOf("本地") != -1)
                meter.FKType = 1;
            else
                meter.FKType = 0;

            //接线方式
            string wMode = GetValue("wiring_mode", row["WIRING_MODE"].ToString().Trim());
            switch (wMode)
            {
                case "三相四线":
                    meter.MD_WiringMode = "三相四线";
                    break;
                case "三相三线":
                    meter.MD_WiringMode = "三相三线";
                    break;
                case "单相":
                    meter.MD_WiringMode = "单相";
                    break;
                default:
                    meter.MD_WiringMode = "单相";
                    break;
            }
            //电压
            string ubtmp = GetValue("ammeter_volt_code", row["VOLT_CODE"].ToString().Trim());
            if (ubtmp.IndexOf("57.7") >= 0)
            {
                meter.MD_UB = 57.7f;
            }
            else if (ubtmp.IndexOf("100") >= 0)
            {
                meter.MD_UB = 100;
            }
            else if (ubtmp.IndexOf("220") >= 0)
            {
                meter.MD_UB = 220;
            }
            else
                meter.MD_UB = meter.MD_WiringMode == "单相" ? 220f : 57.7f;

            //电流
            string ibtmp = GetValue("rated_current", row["RATE_CURRENT"].ToString().Trim());
            meter.MD_UA = ibtmp.Trim('A');
            string apl = GetValue("ap_pre_level_code", row["AP_LEVEL"].ToString().Trim());
            string rpl = GetValue("rp_pre_level_code", row["RP_LEVEL"].ToString().Trim());
            if (rpl == null || rpl == "")
            {
                rpl = GetValue("ap_pre_level_code", row["RP_LEVEL"].ToString().Trim());
            }

            //表等级 
            string accurDJ;
            if (rpl != "")     //假如有功表等级和无功表等级不一致
                accurDJ = apl + "(" + rpl + ")";
            // 新增没有无功的情况下，只显示有功的等级
            else
                accurDJ = apl;
            meter.MD_Grane = accurDJ;

            //表常数
            string cons = row["CONST_CODE"].ToString().Trim();
            string rcons = row["RP_CONST"].ToString().Trim();
            if (rcons != "")
                cons = cons + "(" + rcons + ")";
            meter.MD_Constant = cons;

            //互感器
            meter.MD_ConnectionFlag = "直接式";  //数据库没有 默认

            // 载波
            meter.MD_CarrName = "标准载波";     //数据库没有 默认

            meter.MD_MeterModel = GetValue("meter_model", row["MODEL_CODE"].ToString().Trim());      //表型号
            meter.MD_Customer = GetOrgInfo(true, row["BELONG_DEPT"].ToString().Trim());             //送检单位
            meter.DgnProtocol = null;   //通讯协议置空,由客户自已输入

            meter.MD_ProtocolName = "CDLT698";         //协议    

            meter.MD_JJGC = "JJG596-2012";                  //规程
            string djIR46 = "ABCDE";

            if (djIR46.IndexOf(apl) != -1)
            {
                meter.MD_ProtocolName = "CDLT698";    //协议   
                meter.MD_JJGC = "IR46";                    //规程
            }

            meter.MD_Factory = row["manufacturer"].ToString().Trim();            //生产厂家
            meter.Seal1 = "";                                                 //铅封1,暂时置空
            meter.Seal2 = "";                                                 //铅封2,暂时置空
            meter.Seal3 = "";                                                 //铅封3,暂时置空  
            return true;
        }

        //public bool SchemeDown(string barcode, out string schemeName)
        //{
        //    schemeName = "";
        //    return true;
        //}


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
            string meter_id;

            #region 获取申请编号和meter_id
            if (meter.MD_MadeNo != null && meter.MD_MadeNo != "")
            {
                if (!this.GetMeterByCCBH(meter, out meter_id, out _))
                {
                    MessageBox.Show($"电能表[{meter.MD_MadeNo}]meter_id获取失败!");
                    return false;
                }
                meter.MD_BarCode = "";
            }
            else
            {
                if (meter.MD_BarCode != null && meter.MD_BarCode != "")
                {
                    if (!this.GetMeterByTxm(meter, out meter_id))
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            #endregion

            #region 获取ReadID
            long readID = GetReadID("D_METER_DETECT");
            if (readID == 0)
            {
                string.Format("电能表[{0}]检定记录上传失败!", meter.MD_MadeNo);
                return false;
            }
            DeleteBeforeUpdateInfo(meter_id, readID);
            GetMeterInfo(meter);

            #endregion
            #region 电能表检定记录
            #endregion

            #region 电能表检定记录
            ProgressMeterCheckRecode(meter, meter_id, out CheckRecords checkRecode);
            if (!UpDataCheckRecords(checkRecode, readID))
            {
                string.Format("电能表[{0}]检定记录上传失败!", meter.MD_MadeNo);
                return false;
            }
            #endregion

            #region 电能表检定多功能检定项目
            ProgressMeterDgnCheckRecords(meter, meter_id, out Dgn_CheckRecords dgnCheckRecord);
            if (!UpDataDgnCheckResult(dgnCheckRecord, readID))
            {
                string.Format("电能表[{0}]多功能检定项目上传失败!", meter.MD_MadeNo);
                return false;
            }
            #endregion

            #region 电能表检定结论

            ProgressMeterResult1(meter, meter_id, out CheckResult[] checkResult);
            if (!UpDataCheckResult1(checkResult, readID))
            {
                //string.Format("电能表[{0}]检定结论上传失败!", meter.MD_MadeNo);
                return false;
            }
            #endregion

            #region 电能表检定误差
            ProgressMeterErr(meter, meter_id, out CheckErr[] checkErr);
            if (!UpDataCheckErr(checkErr, readID))
            {
                return false;
            }
            #endregion

            #region 电能表走字记录
            readID = GetReadID("D_METER_DIGIT_WALK");
            ProgressMeterZZ(meter, meter_id, out ZZ_CheckRecords[] zzCheckRecord);

            if (!UpDataCheckZZRecords(zzCheckRecord, readID))
            {
                return false;
            }
            #endregion

            #region 电能表走字计度器
            ProgressMeterRegister(meter, meter_id, out ZZ_CheckRegister[] zzRegister);
            if (!UpDataCheckRegisterRecords(zzRegister, readID))
            {
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
        /// 通过出厂编号获取Meter
        /// </summary>
        /// <param name="meterid">meterid</param>
        /// <returns></returns>
        public bool GetMeterByCCBH(TestMeterInfo meter, out string meterid, out string barcode)
        {
            meterid = "0";
            barcode = "";

            string sql = string.Format("SELECT read_id,meter_id,bar_code,app_no,manufacturer FROM d_meter WHERE MADE_NO='{0}' ORDER BY read_id DESC", meter.MD_MadeNo.Trim());
            DataTable ds = ExecuteReader(sql);
            if (ds.Rows.Count <= 0) return false;

            meterid = ds.Rows[0]["meter_id"].ToString().Trim();
            barcode = ds.Rows[0]["bar_code"].ToString().Trim();
            meter.MD_AssetNo = ds.Rows[0]["app_no"].ToString().Trim();
            meter.MD_TaskNo = ds.Rows[0]["app_no"].ToString().Trim();
            meter.MD_Factory = ds.Rows[0]["manufacturer"].ToString().Trim();

            return true;
        }

        /// <summary>
        /// 通过条形码获取Meterid
        /// </summary>
        /// <param name="meterid">条形码</param>
        /// <returns></returns>
        public bool GetMeterByTxm(TestMeterInfo meter, out string meterid)
        {
            meterid = "0";
            //modify yjt 20220426 修改排序
            //string sql = string.Format("SELECT meter_id,app_no,made_no,manufacturer,belong_dept FROM d_meter WHERE bar_code='{0}' BY app_no DESC", meter.MD_BarCode.Trim());
            //modify yjt 20220426 修改排序
            string sql = string.Format("SELECT read_id,meter_id,app_no,made_no,manufacturer,belong_dept FROM d_meter WHERE bar_code='{0}' BY read_id DESC", meter.MD_BarCode.Trim());
            DataTable dt = ExecuteReader(sql);
            if (dt.Rows.Count <= 0) return false;

            DataRow r = dt.Rows[0];
            meterid = r["meter_id"].ToString().Trim();
            meter.MD_AssetNo = r["app_no"].ToString().Trim();
            meter.MD_TaskNo = r["app_no"].ToString().Trim();
            meter.MD_MadeNo = r["made_no"].ToString().Trim();
            meter.MD_Factory = r["manufacturer"].ToString().Trim();
            meter.MD_Customer = GetOrgInfo(true, r["belong_dept"].ToString().Trim());

            return true;
        }

        /// <summary>
        /// 获取送检单位信息
        /// </summary>
        /// <param name="name"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        private string GetOrgInfo(bool name, string data)
        {
            if (data == "") return "";

            string sql;
            if (name)
                sql = string.Format(@"select * from SA_ORG where DEPT_ID='{0}'", data);//标准
            else
                sql = string.Format(@"select * from SA_ORG where DEPT_NAME='{0}'", data);//标准

            DataTable dr = ExecuteReader(sql);
            if (dr.Rows.Count <= 0) return "";

            DataRow row = dr.Rows[0];
            if (name)
                return row["DEPT_NAME"].ToString().Trim();
            else
                return row["DEPT_ID"].ToString().Trim();
        }

        private long GetReadID(string tableName)
        {
            string sql = string.Format("SELECT SEQ_{0}.NEXTVAL FROM DUAL", tableName);
            object o = ExecuteScalar(sql);
            return Convert.ToInt64(o);
        }



        /// <summary>
        /// 删除上次上传的信息
        /// </summary>
        /// <param name="meter"></param>
        /// <param name="meterid"></param>
        private void DeleteBeforeUpdateInfo(string meterid, long readid)
        {
            //List<string> sqlList = new List<string>();

            //sqlList.Add($"delete from d_md_volt_test where EQUIP_ID='{meterid}'");
            //sqlList.Add($"delete from D_REGISTER_INIT_DIGIT_WALK where METER_ID='{meterid}'");
            //sqlList.Add($"delete from D_REGISTER_DIGIT_WALK where METER_ID='{meterid}'");
            //sqlList.Add($"delete from D_METER_DIGIT_WALK where METER_ID='{meterid}'");
            //sqlList.Add($"delete from D_MULTFUNC_DETECT where METER_ID='{meterid}'");
            //sqlList.Add($"delete from D_METER_DETECT_CONC where METER_ID='{meterid}'");
            ////sqlList.Add($"delete from D_METER_ERR where EQUIP_ID='{meterid}'");
            //sqlList.Add($"delete from D_METER_DETECT where METER_ID='{meterid}'");
            //Execute(sqlList);




            string sql = string.Format(@"delete from d_md_volt_test where EQUIP_ID='{0}'", meterid);
            ExecuteNonQuery(sql);

            sql = string.Format(@"delete from D_REGISTER_INIT_DIGIT_WALK where METER_ID='{0}'", meterid);
            ExecuteNonQuery(sql);

            sql = string.Format(@"delete from D_REGISTER_DIGIT_WALK where METER_ID='{0}'", meterid);
            ExecuteNonQuery(sql);

            sql = string.Format(@"delete from D_METER_DIGIT_WALK where METER_ID='{0}'", meterid);
            ExecuteNonQuery(sql);

            sql = string.Format(@"delete from D_MULTFUNC_DETECT where METER_ID={0}", meterid);
            ExecuteNonQuery(sql);

            sql = string.Format(@"delete from D_METER_DETECT_CONC where METER_ID={0}", meterid);
            ExecuteNonQuery(sql);

            sql = string.Format(@"delete from D_METER_ERR where read_id={0}", readid);
            ExecuteNonQuery(sql);

            sql = string.Format(@"delete from D_METER_DETECT where meter_id={0}", meterid);
            ExecuteNonQuery(sql);


            ////add yjt 20220519 新增查询各个表数据
            //sql = string.Format(@"select *  from D_REGISTER_DIGIT_WALK where METER_ID='{0}'", meterid);
            //DataTable ds = ExecuteReader(sql);
            //sql = string.Format(@"select *  from D_METER_DIGIT_WALK where METER_ID='{0}'", meterid);
            //ds = ExecuteReader(sql);
            //sql = string.Format(@"select *  from D_MULTFUNC_DETECT where METER_ID={0}", meterid);
            //ds = ExecuteReader(sql);
            //sql = string.Format(@"select *  from D_METER_DETECT_CONC where METER_ID={0}", meterid);
            //ds = ExecuteReader(sql);
            //sql = string.Format(@"select *  from D_METER_ERR where read_id={0}", readid);
            //ds = ExecuteReader(sql);
            //sql = string.Format(@"select *  from D_METER_DETECT where meter_id={0}", meterid);
            //ds = ExecuteReader(sql);
            //if (ds.Rows.Count < 0) return;
        }

        /// <summary>
        /// 获取上传之前的信息
        /// </summary>
        /// <param name="meter"></param>
        /// <param name="strMisGroupInfo"></param>
        private void GetMeterInfo(TestMeterInfo meter)
        {
            //string sql = string.Format(@"select * from SA_ORG where ORG_NAME='{0}'", meter.OrgUnit);
            string sql = string.Format(@"select * from SA_ORG where DEPT_NAME='{0}'", meter.MD_Customer);
            DataTable dr = ExecuteReader(sql);
            if (dr.Rows.Count <= 0) return;

            //meter.OrgUnitNo = dr.Rows[0]["ORG_NO"].ToString().Trim(); 
            meter.MD_CustomerNo = dr.Rows[0]["DEPT_ID"].ToString().Trim();
            sql = string.Format(@"select * from sa_user ");
            DataTable dr2 = ExecuteReader(sql);//查询人员数据
            if (dr2.Rows.Count > 0)
            {
                DataRow[] r = dr2.Select($"USER_NAME='{meter.Checker1}'");
                if (r.Length > 0)
                    meter.CheckerNo1 = r[0]["USER_ID"].ToString().Trim();
                r = dr2.Select($"USER_NAME='{meter.Checker2}'");
                if (r.Length > 0)
                    meter.CheckerNo2 = r[0]["USER_ID"].ToString().Trim();
                r = dr2.Select($"USER_NAME='{meter.Manager}'");
                if (r.Length > 0)
                    meter.ManagerNo = r[0]["USER_ID"].ToString().Trim();
                else
                    meter.ManagerNo = dr2.Rows[0]["USER_ID"].ToString().Trim();
            }


            //sql = string.Format(@"select * from sa_user where user_name like '%{0}%'", meter.Checker1);
            // dr2 = ExecuteReader(sql);
            //if (dr2.Rows.Count > 0)
            //{
            //    meter.CheckerNo1 = dr2.Rows[0]["USER_ID"].ToString().Trim();
            //}

            //sql = string.Format(@"select * from sa_user where user_name like '%{0}%'", meter.Checker2);
            //dr2 = ExecuteReader(sql);
            //if (dr2.Rows.Count > 0)
            //{
            //    meter.CheckerNo2 = dr2.Rows[0]["USER_ID"].ToString().Trim();
            //}

            //sql = string.Format(@"select * from sa_user where user_name like '%{0}%'", meter.Manager);
            //dr2 = ExecuteReader(sql);
            //DataRow[] row=  dr2.Select("");
            //if (dr2.Rows.Count > 0)
            //{
            //    meter.ManagerNo = dr2.Rows[0]["USER_ID"].ToString().Trim();
            //}
        }

        /// <summary>
        /// 更新电能表检定记录
        /// </summary>
        /// <param name="data">检定记录</param>
        /// <returns></returns>
        private bool UpDataCheckRecords(CheckRecords data, long readID)
        {
            string insertsql = string.Format("insert into D_METER_DETECT values({0},{1},'{2}'," +
                "'{3}',{4},'{5}',{6},'{7}',{8},'{9}','{10}','{11}'," +
                "'{12}','{13}','{14}','{15}','{16}','{17}',{18},'{19}','{20}','{21}','{22}')", readID,
                DateTime.Now.ToString("yyyyMMdd"), data.App_no, data.Chk_no, data.Meter_id, data.CHECKER_NAME, data.Chk_date,
                data.Checker_no, data.Chk_rec_date, data.Equip_cert_no, data.Chk_desk_no, data.Meter_loc, data.Temp, data.Humidity,
                data.Chk_basis, data.Chk_const, data.Chk_conc, data.Cert_id, data.Chk_valid_date, data.Chk_remark, data.Org_no,
                data.Recipient_name, data.Org_company);
            if (ExecuteNonQuery(insertsql) <= 0) return false;
            if (data.Chk_conc == "1")
                insertsql = string.Format("Update D_Meter set detect='01' where meter_id='{0}' and detect<>'09'", data.Meter_id);
            else
                insertsql = string.Format("Update D_Meter set detect='02' where meter_id='{0}' and detect<>'09'", data.Meter_id);
            ExecuteNonQuery(insertsql);
            return true;
        }

        /// <summary>
        /// 更新电能表检定多功能检定项目
        /// </summary>
        /// <param name="dgnRecords">多功能检定记录</param>
        /// <returns></returns>
        private bool UpDataDgnCheckResult(Dgn_CheckRecords dgnRecords, long readID)
        {
            string insertsql = string.Format("insert into D_MULTFUNC_DETECT values(SEQ_D_MULTFUNC_DETECT.NEXTVAL,{1},{2},'{3}','{4}','{5}','{6}','{7}','{8}','{9}','{10}','{11}','{12}','{13}','{14}','{15}','{16}','{17}','{18}','{19}','{20}','{21}','{22}','{23}','{24}','{25}'," +
                "'{26}','{27}','{28}','{29}','{30}','{31}','{32}','{33}','{34}','{35}','{36}','{37}','{38}','{39}','{40}','{41}','{42}','{43}','{44}','{45}','{46}','{47}','{48}','{49}','{50}','{51}','{52}','{53}','{54}','{55}')", dgnRecords.Id,
                readID, dgnRecords.Meter_id, dgnRecords.Ar_ts_chk, dgnRecords.TS_CHK_CONC_CODE, dgnRecords.DAILY_TIMING_ERR1, dgnRecords.DAILY_TIMING_ERR2, dgnRecords.DAILY_TIMING_ERR3,
                dgnRecords.DAILY_TIMING_ERR4, dgnRecords.DAILY_TIMING_ERR5, dgnRecords.DAILY_TIMING_ERR6, dgnRecords.DAILY_TIMING_ERR7, dgnRecords.DAILY_TIMING_ERR8, dgnRecords.DAILY_TIMING_ERR9, dgnRecords.DAILY_TIMING_ERR10,
                dgnRecords.DAILY_TIMING_ERR_AVG, dgnRecords.DAILY_TIMING_ERR_INT, dgnRecords.DE_STD_IMAX, dgnRecords.DE_IMAX, dgnRecords.DEMAND_READING_ERR, dgnRecords.DE_INT_IMAX, dgnRecords.DE_STD_IB,
                dgnRecords.DE_IB_ACT, dgnRecords.DE_IB, dgnRecords.DE_IB_INT, dgnRecords.DE_P1IB_STD, dgnRecords.DE_P1IB_ACT, dgnRecords.DE_P1IB, dgnRecords.DE_P1IB_INT, dgnRecords.SEL_PERIOD,
                dgnRecords.DMD_PERIOD_IB, dgnRecords.DE_PERIOD_IB, dgnRecords.DE_PERIOD_IB_INT, dgnRecords.BF_PQ, dgnRecords.AF_PQ, dgnRecords.CONC, dgnRecords.BF_PQ_U100T20MS, dgnRecords.AF_PQ_U100T20MS,
                dgnRecords.CONC_U100T20MS, dgnRecords.BF_PQ_U50T1M, dgnRecords.AF_PQ_U50T1M, dgnRecords.CONCLUSION_U50T1M, dgnRecords.CI_CHK_CONC_CODE, dgnRecords.RP_MEMORY_CHK, dgnRecords.OTHER_MEMORY_CHK, dgnRecords.GPS_CALIBRATE_FLAG, dgnRecords.TS_ERR_CONC_CODE,
                dgnRecords.CHANGE1_TYPE, dgnRecords.CHANGE2_TYPE, dgnRecords.CHANGE1_ERR, dgnRecords.CHANGE2_ERR, dgnRecords.INT_CHANGE1_ERR, dgnRecords.INT_CHANGE2_ERR, dgnRecords.DAILY_TIMING_ERR_CONC, dgnRecords.DE_ERR_CONC, dgnRecords.DMD_PERIOD_ERR_CONC
                );
            return ExecuteNonQuery(insertsql) > 0;
        }

        ///// <summary>
        ///// 更新电能表检定结论
        ///// </summary>
        ///// <param name="ckResult">检定结果记录</param>
        ///// <returns></returns>
        //private bool UpDataCheckResult(CheckResult ckResult, long readID)
        //{
        //    string insertsql = string.Format("insert into D_METER_DETECT_CONC values(SEQ_D_METER_DETECT_CONC.NEXTVAL,{1},{2},'{3}',{4},'{5}','{6}','{7}','{8}',{9},{10},{11},'{12}')",
        //        ckResult.id, readID, ckResult.meter_id, ckResult.BOTH_WAY_POWER_FLAG, ckResult.START_CONC_CODE, ckResult.CREEP_CONC_CODE, ckResult.VOLT_CONC,
        //        ckResult.START_CURRENT, ckResult.START_DATE, ckResult.VOLT_TEST_VALUE, ckResult.ERR_UPPER_LIMIT, ckResult.ERR_LOWER_LIMIT, ckResult.ELECT_CONC_CODE
        //        );
        //    return ExecuteNonQuery(insertsql) > 0;
        //}

        //modify yjt 20220421 修改启动潜动四个方向 数组
        /// <summary>
        /// 更新电能表检定结论  修改后
        /// </summary>
        /// <param name="ckResult">检定结果记录</param>
        /// <returns></returns>
        private bool UpDataCheckResult1(CheckResult[] ckResult, long readID)
        {
            if (ckResult == null || ckResult.Length <= 0) return true;
            for (int i = 0; i < ckResult.Length; i++)
            {
                string sql = string.Format("insert into D_METER_DETECT_CONC values(SEQ_D_METER_DETECT_CONC.NEXTVAL,{1},{2},'{3}',{4},'{5}','{6}','{7}','{8}',{9},{10},{11},'{12}')",
    ckResult[i].Id, readID, ckResult[i].Meter_id, ckResult[i].BOTH_WAY_POWER_FLAG, ckResult[i].START_CONC_CODE, ckResult[i].CREEP_CONC_CODE, ckResult[i].VOLT_CONC,
    ckResult[i].START_CURRENT, ckResult[i].START_DATE, ckResult[i].VOLT_TEST_VALUE, ckResult[i].ERR_UPPER_LIMIT, ckResult[i].ERR_LOWER_LIMIT, ckResult[i].ELECT_CONC_CODE
    );
                if (ExecuteNonQuery(sql) <= 0)
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// 更新电能表检定误差
        /// </summary>
        /// <param name="ckErr">误差检定记录</param>
        /// <param name="read_id"></param>
        /// <returns></returns>
        private bool UpDataCheckErr(CheckErr[] ckErr, long readID)
        {
            if (ckErr == null || ckErr.Length <= 0) return true;
            for (int i = 0; i < ckErr.Length; i++)
            {
                string sql = string.Format("insert into D_METER_ERR values(SEQ_D_METER_ERR.NEXTVAL,{1},'{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}','{10}','{11}','{12}','{13}','{14}','{15}','{16}','{17}','{18}')",
                    ckErr[i].id, readID, ckErr[i].pf, ckErr[i].load_currt, ckErr[i].BOTH_WAY_POWER_FLAG, ckErr[i].chk_type_code, ckErr[i].orgn_std_err,
                    ckErr[i].err1, ckErr[i].err2, ckErr[i].err3, ckErr[i].err4, ckErr[i].err5, ckErr[i].ave_err, ckErr[i].int_convert_err, ckErr[i].std_err_int, ckErr[i].load_stats,
                    ckErr[i].group_type, ckErr[i].err_upper_limit, ckErr[i].err_lower_limit
        );
                if (ExecuteNonQuery(sql) <= 0)
                {
                    return false;
                }
                //break;
            }
            return true;
        }

        /// <summary>
        /// 更新电能表走字计度器
        /// </summary>
        /// <param name="zzRecords">走字计度器记录</param>
        /// <returns></returns>
        private bool UpDataCheckRegisterRecords(ZZ_CheckRegister[] data, long readID)
        {
            if (data == null || data.Length <= 0) return true;
            bool[] bZzUpdate = new bool[5];
            for (int i = 0; i < bZzUpdate.Length; i++)
                bZzUpdate[i] = false;

            for (int i = 0; i < data.Length; i++)
            {
                string sql = string.Format("insert into D_REGISTER_DIGIT_WALK values(SEQ_D_REGISTER_DIGIT_WALK.NEXTVAL,{0},{1},'{2}','{3}','{4}',{5},{6},{7},{8},'{9}',{10},'{11}','{12}',{13})",
                readID, data[i].METER_ID, data[i].READ_TYPE_CODE, data[i].BAR_CODE, data[i].MADE_NO, data[i].READING_DIGITS, data[i].LAST_READING,
                data[i].REGISTER_READ, data[i].RUNING_ERR, data[i].T_LAST_READING, data[i].T_END_READING, data[i].AR_TS_READING_ERR, data[i].COMP_ERR, data[i].IR_LAST_READING
                );
                //确保上传正向有功总、尖、峰、平、谷全部上传
                switch (data[i].READ_TYPE_CODE)
                {
                    case "11":
                        bZzUpdate[0] = true;
                        break;
                    case "12":
                        bZzUpdate[1] = true;
                        break;
                    case "13":
                        bZzUpdate[2] = true;
                        break;
                    case "14":
                        bZzUpdate[3] = true;
                        break;
                    case "15":
                        bZzUpdate[4] = true;
                        break;
                }
                if (ExecuteNonQuery(sql) <= 0)
                {
                    //return false;//地方不走字默认合格就要上传，没数据的下一段填充0
                }
            }
            for (int i = 0; i < bZzUpdate.Length; i++)//没数据的填充0
            {
                if (!bZzUpdate[i])
                {
                    string typeCode = (11 + i).ToString();
                    string sql = string.Format("insert into D_REGISTER_DIGIT_WALK values(SEQ_D_REGISTER_DIGIT_WALK.NEXTVAL,{0},{1},'{2}','{3}','{4}',{5},{6},{7},{8},{9},{10},'{11}','{12}',{13})",
                        readID, data[0].METER_ID, typeCode, data[0].BAR_CODE, data[0].MADE_NO, data[0].READING_DIGITS, "0.00",
                            "0.00", "0.00", "0.00", "0.00", "0.00", "0.00", "0.00");
                    if (ExecuteNonQuery(sql) <= 0)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        //modify yjt 20220422 修改电能表走字记录
        /// <summary>
        /// 更新电能表走字记录
        /// </summary>
        /// <param name="zzRecords">走字记录</param>
        /// <returns></returns>
        private bool UpDataCheckZZRecords(ZZ_CheckRecords[] zzRecords, long readID)
        {
            if (zzRecords == null || zzRecords.Length <= 0) return true;
            string strConc = "";

            string sql = string.Format("insert into D_METER_DIGIT_WALK values({0},{1},'{2}','{3}',{4},'{5}',{6},'{7}',{8},'{9}','{10}','{11}','{12}','{13}','{14}','{15}','{16}','{17}','{18}','{19}')",
            readID, zzRecords[0].Send_sn, zzRecords[0].Walk_no, zzRecords[0].App_no, zzRecords[0].Meter_id, zzRecords[0].RUNING_PERSON_NAME, zzRecords[0].RUNING_DATE,
            zzRecords[0].Checker_no, zzRecords[0].Chk_rec_date, zzRecords[0].Runing_desk_no, zzRecords[0].Temp, zzRecords[0].Humidity, zzRecords[0].Pointer_type_code, zzRecords[0].STD_READING_AVG, zzRecords[0].STD_RELATIVE_ERR, zzRecords[0].Comp_err,
            zzRecords[0].TIME_CALIBRATE_FLAG, zzRecords[0].Conc_code, zzRecords[0].Runing_remark, zzRecords[0].Org_no
            );
            string meterid = zzRecords[0].Meter_id;
            if (zzRecords[0].Conc_code.CompareTo("0") == 0 || strConc.CompareTo("02") == 0)
                strConc = "02";
            else
                strConc = "01";
            if (ExecuteNonQuery(sql) <= 0)
            {
                return false;
            }

            sql = string.Format("Update D_Meter set DIGIT_WALK='{0}' where meter_id='{1}'", strConc, meterid);
            if (ExecuteNonQuery(sql) <= 0)
            {
                return false;
            }

            return true;
        }

        //modify yjt 20220422 修改电能表走字记录
        ///// <summary>
        ///// 更新电能表走字记录
        ///// </summary>
        ///// <param name="zzRecords">走字记录</param>
        ///// <returns></returns>
        //private bool UpDataCheckZZRecords1(ZZ_CheckRecords[] zzRecords, long readID)
        //{
        //    if (zzRecords == null || zzRecords.Length <= 0) return true;
        //    string strConc = "";

        //    string sql = string.Format("select * from D_METER_DIGIT_WALK where read_id='8403337'");
        //    DataTable ds = ExecuteReader(sql);
        //    if (ds.Rows.Count <= 0) return false;

        //    for (int i = 0; i < zzRecords.Length; i++)
        //    {
        //        sql = string.Format("insert into D_METER_DIGIT_WALK values({0},{1},'{2}','{3}',{4},'{5}',{6},'{7}',{8},'{9}','{10}','{11}','{12}','{13}','{14}','{15}','{16}','{17}','{18}','{19}')",
        //   readID, zzRecords[i].send_sn, zzRecords[i].walk_no, zzRecords[i].app_no, zzRecords[i].meter_id, zzRecords[i].RUNING_PERSON_NAME, zzRecords[i].RUNING_DATE,
        //   zzRecords[i].checker_no, zzRecords[i].chk_rec_date, zzRecords[i].runing_desk_no, zzRecords[i].temp, zzRecords[i].humidity, zzRecords[i].pointer_type_code, zzRecords[i].STD_READING_AVG, zzRecords[i].STD_RELATIVE_ERR, zzRecords[i].comp_err,
        //   zzRecords[i].TIME_CALIBRATE_FLAG, zzRecords[i].conc_code, zzRecords[i].runing_remark, zzRecords[i].org_no
        //   );
        //        string meterid = zzRecords[i].meter_id;
        //        if (zzRecords[i].conc_code.CompareTo("0") == 0 || strConc.CompareTo("02") == 0)
        //            strConc = "02";
        //        else
        //            strConc = "01";
        //        if (ExecuteNonQuery(sql) <= 0)
        //        {
        //            return false;
        //        }

        //        sql = string.Format("Update D_Meter set DIGIT_WALK='{0}' where meter_id='{1}'", strConc, meterid);
        //        if (ExecuteNonQuery(sql) <= 0)
        //        {
        //            return false;
        //        }
        //    }

        //    return true;
        //}

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
        private void ProgressMeterCheckRecode(TestMeterInfo meter, string meter_id, out CheckRecords record)
        {
            record = new CheckRecords();
            string sendno = GetSendNo();
            //record.read_id = "";                                  //上传时自动获取
            record.Read_id = meter_id.ToString();                                  //上传时自动获取 modify
            record.Send_sn = sendno;                                   //发送批号,根据样本记录可为空,此外留空
            record.App_no = meter.MD_AssetNo;                //申请编号，由营销系统获取
            record.Chk_no = sendno;                                //检定编号
            record.Meter_id = meter_id.ToString();             //电能表标识  
            //record.checker_no =  meter.Checker2.Trim();        //核检人员 modify
            //record.CHECKER_NAME = meter.Checker1.Trim();           //检定人员 modify
            record.Checker_no = meter.CheckerNo2 == "" ? meter.Checker2.Trim() : meter.CheckerNo2;        //核检人员 modify
            record.CHECKER_NAME = meter.CheckerNo1 == "" ? meter.Checker1.Trim() : meter.CheckerNo1;           //检定人员 modify

            record.Chk_date = meter.VerifyDate != "" ?
                DateTime.Parse(meter.VerifyDate).ToString("yyyy-MM-dd") : DateTime.Now.ToString("yyyy-MM-dd");            //检定日期
                                                                                                                          //
            record.Chk_date = string.Format("to_date('{0}','yyyy-mm-dd')", record.Chk_date); //add

            record.Chk_rec_date = meter.ExpiryDate != "" ?
                DateTime.Parse(meter.ExpiryDate).ToString("yyyy-MM-dd") : DateTime.Now.ToString("yyyy-MM-dd");          //核验日期

            record.Chk_rec_date = string.Format("to_date('{0}','yyyy-mm-dd')", record.Chk_rec_date); //add

            record.Equip_cert_no = "";                            //标准装置检定证书号,根据样本记录可为空,此外留空
            record.Chk_desk_no = meter.BenthNo;            //检定台编号
            record.Meter_loc = meter.MD_Epitope.ToString(); //挂表位置
            record.Humidity = meter.Temperature != "" ? float.Parse(meter.Temperature) : 0;  //温度
            record.Temp = meter.Humidity != "" ? float.Parse(meter.Humidity) : 0;  //湿度
            record.Chk_basis = meter.Other5 == "" ? "JJG596-1999" : meter.Other5;             //检定依据
            //record.chk_const = DataHelper.GetMeterNoByConst(meter.GetBcs()[0].ToString());   //校核常数   
            record.Chk_const = meter.Result == ConstHelper.合格 ? "1" : "0";   //校核常数   modify
            record.Chk_conc = meter.Result == ConstHelper.合格 ? "1" : "0"; //检定结论,1合格,0，不合格
            record.Detect = meter.Result == ConstHelper.合格 ? "1" : "0"; //,1合格,0，不合格
            record.Cert_id = meter.MD_CertificateNo;                //证书编号
            record.Chk_valid_date = meter.VerifyDate != "" ?
                DateTime.Parse(meter.VerifyDate).AddYears(10).ToString("yyyy-MM-dd") : DateTime.Now.AddYears(10).ToString("yyyy-MM-dd");    //检定有效日期    
            record.Chk_valid_date = string.Format("to_date('{0}','yyyy-mm-dd')", record.Chk_valid_date); //add
            record.Chk_remark = "";                            //检定说明,根据样本记录可为空,此外留空
            record.Org_no = "";//"65413";                                //供电单位
            record.Recipient_name = "";//"15005069";//meter.CheckerNo2;                            //数据接收人姓名
        }

        /// <summary>
        /// 电能表检定多功能检定项目
        /// </summary>
        /// id                 项目记录标识,本记录的唯一标识号, 由校表台系统填写,具体填写不做要求
        /// read_id            记录标识,该字段为外键字段
        /// meter_id           电能表标识,电能表编号
        /// ar_ts_chk          费率时段检查,本处要求读出电能表内实际时段，例如平07：00-08：00，并与规定时段比较
        /// ts_chk_con         费率时段检查结论,1合格，0不合格
        /// daily_err_1        由电源供电的时钟试验(s)1
        /// daily_err_2        由电源供电的时钟试验(s)2
        /// daily_err_3        由电源供电的时钟试验(s)3
        /// daily_err_4        由电源供电的时钟试验(s)4
        /// daily_err_5        由电源供电的时钟试验(s)5
        /// daily_err_6        由电源供电的时钟试验(s)6
        /// daily_err_7        由电源供电的时钟试验(s)7
        /// daily_err_8        由电源供电的时钟试验(s)8
        /// daily_err_9        由电源供电的时钟试验(s)9
        /// daily_err_10       由电源供电的时钟试验(s)0
        /// daily_erra         由电源供电的时钟试验平均值 
        /// daily_erri         由电源供电的时钟试验化整值
        /// de_std_ima         需量示数误差(负荷点Imax)标准值
        /// de_imax            需量示数误差(负荷点Imax)实际值
        /// demand_err         需量示数误差(负荷点Imax)
        /// de_int_ima         需量示数误差(负荷点Imax)化整值
        /// de_std_ib          需量示数误差(负荷点Ib)标准值
        /// de_ib_act          需量示数误差(负荷点Ib)实际值
        /// de_ib              需量示数误差(负荷点Ib)
        /// de_ib_int          需量示数误差(负荷点Ib)化整值
        /// de_p1ib_st         需量示数误差(负荷点0.1Ib)标准值
        /// de_p1ib_ac         需量示数误差(负荷点0.1Ib)实际值
        /// de_p1ib            需量示数误差(负荷点0.1Ib)
        /// de_p1ib_in         需量示数误差(负荷点0.1Ib)化整值
        /// sel_period         需量选定周期(负荷点Ib)
        /// dmd_period         需量实测周期(负荷点Ib)
        /// de_period          需量周期误差(负荷点Ib)
        /// de_periodi         需量周期误差(负荷点Ib)化整值
        /// bf_pq              (电压中断ΔU=100%，t=1s) 变化前电量
        /// af_pq              (电压中断ΔU=100%，t=1s) 变化后电量
        /// conc               (电压中断ΔU=100%，t=1s) 结论,1合格，0不合格
        /// bf_pq_u100         (电压中断ΔU=100%，t=20ms) 变化前电量
        /// af_pq_u100         (电压中断ΔU=100%，t=20ms) 变化后电量
        /// conc_u100t         (电压中断ΔU=100%，t=20ms) 结论,1合格，0不合格
        /// bf_pq_u50t         (电压降落ΔU=50%，t=1min) 变化前电量
        /// af_pq_u50t         (电压降落ΔU=50%，t=1min) 变化后电量
        /// conclusion         (电压降落ΔU=50%，t=1min) 结论,1合格，0不合格
        /// ci_chk_con          通讯接口测试结论,1合格，0不合格
        /// rp_memory          无功存储器检查,01合格、02不合格
        /// other_memo         其它存储器检查,01合格、02不合格
        /// gps_calibr         GPS对时,01是、02否
        /// ts_err_con         费率时段投切误差结论,1合格，0不合格
        /// <param name="meter"></param>
        /// <returns></returns>
        private void ProgressMeterDgnCheckRecords(TestMeterInfo meter, string meter_id, out Dgn_CheckRecords record)
        {
            //if (_MeterInfo.MeterDgns.Count <= 0) return new ZH.Mis.Struct.Dgn_CheckRecords();
            string[] strrjs = GetRJSData(meter);

            record = new Dgn_CheckRecords
            {
                Id = meter.Meter_ID.ToString(),                 //项目记录标识,本记录的唯一标识号, 由校表台系统填写,具体填写不做要求
                Read_id = meter_id.ToString(),            //记录标识,该字段为外键字段
                Meter_id = meter_id.ToString(), //电能表标识,电能表编号
                Ar_ts_chk = "",                  //费率时段检查,本处要求读出电能表内实际时段，例如平07：00-08：00，并与规定时段比较
                TS_CHK_CONC_CODE = "",                 //费率时段检查结论,1合格，0不合格
                DAILY_TIMING_ERR1 = strrjs[0],        //由电源供电的时钟试验(s)1
                DAILY_TIMING_ERR2 = strrjs[1],        //由电源供电的时钟试验(s)2
                DAILY_TIMING_ERR3 = strrjs[2],        //由电源供电的时钟试验(s)3
                DAILY_TIMING_ERR4 = strrjs[3],        //由电源供电的时钟试验(s)4
                DAILY_TIMING_ERR5 = strrjs[4],        //由电源供电的时钟试验(s)5
                DAILY_TIMING_ERR6 = strrjs[5],        //由电源供电的时钟试验(s)6
                DAILY_TIMING_ERR7 = strrjs[6],        //由电源供电的时钟试验(s)7
                DAILY_TIMING_ERR8 = strrjs[7],        //由电源供电的时钟试验(s)8
                DAILY_TIMING_ERR9 = strrjs[8],        //由电源供电的时钟试验(s)9
                DAILY_TIMING_ERR10 = strrjs[9],       //由电源供电的时钟试验(s)0
                DAILY_TIMING_ERR_AVG = strrjs[10],        //由电源供电的时钟试验平均值 
                DAILY_TIMING_ERR_INT = strrjs[11]        //由电源供电的时钟试验化整值
            };
            string[] strxlwc = GetXLData(meter);
            record.DE_STD_IMAX = strxlwc[0];        //需量示数误差(负荷点Imax)标准值
            record.DE_IMAX = strxlwc[1];           //需量示数误差(负荷点Imax)实际值
            record.DEMAND_READING_ERR = strxlwc[2];        //需量示数误差(负荷点Imax)
            record.DE_INT_IMAX = strxlwc[3];        //需量示数误差(负荷点Imax)化整值
            record.DE_STD_IB = strxlwc[4];        //需量示数误差(负荷点Ib)标准值
            record.DE_IB_ACT = strxlwc[5];        //需量示数误差(负荷点Ib)实际值
            record.DE_IB = strxlwc[6];            //需量示数误差(负荷点Ib)
            record.DE_IB_INT = strxlwc[7];        //需量示数误差(负荷点Ib)化整值
            record.DE_P1IB_STD = strxlwc[8];        //需量示数误差(负荷点0.1Ib)标准值
            record.DE_P1IB_ACT = strxlwc[9];        //需量示数误差(负荷点0.1Ib)实际值
            record.DE_P1IB = strxlwc[10];           //需量示数误差(负荷点0.1Ib)
            record.DE_P1IB_INT = strxlwc[11];        //需量示数误差(负荷点0.1Ib)化整值
            record.SEL_PERIOD = strxlwc[12];        //需量选定周期(负荷点Ib)
            record.DMD_PERIOD_IB = strxlwc[13];        //需量实测周期(负荷点Ib)
            record.DE_PERIOD_IB = strxlwc[14];        //需量周期误差(负荷点Ib)
            record.DE_PERIOD_IB_INT = strxlwc[15];        //需量周期误差(负荷点Ib)化整值
            record.BF_PQ = "";            //(电压中断ΔU=100%，t=1s) 变化前电量
            record.AF_PQ = "";             //(电压中断ΔU=100%，t=1s) 变化后电量
            record.CONC = "";               //(电压中断ΔU=100%，t=1s) 结论,1合格，0不合格
            record.BF_PQ_U100T20MS = "";        //(电压中断ΔU=100%，t=20ms) 变化前电量
            record.AF_PQ_U100T20MS = "";        //(电压中断ΔU=100%，t=20ms) 变化后电量
            record.CONC_U100T20MS = "";        //(电压中断ΔU=100%，t=20ms) 结论,1合格，0不合格
            record.BF_PQ_U50T1M = "";        //(电压降落ΔU=50%，t=1min) 变化前电量
            record.AF_PQ_U50T1M = "";        //(电压降落ΔU=50%，t=1min) 变化后电量
            record.CONCLUSION_U50T1M = "";        //(电压降落ΔU=50%，t=1min) 结论,1合格，0不合格
            record.CI_CHK_CONC_CODE = "1";        //通讯接口测试结论,1合格，0不合格
            record.RP_MEMORY_CHK = "";        //无功存储器检查,01合格、02不合格
            record.OTHER_MEMORY_CHK = "";        //其它存储器检查,01合格、02不合格
            record.GPS_CALIBRATE_FLAG = GetGPSTime(meter);        //GPS对时,01是、02否
            record.TS_ERR_CONC_CODE = GetPeriodChange(meter);        //费率时段投切误差结论,1合格，0不合格
            if (record.TS_ERR_CONC_CODE == "" || record.TS_ERR_CONC_CODE == null)
            {
                record.TS_ERR_CONC_CODE = "1";        //费率时段投切误差结论,1合格，0不合格 modify
            }
            record.DAILY_TIMING_ERR_CONC = "1";

            //add
            record.CHANGE1_ERR = "";
            record.INT_CHANGE1_ERR = "";
            record.CHANGE2_ERR = "";
            record.INT_CHANGE2_ERR = "";
        }

        ///// <summary>
        ///// 电能表检定结论
        ///// </summary>
        ///// <param name="meter"></param>
        ///// id = "";           结论记录标识,本实体记录的唯一标识,由校表台系统填写,具体填写不做要求
        ///// read_id            记录标识,该字段为外键字段	
        ///// meter_id           电能表标识,电能表标识为电能表的唯一内码信息,取任务信息中的电能表标识
        ///// both_way_p         正反向有无功,正向有功/正向无功/反向有功/反向无功
        ///// start_conc         起动试验结论,校验台软件写入
        ///// creep_conc         潜动试验结论,校验台软件写入
        ///// volt_conc          耐压试验结论,校验台软件写入
        ///// start_curr         起动电流值,校验台软件写入
        ///// start_date         起动时间,校验台软件写入,如:1.5代表1.5分钟
        ///// volt_testv         耐压试验值,校验台软件写入
        ///// err_upperl         误差上限,校验台软件写入
        ///// err_lowerl         误差下限,校验台软件写入
        ///// <returns></returns>
        //private void ProgressMeterResult(TestMeterInfo meter, string meter_id, out CheckResult result)
        //{

        //    result = new CheckResult();
        //    result.id = meter.Meter_ID.ToString();                //结论记录标识,本实体记录的唯一标识,由校表台系统填写,具体填写不做要求
        //    result.read_id = meter_id.ToString();           //记录标识,该字段为外键字段	
        //    result.meter_id = meter_id.ToString();          //电能表标识,电能表标识为电能表的唯一内码信息,取任务信息中的电能表标识
        //    result.BOTH_WAY_POWER_FLAG = "01";                        //正反向有无功,正向有功/正向无功/反向有功/反向无功

        //    result.ERR_UPPER_LIMIT = "1";        //误差上限,校验台软件写入
        //    result.ERR_LOWER_LIMIT = "1";        //误差下限,校验台软件写入

        //    result.ELECT_CONC_CODE = "1";                   //耐压试验值,校验台软件写入---------------------
        //    result.VOLT_CONC = "1";                 //耐压试验结论,校验台软件写入

        //    result.VOLT_TEST_VALUE = 2.0f;

        //    result.START_CONC_CODE = "1";
        //    result.CREEP_CONC_CODE = "1";

        //    Dictionary<string, MeterResult> dic = meter.MeterResults;
        //    foreach (string key in dic.Keys)
        //    {
        //        if (key == ProjectID.起动试验)           //只有大于3才可能是小项目,并且当中要包含启动ID和潜动ID
        //        {
        //            result.START_CONC_CODE = meter.MeterResults[key].Result == ConstHelper.不合格 ? "0" : "1";        //起动试验结论,校验台软件写入
        //        }
        //        if (key == ProjectID.潜动试验)
        //        {
        //            result.CREEP_CONC_CODE = meter.MeterResults[key].Result == ConstHelper.不合格 ? "0" : "1";
        //        }
        //    }

        //    //add
        //    foreach (string key in meter.MeterQdQids.Keys)
        //    {
        //        if (key.Length > 5 && (key.Substring(0, 5) == ProjectID.起动试验))           //只有大于5才可能是小项目,并且当中要包含启动ID和潜动ID
        //        {
        //            result.START_CURRENT = (meter.MeterQdQids[key].Current).ToString();
        //        }
        //    }
        //}

        //modify yjt 20220421 修改启动潜动四个方向 数组
        /// <summary>
        /// 电能表检定结论  修改后数组
        /// </summary>
        /// <param name="meter"></param>
        /// id = "";           结论记录标识,本实体记录的唯一标识,由校表台系统填写,具体填写不做要求
        /// read_id            记录标识,该字段为外键字段	
        /// meter_id           电能表标识,电能表标识为电能表的唯一内码信息,取任务信息中的电能表标识
        /// both_way_p         正反向有无功,正向有功/正向无功/反向有功/反向无功
        /// start_conc         起动试验结论,校验台软件写入
        /// creep_conc         潜动试验结论,校验台软件写入
        /// volt_conc          耐压试验结论,校验台软件写入
        /// start_curr         起动电流值,校验台软件写入
        /// start_date         起动时间,校验台软件写入,如:1.5代表1.5分钟
        /// volt_testv         耐压试验值,校验台软件写入
        /// err_upperl         误差上限,校验台软件写入
        /// err_lowerl         误差下限,校验台软件写入
        /// <returns></returns>
        private void ProgressMeterResult1(TestMeterInfo meter, string meter_id, out CheckResult[] result)
        {
            result = new CheckResult[1];
            result[0] = new CheckResult();

            if (meter.MeterQdQids == null || meter.MeterQdQids.Keys.Count <= 0) return;

            int qid = 0;
            int qiand = 0;
            string qidglfx = "";
            string qiandglfx = "";
            foreach (string key in meter.MeterQdQids.Keys)
            {
                if (key.Length > 5 && (key.Substring(0, 5) == ProjectID.起动试验))
                {
                    qid++;

                    qidglfx += meter.MeterQdQids[key].PowerWay + "|";
                }
                if (key.Length > 5 && (key.Substring(0, 5) == ProjectID.潜动试验))
                {
                    qiand++;
                    qiandglfx += meter.MeterQdQids[key].PowerWay + "|";
                }
            }

            if (qid < qiand)
            {
                qid = qiand;
                qidglfx = qiandglfx;
            }
            qidglfx = qidglfx.TrimEnd('|');
            string[] glfxitem = qidglfx.Split('|');



            result = new CheckResult[qid];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = new CheckResult
                {
                    Id = meter.Meter_ID.ToString(),                //结论记录标识,本实体记录的唯一标识,由校表台系统填写,具体填写不做要求
                    Read_id = meter_id.ToString(),           //记录标识,该字段为外键字段	
                    Meter_id = meter_id.ToString()          //电能表标识,电能表标识为电能表的唯一内码信息,取任务信息中的电能表标识
                };
                //result[i].BOTH_WAY_POWER_FLAG = (i+1).ToString();                //正反向有无功,正向有功/正向无功/反向有功/反向无功

                switch (glfxitem[i])    //功率方向
                {
                    case "正向有功":
                        result[i].BOTH_WAY_POWER_FLAG = "01";
                        break;
                    case "正向无功":
                        result[i].BOTH_WAY_POWER_FLAG = "03";
                        break;
                    case "反向有功":
                        result[i].BOTH_WAY_POWER_FLAG = "02";
                        break;
                    case "反向无功":
                        result[i].BOTH_WAY_POWER_FLAG = "04";
                        break;
                    default:
                        result[i].BOTH_WAY_POWER_FLAG = "01";
                        break;
                }

                result[i].ERR_UPPER_LIMIT = "1";        //误差上限,校验台软件写入
                result[i].ERR_LOWER_LIMIT = "1";        //误差下限,校验台软件写入

                result[i].ELECT_CONC_CODE = "1";                   //通电检查---------------------
                result[i].VOLT_CONC = "1";                 //耐压试验结论,校验台软件写入

                result[i].VOLT_TEST_VALUE = 2.0f;

                result[i].START_CONC_CODE = "1";
                result[i].CREEP_CONC_CODE = "1";

                result[i].START_CURRENT = "0.00";

                Dictionary<string, MeterQdQid> dic = meter.MeterQdQids;

                foreach (string key in dic.Keys)
                {
                    if (key.Length > 5 && (key.Substring(0, 5) == ProjectID.起动试验) && meter.MeterQdQids[key].PowerWay == glfxitem[i])           //只有大于3才可能是小项目,并且当中要包含启动ID和潜动ID
                    {
                        result[i].START_CONC_CODE = meter.MeterQdQids[key].Result == ConstHelper.不合格 ? "0" : "1";        //起动试验结论,校验台软件写入
                        result[i].START_CURRENT = meter.MeterQdQids[key].Current.ToString();
                    }
                    if (key.Length > 5 && (key.Substring(0, 5) == ProjectID.潜动试验) && meter.MeterQdQids[key].PowerWay == glfxitem[i])
                    {
                        result[i].CREEP_CONC_CODE = meter.MeterQdQids[key].Result == ConstHelper.不合格 ? "0" : "1";
                    }
                }
            }
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
        private void ProgressMeterErr(TestMeterInfo meter, string read_id, out CheckErr[] checkErr)
        {
            checkErr = new CheckErr[1];
            checkErr[0] = new CheckErr();
            if (meter.MeterErrors == null || meter.MeterErrors.Keys.Count <= 0) return;
            checkErr = new CheckErr[meter.MeterErrors.Keys.Count];
            for (int i = 0; i < checkErr.Length; i++)
            {
                checkErr[i] = new CheckErr();
            }
            string[] keys = new string[meter.MeterErrors.Keys.Count];
            meter.MeterErrors.Keys.CopyTo(keys, 0);
            for (int i = 0; i < keys.Length; i++)
            {
                string key = keys[i];

                MeterError meterErr = meter.MeterErrors[key];

                //string[] strWc = meterErr.WcMore.Split('|');
                //if (strWc.Length <= 2) continue;

                checkErr[i].pf = "1";

                switch (meterErr.GLYS.Trim())
                {
                    case "1.0":
                        checkErr[i].pf = "1";
                        break;
                    case "0.8C":
                        checkErr[i].pf = "2";
                        break;
                    case "0.5L":
                        checkErr[i].pf = "3";
                        break;
                    case "0.5C":
                        checkErr[i].pf = "4";
                        break;
                    case "0.25L":
                        checkErr[i].pf = "5";
                        break;
                }
                //checkErr[i].id =long.Parse(read_id.ToString()+(i+1).ToString());//误差记录标识,本实体记录的唯一标识号, 由校表台系统填写,具体填写不做要求
                checkErr[i].read_id = read_id;                  //记录标识,该字段为外键字段
                checkErr[i].chk_type_code = "03";                  //检定类别,检定类别，01取样检验、02抽样检定/校准、03装用前检定或校准...
                checkErr[i].load_currt = DataHelper.DataHelper.GetMeterINoByDataForDongRuan(meterErr.IbX);        //负载电流,负载电流，Imax、Ib、0.1Ib、0.2Ib......	
                //checkErr[i].both_way_p = ((ZH.Comm.Enum.PowerWay)int.Parse(_TmpHi.Substring(1, 1))).ToString(); //正反向有无功,正向有功/正向无功/反向有功/反向无功
                switch (meterErr.GLFX)
                {
                    case "正向有功":
                        checkErr[i].BOTH_WAY_POWER_FLAG = "01";
                        break;
                    case "正向无功":
                        checkErr[i].BOTH_WAY_POWER_FLAG = "03";
                        break;
                    case "反向有功":
                        checkErr[i].BOTH_WAY_POWER_FLAG = "02";
                        break;
                    case "反向无功":
                        checkErr[i].BOTH_WAY_POWER_FLAG = "04";
                        break;
                    default:
                        checkErr[i].BOTH_WAY_POWER_FLAG = "01";
                        break;
                }                 //正反向有无功,正向有功/正向无功/反向有功/反向无功
                string[] wc = GetWc(meterErr.WCData, meterErr.WCValue, meterErr.WCHZ);
                string[] err = new string[5];
                for (int j = 0; j < err.Length; j++)
                {
                    if (wc.Length - 2 >= j)
                    {
                        err[j] = wc[j];
                    }
                    else
                    {
                        err[j] = "";
                    }
                }
                //if (key[0] == '1')      //基本误差
                //{
                //    checkErr[i].ave_err = wc[wc.Length - 2];        //平均误差
                //    checkErr[i].orgn_std_err = "";        //标准偏差原始值
                //    checkErr[i].int_convert_err = wc[wc.Length - 1];        //化整误差
                //}
                //else                    //标准偏差
                //{
                //    checkErr[i].ave_err = "";        //平均误差
                //    checkErr[i].orgn_std_err = wc[wc.Length - 2];        //标准偏差原始值
                //    checkErr[i].std_err_int = wc[wc.Length - 1];        //标准偏差化整值
                //}

                checkErr[i].ave_err = wc[wc.Length - 2];        //平均误差
                checkErr[i].orgn_std_err = "";        //标准偏差原始值
                checkErr[i].int_convert_err = wc[wc.Length - 1];        //化整误差

                checkErr[i].err1 = err[0];              //误差1
                checkErr[i].err2 = err[1];              //误差2
                checkErr[i].err3 = err[2];              //误差3
                checkErr[i].err4 = err[3];             //误差4
                checkErr[i].err5 = err[4];              //误差5

                if (meterErr.YJ == "H")
                    checkErr[i].load_stats = "01";        //负荷状况,1：平衡负荷、2：不平衡负荷
                else
                    checkErr[i].load_stats = "02";        //负荷状况,1：平衡负荷、2：不平衡负荷
                checkErr[i].group_type = ConvertYuanJiang(meterErr.YJ);        //测试元组,1:A相,2:B相,3:C相,4:合组，必须传入
            }
        }

        /// <summary>
        /// 电能表走字记录
        /// </summary>
        /// read_id       记录标识,本实体记录的唯一标识号, 由校表台系统填写,具体填写不做要求
        /// send_sn       *发送批号,由校表台系统填写,具体填写不做要求,可以为空
        /// walk_no       *走字编号,由校表台系统填写,具体填写不做要求,可以为空
        /// app_no         申请编号
        /// meter_id       电能表标识,为电能表的唯一内码信息,取任务信息中的电能表标识
        /// runing_nam       走字人员
        /// runing_dat       走字日期
        /// checker_no       核验人员
        /// chk_rec_dt       核验日期,实体为字符型19位
        /// runing_des       走字台编号
        /// temp            温度,如有小数则四舍五入
        /// humidity       湿度,如有小数则四舍五入
        /// pointer_ty       走字指针类型
        /// std_readav     标准表示数平均值
        /// std_relati       标准表相对误差
        /// comp_err          示数组合误差
        /// time_calib     是否已校时,缺省为否
        /// conc_code      走字结论,1合格，0不合格
        /// runing_rem     *走字说明
        /// org_no         *供电单位,保留，厂家可以不填
        /// <param name="meter">电能表基本信息</param>
        /// <returns></returns>
        private void ProgressMeterZZ(TestMeterInfo meter, string meter_id, out ZZ_CheckRecords[] record)
        {
            //将MeterInfo.MeterZZErrors的Item项目枚举出，把所有Key字符串放到DataTable中，并且按照升序检索出来
            DataTable dtKeys = new DataTable();
            dtKeys.Columns.Add("Keys", typeof(string));
            dtKeys.Columns.Add("PrjId", typeof(string));
            foreach (string Key in meter.MeterZZErrors.Keys)
            {
                MeterZZError MeterError = meter.MeterZZErrors[Key];
                dtKeys.Rows.Add(Key, MeterError.PrjID);
            }
            DataRow[] Rows = dtKeys.Select("Keys <>'' and PrjId <> ''", "PrjId asc");
            record = new ZZ_CheckRecords[Rows.Length];
            for (int i = 0; i < Rows.Length; i++)
            {
                MeterZZError meterError = meter.MeterZZErrors[Rows[i][0].ToString()];
                //string prjId = meterError.PrjID;

                record[i].Read_id = meter_id;
                record[i].Send_sn = GetSendNo();
                record[i].Walk_no = GetSendNo();
                record[i].Send_sn = record[i].Send_sn + i;
                record[i].Walk_no = record[i].Walk_no + i;
                record[i].App_no = meter.MD_AssetNo;
                record[i].Meter_id = meter_id;
                record[i].RUNING_PERSON_NAME = meter.CheckerNo1;
                record[i].RUNING_DATE = meter.VerifyDate != "" ? DateTime.Parse(meter.VerifyDate).ToShortDateString() : DateTime.Now.ToShortDateString();
                record[i].RUNING_DATE = string.Format("to_date('{0}','yyyy-mm-dd')", record[i].RUNING_DATE);
                record[i].Checker_no = meter.CheckerNo2;
                record[i].Chk_rec_date = meter.VerifyDate != "" ? DateTime.Parse(meter.VerifyDate).ToShortDateString() : DateTime.Now.ToShortDateString();
                record[i].Chk_rec_date = string.Format("to_date('{0}','yyyy-mm-dd')", record[i].Chk_rec_date);
                record[i].Runing_desk_no = meter.BenthNo.Trim();
                record[i].Temp = meter.Temperature != "" ? float.Parse(meter.Temperature) : 25;
                record[i].Humidity = meter.Humidity != "" ? float.Parse(meter.Humidity) : 25;
                record[i].Pointer_type_code = "";                      //不知道怎么填,走字指针类型
                record[i].STD_READING_AVG = "";        //标准表示数平均值,没有
                record[i].STD_RELATIVE_ERR = "";
                record[i].Comp_err = "";           //示数组合误差,没有
                record[i].TIME_CALIBRATE_FLAG = "";
                record[i].Conc_code = meterError.Result.Trim() == ConstHelper.合格 ? "1" : "0";
                record[i].Runing_remark = GetZZruning_rem(meterError);
                record[i].Org_no = "";
                record[i].Checker_no = meter.CheckerNo2 == "" ? meter.Checker2.Trim() : meter.CheckerNo2;
                record[i].RUNING_PERSON_NAME = meter.CheckerNo1 == "" ? meter.Checker1.Trim() : meter.CheckerNo1;
                record[i].Bar_code = meter.MD_BarCode;
            }
        }

        /// <summary>
        /// 走字计度器
        /// </summary>
        /// read_id                 记录标识,本实体记录的唯一标识号, 由校表台系统填写,具体填写不做要求        
        /// meter_id                电能表标识,为电能表的唯一内码信息,取任务信息中的电能表标识
        /// READ_TYPE_CODE          示数类型
        /// BAR_CODE                条形码
        /// MADE_NO                 核验日期,实体为字符型19位
        /// READING_DIGITS          走字台编号
        /// LAST_READING            温度,如有小数则四舍五入
        /// REGISTER_READ           湿度,如有小数则四舍五入
        /// RUNING_ERR              走字指针类型
        /// T_LAST_READING          标准表示数平均值
        /// T_END_READING           标准表相对误差
        /// AR_TS_READING_ERR       示数组合误差
        /// COMP_ERR                是否已校时,缺省为否
        /// IR_LAST_READING         走字结论,1合格，0不合格
        /// <param name="meter">电能表基本信息</param>
        /// <returns></returns>
        private void ProgressMeterRegister(TestMeterInfo meter, string meterId, out ZZ_CheckRegister[] record)
        {
            //将MeterInfo.MeterZZErrors的Item项目枚举出，把所有Key字符串放到DataTable中，并且按照升序检索出来
            DataTable dtKeys = new DataTable();
            dtKeys.Columns.Add("Keys", typeof(string));
            dtKeys.Columns.Add("PrjId", typeof(string));
            foreach (string k in meter.MeterZZErrors.Keys)
            {
                MeterZZError MeterError = meter.MeterZZErrors[k];
                dtKeys.Rows.Add(k, MeterError.PrjID);
            }
            DataRow[] Rows = dtKeys.Select("Keys <>'' and PrjId <> ''", "PrjId asc");
            record = new ZZ_CheckRegister[Rows.Length];
            for (int i = 0; i < Rows.Length; i++)
            {
                MeterZZError data = meter.MeterZZErrors[Rows[i][0].ToString()];

                record[i].Read_id = meterId.ToString();
                record[i].METER_ID = meterId;
                record[i].READ_TYPE_CODE = GetRd_Type(data.PowerWay, data.Fl);
                record[i].BAR_CODE = meter.MD_BarCode; //add yjt 20220519 新增条形码
                record[i].MADE_NO = meter.MD_MadeNo.Trim();
                record[i].READING_DIGITS = "8";
                record[i].LAST_READING = data.PowerStart.ToString().Trim();
                record[i].REGISTER_READ = data.PowerEnd.ToString().Trim();
                record[i].RUNING_ERR = data.PowerError.ToString().Trim();
                record[i].T_LAST_READING = data.PowerSumStart.ToString().Trim();
                record[i].T_END_READING = data.PowerSumEnd.Trim() == "" ? 0 : Convert.ToSingle(data.PowerSumEnd.Trim());
                record[i].AR_TS_READING_ERR = "0.0";
                record[i].COMP_ERR = "0.0";
                record[i].IR_LAST_READING = "0.0";
            }
        }

        /// <summary>
        /// 获取由电源供电的时钟试验数据
        /// </summary>
        /// <param name="meter"></param>
        /// <returns></returns>
        private string[] GetRJSData(TestMeterInfo meter)
        {
            string[] rjsdata = new string[12];
            string ItemKey = ProjectID.日计时误差;

            //平均值
            if (meter.MeterDgns.ContainsKey(ItemKey))
            {
                string[] databefore = meter.MeterDgns[ItemKey].Value.Split('|');
                if (databefore.Length >= 2)
                {
                    rjsdata[10] = databefore[5];        //由电源供电的时钟试验平均值
                    rjsdata[11] = databefore[6];
                }
                else
                {
                    rjsdata[10] = "0.1";        //由电源供电的时钟试验平均值
                    rjsdata[11] = "0";        //由电源供电的时钟试验化整值
                }

            }
            else
            {
                rjsdata[10] = "";        //由电源供电的时钟试验平均值
                rjsdata[11] = "";        //由电源供电的时钟试验化整值
            }
            //前五次
            if (meter.MeterDgns.ContainsKey(ItemKey))
            {
                string[] databefore = meter.MeterDgns[ItemKey].Value.Split('|');
                rjsdata[0] = databefore[0] != "" ? databefore[0] : "0.000";
                rjsdata[1] = databefore[1] != "" ? databefore[1] : "0.000";
                rjsdata[2] = databefore[2] != "" ? databefore[2] : "0.000";
                rjsdata[3] = databefore[3] != "" ? databefore[3] : "0.000";
                rjsdata[4] = databefore[4] != "" ? databefore[4] : "0.000";
            }
            else
            {
                rjsdata[0] = "";
                rjsdata[1] = "";
                rjsdata[2] = "";
                rjsdata[3] = "";
                rjsdata[4] = "";
            }
            //后五次
            if (meter.MeterDgns.ContainsKey(ItemKey))
            {
                string[] databefore = meter.MeterDgns[ItemKey].Value.Split('|');
                if (databefore.Length > 10)
                {
                    rjsdata[5] = databefore[0] != "" ? databefore[0] : "0.000";
                    rjsdata[6] = databefore[1] != "" ? databefore[1] : "0.000";
                    rjsdata[7] = databefore[2] != "" ? databefore[2] : "0.000";
                    rjsdata[8] = databefore[3] != "" ? databefore[3] : "0.000";
                    rjsdata[9] = databefore[4] != "" ? databefore[4] : "0.000";
                }
                else
                {
                    rjsdata[5] = "0.000";
                    rjsdata[6] = "0.000";
                    rjsdata[7] = "0.000";
                    rjsdata[8] = "0.000";
                    rjsdata[9] = "0.000";
                }

            }
            else
            {
                rjsdata[5] = "";
                rjsdata[6] = "";
                rjsdata[7] = "";
                rjsdata[8] = "";
                rjsdata[9] = "";
            }
            return rjsdata;
        }

        /// <summary>
        /// 获取最大需量数据
        /// </summary>
        /// <param name="meter"></param>
        /// <returns></returns>
        private string[] GetXLData(TestMeterInfo meter)
        {
            string[] xlData = new string[19];

            string ItemKey = ProjectID.需量示值误差;

            //最大需量数据MaxIb
            string key = ItemKey + "_1";
            if (meter.MeterDgns.ContainsKey(key))
            {
                string[] maxdata1 = meter.MeterDgns[key].Value.Split('|');
                xlData[0] = maxdata1[3];
                xlData[1] = maxdata1[4];
                xlData[2] = maxdata1[5];
                if (Number.IsNumeric(maxdata1[5]))
                    xlData[3] = float.Parse(maxdata1[5]).ToString("F1");
                else
                    xlData[3] = "";
            }
            else
            {
                xlData[0] = "";
                xlData[1] = "";
                xlData[2] = "";
                xlData[3] = "";
            }
            //最大需量数据1.0Ib
            key = ItemKey + "_2";
            if (meter.MeterDgns.ContainsKey(key))
            {
                string[] maxdata1 = meter.MeterDgns[key].Value.Split('|');
                xlData[4] = maxdata1[3];
                xlData[5] = maxdata1[4];
                xlData[6] = maxdata1[5];
                if (Number.IsNumeric(maxdata1[5]))
                    xlData[7] = float.Parse(maxdata1[5]).ToString("F1");
                else
                    xlData[7] = "";
            }
            else
            {
                xlData[4] = "";
                xlData[5] = "";
                xlData[6] = "";
                xlData[7] = "";
            }

            //最大需量数据0.1Ib
            key = ItemKey + "_3";
            if (meter.MeterDgns.ContainsKey(key))
            {
                string[] maxdata1 = meter.MeterDgns[key].Value.Split('|');
                xlData[8] = maxdata1[3];
                xlData[9] = maxdata1[4];
                xlData[10] = maxdata1[5];
                if (Number.IsNumeric(maxdata1[5]))
                    xlData[11] = float.Parse(maxdata1[5]).ToString("F1");
                else
                    xlData[11] = "";
            }
            else
            {
                xlData[8] = "";
                xlData[9] = "";
                xlData[10] = "";
                xlData[11] = "";
            }

            //需量周期误差
            key = ItemKey;
            if (meter.MeterDgns.ContainsKey(key))
            {
                string[] maxdata1 = meter.MeterDgns[key].Value.Split('|');
                xlData[12] = maxdata1[3];
                xlData[13] = maxdata1[4];
                xlData[14] = maxdata1[5];
                if (maxdata1.Length > 3)
                    xlData[15] = float.Parse(maxdata1[5]).ToString("F1");
                else
                    xlData[15] = "";
            }
            else
            {
                xlData[12] = "";
                xlData[13] = "";
                xlData[14] = "";
                xlData[15] = "";
            }
            return xlData;
        }

        /// <summary>
        /// 获取GSP对时结果
        /// </summary>
        /// <param name="meter"></param>
        /// <returns></returns>
        private string GetGPSTime(TestMeterInfo meter)
        {
            string key = ProjectID.GPS对时;
            if (meter.MeterDgns.ContainsKey(key))
            {
                return meter.MeterDgns[key].Result == ConstHelper.合格 ? "1" : "0";
            }
            return "";
        }


        private string GetPeriodChange(TestMeterInfo meter)
        {
            string itemKey = ProjectID.时段投切;
            if (meter.MeterDgns.ContainsKey(itemKey))
            {
                return meter.MeterDgns[itemKey].Result == ConstHelper.合格 ? "1" : "0";
            }

            return "";
        }

        /// <summary>
        /// 获取发送批号/走字编号
        /// </summary>
        /// <returns></returns>
        private string GetSendNo()
        {
            return DateTime.Now.ToString("yyyyMMdd00mmss");
        }

        /// <summary>
        /// 获取误差值
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private string[] GetWc(string value, string pjz, string hzz)
        {
            string[] wc = value.Split('|');
            string[] pWc = new string[7];
            pWc[5] = pjz;
            pWc[6] = hzz;
            for (int i = 0; i < pWc.Length - 2; i++)
            {
                if (i < wc.Length)
                {
                    pWc[i] = wc[i];
                }
                else
                {
                    pWc[i] = "";
                }
            }
            return pWc;
        }

        private string ConvertYuanJiang(string str)
        {
            string yuanjian;
            switch (str)
            {
                case "1":
                    yuanjian = "04";
                    break;
                case "2":
                    yuanjian = "01";
                    break;
                case "3":
                    yuanjian = "02";
                    break;
                case "4":
                    yuanjian = "03";
                    break;
                default:
                    yuanjian = "04";
                    break;
            }
            return yuanjian;
        }

        /// <summary>
        /// 获取走字说明
        /// </summary>
        /// <param name="meterError"></param>
        /// <returns></returns>
        private string GetZZruning_rem(MeterZZError meterError)
        {
            string[] prjId = meterError.PrjID.Split('_');
            string data = ((PowerWay)int.Parse(prjId[1][0].ToString())).ToString();

            switch (meterError.YJ)
            {
                case "H":
                    data += "|合元"; break;
                case "A":
                    data += "|A元"; break;
                case "B":
                    data += "|B元"; break;
                case "C":
                    data += "|C元"; break;
                default:
                    data += "|合元"; break;
            }

            data += "|" + meterError.GLYS;

            data += "|" + meterError.IbXString;

            switch (meterError.Fl)
            {
                case "总":
                    data += "|" + "总"; break;
                case "尖":
                    data += "|" + "尖"; break;
                case "峰":
                    data += "|" + "峰"; break;
                case "平":
                    data += "|" + "平"; break;
                case "谷":
                    data += "|" + "谷"; break;
                case "深谷":
                    data += "|" + "深谷"; break;
                default:
                    data += "|" + "总"; break;
            }

            data += "|起码:" + meterError.PowerStart.ToString();
            data += "|止码:" + meterError.PowerEnd.ToString();
            data += "|表码差:" + meterError.WarkPower ?? "";
            data += "|误差:" + meterError.PowerError;
            return data;
        }


        private string GetRd_Type(string Glfx, string FeiLv)
        {
            string tmpType;

            switch (Glfx)
            {
                case "正向有功":
                    tmpType = "1";
                    break;
                case "正向无功":
                    tmpType = "2";
                    break;
                case "反向有功":
                    tmpType = "8";
                    break;
                case "反向无功":
                    tmpType = "9";
                    break;
                default:
                    tmpType = "1";
                    break;
            }
            FeiLv = FeiLv.Trim();
            switch (FeiLv)
            {
                case "总":
                    tmpType += "1";
                    break;
                case "峰":
                    tmpType += "3";
                    break;
                case "平":
                    tmpType += "5";
                    break;
                case "谷":
                    tmpType += "4";
                    break;
                case "尖":
                    tmpType += "2";
                    break;
                default:
                    tmpType += "1";
                    break;
            }
            return tmpType;
        }

        /// <summary>
        /// 从SG186数据库根据代码获取代码值
        /// </summary>
        /// <param name="proName"></param>
        /// <param name="proID"></param>
        /// <returns></returns>
        private string GetValue(string proName, string proID)
        {
            string value = "";
            if (proID == "")
                return value;
            string sql = string.Format(@"select name from SA_PROP_LIST where CODE_SORT_ID='{0}' and CODE_ID='{1}'", proName, proID);
            object o = ExecuteScalar(sql);

            if (o != null)
            {
                value = o.ToString().Trim();
            }
            return value;
        }

        public bool SchemeDown(TestMeterInfo barcode, out string schemeName, out Dictionary<string, SchemaNode> Schema)
        {
            throw new NotImplementedException();
        }
    }
}
