using Agebull.Common.Ioc;
using NUnit.Framework;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC;
using ZeroTeam.MessageMVC.Messages;
using ZeroTeam.MessageMVC.ZeroApis;

namespace DiscoryTest
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
            ZeroApp.UseTest(IocHelper.ServiceCollection, this.GetType().Assembly);
        }

        [Test]
        public async Task MulitArg()
        {
            var (msg, ser) = await MessagePoster.Post(new InlineMessage
            {
                ServiceName = "unitTest",
                ApiName = "v1/mulitArg",
                IsInline = true,
                Extend = new System.Collections.Generic.Dictionary<string, object>
                {
                    {"a" ,"string"},
                    {"b" ,"100"},
                    {"c" ,"0.11"},
                    {"d" ,"None"},
                }
            });
            Assert.IsTrue(msg.State == MessageState.Success);
        }
    }
}