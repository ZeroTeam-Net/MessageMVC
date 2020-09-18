using Agebull.Common.Ioc;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NUnit.Framework;
using System;
using ZeroTeam.MessageMVC.ApiContract;
using ZeroTeam.MessageMVC.Context;
using ZeroTeam.MessageMVC.Messages;
using ZeroTeam.MessageMVC.ZeroApis;

namespace ZeroTeam.MessageMVC.Sample.Controllers.UnitTest
{
    [TestFixture]
    public class MessageLifeCycleTest
    {
        [SetUp]
        public void SetUp()
        {
            DependencyHelper.AddTransient<IJsonSerializeProxy, JsonSerializeProxy>();
            DependencyHelper.AddTransient<IXmlSerializeProxy, XmlSerializeProxy>();
            DependencyHelper.ServiceCollection.TryAddSingleton<IInlineMessage, InlineMessage>();
            DependencyHelper.ServiceCollection.TryAddTransient<IApiResultHelper, ApiResultDefault>();
        }

        /// <summary>
        /// 测试接口
        /// </summary>
        [Test]
        public void Serialize()
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
                var json = SmartSerializer.SerializeMessage(message);
                Console.WriteLine(json);
                var message2 = SmartSerializer.ToMessage(json);


                Assert.IsTrue(message2.ID == message.ID, json);
                Assert.IsTrue(message2.State == message.State, json);
                Assert.IsTrue(message2.Topic == message.Topic, json);
                Assert.IsTrue(message2.Content == message.Content, json);
                Assert.IsTrue(message2.Result == message.Result, json);
                Assert.IsTrue(message2.Trace.LocalMachine == message.Trace.LocalMachine, json);
            }
            catch (Exception ex)
            {
                Assert.Fail($"发生异常\n{ex}");
            }
        }

        /// <summary>
        /// 测试接口
        /// </summary>
        [Test]
        public void Serialize2()
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
                SmartSerializer.SerializeMessage(message);

                DateTime start = DateTime.Now;
                for (int i = 0; i < short.MaxValue; i++)
                    SmartSerializer.SerializeMessage(message);
                var time = (DateTime.Now - start).TotalSeconds;
                Console.WriteLine($"{time}s {short.MaxValue / time} qps");
                Assert.IsTrue(time < 2, "性能不好");
            }
            catch (Exception ex)
            {
                Assert.Fail($"发生异常\n{ex}");
            }
        }

        /// <summary>
        /// 消息生命同期测试
        /// </summary>
        [Test]
        public void ArgumentInline()
        {
            var id = Guid.NewGuid().ToString("N").ToUpper();
            IInlineMessage message = new InlineMessage
            {
                ID = id,
                State = MessageState.Accept,
                Topic = "Topic",
                Title = "Title",
                Content = @"{""Value"": ""Content""}",
                Trace = TraceInfo.New(id),
                Result = @"{""Value"": ""Result""}"
            };
            message.PrepareInline();
            Assert.IsTrue(message.DataState == (MessageDataState.ArgumentOffline | MessageDataState.ResultOffline), message.DataState.ToString());

            //message.PrepareResult(null, ApiResultHelper.State);
            message.RestoryContent(DependencyHelper.GetService<IJsonSerializeProxy>(), typeof(Argument));;
            Assert.IsTrue(message.ArgumentData is Argument, message.ArgumentData.GetTypeName());
            Assert.IsTrue(message.DataState == (MessageDataState.ArgumentOffline | MessageDataState.ResultOffline | MessageDataState.ArgumentInline),
                message.DataState.ToString());

        }


        /// <summary>
        /// 消息生命同期测试
        /// </summary>
        [Test]
        public void SetResultData()
        {
            var id = Guid.NewGuid().ToString("N").ToUpper();
            IInlineMessage message = new InlineMessage
            {
                ID = id,
                State = MessageState.Accept,
                Topic = "Topic",
                Title = "Title",
                Content = @"{""Value"": ""Content""}",
                Trace = TraceInfo.New(id),
                Result = @"{""Value"": ""Result""}"
            };
            message.Reset();
            //message.PrepareResult(null, ApiResultHelper.State);
            message.RestoryContent(DependencyHelper.GetService<IJsonSerializeProxy>(), typeof(Argument)); ;
            message.ResultData = new Argument<int>
            {
                Value = 1
            };
            Assert.IsTrue(message.DataState == (MessageDataState.ArgumentOffline | MessageDataState.ArgumentInline | MessageDataState.ResultInline),
                message.DataState.ToString());

        }


        /// <summary>
        /// 消息生命同期测试
        /// </summary>
        [Test]
        public void MessageStateResult()
        {
            var id = Guid.NewGuid().ToString("N").ToUpper();
            IInlineMessage message = new InlineMessage
            {
                ID = id,
                State = MessageState.Accept,
                Topic = "Topic",
                Title = "Title",
                Content = @"{""Value"": ""Content""}",
                Trace = TraceInfo.New(id),
                Result = @"{""Value"": ""Result""}"
            };
            message.Reset();
            //message.PrepareResult(null, ApiResultHelper.State);
            message.RestoryContent(DependencyHelper.GetService<IJsonSerializeProxy>(), typeof(Argument)); ;
            message.State = MessageState.Failed;
            message.OfflineResult();
            Assert.IsTrue(message.Result != null, message.Result);
            Assert.IsTrue(message.DataState == (MessageDataState.ArgumentOffline | MessageDataState.ArgumentInline | MessageDataState.ResultOffline | MessageDataState.ResultInline),
                message.DataState.ToString());

        }

        /// <summary>
        /// 消息生命同期测试
        /// </summary>
        [Test]
        public void ToMessageResult()
        {
            var id = Guid.NewGuid().ToString("N").ToUpper();
            IInlineMessage message = new InlineMessage
            {
                ID = id,
                State = MessageState.Accept,
                Topic = "Topic",
                Title = "Title",
                Content = @"{""Value"": ""Content""}",
                Trace = TraceInfo.New(id),
                ResultData = new Argument<int>
                {
                    Value = 1
                }
            };
            message.PrepareInline();
            Assert.IsTrue(message.DataState == (MessageDataState.ArgumentOffline | MessageDataState.ResultInline),
                message.DataState.ToString());

            message.OfflineResult();
            Assert.IsTrue(message.Result != null, message.Result);
            Assert.IsTrue(message.DataState == (MessageDataState.ArgumentOffline | MessageDataState.ResultOffline | MessageDataState.ResultInline),
                message.DataState.ToString());

            var result = message.ToMessageResult(true);

            var json = SmartSerializer.ToString(result);

            var result2 = SmartSerializer.ToObject<MessageResult>(json);
            Assert.IsTrue(result2.ID == message.ID, result2.ID);
            Assert.IsTrue(result2.ResultData == null, result2.ResultData.GetTypeName());
            Assert.IsTrue(result2.State == message.State, result2.State.ToString());
            Assert.IsTrue(result2.DataState == MessageDataState.ResultOffline, result2.DataState.ToString());
            Assert.IsTrue(result2.Result == message.Result, result2.Result);
            Assert.IsTrue(result2.Trace.LocalMachine == message.Trace.LocalMachine, result2.Trace.LocalMachine);
        }

        /// <summary>
        /// 消息生命同期测试
        /// </summary>
        [Test]
        public void MessageLifeCycle()
        {
            var id = Guid.NewGuid().ToString("N").ToUpper();
            IInlineMessage message = new InlineMessage
            {
                ID = id,
                State = MessageState.Accept,
                Topic = "Topic",
                Title = "Title",
                Content = @"{""Value"": ""Content""}",
                Trace = TraceInfo.New(id),
                Result = @"{""Value"": ""Result""}"
            };
            message.PrepareInline();
            Assert.IsTrue(message.DataState == (MessageDataState.ArgumentOffline | MessageDataState.ResultOffline), message.DataState.ToString());

            //message.PrepareResult(null, ApiResultHelper.State);
            message.RestoryContent(DependencyHelper.GetService<IJsonSerializeProxy>(), typeof(Argument)); ;
            Assert.IsTrue(message.ArgumentData is Argument, message.ArgumentData.GetTypeName());
            Assert.IsTrue(message.DataState == (MessageDataState.ArgumentOffline | MessageDataState.ResultOffline | MessageDataState.ArgumentInline),
                message.DataState.ToString());

            message = new InlineMessage
            {
                ID = id,
                State = MessageState.Accept,
                Topic = "Topic",
                Title = "Title",
                Content = @"{""Value"": ""Content""}",
                Trace = TraceInfo.New(id),
                Result = @"{""Value"": ""Result""}"
            };
            message.Reset();
            Assert.IsTrue(message.DataState == MessageDataState.ArgumentOffline, message.DataState.ToString());
            Assert.IsTrue(message.Result == null, message.Result);
            //message.PrepareResult(null, ApiResultHelper.State);
            message.RestoryContent(DependencyHelper.GetService<IJsonSerializeProxy>(), typeof(Argument)); ;
            Assert.IsTrue(message.DataState == (MessageDataState.ArgumentOffline | MessageDataState.ArgumentInline), message.DataState.ToString());

            message.ResultData = new Argument<int>
            {
                Value = 1
            };
            Assert.IsTrue(message.DataState == (MessageDataState.ArgumentOffline | MessageDataState.ArgumentInline | MessageDataState.ResultInline),
                message.DataState.ToString());

            message.OfflineResult();
            Assert.IsTrue(message.Result != null, message.Result);
            Assert.IsTrue(message.DataState == (MessageDataState.ArgumentOffline | MessageDataState.ArgumentInline | MessageDataState.ResultOffline | MessageDataState.ResultInline),
                message.DataState.ToString());

            var result = message.ToMessageResult(true);

            var json = SmartSerializer.ToString(result);

            var result2 = SmartSerializer.ToObject<MessageResult>(json);
            Assert.IsTrue(result2.ID == message.ID, result2.ID);
            Assert.IsTrue(result2.ResultData == result.ResultData, result2.ResultData.GetTypeName());
            Assert.IsTrue(result2.State == result.State, result2.State.ToString());
            Assert.IsTrue(result2.DataState == result.DataState, result2.DataState.ToString());
            Assert.IsTrue(result2.Result == result.Result, result2.Result);
            Assert.IsTrue(result2.Trace.LocalMachine == message.Trace.LocalMachine, result2.Trace.LocalMachine);
        }
    }
}



