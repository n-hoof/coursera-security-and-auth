using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

[Authorize(Roles = "user")]
public class UserController : Controller
{
    // UserController.cs
    public IActionResult Dashboard()
    {
        if (!User.IsInRole("user"))
            return Forbid();

        var username = User.Identity?.Name ?? "Unknown";
        return Content($"Welcome to your dashboard, {username}!");
}
}
