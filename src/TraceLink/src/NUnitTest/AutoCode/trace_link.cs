using Agebull.Common.Ioc;
using NUnit.Framework;
using System;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC.Messages;
using ZeroTeam.MessageMVC.MessageTraceLink.DataAccess;
using ZeroTeam.MessageMVC.ZeroApis;

namespace ZeroTeam.MessageMVC.MessageTraceLink.WebApi.Entity.UnitTest
{
    [TestFixture]
    public class MessageApiControllerUnitTest
    {
        [SetUp]
        public void Setup()
        {
            DependencyHelper.AddScoped<TraceLinkDatabase>();
            ZeroApp.UseTest(DependencyHelper.ServiceCollection, typeof(MessageApiController).Assembly);
        }

        [TearDown]
        public void TearDown()
        {
            ZeroFlowControl.Shutdown();
        }



        /// <summary>
        /// 
        /// </summary>
        [Test]
        public async Task message_v1_edit_flow()
        {
            var (msg, _) = await MessagePoster.Post(new InlineMessage
            {
                ServiceName = "trace_link",
                ApiName = "message/v1/flow",
                Content =
$@"{{
    ""traceId"" : ""{traceId}""
}}"
            });
            var res = msg.ResultData as IApiResult<string>;
            Assert.IsTrue(msg.State == MessageState.Success, res.ResultData);
        }



        /// <summary>
        /// 
        /// </summary>
        [Test]
        public async Task message_v1_edit_list()
        {
            var (msg, _) = await MessagePoster.Post(new InlineMessage
            {
                ServiceName = "trace_link",
                ApiName = "message/v1/edit/list",
                Content =
$@"{{
    ""page"" : 0,
    ""rows"" : 0
}}"
            });
            Assert.IsTrue(msg.State == MessageState.Success, msg.Result);
        }

        private string traceId;

        /// <summary>
        /// 
        /// </summary>
        [Test]
        public async Task message_v1_edit_first()
        {
            traceId = Guid.NewGuid().ToString("N");
            var (msg, _) = await MessagePoster.Post(new InlineMessage
            {
                ServiceName = "trace_link",
                ApiName = "message/v1/edit/first",
                Content =
$@"{{
    ""Id"" : {id},
    ""TraceId"" : ""{traceId}"",
    ""Start"" : ""2020-04-01"",
    ""End"" : ""2020-04-01"",
    ""LocalId"" : ""测试文本"",
    ""LocalApp"" : ""测试文本"",
    ""LocalMachine"" : ""测试文本"",
    ""CallId"" : ""测试文本"",
    ""CallApp"" : ""测试文本"",
    ""CallMachine"" : ""测试文本"",
    ""Context"" : ""测试文本"",
    ""Token"" : ""测试文本"",
    ""Headers"" : ""测试文本"",
    ""Message"" : ""测试文本""
}}"
            });
            Assert.IsTrue(msg.State == MessageState.Success, msg.Result);
        }



        /// <summary>
        /// 
        /// </summary>
        [Test]
        public async Task message_v1_edit_details()
        {
            var (msg, _) = await MessagePoster.Post(new InlineMessage
            {
                ServiceName = "trace_link",
                ApiName = "message/v1/edit/details",
                Content =
$@"{{
    ""id"" : ""{id}""
}}"
            });
            Assert.IsTrue(msg.State == MessageState.Success, msg.Result);
        }

        private static long id;

        /// <summary>
        /// 
        /// </summary>
        [Test]
        public async Task message_v1_edit_addnew()
        {
            var (msg, _) = await MessagePoster.Post(new InlineMessage
            {
                ServiceName = "trace_link",
                ApiName = "message/v1/edit/addnew",
                Content =
$@"{{
    ""TraceId"" : ""{traceId}"",
    ""Start"" : ""2020-04-01"",
    ""End"" : ""2020-04-01"",
    ""LocalId"" : ""测试文本"",
    ""LocalApp"" : ""测试文本"",
    ""LocalMachine"" : ""测试文本"",
    ""CallId"" : ""测试文本"",
    ""CallApp"" : ""测试文本"",
    ""CallMachine"" : ""测试文本"",
    ""Context"" : ""测试文本"",
    ""Token"" : ""测试文本"",
    ""Headers"" : ""测试文本"",
    ""Message"" : ""测试文本""
}}"
            });
            var data = msg.ResultData as IApiResult<MessageData>;
            id = data.ResultData.Id;
            Assert.IsTrue(msg.State == MessageState.Success, msg.Result);
        }



        /// <summary>
        /// 
        /// </summary>
        [Test]
        public async Task message_v1_edit_update()
        {
            var (msg, _) = await MessagePoster.Post(new InlineMessage
            {
                ServiceName = "trace_link",
                ApiName = "message/v1/edit/update",
                Content =
$@"{{
    ""Id"" : {id},
    ""TraceId"" : ""{traceId}"",
    ""Start"" : ""2020-04-01"",
    ""End"" : ""2020-04-01"",
    ""LocalId"" : ""1"",
    ""LocalApp"" : ""测试文本"",
    ""LocalMachine"" : ""1"",
    ""CallId"" : ""测试文本"",
    ""CallApp"" : ""测试文本"",
    ""CallMachine"" : ""测试文本"",
    ""Context"" : ""测试文本"",
    ""Token"" : ""测试文本"",
    ""Headers"" : ""测试文本"",
    ""Message"" : ""测试文本""
}}"
            });
            Assert.IsTrue(msg.State == MessageState.Success, msg.Result);
        }



        /// <summary>
        /// 
        /// </summary>
        [Test]
        public async Task message_v1_xedit_delete()
        {
            var (msg, _) = await MessagePoster.Post(new InlineMessage
            {
                ServiceName = "trace_link",
                ApiName = "message/v1/edit/delete",
                Content =
$@"{{
    ""selects"" : ""{id}""
}}"
            });
            Assert.IsTrue(msg.State == MessageState.Success, msg.Result);
        }

    }
}
