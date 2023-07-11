using Microsoft.AspNetCore.Mvc;

namespace DividendsHelper.Controllers; 

[ApiController]
[Route("Api/[action]")]
public class TestController : ControllerBase {
    public string Test(string name) {
        return $"test {name} test";
    }
}