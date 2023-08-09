using RequestSDK.Services;
using RequestSDK.Test.Base;
using RequestSDK.Test.Integration;
using Xunit.Abstractions;
using System.Net;
using static RequestSDK.Test.ClassData.AccemblyRouting;
using RequestSDK.Test.ClassData;

namespace RequestSDK.Test.Services;


[Trait("Test", "Request Service Usage")]
public partial class RequestService_Testing : FixtureBase
{
    public RequestService_Testing(ITestOutputHelper consoleWriter, ServerInstanceRunner server) : base(consoleWriter, server)
    {
    }

    [Fact(DisplayName = "Create Request. Registered Client. Empty client base path")]
    public async Task RequestService_RegisteredClient_EmptyBasePath()
    {

        HttpResponseMessage responseMessage = MockHelper.CreateOfflineResponse(HttpStatusCode.OK, ResponseContent);
        RequestService.HttpClientSettings httpClientSettings = new() { HttpClientId = 1, HttpClientName = TargetClientId };
        HttpClient httpClient = MockHelper.CreateOfflineHttpClient(responseMessage, default, requestChecks =>
        {
            requestChecks.CheckRequestMethod(HttpMethod.Get)
                         .CheckRequestUrl(ClientBaseURL)
                         .CheckRequestAcceptType("*/*");
        });
        IHttpClientFactory factory = MockHelper.CreateOfflineFactory(httpClient, httpClientSettings);
        RequestService.RequestServiceOptions requestOptions = new(factory, httpClientSettings);

        RequestService requestService = new(requestOptions);
        HttpResponseMessage response = await requestService.ExecuteRequestAsync(new RequestService.Options(HttpMethod.Get, ClientBaseURL, httpClientSettings.HttpClientId));

        MockHelper.ValidateResponse(responseChecks =>
        {
            responseChecks.CheckResponseStatusCode(HttpStatusCode.OK)
                          .CheckResponseContent(ResponseContent);
        });
    }

    [Fact(DisplayName = "Create Request. Registered Client. Not empty client base path")]
    public async Task RequestService_RegisteredClient_Not_EmptyBasePath()
    {

        HttpResponseMessage offlineResponse = MockHelper.CreateOfflineResponse(HttpStatusCode.OK, ResponseContent);
        RequestService.HttpClientSettings httpClientSettings = new() { HttpClientId = 1, HttpClientName = TargetClientId };

        HttpClient httpClient = MockHelper.CreateOfflineHttpClient(offlineResponse, ClientBaseURL, requestChecks =>
        {
            requestChecks.CheckRequestMethod(HttpMethod.Get)
                         .CheckRequestUrl($"{ClientBaseURL}/controller/action")
                         .CheckRequestAcceptType("*/*");
        });
        IHttpClientFactory factory = MockHelper.CreateOfflineFactory(httpClient, httpClientSettings);
        RequestService.RequestServiceOptions requestOptions = new(factory, httpClientSettings);
        
        RequestService requestService = new(requestOptions);
        HttpResponseMessage response = await requestService.ExecuteRequestAsync(new RequestService.Options(HttpMethod.Get, "controller/action", httpClientSettings.HttpClientId));

        MockHelper.ValidateResponse(responseChecks =>
        {
            responseChecks.CheckResponseStatusCode(HttpStatusCode.OK)
                          .CheckResponseContent(ResponseContent);
        });
    }

    
    [Fact(DisplayName = "Create Request. Without Registered Client")]
    public async Task RequestService_WithoutClient_WithPath()
    {
        const string overrideRequestBasePath = "https://override.com/login/example";
        

        HttpResponseMessage offlineResponse = MockHelper.CreateOfflineResponse(HttpStatusCode.OK, ResponseContent);
        HttpClient httpClient = MockHelper.CreateOfflineHttpClient(offlineResponse, default, requestChecks => 
        { 
            requestChecks.CheckRequestMethod(HttpMethod.Get)
                         .CheckRequestUrl(overrideRequestBasePath)
                         .CheckRequestAcceptType("*/*"); 
        });
        
        RequestService requestService = new(httpClient);
        HttpResponseMessage response = await requestService.ExecuteRequestAsync(new RequestService.Options(HttpMethod.Get, overrideRequestBasePath));

        MockHelper.ValidateResponse(responseChecks =>
        {
            responseChecks.CheckResponseStatusCode(HttpStatusCode.OK)
                          .CheckResponseContent(ResponseContent);
        });
    }

