using Microsoft.Extensions.DependencyInjection;

using RequestSDK.Test.Integration;

using Xunit.Abstractions;

namespace RequestSDK.Test.Base;

public partial class FixtureBase : IClassFixture<ServerInstanceRunner>
{
    protected readonly ServerInstanceRunner serverInstance;
    protected readonly ITestOutputHelper TestContole;
    protected bool ServerIsStarted => serverInstance.IsAlive;
    protected int ServerPort = 5100;
    protected Uri ServerBaseUrl => new(serverInstance.BaseUrl);
    protected MockRequestHelper MockHelper = new();

    protected readonly string RequestURL = "https://example.com";
    protected readonly string RequestControllerURL = "https://example.com/controller/action";
    protected readonly string TargetClientId = "Mock.Client.ID";
    protected readonly string ResponseContent = "Mocked HTTP response content";
    protected readonly Uri RequestControllerUri = new("https://example.com/controller/action");


    public FixtureBase(ITestOutputHelper consoleWriter, ServerInstanceRunner server)
    {
        TestContole = consoleWriter;
        serverInstance = server;
    }

    protected void ThrowsWithMessage<T>(Action action, string message) where T : Exception
    {
        try
        {
            Assert.Throws<T>(action);
        }
        catch
        {
            Assert.Fail(message);
        }
    }

    protected void RunAPI()
    {
#if CLOUD_GIT
        Skip.If(Convert.ToBoolean(Environment.GetEnvironmentVariable("GIT_WORKFLOW")), "Supported only on local machine");
#endif
        serverInstance.Run(ServerPort);
    }

    protected string GenerateRequestRoute(string routeName) => new Uri(ServerBaseUrl, routeName).ToString();

    private void RunModuleTestIntegration(Action<IServiceCollection>? configureServices = default)
    {
        ModuleApiTestRunner app = new ModuleApiTestRunner(default!, configureServices);
        //apiHttpClient = app.CreateClient();
    }
}
