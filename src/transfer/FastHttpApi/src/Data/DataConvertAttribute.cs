﻿using BeetleX.Buffers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using static BeetleX.FastHttpApi.HttpParse;

namespace BeetleX.FastHttpApi.Data
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public abstract class DataConvertAttribute : Attribute
    {
        public abstract void Execute(IDataContext dataContext, HttpRequest request);
    }

    public class JsonDataConvertAttribute : DataConvertAttribute
    {
        public override void Execute(IDataContext dataContext, HttpRequest request)
        {
            if (request.Length > 0)
            {
                using (request.Stream.LockFree())
                {
                    using (System.IO.StreamReader streamReader = new System.IO.StreamReader(request.Stream))
                    using (JsonTextReader reader = new JsonTextReader(streamReader))
                    {
                        JsonSerializer jsonSerializer = JsonSerializer.CreateDefault();
                        JToken token = (JToken)jsonSerializer.Deserialize(reader);
                        DataContextBind.BindJson(dataContext, token);
                    }
                }
            }
        }
    }


    public class FormUrlDataConvertAttribute : DataConvertAttribute
    {

        public FormUrlDataConvertAttribute()
        {

        }

        public FormUrlDataConvertAttribute(string encoding)
        {
            mEncoding = encoding;
        }

        private string mEncoding;

        public override void Execute(IDataContext dataContext, HttpRequest request)
        {
            if (request.Length > 0)
            {
                using (var bytes = System.Buffers.MemoryPool<byte>.Shared.Rent(request.Length))
                {
                    int len = request.Stream.Read(bytes.Memory.Span);
                    Encoding encoding = string.IsNullOrEmpty(mEncoding) ? Encoding.UTF8 : Encoding.GetEncoding(mEncoding);
                    using (var chars = System.Buffers.MemoryPool<char>.Shared.Rent(request.Length))
                    {
                        len = encoding.GetChars(bytes.Memory.Slice(0, len).Span, chars.Memory.Span);
                        DataContextBind.BindFormUrl(dataContext, chars.Memory.Slice(0, len).Span);
                    }
                }
            }
        }
    }

    public class MultiDataConvertAttribute : DataConvertAttribute
    {
        public override void Execute(IDataContext dataContext, HttpRequest request)
        {
            if (request.Method == HttpParse.POST_TAG)
            {
                DataLoader dataLoader = new DataLoader(request.ContentType);
                dataLoader.Load(dataContext, request);
            }
        }
    }
    public class DataLoader
    {
        public DataLoader(string contentType)
        {

            if (!string.IsNullOrEmpty(contentType))
            {
                for (int i = 0; i < contentType.Length; i++)
                {
                    if (contentType[i] == '=')
                    {
                        int offset = i + 1;
                        mStartBoundary = "--" + contentType.Substring(offset);
                        mEndBoundary = mStartBoundary + "--";
                    }
                }
            }
        }

        private string mEndBoundary;

        private string mStartBoundary;

        public const string ContentDisposition = "Content-Disposition";

        public const string ContentType = "Content-Type";

        private void CreateParameter(IDataContext context, HttpRequest request, string name, string filename, string contentType
            , System.IO.MemoryStream stream,string charSet)
        {
            if (string.IsNullOrEmpty(filename))
            {
                string value = Encoding.UTF8.GetString(stream.GetBuffer(), 0, (int)stream.Length - 2);
                context.SetValue(name, value);
            }
            else
            {
                PostFile postFile = new PostFile();
                stream.Position = 0;
                stream.SetLength(stream.Length - 2);
                postFile.Data = stream;
                postFile.CharSet = charSet;
                postFile.ContentType = contentType;
                postFile.FileName = filename;
                context.SetValue(name, postFile);
                request.Files.Add(postFile);
            }
        }

        public void Load(IDataContext dataContext, HttpRequest request)
        {
            if (!string.IsNullOrEmpty(mStartBoundary))
            {
                var stream = request.Stream;
                if (stream.ReadLine() == mStartBoundary)
                {
                    string name = null, filename = null, contentType = null, charSet=null;
                    while (stream.TryReadWith(HeaderTypeFactory.LINE_BYTES, out string headerLine))
                    {
                        if (string.IsNullOrEmpty(headerLine))
                        {
                            System.IO.MemoryStream memoryStream = new System.IO.MemoryStream();
                            while (true)
                            {
                                IndexOfResult indexOf = stream.IndexOf(HeaderTypeFactory.LINE_BYTES);
                                if (indexOf.End == null)
                                {
                                    break;
                                }
                                if (indexOf.Length == mEndBoundary.Length + 2)
                                {
                                    byte[] buffer = HttpParse.GetByteBuffer();
                                    stream.Read(buffer, 0, indexOf.Length);
                                    string line = Encoding.UTF8.GetString(buffer, 0, indexOf.Length - 2);
                                    if (line == mEndBoundary)
                                    {

                                        CreateParameter(dataContext, request, name, filename, contentType, memoryStream,charSet);
                                        name = filename = contentType = null;
                                        return;
                                    }
                                    else
                                    {
                                        memoryStream.Write(buffer, 0, indexOf.Length - 2);
                                    }
                                }
                                else if (indexOf.Length == mStartBoundary.Length + 2)
                                {
                                    byte[] buffer = HttpParse.GetByteBuffer();
                                    stream.Read(buffer, 0, indexOf.Length);
                                    string line = Encoding.UTF8.GetString(buffer, 0, indexOf.Length - 2);
                                    if (line == mStartBoundary)
                                    {
                                        CreateParameter(dataContext, request, name, filename, contentType, memoryStream,charSet);
                                        //name = filename = contentType = null;
                                        break;
                                    }
                                    else
                                    {
                                        memoryStream.Write(buffer, 0, indexOf.Length - 2);
                                    }
                                }
                                else
                                {
                                    IMemoryBlock block = indexOf.Start;
                                    while (block != null)
                                    {
                                        int offset = 0;
                                        int length = block.Length;
                                        if (block.ID == indexOf.Start.ID)
                                            offset = indexOf.StartPostion;
                                        if (block.ID == indexOf.End.ID)
                                        {
                                            length = indexOf.EndPostion + 1;
                                        }
                                        memoryStream.Write(block.Data, offset, length - offset);
                                        if (block.ID == indexOf.End.ID)
                                        {
                                            stream.ReadFree(indexOf.Length);
                                            break;
                                        }
                                        else
                                            block = block.NextMemory;
                                    }
                                }
                            }
                        }
                        else
                        {
                            var result = HttpParse.AnalyzeContentHeader(headerLine);
                            if (string.Compare(result.Name, ContentDisposition, true) == 0)
                            {
                                if (result.Properties != null)
                                {
                                    for (int i = 0; i < result.Properties.Length; i++)
                                    {
                                        if (result.Properties[i].Name.ToLower() == "name")
                                        {
                                            name = result.Properties[i].Value.Replace("\"","");
                                        }
                                        else if (result.Properties[i].Name.ToLower() == "filename")
                                        {
                                            filename = result.Properties[i].Value.Replace("\"", "");
                                        }
                                    }
                                }
                            }
                            else if (string.Compare(result.Name, ContentType, true) == 0)
                            {
                                contentType = result.Value;
                                if (result.Properties != null)
                                {
                                    for (int i = 0; i < result.Properties.Length; i++)
                                    {
                                        if (result.Properties[i].Name.ToLower() == "charset")
                                        {
                                            charSet = result.Properties[i].Value;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
