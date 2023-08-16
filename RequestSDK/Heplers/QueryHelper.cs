using System.Collections.Specialized;
using System.Web;

using RequestSDK.Builders;
using RequestSDK.Validators;

namespace RequestSDK.Helpers;

public static partial class QueryHelper
{

    private static readonly RequestOptionsValidator _requestOptionsValidator = new();

    /// <summary>
    /// Create KeyValue pair that can be used to build request query with <see cref="QueryHelper.CombineQueryWithParameters"/>
    /// </summary>
    /// <param name="key">Parameter Name</param>
    /// <param name="valuesCollection">Parameter Comma-separated values</param>
    /// <returns><see cref="KeyValuePair{string, string}"/> with ignored empty or null values, or empty <see cref="KeyValuePair{TKey, TValue}"/> if all values are null.</returns>
    public static KeyValuePair<string, string> CreateQueryParameter(string key, params string[] valuesCollection)
    {
        if(_requestOptionsValidator.ValidateOptionsCollection(key, valuesCollection))
        {
            return new KeyValuePair<string, string>(key, RequestOptionsBuilder.GetJoinedValues(valuesCollection));
        }
        return new KeyValuePair<string, string>();
    }

    /// <summary>
    /// Create string of separated request query parameters
    /// </summary>
    /// <param name="ignoreEmpty">If 'true' - impty values won't be included</param>
    /// <param name="queryParameters">KeyValues of request parameters to combine</param>
    /// <returns>String of format <![CDATA[ [key]=[value1,value2]&[key2]=[value3] ]]>  </returns>
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

    /// <summary>
    /// Detect all query parameters for given query <![CDATA[ ?parameter1=value&parameter2=value]]>
    /// </summary>
    /// <param name="uri">Absolute Uri</param>
    /// <returns>Dictionary of query parameters</returns>
    public static Dictionary<string, string?> GetQueryParameters(Uri uri) => GetQueryParameters(uri.Query);

    public static Dictionary<string, string?> GetQueryParameters(string query)
    {
        NameValueCollection collection = HttpUtility.ParseQueryString(query);
        return collection.AllKeys.Where(k => !string.IsNullOrWhiteSpace(k)).ToDictionary(k => k!, k => collection[k]);
    }


    /// <summary>
    /// Combine query with parameters or not with given parameters
    /// </summary>
    /// <param name="absolutePath">Query Uri with parameters/without parameters</param>
    /// <param name="ignoreEmpty">true - empty parameter values will be excluded with their keys</param>
    /// <param name="queryParameters"></param>
    /// <returns>String with combined parameters</returns>
    /// <exception cref="InvalidCastException">Empty absolute path is not allowed</exception>
    public static string CombineQueryWithParameters(string absolutePath, bool ignoreEmpty, params KeyValuePair<string, string?>[] queryParameters)
    {
        if (string.IsNullOrWhiteSpace(absolutePath.Trim())) throw new InvalidCastException("Can't combine empty query string with parameters");

        int startParametersIndex = absolutePath.IndexOf('?');
        bool hasParameters = startParametersIndex > 0 && startParametersIndex < absolutePath.Length;
        string parameters = queryParameters.Length > 0 ? (hasParameters ? '&' : '?') + CombineQueryParameters(ignoreEmpty, queryParameters) : string.Empty;
        return string.Concat(absolutePath, parameters);
    }

    public static string CombineQueryWithParameters(string absolutePath, string parameters, bool ignoreEmpty)
    {
        if (string.IsNullOrWhiteSpace(absolutePath.Trim())) throw new InvalidCastException("Can't combine empty query string with parameters");

        int startParametersIndex = absolutePath.IndexOf('?');
        bool hasParameters = startParametersIndex > 0 && startParametersIndex < absolutePath.Length;
        Dictionary<string, string?> queryParameters = GetQueryParameters(parameters);
        string parametersString = queryParameters.Count > 0 ? (hasParameters ? '&' : '?') + CombineQueryParameters(ignoreEmpty, queryParameters.ToArray()) : string.Empty;
        return string.Concat(absolutePath, parametersString);
    }

