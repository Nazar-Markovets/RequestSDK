using System.Net.Http.Headers;
using System.Net.Mime;
using System.Xml.Linq;
using System.Linq;

namespace RequestSDK.Services;

public partial class RequestService
{
    public sealed class Options
    {
        internal HttpMethod? HttpMethod { get; set; }
        internal KeyValuePair<string, string?>[] RequestParameters { get; private set; }
        internal List<MediaTypeWithQualityHeaderValue> AcceptTypes { get; private set; }
        internal string Path { get; set; }
        internal byte? HttpClientId { get; set; }
        public Dictionary<string, string?>? CustomHeaders { get; private set; }

        public string ContentType { get; init; }
        public HttpCompletionOption CompletionOption { get; init; }
        public AuthenticationHeaderValue? Authentication { get; init; }
        public Dictionary<string, object?>? CustomActionFlags { get; init; }

        /// <summary>
        /// Using autorouting based on <see cref="Attributes.ControllerNameAttribute"/>, and <see cref="Attributes.ControllerHttpMethodAttribute"/> metadata
        /// </summary>
        internal readonly bool UseSdkRouting;

        private Options()
        {
            HttpMethod = default;
            Path = string.Empty;
            ContentType = MediaTypeNames.Application.Json;
            AcceptTypes = new List<MediaTypeWithQualityHeaderValue>() { new MediaTypeWithQualityHeaderValue("*/*") };
            RequestParameters = Array.Empty<KeyValuePair<string, string?>>();
        }

        public Options(HttpMethod httpMethod, string requestPath, byte registeredHttpClientId = byte.MinValue)
        {
            HttpMethod = httpMethod;
            Path = requestPath;
            HttpClientId = registeredHttpClientId;
            ContentType = MediaTypeNames.Application.Json;
            CompletionOption = HttpCompletionOption.ResponseContentRead;
            AcceptTypes = new List<MediaTypeWithQualityHeaderValue>() { new MediaTypeWithQualityHeaderValue("*/*") };
            RequestParameters = Array.Empty<KeyValuePair<string, string?>>();
            UseSdkRouting = false;
        }

        public Options(string requestPath) : this(default!, requestPath) => UseSdkRouting = true;

        public Options AddHttpClientId<T>(T clientId) where T : Enum
        {
            HttpClientId = Convert.ToByte(clientId);
            return this;
        }

        public Options AddHttpClientId(byte clientId)
        {
            HttpClientId = clientId;
            return this;
        }

        public Options AddAcceptTypes(params string[] types)
        {
            AcceptTypes = new HashSet<string>(types, StringComparer.OrdinalIgnoreCase).
                              Select(type => new MediaTypeWithQualityHeaderValue(type)).
                              DefaultIfEmpty(new MediaTypeWithQualityHeaderValue("*/*")).
                              ToList();
            return this;
        }

        public Options AddRequestParameters(params KeyValuePair<string, string?>[] parameters)
        {
            RequestParameters = parameters;
            return this;
        }

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

        public Options AddHeader(KeyValuePair<string, string?> header)
        {
            if (IsValidHeader(header))
            {
                CustomHeaders ??= new Dictionary<string, string?>();
                CustomHeaders.Add(header.Key, header.Value);
            }

            return this;
        }

        public Options AddHeaders(params KeyValuePair<string, string?>[] headers)
        {
            CustomHeaders ??= new Dictionary<string, string?>(headers.Length);
            
            foreach (var header in headers.Where(header => IsValidHeader(header))) 
                CustomHeaders.Add(header.Key, header.Value);

            return this;
        }

        private bool IsValidHeader(KeyValuePair<string, string?> header) => !string.IsNullOrWhiteSpace(header.Key) && !string.IsNullOrWhiteSpace(header.Value);
    }
}