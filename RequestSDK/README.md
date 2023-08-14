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
```c#

// -- Program.cs

builder.Services.RegisterHttpClients
(
    gitClient => 
    { 
        gitClient.HttpClientName = "GIT.1";
        gitClient.HttpClientId = 1;
        gitClient.BaseAddress = new Uri("https://api.github.com");
        gitClient.Authentication = (schemes) => new AuthenticationHeaderValue(schemes.Bearer, "XXX-KEY-XXX");
    },
    gitExtendedClient =>
    {
        gitExtendedClient.HttpClientName = "GIT.2";
        gitExtendedClient.HttpClientId = 2;
        gitExtendedClient.BaseAddress = new Uri("https://api.gitextended.com");
    }
);

builder.Services.RegisterRequestService();

```

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
        HttpResponseMessage response = await requestService.ExecuteRequestAsync(options
                                                                                .AddHeader(HeaderNames.UserAgent, "local")
                                                                                .AddHeader(HeaderNames.Accept, "application/vnd.github+json")
                                                                                .AddHeader(HeaderNames.Host, "api.github.com"));
        var content = await response.Content.ReadAsStringAsync();
        return Ok(content);
    }
}



```

</details>

