using System.Net;
using System.Net.Mime;
using System.Text;
using System.Text.Json;

using Moq.Protected;
using Moq;
using RequestSDK.Services;
using System.Reflection.PortableExecutable;
using System.Net.Http;
using Castle.DynamicProxy;

namespace RequestSDK.Test.Base;

public partial class FixtureBase
{
    protected class MockRequestHelper
    {
        private HttpResponseMessage _offlineResponseMessage = default!;

        internal static bool IsPrimitiveType<T>(T _)
        {
            Type type = typeof(T);

            bool isStruct = type.IsValueType && !type.IsPrimitive;
            bool isClass = type.IsClass && !type.Equals(typeof(string));
            return isClass is false || isStruct is false;
        }

        private StringContent? CorrectContentFormat<T>(T content)
        {
            return IsPrimitiveType(content)
                   ? new StringContent(content!.ToString()!)
                   : new StringContent(JsonSerializer.Serialize(content), Encoding.UTF8, MediaTypeNames.Application.Json);
        }

        internal HttpResponseMessage CreateOfflineResponse<T>(HttpStatusCode statusCode, T content)
        {

            HttpResponseMessage responseMessage = new()
            {
                Content = CorrectContentFormat(content),
                StatusCode = statusCode
            };

            return responseMessage;
        }

        internal HttpClient CreateOfflineHttpClient(HttpResponseMessage offlineResponse, string? clientBaseUrl = default, Action<RequestCheckHelper>? requestCheckActions = default)
        {
            Mock<HttpMessageHandler> mockHttpMessageHandler = new();
            _offlineResponseMessage = offlineResponse;
            mockHttpMessageHandler.Protected()
                                  .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                                  .Callback<HttpRequestMessage, CancellationToken>((requestMessage, cancelationToken) =>
                                  {
                                      RequestCheckHelper handler = new(requestMessage);
                                      requestCheckActions?.Invoke(handler);
                                      var result = Task.Run(async () => await handler.RunParallelChecks()).Result;
                                      if (result != null) throw result;
                                  })
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

        internal void ValidateResponse(HttpResponseMessage targetResponseMessage, Action<ResponseCheckHelper> checks)
        {
            ResponseCheckHelper handler = new(targetResponseMessage);
            checks?.Invoke(handler);
            var result = Task.Run(async () => await handler.RunParallelChecks()).Result;
            if (result != null) throw result;
        }

        internal void ValidateResponse(Action<ResponseCheckHelper> checks)
        {
            ResponseCheckHelper handler = new(_offlineResponseMessage);
            checks?.Invoke(handler);
            var result = Task.Run(async () => await handler.RunParallelChecks()).Result;
            if (result != null) throw result;
        }
    }
}
