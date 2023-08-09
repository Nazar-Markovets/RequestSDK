using System.Net.Http.Headers;

namespace RequestSDK.Services;

public partial class RequestService
{
    public sealed class RequestServiceOptions
    {
        public IHttpClientFactory? Factory { get; init; }
        public IEnumerable<HttpClientSettings>? HttpClientSettings { get; init; }
        public Type? AccemblyRoutingType { get; init; }
        public Func<AuthenticationHeaderValue>? Authentication { get; init; }

        public RequestServiceOptions(){}
        public RequestServiceOptions(IHttpClientFactory httpClientFactory, params HttpClientSettings[] settings)
        {
            Factory = httpClientFactory;
            HttpClientSettings = settings;
        }
    }
}