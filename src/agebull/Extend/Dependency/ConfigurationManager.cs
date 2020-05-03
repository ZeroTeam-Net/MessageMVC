using System;
using System.Collections.Generic;
using System.IO;
using Agebull.Common.Ioc;
using Agebull.EntityModel.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;

namespace Agebull.Common.Configuration
{
    /// <summary>
    ///   �����ļ��İ�����
    /// </summary>
    public class ConfigurationManager
    {
        #region Root

        /// <summary>
        /// ȫ������
        /// </summary>
        public static ConfigurationBuilder Builder { get; }

        /// <summary>
        /// ȫ������
        /// </summary>
        public static IConfigurationRoot Root { get; set; }

        /// <summary>
        /// ����Ŀ¼
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

        #region �����ļ����

        /// <summary>
        /// ָ�������Ƿ�����
        /// </summary>
        /// <param name="key">���õļ����ƣ�Ӧ��Լ��Ϊbool����</param>
        /// <param name="def">������ʱĬ��ֵ</param>
        public static bool IsEnable(string key, bool def = false)
        {
            var sec = Root.GetSection(key);
            return sec == null
                ? def
                : string.Equals(sec.Value, "true", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// ˢ��
        /// </summary>
        /// <param name="key">���õļ����ƣ�Ӧ��Լ��Ϊbool����</param>
        /// <param name="def">������ʱĬ��ֵ</param>
        public static bool IsDisable(string key, bool def = false)
        {
            var sec = Root.GetSection(key);
            return sec == null
                ? def
                : string.Equals(sec.Value, "false", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// ˢ��
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
        /// ��ʾʽ�������ö���(����)
        /// </summary>
        /// <param name="configuration"></param>
        public static void SetConfiguration(IConfiguration configuration)
        {
            Builder.AddConfiguration(configuration);
            Flush();
        }


        /// <summary>
        /// ���������ļ�
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
        /// ע����´�����
        /// </summary>
        /// <param name="section">�ڵ�</param>
        /// <param name="action">���´�����</param>
        /// <param name="runNow">�Ƿ�����ִ��һ��</param>
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
                    catch (Exception ex)//��ֹ�쳣����,�ж�Ӧ��
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
        /// <summary>
        /// ע����´�����
        /// </summary>
        /// <param name="section">�ڵ�</param>
        /// <param name="action">���´�����</param>
        /// <param name="runNow">�Ƿ�����ִ��һ��</param>
        public static void RegistOnChange<TConfig>(string section, Action<TConfig> action, bool runNow = true)
            where TConfig : class
        {
            var cfg = new NameValue<string, Action>
            {
                Name = section,
                Value = () =>
                {
                    try
                    {
                        var opt = Option<TConfig>(section);
                        if (opt != null)
                            action(opt);
                    }
                    catch (Exception ex)//��ֹ�쳣����,�ж�Ӧ��
                    {
                        Console.WriteLine(ex);
                    }
                }
            };
            DependencyHelper.ServiceCollection.AddOptions<TConfig>(section);
            actions.Add(cfg);
            ChangeToken.OnChange(() => Root.GetSection(cfg.Name).GetReloadToken(), cfg.Value);
            if (runNow)
                cfg.Value();
        }
        #endregion

        #region ʵ��
        /// <summary>
        /// ���ö���
        /// </summary>
        public IConfigurationSection Configuration { get; private set; }

        /// <summary>
        /// ��ʾʽ�������ö���(����)
        /// </summary>
        public ConfigurationManager Child(string section)
        {
            return new ConfigurationManager
            {
                Configuration = Configuration.GetSection(section)
            };
        }

        /// <summary>
        /// תΪǿ��������
        /// </summary>
        public TConfig ToConfig<TConfig>()
        {
            return Configuration.Get<TConfig>();
        }

        /// <summary>
        /// ��ʾʽ�������ö���(����)
        /// </summary>
        public TConfiguration Child<TConfiguration>(string section)
        {
            var sec = Configuration.GetSection(section);

            return sec.Exists() ? sec.Get<TConfiguration>() : default;
        }

        /// <summary>
        /// �Ƿ�Ϊ��
        /// </summary>
        public bool IsEmpty => Configuration == null || !Configuration.Exists();

        /// <summary>
        /// ȡ����
        /// </summary>
        /// <param name="key">��</param>
        /// <returns>��������</returns>
        public string this[string key]
        {
            get => key == null ? null : Configuration[key];
            set
            {
                if (key != null) Configuration[key] = value;
            }
        }

        /// <summary>
        ///   �õ��ı�ֵ
        /// </summary>
        /// <param name="key"> �� </param>
        /// <param name="def"> ȱʡֵ�������ڻ��д�� </param>
        /// <returns> �ı�ֵ </returns>
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
        ///   �õ�������ֵ
        /// </summary>
        /// <param name="key"> �� </param>
        /// <param name="def"> ȱʡֵ�������ڻ򲻺���ʱʹ�ã� </param>
        /// <returns> ������ֵ </returns>
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
        ///   �õ�������ֵ
        /// </summary>
        /// <param name="key"> �� </param>
        /// <param name="def"> ȱʡֵ�������ڻ򲻺���ʱʹ�ã� </param>
        /// <returns> ������ֵ </returns>
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
        ///   �õ�˫����ֵ
        /// </summary>
        /// <param name="key"> �� </param>
        /// <param name="def"> ȱʡֵ�������ڻ򲻺���ʱʹ�ã� </param>
        /// <returns> ˫����ֵ </returns>
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
        ///   �õ�˫����ֵ
        /// </summary>
        /// <param name="key"> �� </param>
        /// <param name="def"> ȱʡֵ�������ڻ򲻺���ʱʹ�ã� </param>
        /// <returns> ˫����ֵ </returns>
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
        ///   �õ�˫����ֵ
        /// </summary>
        /// <param name="key"> �� </param>
        /// <param name="def"> ȱʡֵ�������ڻ򲻺���ʱʹ�ã� </param>
        /// <returns> ˫����ֵ </returns>
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
        ///   �õ�˫����ֵ
        /// </summary>
        /// <param name="key"> �� </param>
        /// <returns> ˫����ֵ </returns>
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
        ///   �õ�˫����ֵ
        /// </summary>
        /// <param name="key"> �� </param>
        /// <returns> ˫����ֵ </returns>
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
        ///   ����һ����������
        /// </summary>
        /// <typeparam name="T"> </typeparam>
        /// <param name="key">����</param>
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

        #region Ԥ�������


        #region AppSetting

        /// <summary>
        /// ��ʾʽ�������ö���(����)
        /// </summary>
        static ConfigurationManager _appSettings;

        /// <summary>
        /// ��ʾʽ�������ö���(����)
        /// </summary>
        public static ConfigurationManager AppSettings => _appSettings ?? (_appSettings = new ConfigurationManager
        {
            Configuration = Root.GetSection("AppSettings")
        });

        #endregion

        #region ConnectionString

        /// <summary>
        /// ��ʾʽ�������ö���(����)
        /// </summary>
        static ConfigurationManager _connectionStrings;

        /// <summary>
        /// ��ʾʽ�������ö���(����)
        /// </summary>
        public static ConfigurationManager ConnectionStrings => _connectionStrings ?? (_connectionStrings = new ConfigurationManager
        {
            Configuration = Root.GetSection("ConnectionStrings")
        });


        /// <summary>
        /// ��ʾʽ�������ö���(����)
        /// </summary>
        public static ConfigurationManager Get(string section)
        {
            return new ConfigurationManager
            {
                Configuration = Root.GetSection(section)
            };
        }


        /// <summary>
        /// ǿ����ȡ���ڵ�
        /// </summary>
        public static TConfig Get<TConfig>(string section)
            where TConfig : class
        {
            return Root.GetSection(section)?.Get<TConfig>();
        }

        /// <summary>
        /// ǿ����ȡ���ڵ�
        /// </summary>
        public static TConfig Option<TConfig>(string section)
            where TConfig : class
        {
            return Root.GetSection(section)?.Get<TConfig>();
        }

        /// <summary>
        /// ǿ����ȡ���ڵ�
        /// </summary>
        public static TConfig Option<TConfig>(params string[] sections)
            where TConfig : class
        {
            return Root.GetSection(string.Join(':', sections))?.Get<TConfig>();
        }
        #endregion


        #endregion

        #region ���ݻ�ȡ


        /// <summary>
        ///   �õ��ı�ֵ
        /// </summary>
        /// <param name="key"> �� </param>
        /// <param name="def"> ȱʡֵ�������ڻ򲻺���ʱʹ�ã� </param>
        /// <returns> �ı�ֵ </returns>
        public static string GetAppSetting(string key, string def = null)
        {
            return AppSettings[key] ?? def;
        }

        /// <summary>
        ///   �õ�������ֵ
        /// </summary>
        /// <param name="key"> �� </param>
        /// <param name="def"> ȱʡֵ�������ڻ򲻺���ʱʹ�ã� </param>
        /// <returns> ������ֵ </returns>
        public static int GetAppSettingInt(string key, int def)
        {
            return AppSettings.GetInt(key, def);
        }

        /// <summary>
        ///   �õ�������ֵ
        /// </summary>
        /// <param name="key"> �� </param>
        /// <param name="def"> ȱʡֵ�������ڻ򲻺���ʱʹ�ã� </param>
        /// <returns> ������ֵ </returns>
        public static long GetAppSettingLong(string key, long def)
        {
            return AppSettings.GetLong(key, def);
        }

        /// <summary>
        ///   �õ�˫����ֵ
        /// </summary>
        /// <param name="key"> �� </param>
        /// <param name="def"> ȱʡֵ�������ڻ򲻺���ʱʹ�ã� </param>
        /// <returns> ˫����ֵ </returns>
        public static double GetAppSettingDouble(string key, double def = 0.0)
        {
            return AppSettings.GetDouble(key, def);
        }

        /// <summary>
        ///   �õ�˫����ֵ
        /// </summary>
        /// <param name="key"> �� </param>
        /// <param name="def"> ȱʡֵ�������ڻ򲻺���ʱʹ�ã� </param>
        /// <returns> ˫����ֵ </returns>
        public static decimal GetAppSettingDecimal(string key, decimal def = 0M)
        {
            return AppSettings.GetDecimal(key, def);
        }

        /// <summary>
        ///   ��ȡ���Ӵ��Ľڵ���Ϣ
        /// </summary>
        /// <param name="key">����</param>
        /// <param name="def">�Ҳ�����ȱʡֵ</param>
        /// <returns> </returns>
        public static string GetConnectionString(string key, string def = "")
        {
            return key == null ? def : ConnectionStrings[key];
        }

        #endregion
    }
}