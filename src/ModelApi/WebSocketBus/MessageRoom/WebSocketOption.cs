using Agebull.Common.Configuration;
using System;
using System.Collections.Generic;

namespace BeetleX.Zeroteam.WebSocketBus
{
    public class WebSocketOption
    {
        /// <summary>
        /// 消息服务名称
        /// </summary>
        public string ServiceName { get; set; }

        /// <summary>
        /// Secret
        /// </summary>
        public string JwtAppSecret { get; set; }

        /// <summary>
        /// Secret
        /// </summary>
        public byte[] JwtAppSecretByte { get; set; }


        /// <summary>
        /// JWT颁发
        /// </summary>
        public string JwtIssue { get; set; }


        /// <summary>
        /// 一次备份延时秒数
        /// </summary>
        public int BackupDelay { get; set; }

        /// <summary>
        /// 备份最大延时秒数
        /// </summary>
        public int BackupMaxDelay { get; set; }

        public List<MessageRoomOption> Rooms { get; set; }

        /// <summary>
        /// 实例
        /// </summary>
        public static readonly WebSocketOption Instance = new WebSocketOption();


        public static void Load()
        {
            ConfigurationHelper.RegistOnChange<WebSocketOption>("WebSocket", Instance.Update, true);
        }

        /// <summary>
        /// 重新载入并更新
        /// </summary>
        private void Update(WebSocketOption option)
        {
            if (ServiceName == null)
            {
                ServiceName = option.ServiceName ?? "WebSocket";
                JwtIssue = option.JwtIssue;
                JwtAppSecretByte = option.JwtAppSecret?.ToUtf8Bytes();
            }
            if (option.Rooms != null)
            {
                foreach (var opt in option.Rooms)
                {
                    var old = MessageRoomManage.Instance[opt.Name];
                    if (old != null)
                        continue;
                    var room = new MessageRoom
                    {
                        Name = opt.Name,
                        BoundaryCode = opt.BoundaryCode,
                        Group = opt.Group == null || opt.Group.Length == 0 ? null : opt.Group
                    };
                    room.Reload();
                    MessageRoomManage.Instance.Rooms.TryAdd(opt.Name, room);
                }
            }
            BackupDelay = option.BackupDelay < 1 ? 1 : option.BackupDelay;
            BackupMaxDelay = option.BackupMaxDelay < 3 ? 3 : option.BackupMaxDelay;
        }
    }
}
