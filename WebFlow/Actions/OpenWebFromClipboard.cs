using BarRaider.SdTools;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using WebFlow.Helpers;
using WebFlow.Models;

namespace WebFlow.Actions
{
    [PluginActionId("com.quentin-su.webflow.openwebfromclipboard")]
    public class OpenWebFromClipboard : KeypadBase
    {
        private class PluginSettings
        {
            public static PluginSettings CreateDefaultSettings()
            {
                PluginSettings instance = new PluginSettings
                {
                    Url = string.Empty,
                    Browsers = null,
                    InPrivate = false,
                    InBackground = true,
                    BrowserPath = string.Empty
                };
                return instance;
            }

            [JsonProperty(PropertyName = "url")]
            public string Url { get; set; }

            [JsonProperty(PropertyName = "browsers")]
            public List<Browser> Browsers { get; set; }

            [JsonProperty(PropertyName = "inPrivate")]
            public bool InPrivate { get; set; }

            [JsonProperty(PropertyName = "inBackground")]
            public bool InBackground { get; set; }

            [JsonProperty(PropertyName = "browserPath")]
            public string BrowserPath { get; set; }
        }

        private readonly PluginSettings settings;

        public OpenWebFromClipboard(SDConnection connection, InitialPayload payload) : base(connection, payload)
        {
            if (payload.Settings == null || payload.Settings.Count == 0)
            {
                settings = PluginSettings.CreateDefaultSettings();
                Connection.SetSettingsAsync(JObject.FromObject(settings));
            }
            else
            {
                settings = payload.Settings.ToObject<PluginSettings>();
            }
            settings.Browsers = Win_WebBrowser.Instance.GetAllBrowsers().GetAwaiter().GetResult();
            SaveSettingsAsync();
        }

        public override void Dispose()
        {
            Logger.Instance.LogMessage(TracingLevel.INFO, "Destructor called");
        }

        public override void ReceivedSettings(ReceivedSettingsPayload payload)
        {
            Tools.AutoPopulateSettings(settings, payload.Settings);
            SaveSettingsAsync();
        }

        public override void ReceivedGlobalSettings(ReceivedGlobalSettingsPayload payload) { }

        public override void KeyPressed(KeyPayload payload)
        {
            Logger.Instance.LogMessage(TracingLevel.INFO, "Key Pressed");
            OpenNewTabFromClipboard();
        }

        public override void KeyReleased(KeyPayload payload) { }

        public override void OnTick() { }

        private async void SaveSettingsAsync()
        {
            await Connection.SetSettingsAsync(JObject.FromObject(settings));
        }

        private void OpenNewTabFromClipboard()
        {
            // This method has two distinct inputs: copy the last element or the currently selected element.
            try
            {
                System.Threading.Thread thread = new System.Threading.Thread(() =>
                {
                    string lastClipboardItem = Clipboard.GetText();
                    Logger.Instance.LogMessage(TracingLevel.DEBUG, $"OpenNewTabFromClipboard lastClipboardItem: {lastClipboardItem}");

                    SendKeys.SendWait("^c");
                    System.Threading.Thread.Sleep(100);

                    string clipboardItem = Clipboard.GetText();
                    Logger.Instance.LogMessage(TracingLevel.DEBUG, $"OpenNewTabFromClipboard clipboardItem: {clipboardItem}");

                    if (!string.IsNullOrEmpty(clipboardItem)) settings.Url = clipboardItem;
                    else if (!string.IsNullOrEmpty(lastClipboardItem)) settings.Url = lastClipboardItem;
                    else settings.Url = string.Empty;

                    Logger.Instance.LogMessage(TracingLevel.DEBUG, $"OpenNewTabFromClipboard settings.url: {settings.Url}");

                    Process.Start(new ProcessStartInfo
                    {
                        FileName = settings.BrowserPath,
                        UseShellExecute = true,
                        WindowStyle = settings.InBackground ? ProcessWindowStyle.Hidden : ProcessWindowStyle.Maximized,
                        Arguments = $"{settings.Url} {(settings.InPrivate ? settings.Browsers.FirstOrDefault(x => x.BrowserPath == settings.BrowserPath).Argument : string.Empty)}"
                    });
                });

                thread.SetApartmentState(System.Threading.ApartmentState.STA); // Set the thread apartment state to STA (Single Threaded Apartment) for clipboard access
                thread.Start();
            }
            catch (Exception ex)
            {
                Logger.Instance.LogMessage(TracingLevel.ERROR, $"OpenNewTabFromClipboard Exception: {ex}");
            }
        }

    }
}
