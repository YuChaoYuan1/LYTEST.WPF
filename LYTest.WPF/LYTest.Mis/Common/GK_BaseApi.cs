using ICInterface.Base_ICStructure;
using ICInterface.ICApiStructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LYTest.Mis.Common
{
    /// <summary>
    /// 工况基础服务API
    /// </summary>
    public class GK_BaseApi
    {

        GK_ApiConfig GK_Config;
        public GK_BaseApi(GK_ApiConfig gK_Config)
        {
            GK_Config = gK_Config;
        }

        public getLegalCertRsltResult 芯片id合法性认证接口(getLegalCertRslt data, out string Msg, int MaxTime = 60000)
        {
            string LogName = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff") + "芯片id合法性认证接口";
            GKLogHelper.WriteLog(LogName, JsonHelper.序列化对象(data));
            string r = ApiHelper.HttpApi(GK_Config.芯片id合法性认证接口, JsonHelper.序列化对象(data), "POST", MaxTime, out Msg);
            GKLogHelper.WriteLog(LogName, r);
            return JsonHelper.反序列化字符串<getLegalCertRsltResult>(r);
        }
        public bool 检定设备申请接口(applyEquip data, out string Msg, int MaxTime = 60000)
        {
            string LogName = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff") + "检定设备申请接口";
            GKLogHelper.WriteLog(LogName, JsonHelper.序列化对象(data));
            string r = ApiHelper.HttpApi(GK_Config.检定设备申请接口, JsonHelper.序列化对象(data), "POST", MaxTime, out Msg);
            GKLogHelper.WriteLog(LogName, r);
            var callResult = JsonHelper.反序列化字符串<ICCellResult>(r);
            if (callResult == null)
            {
                Msg = "设备申请错误,没有接收到返回值!!!";
                return false;
            }
            if (callResult.resultFlag == "1")
            {
                return true;
            }
            else
            {
                Msg = "设备申请失败:" + callResult.errorInfo;
                return false;
            }
        }
        public ICCellResult 空箱设备申请接口(applyAssistEquip data, out string Msg, int MaxTime = 60000)
        {
            string LogName = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff") + "空箱设备申请接口";
            GKLogHelper.WriteLog(LogName, JsonHelper.序列化对象(data));
            string r = ApiHelper.HttpApi(GK_Config.空箱设备申请接口, JsonHelper.序列化对象(data), "POST", MaxTime, out Msg);
            GKLogHelper.WriteLog(LogName, r);
            return JsonHelper.反序列化字符串<ICCellResult>(r);
        }
        public bool 箱核对信息接口(boxCheckInfo data, out string Msg, int MaxTime = 60000)
        {
            string LogName = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff") + "箱核对信息接口";
            GKLogHelper.WriteLog(LogName, JsonHelper.序列化对象(data));
            string r = ApiHelper.HttpApi(GK_Config.箱核对信息接口, JsonHelper.序列化对象(data), "POST", MaxTime, out Msg);
            GKLogHelper.WriteLog(LogName, r);
            var callResult = JsonHelper.反序列化字符串<ICCellResult>(r);
            if (callResult == null)
            {
                Msg = "箱核对信息错误,没有收到返回值。";
                return false;
            }
            if (callResult.resultFlag == "1")
            {
                return true;
            }
            else
            {
                Msg = "箱核对信息失败:" + callResult.errorInfo;
                return false;
            }
        }
        public ICCellResult 检定设备核对信息上传接口(equipCheckInfo data, out string Msg, int MaxTime = 60000)
        {
            string LogName = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff") + "检定设备核对信息上传接口";
            GKLogHelper.WriteLog(LogName, JsonHelper.序列化对象(data));
            string r = ApiHelper.HttpApi(GK_Config.检定设备核对信息上传接口, JsonHelper.序列化对象(data), "POST", MaxTime, out Msg);
            GKLogHelper.WriteLog(LogName, r);
            return JsonHelper.反序列化字符串<ICCellResult>(r);
        }


        public bool 施封信息接口(uploadSealsCode data, out string Msg, int MaxTime = 60000)
        {
            string LogName = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff") + "施封信息接口";
            GKLogHelper.WriteLog(LogName, JsonHelper.序列化对象(data));
            string r = ApiHelper.HttpApi(GK_Config.施封信息接口, JsonHelper.序列化对象(data), "POST", MaxTime, out Msg);
            GKLogHelper.WriteLog(LogName, r);
            var callResult = JsonHelper.反序列化字符串<ICCellResult>(r);
            if (callResult == null)
            {

                Msg = "施封信息接口错误,没有收到返回值。";
                return false;
            }
            if (callResult.resultFlag != "1")
            {
                Msg = callResult.errorInfo;
                return false;
            }
            return true;
        }
        public bool 设备组箱信息接口(uploadPackInfo data, out string Msg, DateTime Time, int MaxTime = 60000)
        {
            string LogName = Time.ToString("yyyy-MM-dd HH:mm:ss:fff") + "设备组箱信息接口";
            GKLogHelper.WriteLog(LogName, JsonHelper.序列化对象(data));
            string r = ApiHelper.HttpApi(GK_Config.设备组箱信息接口, JsonHelper.序列化对象(data), "POST", MaxTime, out Msg);
            GKLogHelper.WriteLog(LogName, r);
            var callResult = JsonHelper.反序列化字符串<ICCellResult>(r);
            if (callResult == null)
            {

                Msg = "设备组箱信息接口错误,没有收到返回值。";
                return false;
            }
            if (callResult.resultFlag != "1")
            {
                Msg = callResult.errorInfo;
                return false;
            }
            return true;
        }
        public bool 周转箱组垛信息接口(upboxPileDet data, out string Msg, DateTime Time, int MaxTime = 60000)
        {
            string LogName = Time.ToString("yyyy-MM-dd HH:mm:ss:fff") + "周转箱组垛信息接口";
            GKLogHelper.WriteLog(LogName, JsonHelper.序列化对象(data));
            string r = ApiHelper.HttpApi(GK_Config.周转箱组垛信息接口, JsonHelper.序列化对象(data), "POST", MaxTime, out Msg);
            GKLogHelper.WriteLog(LogName, r);
            var callResult = JsonHelper.反序列化字符串<ICCellResult>(r);
            if (callResult == null)
            {
                Msg = "周转箱组垛信息错误,没有收到返回值。";
                return false;
            }
            if (callResult.resultFlag != "1")
            {
                Msg = callResult.errorInfo;
                return false;
            }
            return true;
        }
        public bool 检定任务完成接口(sendTaskFinish data, out string Msg, int MaxTime = 60000)
        {
            string LogName = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff") + "检定任务完成接口";
            GKLogHelper.WriteLog(LogName, JsonHelper.序列化对象(data));
            string r = ApiHelper.HttpApi(GK_Config.检定任务完成接口, JsonHelper.序列化对象(data), "POST", MaxTime, out Msg);
            GKLogHelper.WriteLog(LogName, r);
            var callResult = JsonHelper.反序列化字符串<ICCellResult>(r);
            if (callResult == null)
            {
                Msg = "任务完成错误,没有收到返回值。";
                return false;
            }
            if (callResult.resultFlag == "1")
            {
                return true;
            }
            Msg = callResult.errorInfo;
            return false;
        }
        //
        public T 设备参数信息获取接口<T>(getEquipParamCell data, out string Msg, int MaxTime = 60000)
        {
            string LogName = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff") + "设备参数信息获取接口";
            GKLogHelper.WriteLog(LogName, JsonHelper.序列化对象(data));
            string r = ApiHelper.HttpApi(GK_Config.设备参数信息获取接口, JsonHelper.序列化对象(data), "POST", MaxTime, out Msg);
            GKLogHelper.WriteLog(LogName, r);
            return JsonHelper.反序列化字符串<T>(r);
        }

        public bool 检定综合结论接口(IVeriResult data, out string Msg, DateTime Time, int MaxTime = 60000)
        {
            string LogName = Time.ToString("yyyy-MM-dd HH:mm:ss:fff") + "检定综合结论接口";
            GKLogHelper.WriteLog(LogName, JsonHelper.序列化对象(data));
            string r = ApiHelper.HttpApi(GK_Config.检定综合结论接口, JsonHelper.序列化对象(data), "POST", MaxTime, out Msg);
            GKLogHelper.WriteLog(LogName, r);
            var cellResult = JsonHelper.反序列化字符串<ICCellResult>(r);
            if (cellResult == null)
            {
                Msg = "上传总结论数据错误,没有收到返回值。";
                return false;
            }
            if (cellResult.resultFlag != "1")
            {
                Msg = "上传总结论数据失败:" + cellResult.errorInfo;
                return false;
            }
            return true;
        }
        public bool 检定分项结论接口(IDETedTestData data, out string Msg, DateTime Time, int MaxTime = 60000)
        {
            string LogName = Time.ToString("yyyy-MM-dd HH:mm:ss:fff") + "检定分项结论接口";
            GKLogHelper.WriteLog(LogName, JsonHelper.序列化对象(data));
            string r = ApiHelper.HttpApi(GK_Config.检定分项结论接口, JsonHelper.序列化对象(data), "POST", MaxTime, out Msg);
            GKLogHelper.WriteLog(LogName, r);
            ICCellResult cellResult = JsonHelper.反序列化字符串<ICCellResult>(r);
            if (cellResult == null)
            {
                Msg = "上传分项数据错误,没有收到返回值。";
                return false;
            }
            if (cellResult.resultFlag != "1")
            {
                Msg = "上传分项数据失败:" + cellResult.errorInfo;
                return false;
            }
            return true;
        }
        public getVeriSchInfoResult 检定方案信息获取接口(getVeriSchInfoCell data, out string Msg, int MaxTime = 60000)
        {
            string LogName = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff") + "检定方案信息获取接口";
            GKLogHelper.WriteLog(LogName, JsonHelper.序列化对象(data));
            string r = ApiHelper.HttpApi(GK_Config.检定方案信息获取接口, JsonHelper.序列化对象(data), "POST", MaxTime, out Msg);
            GKLogHelper.WriteLog(LogName, r);
            return JsonHelper.反序列化字符串<getVeriSchInfoResult>(r);
        }
        public getVeriSchInfoResult 检定方案信息获取接口(string json, out string Msg, int MaxTime = 60000)
        {
            string LogName = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff") + "检定方案信息获取接口";
            GKLogHelper.WriteLog(LogName, json);
            string r = ApiHelper.HttpApi(GK_Config.检定方案信息获取接口, json, "POST", MaxTime, out Msg);
            GKLogHelper.WriteLog(LogName, r);
            return JsonHelper.反序列化字符串<getVeriSchInfoResult>(r);
        }
        public getStdCodeResult 标准代码获取接口(getStdCodesCell data, out string Msg, int MaxTime = 60000)
        {
            string LogName = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff") + "标准代码获取接口";
            GKLogHelper.WriteLog(LogName, JsonHelper.序列化对象(data));
            string r = ApiHelper.HttpApi(GK_Config.标准代码获取接口, JsonHelper.序列化对象(data), "POST", MaxTime, out Msg);
            GKLogHelper.WriteLog(LogName, r);
            return JsonHelper.反序列化字符串<getStdCodeResult>(r);
        }
        public getVeriTaskResult 检定台任务信息获取接口(getVeriTaskCell data, out string Msg, int MaxTime = 60000)
        {
            string LogName = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff") + "检定台任务信息获取接口";
            GKLogHelper.WriteLog(LogName, JsonHelper.序列化对象(data));
            string r = ApiHelper.HttpApi(GK_Config.检定台任务信息获取接口, JsonHelper.序列化对象(data), "POST", MaxTime, out Msg);
            GKLogHelper.WriteLog(LogName, r);
            return JsonHelper.反序列化字符串<getVeriTaskResult>(r);

        }

    }


}
