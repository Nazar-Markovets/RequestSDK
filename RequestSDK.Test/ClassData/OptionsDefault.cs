using RequestSDK.Services;
using System.Collections;

namespace RequestSDK.Test.ClassData
{
    public class OptionsDefault : IEnumerable<object[]>
    {
        public enum OptionsType : byte { UseSdkRouting, EmptyHttpClientId, UseHttpClientId, UseGenericHttpClientId }

        public static readonly Func<HttpMethod, string, RequestService.Options> GenerateInstance = (method, route) => new RequestService.Options(method, route);

        public IEnumerator<object[]> GetEnumerator()
        {
            yield return new object[] { new RequestService.Options(AccemblyRouting.ActionRouting.UpdateMessage) };
            yield return new object[] { GenerateInstance(HttpMethod.Post, "action/get?Trace=1&Name=Test") };
            yield return new object[] { GenerateInstance(HttpMethod.Post, "action/get?Trace=1&Name=Test").AddHttpClientId(1) };
            yield return new object[] { GenerateInstance(HttpMethod.Post, "action/get?Trace=1&Name=Test").AddHttpClientId(OptionsType.UseGenericHttpClientId) };
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
