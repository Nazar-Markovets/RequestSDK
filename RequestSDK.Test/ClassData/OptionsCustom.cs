using RequestSDK.Services;
using System.Collections;
using System.Net.Http.Headers;
using System.Net.Mime;

namespace RequestSDK.Test.ClassData;

public sealed class OptionsCustom : IEnumerable<object[]>
{
    public static readonly Func<HttpMethod, string, RequestService.Options> GenerateInstance = (method, route) =>
        new RequestService.Options(method, route)
        {
            Authentication = new AuthenticationHeaderValue("Bearer", "XXXX-AUTHORIZATION"),
            ContentType = MediaTypeNames.Application.Rtf,
            CompletionOption = HttpCompletionOption.ResponseHeadersRead,
            CustomActionFlags = new Dictionary<string, object?> { { "StopAction", true } },
        }.
        AddHttpClientId(1).
        AddHeader("XXX-HEADER", "HEADER_VALUE").
        AddAcceptTypes(MediaTypeNames.Application.Json, MediaTypeNames.Application.Xml).
        AddRequestParameters(RequestService.RequestParameter("Parameter1", "Parameter1_Value")!,
                             RequestService.RequestParameter("Parameter2", "Parameter2_Value")!);

    public IEnumerator<object[]> GetEnumerator()
    {
        yield return new object[] { GenerateInstance(HttpMethod.Get, "action/get?Trace=1&Name=Test") };
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
