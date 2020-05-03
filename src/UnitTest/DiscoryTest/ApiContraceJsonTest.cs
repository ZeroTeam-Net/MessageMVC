using Agebull.Common.Ioc;
using NUnit.Framework;
using ZeroTeam.MessageMVC.ApiContract;
using ZeroTeam.MessageMVC.Messages;

namespace ZeroTeam.MessageMVC.Sample.Controllers.UnitTest
{
    [TestFixture]
    public class ApiContraceJsonTest
    {
        [SetUp]
        public void SetUp()
        {
            DependencyHelper.AddTransient<IJsonSerializeProxy, JsonSerializeProxy>();
        }

        /// <summary>
        /// 测试接口
        /// </summary>
        [Test]
        public void OperatorStatusJson()
        {
            var status = new OperatorStatus
            {
                Success = true,
                Code = 1,
                Message = "Message",
                InnerMessage = "InnerMessage",
                Exception = new System.Exception("Exception")
            };
            var microsoft = new JsonSerializeProxy();
            var newtonsoft = new NewtonJsonSerializeProxy();
            var json1 = newtonsoft.ToString(status, true);
            var json2 = microsoft.ToString(status, true);
            Assert.IsTrue(json2 == json1, json2);
            var status1 = newtonsoft.ToObject<OperatorStatus>(json1);
            var status2 = microsoft.ToObject<OperatorStatus>(json2);

            Assert.IsTrue(status1.Message == status2.Message, json2);
            Assert.IsTrue(status1.Exception == null, json2);

            var xml = new XmlSerializeProxy();
            var status3 = xml.ToObject<OperatorStatus>(xml.ToString(status));
            Assert.IsTrue(status3.Message == status.Message, json2);
            Assert.IsTrue(status3.Exception == null, json2);

            var cdata = new CDataXmlSerializeProxy();
            var status4 = cdata.ToObject<OperatorStatus>(cdata.ToString(status));
            Assert.IsTrue(status4.Message == status.Message, json2);
            Assert.IsTrue(status4.Exception == null, json2);

        }

        /// <summary>
        /// 测试接口
        /// </summary>
        [Test]
        public void ArgumentJson()
        {
            var status = new Argument
            {
                Value= "Value"
            };
            var microsoft = new JsonSerializeProxy();
            var newtonsoft = new NewtonJsonSerializeProxy();
            var json1 = newtonsoft.ToString(status, true);
            var status1 = newtonsoft.ToObject<Argument>(json1);
            Assert.IsTrue(status1.Value == status.Value, json1);
            
            var json2 = microsoft.ToString(status, true);
            var status2 = microsoft.ToObject<Argument>(json2);
            Assert.IsTrue(status2.Value == status.Value, json2);

            Assert.IsTrue(json2 == json1, json2);

            var xml = new XmlSerializeProxy();
            var xml1 = xml.ToString(status);
            var status3 = xml.ToObject<Argument>(xml1);
            Assert.IsTrue(status3.Value == status.Value, xml1);

            var cdata = new CDataXmlSerializeProxy();
            var xml2 = cdata.ToString(status);
            var status4 = cdata.ToObject<Argument>(xml2);
            Assert.IsTrue(status4.Value == status.Value, xml2);
        }

        /// <summary>
        /// 测试接口
        /// </summary>
        [Test]
        public void ApiResultJson()
        {
            var status = new ApiResult<string>
            {
                ResultData = "data",
                Success = true,
                Code = 1,
                Message = "Message",
                InnerMessage = "InnerMessage",
                Exception = new System.Exception("Exception"),
                Trace = new OperatorTrace
                {
                    RequestId = "RequestId",
                    Point = "Point",
                    Guide = "Guide",
                }
            };
            var microsoft = new JsonSerializeProxy();
            var newtonsoft = new NewtonJsonSerializeProxy();
            var json1 = newtonsoft.ToString(status, true);
            var json2 = microsoft.ToString(status, true);
            Assert.IsTrue(json2 == json1, json2);
            var status1 = newtonsoft.ToObject<ApiResult<string>>(json1);
            var status2 = microsoft.ToObject<ApiResult<string>>(json2);

            Assert.IsTrue(status1.Message == status2.Message, json2);
            Assert.IsTrue(status1.Exception == null, json2);

            var xml = new XmlSerializeProxy();
            var status3 = xml.ToObject<ApiResult<string>>(xml.ToString(status));
            Assert.IsTrue(status3.Message == status.Message, json2);
            Assert.IsTrue(status3.ResultData == status.ResultData, json2);

            var cdata = new CDataXmlSerializeProxy();
            var status4 = cdata.ToObject<ApiResult<string>>(cdata.ToString(status));
            Assert.IsTrue(status4.Message == status.Message, json2);
            Assert.IsTrue(status4.Trace.Point == status.Trace.Point, json2);


        }
        /// <summary>
        /// 测试接口
        /// </summary>
        [Test]
        public void OperatorTraceJson()
        {
            var status = new OperatorTrace
            {
                RequestId = "RequestId",
                Point = "Point",
                Guide = "Guide",
            };
            var microsoft = new JsonSerializeProxy();
            var newtonsoft = new NewtonJsonSerializeProxy();
            var json1 = newtonsoft.ToString(status, true);
            var json2 = microsoft.ToString(status, true);

            Assert.IsTrue(json2 == json1, json2);

            var status1 = newtonsoft.ToObject<OperatorTrace>(json1);
            Assert.IsTrue(status1.RequestId == status.RequestId, json1);

            var status2 = microsoft.ToObject<OperatorTrace>(json2);
            Assert.IsTrue(status2.Point == status.Point, json2);

            var xml = new XmlSerializeProxy();
            var status3 = xml.ToObject<OperatorTrace>(xml.ToString(status));
            Assert.IsTrue(status3.Describe == status.Describe, json2);

            var cdata = new CDataXmlSerializeProxy();
            var status4 = cdata.ToObject<OperatorTrace>(cdata.ToString(status));
            Assert.IsTrue(status4.Guide == status.Guide, json2);

        }


        /// <summary>
        /// 测试接口
        /// </summary>
        [Test]
        public void HelperJson()
        {
            var status = new OperatorStatus
            {
                Success = true,
                Code = 1,
                Message = "Message",
                InnerMessage = "InnerMessage",
                Exception = new System.Exception("Exception")
            };
            var json1 = SmartSerializer.ToInnerString(status);
            var opt = SmartSerializer.FromInnerString<OperatorStatus>(json1);
            var json2 = SmartSerializer.ToInnerString(opt);
            opt = (OperatorStatus)SmartSerializer.FromInnerString(json2, typeof(OperatorStatus));
            Assert.IsTrue(opt.Message == "Message", json1);
            Assert.IsTrue(opt.InnerMessage == null, json1);
        }
    }
}



