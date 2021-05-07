using BeetleX.FastHttpApi;
using System;
using System.Collections.Concurrent;

namespace BeetleX.Zeroteam.WebSocketBus
{
    public class MessageRoomManage
    {
        public static MessageRoomManage Instance = new MessageRoomManage();

        public ConcurrentDictionary<string, MessageRoom> Rooms = new ConcurrentDictionary<string, MessageRoom>(StringComparer.OrdinalIgnoreCase);

        public MessageRoom this[string group] => group == null || !Rooms.TryGetValue(group, out var vl) ? null : vl;

        public void Join(string room, IHttpContext context)
        {
            WebUser user = GetUser(context.Session);
            Rooms.TryGetValue(room, out MessageRoom result);
            result?.Join(user);
        }

        public void Left(string room, IHttpContext context)
        {
            WebUser user = GetUser(context.Session);
            Rooms.TryGetValue(room, out MessageRoom result);
            result?.Left(user);
        }

        public void Talk(string message, IHttpContext context)
        {
            //if (!string.IsNullOrEmpty(message))
            //{
            //    var user = GetUser(context.Session);
            //    WebCommand cmd = new WebCommand { Type = "talk", Message = message, User = user };
            //    user?.Room?.Send(cmd);
            //}
        }
        public bool Login(string token, IHttpContext context)
        {
            var user = JwtTokenResolver.TokenToUser(token);
            if (user == null)
            {
                return false;
            }
            user.Session = context.Session;
            user.Address = context.Request.RemoteIPAddress;

            SetUser(context.Session, user);
            return true;
        }

        public void Exit(ISession session)
        {
            GetUser(session)?.Exit();
        }

        internal WebUser GetUser(ISession session)
        {
            return session["__user"] as WebUser;
        }

        private void SetUser(ISession session, WebUser user)
        {
            session["__user"] = user;
        }
    }
}
