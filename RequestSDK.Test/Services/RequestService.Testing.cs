using Moq.Protected;
using Moq;
using RequestSDK.Attributes;
using RequestSDK.Services;
using RequestSDK.Test.Base;
using RequestSDK.Test.Integration;
using Xunit.Abstractions;
using System.Net;

namespace RequestSDK.Test.Services;


[Trait("Test", "Request Service Usage")]
public partial class RequestService_Testing : FixtureBase
{
    public RequestService_Testing(ITestOutputHelper consoleWriter, ServerInstanceRunner server) : base(consoleWriter, server)
    {
    }

    


    [Fact(DisplayName = "Create Request. Registered Client. Empty client base path")]
    public async Task RequestOptions_Initialization()
    {
        HttpResponseMessage responseMessage = MockHelper.GenerateResponseMessage(HttpStatusCode.OK, ResponseContent);

        RequestService.HttpClientSettings httpClientSettings = new() { HttpClientId = 1, HttpClientName = TargetClientId };
        HttpClient httpClient = MockHelper.CreateOfflineHttpClient(responseMessage);
        IHttpClientFactory factory = MockHelper.CreateOfflineFactory(httpClient, httpClientSettings);
        RequestService.RequestServiceOptions requestOptions = new()
        {
            Factory = factory,
            HttpClientSettings = new List<RequestService.HttpClientSettings>(){ httpClientSettings }
        };

        RequestService requestService = new(requestOptions);
        HttpResponseMessage response = await requestService.ExecuteRequestAsync(
                                             new RequestService.Options(HttpMethod.Get, RequestURL, httpClientSettings.HttpClientId), 
                                             httpContent: new StringContent("Request content"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        string content = await response.Content.ReadAsStringAsync();
        Assert.Equal(ResponseContent, content);

    }

    [SkippableFact(DisplayName = "Create Request. Registered Client. Not empty client base path")]
    public async Task HttpClientSettings_Initialization()
    {
        HttpResponseMessage responseMessage = MockHelper.GenerateResponseMessage(HttpStatusCode.OK, ResponseContent);
        RequestService.HttpClientSettings httpClientSettings = new() { HttpClientId = 1, HttpClientName = TargetClientId };
        HttpClient httpClient = MockHelper.CreateOfflineHttpClient(responseMessage, RequestURL, sendRequestChecks: (response) =>
        {
            Assert.Same(HttpMethod.Get, response.Method);
            Assert.Equal(RequestControllerUri, response.RequestUri);
        });
        IHttpClientFactory factory = MockHelper.CreateOfflineFactory(httpClient, httpClientSettings);
        RequestService.RequestServiceOptions requestOptions = new()
        {
            Factory = factory,
            HttpClientSettings = new List<RequestService.HttpClientSettings>() { httpClientSettings }
        };

        RequestService requestService = new(requestOptions);
        HttpResponseMessage response = await requestService.ExecuteRequestAsync(new RequestService.Options(HttpMethod.Get, "controller/action", 1), httpContent: new StringContent("Request content"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        string content = await response.Content.ReadAsStringAsync();
        Assert.Equal(ResponseContent, content);
    }
}

public class AccemblyRouting
{

    [ControllerName("Action")]
    public static class ActionRouting
    {
        [ControllerHttpMethod(Enums.HttpRequestMethod.Delete)]
        public const string DeleteMessage = "delete";

        [ControllerHttpMethod(Enums.HttpRequestMethod.Patch)]
        public const string UpdateMessage = "message/update";

    }

    [ControllerName("Home")]
    public static class HomeRouting
    {
        [ControllerHttpMethod(Enums.HttpRequestMethod.Delete)]
        public const string DeleteMessage = "delete";

        [ControllerHttpMethod(Enums.HttpRequestMethod.Patch)]
        public const string UpdateMessage = "message/update";

    }
}
