using RequestSDK.Attributes;
using RequestSDK.Services;
using RequestSDK.Test.Base;
using Xunit.Abstractions;

namespace RequestSDK.Test.Services;

public partial class RequestService_Testing : FixtureBase
{
    public RequestService_Testing(ITestOutputHelper consoleWriter) : base(consoleWriter){}

    [Fact(DisplayName = "Creating Get")]
    public async Task RequestOptions_Initialization()
    {
        RequestService requestService = new();
        HttpResponseMessage response = await requestService.ExecuteRequestAsync(new RequestService.Options(HttpMethod.Get, "https://api.github.com/users/Nazar-Markovets").
                                                                                    AddHeader("User-Agent", "Awesome-Octocat-App"));

        Assert.True(response.StatusCode == System.Net.HttpStatusCode.OK);
    }

    //[Fact(DisplayName = "Creating Options")]
    //public void HttpClientSettings_Initialization()
    //{
    //    TimeSpan timeSpan = TimeSpan.MaxValue;
    //    RequestService requestService = new RequestService(new RequestService.RequestServiceOptions<>);
    //}

    //[Fact(DisplayName = "Creating Instance")]
    //public void Request_Initialization()
    //{
    //    TimeSpan timeSpan = TimeSpan.MaxValue;
    //    RequestService requestService = new RequestService((time) => typeof(AccemblyRouting));
    //}
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
