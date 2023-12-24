using Newtonsoft.Json;
using System.ComponentModel;


namespace KTX_DAL.Models.ResponseModel
{
    public class DefaultResponseContext
    {
        public DefaultResponseContext()
        {
            Message = "";
            Data = null;
            Count = -999;  // Khi không gán giá trị return thì mặc định là -999
            Pages = -999;  
            CountAll = -999;  
            Summary = null;
        }

        public string Message { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public object Data { get; set; }
        //Json sẽ bỏ qua những biến có giá trị = -999
        [DefaultValue(-999)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int Count { get; set; }
        [DefaultValue(-999)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int Pages { get; set; }

        [DefaultValue(-999)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int CountAll { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public object Summary { get; set; }

    }
}
