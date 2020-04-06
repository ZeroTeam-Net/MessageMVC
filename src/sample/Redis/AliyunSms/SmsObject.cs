namespace ZeroTeam.MessageMVC.RedisMQ.Sample.Controler
{
    using System.Collections.Generic;

    public class SmsObject
    {
        /// <summary>
        /// 手机号
        /// </summary>
        public string Mobile { set; get; }

        /// <summary>
        /// 签名
        /// </summary>
        public string Signature { get; set; }

        /// <summary>
        /// 模板Key
        /// </summary>
        public string TempletKey { set; get; }

        /// <summary>
        /// 短信数据
        /// </summary>
        public AppErrorInfo Info { set; get; }

        /// <summary>
        /// 业务ID
        /// </summary>
        public string OutId { set; get; }
    }


    public class AppErrorInfo
    {
        /// <summary>
        /// 应用名称
        /// </summary>
        public string AppName { get; set; }

        /// <summary>
        /// 模块,可使用类名
        /// </summary>
        public string Module { get; set; }

        /// <summary>
        /// 错误等级
        /// </summary>
        public string Level { get; set; }

        /// <summary>
        /// 错误消息
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// 异常内容
        /// </summary>
        public string Exception { get; set; }
    }
}

