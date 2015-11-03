using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net;
using HtmlAgilityPack;
using System.Text.RegularExpressions;
using NLua;
using System.Reflection;
using System.Threading.Tasks;

namespace WebExecutor
{
    public sealed class ScriptExecutor : IDisposable
    {
        static readonly string[][] UserAgents = new[]
        {
            new[] { "ChromeWindows7",  // Google Chrome 4.0.249.89 под Windows 7
                "Mozilla/5.0 (Windows NT 6.1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/41.0.2228.0 Safari/537.36" },
        };

        TaskScheduler uiScheduler;
        IDownloadManager downloadManager;
        TextWriter debugWriter;

        Lua lua;
        TaskCompletionSource<bool> completitionSource;
        Thread thread;
        bool disposed = false;

        WebSettings webSettings = new WebSettings();

        public bool IsRunning
        {
            get { return completitionSource != null && !completitionSource.Task.IsCompleted; }
        }

        public ScriptExecutor(TaskScheduler uiScheduler, IDownloadManager downloadManager, TextWriter debugWriter)
        {
            this.uiScheduler = uiScheduler;
            this.downloadManager = downloadManager;
            this.debugWriter = debugWriter;
            
            lua = new Lua();
            InitializeInterpreter();
        }

        [LuaFunction]
        public void Write(string text)
        {
            if (disposed)
                return;

            debugWriter.Write(text);
        }

        [LuaFunction]
        public void WriteLine(string text)
        {
            if (disposed)
                return;

            debugWriter.WriteLine(text);
        }

        [LuaFunction]
        public Regex Regex(string regex, bool ignoreCase)
        {
            return new Regex(
                regex, ignoreCase ? RegexOptions.IgnoreCase : RegexOptions.None);
        }

        [LuaFunction]
        public void Sleep(int milliseconds)
        {
            Thread.Sleep(milliseconds);
        }

        [LuaFunction]
        public string UrlEncode(string badUrl)
        {
            return Uri.EscapeDataString(badUrl);
        }

        [LuaFunction]
        public string UrlDecode(string encodedUrl)
        {
            return Uri.UnescapeDataString(encodedUrl);
        }

        [LuaFunction]
        public string HtmlEncode(string data)
        {
            return System.Web.HttpUtility.HtmlEncode(data);
        }

        [LuaFunction]
        public string HtmlDecode(string data)
        {
            return System.Web.HttpUtility.HtmlDecode(data);
        }

        [LuaFunction]
        public HtmlDocument LoadDocument(string url)
        {
            if (disposed)
                return null;

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.UserAgent = webSettings.UserAgent;
            request.CookieContainer = webSettings.Cookies;
            
            HtmlDocument document = new HtmlDocument();
            using (var response = (HttpWebResponse)request.GetResponse())
            using (var stream = response.GetResponseStream())
            {
                document.Load(stream);
            }

            return document;
        }

        [LuaFunction]
        public void DownloadFile(string url, string fileName)
        {
            Uri resource = null;
            try
            {
                resource = new Uri(url, UriKind.Absolute);
            }
            catch (UriFormatException)
            {
                throw;
            }

            Task.Factory.StartNew(state => downloadManager.AddDownload(resource, fileName),
                null, CancellationToken.None, TaskCreationOptions.None, uiScheduler);
        }

        [LuaFunction]
        public int ActiveDownloadsCount()
        {
            var downloadsCount = Task.Factory.StartNew(state => downloadManager.Downloads.Count(
                    d => d.DownloadState == DownloadState.Downloading),
                null, CancellationToken.None, TaskCreationOptions.None, uiScheduler);
            return downloadsCount.Result;
        }

        [LuaFunction]
        public string FormatInt32(int value, string format)
        {
            return value.ToString(format);
        }

        [LuaFunction]
        public LuaTable ToTable(System.Collections.IEnumerable collection)
        {
            return lua.CreateTable(collection);
        }

        private void InitializeInterpreter()
        {
            lua.LoadCLRPackage();
            var luaFunctions = from method in typeof(ScriptExecutor).GetMethods(
                                    BindingFlags.Public | BindingFlags.Instance)
                               let attr = method.GetCustomAttribute<LuaFunctionAttribute>()
                               where attr != null
                               select method;

            foreach (var method in luaFunctions)
            {
                lua.RegisterFunction(this, method);
            }

            lua["web"] = webSettings;

            lua.NewTable("UserAgent");
            foreach (string[] userAgent in UserAgents)
            {
                string path = "UserAgent." + userAgent[0];
                lua[path] = userAgent[1];
            }
        }

        public Task Run(string luaScript, CancellationToken cancel = default(CancellationToken))
        {
            if (disposed)
                return null;
            if (completitionSource != null && !completitionSource.Task.IsCompleted)
                throw new InvalidOperationException("Cannot start script if is it still running");

            completitionSource = new TaskCompletionSource<bool>();
            thread = new Thread(() =>
            {
                if (cancel.IsCancellationRequested)
                {
                    completitionSource.TrySetCanceled();
                    return;
                }

                try
                {
                    lua.DoString(luaScript);
                    completitionSource.TrySetResult(true);
                }
                catch (Exception ex)
                {
                    completitionSource.TrySetException(ex);
                }
            });
            thread.Start();

            cancel.Register(() =>
            {
                if (IsRunning)
                {
                    lua.Dispose();
                    completitionSource.TrySetCanceled();
                }
            });

            return completitionSource.Task;
        }

        public void Dispose()
        {
            if (disposed)
                return;

            lua.Dispose();
            lua = null;
            disposed = true;
        }
    }
}
