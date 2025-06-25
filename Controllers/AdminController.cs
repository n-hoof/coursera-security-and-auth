using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using System.Linq;

[Authorize(Roles = "admin")]
public class AdminController : Controller
{
    private readonly UserManager<IdentityUser> _userManager;

    public AdminController(UserManager<IdentityUser> userManager)
    {
        _userManager = userManager;
    }

    // AdminController.cs
    public IActionResult Dashboard()
    {
        if (!User.IsInRole("admin"))
            return Forbid();

        var users = _userManager.Users
            .Select(u => new UserViewModel { UserName = u.UserName, Email = u.Email })
            .ToList();

        return View(users);
    }

}
