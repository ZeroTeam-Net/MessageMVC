using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC.ApiContract;
using ZeroTeam.MessageMVC.Http;
using ZeroTeam.MessageMVC.Messages;

namespace ZeroTeam.MessageMVC.Sample.Controllers.UnitTest
{
    [TestFixture]
    public class HttpTest
    {
        IMessagePoster poster;
        [SetUp]
        public void SetUp()
        {
            if (poster != null)
                return;
            poster = new HttpPoster();
            poster.Initialize();
        }


        /// <summary>
        /// 测试接口
        /// </summary>
        [Test]
        public async Task PostTest()
        {
            IInlineMessage message = new InlineMessage
            {
                ID = Guid.NewGuid().ToString("N").ToUpper(),
                Topic = "Topic",
                Title = "Title",
                Content = @"{""Value"": ""Content""}"
            };

            var res = await poster.Post(message);
            
            Assert.IsTrue(message.State == MessageState.NoUs, res.Result);
        }
    }
}



