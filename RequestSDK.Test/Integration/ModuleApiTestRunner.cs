using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace RequestSDK.Test.Integration;

internal class ModuleApiTestRunner : WebApplicationFactory<API.Program>
{
    private readonly Action<IServiceCollection>? _serviceOverride;
    private readonly string _environment;
    public ModuleApiTestRunner(string environment = "Development", Action<IServiceCollection>? serviceOverride = default)
    {
        _serviceOverride = serviceOverride;
        _environment = environment;
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.UseEnvironment(_environment);
        //builder.ConfigureServices(_serviceOverride ?? new Action<IServiceCollection>((s) => { }));
        return base.CreateHost(builder);
    }
}
