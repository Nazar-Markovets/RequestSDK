using System.Net;
using System.Net.Mime;
using System.Text;
using System.Text.Json;

using Moq.Protected;
using Moq;
using RequestSDK.Services;
using Castle.Components.DictionaryAdapter.Xml;
using System.Net.Http;

namespace RequestSDK.Test.Base;

public partial class FixtureBase
{
    protected class MockRequestHelper
    {
        
        private StringContent? CorrectContentFormat<T>(T content)
        {
            Type type = typeof(T);

            bool isStruct = type.IsValueType && !type.IsPrimitive;
            bool isClass = type.IsClass && !type.Equals(typeof(string));

            return isClass || isStruct 
                   ? new StringContent(JsonSerializer.Serialize(content), Encoding.UTF8, MediaTypeNames.Application.Json) 
                   : new StringContent(content!.ToString()!);
        }

        internal HttpResponseMessage GenerateResponseMessage<T>(HttpStatusCode statusCode, T content)
        {

            HttpResponseMessage responseMessage = new()
            {
                Content = CorrectContentFormat(content),
                StatusCode = statusCode
            };

            return responseMessage;
        }

        internal HttpClient CreateOfflineHttpClient(HttpResponseMessage offlineResponse, string? clientBaseUrl = default, Action<HttpRequestMessage>? sendRequestChecks = default)
        {
            Mock<HttpMessageHandler> mockHttpMessageHandler = new();

            mockHttpMessageHandler.Protected()
                                  .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                                  .Callback(new InvocationAction(invocation =>
                                  {
                                      HttpRequestMessage? typeArgument = invocation.Arguments[0] as HttpRequestMessage;
                                      if (invocation.Arguments[0] is HttpRequestMessage requestMessage) sendRequestChecks?.Invoke(requestMessage);
                                      else Assert.Fail("Offline HttpClient received wrong values");
                                  }))
                                  .ReturnsAsync(offlineResponse);


            return new(mockHttpMessageHandler.Object) { BaseAddress = clientBaseUrl == null ? default : new Uri(clientBaseUrl) };
        }

        internal IHttpClientFactory CreateOfflineFactory(HttpClient offlineHttpClient, RequestService.HttpClientSettings httpClientSettings)
        {
            Mock<IHttpClientFactory> clientsFactory = new();
            clientsFactory.Setup(factory => factory.CreateClient(httpClientSettings.HttpClientName)).Returns(offlineHttpClient);
            return clientsFactory.Object;
        }

        internal IHttpClientFactory CreateOfflineFactory(params KeyValuePair<HttpClient, RequestService.HttpClientSettings>[] httpClients)
        {
            Mock<IHttpClientFactory> clientsFactory = new();
            foreach (var offlineClient in httpClients)
            {
                clientsFactory.Setup(factory => factory.CreateClient(offlineClient.Value.HttpClientName)).Returns(offlineClient.Key);
            }
            return clientsFactory.Object;
        }
    }
}
