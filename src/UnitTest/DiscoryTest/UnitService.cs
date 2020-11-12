using Agebull.Common.Ioc;
using NUnit.Framework;
using System;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC.Context;
using ZeroTeam.MessageMVC.Messages;
using ZeroTeam.MessageMVC.Tools;
using ZeroTeam.MessageMVC.ZeroApis;

namespace ZeroTeam.MessageMVC.Sample.Controllers.UnitTest
{
    [TestFixture]
    public class TestControlerUnitTest
    {
        [SetUp]
        public void Setup()
        {
            ZeroApp.UseTest(DependencyHelper.ServiceCollection, typeof(NetEventControler).Assembly).Wait();
        }

        [TearDown]
        public void TearDown()
        {
            ZeroFlowControl.Shutdown();
        }


        /// <summary>
        /// 测试接口
        /// </summary>
        [Test]
        public async Task FailedMessage()
        {
            try
            {
                var msg = await MessagePoster.Post(null);
                Assert.Fail("不应执行");
            }
            catch (Exception ex)
            {
                Assert.IsTrue(ex is NotSupportedException, ex.Message);
            }

            try
            {
                var msg = await MessagePoster.Post(new InlineMessage());
                Assert.Fail("不应执行");
            }
            catch (Exception ex)
            {
                Assert.IsTrue(ex is NotSupportedException, ex.Message);
            }

            try
            {

                var msg = await MessagePoster.Post(new InlineMessage
                {
                    ServiceName = "abbc"
                });
                Assert.Fail("不应执行");
            }
            catch (Exception ex)
            {
                Assert.IsTrue(ex is NotSupportedException, ex.Message);
            }
            try
            {
                var msg = await MessagePoster.Post(new InlineMessage
                {
                    ServiceName = "UnitService"
                });
                Assert.Fail("不应执行");
            }
            catch (Exception ex)
            {
                Assert.IsTrue(ex is NotSupportedException, ex.Message);
            }
        }




        /// <summary>
        /// 测试接口
        /// </summary>
        [Test]
        public async Task NonSupport()
        {
            var msg = await MessagePoster.Post(new InlineMessage
            {
                ServiceName = "UnitService",
                ApiName = "abcccceerw"
            });
            Assert.IsTrue(msg.State == MessageState.Unhandled, msg.Result);
        }




        /// <summary>
        /// 测试接口
        /// </summary>
        [Test]
        public async Task Argument()
        {
            var msg = await MessagePoster.Post(new InlineMessage
            {
                ServiceName = "UnitService",
                ApiName = "v1/argument",
                Content =
@"{
    ""Value"" : ""string""
}"
            });
            Assert.IsTrue(msg.State == MessageState.Success, msg.Result);
        }


        /// <summary>
        /// 测试接口
        /// </summary>
        [Test]
        public async Task CustomSerialize()
        {
            var msg = await MessagePoster.Post(new InlineMessage
            {
                ServiceName = "UnitService",
                ApiName = "v1/customSerialize",
                Content = "<xml><Value>val</Value></xml>"
            });
            msg.OfflineResult();
            Console.WriteLine(msg.Result);
            Assert.IsTrue(msg.State == MessageState.Success, msg.Result);
        }

        /// <summary>
        /// 测试接口
        /// </summary>
        [Test]
        public async Task Validate()
        {
            var msg = await MessagePoster.Post(new InlineMessage
            {
                ServiceName = "UnitService",
                ApiName = "v1/validate",
                Content =
@"{
    
}"
            });
            msg.OfflineResult();
            Console.WriteLine(msg.Result);

