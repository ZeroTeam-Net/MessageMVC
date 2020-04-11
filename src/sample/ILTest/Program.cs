using System;

namespace ILTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var str = nameof(Te.None);
            Te obj = (Te)Enum.Parse(typeof(Te), str);

            Console.WriteLine(obj);
        }

    }
    enum Te { None}
}
