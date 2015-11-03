using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace WebExecutor
{
    public sealed class WebSettings
    {
        public string UserAgent { get; set; }

        public CookieContainer Cookies { get; set; }

        public WebSettings()
        {
            UserAgent = null;
            Cookies = null;
        }

        public void ResetCookies()
        {
            Cookies = new CookieContainer();
        }
    }
}
