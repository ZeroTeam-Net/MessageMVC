using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ZeroTeam.MessageMVC.Documents;

namespace ZeroTeam.MessageMVC.ZeroApis
{
    /// <summary>
    /// API扩展功能
    /// </summary>
    public class ApiSqlCode
    {
        /// <summary>
        /// 站点文档信息
        /// </summary>
        public Dictionary<string, ServiceInfo> ServiceInfos { get; set; }

        /// <summary>
        /// 生成单元测试代码
        /// </summary>
        /// <param name="path"></param>
        public void ApiSql(string path)
        {
            foreach (var serviceInfo in ServiceInfos.Values)
            {
                if (serviceInfo.Aips.Count == 0)
                {
                    continue;
                }
                var file = Path.Combine(path, $"{serviceInfo.Name}.sql");
                var code = new StringBuilder();
                foreach (var api in serviceInfo.Aips.Values.Cast<ApiActionInfo>())
                {
                    code.AppendLine($@"
INSERT INTO `tb_app_api`(`app_id`,`service_name`,`model_name`,`model_caption`,`api_name`,`api_caption`,`url`,`option`,`memo`,`binding_pages`) 
VALUES ('请修改为正确的AppId', '{serviceInfo.Name}', '{api.ControllerName}','{api.ControllerCaption}', '{api.Name}', '{api.Caption}', '{serviceInfo.Name}/{api.Route}','{(int)api.AccessOption}', '{api.Description}', '{api.PageUrl}');
");
                }
                File.WriteAllText(file, code.ToString());
            }
        }
    }
}