    [Fact(DisplayName = "Create Request. Without Registered Client. Use SDK Routing")]
    public async Task RequestService_WithoutClient_SDKRouting()
    {
        HttpResponseMessage offlineResponse = MockHelper.CreateOfflineResponse(HttpStatusCode.OK, ResponseContent);
        HttpClient httpClient = MockHelper.CreateOfflineHttpClient(offlineResponse, ClientBaseURL, requestChecks =>
        {
            requestChecks.CheckRequestMethod(HttpMethod.Patch)
                         .CheckRequestUrl("https://example.com/action/message/update")
                         .CheckRequestAcceptType("*/*");
        });

        RequestService requestService = new(httpClient)
        {
            SetupOptions = new (){ AccemblyRoutingType = typeof(AccemblyRouting) }
        };
        HttpResponseMessage response = await requestService.ExecuteRequestAsync(new RequestService.Options(ActionRouting.UpdateMessage));

        MockHelper.ValidateResponse(responseChecks =>
        {
            responseChecks.CheckResponseStatusCode(HttpStatusCode.OK)
                          .CheckResponseContent(ResponseContent);
        });
    }

    [Fact(DisplayName = "Create Request. Registered Client. Send Primitive")]
    public async Task RequestService_RegisteredClient_SendPrimitive()
    {
        const string RequestMessage = nameof(RequestService_RegisteredClient_SendPrimitive);
        HttpResponseMessage offlineResponse = MockHelper.CreateOfflineResponse(HttpStatusCode.OK, ResponseContent);
        RequestService.HttpClientSettings httpClientSettings = new() { HttpClientId = 1, HttpClientName = TargetClientId };
        HttpClient httpClient = MockHelper.CreateOfflineHttpClient(offlineResponse, ClientBaseURL, requestChecks =>
        {
            requestChecks.CheckRequestMethod(HttpMethod.Get)
                         .CheckRequestUrl($"{ClientBaseURL}/controller/action")
                         .CheckRequestAcceptType("*/*")
                         .CheckRequestContent(RequestMessage);
        });
        IHttpClientFactory factory = MockHelper.CreateOfflineFactory(httpClient, httpClientSettings);
        RequestService.RequestServiceOptions requestOptions = new(factory, httpClientSettings);

        RequestService requestService = new(requestOptions);
        HttpResponseMessage response = await requestService.ExecuteRequestAsync(new RequestService.Options(HttpMethod.Get, "controller/action", httpClientSettings.HttpClientId), RequestMessage);

        MockHelper.ValidateResponse(responseChecks =>
        {
            responseChecks.CheckResponseStatusCode(HttpStatusCode.OK)
                          .CheckResponseContent(ResponseContent);
        });
    }

    [Fact(DisplayName = "Create Request. Registered Client. Get Object")]
    public async Task RequestService_RegisteredClient_GetObject()
    {

        HttpResponseMessage offlineResponse = MockHelper.CreateOfflineResponse(HttpStatusCode.OK, ResponseContent);
        RequestService.HttpClientSettings httpClientSettings = new() { HttpClientId = 1, HttpClientName = TargetClientId };
        HttpClient httpClient = MockHelper.CreateOfflineHttpClient(offlineResponse, ClientBaseURL, requestChecks =>
        {
            requestChecks.CheckRequestMethod(HttpMethod.Get)
                         .CheckRequestUrl($"{ClientBaseURL}/controller/action")
                         .CheckRequestAcceptType("*/*");
        });
        IHttpClientFactory factory = MockHelper.CreateOfflineFactory(httpClient, httpClientSettings);
        RequestService.RequestServiceOptions requestOptions = new(factory, httpClientSettings);

        RequestService requestService = new(requestOptions);
        HttpResponseMessage response = await requestService.ExecuteRequestAsync(new RequestService.Options(HttpMethod.Get, "controller/action", httpClientSettings.HttpClientId));

        MockHelper.ValidateResponse(responseChecks =>
        {
            responseChecks.CheckResponseStatusCode(HttpStatusCode.OK)
                          .CheckResponseContent(ResponseContent);
        });
    }
}
