using Microsoft.AspNetCore.Mvc;

namespace RequestSDK.Test.API.Controllers;

[Route("[controller]")]
[ApiController]
public class StatusController : ControllerBase
{
    [HttpGet("service_status")]
    public IActionResult Get() => Ok("API is ready");
}
