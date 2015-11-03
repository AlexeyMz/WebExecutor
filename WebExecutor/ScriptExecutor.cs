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

namespace WebExecutor
{
    public sealed class ScriptExecutor : IDisposable
    {
        static readonly string[][] UserAgents = new[]
        {
            new[] { "ChromeWindows7",  // Google Chrome 4.0.249.89 под Windows 7
                "Mozilla/5.0 (Windows; U; Windows NT 6.1; en-US) AppleWebKit/532.5 " + 
                "(KHTML, like Gecko) Chrome/4.0.249.89 Safari/532.5" },
        };

        public bool IsRunning { get; private set; }

        bool disposed = false;
        
        Lua lua;
        Thread thread;

        IDownloadManager downloadManager;
        TextWriter debugWriter;

        WebSettings webSettings = new WebSettings();

        public event EventHandler<CompletedEventArgs> Completed;

        public ScriptExecutor(IDownloadManager downloadManager, TextWriter debugWriter)
        {
            this.downloadManager = downloadManager;
            this.debugWriter = debugWriter;
            
            lua = new Lua();
            InitializeInterpreter();

            IsRunning = false;
        }

        private void OnCompleted(Exception error)
        {
            var temp = Completed;

            if (temp != null)
            {
                temp(this, new CompletedEventArgs(error));
            }
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

            downloadManager.AddDownload(resource, fileName);
        }

        [LuaFunction]
        public int ActiveDownloadsCount()
        {
            return downloadManager.Downloads.Count(
                d => d.DownloadState == DownloadState.Downloading);
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

        public void Run(string luaScript)
        {
            if (disposed)
                return;

            IsRunning = true;

            thread = new Thread(() =>
            {
                Exception error = null;
                try
                {
                    lua.DoString(luaScript);
                }
                catch (Exception ex)
                {
                    error = ex;
                }

                IsRunning = false;
                OnCompleted(error);
            });

            thread.Start();
        }

        public void Stop()
        {
            if (disposed)
                return;

            if (IsRunning)
            {
                thread.Abort();
                IsRunning = false;
                OnCompleted(null);
            }
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
