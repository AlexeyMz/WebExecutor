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
        IDebugConsole debugConsole;

        Lua lua;
        TaskCompletionSource<bool> completitionSource;
        Thread thread;
        bool disposed = false;

        WebSettings webSettings = new WebSettings();

        public bool IsRunning
        {
            get { return completitionSource != null && !completitionSource.Task.IsCompleted; }
        }

        public ScriptExecutor(TaskScheduler uiScheduler, IDownloadManager downloadManager, IDebugConsole debugConsole)
        {
            this.uiScheduler = uiScheduler;
            this.downloadManager = downloadManager;
            this.debugConsole = debugConsole;
            
            lua = new Lua();
            InitializeInterpreter();
        }

        private static Task RunUsingScheduler(TaskScheduler taskScheduler, Action action)
        {
            return Task.Factory.StartNew(
                state => action(), null, CancellationToken.None,
                TaskCreationOptions.DenyChildAttach, taskScheduler);
        }

        private static Task<T> RunUsingScheduler<T>(TaskScheduler taskScheduler, Func<T> func)
        {
            return Task.Factory.StartNew(
                state => func(), null, CancellationToken.None,
                TaskCreationOptions.None, taskScheduler);
        }

        [LuaFunction]
        public void WriteLine(string text, params object[] args)
        {
            if (text == null) { throw new ArgumentNullException(nameof(text)); }
            if (disposed) { return; }
            RunUsingScheduler(uiScheduler, () =>
                debugConsole.AppendMessage(MessageKind.Debug, string.Format(text, args))).Wait();
        }

        [LuaFunction("print")]
        public void Print(params object[] parts)
        {
            if (parts == null) { throw new ArgumentNullException(nameof(parts)); }
            if (disposed) { return; }
            RunUsingScheduler(uiScheduler, () =>
                debugConsole.AppendMessage(MessageKind.Debug, string.Concat(parts))).Wait();
        }

        [LuaFunction]
        public Regex Regex(string regex, bool ignoreCase)
        {
            if (regex == null) { throw new ArgumentNullException(nameof(regex)); }
            return new Regex(regex, ignoreCase ? RegexOptions.IgnoreCase : RegexOptions.None);
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
            if (url == null) { throw new ArgumentNullException(nameof(url)); }
            if (disposed) { return null; }

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
            if (url == null) { throw new ArgumentNullException(nameof(url)); }
            if (fileName == null) { throw new ArgumentNullException(nameof(fileName)); }
            Uri resource = new Uri(url, UriKind.Absolute);
            RunUsingScheduler(uiScheduler, () => downloadManager.AddDownload(resource, fileName)).Wait();
        }

        [LuaFunction]
        public int ActiveDownloadsCount()
        {
            var downloadsCount = RunUsingScheduler(uiScheduler, () => downloadManager.Downloads.Count(
                d => d.DownloadState == DownloadState.Downloading));
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