    public static string CombineQueryWithParameters(string absolutePath, string route, bool ignoreEmpty, params KeyValuePair<string, string?>[] queryParameters) =>
        CombineQueryWithParameters(AppendPathSafely(absolutePath, route), ignoreEmpty, queryParameters);

    public static string GetBasePath(string absolutePath) => 
        TryGetBasePath(absolutePath, out string? basePath) ? basePath! : throw new InvalidCastException("Can't convert invalid path");

    public static string GetBasePath(Uri absolutePath) => GetBasePath(absolutePath.ToString());

    public static bool TryGetBasePath(string? absolutePath, out string? basePath)
    {
        bool isSuccess = false;
        basePath = null;
        if (string.IsNullOrWhiteSpace(absolutePath)) return isSuccess;

        if (Uri.TryCreate(absolutePath, UriKind.Absolute, out Uri? url) && HasValidScheme(url))
        {
            basePath = $"{url!.Scheme}{Uri.SchemeDelimiter}{url!.Authority}";
            isSuccess = true;
        }
        return isSuccess;
    }

    public static bool TryGetBasePath(Uri? absolutePath, out string? basePath)
    {
        bool isSuccess = false;
        basePath = null;
        if (Uri.IsWellFormedUriString(absolutePath?.ToString(), UriKind.Absolute) == false) return isSuccess;

        if (HasValidScheme(absolutePath))
        {
            basePath = $"{absolutePath.Scheme}{Uri.SchemeDelimiter}{absolutePath.Authority}";
            isSuccess = true;
        }
        return isSuccess;
    }

    /// <summary>
    /// Append given segments to the end of the absolute path of type <see cref="Uri"/>
    /// </summary>
    /// <param name="absolutePath">Only Uri of <see cref="UriKind.Absolute"/> is allowed</param>
    /// <param name="segments">Array ['/route/route', 'route' '/route?param=paramValue']</param>
    /// <returns></returns>
    public static Uri AppendToEnd(Uri absolutePath, params string[] segments)
    {
        QueryValidator.ThrowIfNotAbsolute(absolutePath);
        return AppendToEnd($"{absolutePath.Scheme}{Uri.SchemeDelimiter}{absolutePath.Authority}{absolutePath.LocalPath}", segments);
    }

    /// <summary>
    /// Append given segments to the end of the absolute path of type <see cref="string"/>
    /// </summary>
    /// <param name="absolutePath">Only Uri of <see cref="UriKind.Absolute"/> is allowed</param>
    /// <param name="segments">Array ['/route/route', 'route' '/route?param=paramValue']</param>
    /// <returns></returns>
    public static Uri AppendToEnd(string absolutePath, params string[] segments)
    {
        QueryValidator.ThrowIfNotAbsolute(absolutePath);
        return new Uri(segments.Aggregate(absolutePath, (current, path) =>
        {
            return string.Format("{0}/{1}", current.Trim().TrimEnd('/'), path.Trim().TrimStart('/'));
        }), UriKind.Absolute);
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
            string parameters = Uri.TryCreate(path, UriKind.Absolute, out Uri? url) ? url!.Query : string.Empty;
            return autoEncode ? HttpUtility.UrlEncode(CombineQueryWithParameters(queryPath, parameters, true)) : CombineQueryWithParameters(queryPath, parameters, true);
        }

        return autoEncode ? HttpUtility.UrlEncode(queryPath) : queryPath;
    }

    /// <summary>
    /// Check if Uri scheme is valid
    /// </summary>
    /// <param name="absolutePath"></param>
    /// <returns>true if path scheme is valid</returns>
    public static bool HasValidScheme(Uri absolutePath)
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
        return validSchemes.Contains(absolutePath.Scheme);
    }
}


