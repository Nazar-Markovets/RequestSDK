# About Package

- *.NET Verion* : `6.0`
- *Type* : `ClassLibrary`
- *Access* : `Private`
- *Author* : `Nazar Markovets`

<details>
<summary>Version 0.0.1</summary>

Here is first patch of RequestSDK library.
##### Version changes
- Added ability to create http request using different parameters
- Added ability to use IHttpClient factory that is able from .net5.0 in different types of applications
- Added ability to use only HttpClient created before using RequestService
- Added support of recognizable routing

</details>

<details>
<summary>Version 0.0.2</summary>


##### Update changes
- Added connect extensions
- Added ability to set static authorization tokens for each Http client
- Added ability to register recognizable routing for each Http client
- Added new way to build request options that may save a developer from mistakes

## Ability to connect request service

##### 1. Configure HttpClients and Register Request Service
```c#

// -- Program.cs

builder.Services.RegisterHttpClients
(
    gitClient => { 
        gitClient.HttpClientName = "GIT.1";
        gitClient.HttpClientId = 1;
        gitClient.BaseAddress = new Uri("https://example.com");
        gitClient.Authentication = (schemes) => new AuthenticationHeaderValue(schemes.Bearer, "XXX-KEY-XXX");
    }
);

builder.Services.RegisterRequestService();

```
##### 2. Use Request Service in destination class

```c#

// -- YourController.cs

[Route("[controller]")]
[ApiController]
public class YourController : ControllerBase
{
    private readonly RequestService requestService;

    public StatusController(RequestService requestService) => this.requestService = requestService;

    [HttpGet("your_route")]
    public async Task<IActionResult> Get()
    {
        RequestService.Options options = RequestService.Options.WithRegisteredClient(HttpMethod.Get, "user/packages/versions", 1);
        HttpResponseMessage response = await requestService.ExecuteRequestAsync(options);
        var content = await response.Content.ReadAsStringAsync();
        return Ok(content);
    }
}

```

## Use recognizable routing
##### 1. Implement Routing Strategy

```c#
// -- Routing.cs

public partial class Routing
{
    [ControllerName("Hub")]
    public static class HubController
    {
        [ControllerHttpMethod(HttpRequestMethod.Post)]
        public const string SendMessageForAllUsers = "sendMessage";

        [ControllerHttpMethod(HttpRequestMethod.Post)]
        public const string SendMessageForGroupUsers = "sendMessageGroup"; 

    }
}
```

##### 1. Configure HttpClients and Register Request Service
```c#

// -- Program.cs

builder.Services.RegisterHttpClients
(
    hubClient => { 
        hubClient.HttpClientName = "Hub.Client";
        hubClient.HttpClientId = 1;
        hubClient.BaseAddress = new Uri("https://example.com");
        hubClient.ClientRoutingType = typeof(Routing);
    }
);

builder.Services.RegisterRequestService();

```
##### 2. Use Request Service in destination class

```c#

// -- YourController.cs

[Route("[controller]")]
[ApiController]
public class YourController : ControllerBase
{
    private readonly RequestService requestService;

    public StatusController(RequestService requestService) => this.requestService = requestService;

    [HttpGet("your_route")]
    public async Task<IActionResult> Get()
    {
        RequestService.Options options = RequestService.Options.WithRegisteredRouting(Routing.HubController.SendMessageForAllUsers, 1);
        HttpResponseMessage response = await requestService.ExecuteRequestAsync(options);
        var content = await response.Content.ReadAsStringAsync();
        return Ok(content);
    }
}

```
</details>

<details>
<summary>Version 0.0.3</summary>

##### Version changes
- Hided constructor that can't be called without DJ
- Modied way of building Request Options Headers/Query Parameters
- Moved static methods from RequestService to Helpers -> QueryHelper/HttpContentHelper
- Extended functionality of creating Query Parameters/Query Headers
- Added ability to override functionality of building Request Options Headers/Query Parameters
- Fixed appending registered HttpClient path and given path
</details>

