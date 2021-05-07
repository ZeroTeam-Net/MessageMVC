using Agebull.Common;
using Agebull.Common.Configuration;
using BeetleX.FastHttpApi;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC;
using ZeroTeam.MessageMVC.Context;
using ZeroTeam.MessageMVC.Messages;

namespace BeetleX.Zeroteam.WebSocketBus
{
    /// <summary>
    /// 消息组
    /// </summary>
    public class MessageRoom : MessageRoomOption
    {
        public List<WebUser> Users { get; private set; } = new List<WebUser>();


        public void Send(WebCommand cmd)
        {
            try
            {
                cmd.Room = Name;
                var json = cmd.ToJson();
                var frame = FastHttpApiLifeFlow.Instance.Server.CreateDataFrame(json);

                foreach (var item in GetOnlines())
                    item.Send(frame);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

        }

        public WebUser[] GetOnlines()
        {
            lock (Users)
                return Users.Where(p => !p.Session.IsDisposed).ToArray();
        }

        public void Join(WebUser user)
        {
            if (user == null)
                return;
            if (!user.Rooms.Contains(this))
            {
                lock (Users)
                    Users.Add(user);
                user.Rooms.Add(this);
            }
            //SendEnter(user);
            SendLastValues(user);
        }
        public void Left(WebUser user)
        {
            if (user == null)
                return;
            lock (Users)
                Users.Remove(user);
            user.Rooms.Remove(this);

            //SendExist(user);
        }
        #region 消息
        /// <summary>
        /// 消息
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public Message this[string type]
        {
            get
            {
                if (type == null || !messages.TryGetValue(type, out var msg))
                    return null;
                return msg;
            }
        }

        internal ConcurrentDictionary<string, Message> messages = new ConcurrentDictionary<string, Message>(StringComparer.OrdinalIgnoreCase);
        public class Message
        {
            public string Content { get; set; }


            public string Group { get; set; }


            public string Filter { get; set; }

        }
        public void SendExist(WebUser webUser)
        {
            var msg = $"{webUser.NickName} is left this room";
            foreach (var user in GetOnlines())
            {
                user.Send(FastHttpApiLifeFlow.Instance.Server.CreateDataFrame(new WebCommand { Room = Name, Type = WebCommand.Quit, Message = msg, User = webUser.NickName }));
            }
        }

        public void SendEnter(WebUser webUser)
        {
            var msg = $"{webUser.NickName} is join this room";
            foreach (var user in GetOnlines())
            {
                user.Send(FastHttpApiLifeFlow.Instance.Server.CreateDataFrame(new WebCommand { Room = Name, Type = WebCommand.Enter, Message = msg, User = webUser.NickName }));
            }
        }

        public void SendReal(Message msg)
        {
            foreach (var user in GetOnlines())
            {
                if (msg.Filter != null)
                {
                    var code = user[ZeroTeamJwtClaim.OrganizationCode];
                    if (code != null && (msg.Filter.Length < code.Length || !code.Equals(msg.Filter.Substring(0, code.Length), StringComparison.InvariantCultureIgnoreCase)))
                        continue;
                }
                user.Send(FastHttpApiLifeFlow.Instance.Server.CreateDataFrame(new WebCommand { Room = Name, Type = msg.Group, Message = msg.Content, User = user.NickName }));
            }
        }
        public void SendLastValues(WebUser user)
        {
            foreach (var msg in messages.Values)
            {
                if (msg.Filter != null)
                {
                    var code = user[ZeroTeamJwtClaim.OrganizationCode];
                    if (code != null && (msg.Filter.Length < code.Length || !code.Equals(msg.Filter.Substring(0, code.Length), StringComparison.InvariantCultureIgnoreCase)))
                        continue;
                }
                user.Send(FastHttpApiLifeFlow.Instance.Server.CreateDataFrame(new WebCommand { Room = Name, Type = msg.Group, Message = msg.Content, User = user.NickName }));
            }
        }


        internal void Push(IInlineMessage message)
        {
            var methods = string.Join('_', message.Method.Split('/').Skip(1));
            var msg = new Message
            {
                Group = methods,
                Content = message.Argument
            };
            if (Group != null || BoundaryCode != null)
            {
                try
                {
                    var dir = SmartSerializer.ToObject<Dictionary<string, string>>(message.Argument);

                    if (Group != null)
                    {
                        var builder = new StringBuilder();
                        bool first = true;
                        foreach (var g in Group)
                        {
                            if (first)
                                first = false;
                            else
                                builder.Append('#');
                            if (dir.TryGetValue(g, out var group))
                            {
                                builder.Append(group);
                            }
                        }
                        msg.Group = builder.ToString();
                    }
                    if (dir.TryGetValue(BoundaryCode, out var code))
                    {
                        msg.Filter = code;
                    }
                }
                catch
                {
                }
            }
            messages[msg.Group] = msg;
            SendReal(msg);
            last = DateTime.Now.ToTimestamp();
            _ = Backup();
        }
        #endregion

        #region 数据持久化

        int BackupI;
        int last = DateTime.Now.ToTimestamp();
        internal async Task Backup()
        {
            if (Interlocked.Increment(ref BackupI) > 1)
                return;
            await Task.Yield();
            int cnt = 0;
            var now = DateTime.Now.ToTimestamp();
            //延时写入，防止重复无用操作
            while (++cnt < WebSocketOption.Instance.BackupMaxDelay && now - last < WebSocketOption.Instance.BackupDelay)
            {
                await Task.Delay((now - last) * 1000);
                now = DateTime.Now.ToTimestamp();
            }
            var file = Path.Combine(IOHelper.CheckPath(ZeroAppOption.Instance.DataFolder, "messages"), $"{Name}.json");
            try
            {
                File.WriteAllText(file, messages.ToJson());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            Interlocked.Exchange(ref BackupI, 0);
        }

        internal void Reload()
        {
            messages ??= new ConcurrentDictionary<string, Message>(StringComparer.InvariantCultureIgnoreCase);
            var file = Path.Combine(ZeroAppOption.Instance.DataFolder, "messages", $"{Name}.json");
            try
            {
                if (!File.Exists(file))
                {
                    return;
                }
                var json = File.ReadAllText(file);
                if (string.IsNullOrWhiteSpace(json))
                {
                    return;
                }
                var dir = SmartSerializer.ToObject<Dictionary<string, Message>>(json);
                if (dir == null || dir.Count <= 0)
                    return;
                foreach (var kv in dir)
                {
                    messages.TryAdd(kv.Key, kv.Value);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
        #endregion
    }
}
