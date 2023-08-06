using Newtonsoft.Json;

namespace WebFlow.Models
{
    public class Browser
    {
        [JsonProperty(PropertyName = "browserName")]
        public string DisplayName { get; set; }

        [JsonProperty(PropertyName = "browserPath")]
        public string BrowserPath { get; set; }

        [JsonProperty(PropertyName = "argument")]
        public string Argument { get; set; }
    }
}