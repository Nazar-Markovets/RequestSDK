using System.Net;
using Microsoft.AspNetCore.Http;
using System.Net.Http.Json;

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
            AsParallel(() => Assert.Equal(statusCode, _responseMessage.StatusCode));
            return this;
        }

        public ResponseCheckHelper CheckResponseContent<T>(T expectedContent)
        {
            AsParallel(async () =>
            {
                if (MockRequestHelper.IsPrimitiveType(expectedContent))
                {
                    string content = await _responseMessage.Content.ReadAsStringAsync();
                    Assert.Equal(expectedContent?.ToString(), content);
                }
                else
                {
                    T? content = await _responseMessage.Content.ReadFromJsonAsync<T>();
                    Assert.Equal(expectedContent, content);
                }
            });
            return this;
        }
    }
}
