using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using MyWebApp.Helpers;

public class AccountController : Controller
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;

    public AccountController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    // GET: /Account/Register
    public IActionResult Register() => View();


    [Authorize]
    [HttpGet]
    public async Task<IActionResult> Profile(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return BadRequest("User ID is required.");
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return NotFound();
        }

        var sanitizedUsername = InputSanitizer.Sanitize(user.UserName);

        var roles = await _userManager.GetRolesAsync(user);

        var model = new ProfileViewModel
        {
            Username = sanitizedUsername,
            Email = user.Email,
            Roles = roles
        };

        return View(model);
    }


    // POST: /Account/Register
    [HttpPost]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        // Sanitize input
        var cleanUsername = InputSanitizer.Sanitize(model.Username, isEmail: false);
        var cleanEmail = InputSanitizer.Sanitize(model.Email, isEmail: true);

        if (cleanUsername != model.Username || cleanEmail != model.Email)
        {
            ModelState.AddModelError(string.Empty, "Input was sanitized. Please confirm your submitted data.");
            return View(model);
        }

        var user = new IdentityUser { UserName = cleanUsername, Email = cleanEmail };
        var result = await _userManager.CreateAsync(user, model.Password);

        if (result.Succeeded)
        {
            // Ensure the "user" role exists
            if (!await _userManager.IsInRoleAsync(user, "user"))
            {
                await _userManager.AddToRoleAsync(user, "user");
            }

            await _signInManager.SignInAsync(user, isPersistent: false);
            return RedirectToAction("Index", "Home");
        }

        foreach (var error in result.Errors)
            ModelState.AddModelError(string.Empty, error.Description);

        return View(model);
    }

    // GET: /Account/Login
    public IActionResult Login() => View();

    // POST: /Account/Login
    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var cleanUsername = InputSanitizer.Sanitize(model.Username, isEmail: false);

        if (cleanUsername != model.Username)
        {
            ModelState.AddModelError(string.Empty, "Input was sanitized. Please confirm your username.");
            return View(model);
        }

        var result = await _signInManager.PasswordSignInAsync(cleanUsername, model.Password, isPersistent: false, lockoutOnFailure: false);

        if (result.Succeeded)
            return RedirectToAction("Index", "Home");

        ModelState.AddModelError(string.Empty, "Invalid login attempt.");
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }
}
