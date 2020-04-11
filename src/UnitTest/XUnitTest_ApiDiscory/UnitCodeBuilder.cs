using Agebull.Common;
using Agebull.Common.Ioc;
using System;
using System.IO;
using Xunit;
using ZeroTeam.MessageMVC;
using ZeroTeam.MessageMVC.ZeroApis;

namespace DiscoryTest
{
    public class UnitCodeBuilder : IDisposable
    {
        public UnitCodeBuilder()
        {
            ZeroApp.UseTest(IocHelper.ServiceCollection);
        }
        void IDisposable.Dispose()
        {
            ZeroFlowControl.Shutdown();
        }

        [Fact]
        public void CreateUnitCode()
        {
            var path =
                Path.GetDirectoryName(
                Path.GetDirectoryName(
                Path.GetDirectoryName(
                    System.Environment.CurrentDirectory)));
            ApiDiscover discover = new ApiDiscover();
            discover.Discover(this.GetType().Assembly);
            discover.NUnitCode(IOHelper.CheckPath(path, "AutoCode"));
        }
    }
}