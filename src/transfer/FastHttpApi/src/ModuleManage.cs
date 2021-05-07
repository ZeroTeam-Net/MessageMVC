using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.IO.Compression;
using System.Reflection;
using System.IO;

namespace BeetleX.FastHttpApi
{
    public class ModuleManager
    {

        public ModuleManager(HttpApiServer server)
        {
            Server = server;
            mPath = Directory.GetCurrentDirectory();
            mPath += Path.DirectorySeparatorChar + "_models" + Path.DirectorySeparatorChar;
            if (!Directory.Exists(mPath))
            {
                Directory.CreateDirectory(mPath);
            }
            mRunningPath = Directory.GetCurrentDirectory();
            mRunningPath += Path.DirectorySeparatorChar + "_runing_models" + Path.DirectorySeparatorChar;
            if (!Directory.Exists(mRunningPath))
            {
                Directory.CreateDirectory(mRunningPath);
            }

            fileSystemWatcher = new FileSystemWatcher(mPath, "*.zip");
            fileSystemWatcher.IncludeSubdirectories = false;
            fileSystemWatcher.Changed += OnFileWatchHandler;
            fileSystemWatcher.Created += OnFileWatchHandler;
            fileSystemWatcher.Renamed += OnFileWatchHandler;
            fileSystemWatcher.EnableRaisingEvents = true;
            mUpdateTimer = new System.Threading.Timer(OnUpdateHandler, null, 5000, 5000);
        }

        private readonly System.Threading.Timer mUpdateTimer;

        private int mUpdateCount = 0;

        private readonly System.Collections.Concurrent.ConcurrentDictionary<string, UpdateItem> mUpdateItems = new();

        class UpdateItem
        {
            public string Name { get; set; }

            public string FullName { get; set; }

            public string Module { get; set; }

            public long Time { get; set; }

        }

        private void OnUpdateHandler(object state)
        {
            mUpdateTimer.Change(-1, -1);
            try
            {
                var modules = mUpdateItems.Keys;
                foreach (string module in modules)
                {
                    if (mUpdateItems.TryGetValue(module, out UpdateItem item))
                    {
                        if (Server.BaseServer.GetRunTime() - item.Time > 10 * 1000)
                        {
                            try
                            {
                                Load(item.Module);
                                Server.Log(EventArgs.LogType.Info, $"{item.Module} upgrades success");
                            }
                            catch (Exception e_)
                            {
                                Server.Log(EventArgs.LogType.Error, $"{item.Module} upgrades error {e_.Message} {e_.StackTrace}");
                            }
                            finally
                            {
                                mUpdateItems.TryRemove(module, out item);
                            }
                        }
                    }
                }
            }
            catch (Exception e_)
            {
                Server.Log(EventArgs.LogType.Error, $"upgrades module error {e_.Message} {e_.StackTrace}");
            }
            finally
            {
                mUpdateTimer.Change(5000, 5000);
            }
        }

        private void OnFileWatchHandler(object sender, FileSystemEventArgs e)
        {
            if (mUpdateItems.TryGetValue(e.Name, out UpdateItem item))
            {
                item.Time = Server.BaseServer.GetRunTime();
            }
            else
            {
                item = new UpdateItem();
                item.Name = e.Name;
                item.Module = Path.GetFileNameWithoutExtension(e.Name);
                item.FullName = e.FullPath;
                item.Time = Server.BaseServer.GetRunTime();
                mUpdateItems[e.Name] = item;
                Server.Log(EventArgs.LogType.Info, $"upload {e.Name} module");
            }
        }

        private readonly FileSystemWatcher fileSystemWatcher;

        private readonly string mRunningPath;

        private readonly string mPath;

        public HttpApiServer Server { get; set; }

        public IEnumerable<string> List()
        {
            var result = from a in Directory.GetFiles(mPath) select Path.GetFileNameWithoutExtension(a);
            return result;
        }

