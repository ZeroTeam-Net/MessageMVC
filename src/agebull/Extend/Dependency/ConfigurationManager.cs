using System;
using System.Collections.Generic;
using System.IO;
using Agebull.EntityModel.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;

namespace Agebull.Common.Configuration
{
    /// <summary>
    ///   配置文件的帮助类
    /// </summary>
    public class ConfigurationManager
    {
        #region Root

        /// <summary>
        /// 全局配置
        /// </summary>
        public static ConfigurationBuilder Builder { get; }

        /// <summary>
        /// 全局配置
        /// </summary>
        public static IConfigurationRoot Root { get; set; }

        /// <summary>
        /// 基本目录
        /// </summary>
        public static string BasePath { get; set; }


        static ConfigurationManager()
        {
            Builder = new ConfigurationBuilder();
            Builder.SetBasePath(Environment.CurrentDirectory);

            var files = new string[] { "appsettings.json", "appSettings.json", "AppSettings.json", "Appsettings.json" };
            foreach (var fileName in files)
            {
                var file = Path.Combine(Environment.CurrentDirectory, fileName);
                if (File.Exists(file))
                {
                    Builder.AddJsonFile(file, false, true);
                    break;
                }
            }
            Root = Builder.Build();
        }

        #endregion

        #region 配置文件组合

        /// <summary>
        /// 刷新
        /// </summary>
        public static void Flush()
        {
            Root = Builder.Build();
            foreach (var cfg in actions)
            {
                ChangeToken.OnChange(() => Root.GetSection(cfg.Name).GetReloadToken(), cfg.Value);
                cfg.Value();
            }
            _appSettings = null;
            _connectionStrings = null;

        }

        /// <summary>
        /// 显示式设置配置对象(依赖)
        /// </summary>
        /// <param name="configuration"></param>
        public static void SetConfiguration(IConfiguration configuration)
        {
            Builder.AddConfiguration(configuration);
            Flush();
        }


        /// <summary>
        /// 载入配置文件
        /// </summary>
        public static void Load(string jsonFile, bool isNewtonsoft = false)
        {
            if (isNewtonsoft)
                Builder.AddNewtonsoftJsonFile(jsonFile, true, true);
            else
                Builder.AddJsonFile(jsonFile, true, true);
            Flush();
        }
        static readonly List<NameValue<string, Action>> actions = new List<NameValue<string, Action>>();
        /// <summary>
        /// 注册更新处理器
        /// </summary>
        /// <param name="section">节点</param>
        /// <param name="action">更新处理方法</param>
        /// <param name="runNow">是否现在执行一次</param>
        public static void RegistOnChange(string section, Action action, bool runNow = true)
        {
            var cfg = new NameValue<string, Action>
            {
                Name = section,
                Value = () =>
                {
                    try
                    {
                        action();
                    }
                    catch (Exception ex)//防止异常出错,中断应用
                    {
                        Console.WriteLine(ex);
                    }
                }
            };
            actions.Add(cfg);
            ChangeToken.OnChange(() => Root.GetSection(cfg.Name).GetReloadToken(), cfg.Value);
            if (runNow)
                cfg.Value();
        }

        #endregion

        #region 实例
        /// <summary>
        /// 配置对象
        /// </summary>
        public IConfigurationSection Configuration { get; private set; }

        /// <summary>
        /// 显示式设置配置对象(依赖)
        /// </summary>
        public ConfigurationManager Child(string section)
        {
            return new ConfigurationManager
            {
                Configuration = Configuration.GetSection(section)
            };
        }

        /// <summary>
        /// 转为强类型配置
        /// </summary>
        public TConfig ToConfig<TConfig>()
        {
            return Configuration.Get<TConfig>();
        }

        /// <summary>
        /// 显示式设置配置对象(依赖)
        /// </summary>
        public TConfiguration Child<TConfiguration>(string section)
        {
            var sec = Configuration.GetSection(section);

            return sec.Exists() ? sec.Get<TConfiguration>() : default;
        }

        /// <summary>
        /// 是否为空
        /// </summary>
        public bool IsEmpty => Configuration == null || !Configuration.Exists();

        /// <summary>
        /// 取配置
        /// </summary>
        /// <param name="key">键</param>
        /// <returns>配置内容</returns>
        public string this[string key]
        {
            get => key == null ? null : Configuration[key];
            set
            {
                if (key != null) Configuration[key] = value;
            }
        }

