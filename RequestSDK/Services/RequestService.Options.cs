using System.Net.Http.Headers;
using System.Net.Mime;
using System.Xml.Linq;
using System.Linq;

namespace RequestSDK.Services;

public partial class RequestService
{
    public class Options
    {
        public HttpMethod? HttpMethod { get; internal set; }
        public string Path { get; internal set; }
        public byte? HttpClientId { get; internal set; }
        public KeyValuePair<string, string?>[] RequestParameters { get; private set; }
        public List<MediaTypeWithQualityHeaderValue> AcceptTypes { get; private set; }
        public Dictionary<string, string?>? CustomHeaders { get; private set; }
        public AuthenticationHeaderValue? Authentication { get; private set; }
        public HttpCompletionOption CompletionOption { get; private set; }
        public string ContentType { get; private set; }
        public Dictionary<string, object?>? CustomActionFlags { get; private set; }

        /// <summary>
        /// Using autorouting based on <see cref="Attributes.ControllerNameAttribute"/>, and <see cref="Attributes.ControllerHttpMethodAttribute"/> metadata
        /// </summary>
        internal bool UseSdkRouting { get; private set; }

        private Options()
        {
            CompletionOption = HttpCompletionOption.ResponseContentRead;
            ContentType = MediaTypeNames.Application.Json;
            AcceptTypes = new List<MediaTypeWithQualityHeaderValue>() { new MediaTypeWithQualityHeaderValue("*/*") };
            RequestParameters = Array.Empty<KeyValuePair<string, string?>>();
            Path = string.Empty;
        }

        private Options(HttpMethod httpMethod, string requestPath, byte? registeredHttpClientId = default) : this()
        {
            HttpMethod = httpMethod;
            Path = requestPath;
            HttpClientId = registeredHttpClientId;
        }

        public static Options WithoutRegisteredClient(HttpMethod httpMethod, string requestPath) => new(httpMethod, requestPath, null);
        public static Options WithRegisteredClient(HttpMethod httpMethod, string requestPath, byte clientId) => new (httpMethod, requestPath, clientId);
        
        public static Options WithRegisteredClient<TRoutingContainer>(HttpMethod httpMethod, string requestPath, TRoutingContainer clientId) where TRoutingContainer : Enum
        {
            return new(httpMethod, requestPath, Convert.ToByte(clientId));
        }

        public static Options WithRegisteredRouting(string requestPath, byte? clientId = default)
        {
            return new() { Path = requestPath, HttpClientId = clientId, UseSdkRouting = true };
        }

        public static Options WithRegisteredRouting<TRoutingContainer>(string requestPath, TRoutingContainer clientId) where TRoutingContainer : Enum
        {
            return new() { Path = requestPath, HttpClientId = Convert.ToByte(clientId), UseSdkRouting = true };
        }

        /// <summary>
        /// Use <see cref="MediaTypeNames"/> to set correct values as well
        /// </summary>
        public Options AddAcceptTypes(params string[] types)
        {
            AcceptTypes = new HashSet<string>(types, StringComparer.OrdinalIgnoreCase).
                              Select(type => new MediaTypeWithQualityHeaderValue(type)).
                              DefaultIfEmpty(new MediaTypeWithQualityHeaderValue("*/*")).
                              ToList();
            return this;
        }

        /// <summary>
        /// Use <see cref="MediaTypeNames"/> to set correct values as well
        /// </summary>
        public Options AddContentType(string contentType)
        {
            ContentType = contentType;
            return this;
        }

        public Options ForStreamResponse()
        {
            CompletionOption = HttpCompletionOption.ResponseHeadersRead;
            return this;
        }

        public Options AddRequestParameters(params KeyValuePair<string, string?>[] parameters)
        {
            RequestParameters = parameters;
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
                CustomHeaders ??= new Dictionary<string, string?>();
                CustomHeaders.Add(header.Key, header.Value);
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
                CustomHeaders ??= new Dictionary<string, string?>();
                CustomHeaders.Add(header.Key, header.Value);
            }

            return this;
        }

        /// <summary>
        /// Use <see cref="Microsoft.Net.Http.Headers.HeaderNames"/> to set correct Header Name
        /// </summary>
        public Options AddHeaders(params KeyValuePair<string, string?>[] headers)
        {
            CustomHeaders ??= new Dictionary<string, string?>(headers.Length);
            
            foreach (var header in headers.Where(header => IsValidHeader(header))) 
                CustomHeaders.Add(header.Key, header.Value);

            return this;
        }

        /// <summary>
        /// Use <see cref="Schemes.AuthenticationSchemes"/> to set correct Header Name
        /// </summary>
        public Options AddAuthentication(Func<Schemes.AuthenticationSchemes, string> schemeSelector, string token)
        {
            if (string.IsNullOrEmpty(token)) throw new ArgumentException("Authorization token can't be null or empty");
            string selectedScheme = schemeSelector.Invoke(new());
            Authentication = new AuthenticationHeaderValue(selectedScheme, token);
            return this;
        }

        public Options AddCustomFlags(string key, object? value)
        {
            CustomActionFlags ??= new Dictionary<string, object?>();
            CustomActionFlags.TryAdd(key, value);
            return this;
        }

        private bool IsValidHeader(KeyValuePair<string, string?> header) => !string.IsNullOrWhiteSpace(header.Key) && !string.IsNullOrWhiteSpace(header.Value);
    }
}