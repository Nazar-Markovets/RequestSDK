using System.Net.Http.Headers;
using System.Net.Mime;

using RequestSDK.Builders;
using RequestSDK.Validators;

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
        private List<MediaTypeWithQualityHeaderValue> _requestAcceptTypes;
        private Dictionary<string, object?>? _requestCustomActionFlags;
        private AuthenticationHeaderValue? _authenticationHeader;
        private HttpCompletionOption _requestCompletionOptions = HttpCompletionOption.ResponseContentRead;
        private bool _useRecognizableRouting = false;
        private OptionsBuilder _requestParameterBuilder;
        private OptionsBuilder _requestHeadersBuilder;
        private static RequestOptionsValidator _optionsValidator = new();
        #endregion Private Fields

        #region Readonly Properties
        internal HttpMethod? HttpMethod => _httpMethod;
        internal string Path => _requestPath;
        internal byte? HttpClientId => _httpClientId;
        internal Dictionary<string, string> RequestParameters => _requestParameterBuilder.Build();
        internal Dictionary<string, string> RequestHeaders => _requestHeadersBuilder.Build();
        internal List<MediaTypeWithQualityHeaderValue> AcceptTypes => _requestAcceptTypes;

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
            _requestParameterBuilder = new RequestOptionsBuilder(_optionsValidator);
            _requestHeadersBuilder = new RequestOptionsBuilder(_optionsValidator);
            _requestPath = string.Empty;
        }

        private Options(HttpMethod httpMethod, string requestPath, byte? registeredHttpClientId = default) : this()
        {
            _httpMethod = httpMethod;
            _requestPath = requestPath.Trim().ToLower();
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

        public Options ConfigureBuilders(OptionsBuilder? requestHeadersBuilder = default, OptionsBuilder? requestParametersBuilder = default)
        {
            _requestHeadersBuilder = requestHeadersBuilder ?? _requestHeadersBuilder;
            _requestParameterBuilder = requestParametersBuilder ?? _requestParameterBuilder;
            return this;
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

        public Options AddRequestParameter(string name, params string[] value)
        {
            _requestParameterBuilder.Add(name, value);
            return this;
        }

        public Options AddRequestParameters(Action<OptionsBuilder> buildConfiguration)
        {
            buildConfiguration(_requestParameterBuilder);
            return this;
        }

        /// <summary>
        /// Use <see cref="Microsoft.Net.Http.Headers.HeaderNames"/> to set correct Header Name
        /// </summary>
        public Options AddHeader(string name, params string[] value)
        {
            _requestHeadersBuilder.Add(name, value);
            return this;
        }

        /// <summary>
        /// Use <see cref="Microsoft.Net.Http.Headers.HeaderNames"/> to set correct Header Name
        /// </summary>
        public Options AddHeaders(Action<OptionsBuilder> buildConfiguration)
        {
            buildConfiguration(_requestHeadersBuilder);
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

    }
}