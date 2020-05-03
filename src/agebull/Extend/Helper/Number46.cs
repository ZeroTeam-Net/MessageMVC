// // /*****************************************************
// // (c)2016-2016 Copy right www.gboxt.com
// // 作者:
// // 工程:Agebull.DataModel
// // 建立:2016-06-07
// // 修改:2016-06-16
// // *****************************************************/

#region 引用

using System.Collections.Generic;
using System.Text;

#endregion

namespace Agebull.EntityModel.Common
{
    /// <summary>
    /// 46位数字
    /// </summary>
    public static class Number46
    {
        static List<char> codes = new List<char>
        {
            '0', '1', '2', '3', '4', '5', '6', '7', '8', '9',
            'a', 'b',  'd', 'e', 'f', 'g', 'h','m', 'n', 'q',
            'r', 't','A', 'B', 'C', 'D', 'E', 'F', 'G', 'H',
            'J','K', 'L', 'M', 'N', 'P', 'Q', 'R', 'S', 'T',
            'U','V', 'W', 'X', 'Y', 'Z'
        };

        /// <summary>
        /// 转为46进制
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        public static string To46(this int num)
        {
            var sb = new StringBuilder();
            Dec46(num, sb);
            return sb.ToString();
        }
        /// <summary>
        /// 取余
        /// </summary>
        /// <param name="num"></param>
        /// <param name="code"></param>
        static void Dec46(int num, StringBuilder code)
        {
            if (num < 46)
            {
                code.Insert(0, codes[num]);
                return;
            }
            code.Insert(0, codes[num % 46]);
            Dec46(num / 46, code);
        }

        /// <summary>
        /// 转为46进制
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public static int From46(this string code)
        {
            if (string.IsNullOrEmpty(code))
                return 0;
            int num = 0;
            int bs = 0;
            for (int i = code.Length - 1; i >= 0; i--)
            {
                int idx = codes.IndexOf(code[i]);
                if (bs == 0)
                {
                    num = idx;
                    bs = 46;
                }
                else
                {
                    num += idx * bs;
                    bs *= 46;
                }
            }
            return num;
        }
    }
}
