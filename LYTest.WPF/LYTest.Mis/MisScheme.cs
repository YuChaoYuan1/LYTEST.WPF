using LYTest.Core.Enum;
using LYTest.Core.Model.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LYTest.Mis
{
    class MisScheme
    {

        /// <summary>
        /// 根据编号获取通讯协议检查的名字
        /// </summary>
        /// <param name="Num"></param>
        /// <returns></returns>
        public static string GetProtocolName(string Num)
        {
            string connProtocolItem = Num;
            switch (Num)
            {
                case "0":
                    connProtocolItem = "自动循环显示第1屏";
                    break;
                case "1":
                    connProtocolItem = "自动循环显示第2屏";
                    break;
                case "2":
                    connProtocolItem = "自动循环显示第3屏";
                    break;
                case "3":
                    connProtocolItem = "自动循环显示第4屏";
                    break;
                case "4":
                    connProtocolItem = "自动循环显示第5屏";
                    break;
                case "5":
                    connProtocolItem = "自动循环显示第6屏";
                    break;
                case "6":
                    connProtocolItem = "自动循环显示第7屏";
                    break;
                case "7":
                    connProtocolItem = "按键循环显示第1屏";
                    break;
                case "8":
                    connProtocolItem = "按键循环显示第2屏";
                    break;
                case "9":
                    connProtocolItem = "按键循环显示第3屏";
                    break;
                case "10":
                    connProtocolItem = "按键循环显示第4屏";
                    break;
                case "11":
                    connProtocolItem = "按键循环显示第5屏";
                    break;
                case "12":
                    connProtocolItem = "按键循环显示第6屏";
                    break;
                case "13":
                    connProtocolItem = "按键循环显示第7屏";
                    break;
                case "14":
                    connProtocolItem = "按键循环显示第8屏";
                    break;
                case "15":
                    connProtocolItem = "按键循环显示第9屏";
                    break;
                case "16":
                    connProtocolItem = "按键循环显示第10屏";
                    break;
                case "17":
                    connProtocolItem = "按键循环显示第11屏";
                    break;
                case "18":
                    connProtocolItem = "按键循环显示第12屏";
                    break;
                case "19":
                    connProtocolItem = "按键循环显示第13屏";
                    break;
                case "20":
                    connProtocolItem = "按键循环显示第14屏";
                    break;
                case "21":
                    connProtocolItem = "按键循环显示第15屏";
                    break;
                case "22":
                    connProtocolItem = "按键循环显示第16屏";
                    break;
                case "23":
                    connProtocolItem = "按键循环显示第17屏";
                    break;
                case "24":
                    connProtocolItem = "按键循环显示第18屏";
                    break;
                case "25":
                    connProtocolItem = "按键循环显示第19屏";
                    break;
                case "26":
                    connProtocolItem = "按键循环显示第20屏";
                    break;
                case "27":
                    connProtocolItem = "按键循环显示第21屏";
                    break;
                case "28":
                    connProtocolItem = "按键循环显示第22屏";
                    break;
                case "29":
                    connProtocolItem = "按键循环显示第23屏";
                    break;
                case "30":
                    connProtocolItem = "按键循环显示第24屏";
                    break;
                case "31":
                    connProtocolItem = "按键循环显示第25屏";
                    break;
                case "32":
                    connProtocolItem = "按键循环显示第26屏";
                    break;
                case "33":
                    connProtocolItem = "按键循环显示第27屏";
                    break;
                case "34":
                    connProtocolItem = "按键循环显示第28屏";
                    break;
                case "35":
                    connProtocolItem = "按键循环显示第29屏";
                    break;
                case "36":
                    connProtocolItem = "按键循环显示第30屏";
                    break;
                case "37":
                    connProtocolItem = "按键循环显示第31屏";
                    break;
                case "38":
                    connProtocolItem = "按键循环显示第32屏";
                    break;
                case "39":
                    connProtocolItem = "按键循环显示第33屏";
                    break;
                case "40":
                    connProtocolItem = "按键循环显示第34屏";
                    break;
                case "41":
                    connProtocolItem = "按键循环显示第35屏";
                    break;
                case "42":
                    connProtocolItem = "按键循环显示第36屏";
                    break;
                case "43":
                    connProtocolItem = "按键循环显示第37屏";
                    break;
                case "44":
                    connProtocolItem = "按键循环显示第38屏";
                    break;
                case "45":
                    connProtocolItem = "按键循环显示第39屏";
                    break;
                case "46":
                    connProtocolItem = "按键循环显示第40屏";
                    break;
                case "47":
                    connProtocolItem = "按键循环显示第41屏";
                    break;
                case "48":
                    connProtocolItem = "按键循环显示第42屏";
                    break;
                case "49":
                    connProtocolItem = "按键循环显示第43屏";
                    break;
                case "50":
                    connProtocolItem = "按键循环显示第44屏";
                    break;
                case "51":
                    connProtocolItem = "按键循环显示第45屏";
                    break;
                case "52":
                    connProtocolItem = "按键循环显示第46屏";
                    break;
                case "53":
                    connProtocolItem = "按键循环显示第47屏";
                    break;
                case "54":
                    connProtocolItem = "按键循环显示第48屏";
                    break;
                case "55":
                    connProtocolItem = "按键循环显示第49屏";
                    break;
                case "56":
                    connProtocolItem = "按键循环显示第50屏";
                    break;
                case "57":
                    connProtocolItem = "按键循环显示第51屏";
                    break;
                case "58":
                    connProtocolItem = "按键循环显示第52屏";
                    break;
                case "59":
                    connProtocolItem = "按键循环显示第53屏";
                    break;
                case "60":
                    connProtocolItem = "按键循环显示第54屏";
                    break;
                case "61":
                    connProtocolItem = "按键循环显示第55屏";
                    break;
                case "62":
                    connProtocolItem = "按键循环显示第56屏";
                    break;
                case "63":
                    connProtocolItem = "按键循环显示第57屏";
                    break;
                case "64":
                    connProtocolItem = "按键循环显示第58屏";
                    break;
                case "65":
                    connProtocolItem = "按键循环显示第59屏";
                    break;
                case "66":
                    connProtocolItem = "按键循环显示第60屏";
                    break;
                case "67":
                    connProtocolItem = "按键循环显示第61屏";
                    break;
                case "68":
                    connProtocolItem = "按键循环显示第62屏";
                    break;
                case "69":
                    connProtocolItem = "按键循环显示第63屏";
                    break;
                case "70":
                    connProtocolItem = "按键循环显示第64屏";
                    break;
                case "71":
                    connProtocolItem = "按键循环显示第65屏";
                    break;
                case "72":
                    connProtocolItem = "按键循环显示第66屏";
                    break;
                case "73":
                    connProtocolItem = "按键循环显示第67屏";
                    break;
                case "74":
                    connProtocolItem = "按键循环显示第68屏";
                    break;
                case "75":
                    connProtocolItem = "按键循环显示第69屏";
                    break;
                case "76":
                    connProtocolItem = "按键循环显示第70屏";
                    break;
                case "77":
                    connProtocolItem = "按键循环显示第71屏";
                    break;
                case "78":
                    connProtocolItem = "按键循环显示第72屏";
                    break;
                case "79":
                    connProtocolItem = "按键循环显示第73屏";
                    break;
                case "80":
                    connProtocolItem = "按键循环显示第74屏";
                    break;
                case "81":
                    connProtocolItem = "按键循环显示第75屏";
                    break;
                case "82":
                    connProtocolItem = "按键循环显示第76屏";
                    break;
                case "83":
                    connProtocolItem = "按键循环显示第77屏";
                    break;
                case "84":
                    connProtocolItem = "按键循环显示第78屏";
                    break;
                case "85":
                    connProtocolItem = "按键循环显示第79屏";
                    break;
                case "86":
                    connProtocolItem = "按键循环显示第80屏";
                    break;
                case "87":
                    connProtocolItem = "按键循环显示第81屏";
                    break;
                case "88":
                    connProtocolItem = "按键循环显示第82屏";
                    break;
                case "89":
                    connProtocolItem = "按键循环显示第83屏";
                    break;
                case "90":
                    connProtocolItem = "按键循环显示第84屏";
                    break;
                case "91":
                    connProtocolItem = "有功常数";
                    break;
                case "92":
                    connProtocolItem = "资产编号";
                    break;
                case "93":
                    connProtocolItem = "有功组合方式特征字";
                    break;
                case "94":
                    connProtocolItem = "电表运行特征字1";
                    break;
                case "95":
                    connProtocolItem = "自动循环显示屏数";
                    break;
                case "96":
                    connProtocolItem = "按键显示屏数";
                    break;
                case "97":
                    connProtocolItem = "每月第1结算日";
                    break;
                default:
                    break;
            }
            return connProtocolItem;
        }

        public static string JoinValue(params string[] values)
        {
            return string.Join("|", values);
        }

        //public static Dictionary<string, SchemaNode> SortScheme(Dictionary<string, SchemaNode> Schema)
        //{
        //    Dictionary<string, SchemaNode> TemScheme = new Dictionary<string, SchemaNode>();

        //    if (Schema.ContainsKey(ProjectID.接线检查))
        //        TemScheme.Add(ProjectID.接线检查, Schema[ProjectID.接线检查]);
        //    if (Schema.ContainsKey(ProjectID.密钥更新_预先调试))
        //        TemScheme.Add(ProjectID.密钥更新_预先调试, Schema[ProjectID.密钥更新_预先调试]);
        //    if (Schema.ContainsKey(ProjectID.密钥恢复_预先调试))
        //        TemScheme.Add(ProjectID.密钥恢复_预先调试, Schema[ProjectID.密钥恢复_预先调试]);
        //    if (Schema.ContainsKey(ProjectID.起动试验))
        //        TemScheme.Add(ProjectID.起动试验, Schema[ProjectID.起动试验]);
        //    if (Schema.ContainsKey(ProjectID.潜动试验))
        //        TemScheme.Add(ProjectID.潜动试验, Schema[ProjectID.潜动试验]);
        //    if (Schema.ContainsKey(ProjectID.基本误差试验))
        //        TemScheme.Add(ProjectID.基本误差试验, Schema[ProjectID.基本误差试验]);
        //    if (Schema.ContainsKey(ProjectID.电能表常数试验))
        //        TemScheme.Add(ProjectID.电能表常数试验, Schema[ProjectID.电能表常数试验]);
        //    if (Schema.ContainsKey(ProjectID.GPS对时))
        //        TemScheme.Add(ProjectID.GPS对时, Schema[ProjectID.GPS对时]);
        //    if (Schema.ContainsKey(ProjectID.日计时误差))
        //        TemScheme.Add(ProjectID.日计时误差, Schema[ProjectID.日计时误差]);
        //    if (Schema.ContainsKey(ProjectID.需量示值误差))
        //        TemScheme.Add(ProjectID.需量示值误差, Schema[ProjectID.需量示值误差]);
        //    if (Schema.ContainsKey(ProjectID.电量清零))
        //        TemScheme.Add(ProjectID.电量清零, Schema[ProjectID.电量清零]);
        //    if (Schema.ContainsKey(ProjectID.误差一致性))
        //        TemScheme.Add(ProjectID.误差一致性, Schema[ProjectID.误差一致性]);
        //    if (Schema.ContainsKey(ProjectID.误差变差))
        //        TemScheme.Add(ProjectID.误差变差, Schema[ProjectID.误差变差]);
        //    if (Schema.ContainsKey(ProjectID.负载电流升将变差))
        //        TemScheme.Add(ProjectID.负载电流升将变差, Schema[ProjectID.负载电流升将变差]);
        //    if (Schema.ContainsKey(ProjectID.通讯协议检查试验))
        //        TemScheme.Add(ProjectID.通讯协议检查试验, Schema[ProjectID.通讯协议检查试验]);
        //    if (Schema.ContainsKey(ProjectID.通讯协议检查试验2))
        //        TemScheme.Add(ProjectID.通讯协议检查试验2, Schema[ProjectID.通讯协议检查试验2]);
        //    if (Schema.ContainsKey(ProjectID.身份认证))
        //        TemScheme.Add(ProjectID.身份认证, Schema[ProjectID.身份认证]);
        //    if (Schema.ContainsKey(ProjectID.远程控制))
        //        TemScheme.Add(ProjectID.远程控制, Schema[ProjectID.远程控制]);
        //    if (Schema.ContainsKey(ProjectID.报警功能))
        //        TemScheme.Add(ProjectID.报警功能, Schema[ProjectID.报警功能]);
        //    if (Schema.ContainsKey(ProjectID.远程保电))
        //        TemScheme.Add(ProjectID.远程保电, Schema[ProjectID.远程保电]);
        //    if (Schema.ContainsKey(ProjectID.保电解除))
        //        TemScheme.Add(ProjectID.保电解除, Schema[ProjectID.保电解除]);
        //    if (Schema.ContainsKey(ProjectID.数据回抄))
        //        TemScheme.Add(ProjectID.数据回抄, Schema[ProjectID.数据回抄]);
        //    if (Schema.ContainsKey(ProjectID.钱包初始化))
        //        TemScheme.Add(ProjectID.钱包初始化, Schema[ProjectID.钱包初始化]);
        //    if (Schema.ContainsKey(ProjectID.密钥更新))
        //        TemScheme.Add(ProjectID.密钥更新, Schema[ProjectID.密钥更新]);

        //    //  身份认证 = "25001";
        //    //  远程控制 = "25002";
        //    //  报警功能 = "25003";
        //    //  远程保电 = "25004";
        //    //  保电解除 = "25005";
        //    //  数据回抄 = "25006";
        //    //  钱包初始化 = "25007";
        //    //  密钥更新 = "25008";
        //    //  密钥恢复 = "25009";

        //    return TemScheme;
        //}
    }
}
