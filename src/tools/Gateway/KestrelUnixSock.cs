using Agebull.Common.Configuration;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace ZeroTeam.MessageMVC.Http
{
    /// <summary>
    /// Kestrelv启用UnixSocket的一个BUG(第二次打开时,文件无法使用)的预防(修改文件权限,以便Nginx可访问)
    /// </summary>
    public static class KestrelUnixSock
    {
        internal static void OnOption(KestrelServerOptions option)
        {
            try
            {
                File.Delete("/tmp/kestrel.sock");
                option.ListenUnixSocket("/tmp/kestrel.sock");
                Task.Factory.StartNew(CheckFile);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        static void CheckFile()
        {
            int cnt = 0;
            try
            {
                while (++cnt < 100)
                {
                    Task.Delay(100);
                    if (File.Exists("/tmp/kestrel.sock"))
                        break;
                }
                Process.Start("chmod", "go+w /tmp/kestrel.sock").WaitForExit();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }


        /*// <summary>
        /// 配置HTTP
        /// </summary>
        /// <param name="options"></param>
        public static void Options(KestrelServerOptions options)
        {
            options.AddServerHeader = true;
            //将此选项设置为 null 表示不应强制执行最低数据速率。
            options.Limits.MinResponseDataRate = null;

            var httpOptions = ConfigurationManager.Root.GetSection("http").Get<HttpOption[]>();
            foreach (var option in httpOptions)
            {
                if (option.IsHttps)
                {
                    var filename = option.CerFile[0] == '/'
                        ? option.CerFile
                        : Path.Combine(Environment.CurrentDirectory, option.CerFile);
                    var certificate = new X509Certificate2(filename, option.CerPwd);
                    options.Listen(IPAddress.Any, option.Port, listenOptions =>
                    {
                        listenOptions.UseHttps(certificate);
                        listenOptions.Protocols = HttpProtocols.Http1AndHttp2;
                    });
                }
                else
                {
                    options.Listen(IPAddress.Any, option.Port, listenOptions =>
                    {
                        listenOptions.Protocols = HttpProtocols.Http1AndHttp2;
                    });
                }
            }
        }*/
    }
}