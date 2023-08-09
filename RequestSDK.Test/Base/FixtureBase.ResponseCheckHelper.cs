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

        public ResponseCheckHelper CheckResponseContent<T>(T expectedContent)
        {
            AsParallelAsync(async () =>
            {
                bool? mustBeJson = RequestService.MustBeSendAsJson(expectedContent);
                if (mustBeJson.HasValue is false) Assert.Fail("Expected content is null");

                if (mustBeJson.Value)
                {
                    T? content = await _responseMessage.Content.ReadFromJsonAsync<T>();
                    Assert.Equal(expectedContent, content);
                }
                else
                {
                    string content = await _responseMessage.Content.ReadAsStringAsync();
                    Assert.Equal(expectedContent?.ToString(), content);
                }
            });
            return this;
        }
    }
}
