using System.Net;
using Microsoft.AspNetCore.Http;
using System.Net.Http.Json;
using RequestSDK.Services;
using RequestSDK.Test.ClassData;
using Xunit.Sdk;
using System.Reflection;
using System.Text;
using RequestSDK.Test.Exceptions;

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
            AsParallelSync(() => Assert.Same(expectedMethod, _requestMessage.Method));
            return this;
        }

        public RequestCheckHelper CheckRequestAcceptType(params string[] expectedAcceptType)
        {
            AsParallelSync(() => Assert.Equal(expectedAcceptType, _requestMessage.Headers.Accept.Select(h => h.MediaType)));
            return this;
        }

        public RequestCheckHelper CheckRequestUrl(string expectedUrl)
        {
            AsParallelSync(() => Assert.Equal(new Uri(expectedUrl), _requestMessage.RequestUri));
            return this;
        }

        public RequestCheckHelper CheckRequestParameters(params KeyValuePair<string, string>[] parameters)
        {
            AsParallelSync(() => Assert.Equal(parameters, RequestService.GetQueryParameters(_requestMessage.RequestUri!)!));
            return this;
        }

        public RequestCheckHelper CheckRequestHeaders(params KeyValuePair<string, string>[] headers)
        {
            AsParallelSync(() =>
            {
                KeyValuePair<string, string>[] targetHeaders = 
                    _requestMessage.Headers.Select(h => new KeyValuePair<string, string>(h.Key, string.Join(", ", h.Value)))
                                           .ToArray();

                StringBuilder errorBuilder = new($"Expected Request Headers not found.{Environment.NewLine}");
                List<string> notFoundHeaders = new();
                foreach (KeyValuePair<string, string> header in headers)
                {
                    if(targetHeaders.Contains(header) is false)
                       notFoundHeaders.Add(header.ToString());
                }
                errorBuilder.Append("Expected headers :\n\t").AppendJoin("\n\t", notFoundHeaders).Append(Environment.NewLine);
                errorBuilder.Append("Actual headers :\n\t").AppendJoin("\n\t", targetHeaders);
                if (notFoundHeaders.Any()) throw new Exceptions.NotSupportedException(errorBuilder.ToString());
            });
            return this;
        }

        public RequestCheckHelper CheckRequestContent<T>(T expectedContent) => (RequestCheckHelper)CheckHttpContent(expectedContent, _requestMessage.Content);
    }
}
