﻿using Agebull.EntityModel.Common;
using NUnit.Framework;
using System;

namespace ZeroTeam.MessageMVC.Sample.Controllers.UnitTest
{
    [TestFixture]
    public class Number46Test
    {
        /// <summary>
        /// 测试接口
        /// </summary>
        [Test]
        public void Number46()
        {
            var random = new Random((int)(DateTime.Now.Ticks % int.MaxValue));

            int cnt = 0;
            while (++cnt < 100000)
            {
                var num = random.Next(int.MaxValue);
                Assert.IsTrue(num.To46().From46() == num, $"{num.To46()} | {num}");

            }

        }
    }
}



