using Labb3_DriverInformationSystem.Data;
using Labb3_DriverInformationSystem.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

public class HomeController : Controller
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly NotificationsService _notificationsService;
    private readonly ApplicationDbContext _context;

    public HomeController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, NotificationsService notificationsService, ApplicationDbContext context)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _notificationsService = notificationsService;
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        //Om användaren är inloggad, skicka dem till rätt dashboard
        if (_signInManager.IsSignedIn(User))
        {
            var user = await _userManager.GetUserAsync(User);
            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");

            return isAdmin ? RedirectToAction("AdminDashboard") : RedirectToAction("EmployeeDashboard");
        }

        return RedirectToAction("Login", "Account");
    }

    // Admin Dashboard
    public async Task<IActionResult> AdminDashboard()
    {
        var user = await _userManager.GetUserAsync(User);

        // Hämta antalet olästa notifikationer för användaren
        var unreadNotificationCount = await _notificationsService.GetUnreadNotificationCountAsync(user.Id);

        // Skicka antalet olästa notifikationer till vyn
        ViewBag.NotificationCount = unreadNotificationCount;

        return View();
    }

    // Employee Dashboard
    public async Task<IActionResult> EmployeeDashboard()
    {
        var user = await _userManager.GetUserAsync(User);
        var employee = await _context.Employees.FirstOrDefaultAsync(e => e.IdentityUserId == user.Id);

        if (employee == null)
        {
            return NotFound();
        }
        // Hämta antalet olästa notifikationer för användaren
        var unreadCount = await _notificationsService.GetUnreadNotificationCountAsync(user.Id);

        ViewBag.NotificationCount = unreadCount;

        return View(employee);
    }
}
