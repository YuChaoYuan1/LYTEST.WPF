using System.Text.RegularExpressions;

namespace LYTest.Utility
{
    /// 字符串校验
    /// <summary>
    /// 字符串校验
    /// </summary>
    public class StringCheck
    {
        /// <summary>
        /// 提取冻结次数n，格式如：(上n次)XX、(上n结算日)XX
        /// </summary>
        /// <param name="input"></param>
        /// <param name="NumberString"></param>
        /// <returns></returns>
        public static bool ExtractNumbersForFreezeTimes(string input, out string NumberString)
        {
            NumberString = string.Empty;
            if (string.IsNullOrWhiteSpace(input))
            {
                return false;
            }

            Match match = new Regex(@"\(上(\d+)").Match(input);
            if (match.Success)
            {
                NumberString = match.Value.Replace("(上", "");
            }

            return match.Success;
        }

    }
}
