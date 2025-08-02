using Microsoft.AspNetCore.Mvc;

namespace BrettGravesPortfolio.Controllers;

public class HomeController : Controller
{
    public IActionResult Index() => View(); // /Views/Home/Index.cshtml

    public IActionResult Projects() => View("~/Views/Projects/Index.cshtml");
    public IActionResult Skills() => View("~/Views/Skills/Index.cshtml");
    public IActionResult ApiPlayground() => View("~/Views/ApiPlayground/Index.cshtml");
    public IActionResult AzureDevOps() => View("~/Views/AzureDevOps/Index.cshtml");
    public IActionResult AIAutomation() => View("~/Views/AIAutomation/Index.cshtml");
    public IActionResult SqlData() => View("~/Views/SqlData/Index.cshtml");
    public IActionResult Resume() => View("~/Views/Resume/Index.cshtml");
    public IActionResult CoverLetter() => View("~/Views/CoverLetter/Index.cshtml");
    public IActionResult Chat() => View("~/Views/Chat/Index.cshtml");
}