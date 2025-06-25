using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

public class HomeController : Controller
{
    // Redirects to the secure welcome page after login or registration
    [Authorize]
    public IActionResult Index()
    {
        if (User.IsInRole("admin"))
            return RedirectToAction("Dashboard", "Admin");
        else if (User.IsInRole("user"))
            return RedirectToAction("Dashboard", "User");

        return Unauthorized();
    }


    // Optional default landing action
    public IActionResult WebForm()
    {
        return View();
    }
}
