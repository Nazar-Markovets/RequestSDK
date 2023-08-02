using RequestSDK.Services;
using RequestSDK.Test.Base;
using RequestSDK.Test.ClassData;
using System.Net.Mime;
using Xunit.Abstractions;

namespace RequestSDK.Test.Services
{
    public class RequestOptions_Testing : FixtureBase
    {
        public RequestOptions_Testing(ITestOutputHelper consoleWriter) : base(consoleWriter){}

        [Fact(DisplayName = "Creating. SDK routing")]
        public void RequestOptionsSdkRoutingInitialization() =>
            Assert.Null(Record.Exception(() => new RequestService.Options(AccemblyRouting.ActionRouting.UpdateMessage)));

        [Fact(DisplayName = "Creating. Empty HttpClientId")]
        public void RequestOptionsHttpClientEmptyInitialization() =>
            Assert.Null(Record.Exception(() => new RequestService.Options(HttpMethod.Get, "action/get?Trace=1&Name=Test")));

        [Fact(DisplayName = "Creating. Not empty generic HttpClientId")]
        public void RequestOptionsCustomOptions() =>
            Assert.Null(Record.Exception(() => new RequestService.Options(HttpMethod.Get, "action/get?Trace=1&Name=Test").AddHttpClientId(OptionsDefault.OptionsType.UseGenericHttpClientId)));

        [Fact(DisplayName = "Creating. Not empty HttpClientId")]
        public void RequestOptionsHttpClientInitialization() =>
            Assert.Null(Record.Exception(() => new RequestService.Options(HttpMethod.Get, "action/get?Trace=1&Name=Test").AddHttpClientId(1)));

        [Fact(DisplayName = "Creating. Custom Options")]
        public void RequestOptions_Custom_Initialization() => Assert.Null(Record.Exception(() => OptionsCustom.GenerateInstance(HttpMethod.Get, "action/get?Trace=1&Name=Test")));

        [Theory(DisplayName = "Creating. Default Options. Completetion")]
        [ClassData(typeof(OptionsDefault))]
        public void RequestOptions_Default_Options_Completetion(RequestService.Options option) =>
            Assert.True(option!.CompletionOption == HttpCompletionOption.ResponseContentRead, $"Parameter {nameof(RequestService.Options.CompletionOption)} is not null");

        [Theory(DisplayName = "Creating. Default Options. Content Type")]
        [ClassData(typeof(OptionsDefault))]
        public void RequestOptions_Default_Options_ContentType(RequestService.Options option) =>
            Assert.True(option!.ContentType == "application/json", $"Parameter  {nameof(RequestService.Options.ContentType)} is not equal 'application/json' by default");

        [Theory(DisplayName = "Creating. Custom Options. Completetion")]
        [ClassData(typeof(OptionsCustom))]
        public void RequestOptions_Custom_Initialization_Completetion(RequestService.Options option) =>
            Assert.True(option.CompletionOption == HttpCompletionOption.ResponseHeadersRead, $"Parameter {nameof(RequestService.Options.CompletionOption)} was changed after initialization");


        [Theory(DisplayName = "Creating. Custom Options. Authentication")]
        [ClassData(typeof(OptionsCustom))]
        public void RequestOptions_Custom_Initialization_Authentication(RequestService.Options option)
        {
            Assert.True(option.Authentication!.Scheme == "Bearer", $"Parameter {nameof(RequestService.Options.Authentication)} has changed authentication scheme after initialization");
            Assert.True(option.Authentication.Parameter == "XXXX-AUTHORIZATION", $"Parameter {nameof(RequestService.Options.Authentication)} has changed authentication parameter after initialization");
        }

        [Theory(DisplayName = "Creating. Custom Options. ContentType")]
        [ClassData(typeof(OptionsCustom))]
        public void RequestOptions_Custom_Initialization_ContentType(RequestService.Options option)
        {
            Assert.True(option.ContentType == MediaTypeNames.Application.Rtf, $"Parameter {nameof(RequestService.Options.ContentType)} was changed after initialization");
        }

        [Theory(DisplayName = "Creating. Custom Options. CustomHeaders")]
        [ClassData(typeof(OptionsCustom))]
        public void RequestOptions_Custom_Initialization_CustomHeaders(RequestService.Options option)
        {
            string? customHeader = string.Empty;
            Assert.True(option.CustomHeaders!.Count == 1 && option.CustomHeaders.TryGetValue("XXX-HEADER", out customHeader), $"Parameter {nameof(RequestService.Options.CustomHeaders)} was changed after initialization");
            Assert.True(customHeader == "HEADER_VALUE", $"Parameter {nameof(RequestService.Options.CustomHeaders)} has changed value after initialization");
        }

        [Theory(DisplayName = "Creating. Custom Options. CustomActionFlags")]
        [ClassData(typeof(OptionsCustom))]
        public void RequestOptions_Custom_Initialization_CustomActionFlags(RequestService.Options option)
        {
            object? customActionFlag = null;
            Assert.True(option.CustomActionFlags!.Count == 1 && option.CustomActionFlags.TryGetValue("StopAction", out customActionFlag), $"Parameter {nameof(RequestService.Options.CustomActionFlags)} was changed after initialization");
            Assert.True((bool?)customActionFlag == true, $"Parameter {nameof(RequestService.Options.CustomActionFlags)} has changed value after initialization");
        }


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
            Assert.Null(Record.Exception(() => OptionsDefault.GenerateInstance(method, route)));
        }
    }
}
