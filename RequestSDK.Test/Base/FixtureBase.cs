using Microsoft.Extensions.DependencyInjection;
using RequestSDK.Test.Integration;
using Xunit.Abstractions;

namespace RequestSDK.Test.Base;

public class FixtureBase : IClassFixture<ServerInstanceRunner>
{
    protected readonly ServerInstanceRunner serverInstance;
    protected readonly ITestOutputHelper console;
    protected bool ServerIsStarted => serverInstance.IsAlive;
    protected int ServerPort = 5100;
    protected Uri ServerBaseUrl => new(serverInstance.BaseUrl);
    public FixtureBase(ITestOutputHelper consoleWriter, ServerInstanceRunner server)
    {
        console = consoleWriter;
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
