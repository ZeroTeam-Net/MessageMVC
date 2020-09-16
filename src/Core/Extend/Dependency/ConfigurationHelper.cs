using System;
using System.Collections.Generic;
using System.IO;
using Agebull.Common.Ioc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;

namespace Agebull.Common.Configuration
{
    /// <summary>
    ///   配置文件的帮助类
    /// </summary>
    public class ConfigurationHelper
    {
        #region Root

        /// <summary>
        /// 全局配置
        /// </summary>
        public static IConfigurationBuilder Builder { get; internal set; }

        /// <summary>
        /// 全局配置
        /// </summary>
        public static IConfigurationRoot Root { get; internal set; }

        /// <summary>
        /// 基本目录
        /// </summary>
        public static string BasePath { get; set; }


        static ConfigurationHelper()
        {
            BasePath = Environment.CurrentDirectory;
            Builder = DependencyHelper.GetService<IConfigurationBuilder>() ?? new ConfigurationBuilder();
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

        /// <summary>
        /// 修改根
        /// </summary>
        /// <param name="builder"></param>
        public static void ChangeBuilder(IConfigurationBuilder builder)
        {
            Builder = builder;
            Flush();
        }

        #endregion

        #region 配置文件组合

        /// <summary>
        /// 指定配置是否启用
        /// </summary>
        /// <param name="key">配置的键名称，应已约定为bool类型</param>
        /// <param name="def">不存在时默认值</param>
        public static bool IsEnable(string key, bool def = false)
        {
            var sec = Root.GetSection(key);
            return sec == null
                ? def
                : string.Equals(sec.Value, "true", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// 刷新
        /// </summary>
        /// <param name="key">配置的键名称，应已约定为bool类型</param>
        /// <param name="def">不存在时默认值</param>
        public static bool IsDisable(string key, bool def = false)
        {
            var sec = Root.GetSection(key);
            return sec == null
                ? def
                : string.Equals(sec.Value, "false", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// 刷新
        /// </summary>
        internal static void UpdateDependency()
        {
            var cb = DependencyHelper.GetService<IConfigurationBuilder>();
            if (cb == null)
            {
                DependencyHelper.AddSingleton(p => Builder);
            }
            else if (cb != Builder)
            {
                Builder = cb;
            }

            Root = DependencyHelper.GetService<IConfigurationRoot>();
            if (Root == null)
            {
                Root = Builder.Build();
                DependencyHelper.AddSingleton(p => Root);
            }
        }

        /// <summary>
        /// 刷新
        /// </summary>
        public static void Flush()
        {
            foreach (var cfg in actions)
            {
                cfg.Disposable?.Dispose();
                cfg.Disposable = ChangeToken.OnChange(() => Root.GetSection(cfg.Section).GetReloadToken(), cfg.Action);
                cfg.Action();
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
        internal class ChangeAction
        {
            internal bool IsLoading { get; set; }
            internal string Section { get; set; }
            internal Action Action { get; set; }

            internal IDisposable Disposable { get; set; }

            internal void SetAction(Action action)
            {
                Action = () =>
                {
                    if (IsLoading)
                        return;
                    IsLoading = true;
                    try
                    {
                        action();
                    }
                    catch (Exception ex)//防止异常出错,中断应用
                    {
                        Console.WriteLine(ex);
                    }
                    finally
                    {
                        IsLoading = false;
                    }
                };
            }
            internal void SetAction<TConfig>(Action<TConfig> action)
            where TConfig : class
            {
                Action = () =>
                {
                    if (IsLoading)
                        return;
                    IsLoading = true;
                    try
                    {
                        var opt = ConfigurationHelper.Option<TConfig>(Section);
                        if (opt != null)
                            action(opt);
                    }
                    catch (Exception ex)//防止异常出错,中断应用
                    {
                        Console.WriteLine(ex);
                    }
                    finally
                    {
                        IsLoading = false;
                    }
                };
            }
        }

        static readonly List<ChangeAction> actions = new List<ChangeAction>();
        /// <summary>
        /// 注册更新处理器
        /// </summary>
        /// <param name="section">节点</param>
        /// <param name="reload">更新处理方法</param>
        /// <param name="runNow">是否现在执行一次</param>
        public static void RegistOnChange(string section, Action reload, bool runNow = true)
        {
            var cfg = new ChangeAction
            {
                Section = section
            };
            cfg.SetAction(reload);
            actions.Add(cfg);
            cfg.Disposable = ChangeToken.OnChange(() => Root.GetSection(cfg.Section).GetReloadToken(), cfg.Action);
            if (runNow)
                cfg.Action();
        }

        /// <summary>
        /// 注册更新处理器
        /// </summary>
        /// <param name="section">节点</param>
        /// <param name="reload">更新处理方法</param>
        /// <param name="runNow">是否现在执行一次</param>
        public static void RegistOnChange<TConfig>(string section, Action<TConfig> reload, bool runNow = true)
            where TConfig : class
        {
            var cfg = new ChangeAction
            {
                Section = section
            };
            cfg.SetAction(reload);

            cfg.Disposable = ChangeToken.OnChange(() => Root.GetSection(cfg.Section).GetReloadToken(), cfg.Action);
            if (runNow)
                cfg.Action();
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
        public ConfigurationHelper Child(string section)
        {
            return new ConfigurationHelper
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
            return string.IsNullOrWhiteSpace(value) ? def : value;
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
        static ConfigurationHelper _appSettings;

        /// <summary>
        /// 显示式设置配置对象(依赖)
        /// </summary>
        public static ConfigurationHelper AppSettings => _appSettings ??= new ConfigurationHelper
        {
            Configuration = Root.GetSection("AppSettings")
        };

        #endregion

        #region ConnectionString

        /// <summary>
        /// 显示式设置配置对象(依赖)
        /// </summary>
        static ConfigurationHelper _connectionStrings;

        /// <summary>
        /// 显示式设置配置对象(依赖)
        /// </summary>
        public static ConfigurationHelper ConnectionStrings => _connectionStrings ??= new ConfigurationHelper
        {
            Configuration = Root.GetSection("ConnectionStrings")
        };


        /// <summary>
        /// 显示式设置配置对象(依赖)
        /// </summary>
        public static ConfigurationHelper Get(string section)
        {
            return new ConfigurationHelper
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
        ///   获取连接串的节点信息
        /// </summary>
        /// <param name="key">名称</param>
        /// <param name="def">找不到的缺省值</param>
        /// <returns> </returns>
        public static string GetConnectionString(string key, string def = "")
        {
            return key == null ? def : ConnectionStrings[key];
        }

        #endregion
    }
}