using System.Collections.Immutable;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text.Json;
using System.Text;
using Microsoft.Extensions.Caching.Memory;
using RequestSDK.Attributes;
using RequestSDK.Enums;
using System.Collections.Specialized;
using Microsoft.AspNetCore.WebUtilities;
using System.Web;
using System.Net.Mime;
using System.Runtime.CompilerServices;

namespace RequestSDK.Services;

public partial class RequestService
{

    #region Private Fields

    private readonly IMemoryCache? _cache;
    private readonly string _cacheKey = "RoutingMedatata";
    private readonly ImmutableSortedSet<(string Route, string Controller, string RequestPath, HttpRequestMethod RequestMethod)>? _routingInfo;
    private readonly Dictionary<byte, HttpClientSettings>? _instances;
    private readonly Dictionary<HttpRequestMethod, HttpMethod> _httpMethods = new()
    {
        { HttpRequestMethod.Get, HttpMethod.Get },
        { HttpRequestMethod.Put, HttpMethod.Put },
        { HttpRequestMethod.Delete, HttpMethod.Delete },
        { HttpRequestMethod.Post, HttpMethod.Post },
        { HttpRequestMethod.Head, HttpMethod.Head },
        { HttpRequestMethod.Trace, HttpMethod.Trace },
        { HttpRequestMethod.Patch, HttpMethod.Patch },
        { HttpRequestMethod.Connect, new HttpMethod("CONNECT") },
        { HttpRequestMethod.Options, HttpMethod.Options },
    };
    private readonly HttpClient _defaultHttpClient;
    private RequestServiceOptions _serviceOptions = default!;

    #endregion Private Fields

    public RequestServiceOptions SetupOptions
    {
        get => _serviceOptions;
        init
        {
            _serviceOptions = value;
            if (value.AccemblyRoutingType != null)
            {
                _cache = new MemoryCache(new MemoryCacheOptions());
                _routingInfo = GetCachedMetadata();
            }
            _instances = value.HttpClientSettings?.ToDictionary(kv => kv.HttpClientId, kv => kv);
        }
    }

    public RequestService(HttpClient httpClient)
    {
        _defaultHttpClient = httpClient ?? throw new ArgumentNullException("Given HttpClient can't be null");
        SetupOptions = new RequestServiceOptions();
    }

    public RequestService(RequestServiceOptions requestServiceOptions)
    {
        SetupOptions = requestServiceOptions ?? throw new ArgumentNullException("Request Service Options can't be null");
        _defaultHttpClient = new HttpClient();
    }

    #region Sending Request

    public Task<HttpResponseMessage> ExecuteRequestAsync(Options requestOptions, HttpContent? httpContent, CancellationToken cancellationToken = default) =>
        MakeRequest(requestOptions, httpContent, cancellationToken);

    public Task<HttpResponseMessage> ExecuteRequestAsync(Options requestOptions, CancellationToken cancellationToken = default) =>
        MakeRequest(requestOptions, default, cancellationToken);

    public Task<HttpResponseMessage> ExecuteRequestAsync<TBody>(Options requestOptions, TBody DTO, JsonSerializerOptions serializerOptions = default!, CancellationToken cancellationToken = default) =>
        MakeRequest(requestOptions, GetCorrectHttpContent(DTO, serializerOptions), cancellationToken);

    public Task<HttpResponseMessage> MakeRequest(Options options, HttpContent? content, CancellationToken cancellationToken)
    {
        byte requestedSetting = byte.TryParse(options.HttpClientId?.ToString(), out byte clientId) ? clientId : byte.MinValue;
        HttpClientSettings? registeredSetting = _instances?.ContainsKey(requestedSetting) is true ? _instances![requestedSetting] : default;
        HttpClient httpClient = SetupOptions.Factory?.CreateClient(registeredSetting?.HttpClientName ?? Microsoft.Extensions.Options.Options.DefaultName) ?? _defaultHttpClient;
        string requestQuery = string.Empty;

        //Request Path
        {
            if (options.UseSdkRouting)
            {
                var requestMetadata = GetRequestMetadata(options.Path);
                options.Path = requestMetadata.RequestPath;
                options.HttpMethod = requestMetadata.HttpMethod;
            }
            bool useOptionsAsPath = TryUriParse(options.Path, out Uri? optionsPath);
            string basePath = GetBasePath(httpClient.BaseAddress ?? registeredSetting?.BaseAddress) ?? GetBasePath(optionsPath) ?? throw new Exception("Can't create request uri. Check request options");
            string? path = useOptionsAsPath ? AppendPathSafely(basePath!, optionsPath!.PathAndQuery) : AppendPathSafely(basePath!, options.Path);
            requestQuery = QueryHelpers.AddQueryString(path, options.RequestParameters?.ToDictionary(k => k.Key, v => v.Value) ?? new());
        }

        //Request Options
        {
            httpClient.DefaultRequestHeaders.Clear();
            if (options.AcceptTypes is null) httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));
            else options.AcceptTypes?.ForEach(acceptType => httpClient.DefaultRequestHeaders.Accept.Add(acceptType));