        /// <summary>
        ///   得到文本值
        /// </summary>
        /// <param name="key"> 键 </param>
        /// <param name="def"> 缺省值（不存在会回写） </param>
        /// <returns> 文本值 </returns>
        public string GetStr(string key, string def = null)
        {
            if (key == null)
            {
                return def;
            }
            var value = this[key];
            if (string.IsNullOrWhiteSpace(value))
            {
                return this[key] = def;
            }
            return value;
        }

        /// <summary>
        ///   得到长整数值
        /// </summary>
        /// <param name="key"> 键 </param>
        /// <param name="def"> 缺省值（不存在或不合理时使用） </param>
        /// <returns> 长整数值 </returns>
        public int GetInt(string key, int def)
        {
            if (key == null)
            {
                return def;
            }
            var value = this[key];
            if (string.IsNullOrWhiteSpace(value))
            {
                return def;
            }
            return !int.TryParse(value, out var vl) ? def : vl;
        }

        /// <summary>
        ///   得到长整数值
        /// </summary>
        /// <param name="key"> 键 </param>
        /// <param name="def"> 缺省值（不存在或不合理时使用） </param>
        /// <returns> 长整数值 </returns>
        public long GetLong(string key, long def)
        {
            if (key == null)
            {
                return def;
            }
            var value = this[key];
            if (string.IsNullOrWhiteSpace(value))
            {
                return def;
            }
            return !long.TryParse(value, out var vl) ? def : vl;
        }

        /// <summary>
        ///   得到双精数值
        /// </summary>
        /// <param name="key"> 键 </param>
        /// <param name="def"> 缺省值（不存在或不合理时使用） </param>
        /// <returns> 双精数值 </returns>
        public bool GetBool(string key, bool def = false)
        {
            if (key == null)
            {
                return def;
            }

            var value = this[key];
            if (string.IsNullOrWhiteSpace(value))
            {
                return def;
            }
            return !bool.TryParse(value, out var vl) ? def : vl;
        }

        /// <summary>
        ///   得到双精数值
        /// </summary>
        /// <param name="key"> 键 </param>
        /// <param name="def"> 缺省值（不存在或不合理时使用） </param>
        /// <returns> 双精数值 </returns>
        public double GetDouble(string key, double def = 0.0)
        {
            if (key == null)
            {
                return def;
            }

            var value = this[key];
            if (string.IsNullOrWhiteSpace(value))
            {
                return def;
            }
            return !double.TryParse(value, out var vl) ? def : vl;
        }

        /// <summary>
        ///   得到双精数值
        /// </summary>
        /// <param name="key"> 键 </param>
        /// <param name="def"> 缺省值（不存在或不合理时使用） </param>
        /// <returns> 双精数值 </returns>
        public decimal GetDecimal(string key, decimal def = 0M)
        {
            if (key == null)
            {
                return def;
            }

            var value = this[key];
            if (string.IsNullOrWhiteSpace(value))
            {
                return def;
            }
            return !decimal.TryParse(value, out var vl) ? def : vl;
        }

        /// <summary>
        ///   得到双精数值
        /// </summary>
        /// <param name="key"> 键 </param>
        /// <returns> 双精数值 </returns>
        public DateTime? GetDateTime(string key)
        {
            if (key == null)
            {
                return null;
            }

            var value = this[key];
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            if (DateTime.TryParse(value, out var vl)) return vl;
            return null;
        }

        /// <summary>
        ///   得到双精数值
        /// </summary>
        /// <param name="key"> 键 </param>
        /// <returns> 双精数值 </returns>
        public Guid? GetGuid(string key)
        {
            if (key == null)
            {
                return null;
            }

            var value = this[key];
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            if (Guid.TryParse(value, out var vl)) return vl;
            return null;
        }

        /// <summary>
        ///   配置一个配置内容
        /// </summary>
        /// <typeparam name="T"> </typeparam>
        /// <param name="key">名称</param>
        /// <param name="value"> </param>
        public void SetValue<T>(string key, T value) where T : struct
        {
            if (key == null)
            {
                return;
            }

            this[key] = value.ToString();
        }
        #endregion

        #region 预定义对象


        #region AppSetting

        /// <summary>
        /// 显示式设置配置对象(依赖)
        /// </summary>
        static ConfigurationManager _appSettings;

