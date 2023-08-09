using RequestSDK.Services;
using RequestSDK.Test.Base;
using RequestSDK.Test.Integration;
using Xunit.Abstractions;
using System.Net;
using static RequestSDK.Test.ClassData.AccemblyRouting;
using RequestSDK.Test.ClassData;
using System.Net.Mime;
using Microsoft.Net.Http.Headers;
using System.Net.Http.Headers;

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

        HttpResponseMessage responseMessage = MockRequestHelper.CreateOfflineResponse(HttpStatusCode.OK, ResponseContent);
        RequestService.HttpClientSettings httpClientSettings = new() { HttpClientId = 1, HttpClientName = TargetClientId };
        HttpClient httpClient = MockRequestHelper.CreateOfflineHttpClient(responseMessage, default, requestChecks =>
        {
            requestChecks.CheckRequestMethod(HttpMethod.Get)
                         .CheckRequestUrl(ClientBaseURL)
                         .CheckRequestAcceptType("*/*");
        });
        IHttpClientFactory factory = MockRequestHelper.CreateOfflineFactory(httpClient, httpClientSettings);
        RequestService.RequestServiceOptions requestOptions = new(factory, httpClientSettings);

        RequestService requestService = new(requestOptions);
        HttpResponseMessage response = await requestService.ExecuteRequestAsync(new RequestService.Options(HttpMethod.Get, ClientBaseURL, httpClientSettings.HttpClientId));

        MockRequestHelper.ValidateResponse(response, responseChecks =>
        {
            responseChecks.CheckResponseStatusCode(HttpStatusCode.OK)
                          .CheckResponseContent(ResponseContent);
        });
    }

    [Fact(DisplayName = "Create Request. Registered Client. Not empty client base path")]
    public async Task RequestService_RegisteredClient_Not_EmptyBasePath()
    {

        HttpResponseMessage offlineResponse = MockRequestHelper.CreateOfflineResponse(HttpStatusCode.OK, ResponseContent);
        RequestService.HttpClientSettings httpClientSettings = new() { HttpClientId = 1, HttpClientName = TargetClientId };

        HttpClient httpClient = MockRequestHelper.CreateOfflineHttpClient(offlineResponse, ClientBaseURL, requestChecks =>
        {
            requestChecks.CheckRequestMethod(HttpMethod.Get)
                         .CheckRequestUrl($"{ClientBaseURL}/controller/action")
                         .CheckRequestAcceptType("*/*");
        });
        IHttpClientFactory factory = MockRequestHelper.CreateOfflineFactory(httpClient, httpClientSettings);
        RequestService.RequestServiceOptions requestOptions = new(factory, httpClientSettings);
        
        RequestService requestService = new(requestOptions);
        HttpResponseMessage response = await requestService.ExecuteRequestAsync(new RequestService.Options(HttpMethod.Get, "controller/action", httpClientSettings.HttpClientId));

        MockRequestHelper.ValidateResponse(response, responseChecks =>
        {
            responseChecks.CheckResponseStatusCode(HttpStatusCode.OK)
                          .CheckResponseContent(ResponseContent);
        });
    }

    
    [Fact(DisplayName = "Create Request. Without Registered Client")]
    public async Task RequestService_WithoutClient_WithPath()
    {
        const string overrideRequestBasePath = "https://override.com/login/example";
        

        HttpResponseMessage offlineResponse = MockRequestHelper.CreateOfflineResponse(HttpStatusCode.OK, ResponseContent);
        HttpClient httpClient = MockRequestHelper.CreateOfflineHttpClient(offlineResponse, default, requestChecks => 
        { 
            requestChecks.CheckRequestMethod(HttpMethod.Get)
                         .CheckRequestUrl(overrideRequestBasePath)
                         .CheckRequestAcceptType("*/*"); 
        });
        
        RequestService requestService = new(httpClient);
        HttpResponseMessage response = await requestService.ExecuteRequestAsync(new RequestService.Options(HttpMethod.Get, overrideRequestBasePath));

        MockRequestHelper.ValidateResponse(response, responseChecks =>
        {
            responseChecks.CheckResponseStatusCode(HttpStatusCode.OK)
                          .CheckResponseContent(ResponseContent);
        });
    }

    [Fact(DisplayName = "Create Request. Without Registered Client. Use SDK Routing")]
    public async Task RequestService_WithoutClient_SDKRouting()
    {
        HttpResponseMessage offlineResponse = MockRequestHelper.CreateOfflineResponse(HttpStatusCode.OK, ResponseContent);
        HttpClient httpClient = MockRequestHelper.CreateOfflineHttpClient(offlineResponse, ClientBaseURL, requestChecks =>
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

        MockRequestHelper.ValidateResponse(response, responseChecks =>
        {
            responseChecks.CheckResponseStatusCode(HttpStatusCode.OK)
                          .CheckResponseContent(ResponseContent);
        });
    }

    [Theory(DisplayName = "Create Request. Registered Client. Max load")]
    [ClassData(typeof(RequestData))]
    public async Task RequestService_RegisteredClient_SendPrimitive(object ResponseContent, string ClientBasePath, string ClientRoutePath, 
                                                                    object RequestContent, string[] RequestAcceptTypes,
                                                                    KeyValuePair<string, string>[] RequestHeaders, 
                                                                    KeyValuePair<string, string>[] RequestQueryParameters)
    {
        HttpResponseMessage offlineResponse = MockRequestHelper.CreateOfflineResponse(HttpStatusCode.OK, ResponseContent);
        RequestService.HttpClientSettings httpClientSettings = new() { HttpClientId = 1, HttpClientName = TargetClientId };

        HttpClient httpClient = MockRequestHelper.CreateOfflineHttpClient(offlineResponse, ClientBasePath, requestChecks =>
        {
            requestChecks.CheckRequestMethod(HttpMethod.Get)
                         .CheckRequestUrl(RequestService.CombineQueryWithParameters($"{ClientBasePath}/{ClientRoutePath}", false, RequestQueryParameters!))
                         .CheckRequestAcceptType(RequestAcceptTypes)
                         .CheckRequestContent(RequestContent)
                         .CheckRequestParameters(RequestQueryParameters)
                         .CheckRequestHeaders(RequestHeaders);
        });
        IHttpClientFactory factory = MockRequestHelper.CreateOfflineFactory(httpClient, httpClientSettings);
        RequestService.RequestServiceOptions setupOptions = new(factory, httpClientSettings);

        RequestService requestService = new(setupOptions);
        RequestService.Options requestOptions = new RequestService.Options(HttpMethod.Get, ClientRoutePath)
                                                                  .AddHeaders(RequestHeaders!)
                                                                  .AddAcceptTypes(RequestAcceptTypes)
                                                                  .AddHttpClientId(httpClientSettings.HttpClientId)
                                                                  .AddRequestParameters(RequestQueryParameters!);
        HttpResponseMessage response = await requestService.ExecuteRequestAsync(requestOptions, RequestContent);

        MockRequestHelper.ValidateResponse(response, responseChecks =>
        {
            responseChecks.CheckResponseStatusCode(HttpStatusCode.OK)
                          .CheckResponseContent(ResponseContent);
        });
    }
    
}
