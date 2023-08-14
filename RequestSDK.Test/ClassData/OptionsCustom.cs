using RequestSDK.Services;

using System.Collections;
using System.Net.Http.Headers;
using System.Net.Mime;

namespace RequestSDK.Test.ClassData;

public sealed class OptionsCustom : IEnumerable<object[]>
{
    public static readonly Func<HttpMethod, string, RequestService.Options> GenerateInstance = (method, route) =>
            RequestService.Options.WithRegisteredClient(method, route, 1)
                                  .AddAuthentication(scheme => new AuthenticationHeaderValue(scheme.Bearer, "XXXX-AUTHORIZATION"))
                                  .AddCustomFlags("StopAction", true)
                                  .AddHeader("XXX-HEADER", "HEADER_VALUE")
                                  .ForStreamResponse()
                                  .AddAcceptTypes(MediaTypeNames.Application.Json, MediaTypeNames.Application.Xml)
                                  .AddContentType(MediaTypeNames.Application.Rtf)
                                  .AddRequestParameters(RequestService.RequestParameter("Parameter1", "Parameter1_Value")!,
                                                        RequestService.RequestParameter("Parameter2", "Parameter2_Value")!);

    public IEnumerator<object[]> GetEnumerator()
    {
        yield return new object[] { GenerateInstance(HttpMethod.Get, "action/get?Trace=1&Name=Test") };
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
