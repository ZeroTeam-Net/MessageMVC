using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace BeetleX.FastHttpApi.StaticResurce
{
    public class ResourceCenter
    {
        public ResourceCenter(HttpApiServer server)
        {
            Server = server;
            Path = Server.Options.StaticResourcePath;
            if (Path.IsMissing())
                return;
            if (Path[^1] != System.IO.Path.DirectorySeparatorChar)
            {
                Path += System.IO.Path.DirectorySeparatorChar;
            }

            foreach (string item in server.Options.StaticResurceType.ToLower().Split(';'))
            {
                FileContentType fct = new(item);
                mExts[fct.Ext] = fct;
            }
            mDefaultPages.AddRange(Server.Options.DefaultPage.Split(";"));

        }

        public void AddDefaultPage(string file)
        {
            if (!string.IsNullOrEmpty(file))
                mDefaultPages.Add(file);
        }

        public void SetDefaultPages(string files)
        {
            if (files != null)
            {
                Server.Options.DefaultPage = files;
                mDefaultPages.Clear();
                mDefaultPages.AddRange(files.Split(";"));
            }
        }

        public void SetFileExts(string exts)
        {
            if (exts != null)
            {
                Server.Options.StaticResurceType = exts;
                foreach (string item in exts.ToLower().Split(';'))
                {
                    if (!mExts.ContainsKey(item))
                    {
                        FileContentType fct = new(item);
                        mExts[fct.Ext] = fct;
                    }
                }
            }
        }

        private readonly ConcurrentDictionary<string, FileResource> mResources = new(StringComparer.OrdinalIgnoreCase);

        private readonly ConcurrentDictionary<string, FileContentType> mExts = new(StringComparer.OrdinalIgnoreCase);

        private readonly List<FileSystemWatcher> mFileWatch = new();

        private readonly List<string> mDefaultPages = new();

        public List<string> DefaultPages => mDefaultPages;

        public ConcurrentDictionary<string, FileContentType> Exts => mExts;

        public HttpApiServer Server { get; private set; }

        public string Path { get; internal set; }

        public event EventHandler<EventFindFileArgs> Find;

        public event EventHandler<EventFileResponseArgs> FileResponse;

        public bool Debug { get; set; }

        private static string GetResourceUrl(string name)
        {
            char[] charname = name.ToCharArray();
            List<int> indexs = new();
            for (int i = 0; i < charname.Length; i++)
            {
                if (charname[i] == '.')
                    indexs.Add(i);
            }
            for (int i = 0; i < indexs.Count - 1; i++)
            {
                charname[indexs[i]] = '/';
            }
            //if (Server.Options.UrlIgnoreCase)
            //    return HttpParse.CharToLower(charname);
            //else
            return new string(charname);
        }

        private static void SaveTempFile(System.Reflection.Assembly assembly, string recname, string filename)
        {
            using Stream stream = assembly.GetManifestResourceStream(recname);
            byte[] buffer = HttpParse.GetByteBuffer();
            int length = (int)stream.Length;
            using FileStream fs = File.Create(filename);
            while (length > 0)
            {
                int len = stream.Read(buffer, 0, buffer.Length);
                fs.Write(buffer, 0, len);
                fs.Flush();
                length -= len;
            }
        }

        public void LoadManifestResource(System.Reflection.Assembly assembly)
        {
            return;
            string[] files = assembly.GetManifestResourceNames();
            string tmpFolder = "_tempview";
            if (!Directory.Exists(tmpFolder))
            {
                Directory.CreateDirectory(tmpFolder);
            }
            foreach (string item in files)
            {
                int offset = item.IndexOf(".views");
                if (offset > 0)
                {
                    string url = GetResourceUrl(item.Substring(offset + 6, item.Length - offset - 6));
                    string ext = System.IO.Path.GetExtension(item).ToLower();
                    ext = ext[1..];
                    if (mExts.ContainsKey(ext))
                    {
                        string urlname = url;
                        string filename = tmpFolder + System.IO.Path.DirectorySeparatorChar + item;
                        SaveTempFile(assembly, item, filename);
                        FileResource fr;
                        bool nogzip = !(Server?.Options.NoGzipFiles.IndexOf(ext) >= 0);
                        bool cachefile = Server?.Options.CacheFiles.IndexOf(ext) >= 0;
                        if (Debug)
                        {
                            fr = new NoCacheResource(filename, urlname);
                            if (nogzip)
                                fr.GZIP = true;
                        }
                        else
                        {
                            if (cachefile)
                            {
                                fr = new FileResource(filename, urlname);
                            }
                            else
                            {
                                fr = new NoCacheResource(filename, urlname);
                                if (nogzip)
                                    fr.GZIP = true;
                            }
                        }
                        mResources[urlname] = fr;
                        fr.Load();
                        if (Server != null && Server.BaseServer != null)
                        {
                            if (Server.BaseServer.EnableLog(EventArgs.LogType.Info))
                                Server.BaseServer.Log(EventArgs.LogType.Info, null, "load static resource " + urlname);
                        }

                    }
                }
            }
        }

        public void Load()
        {

            if (Directory.Exists(Path))
            {
                LoadFolder(Path);
                string exts = "js;html;htm;css";
                foreach (string key in mExts.Keys)
                {
                    if (exts.Contains(key, StringComparison.CurrentCulture))
                    {
                        FileSystemWatcher fsw = new(Path, "*." + key);
                        fsw.IncludeSubdirectories = true;
                        fsw.Changed += (o, e) =>
                        {
                            CreateResource(e.FullPath, true);
                        };
                        fsw.EnableRaisingEvents = true;
                        mFileWatch.Add(fsw);
                    }
                }
            }


        }

        private void OutputFileResource(FileContentType fct, FileResource fr, HttpResponse response)
        {
            if (!Debug)
            {
                string IfNoneMatch = response.Request.IfNoneMatch;
                if (!string.IsNullOrEmpty(IfNoneMatch) && IfNoneMatch == fr.FileMD5)
                {
                    if (Server.EnableLog(EventArgs.LogType.Info))
                        Server.BaseServer.Log(EventArgs.LogType.Info, null, $"HTTP {response.Request.ID} {response.Request.RemoteIPAddress} get {response.Request.Url} source no modify ");
                    if (Server.Options.StaticResurceCacheTime > 0)
                    {
                        response.Header.Add(HeaderTypeFactory.CACHE_CONTROL, "public, max-age=" + Server.Options.StaticResurceCacheTime);
                    }
                    NoModifyResult result = new();
                    response.Result(result);
                    return;
                }
            }
            if (!Debug)
            {
                if (!string.IsNullOrEmpty(fr.FileMD5))
                {
                    response.Header.Add(HeaderTypeFactory.ETAG, fr.FileMD5);
                    if (Server.Options.StaticResurceCacheTime > 0)
                    {
                        response.Header.Add(HeaderTypeFactory.CACHE_CONTROL, "public, max-age=" + Server.Options.StaticResurceCacheTime);
                    }
                }
            }
            EventFileResponseArgs efra = new();
            efra.Request = response.Request;
            efra.Response = response;
            efra.Resource = fr;
            FileResponse?.Invoke(this, efra);
            if (!efra.Cancel)
            {
                if (fr.GZIP)
                {
                    SetGZIP(response);
                }
                SetChunked(response);
                if (Server.EnableLog(EventArgs.LogType.Info))
                {
                    Server.BaseServer.Log(EventArgs.LogType.Info, null, $"HTTP {response.Request.ID} {response.Request.RemoteIPAddress} get {response.Request.BaseUrl} response gzip {fr.GZIP}");
                }
                HttpToken token = (HttpToken)response.Session.Tag;
                token.File = fr.CreateFileBlock();
                response.SetContentType(fct.ContentType);
                response.Result(token.File);
            }

        }

        public void OutputFile(FileResult result, HttpRequest request, HttpResponse response)
        {
            var file = result.File;
            if (!file.Contains(System.IO.Path.DirectorySeparatorChar))
            {
                var vfile = MatchVirtuslFolder(file);
                if (vfile == null)
                {
                    file = file.Replace('/', System.IO.Path.DirectorySeparatorChar);
                    if (file[0] != System.IO.Path.DirectorySeparatorChar)
                        file = System.IO.Path.DirectorySeparatorChar + file;
                    var basePath = Server.Options.StaticResourcePath;
                    if (basePath[^1] == System.IO.Path.DirectorySeparatorChar)
                    {
                        file = basePath + file[1..];
                    }
                    else
                    {
                        file = basePath + file;
                    }
                }
                else
                {
                    file = vfile;
                }
            }
            var resource = new NoCacheResource(file, "");
            string ext = System.IO.Path.GetExtension(file).Replace(".", "");
            var fileContentType = new FileContentType(ext);
            if (!string.IsNullOrEmpty(result.ContentType))
            {
                fileContentType.ContentType = result.ContentType;
            }
            resource.GZIP = result.GZip;
            EventFileResponseArgs efra = new();
            efra.Request = response.Request;
            efra.Response = response;
            efra.Resource = resource;
            efra.ContentType = fileContentType;
            FileResponse?.Invoke(this, efra);
            if (!efra.Cancel)
            {
                if (!File.Exists(file))
                {
                    NotFoundResult notFound = new("{0} file not found", request.Url);
                    response.Result(notFound);
                }
                else
                {
                    efra.Resource.Load();
                    if (efra.Resource.GZIP)
                    {
                        SetGZIP(response);
                    }
                    SetChunked(response);
                    if (Server.EnableLog(EventArgs.LogType.Info))
                    {
                        Server.BaseServer.Log(EventArgs.LogType.Info, null, $"HTTP {response.Request.ID} {response.Request.RemoteIPAddress} get {response.Request.BaseUrl} response gzip {efra.Resource.GZIP}");
                    }
                    HttpToken token = (HttpToken)response.Session.Tag;
                    token.File = efra.Resource.CreateFileBlock();
                    response.SetContentType(efra.ContentType.ContentType);
                    response.Result(token.File);
                }
            }
        }

        public string MatchVirtuslFolder(string file)
        {
            if (Server.Options.Virtuals != null)
                foreach (var item in Server.Options.Virtuals)
                {
                    var result = item.Match(file);
                    if (result != null)
                        return result;
                }
            return null;
        }
        public void ProcessFile(HttpRequest request, HttpResponse response)
        {
            FileContentType fct = null;
            FileResource fr = null;
            string url = request.BaseUrl;
            var result = MatchVirtuslFolder(request.BaseUrl);
            if (result != null)
            {
                FileResult fileResult = new(result);
                OutputFile(fileResult, request, response);
                return;
            }

            if (url[^1] == '/')
            {
                for (
                    int i = 0; i < mDefaultPages.Count; i++)
                {
                    string defaultpage = url + mDefaultPages[i];
                    string ext = HttpParse.GetBaseUrlExt(defaultpage);

                    if (!mExts.TryGetValue(ext, out fct))
                    {
                        continue;
                    }
                    fr = GetFileResource(defaultpage);
                    if (fr != null)
                    {
                        OutputFileResource(fct, fr, response);
                        return;
                    }
                }
                if (Server.EnableLog(EventArgs.LogType.Warring))
                    Server.BaseServer.Log(EventArgs.LogType.Warring, null, $"HTTP {request.ID} {request.RemoteIPAddress} get {request.Url} file not found");
                if (!Server.OnHttpRequesNotfound(request, response).Cancel)
                {
                    NotFoundResult notFound = new("{0} file not found", request.Url);
                    response.Result(notFound);
                }
                return;
            }
            if (ExtSupport(request.Ext))
            {  
                url = System.Net.WebUtility.UrlDecode(url);
                fct = mExts[request.Ext];
                fr = GetFileResource(url);
                if (!Server.Options.Debug && fr != null)
                {
                    OutputFileResource(fct, fr, response);
                }
                else
                {
                    string fileurl = HttpParse.GetBaseUrl(request.Url);
                    fileurl = System.Net.WebUtility.UrlDecode(fileurl);
                    if (ExistsFile(request, fileurl, out string file))
                    {
                        fr = CreateResource(file, false);
                        if (fr != null)
                        {

                            OutputFileResource(fct, fr, response);
                        }
                    }
                    else
                    {
                        if (Server.EnableLog(EventArgs.LogType.Warring))
                            Server.BaseServer.Log(EventArgs.LogType.Warring, null, $"HTTP {request.ID} {request.RemoteIPAddress} get {request.Url} file not found");
                        if (!Server.OnHttpRequesNotfound(request, response).Cancel)
                        {
                            NotFoundResult notFound = new("{0} file not found", request.Url);
                            response.Result(notFound);
                        }

                    }
                }
            }
            else
            {

                if (Server.EnableLog(EventArgs.LogType.Warring))
                    Server.BaseServer.Log(EventArgs.LogType.Warring, null, $"HTTP {request.ID} {request.RemoteIPAddress} get { request.BaseUrl} file ext not support");
                NotSupportResult notSupport = new("get {0} file {1} ext not support", request.Url, request.Ext);
                response.Result(notSupport);
            }
        }

        private static void SetGZIP(HttpResponse response)
        {
            response.Header.Add("Content-Encoding", "gzip");
        }

        private static void SetChunked(HttpResponse response)
        {

            response.Header.Add("Transfer-Encoding", "chunked");

        }

        public bool ExtSupport(string ext)
        {

            return mExts.ContainsKey(ext);
        }

        public FileResource GetFileResource(string url)
        {
            mResources.TryGetValue(url, out FileResource result);
            return result;
        }

        public bool ExistsFile(HttpRequest request, string url, out string file)
        {

            file = GetFile(request, url);
            bool has = File.Exists(file);
            return has;
        }

        public string GetFile(HttpRequest request, string url)
        {
            EventFindFileArgs e = new();
            e.Request = request;
            e.Url = url;
            Find?.Invoke(this, e);
            if (string.IsNullOrEmpty(e.File))
            {
                if (Path[^1] == System.IO.Path.DirectorySeparatorChar)
                {
                    return Path + url[1..];
                }
                else
                {
                    return Path + url.Replace('/', System.IO.Path.DirectorySeparatorChar);
                }
            }
            return e.File;
        }

        public string GetUrl(string file)
        {
            ReadOnlySpan<char> filebuffer = file.AsSpan()[Path.Length..file.Length];
            char[] charbuffer = HttpParse.GetCharBuffer();
            int offset = 0;
            if (filebuffer[0] != System.IO.Path.DirectorySeparatorChar)
            {
                offset += 1;
                charbuffer[0] = '/';
            }
            for (int i = 0; i < filebuffer.Length; i++)
            {
                if (filebuffer[i] == '\\')
                {
                    charbuffer[i + offset] = '/';
                }
                else
                {
                    //if (Server.Options.UrlIgnoreCase)
                    //    charbuffer[i + offset] = Char.ToLower(filebuffer[i]);
                    //else
                    charbuffer[i + offset] = filebuffer[i];
                }
            }
            return new string(charbuffer, 0, filebuffer.Length + offset);
        }

        private FileResource CreateResource(string file, bool cache)
        {
            try
            {
                string ext = System.IO.Path.GetExtension(file).ToLower();
                ext = ext[1..];
                if (mExts.ContainsKey(ext))
                {
                    FileResource fr;
                    string urlname = "";
                    urlname = GetUrl(file);
                    if (cache)
                    {

                        if (!Debug)
                        {
                            if (mResources.TryGetValue(urlname, out fr))
                            {
                                if (Server.BaseServer.GetRunTime() - fr.CreateTime < 2000)
                                    return fr;
                            }
                        }
                    }
                    bool nogzip = !(Server.Options.NoGzipFiles.Contains(ext, StringComparison.CurrentCulture));
                    bool cachefile = Server.Options.CacheFiles.Contains(ext, StringComparison.CurrentCulture);
                    if (Debug)
                    {
                        fr = new NoCacheResource(file, urlname);
                        if (nogzip)
                            fr.GZIP = true;
                    }
                    else
                    {
                        FileInfo info = new(file);
                        if (cachefile && info.Length < 1024 * Server.Options.CacheFileSize)
                        {
                            fr = new FileResource(file, urlname);
                        }
                        else
                        {
                            fr = new NoCacheResource(file, urlname);
                            if (nogzip)
                                fr.GZIP = true;
                        }
                    }

                    fr.Load();
                    fr.CreateTime = Server.BaseServer.GetRunTime();
                    if (cache && mResources.Count < 5000)
                    {
                        mResources[urlname] = fr;
                    }
                    if (Server.EnableLog(EventArgs.LogType.Info))
                        Server.BaseServer.Log(EventArgs.LogType.Info, null, "update {0} static resource success", urlname);
                    return fr;
                }
            }
            catch (Exception e_)
            {
                Server.BaseServer.Error(e_, null, "update {0} resource error {1}", file, e_.Message);
            }
            return null;
        }

        private void LoadFolder(string path)
        {
            if (path[^1] != System.IO.Path.DirectorySeparatorChar)
            {
                path += System.IO.Path.DirectorySeparatorChar;
            }
            foreach (string file in Directory.GetFiles(path))
            {
                CreateResource(file, true);
            }
            foreach (string folder in Directory.GetDirectories(path))
            {
                string vfolder = folder.Replace(Server.Options.StaticResourcePath, "")
                    .Replace(System.IO.Path.DirectorySeparatorChar.ToString(), @"\");
                if (Server.Options.NotLoadFolder.IndexOf(vfolder, 0, StringComparison.CurrentCultureIgnoreCase) >= 0)
                    continue;
                LoadFolder(folder);
            }
        }

    }
}
