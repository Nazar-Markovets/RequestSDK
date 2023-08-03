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

    [Fact(DisplayName = "Creating Request with registered clients")]
    public async Task RequestOptions_Initialization()
    {
        HttpResponseMessage responseMessage = new(HttpStatusCode.OK) { Content = new StringContent("Mocked response") };

        Mock<IHttpClientFactory> clientsFactory = new();
        Mock<HttpMessageHandler> mockHttpMessageHandler = new();
        
        mockHttpMessageHandler.Protected()
                              .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                              .ReturnsAsync(responseMessage);


        HttpClient registeredHttpClient = new(mockHttpMessageHandler.Object);

        clientsFactory.Setup(x => x.CreateClient("TestAPI")).Returns(registeredHttpClient);
        RequestService.RequestServiceOptions requestOptions = new()
        {
            Factory = clientsFactory.Object,
            Clients = new List<RequestService.HttpClientSettings>()
            {
                new RequestService.HttpClientSettings(){ HttpClientId = 1, HttpClientName = "TestAPI" }
            }
        };

        var requestService = new RequestService(requestOptions);
        var result = await requestService.ExecuteRequestAsync(new RequestService.Options(HttpMethod.Get, "https://example.com", 1), httpContent : new StringContent("Request content"));

        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        var content = await result.Content.ReadAsStringAsync();
        Assert.Equal("Mocked response", content);

    }

    [Fact(DisplayName = "Creating Variable")]
    public async Task HttpClientSettings_Initialization()
    {
        if(Convert.ToBoolean(Environment.GetEnvironmentVariable("GIT_WORKFLOW")))
        {

        }
        else
        {
            Assert.Fail("NO VARIABLE");
        }
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
