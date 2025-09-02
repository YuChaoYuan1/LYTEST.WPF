using System;

namespace LyTest.Mis.Taida
{
    /// <summary>
    /// 电能表检定误差
    /// </summary>
    public class CheckErr
    {
        ///// <summary>
        ///// 误差记录标识
        ///// 本实体记录的唯一标识号, 由校表台系统填写,具体填写不做要求
        ///// </summary>	
        //public long id { get; set; }

        ///// <summary>
        ///// 记录标识
        ///// 该字段为外键字段
        ///// </summary>
        //public string read_id { get; set; }

        ///// <summary>
        ///// 功率因数
        ///// 功率因数，COSφ＝1、COSφ＝0.5L.......
        ///// </summary>
        //public string pf { get; set; }

        ///// <summary>
        ///// 负载电流
        ///// 负载电流，Imax、Ib、0.1Ib、0.2Ib......
        ///// </summary>	
        //public string load_currt { get; set; }

        ///// <summary>
        ///// 正反向有无功
        ///// 正向有功/正向无功/反向有功/反向无功
        ///// </summary>	
        //public string BOTH_WAY_POWER_FLAG { get; set; }


        ///// <summary>
        ///// 检定类别
        ///// 检定类别，01取样检验、02抽样检定/校准、03装用前检定或校准...
        ///// </summary>
        //public string chk_type_code { get; set; }


        ///// <summary>
        ///// 标准偏差原始值
        ///// </summary>	
        //public string orgn_std_err { get; set; }

        ///// <summary>
        ///// 误差1
        ///// </summary>
        //public string err1 { get; set; }

        ///// <summary>
        ///// 误差2
        ///// </summary>
        //public string err2 { get; set; }

        ///// <summary>
        ///// 误差3
        ///// </summary>	
        //public string err3 { get; set; }

        ///// <summary>
        ///// 误差4
        ///// </summary>
        //public string err4 { get; set; }

        ///// <summary>
        ///// 误差5
        ///// </summary>
        //public string err5 { get; set; }

        ///// <summary>
        ///// 平均误差
        ///// </summary>
        //public string ave_err { get; set; }

        ///// <summary>
        ///// 化整误差
        ///// </summary>
        //public string int_convert_err { get; set; }

        ///// <summary>
        ///// 标准偏差化整值
        ///// </summary>	
        //public string std_err_int { get; set; }

        ///// <summary>
        ///// 负荷状况
        ///// 01：平衡负荷、02：不平衡负荷
        ///// </summary>
        //public string load_stats { get; set; }

        ///// <summary>
        ///// 测试元组
        ///// 01:A相,02:B相,03:C相,04:合组
        ///// </summary>
        //public string group_type { get; set; }

        ///// <summary>
        ///// 误差上限
        ///// </summary>
        //public string err_upper_limit { get; set; }


        ///// <summary>
        ///// 误差下限
        ///// </summary>
        //public string err_lower_limit { get; set; }

        /// <summary>
        /// 
        /// 计量工单号
        /// </summary>
        public string JLDH { get; set; }

        /// <summary>
        /// 
        /// 条形编码
        /// </summary>
        public string Txbm { get; set; }

        /// <summary>
        /// 
        /// 出厂编号
        /// </summary>	
        public string Ccbh { get; set; }

        /// <summary>
        /// 
        /// 校验日期
        /// </summary>	
        public DateTime Xyrq { get; set; }


        /// <summary>
        /// 
        /// 误差1
        /// </summary>
        public string Error1 { get; set; }


        /// <summary>
        /// 误差2
        /// </summary>	
        public string Error2 { get; set; }

        /// <summary>
        /// 化整
        /// </summary>
        public string Chaninct { get; set; }

        /// <summary>
        /// 合分元
        /// </summary>
        public string Balance { get; set; }

        /// <summary>
        /// 电流幅度
        /// </summary>	
        public string Dlfd { get; set; }

        /// <summary>
        /// 电压幅度
        /// </summary>
        public string Dyfd { get; set; }

        /// <summary>
        /// 校验类型
        /// </summary>
        public string Xylx { get; set; }

        /// <summary>
        /// 相序
        /// </summary>
        public string Xx { get; set; }

        /// <summary>
        /// 相位
        /// </summary>
        public string Xw { get; set; }

        /// <summary>
        /// 备注
        /// </summary>	
        public string Bz { get; set; }
    }
}
