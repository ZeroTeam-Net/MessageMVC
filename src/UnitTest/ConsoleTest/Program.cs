using System;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;
using Agebull.Common.Ioc;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ZeroTeam.MessageMVC.ApiContract;
using ZeroTeam.MessageMVC.Context;
using ZeroTeam.MessageMVC.Messages;
using ZeroTeam.MessageMVC.ZeroApis;

namespace ConsoleTest
{
    class Program
    {
        static void Main(string[] args)
        {
            DependencyHelper.AddTransient<IJsonSerializeProxy, JsonSerializeProxy>();
            DependencyHelper.AddTransient<IXmlSerializeProxy, XmlSerializeProxy>();
            DependencyHelper.ServiceCollection.TryAddSingleton<IInlineMessage, InlineMessage>();
            DependencyHelper.ServiceCollection.TryAddTransient<IApiResultHelper, ApiResultDefault>();
            Serialize();
            //Serialize2();
            //Serialize3();
            Console.ReadKey();
        }
        static JsonSerializerOptions option = new JsonSerializerOptions
        {
            IgnoreNullValues = true,
            WriteIndented = false,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            PropertyNameCaseInsensitive = true
            //PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        const int testCount = 100000000;
        static void Serialize()
        {
            try
            {
                var id = Guid.NewGuid().ToString("N").ToUpper();
                IMessageItem message = new MessageItem
                {
                    ID = id,
                    State = MessageState.Accept,
                    Topic = "Topic",
                    Title = "Title",
                    Content = @"{""Value"": ""Content""}",
                    Trace = TraceInfo.New(id),
                    Result = @"{""Value"": ""Result""}"
                };
                Task.Factory.StartNew(() =>
                {
                    JsonSerializerOptions option2 = new JsonSerializerOptions
                    {
                        IgnoreNullValues = true,
                        WriteIndented = false,
                        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                        PropertyNameCaseInsensitive = true
                        //PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    };
                    DateTime start = DateTime.Now;
                    Parallel.For(0, testCount, (idx) => JsonSerializer.Serialize(new MessageItem
                    {
                        ID = id,
                        State = MessageState.Accept,
                        Topic = "Topic",
                        Title = "Title",
                        Content = @"{""Value"": ""Content""}",
                        Trace = TraceInfo.New(id),
                        Result = @"{""Value"": ""Result""}"
                    }, option));
                    var time = (DateTime.Now - start).TotalSeconds;
                    Console.WriteLine($"{testCount / time} /s");
                });
                //Task.Factory.StartNew(() =>
                //{
                //    JsonSerializerOptions option2 = new JsonSerializerOptions
                //    {
                //        IgnoreNullValues = true,
                //        WriteIndented = false,
                //        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                //        PropertyNameCaseInsensitive = true
                //        //PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                //    };
                //    DateTime start = DateTime.Now;
                //    Parallel.For(0, testCount, (idx) => JsonSerializer.Serialize(message, option2));
                //    var time = (DateTime.Now - start).TotalSeconds;
                //    Console.WriteLine($"{testCount / time} /s");
                //});
                //Task.Factory.StartNew(() =>
                //{
                //    DateTime start = DateTime.Now;
                //    Parallel.For(0, testCount, (idx) => JsonSerializer.Serialize(message, new JsonSerializerOptions
                //    {
                //        IgnoreNullValues = true,
                //        WriteIndented = false,
                //        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                //        PropertyNameCaseInsensitive = true
                //        //PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                //    }));
                //    var time = (DateTime.Now - start).TotalSeconds;
                //    Console.WriteLine($"{testCount / time} /s");
                //});
            }
            catch (Exception ex)
            {
                Console.WriteLine($"发生异常\n{ex}");
            }
        }


        static void Serialize2()
        {
            try
            {
                var id = Guid.NewGuid().ToString("N").ToUpper();
                IMessageItem message = new MessageItem
                {
                    ID = id,
                    State = MessageState.Accept,
                    Topic = "Topic",
                    Title = "Title",
                    Content = @"{""Value"": ""Content""}",
                    Trace = TraceInfo.New(id),
                    Result = @"{""Value"": ""Result""}"
                };
                var serialize = new JsonSerializeProxy();
                serialize.ToString(message, false);

                DateTime start = DateTime.Now;
                for (int i = 0; i < testCount; i++)
                    serialize.ToString(message, false);
                var time = (DateTime.Now - start).TotalSeconds;
                Console.WriteLine($"{time}s {testCount / time} qps");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"发生异常\n{ex}");
            }
        }


        static void Serialize3()
        {
            try
            {
                var id = Guid.NewGuid().ToString("N").ToUpper();
                IMessageItem message = new MessageItem
                {
                    ID = id,
                    State = MessageState.Accept,
                    Topic = "Topic",
                    Title = "Title",
                    Content = @"{""Value"": ""Content""}",
                    Trace = TraceInfo.New(id),
                    Result = @"{""Value"": ""Result""}"
                };

                JsonSerializer.Serialize(message);

                DateTime start = DateTime.Now;
                for (int i = 0; i < testCount; i++)
                    JsonSerializer.Serialize(message);
                var time = (DateTime.Now - start).TotalSeconds;
                Console.WriteLine($"{time}s {testCount / time} qps");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"发生异常\n{ex}");
            }
        }
    }
}
