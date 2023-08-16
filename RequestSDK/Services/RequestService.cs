using System.Text.Json;
using System.Reflection;
using System.Collections.Immutable;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.AspNetCore.WebUtilities;

using RequestSDK.Attributes;
using RequestSDK.Enums;
using RequestSDK.Helpers;
using RequestSDK.Extensions;

namespace RequestSDK.Services;

public partial class RequestService
{
    #region Private Fields

    private IMemoryCache? _cache;
    private readonly Dictionary<byte, HttpClientSettings>? _instances;
    private readonly HttpClient _defaultHttpClient;
    private readonly IHttpClientFactory? _httpClientFactory;
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

    #endregion Private Fields

    public RequestService(HttpClient httpClient)
    {
        _defaultHttpClient = httpClient ?? throw new ArgumentNullException("Given HttpClient can't be null");
    }

    internal RequestService(IHttpClientFactory? httpClientFactory, params HttpClientSettings[] httpClientSettings)
    {
        _httpClientFactory = httpClientFactory;
        _instances = httpClientSettings?.ToDictionary(kv => kv.HttpClientId!.Value, kv => kv);
        _defaultHttpClient = new HttpClient();
    }

    #region Sending Request

    public Task<HttpResponseMessage> ExecuteRequestAsync(Options requestOptions, HttpContent? httpContent, CancellationToken cancellationToken = default) =>
        MakeRequest(requestOptions, httpContent, cancellationToken);

    public Task<HttpResponseMessage> ExecuteRequestAsync(Options requestOptions, CancellationToken cancellationToken = default) =>
        MakeRequest(requestOptions, default, cancellationToken);

    public Task<HttpResponseMessage> ExecuteRequestAsync<TBody>(Options requestOptions, TBody DTO, JsonSerializerOptions serializerOptions = default!, CancellationToken cancellationToken = default) =>
        MakeRequest(requestOptions, HttpContentHelper.ConvertToHttpContent(DTO, requestOptions.ContentType, serializerOptions), cancellationToken);

    private Task<HttpResponseMessage> MakeRequest(Options options, HttpContent? content, CancellationToken cancellationToken)
    {
        HttpClientSettings? registeredSetting = options.HttpClientId.HasValue && 
                                                _instances?.TryGetValue(options.HttpClientId.Value, out HttpClientSettings? setting) == true ? setting : default;

        HttpClient httpClient = _httpClientFactory?.CreateClient(registeredSetting?.HttpClientName ?? Microsoft.Extensions.Options.Options.DefaultName) ?? _defaultHttpClient;

        TryGetEndpointMetadata(options, registeredSetting);
        SetRequestPath(options, httpClient, registeredSetting);
        SetRequestHeaders(options, httpClient, registeredSetting);

        HttpRequestMessage requestMessage = new()
        {
            RequestUri = new Uri(options.Path),
            Content = content,
            Method = options.HttpMethod ?? throw new ArgumentNullException("Http Method can't be null")
        };
        return httpClient.SendAsync(requestMessage, options.CompletionOption, cancellationToken);
    }

    #endregion Sending Request

    #region Recognizable Routing

    private void TryGetEndpointMetadata(Options options, HttpClientSettings? httpClientSettings)
    {
        Type? contanterType = httpClientSettings?.ClientRoutingType;
        if (options.UseRecognizableRouting == false || contanterType == null) return;

        string cacheKey = contanterType.FullName ?? contanterType.Name;
        _cache ??= new MemoryCache(new MemoryCacheOptions());

        (string Endpoint, string RequestPath, HttpMethod RequestMethod) =
            _cache.GetOrCreate(cacheKey, factory =>
            {
                factory.AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(1);
                return GetContanterMetadata(contanterType);
            })!.FirstOrDefault(r => r.Endpoint.Equals(options.Path));

        options.SetHttpMethod(RequestMethod)
               .SetRequestEndpoint(RequestPath);
    }

    private ImmutableSortedSet<(string Endpoint, string RequestPath, HttpMethod RequestMethod)> GetContanterMetadata(Type clientRoutingType) =>
        clientRoutingType.GetTypeInfo().DeclaredNestedTypes
                         .SelectMany(typeInfo => typeInfo.DeclaredFields
                         .Where(fieldInfo => fieldInfo.IsLiteral && !fieldInfo.IsInitOnly)
                         .Select(routingEndpointConst =>
                         {
                             string route = routingEndpointConst.GetRawConstantValue()!.ToString()!.ToLower()!;
                             HttpRequestMethod routeMethod = routingEndpointConst.GetCustomAttribute<ControllerHttpMethodAttribute>()?.HttpRequestMethod
                                                             ?? throw new Exception("HttpMethod is not defined");
                             string controller = typeInfo.GetCustomAttribute<ControllerNameAttribute>()?.ControllerName?.ToLower()
                                                 ?? throw new Exception("Controller is not defined");
                             return (route, $"{controller}/{route}", _httpMethods[routeMethod]);
                         })).ToImmutableSortedSet();

    #endregion Recognizable Routing

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

    private static string ClearSSePreffix(string text) => text.IndexOf("data:", StringComparison.Ordinal) != 0 ? text.Trim() : text[5..].Trim();


    #endregion SSE Streaming

    #region Static Methods

    private static void SetRequestHeaders(Options options, HttpClient httpClient, HttpClientSettings? httpClientSettings)
    {
        httpClient.DefaultRequestHeaders.Clear();
        httpClient.DefaultRequestHeaders.Authorization = options.Authentication ?? httpClientSettings?.Authentication?.Invoke(new());
        options.AcceptTypes.ForEach(acceptType => httpClient.DefaultRequestHeaders.Accept.Add(acceptType));
        options.RequestHeaders.ForEach(header => httpClient.DefaultRequestHeaders.Add(header.Key, header.Value));
    }

    private static void SetRequestPath(Options options, HttpClient httpClient, HttpClientSettings? httpClientSettings)
    {
        UriKind uriKind = Uri.IsWellFormedUriString(options.Path, UriKind.Absolute) ? UriKind.Absolute : UriKind.Relative;
        if(Uri.TryCreate(options.Path, UriKind.RelativeOrAbsolute, out Uri? endpointUri))
        {
            string resultPath = endpointUri.ToString();
            if(uriKind == UriKind.Relative)
            {
                Uri? registeredHttpClientUri = httpClient.BaseAddress ?? httpClientSettings?.BaseAddress;
                resultPath = Uri.IsWellFormedUriString(registeredHttpClientUri?.ToString(), UriKind.Absolute)
                             ? QueryHelper.AppendToEnd(registeredHttpClientUri, resultPath).ToString()
                             : throw new UriFormatException($"Relative path {options.Path} can't be used if HttpClient BaseAddress is not set");
            }

            options.SetRequestEndpoint(QueryHelpers.AddQueryString(resultPath, options.RequestParameters));
            return;
        }

        throw new UriFormatException("Given Options Path is not valid for building request path");
    }

    #endregion Static Methods
}