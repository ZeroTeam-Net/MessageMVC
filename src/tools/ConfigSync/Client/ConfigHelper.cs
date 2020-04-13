using Agebull.Common.Configuration;
using CSRedis;
using Microsoft.Extensions.Configuration;
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
        /// 保存配置
        /// </summary>
        /// <param name="path"></param>
        /// <param name="option"></param>
        public static async Task SaveOption(string path, IConfigurationSection option)
        {
            var file = Path.Combine(path, $"{option.Path.Replace(':', '.')}.json");
            if (!File.Exists(file))
            {
                await File.WriteAllTextAsync(file, "{}", Encoding.UTF8);
            }
            ConfigurationManager.Builder.AddJsonFile(file, false, true);
        }

        /// <summary>
        /// 配置节点保存到文件
        /// </summary>
        /// <param name="section">配置节点名称</param>
        /// <param name="json">配置JSON内容</param>
        /// <returns></returns>
        public static async Task SaveToFile(string section, string json = null)
        {
            var sections = section.Split(':', '.');
            var file = Path.Combine(ZeroAppOption.Instance.ConfigFolder, "sync", $"{string.Join('.', sections)}.json");
            if (!File.Exists(file))//本地不需要
                return;
            if(json == null)
            {
                using var redis = new CSRedisClient(ConfigChangOption.Instance.ConnectionString);
                json = await redis.HGetAsync(ConfigChangOption.ConfigRedisKey, section);
            }
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
