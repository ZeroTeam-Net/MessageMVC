using Agebull.Common.Ioc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading.Tasks;

namespace ILTest
{
    class Program
    {
        static void Main(string[] args)
        {
        }

        static async Task CALL(string[] args)
        {
            var test = new Test();
            await test.Do1();
            await test.Do2();

        }
        public class Test
        {
            public Task<int> Do1()
            {
                return Task.FromResult(1);
            }
            public async Task<int> Do2()
            {
                return await Task.FromResult(1);
            }
        }
    }
}
