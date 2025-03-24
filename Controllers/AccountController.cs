using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Research_Assistant.Models;
using Research_Assistant.ViewModels;
using System.Threading.Tasks;

public class AccountController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    // User Registration (GET)
    [HttpGet]
    [Route("Account/Register")]
    public IActionResult Register()
    {
        return View();
    }

    // User Registration (POST)
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Route("Account/Register")]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (ModelState.IsValid)
        {
        var user = new ApplicationUser 
        {
            UserName = model.Email,
            Email = model.Email,
            FullName = model.FullName,
            EmailConfirmed = true 
        };

        user.NormalizedEmail = _userManager.NormalizeEmail(user.Email);
        user.NormalizedUserName = _userManager.NormalizeName(user.UserName);

        var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, model.Role);
                await _signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToAction("Index", "Home");
            }
            else
            {
                foreach (var error in result.Errors)
                {
                ModelState.AddModelError(string.Empty, error.Description);
                }
            }
        }

        return View(model);
    }
    
    // User Login (GET)
    [HttpGet]
    [Route("Account/Login")]
    public IActionResult Login()
    {
        return View();
    }

    // User Login (POST)
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Route("Account/Login")]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            Console.WriteLine("Model state is invalid.");
            return View(model);
        }

        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null)
        {
            Console.WriteLine($"User {model.Email} not found.");
            ModelState.AddModelError("", "Invalid login attempt.");
            return View(model);
        }

        var result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, false);

        if (result.Succeeded)
        {
            Console.WriteLine($"Login successful for {model.Email}");
            return RedirectToAction("Index", "Home");
        }
        else
        {
            Console.WriteLine($"Login failed for {model.Email}. Reason: {result}");
            ModelState.AddModelError("", "Invalid login attempt.");
            return View(model);
        }
    }

    // User Logout
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Route("Account/Logout")]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Login", "Account");
    }

    // User Profile Update (GET)
    [HttpGet]
    [Route("Account/UpdateProfile")]
    public async Task<IActionResult> UpdateProfile()
    {
    var user = await _userManager.GetUserAsync(User);
    if (user == null)
    {
        return NotFound();
    }

    var model = new UpdateProfileViewModel
    {
        FullName = user.FullName,
        Email = user.Email
    };

    return View(model);
    }

    // User Profile Update (POST)
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Route("Account/UpdateProfile")]
    public async Task<IActionResult> UpdateProfile(UpdateProfileViewModel model)
    {
    if (!ModelState.IsValid)
    {
        return View(model);
    }   

    var user = await _userManager.GetUserAsync(User);
    if (user == null)
    {   
        return NotFound();
    }

    // Update fields safely
    user.FullName = model.FullName;
    
    if (string.IsNullOrEmpty(model.Email))
    {
        user.Email = "default@example.com"; // Assign a default email if NULL
    }
    else
    {
        user.Email = model.Email;
    }

    user.NormalizedEmail = user.Email.ToUpper(); // Normalize email

    var result = await _userManager.UpdateAsync(user);
    if (result.Succeeded)
    {
        ViewBag.Message = "Profile updated successfully!";
    }
    else
    {
        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }
    }

    return View(model);
    }
}