using Microsoft.AspNetCore.Mvc;

namespace FinSightPro.Web.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Dashboard");
        return View();
    }

    [Route("/Home/Error/{code:int?}")]
    public IActionResult Error(int? code)
    {
        ViewBag.StatusCode = code ?? 500;
        return View();
    }

    public IActionResult AccessDenied() => View();

    public IActionResult Privacy() => View();
}
