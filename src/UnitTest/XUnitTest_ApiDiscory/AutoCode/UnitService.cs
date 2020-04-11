using System;
using System.Threading.Tasks;
using Xunit;
using Agebull.Common.Ioc;
using ZeroTeam.MessageMVC;
using ZeroTeam.MessageMVC.Messages;

namespace ZeroTeam.MessageMVC.Sample.Controllers.UnitTest
{
    public class TestControlerUnitTest : IDisposable
    {
        public TestControlerUnitTest()
        {
            ZeroApp.UseTest(IocHelper.ServiceCollection, typeof(TestControler).Assembly);
        }
        void IDisposable.Dispose()
        {
            ZeroFlowControl.Shutdown();
        }




        /// <summary>
        /// 
        /// </summary>
        [Fact]
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
            Assert.True(msg.State == MessageState.Success , msg.Result);
        }



        /// <summary>
        /// 
        /// </summary>
        [Fact]
        public async Task Argument2()
        {
            var (msg, ser) = await MessagePoster.Post(new InlineMessage
            {
                ServiceName = "UnitService",
                ApiName = "v1/argument22",
                Content = 
@"{
    ""argument"" : {
        ""Value"" : ""string""
    },
    ""arg2"" : {
        ""Value"" : ""string""
    }
}"
            });
            Assert.True(msg.State == MessageState.Success , msg.Result);
        }



        /// <summary>
        /// 
        /// </summary>
        [Fact]
        public async Task Empty()
        {
            var (msg, ser) = await MessagePoster.Post(new InlineMessage
            {
                ServiceName = "UnitService",
                ApiName = "v1/empty",
                Content = 
@"{
    
}"
            });
            Assert.True(msg.State == MessageState.Success , msg.Result);
        }



        /// <summary>
        /// 
        /// </summary>
        [Fact]
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
            Assert.True(msg.State == MessageState.Success , msg.Result);
        }



        /// <summary>
        /// 
        /// </summary>
        [Fact]
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
            Assert.True(msg.State == MessageState.Success , msg.Result);
        }



        /// <summary>
        /// 
        /// </summary>
        [Fact]
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
            Assert.True(msg.State == MessageState.Success , msg.Result);
        }



        /// <summary>
        /// 
        /// </summary>
        [Fact]
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
            Assert.True(msg.State == MessageState.Success , msg.Result);
        }



        /// <summary>
        /// 
        /// </summary>
        [Fact]
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
            Assert.True(msg.State == MessageState.Success , msg.Result);
        }

    }
}
