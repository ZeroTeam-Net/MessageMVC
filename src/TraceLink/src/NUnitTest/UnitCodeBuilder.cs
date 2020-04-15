using Agebull.Common;
using Agebull.Common.Ioc;
using NUnit.Framework;
using System.Diagnostics;
using System.IO;
using ZeroTeam.MessageMVC;
using ZeroTeam.MessageMVC.MessageTraceLink.WebApi.Entity;
using ZeroTeam.MessageMVC.ZeroApis;

namespace DiscoverTest
{
    [TestFixture]
    public class UnitCodeBuilder
    {
        [SetUp]
        public void Setup()
        {
            ZeroApp.UseTest(DependencyHelper.ServiceCollection);
        }

        [TearDown]
        public void TearDown()
        {
            ZeroFlowControl.Shutdown();
        }

        [Test]
        public void CreateUnitCode()
        {
            var path = 
                Path.GetDirectoryName(
                Path.GetDirectoryName(
                Path.GetDirectoryName(
                    System.Environment.CurrentDirectory)));
            ApiDiscover discover = new ApiDiscover();
            discover.Discover(typeof(MessageApiController).Assembly);
            discover.NUnitCode(IOHelper.CheckPath(path, "AutoCode"));
        }
    }
}