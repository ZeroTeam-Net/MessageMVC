using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using ZeroTeam.MessageMVC.ApiContract;
using ZeroTeam.MessageMVC.Messages;
using ZeroTeam.MessageMVC.ZeroApis;

namespace ZeroTeam.MessageMVC.Sample.Controllers.UnitTest
{
    [TestFixture]
    public class XmlTest
    {
        /// <summary>
        /// 测试接口
        /// </summary>
        [Test]
        public void XMLTest()
        {
            var res = new ApiResult<Dictionary<string, string>>
            {
                Success = true,
                ResultData = new Dictionary<string, string> { { "<Test1>", "<Test2>" } }
            };
            var xmls = new XmlSerializeProxy();
            var xml = xmls.ToString(res);
            Console.WriteLine(xml);
            var res2 = xmls.ToObject<ApiResult<Dictionary<string, string>>>(xml);
            Assert.IsTrue(res.ResultData.Values.First() == res2.ResultData.Values.First(), xml);
        }


        /// <summary>
        /// 测试接口
        /// </summary>
        [Test]
        public void CDataXml()
        {
            var res = new ApiResult<Dictionary<string, string>>
            {
                Success = true,
                ResultData = new Dictionary<string, string> { { "<Test1>", "<Test2>" } }
            };
            var xmls = new CDataXmlSerializeProxy();
            var xml = xmls.ToString(res);
            Console.WriteLine(xml);
            var res2 = xmls.ToObject<ApiResult<Dictionary<string, string>>>(xml);
            Assert.IsTrue(res.ResultData.Values.First() == res2.ResultData.Values.First(), xml);
        }
        /// <summary>
        /// 测试空文本还原为空对象
        /// </summary>
        [Test]
        public void EmptyObjectTest()
        {
            var xmls = new XmlSerializeProxy();
            var res2 = xmls.ToObject<ApiResult<Dictionary<string, string>>>("<xml/>");
            Assert.IsTrue(res2 != null, "应构造出对象");
        }

        /// <summary>
        /// 测试空文本还原为空对象
        /// </summary>
        [Test]
        public void EmptyXmlTest()
        {
            var xmls = new XmlSerializeProxy();
            var res2 = xmls.ToObject<ApiResult<Dictionary<string, string>>>(string.Empty);
            Assert.IsTrue(res2 == null, "不应构造出对象");
        }

        /// <summary>
        /// 测试空文本还原为空对象
        /// </summary>
        [Test]
        public void NullXmlTest()
        {
            var xmls = new XmlSerializeProxy();
            var res2 = xmls.ToObject<ApiResult<Dictionary<string, string>>>(null);
            Assert.IsTrue(res2 == null, "不应构造出对象");
        }
    }
}



