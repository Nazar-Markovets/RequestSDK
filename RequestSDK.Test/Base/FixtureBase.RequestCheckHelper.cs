using System.Net;
using Microsoft.AspNetCore.Http;
using System.Net.Http.Json;
using RequestSDK.Services;
using RequestSDK.Test.ClassData;
using Xunit.Sdk;
using System.Reflection;

namespace RequestSDK.Test.Base;

public partial class FixtureBase
{
    protected sealed class RequestCheckHelper : CheckHelper
    {
        private readonly HttpRequestMessage _requestMessage = default!;

        protected override string RevisorName => "Request Revisor";

        public RequestCheckHelper(HttpRequestMessage httpRequestMessage) => _requestMessage = httpRequestMessage;
        
        public RequestCheckHelper CheckRequestMethod(HttpMethod expectedMethod)
        {
            AsParallel(() => Assert.Same(expectedMethod, _requestMessage.Method));
            return this;
        }

        public RequestCheckHelper CheckRequestAcceptType(string expectedAcceptType)
        {
            AsParallel(() => Assert.Equal(expectedAcceptType, _requestMessage.Headers.Accept.First().MediaType));
            return this;
        }

        public RequestCheckHelper CheckRequestUrl(string expectedUrl)
        {
            AsParallel(() => Assert.Equal(new Uri(expectedUrl), _requestMessage.RequestUri));
            return this;
        }

        public RequestCheckHelper CheckRequestContent<T>(T expectedContent)
        {
            AsParallel(async() =>
            {
                if (MockRequestHelper.IsPrimitiveType(expectedContent))
                {
                    string? content = await(_requestMessage.Content?.ReadAsStringAsync() ?? Task.FromResult(string.Empty));
                    Assert.Equal(expectedContent?.ToString(), content);
                }
                else
                {
                    T? content = await(_requestMessage.Content?.ReadFromJsonAsync<T>() ?? Task.FromResult<T?>(default));
                    Assert.Equal(expectedContent, content);
                }
            });

            return this;
        }
    }
}
