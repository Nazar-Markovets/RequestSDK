using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using RequestSDK.Services;

namespace RequestSDK.Extensions;

public static class ConnectExtensions
{
    /// <summary>
    /// Register <see cref="RequestService.HttpClientSettings"/> that will be passed to registered request service.
    /// In this case you can paste <see cref="RequestService.HttpClientSettings.HttpClientId"/> and set only request endpoints
    /// <code>
    /// class Routing
    /// {
    ///     [ControllerName("api/home")]
    ///     class HomeRoute
    ///     {
    ///         [ControllerHttpMethod(HttpRequestMethod.Get)]
    ///         const string StatusCall = "status_call"
    ///     }
    /// }
    /// 
    /// builder.Services.RegisterHttpClients
    /// (
    ///     clientConfiguration => {
    ///         clientConfiguration.HttpClientName = "X.Client.Name";
    ///         clientConfiguration.HttpClientId = 1;
    ///         clientConfiguration.BaseAddress = new Uri("https://example.com");
    ///         clientConfiguration.Authentication = scheme => new AuthenticationHeaderValue(scheme.Bearer, "X-Token-Key");
    ///         clientConfiguration.ClientRoutingType = typeof(Routing);
    ///     }
    /// );
    /// </code>
    /// </summary>
    public static void RegisterHttpClients(this IServiceCollection services, params Action<RequestService.HttpClientSettings>[] httpClientsConfiguration)
    {
        RequestService.HttpClientSettings[] registeredSettings = new RequestService.HttpClientSettings[httpClientsConfiguration.Length];
        for (int configIndex = 0; configIndex < httpClientsConfiguration.Length; configIndex++)
        {
            RequestService.HttpClientSettings setting = new();
            Action<RequestService.HttpClientSettings>? configuration = httpClientsConfiguration[configIndex];
            configuration?.Invoke(setting);

            if (!setting.HttpClientId.HasValue) throw new ArgumentNullException("Http Client Id is required if you want to use it in request service");
            if (string.IsNullOrWhiteSpace(setting.HttpClientName)) throw new ArgumentException("Http Client Name is required if you want to use it in request service");

            foreach (RequestService.HttpClientSettings existsSetting in registeredSettings.Where(s => s != null))
            {
                if (existsSetting.HttpClientName.Equals(setting.HttpClientName)) throw new ArgumentException($"You have already registered setting with name '{setting.HttpClientName}'");
                if (existsSetting.HttpClientId.Equals(setting.HttpClientId)) throw new ArgumentException($"You have already registered setting with id '{setting.HttpClientId}'");
                if (existsSetting.BaseAddress?.Equals(setting.BaseAddress) == true) throw new ArgumentException($"You have already registered setting with base address '{setting.BaseAddress}'");
            }

            registeredSettings[configIndex] = setting;

            services.AddSingleton(setting);
            services.AddHttpClient(setting.HttpClientName, client =>
            {
                client.BaseAddress = setting?.BaseAddress;
            });
        }
    }

    /// <summary>Register Request Service to call the service via Dependency Injection</summary>
    public static void RegisterRequestService(this IServiceCollection services)
    {
        services.TryAddSingleton(services =>
        {
            return new RequestService(services.GetService<IHttpClientFactory>(),
                                      services.GetServices<RequestService.HttpClientSettings>().ToArray());
        });
    }
}
