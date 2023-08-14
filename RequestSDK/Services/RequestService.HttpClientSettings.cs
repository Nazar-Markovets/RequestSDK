using System.Net.Http.Headers;

namespace RequestSDK.Services;

public partial class RequestService
{
    public class HttpClientSettings
    {
        public byte? HttpClientId { get; set; }
        public string HttpClientName { get; set; } = default!;
        public Uri BaseAddress { get; set; } = default!;
        public byte? RetryCount { get; set; } = 0;
        public int? MilliSecondsSleep { get; set; } = 300;
        public Func<Schemes.AuthenticationSchemes, AuthenticationHeaderValue>? Authentication { get; set; }
        public Type? ClientRoutingType { get; set; }
    }

    public class HttpClientSettings<T> : HttpClientSettings where T : class
    {
        public HttpClientSettings() : base() { ClientRoutingType = typeof(T); }
        
    }
}