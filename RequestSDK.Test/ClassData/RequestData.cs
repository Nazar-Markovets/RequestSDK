using System.Collections;
using System.Net.Mime;

using RequestSDK.Services;

namespace RequestSDK.Test.ClassData;

public sealed class RequestData : IEnumerable<object[]>
{
    public IEnumerator<object[]> GetEnumerator()
    {
        yield return new object[] 
        {
            "Mocked HTTP response content",
            "https://example.com",
            "controller/action",
            "RequestContent",
            
            new string[]{ MediaTypeNames.Application.Json, MediaTypeNames.Text.Html },
            new KeyValuePair<string, string>[] 
            {
                RequestService.RequestHeader("X-KEY", "X-VALUE-1", "X-VALUE-2")
            },
            new KeyValuePair<string, string>[] 
            {
                RequestService.RequestParameter("first-name", "john"),
                RequestService.RequestParameter("last-name", "doe"),
                RequestService.RequestParameter("age", "21")
            } 
        };

        yield return new object[]
        {
            new { ResponseContentName = "Test Name", ResponseMessage = "Test Message", ResponseId = 1 },
            "https://example.com",
            "controller/action",
            "RequestContent",

            new string[]{ MediaTypeNames.Application.Json, MediaTypeNames.Text.Html },
            new KeyValuePair<string, string>[]
            {
                RequestService.RequestHeader("X-KEY", "X-VALUE-1", "X-VALUE-2")
            },
            new KeyValuePair<string, string>[]
            {
                RequestService.RequestParameter("first-name", "john"),
                RequestService.RequestParameter("last-name", "doe"),
                RequestService.RequestParameter("age", "21")
            }
        };
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
