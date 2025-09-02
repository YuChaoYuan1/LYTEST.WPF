namespace LYTest.Mis.DataHelper
{
    internal class DataHelper
    {
        /// <summary>
        /// 通过电流负载值获取编号
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string GetMeterINoByDataForDongRuan(string data)
        {
            string no;
            switch (data)
            {
                case "Imax":
                    no = "01";
                    break;
                case "Ib":
                case "1.0Ib":
                    no = "02";
                    break;
                case "0.1Ib":
                    no = "03";
                    break;
                case "0.2Ib":
                    no = "04";
                    break;
                case "0.05Ib":
                    no = "05";
                    break;
                case "0.01Ib":
                    no = "06";
                    break;
                case "0.02Ib":
                    no = "07";
                    break;
                case "1/2（Imax- Ib）":
                    no = "08";
                    break;
                case "0.5Ib":
                    no = "09";
                    break;
                case "1/2Imax":
                case "0.5Imax":
                    no = "10";
                    break;
                case "1.5Ib":
                    no = "11";
                    break;
                case "0.8Ib":
                    no = "12";
                    break;
                default:
                    no = "01";
                    break;
            }
            return no;
        }
    }
}
