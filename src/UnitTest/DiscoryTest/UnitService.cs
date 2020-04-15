using System;
using System.Threading.Tasks;
using NUnit.Framework;
using Agebull.Common.Ioc;
using ZeroTeam.MessageMVC;
using ZeroTeam.MessageMVC.Messages;
using System.Diagnostics;
using ZeroTeam.MessageMVC.ZeroApis;
using ZeroTeam.MessageMVC.Context;

namespace ZeroTeam.MessageMVC.Sample.Controllers.UnitTest
{
    [TestFixture]
    public class TestControlerUnitTest
    {
        [SetUp]
        public void Setup()
        {
            ZeroApp.UseTest(DependencyHelper.ServiceCollection, typeof(TestControler).Assembly);
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
                var (msg, ser) = await MessagePoster.Post(null);
                Assert.Fail("不应执行");
            }
            catch (Exception ex)
            {
                Assert.IsTrue(ex is NotSupportedException, ex.Message);
            }

            try
            {
                var (msg, ser) = await MessagePoster.Post(new InlineMessage());
                Assert.Fail("不应执行");
            }
            catch (Exception ex)
            {
                Assert.IsTrue(ex is NotSupportedException, ex.Message);
            }

            try
            {

                var (msg, ser) = await MessagePoster.Post(new InlineMessage
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
                var (msg, ser) = await MessagePoster.Post(new InlineMessage
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

            var (msg, ser) = await MessagePoster.Post(new InlineMessage
            {
                ServiceName = "UnitService",
                ApiName = "abcccceerw"
            });
            Assert.IsTrue(msg.State == MessageState.NonSupport, msg.Result);
        }




        /// <summary>
        /// 测试接口
        /// </summary>
        [Test]
        public async Task Argument()
        {
            var (msg, ser) = await MessagePoster.Post(new InlineMessage
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
            var (msg, ser) = await MessagePoster.Post(new InlineMessage
            {
                ServiceName = "UnitService",
                ApiName = "v1/customSerialize",
                Content = "<xml><Value>val</Value></xml>"
            });
            msg.OfflineResult(ser);
            Console.WriteLine(msg.Result);
            Assert.IsTrue(msg.State == MessageState.Success, msg.Result);
        }

        /// <summary>
        /// 测试接口
        /// </summary>
        [Test]
        public async Task Validate()
        {
            var (msg, ser) = await MessagePoster.Post(new InlineMessage
            {
                ServiceName = "UnitService",
                ApiName = "v1/validate",
                Content =
@"{
    
}"
            });
            msg.OfflineResult(ser);
            Console.WriteLine(msg.Result);
            var res = msg.ResultData as IApiResult;
            Assert.IsTrue(res.Code == DefaultErrorCode.ArgumentError, msg.Result);
        }


        /// <summary>
        /// 测试接口
        /// </summary>
        [Test]
        public async Task Validate2()
        {
            var (msg, ser) = await MessagePoster.Post(new InlineMessage
            {
                ServiceName = "UnitService",
                ApiName = "v1/validate",
                Content = @"{""Value"": ""value""}"
            });
            msg.OfflineResult(ser);
            Console.WriteLine(msg.Result);
            var res = msg.ResultData as IApiResult;
            Assert.IsTrue(res.Success, msg.Result);
        }



        /// <summary>
        /// 测试接口
        /// </summary>
        [Test]
        public async Task Error()
        {
            var (msg, ser) = await MessagePoster.Post(new InlineMessage
            {
                ServiceName = "UnitService",
                ApiName = "v1/err"
            });
            msg.OfflineResult(ser);
            var res = msg.ResultData as IApiResult;
            Assert.IsFalse(res.Success, msg.Result);
        }
        /// <summary>
        /// 
        /// </summary>
        [Test]
        public async Task Empty()
        {
            var (msg, ser) = await MessagePoster.Post(new InlineMessage
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
            var (msg, ser) = await MessagePoster.Post(new InlineMessage
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
            traceInfo.Context.User.UserId = 20200312;
            traceInfo.Context.User.NickName = "agebull";
            traceInfo.Context.User.UserCode = "20200312";
            //traceInfo.Context.User.OrganizationId = 20200312;
            traceInfo.Context.User.OrganizationName = "ZeroTeam";
            var (msg, ser) = await MessagePoster.Post(new InlineMessage
            {
                ServiceName = "UnitService",
                ApiName = "v1/context",
                Trace = traceInfo
            }) ;
            msg.OfflineResult(ser);
            Console.WriteLine(msg.Result);
            var ctx = msg.ResultData as IZeroContext;
            Assert.IsTrue(ctx != null, msg.Result);
            Assert.IsTrue(msg.Trace.CallApp == traceInfo.CallApp, msg.Result);
            Assert.IsTrue(msg.Trace.Start == traceInfo.Start, msg.Result);
            Assert.IsTrue(ctx.User.OrganizationId ==UserInfo.UnknownOrganizationId, msg.Result);
        }
        
        /// <summary>
        /// 
        /// </summary>
        [Test]
        public async Task Exception()
        {
            var (msg, ser) = await MessagePoster.Post(new InlineMessage
            {
                ServiceName = "UnitService",
                ApiName = "v1/exception"
            });
            msg.OfflineResult(ser);
            var res = msg.ResultData as IApiResult;
            Assert.IsTrue(res.Code == DefaultErrorCode.BusinessException, msg.Result);
        }


        /// <summary>
        /// 
        /// </summary>
        [Test]
        public async Task JsonString()
        {
            var (msg, ser) = await MessagePoster.Post(new InlineMessage
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
            var (msg, ser) = await MessagePoster.Post(new InlineMessage
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
            var (msg, ser) = await MessagePoster.Post(new InlineMessage
            {
                ServiceName = "UnitService",
                ApiName = "v1/task",
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
        public async Task FromServices()
        {
            var (msg, ser) = await MessagePoster.Post(new InlineMessage
            {
                ServiceName = "UnitService",
                ApiName = "v1/FromServices",
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
        public async Task FromConfig()
        {
            var (msg, ser) = await MessagePoster.Post(new InlineMessage
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
            var (msg, ser) = await MessagePoster.Post(new InlineMessage
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
