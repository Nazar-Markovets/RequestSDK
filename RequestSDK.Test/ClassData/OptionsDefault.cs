using RequestSDK.Services;
using System.Collections;

namespace RequestSDK.Test.ClassData;

public sealed class OptionsDefault : IEnumerable<object[]>
{
    public enum OptionsType : byte { UseSdkRouting, EmptyHttpClientId, UseHttpClientId, UseGenericHttpClientId }

    public static readonly Func<HttpMethod, string, object?, RequestService.Options> GenerateInstance = (method, route, clientId) =>
    {
        if (clientId == null) return RequestService.Options.WithoutRegisteredClient(method, route);
        if (clientId is byte intClientId) return RequestService.Options.WithRegisteredClient(method, route, intClientId);
        if (clientId is OptionsType enumClientId) return RequestService.Options.WithRegisteredClient(method, route, enumClientId);
        throw new Exception("Not suitable case");
    };

    public IEnumerator<object[]> GetEnumerator()
    {
        yield return new object[] { RequestService.Options.WithRegisteredRouting(AccemblyRouting.ActionRouting.UpdateMessage) };
        yield return new object[] { GenerateInstance(HttpMethod.Post, "action/get?Trace=1&Name=Test", null) };
        yield return new object[] { GenerateInstance(HttpMethod.Post, "action/get?Trace=1&Name=Test", (byte)1) };
        yield return new object[] { GenerateInstance(HttpMethod.Post, "action/get?Trace=1&Name=Test", OptionsType.UseGenericHttpClientId) };
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