            Assert.IsTrue(msg.State == MessageState.FormalError, msg.Result);
        }


        /// <summary>
        /// 测试接口
        /// </summary>
        [Test]
        public async Task Validate2()
        {
            var msg = await MessagePoster.Post(new InlineMessage
            {
                ServiceName = "UnitService",
                ApiName = "v1/validate",
                Content = @"{""Value"": ""value""}"
            });
            msg.OfflineResult();
            Console.WriteLine(msg.Result);
            var res = msg.ResultData as IApiResult;
            Assert.IsTrue(res != null && res.Success, msg.Result);
        }



        /// <summary>
        /// 测试接口
        /// </summary>
        [Test]
        public async Task Error()
        {
            var msg = await MessagePoster.Post(new InlineMessage
            {
                ServiceName = "UnitService",
                ApiName = "v1/err"
            });
            msg.OfflineResult();
            var res = msg.ResultData as IApiResult;
            Assert.IsFalse(res.Success, msg.Result);
        }
        /// <summary>
        /// 
        /// </summary>
        [Test]
        public async Task Empty()
        {
            var msg = await MessagePoster.Post(new InlineMessage
            {
                ServiceName = "UnitService",
                ApiName = "v1/empty"
            });
            Assert.IsTrue(msg.State == MessageState.Success, msg.Result);
        }


        /// <summary>
        /// 
        /// </summary>
        [Test]
        public async Task Async()
        {
            var msg = await MessagePoster.Post(new InlineMessage
            {
                ServiceName = "UnitService",
                ApiName = "v1/async",
                Content =
@"{
    
}"
            });
            Assert.IsTrue(msg.State == MessageState.Success, msg.Result);
        }

        /// <summary>
        /// 
        /// </summary>
        [Test]
        public async Task Context()
        {
            var traceInfo = new TraceInfo
            {
                Start = new DateTime(2020, 3, 12),
                CallApp = "UnitTest"
            };
            var msg = await MessagePoster.Post(new InlineMessage
            {
                ServiceName = "UnitService",
                ApiName = "v1/context",
                Trace = traceInfo,
                Context = new System.Collections.Generic.Dictionary<string, string>
                {
                    {"User" , new UserInfo
                    {
                        UserId = "20200312",
                        NickName = "agebull",
                        OpenId = "20200312",
                        OrganizationId = "20200312",
                        OrganizationName = "ZeroTeam"
                    }.ToJson() }
                }
            });
            msg.OfflineResult();
            Console.WriteLine(msg.Result);
            var ctx = msg.ResultData;
            Assert.IsTrue(ctx != null, msg.Result);
            Assert.IsTrue(msg.Trace.CallApp == traceInfo.CallApp, msg.Trace.CallApp);
            Assert.IsTrue(msg.Trace.Start == traceInfo.Start, msg.Trace.Start?.ToString());
            //Assert.IsTrue(ctx.User.OrganizationId == ZeroTeamJwtClaim.UnknownOrganizationId, ctx.User.OrganizationId.ToString());
        }

        /// <summary>
        /// 
        /// </summary>
        [Test]
        public async Task Exception()
        {
            var msg = await MessagePoster.Post(new InlineMessage
            {
                ServiceName = "UnitService",
                ApiName = "v1/exception"
            });
            msg.OfflineResult();
            Assert.IsTrue(msg.State == MessageState.FrameworkError, msg.Result);
        }

        /// <summary>
        /// 
        /// </summary>
        [Test]
        public async Task VoidCall()
        {
            var msg = await MessagePoster.Post(new InlineMessage
            {
                ServiceName = "UnitService",
                ApiName = "v1/void",
                Content = @"{""argument"": ""value""}"
            });
            Assert.IsTrue(msg.Result == null, msg.Result);
        }


        /// <summary>
        /// 
        /// </summary>
        [Test]
        public async Task JsonString()
        {
            var msg = await MessagePoster.Post(new InlineMessage
            {
                ServiceName = "UnitService",
                ApiName = "v1/json",
                Content = @"{""Value"": ""value""}"
            });
            Assert.IsTrue(msg.Result[0] == '{', msg.Result);
        }


        /// <summary>
        /// 
        /// </summary>
        [Test]
        public async Task XmlString()
        {
            var msg = await MessagePoster.Post(new InlineMessage
            {
                ServiceName = "UnitService",
                ApiName = "v1/xml",
                Content = @"{""Value"": ""value""}"
            });
            Assert.IsTrue(msg.Result[0] == '<', msg.Result);
        }

        /// <summary>
        /// 
        /// </summary>
        [Test]
        public async Task TaskTest()
        {
            var msg = await MessagePoster.Post(new InlineMessage
            {
                ServiceName = "UnitService",
                ApiName = "v1/task",
                Content = "{}"
            });
            Assert.IsTrue(msg.State == MessageState.Success, msg.Result);
        }

        /// <summary>
        /// 
        /// </summary>
        [Test]
        public async Task FromServices()
        {
            var msg = await MessagePoster.Post(new InlineMessage
            {
                ServiceName = "UnitService",
                ApiName = "v1/FromServices",
                Content = "{}"
            });
            Assert.IsTrue(msg.State == MessageState.Success, msg.Result);
        }



        /// <summary>
        /// 
        /// </summary>
        [Test]
        public async Task FromConfig()
        {
            var msg = await MessagePoster.Post(new InlineMessage
            {
                ServiceName = "UnitService",
                ApiName = "v1/FromConfig",
                Content =
@"{
    ""AppName"" : ""string"",
    ""MaxWorkThreads"" : 0,
    ""MaxIOThreads"" : 0,
    ""IsolateFolder"" : true,
    ""DataFolder"" : ""string"",
    ""ConfigFolder"" : ""string"",
    ""AddInPath"" : ""string"",
    ""EnableAddIn"" : true
}"
            });
            Assert.IsTrue(msg.State == MessageState.Success, msg.Result);
        }



        /// <summary>
        /// 
        /// </summary>
        [Test]
        public async Task MulitArg()
        {
            var msg = await MessagePoster.Post(new InlineMessage
            {
                ServiceName = "UnitService",
                ApiName = "v1/mulitArg",
                Content =
@"{
    ""a"" : ""string"",
    ""b"" : 0,
    ""c"" : 0,
    ""d"" : 0
}"
            });
            Assert.IsTrue(msg.State == MessageState.Success, msg.Result);
        }

    }
}
