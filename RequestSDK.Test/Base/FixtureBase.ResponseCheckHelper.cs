using System.Net;
using Microsoft.AspNetCore.Http;
using System.Net.Http.Json;
using RequestSDK.Services;

namespace RequestSDK.Test.Base;

public partial class FixtureBase
{
    protected sealed class ResponseCheckHelper : CheckHelper
    {
        private readonly HttpResponseMessage _responseMessage = default!;

        protected override string RevisorName => "Response Revisor";

        public ResponseCheckHelper(HttpResponseMessage httpResponseMessage) => _responseMessage = httpResponseMessage;


        public ResponseCheckHelper CheckResponseStatusCode(HttpStatusCode statusCode)
        {
            AsParallelSync(() => Assert.Equal(statusCode, _responseMessage.StatusCode));
            return this;
        }

        public ResponseCheckHelper CheckResponseContent<T>(T expectedContent) => (ResponseCheckHelper)CheckHttpContent(expectedContent, _responseMessage.Content);
    }
}
