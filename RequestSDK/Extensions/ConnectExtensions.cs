using System.Net.Http.Headers;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using RequestSDK.Services;


namespace RequestSDK.Extensions;

public static class ConnectExtensions
{
    /// <summary>
    /// Register <see cref="RequestService.HttpClientSettings"/> that will be passed to registered request service.
    /// In this case you can paste <see cref="RequestService.HttpClientSettings.HttpClientId"/> and set only request endpoints
    /// </summary>
    /// <param name="httpClientsConfiguration">Configure your future Http Clients</param>
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

    [Obsolete("It is not recommended not to use Http Clients registration and HttpFactory. Use it for your own risk")]
    public static void RegisterRequestService(this IServiceCollection services) =>
        AddRequestServiceSingleton(services, default, default);

    /// <summary>
    /// Register Request Service with known routing contaner
    /// Routing container creation:
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
    /// builder.Services.RegisterRequestService(typeof(Routing), schemes => new AuthenticationHeaderValue(schemes.Bearer, "XXXX-KEY-XXXX"))
    /// builder.Services.RegisterRequestService(typeof(Routing), default)
    /// </code>
    /// </summary>
    /// <param name="routingContainer">Parent-contaner class which has classes marked with ControllerName attributes</param>
    /// <param name="authentication">Authentication key that can be set on demand</param>

    public static void RegisterRequestService(this IServiceCollection services, Type? routingContainer, Func<Schemes.AuthenticationSchemes, AuthenticationHeaderValue>? authentication = default) => 
        AddRequestServiceSingleton(services, routingContainer, authentication);

    public static void RegisterRequestService(this IServiceCollection services, Func<Schemes.AuthenticationSchemes, AuthenticationHeaderValue>? authentication, Type? routingContainer = default) =>
        AddRequestServiceSingleton(services, routingContainer, authentication);

    private static void AddRequestServiceSingleton(IServiceCollection services, Type? routingContainer = default, Func<Schemes.AuthenticationSchemes, AuthenticationHeaderValue>? authentication = default)
    {
        services.TryAddSingleton(services =>
        {
            RequestService.RequestServiceOptions serviceOptions = new()
            {
                Factory = services.GetRequiredService<IHttpClientFactory>(),
                HttpClientSettings = services.GetServices<RequestService.HttpClientSettings>(),
                AccemblyRoutingType = routingContainer,
                Authentication = authentication

            };
            return new RequestService(serviceOptions);
        });
    }
}
