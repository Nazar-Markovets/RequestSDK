using Microsoft.Extensions.DependencyInjection;
using RequestSDK.Test.ClassData;
using RequestSDK.Test.Integration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        try
        {
            serverInstance.Run(ServerPort);
        }
        catch
        {
            throw new ApplicationException("Server instance can't be started. Check accembly for errors");
        }
        
    }

    private void RunModuleTestIntegration(Action<IServiceCollection>? configureServices = default)
    {
        ModuleApiTestRunner app = new ModuleApiTestRunner(default!, configureServices);
        //apiHttpClient = app.CreateClient();
    }
}
