﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BeetleX.FastHttpApi
{
    public class ActionContext
    {

        internal ActionContext(ActionHandler handler, IHttpContext context, ActionHandlerFactory actionHandlerFactory)
        {
            Handler = handler;
            mFilters = handler.Filters;
            HttpContext = context;
            ActionHandlerFactory = actionHandlerFactory;
            Parameters = handler.GetParameters(context);
            Controller = handler.Controller;
            if (handler.InstanceType != InstanceType.Single)
            {
                if (handler.InstanceType == InstanceType.Session)
                {
                    var factory = SessionControllerFactory.GetFactory(context.Session);
                    Controller = factory[handler.ControllerUID];
                    if (Controller == null)
                    {
                        Controller = actionHandlerFactory.GetController(handler.ControllerType, context);
                        if (Controller == null)
                            Controller = Activator.CreateInstance(handler.ControllerType);
                        factory[handler.ControllerUID] = Controller;
                    }
                }
                else
                {
                    Controller = actionHandlerFactory.GetController(handler.ControllerType, context);
                    if (Controller == null)
                        Controller = Activator.CreateInstance(handler.ControllerType);
                }
            }
            if (Controller == null)
                Controller = handler.Controller;
        }

        private List<FilterAttribute> mFilters;

        public object[] Parameters { get; private set; }

        public IHttpContext HttpContext { get; internal set; }

        public ActionHandler Handler { get; private set; }

        public ActionHandlerFactory ActionHandlerFactory { get; private set; }

        public Object Result { get; set; }

        public Exception Exception { get; set; }

        public object Controller { get; set; }

        public string GetCacheKey()
        {
            return Handler.GetCackeKey(Parameters);
        }

        private void OnExecute(IActionResultHandler resultHandler)
        {
            try
            {
                if (FilterExecuting())
                {
                    try
                    {
                        Result = Handler.Invoke(Controller, HttpContext, ActionHandlerFactory, Parameters);
                    }
                    catch (Exception error)
                    {
                        Exception = error;
                    }
                    finally
                    {
                        FilterExecuted();
                    }
                }
                if (Exception != null)
                {
                    Handler.IncrementError();
                    resultHandler.Error(Exception);
                }
                else
                    resultHandler.Success(Result);
            }
            catch (Exception e_)
            {
                Handler.IncrementError();
                resultHandler.Error(e_);
            }
            finally
            {
                DisposedController();
            }
        }

        private async Task OnAsyncExecute(IActionResultHandler resultHandler)
        {
            try
            {
                if (FilterExecuting())
                {
                    try
                    {
                        var task = (Task)Handler.Invoke(Controller, HttpContext, ActionHandlerFactory, Parameters);
                        await task;
                        if (Handler.PropertyHandler != null)
                            Result = Handler.PropertyHandler.Get(task);
                        else
                            Result = null;
                    }
                    catch (Exception error)
                    {
                        Exception = error;
                    }
                    finally
                    {
                        FilterExecuted();
                    }
                }
                if (Exception != null)
                {
                    Handler.IncrementError();
                    resultHandler.Error(Exception);
                }
                else
                    resultHandler.Success(Result);
            }
            catch (Exception e_)
            {
                Handler.IncrementError();
                resultHandler.Error(e_);
            }
            finally
            {
                DisposedController();
            }
        }

        private void DisposedController()
        {
            if (Handler.InstanceType == InstanceType.None)
            {
                try
                {
                    if (Controller is IDisposable disposable)
                        disposable?.Dispose();
                }
                catch (Exception e_)
                {
                    if (HttpContext.Server.EnableLog(EventArgs.LogType.Error))
                    {
                        var request = HttpContext.Request;
                        HttpContext.Server.Log(EventArgs.LogType.Error,
                            $"HTTP {request.RemoteIPAddress} {request.Method} {request.BaseUrl} controller disposed error {e_.Message}@{e_.StackTrace}");
                    }
                }
            }
        }

        struct ActionTask : IEventWork
        {
            public ActionTask(ActionContext context, IActionResultHandler resultHandler)
            {
                Context = context;
                ResultHandler = resultHandler;

            }

            public ActionContext Context { get; set; }

            public IActionResultHandler ResultHandler { get; set; }

            public void Dispose()
            {

            }

            public async Task Execute()
            {
                if (Context.Handler.Async)
                {
                    await Context.OnAsyncExecute(ResultHandler);
                }
                else
                {
                    Context.OnExecute(ResultHandler);
                }
            }
        }


        internal void Execute(IActionResultHandler resultHandler)
        {
            if (Handler.ValidateRPS())
            {
                Handler.IncrementRequest();
                if (Handler.ThreadQueue == null || Handler.ThreadQueue.Type == ThreadQueueType.None)
                {
                    if (Handler.Async)
                    {
                        OnAsyncExecute(resultHandler);
                    }
                    else
                    {
                        OnExecute(resultHandler);
                    }
                }
                else
                {
                    ActionTask actionTask = new ActionTask(this, resultHandler);
                    var queue = Handler.ThreadQueue.GetQueue(this.HttpContext);
                    if (Handler.ThreadQueue.Enabled(queue))
                    {
                        this.HttpContext.Queue = queue;
                        queue.Enqueue(actionTask);
                    }
                    else
                    {
                        Handler.IncrementError();
                        resultHandler.Error(new Exception($"{Handler.SourceUrl} process error,out of queue limit!"), EventArgs.LogType.Warring, 500);
                    }
                }
            }
            else
            {
                Handler.IncrementError();
                resultHandler.Error(new Exception($"{Handler.SourceUrl} process error,out of max rps!"), EventArgs.LogType.Warring, 509);
            }

        }

        private int mFilterIndex;

        private bool FilterExecuting()
        {
            if (mFilters.Count > 0)
            {
                for (int i = 0; i < mFilters.Count; i++)
                {
                    bool result = mFilters[i].Executing(this);
                    mFilterIndex++;
                    if (!result)
                        return false;
                }
            }
            return true;
        }

        private void FilterExecuted()
        {
            if (mFilters.Count > 0)
            {
                int start = mFilterIndex - 1;
                for (int i = start; i >= 0; i--)
                {
                    mFilters[i].Executed(this);
                }
            }
        }
    }
}
