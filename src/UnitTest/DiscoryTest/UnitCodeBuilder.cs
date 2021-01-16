using Agebull.Common;
using Agebull.Common.Ioc;
using NUnit.Framework;
using System;
using System.IO;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC;
using ZeroTeam.MessageMVC.Sample.Controllers;
using ZeroTeam.MessageMVC.ZeroApis;

namespace DiscoverTest
{
    [TestFixture]
    public class UnitCodeBuilder
    {
        [SetUp]
        public async Task Setup()
        {
            await ZeroApp.UseTest(DependencyHelper.ServiceCollection);
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
            discover.Discover(GetType().Assembly);
            var extend = new ApiUnitTestCode
            {
                ServiceInfos = discover.ServiceInfos
            };
            extend.NUnitCode(IOHelper.CheckPath(path, "AutoCode"));
        }
        [Test]
        public void FindAppDomain()
        {
            ApiDiscover.FindAppDomain();
        }


        [Test]
        public void CreateApiSqlCode()
        {
            var path =
                Path.GetDirectoryName(
                Path.GetDirectoryName(
                Path.GetDirectoryName(
                    System.Environment.CurrentDirectory)));
            ApiDiscover discover = new ApiDiscover();
            discover.Discover(typeof(NetEventControler).Assembly);
            var extend = new ApiSqlCode
            {
                ServiceInfos = discover.ServiceInfos
            };
            extend.ApiSql(IOHelper.CheckPath(path, "Sql"));
        }

        [Test]
        public void CreateMarkdown()
        {
            ApiDiscover discover = new ApiDiscover();
            discover.Discover(typeof(NetEventControler).Assembly);

            var path = IOHelper.CheckPath(Environment.CurrentDirectory, "MarkDown");

            var extend = new ApiMarkDown
            {
                ServiceInfos = discover.ServiceInfos
            };
            extend.MarkDown(path);
        }
    }
}