        private void ClearFiles()
        {
            try
            {
                foreach (var folder in Directory.GetDirectories(mRunningPath))
                {
                    Directory.Delete(folder, true);
                }
            }
            catch (Exception e_)
            {
                Server.Log(EventArgs.LogType.Error, $"clear files error {e_.Message}");
            }
        }

        public void Load()
        {
            ClearFiles();
            var items = List();
            if (items != null)
                foreach (var item in items)
                {
                    Load(item);
                }
        }

        private void OnLoadAssembly(IList<string> files, int count)
        {
            List<string> success = new();
            foreach (string file in files)
            {
                string aname = Path.GetFileName(file);
                try
                {
                    Assembly assembly = Assembly.LoadFile(file);
                    Server.ResourceCenter.LoadManifestResource(assembly);
                    Server.ActionFactory.Register(assembly);
                    Server.Log(EventArgs.LogType.Info, $"loaded {aname} assembly success");
                    OnAssemblyLoding(new EventAssemblyLoadingArgs(assembly));
                    success.Add(file);

                }
                catch (Exception e_)
                {
                    Server.Log(EventArgs.LogType.Error, $"load {aname} assembly error {e_.Message} {e_.StackTrace}");
                }
            }
        }

        public bool SaveFile(string name, string md5, bool eof, byte[] data)
        {
            string file = mPath + name + ".tmp";
            using (Stream stream = File.Open(file, FileMode.Append))
            {
                stream.Write(data, 0, data.Length);
                stream.Flush();
            }
            if (eof)
            {
                if (Utils.GetFileMD5(file) != md5)
                {
                    File.Delete(file);
                    throw new Exception("Verify file md5 value error!");

                }
                else
                {
                    string targetFile = mPath + name + ".zip";
                    if (File.Exists(targetFile))
                        File.Delete(targetFile);
                    File.Move(file, targetFile);
                    return true;
                }
            }
            return false;
        }

        public void Load(string module)
        {
            try
            {
                mUpdateCount++;
                if (mUpdateCount >= 1000)
                    mUpdateCount = 0;
                Server.Log(EventArgs.LogType.Info, $"loding {module} module ...");
                string zipfile = mPath + module + ".zip";
                string target = mRunningPath + module + mUpdateCount.ToString("000") + Path.DirectorySeparatorChar;
                if (Directory.Exists(target))
                {
                    Directory.Delete(target, true);
                }
                if (File.Exists(zipfile))
                {
                    if (!Directory.Exists(target))
                        Directory.CreateDirectory(target);
                    ZipFile.ExtractToDirectory(zipfile, target, true);
                    string beetledll = Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + "BeetleX.dll";
                    string fastApidll = Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + "BeetleX.FastHttpApi.dll";
                    File.Copy(beetledll, target + "BeetleX.dll", true);
                    File.Copy(fastApidll, target + "BeetleX.FastHttpApi.dll", true);
                    List<string> files = new();
                    foreach (string assemblyFile in Directory.GetFiles(target, "*.dll"))
                    {
                        files.Add(assemblyFile);
                    }
                    OnLoadAssembly(files, 0);
                    Server.Log(EventArgs.LogType.Info, $"loaded {module} module success");
                }
                else
                {
                    Server.Log(EventArgs.LogType.Warring, $"{module} not found!");
                }
            }
            catch (Exception e_)
            {
                Server.Log(EventArgs.LogType.Error, $"load {module} error {e_.Message} {e_.StackTrace}");
            }
        }


        public event EventHandler<EventAssemblyLoadingArgs> AssemblyLoding;

        protected virtual void OnAssemblyLoding(EventAssemblyLoadingArgs e)
        {
            AssemblyLoding?.Invoke(this, e);
        }

        public class EventAssemblyLoadingArgs : System.EventArgs
        {
            public EventAssemblyLoadingArgs(Assembly assembly)
            {
                Assembly = assembly;
            }
            public Assembly Assembly { get; private set; }
        }
    }
}
