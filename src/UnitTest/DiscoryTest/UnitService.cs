using Agebull.Common.Ioc;
using NUnit.Framework;
using System;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC.Context;
using ZeroTeam.MessageMVC.Messages;
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
                    Service = "abbc"
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
                    Service = "UnitService"
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
                Service = "UnitService",
                Method = "abcccceerw"
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
                Service = "UnitService",
                Method = "v1/argument",
                Argument =
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
                Service = "UnitService",
                Method = "v1/customSerialize",
                Argument = "<xml><Value>val</Value></xml>"
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
                Service = "UnitService",
                Method = "v1/validate",
                Argument =
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
                Service = "UnitService",
                Method = "v1/validate",
                Argument = @"{""Value"": ""value""}"
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
                Service = "UnitService",
                Method = "v1/err"
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
                Service = "UnitService",
                Method = "v1/empty"
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
                Service = "UnitService",
                Method = "v1/async",
                Argument =
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
                Service = "UnitService",
                Method = "v1/context",
                TraceInfo = traceInfo,
                Context = new System.Collections.Generic.Dictionary<string, string>
                {
                    {"User" , new UserInfo
                    {
                        UserId = "20200312",
                        NickName = "agebull",
                        OrganizationId = "20200312"
                    }.ToJson() }
                }
            });
            msg.OfflineResult();
            Console.WriteLine(msg.Result);
            var ctx = msg.ResultData;
            Assert.IsTrue(ctx != null, msg.Result);
            Assert.IsTrue(msg.TraceInfo.CallApp == traceInfo.CallApp, msg.TraceInfo.CallApp);
            Assert.IsTrue(msg.TraceInfo.Start == traceInfo.Start, msg.TraceInfo.Start?.ToString());
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
                Service = "UnitService",
                Method = "v1/exception"
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
                Service = "UnitService",
                Method = "v1/void",
                Argument = @"{""argument"": ""value""}"
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
                Service = "UnitService",
                Method = "v1/json",
                Argument = @"{""Value"": ""value""}"
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
                Service = "UnitService",
                Method = "v1/xml",
                Argument = @"{""Value"": ""value""}"
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
                Service = "UnitService",
                Method = "v1/task",
                Argument = "{}"
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
                Service = "UnitService",
                Method = "v1/FromServices",
                Argument = "{}"
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
                Service = "UnitService",
                Method = "v1/FromConfig",
                Argument =
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
                Service = "UnitService",
                Method = "v1/mulitArg",
                Argument =
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
