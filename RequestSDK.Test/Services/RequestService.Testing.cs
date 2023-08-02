using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Mvc.Testing;
using RequestSDK.Attributes;
using RequestSDK.Services;
using RequestSDK.Test.Base;
using RequestSDK.Test.Integration;
using System.Diagnostics;
using System.Net.Http;
using System.Reflection;
using Xunit.Abstractions;

namespace RequestSDK.Test.Services;


[Trait("Test", "Request Service Usage")]
public partial class RequestService_Testing : FixtureBase
{
    public RequestService_Testing(ITestOutputHelper consoleWriter, ServerInstanceRunner server) : base(consoleWriter, server) => RunAPI();

    [Fact(DisplayName = "Creating Get")]
    public async Task RequestOptions_Initialization()
    {
        RequestService requestService = new();
        HttpResponseMessage response = await requestService.ExecuteRequestAsync(new RequestService.Options(HttpMethod.Get, $"{ServerBaseUrl}status/service_status"));
        Assert.True(response.StatusCode == System.Net.HttpStatusCode.OK);

    }

    [Fact(DisplayName = "Creating Options")]
    public async Task HttpClientSettings_Initialization()
    {
        RequestService requestService = new();
        HttpResponseMessage response = await requestService.ExecuteRequestAsync(new RequestService.Options(HttpMethod.Get, $"{ServerBaseUrl}status/service_status"));
        Assert.True(response.StatusCode == System.Net.HttpStatusCode.OK);
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