            AuthenticationHeaderValue? authenticationHeader = options.Authentication ?? SetupOptions.Authentication?.Invoke();
            httpClient.DefaultRequestHeaders.Authorization = authenticationHeader;

            options.CustomHeaders?.ToList()?.ForEach(header => httpClient.DefaultRequestHeaders.Add(header.Key, header.Value));
        }

        HttpRequestMessage requestMessage = new()
        {
            RequestUri = new Uri(requestQuery),
            Content = content,
            Method = options.HttpMethod ?? throw new ArgumentNullException("Http Method can't be null")
        };
        return httpClient.SendAsync(requestMessage, options.CompletionOption, cancellationToken);
    }

    public static StringContent? GetCorrectHttpContent<T>(T? content, JsonSerializerOptions serializerOptions = default!)
    {
        bool? mustBeJson = MustBeSendAsJson(content);
        if (mustBeJson.HasValue is false) return null;

        return mustBeJson.Value
               ? new StringContent(JsonSerializer.Serialize(content, serializerOptions), Encoding.UTF8, MediaTypeNames.Application.Json)
               : new StringContent(content!.ToString()!);
    }

    public static bool? MustBeSendAsJson<T>(T content)
    {
        if (content is null) return null;

        Type type = content.GetType() ?? typeof(T);

        bool isStruct = type.IsValueType && !type.IsPrimitive;
        bool isClass = (type.IsClass && !type.Equals(typeof(string))) || IsAnonymousType(type);
        return isClass || isStruct;
    }

    private static bool IsAnonymousType(Type type)
    {
        if (type == null)
            throw new ArgumentNullException("Type Can't be null");

        // HACK: The only way to detect anonymous types right now.
        return Attribute.IsDefined(type, typeof(CompilerGeneratedAttribute), false)
               && type.IsGenericType && type.Name.Contains("AnonymousType")
               && (type.Name.StartsWith("<>") || type.Name.StartsWith("VB$"))
               && type.Attributes.HasFlag(TypeAttributes.NotPublic);
    }

    #endregion Sending Request

    #region SSE Streaming

    public static async Task PushSSE<T>(StreamWriter writer, T chunk)
    {
        //"data:" prefix is important part as well as trailing "new empty line"
        await writer.WriteLineAsync($"data:{JsonSerializer.Serialize(chunk)}");
        await writer.WriteLineAsync();
        await writer.FlushAsync();
    }

    public static async IAsyncEnumerable<T> GetSSEResponseAsync<T>(HttpResponseMessage response)
    {
        await using Stream stream = await response.Content.ReadAsStreamAsync();
        using StreamReader reader = new(stream);

        while (!reader.EndOfStream)
        {
            string? streamMessage = await reader.ReadLineAsync();
            if (string.IsNullOrEmpty(streamMessage)) continue;

            streamMessage = ClearSSePreffix(streamMessage);

            T? block;
            try
            {
                block = JsonSerializer.Deserialize<T>(streamMessage);
            }
            catch (Exception)
            {
                continue;
            }

            if (null != block) yield return block;
        }
    }

    #endregion SSE Streaming

    #region Recognizable Routing

    private ImmutableSortedSet<(string Route, string Controller, string RequestPath, HttpRequestMethod RequestMethod)> GetCachedMetadata() =>
        _cache?.GetOrCreate(_cacheKey, factory => { factory.AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(1); return GetMetadata(); }) ?? GetMetadata();

    private ImmutableSortedSet<(string Route, string Controller, string RequestPath, HttpRequestMethod RequestMethod)> GetMetadata()
    {
        var routes = SetupOptions.AccemblyRoutingType!.GetTypeInfo().DeclaredNestedTypes
                                    .SelectMany(typeInfo => typeInfo.DeclaredFields
                                                                    .Where(fieldInfo => fieldInfo.IsLiteral && !fieldInfo.IsInitOnly)
                                                                    .Select(@const =>
                                                                    {
                                                                        string route = @const.GetRawConstantValue()!.ToString()!.ToLower()!;
                                                                        HttpRequestMethod routeMethod = @const.GetCustomAttribute<ControllerHttpMethodAttribute>()?.HttpRequestMethod ?? throw new Exception("HttpMethod is not defined");
                                                                        string controller = typeInfo.GetCustomAttribute<ControllerNameAttribute>()?.ControllerName?.ToLower() ?? throw new Exception("Controller is not defined");
                                                                        return (Route: route,
                                                                                Controller: controller,
                                                                                RequestPath: $"{controller}/{route}",
                                                                                RequestMethod: routeMethod);
                                                                    })).ToImmutableSortedSet();
        return routes;
    }

    private (string RequestPath, HttpMethod HttpMethod) GetRequestMetadata(string path)
    {
        (string Route, string Controller, string RequestPath, HttpRequestMethod RequestMethod) requestInfo = _routingInfo!.FirstOrDefault(r => r.Route.Equals(path, StringComparison.OrdinalIgnoreCase));
        if (string.IsNullOrEmpty(requestInfo.RequestPath)) throw new Exception("Not found routing");
        return (requestInfo.RequestPath, _httpMethods[requestInfo.RequestMethod]);
    }

    #endregion Recognizable Routing

    #region Static Methods

    public static KeyValuePair<string, string> RequestParameter(string? key, string? value) =>
        string.IsNullOrWhiteSpace(key)
        ? new(string.Empty, string.Empty) 
        : new(key, value ?? string.Empty);

    public static KeyValuePair<string, string> RequestHeader(string? key, params string?[] value) => 
        string.IsNullOrWhiteSpace(key)
        ? new(string.Empty, string.Empty)
        : new(key, string.Join(", ", value?.Where(v => !string.IsNullOrWhiteSpace(v)) ?? Array.Empty<string>()));

    public static string CombineQueryParameters(bool ignoreEmpty, params KeyValuePair<string, string?>[] queryParameters)
    {
        if (queryParameters.Length > 0)
        {
            NameValueCollection queryString = new(queryParameters.Length);
            for (int i = 0; i < queryParameters.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(queryParameters[i].Key)) continue;
                if (ignoreEmpty && string.IsNullOrWhiteSpace(queryParameters[i].Value)) continue;
                queryString.Add(queryParameters[i].Key, queryParameters[i].Value);
            }
            return string.Join('&', queryString.AllKeys.Select(key => $"{key}={string.Join(',', queryString.GetValues(key)?.Distinct().Where(value => !string.IsNullOrEmpty(value)) ?? Array.Empty<string>())}"));
        }
        return string.Empty;
    }

    public static Dictionary<string, string?> GetQueryParameters(Uri uri) => GetQueryParameters(uri.Query);

    public static Dictionary<string, string?> GetQueryParameters(string query)
    {
        NameValueCollection collection = HttpUtility.ParseQueryString(query);
        return collection.AllKeys.Where(k => !string.IsNullOrWhiteSpace(k)).ToDictionary(k => k!, k => collection[k]);
    }

    public static string CombineQueryWithParameters(string path, bool ignoreEmpty, params KeyValuePair<string, string?>[] pairs)
    {
        if (string.IsNullOrWhiteSpace(path.Trim())) throw new InvalidCastException("Can't combine empty query string with parameters");

        int startParametersIndex = path.IndexOf('?');
        bool hasParameters = startParametersIndex > 0 && startParametersIndex < path.Length;
        string parameters = pairs.Length > 0 ? (hasParameters ? '&' : '?') + CombineQueryParameters(ignoreEmpty, pairs) : string.Empty;
        return string.Concat(path, parameters);
    }

    public static string CombineQueryWithParameters(string path, string parameters, bool ignoreEmpty)
    {
        if (string.IsNullOrWhiteSpace(path.Trim())) throw new InvalidCastException("Can't combine empty query string with parameters");

        int startParametersIndex = path.IndexOf('?');
        bool hasParameters = startParametersIndex > 0 && startParametersIndex < path.Length;
        Dictionary<string, string?> queryParameters = GetQueryParameters(parameters);
        string parametersString = queryParameters.Count > 0 ? (hasParameters ? '&' : '?') + CombineQueryParameters(ignoreEmpty, queryParameters.ToArray()) : string.Empty;
        return string.Concat(path, parametersString);
    }

    public static string CombineQueryWithParameters(string path, string route, bool ignoreEmpty, params KeyValuePair<string, string?>[] pairs) =>
        CombineQueryWithParameters(AppendPathSafely(path, route), ignoreEmpty, pairs);

    public static string? GetBasePath(string? path)
    {
        if (path is null) return default;
        return TryUriParse(path, out Uri? url) && HasValidScheme(url!)
        ? $"{url!.Scheme}{Uri.SchemeDelimiter}{url!.Authority}"
        : throw new InvalidCastException("Can't convert invalid path");
    }

    public static bool TryUriParse(string? path, out Uri? url) =>
        Uri.TryCreate(path, new UriCreationOptions() { DangerousDisablePathAndQueryCanonicalization = false }, out url);

    public static string? GetBasePath(Uri? path)
    {
        if (path is null) return default;
        if (HasValidScheme(path) == false) throw new UriFormatException("Path has invalid scheme");
        return $"{path.Scheme}{Uri.SchemeDelimiter}{path.Authority}";
    }

    public static string AppendPathSafely(string path, string route, bool cleanExistsParameters = true, bool autoEncode = false)
    {
        path = path.Trim();
        if (string.IsNullOrWhiteSpace(path)) throw new ArgumentNullException("Can't combine empty absolute path");
        route = route.TrimStart('/').Trim();
        string basePath = GetBasePath(path)!;
        string queryPath = $"{basePath}/{route}";

        if (cleanExistsParameters is false)
        {
            string parameters = TryUriParse(path, out Uri? url) ? url!.Query : string.Empty;
            return autoEncode ? HttpUtility.UrlEncode(CombineQueryWithParameters(queryPath, parameters, true)) : CombineQueryWithParameters(queryPath, parameters, true);
        }

        return autoEncode ? HttpUtility.UrlEncode(queryPath) : queryPath;
    }

    public static string Base64Encode(string? plainText) => Convert.ToBase64String(Encoding.UTF8.GetBytes(plainText ?? string.Empty));
    public static string Base64Decode(string? base64EncodedData) => Encoding.UTF8.GetString(Convert.FromBase64String(base64EncodedData ?? string.Empty));

    public static string Base64EncodeObject<T>(T obj)
    {
        string json = JsonSerializer.Serialize(obj);
        byte[] jsonBytes = Encoding.UTF8.GetBytes(json);
        string base64String = Convert.ToBase64String(jsonBytes);
        return base64String;
    }

    public static T? Base64DecodeObject<T>(string base64EncodedJson)
    {
        byte[] jsonBytes = Convert.FromBase64String(base64EncodedJson);
        string json = Encoding.UTF8.GetString(jsonBytes);
        return JsonSerializer.Deserialize<T>(json);
    }

    private static string ClearSSePreffix(string text) => text.IndexOf("data:", StringComparison.Ordinal) != 0 ? text.Trim() : text[5..].Trim();
    private static bool HasValidScheme(Uri path)
    {
        string[] validSchemes = {
            Uri.UriSchemeFile,
            Uri.UriSchemeFtp,
            Uri.UriSchemeSftp,
            Uri.UriSchemeFtps,
            Uri.UriSchemeGopher,
            Uri.UriSchemeHttp,
            Uri.UriSchemeHttps,
            Uri.UriSchemeWs,
            Uri.UriSchemeWss,
            Uri.UriSchemeMailto,
            Uri.UriSchemeNews,
            Uri.UriSchemeNntp,
            Uri.UriSchemeSsh,
            Uri.UriSchemeTelnet,
            Uri.UriSchemeNetTcp,
            Uri.UriSchemeNetPipe
        };
        return validSchemes.Contains(path.Scheme);
    }

    #endregion Static Methods
}