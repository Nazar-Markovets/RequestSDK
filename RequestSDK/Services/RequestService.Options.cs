using System.Net.Http.Headers;
using System.Net.Mime;
using System.Xml.Linq;
using System.Linq;

namespace RequestSDK.Services;

public partial class RequestService
{
    public class Options
    {
        #region Private Fields
        
        private HttpMethod? _httpMethod;
        private string _requestPath;
        private string _requestContentType;
        private byte? _httpClientId;
        private KeyValuePair<string, string?>[] _requestParameters;
        private List<MediaTypeWithQualityHeaderValue> _requestAcceptTypes;
        private Dictionary<string, string?>? _requestHeaders;
        private Dictionary<string, object?>? _requestCustomActionFlags;
        private AuthenticationHeaderValue? _authenticationHeader;
        private HttpCompletionOption _requestCompletionOptions = HttpCompletionOption.ResponseContentRead;
        private bool _useRecognizableRouting = false;
        #endregion Private Fields

        #region Readonly Properties
        internal HttpMethod? HttpMethod => _httpMethod;
        internal string Path => _requestPath;
        internal byte? HttpClientId => _httpClientId;
        internal KeyValuePair<string, string?>[] RequestParameters => _requestParameters;
        internal List<MediaTypeWithQualityHeaderValue> AcceptTypes => _requestAcceptTypes;

        internal Dictionary<string, string?>? CustomHeaders => _requestHeaders;
        internal AuthenticationHeaderValue? Authentication => _authenticationHeader;
        internal HttpCompletionOption CompletionOption => _requestCompletionOptions;
        internal string ContentType => _requestContentType;
        internal Dictionary<string, object?>? CustomActionFlags => _requestCustomActionFlags;

        /// <summary>
        /// Using autorouting based on <see cref="Attributes.ControllerNameAttribute"/>, and <see cref="Attributes.ControllerHttpMethodAttribute"/> metadata
        /// </summary>
        internal bool UseRecognizableRouting => _useRecognizableRouting;

        #endregion Readonly Properties

        private Options()
        {
            _requestCompletionOptions = HttpCompletionOption.ResponseContentRead;
            _requestContentType = MediaTypeNames.Application.Json;
            _requestAcceptTypes = new List<MediaTypeWithQualityHeaderValue>() { new MediaTypeWithQualityHeaderValue("*/*") };
            _requestParameters = Array.Empty<KeyValuePair<string, string?>>();
            _requestPath = string.Empty;
        }

        private Options(HttpMethod httpMethod, string requestPath, byte? registeredHttpClientId = default) : this()
        {
            _httpMethod = httpMethod;
            _requestPath = requestPath;
            _httpClientId = registeredHttpClientId;
        }

        public static Options WithoutRegisteredClient(HttpMethod httpMethod, string requestPath) => new(httpMethod, requestPath, null);
        public static Options WithRegisteredClient(HttpMethod httpMethod, string requestPath, byte clientId) => new (httpMethod, requestPath, clientId);
        
        public static Options WithRegisteredClient<TRoutingContainer>(HttpMethod httpMethod, string requestPath, TRoutingContainer clientId) where TRoutingContainer : Enum
        {
            return new(httpMethod, requestPath, Convert.ToByte(clientId));
        }

        public static Options WithRegisteredRouting(string requestPath, byte? clientId = default)
        {
            return new() { _requestPath = requestPath, _httpClientId = clientId, _useRecognizableRouting = true };
        }

        public static Options WithRegisteredRouting<TRoutingContainer>(string requestPath, TRoutingContainer clientId) where TRoutingContainer : Enum
        {
            return new() { _requestPath = requestPath, _httpClientId = Convert.ToByte(clientId), _useRecognizableRouting = true };
        }

        /// <summary>
        /// Use <see cref="MediaTypeNames"/> to set correct values as well
        /// </summary>
        public Options AddAcceptTypes(params string[] types)
        {
            _requestAcceptTypes = new HashSet<string>(types, StringComparer.OrdinalIgnoreCase)
                                      .Select(type => new MediaTypeWithQualityHeaderValue(type))
                                      .DefaultIfEmpty(new MediaTypeWithQualityHeaderValue("*/*"))
                                      .ToList();
            return this;
        }

        /// <summary>
        /// Use <see cref="MediaTypeNames"/> to set correct values as well
        /// </summary>
        public Options AddContentType(string contentType)
        {
            _requestContentType = contentType;
            return this;
        }

        public Options ForStreamResponse()
        {
            _requestCompletionOptions = HttpCompletionOption.ResponseHeadersRead;
            return this;
        }

        public Options AddRequestParameters(params KeyValuePair<string, string?>[] parameters)
        {
            _requestParameters = parameters;
            return this;
        }

        /// <summary>
        /// Use <see cref="Microsoft.Net.Http.Headers.HeaderNames"/> to set correct Header Name
        /// </summary>
        public Options AddHeader(string name, params string?[] value)
        {
            KeyValuePair<string, string> header = RequestHeader(name, value);
            if (IsValidHeader(header!))
            {
                _requestHeaders ??= new Dictionary<string, string?>();
                _requestHeaders.Add(header.Key, header.Value);
            }

            return this;
        }

        /// <summary>
        /// Use <see cref="Microsoft.Net.Http.Headers.HeaderNames"/> to set correct Header Name
        /// </summary>
        public Options AddHeader(KeyValuePair<string, string?> header)
        {
            if (IsValidHeader(header))
            {
                _requestHeaders ??= new Dictionary<string, string?>();
                _requestHeaders.Add(header.Key, header.Value);
            }

            return this;
        }

        /// <summary>
        /// Use <see cref="Microsoft.Net.Http.Headers.HeaderNames"/> to set correct Header Name
        /// </summary>
        public Options AddHeaders(params KeyValuePair<string, string?>[] headers)
        {
            _requestHeaders ??= new Dictionary<string, string?>(headers.Length);
            
            foreach (var header in headers.Where(header => IsValidHeader(header))) 
                _requestHeaders.Add(header.Key, header.Value);

            return this;
        }

        /// <summary>
        /// Use <see cref="Schemes.AuthenticationSchemes"/> to set correct Header Name
        /// </summary>
        public Options AddAuthentication(Func<Schemes.AuthenticationSchemes, AuthenticationHeaderValue> schemeSelector)
        {
            _authenticationHeader = schemeSelector.Invoke(new());
            return this;
        }

        public Options AddCustomFlags(string key, object? value)
        {
            _requestCustomActionFlags ??= new Dictionary<string, object?>();
            _requestCustomActionFlags.TryAdd(key, value);
            return this;
        }

        internal Options SetHttpMethod(HttpMethod method)
        {
            _httpMethod = method;
            return this;
        }

        internal Options SetRequestEndpoint(string requestEndpoint)
        {
            _requestPath = requestEndpoint;
            return this;
        }

        private static bool IsValidHeader(KeyValuePair<string, string?> header) => !string.IsNullOrWhiteSpace(header.Key) && !string.IsNullOrWhiteSpace(header.Value);
    }
}