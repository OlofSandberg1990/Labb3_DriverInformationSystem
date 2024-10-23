
using Labb3_DriverInformationSystem.Data;
using Labb3_DriverInformationSystem.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Labb3_DriverInformationSystem.Controllers
{
    public class AccountController : Controller
    {

        //Hämta signInManager och userManager för att hantera inloggning och användare
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        

        public AccountController(SignInManager<IdentityUser> signInManager, UserManager<IdentityUser> userManager, ApplicationDbContext context)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            
        }

        // GET: Account/Login
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // POST: Account/Login
        [HttpPost]
        public async Task<IActionResult> Login(LogInViewModel model)
        {
            if (ModelState.IsValid)
            {
                //Försök logga in användaren
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, false, lockoutOnFailure: false);

                //Om inloggningen lyckas, skicka användaren till rätt dashboard
                if (result.Succeeded)
                {
                    var user = await _userManager.FindByEmailAsync(model.Email);
                    if (await _userManager.IsInRoleAsync(user, "Admin"))
                    {
                        return RedirectToAction("AdminDashboard", "Home");
                    } else if (await _userManager.IsInRoleAsync(user, "Employee"))
                    {
                        return RedirectToAction("EmployeeDashboard", "Home");
                    }
                }

                //Lägg till specifikt felmeddelande om inloggningen misslyckas
                ModelState.AddModelError(string.Empty, "Felaktigt e-post eller lösenord.");
            }

            return View(model);
        }


        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login", "Account");
        }
    }
}
