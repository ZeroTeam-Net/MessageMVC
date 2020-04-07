using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace ZeroTeam.MessageMVC.ConfigSync
{
    /// <summary>
    /// 配置辅助类
    /// </summary>
    public static class ConfigHelper
    {
        /// <summary>
        /// 保存到文件
        /// </summary>
        /// <param name="section"></param>
        /// <returns></returns>
        public static async Task SaveToFile(string section, string json = null)
        {
            var sections = section.Split(':', '.');
            var file = Path.Combine(ZeroAppOption.Instance.ConfigFolder, "sync", $"{string.Join('.', sections)}.json");
            if (!File.Exists(file))//本地不需要
                return;
            json ??= await RedisHelper.HGetAsync(ConfigChangOption.ConfigRedisKey, section);
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < sections.Length; i++)
            {
                builder.AppendLine("{");
                builder.Append(' ', (i + 1) * 2);

                builder.Append($"\"{sections[i]}\" : ");
            }
            builder.AppendLine(json.SpaceLine(sections.Length * 2, false));
            for (int i = sections.Length - 1; i >= 0; i--)
            {
                builder.Append(' ', i * 2);
                builder.AppendLine("}");
            }
            //写入文件,更新留给Core自己处理
            File.WriteAllText(file, builder.ToString(), Encoding.UTF8);
        }
    }
}
