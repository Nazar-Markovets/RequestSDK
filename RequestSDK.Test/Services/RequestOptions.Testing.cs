using RequestSDK.Services;
using RequestSDK.Test.Base;
using RequestSDK.Test.ClassData;
using RequestSDK.Test.Integration;
using System.Net.Mime;
using Xunit.Abstractions;

namespace RequestSDK.Test.Services
{
    [Trait("Test", "Request Options")]
    public class RequestOptions_Testing : FixtureBase
    {
        public RequestOptions_Testing(ITestOutputHelper consoleWriter, ServerInstanceRunner server) : base(consoleWriter, server) {}

        [Fact(DisplayName = "Creating. SDK routing")]
        public void RequestOptionsSdkRoutingInitialization() =>
            Assert.Null(Record.Exception(() => RequestService.Options.WithRegisteredRouting(AccemblyRouting.ActionRouting.UpdateMessage)));

        [Fact(DisplayName = "Creating. Empty HttpClientId")]
        public void RequestOptionsHttpClientEmptyInitialization() =>
            Assert.Null(Record.Exception(() => RequestService.Options.WithoutRegisteredClient(HttpMethod.Get, "action/get?Trace=1&Name=Test")));

        [Fact(DisplayName = "Creating. Not empty generic HttpClientId")]
        public void RequestOptionsCustomOptions() =>
            Assert.Null(Record.Exception(() => RequestService.Options.WithRegisteredClient(HttpMethod.Get, "action/get?Trace=1&Name=Test", OptionsDefault.OptionsType.UseGenericHttpClientId)));

        [Fact(DisplayName = "Creating. Not empty HttpClientId")]
        public void RequestOptionsHttpClientInitialization() =>
            Assert.Null(Record.Exception(() => RequestService.Options.WithRegisteredClient(HttpMethod.Get, "action/get?Trace=1&Name=Test", 1)));

        [Fact(DisplayName = "Creating. Custom Options")]
        public void RequestOptions_Custom_Initialization() => Assert.Null(Record.Exception(() => OptionsCustom.GenerateInstance(HttpMethod.Get, "action/get?Trace=1&Name=Test")));

        


        [Theory(DisplayName = "Creating. Custom Options. All HTTP Methods")]
        [ClassData(typeof(HttpMethods))]
        public void RequestOptions_Custom_Initialization_HttpMethods(HttpMethod method, string route)
        {
            Assert.Null(Record.Exception(() => OptionsCustom.GenerateInstance(method, route)));
        }

        [Theory(DisplayName = "Creating. Default Options. All HTTP Methods")]
        [ClassData(typeof(HttpMethods))]
        public void RequestOptions_Default_Initialization_HttpMethods(HttpMethod method, string route)
        {
            Assert.Null(Record.Exception(() => OptionsDefault.GenerateInstance(method, route, null)));
        }
    }
}
