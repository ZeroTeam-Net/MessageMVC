using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ZeroTeam.MessageMVC.Documents;

namespace ZeroTeam.MessageMVC.ZeroApis
{

    /// <summary>
    /// API扩展功能
    /// </summary>
    public class ApiUnitTestCode
    {
        /// <summary>
        /// 站点文档信息
        /// </summary>
        public Dictionary<string, ServiceInfo> ServiceInfos { get; set; }


        #region UnitTest

        /// <summary>
        /// 生成单元测试代码
        /// </summary>
        /// <param name="path"></param>
        public void NUnitCode(string path)
        {
            foreach (var serviceInfo in ServiceInfos.Values)
            {
                if (serviceInfo.Aips.Count == 0)
                {
                    continue;
                }
                var file = Path.Combine(path, $"{serviceInfo.Name}.cs");
                if (File.Exists(file))
                    continue;
                var code = new StringBuilder();
                code.AppendLine($@"using System;
using System.Threading.Tasks;
using NUnit.Framework;
using Agebull.Common.Ioc;
using ZeroTeam.MessageMVC;
using ZeroTeam.MessageMVC.Messages;

namespace {serviceInfo.Type.Namespace}.UnitTest
{{
    [TestFixture]
    public class {serviceInfo.Type.GetTypeName()}UnitTest
    {{
        [SetUp]
        public void Setup()
        {{
            ZeroApp.UseTest(DependencyHelper.ServiceCollection, typeof({serviceInfo.Type.GetTypeName()}).Assembly);
        }}

        [TearDown]
        public void TearDown()
        {{
            ZeroFlowControl.Shutdown();
        }}

");
                foreach (var api in serviceInfo.Aips.Values.Cast<ApiActionInfo>())
                {
                    string json = api.ArgumentJson();
                    code.AppendLine($@"

        /// <summary>
        /// {api.Caption}
        /// </summary>
        [Test]
        public async Task {api.Routes[0].Replace('/', '_')}()
        {{
            var (msg, ser) = await MessagePoster.Post(new InlineMessage
            {{
                ServiceName = ""{serviceInfo.Name}"",
                ApiName = ""{api.Routes[0]}"",
                Content = 
@""{json}""
            }});
            Assert.IsTrue(msg.State == MessageState.Success , msg.Result);
        }}
");
                }
                code.AppendLine("    }");
                code.AppendLine("}");
                File.WriteAllText(file, code.ToString());
            }
        }

        /// <summary>
        /// 生成单元测试代码
        /// </summary>
        /// <param name="path"></param>
        public void XUnitCode(string path)
        {
            foreach (var serviceInfo in ServiceInfos.Values)
            {
                if (serviceInfo.Aips.Count == 0)
                {
                    continue;
                }
                var file = Path.Combine(path, $"{serviceInfo.Name}.cs");
                if (File.Exists(file))
                    continue;
                var code = new StringBuilder();
                code.AppendLine($@"using System;
using System.Threading.Tasks;
using Xunit;
using Agebull.Common.Ioc;
using ZeroTeam.MessageMVC;
using ZeroTeam.MessageMVC.Messages;

namespace {serviceInfo.Type.Namespace}.UnitTest
{{
    public class {serviceInfo.Type.GetTypeName()}UnitTest : IDisposable
    {{
        public {serviceInfo.Type.GetTypeName()}UnitTest()
        {{
            ZeroApp.UseTest(DependencyHelper.ServiceCollection, typeof({serviceInfo.Type.GetTypeName()}).Assembly);
        }}
        void IDisposable.Dispose()
        {{
            ZeroFlowControl.Shutdown();
        }}");
                foreach (var api in serviceInfo.Aips.Values.Cast<ApiActionInfo>())
                {
                    string json = api.ArgumentJson();
                    code.AppendLine($@"

        /// <summary>
        /// {api.Caption}
        /// </summary>
        [Fact]
        public async Task {api.Name}()
        {{
            var (msg, ser) = await MessagePoster.Post(new InlineMessage
            {{
                ServiceName = ""{serviceInfo.Name}"",
                ApiName = ""{api.Routes[0]}"",
                Content = @""{json}""
            }});
            Assert.True(msg.State == MessageState.Success , msg.Result);
        }}");
                }
                code.AppendLine("    }");
                code.AppendLine("}");
                File.WriteAllText(file, code.ToString());
            }
        }

        #endregion

    }
}