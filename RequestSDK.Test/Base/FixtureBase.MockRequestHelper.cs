using System.Net;
using System.Net.Mime;
using System.Text;
using System.Text.Json;

using Moq.Protected;
using Moq;
using RequestSDK.Services;

namespace RequestSDK.Test.Base;

public partial class FixtureBase
{
    protected class MockRequestHelper
    {

        internal static HttpResponseMessage CreateOfflineResponse<T>(HttpStatusCode statusCode, T content)
        {

            HttpResponseMessage responseMessage = new()
            {
                Content = RequestService.GetCorrectHttpContent(content, MediaTypeNames.Application.Json),
                StatusCode = statusCode,
            };

            return responseMessage;
        }

        internal static HttpClient CreateOfflineHttpClient(HttpResponseMessage offlineResponse, string? clientBaseUrl = default, Action<RequestCheckHelper>? requestCheckActions = default)
        {
            Mock<HttpMessageHandler> mockHttpMessageHandler = new();
            mockHttpMessageHandler.Protected()
                                  .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                                  .Callback<HttpRequestMessage, CancellationToken>((requestMessage, cancelationToken) =>
                                  {
                                      RequestCheckHelper handler = new(requestMessage);
                                      requestCheckActions?.Invoke(handler);
                                      var result = Task.Run(async () => await handler.RunParallelChecks()).Result;
                                      if (result != null) throw result;
                                  }).ReturnsAsync(offlineResponse);


            return new(mockHttpMessageHandler.Object) { BaseAddress = clientBaseUrl == null ? default : new Uri(clientBaseUrl) };
        }

        internal static IHttpClientFactory CreateOfflineFactory(HttpClient offlineHttpClient, RequestService.HttpClientSettings httpClientSettings)
        {
            Mock<IHttpClientFactory> clientsFactory = new();
            clientsFactory.Setup(factory => factory.CreateClient(httpClientSettings.HttpClientName)).Returns(offlineHttpClient);
            return clientsFactory.Object;
        }

        internal static IHttpClientFactory CreateOfflineFactory(params KeyValuePair<HttpClient, RequestService.HttpClientSettings>[] httpClients)
        {
            Mock<IHttpClientFactory> clientsFactory = new();
            httpClients.ToList().ForEach(offlineClient => clientsFactory.Setup(factory => factory.CreateClient(offlineClient.Value.HttpClientName)).Returns(offlineClient.Key));
            
            return clientsFactory.Object;
        }

        internal static void ValidateResponse(HttpResponseMessage targetResponseMessage, Action<ResponseCheckHelper> checks)
        {
            ResponseCheckHelper handler = new(targetResponseMessage);
            checks?.Invoke(handler);
            var result = Task.Run(async () => await handler.RunParallelChecks()).Result;
            if (result != null) throw result;
        }
    }
}
