using BarRaider.SdTools;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebFlow.Models;

namespace WebFlow.Helpers
{
    public class Win_WebBrowser
    {
        private static Win_WebBrowser instance = null;
        private static readonly object objLock = new object();
        private static readonly Dictionary<string, string> BrowserKeywords = new Dictionary<string, string> {{ "chrome", "--incognito" }, { "firefox", "-private-window" }, { "edge", "--inprivate" }, { "opera", "-private" }, { "iexplore", "-private" }, { "safari", "-private" }}; 
        private static List<Browser> listBrowsers = new List<Browser>();
        private static readonly object lockBrowsers = new object();

        public static Win_WebBrowser Instance
        {
            get
            {
                if (instance != null)
                {
                    return instance;
                }

                lock (objLock)
                {
                    if (instance == null)
                    {
                        instance = new Win_WebBrowser();
                    }
                    return instance;
                }
            }
        }

        public async Task<List<Browser>> GetAllBrowsers()
        {
            if (listBrowsers.Count == 0)
            {
                await GetInstalledBrowsers();
            }

            return listBrowsers.OrderBy(x => x.DisplayName).ToList();
        }

        private async Task GetInstalledBrowsers()
        {
            const string keyPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths";

            try
            {
                lock (lockBrowsers)
                {
                    using (RegistryKey key = Registry.LocalMachine.OpenSubKey(keyPath))
                    {
                        if (key != null)
                        {
                            foreach (string subKeyName in key.GetSubKeyNames())
                            {
                                using (RegistryKey subKey = key.OpenSubKey(subKeyName))
                                {
                                    string browserPath = subKey.GetValue(null) as string;
                                    if (!string.IsNullOrEmpty(browserPath) && BrowserKeywords.Any(keyword => browserPath.Contains(keyword.Key)))
                                    {
                                        listBrowsers.Add(new Browser() { DisplayName = subKeyName, BrowserPath = browserPath, Argument = BrowserKeywords.FirstOrDefault(keyword => browserPath.Contains(keyword.Key)).Value });
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.LogMessage(TracingLevel.ERROR, $"GetInstalledBrowsers Exception: {ex}");
            }
        }
    }
}
