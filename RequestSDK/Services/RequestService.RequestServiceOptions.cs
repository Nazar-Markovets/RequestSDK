using System.Net.Http.Headers;

namespace RequestSDK.Services;

public partial class RequestService
{
    public class RequestServiceOptions
    {
        internal readonly IHttpClientFactory? _factory;
        internal readonly IEnumerable<HttpClientSettings>? _clients;
        internal readonly Type? _accemblyRoutingType;
        internal readonly Func<AuthenticationHeaderValue>? _authentication;

        public RequestServiceOptions(IHttpClientFactory? httpClientFactory, IEnumerable<HttpClientSettings> factoryClients)
        {
            _factory = httpClientFactory;
            _clients = factoryClients;
            _accemblyRoutingType = default!;
            _authentication = default!;
        }

        public RequestServiceOptions(Func<AuthenticationHeaderValue>? authenticationHeader) : this(default, default!, default!, authenticationHeader){}

        public RequestServiceOptions(IHttpClientFactory? httpClientFactory, IEnumerable<HttpClientSettings> factoryClients, Type accemblyRoutingContaner) : this(httpClientFactory, factoryClients) => _accemblyRoutingType = accemblyRoutingContaner;

        public RequestServiceOptions(IHttpClientFactory? httpClientFactory, IEnumerable<HttpClientSettings> factoryClients, Type accemblyRoutingContaner, Func<AuthenticationHeaderValue>? authenticationHeader = default) : this(httpClientFactory, factoryClients, accemblyRoutingContaner) => _authentication = authenticationHeader;
    }
}