        /// <summary>
        /// 显示式设置配置对象(依赖)
        /// </summary>
        public static ConfigurationManager AppSettings => _appSettings ?? (_appSettings = new ConfigurationManager
        {
            Configuration = Root.GetSection("AppSettings")
        });

        #endregion

        #region ConnectionString

        /// <summary>
        /// 显示式设置配置对象(依赖)
        /// </summary>
        static ConfigurationManager _connectionStrings;

        /// <summary>
        /// 显示式设置配置对象(依赖)
        /// </summary>
        public static ConfigurationManager ConnectionStrings => _connectionStrings ?? (_connectionStrings = new ConfigurationManager
        {
            Configuration = Root.GetSection("ConnectionStrings")
        });


        /// <summary>
        /// 显示式设置配置对象(依赖)
        /// </summary>
        public static ConfigurationManager Get(string section)
        {
            return new ConfigurationManager
            {
                Configuration = Root.GetSection(section)
            };
        }


        /// <summary>
        /// 强类型取根节点
        /// </summary>
        public static TConfig Get<TConfig>(string section)
            where TConfig : class
        {
            return Root.GetSection(section)?.Get<TConfig>();
        }

        /// <summary>
        /// 强类型取根节点
        /// </summary>
        public static TConfig Option<TConfig>(string section)
            where TConfig : class
        {
            return Root.GetSection(section)?.Get<TConfig>();
        }

        /// <summary>
        /// 强类型取根节点
        /// </summary>
        public static TConfig Option<TConfig>(params string[] sections)
            where TConfig : class
        {
            return Root.GetSection(string.Join(':', sections))?.Get<TConfig>();
        }
        #endregion


        #endregion

        #region 内容获取


        /// <summary>
        ///   得到文本值
        /// </summary>
        /// <param name="key"> 键 </param>
        /// <param name="def"> 缺省值（不存在或不合理时使用） </param>
        /// <returns> 文本值 </returns>
        public static string GetAppSetting(string key, string def = null)
        {
            return AppSettings[key] ?? def;
        }

        /// <summary>
        ///   得到长整数值
        /// </summary>
        /// <param name="key"> 键 </param>
        /// <param name="def"> 缺省值（不存在或不合理时使用） </param>
        /// <returns> 长整数值 </returns>
        public static int GetAppSettingInt(string key, int def)
        {
            return AppSettings.GetInt(key, def);
        }

        /// <summary>
        ///   得到长整数值
        /// </summary>
        /// <param name="key"> 键 </param>
        /// <param name="def"> 缺省值（不存在或不合理时使用） </param>
        /// <returns> 长整数值 </returns>
        public static long GetAppSettingLong(string key, long def)
        {
            return AppSettings.GetLong(key, def);
        }

        /// <summary>
        ///   得到双精数值
        /// </summary>
        /// <param name="key"> 键 </param>
        /// <param name="def"> 缺省值（不存在或不合理时使用） </param>
        /// <returns> 双精数值 </returns>
        public static double GetAppSettingDouble(string key, double def = 0.0)
        {
            return AppSettings.GetDouble(key, def);
        }

        /// <summary>
        ///   得到双精数值
        /// </summary>
        /// <param name="key"> 键 </param>
        /// <param name="def"> 缺省值（不存在或不合理时使用） </param>
        /// <returns> 双精数值 </returns>
        public static decimal GetAppSettingDecimal(string key, decimal def = 0M)
        {
            return AppSettings.GetDecimal(key, def);
        }

        /// <summary>
        ///   配置一个配置内容
        /// </summary>
        /// <typeparam name="T"> </typeparam>
        /// <param name="key">名称</param>
        /// <param name="value"> </param>
        public static void SetAppSetting<T>(string key, T value) where T : struct
        {
            if (key == null)
            {
                return;
            }

            AppSettings[key] = value.ToString();
        }

        /// <summary>
        ///   获取连接串的节点信息
        /// </summary>
        /// <param name="key">名称</param>
        /// <param name="def">找不到的缺省值</param>
        /// <returns> </returns>
        public static string GetConnectionString(string key, string def = "")
        {
            return key == null ? def : ConnectionStrings[key];
        }

        /// <summary>
        ///   设置连接串的节点信息
        /// </summary>
        /// <param name="key">名称</param>
        /// <param name="connectionString">连接字符</param>
        public static void SetConnectionString(string key, string connectionString)
        {
            if (key == null)
            {
                return;
            }
            ConnectionStrings[key] = connectionString;
        }

        #endregion
    }
}